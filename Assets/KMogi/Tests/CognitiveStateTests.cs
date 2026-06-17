using KMogi.Core.Sla;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class CognitiveStateTests
    {
        private static DeviationResult Beyond(ProcessabilityStage required, ProcessabilityStage student)
            => new DeviationResult(true, required, student, PtFeature.NounModifyingClause, true);

        [Test]
        public void ApplyDeviation_RaisesConfusionProportionalToStageGap()
        {
            var state = new CognitiveState();

            // Stage 5 structure to a Stage 2 learner => gap 3 => 0.25 * 3 = 0.75.
            state.ApplyDeviation(Beyond(ProcessabilityStage.Stage5Subordinate, ProcessabilityStage.Stage2CanonicalOrder));

            Assert.AreEqual(0.75, state.Confusion, 1e-6);
        }

        [Test]
        public void ApplyDeviation_WithinReach_DoesNothing()
        {
            var state = new CognitiveState();

            state.ApplyDeviation(new DeviationResult(false, ProcessabilityStage.Stage2CanonicalOrder,
                ProcessabilityStage.Stage2CanonicalOrder, PtFeature.CanonicalSov, true));

            Assert.AreEqual(0.0, state.Confusion, 1e-6);
        }

        [Test]
        public void Confusion_IsClampedToOne()
        {
            var state = new CognitiveState();

            // gap 4 => 1.0, applied twice must remain clamped.
            state.ApplyDeviation(Beyond(ProcessabilityStage.Stage5Subordinate, ProcessabilityStage.Stage1Words));
            state.ApplyDeviation(Beyond(ProcessabilityStage.Stage5Subordinate, ProcessabilityStage.Stage1Words));

            Assert.AreEqual(1.0, state.Confusion, 1e-6);
        }

        [Test]
        public void ExplicitThenRecast_RaisesThenRelievesAnxiety()
        {
            var state = new CognitiveState();

            state.ApplyCorrection(CorrectionType.ExplicitCorrection); // anxiety 0.30, confusion 0.10
            Assert.AreEqual(0.30, state.Anxiety, 1e-6);
            Assert.AreEqual(0.10, state.Confusion, 1e-6);

            state.ApplyCorrection(CorrectionType.Recast); // anxiety -0.15, confusion -0.10
            Assert.AreEqual(0.15, state.Anxiety, 1e-6);
            Assert.AreEqual(0.0, state.Confusion, 1e-6);
        }

        [Test]
        public void Recast_FromCalm_ClampsAnxietyAtZero()
        {
            var state = new CognitiveState();

            state.ApplyCorrection(CorrectionType.Recast);

            Assert.AreEqual(0.0, state.Anxiety, 1e-6);
            Assert.AreEqual(0.0, state.Confusion, 1e-6);
        }

        [Test]
        public void ApplyPacing_TooFast_RaisesConfusion()
        {
            var state = new CognitiveState();

            // excess 1.5 morae/s => 0.06 * 1.5 = 0.09.
            var pacing = new PacingResult(morae: 4, seconds: 0.5, moraePerSecond: 8.0, threshold: 6.5);
            state.ApplyPacing(pacing);

            Assert.AreEqual(0.09, state.Confusion, 1e-6);
        }

        [Test]
        public void ApplyPacing_WithinPace_DoesNothing()
        {
            var state = new CognitiveState();

            var pacing = new PacingResult(morae: 4, seconds: 1.0, moraePerSecond: 4.0, threshold: 6.5);
            state.ApplyPacing(pacing);

            Assert.AreEqual(0.0, state.Confusion, 1e-6);
        }

        [Test]
        public void Decay_RelaxesSignalsTowardZero()
        {
            var state = new CognitiveState { DecayPerSecond = 0.08 };
            state.ApplyDeviation(Beyond(ProcessabilityStage.Stage3Phrasal, ProcessabilityStage.Stage2CanonicalOrder)); // 0.25

            state.Decay(1.0); // -0.08
            Assert.AreEqual(0.17, state.Confusion, 1e-6);

            state.Decay(100.0); // clamps at 0
            Assert.AreEqual(0.0, state.Confusion, 1e-6);
        }

        [Test]
        public void Decay_NonPositiveDelta_DoesNothing()
        {
            var state = new CognitiveState();
            state.ApplyDeviation(Beyond(ProcessabilityStage.Stage3Phrasal, ProcessabilityStage.Stage2CanonicalOrder));

            state.Decay(0);

            Assert.AreEqual(0.25, state.Confusion, 1e-6);
        }
    }
}
