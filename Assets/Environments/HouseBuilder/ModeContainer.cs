using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeContainer<T> where T : Interaction
{
    private List<T> m_InteractionList = new List<T>();
    public GameObject Holder { get; private set; }

    public ModeContainer(string holderName, GameObject parent)
    {
        if (null != parent)
        {
            Holder = new GameObject(holderName);
            Holder.transform.SetParent(parent.transform);
        }
    }

    public ModeContainer(){}

    public List<T> GetList => m_InteractionList;
    public T GetAt(int index) => m_InteractionList[index];

    public void AddGameObject(T obj)
    {
        m_InteractionList.Add(obj);
        if (null != Holder)
        {
            obj.transform.SetParent(Holder.transform);
        }
    }

    public void DeleteAll()
    {
        foreach (var item in m_InteractionList)
        {
            UnityEngine.Object.Destroy(item.gameObject);
        }
        m_InteractionList.Clear();
    }

    public void Delete(T obj)
    {
        m_InteractionList.Remove(obj);
        UnityEngine.Object.Destroy(obj.gameObject);
    }

    public void Delete(GameObject obj)
    {
        m_InteractionList.Remove(obj.GetComponent<T>());
        UnityEngine.Object.Destroy(obj.gameObject);
    }

    public void EnableAllColliders(bool enable)
    {
        foreach (var item in m_InteractionList)
        {
            if (null != item.MainCollider)
            {
                item.MainColliderGameObject?.SetActive(enable);
            }

            foreach (var other in item.m_OtherColliders)
            {
                other.enabled = enable;
            }
        }
    }

    public void DisableMainColliderInVR()
    {
        foreach (var item in m_InteractionList)
        {
            if (null != item.MainCollider)
            {
                if (item.PlacementMetaData.m_MainColliderEnabledInVR == true)
                {
                    item.MainColliderGameObject?.SetActive(false);
                }
            }
        }
    }


    public void ShowGameObjects(bool enable)
    {
        Holder.SetActive(enable);
    }
}
