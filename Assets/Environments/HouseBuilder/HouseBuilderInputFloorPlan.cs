using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class HouseBuilderInputFloorPlan : HouseBuilderInput
    {
        protected override string ClassName => nameof(HouseBuilderInputFloorPlan);

        protected override float MANIPULATE_HELD_TIME => 0.1f;
        public ModeContainer<InteractionWallWhole> ModeContainerRef { get; set; }
        private GameObject m_Flag;
        private GameObject m_Target;
        private List<Interaction> m_SelectedPoles = new List<Interaction>();
        private int m_PoleSelectionIndex = 0;

        public class MovingWallData
        {
            public InteractionWallPoint m_FirstPointGameObject;
            public Vector3 m_FirstPointStart;
            public Vector3 m_FirstPointDirection;

            public InteractionWallPoint m_SecondPointGameObject;
            public Vector3 m_SecondPointStart;
            public Vector3 m_SecondPointDirection;

            public Vector3 m_PlanePoint;
            public Vector3 m_PlaneNormal;
        }

        public MovingWallData m_MovingWallData = new MovingWallData();



        [InspectorButton]
        private void PrintGetCurrentInteractionWallPoint() => Debug.LogError($" {GetCurrentInteractionWallPoint().name}");

        public void ShowTarget(bool enable)
        {
            if (m_Target != null)
            {
                m_Target.SetActive(enable);
            }
        }

        public override void Initialise()
        {
            ModeContainerRef = new ModeContainer<InteractionWallWhole>("FloorPlan", this.gameObject);
            if (HouseBuilderRoot.Instance != null)
            {
                m_Flag = CachedObjects.Instance.InstantiateCachedGameObject(HouseBuilderConstants.WALL_INDICATOR_FLAG);
                if (null != m_Flag)
                {
                    m_Flag.SetActive(false);
                }

                CreateTarget((item) =>
                {
                    m_Target = item;
                    if (m_Target != null)
                    {
                        m_Target.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    }
                    else
                    {
                        Debug.LogError($"m_Target is null");
                    }
                });
            }
        }

        public override void CleanUp()
        {
            InteractionWallWhole zero = null;
            for (int i = 0; i < ModeContainerRef.GetList.Count; i++)
            {
                if (ModeContainerRef.GetList[i].m_WallPointList.Count == 0)
                {
                    zero = ModeContainerRef.GetList[i];
                }
            }
            if (zero != null)
            {
                ModeContainerRef.GetList.Remove(zero);
                Destroy(zero.gameObject);
            }
        }

        private void OnRemoveWall()
        {
            if (CurrentInputMode == InputMode.Creating)
            {
                if (GetLastInteraction().GetType() == typeof(InteractionWallWhole))
                {
                    var item = (InteractionWallWhole)GetLastInteraction();
                    if (item != null)
                    {
                        item.m_WallPointList.Last().DeleteThisWall();
                        CurrentInputMode = InputMode.Nothing;
                    }
                }
            }
        }

        private void CreateTarget(Action<GameObject> callback)
        {
            GameObject item = null;
            if (HouseBuilderRoot.Instance != null)
            {
                item = CachedObjects.Instance.InstantiateCachedGameObject(HouseBuilderConstants.TARGET_TEXTURE);
            }
            else
            {
                item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
            callback?.Invoke(item);
        }


        protected override void UpdateInputControlsFor_Nothing()
        {

            ClearAllInteractionsNoMouseUp();
            foreach (var item in m_SelectedPoles)
            {
                item.UnSelectObject();
            }
            m_SelectedPoles.Clear();

            m_MainRay = Camera.main.GameObjectToRay(m_Target);

            if (Physics.Raycast(m_MainRay, out m_MainRayCastHit))
            {
                var pole = GetSelectedWallPoint(m_MainRayCastHit.collider.gameObject);
                if (pole != null)
                {
                    pole.SelectObject();
                    m_SelectedPoles.Add(pole);
                }



                if (HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Tap))
                {
                    Ray mouseDown = PositionGrid.Instance.MouseToRayDown();
                    var hits = Physics.RaycastAll(mouseDown).ToList();

                    var plotWallCollider = GetSelectedPlotWallCollider(hits, out RaycastHit mouseDownHit);
                    if ((plotWallCollider != null) && (m_SelectedPoles.Count == 0))
                    {
                        ClearAllInteractionsNoMouseUp();
                        var wall = ModeContainerRef.GetList[plotWallCollider.WallIndex];
                        SetCurrentInteractionNoCallBack(wall.m_WallPointList[plotWallCollider.WallPartIndex]);
                        CurrentInputMode = InputMode.Add;
                        HouseBuilderRoot.Instance.OnNewDashesBetweenPolesSelected(GetCurrentInteractionWallPoint(), GetCurrentInteractionWallWhole(), mouseDownHit.point);
                        return;
                    }

                    m_MouseHeldTime += Time.deltaTime;

                    SetCurrentInteraction(pole);
                    if (null != GetLastInteraction())
                    {
                        CurrentInputMode = HouseBuilderInput.InputMode.FigureOutCreatingOrManipulating;
                    }
                    else
                    {
                        InteractionFloorPlan interactionFloor = m_MainRayCastHit.collider.gameObject.GetComponentInParent<InteractionFloorPlan>();
                        if (interactionFloor == null)
                        {
                            var allHit = Physics.RaycastAll(m_MainRay);
                            foreach (var floor in allHit)
                            {
                                interactionFloor = m_MainRayCastHit.collider.gameObject.GetComponentInParent<InteractionFloorPlan>();
                                if(interactionFloor != null)
                                {
                                    break;
                                }
                            }
                        }

                        if (interactionFloor != null)
                        {
                            ClearAllInteractions();
                            var interaction = CreateNewWall();
                            SetCurrentInteraction(interaction);

                            GetLastInteraction().SelectObject(); // does nothing
                            CurrentInputMode = InputMode.Creating;

                            HouseBuilderRoot.Instance.RecorderRef.Record();
                            HBGUIManager.Instance.DisableAllHeaderTogglesMenu();
                        }
                    }
                }
                else
                {
                    m_MouseHeldTime = 0;
                }
            }
        }



        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {

            if (true == HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Delete))
            {
                OnDeleteCallback();
            }

            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                m_MouseHeldTime += Time.deltaTime;
                if (IsDragged())
                {
                    GetLastInteraction().SelectObject(); // does nothing
                    CurrentInputMode = InputMode.Manipulating;
                    HouseBuilderRoot.Instance.OnPoleDeselected();
                    return;
                }
                else
                {
                    Ray mouseDown = PositionGrid.Instance.MouseToRayDown();
                    var allHit = Physics.RaycastAll(mouseDown, 100).ToList();

                    if (m_MouseHeldTime == Time.deltaTime) // only the first press of the mouse
                    {
                        InteractionWallPoint pole = null;
                        for (int i = 0; i < allHit.Count; i++)
                        {
                            m_PoleSelectionIndex = m_PoleSelectionIndex.WrapIncrement(allHit);

                            pole = (InteractionWallPoint)GetSelectedWallPoint(allHit[m_PoleSelectionIndex].collider.gameObject);
                            if (pole != null)
                            {
                                pole.SelectObject();
                                m_SelectedPoles.Add(pole);
                                break;
                            }
                        }

                        if (pole == null)
                        {
                            CurrentInputMode = InputMode.Nothing;
                            HouseBuilderRoot.Instance.OnPoleDeselected();
                            return;
                        }
                        else
                        {
                            SetCurrentInteraction(pole);
                            HouseBuilderRoot.Instance.OnNewPoleSelected(pole, GetCurrentInteractionWallWhole());
                        }
                    }
                }
            }
            else
            {

                m_MouseHeldTime = 0;
            }
        }


        public void OnEditCallback(InteractionWallPoint currentPoint, InteractionWallWhole currentWall)
        {
            var item = currentPoint;
            var firstItem = item.WallWholeRef.m_WallPointList.First();
            if (item == firstItem)
            {
                SetCurrentInteractionNoCallBack(item.WallWholeRef);
                ReorderWallItemsReverse(item.WallWholeRef);
                SetCurrentInteractionNoCallBack(item.WallWholeRef);
            }

            var wall = currentWall;
            ClearAllInteractionsNoMouseUp();
            HouseBuilderRoot.Instance.OnPoleDeselected();
            SetCurrentInteractionNoCallBack(wall);
            HouseBuilderRoot.Instance.RecorderRef.Record();
            CurrentInputMode = InputMode.Creating;
        }

        public void OnDeleteCallback()
        {
            var item = GetCurrentInteractionWallPoint();

            item.DeleteThisNode();
            if (item.WallWholeRef.m_WallPointList.Count == 0)
            {
                item.DeleteThisWall();
            }

            HouseBuilderRoot.Instance.RecorderRef.Record();
            CurrentInputMode = InputMode.Nothing;
            HouseBuilderRoot.Instance.OnPoleDeselected();
        }

        public void OnSplitCallback(int index, Vector3 halfway)
        {
            InteractionFloorPlan.CreateNewPartOffWall(GetCurrentInteractionWallWhole());
            // move item
            var justadded = GetCurrentInteractionWallWhole().m_WallPointList.Last();
            GetCurrentInteractionWallWhole().m_WallPointList.Move(GetCurrentInteractionWallWhole().m_WallPointList.Count - 1, index);
            GetCurrentInteractionWallWhole().m_WallPointList[index].transform.position = halfway;

            HouseBuilderRoot.Instance.RecorderRef.Record();
            CurrentInputMode = InputMode.Nothing;
            HouseBuilderRoot.Instance.OnPoleDeselected();
        }

        public void OnHideCallback(int index)
        {
            GetCurrentInteractionWallWhole().m_StructureJsonData.m_RoomData.m_RoomWalls[index].m_RenderThisWall.Toggle();
            HouseBuilderRoot.Instance.RecorderRef.Record();
            CurrentInputMode = InputMode.Nothing;
            HouseBuilderRoot.Instance.OnPoleDeselected();
        }

        public bool GetHideState(int index)
        {
            var wallItem = GetCurrentInteractionWallWhole();
            if (wallItem != null)
            {
                if (index <= wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls.LastIndex())
                {
                    return wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls[index].m_RenderThisWall;
                }
            }
            Debug.LogError($"Not found   {index}");
            return false;
        }


        public void OnBreakCallback(InteractionWallPoint topOfList)
        {
            ReorderWallItemsNewTopOfList(topOfList);
            topOfList.WallWholeRef.m_StructureJsonData.m_IsWallLooped = false;
            CurrentInputMode = InputMode.Nothing;
            HouseBuilderRoot.Instance.OnPoleDeselected();
        }

        public void OnAddNewRoomStartCallback(Vector3? position = null)
        {
            ClearAllInteractions();
            HouseBuilderRoot.Instance.OnPoleDeselected();
            var interaction = CreateNewWall();
            SetCurrentInteraction(interaction);
            if (position != null)
            {
                var wall = interaction;
                wall.m_WallPointList[0].transform.position = (Vector3)position;
            }

            GetLastInteraction().SelectObject(); // does nothing
            CurrentInputMode = InputMode.Creating;

            HouseBuilderRoot.Instance.RecorderRef.Record();
            HBGUIManager.Instance.DisableAllHeaderTogglesMenu();
        }


        private void ReorderWallItemsNewTopOfList(InteractionWallPoint topOfListWallPoint)
        {
            int wallPointIndex = topOfListWallPoint.WallWholeRef.m_WallPointList.FindIndex(e => e == topOfListWallPoint);

            #region Debugging 
            //for debugging
            //Debug.LogError($"currentPoint Before reorder {topOfList.name}   index is {itemIndex}");
            //for (int i = 0; i < topOfList.WallWholeRef.m_WallPointList.Count; i++)
            //{
            //    var hide = GetCurrentInteractionWallWhole().m_StructureJsonData.m_RoomData.m_RoomWalls[i].m_RenderThisWall;
            //    Debug.LogError($"Reorder before {i}  {topOfList.WallWholeRef.m_WallPointList[i].name}  hide State {hide}");
            //}
            #endregion

            // split up, coollect all items attached to this wall 
            var allWallItems = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.FindAll(e => e.InteractionRef.PlacementMetaData.Placement.IsWall());
            var allThisWallItems = allWallItems.FindAll(e => e.m_WallObjectLinkerData.m_WallIndex == topOfListWallPoint.WallWholeRef.WallIndex);


            var json = topOfListWallPoint.WallWholeRef.m_StructureJsonData.m_RoomData;
            List<HouseBuilderConstants.WallData> newjson = new List<HouseBuilderConstants.WallData>();
            List<InteractionWallPoint> newList = new List<InteractionWallPoint>();
            for (int i = 0; i < topOfListWallPoint.WallWholeRef.m_WallPointList.Count; i++)
            {
                int oldIndex = wallPointIndex + i + 1;
                oldIndex = oldIndex.Wrap(topOfListWallPoint.WallWholeRef.m_WallPointList);

                // create new data order
                newList.Add(topOfListWallPoint.WallWholeRef.m_WallPointList[oldIndex]);
                newjson.Add(json.m_RoomWalls[oldIndex]);

                // loop and change al items attached to the wall 
                for (int itemIndex = 0; itemIndex < allThisWallItems.Count; itemIndex++)
                {
                    var item = allThisWallItems[itemIndex];
                    if (item.m_WallObjectLinkerData.m_WallPointIndex == oldIndex)
                    {
                        item.m_WallObjectLinkerData.m_WallPointIndex = i;
                        allThisWallItems.Remove(item); // this is so i not change it if the index comes up again 
                    }
                }

            }

            //assign list 
            topOfListWallPoint.WallWholeRef.m_WallPointList = newList;
            json.m_RoomWalls = newjson;

            #region Debugging 
            // for debugging 
            //for (int i = 0; i < topOfList.WallWholeRef.m_WallPointList.Count; i++)
            //{
            //    var hide = GetCurrentInteractionWallWhole().m_StructureJsonData.m_RoomData.m_RoomWalls[i].m_RenderThisWall;
            //    Debug.LogError($"Reorder after {i}  {topOfList.WallWholeRef.m_WallPointList[i].name}  var hide state {hide}");
            //}
            #endregion
        }


        private void ReorderWallItemsReverse(InteractionWallWhole whole)
        {
            Debug.LogError("ReorderWallItemsReserve,  this is were stuff will go wrong , hiding , wall connected items ect "); // log error, because need to find this, as this is were all stuff will break down i figure 
            whole.m_WallPointList.Reverse();
            whole.m_StructureJsonData.m_RoomData.m_RoomWalls.Reverse();


            foreach (var item in HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList)
            {
                if (item.m_WallObjectLinkerData != null)
                {
                    if (item.m_WallObjectLinkerData.m_WallIndex == whole.WallIndex)
                    {
                        item.m_WallObjectLinkerData.m_WallPointIndex = whole.m_WallPointList.Count - item.m_WallObjectLinkerData.m_WallPointIndex;
                    }
                }
            }
        }


        protected override void UpdateInputControlsFor_Creating()
        {
            m_MainRay = Camera.main.GameObjectToRay(m_Target);
            if (Physics.Raycast(m_MainRay, out m_MainRayCastHit))
            {
                if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Tap))
                {
                    if (true == DidClickOnLastPlace())
                    {
                        return;
                    }

                    // need to check if its close to another wall though 
                    var interaction = GetCurrentInteractionWallWhole();
                    if (null != interaction) // so its still valid creatWalls
                    {
                        InteractionFloorPlan.CreateNewPartOffWall(interaction);
                    }
                    HouseBuilderRoot.Instance.RecorderRef.Record();
                }
                else
                {
                    if (null != GetLastInteraction())
                    {
                        var last = GetLastInteraction();
                        last.MouseDrag();
                    }
                }
            }


            // remove last nodes
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Right, PressType.Hold))
            {
                m_MouseHeldTime += Time.deltaTime;
            }
            else
            {
                //left mouse button press
                if (m_MouseHeldTime > 0)
                {
                    if (GetLastInteraction().GetType() == typeof(InteractionWallWhole))
                    {
                        var item = (InteractionWallWhole)GetLastInteraction();
                        if (item.m_WallPointList.Count > 1)
                        {
                            item.m_WallPointList.Last().DeleteThisNode();
                        }
                        else
                        {
                            item.m_WallPointList.Last().DeleteThisWall();
                            CurrentInputMode = InputMode.Nothing;
                        }
                        HouseBuilderRoot.Instance.OnPoleDeselected();
                        HouseBuilderRoot.Instance.RecorderRef.Record();
                    }
                }
                m_MouseHeldTime = 0;
            }
        }


        protected override void UpdateInputControlsFor_EarlyUpdate()
        {
            Vector3 pos;
            if (true == Interaction.MousePointOnFloor(out pos))
            {
                PositionGrid.Instance.PlaceObjectOnGrid(ref pos);
                m_Target.transform.position = PlaceAtCorrectPoint(pos, true);
                m_Target.transform.position += HouseBuilderConstants.Z_FIGHTING_OFFSET_VECTOR3;
            }
        }

        public static Vector3 PlaceAtCorrectPoint(Vector3 pos, bool useWallPositions)
        {
            float detectionRange = HouseBuilderConstants.WALL_POLE_DETECTION_RANGE;
            if (PositionGrid.Instance.GetGridInfo().m_ShowGrid)
            {
                detectionRange = PositionGrid.Instance.GetGridInfo().m_Spacing / 2f;
            }
            if (true == useWallPositions)
            {
                for (int i = 0; i < HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList.Count; i++)
                {
                    for (int j = 0; j < HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[i].m_WallPointList.Count; j++)
                    {
                        var wallPosition = HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[i].m_WallPointList[j].transform.position;
                        if (Vector3.Distance(wallPosition, pos) < detectionRange)
                        {
                            return wallPosition;
                        }
                    }
                }
            }

            for (int i = 0; i < HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList.Count; i++)
            {
                int index = 0;
                Vector3 point = FindClosePointToWall(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[i], ref index);
                if (Vector3.Distance(point, pos) < detectionRange)
                {
                    return point;
                }
            }
            return pos;
        }

        protected override void UpdateInputControlsFor_Manipulating()
        {
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction())
                {
                    GetCurrentInteractionWallPoint().MouseDrag();
                    Vector3 pos;
                    if (true == Interaction.MousePointOnFloor(out pos))
                    {
                        PositionGrid.Instance.PlaceObjectOnGrid(ref pos);
                        pos.y = 0;
                        GetCurrentInteractionWallPoint().gameObject.transform.position = pos;
                    }
                }
            }
            else
            {
                GetCurrentInteractionWallPoint().SelectObject();
                if (null != HouseBuilder.PositionGrid.Instance)
                {
                    HouseBuilder.PositionGrid.Instance.PlaceObjectOnGrid(GetCurrentInteractionWallPoint().gameObject);
                }
                HouseBuilderRoot.Instance.RecorderRef.Record();

                CurrentInputMode = InputMode.Nothing;
            }
        }

        protected override void UpdateInputControlsFor_Add()
        {
            m_MouseHeldTime += Time.deltaTime;
            if (HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) == true)
            {
                Ray mouseDown = PositionGrid.Instance.MouseToRayDown();
                var allHits = Physics.RaycastAll(mouseDown).ToList();
                var plotWallCollider = GetSelectedPlotWallCollider(allHits, out RaycastHit mouseDownHit);
                if (IsDragged())
                {
                    if (plotWallCollider != null)
                    {
                        CurrentInputMode = InputMode.ManipulatingOther;
                        HouseBuilderRoot.Instance.OnPoleDeselected();
                        GatherMovingWallData();
                    }
                    else
                    {
                        CurrentInputMode = InputMode.Nothing;
                        HouseBuilderRoot.Instance.OnPoleDeselected();
                    }
                }
                else
                {
                    if (m_MouseHeldTime == Time.deltaTime)
                    {
                        if(plotWallCollider == null)
                        { 
                            CurrentInputMode = InputMode.Nothing;
                            HouseBuilderRoot.Instance.OnPoleDeselected();
                        }
                        else
                        {

                            var currentWall = GetCurrentInteractionWallWhole();
                            var currentWallIndex = currentWall.WallIndex;
                            var point = GetCurrentInteractionWallPoint();
                            var currentPointIndex = currentWall.m_WallPointList.FindIndex(e => e == point);

                            var allMouseHit = Physics.RaycastAll(mouseDown).ToList();
                            for (int i = 0; i < allMouseHit.Count; i++)
                            {
                                var loopWallCollider = GetSelectedPlotWallCollider(allMouseHit[i].collider.gameObject);
                                if (loopWallCollider != null)
                                {
                                    if (currentWallIndex != loopWallCollider.WallIndex || currentPointIndex != loopWallCollider.WallPartIndex)
                                    {
                                        ClearAllInteractionsNoMouseUp();
                                        var wall = ModeContainerRef.GetList[loopWallCollider.WallIndex];
                                        SetCurrentInteractionNoCallBack(wall.m_WallPointList[loopWallCollider.WallPartIndex]);
                                        HouseBuilderRoot.Instance.OnNewDashesBetweenPolesSelected(GetCurrentInteractionWallPoint(), GetCurrentInteractionWallWhole(), mouseDownHit.point);
                                    }
                                }
                            }

                        }
                    }
                }
            }
            else
            {
                m_MouseHeldTime = 0;
            }
        }

        private void GatherMovingWallData()
        {
            var wallList = GetCurrentInteractionWallWhole().m_WallPointList;
            int indexThis = wallList.FindIndex(e => e == GetCurrentInteractionWallPoint());
            int nextIndex0 = indexThis.WrapIncrement(wallList);
            Debug.LogError("GatherData");

            m_MovingWallData = new MovingWallData();
            m_MovingWallData.m_FirstPointGameObject = wallList[indexThis];
            m_MovingWallData.m_FirstPointStart = wallList[indexThis].transform.position;

            m_MovingWallData.m_SecondPointGameObject = wallList[nextIndex0];
            m_MovingWallData.m_SecondPointStart = wallList[nextIndex0].transform.position;

            m_MovingWallData.m_PlaneNormal = wallList[indexThis].transform.position - wallList[nextIndex0].transform.position;
            m_MovingWallData.m_PlaneNormal = Quaternion.Euler(0, 90, 0) * m_MovingWallData.m_PlaneNormal;
            m_MovingWallData.m_PlaneNormal.Normalize();

            m_MovingWallData.m_FirstPointDirection = m_MovingWallData.m_PlaneNormal;
            m_MovingWallData.m_SecondPointDirection = m_MovingWallData.m_PlaneNormal;
        }


        // this moves the points which moves the InteractionGameObjectPlotWallCollider
        protected override void UpdateInputControlsFor_ManipulatingOther()
        {
            Ray mouseDown = PositionGrid.Instance.MouseToRayDown();
            Vector3 hitPoint = Vector3.zero;
            if (InputUtility.LinePlaneIntersectionDualDirection(out hitPoint, m_MovingWallData.m_FirstPointStart, m_MovingWallData.m_FirstPointDirection, m_MovingWallData.m_PlaneNormal, mouseDown.origin))
            {
                m_MovingWallData.m_FirstPointGameObject.transform.position = hitPoint;
            }
            else if (InputUtility.LinePlaneIntersectionDualDirection(out hitPoint, m_MovingWallData.m_FirstPointStart, -m_MovingWallData.m_FirstPointDirection, m_MovingWallData.m_PlaneNormal, mouseDown.origin))
            {
                m_MovingWallData.m_FirstPointGameObject.transform.position = hitPoint;
            }


            if (InputUtility.LinePlaneIntersectionDualDirection(out hitPoint, m_MovingWallData.m_SecondPointStart, m_MovingWallData.m_SecondPointDirection, m_MovingWallData.m_PlaneNormal, mouseDown.origin))
            {
                m_MovingWallData.m_SecondPointGameObject.transform.position = hitPoint;
            }
            else if (InputUtility.LinePlaneIntersectionDualDirection(out hitPoint, m_MovingWallData.m_SecondPointStart, -m_MovingWallData.m_SecondPointDirection, m_MovingWallData.m_PlaneNormal, mouseDown.origin))
            {
                m_MovingWallData.m_SecondPointGameObject.transform.position = hitPoint;
            }


            if (HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) == false)
            {
                CurrentInputMode = InputMode.Nothing;
                HouseBuilderRoot.Instance.OnPoleDeselected();
            }
        }


        private void MovePoints(InteractionWallWhole wall, Vector3 position, int correctIndex)
        {
            var last = wall.m_WallPointList.Last();
            last.transform.position = position;

            wall.m_WallPointList.Remove(last);
            wall.m_WallPointList.Insert(correctIndex, last);
        }

        private static Vector3 FindClosePointToWall(InteractionWallWhole current, ref int index)
        {
            Ray mainRay = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
            RaycastHit[] rays = Physics.RaycastAll(mainRay);
            RaycastHit raycastHitRef = new RaycastHit();
            foreach (var item in rays)
            {
                if (null != item.collider.GetComponent<InteractionFloorPlan>())
                {
                    raycastHitRef = item;
                    break;
                }
            }
            return FindClosePointToWall(raycastHitRef, current, ref index);
        }


        private static Vector3 FindClosePointToWall(RaycastHit raycastHitRef, InteractionWallWhole current, ref int index)
        {
            Vector3 MouseImpact = raycastHitRef.point;
            Vector3 point = new Vector3(1000, 10000, 10000);
            float Closedistance = float.MaxValue;
            if (null != current)
            {
                for (int i = 0; i < current.m_WallPointList.Count - 1; i++)
                {
                    var pos = CoreUtils.ClosestPointOnLine(current.m_WallPointList[i].transform.position, current.m_WallPointList[i + 1].transform.position, MouseImpact);
                    float distance = Vector3.Distance(MouseImpact, pos);
                    if (distance < Closedistance)
                    {
                        Closedistance = distance;
                        point = pos;
                        index = i;
                    }
                }
                return point;
            }
            else
            {
                return MouseImpact;
            }
        }


        public InteractionWallWhole GetCurrentInteractionWallWhole()
        {
            if (GetLastInteraction() is InteractionWallPoint)
            {
                InteractionWallPoint currentPoint = (InteractionWallPoint)GetLastInteraction();
                return currentPoint.WallWholeRef;
            }
            else
            {
                return (InteractionWallWhole)GetLastInteraction();
            }
        }

        public InteractionWallPoint GetCurrentInteractionWallPoint()
        {
            if (GetLastInteraction() is InteractionWallPoint)
            {
                return (InteractionWallPoint)GetLastInteraction();
            }
            else
            {
                var wall = (InteractionWallWhole)GetLastInteraction();
                return wall.m_WallPointList.First();
            }
        }


        private bool DidClickOnLastPlace()
        {
            if (GetLastInteraction() == null)
            {
                return false;
            }
            if (GetLastInteraction().GetType() == typeof(InteractionWallWhole))
            {
                InteractionWallWhole current = GetCurrentInteractionWallWhole();
                GameObject lastPoint = current.m_WallPointList.Last().gameObject;
                GameObject firstPoint = current.m_WallPointList.First().gameObject;

                InteractionWallPoint pointClickedOn = m_MainRayCastHit.collider.gameObject.GetComponent<InteractionWallPoint>();

                // if items are lapping
                if (current.m_WallPointList.Contains(pointClickedOn) == false)
                {
                    var allHit = Physics.RaycastAll(m_MainRay, 100);
                    for (int i = 0; i < allHit.Length; i++)
                    {
                        InteractionWallPoint pointClickedOnAll = allHit[i].collider.gameObject.GetComponent<InteractionWallPoint>();
                        if (current.m_WallPointList.Contains(pointClickedOnAll) == true)
                        {
                            m_MainRayCastHit = allHit[i];
                            pointClickedOn = pointClickedOnAll;
                            break;
                        }
                    }
                }

                if (null != pointClickedOn)
                {
                    if (pointClickedOn.gameObject == lastPoint || pointClickedOn.gameObject == firstPoint)
                    {
                        if (GetLastInteraction() != null)
                        {

                            //TO CLOSE THE ROOM WHEN CLICK ON THE FIRST NODE
                            if (pointClickedOn.gameObject == firstPoint)
                            {
                                current.m_StructureJsonData.m_IsWallLooped = true;
                            }
                            else
                            {
                                current.m_StructureJsonData.m_IsWallLooped = false;
                            }

                            GetLastInteraction().MouseUp();
                            SetCurrentInteraction(null);
                        }
                        CurrentInputMode = InputMode.Nothing;
                        HouseBuilderRoot.Instance.OnPoleDeselected();
                        return true;
                    }
                }
            }
            return false;
        }


        private Interaction GetSelectedWallPoint(GameObject gameObjectRef)
        {
            InteractionWallPoint point = gameObjectRef.GetComponent<InteractionWallPoint>();
            return point;
        }

        private InteractionGameObjectPlotWallCollider GetSelectedPlotWallCollider(GameObject gameObjectRef)
        {
            InteractionGameObjectPlotWallCollider point = gameObjectRef.GetComponent<InteractionGameObjectPlotWallCollider>();
            return point;
        }

        private InteractionGameObjectPlotWallCollider GetSelectedPlotWallCollider(List<RaycastHit> list, out RaycastHit hit)
        {
            hit = new RaycastHit();
            foreach (var item in list)
            {
                InteractionGameObjectPlotWallCollider point = item.collider.GetComponent<InteractionGameObjectPlotWallCollider>();
                if (point != null)
                {
                    hit = item;
                    return point;
                }
            }
            return null;
        }

        

        public InteractionWallWhole CreateNewWall()
        {
            InteractionWallWhole newWallCreation = InteractionFloorPlan.BeginNewWall(ModeContainerRef.Holder);

            // keep looping until if finds a unique number, that way not get a couple of differnt walls with same index 
            int index = 0;
            for (; ; )
            {
                var item = ModeContainerRef.GetList.Find(e => e.WallIndex == index);
                if (item == null)
                {
                    ModeContainerRef.AddGameObject(newWallCreation);
                    newWallCreation.WallIndex = index;
                    newWallCreation.m_StructureJsonData.m_WallIndex = newWallCreation.WallIndex;
                    return newWallCreation;
                }
                index++;
            }

            return null;
        }
    }
}
