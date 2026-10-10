using extOSC.UI;
using OscController;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;



public class SliderSimplePrefab : SliderPrefab
{

    public override void SetValidActive(bool active)
    {
        m_Border.color = active ? Color.red : Color.black;
    }
}

public class ButtonTextBorder : SliderPrefab.ButtonText
{
    public Image BorderRef { get; protected set; }

    public ButtonTextBorder(GameObject root) : base(root)
    {
        BorderRef = root.transform.SearchComponent<Image>("Border");
    }

    public ButtonTextBorder(Button button) : base(button)
    {
        if (ButtonRef != null)
        {
            BorderRef = ButtonRef.transform.SearchComponent<Image>("Border");
        }
    }

    public void SetValidActive(bool active)
    {
        BorderRef.color = active ? Color.red : Color.black;
    }
}


public class SliderPrefab : MonoBehaviour
{

    public class ButtonText
    {
        public Button ButtonRef { get; protected set; }
        public Text TextRef { get; protected set; }
        public Text ChannelTextRef { get; protected set; }

        protected ButtonText() { }
    
        public ButtonText(GameObject root)
        {
            ButtonRef = root.GetComponent<Button>();
            TextRef = root.GetComponentInChildren<Text>();
            ChannelTextRef = root.transform.SearchComponent<Text>("Channel", false);
        }

        public ButtonText(Button button)
        {
            ButtonRef = button;
            if (ButtonRef != null)
            {
                TextRef = ButtonRef.GetComponentInChildren<Text>();
                ChannelTextRef = ButtonRef.transform.SearchComponent<Text>("Channel", false);
            }
        }
    }

    public class WheelData
    {
        public int DmxMin;
        public int DmxMax;
        public Color? m_Color;
        public Fixtures.CapabilityType m_CapabilityType;
        public string SpriteName = null;
        public string SpriteText = null;
    }

    public float m_SliderValue;
    public Slider m_Slider;
    public OSCRotary m_OSCRotary;
    public Image m_SliderHandleGroupSelected;
    public Text m_TxtDMXindex;
    public Button m_BtnValue;
    public Text m_BtnTextValue;
    public Text m_TxtChannel;
    public Button m_BtnActive;
    public ButtonText m_BtnSpecial;
    public Image m_Border;
    public UsedFixtureData m_FixtureData;
    public int m_ChannelIndex;
    public int m_DmxIndex;
    public string m_ChannelName = "";
    public Fixtures.CapabilityType m_CapabilityType = Fixtures.CapabilityType.ERROR;
    public List<WheelData> m_WheelData;
    public string m_UseCurveString = "s";
    public FixtureSliderData m_FixtureSliderData;


    public virtual void SetValidActive(bool active)
    {
        m_BtnActive.SetActive(active);
    }

    public void AssignComponentsData()
    {
        this.m_Slider = this.GetComponent<Slider>();
        this.m_OSCRotary = this.GetComponent<OSCRotary>();
        this.m_TxtDMXindex = this.transform.SearchComponent<Text>("DMX", false);
        this.m_BtnValue = this.transform.SearchComponent<Button>("ValueButton", false);
        if (this.m_BtnValue != null)
        {
            this.m_BtnTextValue = m_BtnValue.GetComponentInChildren<Text>();
        }
        
        this.m_TxtChannel = this.transform.SearchComponent<Text>("Channel", false);
        if (this.m_TxtChannel != null)
        {
            this.m_TxtChannel.text = "";
        }
        this.m_BtnSpecial = new ButtonText(this.transform.SearchComponent<Button>("Special", false));
        this.m_Border = this.transform.SearchComponent<Image>("Border", false);
        this.m_BtnActive = this.transform.SearchComponent<Button>("Active", false);
        this.m_SliderHandleGroupSelected = this.transform.SearchComponent<Image>("Handle", false);
    }


    public void AssignComponentsData(GameObject slider, GameObject button)
    {
        this.m_Slider = slider.GetComponent<Slider>();
        this.m_OSCRotary = this.GetComponent<OSCRotary>();
        this.m_TxtDMXindex = this.transform.SearchComponent<Text>("DMX");
        this.m_BtnValue = button.transform.SearchComponent<Button>("ValueButton");
        if (this.m_BtnValue != null)
        {
            this.m_BtnTextValue = m_BtnValue.GetComponentInChildren<Text>();
        }

        this.m_TxtChannel = this.transform.SearchComponent<Text>("Channel");
        if (this.m_TxtChannel != null)
        {
            this.m_TxtChannel.text = "";
        }
        this.m_BtnSpecial = new ButtonText(button.transform.SearchComponent<Button>("Special"));
        this.m_Border = button.transform.SearchComponent<Image>("Border");
        this.m_BtnActive = this.transform.SearchComponent<Button>("Active");
        this.m_SliderHandleGroupSelected = this.transform.SearchComponent<Image>("Handle");
    }



