using UnityEngine;
using UnityEngine.EventSystems;

#if ENABLE_VR
using UnityEngine.XR;
#endif

namespace ArcCanvas
{
    /// <summary>Hover/click controller haptics for VR. Silent no-op on flatscreen/mouse.</summary>
    [AddComponentMenu("ArcCanvas/Arc Canvas Haptics")]
    public class ArcCanvasHaptics : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public bool hapticsOnHover = true;
        [Range(0f, 1f)] public float hoverAmplitude = 0.15f;
        public float hoverDuration = 0.05f;

        public bool hapticsOnClick = true;
        [Range(0f, 1f)] public float clickAmplitude = 0.5f;
        public float clickDuration = 0.08f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hapticsOnHover) SendPulse(eventData, hoverAmplitude, hoverDuration);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (hapticsOnClick) SendPulse(eventData, clickAmplitude, clickDuration);
        }

        void SendPulse(PointerEventData eventData, float amplitude, float duration)
        {
#if ENABLE_VR
            var node = eventData.pointerId == 0 ? XRNode.RightHand : XRNode.LeftHand;
            var device = InputDevices.GetDeviceAtXRNode(node);
            if (device.isValid && device.TryGetHapticCapabilities(out var caps) && caps.supportsImpulse)
                device.SendHapticImpulse(0u, amplitude, duration);
#endif
        }
    }
}
