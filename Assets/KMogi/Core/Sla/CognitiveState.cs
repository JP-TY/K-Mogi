namespace KMogi.Core.Sla
{
    /// <summary>The corrective feedback move a trainee instructor can make on a learner error.</summary>
    public enum CorrectionType
    {
        /// <summary>Implicit reformulation that keeps the conversation flowing; lowers anxiety.</summary>
        Recast,

        /// <summary>Explicit interruption to state the rule; raises the affective filter.</summary>
        ExplicitCorrection
    }

    /// <summary>
    /// The simulated learner's affective/cognitive state, expressed as two bounded [0,1] signals:
    /// <see cref="Confusion"/> (comprehension load) and <see cref="Anxiety"/> (affective filter).
    /// All inputs are clamped, and both signals decay toward zero over time. This is a pure model
    /// with no engine dependency so it can be exhaustively unit-tested; the runtime binds it to
    /// avatar blendshapes and telemetry.
    /// </summary>
    public sealed class CognitiveState
    {
        public double Confusion { get; private set; }
        public double Anxiety { get; private set; }

        // Tuning knobs (exposed so the runtime/data layer can adjust feel without editing logic).
        public double ConfusionPerStageGap { get; set; } = 0.25;
        public double ConfusionPerExcessMora { get; set; } = 0.06;
        public double RecastAnxietyRelief { get; set; } = 0.15;
        public double RecastConfusionRelief { get; set; } = 0.10;
        public double ExplicitAnxietySpike { get; set; } = 0.30;
        public double ExplicitConfusionSpike { get; set; } = 0.10;
        public double DecayPerSecond { get; set; } = 0.08;

        /// <summary>Raise confusion when the utterance demanded structure beyond the learner's stage.</summary>
        public void ApplyDeviation(DeviationResult deviation)
        {
            if (deviation == null || !deviation.StructureBeyondStage)
            {
                return;
            }

            Confusion = Clamp01(Confusion + ConfusionPerStageGap * deviation.StageGap);
        }

        /// <summary>Raise confusion when the teacher spoke faster than the comfortable pace.</summary>
        public void ApplyPacing(PacingResult pacing)
        {
            if (pacing == null || !pacing.TooFast)
            {
                return;
            }

            Confusion = Clamp01(Confusion + ConfusionPerExcessMora * pacing.ExcessMoraePerSecond);
        }

        /// <summary>Apply the affective consequences of a corrective-feedback move.</summary>
        public void ApplyCorrection(CorrectionType correction)
        {
            if (correction == CorrectionType.Recast)
            {
                Anxiety = Clamp01(Anxiety - RecastAnxietyRelief);
                Confusion = Clamp01(Confusion - RecastConfusionRelief);
            }
            else // ExplicitCorrection
            {
                Anxiety = Clamp01(Anxiety + ExplicitAnxietySpike);
                Confusion = Clamp01(Confusion + ExplicitConfusionSpike);
            }
        }

        /// <summary>Relax both signals toward zero by the elapsed time.</summary>
        public void Decay(double deltaSeconds)
        {
            if (deltaSeconds <= 0)
            {
                return;
            }

            double amount = DecayPerSecond * deltaSeconds;
            Confusion = Clamp01(Confusion - amount);
            Anxiety = Clamp01(Anxiety - amount);
        }

        public void Reset()
        {
            Confusion = 0;
            Anxiety = 0;
        }

        private static double Clamp01(double value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value > 1 ? 1 : value;
        }
    }
}
