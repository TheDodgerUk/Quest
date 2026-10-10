using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class UISliderDragCallback
    {
        private UISliderDrag m_UISlider;
        private Slider m_Slider;
        private Text m_Text;
        private bool m_Convert;

        public UISliderDragCallback(Transform transformRef, ref float valueAmount, string sliderName, string textName, float min, float max, bool convert , Action<float> callBack)
        {
            m_Slider = transformRef.SearchComponent<Slider>(sliderName);
            m_Text = transformRef.SearchComponent<Text>(textName);
            m_Slider.onValueChanged.RemoveAllListeners();
            m_Slider.value = valueAmount;
            m_Slider.minValue = min;
            m_Slider.maxValue = max;
            m_Text.text = m_Slider.value.ToString("0.00");
            m_UISlider = m_Slider.gameObject.ForceComponent<UISliderDrag>();
            m_Convert = convert;
            m_UISlider.SetEndDrag(m_Slider, (amount) =>
            {
                m_Text.text = MeasurementManager.ConvertTheMeasure(amount, m_Convert);
                callBack?.Invoke(amount);
            });

            m_UISlider.SetDrag(m_Slider, (amount) =>
            {
                m_Text.text = MeasurementManager.ConvertTheMeasure(amount, m_Convert);
        });
        }

        public void CallBackOnEndDrag(ref float valueAmount, Action<float> callBack)
        {
            m_Slider.value = valueAmount;           
            m_Text.text = MeasurementManager.ConvertTheMeasure(valueAmount, m_Convert);
            m_UISlider.SetEndDrag(m_Slider, (amount) =>
            {             
                m_Text.text = MeasurementManager.ConvertTheMeasure(amount, m_Convert);
                callBack?.Invoke(amount);
            });
        }

        public void EnableUpdateOnDrag()
        {
            m_UISlider.SetDrag(m_Slider, (amount) =>
            {
                m_Slider.value = amount;
                m_Text.text = MeasurementManager.ConvertTheMeasure(amount, m_Convert);
            });
        }
    }
}