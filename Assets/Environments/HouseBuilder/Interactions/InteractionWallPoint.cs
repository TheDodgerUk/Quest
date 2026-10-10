using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class InteractionWallPoint : Interaction
    {
        public InteractionWallWhole WallWholeRef;

        public void Initialise(InteractionWallWhole parent)
        {
            WallWholeRef = parent;
        }

        public override void Awake()
        {
            base.Awake();
            m_Outline = gameObject.ForceComponent<EPOOutline.Outlinable>();
            m_Outline.Initialise();


            // turn of shadows for the flags
            if (this.transform.childCount > 0)
            {
                var flag = this.transform.GetChild(0).gameObject;
                flag.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (flag.transform.childCount > 0)
                {
                    var sphere = flag.transform.GetChild(0).gameObject;
                    sphere.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        public override void MouseDrag()
        {
            if(true == GetMouseAsWorldPointForceZero(false, out Vector3 pos))
            {
                this.gameObject.transform.position = pos;
            }
        }

        public override void MouseUp()
        {
            base.MouseUp();
            HouseBuilderRoot.Instance.RecorderRef.Record();
        }

        public override void SelectObject()
        {
            base.SelectObject();
        }

        public override void UnSelectObject()
        {
            base.UnSelectObject();
        }


        [InspectorButton]
        public void DeleteThisNode()
        {
            HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.Delete(this.gameObject);
            WallWholeRef.m_WallPointList.Remove(this);
            Destroy(this.gameObject);
        }
        [InspectorButton]
        public void DeleteThisWall()
        {
            HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.Delete(WallWholeRef);
            Destroy(WallWholeRef.gameObject);
        }
    }
}
