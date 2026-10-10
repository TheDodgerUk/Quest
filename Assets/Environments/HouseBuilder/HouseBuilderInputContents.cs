using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouseBuilder
{
    public class HouseBuilderInputContents : HouseBuilderInput
    {
        public event Action OnDeleteKeyPressed;
        public event Action OnDuplicateKeyPressed;
        protected override string ClassName => nameof(HouseBuilderInputContents);

        public ModeContainerCatalogueGeneratedData ModeContainerRef { get; set; }

        private Func<KeyCommand, bool> InputKey;

        private StructureType m_StructureType = StructureType.None;
        public void SetPlacementType(StructureType structureType) => m_StructureType = structureType;

        public override void Initialise()
        {
            ModeContainerRef = new ModeContainerCatalogueGeneratedData("Contents", this.gameObject, this);

            InputKey = HouseBuilderConstants.IsKeyboardCommand;
        }

        private void KeyboardInput()
        {
            if(GetCurrentInteractions().Count == 0)
            {
                return;
            }


            if (GetLastInteraction() != null)
            {
                if (InputKey(KeyCommand.Delete) == true)
                {
                    OnDeleteKeyPressed?.Invoke();
                }

                if (InputKey(KeyCommand.Cancel) == true)
                {
                    ClearAllInteractions();
                }

                if (InputKey(KeyCommand.Duplicate) == true)
                {
                    OnDuplicateKeyPressed?.Invoke();
                }

                if(InputKey(KeyCommand.CentreObject) == true)
                {
                    foreach (var item in GetCurrentInteractions())
                    {
                        if (item.RootTransform.parent != null)
                        {
                            var parentPos = item.RootTransform.parent.transform.position;
                            parentPos.y = item.RootTransform.position.y;
                            item.MouseDown();
                            item.KeyboardMove(parentPos);
                            item.MouseUp();
                        }
                    }
                }
                
                if (InputKey(KeyCommand.RotateModifier) == true)
                {
                    
                    float speed = InputKey(KeyCommand.KeyboardMoveSlowModifier) ? (3f * Time.deltaTime) : (13 * Time.deltaTime);
                    float rotateAmount = 0;
                    var rotate = RotateAmount(speed, ref rotateAmount);
                    if (rotate == true)
                    {
                        foreach (var item in GetCurrentInteractions())
                        {
                            if (item.PlacementMetaData.Placement.IsWall() == false)
                            {
                                HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = false;
                                PositionGrid.Instance.IsKeyboardInput = true;
                                var rotItem = item.GetComponent<InteractionGameObjectRotate>();
                                rotItem.MouseDown();
                                rotItem.KeyboardRotate(rotateAmount);
                                rotItem.MouseUp();
                                HBGUIManager.Instance.m_RotationAmountGUI.SetAmount(rotItem.RootTransform.eulerAngles.y);
                                PositionGrid.Instance.IsKeyboardInput = false;
                                HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = true;
                            }
                        }
                    }
                }
                else
                {
                    float speed = InputKey(KeyCommand.KeyboardMoveSlowModifier) ? (0.2f * Time.deltaTime) : (1 * Time.deltaTime);
                    float spaceUp = 0;
                    float spaceRight = 0;

                    var moved = MovementAmount(speed, ref spaceUp, ref spaceRight);
                    
                    if (moved == true)
                    {
                        foreach (var item in GetCurrentInteractions())
                        {
                            item.SnappingPointsRef.EnableSnapping = false;
                            HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = false;
                            PositionGrid.Instance.IsKeyboardInput = true;   
                            if (item.PlacementMetaData.Placement.IsCutout() == false)
                            {
                                item.MouseDown();
                                Vector3 relativeMove = Vector3.zero;
                                if (item.PlacementMetaData.Placement.IsWall() == true)
                                {
                                    Transform trans = item.RootTransform;
                                    relativeMove = (trans.right * spaceRight);
                                    relativeMove = -relativeMove;
                                    relativeMove += (trans.up * spaceUp);
                                }
                                else
                                {
                                    if (HBGUIManager.Instance.m_ViewMode.CurrentView == View.FirstPerson)
                                    {
                                        Transform trans = Camera.main.transform;
                                        relativeMove = (trans.up * spaceUp) + (trans.right * spaceRight);
                                    }
                                    else
                                    {
                                        Transform trans = item.RootTransform;
                                        relativeMove = (trans.forward * spaceUp) + (trans.right * spaceRight);
                                    }                                   
                                }

                                var newpos = item.RootTransform.position + relativeMove;
                                item.KeyboardMove(newpos);
                                item.MouseUp();
                            }
                            else
                            {
                                Debug.LogError("Keyboard not set up for cut out yet");
                            }
                            item.SnappingPointsRef.EnableSnapping = true;
                            HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = true;
                            PositionGrid.Instance.IsKeyboardInput = false;
                        }
                    }
                }
            }
        }

        public override void CleanUp()
        {
            // checks for connected deleted objects
            int count = ModeContainerRef.GetList.Count;
            for (int i = 0; i < count; i++) // use this so no chance of getting stuck in infinate loop
            {
                var deletedObject = ModeContainerRef.GetList.Find(e => e.GeneratedItemRef == null);
                if (deletedObject != null)
                {
                    ModeContainerRef.GetList.Remove(deletedObject);
                }
                else
                {
                    break;
                }
            }
        }


        private bool MovementAmount(float space, ref float spaceUp, ref float spaceRight)
        {
            if (InputKey(KeyCommand.MoveObjectForward))
            {
                spaceUp = space;
            }
            else if (InputKey(KeyCommand.MoveObjectBackward))
            {
                spaceUp = -space;
            }

            if (InputKey(KeyCommand.MoveObjectLeft))
            {
                spaceRight = -space;
            }
            else if (InputKey(KeyCommand.MoveObjectRight))
            {
                spaceRight = space;
            }
            return (spaceRight != 0 || spaceUp != 0);
        }


        private bool RotateAmount(float speed, ref float amount)
        {
            if (InputKey(KeyCommand.RotateObjectAntiClockWise))
            {
                amount = -speed;
            }
            else if (InputKey(KeyCommand.RotateObjectClockWise))
            {
                amount = speed;
            }
            return (amount != 0);
        }


        protected override void UpdateInputControlsFor_Nothing()
        {
            HBGUIManager.Instance.m_RotationAmountGUI.SetActive(false);
            HouseBuilderRoot.Instance.RotateItemMode = false;
            KeyboardInput();
            bool rotateState = (InputKey(KeyCommand.RotateModifier) && true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Right, PressType.Hold));
            if ((true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) || rotateState) && false == EventSystem.current.IsPointerOverGameObject())
            {
                m_MouseHeldTime += Time.deltaTime;
                if (HouseBuilderRoot.Instance.TransformGizmoRef.IsOverGizmo() == true)
                {
                    CurrentInputMode = InputMode.FigureOutCreatingOrManipulating;
                    return;
                }

                m_MainRay = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
                Debug.DrawRay(m_MainRay.origin, m_MainRay.direction * 100, Color.red, 1);


                // find only m_MainColiider on gameObject
                Interaction newInteraction = null;
                var hits = Physics.RaycastAll(m_MainRay).OrderBy(h => h.distance).ToList();
                foreach (var hit in hits)
                {
                    m_MainRayCastHit = hit;
                    newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider, m_StructureType);

                    newInteraction = HouseBuilderInputStaging.FindRootStagingItem(newInteraction, lookat: true);

                    if (newInteraction != null)
                    {
                        break;
                    }
                }

                if (HouseBuilderRoot.Instance.PanMode == true)
                {
                    newInteraction = null;
                }

                if (newInteraction != null)
                {
                    if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Structure)
                    {
                        //SPECIAL CASE SELECTING ITEM CHANGES STRUCT TYPE, TODO: to rework this one way or the other, not both
                        if (newInteraction is InteractionWallGeneratedPoint)
                        {
                            if (m_StructureType != StructureType.Walls)
                            {
                                HBGUIManager.Instance.m_StructureSwitchMenu.ForceEditWalls();
                                return;
                            }
                        }
                        else if (newInteraction.PlacementMetaData.Placement.IsCutout() == true)
                        {
                            if (m_StructureType != StructureType.Openings)
                            {
                                HBGUIManager.Instance.m_StructureSwitchMenu.ForceEditOpenings();
                                return;
                            }
                        }
                    }

                    switch (m_StructureType)
                    {
                        case StructureType.None:
                            if (newInteraction is InteractionWallGeneratedPoint || newInteraction is InteractionWallGeneratedWhole) // no longer allowed in this mode 
                            {
                                return;
                            }
                            if (newInteraction.PlacementMetaData.Placement.IsCutout() == true)
                            {
                                return;
                            }
                            break;
                        case StructureType.Ceiling:
                        case StructureType.Floor:
                            return;
                        case StructureType.Openings:
                            if (newInteraction is InteractionWallGeneratedPoint || newInteraction is InteractionWallGeneratedWhole) // no longer allowed in this mode 
                            {
                                return;
                            }
                            if (newInteraction.PlacementMetaData.Placement.IsCutout() == false)
                            {
                                return;
                            }
                            break;
                        case StructureType.Walls:
                            if (!(newInteraction is InteractionWallGeneratedPoint || newInteraction is InteractionWallGeneratedWhole))
                            {
                                return;
                            }
                            break;
                    }
                           

                              

                    if (InputKey(KeyCommand.MultipleSelect))
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
                    HBGUIManager.Instance.DisableAllHeaderTogglesMenu();
                    SetCurrentInteraction(newInteraction);

                    if (IsMouseClickedAndDragged())
                    {
                        HouseBuilderRoot.Instance.PanMode = true;
                    }
                }
            }
            else
            {
                m_MouseHeldTime = 0;
                HouseBuilderRoot.Instance.PanMode = false;
                if(HBGUIManager.Instance.m_ViewMode.CurrentFirstPersonType == FirstPersonType.Orbit)
                {
                    ClearAllInteractions();
                }
            }
        }


        protected override void UpdateInputControlsFor_Creating()
        {
            CurrentInputMode = InputMode.Nothing;
        }

        protected override void UpdateInputControlsFor_FigureOutCreatingOrManipulating()
        {
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold) && false == EventSystem.current.IsPointerOverGameObject())
            {

                m_MouseHeldTime += Time.deltaTime;

                if (IsMouseClickedAndDragged())
                {
                    CurrentInputMode = InputMode.Manipulating;
                }
            }
            else
            {
                //SetCurrentInteraction(GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider));
                CurrentInputMode = InputMode.Nothing;

            }
        }


        protected override void UpdateInputControlsFor_Manipulating()
        {
            var rotate = InputKey(KeyCommand.RotateModifier);
            if (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction())
                {
                    if (false == rotate)
                    {
                        GetCurrentInteractions().ForEach(e => e.MouseDrag());
                    }
                    else
                    {
                        if (false == (GetLastInteraction() is InteractionGameObjectRotate))
                        {
                            List<Interaction> rots = new List<Interaction>();
                            foreach(var item in GetCurrentInteractions())
                            {
                                
                                rots.Add(item.GetComponent<InteractionGameObjectRotate>());                               
                            }
                            SetCurrentInteractionsNoCallBack(rots);
                            GetCurrentInteractions().ForEach(e => e.SelectObject());
                            HouseBuilderRoot.Instance.RotateItemMode = true;
                            HBGUIManager.Instance.m_RotationAmountGUI.SetActive(true);
                            
                        }
                        if (GetLastInteraction() != null)
                        {
                            GetCurrentInteractions().ForEach(e => e.MouseDrag());

                            HBGUIManager.Instance.m_RotationAmountGUI.SetAmount(GetLastInteraction().RootTransform.eulerAngles.y);
                        }
                    }
                }
            }
            else
            {
                if (null != GetLastInteraction())
                {
                    if (true == (GetLastInteraction() is InteractionGameObjectRotate))
                    {
                        List<Interaction> moves = new List<Interaction>();
                        foreach (var item in GetCurrentInteractions())
                        {
                            moves.Add(item.GetComponent<InteractionGameObjectMove>());
                        }
                        SetCurrentInteractionsNoCallBack(moves);
                    }

                    GetCurrentInteractions().ForEach(e => e.MouseUp());
                    GetCurrentInteractions().ForEach(e => e.SelectObject());
                }
                CurrentInputMode = InputMode.FigureOutCreatingOrManipulating;
            }
        }


        protected override void UpdateInputControlsFor_Placing()
        {
            if(HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Delete))
            {
                OnDeleteKeyPressed?.Invoke();
            }
            if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Cancel))
            {
                OnDeleteKeyPressed?.Invoke();
            }


            if (false == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction()) //this should be combined with the same below
                {
                    GetCurrentInteractions().ForEach(e => e.MouseDrag());
                }
            }
            else
            {
                var interaction = GetLastInteraction();
                if (interaction != null)
                {
                    if (interaction.PlacementMetaData.Placement.IsWall() == true)
                    {
                        if (interaction.m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
                        {
                            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.Delete(interaction.gameObject, true);
                            SetCurrentInteraction(null);
                        }
                        else
                        {
                            GetCurrentInteractions().ForEach(e => e.MouseUp());
                            GetCurrentInteractions().ForEach(e => e.SelectObject());
                        }

                        CurrentInputMode = InputMode.Nothing;
                    }
                    else
                    {
                        CurrentInputMode = InputMode.PlacingHolding;
                    }
                }
                else
                {
                    CurrentInputMode = InputMode.Nothing;
                }
            }
        }

        public void DeleteAll()
        {
            for(int i = ModeContainerRef.GetList.Count-1; i >= 0; i--) // needs to be reversed, so loops correctly 
            {
                var item =  ModeContainerRef.GetList[i];
                DeleteItem((InteractionGameObject)item.InteractionRef, false);
            }
        }

        public void DeleteItem(InteractionGameObject item, bool displayLog)
        {
            CatalogueGeneratedData itemData = ModeContainerRef.GetItem(item.RootGameObject);             
            ModeContainerRef.Delete(item.RootGameObject, displayLog);
        }

        protected override void UpdateInputControlsFor_PlacingHolding()
        {
            if (false == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold))
            {
                if (null != GetLastInteraction())
                {
                    if (true == (GetLastInteraction() is InteractionGameObjectRotate))
                    {
                        List<Interaction> moves = new List<Interaction>();
                        foreach (var item in GetCurrentInteractions())
                        {
                            moves.Add(item.GetComponent<InteractionGameObjectMove>());
                        }
                        SetCurrentInteractionsNoCallBack(moves);
                    }

                    if (GetLastInteraction().m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
                    {
                        ModeContainerRef.Delete(GetLastInteraction().gameObject, true);
                        SetCurrentInteraction(null);
                    }
                    else
                    {
                        foreach(var item in GetCurrentInteractions())
                        {
                            CatalogueGeneratedData data = ModeContainerRef.GetItem(item.RootGameObject);
                        }                       
                        
                        GetCurrentInteractions().ForEach(e => e.MouseUp());
                        GetCurrentInteractions().ForEach(e => e.SelectObject());
                    }
                }

                CurrentInputMode = InputMode.Nothing;
            }
            else
            {
                if (false == (GetLastInteraction() is InteractionGameObjectRotate))
                {
                    List<Interaction> rots = new List<Interaction>();
                    foreach (var item in GetCurrentInteractions())
                    {

                        rots.Add(item.GetComponent<InteractionGameObjectRotate>());
                    }
                    SetCurrentInteractionsNoCallBackNoMouseUp(rots);
                    GetCurrentInteractions().ForEach(e => e.SelectObject());
                }
                if (GetLastInteraction() != null)
                {
                    GetCurrentInteractions().ForEach(e => e.MouseDrag());
                }
            }
        }

    }
}
