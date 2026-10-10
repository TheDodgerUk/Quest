using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HouseBuilder
{
    public class HouseBuilderInputStaging : HouseBuilderInput
    {
        private ModeContainerCatalogueGeneratedData ModeContainerRef;

        private Interaction m_LeftInteraction;
        private Interaction m_RightInteraction;

        public bool m_UpdateInputControlsFor_Nothing = true;

        public override void Initialise()
        {
            ModeContainerRef = HouseBuilderRoot.Instance.InputContents.ModeContainerRef;
        }


        protected override void UpdateInputControlsFor_Nothing()
        {
            if(m_UpdateInputControlsFor_Nothing == false)
            {
                return;
            }
            if (HouseBuilderRoot.Instance.PanMode == true)
            {
                return;
            }

            bool leftMouse = (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold));
            bool rightMouse = (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Right, PressType.Hold));

            if ((leftMouse || rightMouse) && false == EventSystem.current.IsPointerOverGameObject())
            {
                if (HouseBuilderRoot.Instance.IsMouseOverGUI())
                {
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
                    newInteraction = GetCurrentInteractionClassForFillRoomLeftMouse(m_MainRayCastHit.collider, StructureType.None);
                    if (newInteraction is InteractionGameObject)
                    {
                        if (newInteraction.StagingRef != null)
                        {
                            break;
                        }
                    }
                }

                MousePress(leftMouse, rightMouse, newInteraction, addedNewPiece: false);
                Staging left = null;
                if (m_LeftInteraction != null)
                {
                    left = m_LeftInteraction.StagingRef;
                }

                Staging right = null;
                if (m_RightInteraction != null)
                {
                    right = m_RightInteraction.StagingRef;
                }

                DisplayCorrectFilter();

                HBGUIManager.Instance.StagingSettingsMenuRef.ConnectOnReselection(left, right);
                ClearAllInteractions(); // WE DOING IT SEPERATLY BELOW
                //SetCurrentInteraction(newInteraction);              
                CurrentInputMode = InputMode.Waiting;
                this.WaitUntil(1, () => (AnyMousePresseed() == false), () =>
                {
                    CurrentInputMode = InputMode.Nothing;
                });
            }
        }

        public void CreateStagingStartPoint(CatalogueGeneratedData item)
        {
            m_LeftInteraction = null;
            m_RightInteraction = null;
            MousePress(true, false, item.InteractionRef, addedNewPiece: false);
            DisplayCorrectFilter();
            HBGUIManager.Instance.StagingSettingsMenuRef.ConnectOnReselection(item.InteractionRef.StagingRef, null);
            CurrentInputMode = InputMode.Nothing;
        }

        /// <summary>
        /// Only show the staging typoe that will connect with the parent 
        /// </summary>
        private void DisplayCorrectFilter()
        {
            if (m_LeftInteraction != null || m_RightInteraction != null)
            {
                ContentStagingMetaData.ConectionType connection = ContentStagingMetaData.ConectionType.Ladder;
                if (m_LeftInteraction != null)
                {
                    connection = m_LeftInteraction.StagingRef.ContentStagingMetaDataRef.m_ConectionType;
                }
                else
                {
                    connection = m_RightInteraction.StagingRef.ContentStagingMetaDataRef.m_ConectionType;
                }
                switch (connection)
                {
                    case ContentStagingMetaData.ConectionType.Ladder:
                        HBGUIManager.Instance.CatalogueMenuRef.ManualSetFilter(CatalogueMenu.ManualMenuContents.StagingLadder);
                        break;
                    case ContentStagingMetaData.ConectionType.Triangle:
                        HBGUIManager.Instance.CatalogueMenuRef.ManualSetFilter(CatalogueMenu.ManualMenuContents.StagingTrigangle);
                        break;
                    case ContentStagingMetaData.ConectionType.Square:
                        HBGUIManager.Instance.CatalogueMenuRef.ManualSetFilter(CatalogueMenu.ManualMenuContents.StagingSquare);
                        break;
                    case ContentStagingMetaData.ConectionType.Rectangle:
                        HBGUIManager.Instance.CatalogueMenuRef.ManualSetFilter(CatalogueMenu.ManualMenuContents.StagingSquare);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                HBGUIManager.Instance.CatalogueMenuRef.ManualSetFilter(CatalogueMenu.ManualMenuContents.Staging);
            }
        }


        /// <summary>
        /// place a new interaction into the correct mouse button 
        /// </summary>
        public void MousePress(bool leftMouse, bool rightMouse, Interaction newInteraction, bool addedNewPiece)
        {
            if (leftMouse)
            {
                m_RightInteraction = null;

                if (m_LeftInteraction == newInteraction)
                {
                    m_LeftInteraction = null;
                }
                else
                {
                    m_LeftInteraction = newInteraction;
                }
            }
            if (rightMouse)
            {
                if (m_LeftInteraction != null && m_LeftInteraction == newInteraction)
                {
                    m_LeftInteraction = null;
                }

                if (m_RightInteraction == newInteraction)
                {
                    m_RightInteraction = null;
                }
                else
                {
                    m_RightInteraction = newInteraction;
                }
            }

            CheckIfDirectSiblingsSelectionIsValid(addedNewPiece);
            HBGUIManager.Instance.StagingSettingsMenuRef.CollectAndDisplayVisuals(m_LeftInteraction, m_RightInteraction);
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.EnableAllColliders(true);
        }


        private void CheckIfDirectSiblingsSelectionIsValid(bool addedNewPiece)
        {
            // check if right is connected to the left
            if (addedNewPiece == false && m_LeftInteraction != null && m_RightInteraction != null)
            {
                bool isConnected = false;
                foreach (var left in m_LeftInteraction.StagingRef.ContentStagingMetaDataRef.m_StagingData)
                {
                    foreach (var right in m_RightInteraction.StagingRef.ContentStagingMetaDataRef.m_StagingData)
                    {
                        if (left.m_ConnectedTo == right || right.m_ConnectedTo == left)
                        {
                            isConnected = true;
                            break;
                        }
                    }
                }

                // check its correctly parented 
                if(isConnected == true)
                {
                    var allparent = m_RightInteraction.GetComponentsInParent<Interaction>();
                    if(allparent.Contains(m_LeftInteraction) == false)
                    {
                        isConnected = false;
                    }
                }

                if (isConnected == false)
                {
                    Debug.LogError("NOT CONNECTED to ");
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.Beep();
#endif
                    m_RightInteraction = null;
                }

                if(m_RightInteraction != null)
                {
                    SetCorrectConnectionPoints();
                }
            }
        }

        private void SetCorrectConnectionPoints()
        {
            var leftData = m_LeftInteraction.StagingRef.ContentStagingMetaDataRef;
            var rightData = m_RightInteraction.StagingRef.ContentStagingMetaDataRef;
            for (int left = 0; left < leftData.m_StagingData.Count; left ++)
            {
                for (int right = 0; right < rightData.m_StagingData.Count; right++)
                {
                    if(leftData.CurrentData.m_ConnectedTo == rightData.CurrentData)
                    {
                        // gone through the items to match the correct data
                        return;
                    }
                    rightData.WrapIncrementCurrentPoint();
                }
                leftData.WrapIncrementCurrentPoint();
            }
        }

        private bool AnyMousePresseed()
        {
            bool leftMouse = (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Left, PressType.Hold));
            bool rightMouse = (true == HouseBuilderConstants.IsMouseCommand(MouseCommand.Right, PressType.Hold));
            return leftMouse || rightMouse;
        }

        public override void CleanUp()
        {
            m_LeftInteraction = null;
            m_RightInteraction = null;
            HBGUIManager.Instance.StagingSettingsMenuRef.CollectAndDisplayVisuals(m_LeftInteraction, m_RightInteraction);
            m_UpdateInputControlsFor_Nothing = true;
            HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
        }

        protected override void UpdateInputControlsFor_Placing()
        {           
            var last = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.Last();
            if(m_LeftInteraction != null && m_RightInteraction != null)
            {
                //if both selected, and press to creat a new itom , it will move right to left 
                // do you can just keep pressing new images
                m_LeftInteraction = m_RightInteraction;
                m_RightInteraction = null;
            }
            MousePress(false, true, last.InteractionRef, addedNewPiece : true);
            ClearAllInteractions();

            HBGUIManager.Instance.StagingSettingsMenuRef.CollectAndDisplayVisuals(m_LeftInteraction, m_RightInteraction);
            if (m_LeftInteraction != null && m_RightInteraction != null)
            {
                HBGUIManager.Instance.StagingSettingsMenuRef.ConnectOnCreation(m_LeftInteraction, m_RightInteraction);
            }         
            
            CurrentInputMode = InputMode.Nothing;
        }

        /// <summary>
        /// goes though and find the bas ( root item ) 
        /// </summary>
        public static Interaction FindRootStagingItem(Interaction newInteraction, bool lookat)
        {
            Interaction start = newInteraction;
            if (newInteraction != null && newInteraction.StagingRef != null)
            {
                // find root item in StagingRef
                while (newInteraction != null && newInteraction.StagingRef != null)
                {
                    var test = newInteraction.RootGameObject.transform.parent.gameObject.GetComponentInParent<Interaction>(true);
                    if (test != null && test.StagingRef != null)
                    {
                        newInteraction = test;
                    }
                    else
                    {
                        break;
                    }
                }

                if (start != newInteraction && lookat == true)
                {
                    Camera.main.transform.gameObject.MoveToLookAtRealive(newInteraction.transform.gameObject);
                }
            }
            return newInteraction;
        }

    }
}
