using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer;
using static SliderPrefab;

namespace OscController
{
    public class SliderPanel : MonoBehaviour
    {
        public class ListSliderPrefab : List<SliderPrefab>
        {
            public new void Add(SliderPrefab obj)
            {
                obj.m_SliderHandleGroupSelected.color = OscControllerRoot.SELECTED;
                base.Add(obj);
            }
            public new void Sort()
            {
                Sort(delegate (SliderPrefab x, SliderPrefab y)
                {
                    return x.m_DmxIndex.CompareTo(y.m_DmxIndex);
                });
            }

            public new void Remove(SliderPrefab obj)
            {
                obj.m_SliderHandleGroupSelected.color = OscControllerRoot.UN_SELECTED;
                base.Remove(obj);
            }

            public new void Clear()
            {
                ForEach(e => e.m_SliderHandleGroupSelected.color = OscControllerRoot.UN_SELECTED);
                base.Clear();
            }
        }

        private Sprite m_BasicSprite = null;
        private const float F_DISTANCE_MULTIPLYER = 11;

        public static Action<AnimationCurve, float> OnSliderChanged;

        private HorizontalLayoutGroup m_HorizontalLayoutGroup;
        private PoolManagerLocalComponent<SliderPrefab> m_SliderPool;

        private List<SliderPrefab> m_FullSliderPrefabList = new List<SliderPrefab>();

        private ListSliderPrefab m_MultipleSliders = new ListSliderPrefab();
        public ListSliderPrefab MultipleSliders => m_MultipleSliders;

        private RectTransform m_RectTransform;

        public List<SliderPrefab> AllSliderPrefabs() => m_SliderPool.GetPublicList();

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            m_HorizontalLayoutGroup = this.GetComponentInChildren<HorizontalLayoutGroup>();
            GameObject sliderPrefab = CachedObjects.Instance.InstantiateCachedGameObject("SliderPrefab");
            SliderPrefab data = sliderPrefab.ForceComponent<SliderPrefab>();
            data.AssignComponentsData();
            data.m_Slider.minValue = 0;
            data.m_Slider.maxValue = OscControllerRoot.MAX_DMX_VALUE;
            data.m_Slider.wholeNumbers = true;

            m_SliderPool = new PoolManagerLocalComponent<SliderPrefab>(sliderPrefab, m_HorizontalLayoutGroup.transform);
            sliderPrefab.SetActive(false);

            m_RectTransform = this.transform.SearchComponent<RectTransform>("Scroll View");

            FixtureChanged(null);
            DataContainer.Instance.OnUsedFixtureDataChange += FixtureChanged;

            DataContainer.Instance.OnSequenceDataChange += SceneDataChange;
        }


        private void SceneDataChange(DataContainer.OscSequenceData data)
        {
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            SetCorrectPanelSize();
            
            if (m_SliderPool != null)
            {
                m_SliderPool.DeSpawnAll();
                m_FullSliderPrefabList.Clear();

                if (data.m_OptionsName == OscControllerRoot.OPTIONS_ALL)
                { 
                    DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Sort(delegate (DataContainer.OscAllUsedFixtures.UsedFixtureData x, DataContainer.OscAllUsedFixtures.UsedFixtureData y)
                    {
                        return x.m_DmxStartIndex.CompareTo(y.m_DmxStartIndex);
                    });

                    for (int i = 0; i < DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Count; i++)
                    {
                        Show(DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[i], i);
                    }
                }
                else
                {
                    if (data.m_OptionsType == OscControllerRoot.OPTIONS_SINGLE)
                    {
                        var item = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Find(e => e.m_JsonFileName == data.m_OptionsName);
                        Show(item, 0);
                    }
                    else
                    {
                        for (int i = 0; i < DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Count; i++)
                        {
                            if (DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[i].m_JsonFileName == data.m_OptionsName)
                            {
                                Show(DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[i], i);
                            }
                        }


   
                        
                    }
                }
            }
        }

