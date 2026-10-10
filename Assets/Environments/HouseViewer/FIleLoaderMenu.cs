using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HouseViewer
{

    public class FileLoaderMenu : MonoBehaviour
    {
        public static FileLoaderMenu Instance;

        private Button m_BtnLoad;
        private Transform m_ItemPrefabParent;
        private List<Toggle> m_ButtonList = new List<Toggle>();
        private int m_ListIndex = 0;
        private AudioClip m_ButtonPressClip;
        private AudioSource m_ButtonPressSource;

        private PoolManagerLocalComponent<Toggle> m_ItemsPool;

        public void Initialise()
        {
            Instance = this;

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(null, "ButtonPress", (clip) =>
            {
                m_ButtonPressSource = this.gameObject.ForceComponent<AudioSource>();
                m_ButtonPressSource.clip = clip;
                Debug.LogError("FileLoaderMenu");
            });

            var canvas = gameObject.GetComponentInChildren<Canvas>();
            canvas.gameObject.ForceComponent<VRUICanvas>();
            m_BtnLoad = gameObject.GetComponentInChildren<Button>();
            m_BtnLoad.onClick.AddListener(LoadEvent);

            var ItemPrefab = gameObject.transform.SearchComponent<Toggle>("SavedPlot");
            ItemPrefab.SetActive(false);


            m_ItemsPool = new PoolManagerLocalComponent<Toggle>(ItemPrefab.gameObject, this.gameObject.transform);

            m_ItemPrefabParent = ItemPrefab.transform.parent;
            SetupList(null);
            this.gameObject.SetActive(false);
        }


        private void OnEnable()
        {
            m_ListIndex = 0;
            HighlightSelection();
            this.gameObject.PlaceInfrontMainCamera(2);
        }

        private void OnDisable()
        {
            m_ListIndex = 0;
            HighlightSelection();
            this.gameObject.PlaceInfrontMainCamera(2);
        }

        private void HighlightSelection()
        {
            m_ButtonList.ForEach(e => e.SetIsOnWithoutNotify(false));
            if (m_ButtonList.Count > 0)
            {
                m_ButtonList[m_ListIndex].SetIsOnWithoutNotify(true);
            }
        }


        public void SetupList(List<string> list)
        {
            m_ItemsPool.DeSpawnAll();
            if (list != null)
            {
                list.Sort();
            }
            m_ButtonList.Clear();

            if (list == null)
            {
                list = new List<string>();
            }
            if(list.Count == 0)
            {
                list.Add("NOTHING SAVED!");
            }
            m_ListIndex = 0;
            for (int i = 0; i < list.Count; i++)
            {
                int index = i;
                var newButton = m_ItemsPool.SpawnObject(m_ItemPrefabParent);
                newButton.SetActive(true);
                newButton.transform.ClearLocals();
                var text = newButton.GetComponentInChildren<TextMeshProUGUI>(true);
                text.text = list[i];
                newButton.onValueChanged.AddListener((state) =>
                {
                    m_ListIndex = index;
                    HighlightSelection();
                    m_ButtonPressSource.Play();

                });
                m_ButtonList.Add(newButton);
            }
        }


        private void LoadEvent()
        {
            m_ButtonPressSource.Play();
            string fileName = m_ButtonList[m_ListIndex].GetComponentInChildren<TextMeshProUGUI>().text;
            Debug.Log($"load file: '{fileName}'");
            Core.Audio.SteamAudioManagerRef.StartOfFileName = fileName;
            HouseViewerRoot.Instance.Load(fileName, HouseBuilder.LoadSaveType.Network);
            this.SetActive(false);
        }

    }
}
