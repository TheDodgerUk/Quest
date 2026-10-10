using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class SceneSettingsMenu : MonoBehaviour
    {
        public const string SWAP_CAMERA_CONTROLS = "ROOM_BUILDER_SWAP_CAMERA_CONTROLS";
        public enum TimeFormat
        {
            AM,
            PM
        }
        private CanvasGroup m_Group;

        private Button m_BtnResetToDefaults;

        private TMP_Dropdown m_DropdownUnits;

        private TMP_InputField m_InputSpacing;
        private TextMeshProUGUI m_SpacingUnit;
        private TextMeshProUGUI m_InputSpacingPlaceholder;
        private Toggle[] m_GridSizes = new Toggle[4];

        private Toggle m_TglAM;
        private Toggle m_TglPM;
        private Toggle m_TglSwapCameraControls;

        private TextMeshProUGUI m_SettingTimeLabel;
        private Button m_BtnTimeIncrement;
        private Button m_BtnTimeDecrement;
        private int m_CurrentTime = 12;
        private TimeFormat m_CurrentFormat = TimeFormat.AM; //starting state for the AM/PM Toggles is AM is on and PM is off. Should match with this initial value.

        private float m_CurrentSpacing = 1f; // is actualy spacing
        private int m_CurrentSize = 0;
        private Button m_BtnClose;
        private Toggle m_TglGrid;
        private Toggle m_MenuToggle;

        private const int DEFAULT_GRID_SPACING = 1;

        public bool m_IsVisible = false;

        private const float MINIMUM_GRID_SIZE_METERS = 0.5f;

        public void Initialise(Toggle tglGrid, Toggle menuToggle)
        {
            m_Group = gameObject.GetComponent<CanvasGroup>();

            m_TglGrid = tglGrid;
            m_MenuToggle = menuToggle;

            m_BtnResetToDefaults = transform.Find("BtnReset").GetComponent<Button>();
            m_DropdownUnits = transform.Find("UnitsDropdown").GetComponent<TMP_Dropdown>();
            m_InputSpacing = transform.Find("InputSpacing").GetComponent<TMP_InputField>();
            m_SpacingUnit = transform.Find("GridSpacingUnit/SpacingUnit").GetComponent<TextMeshProUGUI>();

            m_TglAM = transform.Find("ToggleTime/TglAM").GetComponent<Toggle>();
            m_TglPM = transform.Find("ToggleTime/TglPM").GetComponent<Toggle>();
            m_TglSwapCameraControls = transform.Find("TglCameraControls/TglCameraOn").GetComponent<Toggle>();

            m_SettingTimeLabel = transform.Find("TimeOfDay/Time").GetComponent<TextMeshProUGUI>();
            m_BtnTimeIncrement = transform.Find("TimeOfDay/ArrowUp").GetComponent<Button>();
            m_BtnTimeDecrement = transform.Find("TimeOfDay/ArrowDown").GetComponent<Button>();

            m_GridSizes[0] = transform.Find("AreaSize/TglSSize").GetComponent<Toggle>();
            m_GridSizes[1] = transform.Find("AreaSize/TglMSize").GetComponent<Toggle>();
            m_GridSizes[2] = transform.Find("AreaSize/TglLSize").GetComponent<Toggle>();
            m_GridSizes[3] = transform.Find("AreaSize/TglXLSize").GetComponent<Toggle>();

            m_BtnClose = transform.Find("BtnClose").GetComponent<Button>();

            m_BtnResetToDefaults.onClick.AddListener(() => ResetSettingsToDefaults());
            m_DropdownUnits.onValueChanged.AddListener((value) => SetUnitsOfMeasurement(value));
            m_InputSpacingPlaceholder = m_InputSpacing.placeholder.GetComponent<TextMeshProUGUI>();
            m_InputSpacingPlaceholder.SetText(m_CurrentSpacing.ToString() + ".00");

            m_InputSpacing.onEndEdit.AddListener((value) => SetGridSpacing(value));

            m_GridSizes[0].onValueChanged.AddListener((value) => SetGridSize(value, 0));
            m_GridSizes[1].onValueChanged.AddListener((value) => SetGridSize(value, 1));
            m_GridSizes[2].onValueChanged.AddListener((value) => SetGridSize(value, 2));
            m_GridSizes[3].onValueChanged.AddListener((value) => SetGridSize(value, 3));

            m_TglAM.onValueChanged.AddListener((value) => SetFormat(TimeFormat.AM, value));
            m_TglPM.onValueChanged.AddListener((value) => SetFormat(TimeFormat.PM, value));

            m_TglSwapCameraControls.SetIsOnWithoutNotify(Convert.ToBoolean(PlayerPrefs.GetInt(SWAP_CAMERA_CONTROLS, 1)));
            m_TglSwapCameraControls.onValueChanged.AddListener((value) => SwapCameraControls(value));
           

            m_BtnTimeIncrement.onClick.AddListener(() => IncrementTime());
            m_BtnTimeDecrement.onClick.AddListener(() => DecrementTime());

            m_TglGrid.onValueChanged.AddListener((value) => ToggleGrid(value));

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            m_BtnClose.onClick.AddListener(() => SetActive(false));

        }


        public bool IsCameraControlsSwapped()
        {
            return m_TglSwapCameraControls.isOn;
        }

        private void SwapCameraControls(bool value)
        {
            PlayerPrefs.SetInt(SWAP_CAMERA_CONTROLS, Convert.ToInt32(value));
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            SetActive(false); //for not is off by default ,would be good to discuss where it would be shown except settings button
            Refresh();
            FloorAndSkyboxManager.Instance.ToggleCompass(false);
            //If you turn it on when its on, it won't change the value and thus won't trigger your event.
        }

        public void SetActive(bool state)
        {
            HouseBuilderRoot.Instance.AllowMouseInteractionBecauseOfGui = !state;
            m_Group.VisibleAndInteractive(state);
            m_IsVisible = state;

            if (state)
            {
                ColorPickerManager.Instance.SetAmbientLightDefaultColours();
                ColorPickerManager.OnColourChanged = FloorAndSkyboxManager.Instance.SetAmbientLight;
            }
            else
            {
                ColorPickerManager.OnColourChanged = null;
                m_MenuToggle.isOn = false;
            }

            ColorPickerManager.Instance.Active(state);
            FloorAndSkyboxManager.Instance.ToggleCompass(state);
        }

        #region UnitsOfMeasurement

        private void SetUnitsOfMeasurement(int value)
        {
            switch (value)
            {
                case 0:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.Meters;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.Meters.ToString());
                    break;

                case 1:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.Centimeters;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.Centimeters.ToString());
                    break;

                case 2:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.Millimeters;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.Millimeters.ToString());
                    break;

                case 3:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.FeetAndInches;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.FeetAndInches.ToString());
                    break;

                case 4:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.Feet;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.Feet.ToString());
                    break;

                case 5:
                    MeasurementManager.MeasurementTypeEnum = MeasurementManager.Unit.Inches;
                    m_SpacingUnit.SetText(MeasurementManager.Unit.Inches.ToString());
                    break;

                default:
                    break;
            }

            HBGUIManager.Instance.WallSettingsMenuRef.ConvertDisplayUnits();

            string converted = MeasurementManager.ConvertMeasureClean(m_CurrentSpacing);
            m_InputSpacing.SetTextWithoutNotify(converted);
        }

        #endregion UnitsOfMeasurement

        #region Grid

        public void ToggleGrid() => m_TglGrid.isOn = !m_TglGrid.isOn;

        private void ToggleGrid(bool state)
        {
            PositionGrid.Instance.SetCurrentGridState(state);
        }

        private void ResetSettingsToDefaults()
        {
            m_DropdownUnits.value = 0;
            m_GridSizes[2].isOn = true;
            SetGridSpacing(DEFAULT_GRID_SPACING.ToString());
            m_InputSpacing.text =  DEFAULT_GRID_SPACING.ToString() + ".00";
            m_CurrentSpacing = DEFAULT_GRID_SPACING;
        }

        private void SetGridSize(bool state, int index)
        {
            if (state)
            {
                m_CurrentSize = index;
                FloorAndSkyboxManager.Instance.SelectTheFloor(m_CurrentSize);
            }
        }

        public void LoadFromFile(HouseBuilderConstants.GridInfo info, int floorIndex)
        {
            m_TglGrid.isOn = info.m_ShowGrid;
            m_GridSizes[floorIndex].isOn = true;
            PositionGrid.Instance.SetGridScale(info.m_Spacing);
        }

        private void SetGridSpacing(string value)
        {
            if (float.TryParse(value, out float spacing) == true)
            {
                float convertedInput = MeasurementManager.ConvertToMeters(spacing, MeasurementManager.MeasurementTypeEnum);
                if(convertedInput >= MINIMUM_GRID_SIZE_METERS)
                {
                    m_CurrentSpacing = convertedInput;
                    PositionGrid.Instance.SetGridScale(m_CurrentSpacing);
                }
                else
                {
                    m_InputSpacing.text = m_CurrentSpacing.ToString();
                }
            }
        }

        #endregion Grid

        #region TimeOfDay

        private void SetFormat(TimeFormat format, bool state)
        {
            if (state)
            {
                m_CurrentFormat = format;

                Refresh();
            }
        }

        private void IncrementTime()
        {
            m_CurrentTime++;

            if (m_CurrentTime == 13)
            {
                m_CurrentTime = 1;
                if (m_CurrentFormat == TimeFormat.AM)
                {
                    m_TglPM.isOn = true;
                }
                else
                {
                    m_TglAM.isOn = true;
                }
            }

            Refresh();
        }

        private void DecrementTime()
        {
            m_CurrentTime--;

            if (m_CurrentTime == 0)
            {
                m_CurrentTime = 12;
                if (m_CurrentFormat == TimeFormat.AM)
                {
                    m_TglPM.isOn = true;
                }
                else
                {
                    m_TglAM.isOn = true;
                }
            }

            Refresh();
        }

        private void Refresh()
        {
            m_SettingTimeLabel.SetText(m_CurrentTime + ":00");

            int currentTime = m_CurrentTime;
            if (m_CurrentFormat == TimeFormat.PM)
            {
                currentTime += 12;
            }
            FloorAndSkyboxManager.Instance.SetTimeOfDay(currentTime);
        }

        public void SetTimeViaFile(int time, TimeFormat format)
        {
            if (m_CurrentFormat != format)
            {
                if (format == TimeFormat.AM)
                {
                    m_TglAM.isOn = true;
                }
                else
                {
                    m_TglPM.isOn = true;
                }
            }

            m_CurrentTime = Mathf.Clamp(time, 0, 12);

            Refresh();
        }

        public void GetTimeForFile(ref int time, ref TimeFormat format)
        {
            time = m_CurrentTime;
            format = m_CurrentFormat;
        }

        #endregion TimeOfDay
    }
}