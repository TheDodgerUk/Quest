using Fixtures;
using OscController;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer;
using static SliderPrefab;

namespace OscController
{
    public class SliderSimpleChoicePanel : MonoBehaviour
    {
        private const string CHOICE_PREFAB = "SliderSimpleChoicePrefab";

        private HorizontalLayoutGroup m_HorizontalLayoutGroup;
        private PoolManagerLocalComponent<Button> m_ChoicePool;

        public void Initialise()
        {
            var item = CachedObjects.Instance.InstantiateCachedGameObject(CHOICE_PREFAB);
            

            m_HorizontalLayoutGroup = this.GetComponentInChildren<HorizontalLayoutGroup>();
            m_ChoicePool = new PoolManagerLocalComponent<Button>(item, m_HorizontalLayoutGroup.transform);
            Destroy(item);
            ReGenerate();
        }

        private void OnEnable()
        {
            ReGenerate();
        }

        public void ReGenerate()
        {
            if (m_ChoicePool != null)
            {
                m_ChoicePool.DeSpawnAll();
                for(int i = 0; i < DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Count; i++)
                {
                    int localIndex = i;
                    bool validLayout = HasValidLayout(localIndex);             
                    ButtonText newButton = new ButtonText(m_ChoicePool.SpawnObject());

                    int colourIndex = localIndex % DataContainer.Colours.Count;
                    newButton.ButtonRef.image.color = DataContainer.Colours[colourIndex];
                    newButton.TextRef.text = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[localIndex].m_Fixture.ShortName;


                    // set alpha low , so can see the selected item 
                    if(DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[localIndex] != DataContainer.Instance.CurrentUsedFixtureData)
                    {
                        Color temp = newButton.ButtonRef.image.color;
                        temp.a = 0.2f;
                        newButton.ButtonRef.image.color = temp;
                    }

                    if (validLayout == false)
                    {
                        newButton.ButtonRef.image.color = Color.white;
                        newButton.TextRef.text = $"ERROR {newButton.TextRef.text}";
                    }



                    newButton.ButtonRef.transform.localScale = Vector3.one;
                    newButton.ButtonRef.onClick.RemoveAllListeners();
                    newButton.ButtonRef.onClick.AddListener(() =>
                    {
                        if (validLayout == true)
                        {
                            DataContainer.Instance.CurrentUsedFixtureData = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[localIndex];
                            OscControllerRoot.Instance.SliderSimplePanelRef.ReGenerate();
                            ReGenerate();
                        }
                        else
                        {
#if UNITY_EDITOR
                            UnityEditor.EditorApplication.Beep();
                            Debug.LogError($"no data been been assigned");
#endif
                        }
                    });
                }
            }
        }


        private bool HasValidLayout(int fixtureIndex)
        {
            var fixture = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList[fixtureIndex];

            if(fixture.m_Fixture.Layouts != null)
            {
                var modeChannels = fixture.m_Fixture.Modes[fixture.m_ModeIndex];
                Layout layout = fixture.m_Fixture.Layouts.Find(e => e.ShortName.ToLower() == modeChannels.ShortName.ToLower());
                if (layout == null)
                {
                    Debug.LogError($"Could not find layout: {modeChannels.ShortName}");
                    return false;
                }
                return true;

            }
            return false;
        }
        
    }
}
