using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class LoadSaveMenuManager : MonoBehaviour
    {

        public static string EXAMPLES = "RoomBuilderExamples";

        private CanvasGroup m_CanvasGroup;
        private Toggle m_FolderTgl;
        private Button m_SaveButton;
        private Button m_LoadButton;
        private Button m_NewButton;
        private Button m_DeleteButton;
        protected TMP_InputField m_InputField;
        private Image m_InputFieldBackground;
        private Transform m_ScrollViewContent;
        private Scrollbar  m_ViewScrollbar;
        private ToggleGroup m_ToggleGroup; 

        private List<string> m_SavedFileNameList = new List<string>();

        private const string STRING_NULL = "";
        protected string m_SelectedFileName;
        protected string m_CurrentFileName;
        public string CurrentFileName => m_CurrentFileName;


        private Color RED_COLOR = new Color(1,0,0,1);
        private Color GREEN_COLOR = new Color(0, 1, 0, 1);
        private Color WHITE_COLOR = new Color(1, 1, 1, 1);

        public bool IsVisible { get; protected set; }

        protected PoolManagerLocalComponent<Transform> m_ItemPoolManager;


        protected virtual string NEW_FILENAME => "New Plot";
        protected virtual string CONTENTS_JSON => HouseBuilderConstants.CONTENTS_JSON;

        public void Initialise(GameObject savedPlotPrefab, Toggle folderToggle)
        {
            this.SetActive(true);
            m_FolderTgl = folderToggle;
            InitialiseFileNameList(null);
            SetUpMenuComponents(savedPlotPrefab);
            m_ItemPoolManager = new PoolManagerLocalComponent<Transform>(savedPlotPrefab, this.transform);
            Toggle(false);
            RefreshButtonList(null);
            ActivateLoadCancelButtons(false);
            SetCurrentFileName(NEW_FILENAME);           
        }

        private void SetUpMenuComponents(GameObject savedPlotPrefab)
        {
            m_CanvasGroup = transform.GetComponent<CanvasGroup>();
            m_SaveButton = transform.Find("BtnSave").GetComponent<Button>();
            m_LoadButton = transform.Find("BtnLoad").GetComponent<Button>();
            m_NewButton = transform.Find("BtnNew").GetComponent<Button>();
            m_DeleteButton = transform.Find("BtnDelete").GetComponent<Button>();
            m_InputField = transform.Find("InputField").GetComponent<TMP_InputField>();
            m_InputFieldBackground = m_InputField.GetComponent<Image>();
            m_ScrollViewContent = transform.Find("Scroll View/Viewport/Content");
            m_ViewScrollbar = transform.Find("Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>();
            m_ToggleGroup = m_ScrollViewContent.GetComponent<ToggleGroup>();

            m_SaveButton.onClick.AddListener(SaveButtonPressed);
            m_LoadButton.onClick.AddListener(LoadButtonPressed);
            m_NewButton.onClick.AddListener(NewButtonPressed);
            m_DeleteButton.onClick.AddListener(DeleteButtonPressed);
            m_SelectedFileName = null;

            m_CanvasGroup.FixBrokenMasks();
        }

        public string GetCurrentSceneName() => GetInputFieldText();

        protected virtual void SaveButtonPressed()
        {
            if (Core.Network.Users.CurrentUser != null)
            {
                string fileName = GetInputFieldText();
                if (string.IsNullOrEmpty(fileName) == false)
                {

                    if (IsFileNameAlreadyUsed(fileName))
                    {
                        GenericPromptMenu.Instance.ShowYesNoMenu("Do you want to overwrite existing file", () => SaveFileName(fileName, true), null);
                    }
                    else
                    {
                        SaveFileName(fileName, false);
                    }
                }
                else
                {
                    GenericPromptMenu.Instance.ShowOkayMenu("You need a validSaveName", null);
                }
            }
            else
            {
                GenericPromptMenu.Instance.ShowOkayMenu("Please log in before saving your scene.");
            }
        }

        protected virtual void SaveFileName(string fileName, bool overwrite)
        {
            if (overwrite)
            {
                HouseBuilderRoot.Instance.DeleteFiles(fileName, () =>
                {
                    Save(fileName);
                });
            }
            else
            {
                Save(fileName);
            }           
        }

        protected virtual void Save(string fileName)
        {
            HouseBuilderRoot.Instance.Save(fileName, LoadSaveType.Network, () =>
            {
                FileNameListManualRefresh(() =>
                {
                    Core.Audio.LoadSaveGeneratedSoundRef.PromptUploadGeneratedSound();
                    ShowSuccessFeedback();
                    SetCurrentFileName(fileName);
                    ResetInterface();
                    DisableAllToggle();
                    Toggle(false);
                });
            });
        }


        protected virtual void LoadButtonPressed()
        {
            if (m_SelectedFileName != null)
            {
                AssetSyncMenu.Instance.SetVisible(true);
                AssetSyncMenu.Instance.SetActiveSpinning();
                AssetSyncMenu.Instance.SetHeaderText("Loading Scene...");
                HouseBuilderRoot.Instance.Load(m_SelectedFileName, LoadSaveType.Network, () =>
                {
                    Core.Audio.LoadSaveGeneratedSoundRef.GetLocalJsonVersion((localVersion) =>
                    {
                        if (null != localVersion)
                        {
                            Debug.Log("using localVersion for SoundSimulationMenuRef");
                            AssetSyncMenu.Instance.SetVisible(true);
                            AssetSyncMenu.Instance.SetActiveSpinning();
                            AssetSyncMenu.Instance.SetHeaderText("Loading local baked data...");
                            
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateProbeSpacing(localVersion.HorizontalSpacing);
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateProbeHeight(localVersion.HeightAboveFloor);

                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateTextureMapPerMetre(localVersion.TextureMapPerMetre);
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateTextureMapMaxHeight(localVersion.TextureMaxHeight);

                            Core.Audio.AudioSimulationSetupRef.m_ProbBoxSize = localVersion.ProbeBoxSize;

                            Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position = localVersion.ProbeBoxCenter;
                            Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.SetScale(localVersion.ProbeBoxSize.x, localVersion.ProbeBoxSize.y, localVersion.ProbeBoxSize.z);


                            SplTextureMap.SplTextureData details = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
                            if (details != null)
                            {
                                details.AmountPerMetre = localVersion.TextureMapPerMetre;
                                details.MaxHeight = localVersion.TextureMaxHeight;
                                details.TextureSize = localVersion.TextureMapSize;
                                details.TextureSizeX = localVersion.TextureSizeX;
                                details.TextureSizeZ = localVersion.TextureSizeZ;
                            }

                            // If there's no local audio data for this scene, there's no need to keep the menu open anymore.
                            Core.Audio.AudioSimulationSetupRef.LoadBakedData(AssetSyncMenu.Instance.SetHeaderText, AssetSyncMenu.Instance.SetProgressBarText, AssetSyncMenu.Instance.SetProgress, () =>
                                {
                                    AssetSyncMenu.Instance.SetVisible(false);
                                });
                        }
                        else
                        {
                            // If there's no local audio data for this scene, there's no need to keep the menu open anymore.
                            AssetSyncMenu.Instance.SetVisible(false);

                            Core.Audio.LoadSaveGeneratedSoundRef.PromptDownloadGeneratedSound(() =>
                            {
                            });
                        }
                    });
                });

                SetCurrentFileName(m_SelectedFileName);
                HBGUIManager.Instance.DisableAllHeaderTogglesMenu();
                ResetInterface();
                DisableAllToggle();
                Toggle(false);
            }
        }

        protected virtual void NewButtonPressed()
        {
            GenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you wish to make a clean plot? ", () =>
            {
                HouseBuilderRoot.Instance.ClearAll();
                HBGUIManager.Instance.RefreshScene();
                SetCurrentFileName(NEW_FILENAME);
                ResetInterface();
                HBGUIManager.Instance.DisableAllHeaderTogglesMenu();
                DisableAllToggle();
            }, null);
        }

        protected virtual void DeleteButtonPressed()
        {
            if (m_SelectedFileName != null)
            {
                GenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you want to delete plot: {m_SelectedFileName}? ", () =>
                {
                    if (m_SelectedFileName == m_CurrentFileName)
                    {
                        SetCurrentFileName(NEW_FILENAME);
                    }
                    HouseBuilderRoot.Instance.DeleteFiles(m_SelectedFileName, () =>
                    {
                        FileNameListManualRefresh(() =>
                        {
                            ResetInterface();
                            DisableAllToggle();
                        });
                    });


                }, null);
            }
        }

        protected virtual void SetCurrentFileName(string currentScene)
        {
            m_CurrentFileName = currentScene;

            if (HBGUIManager.Instance != null)
            {
                HBGUIManager.Instance.UpdateCurrentSceneLabel(CurrentFileName);
                Core.Audio.SteamAudioManagerRef.StartOfFileName = CurrentFileName;
            }
        }

        public virtual void OnItemsReloaded()
        {

        }

        public void Toggle(bool state)
        {
            IsVisible = state;
            m_CanvasGroup.VisibleAndInteractive(state);

            if (state)
            {

                FileNameListManualRefresh(OnItemsReloaded);
            }
            else
            {
                ActivateLoadCancelButtons(false);
                m_ViewScrollbar.value = 1;
                if (m_FolderTgl != null)
                {
                    m_FolderTgl.isOn = false;
                }
            }

            m_SaveButton.SetActive(true);
            m_NewButton.SetActive(true);
            m_DeleteButton.SetActive(true);
#if !UNITY_EDITOR
            if(Core.Network.Users.CurrentUser != null && Core.Network.Users.CurrentUser.m_UserName == EXAMPLES)
            {
                m_SaveButton.SetActive(false);
                m_NewButton.SetActive(false);
                m_DeleteButton.SetActive(false);
            }
#endif
        }

        protected void ActivateLoadCancelButtons(bool state)
        {
            m_LoadButton.interactable = state;
            m_DeleteButton.interactable = state;
        }

        protected void GetSelectedToggle(string sceneName, bool state)
        {
            if (state == false)
            {
                ResetInterface();
            }
            else
            {
                m_InputField.text = sceneName;
                m_SelectedFileName = sceneName;
            }
            ActivateLoadCancelButtons(state);
        }

        protected virtual void ResetInterface()
        {
            m_InputField.text = STRING_NULL;
            m_SelectedFileName = null;
            ActivateLoadCancelButtons(false);
            m_InputFieldBackground.color = WHITE_COLOR;
        }

        protected void ShowErrorFeedback()
        {
            m_InputFieldBackground.color = RED_COLOR;
        }

        protected void ShowSuccessFeedback()
        {
            m_InputFieldBackground.color = GREEN_COLOR;
        }

        protected string GetInputFieldText()
        {
            return m_InputField.text;
        }

        protected void CreateNewPlotSlot(string text)
        {
            string localtext = text;
            var tglClone = m_ItemPoolManager.SpawnObject(m_ScrollViewContent.transform);
            tglClone.transform.localScale = Vector3.one;
            tglClone.SetActive(true);
            Toggle toggle = tglClone.GetComponentInChildren<Toggle>();
            toggle.group = m_ToggleGroup;

            TextMeshProUGUI gui = tglClone.GetComponentInChildren<TextMeshProUGUI>();
            gui.text = text;

            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((value) =>
            {
                GetSelectedToggle(localtext, value);
            });
           
        }


        public void RefreshButtonList(Action callback)
        {
            m_ItemPoolManager.DeSpawnAll();

            for (int i = 0; i < m_SavedFileNameList.Count; ++i)
            {
                CreateNewPlotSlot(m_SavedFileNameList[i]);
            }
            
            callback?.Invoke();
        }

        protected void InitialiseFileNameList(Action callback)
        {
            if (Core.Network.Users != null && Core.Network.Users.CurrentUser != null)
            {
                Core.Network.GetManifestJsonList(CONTENTS_JSON, (list) =>
                {
                    if(list != null)
                    {
                        m_SavedFileNameList = list;
                        m_SavedFileNameList.Sort();

                    }
                    callback?.Invoke();
                });
            }
            else
            {
                Debug.LogError("No saves available, please log in to display your saves.");
                callback?.Invoke();
            }
        }

        protected bool IsFileNameAlreadyUsed(string sceneName) => m_SavedFileNameList.Contains(sceneName);

        public void ResetFileNameList()
        {
            m_SavedFileNameList.Clear();
            RefreshButtonList(null);
        }

        public void FileNameListManualRefresh(Action callback)
        {
            InitialiseFileNameList(() =>
            {
               RefreshButtonList(callback);
            });
        }

        protected void DisableAllToggle()
        {
            if (m_ToggleGroup.AnyTogglesOn())
            {
                m_ToggleGroup.SetAllTogglesOff();
            }
        }
    }
}

