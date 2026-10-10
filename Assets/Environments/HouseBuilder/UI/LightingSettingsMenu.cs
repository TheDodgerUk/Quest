using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class LightingSettingsMenu : MonoBehaviour
    {
        private CanvasGroup m_Group;
        private bool m_IsGroupActive;

        private TMP_Dropdown m_DrdDmx;
        private TMP_Dropdown m_DrdMode;
        private Text m_LightName;

        private List<InteractionGameObject> m_CurrentSelections = null;
        private SimpleDictionaryList<Interaction, ItemSettingsMenu.CombinedMetadata> m_MetaList = new SimpleDictionaryList<Interaction, ItemSettingsMenu.CombinedMetadata>();

        public void PostInitialise(CatalogueMenu catalogue) //Delayed because catalogue must be initialised before this
        {
            
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }


        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_IsGroupActive = false;

            m_LightName = transform.Find("LightName").GetComponentInChildren<Text>();

            InitialiseDmxDropDown();
            m_DrdMode = transform.Find("ModeDropdown").GetComponent<TMP_Dropdown>();
            m_DrdMode.transform.FindAndFixMasks();
            m_DrdMode.ClearOptions();

            HouseBuilderRoot.Instance.OnObjectSelected += (state, data) =>
            {
                if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Lighting)
                {
                    m_CurrentSelections = data;
                    SetVisible(state, false);
                }
            };

            HouseBuilderRoot.Instance.OnModeTransitionBegin += () => SetVisible(false, true);
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void InitialiseDmxDropDown()
        {
            m_DrdDmx = transform.Find("DmxDropdown").GetComponent<TMP_Dropdown>(); //off by default
            m_DrdDmx.transform.FindAndFixMasks();
            m_DrdDmx.ClearOptions();
            List<string> dmx = new List<string>();
            for (int i = 1; i < OscController.OscControllerRoot.MAX_DMX_VALUE; i++)
            {
                dmx.Add(i.ToString());
            }
            m_DrdDmx.AddOptions(dmx);

            var template = m_DrdDmx.transform.SearchComponent<RectTransform>("Template");
            template.sizeDelta = new Vector2(template.sizeDelta.x, 600f); // Make dropdown bigger on vertical 
        }


        public void SetVisible(bool state, bool manual)
        {
            if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Lighting)
            {
                state = false;
                if (m_CurrentSelections != null)
                {
                    m_CurrentSelections.Clear();
                }
            }

            if (state == true)
            {
                if (m_CurrentSelections != null && m_CurrentSelections.Count > 0)
                {
                    Lighting firstItem = m_CurrentSelections.First().LightingRef;

                    m_LightName.text = firstItem.ContentLightingMetaDataRef.m_Fixure.Name;
                    m_DrdDmx.onValueChanged.RemoveAllListeners();
                    m_DrdDmx.SetValueWithoutNotify(firstItem.DmxStartNumber - 1);
                    m_DrdDmx.onValueChanged.AddListener((indexChoice) =>
                    {
                        firstItem.DmxStartNumber = (indexChoice + 1);
                    });

                    m_DrdMode.onValueChanged.RemoveAllListeners();
                    m_DrdMode.ClearOptions();
                    m_DrdMode.AddOptions(firstItem.GetModeNames());
                    m_DrdMode.SetValueWithoutNotify(firstItem.ModeIndex);
                    m_DrdMode.onValueChanged.AddListener((indexChoice) =>
                    {
                        firstItem.ModeIndex = indexChoice;
                    });
                }
            }


            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }


    }
}