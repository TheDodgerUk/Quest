using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{
    public class TimeLinePanelOptions : MonoBehaviour
    {
        public const string ITEM_PREFAB = "SequenceOptionsItemPrefab";
        public const string NUMBER_PREFAB = "SequenceOptionsNumberPrefab";
        


        private PoolManagerLocalComponent<Transform> m_PoolManagerOptions;
        private ToggleGroup m_OptionsToggleGroup;
        private VerticalLayoutGroup m_OptionsVerticalLayoutGroup;
        

        private VerticalLayoutGroup m_NumberVerticalLayoutGroup;
        private ToggleGroup m_NumberToggleGroup;
        private PoolManagerLocalComponent<Transform> m_PoolManagerNumber;


        private OscGroupData m_CurrentOscGroupData;

        private string m_OptionsName;
        private int m_OptionsDmxStart;
        public void Initialise()
        {
            this.transform.FindAndFixMasks();

            AssignOptions();
            AssignNumbers();

            var close = this.transform.SearchComponent<Button>("CloseButton");
            close.onClick.AddListener(() =>
            {
                this.gameObject.SetActive(false);
            });

            var ok = this.transform.SearchComponent<Button>("OkButton");
            ok.onClick.AddListener(() =>
            {
                m_CurrentOscGroupData.m_OptionsName = m_OptionsName;
                m_CurrentOscGroupData.m_OptionsDmxStart = m_OptionsDmxStart;
                m_CurrentOscGroupData.AssignTextOptions();
                this.gameObject.SetActive(false);
            });
        }


        private void AssignOptions()
        {
            var options = this.transform.SearchComponent<Transform>("OptionsGroup");
            m_OptionsVerticalLayoutGroup = options.GetComponentInChildren<VerticalLayoutGroup>();
            m_OptionsToggleGroup = m_OptionsVerticalLayoutGroup.GetComponent<ToggleGroup>();
            m_OptionsToggleGroup.allowSwitchOff = true;

            var itemPrefab = CachedObjects.Instance.GetPrefab(ITEM_PREFAB);
            m_PoolManagerOptions = new PoolManagerLocalComponent<Transform>(itemPrefab, m_OptionsVerticalLayoutGroup.transform);
        }


        private void AssignNumbers()
        {
            var number = this.transform.SearchComponent<Transform>("NumberGroup");
            m_NumberVerticalLayoutGroup = number.GetComponentInChildren<VerticalLayoutGroup>();
            m_NumberToggleGroup = m_NumberVerticalLayoutGroup.GetComponent<ToggleGroup>();
            m_NumberToggleGroup.allowSwitchOff = true;

            var numberPrefab = CachedObjects.Instance.GetPrefab(NUMBER_PREFAB);
            m_PoolManagerNumber = new PoolManagerLocalComponent<Transform>(numberPrefab, m_NumberVerticalLayoutGroup.transform);
        }


        public void FillInData(OscGroupData localGroupData)
        {
            m_CurrentOscGroupData = localGroupData;
            m_OptionsName = m_CurrentOscGroupData.m_OptionsName;
            m_OptionsDmxStart = m_CurrentOscGroupData.m_OptionsDmxStart;
            OptionsData();
        }

        private void OptionsData()
        {
            m_PoolManagerOptions.DeSpawnAll();
            List<string> names = new List<string>
            {
                OscControllerRoot.OPTIONS_ALL
            };
            foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
            {
                if(string.IsNullOrEmpty(item.m_JsonFileName) == false)
                {
                    if (names.Contains(item.m_JsonFileName) == false)
                    {
                        names.Add(item.m_JsonFileName);
                    }
                }
                else
                {
                    Debug.LogError($"item.m_FixtureName is null");
                }

            }


            Toggle selectedToggle = null;
            foreach (var item in names)
            {
                string itemName = item;
                var obj = m_PoolManagerOptions.SpawnObject(m_OptionsVerticalLayoutGroup.transform);
                obj.transform.localScale = Vector3.one;
                obj.GetComponentInChildren<Text>().text = itemName;

                Toggle toggle = MakeSelectable(obj, ref m_OptionsToggleGroup);

                var nameSelect = obj.gameObject.ForceComponent<FakeButton>();
                nameSelect.onClick.RemoveAllListeners();
                nameSelect.onClick.AddListener(() =>
                {
                    m_OptionsName = itemName;
                    toggle.Select();
                    NumberData(itemName);
                });

                if (selectedToggle == null && m_CurrentOscGroupData != null && m_OptionsName == itemName)
                {
                    selectedToggle = toggle;
                }
            }

            if (selectedToggle != null)
            {
                selectedToggle.isOn = true;
                selectedToggle.Select();
                NumberData(m_OptionsName);
            }
        }

        private void NumberData(string fixtureName)
        {
            m_PoolManagerNumber.DeSpawnAll();
            List<int> startIndex = new List<int>();

            foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
            {
                if(item.m_JsonFileName == fixtureName)
                {
                    startIndex.Add(item.m_DmxStartIndex);                  
                }
            }

            Toggle selectedToggle = null;
            foreach (var item in startIndex)
            {
                int itemNumber = item;
                string itemName = itemNumber.ToString();
                var obj = m_PoolManagerNumber.SpawnObject(m_NumberVerticalLayoutGroup.transform);
                obj.transform.localScale = Vector3.one;
                obj.GetComponentInChildren<Text>().text = itemName;

                Toggle toggle = MakeSelectable(obj, ref m_NumberToggleGroup);

                var numberSelected = obj.gameObject.ForceComponent<FakeButton>();
                numberSelected.onClick.RemoveAllListeners();
                numberSelected.onClick.AddListener(() =>
                {
                    m_OptionsDmxStart = itemNumber;
                });

                if (selectedToggle == null && m_CurrentOscGroupData != null && m_OptionsDmxStart == itemNumber)
                {
                    selectedToggle = toggle;
                }
            }

            if (selectedToggle != null)
            {
                selectedToggle.isOn = true;
                selectedToggle.Select();
            }
        }

        private Toggle MakeSelectable(Transform obj, ref ToggleGroup toggleGroup)
        {
            var toggle = obj.GetComponent<Toggle>();
            toggle.group = toggleGroup;

            ColorBlock col = toggle.colors;
            col.selectedColor = OscControllerRoot.SELECTED;
            toggle.colors = col;
            
            return toggle;
        }
    }

}


