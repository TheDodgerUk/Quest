using UnityEngine;

namespace ArcCanvas
{
    /// <summary>Distance & frustum based LOD: lowers quality with distance, skips rebuilds off-screen.</summary>
    [RequireComponent(typeof(ArcCanvasSettings))]
    [AddComponentMenu("ArcCanvas/Arc Canvas LOD")]
    public class ArcCanvasLOD : MonoBehaviour
    {
        public Camera targetCamera;

        [Header("Distance thresholds (meters)")]
        public float highDetailDistance = 3f;
        public float mediumDetailDistance = 8f;
        public float lowDetailDistance = 15f;

        [Header("Quality per tier")]
        [Range(1, 32)] public int highDetailQuality = 16;
        [Range(1, 32)] public int mediumDetailQuality = 8;
        [Range(1, 32)] public int lowDetailQuality = 3;

        [Header("Recheck interval (seconds)")]
        public float highDetailInterval = 0f;
        public float mediumDetailInterval = 0.15f;
        public float lowDetailInterval = 0.5f;

        [Header("Off-screen behaviour")]
        public bool disableRaycasterWhenOffscreen = true;

        ArcCanvasSettings _settings;
        Renderer[] _renderers;
        ArcCanvasInteractionRaycaster _raycaster;
        float _timer;
        int _lastQuality = -1;

        void Awake()
        {
            _settings = GetComponent<ArcCanvasSettings>();
            _raycaster = GetComponent<ArcCanvasInteractionRaycaster>();
            if (targetCamera == null) targetCamera = Camera.main;
            RefreshRenderers();
        }

        public void RefreshRenderers() => _renderers = GetComponentsInChildren<Renderer>(true);

        void Update()
        {
            if (targetCamera == null || _settings == null) return;

            bool visible = IsVisible();
            if (_raycaster != null && disableRaycasterWhenOffscreen)
                _raycaster.enabled = visible;

            if (!visible) return;

            float dist = Vector3.Distance(targetCamera.transform.position, transform.position);
            int quality; float interval;

            if (dist <= highDetailDistance) { quality = highDetailQuality; interval = highDetailInterval; }
            else if (dist <= mediumDetailDistance) { quality = mediumDetailQuality; interval = mediumDetailInterval; }
            else { quality = lowDetailQuality; interval = lowDetailInterval; }

            _timer += Time.deltaTime;
            if (_timer < interval) return;
            _timer = 0f;

            if (quality != _lastQuality)
            {
                _lastQuality = quality;
                _settings.quality = quality;
                _settings.NotifyChanged();
            }
        }

        bool IsVisible()
        {
            if (_renderers == null || _renderers.Length == 0) return true;
            var planes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
            foreach (var r in _renderers)
                if (r != null && GeometryUtility.TestPlanesAABB(planes, r.bounds)) return true;
            return false;
        }
    }
}
