namespace KMogi.Core.Parsing
{
    /// <summary>
    /// An immutable morphological token produced by an <see cref="IMorphologicalParser"/>.
    /// </summary>
    public sealed class Token
    {
        /// <summary>The surface form as it appeared in the input text.</summary>
        public string Surface { get; }

        /// <summary>Coarse part-of-speech category.</summary>
        public PartOfSpeech PartOfSpeech { get; }

        /// <summary>Katakana reading (pronunciation). May be empty when unknown.</summary>
        public string Reading { get; }

        /// <summary>Dictionary / base form. Falls back to <see cref="Surface"/> when not supplied.</summary>
        public string BaseForm { get; }

        /// <summary>
        /// Raw dictionary sub-category, e.g. "格助詞" or "接続助詞", preserved verbatim for
        /// finer-grained rules. Empty when the parser does not expose it.
        /// </summary>
        public string PartOfSpeechDetail { get; }

        public Token(
            string surface,
            PartOfSpeech partOfSpeech,
            string reading = "",
            string baseForm = "",
            string partOfSpeechDetail = "")
        {
            Surface = surface ?? string.Empty;
            PartOfSpeech = partOfSpeech;
            Reading = reading ?? string.Empty;
            BaseForm = string.IsNullOrEmpty(baseForm) ? Surface : baseForm;
            PartOfSpeechDetail = partOfSpeechDetail ?? string.Empty;
        }

        /// <summary>True when this token is the particle with the given surface form.</summary>
        public bool IsParticle(string surface)
            => PartOfSpeech == PartOfSpeech.Particle && Surface == surface;

        /// <summary>True for nominals (nouns and pronouns) that can head a phrase.</summary>
        public bool IsNominal
            => PartOfSpeech == PartOfSpeech.Noun || PartOfSpeech == PartOfSpeech.Pronoun;

        public override string ToString() => Surface + "/" + PartOfSpeech;
    }
}
