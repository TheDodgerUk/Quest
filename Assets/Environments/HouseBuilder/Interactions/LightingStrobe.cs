using Fixtures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class LightingStrobe
    {
        private Lighting m_Lighting;
        private MonoBehaviour m_Mono;
        public LightingStrobe(Lighting lighting, MonoBehaviour mono)
        {
            m_Lighting = lighting;
            m_Mono = mono;
            m_Mono.StartCoroutine(IEnumeratorManualUpdateStrobeEffect());
        }

        public float m_StrobeIntensity = 1f;
        public GameObject m_StrobeGameObject;
        public CapabilityElement m_CapabilityElement;
        private bool m_AllowUpdate = false;

        public float m_Percentage;
        public float m_TotalLoopTime = 0f;
        public float m_CurrentLoopTime = 0f;

        public float m_EffectTime = 0;
        public float m_EffectFlashTime = 0;

        public void StutterStrobe(string strobeLinkName, int dmxValue, GameObject obj)
        {
            m_StrobeGameObject = obj;
            var strobe = m_Lighting.ContentLightingMetaDataRef.m_Fixure.AvailableChannels[strobeLinkName];
            if (strobe.Capabilities != null)
            {
                foreach (var item in strobe.Capabilities)
                {
                    var min = item.DmxRange[0];
                    var max = item.DmxRange[1];
                    if (dmxValue >= min && dmxValue <= max)
                    {
                        var percentage = Mathf.InverseLerp(min, max, dmxValue);
                        UpdateCapabilityElement(percentage, item);
                        UpdateStrobeEffect(dmxValue);
                        return; // only does it once 
                    }
                }
            }
        }

        public void UpdateCapabilityElement(float percentage, CapabilityElement element)
        {
            m_CapabilityElement = element;
            m_Percentage = percentage;
        }

        private void UpdateStrobeEffect(float dmxValue)
        {
            m_EffectFlashTime = 0;
            m_TotalLoopTime = 0;

            if (m_StrobeGameObject != null && m_CapabilityElement != null)
            {
                Lighting.LightBeamData lights = null;
                switch (m_CapabilityElement.ShutterEffect)
                {

                    // one off
                    case ShutterEffectEnum.Closed:
                        m_AllowUpdate = false;
                        lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
                        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * 0f));
                        break;
                    case ShutterEffectEnum.Open:
                        m_AllowUpdate = false;
                        lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
                        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * 1f));
                        break;
                    case ShutterEffectEnum.Fade:
                        m_AllowUpdate = false;
                        lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
                        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * m_Percentage));
                        break;


                    // needs looping 
                    case ShutterEffectEnum.Pulse:
                    case ShutterEffectEnum.RampDown:
                        m_AllowUpdate = true;
                        m_EffectTime = LerpAmountWithRandom(m_CapabilityElement.TimeStrobeRange, m_Percentage);
                        m_TotalLoopTime = m_EffectTime;
                        break;
                    case ShutterEffectEnum.RampUp:
                        m_AllowUpdate = true;
                        m_EffectTime = LerpAmountWithRandom(m_CapabilityElement.TimeStrobeRange, m_Percentage);
                        m_TotalLoopTime = m_EffectTime;
                        break;
                    case ShutterEffectEnum.Strobe:

                        m_AllowUpdate = true;
                        m_EffectTime = LerpAmountWithRandom(m_CapabilityElement.TimeStrobeRange, m_Percentage);
                        m_EffectFlashTime = LerpAmountWithRandom(m_CapabilityElement.TimeFlashRange, m_Percentage);
                        m_TotalLoopTime = m_EffectTime + m_EffectFlashTime;
                        break;
                    default:
                        Debug.LogError($"Not found: {m_CapabilityElement.ShutterEffect}");
                        break;
                }
                Debug.LogError($" {m_CapabilityElement.ShutterEffect} m_Percentage {m_Percentage}   dmxValue {dmxValue}");
            }
        }

        private IEnumerator IEnumeratorManualUpdateStrobeEffect()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                m_CurrentLoopTime += Time.deltaTime;


                if (m_AllowUpdate == true && m_TotalLoopTime != 0) // if m_TotalLoopTime = 0,  then we get Nan when we do  -> m_CurrentLoopTime = (m_CurrentLoopTime % m_TotalLoopTime);
                {
                    m_CurrentLoopTime = (m_CurrentLoopTime % m_TotalLoopTime);
                    float percentage = m_CurrentLoopTime / m_TotalLoopTime;
                    if (m_StrobeGameObject != null && m_CapabilityElement != null)
                    {
                        var lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
                        switch (m_CapabilityElement.ShutterEffect)
                        {
                            case ShutterEffectEnum.Strobe:
                                UpdateFromTween_Strobe(percentage);
                                break;

                            case ShutterEffectEnum.RampUp:
                                UpdateFromTween_Strobe_Ramp(percentage);
                                break;

                            case ShutterEffectEnum.RampDown:
                                UpdateFromTween_Strobe_Ramp(1 - percentage);
                                break;

                            default:
                                Debug.LogError($"Not found: {m_CapabilityElement.ShutterEffect}");
                                break;
                        }
                    }
                }
            }
        }
    
        private float LerpAmountWithRandom(List<double> data, float percentage)
        {
            float amount = Mathf.Lerp((float)data[0], (float)data[1], m_Percentage);
            if (m_CapabilityElement.randomTiming == true)
            {
                amount = Random.Range((float)data[0], (float)data[1]);
            }
            return amount;
        }

        private void UpdateFromTween_Strobe(float percentage)
        {
            float offPercentage = m_EffectTime / m_TotalLoopTime;

            float flashAmount = 0;
            if (percentage >= offPercentage)
            {
                flashAmount = 1;
            }
            var lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
            lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * flashAmount));
        }


        private void UpdateFromTween_Strobe_Ramp(float percentage)
        {
            var lights = m_Lighting.CollectLightBeamData(m_StrobeGameObject);
            lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * percentage));
        }
    }
}
