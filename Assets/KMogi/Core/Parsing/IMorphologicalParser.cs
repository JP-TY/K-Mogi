using System.Collections.Generic;

namespace KMogi.Core.Parsing
{
    /// <summary>
    /// Segments raw Japanese text into morphological <see cref="Token"/>s.
    /// Implementations live in the Unity runtime assembly (e.g. an NMeCab wrapper)
    /// or in tests (a hand-authored fixture parser); the Core depends only on this seam.
    /// </summary>
    public interface IMorphologicalParser
    {
        /// <summary>
        /// Parse <paramref name="text"/> into tokens in surface order.
        /// Implementations must return an empty list (never null) for null/empty input.
        /// </summary>
        IReadOnlyList<Token> Parse(string text);
    }
}
