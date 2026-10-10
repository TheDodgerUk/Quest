using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraZoom : Interaction
{
    protected float m_direction;

    public override void MouseDown()
    {
        if (Camera.main.orthographic == false)
        {
            Camera.main.transform.position += (Camera.main.transform.forward * -m_direction);
        }
        else
        {
            Camera.main.orthographicSize += m_direction * Time.deltaTime * 20;
        }
    }
}