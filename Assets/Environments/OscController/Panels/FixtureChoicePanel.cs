using HouseBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer.OscAllUsedFixtures;
using static OscController.OscControllerRoot;

namespace OscController
{
    public class FixtureChoicePanel : MonoBehaviour
    {
        private const string FIXTURE_PREFAB = "FixtureChoicePrefab";
        private Button m_AddItem;

        private int m_SelectedFixtureIndex = 0;
        private int m_SelectedMode = 0;

        private GameObject m_ModeView;
        private GameObject m_ModeViewContent;
        private ToggleGroup m_ModeToggleGroup;
        private bool m_Initialised = false;


        private InputField m_SearchField;
        private Button m_SearchClear;

        private List<TextMeshProUGUI> m_FixtureList = new List<TextMeshProUGUI>();


        public void Initialise()
        {
            m_ModeView = this.transform.Search("ModeView").gameObject;
            m_ModeViewContent = m_ModeView.transform.Search("Content").gameObject;
            m_ModeToggleGroup = m_ModeView.GetComponentInChildren<ToggleGroup>();

            m_SearchField = this.transform.SearchComponent<InputField>("SearchField");
            m_SearchField.onValueChanged.AddListener((search) =>
            {
                FilterSearchItems(search);
            });

            m_SearchClear = this.transform.SearchComponent<Button>("ClearButton");
            m_SearchClear.onClick.AddListener(() =>
            {
                FilterSearchItems("");
            });
            this.transform.FindAndFixMasks();
        }

        private void OnEnable()
        {
            if (m_Initialised == false)
            {
                this.WaitUntil(1, () => FixtureContainer.Instance.AssetsLoaded == true, () =>
                {
                    var fixtureView = this.transform.Search("FixtureView");
                    var fixtureViewContent = fixtureView.Search("Content");
                    m_Initialised = true;


                    var toggleGroup = fixtureView.GetComponentInChildren<ToggleGroup>();

                    AddButtonSetup();
                    AutoAddButtonSetup();
                    var itemCount = FixtureContainer.Instance.GetFixtureList().Count;
                    for (int i = 0; i < itemCount; i++)
                    {
                        int localIndex = i;
                        var createdItem = CachedObjects.Instance.InstantiateCachedGameObject(FIXTURE_PREFAB, fixtureViewContent.transform);
                        createdItem.GetComponent<Toggle>().group = toggleGroup;
                        TextMeshProUGUI item = createdItem.GetComponentInChildren<TextMeshProUGUI>();
                        item.text = FixtureContainer.Instance.GetFixtureList()[i];

                        m_FixtureList.Add(item);

                        var fake = createdItem.ForceComponent<FakeButton>();
                        fake.onClick.AddListener(() =>
                        {
                            m_SelectedFixtureIndex = localIndex;
                            FillModeItems();
                        });

                        Button infoButton = createdItem.transform.SearchComponent<Button>("InfoButton");
                        var localFixture = FixtureContainer.Instance.GetFixture(localIndex);
                        infoButton.onClick.AddListener(() =>
                        {
                            infoButton.image.color = SELECTED;
                            OscControllerRoot.Instance.FixtureWebInfoPanelRef.SetFixtureDetails(localFixture, () =>
                            {
                                infoButton.image.color = UN_SELECTED;
                            });                         
                        });

                        if (localFixture.Links != null)
                        {
                            infoButton.interactable = true;
                            infoButton.interactable |= (localFixture.Links.Manual != null && localFixture.Links.Manual.Count > 0);
                            infoButton.interactable |= (localFixture.Links.Other != null && localFixture.Links.Other.Count > 0);
                            infoButton.interactable |= (localFixture.Links.ProductPage != null && localFixture.Links.ProductPage.Count > 0);
                            infoButton.interactable |= (localFixture.Links.Video != null && localFixture.Links.Video.Count > 0);
                        }
                        else
                        {
                            infoButton.interactable = false;
                        }
                    }
                    FillModeItems();
                });
            }
        }

        private void FilterSearchItems(string search)
        {
            if(string.IsNullOrEmpty(search) == true)
            {
                foreach (TextMeshProUGUI item in m_FixtureList)
                {
                    item.transform.parent.SetActive(true);
                }
            }
            else
            {
                foreach (TextMeshProUGUI item in m_FixtureList)
                {
                    bool inSearch = item.text.CaseInsensitiveContains(search);
                    item.transform.parent.SetActive(inSearch);
                }
            }

        }

