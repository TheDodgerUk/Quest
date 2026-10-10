using System;
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
    public class SequencePanelOptions : MonoBehaviour
    {
        public const string ITEM_PREFAB = "SequenceOptionsItemPrefab";
        public const string NUMBER_PREFAB = "SequenceOptionsNumberPrefab";
        public const string TYPE_PREFAB = "SequenceOptionsTypePrefab";


        private PoolManagerLocalComponent<Transform> m_PoolManagerOptions;
        private ToggleGroup m_OptionsToggleGroup;
        private VerticalLayoutGroup m_OptionsVerticalLayoutGroup;


        private OscSequenceData m_CurrentOscSequenceData;

        private string m_CurrentOptionsName = "";
        private string m_CurrentOptionsType = "";
        

        private Button m_AllButton;
        private Button m_SingleButton;

        private Action m_Callback;

        private Transform m_TypeRoot;
        private Transform m_GroupFilterRoot;
        public void Initialise()
        {
            this.transform.FindAndFixMasks();

            AssignTypeOptions();



            var close = this.transform.SearchComponent<Button>("CloseButton");
            close.onClick.AddListener(() =>
            {
                m_Callback?.Invoke();
                this.gameObject.SetActive(false);
            });

            var ok = this.transform.SearchComponent<Button>("OkButton");
            ok.onClick.AddListener(() =>
            {
                m_CurrentOscSequenceData.m_OptionsName = m_CurrentOptionsName;
                m_CurrentOscSequenceData.m_OptionsType = m_CurrentOptionsType;

                m_CurrentOscSequenceData.m_TxtOptions.text = m_CurrentOscSequenceData.m_OptionsName;
                m_CurrentOscSequenceData.m_TxtOptionsAllOrSingle.text = m_CurrentOscSequenceData.m_OptionsType[0].ToString();
                DataContainer.Instance.CurrentSequenceData = m_CurrentOscSequenceData;
                OscControllerRoot.Instance.TimeLinePanelRef.FilterHeaders(DataContainer.Instance.CurrentSequenceData);
                m_Callback?.Invoke();
                this.gameObject.SetActive(false);
            });


        }


        private void AssignTypeOptions()
        {
            m_TypeRoot = this.transform.SearchComponent<Transform>("TypeRoot");
            m_AllButton = m_TypeRoot.transform.SearchComponent<Button>("AllButton");
            m_AllButton.onClick.AddListener(() =>
            {
                m_CurrentOptionsType = OscControllerRoot.OPTIONS_ALL;
                AllOrSingleData();
            });

            m_SingleButton = m_TypeRoot.transform.SearchComponent<Button>("SingleButton");
            m_SingleButton.onClick.AddListener(() =>
            {
                m_CurrentOptionsType = OscControllerRoot.OPTIONS_SINGLE;
                AllOrSingleData();
            });


            var options = m_TypeRoot.transform.SearchComponent<Transform>("OptionsGroup");
            m_OptionsVerticalLayoutGroup = options.GetComponentInChildren<VerticalLayoutGroup>();
            m_OptionsToggleGroup = m_OptionsVerticalLayoutGroup.GetComponent<ToggleGroup>();
            m_OptionsToggleGroup.allowSwitchOff = true;

            var itemPrefab = CachedObjects.Instance.GetPrefab(ITEM_PREFAB);
            m_PoolManagerOptions = new PoolManagerLocalComponent<Transform>(itemPrefab, m_OptionsVerticalLayoutGroup.transform);
        }


        public void FillInData(OscSequenceData localGroupData, Action callback)
        {           
            m_Callback = callback;
            m_CurrentOscSequenceData = localGroupData;
            m_CurrentOptionsName = m_CurrentOscSequenceData.m_OptionsName;
            m_CurrentOptionsType = m_CurrentOscSequenceData.m_OptionsType;
            TypeOptionsData();
            AllOrSingleData();
        }

        private void TypeOptionsData()
        {
            m_PoolManagerOptions.DeSpawnAll();
            List<string> names = new List<string>();

            names.Add(OscControllerRoot.OPTIONS_ALL);
            foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
            {
                if (names.Contains(item.m_JsonFileName) == false)
                {
                    names.Add(item.m_JsonFileName);
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
                    m_CurrentOptionsName = itemName;
                    Debug.Log($"Clicked {itemName}");
                    AllOrSingleData();
                });

                if (selectedToggle == null && m_CurrentOscSequenceData != null && m_CurrentOscSequenceData.m_OptionsName == itemName)
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



        private void AllOrSingleData()
        {
            m_AllButton.image.color = OscControllerRoot.UN_SELECTED;
            m_SingleButton.image.color = OscControllerRoot.UN_SELECTED;


            m_SingleButton.SetActive(m_CurrentOptionsName != OscControllerRoot.OPTIONS_ALL);


            switch (m_CurrentOptionsType)
            {
                case OscControllerRoot.OPTIONS_ALL:
                    m_AllButton.image.color = OscControllerRoot.SELECTED;
                    break;
                case OscControllerRoot.OPTIONS_SINGLE:
                    m_SingleButton.image.color = OscControllerRoot.SELECTED;
                    break;
                default:
                    break;
            }
        }
    }

}


