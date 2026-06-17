using System.Collections.Generic;

namespace KMogi.Core.Sla
{
    /// <summary>
    /// Maps each <see cref="PtFeature"/> to the <see cref="ProcessabilityStage"/> at which a
    /// learner can process it. The default mapping encodes a reasonable, monotonic ordering
    /// for Japanese L2 acquisition; it is intentionally data-driven so that the pedagogy can
    /// be retuned (via the runtime ScriptableObject authoring layer) without code changes.
    /// </summary>
    public sealed class PtRuleSet
    {
        private readonly Dictionary<PtFeature, ProcessabilityStage> _required;

        public PtRuleSet(IEnumerable<KeyValuePair<PtFeature, ProcessabilityStage>> rules)
        {
            _required = new Dictionary<PtFeature, ProcessabilityStage>();
            if (rules != null)
            {
                foreach (KeyValuePair<PtFeature, ProcessabilityStage> rule in rules)
                {
                    _required[rule.Key] = rule.Value;
                }
            }
        }

        /// <summary>The built-in default mapping used when no overrides are authored.</summary>
        public static PtRuleSet CreateDefault()
        {
            return new PtRuleSet(new[]
            {
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.SingleWord, ProcessabilityStage.Stage1Words),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.CanonicalSov, ProcessabilityStage.Stage2CanonicalOrder),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.ParticleAttachment, ProcessabilityStage.Stage2CanonicalOrder),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.Adjective, ProcessabilityStage.Stage3Phrasal),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.TeForm, ProcessabilityStage.Stage3Phrasal),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.Passive, ProcessabilityStage.Stage4Interphrasal),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.Causative, ProcessabilityStage.Stage4Interphrasal),
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.NounModifyingClause, ProcessabilityStage.Stage5Subordinate),
            });
        }

        /// <summary>
        /// The stage required to process <paramref name="feature"/>. Unmapped features
        /// default to <see cref="ProcessabilityStage.Stage1Words"/> (always processable).
        /// </summary>
        public ProcessabilityStage RequiredStage(PtFeature feature)
        {
            return _required.TryGetValue(feature, out ProcessabilityStage stage)
                ? stage
                : ProcessabilityStage.Stage1Words;
        }
    }
}
