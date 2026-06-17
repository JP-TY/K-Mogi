namespace KMogi.Core.Sla
{
    /// <summary>
    /// Grammatical features the simulation can detect in teacher utterances. Each feature
    /// is mapped by a <see cref="PtRuleSet"/> to the <see cref="ProcessabilityStage"/> at
    /// which a learner becomes able to process it.
    /// </summary>
    public enum PtFeature
    {
        /// <summary>A bare word or formula with no productive syntax.</summary>
        SingleWord = 0,

        /// <summary>Canonical Subject-Object-Verb clause order.</summary>
        CanonicalSov,

        /// <summary>Core case-particle attachment (は/が/を/に/で).</summary>
        ParticleAttachment,

        /// <summary>Predicative or attributive adjective use.</summary>
        Adjective,

        /// <summary>Te-form linkage (て/で connective).</summary>
        TeForm,

        /// <summary>Noun-modifying (relative / embedded) clause: a verb modifying a following noun.</summary>
        NounModifyingClause,

        /// <summary>Passive voice (れる/られる).</summary>
        Passive,

        /// <summary>Causative voice (せる/させる).</summary>
        Causative
    }
}
