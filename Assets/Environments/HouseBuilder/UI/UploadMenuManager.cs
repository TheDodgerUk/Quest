using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class UploadMenuManager : MonoBehaviour
    {
        public enum UploadType
        {
            Images,
            Movies,
            Sounds,
        }




        public static UploadMenuManager Instance;

        private CanvasGroup m_CanvasGroup;
        private Toggle m_FolderTgl;

        private Button m_UploadButton;
        private Button m_DeleteButton;
        private GameObject m_PlotPrefab;
        private Transform m_ScrollViewContent;
        private Scrollbar  m_ViewScrollbar;
        private ToggleGroup m_ToggleGroup;

        private TMP_Dropdown m_Dropdown;

        private List<Toggle> m_ButtonsList = new List<Toggle>();

        private string m_SelectedItem;

        private UploadType m_UploadType = UploadType.Images;


        public bool m_IsVisible = true;

        public void Initialise(Dictionary<string, GameObject> contentTypes, Toggle folderToggle)
        {
            Instance = this;
            m_FolderTgl = folderToggle;
            m_CanvasGroup = this.GetComponent<CanvasGroup>();
            SetUpMenuComponents(contentTypes);
            Toggle(false);
            UploadListManualRefresh();
            ActivateLoadCancelButtons(false);
        }


        private void SetUpMenuComponents(Dictionary<string, GameObject> contentTypes)
        {
            m_UploadButton = transform.Find("BtnUpload").GetComponent<Button>();
            m_DeleteButton = transform.Find("BtnDelete").GetComponent<Button>();
            m_Dropdown = transform.Find("Type").GetComponent<TMP_Dropdown>();

            m_ScrollViewContent = transform.Find("Scroll View/Viewport/Content");
            m_ViewScrollbar = transform.Find("Scroll View/Scrollbar Vertical").GetComponent<Scrollbar>();
            m_PlotPrefab = contentTypes["SavedPlot"].gameObject;
            m_ToggleGroup = m_ScrollViewContent.GetComponent<ToggleGroup>();

            m_UploadButton.onClick.AddListener(UploadItem);
            m_DeleteButton.onClick.AddListener(DeleteItem);
            m_SelectedItem = null;

            m_Dropdown.ClearOptions();

            List<string> items = new List<string>();
            foreach (var val in Enum.GetValues(typeof(UploadType)))
            {
                items.Add(val.ToString());
            }
            m_Dropdown.AddOptions(items);
            m_Dropdown.onValueChanged.AddListener((e) =>
            {
                m_UploadType = (UploadType)e;
                UploadListManualRefresh();
            });
        }


        private void UploadItem()
        {
            string folderPath = Core.LoadFromFileFBXRef.GetSearchDirectory();
            string file = null;
            switch (m_UploadType)
            {
                case UploadType.Images:
                    file = Crosstales.FB.FileBrowser.OpenSingleFile("Open single file", folderPath, new Crosstales.FB.ExtensionFilter("Items", "png", "jpg"));
                    break;
                case UploadType.Movies:
                    file = Crosstales.FB.FileBrowser.OpenSingleFile("Open single file", folderPath, new Crosstales.FB.ExtensionFilter("Items", "mp4"));
                    break;
                case UploadType.Sounds:
                    file = Crosstales.FB.FileBrowser.OpenSingleFile("Open single file", folderPath, new Crosstales.FB.ExtensionFilter("Items", "mp3", "wav"));
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(file) == false)
            {
                if (file.Contains("_") == true)
                {
                    GenericPromptMenu.Instance.ShowOkayMenu("Cannot upload This file, please remove any '_' in file name", null);
                }
                else
                {
                    GenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you want upload   {file}", () =>
                    {
                        Core.LoadFromFileFBXRef.SetSearchDirectory(file);
                        HouseBuilderRoot.Instance.AllowLeftMouseInput = false;
                        Debug.LogError("no input, VERY VERY SLOW, wait 120 seconds");
                        AssetSyncMenu.Instance.SetVisible(true);
                        AssetSyncMenu.Instance.SetActiveSpinning();
                        AssetSyncMenu.Instance.SetHeaderText("Uploading file");
                        Core.Network.UploadCustomUserFile(file, OnProgress, () =>
                        {
                            HouseBuilderRoot.Instance.AllowLeftMouseInput = true;
                            AssetSyncMenu.Instance.SetVisible(false);
                            Debug.LogError("input back");
                            InitialiseUploadList(null);
                        });
                    }, null);
                }
            }
        }

        private void OnProgress(float progress)
        {

        }

        private void DeleteItem()
        {
            if (m_SelectedItem != null)
            {
                GenericPromptMenu.Instance.ShowYesNoMenu($"Are you sure you want to delete this: {m_SelectedItem}? ", () =>
                {
                    HouseBuilderRoot.Instance.DeleteFiles(m_SelectedItem, null);
                    InitialiseUploadList(null);
                    ResetInterface();
                    DisableAllToggle();
                }, null);
            }
        }


        public void Toggle(bool state)
        {
            m_IsVisible = state;
            m_CanvasGroup.interactable = state;
            m_CanvasGroup.blocksRaycasts = state;

            if (state)
            {
                UploadListManualRefresh();
                m_CanvasGroup.alpha = 1;
            }
            else
            {
                ActivateLoadCancelButtons(false);
                ResetInterface();
                m_CanvasGroup.alpha = 0;
                m_ViewScrollbar.value = 1;
                m_FolderTgl.isOn = false;
            }

            m_UploadButton.SetActive(true);
            m_DeleteButton.SetActive(true);
        }

        private void ActivateLoadCancelButtons(bool state)
        {
            m_DeleteButton.interactable = state;
        }

        private void GetSelectedToggle(string sceneName, bool state)
        {
            if (!state)
            {
                ResetInterface();
            }
            else
            {
                m_SelectedItem = sceneName;
            }
            ActivateLoadCancelButtons(state);
        }

        private void ResetInterface()
        {
            m_SelectedItem = null;
            ActivateLoadCancelButtons(false);
        }

        private void CreateNewItem(string text)
        {
            var tglClone = Instantiate(m_PlotPrefab, m_ScrollViewContent);
            tglClone.SetActive(true);
            tglClone.name = tglClone.name;
            var tgl = tglClone.GetComponentInChildren<Toggle>();
            tgl.group = m_ToggleGroup;
            tgl.onValueChanged.AddListener((value)=> GetSelectedToggle(tglClone.GetComponentInChildren<TextMeshProUGUI>().text, value));
            tglClone.GetComponentInChildren<TextMeshProUGUI>().text = text;
            m_ButtonsList.Add(tgl);
        }


        private void InitialiseUploadList(Action callback)
        {
            if (Core.Network.Users != null && Core.Network.Users.CurrentUser != null)
            {
                Core.Network.GetCustomFileList((list) =>
                {
                    DeleteCurrentList();
                    if(list != null)
                    {
                        List<string> items = new List<string>();
                        foreach(var item in list)
                        {
                            switch (m_UploadType)
                            {
                                case UploadType.Images:
                                    if (item.EndsWith("png", StringComparison.CurrentCultureIgnoreCase) || item.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        items.Add(item);
                                    }
                                    break;
                                case UploadType.Movies:
                                    if (item.EndsWith("mp4", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        items.Add(item);
                                    }
                                    break;
                                case UploadType.Sounds:
                                    if (item.EndsWith("mp3", StringComparison.CurrentCultureIgnoreCase) || item.EndsWith("wav", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        items.Add(item);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        CreateCurrentList(items);
                    }
                    else
                    {
                        CreateCurrentList(null);

                    }
                    callback?.Invoke();
                });
            }
            else
            {
                Debug.LogError("No uploads available, please log in to display your uploads.");
            }
        }

        private void DeleteCurrentList()
        {
            for(int i = 0; i < m_ButtonsList.Count; i++)
            {
                Destroy(m_ButtonsList[i].gameObject);
            }
            m_ButtonsList.Clear();
        }

        private void CreateCurrentList(List<String> items)
        {
            if(items != null)
            {
                items.Sort();
                foreach (var item in items)
                {
                    CreateNewItem(item);
                }
            }
        }


        public void UploadListManualRefresh()
        {
            InitialiseUploadList(() =>
            {
            });
        }

        private void DisableAllToggle()
        {
            if (m_ToggleGroup.AnyTogglesOn())
            {
                m_ToggleGroup.SetAllTogglesOff();
            }
        }
    }
}