    public  void InitialiseData(OscAllUsedFixtures.UsedFixtureData fixture, int channelIndex, int dmxIndex, int fixtureIndex, bool assignDataContainerBorderColor)
    {
        if (assignDataContainerBorderColor == true)
        {
            // assign border colour
            if (fixture.m_Color == null)
            {
                int colourIndex = fixtureIndex % DataContainer.Colours.Count;
                if (this.m_Border != null)
                {
                    this.m_Border.color = DataContainer.Colours[colourIndex];
                }
            }
            else
            {
                if (this.m_Border != null)
                {
                    this.m_Border.color = (Color)fixture.m_Color;
                }
            }
        }
        try
        {
            this.m_DmxIndex = dmxIndex;
            this.m_FixtureData = fixture;
            this.m_ChannelIndex = channelIndex;

            var cat = this.m_FixtureData.m_Fixture.Modes[this.m_FixtureData.m_ModeIndex];
            var sliderType = cat.Channels[this.m_ChannelIndex];
            this.m_ChannelName = sliderType.String;
            this.m_CapabilityType = Fixtures.CapabilityTypeConverter.Singleton.Convert(this.m_ChannelName);

            this.m_TxtChannel.text = fixture.m_Fixture.Modes[fixture.m_ModeIndex].Channels[channelIndex].String;           
            this.m_TxtDMXindex.text = dmxIndex.ToString();
            this.m_BtnTextValue.text = 0.ToString();                       
            this.SetValidActive(false);
            this.m_SliderHandleGroupSelected.color = OscControllerRoot.UN_SELECTED;

            UpdateValueText();
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void UpdateValueText()
    {
        var reggex = Regex.Match(m_BtnTextValue.text, @"\d+").Value;
        m_BtnTextValue.text = reggex + m_UseCurveString;
    }

    public void SetUpBasicCallbacks(SliderPanel.ListSliderPrefab m_MultiplySlider, List<SliderPrefab> m_FullSliderPrefabList)
    {
        var shiftCtrl = this.m_Border.gameObject.ForceComponent<ShiftCtrlClick>();
        ShiftCtrlClick(shiftCtrl, this, m_MultiplySlider, m_FullSliderPrefabList);
        shiftCtrl = this.m_Slider.gameObject.ForceComponent<ShiftCtrlClick>();
        ShiftCtrlClick(shiftCtrl, this, m_MultiplySlider, m_FullSliderPrefabList);
        shiftCtrl = this.m_TxtDMXindex.gameObject.ForceComponent<ShiftCtrlClick>();
        ShiftCtrlClick(shiftCtrl, this, m_MultiplySlider, m_FullSliderPrefabList);


        var right = this.m_Border.gameObject.ForceComponent<RightClick>();
        RightClick(right, this);
        right = this.m_Slider.gameObject.ForceComponent<RightClick>();
        RightClick(right, this);
        right = this.m_TxtDMXindex.gameObject.ForceComponent<RightClick>();
        RightClick(right, this);


        var hover = this.m_Slider.gameObject.ForceComponent<MouseHover>();
        hover.onMouseEnter.RemoveAllListeners();
        hover.onMouseEnter.AddListener(() =>
        {
            if (Input.GetMouseButton(0) == false)
            {
                DataContainer.Instance.CurrentHightlightedSliderPrefab = this;
                if (OscControllerRoot.Instance.CurveEditorPanelRef.IsActive == false)
                {
                    Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_SELECT_DMX_MESSAGE, this.m_FixtureData.m_DmxStartIndex.ToString());
                }
                OscControllerRoot.Instance.SliderExtraPanelRef.SetSliderPrefabOnEnter(this);
            }
        });

        hover.onMouseExit.RemoveAllListeners();
        hover.onMouseExit.AddListener(() =>
        {
            if (Input.GetMouseButton(0) == false)
            {
                OscControllerRoot.Instance.SliderExtraPanelRef.SetSliderPrefabOnExit(this);
            }
        });

    }


    private void ShiftCtrlClick(ShiftCtrlClick click, SliderPrefab slider, SliderPanel.ListSliderPrefab m_MultiplySlider, List<SliderPrefab> fullSliderPrefabList)
    {
        click.onCtrlClick.RemoveAllListeners();
        click.onCtrlClick.AddListener(() =>
        {
            if (m_MultiplySlider.Contains(slider) == true)
            {
                m_MultiplySlider.Remove(slider);
            }
            else
            {
                m_MultiplySlider.Add(slider);
            }
        });


        click.onShiftClick.RemoveAllListeners();
        click.onShiftClick.AddListener(() =>
        {
            if (m_MultiplySlider.Contains(slider) == true)
            {
                m_MultiplySlider.Remove(slider);
            }
            else
            {
                
                if(m_MultiplySlider.Count > 0)
                {
                    int lastSelectedIndex = fullSliderPrefabList.FindLastIndex(e => e == m_MultiplySlider.Last());
                    int currentSliderIndex = fullSliderPrefabList.FindLastIndex(e => e == slider);
                    for(int i = lastSelectedIndex; i <= currentSliderIndex; i++)
                    {
                        var currentSlider = fullSliderPrefabList[i];
                        if (m_MultiplySlider.Contains(currentSlider) == false)
                        {
                            m_MultiplySlider.Add(currentSlider);
                        }
                    }
                }
                else
                {
                    m_MultiplySlider.Add(slider);
                }
            }
        });

        click.onNonClick.RemoveAllListeners();
        click.onNonClick.AddListener(() =>
        {
            if (m_MultiplySlider.Contains(slider) == false)
            {
                m_MultiplySlider.Clear();
            }
        });
    }


    private void RightClick(RightClick click, SliderPrefab slider)
    {
        click.onRightClick.RemoveAllListeners();
        click.onRightClick.AddListener(() =>
        {
            Debug.Log("RightClick");
        });
    }
}