        private void FixtureChanged(DataContainer.OscAllUsedFixtures.UsedFixtureData fixture)
        {
            if (m_SliderPool != null)
            {
                m_SliderPool.DeSpawnAll();
                m_FullSliderPrefabList.Clear();

                DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Sort(delegate (DataContainer.OscAllUsedFixtures.UsedFixtureData x, DataContainer.OscAllUsedFixtures.UsedFixtureData y)
                {
                    return x.m_DmxStartIndex.CompareTo(y.m_DmxStartIndex);
                });

                for (int i = 0; i < DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Count; i++)
                {
                    Show(DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[i], i);
                }
            }
        }


        public void SetCorrectPanelSize()
        {
            bool full = true;
            if (HouseBuilder.HouseBuilderRoot.Instance != null)
            {
                if (OscControllerRoot.Instance != null)
                {
                    full = !OscControllerRoot.Instance.CurveEditorPanelRef.IsActive;
                }
            }


            // anthony magic numbers, ,these will need to be changed later 
            if (full == true)
            {
                m_RectTransform.transform.localPosition = Vector3.zero;
                m_RectTransform.sizeDelta = new Vector2(1800f, 470f);
            }
            else
            {
                m_RectTransform.transform.localPosition = new Vector3(-450f, 0f, 0f);
                m_RectTransform.sizeDelta = new Vector2(900f, 470f);
            }
        }


        private void Show(DataContainer.OscAllUsedFixtures.UsedFixtureData fixture, int fixtureIndex)
        {
            var currentSceneData = DataContainer.Instance.CurrentSequenceData;
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            m_MultipleSliders.Clear();
            if (fixture != null)
            {
                for (int i = 0; i < fixture.m_ChannelCount; i++)
                {
                    int channelIndex = i;
                    int dmxIndex = fixture.m_DmxStartIndex + i;

                    SliderPrefab slider = m_SliderPool.SpawnObject(m_HorizontalLayoutGroup.transform);
                    slider.m_UseCurveString = "s";
                    m_FullSliderPrefabList.Add(slider);
                    slider.AssignComponentsData();

                    #region create new slider
                    slider.InitialiseData(fixture, channelIndex, dmxIndex, fixtureIndex, true);
                    #endregion create new slider


                    #region setup startData
                    // this sets the sliders up 
                    
                    SetToLastValidData(currentSceneData, fixture, channelIndex, slider);
                    SetSpecialButton(slider.m_Slider.value, slider, slider.m_BtnSpecial);
                    slider.SetUpBasicCallbacks(m_MultipleSliders, m_FullSliderPrefabList);
                    #endregion setup startData

                    slider.m_FixtureSliderData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider, false);
                    UpdateIfCurve(slider, slider.m_FixtureSliderData);


                    // reset the to correct slider level
                    slider.m_Slider.onValueChanged.RemoveAllListeners();
                    slider.m_Slider.onValueChanged.AddListener((amount) =>
                    {
                        currentSceneData = SliderValueChanged(fixture, (byte)amount, channelIndex, dmxIndex, slider);
                    });

                    slider.m_BtnActive.onClick.RemoveAllListeners();
                    slider.m_BtnActive.onClick.AddListener(() =>
                    {
                        currentSceneData = DataContainer.Instance.CurrentSequenceData;// thbis is needed otherwise it loses it , very weird 
                        FixtureSliderData validData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider, false);
                        if (validData != null)
                        {
                            currentSceneData.m_SliderData.Remove(validData);
                            slider.SetValidActive(false);
                            SetToLastValidData(currentSceneData, fixture, channelIndex, slider);
                        }
                    });



                    slider.m_BtnValue.onClick.RemoveAllListeners();
                    slider.m_BtnValue.onClick.AddListener(() =>
                    {
                        currentSceneData = DataContainer.Instance.CurrentSequenceData;
                        var btnDataNewData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider, false);
                        if (btnDataNewData != null)
                        {
                            btnDataNewData.m_UseAnimationCurve = !btnDataNewData.m_UseAnimationCurve;
                            UpdateIfCurve(slider, btnDataNewData);
                        }
                    });


                    RightClick click = slider.m_BtnSpecial.ButtonRef.gameObject.ForceComponent<RightClick>();
                    click.onRightClick.RemoveAllListeners();

