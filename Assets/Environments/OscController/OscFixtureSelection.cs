using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OscController
{

    public class OscFixtureSelection : MonoBehaviour
    {
        public GameObject m_Root;
        public TMP_Dropdown m_Fixture;
        public TMP_Dropdown m_DmxMode;
        public Button m_Ok;

        public class FixturesAdded
        {
            public int m_StartIndex;
            public Fixtures.Fixture m_Fixture;
            public int m_Mode;
            public List<string> m_Channels = new List<string>();
        }


        public List<FixturesAdded> m_FixturesAdded = new List<FixturesAdded>();


        public void Initialise(GameObject root)
        {
            m_Root = root.transform.SearchComponent<Transform>("SelectFixture").gameObject;
            m_Fixture = m_Root.transform.SearchComponent<TMP_Dropdown>("Fixture");
            m_DmxMode = m_Root.transform.SearchComponent<TMP_Dropdown>("DmxMode");
            m_Ok = m_Root.transform.SearchComponent<Button>("Ok");

            m_Root.SetActive(false);
        }


        public void RightClick(int index, Action callback)
        {
            m_Root.SetActive(true);

            m_Fixture.ClearOptions();
            m_Fixture.AddOptions(FixtureContainer.Instance.GetFixtureList());
            FixtureSelected(0);
            m_Fixture.onValueChanged.AddListener(FixtureSelected);
            m_DmxMode.ClearOptions();

            m_Ok.onClick.RemoveAllListeners();
            m_Ok.onClick.AddListener(delegate { ClickedOk(m_Fixture.value, m_DmxMode.value, index, callback); });

            Debug.LogError($"RightClick  {index}");
        }



        private void ClickedOk(int fixture, int mode, int clickIndex, Action callback)
        {           
            FixturesAdded newData = new FixturesAdded();
            newData.m_StartIndex = clickIndex;
            newData.m_Fixture = FixtureContainer.Instance.GetFixture(fixture);
            newData.m_Mode = mode;
            newData.m_Channels = FixtureContainer.Instance.GetModeChannels(fixture, mode);
            m_FixturesAdded.Add(newData);
            m_Root.SetActive(false);
            callback.Invoke();
        }

        private void FixtureSelected(int arg0)
        {
            var fixture = FixtureContainer.Instance.GetFixture(arg0);
            m_DmxMode.ClearOptions();
            m_DmxMode.AddOptions(FixtureContainer.Instance.GetModes(FixtureContainer.Instance.GetFixtureName(arg0)));
        }


    }
}
