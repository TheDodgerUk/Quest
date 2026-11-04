using UnityEngine;
using System.Collections;

namespace ArcCanvas
{
    /// <summary>Animates ArcCanvasSettings.curveBlend, radius, or shape swaps over time.</summary>
    [RequireComponent(typeof(ArcCanvasSettings))]
    [AddComponentMenu("ArcCanvas/Arc Canvas Transition Animator")]
    public class ArcCanvasTransitionAnimator : MonoBehaviour
    {
        public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float duration = 0.6f;

        ArcCanvasSettings _settings;
        Coroutine _running;

        void Awake() => _settings = GetComponent<ArcCanvasSettings>();

        public void Flatten() => AnimateBlend(0f);
        public void Curve() => AnimateBlend(1f);

        public void AnimateBlend(float to)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(BlendRoutine(to));
        }

        public void AnimateRadius(float to)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(RadiusRoutine(to));
        }

        public void MorphTo(ArcShape newShape)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(MorphRoutine(newShape));
        }

        IEnumerator BlendRoutine(float to)
        {
            float from = _settings.curveBlend;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = easing.Evaluate(Mathf.Clamp01(t / duration));
                _settings.curveBlend = Mathf.Lerp(from, to, k);
                yield return null;
            }
            _settings.curveBlend = to;
        }

        IEnumerator RadiusRoutine(float to)
        {
            float from = _settings.shapeParams.radius;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = easing.Evaluate(Mathf.Clamp01(t / duration));
                var p = _settings.shapeParams;
                p.radius = Mathf.Lerp(from, to, k);
                _settings.shapeParams = p;
                yield return null;
            }
            var final = _settings.shapeParams;
            final.radius = to;
            _settings.shapeParams = final;
        }

        IEnumerator MorphRoutine(ArcShape newShape)
        {
            yield return BlendRoutine(0f);
            _settings.shape = newShape;
            yield return BlendRoutine(1f);
        }
    }
}
