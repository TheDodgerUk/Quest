using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ModularMenu : MonoBehaviour
    {

        private CanvasGroup m_Group;
        private Button m_BtnForward;
        private Button m_BtnBackward;
        private TextMeshProUGUI m_InfoText;
        private GameObject m_ItemPrefab;
        private ToggleGroup m_ToggleGroup;

        private Scrollbar m_ViewScrollbar;
        private Transform m_ScrollViewContent;
        private List<Toggle> m_ButtonsList = new List<Toggle>();

        private Modular ModularRef;
        private GameObject m_CirclePrefab;
        private Interaction m_Interaction;

        private int m_ItemIndex = 0;

        public void Initialise(Dictionary<string, GameObject> contentTypes)
        {
            m_Group = GetComponent<CanvasGroup>();
            m_BtnForward = transform.Find("BtnForward").GetComponent<Button>();
            m_BtnBackward = transform.Find("BtnBackward").GetComponent<Button>();
            m_InfoText = transform.Find("InfoMsg").GetComponent<TextMeshProUGUI>();


            m_ScrollViewContent = transform.Find("Scroll View/Viewport/Content");
            m_ViewScrollbar = transform.Find("Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>();

            m_ItemPrefab = contentTypes["SavedPlot"].gameObject;
            m_CirclePrefab = Instantiate(contentTypes["ModularCircle"].gameObject);
            m_CirclePrefab.transform.SetParent(this.transform);
            m_ToggleGroup = m_ScrollViewContent.GetComponent<ToggleGroup>();

            SetVisible(false);
        }

        public void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);
        }

        public bool IsActive() => m_Group.interactable;


        public void ShowList(Interaction inter)
        {
            ModularRef = inter.ModularRef;
            DeleteCurrentList();
            CreateCurrentList();
           
            m_BtnForward.gameObject.SetActive(true);
            m_BtnBackward.gameObject.SetActive(true);


            m_BtnForward.onClick.RemoveAllListeners();
            m_BtnForward.onClick.AddListener(() =>
            {
                m_ItemIndex = m_ItemIndex.WrapIncrement(ModularRef.ContentModularMetaDataRef.m_Modular);
                DeleteCurrentList();
                CreateCurrentList();
            });

            m_BtnBackward.onClick.RemoveAllListeners();
            m_BtnBackward.onClick.AddListener(() =>
            {
                m_ItemIndex = m_ItemIndex.WrapDecrement(ModularRef.ContentModularMetaDataRef.m_Modular);
                DeleteCurrentList();
                CreateCurrentList();
            });
        }

        private void CreateCurrentList()
        {
            var startItem = ModularRef.ContentModularMetaDataRef.m_Modular[m_ItemIndex];
            var splitName = startItem.m_ItemsRootGameObjectName.SplitList("/");
            m_InfoText.text = splitName.Last();
            int index = 0;
            foreach(var item in startItem.m_ItemsChildrenGameObject)
            {
                CreateNewItem(item.name, index);
                index++;
            }

            int selectedIndex = ModularRef.ModularDataRef.m_SelectedChildIndexs[m_ItemIndex];
            m_ButtonsList[selectedIndex].SetIsOnWithoutNotify(true);
        }

        private void Update()
        {
            if (ModularRef != null)
            {
                var data = ModularRef.ContentModularMetaDataRef.m_Modular[m_ItemIndex];
                if (data.m_ConnectionPointGameObject != null)
                {
                    Vector3 uiIcon = Camera.main.WorldToScreenPoint(data.m_ConnectionPointGameObject.transform.position);
                    m_CirclePrefab.transform.position = uiIcon;
                    m_CirclePrefab.SetActive(true);
                }
                else
                {
                    m_CirclePrefab.SetActive(false);
                }
            }
            else
            {
                m_CirclePrefab.SetActive(false);
            }
        }

        private void CreateNewItem(string text, int index)
        {
            var tglClone = Instantiate(m_ItemPrefab, m_ScrollViewContent);
            tglClone.SetActive(true);
            tglClone.name = tglClone.name;
            var tgl = tglClone.GetComponentInChildren<Toggle>();
            tgl.group = m_ToggleGroup;
            tgl.onValueChanged.AddListener((value) => SetSelectedToggle(value, index));
            tglClone.GetComponentInChildren<TextMeshProUGUI>().text = text.Replace("_", " ");
            m_ButtonsList.Add(tgl);
        }

        private void SetSelectedToggle(bool state, int index)
        {
            if (state == true)
            {
                ModularRef.ModularDataRef.m_SelectedChildIndexs[m_ItemIndex] = index;
                ModularRef.Rebuild();
                Debug.LogError($"index {index}");
            }
        }

        private void ActivateButtons(bool state)
        {
            m_BtnForward.interactable = state;
            m_BtnBackward.interactable = state;
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
