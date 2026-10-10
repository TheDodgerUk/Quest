using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class CatalogueMenuFilter : MonoBehaviour
    {
        public string m_FilterName;
        private Toggle m_Toggle;
        private TextMeshProUGUI m_Label;
        public bool m_IsActive;
        public bool m_IsToggleOn;
        private Color32 SELECTED_COLOR = new Color32(253, 207, 98, 255);
        private Color UNSELECTED_COLOR = new Color32(74, 144, 226, 255);

        public void Initialise(string group, string name, Action<bool> onValueChanged)
        {
            m_FilterName = group + "/"+ name;
            m_Toggle = GetComponent<Toggle>();
            m_Label = GetComponentInChildren<TextMeshProUGUI>();

            m_Toggle.onValueChanged.AddListener((value) =>
            {
                onValueChanged(value);
                m_IsToggleOn = value;
                ChangeTextColor(value);
            });
            m_Label.SetText(name);

            SetActive(true);
        }

        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
            m_IsActive = state;
        }

        private void ChangeTextColor(bool value)
        {
            if(value)
                m_Label.color = SELECTED_COLOR;
            else
                m_Label.color = UNSELECTED_COLOR;
        }
    }
}