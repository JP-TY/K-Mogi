namespace KMogi.Runtime.Dialogue
{
    public enum StepKind
    {
        /// <summary>The trainee teacher delivers an utterance that is evaluated against the learner.</summary>
        TeacherUtterance,

        /// <summary>The learner produces an erroneous utterance the teacher must choose how to correct.</summary>
        StudentError
    }

    /// <summary>
    /// One beat of the scripted micro-lesson. Authored data (Japanese + romaji + English gloss +
    /// the spoken duration used for pacing/TTT) so the demo runs deterministically. Promoting this
    /// to a ScriptableObject for non-engineer authoring is a documented follow-up.
    /// </summary>
    public sealed class LessonStep
    {
        public StepKind Kind { get; }
        public string Japanese { get; }
        public string Romaji { get; }
        public string Gloss { get; }

        /// <summary>How long the line takes to deliver, in seconds (drives pacing and TTT).</summary>
        public double DurationSeconds { get; }

        // For StudentError steps: the well-formed target the teacher would model in a recast.
        public string CorrectJapanese { get; }
        public string CorrectRomaji { get; }

        private LessonStep(
            StepKind kind, string japanese, string romaji, string gloss, double durationSeconds,
            string correctJapanese, string correctRomaji)
        {
            Kind = kind;
            Japanese = japanese;
            Romaji = romaji;
            Gloss = gloss;
            DurationSeconds = durationSeconds;
            CorrectJapanese = correctJapanese;
            CorrectRomaji = correctRomaji;
        }

        public static LessonStep Teacher(string japanese, string romaji, string gloss, double durationSeconds)
            => new LessonStep(StepKind.TeacherUtterance, japanese, romaji, gloss, durationSeconds, null, null);

        public static LessonStep StudentError(
            string japanese, string romaji, string gloss, double durationSeconds,
            string correctJapanese, string correctRomaji)
            => new LessonStep(StepKind.StudentError, japanese, romaji, gloss, durationSeconds, correctJapanese, correctRomaji);
    }
}
