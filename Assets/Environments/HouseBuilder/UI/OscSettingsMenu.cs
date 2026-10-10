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
    public class OscSettingsMenu : MonoBehaviour
    {
        private CanvasGroup m_Group;
        private bool m_IsGroupActive;
        private Button m_OscButton;

        public void PostInitialise(CatalogueMenu catalogue) //Delayed because catalogue must be initialised before this
        {
            
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            SetVisible(false);
        }


        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_Group.FixBrokenMasks();
            m_IsGroupActive = false;

            m_OscButton = this.transform.SearchComponent<Button>("OscButton");
            m_OscButton.onClick.AddListener(() =>
            {
                HouseBuilderRoot.Instance.CurrentMode = BuilderMode.OSC;
            });

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }


        public void SetVisible(bool state)
        {
            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }


    }
}