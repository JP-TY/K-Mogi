using System.Linq;
using KMogi.Core.Telemetry;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class TttCalculatorTests
    {
        [Test]
        public void FreshCalculator_HasZeroRatioAndIsWithinTarget()
        {
            var ttt = new TttCalculator();

            Assert.AreEqual(0.0, ttt.Ratio, 1e-6);
            Assert.IsTrue(ttt.WithinTarget);
        }

        [Test]
        public void BalancedSession_StaysWithinTarget()
        {
            var ttt = new TttCalculator();
            ttt.AddTeacherSpeech(30);
            ttt.AddStudentSpeech(70);

            Assert.AreEqual(0.30, ttt.Ratio, 1e-6);
            Assert.IsTrue(ttt.WithinTarget);
        }

        [Test]
        public void TeacherDominatedSession_ExceedsTarget()
        {
            var ttt = new TttCalculator();
            ttt.AddTeacherSpeech(60);
            ttt.AddStudentSpeech(40);

            Assert.AreEqual(0.60, ttt.Ratio, 1e-6);
            Assert.IsFalse(ttt.WithinTarget);
        }

        [Test]
        public void NonPositiveDurations_AreIgnored()
        {
            var ttt = new TttCalculator();
            ttt.AddTeacherSpeech(-5);
            ttt.AddStudentSpeech(0);
            ttt.AddTeacherSpeech(10);

            Assert.AreEqual(10.0, ttt.TeacherSeconds, 1e-6);
            Assert.AreEqual(0.0, ttt.StudentSeconds, 1e-6);
        }
    }

    public sealed class GazeQuadrantAccumulatorTests
    {
        [Test]
        public void Classify_PartitionsYawIntoQuadrants()
        {
            var gaze = new GazeQuadrantAccumulator(20.0);

            Assert.AreEqual(GazeQuadrant.Left, gaze.Classify(-30));
            Assert.AreEqual(GazeQuadrant.Center, gaze.Classify(0));
            Assert.AreEqual(GazeQuadrant.Right, gaze.Classify(30));
            Assert.AreEqual(GazeQuadrant.Center, gaze.Classify(-20)); // boundary is inclusive of center
            Assert.AreEqual(GazeQuadrant.Center, gaze.Classify(20));
        }

        [Test]
        public void Shares_AndNeglectedQuadrants_AreComputedFromDwellTime()
        {
            var gaze = new GazeQuadrantAccumulator(20.0);
            gaze.AddSampleByYaw(-30, 80); // Left
            gaze.AddSampleByYaw(0, 20);    // Center
            gaze.AddSampleByYaw(30, 5);    // Right

            Assert.AreEqual(105.0, gaze.TotalSeconds, 1e-6);
            Assert.AreEqual(80.0 / 105.0, gaze.ShareOf(GazeQuadrant.Left), 1e-6);

            var neglected = gaze.NeglectedQuadrants(0.25);
            Assert.AreEqual(2, neglected.Count);
            Assert.IsTrue(neglected.Contains(GazeQuadrant.Center));
            Assert.IsTrue(neglected.Contains(GazeQuadrant.Right));
        }

        [Test]
        public void EmptyAccumulator_HasNoSharesOrNeglect()
        {
            var gaze = new GazeQuadrantAccumulator();

            Assert.AreEqual(0.0, gaze.ShareOf(GazeQuadrant.Left), 1e-6);
            Assert.AreEqual(0, gaze.NeglectedQuadrants(0.25).Count);
        }
    }
}
