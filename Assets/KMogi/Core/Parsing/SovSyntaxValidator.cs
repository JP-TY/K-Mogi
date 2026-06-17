using System.Collections.Generic;

namespace KMogi.Core.Parsing
{
    /// <summary>Result of analysing a token sequence for canonical Japanese SOV structure.</summary>
    public sealed class SovAnalysis
    {
        /// <summary>Index of the nominal acting as subject (marked by は/が), or -1.</summary>
        public int SubjectIndex { get; }

        /// <summary>Index of the nominal acting as direct object (marked by を), or -1.</summary>
        public int ObjectIndex { get; }

        /// <summary>Index of the clause-final verb, or -1.</summary>
        public int VerbIndex { get; }

        public bool HasSubject => SubjectIndex >= 0;
        public bool HasObject => ObjectIndex >= 0;
        public bool HasVerb => VerbIndex >= 0;

        /// <summary>
        /// True when the present constituents appear in canonical Subject-Object-Verb
        /// order (any absent constituent is simply skipped, but a verb must be present
        /// and must follow the subject and object).
        /// </summary>
        public bool IsCanonicalSov { get; }

        public SovAnalysis(int subjectIndex, int objectIndex, int verbIndex, bool isCanonicalSov)
        {
            SubjectIndex = subjectIndex;
            ObjectIndex = objectIndex;
            VerbIndex = verbIndex;
            IsCanonicalSov = isCanonicalSov;
        }
    }

    /// <summary>
    /// Lightweight, dictionary-agnostic detector of canonical Japanese SOV word order
    /// and core case-particle attachment (は/が subject, を object). This is intentionally
    /// heuristic: it is sufficient for Processability-stage feature detection and is fully
    /// deterministic and unit-testable.
    /// </summary>
    public sealed class SovSyntaxValidator
    {
        private static readonly string[] SubjectParticles = { "は", "が" };
        private const string ObjectParticle = "を";

        public SovAnalysis Analyze(IReadOnlyList<Token> tokens)
        {
            int subjectIndex = -1;
            int objectIndex = -1;
            int verbIndex = -1;

            if (tokens == null)
            {
                return new SovAnalysis(-1, -1, -1, false);
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];

                if (token.PartOfSpeech == PartOfSpeech.Particle && i > 0 && tokens[i - 1].IsNominal)
                {
                    if (subjectIndex < 0 && IsSubjectParticle(token.Surface))
                    {
                        subjectIndex = i - 1;
                    }
                    else if (objectIndex < 0 && token.Surface == ObjectParticle)
                    {
                        objectIndex = i - 1;
                    }
                }

                if (token.PartOfSpeech == PartOfSpeech.Verb)
                {
                    verbIndex = i; // keep the last verb as the clause head
                }
            }

            bool canonical =
                verbIndex >= 0 &&
                (subjectIndex < 0 || subjectIndex < verbIndex) &&
                (objectIndex < 0 || objectIndex < verbIndex) &&
                (subjectIndex < 0 || objectIndex < 0 || subjectIndex < objectIndex);

            return new SovAnalysis(subjectIndex, objectIndex, verbIndex, canonical);
        }

        private static bool IsSubjectParticle(string surface)
        {
            for (int i = 0; i < SubjectParticles.Length; i++)
            {
                if (SubjectParticles[i] == surface)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
