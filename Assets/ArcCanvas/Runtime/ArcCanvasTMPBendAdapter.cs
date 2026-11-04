#if TMP_PRESENT
using UnityEngine;
using TMPro;

namespace ArcCanvas
{
    /// <summary>
    /// TextMeshProUGUI does NOT route through Graphic's standard IMeshModifier/
    /// BaseMeshEffect chain the way Image or the legacy Text component does -
    /// it generates and pushes its own mesh directly. That's why
    /// ArcCanvasVertexEffect alone never bends TMP labels. This adapter uses
    /// TMP's supported per-character vertex API instead: each frame, it reads
    /// the ALREADY-LAID-OUT flat corner positions for every character
    /// (TMP_CharacterInfo.vertex_BL/TL/TR/BR - these are TMP's source-of-truth
    /// flat positions, recomputed on every layout pass, so re-warping from
    /// them every frame never compounds), warps each corner through the same
    /// ArcCanvasSettings.Warp() used everywhere else, and pushes the result
    /// back with UpdateVertexData(). This is the same technique used in TMP's
    /// own official curved/warped text examples.
    /// Auto-added by ArcCanvasSettings next to any TMP_Text component found
    /// under the canvas - you don't add this by hand.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    [RequireComponent(typeof(TMP_Text))]
    public class ArcCanvasTMPBendAdapter : MonoBehaviour
    {
        ArcCanvasSettings _settings;
        RectTransform _rootCanvasRect;
        TMP_Text _text;

        public void Bind(ArcCanvasSettings settings)
        {
            _settings = settings;
            _rootCanvasRect = settings.GetComponent<RectTransform>();
        }

        void EnsureBound()
        {
            if (_text == null) _text = GetComponent<TMP_Text>();
            if (_settings != null) return;
            _settings = GetComponentInParent<ArcCanvasSettings>();
            if (_settings != null) _rootCanvasRect = _settings.GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            EnsureBound();
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        void OnTextChanged(UnityEngine.Object obj)
        {
            if (obj == (UnityEngine.Object)_text) Rebend();
        }

        /// <summary>Called by ArcCanvasSettings.NotifyChanged() whenever the curve parameters change.</summary>
        public void SetVerticesDirty() => Rebend();

        // Also re-apply every frame so dragging a live slider stays smooth even
        // between explicit NotifyChanged calls (TMP doesn't re-fire TEXT_CHANGED
        // just because the curve changed, only when the text/layout changes).
        void LateUpdate() => Rebend();

        void Rebend()
        {
            EnsureBound();
            if (_text == null || _settings == null || _rootCanvasRect == null) return;

            _text.ForceMeshUpdate(); // ensures characterInfo reflects current flat layout before we read it
            var textInfo = _text.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var ch = textInfo.characterInfo[i];
                if (!ch.isVisible) continue;

                int materialIndex = ch.materialReferenceIndex;
                var vertices = textInfo.meshInfo[materialIndex].vertices;
                int vIdx = ch.vertexIndex;

                vertices[vIdx + 0] = WarpLocal(ch.vertex_BL.position);
                vertices[vIdx + 1] = WarpLocal(ch.vertex_TL.position);
                vertices[vIdx + 2] = WarpLocal(ch.vertex_TR.position);
                vertices[vIdx + 3] = WarpLocal(ch.vertex_BR.position);
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _text.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        Vector3 WarpLocal(Vector3 flatLocalToThisTransform)
        {
            Vector3 worldPos = transform.TransformPoint(flatLocalToThisTransform);
            Vector3 rootLocal = _rootCanvasRect.InverseTransformPoint(worldPos);
            Vector3 warpedRootLocal = _settings.Warp(new Vector2(rootLocal.x, rootLocal.y));
            warpedRootLocal.z += rootLocal.z;
            Vector3 warpedWorld = _rootCanvasRect.TransformPoint(warpedRootLocal);
            return transform.InverseTransformPoint(warpedWorld);
        }
    }
}
#endif
