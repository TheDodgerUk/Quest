using MathNet.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class PositionGrid : MonoBehaviour
    {
        public static PositionGridInternal Instance;
        private const float DISTANCE_MULTIPLIER = 0.25f;

        private void Awake()
        {
            Instance = new PositionGridInternal(this);
        }

        public static int GetWidthFromGameObject(GameObject obj)
        {
            if (true == obj.name.GetNumber(out int number))
            {
                return number;
            }
            return 0;
        }

        public class PositionGridInternal
        {
            private PositionGrid m_PositionGrid;
            public PositionGridInternal(PositionGrid grid)
            {
                m_PositionGrid = grid;
                IsKeyboardInput = false;
                m_GridInfo = new GridInfo();
                m_GridHolder = new GameObject(nameof(m_GridHolder));
                m_GridHolder.transform.SetParent(grid.transform);
                m_GridMarkerPrefab = CreateGridMarker();
                m_GridMarkerManager = m_GridHolder.transform.AddComponent<DrawInstancedMeshs>();
                m_GridMarkerPrefab.SetActive(false);
                m_GridMarkerManager.Init(m_GridMarkerPrefab);
            }

            private GameObject m_GridHolder;
            private DrawInstancedMeshs m_GridMarkerManager;
            private GameObject m_GridMarkerPrefab;
            private readonly int LAYER_MASK = Layers.NavigationMask;
            private GridInfo m_GridInfo;
            private List<Vector3> m_CreatedPoints = new List<Vector3>();
            private List<Quaternion> m_CreatedQuaternion = new List<Quaternion>();
            public bool IsKeyboardInput { get; set; }


            public GridInfo GetGridInfo() => m_GridInfo;

            public void SetGridInfo(GridInfo info)
            {
                m_GridInfo = info;
                Regenerate();
            }

            public void SetWidth(GameObject obj)
            {
                m_GridInfo.m_Width = PositionGrid.GetWidthFromGameObject(obj);
                Regenerate();
            }


            public void PlaceObjectOnGrid(GameObject gameObjectRef)
            {
                if (true == m_GridInfo.m_ShowGrid && IsKeyboardInput == false)
                {
                    var finalPosition = this.FindNearestPointOnGrid(gameObjectRef.transform.position);
                    gameObjectRef.transform.position = new Vector3(finalPosition.x, gameObjectRef.transform.position.y, finalPosition.z);
                }
            }

            public Vector3 PlaceMouseOnGrid()
            {
                Vector3 pos = Vector3.zero;
                var ray = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
                if (true == InputUtility.LinePlaneIntersectionSingleDirection(out pos, ray.origin, ray.direction, Vector3.up, Vector3.zero))
                {
                    if (true == m_GridInfo.m_ShowGrid && IsKeyboardInput == false)
                    {
                        pos = FindNearestPointOnGrid(pos);
                    }
                }
                else
                {
                    pos = ray.origin + (ray.direction * 100);
                }

                return pos;
            }

            public Ray MouseToRay()
            {
                Vector3 positionInRealWorld = PlaceMouseOnGrid();
                Vector3 direction = positionInRealWorld - Camera.main.transform.position;
                direction.Normalize();
                Ray ray = new Ray(Camera.main.transform.position, direction);
                return ray;
            }

            public Ray MouseToRayDown()
            {
                Ray ray =Camera.main.ScreenPointToRay(Input.mousePosition);
                ray.direction = Vector3.down;
                return ray;
            }

            public Ray KeyboardToRay(Interaction interaction, Vector3 positionInRealWorld)
            {
                if (interaction.PlacementMetaData.Placement.IsGrounded() || interaction.PlacementMetaData.Placement == ContentPlacementMetaData.EnumPlacement.Ceiling)
                {
                    Vector3 direction = positionInRealWorld - Camera.main.transform.position;
                    direction.Normalize();
                    return new Ray(Camera.main.transform.position, direction);
                }
                else
                {
                    Vector3 direction = -interaction.RootTransform.forward;
                    direction.Normalize();
                    return new Ray(interaction.RootTransform.position + (interaction.RootTransform.forward), direction);
                }
            }

            public void PlaceObjectOnGrid(ref Vector3 position)
            {
                if (true == m_GridInfo.m_ShowGrid && IsKeyboardInput == false)
                {
                    var finalPosition = this.FindNearestPointOnGrid(position);
                    position = new Vector3(finalPosition.x, position.y, finalPosition.z);
                }
            }

            public Ray GameObjectToRay(Vector3 pos, bool IncludingGrid = true)
            {
                Vector3 direction = pos - Camera.main.transform.position;
                direction.Normalize();
                Ray ray = new Ray(Camera.main.transform.position, direction);
                return ray;
            }

            public Ray GameObjectToRay(GameObject obj, bool IncludingGrid = true)
            {
                return GameObjectToRay(obj.transform.position, IncludingGrid);
            }

            public void SetCurrentGridState(bool show)
            {
                m_GridInfo.m_ShowGrid = show;
                ShowCurrentGridState();
            }

            public void ShowCurrentGridState() => m_GridHolder.SetActive(m_GridInfo.m_ShowGrid);

            public bool GetCurrentGridState() => m_GridInfo.m_ShowGrid;

            public void SetTempGridState(bool show) => m_GridHolder.SetActive(show);        // bad name

            public void SetOffsetX(float percentage)
            {
                m_GridInfo.m_Offset.x = -percentage; Regenerate();
            }

            public void SetOffsetY(float percentage)
            {
                m_GridInfo.m_Offset.y = -percentage; Regenerate();
            }

            public void SetGridScale(float amount)
            {
                //This only takes in METER based amounts
                m_GridInfo.m_Spacing = amount;
                Regenerate();
            }

            private Vector3 CreateNearestPointOnGrid(Vector3 position)
            {
                int xCount = Mathf.RoundToInt(position.x / m_GridInfo.m_Spacing);
                int yCount = Mathf.RoundToInt(position.y / m_GridInfo.m_Spacing);
                int zCount = Mathf.RoundToInt(position.z / m_GridInfo.m_Spacing);

                float xPosition = (float)xCount * m_GridInfo.m_Spacing;
                float yPosition = (float)yCount * m_GridInfo.m_Spacing;
                float zPosition = (float)zCount * m_GridInfo.m_Spacing;

                float xOffset = (m_GridInfo.m_Offset.x / m_GridInfo.m_Spacing) % 1;
                float yOffset = (m_GridInfo.m_Offset.y / m_GridInfo.m_Spacing) % 1;

                Vector3 result = new Vector3(xPosition + (m_GridInfo.m_Spacing * xOffset), yPosition, zPosition + (m_GridInfo.m_Spacing * yOffset));
                return result;
            }

            private Vector3 FindNearestPointOnGrid(Vector3 position)
            {
                Vector3 result = Vector3.zero;
                float distance = float.MaxValue;

                for (int i = 0; i < m_CreatedPoints.Count; i++)
                {
                    float newDistance = (m_CreatedPoints[i] - position).sqrMagnitude;
                    if (newDistance < distance)
                    {
                        result = m_CreatedPoints[i];
                        distance = newDistance;
                    }
                }

                distance = Mathf.Sqrt((float)distance);
                if (distance < (m_GridInfo.m_Spacing * DISTANCE_MULTIPLIER))
                {
                    return result;
                }

                return position;
            }

            private GameObject CreateGridMarker()
            {
                GameObject item = CachedObjects.Instance.GetPrefab("GridMarker");
                item.name = "GridMarker";
                Renderer r = item.GetComponent<Renderer>();
                r.material.color = HouseBuilderConstants.GRID_COLOR;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                item.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                item.transform.SetParent(m_GridHolder.transform);
                return item;
            }

            public void Regenerate()
            {
                m_CreatedPoints = new List<Vector3>();
                m_CreatedQuaternion = new List<Quaternion>();
                m_GridMarkerManager.ClearAll();
                float widthHalf = m_GridInfo.m_Width / 2;
                float x = -widthHalf;
                float z = -widthHalf;
                while (true)
                {
                    var point = CreateNearestPointOnGrid(new Vector3(x, 0, z));
                    if ((point.x >= -widthHalf) && (point.x <= widthHalf) && (point.z >= -widthHalf) && (point.z >= -widthHalf))
                    {
                        m_CreatedPoints.Add(point);
                        m_CreatedQuaternion.Add(Quaternion.Euler(90,0,0));
                    }
                    x += m_GridInfo.m_Spacing;
                    if (x > widthHalf)
                    {
                        x = -widthHalf;
                        z += m_GridInfo.m_Spacing;
                    }
                    if (z > widthHalf)
                    {
                        break;
                    }
                }
                RegenerateHeight();
            }

            public void RegenerateHeight(bool forceToFloor = false) => Core.Mono.StartCoroutine(InternalRegenerateHeight(forceToFloor));


            private IEnumerator InternalRegenerateHeight(bool forceToFloor = false)
            {
                yield return new WaitForEndOfFrame();
                if (forceToFloor == false)
                {
                    float hight = 1000;
                    float raycastDistance = hight + 10;
                    for (int j = 0; j < m_CreatedPoints.Count; j++)
                    {
                        var point = m_CreatedPoints[j];
                        point.y = hight;
                        var hitPoints = Physics.RaycastAll(point, Vector3.down, raycastDistance, LAYER_MASK).OrderBy(h => h.distance).ToArray();
                        hitPoints.Reverse();
                        if (hitPoints.Length > 0)
                        {
                            for (int i = 0; i < hitPoints.Length; i++)
                            {
                                if (Layers.IsEnvironment(hitPoints[i].collider) == true)
                                {
                                    m_CreatedPoints[j] = hitPoints[i].point;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Vector3 position = m_CreatedPoints[j];
                            position.y = 0;
                            m_CreatedPoints[j] = position;
                        }

                        var pos = m_CreatedPoints[j];
                        pos.y += HouseBuilderConstants.Z_FIGHTING_OFFSET_GRID;
                        m_CreatedPoints[j] = pos;
                    }
                }
                else
                {
                    for (int j = 0; j < m_CreatedPoints.Count; j++)
                    {
                        Vector3 position = m_CreatedPoints[j];
                        position.y = 0 + HouseBuilderConstants.Z_FIGHTING_OFFSET_GRID;
                        m_CreatedPoints[j] = position;
                    }
                }
                m_GridMarkerManager.ClearAll();
                m_GridMarkerManager.Add(m_CreatedPoints, m_CreatedQuaternion, new Vector3(0.15f, 0.15f, 0.15f));
            }
        }
    }
}