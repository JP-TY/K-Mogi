namespace KMogi.Core.Sla
{
    /// <summary>
    /// Pienemann Processability Theory developmental stages, adapted for Japanese as a
    /// second language (after Kawaguchi). A learner can only process and produce
    /// structures at or below their current stage; input above it is not yet acquirable
    /// and drives confusion in the simulation.
    /// </summary>
    public enum ProcessabilityStage
    {
        /// <summary>Lemma access: single words and unanalysed formulae.</summary>
        Stage1Words = 1,

        /// <summary>Category procedure: canonical SOV order, core case particles.</summary>
        Stage2CanonicalOrder = 2,

        /// <summary>Phrasal procedure: adjectives, te-form, intra-phrasal information.</summary>
        Stage3Phrasal = 3,

        /// <summary>Inter-phrasal procedure: passive, causative, derived morphology.</summary>
        Stage4Interphrasal = 4,

        /// <summary>Subordinate-clause procedure: noun-modifying / relative clauses.</summary>
        Stage5Subordinate = 5
    }
}
