using System.Collections.Generic;
using KMogi.Core.Parsing;
using KMogi.Core.Sla;
using NUnit.Framework;

namespace KMogi.Tests
{
    public sealed class PtEvaluatorTests
    {
        private readonly PtEvaluator _evaluator = new PtEvaluator();

        [Test]
        public void RelativeClause_IsBeyondStage2Learner()
        {
            // 本を読む人 demands a Stage 5 noun-modifying clause.
            var tokens = new List<Token>
            {
                Tok.Noun("本"),
                Tok.Particle("を"),
                Tok.Verb("読む"),
                Tok.Noun("人"),
            };

            DeviationResult result = _evaluator.Evaluate(tokens, ProcessabilityStage.Stage2CanonicalOrder);

            Assert.IsTrue(result.StructureBeyondStage);
            Assert.AreEqual(ProcessabilityStage.Stage5Subordinate, result.HighestRequiredStage);
            Assert.AreEqual(PtFeature.NounModifyingClause, result.WorstFeature);
            Assert.AreEqual(3, result.StageGap);
        }

        [Test]
        public void ParticleSentence_IsWithinReachOfStage2Learner()
        {
            var tokens = new List<Token>
            {
                Tok.Pronoun("私"),
                Tok.Particle("は"),
                Tok.Noun("本"),
                Tok.Particle("を"),
                Tok.Verb("読む"),
            };

            DeviationResult result = _evaluator.Evaluate(tokens, ProcessabilityStage.Stage2CanonicalOrder);

            Assert.IsFalse(result.StructureBeyondStage);
            Assert.AreEqual(ProcessabilityStage.Stage2CanonicalOrder, result.HighestRequiredStage);
            Assert.AreEqual(0, result.StageGap);
        }

        [Test]
        public void CanonicalOrder_IsBeyondStage1Learner()
        {
            var tokens = new List<Token>
            {
                Tok.Pronoun("私"),
                Tok.Particle("は"),
                Tok.Verb("行く"),
            };

            DeviationResult result = _evaluator.Evaluate(tokens, ProcessabilityStage.Stage1Words);

            Assert.IsTrue(result.StructureBeyondStage);
            Assert.AreEqual(1, result.StageGap);
        }

        [Test]
        public void EmptyUtterance_HasNoFeaturesAndNoDeviation()
        {
            DeviationResult result = _evaluator.Evaluate(new List<Token>(), ProcessabilityStage.Stage1Words);

            Assert.IsFalse(result.HasFeatures);
            Assert.IsFalse(result.StructureBeyondStage);
        }

        [Test]
        public void CustomRuleSet_OverridesDefaultStageMapping()
        {
            var rules = new PtRuleSet(new[]
            {
                new KeyValuePair<PtFeature, ProcessabilityStage>(PtFeature.ParticleAttachment, ProcessabilityStage.Stage4Interphrasal),
            });
            var evaluator = new PtEvaluator(rules);

            var tokens = new List<Token>
            {
                Tok.Noun("本"),
                Tok.Particle("を"),
            };

            DeviationResult result = evaluator.Evaluate(tokens, ProcessabilityStage.Stage2CanonicalOrder);

            Assert.AreEqual(ProcessabilityStage.Stage4Interphrasal, result.HighestRequiredStage);
            Assert.IsTrue(result.StructureBeyondStage);
        }
    }
}
