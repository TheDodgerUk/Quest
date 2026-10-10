using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//To change the color to an object, just set the m_SelectedObject through the SetCurrentGameObject
//at line 71

namespace HouseBuilder
{
    public class ColorPickerManager : MonoBehaviour
    {
        public static ColorPickerManager Instance;

        public static Action<Color> OnColourChanged;
        private CanvasGroup m_Group;
        private const int BUTTONS_COUNT = 12;
        private const float PALETTE_PANEL_HEIGHT = 187.4f;
        private bool m_ColorPaletteVisible = false;
        private string m_SelectedColorName;

        private List<Button> m_Buttons = new List<Button>();
        private TMP_InputField m_ColorInputField;
        private Color m_SelectedColor = Color.white;
        private GameObject m_PalettePanel;
        private GameObject m_HsvField;
        private GameObject m_BottomBar;

        private ColorPaletteManager m_ColorPaletteManager;
        private HueManager m_HueManager;

        private Color32[] m_InteriorDefaults = new Color32[]
        {
        new Color32(232,229,222,255),
        new Color32(220,217,210,255),
        new Color32(229,221,200,255),
        new Color32(220,190,163,255),

        new Color32(213,189,189,255),
        new Color32(227,186,156,255),
        new Color32(198,136,121,255),
        //new Color(172f,138f,100f), need 11 because of plus button

        new Color32(142,123,108,255),
        new Color32(89,108,104,255),
        new Color32(107,114,122,255),
        new Color32(82,83,87,255)
        };
        private Color32[] m_FurnitureDefaults = new Color32[]
{
        new Color32(106,167,170,255),
        new Color32(191,52,47,255),
        new Color32(233,211,161,255),
        new Color32(127,79,56,255),

        new Color32(63,56,63,255),
        new Color32(204,199,193,255),
        new Color32(36,65,105,255),
        new Color(188,163,133,255),

        new Color32(240,177,196,255),
        new Color32(204,133,45,255),
        new Color32(235,214,105,255)
        //new Color32(82,83,87,255), need 11 because of plus button
};
        private Color[] m_AmbientLightDefaults = new Color[]
        {
        new Color32(230,234,233,255),
        new Color32(167,194,213,255),
        new Color32(146,161,166,255),
        new Color32(110,133,147,255),

        new Color32(248,235,191,255),
        new Color32(246,225,158,255),
        new Color32(251,209,211,255),
        new Color32(235,178,213,255),

        new Color32(241,152,174,255),
        new Color32(160,129,205,255),
        new Color32(86,85,178,255)
        //new Color32(72,58,145) need 11 because of plus button
        };

        public void Initialise()
        {
            Instance = this;

            m_Group = GetComponent<CanvasGroup>();
            var colorPicker = this.transform;
            var topPanel = colorPicker.transform.Find("TopPanel").gameObject;
            var buttonsGroup = topPanel.transform.Find("Buttons").gameObject;

            for (int i = 0; i < BUTTONS_COUNT; ++i)
            {
                var button = buttonsGroup.transform.Find($"Button{i}").GetComponent<Button>();
                m_Buttons.Add(button);
                if (i != BUTTONS_COUNT - 1)
                {
                    button.onClick.AddListener(() => ChangeSelectedColorViaButton(button.targetGraphic.color));
                }
                else
                {
                    button.onClick.AddListener(ToggleColorPalette);
                }
            }

            m_PalettePanel = colorPicker.transform.Find("PalettePanel").gameObject;
            m_HsvField = m_PalettePanel.transform.Find("HSVField").gameObject;
            m_BottomBar = m_PalettePanel.transform.Find("BottomBar").gameObject;

            m_ColorInputField = m_BottomBar.transform.Find("InputField (TMP)").GetComponent<TMP_InputField>();
            m_ColorInputField.onEndEdit.AddListener(delegate { ChangeSelectedColorViaInputField(); });

            var graphicRaycaster = HBGUIManager.Instance.transform.GetComponent<GraphicRaycaster>();
            m_ColorPaletteManager = m_HsvField.transform.Find("ColorBox Container").AddComponent<ColorPaletteManager>();
            m_ColorPaletteManager.Initialise(this, graphicRaycaster);

            m_HueManager = m_HsvField.transform.Find("Hue").AddComponent<HueManager>();
            m_HueManager.Initialise(this);

            InstantCollapsePalette();
            Active(false);

            SetInteriorDefaultColours();
            HBGUIManager.Instance.WallSettingsMenuRef.OnColourSettingsToggled += OnColourSettingsToggled;
            HBGUIManager.Instance.CeilingSettingsMenuRef.OnColourSettingsToggled += OnColourSettingsToggled;
            HBGUIManager.Instance.FloorSettingsMenuRef.OnColourSettingsToggled += OnColourSettingsToggled;
            HBGUIManager.Instance.ItemSettingsMenuRef.OnColourSettingsToggled += OnColourSettingsToggled;
            //The Time settings menu automatically opens it, TODO: make it use event as well if necessary
        }

