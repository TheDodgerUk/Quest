using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{

    public class CatalogueMenuSound : CatalogueMenuObject
    {
        TextMeshProUGUI m_MainLabel;
        public void Initialise(string textureName, string group, string filter, Action onClick)
        {
            m_FilterList = new List<string>() { group + "/" + filter };
            m_Guid = "guidplaceholder?";
            m_ShortName = textureName;
            gameObject.name = m_ShortName;
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(() => onClick());
            m_Icon = transform.Find("Icon").GetComponent<Image>();

            m_SelectedBorder = transform.Find("SelectedBorder").GetComponent<Image>();           
            m_SelectedBorder.color = NORMAL;

            m_MainLabel = m_Icon.GetComponentInChildren<TextMeshProUGUI>();
            AssignMaterialNameTolabel(m_MainLabel);

            m_MainLabel.color = Color.black;

            ToggleThumbnail(false);
        }


        protected override void ToggleThumbnail(bool state)
        {
            m_MainLabel.color = (state == true)? Color.red : Color.black;
        }
    }
}


