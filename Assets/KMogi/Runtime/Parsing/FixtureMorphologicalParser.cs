using System;
using System.Collections.Generic;
using KMogi.Core.Parsing;

namespace KMogi.Runtime.Parsing
{
    /// <summary>
    /// A dictionary-driven longest-match tokenizer over a small curated lexicon. It is NOT a
    /// general Japanese analyzer — it exists so the full parse → PT-evaluation → avatar pipeline
    /// is demoable in the editor today, before the NMeCab + IPADIC dictionary is added. Any
    /// surface in the lexicon (which covers the canned lesson plus common words) tokenizes with a
    /// correct part of speech and katakana reading; unknown runs fall back to single-character
    /// tokens so the flow never breaks.
    /// </summary>
    public sealed class FixtureMorphologicalParser : IMorphologicalParser
    {
        private sealed class Entry
        {
            public PartOfSpeech Pos;
            public string Reading;
            public string BaseForm;
            public string Romaji;
        }

        private readonly Dictionary<string, Entry> _lexicon;
        private readonly int _maxKeyLength;

        public FixtureMorphologicalParser()
        {
            _lexicon = BuildLexicon();
            int max = 1;
            foreach (string key in _lexicon.Keys)
            {
                if (key.Length > max)
                {
                    max = key.Length;
                }
            }

            _maxKeyLength = max;
        }

        public IReadOnlyList<Token> Parse(string text)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(text))
            {
                return tokens;
            }

            int i = 0;
            while (i < text.Length)
            {
                if (char.IsWhiteSpace(text[i]))
                {
                    i++;
                    continue;
                }

                Entry matched = null;
                int matchedLength = 0;
                int maxLen = Math.Min(_maxKeyLength, text.Length - i);
                for (int len = maxLen; len >= 1; len--)
                {
                    string candidate = text.Substring(i, len);
                    if (_lexicon.TryGetValue(candidate, out Entry entry))
                    {
                        matched = entry;
                        matchedLength = len;
                        break;
                    }
                }

                if (matched != null)
                {
                    tokens.Add(new Token(
                        text.Substring(i, matchedLength),
                        matched.Pos,
                        matched.Reading,
                        matched.BaseForm));
                    i += matchedLength;
                }
                else
                {
                    tokens.Add(new Token(text[i].ToString(), PartOfSpeech.Unknown));
                    i++;
                }
            }

            return tokens;
        }

        /// <summary>
        /// Romaji for a surface form, for legible display when no Japanese-capable font is assigned.
        /// Falls back to the surface itself for out-of-lexicon tokens.
        /// </summary>
        public string Romanize(string surface)
        {
            if (string.IsNullOrEmpty(surface))
            {
                return string.Empty;
            }

            return _lexicon.TryGetValue(surface, out Entry entry) && !string.IsNullOrEmpty(entry.Romaji)
                ? entry.Romaji
                : surface;
        }

        private static Dictionary<string, Entry> BuildLexicon()
        {
            var lex = new Dictionary<string, Entry>();

            void Add(string surface, PartOfSpeech pos, string reading, string baseForm, string romaji)
            {
                lex[surface] = new Entry { Pos = pos, Reading = reading, BaseForm = baseForm, Romaji = romaji };
            }

            // Pronouns
            Add("私", PartOfSpeech.Pronoun, "ワタシ", "私", "watashi");
            Add("わたし", PartOfSpeech.Pronoun, "ワタシ", "わたし", "watashi");

            // Nouns
            Add("先生", PartOfSpeech.Noun, "センセイ", "先生", "sensei");
            Add("学生", PartOfSpeech.Noun, "ガクセイ", "学生", "gakusei");
            Add("本", PartOfSpeech.Noun, "ホン", "本", "hon");
            Add("ほん", PartOfSpeech.Noun, "ホン", "ほん", "hon");
            Add("人", PartOfSpeech.Noun, "ヒト", "人", "hito");
            Add("ひと", PartOfSpeech.Noun, "ヒト", "ひと", "hito");
            Add("水", PartOfSpeech.Noun, "ミズ", "水", "mizu");
            Add("山", PartOfSpeech.Noun, "ヤマ", "山", "yama");

            // Case / topic particles (は read 'wa' as a topic marker)
            Add("は", PartOfSpeech.Particle, "ワ", "は", "wa");
            Add("が", PartOfSpeech.Particle, "ガ", "が", "ga");
            Add("を", PartOfSpeech.Particle, "ヲ", "を", "o");
            Add("に", PartOfSpeech.Particle, "ニ", "に", "ni");
            Add("で", PartOfSpeech.Particle, "デ", "で", "de");
            Add("て", PartOfSpeech.Particle, "テ", "て", "te");

            // Verbs (plain forms and the bare stems used before auxiliaries)
            Add("読む", PartOfSpeech.Verb, "ヨム", "読む", "yomu");
            Add("よむ", PartOfSpeech.Verb, "ヨム", "よむ", "yomu");
            Add("読ま", PartOfSpeech.Verb, "ヨマ", "読む", "yoma");
            Add("飲む", PartOfSpeech.Verb, "ノム", "飲む", "nomu");
            Add("飲ま", PartOfSpeech.Verb, "ノマ", "飲む", "noma");
            Add("食べる", PartOfSpeech.Verb, "タベル", "食べる", "taberu");
            Add("食べ", PartOfSpeech.Verb, "タベ", "食べる", "tabe");
            Add("行く", PartOfSpeech.Verb, "イク", "行く", "iku");

            // Adjectives
            Add("高い", PartOfSpeech.Adjective, "タカイ", "高い", "takai");
            Add("大きい", PartOfSpeech.Adjective, "オオキイ", "大きい", "ookii");

            // Auxiliary verbs (passive / causative / polite / copula)
            Add("れる", PartOfSpeech.AuxiliaryVerb, "レル", "れる", "reru");
            Add("られる", PartOfSpeech.AuxiliaryVerb, "ラレル", "られる", "rareru");
            Add("せる", PartOfSpeech.AuxiliaryVerb, "セル", "せる", "seru");
            Add("させる", PartOfSpeech.AuxiliaryVerb, "サセル", "させる", "saseru");
            Add("ます", PartOfSpeech.AuxiliaryVerb, "マス", "ます", "masu");
            Add("です", PartOfSpeech.AuxiliaryVerb, "デス", "です", "desu");
            Add("だ", PartOfSpeech.AuxiliaryVerb, "ダ", "だ", "da");

            // Punctuation
            Add("。", PartOfSpeech.Symbol, string.Empty, "。", ".");
            Add("、", PartOfSpeech.Symbol, string.Empty, "、", ",");

            return lex;
        }
    }
}
