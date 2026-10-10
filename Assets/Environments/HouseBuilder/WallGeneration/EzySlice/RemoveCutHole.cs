using HouseBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

public class RemoveCutHole : MonoBehaviour
{
    private BoxCollider m_BoxCollider;
    private Rigidbody m_Rigidbody;
    private WallObjectLinkerData m_WallObjectLinkerData;

    public void Initialise(BoxCollider boxColliderRef, float wallThickness, WallObjectLinkerData wallObjectLinkerDataRef)
    {
        m_WallObjectLinkerData = wallObjectLinkerDataRef;
        m_WallObjectLinkerData.m_RemovedWalls.Clear();

        m_BoxCollider = this.gameObject.ForceComponent<BoxCollider>();
        m_BoxCollider.size = boxColliderRef.size;
        Vector3 size = m_BoxCollider.size;
        size.z = 1;
        size *= 0.9f;
        m_BoxCollider.size = size;
        m_BoxCollider.isTrigger = true;

        m_Rigidbody = this.gameObject.ForceComponent<Rigidbody>();
        m_Rigidbody.useGravity = false;

        this.gameObject.transform.position = boxColliderRef.gameObject.transform.position;
        this.gameObject.transform.rotation = boxColliderRef.gameObject.transform.rotation;
        this.gameObject.transform.position += (wallThickness * -this.gameObject.transform.forward);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.name == HouseBuilderConstants.GENERATED_UPPER_HULL || other.gameObject.name == HouseBuilderConstants.GENERATED_LOWER_HULL)
        {
            var rendererRef = other.gameObject.GetComponent<Renderer>();
            if(null != rendererRef)
            {
                if (m_WallObjectLinkerData.m_RemovedWalls.Contains(rendererRef) == false)
                {
                    m_WallObjectLinkerData.m_RemovedWalls.Add(rendererRef);
                    rendererRef.enabled = false;
                }
            }
        }
    }

}
