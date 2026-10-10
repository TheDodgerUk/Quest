using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using VLB;
using static ContentLightingMetaData;
using static HouseBuilder.HouseBuilderConstants;

namespace HouseBuilder
{
    public class Lighting
    {
        private const float MAX_LIGHT_RANGE = 40f;
        public class TurnObject
        {
            public GameObject m_GameObject;
            public Vector3 m_Target = Vector3.zero;
            public Vector3 m_Current = Vector3.zero;

            public Vector3 m_Main = Vector3.zero;
            public Vector3 m_Fine = Vector3.zero;

            public float m_SpeedModifierFromJson = 1f;


            // when move slider it knows were it is and the new rotation it needs to be in 
            // and it needs to a percentage of that
            // if is zero and then max it (255), the rotation will takes 3 seconds
            // if its zero and then move it (25), the rotation will still takes 3 seconds
            public float m_DistanceMoveModifier = 1f;
        }

        private TurnObject m_PanTurn = new TurnObject();
        private TurnObject m_TiltTurn = new TurnObject();



        public class CombinedData
        {
            public DynamicOcclusionRaycasting m_DynamicOcclusionRaycasting;
            public VolumetricLightBeam m_VolumetricLightBeam;
            public Light m_BasicLight;
            public float m_BasicLightStartIntensity = 1f;
        }

        public class LightBeamData
        {
            public List<CombinedData> m_CombinedData = new List<CombinedData>();
            public GameObject m_RootGameObject;
            public float m_GroupIntensity = 1;
        }

        private Dictionary<GameObject, LightBeamData> m_AllLightBeams = new Dictionary<GameObject, LightBeamData>();

        private LightingStrobe LightingStrobeRef { get; set; }
        private LightingGobo LightingGoboRef { get; set; }
        private LightingFountain LightingFountainRef { get; set; }

        public ContentLightingMetaData ContentLightingMetaDataRef { get; private set; }
        private InteractionGameObject m_Interaction;
      

        private int m_DmxStartNumber = 1;
        public int DmxStartNumber
        {
            get => m_DmxStartNumber;
            set => m_DmxStartNumber = value;
        }
        public int ModeIndex { get; set; }
       

        public List<string> GetModeNames()
        {
            List<string> names = new List<string>();
            foreach (var item in ContentLightingMetaDataRef.m_Fixure.Modes)
            {
                if(string.IsNullOrEmpty(item.ShortName) == false)
                {
                    names.Add(item.ShortName);
                }
                else
                {
                    names.Add(item.Name);
                }
                
            }
            return names;
        }

        public void Setup(InteractionGameObject inter, ContentLightingMetaData metaData)
        {
            m_Interaction = inter;
            ContentLightingMetaDataRef = metaData;

            ContentLightingMetaDataRef.m_Fixure = FixtureContainer.Instance.GetFixture(ContentLightingMetaDataRef.m_JsonFileName);
            ContentLightingMetaDataRef.CollectAllData(inter);
            ClearAllData();

            PhotonDMX.Instance.CollectDmxData(Callback);
            LightingStrobeRef = new LightingStrobe(this, inter);
            LightingGoboRef = new LightingGobo(this, inter);
            LightingFountainRef = new LightingFountain(this, inter);
            List<DmxData> data = new List<DmxData>();
            for(int i = 0; i < OscControllerRoot.MAX_DMX_INDEX; i++)
            {
                DmxData newData = new DmxData(i, 0);
                data.Add(newData);
            }
            Callback(data);
        }


        private void Callback(List<DmxData> dataList)
        {
            ContentLightingMetaData.GameObjectLinks basic = ContentLightingMetaDataRef.m_GameObjectLinks[ModeIndex];
            foreach (var data in dataList)
            {
                if (data.DMX >= DmxStartNumber && data.DMX <= DmxStartNumber + basic.m_GameObjectLink.Count)
                {
                    // for tracking items getting called Debug.LogError($" dmx {data.DMX}   {m_Interaction.RootGameObject.GetGameObjectPath()}   GUID {m_Interaction.m_GeneratedGUID}");
                    UpdateLinkage(data);
                }
            }
        }

