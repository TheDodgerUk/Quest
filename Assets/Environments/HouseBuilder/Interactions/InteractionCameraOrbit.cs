using HouseBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraOrbit: Interaction
{
    private const float MOUSE_SPEED = 1f;
    private Vector3 m_HitPoint = new Vector3();
    private readonly Vector3 CENTRE_OF_SCREEN = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    private GameObject m_Pivot;

    public float OrbitDampening = 10f;
    private Vector3 m_Rotation;

    public override void Awake()
    {
        m_Pivot = new GameObject("OrbitPivot");
        base.Awake();
    }
    public override void MouseDown()
    {
        if (Camera.main.orthographic == false)
        {
            Camera.main.transform.SetParent(this.transform);
        }
        Ray rayRef = Camera.main.ScreenPointToRay(CENTRE_OF_SCREEN);
        RaycastHit validRaycastHit = new RaycastHit();
        if (Physics.Raycast(rayRef, out validRaycastHit, Mathf.Infinity))
        {
            m_HitPoint = validRaycastHit.point;
        }
        else
        {
            if(false == InputUtility.LinePlaneIntersectionSingleDirection(out m_HitPoint, rayRef.origin, rayRef.direction, Vector3.zero, Vector3.up))
            {
                m_HitPoint = rayRef.origin + (rayRef.direction * 3);
            }
        }

        if(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Modular)
        {
            m_HitPoint = HouseBuilderInputModular.CLONE_PLACEMENT;
        }

        m_Rotation = Vector3.zero;
        m_Pivot.transform.rotation = Camera.main.transform.rotation;
        m_Pivot.transform.position = m_HitPoint;
        Camera.main.transform.SetParent(m_Pivot.transform);
        m_Rotation.x = m_Pivot.transform.eulerAngles.y; // yes, its going from one to another
        m_Rotation.y = m_Pivot.transform.eulerAngles.x; // yes, its going from one to another

        m_Rotation.x = m_Rotation.x.WrapTo180();
        m_Rotation.y = m_Rotation.y.WrapTo180();
    }

    public override void MouseDrag()
    {
        if (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() == null || false == (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() is InteractionGameObjectRotate))
        {
            if (Camera.main.orthographic == false)
            {
                RotateCameraAroundPoint();
            }
            else
            {
                var yaw = MOUSE_SPEED * Input.GetAxis("Mouse X");
                var clampY = Camera.main.transform.localRotation.eulerAngles.y + yaw;
                Camera.main.transform.localRotation = Quaternion.Euler(90, clampY, 0);
            }
        }
    }


    private void RotateCameraAroundPoint()
    {
        m_Rotation.x += Input.GetAxis("Mouse X");
        m_Rotation.y += Input.GetAxis("Mouse Y");

        m_Rotation.y = m_Rotation.y.WrapTo180();
        m_Rotation.y = m_Rotation.y.Clamp(-89, 89f);
        Quaternion QT = Quaternion.Euler(m_Rotation.y, m_Rotation.x, 0); // yes, its going from one to another

        this.m_Pivot.transform.rotation = QT;
    }

    public IEnumerator RotateCameraWithWidget(string direction)
    {
        if(Camera.main.orthographic == false || ((direction != "UpRot" && direction != "DownRot")))
        {
            Ray rayRef = Camera.main.ScreenPointToRay(CENTRE_OF_SCREEN);
            RaycastHit validRaycastHit = new RaycastHit();
            if (Physics.Raycast(rayRef, out validRaycastHit, Mathf.Infinity))
            {
                m_HitPoint = validRaycastHit.point;
            }

            Vector3 rot = Vector3.zero;
            switch (direction)
            {
                case "RightRot":
                    rot.y = -1;
                    break;
                case "LeftRot":
                    rot.y = 1;
                    break;
                case "UpRot":
                    rot.x = -1;
                    break;
                case "DownRot":
                    rot.x = 1;
                    break;
            }

            while (true)
            {
                Camera.main.transform.RotateAround(m_HitPoint, rot, 20 * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public override void MouseUp()
    {
        Camera.main.transform.SetParent(null);
    }
}
