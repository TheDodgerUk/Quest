using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISliderText
{
    public Slider m_Slider;
    public Text m_Text;
    private Transform m_Transform;
    public UISliderText(Transform item, float min, float max)
    {
        m_Transform = item;
        m_Slider = item.SearchComponent<Slider>("Slider");
        m_Slider.minValue = min;
        m_Slider.maxValue = max;
        m_Text = item.SearchComponent<Text>("Text");
    }
    public void Enable(bool enable) => m_Transform.gameObject.SetActive(enable);
}

