using System.Collections.Generic;
using KMogi.Core.Parsing;

namespace KMogi.Core.Sla
{
    /// <summary>Outcome of comparing an utterance's grammatical demands against a learner's stage.</summary>
    public sealed class DeviationResult
    {
        /// <summary>True when the utterance contains structure above the learner's current stage.</summary>
        public bool StructureBeyondStage { get; }

        /// <summary>The highest stage demanded by any feature detected in the utterance.</summary>
        public ProcessabilityStage HighestRequiredStage { get; }

        /// <summary>The learner's active stage this utterance was evaluated against.</summary>
        public ProcessabilityStage StudentStage { get; }

        /// <summary>The feature responsible for <see cref="HighestRequiredStage"/>.</summary>
        public PtFeature WorstFeature { get; }

        /// <summary>True when at least one grammatical feature was detected.</summary>
        public bool HasFeatures { get; }

        /// <summary>How many stages the hardest feature sits above the learner (0 when within reach).</summary>
        public int StageGap
        {
            get
            {
                int gap = (int)HighestRequiredStage - (int)StudentStage;
                return gap > 0 ? gap : 0;
            }
        }

        public DeviationResult(
            bool structureBeyondStage,
            ProcessabilityStage highestRequiredStage,
            ProcessabilityStage studentStage,
            PtFeature worstFeature,
            bool hasFeatures)
        {
            StructureBeyondStage = structureBeyondStage;
            HighestRequiredStage = highestRequiredStage;
            StudentStage = studentStage;
            WorstFeature = worstFeature;
            HasFeatures = hasFeatures;
        }
    }

    /// <summary>
    /// Compares the grammatical features of an utterance (via <see cref="PtFeatureDetector"/>)
    /// against a learner's Processability stage and reports whether the input was beyond reach.
    /// </summary>
    public sealed class PtEvaluator
    {
        private readonly PtRuleSet _rules;
        private readonly PtFeatureDetector _detector;

        public PtEvaluator(PtRuleSet rules = null, PtFeatureDetector detector = null)
        {
            _rules = rules ?? PtRuleSet.CreateDefault();
            _detector = detector ?? new PtFeatureDetector();
        }

        public DeviationResult Evaluate(IReadOnlyList<Token> tokens, ProcessabilityStage studentStage)
        {
            IReadOnlyCollection<PtFeature> features = _detector.Detect(tokens);

            if (features.Count == 0)
            {
                return new DeviationResult(
                    structureBeyondStage: false,
                    highestRequiredStage: ProcessabilityStage.Stage1Words,
                    studentStage: studentStage,
                    worstFeature: PtFeature.SingleWord,
                    hasFeatures: false);
            }

            ProcessabilityStage highest = ProcessabilityStage.Stage1Words;
            PtFeature worst = PtFeature.SingleWord;
            bool first = true;

            foreach (PtFeature feature in features)
            {
                ProcessabilityStage required = _rules.RequiredStage(feature);
                if (first || (int)required > (int)highest)
                {
                    highest = required;
                    worst = feature;
                    first = false;
                }
            }

            bool beyond = (int)highest > (int)studentStage;
            return new DeviationResult(beyond, highest, studentStage, worst, hasFeatures: true);
        }
    }
}
