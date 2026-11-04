using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ArcCanvas
{
    /// <summary>
    /// Subdivides and warps a single Graphic's mesh according to the parent
    /// ArcCanvasSettings. Auto-added by ArcCanvasSettings.RefreshChildEffects -
    /// you normally never add this manually. If it's ever present without being
    /// explicitly bound (e.g. copy-pasted, or added by hand), it self-binds by
    /// searching parents for ArcCanvasSettings, so it never silently does nothing.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // hidden - added automatically by ArcCanvasSettings
    public class ArcCanvasVertexEffect : BaseMeshEffect
    {
        ArcCanvasSettings _settings;
        RectTransform _rootCanvasRect;
        Graphic _graphic;

        public void Bind(ArcCanvasSettings settings)
        {
            _settings = settings;
            _rootCanvasRect = settings.GetComponent<RectTransform>();
        }

        void EnsureBound()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            if (_settings != null) return;
            _settings = GetComponentInParent<ArcCanvasSettings>();
            if (_settings != null) _rootCanvasRect = _settings.GetComponent<RectTransform>();
        }

        /// <summary>
        /// BaseMeshEffect itself has no SetVerticesDirty method - only Graphic
        /// does. ArcCanvasSettings calls this to trigger a rebuild; it forwards
        /// to the attached Graphic.
        /// </summary>
        public void SetVerticesDirty()
        {
            EnsureBound();
            if (_graphic != null) _graphic.SetVerticesDirty();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EnsureBound();
        }

        // Unity's mesh vertex buffer is limited to 65535 (16-bit indices). Each
        // original triangle expands to quality^2 sub-triangles (3 stream
        // vertices each, unindexed) - so a Graphic with several triangles
        // already (a multi-character Text label, a 9-sliced Image) combined
        // with a high Quality setting can exceed that limit and throw deep
        // inside Unity's rendering code. This budget keeps a safety margin
        // and the clamp below makes it mathematically impossible to cross,
        // regardless of what Quality is set to.
        const int VertexBudget = 60000;

        int _lastWarnedQuality = -1;

        public override void ModifyMesh(VertexHelper vh)
        {
            EnsureBound();
            if (!IsActive() || _settings == null || _rootCanvasRect == null) return;

            var originalVerts = new List<UIVertex>();
            vh.GetUIVertexStream(originalVerts);
            if (originalVerts.Count == 0) return;

            int requestedQuality = Mathf.Max(1, _settings.quality);
            int triangleCount = originalVerts.Count / 3;
            int quality = ClampQualityForBudget(triangleCount, requestedQuality);

            if (quality < requestedQuality && quality != _lastWarnedQuality)
            {
                _lastWarnedQuality = quality;
                Debug.Log($"[ArcCanvas] '{name}' has {triangleCount} triangles - Quality " +
                    $"auto-reduced from {requestedQuality} to {quality} to stay under Unity's " +
                    "65535 vertex-per-mesh limit. This only affects this one element; lower " +
                    "the global Quality slider if you want a uniform look, or split this into " +
                    "smaller UI elements.", this);
            }

            var subdivided = SubdivideTriangles(originalVerts, quality);

            for (int i = 0; i < subdivided.Count; i++)
            {
                var v = subdivided[i];
                Vector3 worldPos = transform.TransformPoint(v.position);
                Vector3 rootLocalPos = _rootCanvasRect.InverseTransformPoint(worldPos);

                Vector3 warpedRootLocal = _settings.Warp(new Vector2(rootLocalPos.x, rootLocalPos.y));
                warpedRootLocal.z += rootLocalPos.z;

                Vector3 warpedWorld = _rootCanvasRect.TransformPoint(warpedRootLocal);
                v.position = transform.InverseTransformPoint(warpedWorld);
                subdivided[i] = v;
            }

            vh.Clear();

            // Safety net: even with the clamp above, never let a bad mesh state
            // propagate as an exception that breaks the rest of the UI. Fall
            // back to the original (unwarped) mesh rather than throwing.
            try
            {
                vh.AddUIVertexTriangleStream(subdivided);
            }
            catch (System.Exception ex)
            {
                Debug.Log($"[ArcCanvas] Failed to build curved mesh for '{name}' " +
                    $"({subdivided.Count} vertices) - falling back to flat. {ex.Message}", this);
                vh.Clear();
                vh.AddUIVertexTriangleStream(originalVerts);
            }
        }

        /// <summary>
        /// Largest quality value that keeps this Graphic's output vertex count
        /// under the budget, given how many triangles its original mesh has.
        /// </summary>
        static int ClampQualityForBudget(int triangleCount, int requestedQuality)
        {
            if (triangleCount <= 0) return requestedQuality;
            int maxQuality = Mathf.FloorToInt(Mathf.Sqrt(VertexBudget / (3f * triangleCount)));
            maxQuality = Mathf.Clamp(maxQuality, 1, 32);
            return Mathf.Min(requestedQuality, maxQuality);
        }

        static List<UIVertex> SubdivideTriangles(List<UIVertex> tris, int quality)
        {
            if (quality <= 1) return tris;

            var result = new List<UIVertex>(tris.Count * quality * quality);
            for (int t = 0; t < tris.Count; t += 3)
            {
                UIVertex a = tris[t], b = tris[t + 1], c = tris[t + 2];
                var grid = new UIVertex[quality + 1, quality + 1];

                for (int i = 0; i <= quality; i++)
                {
                    for (int j = 0; j <= quality - i; j++)
                    {
                        float u = (float)i / quality;
                        float v = (float)j / quality;
                        float w = 1f - u - v;
                        grid[i, j] = Lerp3(a, b, c, w, u, v);
                    }
                }

                for (int i = 0; i < quality; i++)
                {
                    for (int j = 0; j < quality - i; j++)
                    {
                        result.Add(grid[i, j]);
                        result.Add(grid[i + 1, j]);
                        result.Add(grid[i, j + 1]);

                        if (j < quality - i - 1)
                        {
                            result.Add(grid[i + 1, j]);
                            result.Add(grid[i + 1, j + 1]);
                            result.Add(grid[i, j + 1]);
                        }
                    }
                }
            }
            return result;
        }

        static UIVertex Lerp3(UIVertex a, UIVertex b, UIVertex c, float wa, float wb, float wc)
        {
            UIVertex r = new UIVertex();
            r.position = a.position * wa + b.position * wb + c.position * wc;
            r.normal = (a.normal * wa + b.normal * wb + c.normal * wc).normalized;
            r.uv0 = a.uv0 * wa + b.uv0 * wb + c.uv0 * wc;
            r.color = new Color32(
                (byte)(a.color.r * wa + b.color.r * wb + c.color.r * wc),
                (byte)(a.color.g * wa + b.color.g * wb + c.color.g * wc),
                (byte)(a.color.b * wa + b.color.b * wb + c.color.b * wc),
                (byte)(a.color.a * wa + b.color.a * wb + c.color.a * wc));
            return r;
        }
    }
}