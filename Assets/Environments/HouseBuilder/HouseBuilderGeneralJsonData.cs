using System;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class HouseBuilderGeneralJsonData
    {
        public GeneralData CurrentGeneralDataRef { get; private set; }
        public void SaveData(string fileName, LoadSaveType loadSave, Action<bool> callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                CollectData(out GeneralData generalData);
                Core.Network.SaveManifestJsonFile<GeneralData>(fileName + HouseBuilderConstants.GENERAL_JSON, generalData, callback);
            }
            else
            {
                CollectData(out GeneralData generalData);
                Json.JsonNet.WriteToFile<GeneralData>(generalData, fileName + HouseBuilderConstants.LOCAL_GENERAL_JSON, true);
                callback?.Invoke(true);
            }
        }

        public void CollectData(out GeneralData generalData)
        {
            generalData = new GeneralData();
            generalData.m_GridInfo = PositionGrid.Instance.GetGridInfo();
            generalData.m_FloorSizeIndex = FloorAndSkyboxManager.Instance.GetCurrentFloorIndex();

            HBGUIManager.Instance.SceneSettingsMenuRef.GetTimeForFile(ref generalData.m_TimeOfDay, ref generalData.m_TimeFormat);
            generalData.m_AmbientLight = FloorAndSkyboxManager.Instance.GetAmbientLight();
            generalData.m_CompassRotation = FloorAndSkyboxManager.Instance.GetCompassRotation();
            generalData.m_LightingProfile = OscController.OscControllerRoot.Instance.OscLoadSaveMenuManagerRef.GetSelectedFileName();

            //generalData.m_SplineData = HBGUIManager.Instance.ShowRoomSettingsMenuRef.m_SplineData.Clone();
            var dataString = Json.FullSerialiser.WriteToText<GeneralData>(generalData, true);
            generalData = null;
            generalData = Json.FullSerialiser.ReadFromText<GeneralData>(dataString);
        }

        public void LoadAndSetData(string fileName, LoadSaveType loadSave, Action callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                Core.Network.LoadManifestJsonFile<GeneralData>(fileName + HouseBuilderConstants.GENERAL_JSON, (generalDataRef) =>
                {
                    if (generalDataRef != null)
                    {
                        LoadAndSetData(generalDataRef, callback);
                        CurrentGeneralDataRef = generalDataRef;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Why is generalDataRef == null");
                        callback?.Invoke();
                    }
                });
            }
            else
            {
                Json.JsonNet.ReadFromFileAsync<GeneralData>(fileName + HouseBuilderConstants.LOCAL_GENERAL_JSON, (generalDataRef) =>
                {
                    if (generalDataRef != null)
                    {
                        LoadAndSetData(generalDataRef, callback);
                        CurrentGeneralDataRef = generalDataRef;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Why is generalDataRef == null");
                        callback?.Invoke();
                    }
                });
            }
        }

        public void LoadAndSetData(GeneralData data, Action callback)
        {
            SetData(data);
            callback?.Invoke();
        }

        public void ResetData()
        {
            var generalData = new GeneralData();
            SetData(generalData);
        }

        private void SetData(GeneralData generalDataRef)
        {
            if (FloorAndSkyboxManager.Instance != null)
            {
                if (HouseBuilderRoot.Instance != null)
                {
                    HBGUIManager.Instance.SceneSettingsMenuRef.SetTimeViaFile(generalDataRef.m_TimeOfDay, generalDataRef.m_TimeFormat);// this in turn sets FloorAndSkyboxManager.Instance.SetTimeOfDay
                }
                else
                {
                    int tod = generalDataRef.m_TimeOfDay;
                    if (generalDataRef.m_TimeFormat == SceneSettingsMenu.TimeFormat.PM)
                    {
                        tod += 12;
                    }
                    FloorAndSkyboxManager.Instance.SetTimeOfDay(tod);
                }
                FloorAndSkyboxManager.Instance.SetAmbientLight(generalDataRef.m_AmbientLight);
                FloorAndSkyboxManager.Instance.SetCompassRotation(generalDataRef.m_CompassRotation);

                if (HouseBuilderRoot.Instance != null)
                {
                    HBGUIManager.Instance.SceneSettingsMenuRef.LoadFromFile(generalDataRef.m_GridInfo, generalDataRef.m_FloorSizeIndex);
                    //HBGUIManager.Instance.ShowRoomSettingsMenuRef.m_SplineData = generalDataRef.m_SplineData.Clone();
                }
                FloorAndSkyboxManager.Instance.SelectTheFloor(generalDataRef.m_FloorSizeIndex);
            }
            if (PositionGrid.Instance != null)
            {
                PositionGrid.Instance.SetGridInfo(generalDataRef.m_GridInfo);
                PositionGrid.Instance.SetCurrentGridState(generalDataRef.m_GridInfo.m_ShowGrid);
            }


            if (string.IsNullOrEmpty(generalDataRef.m_LightingProfile) == false)
            {
                OscController.OscControllerRoot.Instance.OscLoadSaveMenuManagerRef.LoadFile(generalDataRef.m_LightingProfile);
            }
        }
    }
}