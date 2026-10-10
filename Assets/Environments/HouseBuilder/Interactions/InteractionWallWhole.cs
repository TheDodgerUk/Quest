using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;
using static HouseBuilder.HouseBuilderConstants.HouseBuilderFloorPlanJsonData;

namespace HouseBuilder
{
    public class InteractionWallWhole : Interaction
    {

        public HouseBuilderFloorPlanJsonData m_StructureJsonData = new HouseBuilderFloorPlanJsonData();
        public List<InteractionWallPoint> m_WallPointList = new List<InteractionWallPoint>();

        private DrawDots m_DrawDotsFinished;
        private DrawDots m_DrawDotsEditing;
        private DrawDots m_DrawDotsFinishedFade;
        private DrawDots m_DrawDotsEditingFade;
        private DrawPlotColliders m_DrawPlotColliders;
        private readonly Vector3 BOX_COLLIDER = new Vector3(0.01f, 20, 0.01f);
        public int WallIndex { get; set; }

        private GameObject m_Holder;

        [InspectorButton]
        private void PrintWallPoints()
        {
            foreach(var item in m_WallPointList)
            {
                Debug.LogError($" PrintWallPoints   {item.name}");
            }
        }

        public void Initialise(GameObject holder)
        {
            m_Holder = holder;
            base.Awake();
            m_DrawDotsFinished = this.AddComponent<DrawDots>();
            m_DrawDotsFinished.Initialise(HouseBuilderConstants.DOT_TEXTURE, "m_DrawDotsFinished", HouseBuilderConstants.Z_FIGHTING_OFFSET);
            m_DrawDotsEditing = this.AddComponent<DrawDots>();
            m_DrawDotsEditing.Initialise(HouseBuilderConstants.DOT_CURRENT_TEXTURE, "m_DrawDotsEditing", HouseBuilderConstants.Z_FIGHTING_OFFSET_BLUE_DOT);
            
            m_DrawDotsFinishedFade = this.AddComponent<DrawDots>();
            m_DrawDotsFinishedFade.Initialise(HouseBuilderConstants.DOT_TEXTURE_FADE, "m_DrawDotsFinishedFade", HouseBuilderConstants.Z_FIGHTING_OFFSET);
            m_DrawDotsEditingFade = this.AddComponent<DrawDots>();
            m_DrawDotsEditingFade.Initialise(HouseBuilderConstants.DOT_CURRENT_TEXTURE_FADE, "m_DrawDotsEditingFade", HouseBuilderConstants.Z_FIGHTING_OFFSET_BLUE_DOT);

            m_DrawPlotColliders = this.AddComponent<DrawPlotColliders>();
            m_DrawPlotColliders.Initialise();


            m_DrawDotsFinished.transform.SetParent(this.transform);
            m_DrawDotsEditing.transform.SetParent(this.transform);
            m_DrawDotsFinishedFade.transform.SetParent(this.transform);
            m_DrawDotsEditingFade.transform.SetParent(this.transform);
            m_DrawPlotColliders.transform.SetParent(this.transform);
        }

        public override void MouseDown()
        {
            GameObject item = null;
            if (HouseBuilderRoot.Instance != null)
            {
                item = CachedObjects.Instance.InstantiateCachedGameObject(HouseBuilderConstants.WALL_INDICATOR_FLAG);
                item.transform.localScale = (new Vector3(8, 8, 8)); // TODO needs to be figured out 
                var cap = item.GetComponent<CapsuleCollider>();
                cap.radius = HouseBuilderConstants.WALL_POLE_DETECTION_RANGE;
            }
            else
            {
                item = new GameObject("temp");
                item.AddComponent<CapsuleCollider>();
                GameObject flagPart2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flagPart2.transform.SetParent(item.transform);
            }

            // Wallcreation is moving with WASD, cannot trace how this being done 
            // this works, and has no knock on effects
            // but i just dont know why the WallCreation gets moved 

            // this fixs Bug 14677  plot mode movement
            // was item.transform.SetParent(this.transform);
            item.transform.SetParent(m_Holder.transform); 

            if (null != HouseBuilderRoot.Instance)
            {
                if (true == GetMouseAsWorldPointForceZero(false, out Vector3 pos))
                {
                    item.transform.position = pos;
                }
                item.transform.position = HouseBuilderInputFloorPlan.PlaceAtCorrectPoint(item.transform.position, true);
            }

            InteractionWallPoint itemInteraction = item.ForceComponent<InteractionWallPoint>();
            itemInteraction.Initialise(this);

            m_WallPointList.Add(itemInteraction);


            item.name = "Part: " + m_WallPointList.Count;
            if (null != HouseBuilderRoot.Instance)
            {
                if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Position)
                {
                    WallPointMeasurementLabelPanel.Instance.FindThePosOfThePositionLabel(item.transform.position);
                }
            }
            if (m_WallPointList.Count == 1)
            {
                item.name += " And Start";
                
            }

