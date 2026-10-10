using HouseBuilder;
using HouseViewer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionGameObjectRotate : InteractionGameObject
{
    private Quaternion m_LastValidRotation;
    private const float MOUSE_SPEED = 4f;
    private const float STEP_SIZE = 5f;
    float m_Yaw = 0;
    public override void MouseDown()
    {
        m_LastValidRotation = this.gameObject.transform.rotation;
    }

    public override void MouseDrag()
    {
        m_Yaw += MOUSE_SPEED * Input.GetAxis("Mouse X");
        if(Mathf.Abs(m_Yaw) > STEP_SIZE)
        {
            if(m_Yaw > 0)
            {
                m_Yaw = STEP_SIZE;
            }
            else
            {
                m_Yaw = -STEP_SIZE;
            }
            this.transform.Rotate(Vector3.up, m_Yaw);
            m_Yaw = 0;
            if (this.PlacementMetaData.Placement == ContentPlacementMetaData.EnumPlacement.Ceiling)
            {
                if (Camera.main.transform.forward.y < 0)
                {
                    RotationCheck(Vector3.up, Vector3.down);
                }
                else
                {
                    RotationCheck(Vector3.down, Vector3.up);
                }
            }
            else
            {
                RotationCheck(Vector3.up, Vector3.down);
            }
        }

    }


    public override void KeyboardRotate(float amount)
    {
        this.transform.Rotate(Vector3.up, amount);
        if (this.PlacementMetaData.Placement == ContentPlacementMetaData.EnumPlacement.Ceiling)
        {
            if (Camera.main.transform.forward.y < 0)
            {
                RotationCheck(Vector3.down, Vector3.down);
            }
            else
            {
                RotationCheck(Vector3.up, Vector3.up);
            }
        }
        else
        {
            RotationCheck(Vector3.up, Vector3.down);
        }
    }

    public override void MouseUp()
    {
        if (m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
        {
            this.gameObject.transform.rotation = m_LastValidRotation;
            return;
        }

        m_LastValidRotation = transform.rotation;
        HouseBuilderRoot.Instance.RecorderRef.Record();
    }

 
}