        private void OnColourSettingsToggled(bool state)
        {
            if (state)
            {
                InstantCollapsePalette();
            }

            Active(state);
        }

        public void Active(bool state)
        {
            m_Group.VisibleAndInteractive(state);
        }

        public void SetFurnitureDefaultColours()
        {
            for (int i = 0; i < m_Buttons.Count - 1; i++)
            {
                m_Buttons[i].targetGraphic.color = m_FurnitureDefaults[i];
            }
        }

        public void SetInteriorDefaultColours()
        {
            for (int i = 0; i < m_Buttons.Count-1; i++)
            {
                m_Buttons[i].targetGraphic.color = m_InteriorDefaults[i];
            }
        }

        public void SetAmbientLightDefaultColours()
        {
            for (int i = 0; i < m_Buttons.Count - 1; i++)
            {
                m_Buttons[i].targetGraphic.color = m_AmbientLightDefaults[i];
            }
        }

        private void InstantCollapsePalette()
        {
            m_PalettePanel.GetComponent<RectTransform>().SetHeight(0);
            m_HsvField.SetActive(false);
            m_BottomBar.SetActive(false);
            m_ColorPaletteVisible = false;
        }

        //leaving this in here in case event doesnt work
        public Color GetSelectedColour()
        {
            return m_SelectedColor;
        }

        private void ChangeSelectedColorViaButton(Color color)
        {
            m_SelectedColor = color;
            OnColourChanged?.Invoke(m_SelectedColor);
            HouseBuilderRoot.Instance.RecorderRef.Record();
        }

        public void ChangeSelectedColorViaColorPalette(Color color)
        {
            m_ColorInputField.text = ColorUtility.ToHtmlStringRGB(color);
            m_SelectedColor = color;
            OnColourChanged?.Invoke(m_SelectedColor);
        }

        private void ChangeSelectedColorViaInputField()
        {
            m_SelectedColorName = "#" + m_ColorInputField.text.ToString();

            ColorUtility.TryParseHtmlString(m_SelectedColorName, out m_SelectedColor);
            OnColourChanged?.Invoke(m_SelectedColor);
            HouseBuilderRoot.Instance.RecorderRef.Record();
        }

        private void ToggleColorPalette()
        {
            if (!m_ColorPaletteVisible)
                ResizeColorPalettePanel(0, PALETTE_PANEL_HEIGHT);
            else
                ResizeColorPalettePanel(PALETTE_PANEL_HEIGHT, 0);
        }

        private void ResizeColorPalettePanel(float startHeight, float endHeight)
        {
            RectTransform paletteTransform = m_PalettePanel.GetComponent<RectTransform>();
            if (m_ColorPaletteVisible == true)
            {
                ActivatePaletteComponent(false);
            }

            this.Create<ValueTween>(0.2f, EaseType.Linear, () =>
            {
                if (m_ColorPaletteVisible == false)
                {
                    ActivatePaletteComponent(true);
                }
                m_ColorPaletteVisible = !m_ColorPaletteVisible;
            }).Initialise(startHeight, endHeight, (f) =>
            {
                paletteTransform.SetHeight(f);
            });
        }

        private void ActivatePaletteComponent(bool state)
        {
            m_HsvField.SetActive(state);
            m_BottomBar.SetActive(state);
        }

        public void UpdateColorPalette(Color color)
        {
            m_ColorPaletteManager.UpdateGradientTexture(color);
        }
    }
}