using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using EzySlice;

/**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
[CustomEditor(typeof(PlaneUsageExample))]
public class PlaneUsageExampleEditor : Editor
{
    public GameObject source;
    public Material crossMat;

    public class DoorWindow
    {
        public Vector3 m_Top = new Vector3(0, 4, 0);
        public Vector3 m_Bottom = new Vector3(0, -4, 0);
        public Vector3 m_Left = new Vector3(0, 0, -2);
        public Vector3 m_Right = new Vector3(0, 0, 2);
    }

    private const float WIGGLE_ROOM = 0.1f;

    private List<DoorWindow> m_DoorWindowList = new List<DoorWindow>();
    private List<GameObject> m_NotConnected = new List<GameObject>();
    private List<GameObject> m_DoorCheckerList = new List<GameObject>();

    public override void OnInspectorGUI()
    {
        PlaneUsageExample plane = (PlaneUsageExample)target;

        source = (GameObject)EditorGUILayout.ObjectField(source, typeof(GameObject), true);
        //cube.SetActive(false);
        if (source == null)
        {
            EditorGUILayout.LabelField("Add a GameObject to Slice.");

            return;
        }



        if (!source.activeInHierarchy)
        {
            EditorGUILayout.LabelField("Object is Hidden. Cannot Slice.");

            return;
        }

        if (source.GetComponent<MeshFilter>() == null)
        {
            EditorGUILayout.LabelField("GameObject must have a MeshFilter.");

            return;
        }

        crossMat = (Material)EditorGUILayout.ObjectField(crossMat, typeof(Material), true);



        if (GUILayout.Button("Cut Object"))
        {
            var items = source.GetComponentsInChildren<BoxCollider>();
            foreach(var item in items)
            {
                DoorWindow door = new DoorWindow();
                door.m_Top = new Vector3(0, item.bounds.max.y, 0);
                door.m_Bottom = new Vector3(0, item.bounds.min.y, 0);
                door.m_Left = new Vector3(0,  0, item.bounds.min.z);
                door.m_Right = new Vector3(0, 0, item.bounds.max.z);
                m_DoorWindowList.Add(door);
                item.GetComponent<Renderer>().enabled = false;
            }
           

            foreach (var DoorItem in m_DoorWindowList)
            {

                Slice(plane, DoorItem.m_Top, source.transform.up, crossMat);
                Slice(plane, DoorItem.m_Bottom, source.transform.up, crossMat);
                Slice(plane, DoorItem.m_Left, source.transform.forward, crossMat);
                Slice(plane, DoorItem.m_Right, source.transform.forward, crossMat);

                var list = source.GetComponentsInChildren<Renderer>();
                foreach (var item in list)
                {
                    if (item.enabled == true)
                    {
                        var boxCollider = item.gameObject.AddComponent<BoxCollider>();
                        boxCollider.isTrigger = true;
                    }
                }


            }

            foreach (var DoorItem in m_DoorWindowList)
            {
                CreateBox(DoorItem);
            }
        }
    }

    private void CreateBox(DoorWindow door)
    {
        GameObject doorChecker = new GameObject("doorChecker");
        m_DoorCheckerList.Add(doorChecker);
        float height = Mathf.Lerp(door.m_Top.y, door.m_Bottom.y, 0.5f);
        float sides = Mathf.Lerp(door.m_Left.z, door.m_Right.z, 0.5f);
        doorChecker.transform.position = new Vector3(0, height, sides);

        var boxCollider = doorChecker.gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(0, (door.m_Right.y - door.m_Left.y) - WIGGLE_ROOM, (door.m_Top.z - door.m_Bottom.z) - WIGGLE_ROOM);
        boxCollider.isTrigger = true;

        var rigid = doorChecker.AddComponent<Rigidbody>();
        rigid.isKinematic = true;
    }

    private void RemoveAllNonRenderers()
    {
        List<GameObject> Valid = new List<GameObject>();
        List<GameObject> NonValid = new List<GameObject>();
        var list = source.GetComponentsInChildren<Renderer>(true);
        foreach (var item in list)
        {
            if(item.enabled == true)
            {
                Valid.Add(item.gameObject);
            }
            else
            {
                NonValid.Add(item.gameObject);
            }
        }

        foreach (var item in Valid)
        {
            item.transform.SetParent(source.transform);
        }
        foreach (var item in NonValid)
        {
            if (item != source)
            {
                Destroy(item);
            }
        }
        foreach (var item in m_NotConnected)
        {
            if (item != source)
            {
               Destroy(item);
            }
        }

        foreach (var item in m_DoorCheckerList)
        {
            if (item != source)
            {
                Destroy(item);
            }
        }
    }


    private void Slice(PlaneUsageExample plane, Vector3 position, Vector3 direction, Material mat)
    {
        var list = source.GetComponentsInChildren<Renderer>();
        foreach (var item in list)
        {
            if (item.enabled == true)
            {
                SlicedHull hull = plane.SliceObject(item.gameObject, position, direction, mat);

                if (hull != null)
                {
                    GameObject low = hull.CreateLowerHull(item.gameObject, crossMat);
                    if(null == FindSource(low, source))
                    {
                        m_NotConnected.Add(low);
                    }

                    GameObject up = hull.CreateUpperHull(item.gameObject, crossMat);
                    if (null == FindSource(up, source))
                    {
                        m_NotConnected.Add(up);
                    }
                }
            }
        }

        list = source.GetComponentsInChildren<Renderer>();
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
