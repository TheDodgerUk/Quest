using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public enum GenerateCatalogueType
    {
        LoadFromFile,
        CreateInGame,
        Duplicate,
    }

    public class LoadSaveHouseContentsData
    {
        public void SaveHouseContentsData(string fileName, LoadSaveType loadSave, List<CatalogueGeneratedData> houseContentsList, Action<bool> callback)
        {
            CollectData(houseContentsList, out HouseContentsData houseContentsData);
            if (loadSave == LoadSaveType.Network)
            {
                Core.Network.SaveManifestJsonFile<HouseContentsData>(fileName + HouseBuilderConstants.CONTENTS_JSON, houseContentsData, callback);
            }
            else
            {
                Json.JsonNet.WriteToFile<HouseContentsData>(houseContentsData, fileName + HouseBuilderConstants.LOCAL_CONTENTS_JSON, true);
                callback?.Invoke(true);
            }
        }



        public void CollectData(List<CatalogueGeneratedData> houseContentsList, out HouseContentsData houseContentsData)
        {

            List<CatalogueGeneratedData> debugItems = houseContentsList;
            houseContentsData = new HouseContentsData();

            foreach (var item in houseContentsList)
            {
                if (item.GeneratedItemRef != null)
                {
                    if (item.m_JsonData == null)
                    {
                        item.m_JsonData = new HouseBuilderContentsJsonData();
                    }

                    item.m_JsonData.m_GameObjectName = item.CatalogueEntryRef.FullName.ToString();


                    var renderer = item.CatalogueEntryRef.GetMetaData()[MetaDataType.Render];
                    if (renderer != null)
                    {
                        item.m_JsonData.RuntimeDirectory = ((RenderMetaData)renderer).RuntimeDirectory;
                        if (string.IsNullOrEmpty(item.InteractionRef.RootGameObject.name) == true)
                        {
                            item.InteractionRef.RootGameObject.name = System.IO.Path.GetFileNameWithoutExtension(item.m_JsonData.RuntimeDirectory);
                        }
                    }

                    item.m_JsonData.m_CatalogueEntryGUID = item.CatalogueEntryRef.Guid;
                    item.m_JsonData.m_GeneratedGUID = item.InteractionRef.m_RootItemsGeneratedGUID;

                    item.m_JsonData.m_Position = item.GeneratedItemRef.transform.position;
                    item.m_JsonData.m_Rotation = item.GeneratedItemRef.transform.rotation;
                    item.m_JsonData.m_MountToAllRotation = item.InteractionRef.PlacementMetaData.m_MountToAllRotation;
                    item.m_JsonData.m_ExtraPivotRotation = item.InteractionRef.PlacementMetaData.m_ExtraPivotRotation;

                    item.m_JsonData.m_ParentGeneratedGUID = INVALID_GUI;


                    if (item.GeneratedItemRef.transform.parent != null)
                    {
                        var parentInteraction = item.GeneratedItemRef.transform.parent.GetComponentInParent<Interaction>();
                        var parentItem  = houseContentsList.FindLast(e => e.InteractionRef == parentInteraction);

                        if (parentItem != null)
                        {
                            item.m_JsonData.m_ParentGeneratedGUID = parentItem.InteractionRef.m_RootItemsGeneratedGUID;
                        }
                    }

                    if (item.InteractionRef.PlacementMetaData.Placement.IsWall() == true)
                    {
                        item.m_JsonData.m_WallObjectLinkerData = item.m_WallObjectLinkerData;
                    }
                    WriteMetaData(item.m_JsonData, item);
                    houseContentsData.m_HouseBuilderContentsJsonDataList.Add(item.m_JsonData);
                }
                else
                {
                    Debug.LogError($"Item is null  {item.CatalogueEntryRef.FullName}");
                }
            }

            var dataString = Json.FullSerialiser.WriteToText<HouseContentsData>(houseContentsData, true);
            houseContentsData = null;
            houseContentsData = Json.FullSerialiser.ReadFromText<HouseContentsData>(dataString);
        }



        public void LoadHouseContentsDataAndCreateObjects(string fileName, LoadSaveType loadSave, HouseBuilderInputContents contents, Action destroyContainerContents, Action<CatalogueGeneratedData> addContainer, Action finishedCallback)
        {
            if (loadSave == LoadSaveType.Network)
            {
                Core.Network.LoadManifestJsonFile<HouseContentsData>(fileName + HouseBuilderConstants.CONTENTS_JSON, (data) =>
                {
                    LoadHouseContentsDataAndCreateObjects(data, contents, destroyContainerContents, addContainer, () =>
                    {
                        finishedCallback.Invoke();
                    });
                });
            }
            else
            {
                Json.JsonNet.ReadFromFileAsync<HouseContentsData>(fileName + HouseBuilderConstants.LOCAL_CONTENTS_JSON, (data) =>
                {
                    LoadHouseContentsDataAndCreateObjects(data, contents, destroyContainerContents, addContainer, () =>
                    {
                        finishedCallback.Invoke();
                    });
                });
            }
        }





        public void LoadHouseContentsDataAndCreateObjects( HouseContentsData data, HouseBuilderInputContents contents, Action destroyContainerContents, Action<CatalogueGeneratedData> addContainer, Action callback)
        {
            destroyContainerContents();
            var catalogue = Catalogue.Instance.GetCatalogue;
            List<int> invalidIndexes = new List<int>();
            TaskAction task = new TaskAction(data.m_HouseBuilderContentsJsonDataList.Count, () =>
            {

                LinkSounds(contents);
                SetCorrectParentingViaLoadData(data);
                LinkStaging();

                // only remove after parenting 
                // these are not created items, these are removed items from the catalogue 
                invalidIndexes.Reverse();
                for (int index = 0; index < invalidIndexes.Count; index++)
                {
                    int contentIndex = invalidIndexes[index];
                    string name = data.m_HouseBuilderContentsJsonDataList[contentIndex].m_GameObjectName;
                    string guid = data.m_HouseBuilderContentsJsonDataList[contentIndex].m_CatalogueEntryGUID;
                    Debug.LogError($"removing {name} as its not in the database  guid: {guid}");
                    data.m_HouseBuilderContentsJsonDataList.RemoveAt(contentIndex);
                }

                foreach(var item in contents.ModeContainerRef.GetList)
                {
                    DeleteOldSubItems(item.InteractionRef.RootGameObject, item.CatalogueEntryRef);                   
                }


                
                callback.Invoke();
            });


            for (int i = 0; i < data.m_HouseBuilderContentsJsonDataList.Count; i++)
            {
                int index = i;
                HouseBuilderContentsJsonData dataRef = data.m_HouseBuilderContentsJsonDataList[i];
                var entry = catalogue.Find(e => e.Guid.Equals(dataRef.m_CatalogueEntryGUID));
                if (null != entry)
                {
                    GenerateCataloguePrefab(Core.Mono, index, entry, dataRef, addContainer, GenerateCatalogueType.LoadFromFile, task, null);
                }
                else if(string.IsNullOrEmpty(dataRef.RuntimeDirectory) == false)
                {
                    entry = Core.LoadFromFileFBXRef.CreateCatalogueEntry(dataRef.RuntimeDirectory);
                    GenerateCataloguePrefab(Core.Mono, index, entry, dataRef, addContainer, GenerateCatalogueType.LoadFromFile, task, null);
                }
                else
                {
                    Debug.LogError($"Could not find {dataRef.m_GameObjectName}");
                    // second try 
                    entry = catalogue.Find(e => e.ShortName.Equals(dataRef.m_GameObjectName));
                    if (entry != null)
                    {
                        GenerateCataloguePrefab(Core.Mono, index, entry, dataRef, addContainer, GenerateCatalogueType.LoadFromFile, task, null);
                    }
                    else
                    {
                        Debug.LogError($"Could not find {dataRef.m_GameObjectName}");
                        task?.Increment();
                        invalidIndexes.Add(i);
                    }

                }
            }
        }

        private void DeleteOldSubItems(GameObject root, CatalogueEntry entry)
        {
            for (int i = 0; i < entry.m_SubItems.Count; i++)
            {
                GameObject oldGameObject = root.transform.Search(entry.m_SubItems[i].m_ObjectName).gameObject;
                if (oldGameObject != null)
                {
#if UNITY_EDITOR
                    Debug.Log($"DeleteOldSubItems {oldGameObject.GetGameObjectPath()}");
#endif
                    UnityEngine.Object.Destroy(oldGameObject);
                }
            }
        }

        private void LinkSounds(HouseBuilderInputContents contents)
        {
            for (int i = 0; i < contents.ModeContainerRef.GetList.Count; i++)
            {
                var data = contents.ModeContainerRef.GetList[i];
                if (data.InteractionRef.SteamSoundRef != null)
                {
                    if (data.m_JsonData.m_SteamAudioData == null)
                    {
                        data.m_JsonData.m_SteamAudioData = new SteamAudioData();
                    }
                    data.InteractionRef.SteamSoundRef.m_InteractionDirectlyConnectedTo.Clear();
                    foreach (var guid in data.m_JsonData.m_SteamAudioData.m_GUIDConnectedTo)
                    {
                        CatalogueGeneratedData item = null;
                        if(HouseBuilderRoot.Instance != null)
                        {
                            item = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.Find(e => e.InteractionRef.m_RootItemsGeneratedGUID == guid);
                        }
                        else
                        {
                            item = HouseViewer.HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList.Find(e => e.InteractionRef.m_RootItemsGeneratedGUID == guid);
                        }
                        
                        if (item != null)
                        {
                            data.InteractionRef.SteamSoundRef.m_InteractionDirectlyConnectedTo.Add(item.InteractionRef);
                        }
                        else
                        {
                            Debug.LogError($"Could not find  {guid}");
                        }
                    }
                    data.InteractionRef.SteamSoundRef.AssignSavedDetails();
                }
            }
        }

        private void LinkStaging()
        {
            Dictionary<Interaction, ContentStagingMetaData> collectedStagingData = CollectStageData();
            List<Interaction> keyList = collectedStagingData.Keys.ToList();


            foreach(var leftData in collectedStagingData)
            {
                foreach (var leftConnectedSavedJsonData in leftData.Key.StagingRef.StagingDataRef.m_Connected)
                {
                    if(leftConnectedSavedJsonData.m_ConnectedToGUId == INVALID_GUI)
                    {
                        continue;
                    }

                    Interaction rightInteractionConnection = keyList.Find(e => e.m_RootItemsGeneratedGUID == leftConnectedSavedJsonData.m_ConnectedToGUId);
                    if(rightInteractionConnection != null)
                    {
                        ContentStagingMetaData rightLinkage = collectedStagingData[rightInteractionConnection];
                        ContentStagingMetaData leftLinkage  = leftData.Value;
                        var otherData = FindRightData(leftConnectedSavedJsonData.m_ConnectedToGUId, leftConnectedSavedJsonData.m_ConnectedToPointIndex);
                        try
                        {
                            leftLinkage.m_StagingData[leftConnectedSavedJsonData.m_OwnPointIndex].m_RotationIndex = leftConnectedSavedJsonData.m_RotationIndex;

                            leftLinkage.m_StagingData[leftConnectedSavedJsonData.m_OwnPointIndex].m_ConnectedTo = rightLinkage.m_StagingData[leftConnectedSavedJsonData.m_ConnectedToPointIndex];
                            rightLinkage.m_StagingData[leftConnectedSavedJsonData.m_ConnectedToPointIndex].m_ConnectedTo = leftLinkage.m_StagingData[leftConnectedSavedJsonData.m_OwnPointIndex];
                        }
                        catch(Exception e)
                        {
                            Debug.LogError($"Something went wrong, put break point on below , Exception: {e}");
                            var exceptionTest = FindRightData(leftConnectedSavedJsonData.m_ConnectedToGUId, leftConnectedSavedJsonData.m_ConnectedToPointIndex);
                            
                        }
                    }

                }
            }
        }


        private HouseBuilderConstants.StagingData.Data FindRightData(int guid, int index)
        {
            Dictionary<Interaction, ContentStagingMetaData> collectedStagingData = CollectStageData();
            List<Interaction> keyList = collectedStagingData.Keys.ToList();

            foreach (var rightData in collectedStagingData)
            {
                foreach (var rightConnectedData in rightData.Key.StagingRef.StagingDataRef.m_Connected)
                {
                    Interaction leftInteractionConnection = keyList.Find(e => e.m_RootItemsGeneratedGUID == guid);
                    if (leftInteractionConnection != null)
                    {
                        if(index == rightConnectedData.m_ConnectedToPointIndex)
                        {
                            return rightConnectedData;
                        }
                        
                    }
                }
            }
            return null;
        }
        private void SetCorrectParentingViaLoadData(HouseContentsData data)
        {
            List<CatalogueGeneratedData> contents = null;
            if (HouseBuilderRoot.Instance != null)
            {
                contents = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
            }
            else
            {
                contents = HouseViewer.HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList;
            }

            for (int i = 0; i < data.m_HouseBuilderContentsJsonDataList.Count; i++)
            {
                var item = data.m_HouseBuilderContentsJsonDataList[i];
                if (item.m_ParentGeneratedGUID != INVALID_GUI)
                {
                    var foundItem = contents.Find(e => e.InteractionRef.m_RootItemsGeneratedGUID == item.m_GeneratedGUID);
                    if (foundItem != null)
                    {
                        var foundParentItem = contents.Find(e => e.InteractionRef.m_RootItemsGeneratedGUID == item.m_ParentGeneratedGUID);
                        if (foundParentItem != null)
                        {
                            Transform parent = foundParentItem.InteractionRef.RootTransform;
                            foundItem.InteractionRef.RootTransform.SetParent(parent);
                        }
                        else
                        {
                            Debug.LogError($"Trying to find an item , which it cannot find!,  gui {item.m_ParentGeneratedGUID}, if its -1, then its old data");
                        }
                    }
                }
            }
        }

        public static void GenerateCataloguePrefab(MonoBehaviour mono, int index, CatalogueEntry entry, HouseBuilderContentsJsonData dataRef,
            Action<CatalogueGeneratedData> addContainer, GenerateCatalogueType generateCatalogueType, TaskAction task, Action<CatalogueGeneratedData> callback)
        {
            int localIndex = index;
            bool found = false;
            mono.WaitFor(10, () =>
            {
                if(found == false)
                {
                    Debug.LogError($" '{entry.ShortName}' Not Spawning object, check cat side item and master.json for circular dependencys, https://dev.azure.com/MusicTribe/MI_R00398/_wiki/wikis/MI_R00398/946/Item-Error");
                    task?.Increment();
                    callback?.Invoke(null);
                }
            });

#if UNITY_EDITOR
            Debug.Log($"GenerateCataloguePrefab :{entry.ShortName}");
#endif
            Core.Scene.SpawnObject(entry, (generatedGameObjectModule) =>
            {
                found = true;

                if (generatedGameObjectModule == null)
                {
                    task?.Increment();
                    callback?.Invoke(null);
                    Debug.LogError($"generatedGameObjectModule == null :{entry.ShortName},   Guid: {entry.Guid}");
                    return;
                }
                CatalogueGeneratedData newCatalogueGeneratedData = new CatalogueGeneratedData();
                // add relative information
                InteractionGameObjectMove move = generatedGameObjectModule.RootObject.ForceComponent<InteractionGameObjectMove>();

                move.transform.ClearLocals();
                move.SetCatalogueEntry(entry);
                AddInteractions(generatedGameObjectModule.RootObject, entry, move);

                // assign data information
                newCatalogueGeneratedData.SetGeneratedItem(generatedGameObjectModule.RootObject);
                newCatalogueGeneratedData.SetCatalogueEntry(entry);
                newCatalogueGeneratedData.SetInteraction(move);
                newCatalogueGeneratedData.GeneratedItemRef.name = entry.FullName;
                move.transform.position = HouseBuilderConstants.CREATED_ITEM_START_POINT; // create offscreen so it does not show up straight away

                InteractionGameObjectRotate rot = null;
                if (newCatalogueGeneratedData.InteractionRef.PlacementMetaData.Placement.IsWall() == false)
                {
                    rot = generatedGameObjectModule.RootObject.ForceComponent<InteractionGameObjectRotate>();

                    // comented out as it doubles the assigned items and callbacks without data being set correctly 
                    //AddInteractions(generatedGameObjectModule.RootObject, entry, rot);
                    //rot.SetCatalogueEntry(entry);
                }

                if (null != dataRef)
                {
                    generatedGameObjectModule.transform.position = dataRef.m_Position;
                    generatedGameObjectModule.transform.rotation = dataRef.m_Rotation;
                    newCatalogueGeneratedData.m_JsonData = dataRef;
                    if (generateCatalogueType == GenerateCatalogueType.LoadFromFile)
                    {
                        ReadMetaData(move, entry, dataRef, generatedGameObjectModule.RootObject, newCatalogueGeneratedData);
                        newCatalogueGeneratedData.InteractionRef.m_RootItemsGeneratedGUID = dataRef.m_GeneratedGUID;
                        if(rot != null)
                        {
                            rot.m_RootItemsGeneratedGUID = dataRef.m_GeneratedGUID;
                        }
                    }
                    if (move.PlacementMetaData.Placement.IsWall() == true)
                    {
                        newCatalogueGeneratedData.m_WallObjectLinkerData = dataRef.m_WallObjectLinkerData;
                    }
                }

                GenerateUID(newCatalogueGeneratedData, move);

                

                if (addContainer != null)
                {
                    addContainer(newCatalogueGeneratedData);
                }

                if (null == dataRef && generateCatalogueType != GenerateCatalogueType.Duplicate)
                {
                    CreateNewData(entry, newCatalogueGeneratedData);
                }

                if (HouseBuilderRoot.Instance != null)
                {
                    var inter = newCatalogueGeneratedData.InteractionRef;
                    if (newCatalogueGeneratedData.InteractionRef == null || newCatalogueGeneratedData.InteractionRef.MainCollider == null)
                    {
                        Debug.LogError($"Problem loading {newCatalogueGeneratedData.CatalogueEntryRef.Guid}    shortName : {newCatalogueGeneratedData.CatalogueEntryRef.ShortName} ");
                        task?.Increment();
                        callback?.Invoke(null);
                        return;
                    }
                }

                if (generateCatalogueType != GenerateCatalogueType.LoadFromFile)
                {
                    mono.WaitForFrames(HIGHTLIGHT_WAIT_TIME, () =>
                    {
                        if (null != HouseBuilderRoot.Instance)
                        {
                            if(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Staging)  /// THIS NEEDS LOOKING at
                            {
                                HouseBuilderRoot.Instance.InputStaging.CurrentInputMode = HouseBuilderInput.InputMode.Placing;
                                HouseBuilderRoot.Instance.InputStaging.SetCurrentInteraction(move);
                            }
                            else
                            {
                                HouseBuilderRoot.Instance.InputContents.CurrentInputMode = HouseBuilderInput.InputMode.Placing;
                                if(move.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.SetToZero)
                                {
                                    HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(move);
                                }    
                                else
                                {
                                    move.RootTransform.position = Vector3.zero;
                                }
                            }
   
                        }
                        task?.Increment();
                        callback?.Invoke(newCatalogueGeneratedData);
                    });
                }
                else
                {
                    task?.Increment();
                    callback?.Invoke(newCatalogueGeneratedData);
                }
            });
            
        }

        private static void GenerateUID(CatalogueGeneratedData newCatalogueGeneratedData, InteractionGameObjectMove root)
        {
            if (newCatalogueGeneratedData.InteractionRef.m_RootItemsGeneratedGUID == -1)
            {
                var list = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;

                List<int> uidList = new List<int>();
                list.ForEach(e => uidList.Add(e.InteractionRef.m_RootItemsGeneratedGUID));
                int uid = 0;
                while (true)
                {
                    if (uidList.Contains(uid) == true)
                    {
                        uid++;
                        continue;
                    }
                    else
                    {
                        newCatalogueGeneratedData.InteractionRef.m_RootItemsGeneratedGUID = uid;
                        root.m_RootItemsGeneratedGUID = uid;
                        break;
                    }
                }
                Debug.Log($"Generate a UID  {newCatalogueGeneratedData.InteractionRef.m_RootItemsGeneratedGUID}");
            }
        }
    }
}