            if (null != PositionGrid.Instance)
            {
                if (m_WallPointList.Count > 0)
                {
                    PositionGrid.Instance.PlaceObjectOnGrid(m_WallPointList.Last().gameObject);
                }
            }
        }


        private void LateUpdate()
        {
            if(Camera.main == null)
            {
                return;
            }
            if (null == HouseBuilderRoot.Instance)
            {
                return;
            }

            if(m_WallPointList.Count == 0)
            {
                return;
            }

            m_DrawDotsFinished.ClearLines();
            m_DrawDotsEditing.ClearLines();
            m_DrawDotsFinishedFade.ClearLines();
            m_DrawDotsEditingFade.ClearLines();

            m_DrawPlotColliders.ClearColliders();

            DrawDots drawDotsRef = m_DrawDotsFinished;
            DrawDots drawDotsFadeRef = m_DrawDotsFinishedFade;

            var wallCreation = HouseBuilderRoot.Instance.InputFloorPlan.GetCurrentInteractionWallWhole();
            bool canBeEditMode = (wallCreation == this) ? true : false;

            if(canBeEditMode == false)
            {
                m_StructureJsonData.m_DotTypeEnum = DotTypeEnum.Finished;
            }
            else
            {
                m_StructureJsonData.m_DotTypeEnum = DotTypeEnum.Editing;
            }

            if (m_StructureJsonData.m_DotTypeEnum == DotTypeEnum.Editing)
            {
                drawDotsRef = m_DrawDotsEditing;
                drawDotsFadeRef = m_DrawDotsEditingFade;
            }

            m_StructureJsonData.m_RoomData.m_RoomWalls.Resize(m_WallPointList.Count);
            for(int i = 0; i < m_StructureJsonData.m_RoomData.m_RoomWalls.Count; i++)
            {
                if(m_StructureJsonData.m_RoomData.m_RoomWalls[i] == null)
                {
                    m_StructureJsonData.m_RoomData.m_RoomWalls[i] = new WallData();
                }
            }

            for (int i = 0; i < m_WallPointList.Count - 1; i++)
            {              
                if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Position)
                {
                    WallPointMeasurementLabelPanel.Instance.FindThePosOfThePositionLabel(m_WallPointList[i].transform.position);
                }
                else if(WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Distance)
                {
                    WallPointMeasurementLabelPanel.Instance.CalculateMeasurementLabelPosition(m_WallPointList[i].transform.position, m_WallPointList[i + 1].transform.position);
                }

                if (m_StructureJsonData.m_RoomData.m_RoomWalls[i].m_RenderThisWall == true)
                {
                    drawDotsRef.DrawLines(m_WallPointList[i].gameObject, m_WallPointList[i + 1].gameObject);
                }
                else
                {
                    drawDotsFadeRef.DrawLines(m_WallPointList[i].gameObject, m_WallPointList[i + 1].gameObject);
                }

                m_DrawPlotColliders.DrawCollider(WallIndex, i, m_WallPointList[i].transform.position, m_WallPointList[i + 1].gameObject.transform.position);
                ItemWhichIsBetween(m_WallPointList[i].gameObject, m_WallPointList[i + 1].gameObject);
            }


            

