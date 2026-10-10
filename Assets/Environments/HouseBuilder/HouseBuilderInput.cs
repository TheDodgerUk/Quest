using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class HouseBuilderInput : MonoBehaviour
    {
        protected virtual string ClassName => nameof(HouseBuilderInput);

        protected float m_ScrollWheelAmount;
        protected Ray m_MainRay;
        protected RaycastHit m_MainRayCastHit;
        protected float m_MouseHeldTime = 0;
        protected virtual float MANIPULATE_HELD_TIME => 0.02f;

        private List<Interaction> m_PrevSelectedInteractions = new List<Interaction>();
        private List<Interaction> m_CurrentlySelectedInteractions  = new List<Interaction>();
        public static event Action<int> OnSelectedChanged; //should this be connected somewhere ?


        public enum InputMode
        {
            Nothing,
            Creating,
            Manipulating,
            ManipulatingOther,
            Placing,
            PlacingHolding,
            FigureOutCreatingOrManipulating,
            Add,
            Waiting,
        }


        private InputMode m_CurrentInputMode = InputMode.Nothing;
        public InputMode CurrentInputMode
        {
            get { return m_CurrentInputMode; }
            set
            {
                if (m_CurrentInputMode != value)
                {
                    if (value != InputMode.Waiting && m_CurrentInputMode != InputMode.Waiting)
                    {
                        if (HouseBuilderRoot.Instance != null && HouseBuilderRoot.Instance.RecorderRef != null)
                        {
                            HouseBuilderRoot.Instance.RecorderRef.Record();
                        }
                    }
                    m_MouseHeldTime = 0;
                    m_CurrentInputMode = value;
                    Debug.Log($"CurrentInputMode changed to: {ClassName} : {m_CurrentInputMode}");
                }
            }
        }
        public virtual void Initialise()
        {

        }

        public virtual void CleanUp()
        {

        }

        public List<Interaction> GetCurrentInteractions()
        {
            m_CurrentlySelectedInteractions.RemoveAll(item => item == null);
            return m_CurrentlySelectedInteractions;
        }

        private List<Interaction> CurrentInteractions
        {
            get { return m_CurrentlySelectedInteractions; }
            set
            {
                if (m_CurrentlySelectedInteractions != value)
                {
                    if (m_CurrentlySelectedInteractions != null)
                    {
                        m_CurrentlySelectedInteractions.ForEach(e => e.MouseUp());
                        m_CurrentlySelectedInteractions.ForEach(e => e.UnSelectObject());
                    };

                    m_CurrentlySelectedInteractions = value;
                    if (m_CurrentlySelectedInteractions != null)
                    {
                        m_CurrentlySelectedInteractions.ForEach(e => e.MouseDown());
                        m_CurrentlySelectedInteractions.ForEach(e => e.SelectObject());
                    }
                }

            }
        }

        public void SetCurrentInteractionNoCallBack(Interaction interaction)
        {
            ClearAllInteractions();
            m_CurrentlySelectedInteractions.Add(interaction);
            OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
        }

        public void SetCurrentInteractionsNoCallBack(List<Interaction> interactions)
        {
            ClearAllInteractions();
            m_CurrentlySelectedInteractions.AddRange(interactions);
            OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
        }

        public void SetCurrentInteractionsNoCallBackNoMouseUp(List<Interaction> interactions)
        {
            ClearAllInteractionsNoMouseUp();
            m_CurrentlySelectedInteractions.AddRange(interactions);
            OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
        }

        public void SetCurrentInteractions(List<Interaction> interactions)
        {
            m_PrevSelectedInteractions = new List<Interaction>(GetCurrentInteractions());
            ClearAllInteractions();
            foreach(var item in interactions)
            {
                SelectInteraction(item);
            }
        }


        public void SelectInteraction(Interaction interaction)
        {
            if (interaction != null)
            {
                if (m_CurrentlySelectedInteractions.Contains(interaction) == false)
                {
                    if (interaction.PlacementMetaData.Method == ContentPlacementMetaData.EnumMethod.Widget)
                    {
                        HouseBuilderRoot.Instance.TransformGizmoRef.ClearAndAddTarget(interaction.RootTransform);
                    }
                    AddCurrentInteraction(interaction);
                }
                else
                {
                    interaction.MouseUp();
                    interaction.UnSelectObject();
                    m_CurrentlySelectedInteractions.Remove(interaction);
                    m_CurrentlySelectedInteractions.ForEach(e => e.MouseDown());
                    m_CurrentlySelectedInteractions.ForEach(e => e.SelectObject());
                    OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
                }
            }
        }

        public void AddCurrentInteraction(Interaction interaction)
        {
            if (interaction != null)
            {
                if (m_CurrentlySelectedInteractions.Contains(interaction) == false)
                {
                    m_CurrentlySelectedInteractions.Add(interaction);
                    interaction.MouseDown();
                    interaction.SelectObject();
                    OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
                }
            }
        }

        public void SetCurrentInteraction(Interaction interaction)
        {
            ClearAllInteractions();
            SelectInteraction(interaction);
        }


        public void ClearAllInteractions()
        {
            if(m_CurrentlySelectedInteractions == null)
            {
                m_CurrentlySelectedInteractions = new List<Interaction>();
            }

            if (m_PrevSelectedInteractions == null)
            {
                m_PrevSelectedInteractions = new List<Interaction>();
            }

            if (this is HouseBuilderInputFloorPlan)
            {
                //Bug #12239: Plot mode did not clear up selected items correcty
                // this needs to be done ONLY for HouseBuilderInputFloorPlan
                m_CurrentlySelectedInteractions.ForEach(e => e.MouseUp());

                // if its done for else we  get Bug #12372: when stacking and failing , it is still attached to object object you placed on 
                // so not do it for anything else 
            }

            m_CurrentlySelectedInteractions.ForEach(e => e.UnSelectObject());
 
            m_CurrentlySelectedInteractions.Clear();
            m_PrevSelectedInteractions.Clear();
            OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
            HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
        }

        public void ClearAllInteractionsNoMouseUp()
        {
            if (m_CurrentlySelectedInteractions == null)
            {
                m_CurrentlySelectedInteractions = new List<Interaction>();
            }
            m_CurrentlySelectedInteractions.ForEach(e => e.UnSelectObject());
            m_CurrentlySelectedInteractions.Clear();
            OnSelectedChanged?.Invoke(m_CurrentlySelectedInteractions.Count);
        }


        public Interaction GetLastInteraction()
        {
            if (m_CurrentlySelectedInteractions != null && m_CurrentlySelectedInteractions.Count > 0)
            {
                return m_CurrentlySelectedInteractions.Last();
            }
            return null;
        }

        public Interaction GetPrevInteraction()
        {
            if (m_PrevSelectedInteractions != null && m_PrevSelectedInteractions.Count > 0)
            {
                return m_PrevSelectedInteractions.Last();
            }
            return null;
        }

        protected  Interaction GetCurrentInteractionClassForFillRoomLeftMouse(Collider colliderRef, StructureType structureType)
        {
            Interaction returnInteraction = null;
            if (colliderRef == null)
            {
                return returnInteraction;
            }

            if (colliderRef.name == HouseBuilderConstants.GENERATED_UPPER_HULL || colliderRef.name == HouseBuilderConstants.GENERATED_LOWER_HULL)
            {
                if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Placement)
                {
                    var parentRef = colliderRef.GetComponentInParent<InteractionWallGeneratedPoint>();
                    Debug.Log("Getting parent");
                    returnInteraction = parentRef;
                }
            }
            else
            {
                var baseObject = HouseBuilderConstants.GetValidGameObject(colliderRef.gameObject);
                if(baseObject == null)
                {
                    return null; // cutout wall 
                }


                returnInteraction = baseObject.GetComponent<InteractionGameObjectMove>();
                if (returnInteraction != null)
                {
                    Interaction tempHolder = returnInteraction;
                    returnInteraction = null;
                    if (tempHolder.MainCollider == colliderRef)
                    {
                        returnInteraction = tempHolder;
                    }

                    if (tempHolder.PlacementMetaData.m_UseAllMainColliders == true)
                    {
                        // check all children
                        foreach (var other in tempHolder.MainColliders)
                        {
                            if (other == colliderRef)
                            {
                                Debug.Log("Found on Other Collider");
                                returnInteraction = tempHolder;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    returnInteraction = baseObject.GetComponent<InteractionWallGeneratedPoint>();
                }
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Structure && structureType == StructureType.Ceiling)
                {
                    returnInteraction = baseObject.GetComponent<InteractionCeilingAndFloor>();
                }
            }


            // if wall point 
            // need to double check if the is a door or window attached
            if (returnInteraction is InteractionWallGeneratedPoint)
            {
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Placement || HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Sound)
                {
                    return null;
                }
                var point = (InteractionWallGeneratedPoint)returnInteraction;
                int wallIndex = point.GeneratedWholeRef.ThisWallIndex;
                int wallPartIndex = point.ThisWallPointIndex;

                var list = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
                var itemsConnectedToWall = list.FindAll(e => e.m_WallObjectLinkerData != null && e.m_WallObjectLinkerData.m_WallIndex == wallIndex && e.m_WallObjectLinkerData.m_WallPointIndex == wallPartIndex);
                var ItemsHitByMouse = Physics.RaycastAll(m_MainRay).ToList();

                for (int mouseIndex = 0; mouseIndex < ItemsHitByMouse.Count; mouseIndex++)
                {
                    var rayHit = ItemsHitByMouse[mouseIndex];
                    for (int connectedIndex = 0; connectedIndex < itemsConnectedToWall.Count; connectedIndex++)
                    {
                        var baseObject = HouseBuilderConstants.GetValidGameObject(rayHit.collider.gameObject);
                        if (baseObject == itemsConnectedToWall[connectedIndex].GeneratedItemRef)
                        {
                            returnInteraction = itemsConnectedToWall[connectedIndex].InteractionRef;
                        }
                    }
                }
            }
            return returnInteraction;
        }


        public GameObject GetLastSelectedGameObject()
        {
            if(null != CurrentInteractions && CurrentInteractions.Count > 0)
            {
                return CurrentInteractions.Last().gameObject;
            }
            return null;
        }

        //for whenever user left-clicks and moves his mouse with the same motion (like a paintbursh stroke)
        protected bool IsMouseClickedAndDragged()
        {             
            return (m_MouseHeldTime > (MANIPULATE_HELD_TIME) &&
                  ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0)));
        }


        protected bool IsDragged()
        {
            return ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0));
        }

        protected bool IsMouseClickedAndTapped()
        {
            return (m_MouseHeldTime > 0 && m_MouseHeldTime < MANIPULATE_HELD_TIME);
        }

        protected bool IsMouseClickedAndIgnoreDragged()
        {
            return (m_MouseHeldTime > (MANIPULATE_HELD_TIME));
        }

        public void ClearAllInteractionsWithModeChange()
        {
            ClearAllInteractions();
            CurrentInputMode = InputMode.Nothing;
        }


        public virtual void ManualUpdate()
        {
            UpdateInputControlsFor_EarlyUpdate();
            switch (CurrentInputMode)
            {
                case InputMode.Nothing:
                    UpdateInputControlsFor_Nothing();
                    break;
                case InputMode.Creating:
                    UpdateInputControlsFor_Creating();
                    break;
                case InputMode.Manipulating:
                    UpdateInputControlsFor_Manipulating();
                    break;
                case InputMode.ManipulatingOther:
                    UpdateInputControlsFor_ManipulatingOther();
                    break;
                case InputMode.Placing:
                    UpdateInputControlsFor_Placing();
                    break;
                case InputMode.PlacingHolding:
                    UpdateInputControlsFor_PlacingHolding();
                    break;
                case InputMode.FigureOutCreatingOrManipulating:
                    UpdateInputControlsFor_FigureOutCreatingOrManipulating();
                    break;
                case InputMode.Add:
                    UpdateInputControlsFor_Add();
                    break;
                default:
                    break;
            }
            UpdateInputControlsFor_LateUpdate();
        }

        protected virtual void UpdateInputControlsFor_Nothing() { }
        protected virtual void UpdateInputControlsFor_Creating() { }
        protected virtual void UpdateInputControlsFor_Manipulating() { }
        protected virtual void UpdateInputControlsFor_ManipulatingOther() { }
        protected virtual void UpdateInputControlsFor_Placing() { }
        protected virtual void UpdateInputControlsFor_PlacingHolding() { }
        protected virtual void UpdateInputControlsFor_FigureOutCreatingOrManipulating() { }
        protected virtual void UpdateInputControlsFor_Add() { }
        protected virtual void UpdateInputControlsFor_EarlyUpdate() { }
        protected virtual void UpdateInputControlsFor_LateUpdate() { }
    }
}
