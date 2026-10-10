using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class GenericListMenu : MonoBehaviour
    {
        public static GenericListMenu Instance;

        private CanvasGroup m_Group;
        private Button m_BtnYes;
        private Button m_BtnNo;
        private TextMeshProUGUI m_InfoText;
        private GameObject m_ItemPrefab;
        private ToggleGroup m_ToggleGroup;

        private Scrollbar m_ViewScrollbar;
        private Transform m_ScrollViewContent;
        private List<Toggle> m_ButtonsList = new List<Toggle>();

        private string m_SelectedItem;

        public void Initialise(Dictionary<string, GameObject> contentTypes)
        {
            Instance = this;

            m_Group = GetComponent<CanvasGroup>();
            m_BtnYes = transform.Find("BtnNew").GetComponent<Button>();
            m_BtnNo = transform.Find("BtnDelete").GetComponent<Button>();
            m_InfoText = transform.Find("InfoMsg").GetComponent<TextMeshProUGUI>();


            m_ScrollViewContent = transform.Find("Scroll View/Viewport/Content");
            m_ViewScrollbar = transform.Find("Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>();

            m_ItemPrefab = contentTypes["SavedPlot"].gameObject;
            m_ToggleGroup = m_ScrollViewContent.GetComponent<ToggleGroup>();
            

            Instance.SetVisible(false);
        }

        private void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);

            Vector2 correctSize = new Vector2(620, 366);

            var rect = this.GetComponent<RectTransform>();
            if (rect.sizeDelta != correctSize)
            {
                Debug.LogError($"for some reason its changing size, but i cant figure out why !!!!!! meant to be {correctSize} to {rect.sizeDelta} , cahnged back to {correctSize}");
                rect.sizeDelta = new Vector2(620, 366);
            }
        }

        public static void ShowList(string text, List<string> items, Action<string> onYesPressed = null, Action onNoPressed = null)
        {

            foreach(var item in items)
            {
                Instance.CreateNewItem(item);
            }
            Instance.SetVisible(true);
            Instance.m_BtnYes.gameObject.SetActive(true);
            Instance.m_BtnNo.gameObject.SetActive(true);

            Instance.m_InfoText.SetText(text);
            Instance.m_BtnYes.onClick.AddListener(() =>
            {               
                //default for Yes btn should also be to close this menu
                Instance.SetVisible(false);
                Instance.DeleteCurrentList();
                Instance.m_BtnYes.onClick.RemoveAllListeners();
                onYesPressed?.Invoke(Instance.m_SelectedItem);
            });

            Instance.m_BtnNo.onClick.AddListener(() =>
            {
                
                //default for No btn should also be to close this menu
                Instance.SetVisible(false);
                Instance.DeleteCurrentList();
                Instance.m_BtnNo.onClick.RemoveAllListeners();
                onNoPressed?.Invoke();
            });
        }


        private void CreateNewItem(string text)
        {
            var tglClone = Instantiate(m_ItemPrefab, m_ScrollViewContent);
            tglClone.SetActive(true);
            tglClone.name = tglClone.name;
            var tgl = tglClone.GetComponentInChildren<Toggle>();
            tgl.group = m_ToggleGroup;
            tgl.onValueChanged.AddListener((value) => GetSelectedToggle(tglClone.GetComponentInChildren<TextMeshProUGUI>().text, value));
            tglClone.GetComponentInChildren<TextMeshProUGUI>().text = text;
            m_ButtonsList.Add(tgl);
        }

        private void GetSelectedToggle(string item, bool state)
        {
            if (state == false)
            {
                m_SelectedItem = null;
            }
            else
            {
                m_SelectedItem = item;
            }
            ActivateButtons(state);
        }

        private void ActivateButtons(bool state)
        {
            Instance.m_BtnYes.interactable = state;
            Instance.m_BtnNo.interactable = state;
        }

        private void DeleteCurrentList()
        {
            for (int i = 0; i < m_ButtonsList.Count; i++)
            {
                Destroy(m_ButtonsList[i].gameObject);
            }
            m_ButtonsList.Clear();
        }

    }
}