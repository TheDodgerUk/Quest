using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EzySlice;
using static HouseBuilder.HouseBuilderConstants;
using HouseBuilder;
using System;

public class HoleCutter : MonoBehaviour
{
    private static List<GameObject> m_NewHoles = new List<GameObject>();
    public GameObject m_SourceGameObject;
    private Renderer m_Renderer;
    public Material crossMat;
    private float m_WallThickness;
    private List<DoorWindow> m_DoorWindowList = new List<DoorWindow>();
    private List<GameObject> m_NotConnected = new List<GameObject>();
    private bool m_IsCurved = false;
    private HouseBuilder.InteractionWallGeneratedPoint m_GeneratedPoint;

    public class DoorWindow
    {
        public Vector3 m_Top = new Vector3(0, 3f, 0);
        public Vector3 m_Bottom = new Vector3(0, 0.5f, 0);
        public Vector3 m_Left = new Vector3(0, 0, 3);
        public Vector3 m_Right = new Vector3(0, 0, 4);
        public BoxCollider m_BoxCollider;
        public CatalogueGeneratedData m_CatalogueGeneratedData;
    }


    public void SliceWallToGenerateHoles(HouseBuilder.InteractionWallGeneratedPoint generatedPoint, GameObject source, float wallThickness, float bendAmount, Action holesCutoutCallback)
    {
        List<CatalogueGeneratedData> contents = null;
        if (HouseBuilderRoot.Instance != null)
        {
            contents = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
        }
        else
        {
            contents = HouseViewer.HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList;
        }
        var connectedObjects = contents.FindAll(e => e.m_WallObjectLinkerData != null);

        m_IsCurved = (bendAmount != 0);
        m_WallThickness = wallThickness;
        if (connectedObjects == null || connectedObjects.Count == 0)
        {
            return;
        }
        m_GeneratedPoint = generatedPoint;
        m_SourceGameObject = source;
        m_Renderer = m_SourceGameObject.GetComponent<Renderer>();
        m_DoorWindowList.Clear();
        foreach (var item in connectedObjects)
        {
            if (item.InteractionRef.PlacementMetaData.Placement.IsCutout() == false)
            {
                continue;
            }

            if (item.m_WallObjectLinkerData == null)
            {
                continue;
            }

            if (m_GeneratedPoint.GeneratedWholeRef.ThisWallIndex != item.m_WallObjectLinkerData.m_WallIndex || m_GeneratedPoint.ThisWallPointIndex != item.m_WallObjectLinkerData.m_WallPointIndex)
            {
                continue;
            }
#if UNITY_EDITOR
            Debug.Log("SliceWallToGenerateHoles");
#endif
            BoxCollider boxColliderRef = (BoxCollider)item.InteractionRef.MainCollider;
            DoorWindow door = new DoorWindow();
            door.m_CatalogueGeneratedData = item;
            door.m_BoxCollider = boxColliderRef;
            door.m_Top = new Vector3(0, boxColliderRef.size.y / 2, 0) + boxColliderRef.transform.localPosition + boxColliderRef.transform.parent.localPosition;
            door.m_Bottom = new Vector3(0, -boxColliderRef.size.y / 2, 0) + boxColliderRef.transform.localPosition + boxColliderRef.transform.parent.localPosition;
            door.m_Left = new Vector3(boxColliderRef.size.x / 2, 0, 0) + boxColliderRef.transform.localPosition + boxColliderRef.transform.parent.localPosition;
            door.m_Right = new Vector3(-boxColliderRef.size.x / 2, 0, 0) + boxColliderRef.transform.localPosition + boxColliderRef.transform.parent.localPosition;

            door.m_Top = item.GeneratedItemRef.gameObject.transform.TransformPoint(door.m_Top);
            door.m_Bottom = item.GeneratedItemRef.gameObject.transform.TransformPoint(door.m_Bottom);
            door.m_Left = item.GeneratedItemRef.gameObject.transform.TransformPoint(door.m_Left);
            door.m_Right = item.GeneratedItemRef.gameObject.transform.TransformPoint(door.m_Right);
            Debug.Log($"Door/wall {item.CatalogueEntryRef.ShortName}  {item.GeneratedItemRef.transform.position}", item.InteractionRef.RootGameObject);

            m_Renderer.enabled = true;
            m_DoorWindowList.Add(door);
        }

        TaskAction holesCutout = new TaskAction(m_DoorWindowList.Count, () => { holesCutoutCallback.Invoke(); });

        if (false == m_IsCurved)
        {
            foreach (var doorItem in m_DoorWindowList)
            {
                Slice(doorItem.m_Top, m_SourceGameObject.transform.up, crossMat);
                Slice(doorItem.m_Bottom, m_SourceGameObject.transform.up, crossMat);
                Slice(doorItem.m_Left, m_SourceGameObject.transform.right, crossMat);
                Slice(doorItem.m_Right, m_SourceGameObject.transform.right, crossMat);

                AddBoxCollidersToBase();
            }
            foreach (var doorItem in m_DoorWindowList)
            {
                CutOutTheholes(doorItem, doorItem.m_CatalogueGeneratedData.m_WallObjectLinkerData, holesCutout);
            }
        }
        else
        {
            foreach (var doorItem in m_DoorWindowList)
            {
                Slice(doorItem.m_Top, m_SourceGameObject.transform.up, crossMat);
                Slice(doorItem.m_Bottom, m_SourceGameObject.transform.up, crossMat);
                Slice(doorItem.m_Left, m_SourceGameObject.transform.forward, crossMat);
                Slice(doorItem.m_Right, m_SourceGameObject.transform.forward, crossMat);

                AddBoxCollidersToBase();
            }
            foreach (var doorItem in m_DoorWindowList)
            {
                CutOutTheholes(doorItem, doorItem.m_CatalogueGeneratedData.m_WallObjectLinkerData, holesCutout);
            }
        }
    }

