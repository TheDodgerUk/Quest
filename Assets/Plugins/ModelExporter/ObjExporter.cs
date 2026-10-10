using System.IO;
using System.Text;
using UnityEngine;

public class ObjExporter
{
    public static string MeshToString(Mesh mesh, Material[] mats)
    {
        var sb = new StringBuilder();

        sb.Append("o ").AppendLine(mesh.name);
        sb.Append("g ").AppendLine(mesh.name);
        foreach (Vector3 v in mesh.vertices)
        {
            sb.AppendLine(string.Format("v {0} {1} {2}", -v.x, v.y, v.z));
        }
        sb.AppendLine();
        foreach (Vector3 v in mesh.normals)
        {
            sb.AppendLine(string.Format("vn {0} {1} {2}", -v.x, v.y, v.z));
        }
        sb.AppendLine();
        foreach (Vector3 v in mesh.uv)
        {
            sb.AppendLine(string.Format("vt {0} {1}", v.x, v.y));
        }
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            sb.AppendLine();
            if (mats != null && mats.Length > i)
            {
                sb.Append("usemtl ").AppendLine(mats[i].name);
                sb.Append("usemap ").AppendLine(mats[i].name);
            }

            int[] triangles = mesh.GetTriangles(i);
            for (int t = 0; t < triangles.Length; t += 3)
            {
                sb.AppendLine(string.Format("f {2}/{2}/{2} {1}/{1}/{1} {0}/{0}/{0}",
                    triangles[t] + 1, triangles[t + 1] + 1, triangles[t + 2] + 1));
            }
        }
        return sb.ToString();
    }

    public static string MeshToString(MeshFilter mf)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
        return MeshToString(m, mats);
    }

    public static void MeshToFile(string filename, Mesh mesh, params Material[] materials)
    {
        string folder = Path.GetDirectoryName(filename);
        if (Directory.Exists(folder) == false)
        {
            Directory.CreateDirectory(folder);
        }
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mesh, materials));
        }
    }

    public static void MeshToFile(string filename, MeshFilter mf)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mf));
        }
    }
}