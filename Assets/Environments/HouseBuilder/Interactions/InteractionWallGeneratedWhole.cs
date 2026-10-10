using EnvironmentHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class InteractionWallGeneratedWhole : Interaction
    {
        public InteractionWallWhole HostWholeRef;
        public int ThisWallIndex { get; private set; }
        public List<InteractionWallGeneratedPoint> m_WallGeneratedPointList = new List<InteractionWallGeneratedPoint>();
        public HouseBuilderFloorPlanJsonData FloorPlanJsonDataRef => HostWholeRef.m_StructureJsonData;

        public void SetWallIndex(int index) => ThisWallIndex = index;
        public void SetWallHost(InteractionWallWhole host) => HostWholeRef = host;

        public void AddWallPart(InteractionWallGeneratedPoint part)
        {
            ContentPlacementMetaData data = new ContentPlacementMetaData();
            data.Placement = ContentPlacementMetaData.EnumPlacement.NotStackable;
            part.SetPlacement(data);
            part.SetWallPointIndex(m_WallGeneratedPointList.Count);
            m_WallGeneratedPointList.Add(part);
        }

        public override void Awake()
        {
            base.Awake();
        }

        public override void MouseDown()
        {
            Debug.LogError("MouseDown");
        }



        public override void MouseUp()
        {
            Debug.LogError("MouseUp");
        }


        public override void SelectObject()
        {
        }

        public override void UnSelectObject()
        {
        }

        public void WallChangeCallback(HouseBuilderFloorPlanJsonData data)
        {
            //HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[data.m_WallData.m_WallNumber].m_FloorPlanJsonData.m_WallData = data;
        }

    }
}
