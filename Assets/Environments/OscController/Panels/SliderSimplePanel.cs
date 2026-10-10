using extOSC.UI;
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
    public class SliderSimplePanel : MonoBehaviour
    {
        public const float HALF_WAY = 0.5f;
        public readonly Vector3 TEMP_OFFSET = new Vector3(62f, 285f, 0);

        private const string BUTTON = "SimpleButtonPrefab";

        private Sprite m_BasicSprite = null;


        private HorizontalLayoutGroup m_HorizontalLayoutGroup;
        private RectTransform m_RectTransform;

        private ScrollRect m_ScrollView;
        private ContentSizeFitter m_ScrollViewContent;

        private ColorWheelControl m_ColorWheelControl;
        private OSCPad m_Pad;

        private SliderSimplePrefab m_PadAxisX;
        private SliderSimplePrefab m_PadAxisY;

        private List<SliderSimplePrefab> m_Red = new List<SliderSimplePrefab>();
        private List<SliderSimplePrefab> m_Green = new List<SliderSimplePrefab>();
        private List<SliderSimplePrefab> m_Blue = new List<SliderSimplePrefab>();


        private List<SliderSimplePrefab> m_Sliders = new List<SliderSimplePrefab>();
        private List<SliderSimplePrefab> m_Rotarys = new List<SliderSimplePrefab>();

        private List<ButtonTextBorder> m_Buttons = new List<ButtonTextBorder>();
        private List<SliderSimplePrefab> m_FakeSlidersForButton = new List<SliderSimplePrefab>(); // needed for collecting data 

        private PoolManagerLocalComponent<Button> m_ScrollViewButtons;
        private Button m_LightInfoImage;
        private Text m_LightInfoText;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            // scroll view 
            m_ScrollView = this.GetComponentInChildren<ScrollRect>(true);
            m_ScrollViewContent = m_ScrollView.GetComponentInChildren<ContentSizeFitter>(true);
            m_ScrollView.SetActive(false);

            // fix the prefab for the button 
            var buttonPrefab = CachedObjects.Instance.InstantiateCachedGameObject(BUTTON);
            var rect = buttonPrefab.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonPrefab.transform.ClearLocals();
            m_ScrollViewButtons = new PoolManagerLocalComponent<Button>(buttonPrefab, m_ScrollViewContent.transform);

            m_ColorWheelControl = this.transform.SearchComponent<Transform>("ColorWheel").gameObject.ForceComponent<ColorWheelControl>();

            //Pad
            m_Pad = this.transform.SearchComponent<OSCPad>("Pad");
            m_Pad.ResetValue = false;
            m_Pad.WholeNumbers = true;
            m_Pad.MinValue = Vector2.zero;
            m_Pad.MaxValue = new Vector2(OscControllerRoot.MAX_DMX_VALUE, OscControllerRoot.MAX_DMX_VALUE);

            // fake sliders for pad, needed for recording data
            var padAxisX = new GameObject("PadAxisX");
            padAxisX.transform.SetParent(this.transform);
            var padAxisY = new GameObject("PadAxisY");
            padAxisY.transform.SetParent(this.transform);

            m_PadAxisX = padAxisX.ForceComponent<SliderSimplePrefab>();
            m_PadAxisY = padAxisY.ForceComponent<SliderSimplePrefab>();
            m_PadAxisX.AssignComponentsData();
            m_PadAxisY.AssignComponentsData();

            m_PadAxisX.m_Border = m_Pad.transform.SearchComponent<Image>("Border");
            m_PadAxisY.m_Border = m_Pad.transform.SearchComponent<Image>("Border");

            //sliders
            OSCSlider slider = null;
            slider = this.transform.SearchComponent<OSCSlider>("Slider1");
            AddSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("Slider2");
            AddSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("ColorWheelSliderPrefab");
            AddSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("Slider5");
            AddSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("Slider6");
            AddSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("Slider7");
            AddSlider(slider);

            //Rotarys
            OSCRotary rotary = null;
            rotary = this.transform.SearchComponent<OSCRotary>("Rotary1");
            AddRotary(rotary);
            rotary = this.transform.SearchComponent<OSCRotary>("Rotary2");
            AddRotary(rotary);
            rotary = this.transform.SearchComponent<OSCRotary>("Rotary3");
            AddRotary(rotary);
            rotary = this.transform.SearchComponent<OSCRotary>("Rotary4");
            AddRotary(rotary);

            //Buttons
            Button button = null;
            button = this.transform.SearchComponent<Button>("Button1");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button2");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button3");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button4");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button5");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button6");
            AddButton(button);
            button = this.transform.SearchComponent<Button>("Button7");
            AddButton(button);

            //sliders

            slider = this.transform.SearchComponent<OSCSlider>("FakeSlider1");
            AddFakeSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("FakeSlider2");
            AddFakeSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("FakeSlider3");
            AddFakeSlider(slider);
            slider = this.transform.SearchComponent<OSCSlider>("FakeSlider4");
            AddFakeSlider(slider);

            // blank sprite 
            m_BasicSprite =  buttonPrefab.GetComponent<Button>().image.sprite;

            // Light info 
            var lightInfo = this.transform.SearchComponent<Transform>("LightInfo");
            m_LightInfoImage = lightInfo.transform.SearchComponent<Button>("LightInfoImage");
            m_LightInfoImage.image.color = Color.white;
            m_LightInfoText = lightInfo.transform.SearchComponent<Text>("LightInfoText");

            DataContainer.Instance.OnSequenceDataChange += AssignToFixture;

            this.transform.FindAndFixMasks();
        }


        private void AddSlider(OSCSlider slider)
        {
            var sliderPrefab = slider.gameObject.ForceComponent<SliderSimplePrefab>();
            sliderPrefab.AssignComponentsData();
            sliderPrefab.m_Slider.minValue = 0;
            sliderPrefab.m_Slider.maxValue = OscControllerRoot.MAX_DMX_VALUE;
            sliderPrefab.m_Slider.wholeNumbers = true;

            m_Sliders.Add(sliderPrefab);
        }

        private void AddFakeSlider(OSCSlider slider)
        {
            var sliderPrefab = slider.gameObject.ForceComponent<SliderSimplePrefab>();
            sliderPrefab.AssignComponentsData();
            sliderPrefab.m_Slider.minValue = 0;
            sliderPrefab.m_Slider.maxValue = OscControllerRoot.MAX_DMX_VALUE;
            sliderPrefab.m_Slider.wholeNumbers = true;
            sliderPrefab.gameObject.SetActive(false);
            m_FakeSlidersForButton.Add(sliderPrefab);
        }


        private void AddButton(Button button)
        {
            ButtonTextBorder buttonText = new ButtonTextBorder(button);
            m_Buttons.Add(buttonText);
        }


        private void AddRotary(OSCRotary rotary)
        {
            var rotaryPrefab = rotary.gameObject.ForceComponent<SliderSimplePrefab>();
            rotaryPrefab.AssignComponentsData();
            rotaryPrefab.m_OSCRotary.MinValue = 0;
            rotaryPrefab.m_OSCRotary.MaxValue = OscControllerRoot.MAX_DMX_VALUE;
            rotaryPrefab.m_OSCRotary.WholeNumbers = true;

            rotaryPrefab.m_Border.color = Color.black;
            m_Rotarys.Add(rotaryPrefab);
        }

        private void OnEnable()
        {
            if (m_LightInfoText != null)
            {
                AssignToFixture(null);
            }
        }

        public void ReGenerate() => AssignToFixture(null);

        [InspectorButton]
        private void DebugJsonData()
        {
            if (DataContainer.Instance.CurrentUsedFixtureData != null)
            {
                if (DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts == null)
                {
                    DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts = new List<Layout>();

                    DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts.Add(new Layout());
                    DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Sliders = new List<Fixtures.Slider>();
                }

                Fixtures.Slider fixSlider = new Fixtures.Slider();
                fixSlider.Index = 22;

                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Sliders.Add(fixSlider);

                Fixtures.Slider fixSlider1 = new Fixtures.Slider();
                fixSlider1.Index = 33;

                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Sliders.Add(fixSlider1);



                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ShortName = "22";

                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Pad = new Pad();
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Pad.IndexX = 22;
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].Pad.IndexX = 23;


                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel = new ColourWheel();
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Red = new List<int>();
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Red.Add(1);
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Red.Add(8);
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Blue = new List<int>();
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Blue.Add(11);
                DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts[0].ColourWheel.Blue.Add(81);
                var dd = DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.ToJson();
                Debug.LogError(dd);
            }
        }

        private void AssignToFixture(DataContainer.OscSequenceData sceneDataNull)
        {
            ClearAll();

            if (DataContainer.Instance.CurrentUsedFixtureData == null)
            {
                return;
            }

            OscSequenceData currentSceneData = DataContainer.Instance.CurrentSequenceData;
            OscAllUsedFixtures.UsedFixtureData fixture = DataContainer.Instance.CurrentUsedFixtureData;
            Mode modeChannels = fixture.m_Fixture.Modes[fixture.m_ModeIndex];

            Layout layout = null;
            try
            {
                if (DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts != null)
                {
                    layout = DataContainer.Instance.CurrentUsedFixtureData.m_Fixture.Layouts.Find(e => e.ShortName.CaseInsensitiveContains(modeChannels.ShortName));
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"Weird error   {e}");
            }
            if (layout == null)
            {
                ///Debug.LogError($"Could not find layout: {modeChannels.ShortName}");
                return;
            }

            SetLightInfo(fixture);


            // setup pad
            if (layout.Pad != null)
            {
                m_Pad.interactable = true;
                int dmxIndexX = layout.Pad.IndexX + fixture.m_DmxStartIndex;
                int dmxIndexY = layout.Pad.IndexY + fixture.m_DmxStartIndex;

                m_PadAxisX.InitialiseData(fixture, layout.Pad.IndexX, dmxIndexX, 0, false);
                m_PadAxisY.InitialiseData(fixture, layout.Pad.IndexY, dmxIndexY, 0, false);

                if (GetSavedData(currentSceneData, fixture, layout.Pad.IndexX, out int valueX) == true)
                {
                    if (GetSavedData(currentSceneData, fixture, layout.Pad.IndexY, out int valueY) == true)
                    {
                        m_Pad.SetWithNoCallback(new Vector2(valueX, valueY));
                        m_PadAxisX.SetValidActive(true);
                        m_PadAxisY.SetValidActive(true);
                    }
                }

                m_Pad.SetActive(true);
                m_Pad.OnValueChanged.AddListener((vector2) =>
                {
                    byte x = (byte)(vector2.x);
                    byte y = (byte)(vector2.y);
                    OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, x, layout.Pad.IndexX, dmxIndexX, m_PadAxisX);   // X
                    OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, y, layout.Pad.IndexY, dmxIndexY, m_PadAxisY);   // Y
                });

                RightClick padRightClick = m_Pad.gameObject.ForceComponent<RightClick>();
                padRightClick.onRightClick.RemoveAllListeners();
                padRightClick.onRightClick.AddListener(() =>
                {
                    Debug.Log("padRightClick right click");
                    ResetToLastPosition(fixture, layout.Pad.IndexX, m_PadAxisX);
                    ResetToLastPosition(fixture, layout.Pad.IndexY, m_PadAxisY);
                    m_Pad.SetWithNoCallback(new Vector2((float)m_PadAxisX.m_SliderValue, (float)m_PadAxisY.m_SliderValue));
                });
            }

            // colour wheel, flat coded which is why massive
            ColourWheel colourWheel = layout.ColourWheel;
            if (colourWheel != null)
            {
                m_ColorWheelControl.SetEnable(true);
                Color32 loadedColor = Color.white;
                if (m_Red.Count < colourWheel.Red.Count)
                {
                    m_Red.Resize(colourWheel.Red.Count);
                }
                for (int i = 0; i < m_Red.Count; i++)
                {
                    if (m_Red[i] == null)
                    {
                        GameObject temp = new GameObject("Red Wheel");
                        temp.transform.SetParent(this.transform);
                        m_Red[i] = temp.ForceComponent<SliderSimplePrefab>();
                        m_Red[i].AssignComponentsData();
                    }

                    int coreIndex = i;
                    int dmxIndex = colourWheel.Red[i] + fixture.m_DmxStartIndex;
                    int redSliderIndex = colourWheel.Red[i];

                    if (i == 0)
                    {
                        if (GetSavedData(currentSceneData, fixture, colourWheel.Red[i], out int value) == true)
                        {
                            loadedColor.r = (byte)value;
                        }
                    }

                    m_Red[i].InitialiseData(fixture, colourWheel.Red[i], dmxIndex, 0, false);
                    m_ColorWheelControl.OnEventColor.AddListener((colour) =>
                    {
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)(colour.r * OscControllerRoot.MAX_DMX_VALUE), redSliderIndex, dmxIndex, m_Red[coreIndex]);
                    });
                }

                if (m_Green.Count < colourWheel.Green.Count)
                {
                    m_Green.Resize(colourWheel.Green.Count);
                }
                for (int i = 0; i < m_Green.Count; i++)
                {
                    if (m_Green[i] == null)
                    {
                        GameObject temp = new GameObject("Green Wheel");
                        temp.transform.SetParent(this.transform);
                        m_Green[i] = temp.ForceComponent<SliderSimplePrefab>();
                        m_Green[i].AssignComponentsData();
                    }

                    int coreIndex = i;
                    int dmxIndex = colourWheel.Green[i] + fixture.m_DmxStartIndex;
                    int greenSliderIndex = colourWheel.Green[i];

                    if (i == 0)
                    {
                        if (GetSavedData(currentSceneData, fixture, colourWheel.Green[i], out int value) == true)
                        {
                            loadedColor.g = (byte)value;
                        }
                    }

                    m_Green[i].InitialiseData(fixture, colourWheel.Green[i], dmxIndex, 0, false);
                    m_ColorWheelControl.OnEventColor.AddListener((colour) =>
                    {
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)(colour.g * OscControllerRoot.MAX_DMX_VALUE), greenSliderIndex, dmxIndex, m_Green[coreIndex]);
                    });
                }

                if (m_Blue.Count < colourWheel.Blue.Count)
                {
                    m_Blue.Resize(colourWheel.Blue.Count);
                }
                for (int i = 0; i < m_Blue.Count; i++)
                {
                    if (m_Blue[i] == null)
                    {
                        GameObject temp = new GameObject("Blue Wheel");
                        temp.transform.SetParent(this.transform);
                        m_Blue[i] = temp.ForceComponent<SliderSimplePrefab>();
                        m_Blue[i].AssignComponentsData();
                    }

                    int coreIndex = i;
                    int dmxIndex = colourWheel.Blue[i] + fixture.m_DmxStartIndex;
                    int blueSliderIndex = colourWheel.Blue[i];

                    if (i == 0)
                    {
                        if (GetSavedData(currentSceneData, fixture, colourWheel.Blue[i], out int value) == true)
                        {
                            loadedColor.b = (byte)value;
                        }
                    }

                    m_Blue[i].InitialiseData(fixture, colourWheel.Blue[i], dmxIndex, 0, false);
                    m_ColorWheelControl.OnEventColor.AddListener((colour) =>
                    {
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)(colour.b * OscControllerRoot.MAX_DMX_VALUE), blueSliderIndex, dmxIndex, m_Blue[coreIndex]);
                    });
                }

                m_ColorWheelControl.PickColor(loadedColor);
                m_ColorWheelControl.SetActive(true);
            }

            //
            if (layout.Sliders != null)
            {
                for (int i = 0; i < layout.Sliders.Count; i++)
                {
                    Fixtures.Slider sliderDetails = layout.Sliders[i];
                    int dmxIndex = fixture.m_DmxStartIndex + sliderDetails.Index;

                    SliderSimplePrefab sliderPrefab = null;
                    switch (sliderDetails.LayoutEnumRef)
                    {
                        case LayoutEnum.Slider1:
                            sliderPrefab = m_Sliders[0];
                            break;
                        case LayoutEnum.Slider2:
                            sliderPrefab = m_Sliders[1];
                            break;
                        case LayoutEnum.Slider3:
                            sliderPrefab = m_Sliders[2];
                            break;
                        default:
                            break;
                    }


                    if (GetSavedData(currentSceneData, fixture, sliderDetails.Index, out int value) == true)
                    {
                        sliderPrefab.m_Slider.SetValueWithoutNotify(value);
                        sliderPrefab.SetValidActive(true);
                    }

                    sliderPrefab.InitialiseData(fixture, sliderDetails.Index, dmxIndex, 0, false);
                    sliderPrefab.m_Slider.interactable = true;

                    sliderPrefab.m_Slider.onValueChanged.RemoveAllListeners();
                    sliderPrefab.m_Slider.onValueChanged.AddListener((amount) =>
                    {
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)amount, sliderDetails.Index, dmxIndex, sliderPrefab);
                    });


                    RightClick sliderRightClick = sliderPrefab.m_Slider.gameObject.ForceComponent<RightClick>();
                    sliderRightClick.onRightClick.RemoveAllListeners();
                    sliderRightClick.onRightClick.AddListener(() =>
                    {
                        Debug.Log("sliderPrefab right click");
                        ResetToLastPosition(fixture, sliderDetails.Index, sliderPrefab);
                    });

                }
            }


            if (layout.Rotarys != null)
            {
                for (int i = 0; i < layout.Rotarys.Count; i++)
                {
                    Fixtures.Slider rotaryDetails = layout.Rotarys[i];
                    int dmxIndex = fixture.m_DmxStartIndex + rotaryDetails.Index;

                    SliderSimplePrefab sliderPrefab = null;
                    switch (rotaryDetails.LayoutEnumRef)
                    {
                        case LayoutEnum.Rotary1:
                            sliderPrefab = m_Rotarys[0];
                            break;
                        case LayoutEnum.Rotary2:
                            sliderPrefab = m_Rotarys[1];
                            break;
                        case LayoutEnum.Rotary3:
                            sliderPrefab = m_Rotarys[2];
                            break;
                        case LayoutEnum.Rotary4:
                            sliderPrefab = m_Rotarys[3];
                            break;
                        default:
                            break;
                    }

                    if (GetSavedData(currentSceneData, fixture, rotaryDetails.Index, out int value) == true)
                    {
                        sliderPrefab.m_OSCRotary.Value = value;
                        sliderPrefab.SetValidActive(true);
                    }

                    sliderPrefab.InitialiseData(fixture, rotaryDetails.Index, dmxIndex, 0, false);

                    sliderPrefab.m_OSCRotary.interactable = true;
                    sliderPrefab.m_OSCRotary.OnValueChanged.RemoveAllListeners();
                    sliderPrefab.m_OSCRotary.OnValueChanged.AddListener((amount) =>
                    {
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)amount, rotaryDetails.Index, dmxIndex, sliderPrefab);
                    });

                    RightClick sliderRightClick = sliderPrefab.m_OSCRotary.gameObject.ForceComponent<RightClick>();
                    sliderRightClick.onRightClick.RemoveAllListeners();
                    sliderRightClick.onRightClick.AddListener(() =>
                    {
                        Debug.Log("sliderPrefab right click");
                        ResetToLastPosition(fixture, rotaryDetails.Index, sliderPrefab);
                        sliderPrefab.m_OSCRotary.SetWithNoCallback(sliderPrefab.m_SliderValue);
                    });

                }
            }

            // Buttons, need fake sliders, to keep track of data 
            if (layout.Buttons != null)
            {
                for (int i = 0; i < layout.Buttons.Count; i++)
                {
                    Fixtures.Slider buttonDetails = layout.Buttons[i];
                    int dmxIndex = fixture.m_DmxStartIndex + buttonDetails.Index;

                    ButtonTextBorder buttonPrefab = null;
                    SliderSimplePrefab sliderPrefab = null;
                    switch (buttonDetails.LayoutEnumRef)
                    {
                        case LayoutEnum.Button1:
                            buttonPrefab = m_Buttons[0];
                            sliderPrefab = m_FakeSlidersForButton[0];
                            break;
                        case LayoutEnum.Button2:
                            buttonPrefab = m_Buttons[1];
                            sliderPrefab = m_FakeSlidersForButton[1];
                            break;
                        case LayoutEnum.Button3:
                            buttonPrefab = m_Buttons[2];
                            sliderPrefab = m_FakeSlidersForButton[2];
                            break;
                        case LayoutEnum.Button4:
                            buttonPrefab = m_Buttons[3];
                            sliderPrefab = m_FakeSlidersForButton[3];
                            break;

                        default:
                            break;
                    }

                    buttonPrefab.ButtonRef.interactable = true;
                    sliderPrefab.m_Slider.interactable = true;


                    if (GetSavedData(currentSceneData, fixture, buttonDetails.Index, out int value) == true)
                    {
                        sliderPrefab.m_SliderValue = value;
                        sliderPrefab.m_Slider.SetValueWithoutNotify(value);
                        OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(sliderPrefab.m_SliderValue, sliderPrefab, buttonPrefab);
                        buttonPrefab.SetValidActive(true);
                    }


                    sliderPrefab.AssignComponentsData(sliderPrefab.gameObject, buttonPrefab.ButtonRef.gameObject);
                    sliderPrefab.InitialiseData(fixture, buttonDetails.Index, dmxIndex, 0, false);

                    buttonPrefab.ChannelTextRef.text = sliderPrefab.m_ChannelName;
                    buttonPrefab.ButtonRef.onClick.RemoveAllListeners();
                    buttonPrefab.ButtonRef.onClick.AddListener(() =>
                    {
                        CreateScrollView(sliderPrefab, buttonPrefab);
                    });


                    RightClick buttonRightClick = buttonPrefab.ButtonRef.gameObject.ForceComponent<RightClick>();
                    buttonRightClick.onRightClick.RemoveAllListeners();
                    buttonRightClick.onRightClick.AddListener(() =>
                    {
                        ResetToLastPosition(fixture, buttonDetails.Index, sliderPrefab);
                        buttonPrefab.SetValidActive(false);
                    });

                    sliderPrefab.SetActive(false);
                }
            }

            // buttons with sliders
            if (layout.ButtonSliders != null)
            {
                for (int i = 0; i < layout.ButtonSliders.Count; i++)
                {
                    Fixtures.Slider buttonDetails = layout.ButtonSliders[i];
                    int dmxIndex = fixture.m_DmxStartIndex + buttonDetails.Index;

                    ButtonTextBorder buttonPrefab = null;
                    SliderSimplePrefab sliderPrefab = null;
                    switch (buttonDetails.LayoutEnumRef)
                    {
                        case LayoutEnum.ButtonSlider1:
                            buttonPrefab = m_Buttons[4];
                            sliderPrefab = m_Sliders[3];
                            break;
                        case LayoutEnum.ButtonSlider2:
                            buttonPrefab = m_Buttons[5];
                            sliderPrefab = m_Sliders[4];
                            break;
                        case LayoutEnum.ButtonSlider3:
                            buttonPrefab = m_Buttons[6];
                            sliderPrefab = m_Sliders[5];
                            break;
                        default:
                            break;
                    }

                    buttonPrefab.ButtonRef.interactable = true;
                    sliderPrefab.m_Slider.interactable = true;

                    if (GetSavedData(currentSceneData, fixture, buttonDetails.Index, out int value) == true)
                    {
                        sliderPrefab.m_SliderValue = value;
                        sliderPrefab.m_Slider.SetValueWithoutNotify(value);
                        OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(sliderPrefab.m_SliderValue, sliderPrefab, buttonPrefab);
                        sliderPrefab.SetValidActive(true);
                        buttonPrefab.SetValidActive(true);
                    }


                    sliderPrefab.AssignComponentsData();
                    sliderPrefab.InitialiseData(fixture, buttonDetails.Index, dmxIndex, 0, false);


                    sliderPrefab.m_Slider.onValueChanged.RemoveAllListeners();
                    sliderPrefab.m_Slider.onValueChanged.AddListener((amount) =>
                    {
                        sliderPrefab.SetValidActive(true);
                        buttonPrefab.SetValidActive(true);
                        OscControllerRoot.Instance.SliderPanelRef.SliderValueChanged(fixture, (byte)amount, buttonDetails.Index, dmxIndex, sliderPrefab);
                    });


                    buttonPrefab.ChannelTextRef.text = sliderPrefab.m_TxtChannel.text;
                    buttonPrefab.ButtonRef.onClick.RemoveAllListeners();
                    buttonPrefab.ButtonRef.onClick.AddListener(() =>
                    {
                        CreateScrollView(sliderPrefab, buttonPrefab);
                    });


                    RightClick sliderRightClick = sliderPrefab.m_Slider.gameObject.ForceComponent<RightClick>();
                    sliderRightClick.onRightClick.RemoveAllListeners();
                    sliderRightClick.onRightClick.AddListener(() =>
                    {
                        ResetToLastPosition(fixture, buttonDetails.Index, sliderPrefab);
                        sliderPrefab.SetValidActive(false);
                        buttonPrefab.SetValidActive(false);
                    });

                    RightClick buttonRightClick = buttonPrefab.ButtonRef.gameObject.ForceComponent<RightClick>();
                    buttonRightClick.onRightClick.RemoveAllListeners();
                    buttonRightClick.onRightClick.AddListener(() =>
                    {

                        ResetToLastPosition(fixture, buttonDetails.Index, sliderPrefab);
                        sliderPrefab.SetValidActive(false);
                        buttonPrefab.SetValidActive(false);
                    });
                }
            }
        }

        private void SetLightInfo(OscAllUsedFixtures.UsedFixtureData fixture)
        {
            m_LightInfoText.text = fixture.m_Fixture.ShortName;

            string linkage = fixture.m_Fixture.CatalogueLinkageGuid;
            if (string.IsNullOrEmpty(linkage) == false)
            {
                Core.AssetBundlesRef.CatalogueThumbnailAssetBundleRef.GetItem(Core.Mono, linkage, (texture) =>
                {
                    if (texture != null)
                    {
                        Sprite sp = Sprite.Create((Texture2D)texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        m_LightInfoImage.image.sprite = sp;
                    }
                });
            }
        }

        private void ResetToLastPosition(OscAllUsedFixtures.UsedFixtureData fixture, int channelIndex, SliderPrefab sliderPrefab )
        {
            OscSequenceData currentSceneData = DataContainer.Instance.CurrentSequenceData;

            FixtureSliderData validData = OscControllerRoot.Instance.SliderPanelRef.GetFixtureSliderData(currentSceneData, fixture, channelIndex, sliderPrefab, false);
            if (validData != null)
            {
                currentSceneData.m_SliderData.Remove(validData);
                sliderPrefab.SetValidActive(false);
                OscControllerRoot.Instance.SliderPanelRef.SetToLastValidData(currentSceneData, fixture, channelIndex, sliderPrefab);               
            }
        }

        private void ClearAll()
        {
            m_LightInfoText.text = "";
            m_LightInfoImage.image.sprite = m_BasicSprite;
            m_Buttons.ForEach(e => e.ButtonRef.interactable = false);
            m_Buttons.ForEach(e => e.ChannelTextRef.text = "");
            m_Buttons.ForEach(e => e.SetValidActive(false));

            
            m_Sliders.ForEach(e => e.m_TxtChannel.text = "");
            m_Sliders.ForEach(e => e.m_Slider.interactable = false);
            m_Sliders.ForEach(e => e.SetValidActive(false));

            m_Rotarys.ForEach(e => e.m_OSCRotary.interactable = false);
            m_Rotarys.ForEach(e => e.m_TxtChannel.text = "");
            m_Rotarys.ForEach(e => e.SetValidActive(false));

            m_Pad.interactable = false;
            m_Pad.OnValueChanged.RemoveAllListeners();

            m_PadAxisX.SetValidActive(false);
            m_PadAxisY.SetValidActive(false);

            m_ColorWheelControl.SetEnable(false);
            m_ColorWheelControl.OnEventColor.RemoveAllListeners();
        }

        private bool GetSavedData(OscSequenceData scene, OscAllUsedFixtures.UsedFixtureData fixture, int channelIndex, out int value)
        {
            value = 0;
            if (scene != null && scene.m_SliderData != null)
            {
                FixtureSliderData data = scene.m_SliderData.Find(e => e.m_FixtureData == fixture && e.m_ChannelIndex == channelIndex);
                if (data != null)
                {
                    value = data.m_DmxValue;
                    return true;
                }
            }
            return false;
        }
     

        // wheelPanel.cs   public void SetWheelData(SliderPrefab slider, Transform trans)
        private void CreateScrollView(SliderPrefab sliderPrefab, ButtonTextBorder buttonText)
        {
            const int INVALID_DMX_VALUE = -1;

            m_Buttons.ForEach(e => e.ButtonRef.SetActive(true)); // turn all on , safety 
            buttonText.ButtonRef.SetActive(false); // turn specific off

            m_ScrollView.transform.SetAsLastSibling();
            m_ScrollView.SetActive(true);
            m_ScrollView.transform.position = buttonText.ButtonRef.transform.position;
            m_ScrollView.transform.position += TEMP_OFFSET;
            m_ScrollViewButtons.DeSpawnAll();

            OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(INVALID_DMX_VALUE, sliderPrefab, buttonText);

            for (int i = 0; i < sliderPrefab.m_WheelData.Count; i++)
            {
                SliderPrefab.ButtonText newItem = new SliderPrefab.ButtonText(m_ScrollViewButtons.SpawnObject());
                WheelData currentWheelData = sliderPrefab.m_WheelData[i];

                // set the colour of the button on the wheel 
                if (newItem.ButtonRef.image != null && newItem.ButtonRef.image.color != null)
                {
                    newItem.ButtonRef.image.color = OscControllerRoot.UN_SELECTED; // clear the colour
                }
                else
                {
                    Debug.LogError("newItem.ButtonRef.image is null ???");
                }
                if (newItem.TextRef != null)
                {
                    newItem.TextRef.text = "";
                }

                // assign a image to button 
                float value = Mathf.Lerp(currentWheelData.DmxMin, currentWheelData.DmxMax, HALF_WAY);
                OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(value, sliderPrefab, newItem);

                newItem.ButtonRef.onClick.RemoveAllListeners();
                newItem.ButtonRef.onClick.AddListener(() =>
                {
                    m_ScrollView.SetActive(false);
                    buttonText.ButtonRef.SetActive(true);

                    sliderPrefab.m_SliderValue = Mathf.Lerp(currentWheelData.DmxMin, currentWheelData.DmxMax, HALF_WAY);
                    OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(sliderPrefab.m_SliderValue, sliderPrefab.m_DmxIndex, sliderPrefab, buttonText);
                    OscControllerRoot.Instance.SliderPanelRef.SetSpecialButton(sliderPrefab.m_SliderValue, sliderPrefab, buttonText);

                    sliderPrefab.SetValidActive(true);
                    buttonText.SetValidActive(true);

                    if (sliderPrefab.m_Slider != null)
                    {
                        sliderPrefab.m_Slider.SetValueWithoutNotify(sliderPrefab.m_SliderValue);
                        sliderPrefab.m_Slider.minValue = currentWheelData.DmxMin;
                        sliderPrefab.m_Slider.maxValue = currentWheelData.DmxMax;
                        sliderPrefab.m_Slider.wholeNumbers = true;

                        sliderPrefab.m_Slider.onValueChanged.RemoveAllListeners();
                        sliderPrefab.m_Slider.onValueChanged.AddListener((amount) =>
                        {
                            sliderPrefab.m_SliderValue = amount;
                            OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(sliderPrefab.m_SliderValue, sliderPrefab.m_DmxIndex, sliderPrefab, buttonText);
                        });
                    }
                });
            }

        }
    }
}
