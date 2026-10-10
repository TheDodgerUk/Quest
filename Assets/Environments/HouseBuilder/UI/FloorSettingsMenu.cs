using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ContentSteamAudioMetaData;

namespace HouseBuilder
{
    public class FloorSettingsMenu : StructureSettingsMenu
    {
        private InteractionWallGeneratedWhole m_CurrentData;
        private TMP_Dropdown m_MaterialDropDown;

        public override void Initialise()
        {
            base.Initialise();

            HouseBuilderRoot.Instance.OnFloorSelected += OnFloorSelected;

            m_BtnDelete.SetActive(false);
            m_BtnDelete.interactable = false; //until i figure out a good way of doing this


            m_MaterialDropDown = transform.Find("MaterialDropDown").GetComponent<TMP_Dropdown>();
            List<string> options = new List<string>();
            var values = Enum.GetNames(typeof(MaterialPreset));
            for (int i = 0; i < values.Length - 1; i++)
            {
                options.Add(values[i]);
            }
            m_MaterialDropDown.ClearOptions();
            m_MaterialDropDown.AddOptions(options);
            m_MaterialDropDown.onValueChanged.AddListener((value) => SetSteamMaterail(value));
        }

        private void SetSteamMaterail(int value)
        {
            Debug.LogError("SetMaterail still not setting actual materail yet ");
            m_CurrentData.FloorPlanJsonDataRef.m_FloorData.m_SteamAudioMaterial = (SteamAudio.MaterialPreset)(MaterialPreset)value;
            Refresh(m_CurrentData.ThisWallIndex);
        }

        private void OnFloorSelected(bool state, InteractionWallGeneratedWhole data, bool clearAll)
        {
            SetVisible(state, false);

            if (!state || clearAll)
            {
                m_CurrentData = null;
                return;
            }
            else
            {
                m_CurrentData = data;
                if (m_UseNonItemSelected == true)
                {
                    m_CurrentData.FloorPlanJsonDataRef.m_FloorData.m_MaterialName = m_NonItemSelecetedMaterial;
                    m_MaterialDropDown.SetValueWithoutNotify((int)m_CurrentData.FloorPlanJsonDataRef.m_FloorData.m_SteamAudioMaterial);
                    Refresh(m_CurrentData.ThisWallIndex);
                    ClearNonItemMaterial();
                }
            }
            m_TglShow.isOn = m_CurrentData.FloorPlanJsonDataRef.m_RoomData.m_HasFloor;
        }

        public override void OnColourPicked(Color col)
        {
            m_CurrentData.FloorPlanJsonDataRef.m_FloorData.m_MaterialColour = col;

            Refresh(m_CurrentData.ThisWallIndex);
        }

        public override void OnMaterialPicked(CatalogueMenu.MenuContents menuContents, string materialName, string filter)
        {
            if (menuContents == CatalogueMenu.MenuContents.FloorMaterials)
            {
                var floor = m_CurrentData;
                if (floor != null)
                {
                    Core.AssetBundlesRef.MaterialAssetBundleRef.GetItem(Core.Mono, materialName, (mat) =>
                    {
                        floor.FloorPlanJsonDataRef.m_FloorData.m_MaterialName = materialName;
                        floor.FloorPlanJsonDataRef.m_FloorData.m_MaterialColour = mat.color;
                        Refresh(floor.ThisWallIndex);
                    });
                }
                else
                {
                    SetNonItemMaterial(materialName);
                }
            }
        }

        private void Refresh(int index)
        {
            var root = HouseBuilderRoot.Instance;

            root.InputCeiling.ClearAllInteractions();
            root.RefreshFloor(() =>
            {
                var currentItem = root.InputCeiling.FloorModeContainerRefSolid.GetList[index];
                root.InputCeiling.AddCurrentInteraction(currentItem);

            });
        }

        public override void Delete()
        {
            Debug.LogError("Deleted Selected floor: " + m_CurrentData);
            //something something delete the floor,clear interactions
            //HouseBuilderRoot.Instance.DeleteFloor
            //This here because when Delete button is clicked, mouse is on GUI. Possibly not the best place for this, should be triggered by Deselecting a wall but gui raycast check blocks it it
            // SetActive(false, true);
        }

        public override void Display(bool state)
        {       
            HouseBuilderRoot.Instance.InputCeiling.ToggleFloorVisibility(m_CurrentData.ThisWallIndex, state);
        }
    }
}