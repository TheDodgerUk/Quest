#if TMP_PRESENT
using TMPro;
#endif

using UnityEngine;
using System.Collections.Generic;

namespace ArcCanvas
{
    /// <summary>
    /// The main component. Add this to a Canvas to bend it. Drives every
    /// ArcCanvasVertexEffect (and, if TextMeshPro is installed,
    /// ArcCanvasTMPBendAdapter) on child Graphics, plus the interaction
    /// raycaster.
    ///
    /// IMPORTANT: bending only produces a visible result on a WORLD SPACE
    /// canvas, because the curve depth (Z axis) needs camera perspective to
    /// read. Screen Space - Overlay renders orthographically with no
    /// perspective, so a Z-axis bend is mathematically applied but invisible.
    /// This component auto-switches the Canvas to World Space on enable and
    /// warns in the inspector if it can't (e.g. you switch it back manually).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    [AddComponentMenu("ArcCanvas/Arc Canvas Settings")]
    public class ArcCanvasSettings : MonoBehaviour
    {
        [Header("Shape")]
        public ArcShape shape = ArcShape.Cylinder;
        public ArcCanvasShapeParams shapeParams = ArcCanvasShapeParams.Default;

        [Header("Simple Mode")]
        [Tooltip("When on, a single 'Curve Amount' slider drives everything - matches the " +
                 "one-slider workflow people expect. Turn off to access every parameter directly.")]
        public bool simpleMode = true;
        [Tooltip("0 = flat, 1 = fully curved. The single control Simple Mode exposes - " +
                 "internally maps to the right radius/amplitude/blend for whichever shape is selected.")]
        [Range(0f, 1f)] public float curveAmount = 0.6f;

        [Header("Quality")]
        [Tooltip("Grid subdivisions per UI element. Higher = smoother curve, more vertices.")]
        [Range(1, 32)] public int quality = 12;

        [Header("Blend")]
        [Tooltip("0 = flat, 1 = fully curved. In Simple Mode this is driven by Curve Amount; " +
                 "in Advanced mode you can set it directly.")]
        [Range(0f, 1f)] public float curveBlend = 1f;

        [Header("Auto behaviour")]
        [Tooltip("Automatically add the bend effect to every Graphic under this canvas, including ones added later.")]
        public bool autoInjectChildren = true;
        [Tooltip("Force this Canvas to World Space render mode, which is required for the curve to be visible.")]
        public bool autoForceWorldSpace = true;

        public event System.Action SettingsChanged;

        readonly List<ArcCanvasVertexEffect> _trackedEffects = new List<ArcCanvasVertexEffect>();
#if TMP_PRESENT
        readonly List<ArcCanvasTMPBendAdapter> _trackedTMP = new List<ArcCanvasTMPBendAdapter>();
#endif
        int _lastChildCount = -1;
        int _lastParamHash;
        ArcShape _lastShape;
        int _lastQuality;
        float _lastBlend;
        Canvas _canvas;

        public bool IsWorldSpaceIssue => _canvas != null && _canvas.renderMode != RenderMode.WorldSpace;

        void OnEnable()
        {
            _canvas = GetComponent<Canvas>();
            EnforceWorldSpaceIfNeeded();
            RefreshChildEffects();
            CacheDirtyState();
        }

        void OnValidate()
        {
            EnforceWorldSpaceIfNeeded();
            NotifyChanged();
        }

        void Update()
        {
            if (simpleMode)
                ApplySimpleCurveAmount();

            if (autoInjectChildren)
            {
                int childCount = transform.childCount;
                if (childCount != _lastChildCount)
                {
                    _lastChildCount = childCount;
                    RefreshChildEffects();
                }
            }

            // Continuous dirty-check: catches changes made via script, animation,
            // or inspector dragging even without an explicit NotifyChanged() call,
            // so "move the slider, shape changes" works with zero extra wiring.
            if (shape != _lastShape || quality != _lastQuality || !Mathf.Approximately(curveBlend, _lastBlend)
                || shapeParams.ComputeHash() != _lastParamHash)
            {
                CacheDirtyState();
                NotifyChanged();
            }
        }

        void CacheDirtyState()
        {
            _lastShape = shape;
            _lastQuality = quality;
            _lastBlend = curveBlend;
            _lastParamHash = shapeParams.ComputeHash();
        }

