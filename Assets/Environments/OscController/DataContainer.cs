using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer.OscAllUsedFixtures;
using static OscController.OscControllerRoot;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{
    public class DataContainer : Singleton<DataContainer>
    {

        public static readonly List<Color> Colours = new List<Color>()
        {
            { Color.red * 0.9f},
            { Color.green * 0.9f },
            { Color.blue * 0.9f },
            { new Color(255f/255f, 165/255f, 0f, 1f) * 0.9f}, // orange
            { new Color(128f/255f, 0/255f, 128f, 1f) * 0.9f}, // purple
            { Color.cyan * 0.9f},
            { Color.magenta * 0.9f},
            { new Color(255f/255f, 215f/255f, 0f, 1f) * 0.9f}, // gold
        };

        public enum SoundSource
        {
            RoomBuilder,
            TimeLine
        }

        [SerializeField]
        public class FilterGroup
        {
            [SerializeField]
            public string m_GroupName = "";
            [System.NonSerialized]
            public bool m_IsOpen = true;
        }

        [SerializeField]
        public class OscGenericData
        {
            [SerializeField]
            public string m_AudioName;
            [SerializeField]
            public SoundSource m_SoundSource = SoundSource.RoomBuilder;
            [SerializeField]
            public bool m_IsLocalFile = false;
            [SerializeField]
            public string m_LocalFullAudioName;

            [System.NonSerialized]
            public AudioClip m_AudioClip;

            [SerializeField]
            public List<FilterGroup> m_FilterGroupNames = new List<FilterGroup>();

            public void ValidateStart()
            {
                if((m_FilterGroupNames.Count == 0) || (m_FilterGroupNames.Find(e => e.m_GroupName == OscControllerRoot.OPTIONS_ALL)) == null)
                {
                    FilterGroup newGroup = new FilterGroup();
                    newGroup.m_GroupName = OscControllerRoot.OPTIONS_ALL;
                    m_FilterGroupNames.Insert(0, newGroup);
                }
            }
        }

        [SerializeField]
        public OscGenericData GenericDataDataRef = new OscGenericData();


        public SliderPrefab CurrentHightlightedSliderPrefab { get; set; }
        public SliderPrefab CurrentSelectedSliderPrefab { get; set; }



        public void SortTimeLineData()
        {
            List<OscGroupData> newList = new List<OscGroupData>();
            foreach (var item in TimeLineDataRef)
            {
                if(item.m_IsRecorded == false)
                {
                    newList.Add(item);
                }
            }


            bool splitAdded = false;
            foreach (var item in TimeLineDataRef)
            {
                if (item.m_IsRecorded == true && item.m_IsBlank == true)
                {
                    splitAdded = true;
                    newList.Add(item);
                }
            }

            if(splitAdded == false)
            {
                OscGroupData newData = new OscGroupData();
                newData.m_IsRecorded = true;
                newData.m_IsBlank = true;
                newData.m_InputFieldNameText = "Recorded";
                newList.Add(newData);
            }

            foreach (var item in TimeLineDataRef)
            {
                if (item.m_IsRecorded == true && item.m_IsBlank == false)
                {
                    newList.Add(item);
                }
            }

            TimeLineDataRef = newList;
        }


        [SerializeField]
        public List<OscGroupData> TimeLineDataRef = new List<OscGroupData>();
        public class OscGroupData
        {
            [SerializeField]
            public string m_GroupName;
            [System.NonSerialized]
            public Button m_BtnOptions;
            [System.NonSerialized]
            public InputField m_InputFieldName;
            [System.NonSerialized]
            public GameObject m_MainGameObject;
            [System.NonSerialized]
            public Image m_MainGameObjectImage;
            [System.NonSerialized]
            public Image m_BlankImage;
            [System.NonSerialized]
            public Button m_AddButton;
            [System.NonSerialized]
            public Button m_UpButton;
            [System.NonSerialized]
            public Button m_DownButton;
            [System.NonSerialized]
            public Toggle m_SelectedToggle;

            [System.NonSerialized]
            public Text m_TxtOptions;
            [SerializeField]
            public string m_OptionsName = OscControllerRoot.OPTIONS_ALL;
            [SerializeField]
            public int m_OptionsDmxStart = 1;
            [SerializeField]
            public bool m_IsBlank = false;
            [SerializeField]
            public bool m_IsRecorded = false;
            [SerializeField]
            public bool m_IsSelected = true;
            [SerializeField]
            public string m_InputFieldNameText = "";


            [SerializeField]
            public List<GroupLinkageData> m_GroupLinkageData = new List<GroupLinkageData>();

            public void Sort()
            {
                m_GroupLinkageData.Sort(delegate (GroupLinkageData t1, GroupLinkageData t2)
                {
                    return (t1.m_StartTime.CompareTo(t2.m_StartTime));
                });
            }

            public void AssignTextOptions()
            {
                m_TxtOptions.text = m_OptionsName;
                if(m_OptionsName != OscControllerRoot.OPTIONS_ALL)
                {
                    var splitItems = m_OptionsName.Split('/');
                    m_TxtOptions.text = $"{splitItems[1]}  {m_OptionsDmxStart}";

                    var foundindex = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.FindIndex(e => e.m_DmxStartIndex == m_OptionsDmxStart);
                    if(foundindex != -1)
                    {
                        int colourIndex = foundindex % DataContainer.Colours.Count;
                        m_TxtOptions.color = DataContainer.Colours[colourIndex];
                    }
                    else
                    {
                        m_TxtOptions.color = Color.black;
                    }
                }
                else
                {
                    m_TxtOptions.color = Color.black;
                }
            }

            public float FindLastEndTime()
            {
                Sort();
                float time = 0;
                if (m_GroupLinkageData.Count > 1) // needs to be one, as when add first one, it is in 
                {
                    var item = m_GroupLinkageData.Last();
                    time = item.m_StartTime + item.m_Sequence.m_DurationTime;
                }
                return time;
            }

            [SerializeField]
            public class GroupLinkageData
            {
                [System.NonSerialized]
                public GameObject m_GameObjectItem;
                [System.NonSerialized]
                public RectTransform m_RectTransform;
                [System.NonSerialized]
                public Image m_Image;
                [System.NonSerialized]
                public ImageMouseMoveLimited m_Item;
                [System.NonSerialized]
                public OscSequenceData m_Sequence;
                [System.NonSerialized]
                public Text m_SequenceDisplayName;
                [System.NonSerialized]
                public OscGroupData m_OscGroupData;
                [SerializeField]
                public int m_SequenceIndex;
                [SerializeField]
                public float m_StartTime = 0;
            }
        }

       
        public OscAllUsedFixtures AllUsedFixturesRef = new OscAllUsedFixtures();
        public class OscAllUsedFixtures
        {
            public List<UsedFixtureData> m_UsedFixtureDataList = new List<UsedFixtureData>();
            public class UsedFixtureData
            {
                [System.NonSerialized]
                public Fixtures.Fixture m_Fixture;

                [SerializeField]
                public string m_JsonFileName;

                [SerializeField]
                public int m_ChannelCount = 1;

                [SerializeField]
                public int m_ModeIndex = 0;

                [SerializeField]
                public int m_DmxStartIndex = 1;

                [SerializeField]
                public Color? m_Color;
            }
        }

        [SerializeField]
        public class FixtureSliderData
        {
            [System.NonSerialized]
            public UsedFixtureData m_FixtureData;

            [SerializeField]
            public int m_FixtureDataIndex;
            [SerializeField]
            public int m_ChannelIndex = 0;
            [SerializeField]
            public byte m_DmxValue = 0;

            [JsonConverter(typeof(AnimationCurveConverter))]
            public AnimationCurve m_AnimationCurveRaw = new AnimationCurve();

            [JsonConverter(typeof(AnimationCurveConverter))]
            public AnimationCurve m_AnimationCurveRepeated = new AnimationCurve(); // needs to be for save data, and when change in curve editor, this gets updated 
            [SerializeField]
            public int m_AnimationCurveRepeatNumber = 1;
            [SerializeField]
            public bool m_RepeatInvert = false;
            [SerializeField]
            public BezierType m_BezierType = BezierType.AnimationCurve;
            [SerializeField]
            public bool m_UseAnimationCurve = false;
            public FixtureSliderData()
            {
                m_AnimationCurveRaw.AddKey(0, 0);
                m_AnimationCurveRaw.AddKey(3, 0);

                m_AnimationCurveRepeated.AddKey(0, 0);
                m_AnimationCurveRepeated.AddKey(3, 0);
                
            }

            public class AnimationCurveConverter : JsonConverter<AnimationCurve>
            {
                public class AnimationData
                {
                    public float m_Time;
                    public float m_Value;
                }
                public override void WriteJson(JsonWriter writer, AnimationCurve value, JsonSerializer serializer)
                {
                    List<AnimationData> data = new List<AnimationData>();
                    foreach(var item in value.keys)
                    {
                        AnimationData newWriteData = new AnimationData();
                        newWriteData.m_Time = item.time;
                        newWriteData.m_Value = item.value;
                        data.Add(newWriteData);
                    }
                    var newData = Json.FullSerialiser.WriteToText<List<AnimationData>>(data, true);
                    writer.WriteValue(newData);
                }

                public override AnimationCurve ReadJson(JsonReader reader, Type objectType, AnimationCurve existingValue, bool hasExistingValue, JsonSerializer serializer)
                {
                    AnimationCurve curve = new AnimationCurve();
                    var newData = Json.FullSerialiser.ReadFromText<List<AnimationData>>(reader.Value.ToString());
                    foreach(var item in newData)
                    {
                        Keyframe newKey = new Keyframe();
                        newKey.time = item.m_Time;
                        newKey.value = item.m_Value;
                        curve.AddKey(newKey);
                    }
                    
                    return curve;
                }
            }
        }



        [SerializeField]
        public class OscSequenceData
        {
            [System.NonSerialized]
            public Toggle m_Toggle;
            [System.NonSerialized]
            public Image m_BaseImage;
            [System.NonSerialized]
            public GameObject m_Prefab;
            [System.NonSerialized]
            public InputField m_DurationInput;
            [System.NonSerialized]
            public InputField m_LerpInput;
            [System.NonSerialized]
            public Button m_BtnPlay;
            [System.NonSerialized]
            public Button m_BtnReset;

            [System.NonSerialized]
            public Image m_ImgDrag;

            [System.NonSerialized]
            public Text m_TxtOptions;
            [System.NonSerialized]
            public Text m_TxtOptionsAllOrSingle;


            [System.NonSerialized]
            public Button m_BtnOptions;

            [System.NonSerialized]
            public InputField m_InputFieldSequenceName;


            [SerializeField]
            public string m_OptionsName = OscControllerRoot.OPTIONS_ALL;

            [SerializeField]
            public string m_OptionsType = OscControllerRoot.OPTIONS_ALL;

            [SerializeField]
            public string m_FilterGroup = OscControllerRoot.OPTIONS_ALL;


            [SerializeField]
            public string m_SequenceName = "New";
            [SerializeField]
            public float m_DurationTime = 3f;
            [SerializeField]
            public float m_LerpTime = 0f;
            [SerializeField]
            public List<FixtureSliderData> m_SliderData = new List<FixtureSliderData>();
        }

        [SerializeField]
        public List<OscSequenceData> SequenceDataRef = new List<OscSequenceData>();


        public Action<OscSequenceData> OnSequenceDataChange;

        public OscSequenceData m_CurrentSequenceData;
        public OscSequenceData CurrentSequenceData
        {
            get => m_CurrentSequenceData;

            set
            {
                if (m_CurrentSequenceData != value)
                {
                    m_CurrentSequenceData = value;
                    OnSequenceDataChange?.Invoke(m_CurrentSequenceData);
                }
            }
        }

        public void ForceReselectCurrentSceneData() => OnSequenceDataChange?.Invoke(m_CurrentSequenceData);


        public Action<OscGroupData.GroupLinkageData> OnGroupLinkageDataChange;

        public OscGroupData.GroupLinkageData m_CurrentGroupLinkageData;
        public OscGroupData.GroupLinkageData CurrentGroupLinkageData
        {
            get => m_CurrentGroupLinkageData;

            set
            {
                if (m_CurrentGroupLinkageData != value)
                {
                    m_CurrentGroupLinkageData = value;
                    OnGroupLinkageDataChange?.Invoke(m_CurrentGroupLinkageData);
                }
            }
        }
        

        /// <summary>
        /// ///////////////////////////
        /// </summary>



        public Action<AnimationCurve> OnAnimationCurveChange;

        public AnimationCurve m_CurrentAnimationCurve;
        public AnimationCurve CurrentAnimationCurve
        {
            get => m_CurrentAnimationCurve;

            set
            {
                if (m_CurrentAnimationCurve != value)
                {
                    m_CurrentAnimationCurve = value;
                    OnAnimationCurveChange?.Invoke(m_CurrentAnimationCurve);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public Action<OscAllUsedFixtures.UsedFixtureData> OnUsedFixtureDataChange;

        private OscAllUsedFixtures.UsedFixtureData m_CurrentUsedFixtureData;
        public OscAllUsedFixtures.UsedFixtureData CurrentUsedFixtureData
        {
            get => m_CurrentUsedFixtureData;

            set
            {
                if (m_CurrentUsedFixtureData != value)
                {
                    m_CurrentUsedFixtureData = value;
                    OnUsedFixtureDataChange?.Invoke(m_CurrentUsedFixtureData);
                }
            }
        }


        public bool CanItemBePlacedHere(int dmx, int channelCount) => CanItemBePlacedHere(dmx, channelCount, null);

        public bool CanItemBePlacedHere(int dmx, int channelCount, OscAllUsedFixtures.UsedFixtureData self)
        {
            foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
            {
                if (self != null && item == self)
                {
                    continue;
                }
                for (int i = 0; i < channelCount; i++)
                {
                    int dmxIndexToTest = dmx + i;
                    if (dmxIndexToTest >= item.m_DmxStartIndex && dmxIndexToTest < (item.m_DmxStartIndex + item.m_ChannelCount))
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        public bool CanItemBePlacedHere(OscAllUsedFixtures.UsedFixtureData testData) => CanItemBePlacedHere(testData.m_DmxStartIndex, testData.m_ChannelCount);

        public int FindNextFreeSpace(int startDmx, int ChannelCount) => FindNextFreeSpace(startDmx, ChannelCount, null);

        public int FindNextFreeSpace(int startDmx, int ChannelCount, OscAllUsedFixtures.UsedFixtureData self)
        {
            for (int i = startDmx; i < OscControllerRoot.MAX_DMX_INDEX; i++)
            {
                bool valid = CanItemBePlacedHere(i, ChannelCount, self);
                if (valid == true)
                {
                    return i;
                }
            }
            return 0;
        }
    }



}
