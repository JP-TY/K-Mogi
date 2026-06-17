using System.Collections.Generic;
using KMogi.Core.Parsing;

namespace KMogi.Core.Sla
{
    /// <summary>
    /// Detects which <see cref="PtFeature"/>s are present in a morphological token sequence.
    /// Heuristic and deterministic by design — accuracy is traded for testability and zero
    /// external dependencies, which is appropriate for driving avatar reactions in a trainer.
    /// </summary>
    public sealed class PtFeatureDetector
    {
        private static readonly string[] CaseParticles = { "は", "が", "を", "に", "で" };

        private readonly SovSyntaxValidator _sov;

        public PtFeatureDetector(SovSyntaxValidator sov = null)
        {
            _sov = sov ?? new SovSyntaxValidator();
        }

        public IReadOnlyCollection<PtFeature> Detect(IReadOnlyList<Token> tokens)
        {
            var features = new HashSet<PtFeature>();
            if (tokens == null || tokens.Count == 0)
            {
                return features;
            }

            if (CountContentWords(tokens) <= 1 && !HasVerb(tokens))
            {
                features.Add(PtFeature.SingleWord);
            }

            SovAnalysis sov = _sov.Analyze(tokens);
            if (sov.IsCanonicalSov)
            {
                features.Add(PtFeature.CanonicalSov);
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                Token token = tokens[i];

                if (token.PartOfSpeech == PartOfSpeech.Particle &&
                    i > 0 && tokens[i - 1].IsNominal &&
                    IsCaseParticle(token.Surface))
                {
                    features.Add(PtFeature.ParticleAttachment);
                }

                if (token.PartOfSpeech == PartOfSpeech.Adjective ||
                    token.PartOfSpeech == PartOfSpeech.AdjectivalNoun)
                {
                    features.Add(PtFeature.Adjective);
                }

                if (IsTeFormConnective(token) && HasVerbBefore(tokens, i))
                {
                    features.Add(PtFeature.TeForm);
                }

                if (token.PartOfSpeech == PartOfSpeech.AuxiliaryVerb)
                {
                    if (IsPassiveAuxiliary(token))
                    {
                        features.Add(PtFeature.Passive);
                    }

                    if (IsCausativeAuxiliary(token))
                    {
                        features.Add(PtFeature.Causative);
                    }
                }

                // Noun-modifying / relative clause: a verb directly modifying a following nominal.
                if (token.PartOfSpeech == PartOfSpeech.Verb &&
                    i + 1 < tokens.Count && tokens[i + 1].IsNominal)
                {
                    features.Add(PtFeature.NounModifyingClause);
                }
            }

            return features;
        }

        private static int CountContentWords(IReadOnlyList<Token> tokens)
        {
            int count = 0;
            for (int i = 0; i < tokens.Count; i++)
            {
                PartOfSpeech pos = tokens[i].PartOfSpeech;
                if (pos != PartOfSpeech.Particle &&
                    pos != PartOfSpeech.AuxiliaryVerb &&
                    pos != PartOfSpeech.Symbol)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool HasVerb(IReadOnlyList<Token> tokens)
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].PartOfSpeech == PartOfSpeech.Verb)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasVerbBefore(IReadOnlyList<Token> tokens, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (tokens[i].PartOfSpeech == PartOfSpeech.Verb)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsCaseParticle(string surface)
        {
            for (int i = 0; i < CaseParticles.Length; i++)
            {
                if (CaseParticles[i] == surface)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTeFormConnective(Token token)
        {
            // The connective て/で is tagged as a particle (接続助詞) by most dictionaries.
            return token.PartOfSpeech == PartOfSpeech.Particle &&
                   (token.Surface == "て" || token.Surface == "で");
        }

        private static bool IsPassiveAuxiliary(Token token)
        {
            string form = token.BaseForm;
            return form == "れる" || form == "られる";
        }

        private static bool IsCausativeAuxiliary(Token token)
        {
            string form = token.BaseForm;
            return form == "せる" || form == "させる";
        }
    }
}
