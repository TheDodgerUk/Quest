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
    public class SequenceRunTimePanel : MonoBehaviour
    {
        public class SequenceRunTimeClass
        {
            public GameObject m_Root;
            public Image m_RootImage;
            public LeftRightClick m_RootButton;
            public Text m_Text;
            public Image m_Flashing;
            public OscSequenceData m_OscSequenceData;
            public bool m_IsRepeating = false;
            public SequenceRunTimeGroupClass m_SequenceRunTimeGroupClass;
        }

        public class SequenceRunTimeGroupClass
        {
            public DataContainer.FilterGroup m_FilterGroup;
            public GameObject m_Root;
            public RectTransform m_RootRect;
            public GridLayoutGroup m_GridLayoutGroup;

            public RectTransform m_GroupRect;
            public Text m_GroupName;
            public Image m_GroupSpeedImage;
            public ImageMouseDifference m_ImageMouseDiffernce;
            public float m_GroupSpeedMultiplierStart = 1;
            public float m_GroupSpeedMultiplier = 1;
        }

        private const int NUMBER_OF_ROWS = 7; // one less than need to allow for the header part
        private const int NUMBER_OF_ROWS_WITH_HEADER = NUMBER_OF_ROWS +1; // one less than need to allow for the header part
        private const int GROUP_SPACING = 10;
        private const int PADDING = 5;

        private const string ITEM_PREFAB = "RuntimeSequenceItemPrefab";
        private const string GROUP_PREFAB = "RuntimeSequenceItemGroupPrefab";

        private PoolManager<SequenceRunTimeGroupClass> m_SequenceRunTimeGroupClass;
        private PoolManager<SequenceRunTimeClass> m_RunTimeSequenceClass;

        private RectTransform m_ItemPrefabRect;
        
        public enum ButtonTypes
        {
            All,
            None,
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
        }

        private Dictionary<ButtonTypes, PoolManagerLocalComponent<Image>> m_ButtonImages = new Dictionary<ButtonTypes, PoolManagerLocalComponent<Image>>();

        private HorizontalLayoutGroup m_HorizontalLayoutGroup;

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            this.transform.GetComponent<Image>().raycastTarget = false;
            m_HorizontalLayoutGroup = this.GetComponentInChildren<HorizontalLayoutGroup>();
            m_HorizontalLayoutGroup.spacing = GROUP_SPACING;

            GameObject itemPrefab = CachedObjects.Instance.GetPrefab(ITEM_PREFAB);
            m_RunTimeSequenceClass = new PoolManager<SequenceRunTimeClass>(itemPrefab, this.gameObject, onCreatNew, onSpawn, onDeSpawn);
            m_ItemPrefabRect = itemPrefab.GetComponent<RectTransform>();

          
            GameObject groupPrefab = CachedObjects.Instance.GetPrefab(GROUP_PREFAB);
            m_SequenceRunTimeGroupClass = new PoolManager<SequenceRunTimeGroupClass>(groupPrefab, this.gameObject, onCreatNewGroup, onSpawnGroup, onDeSpawnGroup);

            LoadButtons();
        }


        private void LoadButtons()
        {
            var item = CachedObjects.Instance.GetPrefab("CurvedButtonHolder");

            GameObject holder = new GameObject("CurvedButtonHolder Pool items");
            holder.transform.SetParent(this.gameObject.transform);

            Image image = item.transform.SearchComponent<Image>("Curved N");
            m_ButtonImages.Add(ButtonTypes.North, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved NE");
            m_ButtonImages.Add(ButtonTypes.NorthEast, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved E");
            m_ButtonImages.Add(ButtonTypes.East, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved SE");
            m_ButtonImages.Add(ButtonTypes.SouthEast, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved S");
            m_ButtonImages.Add(ButtonTypes.South, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved SW");
            m_ButtonImages.Add(ButtonTypes.SouthWest, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved W");
            m_ButtonImages.Add(ButtonTypes.West, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved NW");
            m_ButtonImages.Add(ButtonTypes.NorthWest, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved All");
            m_ButtonImages.Add(ButtonTypes.All, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));

            image = item.transform.SearchComponent<Image>("Curved None");
            m_ButtonImages.Add(ButtonTypes.None, new PoolManagerLocalComponent<Image>(image.gameObject, holder.transform));
        }

        private SequenceRunTimeClass onDeSpawn(SequenceRunTimeClass arg)
        {
            arg.m_Root.transform.SetParent(this.transform);
            arg.m_Root.SetActive(false);
            arg.m_RootButton.onLeftClick.RemoveAllListeners();
            arg.m_RootButton.onRightClick.RemoveAllListeners();
            return arg;
        }

        private SequenceRunTimeClass onSpawn(SequenceRunTimeClass arg)
        {
            arg.m_Root.transform.SetParent(m_HorizontalLayoutGroup.transform);
            arg.m_Root.SetActive(true);
            arg.m_IsRepeating = false;
            arg.m_Flashing.SetActive(arg.m_IsRepeating);
            arg.m_RootImage.color = OscControllerRoot.UN_SELECTED;
            arg.m_RootButton.onLeftClick.RemoveAllListeners();
            arg.m_RootButton.onRightClick.RemoveAllListeners();

            return arg;
        }

        private SequenceRunTimeClass onCreatNew(GameObject obj)
        {
            SequenceRunTimeClass item = new SequenceRunTimeClass();
            item.m_Root = obj;
            item.m_RootImage = item.m_Root.transform.SearchComponent<Image>("ImageRoot");
            item.m_RootButton = item.m_Root.ForceComponent<LeftRightClick>();
            item.m_Text = item.m_Root.GetComponentInChildren<Text>();
            item.m_Flashing = item.m_Root.transform.SearchComponent<Image>("Flashing");
            return item;
        }


        private SequenceRunTimeGroupClass onDeSpawnGroup(SequenceRunTimeGroupClass arg)
        {
            arg.m_Root.transform.SetParent(this.transform);
            arg.m_Root.SetActive(false);
            arg.m_ImageMouseDiffernce.OnDifference.RemoveAllListeners();
            arg.m_ImageMouseDiffernce.OnDifferenceEnd.RemoveAllListeners();
            return arg;
        }

        private SequenceRunTimeGroupClass onSpawnGroup(SequenceRunTimeGroupClass arg)
        {
            arg.m_Root.transform.SetParent(m_HorizontalLayoutGroup.transform);
            arg.m_Root.SetActive(true);
            return arg;
        }

        private SequenceRunTimeGroupClass onCreatNewGroup(GameObject obj)
        {
            SequenceRunTimeGroupClass item = new SequenceRunTimeGroupClass();
            item.m_Root = obj;
            item.m_RootRect = item.m_Root.GetComponent<RectTransform>();
            item.m_GridLayoutGroup = item.m_Root.transform.GetComponentInChildren<GridLayoutGroup>(true);

            item.m_Root.transform.SearchComponent<RectTransform>("Viewport").sizeDelta = Vector2.zero;
            item.m_Root.transform.SearchComponent<RectTransform>("Content").sizeDelta = Vector2.zero;

            item.m_GridLayoutGroup.cellSize = m_ItemPrefabRect.sizeDelta;
            item.m_GridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            item.m_GridLayoutGroup.constraintCount = NUMBER_OF_ROWS;
            item.m_GridLayoutGroup.padding.left = PADDING;
            item.m_RootRect.sizeDelta = new Vector2(m_ItemPrefabRect.sizeDelta.x, (NUMBER_OF_ROWS * m_ItemPrefabRect.sizeDelta.y));

            item.m_GroupRect = item.m_Root.transform.SearchComponent<RectTransform>("Group");
            item.m_GroupName = item.m_Root.transform.SearchComponent<Text>("GroupName");
            item.m_GroupSpeedImage = item.m_Root.transform.SearchComponent<Image>("Speed");
            item.m_ImageMouseDiffernce = item.m_GroupSpeedImage.gameObject.ForceComponent<ImageMouseDifference>();
            return item;
        }


        public void StopAllReapeats()
        {
            foreach (var item in DataContainer.Instance.SequenceDataRef)
            {
                OscControllerRoot.Instance.PlayDataRef.ForceStopSequenceRepeat(item, null, (runTimeSceneClass) =>
                {
                    if (runTimeSceneClass != null)
                    {
                        runTimeSceneClass.m_RootImage.color = OscControllerRoot.UN_SELECTED;
                    }
                });
            }
        }

        public void CreateDisplaySequenceItems()
        {
            m_SequenceRunTimeGroupClass.DeSpawnAll();
            m_RunTimeSequenceClass.DeSpawnAll();
            foreach (var item in m_ButtonImages)
            {
                item.Value.DeSpawnAll();
            }

            foreach (var filter in DataContainer.Instance.GenericDataDataRef.m_FilterGroupNames)
            {
                bool createItems = false;
                foreach (OscSequenceData item in DataContainer.Instance.SequenceDataRef)
                {
                    if (filter.m_GroupName == item.m_FilterGroup)
                    {
                        createItems = true;
                    }
                }
                if(createItems == true)
                {
                    SequenceRunTimeGroupClass createdGroupItem = m_SequenceRunTimeGroupClass.SpawnObject();
                    createdGroupItem.m_Root.transform.SetAsLastSibling();
                    createdGroupItem.m_FilterGroup = filter;
                    CreateItem(createdGroupItem);
                }                       
            }
        }


        private void CreateItem(SequenceRunTimeGroupClass group)
        {
            group.m_GroupSpeedMultiplierStart = group.m_GroupSpeedMultiplier;
            group.m_ImageMouseDiffernce.OnDifference.AddListener((differnce) =>
            {
                UpdateGroupSpeedMultiplier(group, differnce);
            });

            group.m_ImageMouseDiffernce.OnDifferenceEnd.AddListener((differnce) =>
            {
                UpdateGroupSpeedMultiplier(group, differnce);
                group.m_GroupSpeedMultiplierStart = group.m_GroupSpeedMultiplier;
            });




            // figure out the sizes of everything
            int count = 0;
            foreach (OscSequenceData item in DataContainer.Instance.SequenceDataRef)
            {
                if (group.m_FilterGroup.m_GroupName == item.m_FilterGroup)
                {
                    count++;
                }
            }
            int numberOfColumns = (int)((float)count / (float)(NUMBER_OF_ROWS_WITH_HEADER));
            numberOfColumns++;

            if(numberOfColumns > 1)
            {
                int remain = count % NUMBER_OF_ROWS_WITH_HEADER;
                if (remain == NUMBER_OF_ROWS)
                {
                    numberOfColumns++; 
                }
            }

            group.m_RootRect.sizeDelta = new Vector2((m_ItemPrefabRect.sizeDelta.x + PADDING) * numberOfColumns, (NUMBER_OF_ROWS_WITH_HEADER * m_ItemPrefabRect.sizeDelta.y));
            group.m_GroupRect.sizeDelta = new Vector2(group.m_RootRect.sizeDelta.x, group.m_GroupRect.sizeDelta.y);
            group.m_GroupName.text = group.m_FilterGroup.m_GroupName;

            int totalNumber = count;


            // go through all SequenceDataRef
            count = 0;
            foreach (OscSequenceData item in DataContainer.Instance.SequenceDataRef)
            {
                if (group.m_FilterGroup.m_GroupName == item.m_FilterGroup)
                {
                    SequenceRunTimeClass createdItem = m_RunTimeSequenceClass.SpawnObject();
                    createdItem.m_Root.transform.SetParent(group.m_GridLayoutGroup.transform);
                    createdItem.m_Root.transform.SetAsLastSibling();

                    createdItem.m_OscSequenceData = item;
                    createdItem.m_Text.text = item.m_SequenceName;
                    createdItem.m_SequenceRunTimeGroupClass = group; // needs to be a ref , so it will change on the fly 


                    AssignCorrectButtonImage(count, totalNumber, numberOfColumns, createdItem);

                    createdItem.m_RootButton.onLeftClick.AddListener(() =>
                    {
                        if (createdItem.m_IsRepeating == false)
                        {
                            createdItem.m_RootImage.color = OscControllerRoot.SELECTED;
                            OscControllerRoot.Instance.PlayDataRef.PlaySequence(createdItem, null, () =>
                            {
                                createdItem.m_RootImage.color = OscControllerRoot.UN_SELECTED;
                            });
                        }
                    });

                    createdItem.m_RootButton.onRightClick.AddListener(() =>
                    {
                        if (createdItem.m_IsRepeating == true)
                        {
                            OscControllerRoot.Instance.PlayDataRef.StopSequenceRepeat(createdItem);
                        }
                        else
                        {
                            OscControllerRoot.Instance.PlayDataRef.PlaySequenceRepeat(createdItem, () =>
                            {
                                createdItem.m_RootImage.color = OscControllerRoot.SELECTED;

                            }, () =>
                            {
                                Debug.LogError("finished");
                                createdItem.m_RootImage.color = OscControllerRoot.UN_SELECTED;
                            });
                        }

                        createdItem.m_IsRepeating = !createdItem.m_IsRepeating;
                        createdItem.m_Flashing.SetActive(createdItem.m_IsRepeating);
                    });
                    count++;
                }
            }

        }

        private void UpdateGroupSpeedMultiplier(SequenceRunTimeGroupClass group, Vector2 differnce)
        {
            float dif = differnce.x;
            dif /= 1000;
            group.m_GroupSpeedMultiplier = group.m_GroupSpeedMultiplierStart + dif;
            group.m_GroupSpeedMultiplier = Mathf.Clamp(group.m_GroupSpeedMultiplier, 0.1f, 2.0f);
            OscControllerRoot.Instance.SliderExtraPanelRef.SetMessage($"Speed * {group.m_GroupSpeedMultiplier.ToString("0.00")}");
        }

        private void AssignCorrectButtonImage(int count, int totalNumber, int numberOfColumns, SequenceRunTimeClass createdItem)
        {
            bool isEvenNumber = totalNumber % numberOfColumns == 0;
            Image image = null;
            if (numberOfColumns == 1)
            {
                if (count == 0)
                {
                    image = m_ButtonImages[ButtonTypes.North].SpawnObject();
                }
                else if (count == (totalNumber - 1))
                {
                    image = m_ButtonImages[ButtonTypes.South].SpawnObject();
                }
                else
                {
                    image = m_ButtonImages[ButtonTypes.None].SpawnObject();
                }      
                
                if(totalNumber == 1)
                {
                    image = m_ButtonImages[ButtonTypes.All].SpawnObject();
                }
            }
            else
            {

                // gos left to right then down , like a book 
                // first row
                if (count == 0)
                {
                    image = m_ButtonImages[ButtonTypes.NorthWest].SpawnObject();
                }

                if (count == (numberOfColumns -1))
                {
                    image = m_ButtonImages[ButtonTypes.NorthEast].SpawnObject();
                }


                if (isEvenNumber == true)
                {

                    if (count == (totalNumber - 2))
                    {
                        image = m_ButtonImages[ButtonTypes.SouthWest].SpawnObject();
                    }


                    if (count == (totalNumber - 1))
                    {
                        image = m_ButtonImages[ButtonTypes.SouthEast].SpawnObject();
                    }
                }
                else
                {
                    if (count == (totalNumber - 1))
                    {
                        image = m_ButtonImages[ButtonTypes.South].SpawnObject();
                    }

                    if (numberOfColumns == 2)
                    {
                        if (count == (totalNumber - numberOfColumns))
                        {
                            image = m_ButtonImages[ButtonTypes.SouthEast].SpawnObject();
                        }
                    }
                    else
                    {
                        if (count == (totalNumber - (numberOfColumns-1)))
                        {
                            image = m_ButtonImages[ButtonTypes.SouthEast].SpawnObject();
                        }
                    }
                }

                if(image == null)
                {
                    image = m_ButtonImages[ButtonTypes.None].SpawnObject();
                }
            }

            createdItem.m_RootImage.sprite = image.sprite;
            image.gameObject.SetActive(false);
        }
    }

}


