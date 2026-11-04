using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ArcCanvas
{
    /// <summary>
    /// Replaces GraphicRaycaster for a bent canvas: casts against an invisible
    /// collider proxy that matches the curve exactly, decodes the hit back to
    /// a flat local point via baked UVs, then re-runs standard Unity UI hit
    /// testing at that flat point - so every Selectable/ScrollRect/EventTrigger
    /// keeps working unmodified.
    /// </summary>
    [RequireComponent(typeof(ArcCanvasSettings))]
    [AddComponentMenu("ArcCanvas/Arc Canvas Interaction Raycaster")]
    public class ArcCanvasInteractionRaycaster : BaseRaycaster
    {
        public enum PointerSource { ScreenPointCamera, WorldSpaceTransform }

        [Header("Pointer source")]
        public PointerSource pointerSource = PointerSource.ScreenPointCamera;
        [Tooltip("Used when pointerSource = ScreenPointCamera (mouse, gaze). Defaults to Camera.main if left empty.")]
        public Camera pointerCamera;
        [Tooltip("Used when pointerSource = WorldSpaceTransform (VR controller, hand ray).")]
        public Transform pointerOrigin;
        public float maxRayDistance = 50f;

        ArcCanvasSettings _settings;
        Canvas _canvas;
        MeshCollider _collider;
        Mesh _colliderMesh;

        public override Camera eventCamera => pointerCamera != null ? pointerCamera : Camera.main;

        protected override void Awake()
        {
            base.Awake();
            _settings = GetComponent<ArcCanvasSettings>();
            _canvas = GetComponent<Canvas>();
            BuildOrRefreshCollider();
            if (_settings != null) _settings.SettingsChanged += BuildOrRefreshCollider;
        }

        void OnDestroy()
        {
            if (_settings != null) _settings.SettingsChanged -= BuildOrRefreshCollider;
            if (_colliderMesh != null) DestroyImmediate(_colliderMesh);
        }

        void BuildOrRefreshCollider()
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null || _settings == null) return;

            const int grid = 24;
            Vector2 size = rt.rect.size;
            var verts = new Vector3[(grid + 1) * (grid + 1)];
            var uvs = new Vector2[verts.Length];
            var tris = new List<int>();

            for (int y = 0; y <= grid; y++)
            {
                for (int x = 0; x <= grid; x++)
                {
                    float u = (float)x / grid;
                    float v = (float)y / grid;
                    Vector2 flat = new Vector2((u - 0.5f) * size.x, (v - 0.5f) * size.y);
                    int idx = y * (grid + 1) + x;
                    verts[idx] = _settings.Warp(flat);
                    uvs[idx] = new Vector2(u, v);
                }
            }

            for (int y = 0; y < grid; y++)
            {
                for (int x = 0; x < grid; x++)
                {
                    int i0 = y * (grid + 1) + x;
                    int i1 = i0 + 1;
                    int i2 = i0 + (grid + 1);
                    int i3 = i2 + 1;
                    tris.Add(i0); tris.Add(i2); tris.Add(i1);
                    tris.Add(i1); tris.Add(i2); tris.Add(i3);
                }
            }

            if (_colliderMesh == null) _colliderMesh = new Mesh { name = "ArcCanvas_ColliderProxy" };
            _colliderMesh.Clear();
            _colliderMesh.vertices = verts;
            _colliderMesh.uv = uvs;
            _colliderMesh.triangles = tris.ToArray();
            _colliderMesh.RecalculateNormals();
            _colliderMesh.RecalculateBounds();

            if (_collider == null)
            {
                var colliderGO = new GameObject("ArcCanvas_ColliderProxy (auto)");
                colliderGO.transform.SetParent(transform, false);
                colliderGO.hideFlags = HideFlags.HideInHierarchy;
                _collider = colliderGO.AddComponent<MeshCollider>();
            }
            _collider.sharedMesh = _colliderMesh;
            _collider.convex = false;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_collider == null) BuildOrRefreshCollider();
            if (_collider == null) return;

            Ray ray;
            if (pointerSource == PointerSource.WorldSpaceTransform && pointerOrigin != null)
            {
                ray = new Ray(pointerOrigin.position, pointerOrigin.forward);
            }
            else
            {
                if (eventCamera == null) return;
                ray = eventCamera.ScreenPointToRay(eventData.position);
            }

            if (!_collider.Raycast(ray, out RaycastHit hit, maxRayDistance)) return;

            var rt = GetComponent<RectTransform>();
            Vector2 size = rt.rect.size;
            Vector2 flatLocal = new Vector2((hit.textureCoord.x - 0.5f) * size.x, (hit.textureCoord.y - 0.5f) * size.y);

            var graphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);
            for (int i = graphics.Count - 1; i >= 0; i--)
            {
                var g = graphics[i];
                if (g == null || !g.raycastTarget) continue;
                var grt = g.rectTransform;
                if (grt == null) continue;

                Vector3 rootWorld = rt.TransformPoint(new Vector3(flatLocal.x, flatLocal.y, 0f));

                Vector2 screenPointForTest = eventCamera != null ? (Vector2)eventCamera.WorldToScreenPoint(rootWorld) : (Vector2)rootWorld;

                if (RectTransformUtility.RectangleContainsScreenPoint(grt, screenPointForTest, eventCamera))
                {
                    resultAppendList.Add(new RaycastResult
                    {
                        gameObject = g.gameObject,
                        module = this,
                        distance = hit.distance,
                        worldPosition = hit.point,
                        worldNormal = hit.normal,
                        screenPosition = eventData.position,
                        index = resultAppendList.Count,
                        depth = g.depth,
                        sortingLayer = _canvas.sortingLayerID,
                        sortingOrder = _canvas.sortingOrder
                    });
                }
            }
        }
    }
}
