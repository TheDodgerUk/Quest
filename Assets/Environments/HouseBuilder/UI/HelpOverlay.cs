using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class HelpOverlay : MonoBehaviour
    {
        private enum Switch
        {
            None,
            Structure,
            Visibility
        }
        private CanvasGroup m_Group;
        private CanvasGroup m_GenericTips;
        private GameObject m_StructureSwitch;
        private GameObject m_VisibilitySwitch;
        private GameObject m_ViewMode;
        private GameObject m_ItemSettings;

        private GameObject m_RotateItemSetting;
        private GameObject m_ExpandItemSetting;

        private CanvasGroup m_Full3DPlot;
        private CanvasGroup m_Full3DStructure;
        private CanvasGroup m_Full3DFurniture;

        private CanvasGroup m_TopdownPlot;
        private CanvasGroup m_TopdownStructure;
        private CanvasGroup m_TopdownFurniture;

        private CanvasGroup m_FirstPersonPlot;
        private CanvasGroup m_FirstPersonStructure;
        private CanvasGroup m_FirstPersonFurniture;

        private bool m_IsGroupActive;
        public bool m_IsVisible => m_IsGroupActive;
        private List<InteractionGameObject> m_CurrenteSelection;
        private bool m_ItemIsRotatable = false;
        private bool m_ItemIsModular = false;


        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>();
            m_IsGroupActive = true;

            m_GenericTips = transform.Find("Generic").GetComponent<CanvasGroup>();
            m_StructureSwitch = m_GenericTips.transform.Find("StructureSwitch").gameObject;
            m_VisibilitySwitch = m_GenericTips.transform.Find("VisibilitySwitch").gameObject;
            m_ItemSettings = m_GenericTips.transform.Find("ItemSettings").gameObject;
            m_ViewMode = m_GenericTips.transform.Find("ViewMode").gameObject;

            m_RotateItemSetting = m_ItemSettings.transform.Find("Rotate").gameObject;
            m_ExpandItemSetting = m_ItemSettings.transform.Find("Expand").gameObject;

            var full3D = transform.Find("Full 3D");
            m_Full3DPlot = full3D.transform.Find("Plot").GetComponent<CanvasGroup>();
            m_Full3DStructure = full3D.transform.Find("Structure").GetComponent<CanvasGroup>();
            m_Full3DFurniture = full3D.transform.Find("Furniture").GetComponent<CanvasGroup>();

            var topdown = transform.Find("Topdown View");
            m_TopdownPlot = topdown.transform.Find("Plot").GetComponent<CanvasGroup>();
            m_TopdownStructure = topdown.transform.Find("Structure").GetComponent<CanvasGroup>();
            m_TopdownFurniture = topdown.transform.Find("Furniture").GetComponent<CanvasGroup>();

            var firstPerson = transform.Find("First Person View");
            m_FirstPersonPlot = firstPerson.transform.Find("Plot").GetComponent<CanvasGroup>();
            m_FirstPersonStructure = firstPerson.transform.Find("Structure").GetComponent<CanvasGroup>();
            m_FirstPersonFurniture = firstPerson.transform.Find("Furniture").GetComponent<CanvasGroup>();

            SetVisible(false);//hide by default

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        public void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);
            m_IsGroupActive = state;

            HideAllKeybinds();

            if (state)
            {
                ShowKeybindsPage();
                var mode = HouseBuilderRoot.Instance.CurrentMode;
                switch (mode)
                {
                    case BuilderMode.Plot:
                        ToggleSwitchOverlay(Switch.None);
                        break;
                    case BuilderMode.Structure:
                        ToggleSwitchOverlay(Switch.Structure);
                        break;
                    case BuilderMode.Placement:
                        ToggleSwitchOverlay(Switch.Visibility);
                        break;
                    default:
                        Debug.LogError($"{mode} not found");
                        break;
                }
            }
            else
            {
                ToggleSwitchOverlay(Switch.None);
            }
        }

        private void OnEnvironmentLoadingComplete()
        {
            HouseBuilderRoot.Instance.OnModeTransitionEnd += RefreshOnModeTransition;
            HBGUIManager.Instance.m_ViewMode.OnViewChanged += RefreshOnModeTransition;

            HouseBuilderRoot.Instance.OnObjectSelected += (state, data) =>
            {
                m_CurrenteSelection = data;
                ToggleAvailableItemEditingOptions(state);
            };
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }

        private void RefreshOnModeTransition()
        {
            if (m_IsGroupActive)
            {
                HideAllKeybinds();
                ShowKeybindsPage();

                var mode = HouseBuilderRoot.Instance.CurrentMode;
                switch (mode)
                {
                    case BuilderMode.Plot:
                        ToggleSwitchOverlay(Switch.None);
                        break;
                    case BuilderMode.Structure:
                        ToggleSwitchOverlay(Switch.Structure);
                        break;
                    case BuilderMode.Placement:
                        ToggleSwitchOverlay(Switch.Visibility);
                        break;
                    default:
                        Debug.LogError($"{mode} not found");
                        break;
                }
                ToggleAvailableItemEditingOptions(false);
            }
        }
        private void ToggleSoloGroup(CanvasGroup group, bool state)
        {
            group.VisibleAndInteractive(state);
            group.interactable = false; //no clicky on it
            group.blocksRaycasts = false;
        }

        private void HideAllKeybinds()
        {
            ToggleSoloGroup(m_Full3DPlot, false);
            ToggleSoloGroup(m_Full3DStructure, false);
            ToggleSoloGroup(m_Full3DFurniture, false);
            ToggleSoloGroup(m_TopdownPlot, false);
            ToggleSoloGroup(m_TopdownStructure, false);
            ToggleSoloGroup(m_TopdownFurniture, false);
            ToggleSoloGroup(m_FirstPersonPlot, false);
            ToggleSoloGroup(m_FirstPersonStructure, false);
            ToggleSoloGroup(m_FirstPersonFurniture, false);
        }

        private void ToggleSwitchOverlay(Switch visibleSwitch)
        {
            m_StructureSwitch.SetActive(false);
            m_VisibilitySwitch.SetActive(false);
            m_ViewMode.SetActive(false);
            ToggleAvailableItemEditingOptions(false);
            switch (visibleSwitch)
            {
                case Switch.None:
                    break;

                case Switch.Structure:
                    m_StructureSwitch.SetActive(true);
                    m_ViewMode.SetActive(true);
                    ToggleAvailableItemEditingOptions(HBGUIManager.Instance.ItemSettingsMenuRef.m_IsGroupActive);
                    break;

                case Switch.Visibility:
                    m_VisibilitySwitch.SetActive(true);
                    m_ViewMode.SetActive(true);
                    ToggleAvailableItemEditingOptions(HBGUIManager.Instance.ItemSettingsMenuRef.m_IsGroupActive);
                    break;
            }
        }

        private void ShowKeybindsPage()
        {
            var mode = HouseBuilderRoot.Instance.CurrentMode;
            var view = HBGUIManager.Instance.m_ViewMode.CurrentView;
            switch (view)
            {
                case View.Full3D:
                    switch (mode)
                    {
                        case BuilderMode.Plot:
                            ToggleSoloGroup(m_Full3DPlot, true);
                            break;

                        case BuilderMode.Structure:
                            ToggleSoloGroup(m_Full3DStructure, true);
                            break;

                        case BuilderMode.Placement:
                            ToggleSoloGroup(m_Full3DFurniture, true);
                            break;
                        default:
                            Debug.LogError($"{mode} not found");
                            break;
                    }

                    break;

                case View.Topdown:
                    switch (mode)
                    {
                        case BuilderMode.Plot:
                            ToggleSoloGroup(m_TopdownPlot, true);
                            break;

                        case BuilderMode.Structure:
                            ToggleSoloGroup(m_TopdownStructure, true);
                            break;

                        case BuilderMode.Placement:
                            ToggleSoloGroup(m_TopdownFurniture, true);
                            break;
                        default:
                            Debug.LogError($"{mode} not found");
                            break;
                    }
                    break;

                case View.FirstPerson:
                    switch (mode)
                    {
                        case BuilderMode.Plot:
                            ToggleSoloGroup(m_FirstPersonPlot, true);
                            break;

                        case BuilderMode.Structure:
                            ToggleSoloGroup(m_FirstPersonStructure, true);
                            break;

                        case BuilderMode.Placement:
                            ToggleSoloGroup(m_FirstPersonFurniture, true);
                            break;
                        default:
                            Debug.LogError($"{mode} not found");
                            break;
                    }
                    break;
            }
        }

        private void ToggleAvailableItemEditingOptions(bool state)
        {
            m_ItemSettings.SetActive(state);
            if (state && (m_CurrenteSelection != null && m_CurrenteSelection.Count != 0))
            {
                m_ItemIsRotatable = false;
                m_ItemIsModular = false;

                foreach (var data in m_CurrenteSelection)
                {
                    if (data.CatalogueEntryRef != null)
                    {
                        foreach (var metadata in data.CatalogueEntryRef.GetLoadedMetaData())
                        {
                            if (metadata.GetMetaDataType() == MetaDataType.ContentAdaptive)
                            {
                                m_ItemIsModular = true;
                            }

                            if (metadata.GetMetaDataType() == MetaDataType.ContentScale)
                            {
                                //TODO
                            }
                        }
                    }
                }

                foreach (var data in m_CurrenteSelection)
                {
                    if (data.PlacementMetaData.Placement.IsGrounded())
                    {
                        m_ItemIsRotatable = true;
                        break;
                    }
                }

                m_RotateItemSetting.SetActive(m_ItemIsRotatable);
                m_ExpandItemSetting.SetActive(m_ItemIsModular);
            }
        }
    }
}
