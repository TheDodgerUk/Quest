using HouseBuilder;
using HouseViewer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionGameObjectMove : InteractionGameObject
{
    private Vector3 m_LastValidPosition;
    private Vector3 m_LastValidPositionDif;
    private GameObject m_InitialHitGameObject;
    public Quaternion m_LastValidRotation { get; private set; }
    private HouseBuilderConstants.WallObjectLinkerData m_WallObjectLinkerData;

    public override void MouseDown()
    {
        m_LastValidPosition = this.transform.position;
        if (HouseBuilderConstants.IsAtCreatedStartPoint(m_LastValidPosition) == true)
        {
            // this means that its off screen until you move it onto screen
            m_LastValidPosition = Vector3.zero;
        }
        m_LastValidRotation = RootTransform.rotation;
        m_LastValidPositionDif = PositionGrid.Instance.PlaceMouseOnGrid() - m_LastValidPosition;
        m_LastValidPositionDif = m_LastValidPosition;
    }

    public override void MouseDrag()
    {
        TurnCutoutHolesOn();

        PositionGrid.Instance.PlaceObjectOnGrid(this.gameObject);
        Vector3? last = null;
        if (HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions().Count > 1)
        {
            last = m_LastValidPositionDif;
        }
        if (PlacementMetaData.Method == ContentPlacementMetaData.EnumMethod.Gravity)
        {
            if (true == GetMouseAsWorldPoint(true, last, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject))
            {
                RootTransform.position = gameObjectNewPosition;
                m_InitialHitGameObject = initialHitGameObject;
            }
        }
        else
        {
            if (HouseBuilderRoot.Instance.InputContents.CurrentInputMode == HouseBuilderInput.InputMode.Placing)
            {
                if (true == GetMouseAsWorldPoint(true, last, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject))
                {
                    RootTransform.position = gameObjectNewPosition;
                    m_InitialHitGameObject = initialHitGameObject;
                }
            }
        }
        
    }

    public override void KeyboardMove(Vector3 positionInRealWorld)
    {
        TurnCutoutHolesOn();

        RootTransform.position = positionInRealWorld;
        if (true == KeyboardMove(positionInRealWorld, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject))
        {
            RootTransform.position = gameObjectNewPosition;
            m_InitialHitGameObject = initialHitGameObject;
        }
    }

    private void TurnCutoutHolesOn()
    {
        if (m_WallObjectLinkerData != null)
        {
            m_WallObjectLinkerData.m_RemovedWalls.ForEach(e => e.enabled = true);
        }
    }

    public override void MouseUp()
    {
        foreach (var item in HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions())
        {
            if (item.m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
            {
                RootTransform.position = m_LastValidPosition;
                RootTransform.rotation = m_LastValidRotation;
                return;
            }
        }

        if (null != CurrentSelectedInteraction)
        {
            if (this.PlacementMetaData.Placement.IsWall() == true)
            {
                if (CurrentSelectedInteraction is InteractionWallGeneratedPoint)
                {
                    var interaction = (InteractionWallGeneratedPoint)CurrentSelectedInteraction;
                    CreateLinkerData(interaction);
                }
                if (PositionGrid.Instance.IsKeyboardInput == false)
                {
                    HouseBuilderRoot.Instance?.GenerateWallsRef.RegenerateWalls(null);
                }
            }
        }

        if (null != HouseBuilder.PositionGrid.Instance)
        {
            if (this.PlacementMetaData.Placement.IsWall() == false)
            {
                HouseBuilder.PositionGrid.Instance.PlaceObjectOnGrid(this.gameObject);
            }
        }

        m_LastValidPosition = RootTransform.position;
        m_LastValidRotation = RootTransform.rotation;

        if (PlacementMetaData.Method == ContentPlacementMetaData.EnumMethod.Gravity)
        {
            if (m_InitialHitGameObject == null || m_InitialHitGameObject.GetComponent<InteractionFloorPlan>() != null)
            {
                HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AttachToThisParent(RootGameObject);
            }
            else
            {
                var floor = m_InitialHitGameObject.GetComponent<InteractionCeilingAndFloor>();
                if (this.PlacementMetaData.Placement.IsWall() == false && floor == null) // do not attach to an object that gets deleted
                {
                    this.transform.SetParent(m_InitialHitGameObject.transform);
                }
            }
        }
        else 
        {
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AttachToThisParent(RootGameObject);
        }
        HouseBuilderRoot.Instance.RecorderRef.Record();
    }

    private void CreateLinkerData(InteractionWallGeneratedPoint interaction)
    {
        HouseBuilderConstants.WallObjectLinkerData linkerData = new HouseBuilderConstants.WallObjectLinkerData();
        m_WallObjectLinkerData = linkerData;
        // figure out percenatge of distance
        Vector3 thisPositionStart = RootTransform.position;
        Vector3 thisPosition = thisPositionStart;
        thisPosition.y = interaction.Position.y;
        float distance = Vector3.Distance(interaction.Position, thisPosition);
        float totalDistance = Vector3.Distance(interaction.Position, interaction.NextPosition);

        linkerData.m_WallIndex = interaction.GeneratedWholeRef.ThisWallIndex;
        linkerData.m_WallPointIndex = interaction.ThisWallPointIndex;
        linkerData.m_PercentageHeight = RootTransform.position.y / interaction.WallPointRef.WallWholeRef.m_StructureJsonData.m_RoomData.m_Height;
        linkerData.m_PercentageLength = distance / totalDistance;

        AssignWallSide(interaction, linkerData);

        CatalogueGeneratedData item = null;
        if (HouseBuilderRoot.Instance != null)
        {
            item = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.Find(e => e.InteractionRef == this);
            item.m_WallObjectLinkerData = linkerData;
        }
        else
        {
            item = HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList.Find(e => e.InteractionRef == this);
            item.m_WallObjectLinkerData = linkerData;
        }
    }

    private void AssignWallSide(InteractionWallGeneratedPoint interaction, HouseBuilderConstants.WallObjectLinkerData linkerData)
    {
        // linked to GenerateWalls.cs 252

        var floorsHit = GenerateWalls.WallItemFloorhit(this.transform);

        linkerData.m_WallSide = HouseBuilderConstants.WallSide.Outside;
        foreach (var floor in floorsHit)
        {
            if (true == floor.collider.gameObject.name.GetNumber(out int number))
            {
                if (number == interaction.GeneratedWholeRef.ThisWallIndex)
                {
                    linkerData.m_WallSide = HouseBuilderConstants.WallSide.Inside;
                }
                Debug.Log($"Wall {number}  {linkerData.m_WallSide}");
            }
        }

        //Debug.LogError("need to disable the floors here, or put back to the there pria state");
    }

    public override void SelectObject()
    {
        base.SelectObject();
    }

    public override void UnSelectObject()
    {
        base.UnSelectObject();
    }

    public void Update()
    {
        if (MeasurementLabelPanel.Instance != null)
        {
            MeasurementLabelPanel.Instance.ClearLabel();
        }
        if (LightingRef != null)
        {
            LightingRef.ManualUpdate();
        }
    }

    private void OnDisable()
    {
        if (MeasurementLabelPanel.Instance != null)
        {
            MeasurementLabelPanel.Instance.ClearLabel();
        }
    }

    private void LateUpdate()
    {
        if (HouseBuilderRoot.Instance != null)
        {
            if (HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions() != null)
            {
                if (HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions().Exists(e => e.gameObject == this.gameObject) == true)
                {
                    var self = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetItem(this.RootGameObject);
                    if (this.PlacementMetaData.Placement.IsWall())
                    {
                        if (self != null && self.m_WallObjectLinkerData != null)
                        {
                            if (HouseBuilderRoot.Instance.GenerateWallsRef.GeneratedWholeList.ContainsIndex(self.m_WallObjectLinkerData.m_WallIndex) == true)
                            {
                                var wall = HouseBuilderRoot.Instance.GenerateWallsRef.GeneratedWholeList[self.m_WallObjectLinkerData.m_WallIndex];

                                List<Vector3> wallpoints = wall.m_WallGeneratedPointList.Select(e => e.Position).ToList();
                                if (wallpoints.Count == 0)
                                {
                                    Debug.LogError("ERROR, there should be no walls with zero points");
                                    return;
                                }
                                WallItemMeasurement(self, wall, wallpoints);
                            }
                        }
                    }
                    FloorItemMeasurement(self);
                }
            }
        }
    }

    private void WallItemMeasurement(CatalogueGeneratedData self, InteractionWallGeneratedWhole wall, List<Vector3> wallpoints)
    {
        if (this.PlacementMetaData.Placement.IsWall() == true)
        {
            BoxCollider boxCollider = (BoxCollider)this.MainCollider;
            float colliderHalfWidth = boxCollider.size.x * 0.5f;
            float colliderHalfHeight = boxCollider.size.y * 0.5f;

            int start = self.m_WallObjectLinkerData.m_WallPointIndex;
            int next = start.WrapIncrement(wallpoints);

            float wallThickness = wall.FloorPlanJsonDataRef.m_RoomData.m_Depth;
            float wallHeight = wall.FloorPlanJsonDataRef.m_RoomData.m_Height;

            Vector3 leftEdge = RootTransform.position + (-RootTransform.right * colliderHalfWidth);
            Vector3 rightEdge = RootTransform.position + (RootTransform.right * colliderHalfWidth);
            if (this.PlacementMetaData.Placement.IsWallAndGrounded() == true)
            {
                leftEdge.y += colliderHalfHeight;
                rightEdge.y += colliderHalfHeight;
            }

            Vector3 line1 = wallpoints[start];
            line1.y = rightEdge.y;
            Vector3 line2 = rightEdge;
            float distance = Vector3.Distance(line1, line2);
            MeasurementLabelPanel.Instance.PlaceLineRender(line1, line2, this.transform, this.transform.forward, wallThickness, true);
            // not used   using PlaceTheMeasurementLabelCutOut  , might need for later though //MeasurementLabelPanel.Instance.PlaceTheMeasurementLabel(distance, rightEdge + (this.transform.right * 0.2f), this.transform, this.transform.forward, wallThickness);

            line1 = leftEdge;
            line2 = wallpoints[next];
            line2.y = leftEdge.y;
            distance = Vector3.Distance(line1, line2);
            MeasurementLabelPanel.Instance.PlaceLineRender(line1, line2, this.transform, this.transform.forward, wallThickness, true);
            // not used   using PlaceTheMeasurementLabelCutOut  , might need for later though //MeasurementLabelPanel.Instance.PlaceTheMeasurementLabel(distance, leftEdge + (-this.transform.right * 0.2f), this.transform, this.transform.forward, wallThickness);

            MeasurementLabelPanel.Instance.PlaceTheMeasurementTextCutOut(wallpoints[start], wallpoints[next], leftEdge, rightEdge, this.transform, this.transform.forward, wallThickness);

            // top and bottom
            Vector3 floor = new Vector3(0, 0, 0);
            Vector3 top = new Vector3(0, this.transform.position.y, 0);

            Vector3 topEdge = RootTransform.position + (RootTransform.up * colliderHalfHeight);
            Vector3 bottomEdge = RootTransform.position + (RootTransform.up * -colliderHalfHeight);

            if (this.PlacementMetaData.Placement.IsWallAndGrounded() == true)
            {
                topEdge = RootTransform.position + (RootTransform.up * colliderHalfHeight * 2);
                Top(wallThickness, wallHeight, topEdge);
            }
            else
            {
                Top(wallThickness, wallHeight, topEdge);
                Bottom(wallThickness, bottomEdge);
            }
        }
    }

    private void Top(float wallThickness, float wallHeight, Vector3 topEdge)
    {
        Vector3 line1 = topEdge;
        Vector3 line2 = topEdge;
        line2.y = wallHeight;

        MeasurementLabelPanel.Instance.PlaceLineRender(line1, line2, RootTransform, RootTransform.forward, wallThickness, true);
        MeasurementLabelPanel.Instance.PlaceTheMeasurementText(Mathf.Abs(topEdge.y - wallHeight), topEdge + (RootTransform.up * 0.2f), RootTransform, RootTransform.forward, wallThickness, true);
    }

    private void Bottom(float wallThickness, Vector3 bottomEdge)
    {
        Vector3 line1 = bottomEdge;
        Vector3 line2 = bottomEdge;
        line2.y = 0;
        MeasurementLabelPanel.Instance.PlaceLineRender(line1, line2, RootTransform, RootTransform.forward, wallThickness, true);
        MeasurementLabelPanel.Instance.PlaceTheMeasurementText(bottomEdge.y, bottomEdge + (-RootTransform.up * 0.2f), RootTransform, RootTransform.forward, wallThickness, true);
    }

    private void FloorItemMeasurement(CatalogueGeneratedData self)
    {
        if (this.PlacementMetaData.Placement.IsWall() == false)
        {
            if (this.MainCollider is BoxCollider)
            {
                BoxCollider boxCollider = (BoxCollider)this.MainCollider;
                float colliderHalfWidth = boxCollider.size.x * 0.5f;
                float colliderHalfHeight = boxCollider.size.y * 0.5f;

                Ray[] ray = new Ray[4];

                ray[0].origin = RootTransform.TransformPoint(boxCollider.center + new Vector3(0, boxCollider.size.y, -boxCollider.size.z) * 0.5f);
                ray[1].origin = RootTransform.TransformPoint(boxCollider.center + new Vector3(boxCollider.size.x, boxCollider.size.y, 0) * 0.5f);
                ray[2].origin = RootTransform.TransformPoint(boxCollider.center + new Vector3(-boxCollider.size.x, boxCollider.size.y, 0) * 0.5f);
                ray[3].origin = RootTransform.TransformPoint(boxCollider.center + new Vector3(0, boxCollider.size.y, boxCollider.size.z) * 0.5f);

                ray[0].direction = -RootTransform.forward;
                ray[1].direction = RootTransform.right;
                ray[2].direction = -RootTransform.right;
                ray[3].direction = RootTransform.forward;

                for (int i = 0; i < 4; i++)
                {
                    RaycastHit hit;
                    Vector3 boxShape = new Vector3(0.1f, boxCollider.size.y / 2, 0.1f);

                    if (Physics.BoxCast(ray[i].origin, boxShape, ray[i].direction, out hit, Quaternion.identity, float.MaxValue) == true)
                    {
                        Vector3 hitPoint = hit.point;
                        hitPoint.y = ray[i].origin.y;
                        MeasurementLabelPanel.Instance.PlaceLineRender(ray[i].origin, hitPoint, RootTransform, Vector3.up, 0, false);

                        float distance = Vector3.Distance(ray[i].origin, hitPoint);
                        Vector3 placement = Vector3.Lerp(ray[i].origin, hitPoint, 0.5f);
                        MeasurementLabelPanel.Instance.PlaceTheMeasurementText(distance, placement, RootTransform, Vector3.up, 0.01f, false);
                    }
                }
            }
            else
            {
                DebugOnce.LogError($"this.MainCollider is not Box Collider {self.InteractionRef.name}", self.InteractionRef.RootGameObject);
            }
        }
    }
}