        /// <summary>
        /// The single-slider workflow: curveAmount = 0 is flat, anything above
        /// that engages the curve and scales its intensity, mapped to whichever
        /// underlying parameter actually controls "how curved" for the current
        /// shape (radius for most shapes, amplitude for Wave, both X/Z radii
        /// for Ellipse). This is what makes ArcCanvas behave like a familiar
        /// one-slider curved-canvas tool by default, while everything else
        /// stays available the moment Simple Mode is switched off.
        /// </summary>
        void ApplySimpleCurveAmount()
        {
            if (curveAmount <= 0.001f)
            {
                curveBlend = 0f;
                return;
            }

            curveBlend = 1f;
            var p = shapeParams;

            switch (shape)
            {
                case ArcShape.Wave:
                    p.waveAmplitude = Mathf.Lerp(0f, 150f, curveAmount);
                    break;
                case ArcShape.Ellipse:
                    float rx = Mathf.Lerp(2500f, 300f, curveAmount);
                    p.ellipseRadiusX = rx;
                    p.ellipseRadiusZ = rx * 0.5f;
                    break;
                default: // Cylinder, Ring, Sphere, Cone, Spiral - all read from radius
                    p.radius = Mathf.Lerp(2500f, 300f, curveAmount);
                    break;
            }

            shapeParams = p;
        }

        void EnforceWorldSpaceIfNeeded()
        {
            if (!autoForceWorldSpace) return;
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_canvas != null && _canvas.renderMode != RenderMode.WorldSpace)
            {
                _canvas.renderMode = RenderMode.WorldSpace;
                if (_canvas.worldCamera == null) _canvas.worldCamera = Camera.main;
            }
        }

        public void NotifyChanged()
        {
            SettingsChanged?.Invoke();
            foreach (var e in _trackedEffects)
                if (e != null) e.SetVerticesDirty();
#if TMP_PRESENT
            foreach (var t in _trackedTMP)
                if (t != null) t.SetVerticesDirty();
#endif
        }

        /// <summary>
        /// Finds every Graphic under this canvas and binds it to the correct
        /// bend path: standard Graphics (Image, RawImage, legacy Text) get
        /// ArcCanvasVertexEffect; TextMeshPro components get
        /// ArcCanvasTMPBendAdapter instead, since TMP bypasses the standard
        /// mesh-modifier chain entirely.
        /// </summary>
        public void RefreshChildEffects()
        {
            _trackedEffects.Clear();
#if TMP_PRESENT
            _trackedTMP.Clear();
#endif
            var graphics = GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var g in graphics)
            {
#if TMP_PRESENT
                if (g is TMP_Text)
                {
                    var tmpAdapter = g.GetComponent<ArcCanvasTMPBendAdapter>();
                    if (tmpAdapter == null) tmpAdapter = g.gameObject.AddComponent<ArcCanvasTMPBendAdapter>();
                    tmpAdapter.Bind(this);
                    _trackedTMP.Add(tmpAdapter);
                    continue;
                }
#endif
                var effect = g.GetComponent<ArcCanvasVertexEffect>();
                if (effect == null) effect = g.gameObject.AddComponent<ArcCanvasVertexEffect>();
                effect.Bind(this);
                _trackedEffects.Add(effect);
            }
            NotifyChanged();
        }

        /// <summary>Warp a flat local point (in this canvas's rect space) into curved local space.</summary>
        public Vector3 Warp(Vector2 flatLocal)
        {
            var curved = ArcCanvasShapeMath.Forward(shape, flatLocal, shapeParams);
            return Vector3.Lerp(new Vector3(flatLocal.x, flatLocal.y, 0f), curved, curveBlend);
        }

        // ---- One-click presets so a usable shape is a single button press away ----
        // Presets set exact values, so they switch off Simple Mode - otherwise
        // the Curve Amount mapping would overwrite them on the very next frame.

        public void ApplyPreset_SubtleCurve()
        {
            simpleMode = false;
            shape = ArcShape.Cylinder;
            shapeParams.radius = 1600f;
            curveBlend = 1f;
            NotifyChanged();
        }

        public void ApplyPreset_WrapAroundViewer()
        {
            simpleMode = false;
            shape = ArcShape.Cylinder;
            shapeParams.radius = 600f;
            curveBlend = 1f;
            NotifyChanged();
        }

        public void ApplyPreset_CurvedMonitor()
        {
            simpleMode = false;
            shape = ArcShape.Cylinder;
            shapeParams.radius = 1000f;
            shapeParams.curveDirection = ArcCurveDirection.Toward;
            curveBlend = 1f;
            NotifyChanged();
        }

        public void ApplyPreset_SpaceHelmet()
        {
            simpleMode = false;
            shape = ArcShape.Sphere;
            shapeParams.radius = 900f;
            curveBlend = 1f;
            NotifyChanged();
        }

        public void ApplyPreset_Flat()
        {
            if (simpleMode) curveAmount = 0f;
            else curveBlend = 0f;
            NotifyChanged();
        }
    }
}
