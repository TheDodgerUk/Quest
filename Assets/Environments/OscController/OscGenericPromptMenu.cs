using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OscController
{
    public class OscGenericPromptMenu : HouseBuilder.GenericPromptMenu
    {

        public static new OscGenericPromptMenu Instance { get; private set; }
        public override void Initialise()
        {
            Instance = this;
            this.SetActive(true);
            m_Group = GetComponent<CanvasGroup>();
            m_BtnYes = transform.Find("YesNoBtns/BtnYes").GetComponent<Button>();
            m_BtnNo = transform.Find("YesNoBtns/BtnNo").GetComponent<Button>();
            m_BtnOkay = transform.Find("BtnOkay").GetComponent<Button>();
            m_InfoText = transform.Find("InfoMsg").GetComponent<TextMeshProUGUI>();

            SetVisible(false);
        }

    }
}
