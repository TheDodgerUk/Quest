using EnvironmentHelpers;
using HouseBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HouseBuilder
{
    public class GenerateWalls : MonoBehaviour
    {
        private const int DEBUG_TIME = 40;
        private List<GameObject> m_GeneratedWallsList = new List<GameObject>();
        private GameObject m_GeneratedWallsHolder;
        private bool m_ShowWalls = false;
        private static Material m_BlackMaterial;
        public GameObject Holder => m_GeneratedWallsHolder;

        public List<HouseBuilder.InteractionWallWhole> m_FloorPlanDataListRef = new List<HouseBuilder.InteractionWallWhole>();
        public List<HouseBuilder.InteractionWallGeneratedWhole> GeneratedWholeList = new List<HouseBuilder.InteractionWallGeneratedWhole>();

        private Dictionary<MeshRenderer, BoxCollider> m_CutOutWalls = new Dictionary<MeshRenderer, BoxCollider>();

        public class GroupMaterialData
        {
            public bool m_UseSeperateMaterial = true;
            public int m_Index = 0;
            public Material m_Material = null;
            public Color m_MaterialColour;
        }

        public void ToggleShadows(bool enable)
        {
            GeneratedWholeList.ForEach(e => e.SetShadows(enable));
            foreach (var wall in GeneratedWholeList)
            {
                foreach (var point in wall.m_WallGeneratedPointList)
                {
                    point.SetShadows(enable);
                }
            }
        }

        public void Initialise(List<HouseBuilder.InteractionWallWhole> listRef)
        {
            if (null == m_BlackMaterial)
            {
                m_BlackMaterial = Resources.Load<Material>(HouseBuilderConstants.SLICED_WALL_MATERIAL);
            }
            m_GeneratedWallsHolder = new GameObject(nameof(m_GeneratedWallsHolder));
            m_GeneratedWallsHolder.transform.SetParent(this.transform);

            m_FloorPlanDataListRef = listRef;
        }

        public void RegenerateWalls(Action callback)
        {
            DeleteGeneratedWallAndGenerateWallsFromFloorPlan(callback);           
        }

        private void CollectCutOutWalls()
        {
            m_CutOutWalls.Clear();
            foreach (var wall in GeneratedWholeList)
            {
                var transforms = wall.GetComponentsInChildren<Transform>(true);
                foreach (var tran in transforms)
                {
                    var rend = tran.GetComponent<MeshRenderer>();
                    if (rend != null && rend.enabled == false)
                    {
                        var box = rend.GetComponent<BoxCollider>();
                        if (box != null)
                        {
                            m_CutOutWalls.Add(rend, box);
                        }
                    }
                }
            }
        }

        public void CutoutWallsColliders(bool enabled)
        {
            foreach (var item in m_CutOutWalls)
            {
                if (item.Key != null)
                {
                    if (item.Key.enabled == false)
                    {
                        item.Value.enabled = enabled;
                    }
                }
            }
        }

        public void GenerateAndShowWallsAndPlaceWallObjects(bool show, Action callback = null)
        {
            m_ShowWalls = show;
            m_GeneratedWallsHolder.SetActive(m_ShowWalls);
            if (false == m_ShowWalls)
            {
                callback?.Invoke();
            }
            else
            {
                DeleteGeneratedWallAndGenerateWallsFromFloorPlan(callback);
            }
        }

        [InspectorButton]
        public void DeleteGeneratedWallAndGenerateWallsFromFloorPlan(Action callback)
        {
            DeleteWalls();
            this.WaitForFrames(2, () => // to make sure walls are removed
            {
                CreateWallsPart1(() =>
                {
                    CreateWallsPart2(callback);
                });
            });
        }


        private void DeleteWalls()
        {
            Debug.Log("DeleteWalls");
            for (int i = 0; i < GeneratedWholeList.Count; i++)
            {
                Destroy(GeneratedWholeList[i].gameObject);
            }
            GeneratedWholeList.Clear();
        }


        private void CreateWallsPart1(Action callback)
        {
            TaskAction task = new TaskAction(m_FloorPlanDataListRef.Count, () =>
            {
                callback?.Invoke();
            });

            // it has to load in Materials , so it has callback 
            for (int i = 0; i < m_FloorPlanDataListRef.Count; i++)
            {
                CreateWall(m_FloorPlanDataListRef[i], i, () =>
                {
                    task.Increment();
                });
            }
        }


        private void CreateWallsPart2(Action callback)
        {
            for (int i = 0; i < m_FloorPlanDataListRef.Count; i++)
            {
                CreateWallEnd(m_FloorPlanDataListRef[i], i);
            }

            CutHole(callback);
        }

        private void CutHole(Action callback)
        {
            this.WaitForFrames(2, () =>
            {

                foreach (var room in GeneratedWholeList)
                {

                    for (int i = 0; i < room.m_WallGeneratedPointList.Count; i++)
                    {
                        InteractionWallGeneratedPoint item = room.m_WallGeneratedPointList[i];
                        HoleCutter holeCutterRef = item.gameObject.ForceComponent<HoleCutter>();

                        try
                        {
                            float depth = item.WallPointRef.WallWholeRef.m_StructureJsonData.m_RoomData.m_Depth;
                            float bendAmount = item.WallPointRef.WallWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls[i].m_BendAmount;
                            holeCutterRef.SliceWallToGenerateHoles(item, item.gameObject, depth, bendAmount, CollectCutOutWalls);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"i:{i},  m_BendAmount.Count {item.WallPointRef.WallWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls.Count}    {e.Message}");
                        }

                    }
                    MoveAndRotateWindowsDoorsWithMovementWalls(room.m_WallGeneratedPointList);
                }


                FillInMissingMaterials();
                callback?.Invoke();
            });
        }

        private void FillInMissingMaterials()
        {
            foreach (var wall in GeneratedWholeList)
            {
                var RendererList = wall.GetComponentsInChildren<Renderer>().ToList();
                foreach (var render in RendererList)
                {
                    for (int i = 0; i < render.materials.Length; i++)
                    {
                        if (render.materials[i] == null)
                        {
                            render.materials[i] = m_BlackMaterial;
                        }
                    }

                    Interaction.SetGeneratedMeshRenderer(render);
                }
            }
        }

        private void MoveAndRotateWindowsDoorsWithMovementWalls(List<InteractionWallGeneratedPoint> generatedPoinRef)
        {
            List<CatalogueGeneratedData> connectsList = null;
            if (HouseBuilderRoot.Instance != null)
            {
                connectsList = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.FindAll(e => e.m_WallObjectLinkerData != null);
            }
            else
            {
                connectsList = HouseViewer.HouseViewerRoot.Instance.InputContents.ModeContainerRef.GetList.FindAll(e => e.m_WallObjectLinkerData != null);
            }

            foreach (var item in connectsList)
            {
                CatalogueGeneratedData debugInfo = item;
                var connectWall = generatedPoinRef.FindLast(e => e.ThisWallPointIndex == item.m_WallObjectLinkerData.m_WallPointIndex && e.GeneratedWholeRef.ThisWallIndex == item.m_WallObjectLinkerData.m_WallIndex);
                if (connectWall != null)
                {
#if UNITY_EDITOR
                    Debug.Log($"Its a {item.InteractionRef.PlacementMetaData.Placement}", item.InteractionRef.RootGameObject);
#endif
                    if (item.InteractionRef.PlacementMetaData.Placement.IsWall() == true)
                    {
                        var linkerData = item.m_WallObjectLinkerData;
                        FigureOutWereToPlaceItem(item, connectWall, linkerData, generatedPoinRef, item.InteractionRef.PlacementMetaData.Placement);
                    }
                }
            }
        }

        public static List<RaycastHit> WallItemFloorhit(Transform trans)
        {
            // linked to InteractionGameObjectMove.cs 95
            Vector3 rayCastPosition = trans.position + (trans.forward * Interaction.TINY_AMOUNT_OFFSET * 20);
            rayCastPosition += (Vector3.up * Interaction.TINY_AMOUNT_OFFSET * 20);

            var objectsHit = Physics.RaycastAll(rayCastPosition, Vector3.down, 100).ToList();
            return objectsHit.FindAll(e => e.collider.gameObject.layer == Layers.DefaultLayer).ToList();
        }

        private void FigureOutWereToPlaceItem(CatalogueGeneratedData item, InteractionWallGeneratedPoint connectWall, HouseBuilderConstants.WallObjectLinkerData linkerData, List<InteractionWallGeneratedPoint> generatedPoinRef, ContentPlacementMetaData.EnumPlacement placement)
        {
            Vector3 startPosition = Vector3.Lerp(connectWall.Position, connectWall.NextPosition, linkerData.m_PercentageLength);

            float angle = Vector3.SignedAngle(Vector3.forward, (connectWall.NextPosition - connectWall.Position), Vector3.up);
            if (placement.IsCutout() == true)
            {
                float wallOffset = connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Depth * 0.5f;
                MoveItem(item.GeneratedItemRef, startPosition, (angle + 90), wallOffset, connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Height, linkerData.m_PercentageHeight);
            }
            else
            {
                // wall mounted
                float wallOffset = connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Depth * 0.5f;
                MoveItem(item.GeneratedItemRef, startPosition, (angle + 90), wallOffset, connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Height, linkerData.m_PercentageHeight);
            }

            var floorsHit = WallItemFloorhit(item.GeneratedItemRef.transform);

            var hitFloor = HouseBuilderConstants.WallSide.Outside;
            foreach (var floor in floorsHit)
            {
                if (true == floor.collider.gameObject.name.GetNumber(out int number))
                {
                    if (number == linkerData.m_WallIndex)
                    {
                        hitFloor = HouseBuilderConstants.WallSide.Inside;
#if UNITY_EDITOR
                        Debug.Log($"hitFloor == true", item.InteractionRef.RootGameObject);
#endif
                    }
                }
            }

            if (linkerData.m_WallSide != hitFloor)
            {
#if UNITY_EDITOR
                Debug.Log($"hitFloor == false");
#endif
                if (placement.IsCutout() == true)
                {
                    float wallOffset = 0;
                    MoveItem(item.GeneratedItemRef, startPosition, (angle - 90), wallOffset, connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Height, linkerData.m_PercentageHeight);
                }
                else
                {
                    // wall mounted
                    float wallOffset = connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Depth * 0.5f;
                    MoveItem(item.GeneratedItemRef, startPosition, (angle - 90), wallOffset, connectWall.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Height, linkerData.m_PercentageHeight);
                }
            }

        }

        private void MoveItem(GameObject item, Vector3 startPos, float angle, float wallOfsetAmount, float wallHeight, float percentage)
        {
            var newPosition = startPos;
            item.transform.eulerAngles = new Vector3(0, angle, 0);
            newPosition += (item.transform.forward * wallOfsetAmount);
            newPosition.y = Mathf.Lerp(0, wallHeight, percentage);
            item.transform.position = newPosition;
        }


        private void DestroyContentsList()
        {
            foreach (GameObject gameObjectItem in m_GeneratedWallsList)
            {
                UnityEngine.Object.Destroy(gameObjectItem);
            }
            m_GeneratedWallsList.Clear();
        }

        private void CreateWallEnd(HouseBuilder.InteractionWallWhole wallItem, int wallCount)
        {
            ConsoleExtra.Log("CreateWallEnd", this.gameObject, ConsoleExtraEnum.EDebugType.Todo);
            // figure out the end corners
            // and draw edge to edge  to close off the walls
        }

        private void CreateWall(HouseBuilder.InteractionWallWhole wallItem, int wallCount, Action callback)
        {
            List<GroupMaterialData> groupMaterialData = new List<GroupMaterialData>();
            Material mainMat = null;

            TaskAction task = new TaskAction(1 + wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls.Count, () =>
            {
                GameObject wallGeneratorWhole = new GameObject($"WallGeneratorWhole: {wallCount}");
                var generatedWholeRef = wallGeneratorWhole.ForceComponent<HouseBuilder.InteractionWallGeneratedWhole>();
                GeneratedWholeList.Add(generatedWholeRef);

                generatedWholeRef.SetWallIndex(wallCount);
                generatedWholeRef.SetWallHost(wallItem);
                wallGeneratorWhole.transform.SetParent(m_GeneratedWallsHolder.transform);
                generatedWholeRef.HostWholeRef = wallItem;
                generatedWholeRef.HostWholeRef.m_StructureJsonData.m_RoomData.m_WallNumber = wallCount;

                var wallpoints = LoopsThooughPoints(wallItem, wallCount, mainMat, groupMaterialData, wallGeneratorWhole, generatedWholeRef);

                wallGeneratorWhole.SetLayerRecursively(Layers.DefaultLayer);

                callback?.Invoke();
            });



            Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(this, wallItem.m_StructureJsonData.m_RoomData.m_MaterialName, (mat) =>
            {
                mainMat = mat;
                task.Increment();
            });

            for(int i = 0; i < wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls.Count; i++)
            {
                int localIndex = i;
                Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(this, wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls[localIndex].m_OverrideData.m_MaterialName, (mat) =>
                {
                    var data = new GroupMaterialData();
                    data.m_Index = localIndex;
                    data.m_Material = mat;
                    data.m_UseSeperateMaterial = wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls[localIndex].m_UseOverrideData;
                    data.m_MaterialColour = wallItem.m_StructureJsonData.m_RoomData.m_RoomWalls[localIndex].m_OverrideData.m_MaterialColour;
                    groupMaterialData.Add(data);
                    task.Increment();
                });
            }
        }


        private List<InteractionWallGeneratedPoint> LoopsThooughPoints(InteractionWallWhole wallItem, int wallCount, Material mainMaterial, List<GroupMaterialData> groupDataRef, GameObject wallGeneratorWhole, InteractionWallGeneratedWhole generatedWholeRef)
        {
            List<InteractionWallGeneratedPoint> WallParts = new List<InteractionWallGeneratedPoint>();
            List<float> m_Next = new List<float>();
            bool isConnected = wallItem.m_StructureJsonData.m_IsWallLooped;
#if UNITY_EDITOR
            Debug.Log($"is the wall compleated {isConnected}");
#endif

            int wallPointListCount = wallItem.m_WallPointList.Count;
            for (int i = 0; i < wallPointListCount; i++)
            {
                int loopIndex = i.Wrap(wallItem.m_WallPointList.Count);
                GameObject newGameObject = new GameObject($"WallGeneratedPart: {loopIndex}");
                newGameObject.transform.SetParent(wallGeneratorWhole.transform);
                InteractionWallGeneratedPoint generatedPointRef = null;
                if (null != generatedWholeRef)
                {
                    generatedPointRef = newGameObject.ForceComponent<HouseBuilder.InteractionWallGeneratedPoint>();
                    generatedPointRef.WallPointRef = wallItem.m_WallPointList[loopIndex];
                    generatedPointRef.GeneratedWholeRef = generatedWholeRef;
                    generatedPointRef.SnappingPointsRef.SetupPointsForWall(newGameObject, ContentPlacementMetaData.SnappingPointEnum.MiddleX);
                    generatedPointRef.SnappingPointsRef.SetupPointsForWall(newGameObject, ContentPlacementMetaData.SnappingPointEnum.MiddleZ);
                }

                WallParts.Add(generatedPointRef);
                newGameObject.AddComponent<DrawNormals>();
                newGameObject.AddComponent<SaveMeshToFile>();
                newGameObject.layer = Layers.DefaultLayer;


                // get positions
                Vector3 prevPosition1, prevPosition0, startPosition, nextPosition0, nextPosition1;
                GetPositions(isConnected, wallItem, loopIndex, out prevPosition1, out prevPosition0, out startPosition, out nextPosition0, out nextPosition1);


                float depth = HouseBuilderConstants.SMALL_WALL_DEPTH;
                float height = HouseBuilderConstants.SMALL_WALL_HEIGHT;
                HouseBuilderConstants.RoomData wallData = null;
                if (generatedWholeRef != null)
                {
                    wallData = generatedWholeRef.FloorPlanJsonDataRef.m_RoomData;
                    wallData.m_RoomWalls.Resize(wallItem.m_WallPointList.Count);
                    depth = wallData.m_Depth;
                    height = wallData.m_Height;
                }
                else
                {
                    wallData = wallItem.m_StructureJsonData.m_RoomData;
                }


                GetPrevNextOffset(out float prev, out float next, isConnected, loopIndex, wallItem.m_WallPointList, depth);
                Color col = Color.white;

                Material materialRef = mainMaterial;

                col = wallItem.m_StructureJsonData.m_RoomData.m_MaterialColour;


                var singleDataRef = groupDataRef.FindLast(e => e.m_Index == loopIndex);
                if(singleDataRef.m_UseSeperateMaterial == true)
                {
                    materialRef = singleDataRef.m_Material;
                    col = singleDataRef.m_MaterialColour;
                    materialRef = singleDataRef.m_Material;
                }

                if (wallData.m_RoomWalls[loopIndex].m_BendAmount != 0)
                {
                    WallGeneratorBezier3D item = newGameObject.AddComponent<WallGeneratorBezier3D>();
                    item.CreateWall(depth, height, startPosition, wallData.m_RoomWalls[loopIndex].m_BendAmount, nextPosition0, materialRef, col, wallData.m_TilingAmount);
                    // not wanted for bend  item.transform.position = startPosition;
                    // not wanted for bend  item.transform.LookAt(endPosition);
                    item.AddComponent<MeshCollider>();
                }
                else
                {
                    WallGenerator item = newGameObject.AddComponent<WallGenerator>();
                    item.CreateWall(next, prev, depth, height, Vector3.Distance(startPosition, nextPosition0), materialRef, col, wallData.m_TilingAmount);
                    item.transform.position = Vector3.Lerp(startPosition, nextPosition0, 0.5f);
                    item.transform.LookAt(nextPosition0);
                    item.transform.Rotate(Vector3.up, 90);
                    item.AddComponent<BoxCollider>();
                }
                if (null != generatedWholeRef)
                {
                    generatedWholeRef.AddWallPart(generatedPointRef);
                }
                if (generatedPointRef != null)
                {
                    var outline = generatedPointRef.GetComponent<EPOOutline.Outlinable>();
                    if (outline != null)
                    {
                        outline.Initialise(generatedPointRef.gameObject);
                        generatedPointRef.SetShadowsToCurrentState();
                    }
                }



                // assign steam materail 
                // the wait is to make sure the awake is triggerd in InteractionWallGeneratedPoint
                InteractionWallGeneratedWhole localGeneratedWholeRef = generatedWholeRef;       // to make sure it local for the wait 
                SteamAudio.MaterialPreset localMaterail = localGeneratedWholeRef.HostWholeRef.m_StructureJsonData.m_RoomData.m_SteamAudioMaterial; // to make sure it local for the wait 

                this.WaitForFrames(3, () =>
                {
                    foreach (var part in WallParts)
                    {
                        if (null != part)
                        {
                            part.SteamSoundRef.m_SteamAudioMaterial.Preset = localMaterail;
                            // i not think its needed , but not 100% sure part.SteamSoundRef.m_SteamAudioMaterial.Value = SteamAudio.MaterialPresetList.PresetValue((int)localMaterail);

                        }
                    }
                });



                if (null != generatedWholeRef)
                {
                    if (generatedWholeRef.HostWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls[loopIndex].m_RenderThisWall == false)
                    {
                        if (HouseBuilderRoot.Instance != null && HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Structure)
                        {
                            var rend = generatedPointRef.GetComponent<MeshRenderer>();
                            rend.enabled = false;
                        }
                        else
                        {
                            var rend = generatedPointRef.GetComponent<MeshRenderer>();
                            rend.enabled = false;

                            var collider = generatedPointRef.GetComponent<Collider>();
                            collider.enabled = false;
                        }
                    }

                    if (generatedWholeRef.HostWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls[loopIndex].m_EnableThisWall == false)
                    {
                        var collider = generatedPointRef.GetComponent<Collider>();
                        collider.enabled = false;
                    }
                }



                wallCount++;
            }

            return WallParts;
        }


        private void GetPrevNextOffset(out float prev, out float next, bool isConnected, int startIndex, List<InteractionWallPoint> wallPoints, float wallDepth)
        {
            prev = 0;
            next = 0;
            int prevIndex = 0;
            int nextIndex0 = 0;
            int nextIndex1 = 0;
            MathsExtensions.ClampWrap clampWrap = isConnected ? MathsExtensions.ClampWrap.Wrap : MathsExtensions.ClampWrap.Clamp;

            prevIndex = startIndex.Decrement(wallPoints, clampWrap);
            nextIndex0 = startIndex.Increment(wallPoints, clampWrap);
            nextIndex1 = nextIndex0.Increment(wallPoints, clampWrap);


            Vector3 prevWallVector = wallPoints[prevIndex].transform.position - wallPoints[startIndex].transform.position;
            prevWallVector.Normalize();
            Vector3 prevWallNormal = Quaternion.Euler(0, 90, 0) * prevWallVector;
            prevWallNormal.Normalize();
            Vector3 prevWallPosition = wallPoints[prevIndex].transform.position + (prevWallNormal * (wallDepth / 2));


            Vector3 thisWallVector = wallPoints[startIndex].transform.position - wallPoints[nextIndex0].transform.position;
            thisWallVector.Normalize();
            Vector3 thisWallNormal = Quaternion.Euler(0, 90, 0) * thisWallVector;
            thisWallNormal.Normalize();
            Vector3 thisWallPosition = wallPoints[startIndex].transform.position + (thisWallNormal * (wallDepth / 2));


            Vector3 nextWallVector = wallPoints[nextIndex0].transform.position - wallPoints[nextIndex1].transform.position;
            nextWallVector.Normalize();
            Vector3 nextWallNormal = Quaternion.Euler(0, 90, 0) * nextWallVector;
            nextWallNormal.Normalize();
            Vector3 nextWallPosition = wallPoints[nextIndex0].transform.position + (nextWallNormal * (wallDepth / 2));


            // next
            if( false == InputUtility.LinePlaneIntersectionDualDirection(out Vector3 intersectNext, thisWallPosition, thisWallVector, nextWallNormal, nextWallPosition))
            {
                next = 0;
            }
            else
            {
                var intersectDistance = Vector3.Distance(intersectNext, thisWallPosition);
                var pointDistance = Vector3.Distance(wallPoints[startIndex].transform.position, wallPoints[nextIndex0].transform.position);
                next = pointDistance - intersectDistance;
                if(next > wallDepth)
                {
                    next = 0;
                }
            }


            // prev
            if (false == InputUtility.LinePlaneIntersectionDualDirection(out Vector3 intersectPrev, thisWallPosition, thisWallVector, prevWallNormal, prevWallPosition))
            {
                prev = 0;
            }
            else
            {
                var intersectDistance = Vector3.Distance(intersectPrev, prevWallPosition);
                var pointDistance = Vector3.Distance(wallPoints[startIndex].transform.position, wallPoints[prevIndex].transform.position);
                prev = pointDistance - intersectDistance;
                if (prev > wallDepth)
                {
                    prev = 0;
                }
            }
        }



        private void GetPositions(bool isConnected, InteractionWallWhole wallItem, int loopIndex, out Vector3 prevPosition1, out Vector3 prevPosition0, out Vector3 startPosition, out Vector3 nextPosition0, out Vector3 nextPosition1)
        {

            MathsExtensions.ClampWrap clampWrap = isConnected ? MathsExtensions.ClampWrap.Wrap : MathsExtensions.ClampWrap.Clamp;
            var wallPointList = wallItem.m_WallPointList;

            // prev
            int prevIndex0 = loopIndex;
            prevIndex0 = prevIndex0.Decrement(wallPointList, clampWrap);
            prevPosition0 = wallItem.m_WallPointList[prevIndex0].transform.position;
            int prevIndex1 = prevIndex0.Decrement(wallPointList, clampWrap);
            prevPosition1 = wallItem.m_WallPointList[prevIndex1].transform.position;

            // current 
            startPosition = wallItem.m_WallPointList[loopIndex].transform.position;

            // next 
            int nextIndex0 = loopIndex;
            nextIndex0 = nextIndex0.Increment(wallPointList, clampWrap);
            nextPosition0 = wallItem.m_WallPointList[nextIndex0].transform.position;
            int nextIndex1 = nextIndex0.Increment(wallPointList, clampWrap);
            nextPosition1 = wallItem.m_WallPointList[nextIndex1].transform.position;
        }

        private void DebugDrawPlane(Vector3 position, Vector3 normal)
        {
            Vector3 v3 = Vector3.zero;
            if (normal.normalized != Vector3.forward)
                v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
            else
                v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;

            var corner0 = position + v3;
            var corner2 = position - v3;
            var q = Quaternion.AngleAxis(90.0f, normal);
            v3 = q * v3;
            var corner1 = position + v3;
            var corner3 = position - v3;

            Debug.DrawLine(corner0, corner2, Color.green, DEBUG_TIME);
            Debug.DrawLine(corner1, corner3, Color.green, DEBUG_TIME);
            Debug.DrawLine(corner0, corner1, Color.green, DEBUG_TIME);
            Debug.DrawLine(corner1, corner2, Color.green, DEBUG_TIME);
            Debug.DrawLine(corner2, corner3, Color.green, DEBUG_TIME);
            Debug.DrawLine(corner3, corner0, Color.green, DEBUG_TIME);
            Debug.DrawRay(position, normal * 10, Color.blue, DEBUG_TIME);

        }

        private void DebugDrawRay(Vector3 position, Vector3 vect)
        {
            Debug.DrawRay(position, vect * 10, Color.red, DEBUG_TIME);
        }

        private void AddContentsToList(GameObject obj)
        {
            m_GeneratedWallsList.Add(obj);
            obj.layer = Layers.DefaultLayer;
        }

        public void EnabableColliderOnCutWalls(bool enable, Action callback)
        {
            this.WaitForFrames(10, () =>
            {
                var generatedPoinRef = m_GeneratedWallsHolder.GetComponentsInChildren<HouseBuilder.InteractionWallGeneratedPoint>().ToList();

                for (int i = 0; i < generatedPoinRef.Count; i++)
                {
                    if (generatedPoinRef[i].transform.childCount != 0)
                    {
                        BoxCollider box = generatedPoinRef[i].GetComponent<BoxCollider>();
                        box.enabled = enable;
                    }
                }
                callback?.Invoke();
            });
        }
    }
}

