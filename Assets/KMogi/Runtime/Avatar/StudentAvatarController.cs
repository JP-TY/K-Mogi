using System;
using TMPro;
using UnityEngine;

namespace KMogi.Runtime.Avatar
{
    /// <summary>
    /// Drives the simulated learner's visible reaction from the bounded cognitive signals. Works in
    /// two modes: a procedural placeholder (face color shift + head tilt + an ASCII expression label)
    /// that needs no art, and an optional blendshape mode that drives named shapes on a supplied
    /// rigged model. Displayed values are smoothed so reactions ease in/out rather than snapping.
    /// </summary>
    public sealed class StudentAvatarController : MonoBehaviour
    {
        [Header("Procedural placeholder")]
        [SerializeField] private Renderer faceRenderer;
        [SerializeField] private Transform headTransform;
        [SerializeField] private TMP_Text faceLabel;
        [SerializeField] private Color calmColor = new Color(0.60f, 0.85f, 0.62f);
        [SerializeField] private Color stressedColor = new Color(0.92f, 0.46f, 0.34f);
        [SerializeField] private float maxTiltDegrees = 16f;
        [SerializeField] private float smoothing = 4f;

        [Header("Optional rigged model")]
        [SerializeField] private SkinnedMeshRenderer blendShapeRenderer;
        [SerializeField] private string confusionBlendShape = "confusion";
        [SerializeField] private string anxietyBlendShape = "anxiety";

        private MaterialPropertyBlock _propertyBlock;
        private Quaternion _baseLocalRotation;
        private int _confusionIndex = -1;
        private int _anxietyIndex = -1;
        private float _targetConfusion;
        private float _targetAnxiety;
        private float _shownConfusion;
        private float _shownAnxiety;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public void Configure(Renderer face, Transform head, TMP_Text label)
        {
            faceRenderer = face;
            headTransform = head != null ? head : transform;
            faceLabel = label;
            _baseLocalRotation = headTransform.localRotation;
        }

        public void SetBlendShapeRenderer(SkinnedMeshRenderer renderer, string confusionShape, string anxietyShape)
        {
            blendShapeRenderer = renderer;
            confusionBlendShape = confusionShape;
            anxietyBlendShape = anxietyShape;
            ResolveBlendShapeIndices();
        }

        /// <summary>Set the (unsmoothed) target cognitive signals in [0,1].</summary>
        public void SetTarget(double confusion, double anxiety)
        {
            _targetConfusion = Mathf.Clamp01((float)confusion);
            _targetAnxiety = Mathf.Clamp01((float)anxiety);
        }

        private void Awake()
        {
            if (headTransform == null)
            {
                headTransform = transform;
            }

            _baseLocalRotation = headTransform.localRotation;
            _propertyBlock = new MaterialPropertyBlock();
            ResolveBlendShapeIndices();
        }

        private void Update()
        {
            float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
            _shownConfusion = Mathf.Lerp(_shownConfusion, _targetConfusion, t);
            _shownAnxiety = Mathf.Lerp(_shownAnxiety, _targetAnxiety, t);
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            float intensity = Mathf.Max(_shownConfusion, _shownAnxiety);

            if (faceRenderer != null)
            {
                faceRenderer.GetPropertyBlock(_propertyBlock);
                Color c = Color.Lerp(calmColor, stressedColor, intensity);
                _propertyBlock.SetColor(BaseColorId, c);
                _propertyBlock.SetColor(ColorId, c);
                faceRenderer.SetPropertyBlock(_propertyBlock);
            }

            if (headTransform != null)
            {
                headTransform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, intensity * maxTiltDegrees);
            }

            if (faceLabel != null)
            {
                faceLabel.text = ExpressionFor(_shownConfusion, _shownAnxiety);
            }

            if (blendShapeRenderer != null)
            {
                if (_confusionIndex >= 0)
                {
                    blendShapeRenderer.SetBlendShapeWeight(_confusionIndex, _shownConfusion * 100f);
                }

                if (_anxietyIndex >= 0)
                {
                    blendShapeRenderer.SetBlendShapeWeight(_anxietyIndex, _shownAnxiety * 100f);
                }
            }
        }

        // Pure ASCII so the placeholder is legible without any specific font.
        private static string ExpressionFor(float confusion, float anxiety)
        {
            if (anxiety > 0.6f)
            {
                return ">_<";
            }

            if (confusion > 0.6f)
            {
                return ":?";
            }

            if (confusion > 0.3f)
            {
                return ":/";
            }

            return ":)";
        }

        private void ResolveBlendShapeIndices()
        {
            _confusionIndex = -1;
            _anxietyIndex = -1;
            if (blendShapeRenderer == null || blendShapeRenderer.sharedMesh == null)
            {
                return;
            }

            Mesh mesh = blendShapeRenderer.sharedMesh;
            _confusionIndex = mesh.GetBlendShapeIndex(confusionBlendShape);
            _anxietyIndex = mesh.GetBlendShapeIndex(anxietyBlendShape);
        }
    }
}