        private void ClearAllData()
        {
            // send 0 to self , to clear the current data 
            ContentLightingMetaData.GameObjectLinks linkage = ContentLightingMetaDataRef.m_GameObjectLinks[ModeIndex];
            for (int objectIndex = 0; objectIndex < linkage.m_GameObjectLink.Count; objectIndex++)
            {
                DmxData clearDmx = new DmxData(objectIndex + 1, 0);
                UpdateLinkage(clearDmx);
            }


            // update spot angle , set it to minimum
            for (int localModeIndex = 0; localModeIndex < ContentLightingMetaDataRef.m_GameObjectLinks.Count; localModeIndex++)
            {
                ContentLightingMetaData.GameObjectLinks basic = ContentLightingMetaDataRef.m_GameObjectLinks[localModeIndex];
                if (ContentLightingMetaDataRef.m_Fixure.Physical != null && ContentLightingMetaDataRef.m_Fixure.Physical.Lens != null && ContentLightingMetaDataRef.m_Fixure.Physical.Lens.DegreesMinMax != null)
                {
                    for (int i = 0; i < basic.m_GameObjectLink.Count; i++)
                    {
                        var lights = CollectLightBeamData(basic.m_GameObjectLink[i].m_GameObject);
                        lights.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.spotAngle = (float)ContentLightingMetaDataRef.m_Fixure.Physical.Lens.DegreesMinMax[0]);
                    }
                }
            }
        }


        private void UpdateLinkage(DmxData data)
        {
            GameObjectLinks basic = ContentLightingMetaDataRef.m_GameObjectLinks[ModeIndex];
            for (int i = 0; i < basic.m_GameObjectLink.Count; i++)
            {
                var currentDmxIndex = DmxStartNumber + i;
                if (currentDmxIndex == data.DMX)
                {
                    ChangeTheData(ContentLightingMetaDataRef.m_Fixure, data, basic, i);
                }
            }

            UpdateAllDimmers();
        }

        private void UpdateAllDimmers()
        {
            GameObjectLinks basic = ContentLightingMetaDataRef.m_GameObjectLinks[ModeIndex];
            for (int i = 0; i < basic.m_GameObjectLink.Count; i++)
            {
                var root = CollectLightBeamData(basic.m_GameObjectLink[i].m_GameObject);
                if(root != null)
                {

                }
            }
        }

