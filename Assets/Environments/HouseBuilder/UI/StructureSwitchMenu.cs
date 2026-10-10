using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public enum StructureType
    {
        None,
        Ceiling,
        Openings,
        Walls,
        Floor,
        Staging
    }

    public class StructureSwitchMenu : MonoBehaviour
    {
        public event Action<StructureType> OnStructureSwitchChanged;

        private CanvasGroup m_Group;
        private ToggleGroup m_ToggleGroup;
        private const int COUNT_OF_TOGGLES = 4;
        private Toggle[] m_Toggles = new Toggle[COUNT_OF_TOGGLES];
        private StructureType m_CurrentSelection = StructureType.None;

        private string m_SelectedStructure = null;

        public void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>();
            m_ToggleGroup = transform.Find("ToggleGroup").GetComponent<ToggleGroup>();

            var toggleParent = transform.Find("ToggleGroup");
            for (int count = 0; count < COUNT_OF_TOGGLES; ++count)
            {
                var toggle = toggleParent.GetChild(count);
                m_Toggles[count] = toggle.GetComponent<Toggle>();
                m_Toggles[count].onValueChanged.AddListener((value) => SwitchStructure(toggle.name));
            };

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            HouseBuilderRoot.Instance.OnModeTransitionBegin += OnModeTransitionBegin;
            HouseBuilderRoot.Instance.OnStructureModeSelected += OnStructureModeSelected;
        }

        private void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);

            if(state == false)
            {
                m_CurrentSelection = StructureType.None;
                OnStructureSwitchChanged?.Invoke(m_CurrentSelection);
            }
            else
            {
                DeselectCurrentOption();
            }
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }

        private void OnStructureModeSelected()
        {
            SetVisible(true);
        }

        private void OnModeTransitionBegin()
        {
            SetVisible(false);
        }

        public void ForceEditCeiling()
        {
            m_Toggles[0].isOn = true;
        }

        public void ForceEditOpenings()
        {
            m_Toggles[1].isOn = true;
        }

        public void ForceEditWalls()
        {
            m_Toggles[2].isOn = true;
        }

        public void ForceEditFloor()
        {
            m_Toggles[3].isOn = true;
        }
        private void SwitchStructure(string selectedStructure)
        {
            if(selectedStructure == m_SelectedStructure)
            {
                m_SelectedStructure = null;
                m_CurrentSelection = StructureType.None;

                if (MaterialSelectionGUI.Instance.IsSelectionGuiEnabled())
                {
                    MaterialSelectionGUI.Instance.ForceClearSelectedMaterial();
                }
            }
            else
            {
                m_SelectedStructure = selectedStructure;
                switch (selectedStructure)
                {
                    case "TglCeiling":
                        m_CurrentSelection = StructureType.Ceiling;
                        break;
                    case "TglOpenings":
                        m_CurrentSelection = StructureType.Openings;
                        break;
                    case "TglWalls":
                        m_CurrentSelection = StructureType.Walls;
                        break;
                    case "TglFloor":
                        m_CurrentSelection = StructureType.Floor;
                        break;
                }
            }
            OnStructureSwitchChanged?.Invoke(m_CurrentSelection);
        }

        public void DeselectCurrentOption()
        {
            if (m_ToggleGroup.AnyTogglesOn())
                m_ToggleGroup.SetAllTogglesOff();
        }
    }
}