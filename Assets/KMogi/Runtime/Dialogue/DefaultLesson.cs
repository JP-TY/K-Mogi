using System.Collections.Generic;

namespace KMogi.Runtime.Dialogue
{
    /// <summary>
    /// The built-in scripted micro-lesson used by the showcase. It is tuned for a Stage-2 learner so
    /// the arc reads clearly: calm in-reach input → mild over-reach (adjective) → strong over-reach
    /// (relative clause) → too-fast passive → a learner particle error to correct → calm recovery.
    /// All surfaces are covered by <see cref="Parsing.FixtureMorphologicalParser"/>'s lexicon.
    /// </summary>
    public static class DefaultLesson
    {
        public static IReadOnlyList<LessonStep> CreateSteps()
        {
            return new List<LessonStep>
            {
                LessonStep.Teacher("私は本を読む", "watashi wa hon o yomu",
                    "I read a book. (canonical, in reach)", 2.4),

                LessonStep.Teacher("高い山", "takai yama",
                    "a tall mountain (adjective — Stage 3)", 1.6),

                LessonStep.Teacher("本を読む人", "hon o yomu hito",
                    "the person who reads a book (relative clause — Stage 5)", 1.8),

                LessonStep.Teacher("水が飲まれる", "mizu ga nomareru",
                    "water is drunk (passive — Stage 4, delivered too fast)", 0.8),

                LessonStep.StudentError("私は本が読む", "watashi wa hon ga yomu",
                    "learner uses が where を is needed", 2.2,
                    "私は本を読む", "watashi wa hon o yomu"),

                LessonStep.Teacher("学生は水を飲む", "gakusei wa mizu o nomu",
                    "the student drinks water (canonical, recovery)", 2.5),
            };
        }
    }
}
