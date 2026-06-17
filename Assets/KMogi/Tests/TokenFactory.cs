using KMogi.Core.Parsing;

namespace KMogi.Tests
{
    /// <summary>
    /// Terse helpers for hand-building token sequences in tests. The Core has no concrete
    /// parser of its own (NMeCab lives in the runtime assembly), so Core logic is exercised
    /// against directly-authored tokens that stand in for parser output.
    /// </summary>
    internal static class Tok
    {
        public static Token Noun(string surface, string reading = "")
            => new Token(surface, PartOfSpeech.Noun, reading);

        public static Token Pronoun(string surface, string reading = "")
            => new Token(surface, PartOfSpeech.Pronoun, reading);

        public static Token Verb(string surface, string reading = "", string baseForm = "")
            => new Token(surface, PartOfSpeech.Verb, reading, baseForm);

        public static Token Particle(string surface)
            => new Token(surface, PartOfSpeech.Particle);

        public static Token Adjective(string surface, string reading = "")
            => new Token(surface, PartOfSpeech.Adjective, reading);

        public static Token Aux(string surface, string baseForm)
            => new Token(surface, PartOfSpeech.AuxiliaryVerb, "", baseForm);
    }
}
