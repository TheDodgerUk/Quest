using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class GenericPromptMenu : MonoBehaviour
    {
        public static GenericPromptMenu Instance { get; private set; }

        protected CanvasGroup m_Group;
        protected Button m_BtnYes;
        protected Button m_BtnNo;
        protected Button m_BtnOkay;
        protected TextMeshProUGUI m_InfoText;

        public virtual void Initialise()
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

        protected void SetVisible(bool state)
        {
            this.gameObject.transform.SetAsLastSibling();
            m_Group.VisibleAndInteractive(state);
        }

        public void ShowYesNoMenu(string text, Action onYesPressed = null, Action onNoPressed = null)
        {          
            SetVisible(true);
            m_BtnOkay.gameObject.SetActive(false);
            m_BtnYes.gameObject.SetActive(true);
            m_BtnNo.gameObject.SetActive(true);

            m_InfoText.SetText(text);
            ClearListeners();


            m_BtnYes.onClick.AddListener(() =>
            {
                ClearListeners();
                onYesPressed?.Invoke();
                //default for Yes btn should also be to close this menu
                SetVisible(false);
                ClearListeners();
            });

            m_BtnNo.onClick.AddListener(() =>
            {
                ClearListeners();
                onNoPressed?.Invoke();
                //default for No btn should also be to close this menu
                SetVisible(false);
                ClearListeners();
            });
        }

        public void ShowOkayMenu(string text, Action onOkayPressed = null)
        {
            SetVisible(true);
            m_BtnOkay.gameObject.SetActive(true);
            m_BtnYes.gameObject.SetActive(false);
            m_BtnNo.gameObject.SetActive(false);

            m_InfoText.SetText(text);

            ClearListeners();
            m_BtnOkay.onClick.AddListener(() =>
            {
                ClearListeners();
                onOkayPressed?.Invoke();
                //default for Okay should also be to close this menu
                SetVisible(false);
                ClearListeners();
            });
        }

        private void ClearListeners()
        {
            m_BtnOkay.onClick.RemoveAllListeners();
            m_BtnYes.onClick.RemoveAllListeners();
            m_BtnNo.onClick.RemoveAllListeners();
        }
    }
}