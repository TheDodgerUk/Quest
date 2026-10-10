using SteamAudio;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HouseBuilder
{
    public class SteamSound 
    {
        private const int BAD_FREQUENCY = 44100;
        private const int GOOD_FREQUENCY = 48000;
        private List<string> m_ErrorMessages = new List<string>();

        public  SteamAudioMaterial m_SteamAudioMaterial;
        private SteamAudioGeometry m_SteamAudioGeometry;
        private Interaction m_RootObject;
        private List<AudioSource> m_AudioSources = new List<AudioSource>();
        public List<AudioSource> AudioSources => m_AudioSources;

        private List<SteamSoundChannels> m_SteamSoundChannels = new List<SteamSoundChannels>();
        public List<SteamSoundChannels> SteamSoundChannelsRef => m_SteamSoundChannels;

        public ContentSteamAudioMetaData.SoundEnumType AudioSoundType { get; private set; }

        public List<Interaction> m_InteractionDirectlyConnectedTo = new List<Interaction>();

        public ContentSteamAudioMetaData MetaDataRef { get; private set;  }

        public SteamSound(Interaction baseItem)
        {
            m_RootObject = baseItem;

            if (m_RootObject.gameObject.GetComponent<SteamAudio.SteamAudioMaterial>() == null)
            {
                m_SteamAudioMaterial = m_RootObject.gameObject.ForceComponent<SteamAudio.SteamAudioMaterial>();
                m_SteamAudioMaterial.Preset = SteamAudio.MaterialPreset.Concrete;
            }

            m_SteamAudioGeometry = m_RootObject.gameObject.ForceComponent<SteamAudio.SteamAudioGeometry>();
            m_SteamAudioGeometry.exportAllChildren = true;

        }

        public void SetupSteamSoundGeometry(ContentSteamAudioMetaData data, GameObject rootObject)
        {
            MetaDataRef = data;
            MetaDataRef.CollectAllData(rootObject);
            AudioSoundType = data.m_SoundEnumType;
            if (m_SteamAudioGeometry != null)
            {
                UnityEngine.GameObject.Destroy(m_SteamAudioGeometry);

                if (data.m_SoundGameObject != null)
                {
                    m_SteamAudioGeometry = data.m_SoundGameObject.ForceComponent<SteamAudio.SteamAudioGeometry>();
                    m_SteamAudioGeometry.exportAllChildren = data.m_ExportAllChildren;
                }
                else
                {
                    Debug.LogError($"Could not find {data.m_SoundGameObjectName}, on {m_RootObject.name}");
                }
            }


            if (m_SteamAudioMaterial != null)
            {
                UnityEngine.GameObject.Destroy(m_SteamAudioMaterial);

                m_SteamAudioMaterial = m_SteamAudioGeometry.gameObject.ForceComponent<SteamAudioMaterial>();
                m_SteamAudioMaterial.Preset = (MaterialPreset)(int)data.m_MaterialPreset;
                if (m_SteamAudioMaterial.Preset == MaterialPreset.Custom)
                {
                    m_SteamAudioMaterial.Value.LowFreqAbsorption = data.m_MaterialValue.LowFreqAbsorption;
                    m_SteamAudioMaterial.Value.MidFreqAbsorption = data.m_MaterialValue.MidFreqAbsorption;
                    m_SteamAudioMaterial.Value.HighFreqAbsorption = data.m_MaterialValue.HighFreqAbsorption;

                    m_SteamAudioMaterial.Value.Scattering = data.m_MaterialValue.Scattering;

                    m_SteamAudioMaterial.Value.LowFreqTransmission = data.m_MaterialValue.LowFreqTransmission;
                    m_SteamAudioMaterial.Value.MidFreqTransmission = data.m_MaterialValue.MidFreqTransmission;
                    m_SteamAudioMaterial.Value.HighFreqTransmission = data.m_MaterialValue.HighFreqTransmission;
                }
                else
                {
                    /////does its self  m_SteamAudioMaterial.Value.CopyFrom(MaterialPresetList.PresetValue((int)m_SteamAudioMaterial.Preset));
                }
            }

            var stack = System.Environment.StackTrace;
            DisableBadSteamSoundGeometry(stack);
            SetupSteamAudioSources();
        }

        private void SetupSteamAudioSources()
        {
            if (AudioSoundType == ContentSteamAudioMetaData.SoundEnumType.Play)
            {
                m_AudioSources = m_RootObject.gameObject.GetComponentsInChildren<AudioSource>(true).ToList();
                foreach (var sound in m_AudioSources)
                {
                    var channel = sound.gameObject.ForceComponent<SteamSoundChannels>();
                    m_SteamSoundChannels.Add(channel);
                    sound.spatialize = true;
                    sound.spatializePostEffects = true;
                    sound.spatialBlend = System.Convert.ToInt32(true);
                    sound.loop = false;
                    if (sound.playOnAwake == true)
                    {
                        sound.playOnAwake = false; // safty check
                        sound.Stop();
                    }

                    var steam = sound.gameObject.ForceComponent<SteamAudio.SteamAudioSource>();
                    steam.directBinaural = true;
                    steam.interpolation = SteamAudio.HRTFInterpolation.Bilinear;
                    steam.physicsBasedAttenuation = true;

                    //sound Occulation
                    steam.occlusionMode = SteamAudio.OcclusionMode.OcclusionWithFrequencyDependentTransmission;
                    steam.occlusionMethod = OcclusionMethod.Partial;
                    steam.occlusionSamples = 256;
                    steam.airAbsorption = true;

                    // indirectSound
                    steam.reflections = true;
                    steam.simulationType = SteamAudio.SourceSimulationType.BakedStaticSource;
                    steam.physicsBasedAttenuationForIndirect = true;

                    steam.dipolePower = 4f;
                    steam.dipoleWeight = 0.75f;



                    var clip = sound.clip;
                    ////////if (clip != null)
                    ////////{
                    ////////    if (clip.frequency == BAD_FREQUENCY)
                    ////////    {
                    ////////        //https://dev.azure.com/MusicTribe/MI_R00398/_workitems/edit/17496/
                    ////////        steam.reflections = false;
                    ////////        Debug.LogError($"reflections have been disabled due to poppong sounds    Clip {BAD_FREQUENCY}, {clip.name}", m_RootObject);
                    ////////        Debug.LogError($"System does not like clip.frequency == {BAD_FREQUENCY}", m_RootObject);
                    ////////    }
                    ////////}
                    ////////else
                    ////////{
                    ////////    Debug.LogError($"clip is null in item : {baseItem.gameObject.GetGameObjectPath()}", baseItem.gameObject);
                    ////////}
                }
            }
        }

        public void StopConnectedSounds()
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;
            var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
            foreach (var audioItem in allItems)
            {
                audioItem.Stop();
            }
        }

        public void ResetConnectedSounds()
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;
            var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
            foreach (var audioItem in allItems)
            {
                audioItem.Stop();
                audioItem.time = 0;
            }
        }

        public void PlayConnectedSounds()
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;
            var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
            foreach (var audioItem in allItems)
            {
                if (audioItem.clip != null && audioItem.clip.frequency == BAD_FREQUENCY)
                {
                    if(m_ErrorMessages.Contains(audioItem.clip.name) == false)
                    {
                        Debug.LogError($"{audioItem.clip.name} is set to BAD_FREQUENCY == {BAD_FREQUENCY}");
                        m_ErrorMessages.Add(audioItem.clip.name);
                    }
                    
                }
                audioItem.Play();
            }
        }


        public float GetConnectedSoundsLength()
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;
            var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
            foreach (var audioItem in allItems)
            {
                if (audioItem.clip != null && audioItem.clip.frequency == BAD_FREQUENCY)
                {
                    if (m_ErrorMessages.Contains(audioItem.clip.name) == false)
                    {
                        Debug.LogError($"{audioItem.clip.name} is set to BAD_FREQUENCY == {BAD_FREQUENCY}");
                        m_ErrorMessages.Add(audioItem.clip.name);
                    }

                }
                return audioItem.clip.length;
            }
            return 0;
        }

        public void SetTimeConnectedSounds(float time)
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;
            var allItems = item.SteamSoundRef.GetAllDirectAndIndirectConnectedAudioSouces();
            foreach (var audioItem in allItems)
            {
                audioItem.time = time;
            }
        }



        private void DisableBadSteamSoundGeometry(string stack)
        {
            foreach (var audio in m_AudioSources)
            {
                if (audio != null)
                {
                    if (m_RootObject.MainCollider.bounds.Contains(audio.transform.position))
                    {
                        if (m_SteamAudioMaterial != null)
                        {
                            ConsoleExtra.Log($"removing, if sound is within object it cannot bouncem which gives zero effect on bouncing around room, needs looking at  {m_RootObject.name}  {stack}", m_RootObject.gameObject, ConsoleExtraEnum.EDebugType.Generic);
                            UnityEngine.GameObject.Destroy(m_SteamAudioMaterial);
                        }

                        if (m_SteamAudioGeometry != null)
                        {
                            ConsoleExtra.Log($"removing, if sound is within object it cannot bouncem which gives zero effect on bouncing around room, needs looking at  {m_RootObject.name}  {stack}", m_RootObject.gameObject, ConsoleExtraEnum.EDebugType.Generic);
                            UnityEngine.GameObject.Destroy(m_SteamAudioGeometry);
                        }
                    }
                }
            }

        }

        public List<AudioSource> GetAllDirectAndIndirectConnectedAudioSouces()
        {
            List<AudioSource> sources = new List<AudioSource>();
            var all = GetAllDirectAndIndirectConnected();
            foreach(var item in all)
            {
                sources.AddRange(item.SteamSoundRef.AudioSources);
            }
            return sources.Distinct().ToList();
        }


        public List<Interaction> GetAllDirectAndIndirectConnected()
        {
            List<Interaction> mainConnections = new List<Interaction>();
            List<Interaction> allConnections = new List<Interaction>();
            GetSubAllDirectAndIndirectConnected(m_InteractionDirectlyConnectedTo, ref mainConnections, ref allConnections);

            allConnections = allConnections.Distinct().ToList();
            return allConnections;
        }

        private void GetSubAllDirectAndIndirectConnected(List<Interaction> current, ref List<Interaction> main, ref List<Interaction> all)
        {
            for (int i = 0; i < current.Count; i++)
            {
                if(main.Contains(current[i]) == false)
                {
                    main.Add(current[i]);
                    all.AddRange(current[i].SteamSoundRef.m_InteractionDirectlyConnectedTo.ToList());
                    GetSubAllDirectAndIndirectConnected(current[i].SteamSoundRef.m_InteractionDirectlyConnectedTo, ref main, ref all);
                }
            }
        }



        public void AssignSavedDetails()
        {
            var data = m_RootObject.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData;
            var channel = data.m_PlayChannel;
            m_SteamSoundChannels.ForEach(e => e.m_PlayChannel = channel);
            AssignSound();
        }

        private void AssignSound()
        {
            InteractionGameObject item = (InteractionGameObject)m_RootObject;

            string filter = item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_AudioFilerName;
            string soundName = item.CatalogueGeneratedDataRef.m_JsonData.m_SteamAudioData.m_AudioFileName;


            foreach (var connected in item.SteamSoundRef.m_InteractionDirectlyConnectedTo)
            {
                var sources = connected.SteamSoundRef.AudioSources;
                foreach (var source in sources)
                {
                    if (string.IsNullOrEmpty(soundName) == true)
                    {
                        source.clip = null;
                        continue;
                    }
                    
                    if (source != null)
                    {
                        if (source.clip == null || source.clip.name != soundName)
                        {
                            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, soundName, (clip) =>
                            {
                                if (clip == null)
                                {
                                    Core.Network.GetCustomFileAudioClip(soundName, (customClip) =>
                                    {
                                        AssignClip(source, customClip);
                                    });
                                }
                                else
                                {
                                    AssignClip(source, clip);
                                }
                            });
                        }
                    }
                    else
                    {
                        Debug.LogError("Cannot find any audio");
                    }
                }

            }
        }

        private void AssignClip(AudioSource source, AudioClip clip)
        {
            source.Stop();
            source.clip = clip;
            source.time = 0;
        }

    }
}
