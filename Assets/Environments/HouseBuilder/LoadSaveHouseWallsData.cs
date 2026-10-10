using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class LoadSaveHouseWallsData
    {
        public void SaveDataFromFloorPlanData(string fileName, LoadSaveType loadSave, List<InteractionWallWhole> InteractionList, Action<bool> callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                CollectData(InteractionList, out FloorCollectionData floorCollectionData);
                Core.Network.SaveManifestJsonFile<FloorCollectionData>(fileName + HouseBuilderConstants.FLOOR_JSON, floorCollectionData, callback);
            }
            else
            {
                CollectData(InteractionList, out FloorCollectionData floorCollectionData);
                Json.JsonNet.WriteToFile<FloorCollectionData>(floorCollectionData, fileName + HouseBuilderConstants.LOCAL_FLOOR_JSON, true);
                callback?.Invoke(true);
            }
        }



        public void CollectData(List<InteractionWallWhole> InteractionList, out FloorCollectionData floorCollectionData)
        {
            floorCollectionData = new FloorCollectionData();
            floorCollectionData.m_StructureJsonData.Clear();
            foreach (InteractionWallWhole item in InteractionList)
            {
                floorCollectionData.m_StructureJsonData.Add(item.m_StructureJsonData);

                item.m_StructureJsonData.m_Positions.Clear();
                for (int i = 0; i < item.m_WallPointList.Count; i++)
                {
                    if(item.m_WallPointList[i].transform.position.y != 0)
                    {
                        Debug.LogError($" wall point {item.m_WallPointList[i]} Y was not Zero, setting it");
                        var pos = item.m_WallPointList[i].transform.position;
                        pos.y = 0;
                        item.m_WallPointList[i].transform.position = pos;
                        
                    }
                    item.m_StructureJsonData.m_Positions.Add(item.m_WallPointList[i].transform.position);
                }
            }
            var dataString = Json.FullSerialiser.WriteToText<FloorCollectionData>(floorCollectionData, true);
            floorCollectionData = null;
            floorCollectionData = Json.FullSerialiser.ReadFromText<FloorCollectionData>(dataString);
        }


        public void LoadFloorDataAndGenerateWalls(string fileName, LoadSaveType loadSave, Action deleteAllContainer,  Action<InteractionWallWhole> addToFloorPlan, Action callback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                Core.Network.LoadManifestJsonFile<FloorCollectionData>(fileName + HouseBuilderConstants.FLOOR_JSON, (data) =>
                {
                    LoadFloorDataAndGenerateWalls(data, deleteAllContainer, addToFloorPlan, callback);
                });
            }
            else
            {
                Json.JsonNet.ReadFromFileAsync<FloorCollectionData>(fileName + HouseBuilderConstants.LOCAL_FLOOR_JSON, (data) =>
                {
                    LoadFloorDataAndGenerateWalls(data, deleteAllContainer, addToFloorPlan, callback);
                });
            }
        }

        public void LoadFloorDataAndGenerateWalls(FloorCollectionData data, Action deleteAllContainer, Action<InteractionWallWhole> addToFloorPlan, Action callback)
        {
            deleteAllContainer();
            if (null != data)
            {
                for (int i = 0; i < data.m_StructureJsonData.Count; i++)
                {
                    GameObject newItem = new GameObject("WallCreation");
                    var wall = newItem.ForceComponent<InteractionWallWhole>();
                    wall.m_StructureJsonData = data.m_StructureJsonData[i];
                    wall.WallIndex = wall.m_StructureJsonData.m_WallIndex;

                    for (int j = 0; j < data.m_StructureJsonData[i].m_Positions.Count; j++)
                    {
                        if (HouseBuilderRoot.Instance != null)
                        {
                            wall.Initialise(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.Holder);
                        }
                        else
                        {
                            wall.Initialise(HouseViewer.HouseViewerRoot.Instance.InputFloorPlan.ModeContainerRef.Holder);
                        }
                        
                        wall.MouseDown();
                        Vector3 position = data.m_StructureJsonData[i].m_Positions[j];
                        wall.m_WallPointList[j].transform.position = position;
                    }
                    wall.MouseUp();
                    addToFloorPlan((InteractionWallWhole)wall);
                }
            }
            callback?.Invoke();
        }

    }
}