                    click.onRightClick.AddListener(() =>
                    {
                        if (slider.m_WheelData != null && slider.m_WheelData.Count != 0)
                        {
                            OscControllerRoot.Instance.WheelPanelRef.SetWheelData(slider, OscControllerRoot.Instance.CurveEditorPanelRef.SpecailButton.ButtonRef.transform);
                            OscControllerRoot.Instance.WheelPanelRef.SetActive(!OscControllerRoot.Instance.WheelPanelRef.IsActive);
                        }
                        SetCorrectPanelSize();
                    });


                    slider.m_BtnSpecial.TextRef.text = "";
                    slider.m_BtnSpecial.ButtonRef.onClick.RemoveAllListeners();
                    slider.m_BtnSpecial.ButtonRef.onClick.AddListener(() =>
                    {                       
                        if (OscControllerRoot.Instance.CurveEditorPanelRef.IsActive == true)
                        {
                            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
                            SetCorrectPanelSize();
                        }
                        else
                        {
                            Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_SELECT_DMX_MESSAGE, slider.m_FixtureData.m_DmxStartIndex.ToString());
                            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(true);
                            SetCorrectPanelSize();


                            // if you press the curve button, it makes it curve
                            currentSceneData = DataContainer.Instance.CurrentSequenceData;
                            var btnDataNewData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider, false);
                            if (btnDataNewData != null)
                            {
                                btnDataNewData.m_UseAnimationCurve = true;
                                UpdateIfCurve(slider, btnDataNewData);
                            }
                        }
                        if (OscControllerRoot.Instance.CurveEditorPanelRef.IsActive == true)
                        {
                            var fixtureData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider);
                            if (fixtureData != null)
                            {
                                OscControllerRoot.Instance.CurveEditorPanelRef.SetData(fixtureData, fixture.m_DmxStartIndex + fixtureData.m_ChannelIndex, slider);
                                SetCorrectPanelSize();
                            }
                            else
                            {
                                Debug.LogError($"fixtureData == null");
                            }
                        }
                    });

                }
            }
        }

        public OscSequenceData SliderValueChanged(OscAllUsedFixtures.UsedFixtureData fixture, byte amount, int channelIndex, int dmxIndex, SliderPrefab slider)
        {
            // remove all items if moving not one of them 
            SliderPrefab foundSelf = m_MultipleSliders.Find(e => e.m_Slider == slider.m_Slider);
            if (foundSelf == null)
            {
                m_MultipleSliders.Clear();
            }

            OscSequenceData currentSceneData = DataContainer.Instance.CurrentSequenceData;// thbis is needed otherwise it loses it , very weird 
            if (currentSceneData != null)
            {
                if (m_MultipleSliders.Count == 0)
                {
                    CreateAndSendData(currentSceneData, fixture, channelIndex, slider, amount);
                }
                else
                {
                    for (int j = 0; j < m_MultipleSliders.Count; j++)
                    {
                        CreateAndSendData(currentSceneData, m_MultipleSliders[j].m_FixtureData, m_MultipleSliders[j].m_ChannelIndex, m_MultipleSliders[j], amount);
                    }
                }
            }
            else
            {
                if (m_MultipleSliders.Count == 0)
                {
                    DisplayVisualsAndSendSlideData(amount, dmxIndex, slider);
                }
                else
                {
                    for (int j = 0; j < m_MultipleSliders.Count; j++)
                    {
                        DisplayVisualsAndSendSlideData(amount, m_MultipleSliders[j].m_DmxIndex, m_MultipleSliders[j]);
                    }
                }
            }

            FixtureSliderData fixtureData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider);
            if (fixtureData != null)
            {
                Keyframe newKey = fixtureData.m_AnimationCurveRaw.keys[OscControllerRoot.Instance.CurveEditorPanelRef.CurrentIndex];
                newKey.value = amount;
                fixtureData.m_AnimationCurveRaw.MoveKey(OscControllerRoot.Instance.CurveEditorPanelRef.CurrentIndex, newKey);


                ////////////////// this somehow  sets the SliderValue to a ranodm number and locks it there via 
                ////////////////// slider.m_Slider.onValueChanged.AddListener((amount) =>
                ////////////////// {
                //////////////////    currentSceneData = SliderValueChanged(fixture, (byte)amount, channelIndex, dmxIndex, slider);
                ////////////////// });
                //////////////////Todo This causes  the slider to lock up 
                ////// curveEditorPanel 668
                OscControllerRoot.Instance.CurveEditorPanelRef.RefreshCurve();

            }

            return currentSceneData;
        }

        private void UpdateIfCurve(SliderPrefab slider, FixtureSliderData btnData)
        {
            if (btnData != null)
            {
                slider.m_UseCurveString = btnData.m_UseAnimationCurve ? "c" : "s";
                slider.UpdateValueText();
            }
        }
        public void SetToLastValidData(OscSequenceData currentData, OscAllUsedFixtures.UsedFixtureData fixture, int sliderIndex, SliderPrefab slider)
        {
            int index = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == currentData);
            int startIndex = index;
            while (index > -1)
            {
                var currentSceneData = DataContainer.Instance.SequenceDataRef[index];
                var fixtureData = GetFixtureSliderData(currentSceneData, fixture, sliderIndex, slider, false);
                if (fixtureData != null)
                {
                    if (fixtureData.m_UseAnimationCurve == false)
                    {
                        DisplayVisualsAndSendSlideData(fixtureData.m_DmxValue, slider.m_DmxIndex, slider);
                    }
                    else
                    {
                        var lastIndex = fixtureData.m_AnimationCurveRaw.length - 1;
                        var LastValue = fixtureData.m_AnimationCurveRaw.keys[lastIndex].value;
                        DisplayVisualsAndSendSlideData(LastValue, slider.m_DmxIndex, slider);
                    }


                    if (startIndex == index)
                    {
                        // button set to on if this is were the data was changed 
                        slider.SetValidActive(true);
                    }
                    return;
                }
                index--;
            }
            DisplayVisualsAndSendSlideData(0, slider.m_DmxIndex, slider);
        }


        public void CreateAndSendData(OscSequenceData currentSceneData, OscAllUsedFixtures.UsedFixtureData fixture, int channelIndex, SliderPrefab slider, byte amount)
        {
            FixtureSliderData validData = GetFixtureSliderData(currentSceneData, fixture, channelIndex, slider);
            if (validData.m_DmxValue != amount)
            {
                validData.m_DmxValue = amount;
                DisplayVisualsAndSendSlideData(amount, slider.m_DmxIndex, slider);
            }
        }


        public FixtureSliderData GetFixtureSliderData(OscSequenceData scene, SliderPrefab slider, bool createData = true)
        {
            return GetFixtureSliderData(scene, slider.m_FixtureData, slider.m_ChannelIndex, slider, createData);
        }

        public FixtureSliderData GetFixtureSliderData(OscSequenceData scene, OscAllUsedFixtures.UsedFixtureData fixture, int sliderIndex, SliderPrefab slider, bool createData = true)
        {
            if (scene != null)
            {
                FixtureSliderData data = scene.m_SliderData.Find(e => e.m_FixtureData == fixture && e.m_ChannelIndex == sliderIndex);
                if (data == null && createData == true)
                {
                    FixtureSliderData newData = new FixtureSliderData();
                    newData.m_ChannelIndex = sliderIndex;
                    newData.m_FixtureData = fixture;

                    scene.m_SliderData.Add(newData);
                    data = newData;
                    slider.SetValidActive(true);
                }

                return data;
            }
            return null;
        }

        public void DisplayVisualsAndSendSlideData(float amount, int dmxIndex, SliderPrefab slider, ButtonText extraButton = null)
        {
            PhotonDMX.Instance.SendDmxData(dmxIndex, (byte)amount);

            if (slider != null)
            {
                slider.m_SliderValue = amount;

                if (slider.m_Slider != null)
                {
                    slider.m_Slider.SetValueWithoutNotify(amount);
                }
                if (slider.m_BtnTextValue != null)
                {
                    slider.m_BtnTextValue.text = ((int)amount).ToString() + slider.m_UseCurveString;
                }

                if (slider.m_BtnSpecial != null && slider.m_BtnSpecial.ButtonRef != null)
                {
                    SetSpecialButton(slider.m_SliderValue, slider, slider.m_BtnSpecial);
                }

                if (extraButton != null && extraButton.ButtonRef != null)
                {
                    SetSpecialButton(slider.m_SliderValue, slider, extraButton);
                }
            }
            else
            {
                /////////////////////////////////////////////////////////////////////////////////////////////////////Debug.LogError("slider is null");
            }
        }

        public SliderPrefab FindSliderPrefab(int fullDmx) => m_FullSliderPrefabList.Find(e => e.m_DmxIndex == fullDmx);


        public void SetSpecialButton(float dmxValue, SliderPrefab slider, ButtonText button)
        {
            switch (slider.m_CapabilityType)
            {
                case CapabilityType.Red:
                    button.ButtonRef.image.color = new Color32((byte)dmxValue, 0, 0, OscControllerRoot.MAX_DMX_VALUE);
                    break;
                case CapabilityType.Green:
                    button.ButtonRef.image.color = new Color32(0, (byte)dmxValue, 0, OscControllerRoot.MAX_DMX_VALUE);
                    break;
                case CapabilityType.Blue:
                    button.ButtonRef.image.color = new Color32(0, 0, (byte)dmxValue, OscControllerRoot.MAX_DMX_VALUE);
                    break;
                case CapabilityType.White:
                    button.ButtonRef.image.color = new Color32((byte)dmxValue, (byte)dmxValue, (byte)dmxValue, OscControllerRoot.MAX_DMX_VALUE);
                    break;
                case CapabilityType.Amber:
                    button.ButtonRef.image.color = new Color(1, 194f / OscControllerRoot.MAX_DMX_VALUE, 0, 1) * ((float)dmxValue / OscControllerRoot.MAX_DMX_VALUE);
                    break;

                case CapabilityType.Dimmer:
                case CapabilityType.Dimmer2:
                case CapabilityType.CWDimmer:
                    button.ButtonRef.image.color = new Color32(OscControllerRoot.MAX_DMX_VALUE, OscControllerRoot.MAX_DMX_VALUE, OscControllerRoot.MAX_DMX_VALUE, (byte)dmxValue);
                    break;

                case CapabilityType.WWDimmer:
                    float max = 6400;
                    if (slider.m_FixtureData.m_Fixture.Physical != null && slider.m_FixtureData.m_Fixture.Physical.Bulb != null && slider.m_FixtureData.m_Fixture.Physical.Bulb.ColorTemperature != null)
                    {
                        max = (float)slider.m_FixtureData.m_Fixture.Physical.Bulb.ColorTemperature;
                    }
                    float percentage = dmxValue / (float)OscControllerRoot.MAX_DMX_VALUE;
                    float temperature = Mathf.Lerp(2400, max, percentage);
                    var temperatureColor = Mathf.CorrelatedColorTemperatureToRGB(temperature);
                    button.ButtonRef.image.color = temperatureColor;
                    break;

                case CapabilityType.ColorPreset:
                case CapabilityType.ColorWheel:
                case CapabilityType.GoboWheel:
                case CapabilityType.StrobeShutter:
                case CapabilityType.ShutterStrobe:
                case CapabilityType.Strobe:
                case CapabilityType.Prism:
                    slider.m_WheelData = new List<SliderPrefab.WheelData>();
                    AvailableChannelValue dataColor = slider.m_FixtureData.m_Fixture.AvailableChannels[slider.m_ChannelName];
                    var dataWheels = slider.m_FixtureData.m_Fixture.Wheels;
                    if (dataColor.Capabilities != null)
                    {
                        slider.m_WheelData.Resize(dataColor.Capabilities.Count);

                        for (int i = 0; i < dataColor.Capabilities.Count; i++)
                        {
                            CapabilityElement cap = dataColor.Capabilities[i];
                            CapabilityType capType = cap.Type;
                            slider.m_WheelData[i].DmxMin = (int)cap.DmxRange[0];
                            slider.m_WheelData[i].DmxMax = (int)cap.DmxRange[1];

                            slider.m_WheelData[i].SpriteName = cap.SpriteName;
                            slider.m_WheelData[i].SpriteText = cap.SpriteText;

                            bool validItem = ((dmxValue >= slider.m_WheelData[i].DmxMin) && (dmxValue <= slider.m_WheelData[i].DmxMax));
                            if (validItem == true)
                            {
                                FixtureType(capType, cap, slider.m_WheelData[i], dataWheels, slider.m_ChannelName, validItem, button);
                            }
                        }
                    }
                    break;
            }
        }

        private void FixtureType(CapabilityType capType, CapabilityElement data, SliderPrefab.WheelData sliderWheelData, Dictionary<string, Wheel> wheelData, string channelName, bool validItem, ButtonText button)
        {
            sliderWheelData.m_CapabilityType = capType;
            switch (capType)
            {
                case CapabilityType.BeamAngle:
                    break;
                case CapabilityType.BeamPosition:
                    break;
                case CapabilityType.BladeInsertion:
                    break;
                case CapabilityType.BladeRotation:
                    break;
                case CapabilityType.BladeSystemRotation:
                    break;
                case CapabilityType.ColorIntensity:
                    break;
                case CapabilityType.ColorPreset:
                    if (data.Colors != null)
                    {
                        if (ColorUtility.TryParseHtmlString(data.Colors[0], out Color colorOut))
                        {
                            sliderWheelData.m_Color = colorOut;
                        }
                        else
                        {
                            SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                        }
                    }
                    else
                    {
                        SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                    }
                    break;
                case CapabilityType.ColorTemperature:
                    break;
                case CapabilityType.Effect:
                    break;
                case CapabilityType.EffectDuration:
                    break;
                case CapabilityType.EffectParameter:
                    break;
                case CapabilityType.EffectSpeed:
                    break;
                case CapabilityType.Focus:
                    break;
                case CapabilityType.Fog:
                    break;
                case CapabilityType.FogOutput:
                    break;
                case CapabilityType.FogType:
                    break;
                case CapabilityType.Frost:
                    break;
                case CapabilityType.FrostEffect:
                    break;
                case CapabilityType.Generic:
                    break;
                case CapabilityType.Intensity:
                    break;
                case CapabilityType.Iris:
                    break;
                case CapabilityType.IrisEffect:
                    break;
                case CapabilityType.Maintenance:
                    break;
                case CapabilityType.NoFunction:
                    break;
                case CapabilityType.Pan:
                    break;
                case CapabilityType.PanFine:
                    break;
                case CapabilityType.PanContinuous:
                    break;
                case CapabilityType.PanTiltSpeed:
                    break;
                case CapabilityType.Prism:
                    SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                    break;
                case CapabilityType.PrismRotation:
                    break;
                case CapabilityType.Rotation:
                    break;
                case CapabilityType.GoboWheel:
                    SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                    break;
                case CapabilityType.ShutterStrobe:
                    SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                    break;
                case CapabilityType.SoundSensitivity:
                    break;
                case CapabilityType.Speed:
                    break;
                case CapabilityType.StrobeDuration:
                    break;
                case CapabilityType.StrobeSpeed:
                    break;
                case CapabilityType.Tilt:
                    break;
                case CapabilityType.TiltFine:
                    break;
                case CapabilityType.TiltContinuous:
                    break;
                case CapabilityType.Time:
                    break;
                case CapabilityType.WheelRotation:
                    if (validItem == true)
                    {
                        button.ButtonRef.image.color = Color.white;
                        Debug.LogError("WheelRotation");
                    }
                    break;
                case CapabilityType.WheelShake:
                    break;
                case CapabilityType.WheelSlot:

                    if (channelName == "Color Wheel")
                    {
                        WheelSlot_ColorWheel(data, sliderWheelData, wheelData, channelName, button);
                    }
                    else
                    {
                        SetImageAndText(data, sliderWheelData, wheelData, channelName, button);
                    }

                    break;
                case CapabilityType.WheelSlotRotation:
                    if (validItem == true)
                    {
                        button.ButtonRef.image.color = Color.white;
                        Debug.LogError("WheelSlotRotation");
                    }
                    break;
                case CapabilityType.Zoom:
                    break;
                default:
                    break;
            }
        }

        private void WheelSlot_ColorWheel(CapabilityElement data, SliderPrefab.WheelData sliderWheelData, Dictionary<string, Wheel> wheelData, string channelName, ButtonText button)
        {
            var wheel = wheelData[channelName];
            float realSlotNumber = data.SlotNumber - 1;  // they start at 1 and can be 2.5 , so that you lerp between them 
            int start = realSlotNumber.Floor();
            int end = realSlotNumber.Ceil();

            Color startColor = Color.white;
            Color endColor = Color.white;
            try
            {
                if (wheel.Slots[start].Type == SlotType.Color)
                {
                    if (ColorUtility.TryParseHtmlString(wheel.Slots[start].Colors[0], out Color colorOut1))
                    {
                        startColor = colorOut1;
                    }

                    if (ColorUtility.TryParseHtmlString(wheel.Slots[end].Colors[0], out Color colorOut2))
                    {
                        endColor = colorOut2;
                    }
                    if (start != end)
                    {
                        sliderWheelData.m_Color = Color.Lerp(startColor, endColor, (end - realSlotNumber));
                        button.ButtonRef.image.color = (Color)sliderWheelData.m_Color;
                    }
                    else
                    {
                        sliderWheelData.m_Color = startColor;
                        button.ButtonRef.image.color = (Color)startColor;

                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("ERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRROR, json has been parsed correclty, but its trying to ref data which they have not included ");
            }
        }

        private void SetImageAndText(CapabilityElement data, SliderPrefab.WheelData sliderWheelData, Dictionary<string, Wheel> wheelData, string channelName, ButtonText button)
        {
            if (m_BasicSprite == null)
            {
                m_BasicSprite = button.ButtonRef.image.sprite;
            }
            sliderWheelData.m_Color = Color.white;

            if (string.IsNullOrEmpty(sliderWheelData.SpriteName) == false)
            {
                button.ButtonRef.image.color = Color.white;
                Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItem(Core.Mono, sliderWheelData.SpriteName, (item) => SetImage(button, item, sliderWheelData.SpriteName));
            }
            else
            {
                button.ButtonRef.image.sprite = m_BasicSprite;
            }
            button.TextRef.text = sliderWheelData.SpriteText;

        }


        private void SetImage(ButtonText button, Texture2D texture, string textureName)
        {
            if (button == null || button.ButtonRef == null || button.ButtonRef.image == null)
            {
                //Debug.LogError($"Image not found ", button.ButtonRef.gameObject);
                return;
            }

            if (texture != null)
            {
                Sprite sp = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                button.ButtonRef.image.sprite = sp;
            }
            else
            {
                Debug.LogError($"cannot find image: {textureName}");
            }
        }

        private void Paste()
        {
            if (DataContainer.Instance.CurrentSelectedSliderPrefab == null)
            {
                return;
            }
            int index = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == DataContainer.Instance.CurrentSequenceData);
            var currentSceneData = DataContainer.Instance.SequenceDataRef[index];
            var originalFixtureData = DataContainer.Instance.CurrentSelectedSliderPrefab.m_FixtureData;
            var destinationFixtureData = DataContainer.Instance.CurrentHightlightedSliderPrefab.m_FixtureData;

            if (originalFixtureData.m_JsonFileName != destinationFixtureData.m_JsonFileName)
            {
                Debug.LogError("Not matching ");
                return;
            }
            for (int i = 0; i < originalFixtureData.m_ChannelCount; i++)
            {
                
                var originalSlider = m_SliderPool.GetPublicList().Find(e => (e.m_FixtureData == originalFixtureData && e.m_ChannelIndex == i));
                var originalData = GetFixtureSliderData(currentSceneData, originalFixtureData, i, originalSlider, false);

                var destinationSlider = m_SliderPool.GetPublicList().Find(e => (e.m_FixtureData == destinationFixtureData && e.m_ChannelIndex == i));
                var destinationData = GetFixtureSliderData(currentSceneData, destinationFixtureData, i, destinationSlider, originalData != null);  // originalData != null this creats data if need to 
                
                // this deletes data if need to , to match orginal 
                if (originalData == null)
                {
                    destinationData = null;
                }
                CopySlider(currentSceneData, i, originalSlider, originalData, destinationSlider, destinationData, destinationFixtureData);

                // send newly copied slider data , if no data then do not send
                if (destinationData != null)
                {
                    DisplayVisualsAndSendSlideData(destinationData.m_DmxValue, destinationSlider.m_DmxIndex, destinationSlider);
                }
            }
            Debug.Log("Send Duplicate data");
        }
         
        public void CopySlider(OscSequenceData currentSceneData, int channelIndex, SliderPrefab originalSlider, FixtureSliderData originalData, SliderPrefab destinationSlider, FixtureSliderData destinationData, OscAllUsedFixtures.UsedFixtureData destinationFixtureData)
        {
            if (originalData == null && destinationData != null)
            {
                destinationSlider.SetValidActive(false);
                SetToLastValidData(currentSceneData, destinationFixtureData, channelIndex, destinationSlider);
            }
            if (originalData != null && destinationData == null)
            {
                destinationData = GetFixtureSliderData(currentSceneData, destinationFixtureData, channelIndex, destinationSlider, true);
            }

            if (originalData == null)
            {
                return;
            }

            destinationSlider.SetValidActive(true);
            destinationData.m_DmxValue = originalData.m_DmxValue;

            destinationData.m_AnimationCurveRaw = new AnimationCurve();
            for (int j = 0; j < originalData.m_AnimationCurveRaw.length; j++)
            {
                destinationData.m_AnimationCurveRaw.AddKey(originalData.m_AnimationCurveRaw[j]);
            }

            destinationData.m_AnimationCurveRepeated = new AnimationCurve();
            for (int j = 0; j < originalData.m_AnimationCurveRepeated.length; j++)
            {
                destinationData.m_AnimationCurveRepeated.AddKey(originalData.m_AnimationCurveRepeated[j]);
            }


            destinationData.m_AnimationCurveRepeatNumber = originalData.m_AnimationCurveRepeatNumber;

            destinationData.m_RepeatInvert = originalData.m_RepeatInvert;
            destinationData.m_UseAnimationCurve = originalData.m_UseAnimationCurve;
            destinationData.m_BezierType = originalData.m_BezierType;

            if (destinationData.m_UseAnimationCurve == true && originalData.m_AnimationCurveRaw.length > 0)
            {
                destinationData.m_DmxValue = (byte)originalData.m_AnimationCurveRaw[0].value;
            }
            UpdateIfCurve(destinationSlider, destinationData);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
            {
                Debug.LogError("Copy");
                DataContainer.Instance.CurrentSelectedSliderPrefab = DataContainer.Instance.CurrentHightlightedSliderPrefab;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                Debug.LogError("Paste");
                Paste();
            }
        }

        public void CopyAllMultipleCurves(FixtureSliderData currentFixtureData, SliderPrefab sliderPrefab)
        {
            if(OscControllerRoot.Instance.CurveEditorPanelRef.IsActive == false)
            {
                return;
            }
            Debug.Log("CopyAllMultipleCurves");
            if(MultipleSliders.Count > 0)
            {
                var currentSceneData = DataContainer.Instance.CurrentSequenceData;
                var original = currentFixtureData;

                for (int i = 0; i < MultipleSliders.Count; i++)
                {
                    var destinationSlider = MultipleSliders[i];
                    destinationSlider.m_FixtureSliderData = GetFixtureSliderData(currentSceneData, destinationSlider.m_FixtureData, destinationSlider.m_ChannelIndex, destinationSlider, true);

                    if (original == destinationSlider.m_FixtureSliderData)
                    {
                        return;
                    }

                    
                    AnimationCurve newCurve = new AnimationCurve();
                    for (int j = 0; j < original.m_AnimationCurveRaw.length; j++)
                    {
                        newCurve.AddKey(original.m_AnimationCurveRaw[j]);
                    }
                    destinationSlider.m_FixtureSliderData.m_AnimationCurveRaw = newCurve;
                    destinationSlider.m_FixtureSliderData.m_BezierType = original.m_BezierType;
                    destinationSlider.m_FixtureSliderData.m_UseAnimationCurve = original.m_UseAnimationCurve;
                    destinationSlider.m_FixtureSliderData.m_RepeatInvert = original.m_RepeatInvert;
                    destinationSlider.m_FixtureSliderData.m_AnimationCurveRepeatNumber = original.m_AnimationCurveRepeatNumber;

                    destinationSlider.m_FixtureSliderData.m_DmxValue = original.m_DmxValue;
                    destinationSlider.m_SliderValue = sliderPrefab.m_SliderValue;
                    destinationSlider.m_Slider.SetValueWithoutNotify(sliderPrefab.m_Slider.value);
                    UpdateIfCurve(destinationSlider, destinationSlider.m_FixtureSliderData);
                }
            }
        }

    }
}
