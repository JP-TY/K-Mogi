using KMogi.Core.Sla;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class MoraCounterTests
    {
        [TestCase("ニホンゴ", 4)]   // ni-ho-n-go
        [TestCase("ガッコウ", 4)]   // ga-(sokuon)-ko-u
        [TestCase("トーキョー", 4)] // to-(long)-kyo-(long)
        [TestCase("キョウ", 2)]      // kyo-u (small ョ attaches)
        [TestCase("", 0)]
        public void Count_MatchesExpectedMorae(string reading, int expected)
        {
            Assert.AreEqual(expected, MoraCounter.Count(reading));
        }

        [Test]
        public void Count_NullIsZero()
        {
            Assert.AreEqual(0, MoraCounter.Count(null));
        }

        [Test]
        public void Count_IgnoresNonKanaCharacters()
        {
            // Kanji carry no reading here, so only the katakana morae are counted.
            Assert.AreEqual(2, MoraCounter.Count("本ヨム"));
        }
    }

    public sealed class PacingMonitorTests
    {
        [Test]
        public void FastDelivery_IsFlaggedWithExcessOverThreshold()
        {
            var monitor = new PacingMonitor(); // default 6.5 morae/s

            // 4 morae spoken in 0.5 s => 8.0 morae/s.
            PacingResult result = monitor.Measure("ニホンゴ", 0.5);

            Assert.AreEqual(4, result.Morae);
            Assert.AreEqual(8.0, result.MoraePerSecond, 1e-6);
            Assert.IsTrue(result.TooFast);
            Assert.AreEqual(1.5, result.ExcessMoraePerSecond, 1e-6);
        }

        [Test]
        public void ComfortableDelivery_IsNotFlagged()
        {
            var monitor = new PacingMonitor();

            // 4 morae in 1.0 s => 4.0 morae/s, under the 6.5 ceiling.
            PacingResult result = monitor.Measure("ニホンゴ", 1.0);

            Assert.IsFalse(result.TooFast);
            Assert.AreEqual(0.0, result.ExcessMoraePerSecond, 1e-6);
        }

        [Test]
        public void ZeroDuration_YieldsZeroRateAndIsNotFlagged()
        {
            var monitor = new PacingMonitor();

            PacingResult result = monitor.Measure("ニホンゴ", 0.0);

            Assert.AreEqual(0.0, result.MoraePerSecond, 1e-6);
            Assert.IsFalse(result.TooFast);
        }

        [Test]
        public void CustomThreshold_IsRespected()
        {
            var monitor = new PacingMonitor(3.0);

            PacingResult result = monitor.Measure("ニホンゴ", 1.0); // 4 morae/s

            Assert.IsTrue(result.TooFast);
            Assert.AreEqual(1.0, result.ExcessMoraePerSecond, 1e-6);
        }

        [Test]
        public void NonPositiveThreshold_FallsBackToDefault()
        {
            var monitor = new PacingMonitor(0);
            Assert.AreEqual(PacingMonitor.DefaultThresholdMoraePerSecond, monitor.ThresholdMoraePerSecond, 1e-6);
        }
    }
}
