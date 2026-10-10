using EPOOutline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ContentStagingMetaData;
using static ContentStagingMetaData.StagingData;

namespace HouseBuilder
{
    public class StagingSettingsMenu : MonoBehaviour
    {

        public static Dictionary<ConectionType, string> STAGE_START_ITEMS = new Dictionary<ConectionType, string>
        {
            { ConectionType.Ladder, "81410524-7454-4f9f-85d3-46146bb33131" },
            { ConectionType.Triangle, "12d552ce-edd9-4152-8263-d405a9854555" },
            { ConectionType.Square, "12d552ce-edd9-4152-8263-d405a9854555" },
            { ConectionType.Rectangle, "12d552ce-edd9-4152-8263-d405a9854555" },
        };


        public enum MoveType
        {
            Next, 
            Prev,
        }
        public enum ConnectType
        {
            PointChange,
            Rolling,
            Creation
        }


        private CanvasGroup m_Group;
        private bool m_IsGroupActive;

        private Image m_LeftBackGroundNotSelected;
        private Image m_LeftBackGroundSelected;
        private Button m_LeftNextButton;
        private Button m_LeftPrevButton;

        private Image m_RightBackGroundNotSelected;
        private Image m_RightBackGroundSelected;
        private Button m_RightNextButton;
        private Button m_RightPrevButton;
        private Button m_RightRotClock;
        private Button m_RightRotAnti;


        private Button m_NothingButton;
        private Button m_MoveButton;
        private Button m_PivotButton;

        private Button m_StartingStagingBasePrev;
        private Button m_StartingStagingBaseNext;

        private Dictionary<ConectionType, Button> m_BtnCreateStaging = new Dictionary<ConectionType, Button>();

        private List<StagingData> m_LeftEmptyStagingPoints = new List<StagingData>();
        private List<StagingData> m_RighEmptyStagingPoints = new List<StagingData>();

        private Staging m_LeftStaging;
        private Staging m_RightStaging;
        private Transform m_TransformGizmo = null;

        int m_StartingItemIndex = 0;

        private HouseBuilderInputStaging InputStagingRef;

        public void PostInitialise(CatalogueMenu catalogue) //Delayed because catalogue must be initialised before this
        {           
        }


        public void InitCreateStartButtons(Dictionary<ConectionType, Button> buttons)
        {
            m_BtnCreateStaging = buttons;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();



            // checks that STAGE_START_ITEMS are valid gui's
            foreach (var item in STAGE_START_ITEMS)
            {
                var entry = Core.Catalogue.GetEntry(item.Value);
                if (entry == null)
                {
                    Debug.LogError($"cannot find start item for: {item.Key}, looking for GUID: {item.Value}");
                }
            }

            InputStagingRef = HouseBuilderRoot.Instance.InputStaging;
            SetGuiButtonStates(null, null);
        }

       

        private CatalogueGeneratedData FindStartingItem(MoveType move)
        {
            List<CatalogueGeneratedData> foundItems = CollectAllStartingItems();

            if(move == MoveType.Next)
            {
                m_StartingItemIndex = m_StartingItemIndex.WrapIncrement(foundItems);
            }
            else
            {
                m_StartingItemIndex = m_StartingItemIndex.WrapDecrement(foundItems);
            }

            return foundItems[m_StartingItemIndex];
        }


        private int CollectAllStartingItemsCount => CollectAllStartingItems().Count;
        private List<CatalogueGeneratedData> CollectAllStartingItems()
        {
            List<CatalogueGeneratedData> foundItems = new List<CatalogueGeneratedData>();
            var allItems = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
            foreach (var item in allItems)
            {
                foreach (var stage in STAGE_START_ITEMS)
                {
                    if (item.CatalogueEntryRef.Guid == stage.Value)
                    {
                        if (foundItems.Contains(item) == false)
                        {
                            foundItems.Add(item);
                        }
                    }
                }
            }
            return foundItems;
        }

        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_IsGroupActive = false;

            m_LeftBackGroundNotSelected = this.transform.SearchComponent<Image>("LeftBackGroundNotSelected");
            m_LeftBackGroundSelected = this.transform.SearchComponent<Image>("LeftBackGroundSelected");


