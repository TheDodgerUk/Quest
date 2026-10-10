using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class LightPanel : UserInterface
    {
        public static LightPanel Instance;
        private List<Light> m_Lights = new List<Light>();
        private UISliderText m_Range;
        private UISliderText m_Intensity;
        private UISliderText m_Angle;
        public override void Initialise()
        {
            Instance = this;
            this.SetActive(true);
            m_Range = new UISliderText(transform.SearchChildrenWithNameContains("Range"), 1, 10);
            m_Intensity = new UISliderText(transform.SearchChildrenWithNameContains("Intensity"), 0, 100);
            m_Angle = new UISliderText(transform.SearchChildrenWithNameContains("Angle"), 0, 100);

            m_PanelGroupLayout = this.GetComponent<CanvasGroup>();
        }

        ////public void SetSpotLight(List<Light> lights, ContentSpotLightMetaData data)
        ////{
        ////    m_Angle.Enable(true);
        ////    m_Lights = lights;
        ////    m_Range.m_Slider.minValue = data.m_Range.x;
        ////    m_Range.m_Slider.maxValue = data.m_Range.y;
        ////    m_Intensity.m_Slider.minValue = data.m_Intensity.x;
        ////    m_Intensity.m_Slider.maxValue = data.m_Intensity.y;
        ////    m_Angle.m_Slider.minValue = data.m_Angle.x;
        ////    m_Angle.m_Slider.maxValue = data.m_Angle.y;

        ////    m_Range.m_Slider.value = lights[0].range;
        ////    m_Intensity.m_Slider.value = lights[0].intensity;
        ////    m_Angle.m_Slider.value = lights[0].spotAngle;
        ////}

        ////public void SetPointLight(List<Light> lights, ContentPointLightMetaData data)
        ////{
        ////    m_Angle.Enable(false);
        ////    m_Lights = lights;
        ////    m_Range.m_Slider.minValue = data.m_Range.x;
        ////    m_Range.m_Slider.maxValue = data.m_Range.y;
        ////    m_Intensity.m_Slider.minValue = data.m_Intensity.x;
        ////    m_Intensity.m_Slider.maxValue = data.m_Intensity.y;

        ////    m_Range.m_Slider.value = lights[0].range;
        ////    m_Intensity.m_Slider.value = lights[0].intensity;
        ////    m_Angle.m_Slider.value = lights[0].spotAngle;
        ////}


        //TODO: LUBO this is a crap way i did this 
        private void Update()
        {
            foreach (var light in m_Lights)
            {

                light.range = m_Range.m_Slider.value;
                m_Range.m_Text.text = m_Range.m_Slider.value.ToString("0.0");

                light.intensity = m_Intensity.m_Slider.value;
                m_Intensity.m_Text.text = m_Intensity.m_Slider.value.ToString("0.0");

                light.spotAngle = m_Angle.m_Slider.value;
                m_Angle.m_Text.text = m_Angle.m_Slider.value.ToString("0.0");
            }
        }
    }
}
