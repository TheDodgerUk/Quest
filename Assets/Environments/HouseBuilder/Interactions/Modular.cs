using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class Modular
    {
        private const int START_INDEX = 1;

        public ContentModularMetaData ContentModularMetaDataRef { get; private set; }
        private InteractionGameObject m_Interaction;
        public ModularData ModularDataRef;

        public void Setup(InteractionGameObject inter, ContentModularMetaData data, ModularData modData)
        {
            m_Interaction = inter;
            ContentModularMetaDataRef = data;
            ModularDataRef = modData;

            ModularDataRef.m_SelectedChildIndexs.Resize(ContentModularMetaDataRef.m_Modular.Count);

            foreach (var item in ContentModularMetaDataRef.m_Modular)
            {
                item.m_ConnectionPointGameObject = MetaData.StringToGameObject(m_Interaction.RootGameObject, item.m_ConnectionPointGameObjectName);
                item.m_ItemsRootGameObject = MetaData.StringToGameObject(m_Interaction.RootGameObject, item.m_ItemsRootGameObjectName);
                item.m_ItemsChildrenGameObject.Clear();
                item.m_ItemsChildrenGameObject.AddRange(item.m_ItemsRootGameObject.transform.GetDirectChildren());
            }

            foreach (var item in ContentModularMetaDataRef.m_Modular)
            {
                item.m_ItemsChildrenGameObject.ForEach(e => e.gameObject.SetActive(false));
                item.m_ItemsChildrenGameObject.First().SetActive(true);
            }
            Rebuild();
        }

        public void Rebuild()
        {
            for(int i = 0; i < ModularDataRef.m_SelectedChildIndexs.Count; i++)
            {
                var item = ContentModularMetaDataRef.m_Modular[i];
                item.m_ItemsChildrenGameObject.ForEach(e => e.gameObject.SetActive(false));

                int index = ModularDataRef.m_SelectedChildIndexs[i];
                item.m_ItemsChildrenGameObject[index].SetActive(true);
            }
        }

        public void Rebuild(ModularData data)
        {
            ModularDataRef = data;
            Rebuild();
        }


    }
}

