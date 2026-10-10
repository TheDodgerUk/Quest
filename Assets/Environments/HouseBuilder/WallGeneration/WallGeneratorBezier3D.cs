using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HouseBuilder;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WallGeneratorBezier3D : MonoBehaviour
{
    private const int CURVE_MULIPLIER_FROM_INPUT = 2;
    public float m_Thickness = 0.25f;
    public float m_Height = 1f;

    private Vector3 m_Start;
    private Vector3 m_Middle;
    private Vector3 m_End;

    private MeshFilter m_MeshFilter;
    private Material m_Material;
    private Renderer m_Renderer;

    public void CreateWall(float thickness, float height, Vector3 start, float middleAmount, Vector3 end, Material mat,Color col, float tilingAmount)
    {
        middleAmount *= CURVE_MULIPLIER_FROM_INPUT;
        float segmentLength = _CreateWall(thickness, height, start, middleAmount, end);
        m_Material = mat;
        if (null != m_Material)
        {
            float x = Vector3.Distance(start, end) / height;
            m_Material.mainTextureScale = new Vector2((x / HouseBuilderConstants.CURVED_WALL_RESOLUTION)* tilingAmount, (1 * tilingAmount));
            Debug.Log($"CreateWall Changed to segmentLength: {segmentLength}      m_Material.mainTextureScale {m_Material.mainTextureScale}");
            m_Material.color = col;
        }
        m_Renderer.material = m_Material;
    }

    private float _CreateWall(float thickness, float height, Vector3 start, float middleAmount, Vector3 end)
    { 
        m_MeshFilter = this.gameObject.ForceComponent<MeshFilter>();
        m_Renderer = this.gameObject.ForceComponent<Renderer>();
        m_Material = m_Renderer.material;
        m_Thickness = thickness * 2;
        m_Height = height;
        m_Start = start;
        m_Middle = CalculateMiddle(start, end, middleAmount);
        m_End = end;

        float segmentLength = 0;
        m_MeshFilter.mesh = CreateMesh(ref segmentLength);
        return segmentLength;
    }

    public static Vector3 CalculateMiddle(Vector3 start, Vector3 end, float bendAmount)
    {
        Vector3 direction = start - end;
        Vector3 middle = Vector3.Lerp(start, end, 0.5f);
        direction.Normalize();
        direction = Quaternion.Euler(0, -90, 0) * direction;
        middle += direction * bendAmount;
        return middle;
    }

    public Mesh CreateMesh(ref float segmentLength)
    {
        Mesh mesh;

        mesh = new Mesh();

        float scaling = 1.5f;
        float width = m_Thickness / 2f;
        List<Vector3> vertList = new List<Vector3>();
        List<int> triList = new List<int>();
        List<Vector2> uvList = new List<Vector2>();
        Vector3 upNormal = Vector3.up;

        triList.AddRange(new int[] {
            2, 1, 0,    //start face
			0, 3, 2
        });

        float startAmount = HouseBuilderConstants.CURVED_WALL_RESOLUTION / 2;
        float stepAmount = 1;

        for (int s = 0; s < HouseBuilderConstants.CURVED_WALL_RESOLUTION; s++)
        {
            float t = ((float)s) / HouseBuilderConstants.CURVED_WALL_RESOLUTION;
            float futureT = ((float)s + 1) / HouseBuilderConstants.CURVED_WALL_RESOLUTION;

            Vector3 segmentStart = Bezier.PointOnPathQuad(t, m_Start, m_Middle,  m_End);
            Vector3 segmentEnd = Bezier.PointOnPathQuad(futureT, m_Start, m_Middle, m_End);

            Vector3 segmentDirection = segmentEnd - segmentStart;

            //if (s == 0 || s == resolution - 1)
            //{
            //    // this creates the point part
            //    segmentDirection = new Vector3(0, 1, 0);
            //}

            segmentDirection.Normalize();
            Vector3 segmentRight = Vector3.Cross(upNormal, segmentDirection);
            Vector3 segmentRightNorm = segmentRight;
                    segmentRight *= (width/2); // width of the wall 
            Vector3 offset = segmentRightNorm.normalized * (width/8) * scaling;
            offset = Vector3.zero;
            Vector3 br = segmentRight + (upNormal * m_Height) + offset;
            Vector3 tr = segmentRight + (upNormal * -0) + offset;
            Vector3 bl = -segmentRight + (upNormal * m_Height) + offset;
            Vector3 tl = -segmentRight + (upNormal * -0) + offset;

            int curTriIdx = vertList.Count;

            Vector3[] segmentVerts = new Vector3[]
            {
                segmentStart + br,
                segmentStart + bl,
                segmentStart + tl,
                segmentStart + tr,
            };
            vertList.AddRange(segmentVerts);

            Vector2[] uvs = new Vector2[segmentVerts.Length];
            for (var i = 0; i < segmentVerts.Length; i++)
            {
                uvs[i] = new Vector2(segmentVerts[i].x, segmentVerts[i].y);
            }

            uvs[0] = new Vector2(startAmount + (stepAmount * (s+0)), 0);
            uvs[1] = new Vector2(startAmount + (stepAmount * (s+1)), 1);
            uvs[2] = new Vector2(startAmount + (stepAmount * (s+1)), 0);
            uvs[3] = new Vector2(startAmount + (stepAmount * (s+0)), 1);

            uvList.AddRange(uvs);

            int[] segmentTriangles = new int[]
            {
                curTriIdx + 6, curTriIdx + 5, curTriIdx + 1, //left face
				curTriIdx + 1, curTriIdx + 2, curTriIdx + 6,
                curTriIdx + 7, curTriIdx + 3, curTriIdx + 0, //right face
				curTriIdx + 0, curTriIdx + 4, curTriIdx + 7,
                curTriIdx + 1, curTriIdx + 5, curTriIdx + 4, //top face
				curTriIdx + 4, curTriIdx + 0, curTriIdx + 1,
                curTriIdx + 3, curTriIdx + 7, curTriIdx + 6, //bottom face
				curTriIdx + 6, curTriIdx + 2, curTriIdx + 3
            };
            triList.AddRange(segmentTriangles);

            //final segment fenceposting: finish segment and add end face
            if (s == HouseBuilderConstants.CURVED_WALL_RESOLUTION - 1)
            {
                curTriIdx = vertList.Count;

                vertList.AddRange(new Vector3[] {
                           segmentEnd + br,
                           segmentEnd + bl,
                           segmentEnd + tl,
                           segmentEnd + tr
                       });

                Vector2[] uvsExtra = new Vector2[4];
                uvsExtra[0] = new Vector2(startAmount + (stepAmount * (s + 1)), 0);
                uvsExtra[1] = new Vector2(startAmount + (stepAmount * (s + 2)), 1);
                uvsExtra[2] = new Vector2(startAmount + (stepAmount * (s + 2)), 0);
                uvsExtra[3] = new Vector2(startAmount + (stepAmount * (s + 1)), 1);
                uvList.AddRange(uvsExtra);

                triList.AddRange(new int[] {
                            curTriIdx + 0, curTriIdx + 1, curTriIdx + 2, //end face
                            curTriIdx + 2, curTriIdx + 3, curTriIdx + 0});
            }
        }

        mesh.vertices = vertList.ToArray();
        mesh.triangles = triList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        Vector3 startPoint = Bezier.PointOnPathQuad(0, m_Start, m_Middle, m_End);
        Vector3 nextPoint = Bezier.PointOnPathQuad(1, m_Start, m_Middle, m_End);
        segmentLength = Vector3.Distance(startPoint, nextPoint);

        return mesh;
    }
}
