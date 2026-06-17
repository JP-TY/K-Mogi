using System;
using System.Collections.Generic;
using KMogi.Core.Parsing;

namespace KMogi.Runtime.Parsing
{
    /// <summary>
    /// Wraps the managed NMeCab analyzer behind the Core <see cref="IMorphologicalParser"/> seam,
    /// loading an IPADIC dictionary from StreamingAssets. This is the production parser; the demo
    /// uses <see cref="FixtureMorphologicalParser"/> until it is enabled.
    ///
    /// To enable:
    ///   1. Add the managed NMeCab assembly (e.g. NMeCab.dll) under Assets/ (Plugins) and reference
    ///      it from KMogi.Runtime.asmdef (or rely on auto-referencing).
    ///   2. Place an IPADIC dictionary under Assets/StreamingAssets/ipadic/ (BSD-style license).
    ///   3. Define the scripting symbol KMOGI_NMECAB (Project Settings → Player → Scripting Define
    ///      Symbols) so the real implementation below is compiled in.
    /// Note: NMeCab forks differ slightly; the feature-CSV mapping below targets the classic
    /// MeCab/IPADIC layout and may need a small tweak for a specific fork.
    /// </summary>
#if KMOGI_NMECAB
    public sealed class NMeCabParser : IMorphologicalParser, IDisposable
    {
        private readonly NMeCab.MeCabTagger _tagger;

        public NMeCabParser(string dictionaryDirectory)
        {
            var param = new NMeCab.MeCabParam { DicDir = dictionaryDirectory };
            _tagger = NMeCab.MeCabTagger.Create(param);
        }

        public IReadOnlyList<Token> Parse(string text)
        {
            var tokens = new List<Token>();
            if (string.IsNullOrEmpty(text))
            {
                return tokens;
            }

            for (NMeCab.MeCabNode node = _tagger.ParseToNode(text); node != null; node = node.Next)
            {
                if (node.Stat == NMeCab.MeCabNodeStat.Bos || node.Stat == NMeCab.MeCabNodeStat.Eos)
                {
                    continue;
                }

                string surface = node.Surface;
                if (string.IsNullOrEmpty(surface))
                {
                    continue;
                }

                // IPADIC feature CSV: 品詞,細分類1,細分類2,細分類3,活用型,活用形,原形,読み,発音
                string[] f = (node.Feature ?? string.Empty).Split(',');
                string pos = f.Length > 0 ? f[0] : string.Empty;
                string detail1 = f.Length > 1 ? f[1] : string.Empty;
                string baseForm = f.Length > 6 && f[6] != "*" ? f[6] : surface;
                string reading = f.Length > 7 && f[7] != "*" ? f[7] : string.Empty;

                tokens.Add(new Token(surface, MapPartOfSpeech(pos, detail1), reading, baseForm, detail1));
            }

            return tokens;
        }

        private static PartOfSpeech MapPartOfSpeech(string pos, string detail1)
        {
            switch (pos)
            {
                case "名詞":
                    return detail1 == "代名詞" ? PartOfSpeech.Pronoun : PartOfSpeech.Noun;
                case "動詞":
                    return PartOfSpeech.Verb;
                case "形容詞":
                    return PartOfSpeech.Adjective;
                case "形容動詞":
                case "形状詞":
                    return PartOfSpeech.AdjectivalNoun;
                case "副詞":
                    return PartOfSpeech.Adverb;
                case "助詞":
                    return PartOfSpeech.Particle;
                case "助動詞":
                    return PartOfSpeech.AuxiliaryVerb;
                case "接続詞":
                    return PartOfSpeech.Conjunction;
                case "接頭詞":
                    return PartOfSpeech.Prefix;
                case "接尾辞":
                    return PartOfSpeech.Suffix;
                case "感動詞":
                    return PartOfSpeech.Interjection;
                case "記号":
                    return PartOfSpeech.Symbol;
                case "フィラー":
                    return PartOfSpeech.Filler;
                default:
                    return PartOfSpeech.Unknown;
            }
        }

        public void Dispose()
        {
            (_tagger as IDisposable)?.Dispose();
        }
    }
#else
    /// <summary>
    /// Inactive placeholder compiled when KMOGI_NMECAB is not defined. Instantiating it fails fast
    /// with setup guidance; the demo never constructs it (it uses the fixture parser instead).
    /// </summary>
    public sealed class NMeCabParser : IMorphologicalParser
    {
        public NMeCabParser(string dictionaryDirectory)
        {
            throw new NotSupportedException(
                "NMeCabParser is disabled. Add the NMeCab assembly + an IPADIC dictionary under " +
                "Assets/StreamingAssets/ipadic/ and define the KMOGI_NMECAB scripting symbol. " +
                "Until then, use FixtureMorphologicalParser.");
        }

        public IReadOnlyList<Token> Parse(string text) => Array.Empty<Token>();
    }
#endif
}
