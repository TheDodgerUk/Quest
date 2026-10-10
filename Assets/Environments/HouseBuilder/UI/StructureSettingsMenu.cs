using Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class StructureSettingsMenu : MonoBehaviour
    {
        public Action<bool> OnColourSettingsToggled;

        protected CanvasGroup m_Group;
        protected Button m_BtnDelete;
        protected Toggle m_TglShow;

        protected string m_NonItemSelecetedMaterial = "";
        protected Color m_NonItemSelecetedColour = Color.white;
        protected bool m_UseNonItemSelected = false;

        public virtual void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default

            m_BtnDelete = transform.Find("DeleteShowBtns/BtnDelete").GetComponent<Button>();
            m_TglShow = transform.Find("DeleteShowBtns/TglShow").GetComponent<Toggle>(); //on by default

            m_TglShow.onValueChanged.AddListener((value) => Display(value));
            m_BtnDelete.onClick.AddListener(() => Delete());

            HouseBuilderRoot.Instance.OnModeTransitionBegin += OnModeTransitionBegin;
            HBGUIManager.Instance.m_StructureSwitchMenu.OnStructureSwitchChanged += (value) => { SetVisible(false, true); };
        }

        public void PostInitialise(CatalogueMenu catalogue, Action<bool> onColToggled) //Delayed because catalogue must be initialised before this
        {
            catalogue.OnMaterialPicked += OnMaterialPicked;
            OnColourSettingsToggled += onColToggled;
        }

        private void OnModeTransitionBegin()
        {
            ClearNonItemMaterial();
            SetVisible(false, true);
        }


        public void SetVisible(bool state, bool manual)
        {
            if (HouseBuilderRoot.Instance.IsMouseOverGUI() && !manual) return;

            m_Group.VisibleAndInteractive(state);

            SetColoursActive(state);
        }

        private void SetColoursActive(bool state)
        {
            if (state)
            {
                ColorPickerManager.Instance.SetInteriorDefaultColours();
                ColorPickerManager.OnColourChanged = OnColourPicked;
            }
            else
            {
                ColorPickerManager.OnColourChanged = null;
            }

            OnColourSettingsToggled?.Invoke(state);
        }

        public void ClearNonItemMaterial()
        {
            m_UseNonItemSelected = false;
            if (MaterialSelectionGUI.Instance != null && MaterialSelectionGUI.Instance.IsSelectionGuiEnabled())
            {
                MaterialSelectionGUI.Instance.EnableSelectionGui(false);
            }
        }
        protected void SetNonItemMaterial(string mat)
        {
            m_UseNonItemSelected = true;
            m_NonItemSelecetedMaterial = mat;
            MaterialSelectionGUI.Instance.EnableSelectionGui(true, mat);
        }

        public virtual void OnColourPicked(Color col)
        { }

        public virtual void OnMaterialPicked(CatalogueMenu.MenuContents menuContents, string materialName, string filter)
        { }

        public virtual void Delete()
        { }

        public virtual void Display(bool state)
        { }
    }
}