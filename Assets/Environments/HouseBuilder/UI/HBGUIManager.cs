using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class HBGUIManager : MonoBehaviour
    {
        public static HBGUIManager Instance;
        private Dictionary<string, GameObject> m_ContentTypes = new Dictionary<string, GameObject>();

        private Transform m_Header;

        private Button m_BtnScreenshot;
        private Button m_QuitBtn;
        private ToggleGroup m_MenuToggleGroup;

        private ModeToggle m_PlotMode;
        private ModeToggle m_StructureMode;
        private ModeToggle m_PlacementMode;
        private ModeToggle m_SoundMode;
        private ModeToggle m_ShowRoomMode;
        private ModeToggle m_LightingMode;
        private ModeToggle m_StagingMode;

        private ModeToggleHidden m_ModularMode;
        private ModeToggleHidden m_OSCMode;

        public Image m_Blackout;
        private CanvasGroup m_CurrentModeCanvasGroup;
        private TextMeshProUGUI m_CurrentModeLabel;
        private TextMeshProUGUI m_CurrentSceneLabel;

        public ViewMode m_ViewMode;
        public CameraWidgetManager CameraWidgetRef { get; private set; }
        public SceneSettingsMenu SceneSettingsMenuRef { get; private set; }
        public CatalogueMenu CatalogueMenuRef { get; private set; }
        public WallSettingsMenu WallSettingsMenuRef { get; private set; }
        public CeilingSettingsMenu CeilingSettingsMenuRef { get; private set; }
        public FloorSettingsMenu FloorSettingsMenuRef { get; private set; }
        public ItemSettingsMenu ItemSettingsMenuRef { get; private set; }
        public SoundSettingsMenu SoundSettingsMenuRef { get; private set; }
        
        public ShowRoomSettingsMenu ShowRoomSettingsMenuRef { get; private set; }
        public SoundSimulationMenu SoundSimulationMenuRef { get; private set; }
        public LightingSettingsMenu LightingSettingsMenuRef { get; private set; }
        public StagingSettingsMenu StagingSettingsMenuRef { get; private set; }
        public OscSettingsMenu OscSettingsMenuRef { get; private set; }

        public ModularMenu ModularMenuRef { get; private set; }
        public StructureSwitchMenu m_StructureSwitchMenu;
        private SceneItemsList m_SceneItemsList;
        private HelpOverlay m_HelpOverlay;

        public MaterialSelectionGUI m_MaterialSelectionGUI;
        public RotationAmountGUI m_RotationAmountGUI;

        private TextMeshProUGUI m_UserName;
        private CanvasGroup m_HeaderCanvasGroup;

        private const string DEFAULT_USER_NAME = "Guest User";

        private const int SESSION_DETAILS_RIGHT_TOGGLES_COUNT = 4;
        private const int PLOT_DETAILS_TOGGLES_COUNT = 2;
        private int m_TogglesInitialisationCount = 0;
        public LoadSaveMenuManager LoadSaveMenuManagerRef { get; private set; }

        public void Initialise()
        {
            Instance = this;
            GetContentTypes();

            m_Header = transform.Find("Header");
            m_HeaderCanvasGroup = m_Header.GetComponent<CanvasGroup>();
            m_HeaderCanvasGroup.VisibleAndInteractive(true);
            m_Blackout = transform.Find("Blackout").GetComponent<Image>();
            m_Blackout.SetActive(true);

            AssetSyncMenu assetSyncMenu = transform.Find("AssetSync").gameObject.ForceComponent<AssetSyncMenu>();
            assetSyncMenu.Initialise();

            GameObject currentModePanel = m_Header.Find("CurrentMode").gameObject;
            m_CurrentModeCanvasGroup = currentModePanel.GetComponent<CanvasGroup>();
            m_CurrentModeLabel = currentModePanel.GetComponentInChildren<TextMeshProUGUI>();

            m_CurrentSceneLabel = m_Header.Find("Plot Details/TglGroup/TglBrowse/PlotName").GetComponent<TextMeshProUGUI>();
            m_UserName = m_Header.Find("Plot Details/TglGroup/TglUser/Username").GetComponent<TextMeshProUGUI>();

            m_BtnScreenshot = m_Header.Find("Session Details/LeftBtns/ScreenShotButton").GetComponent<Button>();
            m_BtnScreenshot.onClick.AddListener(HouseBuilderRoot.Instance.TakeScreenshot);

            m_MenuToggleGroup = m_Header.GetComponent<ToggleGroup>();

            var promptMenu = transform.Find("GenericPopup/GenericMenu").gameObject.ForceComponent<GenericPromptMenu>();
            promptMenu.Initialise();

            var listMenu = transform.Find("GenericPopup/GenericListMenu").gameObject.ForceComponent<GenericListMenu>();
            listMenu.Initialise(m_ContentTypes);

            SetupNavigation();
            SetupWidgets();
            SetupSessionDettailsTgls();
            SetupPlotDetailsTgls();

            CatalogueMenuRef = transform.Find("ModeTools/Catalogue").gameObject.ForceComponent<CatalogueMenu>();
            CatalogueMenuRef.Initialise(m_ContentTypes);

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            HouseBuilderRoot.Instance.m_InitialisationsToWaitForTotal++;
            Interaction.SnappingEnabled = true;

            var quitBtn = m_Header.Find("Session Details/RightBtns/PowerOff").GetComponent<Button>();
            quitBtn.onClick.AddListener(HouseBuilderRoot.Instance.QuitApplication);

            m_HelpOverlay.SetVisible(false);
            //OnStructureModeSelected();
            HouseBuilderRoot.Instance.OnModularModeSelected += (e) => m_ModularMode?.ForceOnChange();

            // Setup the sound sim menu.
            Button btnSoundSim = m_Header.Find("Session Details/LeftBtns/Open Sound Sim Settings").GetComponent<Button>();
            SoundSimulationMenuRef = transform.Find("Sound Sim Menu").gameObject.ForceComponent<SoundSimulationMenu>();
            SoundSimulationMenuRef.Initialise(btnSoundSim);
            btnSoundSim.onClick.AddListener(SoundSimulationMenuRef.OpenCloseMenu);
        }


        private void SetupNavigation()
        {
            m_PlotMode = m_Header.Find("Navigation/TglPlotMode").AddComponent<ModeToggle>();
            m_StructureMode = m_Header.Find("Navigation/TglDecMode").AddComponent<ModeToggle>();
            m_PlacementMode = m_Header.Find("Navigation/TglPlaceMode").AddComponent<ModeToggle>();
            m_SoundMode = m_Header.Find("Navigation/TglSoundMode").AddComponent<ModeToggle>();
            m_ShowRoomMode = m_Header.Find("Navigation/TglShowRoomMode").AddComponent<ModeToggle>();
            m_LightingMode = m_Header.Find("Navigation/TglLightingMode").AddComponent<ModeToggle>();
            m_StagingMode = m_Header.Find("Navigation/TglStagingMode").AddComponent<ModeToggle>();

            m_OSCMode = m_Header.Find("Navigation/TgOscMode").AddComponent<ModeToggleHidden>();
            m_ModularMode = m_Header.Find("Navigation/TglModularMode").AddComponent<ModeToggleHidden>();

            m_PlotMode.Initialise(BuilderMode.Plot);
            m_StructureMode.Initialise(BuilderMode.Structure);
            m_PlacementMode.Initialise(BuilderMode.Placement);
            m_SoundMode.Initialise(BuilderMode.Sound);
            m_ShowRoomMode.Initialise(BuilderMode.ShowRoom);
            m_OSCMode.Initialise(BuilderMode.OSC);
            m_ModularMode.Initialise(BuilderMode.Modular);
            m_LightingMode.Initialise(BuilderMode.Lighting);
            m_StagingMode.Initialise(BuilderMode.Staging);

            m_CurrentModeCanvasGroup.VisibleAndInteractive(false);
            m_CurrentModeCanvasGroup.alpha = 0;
            m_CurrentModeCanvasGroup.interactable = false;
            m_CurrentModeCanvasGroup.blocksRaycasts = false;
        }

        public void ForcePlacementMode()
        {
            // needs to be done twice otherwise the header icon not change
            m_PlacementMode.m_Toggle.isOn = true;
            m_PlacementMode.m_Toggle.isOn = true;
        }


        private void SetupSessionDettailsTgls()
        {
            var toggleRightGroup = m_Header.Find("Session Details/RightBtns/TglGroup").gameObject;

            for (int i = 0; i < SESSION_DETAILS_RIGHT_TOGGLES_COUNT; ++i)
            {
                var toggle = toggleRightGroup.transform.GetChild(i).GetComponent<Toggle>();
                string toggleName = toggle.name;

                switch (toggleName)
                {
                    case "TglInfo":
                        m_HelpOverlay = transform.Find("Help").gameObject.ForceComponent<HelpOverlay>();
                        m_HelpOverlay.Initialise();
                        break;
                    case "TglItemsList":
                        m_SceneItemsList = transform.Find("ModeTools/ItemList").gameObject.ForceComponent<SceneItemsList>();
                        m_SceneItemsList.Initialise(m_ContentTypes);
                        break;
                    case "TglSettings":
                        var tglGrid = m_Header.Find("Session Details/LeftBtns/TglGrid").GetComponent<Toggle>();
                        SceneSettingsMenuRef = transform.Find("SettingsMenu").gameObject.ForceComponent<SceneSettingsMenu>();
                        SceneSettingsMenuRef.Initialise(tglGrid,toggle);
                        break;
                    case "TglUpload":
                        var uploadMenu = transform.Find("UploadMenu").gameObject.ForceComponent<UploadMenuManager>();
                        uploadMenu.Initialise(m_ContentTypes, toggle);
                        break;

                }
                toggle.onValueChanged.AddListener((value) => ToggleMenu(toggleName, value));
                ++m_TogglesInitialisationCount;

            }

            Toggle snappingTgl = m_Header.Find("Session Details/LeftBtns/TglSnap").GetComponent<Toggle>();
            snappingTgl.onValueChanged.AddListener((value) => ToggleSnapping(value));

            Toggle lightTgl = m_Header.Find("Session Details/LeftBtns/TglLight").GetComponent<Toggle>();
            lightTgl.onValueChanged.AddListener((value) => ToggleLight(value));

        }      
        
        public void ShowRoomMode(bool show)
        {
            m_HeaderCanvasGroup.VisibleAndInteractive(!show);
 
            CatalogueMenuRef.SetActive(!show);
        }

        private void SetupPlotDetailsTgls()
        {
            var toggleGroup = m_Header.Find("Plot Details/TglGroup").gameObject;

            UserNamePassword userNamePasswordMenu = null;

            for (int i = 0; i < PLOT_DETAILS_TOGGLES_COUNT; ++i)
            {
                var toggle = toggleGroup.transform.GetChild(i).GetComponent<Toggle>();
                string toggleName = toggle.name;

                switch (toggleName)
                {
                    case "TglUser":
                        userNamePasswordMenu = transform.Find("UserNamePassword").gameObject.ForceComponent<UserNamePassword>();                      
                        break;
                    case "TglBrowse":
                        LoadSaveMenuManagerRef = transform.Find("LoadSaveMenu").gameObject.ForceComponent<LoadSaveMenuManager>();
                        LoadSaveMenuManagerRef.Initialise(m_ContentTypes["SavedPlot"].gameObject, toggle);
                        break;
                }
                toggle.onValueChanged.AddListener((value) => ToggleMenu(toggleName, value));
                ++m_TogglesInitialisationCount;
            }


            // this has to be done afterwards
            for (int i = 0; i < PLOT_DETAILS_TOGGLES_COUNT; ++i)
            {
                var toggle = toggleGroup.transform.GetChild(i).GetComponent<Toggle>();
                string toggleName = toggle.name;
                switch (toggleName)
                {
                    case "TglUser":
                        userNamePasswordMenu.Initialise(toggle, LoadSaveMenuManagerRef);
                        break;
                }                        
            }
        }

        private void ToggleMenu(string toggleName, bool state)
        {
            switch (toggleName)
            {
                case "TglInfo":
                    m_HelpOverlay.SetVisible(state);
                    break;
                case "TglItemsList":
                    m_SceneItemsList.SetVisible(state);
                    break;
                case "TglSettings":
                    SceneSettingsMenuRef.SetActive(state);
                    break;
                case "TglUser":
                    UserNamePassword.Instance.Toggle(state);
                    break;
                case "TglBrowse":
                    LoadSaveMenuManagerRef.Toggle(state);
                    break;
                case "TglUpload":
                    UploadMenuManager.Instance.Toggle(state);
                    break;
            }
        }

        public void DisableAllHeaderTogglesMenu()
        {
            if (m_MenuToggleGroup.AnyTogglesOn())
                m_MenuToggleGroup.SetAllTogglesOff();
        }

        private void SetupWidgets()
        {
            Transform widgetRoot = transform.Find("Widgets");

            m_ViewMode = widgetRoot.Find("ViewMode").gameObject.ForceComponent<ViewMode>();
            m_ViewMode.Initialise();

            m_StructureSwitchMenu = transform.Find("ModeTools/StructureSwitch").gameObject.ForceComponent<StructureSwitchMenu>();
            m_StructureSwitchMenu.Initialise();

            WallSettingsMenuRef = widgetRoot.Find("WallSettings").gameObject.ForceComponent<WallSettingsMenu>();
            WallSettingsMenuRef.Initialise();

            CeilingSettingsMenuRef = widgetRoot.Find("CeilingSettings").gameObject.ForceComponent<CeilingSettingsMenu>();
            CeilingSettingsMenuRef.Initialise();

            FloorSettingsMenuRef = widgetRoot.Find("FloorSettings").gameObject.ForceComponent<FloorSettingsMenu>();
            FloorSettingsMenuRef.Initialise();

            ItemSettingsMenuRef = widgetRoot.Find("ItemSettings").gameObject.ForceComponent<ItemSettingsMenu>();
            ItemSettingsMenuRef.Initialise(m_ContentTypes);

            SoundSettingsMenuRef = widgetRoot.Find("SoundSettings").gameObject.ForceComponent<SoundSettingsMenu>();
            SoundSettingsMenuRef.Initialise();

            ShowRoomSettingsMenuRef = widgetRoot.Find("ShowRoomSettings").gameObject.ForceComponent<ShowRoomSettingsMenu>();
            ShowRoomSettingsMenuRef.Initialise();
            ShowRoomSettingsMenuRef.SetActive(false);

            LightingSettingsMenuRef = widgetRoot.Find("LightingSettings").gameObject.ForceComponent<LightingSettingsMenu>();
            LightingSettingsMenuRef.Initialise();

            StagingSettingsMenuRef = widgetRoot.Find("StagingSettings").gameObject.ForceComponent<StagingSettingsMenu>();
            StagingSettingsMenuRef.Initialise();

            OscSettingsMenuRef = widgetRoot.Find("OscSettings").gameObject.ForceComponent<OscSettingsMenu>();
            OscSettingsMenuRef.Initialise();

            ModularMenuRef = widgetRoot.Find("ModularMenu").gameObject.ForceComponent<ModularMenu>();
            ModularMenuRef.Initialise(m_ContentTypes);


            var poleSettingsMenu = widgetRoot.Find("PoleSettings").gameObject.ForceComponent<PoleSettingsMenu>();
            poleSettingsMenu.Initialise();

            m_MaterialSelectionGUI = widgetRoot.Find("SelectedItem").gameObject.ForceComponent<MaterialSelectionGUI>();

            var canvas = this.transform.GetComponent<Canvas>();
            m_RotationAmountGUI = widgetRoot.Find("RotationAmount").gameObject.ForceComponent<RotationAmountGUI>();
            m_RotationAmountGUI.Initialise(canvas);

            var visibilitySwitch = transform.Find("ModeTools/VisibilitySwitch").gameObject.ForceComponent<VisibilitySwitchMenu>();
            visibilitySwitch.Initialise();

            var colPicker = widgetRoot.Find("ColorPicker").gameObject.ForceComponent<ColorPickerManager>();
            colPicker.Initialise();

            CameraWidgetRef = this.transform.Find("Widgets/CameraWidget").gameObject.ForceComponent<CameraWidgetManager>();
            CameraWidgetRef.Initialise();

        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;

            //other stuff here after setup is done
            m_PlotMode.m_Toggle.SetIsOnWithoutNotify(true);

            HouseBuilderRoot.Instance.m_InitialisationsToWaitForSoFar++;

            var canvas = this.transform.GetComponent<Canvas>();
            m_MaterialSelectionGUI.Initialise(CatalogueMenuRef.GetPossibleItems(), canvas);
            m_SceneItemsList.ProcessCatalogueIcons(CatalogueMenuRef.GetPossibleItems());
        }


        public void SetUserName()
        {
            if (Core.Network.Users.CurrentUser == null)
            {
                m_UserName.text = DEFAULT_USER_NAME;
            }
            else
            {
                m_UserName.text = Core.Network.Users.CurrentUser.m_UserName;
            }
        }

        public void RefreshScene()
        {
            m_PlotMode.m_Toggle.isOn = true;
            DisableAllHeaderTogglesMenu();
        }

        public void CurrentModeLabelFadeOff(string modeName)
        {
            m_CurrentModeLabel.SetText(modeName);
            this.Create<ValueTween>(2.5f, EaseType.ExpoIn).Initialise(1f, 0f, (f) =>
            {
                m_CurrentModeCanvasGroup.alpha = f;
            });
        }

        private void GetContentTypes()
        {
            Transform contentHolder = transform.Find("ContentTypes");

            List<Transform> children = contentHolder.GetDirectChildren();

            foreach (var child in children)
            {
                if (false == m_ContentTypes.ContainsKey(child.name))
                {
                    m_ContentTypes.Add(child.name, null);
                }

                m_ContentTypes[child.name] = child.gameObject;
                m_ContentTypes[child.name].SetActive(false);
            }
        }

        public void UpdateCurrentSceneLabel(string name)
        {
           m_CurrentSceneLabel.text = name;

        }

        private void ToggleSnapping(bool state)
        {
            Interaction.SnappingEnabled = state;
        }

        private void ToggleLight(bool state)
        {
            FloorAndSkyboxManager.Instance.SetUseAmbientLight(state);
        }
    }
}