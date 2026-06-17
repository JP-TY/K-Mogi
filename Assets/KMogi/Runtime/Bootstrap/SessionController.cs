using System.Collections.Generic;
using System.Text;
using KMogi.Core.Parsing;
using KMogi.Core.Sla;
using KMogi.Core.Telemetry;
using KMogi.Runtime.Avatar;
using KMogi.Runtime.Dialogue;
using KMogi.Runtime.Parsing;
using KMogi.Runtime.Platform;
using KMogi.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KMogi.Runtime.Bootstrap
{
    /// <summary>
    /// Orchestrates the showcase micro-lesson, tying the SLA core (parse → PT evaluation → pacing →
    /// cognitive state → telemetry) to the avatar and the three spatial screens. Advance the lesson
    /// with Space or the platform select trigger; resolve learner errors with R (recast) or E
    /// (explicit). Everything updates live on the panels and the student avatar.
    /// </summary>
    public sealed class SessionController : MonoBehaviour
    {
        private enum Phase
        {
            Ready,
            AwaitingAdvance,
            AwaitingCorrection,
            Finished
        }

        private PlatformManager _platform;
        private SpatialPanel _leftPanel;
        private SpatialPanel _centerPanel;
        private SpatialPanel _rightPanel;
        private StudentAvatarController _avatar;
        private FixtureMorphologicalParser _parser;

        private readonly PtEvaluator _evaluator = new PtEvaluator();
        private readonly PacingMonitor _pacing = new PacingMonitor();
        private readonly CognitiveState _cognitive = new CognitiveState();
        private readonly TttCalculator _ttt = new TttCalculator();
        private readonly GazeQuadrantAccumulator _gaze = new GazeQuadrantAccumulator();

        private ProcessabilityStage _studentStage = ProcessabilityStage.Stage2CanonicalOrder;
        private IReadOnlyList<LessonStep> _steps;
        private int _index = -1;
        private Phase _phase = Phase.Ready;

        // Last-processed step display state.
        private LessonStep _currentStep;
        private IReadOnlyList<Token> _currentTokens;
        private DeviationResult _currentDeviation;
        private PacingResult _currentPacing;
        private string _correctionResult = string.Empty;

        public void Configure(
            PlatformManager platform,
            SpatialPanel left,
            SpatialPanel center,
            SpatialPanel right,
            StudentAvatarController avatar,
            FixtureMorphologicalParser parser,
            ProcessabilityStage studentStage)
        {
            _platform = platform;
            _leftPanel = left;
            _centerPanel = center;
            _rightPanel = right;
            _avatar = avatar;
            _parser = parser;
            _studentStage = studentStage;
        }

        private void Start()
        {
            _steps = DefaultLesson.CreateSteps();
            ApplyLayout();
            RefreshLeft();
            ShowIntro();
            RefreshRight();
        }

        private void Update()
        {
            HandleInput();
            _cognitive.Decay(Time.deltaTime);
            if (_avatar != null)
            {
                _avatar.SetTarget(_cognitive.Confusion, _cognitive.Anxiety);
            }

            RefreshRight();
        }

        private void LateUpdate()
        {
            IPlatformDeviceAdapter adapter = _platform != null ? _platform.Adapter : null;
            if (adapter != null)
            {
                _gaze.AddSampleByYaw(adapter.HeadYawDegrees, Time.deltaTime);
            }
        }

        private void HandleInput()
        {
            Keyboard keyboard = Keyboard.current;
            IPlatformDeviceAdapter adapter = _platform != null ? _platform.Adapter : null;

            bool advance = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                           (adapter != null && adapter.SelectPressedThisFrame);

            if (_phase == Phase.AwaitingCorrection)
            {
                if (keyboard == null)
                {
                    return;
                }

                if (keyboard.rKey.wasPressedThisFrame)
                {
                    ResolveCorrection(CorrectionType.Recast);
                }
                else if (keyboard.eKey.wasPressedThisFrame)
                {
                    ResolveCorrection(CorrectionType.ExplicitCorrection);
                }

                return;
            }

            if (advance && _phase != Phase.Finished)
            {
                ProcessNextStep();
            }
        }

        private void ProcessNextStep()
        {
            _index++;
            if (_steps == null || _index >= _steps.Count)
            {
                _phase = Phase.Finished;
                ShowSummary();
                return;
            }

            _currentStep = _steps[_index];
            _correctionResult = string.Empty;

            if (_currentStep.Kind == StepKind.TeacherUtterance)
            {
                EvaluateTeacherUtterance(_currentStep);
                _phase = Phase.AwaitingAdvance;
            }
            else
            {
                // Learner produced an erroneous utterance — count their speaking time and wait
                // for the trainee to choose a correction strategy.
                _currentTokens = _parser != null ? _parser.Parse(_currentStep.Japanese) : null;
                _ttt.AddStudentSpeech(_currentStep.DurationSeconds);
                _phase = Phase.AwaitingCorrection;
            }

            RefreshLeft();
            RefreshCenter();
        }

        private void EvaluateTeacherUtterance(LessonStep step)
        {
            _currentTokens = _parser != null ? _parser.Parse(step.Japanese) : new List<Token>();
            _currentDeviation = _evaluator.Evaluate(_currentTokens, _studentStage);
            _currentPacing = _pacing.Measure(JoinReadings(_currentTokens), step.DurationSeconds);

            _cognitive.ApplyDeviation(_currentDeviation);
            _cognitive.ApplyPacing(_currentPacing);
            _ttt.AddTeacherSpeech(step.DurationSeconds);
        }

        private void ResolveCorrection(CorrectionType correction)
        {
            _cognitive.ApplyCorrection(correction);
            _correctionResult = correction == CorrectionType.Recast
                ? "<color=#7cd992>Recast (implicit)</color>: the correct form was modelled in context. Anxiety eased — affective filter lowered."
                : "<color=#ff8a5c>Explicit over-correction</color>: flow interrupted to state the rule. Anxiety spiked — vocal output likely to drop.";
            _phase = Phase.AwaitingAdvance;
            RefreshCenter();
        }

        private void ApplyLayout()
        {
            if (_platform == null || _platform.Adapter == null)
            {
                return;
            }

            var layout = GetComponentInChildren<ThreeScreenLayout>();
            if (layout != null)
            {
                Vector3 headPosition = _platform.HeadCamera != null
                    ? _platform.HeadCamera.transform.position
                    : Vector3.up * 1.6f;
                layout.Apply(_platform.Adapter.PreferredLayout, headPosition);
            }
        }

        // ---- Panel composition -------------------------------------------------

        private void ShowIntro()
        {
            if (_centerPanel != null)
            {
                _centerPanel.SetText(
                    "<b>K-Mogi — Instructor Trainer</b>\n\n" +
                    "Press <b>Space</b> (or the controller trigger) to deliver each lesson line.\n" +
                    "Watch the student's confusion/anxiety and the telemetry react to your pacing " +
                    "and grammatical level.\n\n" +
                    "When the learner makes an error, choose <b>[R] Recast</b> or <b>[E] Explicit</b>.\n\n" +
                    "Hold <b>Right Mouse</b> and drag to look around the three spatial screens.");
            }
        }

        private void RefreshLeft()
        {
            if (_leftPanel == null)
            {
                return;
            }

            int total = _steps != null ? _steps.Count : 0;
            int shown = Mathf.Clamp(_index + 1, 0, total);
            var sb = new StringBuilder();
            sb.AppendLine("<b>EER · Lesson Plan</b>");
            sb.AppendLine();
            sb.AppendLine("Lesson: Irodori — particles & verbs");
            sb.AppendLine("Learner PT stage: <b>" + (int)_studentStage + "</b>");
            sb.AppendLine("Progress: " + shown + " / " + total);
            sb.AppendLine();
            if (_currentStep != null)
            {
                sb.AppendLine("Now: " + _currentStep.Gloss);
                sb.AppendLine();
            }

            sb.AppendLine("<b>Controls</b>");
            sb.AppendLine("[Space]/Trigger — next line");
            sb.AppendLine("[R] — recast   [E] — explicit");
            sb.AppendLine("[RMB+drag] — look around");
            _leftPanel.SetText(sb.ToString());
        }

        private void RefreshCenter()
        {
            if (_centerPanel == null || _currentStep == null)
            {
                return;
            }

            var sb = new StringBuilder();

            if (_currentStep.Kind == StepKind.StudentError)
            {
                sb.AppendLine("<b>Student</b> (error)");
                sb.AppendLine("<size=130%>" + _currentStep.Japanese + "</size>");
                sb.AppendLine("<i>" + _currentStep.Romaji + "</i>");
                sb.AppendLine(_currentStep.Gloss);
                sb.AppendLine();
                AppendMorphology(sb);
                sb.AppendLine();
                if (_phase == Phase.AwaitingCorrection)
                {
                    sb.AppendLine("Target form: " + _currentStep.CorrectJapanese + "  (" + _currentStep.CorrectRomaji + ")");
                    sb.AppendLine();
                    sb.AppendLine("<b>Choose a correction:</b>");
                    sb.AppendLine("[R] Recast (gentle, implicit)");
                    sb.AppendLine("[E] Explicit (state the rule)");
                }
                else if (!string.IsNullOrEmpty(_correctionResult))
                {
                    sb.AppendLine(_correctionResult);
                    sb.AppendLine();
                    sb.AppendLine("Press [Space] to continue.");
                }
            }
            else
            {
                sb.AppendLine("<b>Teacher</b>");
                sb.AppendLine("<size=130%>" + _currentStep.Japanese + "</size>");
                sb.AppendLine("<i>" + _currentStep.Romaji + "</i>");
                sb.AppendLine(_currentStep.Gloss);
                sb.AppendLine();
                AppendMorphology(sb);
                sb.AppendLine();
                sb.AppendLine("<b>Assessment</b>");
                sb.AppendLine(BuildVerdict());
            }

            _centerPanel.SetText(sb.ToString());
        }

        private void AppendMorphology(StringBuilder sb)
        {
            sb.AppendLine("<b>Morphology</b>");
            if (_currentTokens == null || _currentTokens.Count == 0)
            {
                sb.AppendLine("(none)");
                return;
            }

            foreach (Token token in _currentTokens)
            {
                string romaji = _parser != null ? _parser.Romanize(token.Surface) : token.Surface;
                sb.AppendLine("• " + token.Surface + "  (" + romaji + ")  <color=#9fb6ff>" + token.PartOfSpeech + "</color>");
            }
        }

        private string BuildVerdict()
        {
            if (_currentDeviation == null)
            {
                return "(no analysis)";
            }

            var sb = new StringBuilder();
            if (!_currentDeviation.HasFeatures)
            {
                sb.AppendLine("No analyzable structure.");
            }
            else if (_currentDeviation.StructureBeyondStage)
            {
                sb.AppendLine("<color=#ff8a5c>BEYOND stage " + (int)_studentStage + "</color>: needs stage " +
                              (int)_currentDeviation.HighestRequiredStage + " (" + Friendly(_currentDeviation.WorstFeature) +
                              "). The learner cannot process this yet.");
            }
            else
            {
                sb.AppendLine("<color=#7cd992>Within reach</color> (stage " +
                              (int)_currentDeviation.HighestRequiredStage + ").");
            }

            if (_currentPacing != null)
            {
                if (_currentPacing.TooFast)
                {
                    sb.AppendLine("<color=#ff8a5c>Too fast</color>: " + _currentPacing.MoraePerSecond.ToString("F1") +
                                  " morae/s (> " + _currentPacing.ThresholdMoraePerSecond.ToString("F1") + ").");
                }
                else
                {
                    sb.AppendLine("Pace: " + _currentPacing.MoraePerSecond.ToString("F1") + " morae/s (comfortable).");
                }
            }

            return sb.ToString();
        }

        private void RefreshRight()
        {
            if (_rightPanel == null)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("<b>Live Telemetry</b>");
            sb.AppendLine();

            string tttColor = _ttt.WithinTarget ? "#7cd992" : "#ff8a5c";
            sb.AppendLine("Teacher Talk Time");
            sb.AppendLine("<color=" + tttColor + ">" + Mathf.RoundToInt((float)_ttt.Ratio * 100) + "%</color>  " +
                          Bar(_ttt.Ratio) + "  (target ≤ 40%)");
            sb.AppendLine();

            sb.AppendLine("Learner state");
            sb.AppendLine("Confusion " + Bar(_cognitive.Confusion));
            sb.AppendLine("Anxiety   " + Bar(_cognitive.Anxiety));
            sb.AppendLine();

            sb.AppendLine("Gaze distribution");
            sb.AppendLine("Left   " + Bar(_gaze.ShareOf(GazeQuadrant.Left)));
            sb.AppendLine("Center " + Bar(_gaze.ShareOf(GazeQuadrant.Center)));
            sb.AppendLine("Right  " + Bar(_gaze.ShareOf(GazeQuadrant.Right)));

            IReadOnlyList<GazeQuadrant> neglected = _gaze.NeglectedQuadrants(0.15);
            if (neglected.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("<color=#ffcf6b>Neglecting: " + string.Join(", ", neglected) + "</color>");
            }

            _rightPanel.SetText(sb.ToString());
        }

        private void ShowSummary()
        {
            if (_centerPanel != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("<b>Session Complete</b>");
                sb.AppendLine();
                sb.AppendLine("Teacher Talk Time: " + Mathf.RoundToInt((float)_ttt.Ratio * 100) + "%  " +
                              (_ttt.WithinTarget ? "(within target)" : "(over 40% target)"));
                sb.AppendLine("Final confusion: " + Mathf.RoundToInt((float)_cognitive.Confusion * 100) + "%");
                sb.AppendLine("Final anxiety: " + Mathf.RoundToInt((float)_cognitive.Anxiety * 100) + "%");
                sb.AppendLine();
                sb.AppendLine("Gaze — L " + Pct(_gaze.ShareOf(GazeQuadrant.Left)) +
                              " · C " + Pct(_gaze.ShareOf(GazeQuadrant.Center)) +
                              " · R " + Pct(_gaze.ShareOf(GazeQuadrant.Right)));
                _centerPanel.SetText(sb.ToString());
            }

            if (_leftPanel != null)
            {
                RefreshLeft();
            }
        }

        // ---- Helpers -----------------------------------------------------------

        private static string JoinReadings(IReadOnlyList<Token> tokens)
        {
            if (tokens == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (Token token in tokens)
            {
                sb.Append(token.Reading);
            }

            return sb.ToString();
        }

        private static string Bar(double value01, int width = 10)
        {
            float clamped = Mathf.Clamp01((float)value01);
            int filled = Mathf.RoundToInt(clamped * width);
            return "[" + new string('#', filled) + new string('-', width - filled) + "]";
        }

        private static string Pct(double value01) => Mathf.RoundToInt((float)value01 * 100) + "%";

        private static string Friendly(PtFeature feature)
        {
            switch (feature)
            {
                case PtFeature.SingleWord: return "single word";
                case PtFeature.CanonicalSov: return "canonical SOV";
                case PtFeature.ParticleAttachment: return "particle attachment";
                case PtFeature.Adjective: return "adjective";
                case PtFeature.TeForm: return "te-form";
                case PtFeature.NounModifyingClause: return "relative clause";
                case PtFeature.Passive: return "passive";
                case PtFeature.Causative: return "causative";
                default: return feature.ToString();
            }
        }
    }
}
