using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class Duplication
    {
        public class DuplicateData
        {
            public CatalogueGeneratedData m_CatalogueGeneratedData;
            public int m_CoreGeneratedGUID;
        }

        public enum DirectionType
        {
            Up,
            Down,
            Left,
            Right,
        }

        private readonly List<int> NUMBERS_TO_CREATE = new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
        };

        private List<Interaction> m_TemporaryDuplicateList = new List<Interaction>();
        private Text m_ShortMeasurementType;
        private List<InteractionGameObject> m_CurrentSelections;
        private TMP_InputField m_DistanceField;


        public void SetupMulipleDuplicate(CanvasGroup panel, List<InteractionGameObject> currentSelections)
        {
            m_CurrentSelections = currentSelections;
            var multipleDuplicateUp = panel.transform.Find("BtnUp").GetComponent<Button>();
            var multipleDuplicateDown = panel.transform.Find("BtnDown").GetComponent<Button>();
            var multipleDuplicateLeft = panel.transform.Find("BtnLeft").GetComponent<Button>();
            var multipleDuplicateRight = panel.transform.Find("BtnRight").GetComponent<Button>();

            m_ShortMeasurementType = panel.transform.Find("ShortMeasurementType").GetComponent<Text>();
            #region NumberToCreate
            var numberToCreate = panel.transform.Find("NumberToCreate").GetComponent<TMP_Dropdown>();
            List<string> numbersToCreateList = new List<string>();
            for (int i = 0; i < NUMBERS_TO_CREATE.Count; i++)
            {
                numbersToCreateList.Add(NUMBERS_TO_CREATE[i].ToString());
            }
            numberToCreate.ClearOptions();
            numberToCreate.AddOptions(numbersToCreateList);
            #endregion

            m_DistanceField = panel.transform.Find("DistanceField").GetComponent<TMP_InputField>();
            m_DistanceField.contentType = TMP_InputField.ContentType.DecimalNumber;
            m_DistanceField.text = (1.5f).ToString();

            multipleDuplicateUp.onClick.AddListener(() => { DuplicateMultiple(NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text), Duplication.DirectionType.Up); });
            multipleDuplicateDown.onClick.AddListener(() => { DuplicateMultiple(NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text), Duplication.DirectionType.Down); });
            multipleDuplicateLeft.onClick.AddListener(() => { DuplicateMultiple(NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text), Duplication.DirectionType.Left); });
            multipleDuplicateRight.onClick.AddListener(() => { DuplicateMultiple(NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text), Duplication.DirectionType.Right); });

            var up = multipleDuplicateUp.gameObject.ForceComponent<EventTrigger>();
            up.AddEventTriggerListener(EventTriggerType.PointerEnter, (data) => { OnHoverButtonEnter(m_CurrentSelections, Duplication.DirectionType.Up, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });
            up.AddEventTriggerListener(EventTriggerType.PointerExit, (data) => { OnHoverButtonExit(Duplication.DirectionType.Up, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });

            var down = multipleDuplicateDown.gameObject.ForceComponent<EventTrigger>();
            down.AddEventTriggerListener(EventTriggerType.PointerEnter, (data) => { OnHoverButtonEnter(m_CurrentSelections, Duplication.DirectionType.Down, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });
            down.AddEventTriggerListener(EventTriggerType.PointerExit, (data) => { OnHoverButtonExit(Duplication.DirectionType.Down, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });

            var right = multipleDuplicateRight.gameObject.ForceComponent<EventTrigger>();
            right.AddEventTriggerListener(EventTriggerType.PointerEnter, (data) => { OnHoverButtonEnter(m_CurrentSelections, Duplication.DirectionType.Right, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });
            right.AddEventTriggerListener(EventTriggerType.PointerExit, (data) => { OnHoverButtonExit(Duplication.DirectionType.Right, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });

            var left = multipleDuplicateLeft.gameObject.ForceComponent<EventTrigger>();
            left.AddEventTriggerListener(EventTriggerType.PointerEnter, (data) => { OnHoverButtonEnter(m_CurrentSelections, Duplication.DirectionType.Left, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });
            left.AddEventTriggerListener(EventTriggerType.PointerExit, (data) => { OnHoverButtonExit(Duplication.DirectionType.Left, NUMBERS_TO_CREATE[numberToCreate.value], float.Parse(m_DistanceField.text)); });
        }

        public void DestroyTemporaryDuplicateList()
        {
            for (int i = 0; i < m_TemporaryDuplicateList.Count; i++)
            {
                GameObject.Destroy(m_TemporaryDuplicateList[i].gameObject);
            }
            m_TemporaryDuplicateList.Clear();
        }

        public void SetCurrentSelection(List<InteractionGameObject> currentSelections) => m_CurrentSelections = currentSelections;

        public void SetDistanceVaribles()
        {
            if (m_CurrentSelections.Count > 0)
            {
                var size = m_CurrentSelections.First().BoundsRef.size;
                var max = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                var convertedAmount = MeasurementManager.ConvertFromMeters((max * 1.5f)); // so half there width apart
                m_DistanceField.text = convertedAmount.ToString("0.00"); // 2 decimal places 
                m_ShortMeasurementType.text = MeasurementManager.ShortMeasurementType();
            }
        }

        public void OnHoverButtonEnter(List<InteractionGameObject> currentSelections, DirectionType direction, int numberToCreate, float distance)
        {
            distance = MeasurementManager.ConvertToMeters(distance);

            if (numberToCreate != m_TemporaryDuplicateList.Count)
            {
                DestroyTemporaryDuplicateList();

                // recreate them 
                for (int i = 0; i < numberToCreate; i++)
                {
                    var clone = UnityEngine.GameObject.Instantiate(currentSelections.First());
                    clone.PlacementMetaData.Placement = currentSelections.First().PlacementMetaData.Placement;
                    m_TemporaryDuplicateList.Add(clone);
                }
            }

            // reset them to original place 
            m_TemporaryDuplicateList.ForEach(e => e.RootTransform.position = currentSelections.First().RootTransform.position);

            // apply offsets
            for (int i = 0; i < m_TemporaryDuplicateList.Count; i++)
            {
                float offset = (i + 1) * distance;
                ApplyMultyOffset(direction, offset, m_TemporaryDuplicateList[i]);
            }

            Core.Mono.WaitForFrames(2, () =>
            {
                m_TemporaryDuplicateList.ForEach(e => e.SetRenderModeFade());// this does not work straight away so thats why delay
            });
            m_TemporaryDuplicateList.ForEach(e => e.SetActive(true));
        }

        public void OnHoverButtonExit(DirectionType direction, int numberToCreate, float distance)
        {
            m_TemporaryDuplicateList.ForEach(e => e.SetActive(false)); // turn them off
        }

        public void DuplicateMultiple(int numberToCreate, float distance, DirectionType directionType)
        {
            Debug.Log("DuplicateMultiple");
            var inputContentsRef = HouseBuilderRoot.Instance.InputContents;

            var original = (InteractionGameObject)inputContentsRef.GetCurrentInteractions()[0];

            List<InteractionGameObject> items = new List<InteractionGameObject>();
            List<float> offsets = new List<float>();
            for (int i = 0; i < numberToCreate; i++)
            {
                items.Add(original);
                offsets.Add((i + 1) * distance); // cant be  zero for these
            }

            DuplicateItems(original, items, directionType, offsets);
        }

        public void DuplicateItems(InteractionGameObject original, List<InteractionGameObject> objects, DirectionType directionType = DirectionType.Right, List<float> offsets = null)
        {
            HouseBuilderRoot.Instance.AllowLeftMouseInput = false;
            HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = false;

            var inputContentsRef = HouseBuilderRoot.Instance.InputContents;

            // save data locally, clear interactions
            objects = objects.Clone();
            inputContentsRef.ClearAllInteractionsNoMouseUp();

            // collect all items and subItems
            SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove> templateAttachedObjects = new SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove>();
            CollectAllData(objects, offsets, templateAttachedObjects);

            int offsetCount = 1;
            if (offsets != null)
            {
                offsetCount = offsets.Count;
            }

            bool isMoreThanOne = (templateAttachedObjects.GetKeys().Count > 1);
            List<DuplicateData> allGeneratedList = new List<DuplicateData>();

            int templateAttachedObjectsCount = templateAttachedObjects.CountAll();
            int total = (templateAttachedObjectsCount * objects.Count) + objects.Count;

            TaskAction task = FinishAllObjects(objects, offsets, inputContentsRef, allGeneratedList, total, () =>                                                                                               // 3rd
            {
                // this breaks it for now 
                ////////////////if (((InteractionGameObjectMove)original).CatalogueGeneratedDataRef.m_WallObjectLinkerData != null)
                ////////////////{
                ////////////////    InteractionWallGeneratedWhole rootWall = HouseBuilderRoot.Instance.GenerateWallsRef.GeneratedWholeList.FindLast(e => e.ThisWallIndex == original.CatalogueGeneratedDataRef.m_WallObjectLinkerData.m_WallIndex);
                ////////////////    InteractionWallGeneratedPoint pointWall = rootWall.m_WallGeneratedPointList.Find(e => e.ThisWallPointIndex == original.CatalogueGeneratedDataRef.m_WallObjectLinkerData.m_WallPointIndex);
                ////////////////    // find original wall 
                ////////////////    foreach (var newItem in allGeneratedList)
                ////////////////    {
                ////////////////        InteractionGameObjectMove converted = (InteractionGameObjectMove)newItem.m_CatalogueGeneratedData.InteractionRef;
                ////////////////        converted.CurrentSelectedInteraction = pointWall;
                ////////////////    }
                ////////////////}


                allGeneratedList.ForEach(e => e.m_CatalogueGeneratedData.InteractionRef.MouseUp());

                var cloneAllGeneratedList = allGeneratedList.Clone();
                SetParents(templateAttachedObjects, allGeneratedList);
                ApplyOffsets(directionType, offsets, inputContentsRef, templateAttachedObjects, cloneAllGeneratedList);

                HouseBuilderRoot.Instance.AllowLeftMouseInput = true;
                HouseBuilderRoot.Instance.RecorderRef.RecordEnabled = true;
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            CatalogueGeneratedData coreRootItemWithOffsets = inputContentsRef.ModeContainerRef.GetItem(objects.First().RootGameObject);                                                                         // 2nd
            var keys = templateAttachedObjects.GetKeys();
            int totalKeyCount = keys.Count;

            TaskAction coreTask = CreateAllChildObjects(objects, directionType, offsets, inputContentsRef, templateAttachedObjects, offsetCount, isMoreThanOne, allGeneratedList, task, coreRootItemWithOffsets, keys); // 1st,

            CreateBaseObjects(directionType, offsets, inputContentsRef, offsetCount, isMoreThanOne, allGeneratedList, coreRootItemWithOffsets, keys, coreTask);
        }

        private void ApplyOffsets(DirectionType directionType, List<float> offsets, HouseBuilderInputContents inputContentsRef, SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove> templateAttachedObjects, List<DuplicateData> cloneAllGeneratedList)
        {
            if (offsets != null)
            {
                Debug.LogError("offsets are not used, this needs fixing, below is close but not right");
                foreach (var root in templateAttachedObjects.GetKeys())
                {
                    for (int offsetIndex = 0; offsetIndex < offsets.Count; offsetIndex++)
                    {
                        var foundRoot = cloneAllGeneratedList.Find(e => e.m_CoreGeneratedGUID == root.m_RootItemsGeneratedGUID);
                        cloneAllGeneratedList.Remove(foundRoot);
                        ApplyMultyOffset(directionType, offsets[offsetIndex], foundRoot.m_CatalogueGeneratedData.InteractionRef);
                    }
                }
            }
            else
            {
                foreach (var root in templateAttachedObjects.GetKeys())
                {
                    var foundRoot = cloneAllGeneratedList.Find(e => e.m_CoreGeneratedGUID == root.m_RootItemsGeneratedGUID);
                    inputContentsRef.AddCurrentInteraction(foundRoot.m_CatalogueGeneratedData.InteractionRef);
                    break;
                }
            }
        }

        private void SetParents(SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove> templateAttachedObjects, List<DuplicateData> allGeneratedList)
        {
            List<DuplicateData> toRemove = new List<DuplicateData>();
            foreach (var key in templateAttachedObjects.GetKeys())
            {
                foreach (var coreRoot in templateAttachedObjects[key])
                {
                    var coreParent = coreRoot.gameObject.GetComponentInParentNotSelf<InteractionGameObjectMove>();
                    var duplicateRoot = allGeneratedList.Find(e => e.m_CoreGeneratedGUID == coreRoot.m_RootItemsGeneratedGUID);
                    var duplicateParent = allGeneratedList.Find(e => e.m_CoreGeneratedGUID == coreParent.m_RootItemsGeneratedGUID);
                    toRemove.Add(duplicateRoot);
                    if (duplicateParent != null)
                    {
                        toRemove.Add(duplicateParent);
                        duplicateRoot.m_CatalogueGeneratedData.InteractionRef.RootTransform.SetParent(null);
                        duplicateRoot.m_CatalogueGeneratedData.InteractionRef.RootTransform.SetParent(duplicateParent.m_CatalogueGeneratedData.InteractionRef.RootTransform);
                    }
                    else
                    {
                        Debug.LogError($"Cannot find parent for {coreParent.name}");
                    }
                }
            }


            // this is so when doing multiple items,not use ones used before 
            foreach (var item in toRemove)
            {
                allGeneratedList.Remove(item);
            }
        }

        private void CollectAllData(List<InteractionGameObject> objects, List<float> offsets, SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove> attachedObjects)
        {
            if (offsets == null)
            {
                foreach (var item in objects)
                {
                    var items = item.RootGameObject.GetComponentsInChildren<InteractionGameObjectMove>().ToList();
                    items.Remove((InteractionGameObjectMove)item);
                    attachedObjects.AddToList(item, items);
                }
            }
            else
            {
                var first = objects.First();
                var items = first.RootGameObject.GetComponentsInChildren<InteractionGameObjectMove>().ToList();
                items.Remove((InteractionGameObjectMove)first);
                attachedObjects.AddToList(first, items);
            }
        }

        private TaskAction FinishAllObjects(List<InteractionGameObject> objects, List<float> offsets, HouseBuilderInputContents inputContentsRef, List<DuplicateData> allGeneratedList, int total, Action callback)
        {
            TaskAction task = new TaskAction(total, () =>
            {
                inputContentsRef.ClearAllInteractionsNoMouseUp();
                if (offsets == null)
                {
                    Core.Mono.WaitForEndOfFrame(() =>
                    {
                        foreach (var item in allGeneratedList)
                        {

                            // if just button pressed, select the root items
                            if (IsGeneratedObjectRoot(item.m_CatalogueGeneratedData) == true)
                            {
                                inputContentsRef.SelectInteraction(item.m_CatalogueGeneratedData.InteractionRef);
                            }
                        }

                        callback?.Invoke();
                    });
                }
                else
                {
                    DeleteDuplicateCollidingItems(objects, inputContentsRef, allGeneratedList);
                    callback?.Invoke();
                }
            });
            return task;
        }

        private void DeleteDuplicateCollidingItems(List<InteractionGameObject> objects, HouseBuilderInputContents inputContentsRef, List<DuplicateData> allGeneratedList)
        {
            if (objects.First().StagingRef == null)
            {
                foreach (var item in allGeneratedList)
                {
                    if (item.m_CatalogueGeneratedData.InteractionRef.KeyboardMove(item.m_CatalogueGeneratedData.InteractionRef.RootTransform.position, out Vector3 newPos, out GameObject init) == false)
                    {
                        inputContentsRef.DeleteItem((InteractionGameObject)item.m_CatalogueGeneratedData.InteractionRef, true);
                    }
                }

                // if non wall items are not same height, means they on top of something , remove them 
                if (objects.First().PlacementMetaData.Placement.IsWall() == false)
                {
                    var posY = objects.First().RootTransform.position.y;
                    foreach (var item in allGeneratedList)
                    {
                        if (IsGeneratedObjectRoot(item.m_CatalogueGeneratedData) == true)
                        {
                            float dif = Mathf.Abs(posY - item.m_CatalogueGeneratedData.InteractionRef.RootTransform.position.y);
                            if (dif > 0.001f)
                            {
                                inputContentsRef.DeleteItem((InteractionGameObject)item.m_CatalogueGeneratedData.InteractionRef, true);
                            }
                        }
                    }
                }
            }
        }

        private TaskAction CreateAllChildObjects(List<InteractionGameObject> objects, DirectionType directionType, List<float> offsets, HouseBuilderInputContents inputContentsRef, SimpleDictionaryList<InteractionGameObject, InteractionGameObjectMove> attachedObjects, int offsetCount, bool isMoreThanOne, List<DuplicateData> allGeneratedList, TaskAction task, CatalogueGeneratedData coreRootItemWithOffsets, List<InteractionGameObject> keys)
        {
            return new TaskAction(objects.Count, () =>
            {
                for (int taskIndex = 0; taskIndex < (keys.Count * offsetCount); taskIndex++)
                {
                    task?.Increment();
                }
                for (int offsetIndex = 0; offsetIndex < offsetCount; offsetIndex++)
                {
                    for (int i = 0; i < keys.Count; i++)
                    {
                        int keyIndex = i;
                        InteractionGameObject item = keys[keyIndex];
                        var itemList = attachedObjects.GetList(item);
                        for (int j = 0; j < itemList.Count; j++)
                        {
                            int itemIndex = j;
                            InteractionGameObject itemSub = itemList[itemIndex];
                            CatalogueGeneratedData itemSubData = inputContentsRef.ModeContainerRef.GetItem(itemSub.RootGameObject);
                            var parentingOffset = offsetIndex + ((keyIndex + 1) * 10);
                            CreateObject(directionType, offsets, inputContentsRef, isMoreThanOne, allGeneratedList, task, coreRootItemWithOffsets, itemSub, itemSubData, offsetIndex, parentingOffset);
                        }
                    }
                }

            });
        }

        private void CreateBaseObjects(DirectionType directionType, List<float> offsets, HouseBuilderInputContents inputContentsRef, int offsetCount, bool isMoreThanOne, List<DuplicateData> allGeneratedList, CatalogueGeneratedData coreRootItemWithOffsets, List<InteractionGameObject> keys, TaskAction coreTask)
        {
            // This is done first
            // generate all base items , create all base items
            for (int offsetIndex = 0; offsetIndex < offsetCount; offsetIndex++)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    int keyIndex = i;
                    InteractionGameObject item = keys[keyIndex];
                    CatalogueGeneratedData itemData = inputContentsRef.ModeContainerRef.GetItem(item.RootGameObject);
                    var parentingOffset = offsetIndex + ((keyIndex + 1) * 10);
                    CreateObject(directionType, offsets, inputContentsRef, isMoreThanOne, allGeneratedList, coreTask, coreRootItemWithOffsets, item, itemData, offsetIndex, parentingOffset);
                }
            }
        }

        private bool IsGeneratedObjectRoot(CatalogueGeneratedData item) => (item.GeneratedItemRef.transform.parent == null || item.GeneratedItemRef.transform.parent == HouseBuilderRoot.Instance.InputContents.ModeContainerRef.Holder.transform);


        private void CreateObject(DirectionType directionType, List<float> offsets, HouseBuilderInputContents inputContentsRef, bool isMoreThanOne, List<DuplicateData> addedList, TaskAction task, CatalogueGeneratedData coreRootItem, InteractionGameObject item, CatalogueGeneratedData itemData, int offsetIndex, int parentingOffset )
        {
            Core.Mono.WaitForEndOfFrame(() =>
            {
                int coreGuid = item.m_RootItemsGeneratedGUID;
                LoadSaveHouseContentsData.GenerateCataloguePrefab(Core.Mono, 0, item.CatalogueEntryRef, null, inputContentsRef.ModeContainerRef.AddGameObject, GenerateCatalogueType.Duplicate, null, (justAddedItem) =>
                {
   
                    justAddedItem.GeneratedItemRef.transform.position = itemData.GeneratedItemRef.transform.position;
                    justAddedItem.GeneratedItemRef.transform.rotation = itemData.GeneratedItemRef.transform.rotation;

                    var move = (InteractionGameObjectMove)justAddedItem.InteractionRef;
                    HouseBuilderConstants.AddInteractions(justAddedItem.InteractionRef.RootGameObject, item.CatalogueEntryRef, move);
                    HouseBuilderConstants.CopyData(itemData, justAddedItem);



                    DuplicateData newData = new DuplicateData();
                    newData.m_CatalogueGeneratedData = justAddedItem;
                    newData.m_CoreGeneratedGUID = coreGuid;

                    addedList.Add(newData);
                    if (isMoreThanOne == true || offsets != null)
                    {
                        if (offsets != null)
                        {
                           ////////// ApplyMultyOffset(directionType, offsets[offsetIndex], justAddedItem.InteractionRef);
                        }
                        


                        // this is needed to place wall items on wall with all linkage being done
                        if (move.PlacementMetaData.Placement.IsWall() && offsets != null)
                        {
                            var wallIndex = coreRootItem.m_WallObjectLinkerData.m_WallIndex;
                            var wallPartIndex = coreRootItem.m_WallObjectLinkerData.m_WallPointIndex;
                            var wall = HouseBuilderRoot.Instance.GenerateWallsRef.GeneratedWholeList.Find(e => e.ThisWallIndex == wallIndex);
                            var wallPart = wall.m_WallGeneratedPointList[wallPartIndex];
                            move.CurrentSelectedInteraction = wallPart;
                        }
                        move.MouseDown();
                        // this has to be done out of loop as regenerate walls, move.MouseUp();   -> allGeneratedList.ForEach(e => e.m_CatalogueGeneratedData.InteractionRef.MouseUp());
                    }

                    task?.Increment();
                });
            });
        }

        private void ApplyMultyOffset(DirectionType directionType, float offsets, Interaction item)
        {
            switch (directionType)
            {
                case DirectionType.Up:
                    if (item.PlacementMetaData.Placement.IsWall())
                    {
                        item.RootTransform.position += item.RootTransform.up * offsets;
                    }
                    else
                    {
                        item.RootTransform.position += item.RootTransform.forward * offsets;
                    }
                    break;
                case DirectionType.Down:
                    if (item.PlacementMetaData.Placement.IsWall())
                    {
                        item.RootTransform.position += -item.RootTransform.up * offsets;
                    }
                    else
                    {
                        item.RootTransform.position += -item.RootTransform.forward * offsets;
                    }
                    break;
                case DirectionType.Left:
                    if (item.PlacementMetaData.Placement.IsWall())
                    {
                        item.RootTransform.position += item.RootTransform.right * offsets;
                    }
                    else
                    {
                        item.RootTransform.position += -item.RootTransform.right * offsets;
                    }
                    break;
                case DirectionType.Right:
                    if (item.PlacementMetaData.Placement.IsWall())
                    {
                        item.RootTransform.position += -item.RootTransform.right * offsets;
                    }
                    else
                    {
                        item.RootTransform.position += item.RootTransform.right * offsets;
                    }
                    break;
                default:
                    break;
            }
        }


    }
}
