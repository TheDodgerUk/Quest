using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{
    public class SettingsPanel : MonoBehaviour
    {
        public const string ITEM_PREFAB = "SequenceOptionsItemPrefab";
        public const string SOURCE_PREFAB = "TimeLineSourceItemPrefab";

        private PoolManagerLocalComponent<Transform> m_PoolManagerOptions;
        private ToggleGroup m_OptionsToggleGroup;
        private VerticalLayoutGroup m_OptionsVerticalLayoutGroup;


        private PoolManagerLocalComponent<Transform> m_PoolManagerSoundSource;
        private VerticalLayoutGroup m_SoundSourceVerticalLayoutGroup;
        private ToggleGroup m_SoundSourceToggleGroup;


        private OscGenericData m_TimeLineAudioData;

        private OscGenericData m_LocalData = new OscGenericData();

        private Text m_TxtLocalFileName;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();

            AssignOptions();
            AssignSoundSource();

            var close = this.transform.SearchComponent<Button>("CloseButton");
            close.onClick.AddListener(() =>
            {
                this.gameObject.SetActive(false);
            });

            var ok = this.transform.SearchComponent<Button>("OkButton");
            ok.onClick.AddListener(() =>
            {
                m_TimeLineAudioData.m_AudioName = m_LocalData.m_AudioName;
                m_TimeLineAudioData.m_SoundSource = m_LocalData.m_SoundSource;

                if (string.IsNullOrEmpty(DataContainer.Instance.GenericDataDataRef.m_AudioName) == false)
                {
                    Core.Network.GetCustomFileAudioClip(DataContainer.Instance.GenericDataDataRef.m_AudioName, (clip) =>
                    {
                        DataContainer.Instance.GenericDataDataRef.m_AudioClip = clip;
                        OscControllerRoot.Instance.AudioSourceRef.clip = clip;
                    });
                }
                else
                {
                    DataContainer.Instance.GenericDataDataRef.m_AudioClip = null;
                    OscControllerRoot.Instance.AudioSourceRef.clip = null;
                }

                OscControllerRoot.Instance.TimeLinePanelRef.ScaleAllItems();
                OscControllerRoot.Instance.TimeLinePanelRef.CreateAndAssignWave(DataContainer.Instance.GenericDataDataRef);
                this.gameObject.SetActive(false);
            });


            var loadFromFile = this.transform.SearchComponent<Button>("LocalFileButton");
            loadFromFile.onClick.AddListener(() =>
            {
                Core.Audio.PromptAndLoadAudioFile((clipData) =>
                {
                    if (clipData != null)
                    {
                        DataContainer.Instance.GenericDataDataRef.m_AudioClip = clipData.m_AudioClip;
                        DataContainer.Instance.GenericDataDataRef.m_AudioName = clipData.m_AudioClip.name;
                        DataContainer.Instance.GenericDataDataRef.m_LocalFullAudioName = clipData.m_FullName;
                        DataContainer.Instance.GenericDataDataRef.m_IsLocalFile = true;
                        m_TxtLocalFileName.text = DataContainer.Instance.GenericDataDataRef.m_AudioName;
                    }
                    else
                    {
                        DataContainer.Instance.GenericDataDataRef.m_AudioClip = null;
                        DataContainer.Instance.GenericDataDataRef.m_AudioName = "";
                        DataContainer.Instance.GenericDataDataRef.m_LocalFullAudioName = "";
                        DataContainer.Instance.GenericDataDataRef.m_IsLocalFile = true;
                        m_TxtLocalFileName.text = "";
                    }
                    OscControllerRoot.Instance.TimeLinePanelRef.CreateAndAssignWave(DataContainer.Instance.GenericDataDataRef);
                });
            });

            m_TxtLocalFileName = this.transform.SearchComponent<Text>("LocalFileName");
        }


        private void AssignOptions()
        {
            var options = this.transform.SearchComponent<Transform>("OptionsGroup");
            m_OptionsVerticalLayoutGroup = options.GetComponentInChildren<VerticalLayoutGroup>();
            m_OptionsToggleGroup = m_OptionsVerticalLayoutGroup.GetComponent<ToggleGroup>();
            m_OptionsToggleGroup.allowSwitchOff = true;

            var itemPrefab = CachedObjects.Instance.GetPrefab(ITEM_PREFAB);
            m_PoolManagerOptions = new PoolManagerLocalComponent<Transform>(itemPrefab, m_OptionsVerticalLayoutGroup.transform);
        }



        private void AssignSoundSource()
        {
            var sound = this.transform.SearchComponent<Transform>("SoundSource");
            m_SoundSourceVerticalLayoutGroup = sound.GetComponentInChildren<VerticalLayoutGroup>();
            m_SoundSourceToggleGroup = m_SoundSourceVerticalLayoutGroup.GetComponent<ToggleGroup>();
            m_SoundSourceToggleGroup.allowSwitchOff = true;

            var sourcePrefab = CachedObjects.Instance.GetPrefab(SOURCE_PREFAB);
            m_PoolManagerSoundSource = new PoolManagerLocalComponent<Transform>(sourcePrefab, m_SoundSourceVerticalLayoutGroup.transform);
        }




        public void FillInData(OscGenericData timeLineAudioData)
        {
            m_TimeLineAudioData = timeLineAudioData;
            m_LocalData.m_AudioName = m_TimeLineAudioData.m_AudioName;
            m_LocalData.m_SoundSource = m_TimeLineAudioData.m_SoundSource;
            OptionsData();
            SoundSourceData();

            m_TxtLocalFileName.text = m_LocalData.m_AudioName;
        }

        private void OptionsData()
        {
            m_PoolManagerOptions.DeSpawnAll();
            List<string> names = new List<string>();

            Core.Network.GetCustomFileListAudio((items) =>
            {
                foreach (var item in items)
                {
                    if (names.Contains(item) == false)
                    {
                        names.Add(item);
                    }
                }

                Toggle selectedToggle = null;
                foreach (var item in names)
                {
                    string itemName = item;
                    var obj = m_PoolManagerOptions.SpawnObject(m_OptionsVerticalLayoutGroup.transform);
                    obj.transform.localScale = Vector3.one;
                    obj.GetComponentInChildren<Text>().text = itemName;

                    Toggle toggle = MakeSelectable(obj, ref m_OptionsToggleGroup);

                    var nameSelect = obj.gameObject.ForceComponent<FakeButton>();
                    nameSelect.onClick.RemoveAllListeners();
                    nameSelect.onClick.AddListener(() =>
                    {
                        m_LocalData.m_AudioName = itemName;
                        m_TimeLineAudioData.m_IsLocalFile = false;
                        toggle.Select();
                    });

                    if (selectedToggle == null && m_TimeLineAudioData != null && m_LocalData.m_AudioName == itemName)
                    {
                        selectedToggle = toggle;
                    }
                }

                if (selectedToggle != null)
                {
                    selectedToggle.isOn = true;
                    selectedToggle.Select();
                    
                }
            });

        }





        private void SoundSourceData()
        {
            m_PoolManagerSoundSource.DeSpawnAll();
            Toggle selectedToggle = null;
            foreach (string name in Enum.GetNames(typeof(SoundSource)))
            {

                string itemName = name.ToString();
                var obj = m_PoolManagerSoundSource.SpawnObject(m_SoundSourceVerticalLayoutGroup.transform);
                obj.transform.localScale = Vector3.one;
                obj.GetComponentInChildren<Text>().text = itemName;

                Toggle toggle = MakeSelectable(obj, ref m_SoundSourceToggleGroup);

                var sourceSelected = obj.gameObject.ForceComponent<FakeButton>();
                sourceSelected.onClick.RemoveAllListeners();
                sourceSelected.onClick.AddListener(() =>
                {
                    m_LocalData.m_SoundSource = (SoundSource)Enum.Parse(typeof(SoundSource), itemName);
                });

                if (selectedToggle == null && m_TimeLineAudioData != null && m_LocalData.m_SoundSource == (SoundSource)Enum.Parse(typeof(SoundSource), itemName))
                {
                    selectedToggle = toggle;
                }
            }

            if (selectedToggle != null)
            {
                selectedToggle.isOn = true;
                selectedToggle.Select();
            }
        }

        private Toggle MakeSelectable(Transform obj, ref ToggleGroup toggleGroup)
        {
            var toggle = obj.GetComponent<Toggle>();
            toggle.group = toggleGroup;

            ColorBlock col = toggle.colors;
            col.selectedColor = OscControllerRoot.SELECTED;
            toggle.colors = col;

            return toggle;
        }



    }

}


