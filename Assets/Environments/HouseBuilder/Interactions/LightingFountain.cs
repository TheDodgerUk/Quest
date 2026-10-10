using Fixtures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class LightingFountain
    {
        private Lighting m_Lighting;
        private MonoBehaviour m_Mono;
        private GameObject m_FountainObject;
        private CapabilityElement m_CapabilityElement;
        private int m_DmxValue;
        //private Dictionary<GameObject, ParticleEffect> m_ParticleDictionary = new Dictionary<GameObject, ParticleEffect>();


        public LightingFountain(Lighting lighting, MonoBehaviour mono)
        {
            m_Lighting = lighting;
            m_Mono = mono;
        }

        public void Fountain(string linkName, int dmxValue, GameObject obj)
        {
            
            m_FountainObject = obj;
            AvailableChannelValue gobo = m_Lighting.ContentLightingMetaDataRef.m_Fixure.AvailableChannels[linkName];
            foreach (CapabilityElement item in gobo.Capabilities)
            {
                int min = (int)item.DmxRange[0];
                int max = (int)item.DmxRange[1];
                if (dmxValue >= min && dmxValue <= max)
                {
                    m_CapabilityElement = item;
                    m_DmxValue = dmxValue;
                    UpdateFountainEffect();
                    return; // only does it once 
                }
            }
        }


        ////private ParticleEffect GetParticleEffect(GameObject obj)
        ////{
        ////    if(m_ParticleDictionary.ContainsKey(obj) == false)
        ////    {
        ////        var item = obj.ForceComponent<ParticleEffect>();
        ////        m_ParticleDictionary.Add(obj, item);
        ////    }
        ////    return m_ParticleDictionary[obj];
        ////}

        private void UpdateFountainEffect()
        {
            if (m_FountainObject != null && m_CapabilityElement != null)
            {
               // var item = GetParticleEffect(m_FountainObject);
               // item.Toggle(true);


                if (m_CapabilityElement.Type == CapabilityType.EffectParameter)
                {
                    if (float.TryParse(m_CapabilityElement.Parameter, out float amount) == true)
                    {
                        float value = amount / 10; // they are all ints, so  10 = 1, normalized  
                      //  item.SetLifeTimePercentageNormalized(value);
                    }
                }

                if (m_CapabilityElement.Type == CapabilityType.Fog || m_CapabilityElement.Type == CapabilityType.Confetti)
                {
                    float value = m_DmxValue / OscController.OscControllerRoot.MAX_DMX_VALUE_FLOAT;
                   // item.SetLifeTimePercentageNormalized(value);
                  //  item.SetEmissionRateNormalized(value);
                }


                if (m_CapabilityElement.Type == CapabilityType.NoFunction)
                {
                  //  item.SetLifeTimePercentageNormalized(0);
                }
            }
        }



    }
}
