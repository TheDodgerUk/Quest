using HouseBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ModeContainerCatalogueGeneratedData
{
    public event Action<List<CatalogueGeneratedData>> OnItemsChanged;

    private GameObject m_Holder;
    private MonoBehaviour m_Mono;

    public GameObject Holder => m_Holder;

    [SerializeField]
    private List<CatalogueGeneratedData> m_InteractionList = new List<CatalogueGeneratedData>();

    public ModeContainerCatalogueGeneratedData(string holderName, GameObject parent, MonoBehaviour mono)
    {
        m_Holder = new GameObject(holderName);
        m_Holder.transform.SetParent(parent.transform);
        m_Mono = mono;
    }

    public void AttachToThisParent(GameObject obj)
    {
        obj.transform.SetParent(m_Holder.transform);
    }


    public CatalogueGeneratedData GetItem(GameObject obj) => m_InteractionList.Find(e => e.GeneratedItemRef == obj);
    public List<CatalogueGeneratedData> GetList => m_InteractionList;
    public void ToggleShadows(bool enable) => GetList.ForEach(e => e.InteractionRef.SetShadows(enable));


    public void AddGameObject(CatalogueGeneratedData obj)
    {
        m_InteractionList.Add(obj);
        obj.GeneratedItemRef.transform.SetParent(m_Holder.transform);
        OnItemsChanged?.Invoke(m_InteractionList);
    }



    public void Delete(CatalogueGeneratedData obj, bool displayLog = true)
    {
        if (displayLog == true)
        {
            Debug.Log($"Catalogue item Deleted: {obj.CatalogueEntryRef.ShortName}");
        }


        int startCount = m_InteractionList.Count;
        m_InteractionList.Remove(obj);
        if (obj != null && obj.InteractionRef != null)
        {
            UnityEngine.Object.Destroy(obj.InteractionRef);
            m_Mono.WaitForEndOfFrame(() =>
            {
                if (obj != null && obj.GeneratedItemRef != null)
                {
                    UnityEngine.Object.Destroy(obj.GeneratedItemRef);
                }
            });
        }
        OnItemsChanged?.Invoke(m_InteractionList);

    }

    public void Delete(GameObject obj, bool displayLog = true)
    {
        var item = m_InteractionList.Find(e => e.GeneratedItemRef == obj);

        Delete(item, displayLog);
    }

    public void EnableAllColliders(bool enable)
    {
        foreach (var item in m_InteractionList)
        {
            item.InteractionRef.m_OtherColliders.RemoveAll(e => e == null);
        }


        foreach (var item in m_InteractionList)
        {
            if (null != item.InteractionRef.MainCollider)
            {
                item.InteractionRef.MainColliderGameObject.SetActive(enable);
            }

            foreach (var other in item.InteractionRef.m_OtherColliders)
            {
                other.enabled = enable;
            }
        }
    }

    public void DisableMainColliderInVR()
    {
        foreach (var item in m_InteractionList)
        {
            if (null != item.InteractionRef.MainCollider && item.InteractionRef.m_OtherColliders.Count > 0)
            {
                item.InteractionRef.MainColliderGameObject.SetActive(item.InteractionRef.PlacementMetaData.m_MainColliderEnabledInVR);
            }
        }
    }

    public void EnableMainColliderInVR()
    {
        foreach (var item in m_InteractionList)
        {
            if (null != item.InteractionRef.MainCollider && item.InteractionRef.m_OtherColliders.Count > 0)
            {
                item.InteractionRef.MainColliderGameObject.SetActive(true);
            }
        }
    }


    public void ShowGameObjects(bool enable)
    {
        m_Holder.SetActive(enable);
    }
}
