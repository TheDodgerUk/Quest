#if UNITY_ANDROID && VR_INTERACTION
//using OculusSampleFramework;
#endif

using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class VrSliders
    {
        public void Setup(InteractionGameObject inter, ContentSliderMetaData data)
        {
            List<GameObject> newSliders = new List<GameObject>();
            for (int i = 0; i < data.m_ItemData.Count; i++)
            {
                int index = i;
                ContentSliderMetaData.ItemData sliderItemData = data.m_ItemData[index];

                sliderItemData.m_ModelGameObject = inter.transform.Search(data.m_ItemData[index].m_ModelGameObjectName).gameObject;
                var col = sliderItemData.m_ModelGameObject.ForceComponent<BoxCollider>();
                col.isTrigger = true;
                newSliders.Add(sliderItemData.m_ModelGameObject);
                var slider = sliderItemData.m_ModelGameObject.ForceComponent<VrInteractionSlider>();


                slider.Initialise(inter.RootGameObject, sliderItemData, (amout) =>
                {
                    Debug.LogError($" {amout}", data.m_ItemData[index].m_ModelGameObject);
                });
            }
        }

    }
}