            if (m_StructureJsonData.m_IsWallLooped)
            {
                if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Position)
                {
                    WallPointMeasurementLabelPanel.Instance.FindThePosOfThePositionLabel(m_WallPointList.Last().transform.position);
                }
                else if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Distance)
                {
                    WallPointMeasurementLabelPanel.Instance.CalculateMeasurementLabelPosition(m_WallPointList.Last().transform.position, m_WallPointList.First().transform.position);
                }

                if(m_StructureJsonData.m_RoomData.m_RoomWalls.Last().m_RenderThisWall == true)
                {
                    drawDotsRef.DrawLines(m_WallPointList.Last().gameObject, m_WallPointList.First().gameObject);
                }
                else
                {
                    drawDotsFadeRef.DrawLines(m_WallPointList.Last().gameObject, m_WallPointList.First().gameObject);
                }
                
                m_DrawPlotColliders.DrawCollider(WallIndex, m_WallPointList.Count-1, m_WallPointList.Last().transform.position, m_WallPointList.First().gameObject.transform.position);
                ItemWhichIsBetween(m_WallPointList.Last().gameObject, m_WallPointList.First().gameObject);
            }


            if (m_StructureJsonData.m_DotTypeEnum == DotTypeEnum.Editing && m_StructureJsonData.m_IsWallLooped == false && HouseBuilderRoot.Instance.InputFloorPlan.CurrentInputMode == HouseBuilderInput.InputMode.Creating)
            {
                if (m_WallPointList.Count - 1 >= 0)
                {
                    GetMouseAsWorldPointForceZero(false, out Vector3 mouseAsWorldPoint);

                    // modify mouse position 
                    PositionGrid.Instance.PlaceObjectOnGrid(ref mouseAsWorldPoint);
                    mouseAsWorldPoint = HouseBuilderInputFloorPlan.PlaceAtCorrectPoint(mouseAsWorldPoint, true);

                    var pointPosition = m_WallPointList.Last().transform.position;
                    drawDotsRef.DrawLines(m_WallPointList.Last().gameObject, mouseAsWorldPoint);
                    ItemWhichIsBetween(m_WallPointList.Last().gameObject, mouseAsWorldPoint);


                    if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Position)
                    {
                        WallPointMeasurementLabelPanel.Instance.FindThePosOfThePositionLabel(mouseAsWorldPoint);
                    }
                    else if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Distance)
                    {
                        WallPointMeasurementLabelPanel.Instance.CalculateMeasurementLabelPosition(mouseAsWorldPoint, pointPosition);
                    }
                }
            }

            if (WallPointMeasurementLabelPanel.measureTypeEnum == WallPointMeasurementLabelPanel.MeasureType.Position)
            {
                WallPointMeasurementLabelPanel.Instance.FindThePosOfThePositionLabel(m_WallPointList.Last().transform.position);
            }
        }

        private void ItemWhichIsBetween(GameObject point1, GameObject point2) => ItemWhichIsBetween(point1.transform.position, point2.transform.position);
        private void ItemWhichIsBetween(GameObject point1, Vector3 point2) => ItemWhichIsBetween(point1.transform.position, point2);

        private void ItemWhichIsBetween(Vector3 point1, Vector3 point2)
        {
            var hitItems = Physics.BoxCastAll(point1, BOX_COLLIDER, point2 - point1, Quaternion.identity, Vector3.Distance(point1, point2));
            foreach(var hitItem in hitItems)
            {
                var item = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.Find(e => e.InteractionRef.MainCollider == hitItem.collider);
                if(item != null && item.InteractionRef.PlacementMetaData.Placement.IsWall() == false)
                {
                    item.InteractionRef.EnableRenderer(true);
                }
            }
        }

        public override void MouseUp() { }

        private void OnDestroy()
        {
            for(int i = 0; i < m_WallPointList.Count; i++)
            {
                Destroy(m_WallPointList[i].gameObject);
            }
        }
    }
}
