namespace KMogi.Core.Sla
{
    /// <summary>Result of measuring the delivery speed of a single utterance.</summary>
    public sealed class PacingResult
    {
        public int Morae { get; }
        public double Seconds { get; }
        public double MoraePerSecond { get; }
        public double ThresholdMoraePerSecond { get; }
        public bool TooFast { get; }

        /// <summary>Morae-per-second above the comfortable threshold (0 when within pace).</summary>
        public double ExcessMoraePerSecond { get; }

        public PacingResult(int morae, double seconds, double moraePerSecond, double threshold)
        {
            Morae = morae;
            Seconds = seconds;
            MoraePerSecond = moraePerSecond;
            ThresholdMoraePerSecond = threshold;
            TooFast = moraePerSecond > threshold;
            double excess = moraePerSecond - threshold;
            ExcessMoraePerSecond = excess > 0 ? excess : 0;
        }
    }

    /// <summary>
    /// Measures teacher speech rate in morae per second and flags delivery that is too fast for
    /// a learner to follow. Native conversational Japanese runs ~7-8 morae/s; comprehension for
    /// learners degrades well below that, so the default comfortable ceiling is lower and tunable.
    /// </summary>
    public sealed class PacingMonitor
    {
        public const double DefaultThresholdMoraePerSecond = 6.5;

        public double ThresholdMoraePerSecond { get; }

        public PacingMonitor(double thresholdMoraePerSecond = DefaultThresholdMoraePerSecond)
        {
            ThresholdMoraePerSecond = thresholdMoraePerSecond > 0
                ? thresholdMoraePerSecond
                : DefaultThresholdMoraePerSecond;
        }

        /// <summary>
        /// Measure the pace of an utterance given its kana <paramref name="reading"/> and the
        /// wall-clock <paramref name="seconds"/> it took to deliver.
        /// </summary>
        public PacingResult Measure(string reading, double seconds)
        {
            int morae = MoraCounter.Count(reading);
            double rate = seconds > 0 ? morae / seconds : 0;
            return new PacingResult(morae, seconds, rate, ThresholdMoraePerSecond);
        }
    }
}