            m_StartingStagingBaseNext = this.transform.SearchComponent<Button>("StartPointNext");
            m_StartingStagingBaseNext.onClick.AddListener(() =>
            {
                var startingItem = FindStartingItem(MoveType.Next);
                m_LeftStaging = startingItem.InteractionRef.StagingRef;
                m_RightStaging = null;
                InputStagingRef.MousePress(true, true, null, false);
                InputStagingRef.MousePress(true, false, startingItem.InteractionRef, false);

                CollectAndDisplayVisuals(m_LeftStaging.Interaction, null);
                FindNewFreePoint(m_LeftStaging, m_RightStaging);
                Camera.main.transform.gameObject.MoveToLookAtRealive(startingItem.InteractionRef.RootGameObject);
                CollectAndDisplayVisuals();
            });

            m_StartingStagingBasePrev = this.transform.SearchComponent<Button>("StartPointPrev");
            m_StartingStagingBasePrev.onClick.AddListener(() =>
            {
                var startingItem = FindStartingItem(MoveType.Prev);
                m_LeftStaging = startingItem.InteractionRef.StagingRef;
                m_RightStaging = null;
                InputStagingRef.MousePress(true, true, null, false);
                InputStagingRef.MousePress(true, false, startingItem.InteractionRef, false);

                CollectAndDisplayVisuals(m_LeftStaging.Interaction, null);
                FindNewFreePoint(m_LeftStaging, m_RightStaging);
                Camera.main.transform.gameObject.MoveToLookAtRealive(startingItem.InteractionRef.RootGameObject);
                CollectAndDisplayVisuals();
            });


