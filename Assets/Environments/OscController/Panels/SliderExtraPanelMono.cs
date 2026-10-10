using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer;
using static SliderPrefab;

namespace OscController
{

    public class SliderExtraPanel
    {
        private SliderExtraPanelMono m_SliderExtraPanelMono;
        public SliderExtraPanel(SliderExtraPanelMono sliderExtraPanelMono)
        {
            m_SliderExtraPanelMono = sliderExtraPanelMono;
        }

        public void Initialise() => m_SliderExtraPanelMono.Initialise();
        public void SetSliderPrefabOnEnter(SliderPrefab prefab) => m_SliderExtraPanelMono.SetSliderPrefabOnEnter(prefab);
        public void SetSliderPrefabOnExit(SliderPrefab prefab) => m_SliderExtraPanelMono.SetSliderPrefabOnExit(prefab);
        public void SetMessage(string message) => m_SliderExtraPanelMono.SetMessage(message);
    }


    public class SliderExtraPanelMono : MonoBehaviour
    {
        private CanvasGroup m_CanvasGroup;
        private Text m_Text;
        private SliderPrefab m_CurrentPrefab;
        private AvailableChannelValue m_AvailableChannelValue;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            m_Text = this.GetComponentInChildren<Text>();
            m_CanvasGroup = this.gameObject.ForceComponent<CanvasGroup>();
            m_CanvasGroup.VisibleAndInteractive(false);
        }

        public void SetSliderPrefabOnEnter(SliderPrefab prefab)
        {
            m_CurrentPrefab = prefab;
            switch (m_CurrentPrefab.m_CapabilityType)
            {
                case CapabilityType.Pan:
                case CapabilityType.PanFine:
                case CapabilityType.Tilt:
                case CapabilityType.TiltFine:
                case CapabilityType.Red:
                case CapabilityType.Green:
                case CapabilityType.Blue:
                case CapabilityType.White:
                case CapabilityType.Dimmer:
                case CapabilityType.Dimmer2:
                case CapabilityType.CWDimmer:
                case CapabilityType.WWDimmer:
                case CapabilityType.ShutterStrobe:
                case CapabilityType.StrobeShutter:
                    m_CanvasGroup.alpha = 1;
                    break;
                default:
                    DecreaseAlpha(force: true);
                    break;
            }

            InitialiseData();
        }

        private void InitialiseData()
        {
            switch (m_CurrentPrefab.m_CapabilityType)
            {
                case CapabilityType.Pan:
                case CapabilityType.Tilt:
                    m_AvailableChannelValue = m_CurrentPrefab.m_FixtureData.m_Fixture.AvailableChannels[m_CurrentPrefab.m_CapabilityType.ToString()];
                    break;
            }
        }

        private void DecreaseAlpha(bool force = false)
        {
            if (m_CanvasGroup.alpha < 1f || force == true)
            {
                m_CanvasGroup.alpha -= (Time.deltaTime);
                m_CanvasGroup.alpha = Mathf.Clamp(m_CanvasGroup.alpha, 0f, 1f);
            }
        }


        public void SetSliderPrefabOnExit(SliderPrefab prefab)
        {
            m_CurrentPrefab = prefab;
            DecreaseAlpha(force: true);
        }

        private void Update()
        {
            if(m_CurrentPrefab == false)
            {
                DecreaseAlpha();
                return;
            }
            switch (m_CurrentPrefab.m_CapabilityType)
            {
                case CapabilityType.Pan:
                case CapabilityType.Tilt:
                    PanTilt(m_AvailableChannelValue);
                    break;
                default:
                    Percentage();
                    break;
            }
            DecreaseAlpha();
        }

        private void Percentage()
        {
            float value = (m_CurrentPrefab.m_SliderValue / 255f) * 100f;
            m_Text.text = $"{value.ToString("0.00")} %";
        }

        public void SetMessage(string message)
        {
            m_CanvasGroup.alpha = 1;
            m_Text.text = message;
            DecreaseAlpha(force: true);
        }

        private void PanTilt(AvailableChannelValue extraPan)
        {
            if (extraPan != null)
            {
                try
                {
                    HouseBuilder.Lighting.GetAngles(extraPan.Capability, out float valStart, out float valEnd);

                    float value = (valEnd - valStart) * (((float)m_CurrentPrefab.m_SliderValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
                    value += valStart;

                    m_Text.text = $"{value.ToString("0.00")} Deg";
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Something went wrong here  {ex}");
                }
            }
        }

    }
}
