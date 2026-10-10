using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraPivot : Interaction
{
    private const float MOUSE_SPEED = 1f;
    private Vector3 m_HitPoint = new Vector3();

    public override void MouseDown()
    {
        if (Camera.main.orthographic == false)
        {
            Camera.main.transform.SetParent(this.transform);
        }

        Ray rayRef = Camera.main.ScreenPointToRay(HouseBuilder.HouseBuilderConstants.MousePosition());
        RaycastHit validRaycastHit = new RaycastHit();
        Physics.Raycast(rayRef, out validRaycastHit, Mathf.Infinity);
        m_HitPoint = validRaycastHit.point;
    }

    public override void MouseDrag()
    {
        
        if (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() == null || false == (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() is InteractionGameObjectRotate))
        {
            if (Camera.main.orthographic == false)
            {
                PivotCamera();
                //RotateCameraAroundPoint();
            }
            else
            {

                var yaw = MOUSE_SPEED * Input.GetAxis("Mouse X");
                var clampY = Camera.main.transform.localRotation.eulerAngles.y + yaw;
                Camera.main.transform.localRotation = Quaternion.Euler(90, clampY, 0);
            }

        }
    }



    private void PivotCamera()
    {
        var pitch = MOUSE_SPEED * -Input.GetAxis("Mouse Y");
        Vector3 oldRotation = Camera.main.transform.localRotation.eulerAngles;
        Camera.main.transform.Rotate(Vector3.left, -pitch);

        var x = Camera.main.transform.localRotation.eulerAngles.x;
        x = x.WrapTo180();
        if (x <= -88 || x >= 88)
        {
            Camera.main.transform.localRotation = Quaternion.Euler(oldRotation);
        }

        var yaw = MOUSE_SPEED * Input.GetAxis("Mouse X");
        Camera.main.transform.Rotate(Vector3.up, yaw);
        var clampX = Camera.main.transform.localRotation.eulerAngles.x;
        Camera.main.transform.localRotation = Quaternion.Euler(Camera.main.transform.localRotation.eulerAngles.x, Camera.main.transform.localRotation.eulerAngles.y, 0);
    }

    private void RotateCameraAroundPoint()
    {
        Camera.main.transform.RotateAround(m_HitPoint, Vector3.right, -Input.GetAxis("Mouse Y"));
        Camera.main.transform.RotateAround(m_HitPoint, Vector3.up, Input.GetAxis("Mouse X"));
        Vector3 rotation = Camera.main.transform.rotation.eulerAngles;
        rotation.z = 0;
        Camera.main.transform.rotation = Quaternion.Euler(rotation);
    }

    public override void MouseUp()
    {
    }
}
