using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class SaveMeshToFile : MonoBehaviour
{
#if UNITY_EDITOR
    [InspectorButton]
    private void WriteMeshToFileAsAsset()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Directory.CreateDirectory($"{Application.dataPath}/GeneratedMesh");

        string name = this.gameObject.name;
        Regex rgx = new Regex("[^a-zA-Z0-9]");
        name = rgx.Replace(name, "");

        UnityEditor.AssetDatabase.CreateAsset(mesh, $"Assets/GeneratedMesh/{name}.asset");
        UnityEditor.AssetDatabase.SaveAssets();
    }

    [InspectorButton]
    private void WriteMeshToFileAsMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Directory.CreateDirectory($"{Application.dataPath}/GeneratedMesh");

        string name = this.gameObject.name;

        Regex rgx = new Regex("[^a-zA-Z0-9]");
        name = rgx.Replace(name, "");

        string path = $"{Application.dataPath}/GeneratedMesh/{name}.fbx";

        byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
        File.WriteAllBytes(path, bytes);
    }
#endif
}
