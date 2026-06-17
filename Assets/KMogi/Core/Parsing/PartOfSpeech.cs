namespace KMogi.Core.Parsing
{
    /// <summary>
    /// Coarse part-of-speech categories used across the SLA pipeline.
    /// A concrete <see cref="IMorphologicalParser"/> (e.g. an NMeCab wrapper)
    /// maps its dictionary-specific tags down onto these values so that the
    /// platform-agnostic Core never depends on a particular dictionary schema.
    /// </summary>
    public enum PartOfSpeech
    {
        Unknown = 0,
        Noun,            // 名詞
        Pronoun,         // 代名詞
        Verb,            // 動詞
        Adjective,       // 形容詞 (i-adjective)
        AdjectivalNoun,  // 形容動詞 / 形状詞 (na-adjective)
        Adverb,          // 副詞
        Particle,        // 助詞
        AuxiliaryVerb,   // 助動詞
        Conjunction,     // 接続詞
        Prefix,          // 接頭辞
        Suffix,          // 接尾辞
        Interjection,    // 感動詞
        Symbol,          // 記号
        Filler           // フィラー
    }
}
