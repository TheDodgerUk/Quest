using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace HouseBuilder
{
    public class HouseBuilderInputCeiling : HouseBuilderInput
    {
        protected override string ClassName => nameof(HouseBuilderInputCeiling);

        public CeilingFloorGenerator CeilingFloorGeneratorRef { get; private set; }
        public ModeContainer<InteractionCeilingAndFloor> CeilingModeContainerRefSolid { get; set; }
        public ModeContainer<InteractionCeilingAndFloor> FloorModeContainerRefSolid { get; set; }

        public enum PanelTypeEnum
        {
            Ceiling,
            Floor,
        };

        //This should be renamed to something relating to walls instead
        private HouseBuilderInputFloorPlan m_FloorPlan;

        private StructureType m_StructureSwitch = StructureType.None;

        public override void Initialise()
        {
            CeilingFloorGeneratorRef = this.gameObject.ForceComponent<CeilingFloorGenerator>();
            CeilingModeContainerRefSolid = new ModeContainer<InteractionCeilingAndFloor>("CeilingSolid", this.gameObject);
            FloorModeContainerRefSolid = new ModeContainer<InteractionCeilingAndFloor>("FloorSolid", this.gameObject);

            if (HouseBuilderRoot.Instance != null)
            {
                HBGUIManager.Instance.m_StructureSwitchMenu.OnStructureSwitchChanged += (value) => { m_StructureSwitch = value; };
            }
        }

        public void SetInputFloor(HouseBuilderInputFloorPlan floorPlan)
        {
            m_FloorPlan = floorPlan;
        }

        public void ToggleCeilingShadows(bool enable)
        {
            CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetShadows(enable));
        }

        public void GenerateCeilingsAndFloorsForRooms(Action callback)
        {
#if UNITY_EDITOR
            Debug.Log("GenerateCeilingsAndFloorsForRooms");
#endif
            TaskAction task = new TaskAction(2, () => { callback?.Invoke(); });

            CeilingFloorGeneratorRef.CreateAll(m_FloorPlan.ModeContainerRef, CeilingModeContainerRefSolid, PanelTypeEnum.Ceiling, task);
            CeilingFloorGeneratorRef.CreateAll(m_FloorPlan.ModeContainerRef, FloorModeContainerRefSolid, PanelTypeEnum.Floor, task);
        }

        #region Ceiling
        public void ToggleTemporaryAllCeilingVisibility(bool state)
        {
            for (int i = 0; i < m_FloorPlan.ModeContainerRef.GetList.Count; i++)
            {
                var data = m_FloorPlan.ModeContainerRef.GetList[i].m_StructureJsonData.m_RoomData;
                if (i < CeilingModeContainerRefSolid.GetList.Count)
                {
                    var ceilingRef = CeilingModeContainerRefSolid.GetList[i];
                    var mesh = ceilingRef.GetComponent<MeshRenderer>();
                    SetMesh(ceilingRef, state);
                }
            }
        }

        public void ToggleCeilingVisibility(int ceilingIndex, bool state)
        {
            var data = m_FloorPlan.ModeContainerRef.GetList[ceilingIndex].m_StructureJsonData.m_RoomData;
            data.m_HasRoof = state;

            var ceilingRef = CeilingModeContainerRefSolid.GetList[ceilingIndex];
            SetMesh(ceilingRef, state);
        }

        public void ShowCorrectCeilingVisibility()
        {
            for (int i = 0; i < m_FloorPlan.ModeContainerRef.GetList.Count; i++)
            {
                var data = m_FloorPlan.ModeContainerRef.GetList[i].m_StructureJsonData.m_RoomData;
                if (i < FloorModeContainerRefSolid.GetList.Count)
                {
                    var ceiling = CeilingModeContainerRefSolid.GetList[i];
                    SetMesh(ceiling, data.m_HasRoof);
                }
            }
        }

        #endregion Ceiling
        #region Floor

        public void ToggleTemporaryAllFloorVisibility(bool state)
        {
            for (int i = 0; i < m_FloorPlan.ModeContainerRef.GetList.Count; i++)
            {
                var data = m_FloorPlan.ModeContainerRef.GetList[i].m_StructureJsonData.m_RoomData;
                if (i < FloorModeContainerRefSolid.GetList.Count)
                {
                    var floorRef = FloorModeContainerRefSolid.GetList[i];
                    SetMesh(floorRef, state);
                }
            }
        }

        public void ToggleFloorVisibility(int floorIndex, bool state)
        {
            var data = m_FloorPlan.ModeContainerRef.GetList[floorIndex].m_StructureJsonData.m_RoomData;
            data.m_HasFloor = state;

            var floorRef = FloorModeContainerRefSolid.GetList[floorIndex];
            SetMesh(floorRef, state);
        }

        public void ShowCorrectFloorVisibility()
        {
            for (int i = 0; i < m_FloorPlan.ModeContainerRef.GetList.Count; i++)
            {
                var data = m_FloorPlan.ModeContainerRef.GetList[i].m_StructureJsonData.m_RoomData;
                if (i < FloorModeContainerRefSolid.GetList.Count)
                {
                    var floorRef = FloorModeContainerRefSolid.GetList[i];
                    SetMesh(floorRef, data.m_HasFloor);
                }
            }
        }

        private void SetMesh(Interaction interaction, bool state)
        {
            var mesh = interaction.GetComponent<MeshRenderer>();
            if (state)
            {
                mesh.shadowCastingMode = ShadowCastingMode.TwoSided;
            }
            else
            {
                mesh.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        #endregion Floor

        private Interaction FindNextInteraction(string structureSwitchName)
        {
            List<RaycastHit> rays = Physics.RaycastAll(m_MainRay).OrderBy(h => h.distance).ToList(); // closest first
            rays.RemoveAll(e => e.collider.name.Contains(structureSwitchName) == false);// remove invalid

            Interaction newInteraction = null;
            int index = 0;
            bool found = false;

            if (GetLastInteraction() != null) // if it has one sisnse last time, find it again 
            {
                for (index = 0; index < rays.Count; index++)
                {
                    m_MainRayCastHit = rays[index];
                    newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider.gameObject);
                    if (GetLastInteraction() == newInteraction)
                    {
                        found = true;
                        break;
                    }
                }
            }

            var raysClone = rays.Clone();
            rays.AddRange(raysClone); // double it , this will help the, for loop code

            if (found == true)
            {
                index++; // so it starts on the next possible item
            }
            for (; index < rays.Count; index++)
            {
                m_MainRayCastHit = rays[index];
                newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider.gameObject);
                if (newInteraction != null)
                {
                    break;
                }
            }

            return newInteraction;
        }

        protected override void UpdateInputControlsFor_Nothing()
        {
            if (HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Tap) &&
                false == EventSystem.current.IsPointerOverGameObject())
            {
                string structureSwitchName = "NOT VALID";
                HBGUIManager.Instance.DisableAllHeaderTogglesMenu();

                m_MainRay = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());

                bool clickedOnWall = false;
                Interaction newInteraction = null;
                // find only m_MainColiider on gameObject
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Structure)                
                {                    
                    var hits = Physics.RaycastAll(m_MainRay).OrderBy(h => h.distance).ToList();
                    foreach (var hit in hits)
                    {
                        m_MainRayCastHit = hit;
                        newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider, m_StructureSwitch);
                        if(newInteraction != null)
                        {
                            if ((newInteraction is InteractionWallGeneratedPoint) || (newInteraction.PlacementMetaData.Placement.IsCutout() == true))
                            {
                                clickedOnWall = true;
                                break;
                            }
                        }
 
                    }

                }

                //SPECIAL CASE SELECTING ITEM CHANGES STRUCT TYPE, TODO: to rework this one way or the other, not both
                if (m_StructureSwitch != StructureType.Floor && 
                    m_StructureSwitch != StructureType.Ceiling && clickedOnWall == false)
                {
                    Interaction floor = FindNextInteraction(HouseBuilderConstants.FLOOR_NAME);
                    if (floor != null)
                    {
                        HBGUIManager.Instance.m_StructureSwitchMenu.ForceEditFloor();
                        m_StructureSwitch = StructureType.Floor;//have to do it here or wait 1 frame for the toggle to change it                        
                    }
                }//not necessary to do this for ceiling because its hidden by default in Structure mode

                switch (m_StructureSwitch)
                {
                    case StructureType.Ceiling:
                        structureSwitchName = HouseBuilderConstants.CEILING_NAME;
                        break;
                    case StructureType.Floor:
                        structureSwitchName = HouseBuilderConstants.FLOOR_NAME;
                        break;
                }

                m_MouseHeldTime += Time.deltaTime;

                newInteraction = FindNextInteraction(structureSwitchName);

                if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.MultipleSelect))
                {
                    SelectInteraction(newInteraction);
                }
                else
                {
                    SetCurrentInteraction(newInteraction);
                }

                CurrentInputMode = InputMode.Nothing;

                if (null != GetLastInteraction())
                {
                    CurrentInputMode = InputMode.FigureOutCreatingOrManipulating;
                }
            }
            else
            {
                m_MouseHeldTime = 0;
            }
        }

        protected override void UpdateInputControlsFor_Creating()
        {
            CurrentInputMode = InputMode.Nothing;
        }

        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {
            if (HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) && false == EventSystem.current.IsPointerOverGameObject())
            {
                m_MouseHeldTime += Time.deltaTime;
                if (IsMouseClickedAndDragged())
                {
                    CurrentInputMode = InputMode.Manipulating;
                }
            }
            else
            {
                AddCurrentInteraction(GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider.gameObject));
                CurrentInputMode = InputMode.Nothing;
            }
        }

        protected override void UpdateInputControlsFor_Manipulating()
        {
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction())
                {
                    GetLastInteraction().MouseDrag();
                }
            }
            else
            {
                if (null != GetLastInteraction())
                {
                    GetLastInteraction().MouseUp();
                    GetLastInteraction().SelectObject();
                }
                CurrentInputMode = InputMode.FigureOutCreatingOrManipulating;
            }
        }

        protected override void UpdateInputControlsFor_Placing()
        {
            if (false == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction())
                {
                    GetLastInteraction().MouseDrag();
                }
            }
            else
            {
                if (null != GetLastInteraction())
                {
                    if (GetLastInteraction().m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
                    {
                        HouseBuilderRoot.Instance.InputContents.ModeContainerRef.Delete(GetLastInteraction().gameObject);
                        SetCurrentInteraction(null);
                    }
                    else
                    {
                        GetLastInteraction().MouseUp();
                        GetLastInteraction().SelectObject();
                    }
                }

                CurrentInputMode = InputMode.Nothing;
            }
        }

        private Interaction GetCurrentInteractionClassForFillRoomLeftMouse(GameObject gameObjectRef)
        {
            Interaction returnInteraction = null;
            if (null == gameObjectRef.GetComponent<Renderer>())
            {
                // this is from the cataloge
                Transform transformRoot = gameObjectRef.transform.parent.parent;
                returnInteraction = transformRoot.GetComponent<InteractionCeilingAndFloor>();
            }
            else
            {
                returnInteraction = gameObjectRef.GetComponent<InteractionCeilingAndFloor>();
            }
            return returnInteraction;
        }
    }
}