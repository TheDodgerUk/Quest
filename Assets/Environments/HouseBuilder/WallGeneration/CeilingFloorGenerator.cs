using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class CeilingFloorGenerator : MonoBehaviour
    {
        private const int INTERNAL_WAIT_FRAMES = 2;
        private const int TASK_WAIT_FRAMES = 4;

        private MeshDoubleSidedData m_MeshData;

        #region Nested Class

        private class Triangulator
        {
            private List<Vector2> m_points = new List<Vector2>();

            public Triangulator(Vector2[] points)
            {
                m_points = new List<Vector2>(points);
            }

            public int[] Triangulate()
            {
                List<int> indices = new List<int>();

                int n = m_points.Count;
                if (n < 3)
                    return indices.ToArray();

                int[] V = new int[n];
                if (Area() > 0)
                {
                    for (int v = 0; v < n; v++)
                        V[v] = v;
                }
                else
                {
                    for (int v = 0; v < n; v++)
                        V[v] = (n - 1) - v;
                }

                int nv = n;
                int count = 2 * nv;
                for (int m = 0, v = nv - 1; nv > 2;)
                {
                    if ((count--) <= 0)
                        return indices.ToArray();

                    int u = v;
                    if (nv <= u)
                        u = 0;
                    v = u + 1;
                    if (nv <= v)
                        v = 0;
                    int w = v + 1;
                    if (nv <= w)
                        w = 0;

                    if (Snip(u, v, w, nv, V))
                    {
                        int a, b, c, s, t;
                        a = V[u];
                        b = V[v];
                        c = V[w];
                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(c);
                        m++;
                        for (s = v, t = v + 1; t < nv; s++, t++)
                            V[s] = V[t];
                        nv--;
                        count = 2 * nv;
                    }
                }

                indices.Reverse();
                return indices.ToArray();
            }

            private float Area()
            {
                int n = m_points.Count;
                float A = 0.0f;
                for (int p = n - 1, q = 0; q < n; p = q++)
                {
                    Vector2 pval = m_points[p];
                    Vector2 qval = m_points[q];
                    A += pval.x * qval.y - qval.x * pval.y;
                }
                return (A * 0.5f);
            }

            private bool Snip(int u, int v, int w, int n, int[] V)
            {
                int p;
                Vector2 A = m_points[V[u]];
                Vector2 B = m_points[V[v]];
                Vector2 C = m_points[V[w]];
                if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                    return false;
                for (p = 0; p < n; p++)
                {
                    if ((p == u) || (p == v) || (p == w))
                        continue;
                    Vector2 P = m_points[V[p]];
                    if (InsideTriangle(A, B, C, P))
                        return false;
                }
                return true;
            }

            private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
            {
                float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
                float cCROSSap, bCROSScp, aCROSSbp;

                ax = C.x - B.x; ay = C.y - B.y;
                bx = A.x - C.x; by = A.y - C.y;
                cx = B.x - A.x; cy = B.y - A.y;
                apx = P.x - A.x; apy = P.y - A.y;
                bpx = P.x - B.x; bpy = P.y - B.y;
                cpx = P.x - C.x; cpy = P.y - C.y;

                aCROSSbp = ax * bpy - ay * bpx;
                cCROSSap = cx * apy - cy * apx;
                bCROSScp = bx * cpy - by * cpx;

                return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
            }
        }

        #endregion Nested Class

        public List<GameObject> m_Ceiling = new List<GameObject>();
        public List<GameObject> m_Floors = new List<GameObject>();

        public void CreateAll(ModeContainer<InteractionWallWhole> modeWallContainer, ModeContainer<InteractionCeilingAndFloor> modeContainer, HouseBuilderInputCeiling.PanelTypeEnum panelType, TaskAction task)
        {
            m_Ceiling.Clear();
            m_Floors.Clear();

            // safety for bad walls
            List<InteractionWallWhole> toRemove = new List<InteractionWallWhole>();
            foreach (var item in modeWallContainer.GetList)
            {
                if (item.m_WallPointList.Count <= 1)
                {
                    toRemove.Add(item);
                }
            }

            for (int i = 0; i < toRemove.Count; i++)
            {
                modeWallContainer.GetList.Remove(toRemove[i]);
            }

            modeContainer.DeleteAll();
            int index = 0;
            foreach (var interactionWallWholeRef in modeWallContainer.GetList)
            {
                List<Vector2> points = new List<Vector2>();

                for (int i = 0; i < interactionWallWholeRef.m_WallPointList.Count; i++)
                {
                    FillInMissingData(interactionWallWholeRef);

                    Vector3 start = interactionWallWholeRef.m_WallPointList[i].transform.position;

                    int endIndex = i;
                    endIndex = endIndex.WrapIncrement(interactionWallWholeRef.m_WallPointList);
                    Vector3 end = interactionWallWholeRef.m_WallPointList[endIndex].transform.position;

                    int bendIndex = i;
                    bendIndex = bendIndex.Wrap(interactionWallWholeRef.m_WallPointList);
                    interactionWallWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls.Resize(interactionWallWholeRef.m_WallPointList.Count);

                    if (interactionWallWholeRef.m_StructureJsonData.m_RoomData.m_RoomWalls[bendIndex].m_BendAmount != 0)
                    {
                        AddCurvedWall(interactionWallWholeRef, points, i, start, end);
                    }
                    else
                    {
                        AddStraightWall(points, start);
                    }
                }

                //TODO: This part here causes duplicated calls to create ceilings and floors, related to line  259 and 260
                if (panelType == HouseBuilderInputCeiling.PanelTypeEnum.Ceiling)
                {
                    float height = interactionWallWholeRef.m_StructureJsonData.m_RoomData.m_Height - HouseBuilderConstants.Z_FIGHTING_OFFSET;
                    var obj = CreateAllInternal(interactionWallWholeRef, points, height, modeContainer, interactionWallWholeRef.m_StructureJsonData.m_CeilingData, HouseBuilderConstants.CEILING_NAME, panelType, index);
                    m_Ceiling.Add(obj);
                }
                else
                {
                    float height = (HouseBuilderConstants.Z_FIGHTING_OFFSET / 10) * (index + 1);
                    var obj = CreateAllInternal(interactionWallWholeRef, points, height, modeContainer, interactionWallWholeRef.m_StructureJsonData.m_FloorData, HouseBuilderConstants.FLOOR_NAME, panelType, index);
                    m_Floors.Add(obj);
                }
                index++;
            }

            this.WaitForFrames(TASK_WAIT_FRAMES, () => task.Increment());
        }

        private void FillInMissingData(InteractionWallWhole data)
        {
            data.m_StructureJsonData.m_RoomData.m_RoomWalls.Resize(data.m_StructureJsonData.m_RoomData.m_RoomWalls.Count);
        }

        private void AddStraightWall(List<Vector2> points, Vector3 start)
        {
            points.Add(new Vector2(start.x, start.z));
        }

        private void AddCurvedWall(InteractionWallWhole item, List<Vector2> points, int i, Vector3 start, Vector3 end)
        {
            Vector3 middle = WallGeneratorBezier3D.CalculateMiddle(start, end, item.m_StructureJsonData.m_RoomData.m_RoomWalls[i].m_BendAmount * 2); // times 2 to compinsate for how the bend works 
            for (int s = 0; s < HouseBuilderConstants.CURVED_WALL_RESOLUTION; s++)
            {
                float t = (float)s / HouseBuilderConstants.CURVED_WALL_RESOLUTION;
                Vector3 resultPoint = Bezier.PointOnPathQuad(t, start, middle, end);
                points.Add(new Vector2(resultPoint.x, resultPoint.z));
            }
            if (i == (item.m_WallPointList.LastIndex()))
            {
                Vector3 finalPoint = Bezier.PointOnPathQuad(1, start, middle, end);
                points.Add(new Vector2(finalPoint.x, finalPoint.z));
            }
        }

        private GameObject CreateAllInternal(InteractionWallWhole interactionWallWholeRef, List<Vector2> points, float wallHeight, ModeContainer<InteractionCeilingAndFloor> mode, HouseBuilderConstants.MaterialData materialData, string gameObjectName, HouseBuilderInputCeiling.PanelTypeEnum panelType, int index)
        {
            if (points.Count == 0)
            {
                return null;
            }
            GameObject newGameObject = new GameObject($"{gameObjectName} {index}");

            m_MeshData = newGameObject.ForceComponent<MeshDoubleSidedData>();
            newGameObject.ForceComponent<MeshFilter>();
            m_MeshData.m_MeshRender = newGameObject.ForceComponent<MeshRenderer>();

            m_MeshData.m_Height = wallHeight;
            // needed the double call
            CreateMesh(points, wallHeight, newGameObject, HouseBuilderInputCeiling.PanelTypeEnum.Ceiling);
            CreateMesh(points, wallHeight, newGameObject, HouseBuilderInputCeiling.PanelTypeEnum.Floor);

            var boxCollid = newGameObject.ForceComponent<BoxCollider>();

            var inter = newGameObject.ForceComponent<InteractionCeilingAndFloor>();
            var outline = newGameObject.ForceComponent<EPOOutline.Outlinable>();

            outline.Initialise(newGameObject);
            inter.Initialise(outline, interactionWallWholeRef);
            mode.AddGameObject(inter);

            AddMaterial(materialData, m_MeshData.m_MeshRender, boxCollid);

            this.WaitForFrames(INTERNAL_WAIT_FRAMES, () =>
            {
                inter.SteamSoundRef.m_SteamAudioMaterial.Preset = materialData.m_SteamAudioMaterial;
                if (gameObjectName == HouseBuilderConstants.FLOOR_NAME)
                {
                    var col = newGameObject.ForceComponent<MeshCollider>();
                    var rig = newGameObject.ForceComponent<Rigidbody>();
                    rig.isKinematic = true;
                }
            });
            return newGameObject;
        }

        private void CreateMesh(List<Vector2> points, float wallHeight, GameObject newGameObject, HouseBuilderInputCeiling.PanelTypeEnum panelType)
        {
            m_MeshData.m_MeshFilter = newGameObject.ForceComponent<MeshFilter>();

            Vector2 start = points[0];
            Vector2 last = points.Last();

            if (start == last)
            {
                points.RemoveAt(points.LastIndex());
            }
            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(points.ToArray());
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices

            List<Vector3> norm = new List<Vector3>();
            Vector3[] vertices = new Vector3[points.Count];
            if (panelType == HouseBuilderInputCeiling.PanelTypeEnum.Floor)
            {
                // normals point upwards:  Material and Collider face upwards
                for (int i = 0; i < points.Count; i++)
                {
                    vertices[i] = new Vector3(points[i].x, wallHeight, points[i].y);
                    norm.Add(Vector3.up);
                }
            }
            else
            {
                // normals point upwards:  Material and Collider face upwards
                for (int i = 0; i < points.Count; i++)
                {
                    vertices[i] = new Vector3(points[i].x, wallHeight, points[i].y);
                    norm.Add(Vector3.down);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = panelType.ToString();
            mesh.vertices = vertices;

            if (panelType == HouseBuilderInputCeiling.PanelTypeEnum.Floor)
            {
                mesh.triangles = indices;
            }
            else
            {
                var reverse = indices.ToList();
                reverse.Reverse();
                mesh.triangles = reverse.ToArray();
            }

            mesh.SetNormals(norm);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x / mesh.bounds.size.x, vertices[i].z / mesh.bounds.size.z);
            }
            mesh.uv = uvs;
#if UNITY_EDITOR
            Debug.Log($"{panelType}  Surface area of mesh {mesh.CalculateSurfaceArea()}");
#endif

            m_MeshData.m_MeshCollider = newGameObject.ForceComponent<MeshCollider>();
            newGameObject.ForceComponent<DrawNormals>();
            //m_MeshData.m_MeshCollider.sharedMesh.vertices = vertices;

            if (panelType == HouseBuilderInputCeiling.PanelTypeEnum.Floor)
            {
                m_MeshData.m_LookingDownVertices = vertices;
                m_MeshData.m_LookingDownMesh = mesh;
                mesh.GetNormals(m_MeshData.m_LookingDownNormals);
                m_MeshData.m_MeshFilter.mesh = mesh;
            }
            else
            {
                m_MeshData.m_LookingUpVertices = vertices;
                m_MeshData.m_LookingUpMesh = mesh;
                mesh.GetNormals(m_MeshData.m_LookingUpNormals);
            }
        }

        private void AddMaterial(HouseBuilderConstants.MaterialData materialData, MeshRenderer meshRendererRef, BoxCollider boxCollid)
        {
            var localMaterialData = materialData;
            Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(this, localMaterialData.m_MaterialName, (mat) =>
            {
                if (meshRendererRef != null)
                {
                    meshRendererRef.material = mat;
                    if (null != meshRendererRef.material)
                    {
                        meshRendererRef.material.color = localMaterialData.m_MaterialColour;
                        float x = boxCollid.size.x / boxCollid.size.z;
                        if (localMaterialData.m_TilingAmount < 1)
                        {
                            localMaterialData.m_TilingAmount = 1;
                        }
                        if (float.IsNaN(x))
                        {
                            x = 1;
                            Debug.LogError("X was NaN");
                        }

                        Vector2 tiling = new Vector2(x * localMaterialData.m_TilingAmount * boxCollid.size.z, localMaterialData.m_TilingAmount * boxCollid.size.z);
                        if (tiling == Vector2.zero)
                        {
                            tiling = Vector2.one;
                            Debug.LogError("tiling was Vector2.zero");
                        }
                        tiling /= HouseBuilderConstants.GetTilingAmountFromTextureName(localMaterialData.m_MaterialName);
                        meshRendererRef.material.SetTextureScale(HouseBuilderConstants.TILING_ID, tiling);
                        meshRendererRef.material.SetTextureScale(HouseBuilderConstants.TILING_ID_SECONDARY, tiling);
                        Destroy(boxCollid);
                    }
                }
            });
        }
    }
}