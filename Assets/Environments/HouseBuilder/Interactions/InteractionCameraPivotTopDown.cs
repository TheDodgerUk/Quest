using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraPivotTopDown : Interaction
{
    private const float MOUSE_SPEED = 1f;
    private Vector3 m_HitPoint = new Vector3();

    public override void MouseDown()
    {
    }

    public override void MouseDrag()
    {    
        if (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() == null || false == (HouseBuilder.HouseBuilderRoot.Instance.InputContents.GetLastInteraction() is InteractionGameObjectRotate))
        {
            var yaw = MOUSE_SPEED * -Input.GetAxis("Mouse X");
            var clampY = Camera.main.transform.localRotation.eulerAngles.y + yaw;
            Camera.main.transform.localRotation = Quaternion.Euler(90, clampY, 0);
        }
    }

    public override void MouseUp()
    {
    }
}