        private void AddButtonSetup()
        {
            m_AddItem = this.transform.SearchComponent<Button>("AddItem");
            m_AddItem.onClick.AddListener(() =>
            {
                UsedFixtureData newData = new UsedFixtureData();
                newData.m_Fixture = FixtureContainer.Instance.GetFixture(m_SelectedFixtureIndex);
                newData.m_JsonFileName = FixtureContainer.Instance.GetFixtureName(m_SelectedFixtureIndex);
                newData.m_ModeIndex = m_SelectedMode;
                try
                {
                    newData.m_ChannelCount = newData.m_Fixture.Modes[newData.m_ModeIndex].Channels.Count;
                }
                catch(Exception e)
                {
                    // think some of the data is broken , and want to track it down 
                    Debug.LogError($"something went wromg here, repeat and check the above data  {e}");
                }

                newData.m_DmxStartIndex = DataContainer.Instance.FindNextFreeSpace(1, newData.m_ChannelCount);
                DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Add(newData);
                DataContainer.Instance.CurrentUsedFixtureData = newData;
                OscControllerRoot.Instance.GridPanelRef.UpdateItems();
                DataContainer.Instance.CurrentUsedFixtureData = newData;
            });
        }

        private void AutoAddButtonSetup()
        {
            Button autoAdd = this.transform.SearchComponent<Button>("AutoAdd");
            autoAdd.onClick.AddListener(() =>
            {
                OscGenericPromptMenu.Instance.ShowYesNoMenu("AutoFind All", () =>
                {
                    DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Clear();
                    OscControllerRoot.Instance.GridPanelRef.UpdateItems();

                    var listOfItems = HouseBuilder.HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList;
                    foreach(CatalogueGeneratedData item in listOfItems)
                    {
                        if (item.InteractionRef.LightingRef != null)
                        {
                            UsedFixtureData newData = new UsedFixtureData();
                            newData.m_Fixture = item.InteractionRef.LightingRef.ContentLightingMetaDataRef.m_Fixure;
                            newData.m_DmxStartIndex = item.InteractionRef.LightingRef.DmxStartNumber;
                            newData.m_ModeIndex = item.InteractionRef.LightingRef.ModeIndex;
                            newData.m_ChannelCount = newData.m_Fixture.Modes[newData.m_ModeIndex].Channels.Count;
                            newData.m_JsonFileName = item.InteractionRef.LightingRef.ContentLightingMetaDataRef.m_JsonFileName;

                            // only add if the DmxStartNumber has not been added before 
                            if (null == DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Find(e => (e.m_DmxStartIndex == newData.m_DmxStartIndex) && (e.m_Fixture == newData.m_Fixture)))
                            {                           
                                DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Add(newData);
                            }
                        }
                    }

                    OscControllerRoot.Instance.GridPanelRef.UpdateItems();
                });
            });

            if(HouseBuilder.HouseBuilderRoot.Instance == null)
            {
                autoAdd.enabled = false;
            }
        }



        private void FillModeItems()
        {
            if (m_ModeViewContent != null)
            {
                var items = m_ModeViewContent.transform.GetDirectChildren();
                for (int i = 0; i < items.Count; i++)
                {
                    Destroy(items[i].gameObject);
                }

                var modes = FixtureContainer.Instance.GetModes(m_SelectedFixtureIndex);
                for (int i = 0; i < modes.Count; i++)
                {
                    int localIndex = i;
                    GameObject createdItem = CachedObjects.Instance.InstantiateCachedGameObject(FIXTURE_PREFAB, m_ModeViewContent.transform);
                    Toggle toggle = createdItem.GetComponent<Toggle>();
                    toggle.group = m_ModeToggleGroup;
                    createdItem.GetComponentInChildren<TextMeshProUGUI>().text = modes[i];

                    FakeButton fake = createdItem.ForceComponent<FakeButton>();
                    fake.onClick.AddListener(() =>
                    {
                        m_SelectedMode = localIndex;
                    });
                    if (i == 0)
                    {
                        toggle.isOn = true;
                    }

                    Button infoButton = createdItem.transform.SearchComponent<Button>("InfoButton");
                    infoButton.SetActive(false);
                }
            }

        }


    }
}
