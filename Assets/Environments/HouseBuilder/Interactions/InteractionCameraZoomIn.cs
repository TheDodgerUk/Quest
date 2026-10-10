using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraZoomIn : InteractionCameraZoom
{

    public override void Awake()
    {
        m_direction = -1;
    }
}
