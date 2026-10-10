using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class InteractionWallGeneratedPoint : Interaction
    {
        [SerializeField]
        private int m_ThisWallPointIndex;
        public int ThisWallPointIndex => m_ThisWallPointIndex;
        public InteractionWallGeneratedWhole GeneratedWholeRef;
        public InteractionWallPoint WallPointRef;

        public Vector3 Position => WallPointRef.WallWholeRef.m_WallPointList[ThisWallPointIndex].transform.position;

        public Vector3 NextPosition => WallPointRef.WallWholeRef.m_WallPointList[InternalNextIndex()].transform.position;
        public Vector3 PrevPosition => WallPointRef.WallWholeRef.m_WallPointList[InternalPrevIndex()].transform.position;
        public int NextIndex => InternalNextIndex();
        public int PrevIndex => InternalPrevIndex();

        public void SetWallPointIndex(int index) => m_ThisWallPointIndex = index;

        private int InternalNextIndex()
        {
            int next = m_ThisWallPointIndex;
            next = next.WrapIncrement(WallPointRef.WallWholeRef.m_WallPointList);
            return next;
        }

        private int InternalPrevIndex()
        {
            int next = m_ThisWallPointIndex;
            next = next.WrapDecrement(WallPointRef.WallWholeRef.m_WallPointList);
            return next;
        }

        public override void Awake()
        {
            base.Awake();
            m_Outline = gameObject.ForceComponent<EPOOutline.Outlinable>();
            m_Outline.Initialise();
        }

        public override void MouseDown()
        {
            Debug.Log("MouseDown");
        }


        public override void MouseUp()
        {
            Debug.Log("MouseUp");
        }


        public override void SelectObject()
        {
            OutlineOnlyThisItem();

            HouseBuilderRoot.Instance.OnNewWallSelected(m_ThisWallPointIndex, GeneratedWholeRef);
        }

        public override void UnSelectObject()
        {
            HouseBuilderRoot.Instance.OnWallDeselected(GeneratedWholeRef);

            foreach (var item in GeneratedWholeRef.m_WallGeneratedPointList)
            {
                item.m_Outline.SetOutLine(false);
                item.m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Selected;
            }
        }

        protected override void StoreOriginalShadowCastingModes()
        {
            if (m_Outline != null)
            {
                if ((m_OriginalShadowCastingModes == null) || (m_OriginalShadowCastingModes.Count == 0))
                {
                    m_OriginalShadowCastingModes = new List<UnityEngine.Rendering.ShadowCastingMode>();
                    foreach (var item in m_Outline.RenderersRef)
                    {
                        m_OriginalShadowCastingModes.Add(UnityEngine.Rendering.ShadowCastingMode.TwoSided); // force to double sided 
                    }
                }
            }
        }

        private void OutlineOnlyThisItem()
        {
            foreach (var item in GeneratedWholeRef.m_WallGeneratedPointList)
            {
                item.m_Outline.SetOutLine(true);
                if (this != item)
                {
                    item.m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Selected;
                }
                else
                {
                    item.m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Valid;
                }
            }
        }
    }
}
