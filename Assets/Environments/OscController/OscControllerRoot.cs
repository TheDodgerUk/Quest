using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{

    public class OscControllerRoot : MonoBehaviour
    {
        public const string OPTIONS_ALL = "All";
        public const string OPTIONS_SINGLE = "Single";


        public static string GENERIC_HEADER_HEADER = "DMX_GENERIC";
        public static string GENERIC_PLAY_MUSIC_MESSAGE = "PlayMusic";
        public static string GENERIC_STOP_ALL_MESSAGE = "StopAll";
        public static string GENERIC_RESET_ALL_MESSAGE = "ResetAll";


        public static string GENERIC_SELECT_DMX_MESSAGE = "SelectDmx";
        public static string GENERIC_SELECT_FEATURE_MESSAGE = "SelectFeature";
        public static string GENERIC_SELECT_ALL_DMX_MESSAGE = "SelectAllDmx";
        public static string GENERIC_UNSELECT_ALL_DMX_MESSAGE = "UnSelectAllDmx";

        public static string ALL_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(OscAllUsedFixtures)}{GlobalConsts.JSON}";
        public static string SEQUENCE_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(OscSequenceData)}{GlobalConsts.JSON}";
        public static string TIME_LINE_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(OscGroupData)}{GlobalConsts.JSON}";
        public static string GENERIC_JSON => $"_{Core.Network.Users.CurrentUser.m_UserName.ToLower()}_{nameof(OscGenericData)}{GlobalConsts.JSON}";

        private string ALL => $"{Application.persistentDataPath}/AllUsedFixturesRef";
        private string SCENE => $"{Application.persistentDataPath}/SCENE";
        private string TIME_LINE => $"{Application.persistentDataPath}/TIME_LINE";

        public static readonly Color SELECTED = Color.red;
        public static readonly Color UN_SELECTED = Color.white;

        public const int MAX_DMX_INDEX = 512;
        public const int MAX_DMX_VALUE = 255;
        public const float  MAX_DMX_VALUE_FLOAT = (float)MAX_DMX_VALUE;

        public const float BACKGROUND_SCALE_SIZE = 1.1f;


        public class OSCAssetSyncMenu : HouseBuilder.AssetSyncMenuBase
        {
            public static OSCAssetSyncMenu Instance;
            public override void Initialise()
            {
                Instance = this;
                base.Initialise();
            }
        }

        public static OscControllerRoot Instance;

        
        public SliderPanel SliderPanelRef { get; private set; }

        public SliderExtraPanel SliderExtraPanelRef { get; private set; }
        public SliderSimplePanel SliderSimplePanelRef { get; private set; }
        public SliderSimpleChoicePanel SliderSimpleChoicePanelRef { get; private set; }
        public FixtureChoicePanel FixtureChoicePanelRef { get; private set; }
        public FixtureWebInfoPanel FixtureWebInfoPanelRef { get; private set; }
        public GridPanel GridPanelRef { get; private set; }
        public MenuPanel MenuPanelRef { get; private set; }

        public MacroPanel MacroPanelRef { get; private set; }

        public HeaderPanel HeaderPanelRef { get; private set; }
        public SequencePanel SequencePanelRef { get; private set; }
        public SequenceRunTimePanel SequenceRunTimePanelRef { get; private set; }

        public SequencePanelOptions SequencePanelOptionsRef { get; private set; }
        public TimeLinePanelOptions TimeLinePanelOptionsRef { get; private set; }

        public SettingsPanel SettingsPanelRef { get; private set; }
        

        public TimeLinePanel TimeLinePanelRef { get; private set; }
        public CurveEditorPanel CurveEditorPanelRef { get; private set; }
        public WheelPanel WheelPanelRef { get; private set; }
        public PlayData PlayDataRef { get; private set; }

        public HouseBuilder.UserNamePassword UserNamePasswordRef { get; private set; }
        public OscLoadSaveMenuManager OscLoadSaveMenuManagerRef { get; private set; }

        private bool m_Connected = false;

        private List<Transform> m_BlinkingLight;
        private IEnumerator m_PlayWholeScene;
        private Camera m_Camera;

        public Image BackGroundImageFull { get; private set; }
        private Image m_BackGroundCutout;


        private const float DISPLAY_TIME = 0.1f;

        public AudioSource AudioSourceRef { get; private set; }

        public void Initialise()
        {
            Instance = this;
            DataContainer.CreateInstance();

            if (HouseBuilder.HouseBuilderRoot.Instance == null && HouseViewer.HouseViewerRoot.Instance == null)
            {
                FixtureContainer.CreateInstance();
                FixtureContainer.Instance.Initialise();
                Core.SceneLoader.SetMouseLockVisible(false, true);
            }

            
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }


        private void OnEnable()
        {
            ScreenSizeChange(new Vector2(Screen.width, Screen.height));
            //stop the ghosting on the GUI
            if (HouseBuilder.HouseBuilderRoot.Instance != null || HouseViewer.HouseViewerRoot.Instance != null)
            {
                if (m_Camera != null)
                {
                    m_Camera.SetActive(true);
                    this.WaitFor(Time.deltaTime, () =>
                    {
                        m_Camera.SetActive(false);
                    });
                }
            }
        }


        private void OnEnvironmentLoadingComplete()
        {
            if (HouseBuilder.HouseBuilderRoot.Instance == null && HouseViewer.HouseViewerRoot.Instance == null)
            {
                var PhotonSetupRef = Core.PhotonMultiplayerRef;
                PhotonSetupRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayerNoVoice);
                PhotonSetupRef.ChangeRoom("HouseBuilderRoot");
            }


            this.gameObject.SetActive(true);
            AudioSourceRef = this.gameObject.ForceComponent<AudioSource>();
            AudioSourceRef.playOnAwake = false;

            MenuPanelRef = this.gameObject.FindAndForceComponent<MenuPanel>();
            MacroPanelRef = this.gameObject.FindAndForceComponentMono<MacroPanel, MacroPanelMono>();

            HeaderPanelRef = this.gameObject.FindAndForceComponent<HeaderPanel>();
            SliderPanelRef = this.gameObject.FindAndForceComponent<SliderPanel>();

            SliderExtraPanelRef = this.gameObject.FindAndForceComponentMono<SliderExtraPanel, SliderExtraPanelMono>();

            SliderSimplePanelRef = this.gameObject.FindAndForceComponent<SliderSimplePanel>();
            SliderSimpleChoicePanelRef = this.gameObject.FindAndForceComponent<SliderSimpleChoicePanel>();
            
            FixtureChoicePanelRef = this.gameObject.FindAndForceComponent<FixtureChoicePanel>();
            FixtureWebInfoPanelRef = this.gameObject.FindAndForceComponent<FixtureWebInfoPanel>();

            GridPanelRef = this.gameObject.FindAndForceComponent<GridPanel>();
            SequencePanelRef = this.gameObject.FindAndForceComponent<SequencePanel>();
            SequenceRunTimePanelRef = this.gameObject.FindAndForceComponent<SequenceRunTimePanel>();

            SequencePanelOptionsRef = this.gameObject.FindAndForceComponent<SequencePanelOptions>();
            TimeLinePanelOptionsRef = this.gameObject.FindAndForceComponent<TimeLinePanelOptions>();
            SettingsPanelRef = this.gameObject.FindAndForceComponent<SettingsPanel>();

            TimeLinePanelRef = this.gameObject.FindAndForceComponent<TimeLinePanel>();

            CurveEditorPanelRef = this.gameObject.FindAndForceComponentMono<CurveEditorPanel, CurveEditorPanelMono>();
            WheelPanelRef = this.gameObject.FindAndForceComponentMono<WheelPanel, WheelPanelMono>();

            PlayDataRef = new PlayData(this.gameObject.ForceComponent<PlayDataMono>());
            OscLoadSaveMenuManagerRef = this.transform.Search("LoadSaveMenu").gameObject.ForceComponent<OscLoadSaveMenuManager>();


            var assetSyncMenu = this.transform.Search("AssetSync").gameObject.ForceComponent<OSCAssetSyncMenu>();
            assetSyncMenu.Initialise();

            GameObject prefab = CachedObjects.Instance.GetPrefab("SavedPlot");
            OscLoadSaveMenuManagerRef.Initialise(prefab, null);


            BackGroundImageFull = this.transform.SearchComponent<Image>("BackGroundFull");
            BackGroundImageFull.raycastTarget = false;

            m_BackGroundCutout = this.transform.SearchComponent<Image>("BackGroundCornerCut");
            m_BackGroundCutout.raycastTarget = false;


            HeaderPanelRef.Initialise();
            SliderPanelRef.Initialise();
            SliderExtraPanelRef.Initialise();
            SliderSimplePanelRef.Initialise();
            SliderSimpleChoicePanelRef.Initialise();
            FixtureChoicePanelRef.Initialise();
            FixtureWebInfoPanelRef.Initialise();
            GridPanelRef.Initialise();
            SequencePanelRef.Initialise();
            SequenceRunTimePanelRef.Initialise();
            SequencePanelOptionsRef.Initialise();
            TimeLinePanelOptionsRef.Initialise();
            SettingsPanelRef.Initialise();
            TimeLinePanelRef.Initialise();
            MenuPanelRef.Initialise(); // needs to be last 
            MacroPanelRef.Initialise();



            m_BlinkingLight = this.transform.Search("ConnectedLight").GetDirectChildren();
            m_BlinkingLight.ForEach(e => e.SetActive(false));


            var promptMenu = transform.Search("GenericMenu").gameObject.ForceComponent<OscGenericPromptMenu>();
            promptMenu.Initialise();


            if (HouseBuilder.HouseBuilderRoot.Instance == null)
            {
                StartCoroutine(BlinkingLights());
            }


            var userNamePasswordMenuObject = transform.Search("UserNamePassword").gameObject;
            if (HouseBuilder.HouseBuilderRoot.Instance == null)
            {
                UserNamePasswordRef = userNamePasswordMenuObject.ForceComponent<HouseBuilder.UserNamePassword>();
                UserNamePasswordRef.Initialise(null, OscLoadSaveMenuManagerRef);
            }
            else
            {
                userNamePasswordMenuObject.SetActive(false);
            }


            if (HouseBuilder.HouseBuilderRoot.Instance == null && HouseViewer.HouseViewerRoot.Instance == null)
            {
                if (Core.Environment.Environments.Count == 1 && Core.Environment.CurrentEnvironment.IncludeCataloguePrefabs == false)
                {
                    Core.GenericLoadingRef.SetActive(true);
                    Core.GenericLoadingRef.OnMessage("Waiting");
                    Core.Assets.UpdateLocalAssetsFromServer(Core.GenericLoadingRef.OnMessage, Core.GenericLoadingRef.OnProgress, CloseAssetMenu);
                }
                else
                {
                    CloseAssetMenu(0); // No assets need updating therefore this menu should not be visible
                }
            }

            MenuPanelRef.OnLoadSaveSelected += LoadSaveSelected;
            MenuPanelRef.OnUserSelected += UserSelected;



            if (HouseBuilder.HouseBuilderRoot.Instance != null || HouseViewer.HouseViewerRoot.Instance != null)
            {
                m_Camera = this.GetComponentInChildren<Camera>();
                var listener = m_Camera.GetComponent<AudioListener>();
                Destroy(listener);               
                BackGroundImageFull.SetActive(false);
                m_BackGroundCutout.SetActive(true);

                this.WaitForEndOfFrame(() =>
                {
                    this.gameObject.SetActive(false);
                });

                if (HouseViewer.HouseViewerRoot.Instance != null)
                {
                    this.transform.SearchComponent<Transform>("BaseCanvas").SetActive(false);
                    this.transform.SearchComponent<Camera>("Camera").SetActive(false);
                }
            }
            else
            {
                Screen.SetResolution(1920, 1080, false);
                BackGroundImageFull.SetActive(true);
                m_BackGroundCutout.SetActive(false);
            }


            ScreenSizeChange(new Vector2(Screen.width, Screen.height));
            Core.OnScreenSizeChange += ScreenSizeChange;

            var blackout = this.transform.SearchComponent<Transform>("LoadingBlackout");
            blackout.SetActive(false);
        }

        private void ScreenSizeChange(Vector2 size)
        {
            if (m_BackGroundCutout != null)
            {
                var rect = m_BackGroundCutout.GetComponent<RectTransform>();
                rect.sizeDelta = size * BACKGROUND_SCALE_SIZE;

                rect = BackGroundImageFull.GetComponent<RectTransform>();
                rect.sizeDelta = size * BACKGROUND_SCALE_SIZE;
            }
        }

        private void CloseAssetMenu(float obj)
        {
            Core.GenericLoadingRef.SetActive(false);
        }

        private IEnumerator BlinkingLights()
        {
            while(m_BlinkingLight == null)
            {
                yield return new WaitForEndOfFrame();
            }

            while (true)
            {
                if (m_Connected == true)
                {
                    m_BlinkingLight.ForEach(e => e.SetActive(false));
                    m_BlinkingLight[0].SetActive(true);
                    yield return new WaitForSeconds(0.3f);

                    m_BlinkingLight.ForEach(e => e.SetActive(false));
                    m_BlinkingLight[1].SetActive(true);
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    m_BlinkingLight.ForEach(e => e.SetActive(false));
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }


        private void LoadSaveSelected()
        {
            OscLoadSaveMenuManagerRef.Toggle(!OscLoadSaveMenuManagerRef.IsVisible);          
        }

        private void UserSelected()
        {
            UserNamePasswordRef.Toggle(!UserNamePasswordRef.IsVisible);
        }


        public void DeleteOldData()
        {
            DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Clear();
            GridPanelRef.UpdateItems();

            int count = DataContainer.Instance.SequenceDataRef.Count;
            for (int i = 0; i < count; i++)
            {
                SequencePanelRef.DeleteScene(0);
            }
            DataContainer.Instance.SequenceDataRef.Clear();
            TimeLinePanelRef.DeleteAllItems();
            DataContainer.Instance.TimeLineDataRef.Clear();
        }

        public void ReInitialiseAll()
        {
            TimeLinePanelRef.ScaleAllItems();
            SaveLocally(out string allSave, out string sceneSave, out string timeSave);
            DeleteOldData();
            LoadLocally(allSave, sceneSave, timeSave);

            InitialiseLoadedData();
        }

        public void InitialiseLoadedData()
        {
            foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
            {
                item.m_Fixture = FixtureContainer.Instance.GetFixture(item.m_JsonFileName);
            }

            foreach (var scene in DataContainer.Instance.SequenceDataRef)
            {
                foreach (var item in scene.m_SliderData)
                {
                    if (DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.ContainsIndex(item.m_FixtureDataIndex) == true)
                    {
                        item.m_FixtureData = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[item.m_FixtureDataIndex];
                    }
                }
            }



            foreach (var scene in DataContainer.Instance.SequenceDataRef)
            {
                int count = scene.m_SliderData.RemoveAll(e => e.m_FixtureData == null);
                if (count != 0)
                {
                    Debug.LogError($"Removed {count} as these were invalid");
                }
            }


            DataContainer.Instance.SortTimeLineData();
            List<OscGroupData.GroupLinkageData> toRemove = new List<OscGroupData.GroupLinkageData>();
            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                toRemove.Clear();
                foreach (var item in group.m_GroupLinkageData)
                {
                    if(item.m_SequenceIndex == -1)
                    {
                        toRemove.Add(item);
                    }
                    else
                    {
                        if (DataContainer.Instance.SequenceDataRef.ContainsIndex(item.m_SequenceIndex) == true)
                        {
                            item.m_Sequence = DataContainer.Instance.SequenceDataRef[item.m_SequenceIndex];
                        }
                    }                    
                }
                for (int i = 0; i < toRemove.Count; i++)
                {
                    group.m_GroupLinkageData.Remove(toRemove[i]);
                }
                toRemove.Clear();
            }


            GridPanelRef.UpdateItems();
            SequencePanelRef.InitialiseLoadedItems();
            TimeLinePanelRef.InitialiseLoadedItems();

            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                group.m_InputFieldName.text = group.m_GroupName;
            }



            if (string.IsNullOrEmpty(DataContainer.Instance.GenericDataDataRef.m_AudioName) == false)
            {
                if (DataContainer.Instance.GenericDataDataRef.m_IsLocalFile == false)
                {
                    Core.Network.GetCustomFileAudioClip(DataContainer.Instance.GenericDataDataRef.m_AudioName, (clip) =>
                    {
                        DataContainer.Instance.GenericDataDataRef.m_AudioClip = clip;
                        OscControllerRoot.Instance.AudioSourceRef.clip = clip;
                    });
                }
                else
                {
                    Core.Audio.LoadAudioFile(DataContainer.Instance.GenericDataDataRef.m_LocalFullAudioName, (clip) =>
                    {
                        DataContainer.Instance.GenericDataDataRef.m_AudioClip = clip;
                        OscControllerRoot.Instance.AudioSourceRef.clip = clip;
                    });
                }
            }
            else
            {
                DataContainer.Instance.GenericDataDataRef.m_AudioClip = null;
                OscControllerRoot.Instance.AudioSourceRef.clip = null;
            }

            if (HouseViewer.HouseViewerRoot.Instance == null) // not want these created in viewer, as waste of memory
            {
                TimeLinePanelRef.ScaleAllItems();
                TimeLinePanelRef.CreateAndAssignWave(DataContainer.Instance.GenericDataDataRef);
            }

        }

        private void SaveLocally(out string allSave, out string sceneSave, out string timeSave)
        {
            foreach (var scene in DataContainer.Instance.SequenceDataRef)
            {
                foreach (var item in scene.m_SliderData)
                {
                    int index = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.FindIndex(e => e == item.m_FixtureData);
                    item.m_FixtureDataIndex = index;
                }
            }

            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                group.m_GroupName = group.m_InputFieldName.text;
                foreach (var item in group.m_GroupLinkageData)
                {
                    int index = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == item.m_Sequence);
                    item.m_SequenceIndex = index;
                }
            }


            allSave = Json.JsonNet.WriteToText<OscAllUsedFixtures>(DataContainer.Instance.AllUsedFixturesRef, true);
            sceneSave = Json.JsonNet.WriteToText<List<OscSequenceData>>(DataContainer.Instance.SequenceDataRef, true);
            timeSave = Json.JsonNet.WriteToText<List<OscGroupData>>(DataContainer.Instance.TimeLineDataRef, true);
        }


        private void LoadLocally(string allSave, string sceneSave, string timeSave)
        {
            DataContainer.Instance.AllUsedFixturesRef = Json.JsonNet.ReadFromText<OscAllUsedFixtures>(allSave);
            DataContainer.Instance.SequenceDataRef = Json.JsonNet.ReadFromText<List<OscSequenceData>>(sceneSave);
            DataContainer.Instance.TimeLineDataRef = Json.JsonNet.ReadFromText<List<OscGroupData>>(timeSave);
        }

    }
}
