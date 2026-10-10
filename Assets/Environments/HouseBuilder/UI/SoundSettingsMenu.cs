using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class SoundSettingsMenu : MonoBehaviour
    {
        private CanvasGroup m_Group;
        private bool m_IsGroupActive;

        private Button m_BtnMicrophone;
        private Button m_BtnConnect;
        private Button m_BtnPlay;
        private Button m_BtnStop;
        private Toggle m_TglLoop;
        private Slider m_SldVolume;
        private Slider m_SldPercentage;
        private Dropdown m_DrpChannelChoice;
        private TMP_Text m_MusicNameText;
        private GameObject m_MusicNameGameObject;

        private string m_MicrophoneChoice;
        private float[] samples;
        private AudioClip m_clip;


        private List<InteractionGameObject> m_CurrentSelections = null;
        private SimpleDictionaryList<Interaction, ItemSettingsMenu.CombinedMetadata> m_MetaList = new SimpleDictionaryList<Interaction, ItemSettingsMenu.CombinedMetadata>();

        public void PostInitialise(CatalogueMenu catalogue) //Delayed because catalogue must be initialised before this
        {
            catalogue.OnSoundPicked += OnSoundPicked;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }


        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_IsGroupActive = false;

            
            m_BtnConnect = transform.Find("ConnectButton").GetComponent<Button>(); //off by default
            m_BtnConnect.onClick.AddListener(ConnectButtonPressed);


            m_BtnMicrophone = transform.Find("PlayMicrophone").GetComponent<Button>(); //off by default
            m_BtnMicrophone.onClick.AddListener(PlayMicrophonePressed);

            m_BtnPlay = transform.Find("PlayButton").GetComponent<Button>(); //off by default
            m_BtnPlay.onClick.AddListener(PlayButtonPressed);

            m_BtnStop = transform.Find("StopButton").GetComponent<Button>(); //off by default
            m_BtnStop.onClick.AddListener(StopButtonPressed);

            m_MusicNameGameObject = transform.SearchComponent<Transform>("MusicNameHolder").gameObject;
            m_MusicNameText = transform.SearchComponent<TMP_Text>("MusicNameText");

            m_TglLoop = transform.Find("LoopToggle").GetComponent<Toggle>(); //off by default
            m_TglLoop.onValueChanged.AddListener(LoopButtonPressed);

            m_SldVolume = transform.Find("VolumeSlider").GetComponent<Slider>(); //off by default
            m_SldVolume.onValueChanged.AddListener(VolumeChange);

            m_SldPercentage = transform.Find("PercentageSlider").GetComponent<Slider>(); //off by default
            m_SldPercentage.onValueChanged.AddListener(PercentageChange);


            m_DrpChannelChoice = transform.Find("ChannelChoice").GetComponent<Dropdown>(); //off by default
            m_DrpChannelChoice.ClearOptions();

            List<string> options = new List<string>();

            foreach (var enumValue in Enum.GetValues(typeof(SteamSoundChannels.PlayChannel)))
            {
                options.Add(enumValue.ToString());
            }
            m_DrpChannelChoice.AddOptions(options);
            m_DrpChannelChoice.onValueChanged.AddListener(ChannelChoiceChange);



            HouseBuilderRoot.Instance.OnObjectSelected += (state, data) =>
            {
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Sound)
                {
                    m_CurrentSelections = data;
                    SetActive(state, false);
                }
            };

            HouseBuilderRoot.Instance.OnModeTransitionBegin += () => SetActive(false, true);

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void ChannelChoiceChange(int arg0)
        {
            SteamSoundChannels.PlayChannel choice = (SteamSoundChannels.PlayChannel)arg0;
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                item.SteamSoundRef.SteamSoundChannelsRef.ForEach(e => e.m_PlayChannel = choice);
                item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_PlayChannel = choice;
                Debug.LogError("need better way and save load");
            }
        }

        private void PlayMicrophonePressed()
        {
#if !UNITY_WEBGL // Unity WebGL hasn't got the microphone class.
            var items = Microphone.devices;
            GenericListMenu.ShowList("Choose Device", items.ToList(), (microphoneChoice) =>
            {
                m_MicrophoneChoice = microphoneChoice;
                if (string.IsNullOrEmpty(microphoneChoice) == false)
                {
                    for (int i = 0; i < m_CurrentSelections.Count; i++)
                    {
                        InteractionGameObject item = m_CurrentSelections[i];
                        var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                        foreach (var audio in allItems)
                        {
                            Debug.LogError($"m_MicrophoneChoice {m_MicrophoneChoice}");
                            audio.clip = Microphone.Start(m_MicrophoneChoice, true, 1, 4800);
                            audio.loop = true;
                            //while (!(Microphone.GetPosition(null) > 0)) { }
                            audio.Play();
                            m_clip = audio.clip;
                            samples = new float[m_clip.channels];
                        }
                    }
                }
            });
#endif
        }

        private void LoopButtonPressed(bool arg0)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                foreach (var audio in allItems)
                {
                    audio.loop = arg0;
                    item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_Loop = arg0;
                }
            }
        }

        private void StopButtonPressed()
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                foreach (var audio in allItems)
                {
                    audio.Stop();
                }
            }
        }

        private void PlayButtonPressed()
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                foreach (var audio in allItems)
                {
                    audio.Play();
                }
            }
        }

        private void VolumeChange(float arg0)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                foreach (var audio in allItems)
                {
                    audio.volume = arg0;
                    item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_Volume = arg0;
                }
            }
        }


        private void PercentageChange(float arg0)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                foreach (var audio in allItems)
                {
                    m_SldPercentage.value = arg0;
                    audio.time = audio.clip.length * arg0;
                    item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_Time = arg0;
                }
            }
        }

        private void Update()
        {
            if (m_CurrentSelections != null && m_CurrentSelections.Count > 0)
            {
                InteractionGameObject item = m_CurrentSelections.First();
                var combined = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
                float percentage = 0;
                switch (item.SteamSoundRef.AudioSoundType)
                {
                    case ContentSteamAudioMetaData.SoundEnumType.None:
                        break;
                    case ContentSteamAudioMetaData.SoundEnumType.Create:
                        if (combined != null && combined.Count > 0)
                        {
                            var audioCreate = combined.First();

                            if (audioCreate.clip != null)
                            {
                                percentage = audioCreate.time / audioCreate.clip.length;
                            }
                            else
                            {
                                percentage = 0;
                            }
                            m_SldPercentage.SetValueWithoutNotify(percentage);
                            item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_Time = percentage;

                            m_MusicNameText.text = item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_AudioFileName;
                        }
                        else
                        {
                            m_MusicNameText.text = "";
                        }
                        break;
                    case ContentSteamAudioMetaData.SoundEnumType.Modify:
                        break;
                    case ContentSteamAudioMetaData.SoundEnumType.Play:
                        if (combined != null && combined.Count > 0)
                        {
                            var audio = combined.First();
                            if (audio.clip != null)
                            {
                                percentage = audio.time / audio.clip.length;
                            }
                            else
                            {
                                percentage = 0;
                            }
                            m_SldPercentage.SetValueWithoutNotify(percentage);
                            item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_Time = percentage;
                        }
                        break;
                    default:
                        break;
                }

                if (m_clip != null)
                {
#if !UNITY_WEBGL // Unity WebGL hasn't got the microphone class.
                    int pos = Microphone.GetPosition(m_MicrophoneChoice);
                    m_clip.GetData(samples, pos);
                    Debug.LogError(samples.Sum());
#endif
                }
                
            }
        }

        private void ConnectButtonPressed()
        {
            Debug.LogError("Connect");
            HouseBuilderRoot.Instance.InputSound.CurrentInputMode = HouseBuilderInput.InputMode.Creating;
        }


        public void SetActive(bool state, bool manual)
        {
            if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Sound)
            {
                state = false;
                if (m_CurrentSelections != null)
                {
                    m_CurrentSelections.Clear();
                }
            }

            if (state == true)
            {
                if (m_CurrentSelections != null && m_CurrentSelections.Count > 0)
                {
                    InteractionGameObject item = m_CurrentSelections.First();
                    HouseBuilderConstants.SteamAudioData data = item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData;
                    switch (item.SteamSoundRef.AudioSoundType)
                    {
                        case ContentSteamAudioMetaData.SoundEnumType.None:
                            break;
                        case ContentSteamAudioMetaData.SoundEnumType.Create:
                            m_BtnPlay.SetActive(true);
                            m_BtnStop.SetActive(true);
                            m_TglLoop.SetActive(true);
                            m_SldPercentage.SetActive(true);
                            m_SldVolume.SetActive(false);
                            m_DrpChannelChoice.SetActive(false);
                            m_MusicNameGameObject.SetActive(true);
                            m_DrpChannelChoice.value = (int)data.m_PlayChannel;

                            if (string.IsNullOrEmpty(data.m_AudioFileName) == false)
                            {
                                OnSoundPicked(data.m_AudioFileName, data.m_AudioFilerName);
                            }
                            m_BtnPlay.interactable = false;
                            m_BtnStop.interactable = false;
                            m_TglLoop.interactable = false;
                            m_SldPercentage.interactable = false;
                            HBGUIManager.Instance.CatalogueMenuRef.SetRelevantObjectsInteractable(true);
                            foreach (var connected in item.SteamSoundRef.m_InteractionDirectlyConnectedTo)
                            {
                                if (connected.SteamSoundRef.AudioSoundType == ContentSteamAudioMetaData.SoundEnumType.Play)
                                {
                                    m_BtnPlay.interactable = true;
                                    m_BtnStop.interactable = true;
                                    m_TglLoop.interactable = true;
                                    m_SldPercentage.interactable = true;
                                }
                            }
                            Debug.LogError("Create soundMenu needed");
                            break;

                        case ContentSteamAudioMetaData.SoundEnumType.Modify:
                            m_BtnPlay.SetActive(false);
                            m_BtnStop.SetActive(false);
                            m_TglLoop.SetActive(false);
                            m_SldPercentage.SetActive(false);
                            m_SldVolume.SetActive(true);
                            m_DrpChannelChoice.SetActive(true);
                            m_MusicNameGameObject.SetActive(false);
                            m_DrpChannelChoice.value = (int)item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_PlayChannel;
                            HBGUIManager.Instance.CatalogueMenuRef.SetRelevantObjectsInteractable(false);
                            Debug.LogError("Modify soundMenu needed");
                            break;

                        case ContentSteamAudioMetaData.SoundEnumType.Play:
                            m_BtnPlay.SetActive(false);
                            m_BtnStop.SetActive(false);
                            m_TglLoop.SetActive(false);
                            m_SldPercentage.SetActive(false);
                            m_SldVolume.SetActive(true);
                            m_DrpChannelChoice.SetActive(true);
                            m_MusicNameGameObject.SetActive(false);
                            m_DrpChannelChoice.value = (int)item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_PlayChannel;
                            HBGUIManager.Instance.CatalogueMenuRef.SetRelevantObjectsInteractable(false);
                            Debug.LogError("Play soundMenu needed");
                            var audio = item.SteamSoundRef.AudioSources.First();
                            if (audio != null)
                            {
                                m_SldVolume.SetValueWithoutNotify(audio.volume);
                                m_TglLoop.SetIsOnWithoutNotify(audio.loop);
                            }
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    HBGUIManager.Instance.CatalogueMenuRef.SetRelevantObjectsInteractable(false);
                }
            }
            else
            {
                HBGUIManager.Instance.CatalogueMenuRef.SetRelevantObjectsInteractable(false);
            }


            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }


        private void OnSoundPicked(string soundName, string filter)
        {
            if (m_CurrentSelections == null)
            {
                return;
            }

            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_AudioFileName = soundName;
                item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_AudioFilerName = filter;
                item.SteamSoundRef.AssignSavedDetails();
            }
        }


    }
}