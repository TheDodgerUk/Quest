using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{

    public class MacroPanel
    {
        private MacroPanelMono m_MacroPanelMono;
        public MacroPanel(MacroPanelMono macroPanelMono)
        {
            m_MacroPanelMono = macroPanelMono;           
        }

        public void Initialise() => m_MacroPanelMono.Initialise();
        public bool IsActive() => m_MacroPanelMono.isActiveAndEnabled;

        public void SetActiveAndAssign(bool active)
        {
            m_MacroPanelMono.SetActive(active);
            m_MacroPanelMono.CreateOptions();
        }

    }


    public class MacroPanelMono : MonoBehaviour
    {
        private const string MACRO_PREFAB = "MacroChoicePrefab";
        private const string EFFECTS = "Effects";
        private const string CHANNEL = "Channel";

        private enum EffectTypes
        {
            Fan,

        }

        private GameObject m_ModeView;
        private GameObject m_ModeViewContent;
        private ToggleGroup m_ModeToggleGroup;


        private SimpleDictionaryList<Item, Item> m_Items = new SimpleDictionaryList<Item, Item>();
        private List<Item> m_Core = new List<Item>();



        public class Item
        {
            public GameObject m_GameObject;
            public Toggle m_Toggle;
            public string m_FullName;
            public TextMeshProUGUI m_Text;
            public GameObject m_MainCat;
        }
        public void Initialise()
        {
            m_ModeView = this.transform.Search("View").gameObject;
            m_ModeViewContent = m_ModeView.transform.Search("Content").gameObject;
            this.transform.FindAndFixMasks();;
            this.SetActive(false);
        }


        public void SetActiveAndAssign(bool active)
        {
            this.SetActive(active);
            CreateOptions();
        }

        public void CreateOptions()
        {
            if(m_ModeViewContent == null)
            {
                return;
            }
            var children = m_ModeViewContent.transform.GetDirectChildren();
            for(int i = 0; i < children.Count; i++)
            {
                Destroy(children[i].gameObject);
            }
            m_Items.ClearAll();
            m_Core.Clear();


            List<string> allItems = new List<string>();
            allItems.Add(EFFECTS);
            foreach (string effect in Enum.GetNames(typeof(EffectTypes)))
            {
                allItems.Add($"{EFFECTS}/{effect}");
            }

            allItems.Add(CHANNEL);
            var allSlider = OscControllerRoot.Instance.SliderPanelRef.AllSliderPrefabs();
            foreach(var item in allSlider)
            {
                string finalName = $"{CHANNEL}/{item.m_ChannelName}";
                if(allItems.Contains(finalName) == false)
                {
                    allItems.Add(finalName);
                }
            }

            for (int i = 0; i < allItems.Count; i++)
            {
                int localIndex = i;
                Item newItem = new Item();
                newItem.m_GameObject = CachedObjects.Instance.InstantiateCachedGameObject(MACRO_PREFAB, m_ModeViewContent.transform);
                newItem.m_Toggle = newItem.m_GameObject.GetComponent<Toggle>();
                newItem.m_Toggle.group = m_ModeToggleGroup;

                newItem.m_MainCat = newItem.m_GameObject.transform.SearchComponent<Transform>("MainCat").gameObject;

                newItem.m_Text = newItem.m_GameObject.GetComponentInChildren<TextMeshProUGUI>();
                newItem.m_Text.text = allItems[localIndex];
                var fake = newItem.m_GameObject.ForceComponent<FakeButton>();

                newItem.m_FullName = allItems[i];

                bool isSubCat = allItems[localIndex].Contains("/") == true;
                string subcatName = isSubCat ? allItems[localIndex].Split('/')[1] : "";
                string subMainName = isSubCat ? allItems[localIndex].Split('/')[0] : allItems[localIndex];

                if (isSubCat == true)
                {
                    newItem.m_MainCat.SetActive(false);
                    string search = subMainName;
                    var root = m_Core.Find(e => e.m_FullName == $"{subMainName}");
                    newItem.m_Text.text = $"          {subcatName}";
                    m_Items.AddToList(root, newItem);
                }
                else
                {
                    m_Items.AddToList(newItem);
                    m_Core.Add(newItem);
                }

                fake.onClick.AddListener(() =>
                {
                    ShowOnlyKeys();

                    if (isSubCat == false)
                    {
                        var root = m_Core.Find(e => e.m_FullName == $"{subMainName}");
                        m_Items.GetValuesList(root).ForEach(e => e.m_GameObject.SetActive(true));
                    }
                    else
                    {
                        switch (subMainName)
                        {
                            case EFFECTS:
                                Effects(subcatName);
                                break;
                            case CHANNEL:
                                Channel(subcatName);
                                break;
                        }

                        this.transform.SetActive(false);
                    }

                });
            }

            ShowOnlyKeys();
        }

        private void ShowOnlyKeys()
        {
            var keys = m_Items.GetKeys();
            foreach (var key in keys)
            {
                m_Items.GetValuesList(key).ForEach(e => e.m_GameObject.SetActive(false));
            }
            m_Core.ForEach(e => e.m_GameObject.SetActive(true));
        }

        private void Channel(string subname)
        {
            var allSlider = OscControllerRoot.Instance.SliderPanelRef.AllSliderPrefabs();
            var multiplySlider = OscControllerRoot.Instance.SliderPanelRef.MultipleSliders;;
            multiplySlider.Clear();

            foreach(var slider in allSlider)
            {
                if(slider.m_ChannelName == subname)
                {
                    multiplySlider.Add(slider);
                }
            }
        }


        private void Effects(string subname)
        {
            if(Enum.TryParse<EffectTypes>(subname, out EffectTypes result))
            {
                switch (result)
                {
                    case EffectTypes.Fan:
                        Fan();
                        break;
                }
            }
            else
            {
                Debug.LogError($"Bad parsing {subname}");
            }

        }

        private void Fan()
        {
            OscControllerRoot.Instance.SliderPanelRef.MultipleSliders.Sort();

            var items = OscControllerRoot.Instance.SliderPanelRef.MultipleSliders;
            var first = items.First().m_Slider.value;

            var last = items.Last().m_Slider.value;
            int count = items.Count;
            for (int i = 1; i < count - 1; i++)
            {
                var slider = OscControllerRoot.Instance.SliderPanelRef.MultipleSliders[i];
                float precentage = ((float)i) / ((float)(count - 1));
                slider.m_Slider.SetValueWithoutNotify(Mathf.Lerp(first, last, precentage));
                slider.m_SliderValue = slider.m_Slider.value;
            }
        }


    }

}


