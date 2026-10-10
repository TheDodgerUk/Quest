using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCameraZoomOut : InteractionCameraZoom
{

    public override void Awake()
    {
        m_direction = 1;
    }
}
