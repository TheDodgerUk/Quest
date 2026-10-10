using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public static class BuilderModeExtensions
    {
        public static bool IsHeaderSelectable(this BuilderMode enumValue)
        {
            switch (enumValue)
            {
                case BuilderMode.OSC:
                case BuilderMode.Modular:
                    return false;
                    break;
            }
            return true;
        }
    }

    public enum LoadSaveType
    {
        Network,
        Local,
    }

    public enum BuilderMode
    {
        NONE,
        Plot,
        Structure,
        Placement,
        Sound,
        ShowRoom, 
        Modular,
        Lighting,
        Staging,
        OSC
    }

    public class HouseBuilderRoot : MonoBehaviour
    {
        public static HouseBuilderRoot Instance;
        private Camera m_MainCamera;

        private string LOCAL_LOADSAVE => $"{Application.persistentDataPath}/LocalSave";

        public LoadSaveHouseContentsData m_LoadSaveHouseContentsData;
        public LoadSaveHouseWallsData m_LoadSaveHouseWallsData;
        public HouseBuilderGeneralJsonData m_LoadSaveGeneralData;
        private PositionGrid m_PositionGrid;

        public HouseBuilderInputContents InputContents { get; private set; }
        public HouseBuilderInputCeiling InputCeiling { get; private set; }
        public HouseBuilderInputFloorPlan InputFloorPlan { get; private set; }
        public HouseBuilderInputSound InputSound { get; private set; }
        public HouseBuilderInputShowRoom InputShowRoom { get; private set; }
        public HouseBuilderInputModular InputModular { get; private set; }
        public HouseBuilderInputCamera InputCamera { get; private set; }
        public HouseBuilderInputLighting InputLighting { get; private set; }

        public HouseBuilderInputStaging InputStaging { get; private set; }

        public GenerateWalls GenerateWallsRef { get; private set; }
        public ModeContainer<Interaction> m_FloorInteraction;
        public Recorder RecorderRef { get; private set; }

        public bool AllowLeftMouseInput { get; set; }
        public bool AllowMouseInteractionBecauseOfGui { get; set; }

        private readonly int UI_LAYER = Layers.ControllerUILayer;
        private PointerEventData m_PointerEventData;
        private bool m_HasBeenInitialised = false;

        public event Action<bool, InteractionWallPoint, InteractionWallWhole, PlotElement, Vector3> OnPoleSelected;
        public event Action<bool, int, InteractionWallGeneratedWhole, bool> OnWallSelected;
        public event Action<bool, List<InteractionGameObject>> OnObjectSelected;
        public event Action<bool, InteractionWallGeneratedWhole, bool> OnCeilingSelected;
        public event Action<bool, InteractionWallGeneratedWhole, bool> OnFloorSelected;

        public event Action OnAllAssetsLoaded;
        public event Action OnModeTransitionBegin;
        public event Action OnPlotModeSelected;
        public event Action OnStructureModeSelected;
        public event Action OnPlacementModeSelected;
        public event Action OnSoundModeSelected;
        public event Action OnShowRoomModeSelected;
        public event Action OnLightingModeSelected;
        public event Action OnStagingModeSelected;
        public event Action OnOSCModeSelected;

        public event Action<Interaction> OnModularModeSelected;
        public event Action OnModeTransitionEnd;

        public int m_InitialisationsToWaitForTotal = 0;
        public int m_InitialisationsToWaitForSoFar = 0;

        private StructureType m_StructureSwitch = StructureType.None;
        private BuilderMode m_CurrentMode = BuilderMode.NONE;
        public BuilderMode PrevMode { get; private set; }

        public bool PanMode { get; set; }
        public bool RotateItemMode { get; set; }

        public bool KeyboardInput { get; set; }

        private Interaction m_LastInteractions;


        public RuntimeGizmos.TransformGizmo TransformGizmoRef { get; private set; }

        private System.Diagnostics.Process m_EnttecClient;
        public BuilderMode CurrentMode
        {
            get { return m_CurrentMode; }
            set
            {
                if (value != m_CurrentMode)
                {
                    CleanAll(value);
                }
            }
        }

        private void CleanAll(BuilderMode value)
        {
            InputContents.CleanUp();
            InputFloorPlan.CleanUp();
            InputCeiling.CleanUp();
            InputSound.CleanUp();
            InputShowRoom.CleanUp();
            InputModular.CleanUp();
            InputLighting.CleanUp();
            InputStaging.CleanUp();

            switch (m_CurrentMode)
            {
                case BuilderMode.Plot:
                    m_LastInteractions = null;
                    break;
                case BuilderMode.Structure:
                    m_LastInteractions = null;
                    break;
                case BuilderMode.Placement:
                case BuilderMode.Sound:
                    m_LastInteractions = InputContents.GetCurrentInteractions().First();
                    break;
                case BuilderMode.ShowRoom:
                    m_LastInteractions = null;
                    break;
                case BuilderMode.Modular:
                    m_LastInteractions = InputContents.GetCurrentInteractions().First();
                    break;
                default:
                    break;
            }

            InputContents.ClearAllInteractionsWithModeChange();
            InputFloorPlan.ClearAllInteractionsWithModeChange();
            InputCeiling.ClearAllInteractionsWithModeChange();
            InputSound.ClearAllInteractionsWithModeChange();
            InputModular.ClearAllInteractionsWithModeChange();
            InputLighting.ClearAllInteractionsWithModeChange();
            InputStaging.ClearAllInteractionsWithModeChange();

            PrevMode = m_CurrentMode;
            m_CurrentMode = value;

            ModeStateChange(null);
            GC.Collect();
        }

        public void Initialise()
        {
            Core.PhotonMultiplayerRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayerNoVoice);
            Core.PhotonMultiplayerRef.ChangeRoom("HouseBuilderRoot");


            RecorderRef = new Recorder();
            if (Core.Network.Users.UserCount == 0)
            {
#if UNITY_EDITOR

                UnityEditor.EditorUtility.DisplayDialog("There are no current users", "This is Editor only warning", "OK");
#endif
            }

            Instance = this;
            Core.SceneLoader.SetMouseLockVisible(false, true);
            m_PositionGrid = this.AddComponent<PositionGrid>();

            m_MainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
            m_MainCamera.gameObject.ForceComponent<AudioListener>();
            m_MainCamera.gameObject.ForceComponent<SteamAudio.SteamAudioListener>();
            if (m_MainCamera.stereoTargetEye != StereoTargetEyeMask.None)
            {
                Debug.Log("Change camera to correct settings for non VR stuff");
                m_MainCamera.stereoTargetEye = StereoTargetEyeMask.None;
            }

            TransformGizmoRef = m_MainCamera.gameObject.ForceComponent<RuntimeGizmos.TransformGizmo>();
            TransformGizmoRef.space = RuntimeGizmos.TransformSpace.Local;

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
            EnvironmentLoader.OnEnvironmentsMenuOpened += OnEnvironmentsMenuOpened;
            m_InitialisationsToWaitForTotal++;

            var item = SteamAudio.SteamAudioManager.GetSingleton();
            item.LoadSaveNameOverride = "RoomBuilder";
            item.SceneEndsOverride = "_Main";
            item.CurrentFileNameAppend = "RoomBuilder";

            //var item = SteamAudio.SteamAudioManager.GetSingleton();
            //item.loadSaveOverride = "RoomBuilder";
            //item.sceneOverride = "_Main";

            Debug.Log("HouseBuilderRoot End Initialise");
        }



        private void OnEnvironmentLoadingComplete()
        {
            Debug.Log("HouseBuilderRoot OnEnvironmentLoadingComplete");
            HBGUIManager.Instance.m_StructureSwitchMenu.OnStructureSwitchChanged += OnStructureSwitchChanged;

            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            Core.SceneLoader.DisableMenu();
            EnvironmentLoader.OnEnvironmentsMenuOpened += OnEnvironmentsMenuOpened;

            m_MainCamera.enabled = true;

            m_LoadSaveHouseContentsData = new LoadSaveHouseContentsData();
            m_LoadSaveHouseWallsData = new LoadSaveHouseWallsData();
            m_LoadSaveGeneralData = new HouseBuilderGeneralJsonData();

            InputContents = this.AddComponent<HouseBuilderInputContents>();
            InputContents.Initialise();

            InputCeiling = this.AddComponent<HouseBuilderInputCeiling>();
            InputCeiling.Initialise();

            InputFloorPlan = this.AddComponent<HouseBuilderInputFloorPlan>();
            InputFloorPlan.Initialise();

            InputSound = this.AddComponent<HouseBuilderInputSound>();
            InputSound.Initialise();

            InputShowRoom = this.AddComponent<HouseBuilderInputShowRoom>();
            InputShowRoom.Initialise();

            InputLighting = this.AddComponent<HouseBuilderInputLighting>();
            InputLighting.Initialise();

            InputStaging = this.AddComponent<HouseBuilderInputStaging>();
            InputStaging.Initialise();

            InputModular = this.AddComponent<HouseBuilderInputModular>();
            InputModular.Initialise();

            InputCamera = this.AddComponent<HouseBuilderInputCamera>();
            InputCamera.Initialise();

            GenerateWallsRef = this.AddComponent<GenerateWalls>();
            GenerateWallsRef.Initialise(InputFloorPlan.ModeContainerRef.GetList);
            GenerateWallsRef.DeleteGeneratedWallAndGenerateWallsFromFloorPlan(null);

            m_FloorInteraction = new ModeContainer<Interaction>();

            InputCeiling.SetInputFloor(InputFloorPlan);

            Camera.main.gameObject.AddComponent<EPOOutline.Outliner>();
            Camera.main.fieldOfView = 60;
            var initEffect = EPOOutline.Outliner.Instance;

            m_PointerEventData = new PointerEventData(EventSystem.current);

            var items = FindObjectsOfType<InteractionFloorPlan>().ToList();
            foreach (var item in items)
            {
                m_FloorInteraction.AddGameObject(item);
            }

            GameObject test = new GameObject(nameof(RayCastTester));
            var ray = test.AddComponent<RayCastTester>();
            ray.Initialise();

            if (Core.Environment.Environments.Count == 1 && Core.Environment.CurrentEnvironment.IncludeCataloguePrefabs == false)
            {
                Debug.Log("HouseBuilderRoot UpdateLocalAssetsFromServer");
                HBGUIManager.Instance.m_Blackout.SetActive(false);
                AssetSyncMenu.Instance.SetVisible(true);
                AssetSyncMenu.Instance.SetProgressBar();
                AssetSyncMenu.Instance.SetProgressBarText("Calculating...");
                Core.Assets.UpdateLocalAssetsFromServer(AssetSyncMenu.Instance.SetProgressBarText, AssetSyncMenu.Instance.SetProgress, CloseAssetMenu);
            }
            else
            {
                CloseAssetMenu(0); // No assets need updating therefore this menu should not be visible
            }

            ClearAll();

            this.WaitUntil(1, () => (m_InitialisationsToWaitForTotal == m_InitialisationsToWaitForSoFar), () =>
            {
                CurrentMode = BuilderMode.Plot;
                HBGUIManager.Instance.m_Blackout.SetActive(false);
                m_HasBeenInitialised = true;

            });
            m_InitialisationsToWaitForSoFar++;
            RecorderRef.RecordEnabled = true;
            KeyboardInput = true;


#if DEVELOPMENT_BUILD
            Debug.LogError("Need a error for DebugStatsDisplayRef to stay on");
            Core.VisualLoggerRef.Show(true);
#endif

            this.WaitUntil(1, () => Core.PhotonGenericRef != null && Core.PhotonGenericRef.IsInitilised() == true, () =>
            {
                Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(OscController.OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE, (timeString) =>
                {
                    float time = float.Parse(timeString);
                    Debug.Log($"CollectGenericDataHit   {OscController.OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE}  {timeString}");
                    PlayAllSoundAndLighting(time);
                });

                Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(OscController.OscControllerRoot.GENERIC_SELECT_DMX_MESSAGE, (data) =>
                {
                    // can be more than one 
                    int dmx = int.Parse(data);
                    Debug.Log($"CollectGenericDataHit   {OscController.OscControllerRoot.GENERIC_SELECT_DMX_MESSAGE} {data}");
                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.ForceUnSelectObject());
                    foreach (var item in InputContents.ModeContainerRef.GetList)
                    {
                        if (item.InteractionRef.LightingRef != null)
                        {
                            if (item.InteractionRef.LightingRef.DmxStartNumber == dmx)
                            {
                                item.InteractionRef.ForceSelectObject();
                            }
                        }
                    }
                });

                Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(OscController.OscControllerRoot.GENERIC_SELECT_FEATURE_MESSAGE, (featureName) =>
                {
                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.ForceUnSelectObject());
                    foreach (var item in InputContents.ModeContainerRef.GetList)
                    {
                        if (item.InteractionRef.LightingRef != null)
                        {
                            if (item.InteractionRef.LightingRef.ContentLightingMetaDataRef.m_JsonFileName == featureName)
                            {
                                item.InteractionRef.ForceSelectObject();
                            }
                        }
                    }
                });

                Core.PhotonGenericRef.CollectGenericHeader(OscController.OscControllerRoot.GENERIC_SELECT_ALL_DMX_MESSAGE, () =>
                {
                    foreach (var item in InputContents.ModeContainerRef.GetList)
                    {
                        if (item.InteractionRef.LightingRef != null)
                        {
                            item.InteractionRef.ForceSelectObject();
                        }
                    }
                });

                Core.PhotonGenericRef.CollectGenericHeader(OscController.OscControllerRoot.GENERIC_UNSELECT_ALL_DMX_MESSAGE, () =>
                {
                    foreach (var item in InputContents.ModeContainerRef.GetList)
                    {
                        if (item.InteractionRef.LightingRef != null)
                        {
                            item.InteractionRef.ForceUnSelectObject();
                        }
                    }
                });
            });

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            this.WaitFor(5.0f, () => // delay to make sure everything is set up 
            {
                ConnectToEnttecClient();
            });
