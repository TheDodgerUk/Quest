#if UNITY_ANDROID &&VR_INTERACTION
//using OculusSampleFramework;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class VrButtons
    {
        public void Setup(InteractionGameObject inter, ContentButtonMetaData data)
        {
            data.CollectAllData(inter.gameObject);
            List<GameObject> newSliders = new List<GameObject>();
            for (int i = 0; i < data.m_ItemData.Count; i++)
            {
                int index = i;
                
                ContentButtonMetaData.ButtonItemData buttonItemData = data.m_ItemData[index];

                var col = buttonItemData.m_ModelGameObject.GetComponent<Collider>();
                if (col != null)
                {
                    col.isTrigger = true;
                }
                else
                {
                    Debug.LogError($"inter {inter.name}  is missinga collider on {data.m_ItemData[i].m_ModelGameObjectName}");
                }
                newSliders.Add(buttonItemData.m_ModelGameObject);

                var button = buttonItemData.m_ModelGameObject.ForceComponent<VrInteractionButtonLatched>();

                button.Initialise(inter.RootGameObject, buttonItemData, (amout) =>
                {
                    Debug.LogError($" {amout}", data.m_ItemData[index].m_ModelGameObject);
                });

            }
        }

    }
}