            m_NothingButton = this.transform.SearchComponent<Button>("Nothing");
            m_NothingButton.onClick.AddListener(() =>
            {
                HouseBuilderRoot.Instance.InputStaging.m_UpdateInputControlsFor_Nothing = true;
                m_TransformGizmo = null;
                HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
                CollectAndDisplayVisuals();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            m_MoveButton = this.transform.SearchComponent<Button>("Move");
            m_MoveButton.onClick.AddListener(() =>
            {
                HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
                if (m_LeftStaging != null)
                {
                    HouseBuilderRoot.Instance.InputStaging.m_UpdateInputControlsFor_Nothing = false;
                    if (m_TransformGizmo == null)
                    {
                        Interaction root = HouseBuilderInputStaging.FindRootStagingItem(m_LeftStaging.Interaction, lookat: false);
                        HouseBuilderRoot.Instance.TransformGizmoRef.AddTarget(root.RootTransform);
                        CollectAndDisplayVisuals();
                    }
                    HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Move;
                }
            });

            m_PivotButton = this.transform.SearchComponent<Button>("Pivot");
            m_PivotButton.onClick.AddListener(() =>
            {
                HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
                if (m_LeftStaging != null)
                {
                    HouseBuilderRoot.Instance.InputStaging.m_UpdateInputControlsFor_Nothing = false;
                    if (m_TransformGizmo == null)
                    {
                        Interaction root = HouseBuilderInputStaging.FindRootStagingItem(m_LeftStaging.Interaction, lookat: false);
                        HouseBuilderRoot.Instance.TransformGizmoRef.AddTarget(root.RootTransform);
                        CollectAndDisplayVisuals();
                    }
                    HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Rotate;
                }
            });


            m_LeftNextButton = this.transform.SearchComponent<Button>("LeftNext");
            m_LeftNextButton.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                FindPrevNextFreePoint(m_LeftStaging, MoveType.Next);
                Connect(m_LeftStaging, m_RightStaging, ConnectType.PointChange);
                CollectAndDisplayVisuals();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            m_LeftPrevButton = this.transform.SearchComponent<Button>("LeftPrev");
            m_LeftPrevButton.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                FindPrevNextFreePoint(m_LeftStaging, MoveType.Prev);
                Connect(m_LeftStaging, m_RightStaging, ConnectType.PointChange);
                CollectAndDisplayVisuals();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            /////////////////////////////////////////////////////////

            m_RightBackGroundNotSelected = this.transform.SearchComponent<Image>("RightBackGroundNotSelected");
            m_RightBackGroundSelected = this.transform.SearchComponent<Image>("RightBackGroundSelected");

            m_RightNextButton = this.transform.SearchComponent<Button>("RightNext");
            m_RightNextButton.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                FindPrevNextFreePoint(m_RightStaging, MoveType.Next);
                Connect(m_LeftStaging, m_RightStaging, ConnectType.PointChange);
                CollectAndDisplayVisuals();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            m_RightPrevButton = this.transform.SearchComponent<Button>("RightPrev");
            m_RightPrevButton.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                FindPrevNextFreePoint(m_RightStaging, MoveType.Prev);
                Connect(m_LeftStaging, m_RightStaging, ConnectType.PointChange);
                CollectAndDisplayVisuals();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            m_RightRotClock = this.transform.SearchComponent<Button>("RightRotClock");
            m_RightRotClock.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                m_RightStaging.ContentStagingMetaDataRef.WrapIncrementRotationIndex();
                Connect(m_LeftStaging, m_RightStaging, ConnectType.Rolling);
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            m_RightRotAnti = this.transform.SearchComponent<Button>("RightRotAnti");
            m_RightRotAnti.onClick.AddListener(() =>
            {
                CollectAndDisplayVisuals();
                m_RightStaging.ContentStagingMetaDataRef.WrapDecrementRotationIndex();
               Connect(m_LeftStaging, m_RightStaging, ConnectType.Rolling);
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            HouseBuilderRoot.Instance.OnObjectSelected += (state, data) =>
            {
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Staging)
                {
                    SetVisible(true, false);
                }
            };

            HouseBuilderRoot.Instance.OnModeTransitionBegin += () => SetVisible(false, true);
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;           
        }


        private void FindNewFreePoint(Staging leftStaging, Staging rightStaging)
        {
            if (leftStaging == null || rightStaging == null)
            {
                return;
            }

            var leftMetaData = leftStaging.ContentStagingMetaDataRef;
            StagingData left = leftMetaData.CurrentData;

            int count = 0;
            while (left.m_ConnectedTo != null)
            {
                leftMetaData.WrapIncrementCurrentPoint();
                left = leftMetaData.CurrentData;
                count++;
                if (count > 100)
                {
                    Debug.LogError("Something gone wrong, this should never be hit");
                    break;
                }
            }
        }

        private void FindPrevNextFreePoint(Staging staging, MoveType moveType)
        {
            if(staging == null)
            {
                return;
            }

            // make the current one null, so there are not duplicate linkages
            var metaData = staging.ContentStagingMetaDataRef;
            var other = metaData.CurrentData.m_ConnectedTo;

            metaData.CurrentData.m_ConnectedTo = null;
            if (other != null)
            {
                other.m_ConnectedTo = null;
            }


            for (int i = 0; i < 100; i++)
            {
                CollectAndDisplayVisuals();

                metaData = staging.ContentStagingMetaDataRef;
                switch (moveType)
                {
                    case MoveType.Next:
                        metaData.WrapIncrementCurrentPoint();
                        break;
                    case MoveType.Prev:
                        metaData.WrapDecrementCurrentPoint();
                        break;
                    default:
                        break;
                }
  

                StagingData data = metaData.CurrentData;


                // if its free 
                if (data.m_ConnectedTo == null)
                {
                    return;
                }

            }
            Debug.LogError("Something gone wrong, this should never be hit");
        }

        public void CollectAndDisplayVisuals(Interaction left, Interaction right)
        {
            CollectEmptyStagingPoints();
            SetGuiButtonStates(left, right);
        }

        private void CollectAndDisplayVisuals()
        {
            CollectEmptyStagingPoints();
            SetGuiButtonStates(m_LeftStaging?.Interaction, m_RightStaging?.Interaction);
        }

        private void CollectEmptyStagingPoints()
        {
            m_LeftEmptyStagingPoints = new List<StagingData>();
            m_RighEmptyStagingPoints = new List<StagingData>();

            if (m_LeftStaging != null)
            {
                foreach (var item in m_LeftStaging.ContentStagingMetaDataRef.m_StagingData)
                {
                    if (item.m_ConnectedTo == null)
                    {
                        m_LeftEmptyStagingPoints.Add(item);
                    }
                }
            }

            if (m_RightStaging != null)
            {
                foreach (var item in m_RightStaging.ContentStagingMetaDataRef.m_StagingData)
                {
                    if (item.m_ConnectedTo == null)
                    {
                        m_RighEmptyStagingPoints.Add(item);
                    }
                }
            }

            m_LeftEmptyStagingPoints = m_LeftEmptyStagingPoints.Distinct().ToList();
            m_RighEmptyStagingPoints = m_RighEmptyStagingPoints.Distinct().ToList();
        }

        private void SetGuiButtonStates(Interaction left, Interaction right)
        {
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.m_Outline.SetOutLine(false));

            var initEffect = EPOOutline.Outliner.Instance;

            m_LeftBackGroundSelected.SetActive(left != null);
            m_LeftBackGroundSelected.color = initEffect.GetLineColor(EPOOutline.Outliner.EnumItemSelected.Valid) * new Color(1, 1, 1, 0.5f);
            m_LeftBackGroundNotSelected.SetActive(left == null);

            m_RightBackGroundSelected.SetActive(right != null);
            m_RightBackGroundSelected.color = initEffect.GetLineColor(EPOOutline.Outliner.EnumItemSelected.Selected) * new Color(1, 1, 1, 0.5f);
            m_RightBackGroundNotSelected.SetActive(right == null);


            if (left != null)
            {
                left.m_Outline.SetOutLine(true);
                left.m_Outline.ItemSelected = Outliner.EnumItemSelected.Valid;
            }

            if (right != null)
            {
                right.m_Outline.SetOutLine(true);
                right.m_Outline.ItemSelected = Outliner.EnumItemSelected.Selected;
            }

            m_StartingStagingBaseNext.interactable = CollectAllStartingItemsCount != 0;
            m_StartingStagingBasePrev.interactable = m_StartingStagingBaseNext.interactable;

            // if data is valid and more than 2 posible points, and there are some points free
            if (m_LeftStaging != null)
            {
                m_LeftNextButton.interactable = (left != null && m_LeftStaging.ContentStagingMetaDataRef.StagingDataCount > 2 && m_LeftEmptyStagingPoints.Count > 0);
                m_LeftPrevButton.interactable = m_LeftNextButton.interactable;
            }
            else
            {
                m_LeftNextButton.interactable = (left != null && m_LeftEmptyStagingPoints.Count > 1);
                m_LeftPrevButton.interactable = m_LeftNextButton.interactable;
                HouseBuilderRoot.Instance.TransformGizmoRef.ClearTargets();
            }

            if (m_RightStaging != null)
            {
                m_RightNextButton.interactable = (right != null && m_RightStaging.ContentStagingMetaDataRef.StagingDataCount > 2 && m_RighEmptyStagingPoints.Count > 0);
                m_RightPrevButton.interactable = m_RightNextButton.interactable;
            }
            else
            {
                m_RightNextButton.interactable = (right != null && m_RighEmptyStagingPoints.Count > 1);
                m_RightPrevButton.interactable = m_RightNextButton.interactable;
            }


            m_RightRotAnti.interactable = (right != null);
            m_RightRotClock.interactable = (right != null);


            if (HouseBuilderRoot.Instance.TransformGizmoRef.HasTarget() == true)
            {
                m_NothingButton.interactable = true;
                m_MoveButton.interactable = true;
                m_PivotButton.interactable = true;
            }
            else
            {
                m_NothingButton.interactable = false;
                m_MoveButton.interactable = true;
                m_PivotButton.interactable = true;
            }

            // over rides everything 
            if (m_LeftStaging == null)
            {
                m_LeftNextButton.interactable = false;
                m_LeftPrevButton.interactable = false;

                m_NothingButton.interactable = false;
                m_MoveButton.interactable = false;
                m_PivotButton.interactable = false;
            }

            foreach(var item in m_BtnCreateStaging)
            {
                item.Value.interactable = m_LeftStaging == null;
            }


            CatalogueItemsInteractible(HasAnyFreePonts(m_LeftStaging, m_RightStaging));
        }

        private void CatalogueItemsInteractible(bool interactable)
        {
            if (HBGUIManager.Instance != null && HBGUIManager.Instance.CatalogueMenuRef != null)
            {
                foreach (var item in HBGUIManager.Instance.CatalogueMenuRef.AllMenuObjects)
                {
                    if (item.m_Button != null)
                    {
                        item.m_Button.interactable = interactable;
                    }
                }
            }
        }


        private bool HasAnyFreePonts(Staging leftStaging, Staging rightStaging)
        {
            if (leftStaging == null && rightStaging == null)
            {
                return false;
            }

            if(rightStaging != null)
            {
                var rightMetaData = rightStaging.ContentStagingMetaDataRef;
                foreach(var right in rightMetaData.m_StagingData)
                {
                    if(right.m_ConnectedTo == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            if (leftStaging != null)
            {
                var leftMetaData = leftStaging.ContentStagingMetaDataRef;
                foreach (var left in leftMetaData.m_StagingData)
                {
                    if (left.m_ConnectedTo == null)
                    {
                        return true;
                    }
                }
                return false;
            }

            return true;
        }


        public void SetVisible(bool state, bool manual)
        {
            if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Staging)
            {
                state = false;
            }

            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }

        public void SetVisible(bool state)
        {
            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }

        public void ConnectOnCreation(Interaction left, Interaction right)
        {
            m_LeftStaging = left.StagingRef;
            m_RightStaging = right.StagingRef;

            CollectAndDisplayVisuals();
            FindNewFreePoint(m_LeftStaging, m_RightStaging);
            Connect(m_LeftStaging, m_RightStaging, ConnectType.Creation);
            CollectAndDisplayVisuals();
            right.RootTransform.SetParent(left.RootTransform); // rotation goes wrong when connected
        }

        public void ConnectOnReselection(Staging leftStaging, Staging rightStaging)
        {
            m_LeftStaging = leftStaging;
            m_RightStaging = rightStaging;

            CollectAndDisplayVisuals();
            //not needed  FindPointExisting(m_LeftStaging, m_RightStaging);
            //not needed  Connect(m_LeftStaging, m_RightStaging);
            CollectAndDisplayVisuals();
        }


        public void Connect(Staging leftStaging, Staging rightStaging, ConnectType connectType)
        {
            m_LeftStaging = leftStaging;
            m_RightStaging = rightStaging;

            if (leftStaging ==  null || rightStaging ==  null)
            {
                Debug.LogError("there is a null, this should never be hit");
                return;
            }

            // connect link
            if (connectType == ConnectType.Creation || connectType == ConnectType.PointChange)
            {
                leftStaging.ContentStagingMetaDataRef.CurrentData.m_ConnectedTo = rightStaging.ContentStagingMetaDataRef.CurrentData;
                rightStaging.ContentStagingMetaDataRef.CurrentData.m_ConnectedTo = leftStaging.ContentStagingMetaDataRef.CurrentData;
            }

            StagingData leftStagingData = leftStaging.ContentStagingMetaDataRef.CurrentData;
            StagingData rightStagingData = rightStaging.ContentStagingMetaDataRef.CurrentData;

            int rotationIndex = rightStagingData.m_RotationIndex;
            ConectionType connectionType = rightStaging.ContentStagingMetaDataRef.m_ConectionType;

            GameObject leftGameObject  = leftStagingData.m_ModelGameObject;
            GameObject rightGameObject = rightStagingData.m_ModelGameObject;

            // reparent to the point is controlling the model 
            Transform oldParent = rightStaging.Interaction.RootTransform.parent;
            int oldSiblingIndex = rightStaging.Interaction.RootTransform.GetSiblingIndex();
            rightGameObject.transform.transform.SetParent(null);
            rightStaging.Interaction.RootTransform.SetParent(rightGameObject.transform);
            rightGameObject.transform.position = leftGameObject.transform.position;
            rightGameObject.transform.SetParent(rightStagingData.m_OriginalParent);

            rightGameObject.transform.rotation = leftGameObject.transform.rotation;
  
            float angle = 0f;
            switch (connectionType)
            {
                case ConectionType.Ladder:
                case ConectionType.Rectangle:
                    if (rotationIndex == 0)
                    {
                        angle = 0f;
                    }
                    else
                    {
                        angle = 180f;
                    }
                    break;
                case ConectionType.Triangle:
                    if (rotationIndex == 0)
                    {
                        angle = 0f;
                    }
                    else if (rotationIndex == 1)
                    {
                        angle = 120f;
                    }
                    else
                    {
                        angle = 240;
                    }
                    break;
                case ConectionType.Square:
                    if (rotationIndex == 0)
                    {
                        angle = 0f;
                    }
                    else if (rotationIndex == 1)
                    {
                        angle = 90f;
                    }
                    else if(rotationIndex == 2)
                    {
                        angle = 180;
                    }
                    else
                    {
                        angle = 270f;
                    }
                    break;
                default:
                    break;
            }

            rightGameObject.transform.localRotation *= Quaternion.Euler(0f, 180, angle);

            // revert back to correct parent and transform data
            leftGameObject.transform.localPosition = leftStagingData.m_OriginalPosition;
            leftGameObject.transform.localRotation = leftStagingData.m_OriginalQuaternion;

            rightStaging.Interaction.RootTransform.SetParent(oldParent);
            rightStaging.Interaction.RootTransform.SetSiblingIndex(oldSiblingIndex);
            rightGameObject.transform.SetParent(rightStagingData.m_OriginalParent);

        }

    }
}