        private void ChangeTheData(Fixture fixture, DmxData data, ContentLightingMetaData.GameObjectLinks basic, int i)
        {
            GameObjectLink link = basic.m_GameObjectLink[i];
            CapabilityType capType = basic.m_GameObjectLink[i].m_CapabilityTypeEnum;
            switch (capType)
            {
                case CapabilityType.ERROR:
                    Debug.LogError($"ERROR ERROR ERROR    link: {link.m_CapabilityType} index : {i}");
                    break;

                case CapabilityType.Pan:
                    var extraPan = ContentLightingMetaDataRef.m_Fixure.AvailableChannels[link.m_CapabilityType];
                    Pan(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject, extraPan.Capability);
                    break;
                case CapabilityType.PanFine:
                    var extraPanFine = ContentLightingMetaDataRef.m_Fixure.AvailableChannels[link.m_CapabilityType];
                    PanFine(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject, extraPanFine.Capability);
                    break;
                case CapabilityType.Speed:
                    var extraSpeed = ContentLightingMetaDataRef.m_Fixure.AvailableChannels[link.m_CapabilityType];
                    Speed(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject, extraSpeed.Capability);
                    break;
                case CapabilityType.Tilt:
                    var extraTilt = ContentLightingMetaDataRef.m_Fixure.AvailableChannels[link.m_CapabilityType];
                    Tilt(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject, extraTilt.Capability);
                    break;
                case CapabilityType.TiltFine:
                    var extraTiltFine = ContentLightingMetaDataRef.m_Fixure.AvailableChannels[link.m_CapabilityType];
                    TiltFine(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject, extraTiltFine.Capability);
                    break;
                case CapabilityType.Red:
                    Red(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.Green:
                    Green(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.Blue:
                    Blue(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.Amber:
                    Amber(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.White:
                    White(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;

                case CapabilityType.ColorPreset:
                    LightingGoboRef?.GoboColorPreset(basic.m_GameObjectLink[i].m_CapabilityType, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.ColorWheel:
                    LightingGoboRef?.GoboColourWheel(basic.m_GameObjectLink[i].m_CapabilityType, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    Debug.LogError("ColorWheel");
                    break;
                case CapabilityType.Dimmer:
                case CapabilityType.Dimmer2:
                    Dimmer(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.WWDimmer:
                    WWDimmer(fixture, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.CWDimmer:
                    CWDimmer(data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.ShutterStrobe:
                case CapabilityType.Strobe:
                    LightingStrobeRef?.StutterStrobe(basic.m_GameObjectLink[i].m_CapabilityType, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                case CapabilityType.GoboWheel:
                    LightingGoboRef?.GoboWheel(basic.m_GameObjectLink[i].m_CapabilityType, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;

                case CapabilityType.Confetti:
                case CapabilityType.Fountain:
                case CapabilityType.Fog:
                    LightingFountainRef?.Fountain(basic.m_GameObjectLink[i].m_CapabilityType, data.DmxValue, basic.m_GameObjectLink[i].m_GameObject);
                    break;
                default:
                    //ConsoleExtra.Log($"Missing case for: {capType}", null, ConsoleExtraEnum.EDebugType.OSC_Sent);
                    break;

            }
        }

        public static  void GetAngles(PurpleCapability cap, out float start, out float end)
        {
            start = 0;
            var startStringNumber = Regex.Match(cap.AngleStart, @"\d+");
            if (int.TryParse(startStringNumber.Value, out int valStart))
            {
                start = valStart;
            }
            end = 0;
            var endStringNumber = Regex.Match(cap.AngleEnd, @"\d+");
            if (int.TryParse(endStringNumber.Value, out int valEnd))
            {
                end = valEnd;
            }
        }

        public static void GetSpeed(PurpleCapability cap, out float start, out float end)
        {
            start = 0;
            var startStringNumber = Regex.Match(cap.SpeedStart, @"\d+");
            if (int.TryParse(startStringNumber.Value, out int valStart))
            {
                start = valStart;
            }
            end = 0;
            var endStringNumber = Regex.Match(cap.SpeedEnd, @"\d+");
            if (int.TryParse(endStringNumber.Value, out int valEnd))
            {
                end = valEnd;
            }
        }





        public void ManualUpdate()
        {
            if (m_PanTurn.m_GameObject != null)
            {
                m_PanTurn.m_Current = Vector3.MoveTowards(m_PanTurn.m_Current, m_PanTurn.m_Target, Time.deltaTime * ContentLightingMetaDataRef.m_SpeedPan * m_PanTurn.m_SpeedModifierFromJson * m_PanTurn.m_DistanceMoveModifier);
                m_PanTurn.m_GameObject.transform.localEulerAngles = m_PanTurn.m_Current;
            }

            if (m_TiltTurn.m_GameObject != null)
            {
                m_TiltTurn.m_Current = Vector3.MoveTowards(m_TiltTurn.m_Current, m_TiltTurn.m_Target, Time.deltaTime * ContentLightingMetaDataRef.m_SpeedTilt * m_TiltTurn.m_SpeedModifierFromJson * m_TiltTurn.m_DistanceMoveModifier);
                m_TiltTurn.m_GameObject.transform.localEulerAngles = m_TiltTurn.m_Current;
            }
        }



        private void Pan(int dmxValue, GameObject obj, PurpleCapability cap)
        {
            m_PanTurn.m_GameObject = obj;
            GetAngles(cap, out float valStart, out float valEnd);
            if (ContentLightingMetaDataRef.m_InvertPan == true)
            {
                valEnd = -valEnd;
                valStart = -valStart;
            }

            float dmxPercentage = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            float value = Mathf.Lerp(valStart, valEnd, dmxPercentage);
            m_PanTurn.m_Main = new Vector3(0, value, 0);
            m_PanTurn.m_Target = m_PanTurn.m_Main + m_PanTurn.m_Fine;


            float range = (valEnd - valStart);
            float dif = Mathf.Abs(value - m_PanTurn.m_Current.y);
            m_PanTurn.m_DistanceMoveModifier = dif / range;
            m_PanTurn.m_DistanceMoveModifier = Mathf.Abs(m_PanTurn.m_DistanceMoveModifier);
        }

        private void PanFine(int dmxValue, GameObject obj, PurpleCapability cap)
        {
            m_PanTurn.m_GameObject = obj;
            GetAngles(cap, out float valStart, out float valEnd);
            if (ContentLightingMetaDataRef.m_InvertPan == true)
            {
                valEnd = -valEnd;
                valStart = -valStart;
            }

            float dmxPercentage = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            float value = Mathf.Lerp(valStart, valEnd, dmxPercentage);
            m_PanTurn.m_Fine = new Vector3(0, value, 0);
            m_PanTurn.m_Target = m_PanTurn.m_Main + m_PanTurn.m_Fine;
        }

        private void Speed(int dmxValue, GameObject obj, PurpleCapability cap)
        {
            GetSpeed(cap, out float valStart, out float valEnd);
            float dmxPercentage = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            float value = Mathf.Lerp(valStart, valEnd, dmxPercentage);
            m_PanTurn.m_SpeedModifierFromJson = value;
            m_TiltTurn.m_SpeedModifierFromJson = value;
        }


        private void Tilt(int dmxValue, GameObject obj, PurpleCapability cap)
        {
            m_TiltTurn.m_GameObject = obj;
            GetAngles(cap, out float valStart, out float valEnd);
            if (ContentLightingMetaDataRef.m_InvertTilt == true)
            {
                valEnd = -valEnd;
                valStart = -valStart;
            }

            float dmxPercentage = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            float value = Mathf.Lerp(valStart, valEnd, dmxPercentage);
            m_TiltTurn.m_Main = new Vector3(value, 0, 0);
            m_TiltTurn.m_Target = m_TiltTurn.m_Main + m_TiltTurn.m_Fine;


            float range = (valEnd - valStart);
            float dif = Mathf.Abs(value - m_TiltTurn.m_Current.x);
            m_TiltTurn.m_DistanceMoveModifier = dif / range;
            m_TiltTurn.m_DistanceMoveModifier = Mathf.Abs(m_TiltTurn.m_DistanceMoveModifier);
        }

        private void TiltFine(int dmxValue, GameObject obj, PurpleCapability cap)
        {
            m_TiltTurn.m_GameObject = obj;
            GetAngles(cap, out float valStart, out float valEnd);
            if (ContentLightingMetaDataRef.m_InvertTilt == true)
            {
                valEnd = -valEnd;
                valStart = -valStart;
            }

            float dmxPercentage = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            float value = Mathf.Lerp(valStart, valEnd, dmxPercentage);
            m_TiltTurn.m_Fine = new Vector3(value, 0, 0);
            m_TiltTurn.m_Target = m_TiltTurn.m_Main + m_TiltTurn.m_Fine;
        }

        private void Red(byte dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(dmxValue, 0, 0, OscControllerRoot.MAX_DMX_VALUE));
        }

        private void Green(byte dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(0, dmxValue, 0, OscControllerRoot.MAX_DMX_VALUE));
        }

        private void Blue(byte dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(0, 0, dmxValue, OscControllerRoot.MAX_DMX_VALUE));
        }

        private void Amber(byte dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            byte amber = (byte)(194f / (float)OscControllerRoot.MAX_DMX_VALUE);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(OscControllerRoot.MAX_DMX_VALUE, amber, 0, OscControllerRoot.MAX_DMX_VALUE));
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color.a *= ((float)dmxValue / OscControllerRoot.MAX_DMX_VALUE));
        }

        private void White(byte dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(dmxValue, dmxValue, dmxValue, OscControllerRoot.MAX_DMX_VALUE));
        }

        private void Dimmer(int dmxValue, GameObject obj)
        {
            var lightBeam = CollectLightBeamData(obj);
            var value = (((float)dmxValue) / ((float)OscControllerRoot.MAX_DMX_VALUE));
            lightBeam.m_GroupIntensity = value;

            foreach(var item in lightBeam.m_CombinedData)
            {
                if (item.m_BasicLight != null)
                {
                    item.m_BasicLight.intensity = item.m_BasicLightStartIntensity * lightBeam.m_GroupIntensity;
                }
            }
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.intensityGlobal = lightBeam.m_GroupIntensity);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.UpdateAfterManualPropertyChange());
        }

        private void CWDimmer(byte dmxValue, GameObject obj) // cold color, bright white 
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(dmxValue, dmxValue, dmxValue, OscControllerRoot.MAX_DMX_VALUE));
        }

        private void GoboWheel(byte dmxValue, GameObject obj) // cold color, bright white 
        {
            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = new Color32(dmxValue, dmxValue, dmxValue, OscControllerRoot.MAX_DMX_VALUE));
        }


        private void WWDimmer(Fixture fixture, byte dmxValue, GameObject obj)
        {
            float max = 6400;                                                                       // magic numbers , need replacing later , when i find out more 
            if (fixture.Physical != null && fixture.Physical.Bulb != null && fixture.Physical.Bulb.ColorTemperature != null)
            {
                max = (float)fixture.Physical.Bulb.ColorTemperature;
            }

            float percentage = dmxValue / (float)OscControllerRoot.MAX_DMX_VALUE;
            float temperature = Mathf.Lerp(2400, max, percentage);                                  // magic numbers , need replacing later , when i find out more 
            var temperatureColor = Mathf.CorrelatedColorTemperatureToRGB(temperature);

            var lightBeam = CollectLightBeamData(obj);
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color = (temperatureColor *= percentage));
            lightBeam.m_CombinedData.ForEach(e => e.m_VolumetricLightBeam.color.a = 1f);
        }

