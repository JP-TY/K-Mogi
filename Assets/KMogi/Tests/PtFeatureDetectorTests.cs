using System.Collections.Generic;
using System.Linq;
using KMogi.Core.Parsing;
using KMogi.Core.Sla;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class PtFeatureDetectorTests
    {
        private readonly PtFeatureDetector _detector = new PtFeatureDetector();

        [Test]
        public void SingleNoun_IsDetectedAsSingleWord()
        {
            var features = _detector.Detect(new List<Token> { Tok.Noun("本") });

            Assert.IsTrue(features.Contains(PtFeature.SingleWord));
            Assert.IsFalse(features.Contains(PtFeature.CanonicalSov));
        }

        [Test]
        public void CanonicalSentence_DetectsParticleAttachmentAndSovButNotSingleWord()
        {
            var tokens = new List<Token>
            {
                Tok.Pronoun("私"),
                Tok.Particle("は"),
                Tok.Noun("本"),
                Tok.Particle("を"),
                Tok.Verb("読む"),
            };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.ParticleAttachment));
            Assert.IsTrue(features.Contains(PtFeature.CanonicalSov));
            Assert.IsFalse(features.Contains(PtFeature.SingleWord));
        }

        [Test]
        public void VerbModifyingFollowingNoun_DetectsNounModifyingClause()
        {
            // 本を読む人 — "the person who reads a book"
            var tokens = new List<Token>
            {
                Tok.Noun("本"),
                Tok.Particle("を"),
                Tok.Verb("読む"),
                Tok.Noun("人"),
            };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.NounModifyingClause));
        }

        [Test]
        public void Adjective_IsDetected()
        {
            var tokens = new List<Token> { Tok.Adjective("高い"), Tok.Noun("山") };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.Adjective));
        }

        [Test]
        public void PassiveAuxiliary_DetectsPassive()
        {
            var tokens = new List<Token> { Tok.Verb("食べ"), Tok.Aux("られる", "られる") };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.Passive));
        }

        [Test]
        public void CausativeAuxiliary_DetectsCausative()
        {
            var tokens = new List<Token> { Tok.Verb("食べ"), Tok.Aux("させる", "させる") };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.Causative));
        }

        [Test]
        public void TeFormConnective_DetectsTeForm()
        {
            var tokens = new List<Token> { Tok.Verb("食べ"), Tok.Particle("て") };

            var features = _detector.Detect(tokens);

            Assert.IsTrue(features.Contains(PtFeature.TeForm));
        }

        [Test]
        public void EmptyInput_YieldsNoFeatures()
        {
            Assert.AreEqual(0, _detector.Detect(new List<Token>()).Count);
            Assert.AreEqual(0, _detector.Detect(null).Count);
        }
    }
}
