using System.Collections.Generic;
using KMogi.Core.Parsing;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class SovSyntaxValidatorTests
    {
        private readonly SovSyntaxValidator _validator = new SovSyntaxValidator();

        [Test]
        public void CanonicalSentence_IdentifiesSubjectObjectVerbInOrder()
        {
            // 私は本を読む — "I read a book"
            var tokens = new List<Token>
            {
                Tok.Pronoun("私"),
                Tok.Particle("は"),
                Tok.Noun("本"),
                Tok.Particle("を"),
                Tok.Verb("読む"),
            };

            SovAnalysis analysis = _validator.Analyze(tokens);

            Assert.IsTrue(analysis.IsCanonicalSov);
            Assert.AreEqual(0, analysis.SubjectIndex);
            Assert.AreEqual(2, analysis.ObjectIndex);
            Assert.AreEqual(4, analysis.VerbIndex);
        }

        [Test]
        public void EmptySequence_IsNotCanonicalAndHasNoConstituents()
        {
            SovAnalysis analysis = _validator.Analyze(new List<Token>());

            Assert.IsFalse(analysis.IsCanonicalSov);
            Assert.IsFalse(analysis.HasSubject);
            Assert.IsFalse(analysis.HasObject);
            Assert.IsFalse(analysis.HasVerb);
        }

        [Test]
        public void SentenceWithoutVerb_IsNotCanonical()
        {
            var tokens = new List<Token>
            {
                Tok.Noun("猫"),
                Tok.Particle("が"),
            };

            SovAnalysis analysis = _validator.Analyze(tokens);

            Assert.IsFalse(analysis.IsCanonicalSov);
            Assert.IsTrue(analysis.HasSubject);
            Assert.IsFalse(analysis.HasVerb);
        }

        [Test]
        public void NullInput_IsHandledSafely()
        {
            SovAnalysis analysis = _validator.Analyze(null);
            Assert.IsFalse(analysis.IsCanonicalSov);
            Assert.AreEqual(-1, analysis.VerbIndex);
        }
    }
}
