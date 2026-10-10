using HouseBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadSaveGeneratedSound
{
    public class SoundBundleVersion
    {
        public string FileName = "";
        public float TotalFileSizeMB;
        public float PhononSizeMB;
        public float ProbeSizeMB;
        public long DateTimeBinary;
        public string CurrentHashCode;
        public float HeightAboveFloor;
        public float HorizontalSpacing;
        public Vector3 ProbeBoxSize;
        public Vector3 ProbeBoxCenter;

        public int TextureMapPerMetre;
        public float TextureMaxHeight;
        public Vector3 TextureMapSize;
        public int TextureSizeX;
        public int TextureSizeZ;
    }


    public void CreateZipFiles(Action callBack)
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();


        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);
        TaskAction startUploadTask = new TaskAction(2, () =>                            
        {
            WriteJsonFileLocally();
            callBack();
        });

        Zip.Compress(phonon, startUploadTask.Increment);
        Zip.Compress(probe, startUploadTask.Increment);

    }

    public void PromptUploadGeneratedSound()
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);

        string json = GetJsonVersion(phonon);

        if (File.Exists(phonon) == true)
        {
            Core.Network.DownloadCustomFileJsonFile<SoundBundleVersion>(json, true, (networkData) =>
            {
                GetLocalJsonVersion((localVersion) =>
                {
                    if (networkData == null || localVersion == null || networkData.DateTimeBinary != localVersion.DateTimeBinary)
                    {
                        if (localVersion != null)
                        {
                            GenericPromptMenu.Instance.ShowYesNoMenu($"Upload new Sound files: {localVersion.TotalFileSizeMB.ToString("0.000")}MB", () =>
                            {
                                ConfirmUploadGeneratedSound();
                            }, null);
                        }
                    }
                });
            });
        }
    }

    public void ConfirmUploadGeneratedSound()
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);

        string json = GetJsonVersion(phonon);


        GetLocalJsonVersion((localVersion) =>
        {
            AssetSyncMenu.Instance.SetVisible(true);
            AssetSyncMenu.Instance.SetHeaderText("Uploading data");
            AssetSyncMenu.Instance.SetProgressBar();

            Core.Network.UploadCustomUserFile(phononZip, (amountPhonon) => ProgressPhonon("Upload", amountPhonon, localVersion), () =>
            {
                Core.Network.UploadCustomUserFile(probeZip, (amountProbe) => ProgressProbe("Upload", amountProbe, localVersion), () =>
                {
                    Core.Network.UploadCustomUserFile(json + GlobalConsts.JSON, (percentage) => AssetSyncMenu.Instance.SetProgress(percentage), () =>
                    {
                        var fileNames = GetAudioTextureNames();
                        Core.Network.UploadCustomUserFiles(fileNames, null, () =>
                        {
                            Debug.LogError("Loading SPL files, its wrong ");
                            foreach(var file in fileNames)
                            {
                                Debug.LogError($"upload file  {file}");
                            }
                            AssetSyncMenu.Instance.SetVisible(false);
                        });
                        
                    });
                });
            });
        });
    }

    private void ProgressPhonon(string message, float amount, SoundBundleVersion localVersion)
    {
        float total = localVersion.TotalFileSizeMB;
        float size = localVersion.PhononSizeMB;
        float downloadedAmount = (size * amount);
        float percentage = downloadedAmount / total;
        AssetSyncMenu.Instance.SetProgressBarText($"{message}  {downloadedAmount.ToString("0.00")}MB/{total.ToString("0.00")}MB");
        AssetSyncMenu.Instance.SetProgress(percentage);
    }

    private void ProgressProbe(string message, float amount, SoundBundleVersion localVersion)
    {
        float total = localVersion.TotalFileSizeMB;
        float size = localVersion.ProbeSizeMB;
        float downloadedAmount = (size * amount) + localVersion.PhononSizeMB;
        float percentage = downloadedAmount / total;
        AssetSyncMenu.Instance.SetProgressBarText($"{message} {downloadedAmount.ToString("0.00")}MB/{total.ToString("0.00")}MB");
        AssetSyncMenu.Instance.SetProgress(percentage);
    }


    public void GetLocalJsonVersion(Action<SoundBundleVersion> callback)
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string json = GetJsonVersion(phonon);
        Json.AndroidNet.ReadFromFileAsync<SoundBundleVersion>(Core.Mono, false, json, JsonLibraryType.JsonNet, callback);
    }

    public void GetNetworkJsonVersion(Action<SoundBundleVersion> callback)
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string json = GetJsonVersion(phonon);
        Core.Network.DownloadCustomFileJsonFile<SoundBundleVersion>(json, true, callback);
    }


    public SoundBundleVersion WriteJsonFileLocally()
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);

        long phononSize = new FileInfo(phononZip).Length;
        long probeSize = new FileInfo(probeZip).Length;
        SoundBundleVersion asset = new SoundBundleVersion();

        asset.PhononSizeMB = phononSize / 1024 / 1024;
        asset.ProbeSizeMB = probeSize / 1024 / 1024;
        asset.TotalFileSizeMB = (asset.PhononSizeMB + asset.ProbeSizeMB);
        asset.FileName = phonon;
        asset.DateTimeBinary = DateTime.Now.ToBinary();
        asset.CurrentHashCode = Core.Audio.SteamAudioManagerRef.CurrentHashCode;
        asset.HeightAboveFloor = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.heightAboveFloor;
        asset.HorizontalSpacing = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.horizontalSpacing;
        asset.ProbeBoxCenter = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position;
        asset.ProbeBoxSize = Core.Audio.AudioSimulationSetupRef.m_ProbBoxSize;
        asset.ProbeBoxCenter = Core.Audio.AudioSimulationSetupRef.m_ProbBoxCenter;
        SplTextureMap.SplTextureData textureData = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
        if (textureData != null)
        {
            asset.TextureMapPerMetre = textureData.AmountPerMetre;
            asset.TextureMaxHeight = textureData.MaxHeight;
            asset.TextureMapSize = textureData.TextureSize;
            asset.ProbeBoxCenter = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position;
            asset.TextureSizeX = textureData.TextureSizeX;
            asset.TextureSizeZ = textureData.TextureSizeZ;
        }

        string jsonPath = GetJsonVersion(phonon);
        Json.JsonNet.WriteToFile<SoundBundleVersion>(asset, jsonPath, true);
        return asset;
    }

    private string GetJsonVersion(string file) => $"{System.IO.Path.ChangeExtension(file, null)}_Version";

    public void PromptDownloadGeneratedSound(Action callback)
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);

        string shortPhononZip = Zip.GetCompressionFileName(phonon);

        AssetSyncMenu.Instance.SetVisible(true);
        AssetSyncMenu.Instance.SetActiveSpinning();

        bool localFileExists = File.Exists(phonon);
        string json = GetJsonVersion(phonon);
        Core.Network.DownloadCustomFileJsonFile<SoundBundleVersion>(json, true, (networkData) =>
        {
            GetLocalJsonVersion((localVersion) =>
            {
                if ((networkData != null) && ((localVersion == null) || (localVersion.DateTimeBinary != networkData.DateTimeBinary)))
                {

                    string message = $"No local sound Data, Download sound: {networkData.TotalFileSizeMB.ToString("0.000")}MB";
                    if ((localVersion != null) && (localVersion.DateTimeBinary != networkData.DateTimeBinary))
                    {
                        message = $"Local sound data is out of date, Download sound: {networkData.TotalFileSizeMB.ToString("0.000")}MB";
                    }

                    // Turn off the spinning menu before showing the yes/no menu.
                    AssetSyncMenu.Instance.SetVisible(false);

                    // Show the yes/no menu asking if we should download audio.
                    GenericPromptMenu.Instance.ShowYesNoMenu(message, () =>
                    {
                        ConfirmUpdateGeneratedSound(callback);
                    }, () =>
                    {
                        AssetSyncMenu.Instance.SetVisible(false);

                        SplTextureMap.SplTextureData texture = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
                        if (texture != null)
                        {
                            texture.AmountPerMetre = 0;
                            texture.MaxHeight = 2f;
                            texture.TextureSize = Vector2.zero;
                            texture.TextureSizeX = 0;
                            texture.TextureSizeZ = 0;
                        }

                        callback?.Invoke();
                    });
                }
                else
                {
                    AssetSyncMenu.Instance.SetVisible(false);
                    SplTextureMap.SplTextureData texture = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
                    if (texture != null)
                    {
                        if (localVersion != null)
                        {
                            texture.AmountPerMetre = localVersion.TextureMapPerMetre;
                            texture.MaxHeight = localVersion.TextureMaxHeight;
                            texture.TextureSize = localVersion.TextureMapSize;
                            texture.TextureSizeX = localVersion.TextureSizeX;
                            texture.TextureSizeZ = localVersion.TextureSizeZ;
                        }
                        else
                        {
                            texture.AmountPerMetre = 0;
                            texture.MaxHeight = 2f;
                            texture.TextureSize = Vector2.zero;
                            texture.TextureSizeX = 0;
                            texture.TextureSizeZ = 0;
                        }
                    }
                    callback?.Invoke();
                }
            });

        });

    }

    public void ViewerDownloadSound(Action callback)
    {
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);

        string shortPhononZip = Zip.GetCompressionFileName(phonon);

        AssetSyncMenu.Instance.SetVisible(true);
        AssetSyncMenu.Instance.SetActiveSpinning();

        bool localFileExists = File.Exists(phonon);
        string json = GetJsonVersion(phonon);
        Core.Network.DownloadCustomFileJsonFile<SoundBundleVersion>(json, true, (networkData) =>
        {
            GetLocalJsonVersion((localVersion) =>
            {
                if (networkData != null)
                {
                    if ( (localVersion == null) ||  (localVersion.DateTimeBinary != networkData.DateTimeBinary)  )
                    {
                        ConfirmUpdateGeneratedSound(callback, downloadFiles: true);
                    }
                    else
                    {
                        ConfirmUpdateGeneratedSound(callback, downloadFiles: false);
                    }
                }
                else if (localVersion != null)
                {
                    ConfirmUpdateGeneratedSound(callback, downloadFiles: false);
                }
                else
                {
                    AssetSyncMenu.Instance.SetVisible(false);
                    callback?.Invoke();
                }
            });
        });
    }



    public void ConfirmUpdateGeneratedSound(Action callback, bool downloadFiles = true)
    {
        AssetSyncMenu.Instance.SetVisible(true);
        AssetSyncMenu.Instance.SetHeaderText("Downloading Sound files");
        AssetSyncMenu.Instance.SetProgressBar();
        string phonon = Core.Audio.AudioSimulationSetupRef.GetFinalPhononSceneName();
        string probe = Core.Audio.AudioSimulationSetupRef.GetFinalProbesSceneName();

        string phononZip = Zip.GetCompressionFullPathName(phonon);
        string probeZip = Zip.GetCompressionFullPathName(probe);


        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        string json = GetJsonVersion(phonon);

        SoundBundleVersion soundBundleVersion = null;
        byte[] phononData = null;
        byte[] probeData = null;

        Dictionary<string, float[,]> downloadedAllTextures = new Dictionary<string, float[,]>();

        // Once the audio sim files are unzipped, load them into the simulation.
        TaskAction AllUnzipped = new TaskAction(3, () =>                    // 3rd
        {
            stopwatch.Stop();
            Debug.LogError($"Time taken {stopwatch.Elapsed.TotalSeconds}");
            AssetSyncMenu.Instance.SetHeaderText("Applying Sound files");
            AssetSyncMenu.Instance.SetActiveSpinning();
            if ((File.Exists(phonon) == true) && (File.Exists(probe) == true))
            {
                Core.Audio.LoadSaveGeneratedSoundRef.GetLocalJsonVersion((localVersion) =>
                {
                    if (null != localVersion)
                    {
                        Debug.Log($"TOM Local Version loaded is {localVersion.FileName}");

                        if (HouseBuilderRoot.Instance != null)
                        {
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateProbeSpacing(localVersion.HorizontalSpacing);
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateProbeHeight(localVersion.HeightAboveFloor);
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateTextureMapPerMetre(localVersion.TextureMapPerMetre);
                            HBGUIManager.Instance.SoundSimulationMenuRef.UpdateTextureMapMaxHeight(localVersion.TextureMaxHeight);
                            Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst().AmountPerMetre = localVersion.TextureMapPerMetre;
                            Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst().TextureSize = localVersion.TextureMapSize;
                            Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst().TextureSizeX = localVersion.TextureSizeX;
                            Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst().TextureSizeZ = localVersion.TextureSizeZ;
                            Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position = localVersion.ProbeBoxCenter;
                            SaveAudioTextureData(downloadedAllTextures);

                        }

                        Core.Audio.AudioSimulationSetupRef.LoadBakedData(AssetSyncMenu.Instance.SetHeaderText, AssetSyncMenu.Instance.SetProgressBarText, AssetSyncMenu.Instance.SetProgress, () =>
                        {
                            AssetSyncMenu.Instance.SetVisible(false);
                            callback?.Invoke();
                        });
                    }
                    else
                    {
                        AssetSyncMenu.Instance.SetVisible(false);
                        callback?.Invoke();
                    }
                });
            }
            else
            {
                AssetSyncMenu.Instance.SetVisible(false);
                callback?.Invoke();
            }
        });

        // Once the audio sim data is downloaded, unzip the files.
        TaskAction downloadComplete = new TaskAction(1, () =>                  //2nd
        {
            /////////////////////////////////////////////////////
            if (phononData != null)
            {
                Zip.WriteZipFile(phononData, phononZip, () =>
                {
                    Zip.Decompress(phononZip, AllUnzipped.Increment);
                });
            }
            else
            {
                AllUnzipped.Increment();
            }

            /////////////////////////////////////////////////////
            if (probeData != null)
            {
                Zip.WriteZipFile(probeData, probeZip, () =>
                {
                    Zip.Decompress(probeZip, AllUnzipped.Increment);
                });
            }
            else
            {
                AllUnzipped.Increment();
            }

            /////////////////////////////////////////////////////
            if (soundBundleVersion != null)
            {
                Json.JsonNet.WriteToFile<SoundBundleVersion>(soundBundleVersion, json, true);
                AllUnzipped.Increment();
            }
            else
            {
                AllUnzipped.Increment();
            }

        });

        if (true == downloadFiles)
        {
            // Downloads the audio sim data from the server as zip files.
            Core.Network.DownloadCustomFileJsonFile<SoundBundleVersion>(json, true, (jsonData) => //1st
            {
                soundBundleVersion = jsonData;
                Core.Network.DownloadCustomUserFile(phononZip, (phononZipCallback) =>
                {
                    phononData = phononZipCallback;
                    Core.Network.DownloadCustomUserFile(probeZip, (probeZipCallback) =>
                    {
                        probeData = probeZipCallback;


                        if (HouseBuilderRoot.Instance != null)
                        {
                            var fileNames = GetAudioTextureNames();

                            ConsoleExtra.Log("Load the texture maps for audio maps", null, ConsoleExtraEnum.EDebugType.Todo);
                            downloadedAllTextures.Clear();

                            AssetSyncMenu.Instance.SetHeaderText("Downloading SPL files");
                            AssetSyncMenu.Instance.SetProgressBarText("");
                            Core.Network.DownloadCustomFileJsonFiles<float[,]>(fileNames, true, (data) =>
                            {
                                foreach (var file in fileNames)
                                {
                                    Debug.LogError($"download file  SPL {file}");
                                }
                                Debug.LogError("Loading SPL files, its wrong ");
                                downloadedAllTextures = data;
                                downloadComplete.Increment();
                            });
                        }
                        else
                        {
                            downloadComplete.Increment();
                        }

                        
                    }, (progressProbe) =>
                    {
                        ProgressProbe("Download", progressProbe, soundBundleVersion);
                    });
                }, (progressPhonon) =>
                {
                    ProgressPhonon("Download", progressPhonon, soundBundleVersion);
                });
            });
        }
        else
        {
            // If we already have them and just want to re-apply them, just say the download is done.
            downloadComplete.Increment();
        }
    }

    private void SaveAudioTextureData(Dictionary<string, float[,]> downloadedAllTextures)
    {
        // save
        foreach (var item in downloadedAllTextures)
        {

            string filePath = item.Key;
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            if (item.Value != null)
            {
                Json.JsonNet.WriteToFile<float[,]>(item.Value, filePath,  true);
            }
            else
            {
                Debug.LogError($"Bad data in {item.Key}");
            }

        }
    }



    private List<string> GetAudioTextureNames()
    {
        string fileName = "";
        if (HouseBuilder.HouseBuilderRoot.Instance != null)
        {
            fileName = HBGUIManager.Instance.LoadSaveMenuManagerRef.CurrentFileName;
        }
        else
        {
            fileName = HouseViewer.HouseViewerRoot.Instance.CurrentFileName;
        }

        var data = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
        var allSteam = GameObject.FindObjectsOfType<SteamAudio.SteamAudioSource>();
        List<string> fileNames = new List<string>();
        if (data != null)
        {
            for (int i = 0; i < allSteam.Length; i++)
            {
                foreach (SteamAudio.TextureMapType textureMap in Enum.GetValues(typeof(SteamAudio.TextureMapType)))
                {
                    string fileNameNew = data.GeneratedTextureFileNameLocally(fileName, i, textureMap);
                    fileNameNew = Json.ConvertToJsonFileName(fileNameNew);
                    fileNames.Add(fileNameNew);
                }
            }
        }
        return fileNames;
    }
}
