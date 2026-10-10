using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class UISliderDrag : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerClickHandler
    {
        private Slider m_Slider;
        private UnityAction<float> m_OnEndDrag;
        private UnityAction<float> m_Drag;
        private float m_Amount;
        public void SetEndDrag(Slider slider, UnityAction<float> unityAction)
        {
            m_Slider = slider;
            m_OnEndDrag = unityAction;
        }

        public void SetDrag(Slider slider, UnityAction<float> unityAction)
        {
            m_Slider = slider;
            m_Drag = unityAction;
        }

        public void OnPointerClick(PointerEventData pointerEventData)
        {
            m_OnEndDrag?.Invoke(m_Slider.value);
        }

        public void OnEndDrag(PointerEventData data)
        {
            m_OnEndDrag?.Invoke(m_Slider.value);
        }
        public void OnDrag(PointerEventData data)
        {
            m_Drag?.Invoke(m_Slider.value);
        }
    }
}