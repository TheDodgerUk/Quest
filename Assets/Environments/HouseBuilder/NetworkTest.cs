using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static HouseBuilder.HouseBuilderConstants;

public class NetworkTest : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(StartUp());
    }

    [InspectorButton]
    private void DebugPutFile() => StartCoroutine(PutFile());

    [InspectorButton]
    private void DebugGetFile() => StartCoroutine(GetFile());

    [InspectorButton]
    private void DebugGetFileTest() => StartCoroutine(GetFileTest());

    [InspectorButton]
    private void DebugDeleteFile() => StartCoroutine(DeleteFile());

    [InspectorButton]
    private void DebugDeleteFileTest() => StartCoroutine(DeleteFileTest()); 

    [InspectorButton]
    private void DebugGetManifest() => StartCoroutine(GetManifest());

    private const string SECTRET = "2edbd85f271f7d4535e19725eafe4372f450fe8e39a39d01d4c7d2fa4be0bb160e30373340710387d4a9ff6f9561a39518623ebe356e850b6a15120f82d539ffde07ae4a0c260c966a0c6b15dd869f5e1a4fbe1fc02c45f65012529bd5d7d745b2b9cd5bd4b491e3abe7e45d0f9f109e04a7abfd7d453be6a7a00df261f330a4c27284fb62f42bcde10a5992bde9a16d3b6f4a2d3f716cd6145a3df21a3cbb22e7f6ee5b9bce9fd1891bfb6ae49c9e70f1808abcfd3ceeb5e0769eecd61f80f3c0cb5ebbf019aa656df273c6b3dcde7c51bafcc61d07b87dde67b467a3a8e1dd5ee44f54e42fc8f2725e66bbe3399bf70727dd8ee62f013af22f72a9ba706a16dba214d386cde5d0dc01bea33582e5b50004f9a6547d111181c804fb2ca9081df7c903556c39caa36d24f98f65570dd9a70a9cecd7f39ec3a45e667e02bb847810f5ca975b0dc99c7bbb57ada9a7da8b52aba60af3523dd6e1191c352aaf90ca67bd36af3ce7ecb2dc03c910387a0d6acd5e1db997007691c8b5a6cc0f1c80d039ec9a25cb784040ecd99922b5bb667a3747eed8b65b74340db4fe49db7b632e2e22412a6d95b75e931dc30076496a24f5eedc9909bc6351e6ea121882b89fda68ba578e91ec779466aff046b6474eb8e10f1d5beba98d72d6042fa020b3193965148f81a7aeaba3e5db3ba2673995eca1684f91e0a5be52467c8d578e2425cb";
    private const string BASE_TOKEN_ADDRESS = "http://internal.cloud.midasconsoles.com/api/v2";
    private const string TOKEN_ADDRESS = BASE_TOKEN_ADDRESS + "/auth/tokens";
    private const string UUID = "c67d049a-9854-4e4a-916d-1be9fafde57a";

    private const string MANIFEST = "manifest";
    private const string FILE = MANIFEST + "/GeneralData1.json";
    private const string FILE_TEST = MANIFEST + "/test.json";

    private const string BASE = "http://internal.cloud.midasconsoles.com:9095/builder/v1/";

    private const string AUT = "Authorization";
    private string BEARING = "";

    IEnumerator StartUp()
    {
        Debug.LogError("StartUp");
        UnityEngine.WWWForm form = new UnityEngine.WWWForm();
        form.AddField("client", UUID);
        UnityWebRequest requestU = UnityWebRequest.Post(TOKEN_ADDRESS, form);

        yield return requestU.SendWebRequest();

        if ((requestU.isNetworkError == false && requestU.responseCode == (int)System.Net.HttpStatusCode.OK) == false)
        {
            Debug.LogError($"responseWeb {requestU.error}");
            yield break;
        }
        var parseData = JObject.Parse(requestU.downloadHandler.text);
        var proofSalt = Convert.ToString(parseData["client"]["proofSalt"]);
        var proof = Convert.ToString(parseData["client"]["proof"]);
        var salt = Convert.ToString(parseData["client"]["salt"]);
        var responsePath = Convert.ToString(parseData["responsePath"]);
        responsePath = BASE_TOKEN_ADDRESS + responsePath;


        var secretPlusProofSalt = SECTRET + proofSalt;
        var validCheck = SHA256HexHashString(secretPlusProofSalt).ToLower();

        Debug.LogError($"Is proof valid {validCheck == proof}");


        var secretPlusSalt = SECTRET + salt;
        var response = SHA256HexHashString(secretPlusSalt).ToLower();
        UnityEngine.WWWForm form1 = new UnityEngine.WWWForm();
        form1.AddField("clientResponse", response);
        UnityWebRequest responseWeb = UnityWebRequest.Post(responsePath, form1);

        yield return responseWeb.SendWebRequest();
        Debug.LogError($"responseWeb {responseWeb.downloadHandler.text}");
        parseData = JObject.Parse(responseWeb.downloadHandler.text);
        var token = Convert.ToString(parseData["token"]);

        BEARING = "Bearer " + token;
    }


    IEnumerator PutFile()
    {
        GeneralData newData = new GeneralData();
        newData.m_GridInfo = new GridInfo();
        newData.m_TimeOfDay = 3;
        string jsonString = Json.Unity.WriteToText<GeneralData>(newData, true);

        string file = BASE + FILE;
        UnityWebRequest requestU = UnityWebRequest.Put(BASE + FILE, jsonString);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        yield return requestU.SendWebRequest();

        if (requestU.isNetworkError == false && requestU.responseCode == (int)System.Net.HttpStatusCode.OK)
        {
            Debug.LogError($"PutFile requestU.uploadedBytes {requestU.uploadedBytes}   uploadedBytes {requestU.uploadHandler.data}   progress {requestU.uploadProgress * 100}%    {jsonString},    ");
            Debug.LogError($"PutFile isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
        }
        else
        {
            Debug.LogError($"PutFile FAIL:    isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
        }
    }


    IEnumerator GetManifest()
    {
        string file = BASE + MANIFEST;
        UnityWebRequest requestU = UnityWebRequest.Get(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        yield return requestU.SendWebRequest();

        Debug.LogError($"GetManifest {requestU.downloadHandler.text}");
        Debug.LogError($"isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
    }

    IEnumerator DeleteFile()
    {
        string file = BASE + FILE;
        UnityWebRequest requestU = UnityWebRequest.Delete(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        yield return requestU.SendWebRequest();

        Debug.LogError($"isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
    }

    IEnumerator DeleteFileTest()
    {
        string file = BASE + FILE_TEST;
        UnityWebRequest requestU = UnityWebRequest.Delete(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        yield return requestU.SendWebRequest();

        Debug.LogError($"isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
    }


    IEnumerator GetFile()
    {
        string file = BASE + FILE;
        UnityWebRequest requestU = UnityWebRequest.Get(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        // "missing Location header"
        requestU.redirectLimit = 100;
        yield return requestU.SendWebRequest();

        if (requestU.isNetworkError == false && requestU.responseCode == (int)System.Net.HttpStatusCode.OK)
        {          
            Debug.LogError($"GetFile data:{requestU.downloadHandler.text}   requestU.downloadedBytes {requestU.downloadedBytes}");
            Debug.LogError($"GetFile isHttpError:{requestU.isHttpError}  {requestU.error}   {requestU.responseCode}   {file}");
        }
        else
        {
            Debug.LogError($"GetFile FAIL:    isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
        }
    }


    IEnumerator GetFileTest()
    {
        string file = BASE + FILE_TEST;
        UnityWebRequest requestU = UnityWebRequest.Get(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.redirectLimit = 100;
        yield return requestU.SendWebRequest();

        if (requestU.isNetworkError == false && requestU.responseCode == (int)System.Net.HttpStatusCode.OK)
        {
            Debug.LogError($"GetFile data:{requestU.downloadHandler.text}");
            Debug.LogError($"GetFile isHttpError:{requestU.isHttpError}  {requestU.error}   {requestU.responseCode}   {file}");
        }
        else
        {
            Debug.LogError($"GetFile FAIL:    isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");
        }
    }



    private string SHA256HexHashString(string StringIn)
    {
        string hashString;
        using (var sha256 = SHA256Managed.Create())
        {

            var bytes = new byte[StringIn.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                var s = StringIn.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(s, 16);
            }

            var hash = sha256.ComputeHash(bytes);
            hashString = ToHex(hash, false);
        }
        return hashString;
    }

    private string ToHex(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

        return result.ToString();
    }

    [InspectorButton]
    public void GetList(Action <List<string>> callback)
    {
        StartCoroutine(InternalGetList(callback));
    }

    IEnumerator InternalGetList(Action<List<string>> callback)
    {
        string file = BASE + MANIFEST;
        UnityWebRequest requestU = UnityWebRequest.Get(file);
        requestU.SetRequestHeader(AUT, BEARING);
        requestU.SetRequestHeader("Content-Type", "application/json");
        requestU.SetRequestHeader("Accept", "application/json");

        yield return requestU.SendWebRequest();

        Debug.LogError($"GetManifest {requestU.downloadHandler.text}");
        
        Debug.LogError($"isHttpError:{requestU.isHttpError}  {requestU.error}  {file}");


        // split and remove unwanted characters
        string[] parts = requestU.downloadHandler.text.Split(','); // Call Split method
        List<string> list = new List<string>(parts); // Use 


        var charsToRemove = new string[] { @"\\", "\"" };
        for (int i = 0; i < list.Count; i++)
        {
            foreach (var c in charsToRemove)
            {
                list[i] = list[i].Replace(c, string.Empty);
            }

            if (list[i].IndexOf('[') != -1)
            {
                list[i] = list[i].Substring(list[i].IndexOf('[') + 1);
            }

            if (list[i].IndexOf(']') != -1)
            {
                list[i] = list[i].Substring(0, list[i].IndexOf("]") + 0);
            }
        }


        // get unique items which ends in 
        var endsWith = $"{nameof(HouseBuilderContentsJsonData)}.json";
        List<string> seperateList = new List<string>();     
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item.EndsWith(endsWith) == true)
            {
                seperateList.Add(item);
            }
        }
           
        callback?.Invoke(seperateList);
    }




}