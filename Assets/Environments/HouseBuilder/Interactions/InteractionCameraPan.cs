using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraPan : Interaction
{
    private const float CAMERA_SPEED = 0.25f;


    public override void MouseDrag()
    {
        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        Vector3 up = Camera.main.transform.up;


        Vector3 right = Camera.main.transform.right;
        Vector3 position = Camera.main.transform.position;

        position += up * (-y * CAMERA_SPEED);
        position += right * (-x * CAMERA_SPEED);


        Camera.main.transform.position = position;
    }
}
