namespace KMogi.Core.Telemetry
{
    /// <summary>
    /// Accumulates teacher vs. student speaking time and reports the Teacher Talk Time ratio.
    /// The PRD leaves the formula unstated; it is defined here explicitly as
    /// <c>TTT = teacherTime / (teacherTime + studentTime)</c>, with an Irodori target of ≤ 0.40.
    /// </summary>
    public sealed class TttCalculator
    {
        /// <summary>Irodori lesson target: teacher talk time should not exceed this share.</summary>
        public const double TargetMaxRatio = 0.40;

        public double TeacherSeconds { get; private set; }
        public double StudentSeconds { get; private set; }

        public double TotalSeconds => TeacherSeconds + StudentSeconds;

        /// <summary>Teacher Talk Time ratio in [0,1]; 0 when no speech has been recorded.</summary>
        public double Ratio => TotalSeconds <= 0 ? 0 : TeacherSeconds / TotalSeconds;

        /// <summary>True while the running ratio is at or below <see cref="TargetMaxRatio"/>.</summary>
        public bool WithinTarget => Ratio <= TargetMaxRatio;

        public void AddTeacherSpeech(double seconds)
        {
            if (seconds > 0)
            {
                TeacherSeconds += seconds;
            }
        }

        public void AddStudentSpeech(double seconds)
        {
            if (seconds > 0)
            {
                StudentSeconds += seconds;
            }
        }

        public void Reset()
        {
            TeacherSeconds = 0;
            StudentSeconds = 0;
        }
    }
}
