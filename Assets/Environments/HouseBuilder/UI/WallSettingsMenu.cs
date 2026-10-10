using SteamAudio;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class CombinedDataIndex
    {
        public InteractionWallGeneratedWhole m_Data;
        public int m_Index;
    }

    public class WallSettingsMenu : StructureSettingsMenu
    {
        private TMP_InputField m_InputHeight;
        private TMP_InputField m_InputDepth;
        private TMP_InputField m_InputCurve;
        private TextMeshProUGUI m_HeightUnitLabel;
        private TextMeshProUGUI m_DepthUnitLabel;
        private TMP_Dropdown m_MaterialDropDown;
        private Toggle m_TglDetach;
        private Button m_BtnReset;

        protected CombinedDataIndex m_CurrentData = new CombinedDataIndex();

        public override void Initialise()
        {
            base.Initialise();
            m_TglDetach = transform.Find("HeadBtns/TglDetach").GetComponent<Toggle>();
            m_BtnReset = transform.Find("HeadBtns/BtnReset").GetComponent<Button>();

            m_InputHeight = transform.Find("InputHeight").GetComponent<TMP_InputField>();
            m_InputDepth = transform.Find("InputDepth").GetComponent<TMP_InputField>();
            m_InputCurve = transform.Find("InputCurve").GetComponent<TMP_InputField>();
            m_HeightUnitLabel = transform.Find("HeightUnit").GetComponent<TextMeshProUGUI>();
            m_DepthUnitLabel = transform.Find("DepthUnit").GetComponent<TextMeshProUGUI>();
            m_MaterialDropDown = transform.Find("MaterialDropDown").GetComponent<TMP_Dropdown>();



            // adds all but Custom which is at the end 
            List<string> options = new List<string>();
            var values = Enum.GetNames(typeof(MaterialPreset));
            for(int i = 0; i < values.Length -1; i++)
            {
                options.Add(values[i]);
            }
            m_MaterialDropDown.ClearOptions();
            m_MaterialDropDown.AddOptions(options);


            m_TglDetach.onValueChanged.AddListener((value) => DetachWall(value));
            m_BtnReset.onClick.AddListener(() => ResetWallToDefault());
            m_InputHeight.onEndEdit.AddListener((value) => SetWallHeight(value));
            m_InputDepth.onEndEdit.AddListener((value) => SetWallDepth(value));
            m_InputCurve.onEndEdit.AddListener((value) => SetWallCurve(value));
            m_MaterialDropDown.onValueChanged.AddListener((value) => SetWallMaterail(value));

            m_BtnDelete.SetActive(false);
            HouseBuilderRoot.Instance.OnWallSelected += OnWallSelected;
        }

        private void OnWallSelected(bool state, int index, InteractionWallGeneratedWhole data, bool clearAll)
        {
            SetVisible(state, false);

            if (state == true)
            {
                CombinedDataIndex item = new CombinedDataIndex();
                item.m_Data = data;
                item.m_Index = index;
                m_CurrentData = item;
                m_TglDetach.isOn = item.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_UseOverrideData;

                if (m_UseNonItemSelected == true)
                {
                    UsePrevSelectedMaterial();
                }

                ConvertDisplayUnits();
            }
            else
            {
                m_CurrentData = null;
            }
        }

        private void UsePrevSelectedMaterial()
        {
            var roomData = m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData;

            if (m_TglDetach.isOn == true)
            {
                roomData.m_RoomWalls[m_CurrentData.m_Index].m_OverrideData.m_MaterialName = m_NonItemSelecetedMaterial;
                roomData.m_RoomWalls[m_CurrentData.m_Index].m_OverrideData.m_MaterialColour = m_NonItemSelecetedColour;
            }
            else
            {
                roomData.m_MaterialName = m_NonItemSelecetedMaterial;
                roomData.m_MaterialColour = m_NonItemSelecetedColour;
            }

            ClearNonItemMaterial();

            var root = HouseBuilderRoot.Instance;
            root.ShowAll(() =>
            {
                Refresh();
                root.ReselectWalls(m_CurrentData);
            });
        }

        public override void OnColourPicked(Color col)
        {
            var roomData = m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData;

            if (roomData.m_RoomWalls[m_CurrentData.m_Index].m_UseOverrideData == true)
            {
                roomData.m_RoomWalls[m_CurrentData.m_Index].m_OverrideData.m_MaterialColour = col;
            }
            else
            {
                roomData.m_MaterialColour = col;
            }

            var root = HouseBuilderRoot.Instance;
            root.ShowAll(() =>
            {
                Refresh();
                root.ReselectWalls(m_CurrentData);
            });
        }

        public override void OnMaterialPicked(CatalogueMenu.MenuContents menuContents, string materialName, string filter)
        {
            if (menuContents == CatalogueMenu.MenuContents.WallMaterials)
            {
                var root = HouseBuilderRoot.Instance;
                if (m_CurrentData != null)
                {
                    Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(Core.Mono, materialName, (mat) =>
                    {
                        var sepWall = m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index];
                        if (sepWall.m_UseOverrideData == false)
                        {
                            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_MaterialColour = mat.color;
                            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_MaterialName = materialName;
                        }
                        else
                        {
                            sepWall.m_OverrideData.m_MaterialColour = mat.color;
                            sepWall.m_OverrideData.m_MaterialName = materialName;
                        }

                        root.ShowAll(() =>
                        {
                            Refresh();
                            root.ReselectWalls(m_CurrentData);
                        });
                    });
                }
                else
                {
                    SetNonItemMaterial(materialName);
                }
            }
        }

        private void DetachWall(bool state)
        {
            Debug.Log($"DetachWall {state}");

            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_UseOverrideData = state;
        }

        private void ResetWallToDefault()
        {
            var data = m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index];
            data.m_BendAmount = 0;
            data.m_OverrideData.m_MaterialName = null;
            data.m_UseOverrideData = false;
            data.m_RenderThisWall = true;
            data.m_EnableThisWall = true;

            HouseBuilderRoot.Instance.ShowAll(() =>
            {
                Refresh();
                HouseBuilderRoot.Instance.ReselectWalls(m_CurrentData);
            });
        }

        public override void Delete()
        {
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_RenderThisWall = false;
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_EnableThisWall = false;

            Debug.LogError("Deleted Selected Wall: " + m_CurrentData);
            HouseBuilderRoot.Instance.DeleteWall(ref m_CurrentData);
            //This here because when Delete button is clicked, mouse is on GUI. Possibly not the best place for this, should be triggered by Deselecting a wall but gui raycast check blocks it it
            SetVisible(false, true);
        }

        public override void Display(bool state)
        {
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_RenderThisWall = state;
            HouseBuilderRoot.Instance.ShowAll(() =>
            {
                Refresh();
                HouseBuilderRoot.Instance.ReselectWalls(m_CurrentData);
            });
        }

        private void SetWallHeight(string value)
        {
            float newHeight = float.Parse(value, CultureInfo.InvariantCulture);
            newHeight = MeasurementManager.ConvertFromMeters(newHeight);
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_Height = newHeight;

            HouseBuilderRoot.Instance.ShowAll(() =>
            {
                Refresh();
                HouseBuilderRoot.Instance.ReselectWalls(m_CurrentData);
            });
        }

        //Also known as thickness
        private void SetWallDepth(string value)
        {
            float newDepth = float.Parse(value, CultureInfo.InvariantCulture);

            newDepth = MeasurementManager.ConvertFromMeters(newDepth);
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_Depth = newDepth;

            HouseBuilderRoot.Instance.ShowAll(() =>
            {
                Refresh();
                HouseBuilderRoot.Instance.ReselectWalls(m_CurrentData);
            });
        }

        private void SetWallMaterail(int dropdownIndex)
        {
            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_SteamAudioMaterial = (MaterialPreset)dropdownIndex;
        }

        private void SetWallCurve(string value)
        {
            float newCurve = float.Parse(value, CultureInfo.InvariantCulture);

            m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData.m_RoomWalls[m_CurrentData.m_Index].m_BendAmount = newCurve;

            HouseBuilderRoot.Instance.ShowAll(() =>
            {
                Refresh();
                HouseBuilderRoot.Instance.ReselectWalls(m_CurrentData);
            });
        }

        private void Refresh()
        {
            HouseBuilderRoot.Instance.InputCeiling.ToggleTemporaryAllCeilingVisibility(false);
            HouseBuilderRoot.Instance.InputCeiling.ToggleTemporaryAllFloorVisibility(true);
        }

        public void ConvertDisplayUnits()
        {
            if (m_Group.interactable) // This check will allow the toggle for measurement units to update the values while the menu is opened
            {
                SetUnitsOfMeasurement(MeasurementManager.ShortMeasurementType());

                if (m_CurrentData.m_Data != null)
                {
                    var roomData = m_CurrentData.m_Data.FloorPlanJsonDataRef.m_RoomData;
                    string convertedHeight = MeasurementManager.ConvertMeasureClean(roomData.m_Height);
                    string convertedDepth = MeasurementManager.ConvertMeasureClean(roomData.m_Depth);

                    m_InputHeight.SetTextWithoutNotify(convertedHeight);
                    m_InputDepth.SetTextWithoutNotify(convertedDepth);

                    var data = roomData.m_RoomWalls[m_CurrentData.m_Index];
                    m_InputCurve.SetTextWithoutNotify(data.m_BendAmount.ToString());
                    m_TglShow.SetIsOnWithoutNotify(data.m_RenderThisWall);
                    m_TglDetach.SetIsOnWithoutNotify(data.m_UseOverrideData);
                    m_MaterialDropDown.SetValueWithoutNotify((int)roomData.m_SteamAudioMaterial);
                }
            }
        }

        private void SetUnitsOfMeasurement(string units)
        {
            m_HeightUnitLabel.SetText(units);
            m_DepthUnitLabel.SetText(units);
            //Curve unit is always "deg" (degrees)
        }
    }
}