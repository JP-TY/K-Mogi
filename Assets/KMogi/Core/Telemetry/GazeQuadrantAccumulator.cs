using System.Collections.Generic;

namespace KMogi.Core.Telemetry
{
    /// <summary>Horizontal region of the virtual classroom the instructor is facing.</summary>
    public enum GazeQuadrant
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Accumulates how long the instructor's gaze dwells in the left/center/right regions of the
    /// room, building the data behind the Gaze Distribution Metric (GDM). Classification is by head
    /// yaw, using the Unity convention that positive yaw turns to the right and negative to the left;
    /// yaw within ±<see cref="CenterHalfAngleDegrees"/> counts as center. The accumulator itself is a
    /// pure model — the runtime feeds it yaw samples derived from the platform head pose.
    /// </summary>
    public sealed class GazeQuadrantAccumulator
    {
        public const double DefaultCenterHalfAngleDegrees = 20.0;

        public double CenterHalfAngleDegrees { get; }

        public double LeftSeconds { get; private set; }
        public double CenterSeconds { get; private set; }
        public double RightSeconds { get; private set; }

        public double TotalSeconds => LeftSeconds + CenterSeconds + RightSeconds;

        public GazeQuadrantAccumulator(double centerHalfAngleDegrees = DefaultCenterHalfAngleDegrees)
        {
            CenterHalfAngleDegrees = centerHalfAngleDegrees > 0
                ? centerHalfAngleDegrees
                : DefaultCenterHalfAngleDegrees;
        }

        /// <summary>Classify a signed head-yaw angle (degrees) into a quadrant.</summary>
        public GazeQuadrant Classify(double yawDegrees)
        {
            if (yawDegrees < -CenterHalfAngleDegrees)
            {
                return GazeQuadrant.Left;
            }

            if (yawDegrees > CenterHalfAngleDegrees)
            {
                return GazeQuadrant.Right;
            }

            return GazeQuadrant.Center;
        }

        public void AddSample(GazeQuadrant quadrant, double seconds)
        {
            if (seconds <= 0)
            {
                return;
            }

            switch (quadrant)
            {
                case GazeQuadrant.Left:
                    LeftSeconds += seconds;
                    break;
                case GazeQuadrant.Center:
                    CenterSeconds += seconds;
                    break;
                case GazeQuadrant.Right:
                    RightSeconds += seconds;
                    break;
            }
        }

        /// <summary>Classify <paramref name="yawDegrees"/> and accumulate the dwell time.</summary>
        public void AddSampleByYaw(double yawDegrees, double seconds)
            => AddSample(Classify(yawDegrees), seconds);

        public double SecondsOf(GazeQuadrant quadrant)
        {
            switch (quadrant)
            {
                case GazeQuadrant.Left: return LeftSeconds;
                case GazeQuadrant.Center: return CenterSeconds;
                default: return RightSeconds;
            }
        }

        /// <summary>Fraction of total dwell time spent in <paramref name="quadrant"/> (0 when no data).</summary>
        public double ShareOf(GazeQuadrant quadrant)
        {
            double total = TotalSeconds;
            return total <= 0 ? 0 : SecondsOf(quadrant) / total;
        }

        /// <summary>
        /// Quadrants the instructor under-attended, i.e. whose share fell below
        /// <paramref name="minShare"/>. Returns an empty list before any samples are recorded.
        /// </summary>
        public IReadOnlyList<GazeQuadrant> NeglectedQuadrants(double minShare)
        {
            var neglected = new List<GazeQuadrant>();
            if (TotalSeconds <= 0)
            {
                return neglected;
            }

            foreach (GazeQuadrant quadrant in new[] { GazeQuadrant.Left, GazeQuadrant.Center, GazeQuadrant.Right })
            {
                if (ShareOf(quadrant) < minShare)
                {
                    neglected.Add(quadrant);
                }
            }

            return neglected;
        }

        public void Reset()
        {
            LeftSeconds = 0;
            CenterSeconds = 0;
            RightSeconds = 0;
        }
    }
}
