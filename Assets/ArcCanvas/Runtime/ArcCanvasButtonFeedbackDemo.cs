using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ArcCanvas
{
    /// <summary>Flashes a button's color on click so demo buttons are visibly interactive, not just decorative rectangles.</summary>
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("")]
    public class ArcCanvasButtonFeedbackDemo : MonoBehaviour
    {
        Image _image;
        Color _original;

        void Awake()
        {
            _image = GetComponent<Image>();
            _original = _image.color;
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(Flash);
        }

        public void Flash()
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }

        IEnumerator FlashRoutine()
        {
            _image.color = Color.white;
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                _image.color = Color.Lerp(Color.white, _original, t / 0.25f);
                yield return null;
            }
            _image.color = _original;
        }
    }
}