    private void DebugDrawPlanes(DoorWindow door, Transform transformRef)
    {
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Plane);
        top.name = "top";
        top.transform.position = door.m_Top;

        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Plane);
        bottom.name = "bottom";
        bottom.transform.position = door.m_Bottom;

        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Plane);
        left.name = "left";
        left.transform.position = door.m_Left;
        left.transform.rotation = m_SourceGameObject.transform.rotation;
        left.transform.Rotate(90, 0, 0);

        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Plane);
        right.name = "right";
        right.transform.position = door.m_Right;
        right.transform.rotation = m_SourceGameObject.transform.rotation;
        right.transform.Rotate(90, 0, 0);
    }


    private void AddBoxCollidersToBase()
    {
        var list = m_SourceGameObject.GetComponentsInChildren<Renderer>();
        foreach (var item in list)
        {
            if (item.enabled == true)
            {
                if (true == m_IsCurved)
                {
                    var boxCollider = item.gameObject.ForceComponent<MeshCollider>();
                }
                else
                {
                    var boxCollider = item.gameObject.ForceComponent<BoxCollider>();
                }

            }
        }
    }


    private void CutOutTheholes(DoorWindow door, WallObjectLinkerData wallObjectLinkerDataRef, TaskAction holesCutout)
    {
        GameObject newHole = new GameObject("newHole");
        m_NewHoles.Add(newHole);
        RemoveCutHole removeCutHoleRef = newHole.ForceComponent<RemoveCutHole>();
        removeCutHoleRef.Initialise(door.m_BoxCollider, m_WallThickness, wallObjectLinkerDataRef);

        // for some reason when load in a scene some holes were not getting deleted
        this.WaitForFrames(2, () =>
        {
            for (int i = 0; i < m_NewHoles.Count; i++)
            {
                if (m_NewHoles[i] != null)
                {
                    Destroy(m_NewHoles[i]);
                }
            }
            holesCutout.Increment();
        });

        this.WaitForFrames(10, () =>
        {
            m_NewHoles.Clear();
            Destroy(removeCutHoleRef);
        });
    }




    private void Slice(Vector3 position, Vector3 direction, Material mat)
    {
        var list = m_SourceGameObject.GetComponentsInChildren<Renderer>();
        foreach (var item in list)
        {
            if (item.enabled == true)
            {
                SlicedHull hull = item.gameObject.Slice(position, direction, mat);

                if (hull != null)
                {
                    GameObject low = hull.CreateLowerHull(item.gameObject, crossMat);
                    if (low.name != GENERATED_ERROR)
                    {
                        if (null == FindSource(low, m_SourceGameObject))
                        {
                            m_NotConnected.Add(low);
                        }
                    }
                    else
                    {
                        Destroy(low);
                    }

                    GameObject up = hull.CreateUpperHull(item.gameObject, crossMat);
                    if (low.name != GENERATED_ERROR)
                    {
                        if (null == FindSource(up, m_SourceGameObject))
                        {
                            m_NotConnected.Add(up);
                        }
                    }
                    else
                    {
                        Destroy(up);
                    }
                }
            }
        }

        list = m_SourceGameObject.GetComponentsInChildren<Renderer>();
        foreach (var item in list)
        {
            if (item.gameObject.transform.childCount > 0)
            {
                item.enabled = false;
            }
        }
    }


    public static GameObject FindSource(GameObject childObject, GameObject source)
    {
        Transform t = childObject.transform;
        while (t.parent != null)
        {
            if (t.parent.gameObject == source.gameObject)
            {
                return t.parent.gameObject;
            }
            t = t.parent.transform;
        }
        return null; // Could not find a parent with given tag.
    }

}