        public LightBeamData CollectLightBeamData(GameObject obj)
        {
            if (obj != null)
            {
                if (m_AllLightBeams.ContainsKey(obj) == false)
                {
                    var volumetricLightBeams = obj.GetComponentsInChildren<VolumetricLightBeam>(true).ToList();
                    volumetricLightBeams.ForEach(e => e.trackChangesDuringPlaytime = true);//lights.ForEach(e => e.UpdateAfterManualPropertyChange()); could be used instead on each funtion  // depending on the fps hit             
                    volumetricLightBeams.ForEach(e => e.fallOffEnd = MAX_LIGHT_RANGE);
                    volumetricLightBeams.ForEach(e => e.noiseMode = NoiseMode.WorldSpace);
                    LightBeamData allLightBeamData = new LightBeamData();
                    allLightBeamData.m_GroupIntensity = 1f;
                    allLightBeamData.m_RootGameObject = obj;

                    allLightBeamData.m_CombinedData = new List<CombinedData>();

                    CombinedData combined = null;
                    foreach (var light in volumetricLightBeams)
                    {
                        combined = new CombinedData();
                        combined.m_VolumetricLightBeam = light;
                        combined.m_BasicLight = light.gameObject.GetComponent<Light>();
                        if(combined.m_BasicLight != null)
                        {
                            combined.m_BasicLightStartIntensity = combined.m_BasicLight.intensity;
                        }
                        combined.m_DynamicOcclusionRaycasting = light.gameObject.ForceComponent<DynamicOcclusionRaycasting>();
                        combined.m_DynamicOcclusionRaycasting.layerMask = -1;
                        combined.m_DynamicOcclusionRaycasting.considerTriggers = true;
                        combined.m_DynamicOcclusionRaycasting.updateRate = DynamicOcclusionUpdateRate.OnBeamMove;
                        if(combined.m_BasicLight != null)
                        {
                            combined.m_DynamicOcclusionRaycasting.enabled = false;
                            combined.m_VolumetricLightBeam.enabled = false;
                        }
                        allLightBeamData.m_CombinedData.Add(combined);
                    }


                    m_AllLightBeams.Add(obj, allLightBeamData);
                }
                return m_AllLightBeams[obj];
            }
            else
            {
                return new LightBeamData();
            }
        }

        public void OnDestroy()
        {
            PhotonDMX.Instance.RemoveCollectDmxData(Callback);
        }
    }
}

