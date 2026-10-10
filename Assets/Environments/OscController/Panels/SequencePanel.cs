using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{
    public class SequencePanel : MonoBehaviour
    {
        private const string ITEM_PREFAB = "SequenceItemPrefab";
        private const string ITEM_FILTER_PREFAB = "SequenceItemFilterGroupPrefab";

        private Button m_AddGroupingHeader;

        private Button m_AddSequence;
        private Button m_RemoveSequence;
        private Button m_DuplicateSequence;
        private Button m_Macros;

        private VerticalLayoutGroup m_VerticalLayoutGroup;
        private ScrollRect m_VerticalScrollView;
        private ToggleGroup m_ToggleGroup;

        private int m_SelectedSceneIndex = 0;

        private const float MIN_TIME = 0.1f;
        private const float MAX_TIME = 1000f;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            this.transform.GetComponent<Image>().raycastTarget = false;
            m_ToggleGroup = this.GetComponentInChildren<ToggleGroup>();
            m_ToggleGroup.allowSwitchOff = true;
            m_VerticalLayoutGroup = this.GetComponentInChildren<VerticalLayoutGroup>();
            m_VerticalScrollView = this.transform.SearchComponent<ScrollRect>("Scroll View");

            m_AddGroupingHeader = this.transform.SearchComponent<Button>("AddGroupingHeader");
            m_AddGroupingHeader.onClick.AddListener(() =>
            {

                
                int count = 0;
                while (true)
                {
                    string TEMP_HEADER = $"TEMP_HEADER_{count}";
                    if (DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames.Find(e => e.m_GroupName == TEMP_HEADER) == null)
                    {
                        DataContainer.FilterGroup newGroup = new DataContainer.FilterGroup();
                        newGroup.m_GroupName = TEMP_HEADER;

                        DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames.Add(newGroup);
                        DeleteAndReCreateAll();
                        m_VerticalScrollView.verticalScrollbar.value = 0; // this is the bottom
                        break;
                    }
                    count++;
                    if(count > 10000)
                    {
                        Debug.LogError("Something gone very wrong, check code as its broken ");
                        break;
                    }
                }
    
            });


            m_AddSequence = this.transform.SearchComponent<Button>("AddSequence");
            m_AddSequence.onClick.AddListener(() =>
            {
                CreateNewScene();
                CheckInteractableStates();
            });

            m_RemoveSequence = this.transform.SearchComponent<Button>("RemoveSequence");
            m_RemoveSequence.onClick.AddListener(() =>
            {
                DeleteScene(m_SelectedSceneIndex);

                if (DataContainer.Instance.SequenceDataRef.Count > 0)
                {
                    m_SelectedSceneIndex = m_SelectedSceneIndex.ClampDecrement(m_SelectedSceneIndex);               
                    if (DataContainer.Instance.SequenceDataRef.Count > 0)
                    {
                        DataContainer.Instance.CurrentSequenceData = DataContainer.Instance.SequenceDataRef[m_SelectedSceneIndex];
                    }
                    else
                    {
                        DataContainer.Instance.CurrentSequenceData = null;
                    }
                }
                CheckInteractableStates();
                OscControllerRoot.Instance.ReInitialiseAll();
            });

            m_DuplicateSequence = this.transform.SearchComponent<Button>("Duplicate");
            m_DuplicateSequence.onClick.AddListener(() =>
            {
                DuplicateScene();
            });


            m_Macros = this.transform.SearchComponent<Button>("Macros");
            m_Macros.onClick.AddListener(() =>
            {
                OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(!OscControllerRoot.Instance.MacroPanelRef.IsActive());
            });
        }

        public void SetButtonStates(bool isOn)
        {
            m_AddSequence.SetActive(isOn);
            m_RemoveSequence.SetActive(isOn);
            m_DuplicateSequence.SetActive(isOn);
            m_AddGroupingHeader.SetActive(isOn);
            DeleteAndReCreateAll();
        }

        private void DuplicateScene()
        {
            int originalSceneIndex = m_SelectedSceneIndex;
            CreateNewScene();
            int duplicateSceneIndex = m_SelectedSceneIndex;


            OscSequenceData duplicateScene = DataContainer.Instance.SequenceDataRef[duplicateSceneIndex];
            OscSequenceData originalScene = DataContainer.Instance.SequenceDataRef[originalSceneIndex];

            duplicateScene.m_DurationTime = originalScene.m_DurationTime;
            duplicateScene.m_DurationInput.text = duplicateScene.m_DurationTime.ToString("0.00");

            duplicateScene.m_LerpTime = originalScene.m_LerpTime;
            duplicateScene.m_LerpInput.text = duplicateScene.m_LerpTime.ToString("0.00");

            duplicateScene.m_SequenceName = originalScene.m_SequenceName;
            duplicateScene.m_SequenceName += "Duplicate";
            duplicateScene.m_InputFieldSequenceName.text = duplicateScene.m_SequenceName;

            duplicateScene.m_OptionsType = originalScene.m_OptionsType;
            duplicateScene.m_OptionsName = originalScene.m_OptionsName;
            duplicateScene.m_LerpTime = originalScene.m_LerpTime;
            duplicateScene.m_TxtOptions.text = originalScene.m_OptionsName;

            string data = Json.JsonNet.WriteToText<OscSequenceData>(DataContainer.Instance.SequenceDataRef[originalSceneIndex], true);
            OscSequenceData duplicateData = Json.JsonNet.ReadFromText<OscSequenceData>(data);

            DataContainer.Instance.SequenceDataRef[duplicateSceneIndex].m_SliderData = duplicateData.m_SliderData.Clone();

            List<FixtureSliderData> duplicateSliderData = DataContainer.Instance.SequenceDataRef[duplicateSceneIndex].m_SliderData;
            List<FixtureSliderData> originalSliderData = DataContainer.Instance.SequenceDataRef[originalSceneIndex].m_SliderData;

            for (int i = 0; i < originalSliderData.Count; i++)
            {
                duplicateSliderData[i].m_FixtureData = originalSliderData[i].m_FixtureData;
            }

            CheckInteractableStates();
            DataContainer.Instance.CurrentSequenceData = DataContainer.Instance.SequenceDataRef[m_SelectedSceneIndex];
        }

        public void DeleteScene(int sceneIndex)
        {
            var currentScene = DataContainer.Instance.SequenceDataRef[sceneIndex];
            DataContainer.Instance.SequenceDataRef.Remove(currentScene);
            Destroy(currentScene.m_Prefab);
            currentScene = null;
        }


        private void DeleteAndReCreateAll()
        {
            m_OscSequenceDataList.Clear();
            DeleteAll();
            int headerCount = 0;
            int sequenceCount = 0;
            DataContainer.Instance.GenericDataDataRef.ValidateStart();

            foreach (var filter in DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames)
            {
                AddGroupHeader(filter, headerCount);
                headerCount++;
                foreach (var item in DataContainer.Instance.SequenceDataRef)
                {

                    if (filter.m_GroupName == item.m_FilterGroup)
                    {
                        AddNewSequence(item);
                        sequenceCount++;
                        item.m_InputFieldSequenceName.SetTextWithoutNotify(item.m_SequenceName);
                        if(filter.m_IsOpen == false)
                        {
                            item.m_Prefab.SetActive(false);
                        }
                    }
                }
            }
        }

        private OscSequenceData CreateNewScene()
        {
            OscSequenceData newData = new OscSequenceData();
            newData.m_SequenceName = $"NewSequence {DataContainer.Instance.SequenceDataRef.Count}";
            DataContainer.Instance.SequenceDataRef.Add(newData);
            DeleteAndReCreateAll();

            TurnOnSelf(newData);
            return newData;
        }

        public void CheckInteractableStates()
        {
            m_RemoveSequence.interactable = (DataContainer.Instance.SequenceDataRef.Count > 0);
            m_DuplicateSequence.interactable = (DataContainer.Instance.SequenceDataRef.Count > 0);
        }

        private void AddGroupHeader(DataContainer.FilterGroup group, int index)
        {
            string localName = group.m_GroupName;
            int localIndex = index;
            GameObject header = CachedObjects.Instance.InstantiateCachedGameObject(ITEM_FILTER_PREFAB, m_VerticalLayoutGroup.transform);
            var inputField = header.transform.SearchComponent<InputField>("InputField");

            inputField.text = localName;

            inputField.onEndEdit.AddListener((newName) =>
            {           
                if( DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames.Find(e => e.m_GroupName == newName) == null)
                {
                    DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames[localIndex].m_GroupName = newName;
                    foreach(var item in DataContainer.Instance.SequenceDataRef)
                    {
                        if(item.m_FilterGroup == localName)
                        {
                            item.m_FilterGroup = newName;
                        }
                    }

                    DeleteAndReCreateAll();
                }
            });

            Button button = header.transform.SearchComponent<Button>("Remove");
            button.gameObject.SetActive(localName != OscControllerRoot.OPTIONS_ALL);
            inputField.readOnly = (localName == OscControllerRoot.OPTIONS_ALL);

            button.onClick.AddListener(() =>
            {
                if (DataContainer.Instance.SequenceDataRef.FindAll(e => e.m_FilterGroup == localName).Count != 0)
                {
                    OscGenericPromptMenu.Instance.ShowOkayMenu($"There are still items connected to the header", null);
                }
                else
                {
                    OscGenericPromptMenu.Instance.ShowYesNoMenu($"Remove {name}", () =>
                    {
                        Debug.LogError($"Remove {localName}");
                        DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames.RemoveAll(e => e.m_GroupName == (localName));
                        DeleteAndReCreateAll();
                    });
                }
            });

            if(name == null)
            {
                // bassicly clear the render parts , so there is a space at the bottom 
                header.GetComponent<Image>().enabled = false;
                inputField.gameObject.SetActive(false);
                button.gameObject.SetActive(false);
            }

            Button arrowButton = header.transform.SearchComponent<Button>("ArrowButtons");

            if(group.m_IsOpen == true)
            {
                arrowButton.transform.SearchComponent<Image>("ArrowRight").SetActive(false);
            }
            else
            {
                arrowButton.transform.SearchComponent<Image>("ArrowDown").SetActive(false);
            }
            arrowButton.onClick.AddListener(() =>
            {
                group.m_IsOpen = !group.m_IsOpen;
                DeleteAndReCreateAll();
            });
        }

        List<OscSequenceData> m_OscSequenceDataList = new List<OscSequenceData>();
        private void AddNewSequence(OscSequenceData newData)
        {
            int currentIndex = m_OscSequenceDataList.Count;
            OscSequenceData localNewData = newData;
            localNewData.m_Prefab = CachedObjects.Instance.InstantiateCachedGameObject(ITEM_PREFAB, m_VerticalLayoutGroup.transform);
            localNewData.m_Toggle = newData.m_Prefab.GetComponent<Toggle>();
            localNewData.m_Toggle.group = m_ToggleGroup;
            localNewData.m_BaseImage = newData.m_Prefab.GetComponent<Image>();

            localNewData.m_DurationInput = newData.m_Prefab.transform.SearchComponent<InputField>("InputTime");
            localNewData.m_DurationInput.text = newData.m_DurationTime.ToString("0.00");

            localNewData.m_LerpInput = newData.m_Prefab.transform.SearchComponent<InputField>("LerpTime");
            localNewData.m_LerpInput.text = newData.m_LerpTime.ToString("0.00");


            localNewData.m_TxtOptions = newData.m_Prefab.transform.SearchComponent<Text>("OptionInformation");
            localNewData.m_TxtOptionsAllOrSingle = newData.m_Prefab.transform.SearchComponent<Text>("OptionInformationAllSingleText");

            localNewData.m_BtnOptions = newData.m_Prefab.transform.SearchComponent<Button>("Option");

            localNewData.m_BtnReset = newData.m_Prefab.transform.SearchComponent<Button>("Reset");
            localNewData.m_BtnPlay = newData.m_Prefab.transform.SearchComponent<Button>("Play");
            localNewData.m_InputFieldSequenceName = newData.m_Prefab.transform.SearchComponent<InputField>("SequenceName");

            localNewData.m_ImgDrag = newData.m_Prefab.transform.SearchComponent<Image>("DragImage");


            // only allow items to be moved around when in Sequence mode
            if (OscControllerRoot.Instance.MenuPanelRef.Mode == MenuPanel.ModeEnum.Sequence)
            {
                var drag = localNewData.m_ImgDrag.gameObject.ForceComponent<DragGroupController>();
                drag.Initilise(DragGroupController.DragType.Vertical, localNewData.m_Prefab, (oldIndex, newIndex) =>
                {
                // change localNewData.m_FilterGroup to the new group its in 
                var children = m_VerticalLayoutGroup.transform.GetDirectChildren();
                    for (int i = newIndex; i >= 0; i--) // go up the list 
                {
                        if (children[i].name.StartsWith(ITEM_FILTER_PREFAB) == true)
                        {
                            localNewData.m_FilterGroup = children[i].transform.SearchComponent<InputField>("InputField").text;
                            break;
                        }
                    }


                // reorder the DataContainer.Instance.SequenceDataRef to the new order displayed
                var sequence = DataContainer.Instance.SequenceDataRef;
                    int currentIndex = sequence.FindIndex(e => (e == localNewData));
                    if (newIndex == 0)
                    {
                        sequence.Move(currentIndex, 0);
                    }
                    else
                    {
                        for (int i = newIndex - 1; i >= 0; i--) // go up the list 
                    {
                            if (children[i].name.StartsWith(ITEM_PREFAB) == true)
                            {
                                int aboveItemIndex = sequence.FindIndex(e => (e.m_Prefab == children[i].gameObject));
                                if (aboveItemIndex != -1)
                                {
                                    sequence.Move(currentIndex, aboveItemIndex);
                                    break;
                                }
                            }
                        }
                    }
                }, 1);
            }
            else
            {
                localNewData.m_ImgDrag.SetActive(false); // remove the tripple lines so not look like can move it 
            }

            localNewData.m_Prefab.SetActive(true);

            ColorBlock col = localNewData.m_Toggle.colors;
            col.selectedColor = OscControllerRoot.SELECTED;
            localNewData.m_Toggle.colors = col;


            m_RemoveSequence.interactable = (DataContainer.Instance.SequenceDataRef.Count > 0);


            var fake = localNewData.m_Prefab.ForceComponent<FakeButton>();
            fake.onClick.AddListener(() =>
            {
                TurnOnSelf(localNewData);
            });

            var nameSelect = localNewData.m_InputFieldSequenceName.gameObject.ForceComponent<FakeButton>();
            nameSelect.onClick.AddListener(() =>
            {
                TurnOnSelf(localNewData);
            });

            localNewData.m_InputFieldSequenceName.onValueChanged.AddListener((updateName) =>
            {
                TurnOnSelf(localNewData);
                localNewData.m_SequenceName = updateName;
                OscControllerRoot.Instance.TimeLinePanelRef.UpdateSceneNames();
            });

            localNewData.m_BtnReset.onClick.AddListener(() =>
            {
                TurnOnSelf(localNewData);
                OscControllerRoot.Instance.PlayDataRef.ResetSequence();
            });

            localNewData.m_BtnPlay.onClick.AddListener(() =>
            {
                TurnOnSelf(localNewData);
                OscControllerRoot.Instance.PlayDataRef.PlayCurrentSequence((time) =>
                {
                   OscControllerRoot.Instance.CurveEditorPanelRef.SetSliderNoCallback(time);
                });
            });

            localNewData.m_DurationInput.onEndEdit.AddListener((newInput) =>
            {
                float floatValue = float.Parse(newInput);
                floatValue = Mathf.Clamp(floatValue, MIN_TIME, MAX_TIME);
                localNewData.m_DurationInput.SetTextWithoutNotify(floatValue.ToString("0.00"));
                localNewData.m_DurationTime = floatValue;
                TurnOnSelf(localNewData);

                OscControllerRoot.Instance.TimeLinePanelRef.ReCalcuateAllStartTimesAndSizes();
                OscControllerRoot.Instance.CurveEditorPanelRef.RefreshAllCurves();
                OscControllerRoot.Instance.TimeLinePanelRef.UpdateSceneNames();
            });

            localNewData.m_LerpInput.onEndEdit.AddListener((newInput) =>
            {
                TurnOnSelf(localNewData);
                if (float.TryParse(newInput, out float amount) == true)
                {
                    localNewData.m_LerpTime = Mathf.Abs(amount);
                    OscControllerRoot.Instance.TimeLinePanelRef.ReCalcuateAllStartTimesAndSizes();
                    OscControllerRoot.Instance.CurveEditorPanelRef.RefreshAllCurves();
                }
            });

            localNewData.m_BtnOptions.onClick.AddListener(() =>
            {
               OscControllerRoot.Instance.SequencePanelOptionsRef.SetActive(true);
                OscControllerRoot.Instance.SequencePanelOptionsRef.FillInData(localNewData, () =>
                {
                    OnOptionsClosed();
                    TurnOnSelf(m_OscSequenceDataList[currentIndex]);
                });
            });

            localNewData.m_TxtOptions.text = localNewData.m_OptionsName;
            localNewData.m_TxtOptionsAllOrSingle.text = localNewData.m_OptionsType.First().ToString();
            m_OscSequenceDataList.Add(localNewData);
        }

        private void OnOptionsClosed()
        {           
            foreach(var item in DataContainer.Instance.SequenceDataRef)
            {
                if (DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames.FindAll(e => e.m_GroupName == item.m_FilterGroup) == null)
                {
                    Debug.LogError($"item.m_FilterGroup {item.m_FilterGroup} no longer exists, force to {OscControllerRoot.OPTIONS_ALL}");
                    item.m_FilterGroup = OscControllerRoot.OPTIONS_ALL;
                }
            }
            DeleteAndReCreateAll();
            DataContainer.Instance.ForceReselectCurrentSceneData();
        }

        public void SelectFirstItem()
        {
            var firstData = DataContainer.Instance.SequenceDataRef.First();
            if(firstData != null)
            {
                TurnOnSelf(firstData);
            }          
        }


        public void TurnOnSelf(OscSequenceData newData, bool sendMessage = true)
        {
            if (sendMessage == true)
            {
                SendMessageToSelectModel(newData);
            }

            for (int i = 0; i < DataContainer.Instance.SequenceDataRef.Count; i++)
            {
                var item = DataContainer.Instance.SequenceDataRef[i];
                if(item.m_BaseImage != null)
                {
                    item.m_BaseImage.color = OscControllerRoot.UN_SELECTED;
                }
            }

            newData.m_BaseImage.color = OscControllerRoot.SELECTED;

            m_SelectedSceneIndex = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == newData);
            DataContainer.Instance.CurrentSequenceData = DataContainer.Instance.SequenceDataRef[m_SelectedSceneIndex];
            ///Debug.LogError("OscControllerRoot.Instance.SliderPanelRef.SetCorrectSize();");
            // this broke it , need to figure out why OscControllerRoot.Instance.SliderPanelRef.SetCorrectSize();
        }

        private void SendMessageToSelectModel(OscSequenceData oscSceneData)
        {
            if (oscSceneData.m_OptionsName != OscControllerRoot.OPTIONS_ALL)
            {
                Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_SELECT_FEATURE_MESSAGE, oscSceneData.m_OptionsName);
            }
            else
            {
                Core.PhotonGenericRef.SendGenericHeader(OscControllerRoot.GENERIC_SELECT_ALL_DMX_MESSAGE);
            }
        }


        public void PlayBackVisual(int sceneIndex)
        {
            DataContainer.Instance.SequenceDataRef.ForEach(e => e.m_BaseImage.color = OscControllerRoot.UN_SELECTED);
            DataContainer.Instance.SequenceDataRef[sceneIndex].m_BaseImage.color = OscControllerRoot.SELECTED;
        }


        public void SetVisulsBackAfterPlayBack() => PlayBackVisual(m_SelectedSceneIndex);

        public void DeleteAll()
        {
            var children = m_VerticalLayoutGroup.transform.GetDirectChildren();
            foreach(var item in children)
            {
                Destroy(item.gameObject);
            }
        }


        public void InitialiseLoadedItems()
        {
            DeleteAndReCreateAll();


            if (DataContainer.Instance.SequenceDataRef.Count > 0)
            {
                TurnOnSelf(DataContainer.Instance.SequenceDataRef.First());
            }

            CheckInteractableStates();
        }

    }

}


