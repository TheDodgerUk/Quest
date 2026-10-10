using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ListItem : MonoBehaviour
    {
        public CatalogueGeneratedData m_ItemData;
        private Button m_BtnExpand;
        private Button m_BtnCollapse;
        private Animator m_Animator;
        private TextMeshProUGUI m_Title;
        private TextMeshProUGUI m_ItemID;
        private TextMeshProUGUI m_Content;
        private Button m_BtnSelect;
        private Image m_Icon;

        internal void Initialise(CatalogueGeneratedData itemData, Image icon)
        {
            m_ItemData = itemData;
            Transform holder = transform.Find("Container");
            m_Icon = holder.Find("Icon").GetComponent<Image>();
            m_BtnExpand = holder.Find("BtnExpand").GetComponent<Button>();
            m_BtnCollapse = holder.Find("BtnCollapse").GetComponent<Button>();
            m_BtnSelect = holder.Find("BtnSelect").GetComponent<Button>();
            m_Title = holder.Find("Title/LabelTitle").GetComponent<TextMeshProUGUI>();
            m_ItemID = holder.Find("Title/LabelItemID").GetComponent<TextMeshProUGUI>();
            m_Content = holder.Find("Description/LabelDescription").GetComponent<TextMeshProUGUI>();
            m_Animator = GetComponent<Animator>();
            //Off by default
            m_BtnExpand.onClick.AddListener(() => Toggle());
            m_BtnCollapse.onClick.AddListener(() => Toggle());
            m_BtnSelect.onClick.AddListener(() => SelectItem());
            m_Icon.sprite = icon.sprite;

            m_Title.SetText(m_ItemData.CatalogueEntryRef.FullName);
            m_ItemID.SetText(m_ItemData.CatalogueEntryRef.Guid);

            string content = m_ItemData.CatalogueEntryRef.ShortName + " " +
                             "placeholder/example"; // Don't know what other data to show for these, any ideas would be good.
            m_Content.SetText(content);
        }

        private void Toggle()
        {
            m_Animator.SetTrigger("ToggleItem");
        }

        private void SelectItem()
        {
            HouseBuilderRoot.Instance.InputContents.ClearAllInteractions();
            HouseBuilderRoot.Instance.InputContents.SelectInteraction((InteractionGameObject)m_ItemData.InteractionRef);
            HouseBuilderRoot.Instance.InputCamera.ManualGUIFocus();

            //EXCEPTION JUST IN THIS CASE, NOT TO BE USED OTHERWISE, used to override IsMouseOverGUI()
            HBGUIManager.Instance.ItemSettingsMenuRef.SetActive(true, true);
        }
    }


    public class SceneItemsList : MonoBehaviour
    {

        public static SceneItemsList Instance;

        private CanvasGroup m_Group;
        private GameObject m_ItemPrefab;
        private Transform m_ScrollViewContent;
        private TextMeshProUGUI m_ItemsTotalCount;
        private TextMeshProUGUI m_ItemsTotalPrice;
        private TextMeshProUGUI m_SuccessMessage;
        private Color m_StartingColor;
        private List<ListItem> m_Contents;
        private Dictionary<string, Image> m_ListOfCatIcons = new Dictionary<string, Image>();

        private List<CatalogueMenuObject> m_CatalogueMenuObjectList;

        internal void Initialise(Dictionary<string, GameObject> contentTypes)
        {
            Instance = this;
            m_Group = gameObject.GetComponent<CanvasGroup>();
            m_ItemPrefab = contentTypes["ItemListItem"];
            m_Contents = new List<ListItem>();

            m_ScrollViewContent = transform.Find("Scroll View/Viewport/Content");
            m_ItemsTotalCount = transform.Find("Footer/Item Count").GetComponent<TextMeshProUGUI>();
            m_ItemsTotalPrice = transform.Find("Footer/Price Count").GetComponent<TextMeshProUGUI>();

            var exportButton = transform.Find("Footer/BtnPrint").GetComponent<Button>();
            exportButton.onClick.AddListener(() => PrintList());
            exportButton.onClick.AddListener(() => ShowFolder());
            SetVisible(false);
            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void OnEnvironmentLoadingComplete()
        {
            HouseBuilderRoot.Instance.InputContents.ModeContainerRef.OnItemsChanged += OnUpdateItems;

            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;

            UpdateTotalCount();
            UpdateTotalPrice();
            //other stuff here after setup is done
        }

        private void UpdateTotalCount() => m_ItemsTotalCount.SetText(m_Contents.Count.ToString());
        private void UpdateTotalPrice() => m_ItemsTotalPrice.SetText((m_Contents.Count * 1000).ToString("#,##0"));

        //TODO: rework this so that icons come from one reference and there are no timing issues
        public void ProcessCatalogueIcons(List<CatalogueMenuObject> itemList)
        {
            m_CatalogueMenuObjectList = itemList;
            AddCatIcon();
        }

        private void AddCatIcon()
        {
            foreach (var item in m_CatalogueMenuObjectList)
            {
                if (m_ListOfCatIcons.ContainsKey(item.name) == false)
                {
                    m_ListOfCatIcons.Add(item.name, item.transform.Find("Icon").GetComponent<Image>());
                }
                else
                {
                    Debug.LogError($"Key has been added before key: {item.name}", item);
                }
            }
        }

        private void PrintList()
        {
            PDFExport.WritePDF(m_Contents, Application.persistentDataPath);
            ShowSuccessMessage();
        }

        public void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);
        }

        private void OnUpdateItems(List<CatalogueGeneratedData> itemDataList)
        {
            // delete items which no longer exist 
            List<ListItem> toRemove = new List<ListItem>();
            for(int i = m_Contents.LastIndex(); i >= 0; i--)
            {
                var item = itemDataList.Find(e => e.GeneratedItemRef == m_Contents[i].m_ItemData.GeneratedItemRef);
                if(item == null)
                {
                    ItemDeleted(m_Contents[i]);
                }
            }

            // add new items 
            for (int i = 0; i < itemDataList.Count; i++)
            {
                var item = m_Contents.Find(e => e.m_ItemData.GeneratedItemRef == itemDataList[i].GeneratedItemRef);
                if (item == null)
                {
                    ItemAdded(itemDataList[i]);
                }
            }
        }

        private void ItemAdded(CatalogueGeneratedData itemData)
        {
            if (m_ListOfCatIcons.ContainsKey(itemData.CatalogueEntryRef.ShortName) == false)
            {
                AddCatIcon();
            }

            if (m_ListOfCatIcons.ContainsKey(itemData.CatalogueEntryRef.ShortName) == true)
            {
                if (itemData != null)
                {
                    var newGameObject = Instantiate(m_ItemPrefab, m_ScrollViewContent);
                    newGameObject.name = itemData.CatalogueEntryRef.ShortName;
                    newGameObject.SetActive(true);
                    ListItem newItem = newGameObject.AddComponent<ListItem>();
                    newItem.Initialise(itemData, m_ListOfCatIcons[newGameObject.name]);

                    m_Contents.Add(newItem);

                    UpdateTotalCount();
                    UpdateTotalPrice();
                }
            }
            else
            {

                var data = itemData.CatalogueEntryRef.GetMetaData<RenderMetaData>();
                if(data != null && string.IsNullOrEmpty(data.RuntimeDirectory) == false)
                {
                    Debug.LogError($"{itemData.CatalogueEntryRef.ShortName}, this is a load in at runtime model");
                }
                else
                {
                    Debug.LogError($"cannot find icon for {itemData.CatalogueEntryRef.ShortName}, this is because       public void ProcessCatalogueIcons(List<CatalogueMenuObject> itemList)       itemlist has not been compleated yet ");
                }       
            }
        }

        private void ItemDeleted(ListItem itemData)
        {
            var item = m_Contents.Find(e => e.m_ItemData.GeneratedItemRef);
            if (item != null)
            {               
                m_Contents.RemoveAll(e => e.gameObject == itemData.gameObject);
                Destroy(itemData.gameObject);

                UpdateTotalCount();
                UpdateTotalPrice();
            }
        }

        private void ShowSuccessMessage()
        {
            GenericPromptMenu.Instance.ShowOkayMenu("PDF correctly downloaded.");
        }

        private void ShowFolder()
        {
            string fullpath = Application.persistentDataPath + "/";
            fullpath = fullpath.Replace(@"/", @"\");   // explorer doesn't like front slashes
            System.Diagnostics.Process.Start("explorer.exe", "/root," + fullpath);
        }

    }
}