#else
            Debug.LogError("need to EnttecClient/Enttec Test.exe  on the mac");
#endif
        }

        private void ConnectToEnttecClient()
        {
            if (m_EnttecClient == null || m_EnttecClient.IsRunning() == false)
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, "EnttecClient/Enttec Test.exe");

                if (File.Exists(path))
                {
                    m_EnttecClient = new System.Diagnostics.Process();
                    m_EnttecClient.StartInfo.FileName = path;
                    m_EnttecClient.Start();
                }
                else
                {
                    Debug.LogError($"path does not exist : {path}");
                }
            }
        }

        [InspectorButton]
        private void LocalLoadFile()
        {
            Load(LOCAL_LOADSAVE, LoadSaveType.Local, null);
        }

        [InspectorButton]
        private void LocalSaveFile()
        {
            Save(LOCAL_LOADSAVE, LoadSaveType.Local, null);
        }




        private void CloseAssetMenu(float time)
        {
            //Initial mode is BuilderMode.Plot, which means this will go through mode switch at the last possible moment.
            ModeStateChange(() =>
            {
                this.WaitFor(time, () =>
                {
                    HBGUIManager.Instance.m_Blackout.SetActive(false);
                    OnAllAssetsLoaded?.Invoke();
                    AssetSyncMenu.Instance.SetVisible(false);
                    AllowLeftMouseInput = true;
                });
            });
        }

        internal void OnStructureSwitchChanged(StructureType currentSelection)
        {
            m_StructureSwitch = currentSelection;

            InputFloorPlan.ClearAllInteractions();
            InputCeiling.ClearAllInteractions();
            InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingUp());

            switch (m_StructureSwitch)
            {
                case StructureType.Ceiling:
                    InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingDown());
                    InputCeiling.ShowCorrectCeilingVisibility();
                    InputCeiling.ShowCorrectFloorVisibility();
                    break;

                case StructureType.Openings:
                    InputCeiling.ShowCorrectCeilingVisibility();
                    InputCeiling.ShowCorrectFloorVisibility();
                    break;

                case StructureType.Walls:
                    InputCeiling.ShowCorrectCeilingVisibility();
                    InputCeiling.ShowCorrectFloorVisibility();
                    break;

                case StructureType.None:
                case StructureType.Floor:
                    InputCeiling.ShowCorrectCeilingVisibility();
                    InputCeiling.ShowCorrectFloorVisibility();
                    break;
            }
        }

        private void ModeStateChange(Action callback)
        {
            this.WaitUntil(1, () => Core.PhotonGenericRef != null && Core.PhotonGenericRef.IsInitilised() == true, () =>
            {
                Core.PhotonGenericRef.SendGenericHeader(OscController.OscControllerRoot.GENERIC_RESET_ALL_MESSAGE);
            });

            InputContents.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputCeiling.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputFloorPlan.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputSound.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputModular.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputLighting.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;
            InputStaging.CurrentInputMode = HouseBuilderInput.InputMode.Nothing;

            OnModeTransitionBegin?.Invoke();

            OnWallDeselectedAll();
            OnCeilingFloorDeselectedAll();
            OnObjectDeselected();

            InputFloorPlan.ShowTarget(CurrentMode == BuilderMode.Plot);
            Debug.Log($"Mode set to {CurrentMode}");
            MeasurementLabelPanel.Instance.OpenInstant();

            HBGUIManager.Instance.ShowRoomMode(false);
            PositionGrid.Instance.ShowCurrentGridState();
            PositionGrid.Instance.RegenerateHeight();

            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.EnableLights(true));
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.RootGameObject.SetActive(true));
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.ResetConnectedSounds());
            OscController.OscControllerRoot.Instance?.PlayDataRef?.ResetTimeLine();
            RecorderRef.Record();
            switch (CurrentMode)
            {
                case BuilderMode.Plot:
                    OnPlotModeSelected?.Invoke();
                    PositionGrid.Instance.RegenerateHeight(true);
                    InputFloorPlan.ModeContainerRef.ShowGameObjects(true);
                    InputFloorPlan.ModeContainerRef.EnableAllColliders(true);

                    InputContents.ModeContainerRef.ShowGameObjects(true);
                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.EnableRenderer(false));
                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.EnableLights(false));

                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SetRenderModeFade());
                    GenerateWallsRef.GenerateAndShowWallsAndPlaceWallObjects(false);

                    InputCeiling.FloorModeContainerRefSolid.ShowGameObjects(false);
                    InputCeiling.CeilingModeContainerRefSolid.ShowGameObjects(false);

                    m_FloorInteraction.EnableAllColliders(true);

                    HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(false);
                    OnModeTransitionEnd?.Invoke();
                    break;

                case BuilderMode.Structure:
                    OnStructureModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        m_FloorInteraction.EnableAllColliders(false);
                        InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingUp());
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.RootGameObject.SetActive(e.InteractionRef.PlacementMetaData.Placement.IsCutout()));
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                        PositionGrid.Instance.RegenerateHeight(true);
                    });
                    break;

                case BuilderMode.Placement:
                    OnPlacementModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;

                case BuilderMode.Sound:
                    OnSoundModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SetRenderModeFadeNonSound());
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        PositionGrid.Instance.SetTempGridState(false);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;

                case BuilderMode.ShowRoom:
                    OnShowRoomModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        HBGUIManager.Instance.ShowRoomMode(true);
                        PositionGrid.Instance.SetTempGridState(false);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;



                case BuilderMode.Modular:
                    OnModularModeSelected?.Invoke(m_LastInteractions);
                    FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                    InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    break;
                case BuilderMode.OSC:
                    OnOSCModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        PositionGrid.Instance.SetTempGridState(false);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;
                case BuilderMode.Lighting:
                    OnLightingModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SetRenderModeFadeNonLighting());
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        PositionGrid.Instance.SetTempGridState(false);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;
                case BuilderMode.Staging:
                    OnStagingModeSelected?.Invoke();
                    ShowAll(() =>
                    {
                        ManualUpdateCeilingFloors();
                        InputCeiling.ShowCorrectCeilingVisibility();
                        InputCeiling.ShowCorrectFloorVisibility();
                        HBGUIManager.Instance.CameraWidgetRef.EnableRotationBtns(true);
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SetRenderModeFadeNonStaging());
                        OnModeTransitionEnd?.Invoke();
                        FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.DisableOutlineOverrideRender());
                    });
                    break;

                case BuilderMode.NONE:
                    break;
                default:
                    Debug.LogError($"{CurrentMode} not found");
                    break;
            }
            
            callback?.Invoke();
        }

        public void ShowAll(Action onFinish)
        {
            AllowLeftMouseInput = false;
            InputFloorPlan.ModeContainerRef.ShowGameObjects(false);
            InputFloorPlan.ModeContainerRef.EnableAllColliders(false);

            InputContents.ModeContainerRef.ShowGameObjects(true);
            InputContents.ModeContainerRef.EnableAllColliders(true);
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.UnSelectObject());
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SetOriginalRenderMode());

            InputCeiling.FloorModeContainerRefSolid.ShowGameObjects(true);
            InputCeiling.CeilingModeContainerRefSolid.ShowGameObjects(true);
            m_FloorInteraction.EnableAllColliders(true);

            if (PrevMode != BuilderMode.Plot && PrevMode != BuilderMode.Structure && CurrentMode != BuilderMode.Plot && CurrentMode != BuilderMode.Structure)
            {
                AllowLeftMouseInput = true;
                onFinish?.Invoke();
                return;
            }

            Resources.UnloadUnusedAssets();
            InputCeiling.GenerateCeilingsAndFloorsForRooms(() =>// needed for what side wall objects are on
            {
                InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());// make sure floor is facing up
                GenerateWallsRef.GenerateAndShowWallsAndPlaceWallObjects(true, () =>
                {
                    List<CatalogueGeneratedData> invalidItems = CollectInvalidWallItems();

                    if (invalidItems.Count > 0)
                    {
                        for (int i = 0; i < invalidItems.Count; i++)
                        {
                            Debug.Log($"wall item became invalid {invalidItems[i]}");
                            InputContents.ModeContainerRef.Delete(invalidItems[i]);
                        }

                        // need to regenerate AGAIN after items removed from wall 
                        GenerateWallsRef.GenerateAndShowWallsAndPlaceWallObjects(true, () =>
                        {
                            Debug.Log("GenerateAndShowWalls");
                            InputCeiling.ToggleTemporaryAllCeilingVisibility(true);
                            InputCeiling.ToggleTemporaryAllFloorVisibility(true);

                            PositionGrid.Instance.RegenerateHeight();
                            AllowLeftMouseInput = true;
                            onFinish?.Invoke();
                        });
                    }
                    else
                    {
                        Debug.Log("GenerateAndShowWalls");
                        InputCeiling.ToggleTemporaryAllCeilingVisibility(true);
                        InputCeiling.ToggleTemporaryAllFloorVisibility(true);

                        PositionGrid.Instance.RegenerateHeight();
                        AllowLeftMouseInput = true;
                        onFinish?.Invoke();
                    }
                });
            });
        }

        private List<CatalogueGeneratedData> CollectInvalidWallItems()
        {
            List<CatalogueGeneratedData> invalidItems = new List<CatalogueGeneratedData>();
            var wallStuff = InputContents.ModeContainerRef.GetList.FindAll(e => e.InteractionRef.PlacementMetaData.Placement.IsWall());
            foreach (var wallItem in wallStuff)
            {
                if (wallItem.m_WallObjectLinkerData == null)
                {
                    Debug.LogError($"wallItem, does not have linkage items :{wallItem.CatalogueEntryRef.FullName}, GUID: {wallItem.InteractionRef.m_RootItemsGeneratedGUID}");
                }
                else
                {
                    var room = GenerateWallsRef.GeneratedWholeList.FindLast(e => e.ThisWallIndex == wallItem.m_WallObjectLinkerData.m_WallIndex);
                    if (room != null)
                    {
                        if (wallItem.m_WallObjectLinkerData.m_WallPointIndex > room.m_WallGeneratedPointList.LastIndex())
                        {
                            Debug.LogError($"Item is out of range  last index is {room.m_WallGeneratedPointList.LastIndex()}   trying to access index {wallItem.m_WallObjectLinkerData.m_WallPointIndex}");
                            Debug.LogError($"Decrementing this index to see what happens");
                            wallItem.m_WallObjectLinkerData.m_WallPointIndex = wallItem.m_WallObjectLinkerData.m_WallPointIndex.Clamp(0, room.m_WallGeneratedPointList.LastIndex());

                        }
                        var generatedWallPoint = room.m_WallGeneratedPointList[wallItem.m_WallObjectLinkerData.m_WallPointIndex];

                        wallItem.InteractionRef.TestWallObjectIsStillValid(generatedWallPoint);
                        if (wallItem.InteractionRef.m_Outline.ItemSelected == EPOOutline.Outliner.EnumItemSelected.Invalid)
                        {
                            wallItem.InteractionRef.TestWallObjectIsStillValid(generatedWallPoint);
                            invalidItems.Add(wallItem);
                        }
                    }
                    else
                    {
                        Debug.LogError($"Could not find Whole wall index {wallItem.m_WallObjectLinkerData.m_WallIndex}");
                    }
                }
            }

            return invalidItems;
        }

        public void DeleteWall(ref CombinedDataIndex data)
        {
            InputContents.ClearAllInteractions();


            // collect list of connected objects to the wall
            List<CatalogueGeneratedData> items = new List<CatalogueGeneratedData>();
            foreach (var content in InputContents.ModeContainerRef.GetList)
            {
                if (content.m_WallObjectLinkerData != null)
                {
                    if (content.m_WallObjectLinkerData.m_WallIndex == data.m_Index)
                    {
                        items.Add(content);
                    }
                }
            }

            // delete items
            foreach (var item in items)
            {
                InputContents.ModeContainerRef.Delete(item.GeneratedItemRef);
            }

            //refresh structure
            RefreshWall();
        }

        public void RefreshWall(Action callback = null)
        {
            ShowAll(() =>
            {
                ManualUpdateCeilingFloors();

                OnStructureSwitchChanged(m_StructureSwitch);
                m_FloorInteraction.EnableAllColliders(false);

                callback?.Invoke();
            });
        }

        public void ReselectWalls(CombinedDataIndex data)
        {
            InputContents.ClearAllInteractions();

            foreach (var structure in GenerateWallsRef.GeneratedWholeList)
            {
                if (structure.FloorPlanJsonDataRef == data.m_Data.FloorPlanJsonDataRef)
                {
                    foreach (var wall in structure.m_WallGeneratedPointList)
                    {
                        string resultString = wall.name.GetNumberString();
                        if (Int32.Parse(resultString) == data.m_Index)
                        {
                            InputContents.AddCurrentInteraction(wall);
                        }
                    }
                }
            }

        }


        public void RefreshCeiling(Action callback)
        {
            AllowLeftMouseInput = false;
            InputCeiling.GenerateCeilingsAndFloorsForRooms(() =>
            {
                AllowLeftMouseInput = true;
                ManualUpdateCeilingFloors();

                OnStructureSwitchChanged(m_StructureSwitch); //shows and hides floor/ceiling as necessary
                callback?.Invoke();
            });
        }


        public void RefreshFloor(Action callback)
        {
            AllowLeftMouseInput = false;
            InputCeiling.GenerateCeilingsAndFloorsForRooms(() =>
            {
                AllowLeftMouseInput = true;
                InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());
                InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());
                OnStructureSwitchChanged(m_StructureSwitch); //shows and hides floor/ceiling as necessary
                callback?.Invoke();
            });
        }

        public void OnNewPoleSelected(InteractionWallPoint pole, InteractionWallWhole dottedWall) => OnPoleSelected?.Invoke(true, pole, dottedWall, PlotElement.Point, Vector3.zero);
        public void OnPoleDeselected() => OnPoleSelected?.Invoke(false, null, null, PlotElement.Point, Vector3.zero);

        public void OnNewDashesBetweenPolesSelected(InteractionWallPoint pole, InteractionWallWhole dottedWall, Vector3 mousePoint) => OnPoleSelected?.Invoke(true, pole, dottedWall, PlotElement.DashesBetweenPoles, mousePoint);

        public void OnNewObjectSelected(List<InteractionGameObject> selectedObjects) => OnObjectSelected?.Invoke(true, selectedObjects);
        public void OnObjectDeselected() => OnObjectSelected?.Invoke(false, null);

        public void OnNewWallSelected(int wallIndex, InteractionWallGeneratedWhole structureData) => OnWallSelected?.Invoke(true, wallIndex, structureData, false);
        public void OnWallDeselected(InteractionWallGeneratedWhole structureData) => OnWallSelected?.Invoke(false, 0, structureData, false);

        public void OnWallDeselectedAll() => OnWallSelected?.Invoke(false, 0, null, true);

        public void OnCeilingFloorDeselectedAll()
        {
            OnCeilingSelected?.Invoke(false, null, true);
            OnFloorSelected?.Invoke(false, null, true);
        }

        public void OnNewCeilingFloorSelected(InteractionWallWhole interactionWallWholeRef)
        {
            var item = GenerateWallsRef.GeneratedWholeList.Find(e => e.HostWholeRef == interactionWallWholeRef);
            if (m_StructureSwitch == StructureType.Ceiling)
            {
                OnCeilingSelected?.Invoke(true, item, false);
            }
            else if (m_StructureSwitch == StructureType.Floor)
            {
                OnFloorSelected?.Invoke(true, item, false);
            }
        }

        public void OnCeilingFloorDeselected(InteractionWallWhole interactionWallWholeRef)
        {
            var item = GenerateWallsRef.GeneratedWholeList.Find(e => e.HostWholeRef == interactionWallWholeRef);
            if (m_StructureSwitch == StructureType.Ceiling)
            {
                OnCeilingSelected?.Invoke(false, item, false);
            }
            else if (m_StructureSwitch == StructureType.Floor)
            {
                OnFloorSelected?.Invoke(false, item, false);
            }
        }

        private void OnEnvironmentsMenuOpened(bool input)
        {
        }

        private void OnDestroy()
        {

            //not used as cannot change back , because i disabled it Core.SceneLoader.SetInputSystemToVR(true);
            EnvironmentLoader.OnEnvironmentsMenuOpened -= OnEnvironmentsMenuOpened;
            StopAllCoroutines();
            m_EnttecClient.Kill();
        }

        private void Update()
        {
#if DEVELOPMENT_BUILD
            if (Debug.developerConsoleVisible == false)
            {
                Core.VisualLoggerRef.Show(false);
            }
#endif

            if(Camera.main == null)
            {
                return;
            }

            if (false == m_HasBeenInitialised || KeyboardInput == false)
            {
                return;
            }


            if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.GridToggle))
            {
                HBGUIManager.Instance.SceneSettingsMenuRef.ToggleGrid();
            }

            if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.RecordStepForward))
            {
                if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Lighting)
                {
                    KeyboardInput = false;
                    RecorderRef.StepForward(() =>
                    {
                        KeyboardInput = true;
                    });
                }
                else
                {
                    Debug.LogError("This has not been impliment in this mode yet");
                }
            }
            if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.RecordStepBack))
            {
                if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Lighting)
                {
                    KeyboardInput = false;
                    RecorderRef.StepBack(() =>
                    {
                        KeyboardInput = true;
                    });
                }
                else
                {
                    Debug.LogError("This has not been impliment in this mode yet");
                }
            }

            WallPointMeasurementLabelPanel.Instance.ClearLabel();

            if (CurrentMode == BuilderMode.Staging && InputStaging.CurrentInputMode == HouseBuilderInput.InputMode.Placing)
            {
                InputStaging.ManualUpdate();
            }


            if (IsMouseOverGUI())
            {
                return;
            }
            InputCamera.ManualUpdate();

            if (PanMode == true)
            {
                return;
            }



            if (true == AllowLeftMouseInput && true == AllowMouseInteractionBecauseOfGui)
            {
                switch (CurrentMode)
                {
                    case BuilderMode.Plot:
                        InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.EnableRenderer(false));                      
                        InputFloorPlan.ManualUpdate();
                        break;

                    case BuilderMode.Structure:
                        InputContents.SetPlacementType(m_StructureSwitch);
                        InputContents.ManualUpdate();
                        InputCeiling.ManualUpdate();
                        break;

                    case BuilderMode.Placement:
                        InputContents.SetPlacementType(m_StructureSwitch);
                        InputContents.ManualUpdate();
                        break;

                    case BuilderMode.Sound:
                        InputSound.ManualUpdate();
                        break;

                    case BuilderMode.ShowRoom:
                        InputShowRoom.ManualUpdate();
                        break;

                    case BuilderMode.Modular:
                        InputModular.ManualUpdate();
                        break;

                    case BuilderMode.Lighting:
                        InputLighting.ManualUpdate();
                        break;

                    case BuilderMode.Staging:
                        InputStaging.ManualUpdate();
                        break;
                    case BuilderMode.OSC:
                        //do nothing
                        break;
                    default:
                        Debug.LogError($"Update: {CurrentMode} not found");
                        break;
                }
            }
        }

        public void ManualUpdateCeilingFloors()
        {
            
            if (CurrentMode == BuilderMode.Structure)
            {
                InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());
                InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());
            }
            else
            {
                InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingUp());
                InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingDown());
            }
        }

        public bool IsMouseOverGUI()
        {
            if (true == EventSystem.current.IsPointerOverGameObject())
            {
                m_PointerEventData.position = HouseBuilderConstants.MousePosition();
                List<RaycastResult> raycastResults = new List<RaycastResult>();
                EventSystem.current.RaycastAll(m_PointerEventData, raycastResults);
                foreach (var item in raycastResults)
                {
                    if (item.gameObject.layer == UI_LAYER)
                    {
                        // this means there ig GUI in the way
                        // this is needed to stop raycasts for putting object down
                        return true;
                    }
                }
            }
            return false;
        }

        public void Save(string fileName, LoadSaveType loadSave, Action callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                if (Core.Network.Users.CurrentUser == null)
                {
                    GenericPromptMenu.Instance.ShowOkayMenu("Please log in before saving your scene.");
                    return;
                }
            }


            List<bool> pass = new List<bool>();
            TaskAction task = new TaskAction(3, () =>
            {
                if (pass.Contains(false) == true)
                {
                    Debug.LogError($"Save files saved, ERROR");
                }
                else
                {
                    Debug.Log($"Save files saved");
                }
                callback?.Invoke();
            });

            m_LoadSaveHouseContentsData.SaveHouseContentsData(fileName, loadSave, InputContents.ModeContainerRef.GetList, (valid) => { pass.Add(valid); task.Increment(); });
            m_LoadSaveHouseWallsData.SaveDataFromFloorPlanData(fileName, loadSave, InputFloorPlan.ModeContainerRef.GetList, (valid) => { pass.Add(valid); task.Increment(); });
            m_LoadSaveGeneralData.SaveData(fileName, loadSave, (valid) => { pass.Add(valid); task.Increment(); });
            Debug.Log("Save");
        }

        public void Load(HouseContentsData houseClone, FloorCollectionData floorClone, GeneralData generalClone, Action callback)
        {
            TaskAction task = new TaskAction(3, () => 
            {
                // Is done in LoadHouseContentsDataAndCreateObjects, leave this here so i know not to do it, ParentLoadedItems();
                ModeStateChange(callback);
                CleanAll(CurrentMode);
                FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
            });

            m_LoadSaveHouseContentsData.LoadHouseContentsDataAndCreateObjects(houseClone, InputContents, InputContents.DeleteAll, InputContents.ModeContainerRef.AddGameObject, () =>
            {
                task.Increment();
            });
            m_LoadSaveHouseWallsData.LoadFloorDataAndGenerateWalls(floorClone, InputFloorPlan.ModeContainerRef.DeleteAll, InputFloorPlan.ModeContainerRef.AddGameObject, () =>
            {
                task.Increment();
            });
            m_LoadSaveGeneralData.LoadAndSetData(generalClone, () =>
            {
                task.Increment();
            });
        }

        public void Load(string fileName, LoadSaveType loadSave, Action callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                if (Core.Network.Users.CurrentUser == null)
                {
                    GenericPromptMenu.Instance.ShowOkayMenu("Please log in before loading a scene.");
                    return;
                }
            }

            HBGUIManager.Instance.SoundSimulationMenuRef.TurnOffSPLMap();
            Core.PhotonMultiplayerRef.ChangeRoom($"{Core.Network.Users.CurrentUser.m_UserName}_{fileName}");


                AssetSyncMenu.Instance.SetVisible(true);
                AssetSyncMenu.Instance.SetHeaderText("Loading Scene.....");
                AssetSyncMenu.Instance.SetActiveSpinning();


                TaskAction task = new TaskAction(3, () =>
                {
                    // Is done in LoadHouseContentsDataAndCreateObjects, leave this here so i know not to do it, ParentLoadedItems();
                    ModeStateChange(callback);
                    CleanAll(CurrentMode);
                });

                m_LoadSaveHouseContentsData.LoadHouseContentsDataAndCreateObjects(fileName, loadSave, InputContents, InputContents.DeleteAll, InputContents.ModeContainerRef.AddGameObject, () =>
                {
                    task.Increment();
                });
                m_LoadSaveHouseWallsData.LoadFloorDataAndGenerateWalls(fileName, loadSave, InputFloorPlan.ModeContainerRef.DeleteAll, InputFloorPlan.ModeContainerRef.AddGameObject, () =>
                {
                    task.Increment();
                });
                m_LoadSaveGeneralData.LoadAndSetData(fileName, loadSave, () =>
                {
                    task.Increment();
                });
        }

        private void ParentLoadedItems()
        {
            var offset = new Vector3(0, Interaction.TINY_AMOUNT_OFFSET, 0);
            RecorderRef.RecordEnabled = false;
            foreach(var item in InputContents.ModeContainerRef.GetList)
            {
                var interactionRef = (InteractionGameObjectMove)item.InteractionRef;
                if (interactionRef.PlacementMetaData.Placement.IsWall() == false && interactionRef.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.Ceiling)
                {
                    if (Physics.Raycast(interactionRef.RootTransform.position + offset, Vector3.down, out RaycastHit hit) == true)
                    {
                        var collidInteraction = hit.collider.GetComponentInParent<Interaction>();

                        if (collidInteraction != null && ((collidInteraction is InteractionCeilingAndFloor) == false) && ((collidInteraction is InteractionFloorPlan) == false))
                        {
                            if (collidInteraction.PlacementMetaData.Placement.IsWall() == false && collidInteraction.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.Ceiling)
                            {
                                interactionRef.RootTransform.SetParent(collidInteraction.RootTransform);
                            }
                        }
                    }
                }
            }
            RecorderRef.RecordEnabled = true;
        }

        public void ParentItems(List<CatalogueGeneratedData> items)
        {
            var offset = new Vector3(0, Interaction.TINY_AMOUNT_OFFSET, 0);
            RecorderRef.RecordEnabled = false;
            foreach (var item in items)
            {
                var interactionRef = (InteractionGameObjectMove)item.InteractionRef;
                if (interactionRef.PlacementMetaData.Placement.IsWall() == false && interactionRef.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.Ceiling)
                {
                    if (Physics.Raycast(interactionRef.RootTransform.position + offset, Vector3.down, out RaycastHit hit) == true)
                    {
                        var interaction = hit.collider.GetComponentInParent<Interaction>();
                        if (interaction != null && ((interaction is InteractionCeilingAndFloor) == false) && ((interaction is InteractionFloorPlan) == false))
                        {
                            interactionRef.RootTransform.SetParent(interaction.RootTransform);
                        }
                    }
                }
            }
            RecorderRef.RecordEnabled = true;
            RecorderRef.Record();
        }

        public void ParentItem(CatalogueGeneratedData item)
        {
            if (item.InteractionRef.PlacementMetaData.Placement.IsCutout() == false)
            {
                var offset = new Vector3(0, Interaction.TINY_AMOUNT_OFFSET, 0);
                RecorderRef.RecordEnabled = false;

                var interactionRef = (InteractionGameObjectMove)item.InteractionRef;
                if (interactionRef.PlacementMetaData.Placement.IsWall() == false && interactionRef.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.Ceiling)
                {
                    if (Physics.Raycast(interactionRef.RootTransform.position + offset, Vector3.down, out RaycastHit hit) == true)
                    {
                        var interaction = hit.collider.GetComponentInParent<Interaction>();
                        if (interaction != null && ((interaction is InteractionCeilingAndFloor) == false) && ((interaction is InteractionFloorPlan) == false))
                        {
                            interactionRef.RootTransform.SetParent(interaction.RootTransform);
                        }
                    }
                }
                RecorderRef.RecordEnabled = true;
                RecorderRef.Record();
            }
        }


        public void DeleteFiles(string fileName, Action callback)
        {
            if (Core.Network.Users.CurrentUser != null)
            {
                List<bool> pass = new List<bool>();
                TaskAction task = new TaskAction(3, () =>
                {
                    if (pass.Contains(false) == true)
                    {
                        Debug.LogError($"Save files deleted, ERROR");
                    }
                    else
                    {
                        Debug.Log($"Save files deleted");
                    }
                    callback?.Invoke();
                });

                Core.Network.DeleteManifestJsonFile(fileName + HouseBuilderConstants.CONTENTS_JSON, (valid) => { pass.Add(valid); task.Increment(); });
                Core.Network.DeleteManifestJsonFile(fileName + HouseBuilderConstants.FLOOR_JSON, (valid) => { pass.Add(valid); task.Increment(); });
                Core.Network.DeleteManifestJsonFile(fileName + HouseBuilderConstants.GENERAL_JSON, (valid) => { pass.Add(valid); task.Increment(); });
            }
            else
            {
                GenericPromptMenu.Instance.ShowOkayMenu("Please log in before deleting a scene.");
            }
        }

        public void ClearAll()
        {
            InputContents.DeleteAll();
            InputFloorPlan.ModeContainerRef.DeleteAll();
            m_LoadSaveGeneralData.ResetData();
        }


        public void QuitApplication()
        {
            GenericPromptMenu.Instance.ShowYesNoMenu("Are you sure you want to quit the application?", () =>
            {
                Application.Quit();
            }, null);
        }


        public void TakeScreenshot()
        {
            StartCoroutine(InternalTakeScreenshot());
        }

        private IEnumerator InternalTakeScreenshot()
        {
            var picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            int count = 0;
            string path = picturesDir + $"/RoomBuilder_{count}.png";

            while (File.Exists(path) == true)
            {
                count++;
                path = picturesDir + $"/RoomBuilder_{count}.png";
            }

            int resolutionMultiple = 2; // this was 4 which makes a picture of 103meg, 2 makes picture size of 27meg
            HBGUIManager.Instance.SetActive(false);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path, resolutionMultiple);
            yield return new WaitForEndOfFrame();
            HBGUIManager.Instance.SetActive(true);
            GenericPromptMenu.Instance.ShowOkayMenu("Screenshot saved in Pictures as " + $"/RoomBuilder_{count}.png" + ".");
        }


        public void ImportFromDirectory()
        {
            string folderPath = Core.LoadFromFileFBXRef.GetSearchDirectory();
            string file = Crosstales.FB.FileBrowser.OpenSingleFile("Open single file", folderPath, new Crosstales.FB.ExtensionFilter("CAD", "obj", "fbx", "gltf2", "stl", "ply", "3mf"));
            LoadFile(file);
        }

        private static void LoadFile(string file)
        {
            if (string.IsNullOrEmpty(file) == false)
            {
                Core.LoadFromFileFBXRef.SetSearchDirectory(file);
                HouseBuilderRoot.Instance.AllowLeftMouseInput = false;
                var data = Core.LoadFromFileFBXRef.CreateCatalogueEntry(file);
                Core.Mono.WaitForFrame(() =>
                {
                    LoadSaveHouseContentsData.GenerateCataloguePrefab(Core.Mono, 0, data, null, HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AddGameObject, GenerateCatalogueType.CreateInGame, null, (item) =>
                    {
                        Instance.RecorderRef.Record();

                        Core.Mono.WaitForFrames(2, () =>
                        {
                            if (item != null)
                            {
                                HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(item.InteractionRef);
                                HouseBuilderRoot.Instance.AllowLeftMouseInput = true;
                            }
                            else
                            {
                                HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(null);
                                HouseBuilderRoot.Instance.AllowLeftMouseInput = true;
                            }
                        });
                    });
                });
            }
        }




        public void PlayAllSoundAndLighting(float time)
        {

            OscController.OscControllerRoot.Instance.PlayDataRef.PlayTimeLine();
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.SetTimeConnectedSounds(time));
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.PlayConnectedSounds()); 
        }

        public void PlayAllLighting(float time)
        {
            Core.PhotonGenericRef.SendGenericHeaderAndData(OscController.OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE, time.ToString());
            OscController.OscControllerRoot.Instance.PlayDataRef.PlayTimeLine();
        }

        public void StopAllSoundAndLighting()
        {
            Core.PhotonGenericRef.SendGenericHeader(OscController.OscControllerRoot.GENERIC_STOP_ALL_MESSAGE);
            OscController.OscControllerRoot.Instance.PlayDataRef.StopTimeLine();
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.StopConnectedSounds());
        }

        public void ResetAllSoundAndLighting()
        {
            Core.PhotonGenericRef.SendGenericHeader(OscController.OscControllerRoot.GENERIC_RESET_ALL_MESSAGE);
            OscController.OscControllerRoot.Instance.PlayDataRef.ResetTimeLine();
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.ResetConnectedSounds());
        }
    }
}