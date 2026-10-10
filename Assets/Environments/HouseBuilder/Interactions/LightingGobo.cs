using Fixtures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class LightingGobo
    {
        private Lighting m_Lighting;
        private MonoBehaviour m_Mono;
        public LightingGobo(Lighting lighting, MonoBehaviour mono)
        {
            m_Lighting = lighting;
            m_Mono = mono;
        }

        public float m_StrobeIntensity = 1f;
        public GameObject m_rootObject;
        public CapabilityElement m_CapabilityElement;



        public void GoboColorPreset(string goboLinkName, int dmxValue, GameObject obj)
        {
            m_rootObject = obj;
            AvailableChannelValue gobo = m_Lighting.ContentLightingMetaDataRef.m_Fixure.AvailableChannels[goboLinkName];
            for (int i = 0; i < gobo.Capabilities.Count; i++)
            {
                CapabilityElement item = gobo.Capabilities[i];
                long min = item.DmxRange[0];
                long max = item.DmxRange[1];
                if (dmxValue >= min && dmxValue <= max)
                {
                    m_CapabilityElement = item;
                    InternalGoboColorPreset(m_CapabilityElement);
                    return; // only does it once 
                }
            }
        }


        private void InternalGoboColorPreset(CapabilityElement data)
        {
            Color startColor = Color.white;
            if (data.Colors.ContainsIndex(0) == true)
            {
                if (ColorUtility.TryParseHtmlString(data.Colors[0], out Color colorOut1))
                {
                    startColor = colorOut1;
                }

                var lights = m_Lighting.CollectLightBeamData(m_rootObject);
                foreach (var light in lights.m_CombinedData)
                {
                    if (light.m_BasicLight != null)
                    {
                        light.m_BasicLight.color = startColor;
                    }
                    else
                    {
                        light.m_VolumetricLightBeam.color = startColor;
                    }
                }
            }
            else
            {
                Debug.LogError("data.Colors == null");
            }
        }



        public void GoboWheel(string goboLinkName, int dmxValue, GameObject obj)
        {
            m_rootObject = obj;
            var gobo = m_Lighting.ContentLightingMetaDataRef.m_Fixure.AvailableChannels[goboLinkName];
            foreach (var item in gobo.Capabilities)
            {
                var min = item.DmxRange[0];
                var max = item.DmxRange[1];
                if (dmxValue >= min && dmxValue <= max)
                {
                    var percentage = Mathf.InverseLerp(min, max, dmxValue);
                    m_CapabilityElement = item;
                    UpdateGoboEffect();
                    return; // only does it once 
                }
            }
        }


        public void GoboColourWheel(string goboLinkName, int dmxValue, GameObject obj)
        {
            m_rootObject = obj;
            AvailableChannelValue gobo = m_Lighting.ContentLightingMetaDataRef.m_Fixure.AvailableChannels[goboLinkName];
            Dictionary<string, Wheel> dataWheels = m_Lighting.ContentLightingMetaDataRef.m_Fixure.Wheels;
            for (int i = 0; i < gobo.Capabilities.Count; i++)
            {
                CapabilityElement item = gobo.Capabilities[i];
                long min = item.DmxRange[0];
                long max = item.DmxRange[1];
                if (dmxValue >= min && dmxValue <= max)
                {
                    m_CapabilityElement = item;
                    InternalGoboColourWheel(m_CapabilityElement, dataWheels, goboLinkName);
                    return; // only does it once 
                }
            }
        }


        // like sliderPanel.cs
        private void InternalGoboColourWheel(CapabilityElement data, Dictionary<string, Wheel> wheelData, string channelName)
        {
            var wheel = wheelData[channelName];
            float realSlotNumber = data.SlotNumber - 1;  // they start at 1 and can be 2.5 , so that you lerp between them 
            int start = realSlotNumber.Floor();
            int end = realSlotNumber.Ceil();

            Color startColor = Color.white;
            Color endColor = Color.white;


            var lights = m_Lighting.CollectLightBeamData(m_rootObject);

            try
            {
                if (wheel.Slots[start].Type == SlotType.Color)
                {
                    if (ColorUtility.TryParseHtmlString(wheel.Slots[start].Colors[0], out Color colorOut1))
                    {
                        startColor = colorOut1;
                        endColor = startColor;
                    }

                    
                    if (wheel.Slots[end].Colors.ContainsIndex(1) == true && ColorUtility.TryParseHtmlString(wheel.Slots[end].Colors[1], out Color colorOut2))
                    {
                        endColor = colorOut2;
                    }
                    if (start != end)
                    {
                        foreach(var light in lights.m_CombinedData)
                        {
                            if (light.m_BasicLight != null)
                            {
                                light.m_BasicLight.color = Color.Lerp(startColor, endColor, (end - realSlotNumber));
                            }
                            else
                            {
                                light.m_VolumetricLightBeam.color = Color.Lerp(startColor, endColor, (end - realSlotNumber));
                            }
                        }

                    }
                    else
                    {
                        foreach (var light in lights.m_CombinedData)
                        {
                            if (light.m_BasicLight != null)
                            {
                                light.m_BasicLight.color = startColor;
                            }
                            else
                            {
                                light.m_VolumetricLightBeam.color = startColor;
                            }
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("ERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRROR, json has been parsed correclty, but its trying to ref data which they have not included ");
            }
        }



        private void UpdateGoboEffect()
        {
            if (m_rootObject != null && m_CapabilityElement != null)
            {
                var lights = m_Lighting.CollectLightBeamData(m_rootObject);
                Debug.LogError($"lights  {m_CapabilityElement.SpriteName} ");
                Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, m_CapabilityElement.SpriteName, (texture) =>
                {
                    lights.m_CombinedData.ForEach(e => e.m_BasicLight.cookie = texture);
                });
            }
                ////switch (m_CapabilityElement.ShutterEffect)
                ////{

                ////    case ShutterEffectEnum.Closed:
                ////        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * 0f));
                ////        m_AllowUpdate = false;
                ////        break;
                ////    case ShutterEffectEnum.Open:
                ////        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = (lights.m_GroupIntensity * 1f));
                ////        m_AllowUpdate = false;
                ////        break;

                ////    case ShutterEffectEnum.Pulse:
                ////    case ShutterEffectEnum.RampDown:
                ////    case ShutterEffectEnum.RampUp:
                ////    case ShutterEffectEnum.Strobe:
                ////        m_CurrentStrobeTime = 0;
                ////        m_AllowUpdate = true;
                ////        m_CurrentStrobeUpdateTimeLimit = new Vector2(m_Percentage * 5f, m_Percentage * 5f); // magic numbers ??!!
                ////        m_CurrentEffectMinMaxTime = new Vector2(m_Percentage * 5f, m_Percentage * 5f);
                ////        if (m_CapabilityElement.randomTiming == true)
                ////        {
                ////            m_CurrentStrobeUpdateTimeLimit = new Vector2(m_Percentage * 4f, m_Percentage * 5f); // magic numbers ??!!
                ////            m_CurrentEffectMinMaxTime = new Vector2(m_Percentage * 2f, m_Percentage * 4f);
                ////        }
                ////        break;
                ////    default:
                ////        Debug.LogError($"Not found {m_CapabilityElement.ShutterEffect}");
                ////        break;
                ////}
            
        }



    }
}
