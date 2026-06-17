using System.Collections.Generic;

namespace KMogi.Core.Sla
{
    /// <summary>
    /// Counts morae in a kana string. Japanese rhythm is mora-timed, so pacing is measured in
    /// morae rather than English-style "words per minute". Input is expected to be the katakana
    /// (or hiragana) reading of an utterance, as produced by the morphological parser.
    /// Rules: each full kana = 1 mora; small yōon/vowel kana (ゃゅょ, ぁぃぅぇぉ, etc.) attach to
    /// the preceding mora and count 0; the sokuon (っ/ッ), moraic nasal (ん/ン) and the long-vowel
    /// mark (ー) each count as 1 mora; non-kana characters are ignored.
    /// </summary>
    public static class MoraCounter
    {
        private static readonly HashSet<char> SmallKana = new HashSet<char>
        {
            // Hiragana small vowels and yōon
            'ぁ', 'ぃ', 'ぅ', 'ぇ', 'ぉ', // ぁぃぅぇぉ
            'ゃ', 'ゅ', 'ょ',                     // ゃゅょ
            'ゎ',                                         // ゎ
            'ゕ', 'ゖ',                               // ゕゖ
            // Katakana small vowels and yōon
            'ァ', 'ィ', 'ゥ', 'ェ', 'ォ', // ァィゥェォ
            'ャ', 'ュ', 'ョ',                     // ャュョ
            'ヮ',                                         // ヮ
            'ヵ', 'ヶ'                                // ヵヶ
        };

        public static int Count(string reading)
        {
            if (string.IsNullOrEmpty(reading))
            {
                return 0;
            }

            int morae = 0;
            foreach (char c in reading)
            {
                if (SmallKana.Contains(c))
                {
                    continue; // attaches to the preceding mora
                }

                if (IsCountableKana(c))
                {
                    morae++;
                }
            }

            return morae;
        }

        private static bool IsCountableKana(char c)
        {
            // Hiragana block (excluding the small kana already filtered out).
            if (c >= 'ぁ' && c <= 'ゖ')
            {
                return true;
            }

            // Katakana block.
            if (c >= 'ァ' && c <= 'ヺ')
            {
                return true;
            }

            // Long-vowel mark ー and katakana iteration marks ヽヾ.
            return c == 'ー' || c == 'ヽ' || c == 'ヾ';
        }
    }
}
