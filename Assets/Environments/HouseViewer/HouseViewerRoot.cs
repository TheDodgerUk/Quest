using HouseBuilder;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HouseViewer
{
    public class HouseViewerRoot : MonoBehaviour
    {
        public static HouseViewerRoot Instance { get; set; }

        private HouseBuilder.LoadSaveHouseContentsData m_LoadSaveHouseContentsData;
        private HouseBuilder.LoadSaveHouseWallsData m_LoadSaveHouseWallsData;
        private HouseBuilderGeneralJsonData m_LoadSaveGeneralData;

        public HouseBuilderInputContents InputContents { get; private set; }
        public HouseBuilderInputCeiling InputCeiling { get; private set; }
        public HouseBuilderInputFloorPlan InputFloorPlan { get; private set; }
        public GenerateWalls GenerateWallsRef { get; private set; }

#if Photon
        private PhotonMultiplayer PhotonMultiplayerRef;
#endif

        public string CurrentFileName { get; private set; }

        public RoomViewerControllerInteraction ControllerRef { get; private set; }


        public void Initialise()
        {
 #if Photon
            PhotonMultiplayerRef = Core.PhotonMultiplayerRef;
            PhotonMultiplayerRef.Initialise(PhotonMultiplayer.NetworkType.FullPlayer);
            PhotonMultiplayerRef.ChangeRoom("HouseViewerRoot");
#endif
            RenderSettings.ambientLight = new Color(0, 0, 0);
            Instance = this;
            m_LoadSaveHouseContentsData = new HouseBuilder.LoadSaveHouseContentsData();
            m_LoadSaveHouseWallsData = new HouseBuilder.LoadSaveHouseWallsData();
            m_LoadSaveGeneralData = new HouseBuilder.HouseBuilderGeneralJsonData();

            GenerateWallsRef = this.gameObject.AddComponent<GenerateWalls>();

            InputFloorPlan = this.AddComponent<HouseBuilderInputFloorPlan>();
            InputFloorPlan.Initialise();
            InputCeiling = this.gameObject.AddComponent<HouseBuilderInputCeiling>();
            InputCeiling.Initialise();
            InputCeiling.SetInputFloor(InputFloorPlan);

            InputContents = this.AddComponent<HouseBuilderInputContents>();
            InputContents.Initialise();

            GenerateWallsRef.Initialise(InputFloorPlan.ModeContainerRef.GetList);

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            var item = SteamAudio.SteamAudioManager.GetSingleton();
            item.LoadSaveNameOverride = "RoomBuilder";
            item.SceneEndsOverride = "_HouseViewerRoot";
            item.CurrentFileNameAppend = "RoomBuilder";

#if !VR_INTERACTION
#if UNITY_EDITOR
            UnityEditor.EditorUtility.DisplayDialog("ERROR", "Needs to be VR_INTERACTION", "ok");
#endif
#endif
            var audioListener = Camera.main.GetComponentInChildren<AudioListener>();
            audioListener.gameObject.ForceComponent<SteamAudio.SteamAudioListener>();

            OscController.DataContainer.CreateInstance();
            FixtureContainer.CreateInstance();
            FixtureContainer.Instance.Initialise();
        }

        private void OnEnvironmentLoadingComplete()
        {
            Camera.main.gameObject.AddComponent<EPOOutline.Outliner>();

            Debug.LogError("VOMIT, messes with eyes, needs fixing PostProcessLayer   FISH eye ");
            ////////////var post = Camera.main.gameObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>();
            ////////////post.volumeLayer = Layers.EnvironmentMask;
            ////////////post.volumeTrigger = Camera.main.transform;
            ////////////post.Init(Resources.Load<UnityEngine.Rendering.PostProcessing.PostProcessResources>("PostProcessResources"));


            FloorAndSkyboxManager.Instance.ToggleCompass(false);
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;

            GetSaveFiles();

            if (Core.Environment.Environments.Count == 1 && Core.Environment.CurrentEnvironment.IncludeCataloguePrefabs == false)
            {
                AssetSyncMenu.Instance.SetVisible(true);
                AssetSyncMenu.Instance.SetProgressBar();
                AssetSyncMenu.Instance.gameObject.PlaceInfrontMainCamera(3f);
                Core.Assets.UpdateLocalAssetsFromServer(AssetSyncMenu.Instance.SetProgressBarText, AssetSyncMenu.Instance.SetProgress, EnableLoginSelection);
            }
            else
            {
                EnableLoginSelection(0f);
            }
#if Photon
            this.WaitUntil(1, () => Core.PhotonGenericRef != null && Core.PhotonGenericRef.IsInitilised() == true, () =>
            {
                Core.PhotonGenericRef.CollectGenericHeaderAndData<string>(OscController.OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE, (timeString) =>
                {
                    float time = float.Parse(timeString);
                    PlaySounds(time);
                });
                Core.PhotonGenericRef.CollectGenericHeader(OscController.OscControllerRoot.GENERIC_STOP_ALL_MESSAGE, () =>
                {
                    StopAllSoundAndLighting();
                });
                Core.PhotonGenericRef.CollectGenericHeader(OscController.OscControllerRoot.GENERIC_RESET_ALL_MESSAGE, () =>
                {
                    ResetAllSoundAndLighting();
                });
            });
#endif


            //Core.DebugStatsDisplayRef.Show(true);
        }


        private void EnableLoginSelection(float time)
        {
            GC.Collect();

            var initEffect = EPOOutline.Outliner.Instance;
            if (initEffect != null)
            {
                initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Invalid, Color.white);
                initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Selected, Color.white);
                initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Valid, Color.white);
                //////initEffect.Data.lineThickness = 1f;
                //////initEffect.Data.lineIntensity = 10;
                //////initEffect.Data.fillAmount = 0;
                //////initEffect.additiveRendering = true;
            }

            InputManagerVR.Instance.AnySubscription.Grab.Clear();

            this.WaitUntil(1, () => UserSelectionMenu.Instance.HasBeenInitilised, () =>
            {
                this.WaitFor(time, () =>
                {
                    AssetSyncMenu.Instance.SetVisible(false);
                    UserSelectionMenu.Instance.gameObject.PlaceInfrontMainCamera(3);
                    UserSelectionMenu.Instance.SetActive(true);
                    UserSelectionMenu.Instance.AssignLoggedInCallback(() =>
                    {
                        ControllerRef = this.AddComponent<RoomViewerControllerInteraction>();
                    });
                });
            });
        }

        public void GetSaveFiles()
        {
            if (Core.Network.Users.CurrentUser != null)
            {
                Core.Network.GetManifestJsonList(HouseBuilderConstants.CONTENTS_JSON, (list) =>
                 {
                     FileLoaderMenu.Instance.SetupList(list);
                 });
            }
        }

        [InspectorButton]
        public void Load(string fileToLoad, LoadSaveType loadsave)
        {
            CurrentFileName = fileToLoad;

            if (loadsave == LoadSaveType.Network)
            {
                if (Core.Network.Users.CurrentUser == null)
                {
                    return;
                }
            }

#if Photon
            PhotonMultiplayerRef.ChangeRoom($"{Core.Network.Users.CurrentUser.m_UserName}_{fileToLoad}");
#endif
            GC.Collect();
            //3rd
            TaskAction allSetup = new TaskAction(1, () =>
            {
                AssetSyncMenu.Instance.SetVisible(true);
                AssetSyncMenu.Instance.SetActiveSpinning();
                AssetSyncMenu.Instance.SetHeaderText("Loading local baked data...");

                this.WaitFor(1.0f, () =>
                {
                    Core.Audio.LoadSaveGeneratedSoundRef.ViewerDownloadSound(() =>
                    {
                        AssetSyncMenu.Instance.SetVisible(false);

                        if (HouseViewerRoot.Instance != null && HouseViewerRoot.Instance.InputContents != null)
                        {
                            List<CatalogueGeneratedData> list = HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList;
                            float maxValue = 0f;
                            foreach (var item in list)
                            {
                                maxValue = Mathf.Max(maxValue, item.InteractionRef.SteamSoundRef.GetConnectedSoundsLength());
                            }
                            SoundLightMenu.Instance.SetSliderMaxValue(maxValue);
                            OscController.OscControllerRoot.Instance.TimeLinePanelRef.SetTimeLineMaxValue(maxValue);
                        }

                        PlayAllSoundAndLighting(0f);
                    });
                });
            });


            //2nd
            TaskAction allItemsLoaded = new TaskAction(3, () =>
            {
                AssetSyncMenu.Instance.SetHeaderText("Initialising Scene.....");
                this.WaitForOp(Resources.UnloadUnusedAssets(), (completed) =>
                {
                    InputFloorPlan.ModeContainerRef.ShowGameObjects(false);
                    InputFloorPlan.ModeContainerRef.EnableAllColliders(false);

                    InputContents.ModeContainerRef.ShowGameObjects(true);
                    InputContents.ModeContainerRef.EnableAllColliders(true);

                    InputCeiling.FloorModeContainerRefSolid.ShowGameObjects(true);
                    InputCeiling.CeilingModeContainerRefSolid.ShowGameObjects(true);

                    InputCeiling.ToggleTemporaryAllCeilingVisibility(true);
                    InputCeiling.ToggleTemporaryAllFloorVisibility(true);

                    InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());// make sure floor is facing up
                    InputCeiling.GenerateCeilingsAndFloorsForRooms(() =>// needed for what side wall objects are on
                    {
                        InputCeiling.ToggleTemporaryAllCeilingVisibility(true);
                        InputCeiling.ToggleTemporaryAllFloorVisibility(true);
                        InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());// make sure floor is facing up
                        GenerateWallsRef.GenerateAndShowWallsAndPlaceWallObjects(true, () =>
                        {
                            Debug.Log("GenerateAndShowWalls");
                            InputCeiling.GenerateCeilingsAndFloorsForRooms(null); // to correctly set the items
                            InputCeiling.ShowCorrectCeilingVisibility();
                            InputCeiling.ShowCorrectFloorVisibility();

                            InputContents.ModeContainerRef.EnableAllColliders(true);

                            InputCeiling.CeilingModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());
                            InputCeiling.FloorModeContainerRefSolid.GetList.ForEach(e => e.SetLookingViaCamera());

                            // needs extra delay for colliders
                            this.WaitForFrames(5, () =>
                            {
                                AssetSyncMenu.Instance.SetVisible(false);
                                GenerateWallsRef.CutoutWallsColliders(false);
                                InputContents.ModeContainerRef.DisableMainColliderInVR();
                                FloorAndSkyboxManager.Instance.ToggleParticleSystems(true);
                                FloorAndSkyboxManager.Instance.UpdateReflectionProbe();
                                allSetup.Increment();
                            });
                        });
                    });
                });
            });

            //1st
            AssetSyncMenu.Instance.SetVisible(true);
            AssetSyncMenu.Instance.SetHeaderText("Loading Scene.....");
            AssetSyncMenu.Instance.SetActiveSpinning();
            AssetSyncMenu.Instance.gameObject.PlaceInfrontMainCamera(3f);


            m_LoadSaveHouseContentsData.LoadHouseContentsDataAndCreateObjects(fileToLoad, loadsave, InputContents, InputContents.DeleteAll, InputContents.ModeContainerRef.AddGameObject, () =>
            {
                allItemsLoaded.Increment();
            });
            m_LoadSaveHouseWallsData.LoadFloorDataAndGenerateWalls(fileToLoad, loadsave, InputFloorPlan.ModeContainerRef.DeleteAll, InputFloorPlan.ModeContainerRef.AddGameObject, () =>
            {
                allItemsLoaded.Increment();
            });
            m_LoadSaveGeneralData.LoadAndSetData(fileToLoad, loadsave, () =>
            {
                allItemsLoaded.Increment();
            });

            InputFloorPlan.ShowTarget(false);

            Debug.Log("Load");
        }

        private void DestroyAllFlags()
        {
            foreach (GameObject gameObj in FindObjectsOfType(typeof(GameObject)) as GameObject[])
            {
                if (gameObj.name == HouseBuilderConstants.WALL_INDICATOR_FLAGPOLE)
                {
                    Destroy(gameObj);
                }
            }
        }

        public void RoomChoice()
        {
            //if (m_ControlMode == ControlMode.Interaction)
            {
                bool active = FileLoaderMenu.Instance.isActiveAndEnabled;
                active = !active;
                FileLoaderMenu.Instance.gameObject.SetActive(active);

                Core.Network.GetManifestJsonList(HouseBuilderConstants.CONTENTS_JSON, (list) =>
                {
                    FileLoaderMenu.Instance.SetupList(list);
                });
            }
        }

        private void OnSoundLightPress(ControllerStateInteraction interaction, bool sendPhotonMessage = true)
        {
            //if (m_ControlMode == ControlMode.Interaction)
            {
                bool active = SoundLightMenu.Instance.isActiveAndEnabled;
                active = !active;
                SoundLightMenu.Instance.gameObject.SetActive(active);
            }
        }


        public void PlayAllSoundAndLighting(float time)
        {
            OscController.OscControllerRoot.Instance.TimeLinePanelRef.SetTimeLineValue(time);
            OscController.OscControllerRoot.Instance.PlayDataRef.PlayTimeLine();
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.SetTimeConnectedSounds(time));
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.PlayConnectedSounds());
        }

        public void PlaySounds(float time)
        {
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.SetTimeConnectedSounds(time));
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.PlayConnectedSounds());
        }

        public void StopAllSoundAndLighting()
        {
            OscController.OscControllerRoot.Instance.PlayDataRef.StopTimeLine();
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.StopConnectedSounds());
        }

        public void ResetAllSoundAndLighting()
        {
            OscController.OscControllerRoot.Instance.PlayDataRef.ResetTimeLine();
            InputContents.ModeContainerRef.GetList.ForEach(e => e.InteractionRef.SteamSoundRef.ResetConnectedSounds());
        }
    }
}
