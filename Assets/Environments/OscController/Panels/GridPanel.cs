using OscController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer.OscAllUsedFixtures;
using static OscController.OscControllerRoot;

public class GridPanel : MonoBehaviour
{
    public List<ImageMouseMove> m_Items = new List<ImageMouseMove>();
    private GridLayoutGroup m_GridLayoutGroup;
    private Vector2 m_GridItemSize;
    private Dictionary<int, GameObject> m_FullGridItems = new Dictionary<int, GameObject>();


    public void Initialise()
    {
        m_GridLayoutGroup = this.GetComponentInChildren<GridLayoutGroup>();

        var GridPrefab = this.transform.SearchComponent<Transform>("GridPrefab").gameObject;
        var rect = GridPrefab.GetComponent<RectTransform>();
        m_GridItemSize = rect.sizeDelta;


        for (int i = 0; i < MAX_DMX_INDEX; i++)
        {
            var newItem = Instantiate(GridPrefab, GridPrefab.transform.parent);
            int index = (i + 1);
            newItem.GetComponentInChildren<Text>().text = index.ToString();
            m_FullGridItems.Add(index, newItem.gameObject);
        }

        var prefab = CachedObjects.Instance.GetPrefab("GridItem");
        prefab.ForceComponent<ImageMouseMove>();
        GridPrefab.SetActive(false);
    }

   


    public void UpdateItems()
    {
        for(int i = 0; i < m_Items.Count; i++)
        {
            Destroy(m_Items[i].gameObject);
        }
        m_Items.Clear();

        int index = 0;


        // orders the list via m_DmxStartIndex
        DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Sort(delegate (UsedFixtureData x, UsedFixtureData y)
        {
            return x.m_DmxStartIndex.CompareTo(y.m_DmxStartIndex);
        });

        foreach (var item in DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList)
        {
            var newRootItem = CachedObjects.Instance.InstantiateCachedGameObject("GridItemPrefab");
            newRootItem.transform.SetParent(this.transform);
            newRootItem.transform.localScale = Vector3.one;

            SetCorrectoffsetPosition(item, newRootItem);

            var mov = newRootItem.ForceComponent<ImageMouseMove>();

            Text t = newRootItem.GetComponentInChildren<Text>();
            t.lineSpacing = 0; // stops it wrapping 
            if (item.m_Fixture !=null && string.IsNullOrEmpty(item.m_Fixture.ShortName) == false)
            {
                t.text = item.m_Fixture.ShortName;
            }
            else
            {
                t.text = item.m_Fixture.Name;
            }

            mov.OnRightClick.AddListener(() =>
            {
                Debug.LogError("Bring up menu, remove, color change, name change");
                OscGenericPromptMenu.Instance.ShowYesNoMenu("Do you wish to delete this item?", () =>
                {
                    RemoveItem(item);
                }, null);
                
            });

            mov.OnPointerEnd.AddListener(() =>
            {
                FindClosestGrid(newRootItem, item);
            });
            m_Items.Add(mov);



            var gridItem = m_FullGridItems[item.m_DmxStartIndex];
            newRootItem.transform.position = gridItem.transform.position;

            // assign grid colour
            if (item.m_Color == null)
            {
                int colourIndex = index % DataContainer.Colours.Count;
                newRootItem.GetComponentInChildren<Image>().color = DataContainer.Colours[colourIndex];
            }
            else
            {
                newRootItem.GetComponentInChildren<Image>().color = (Color)item.m_Color;
            }

            index++;
        }
    }

    private void SetCorrectoffsetPosition(UsedFixtureData item, GameObject newRootItem)
    {
        var imageRoot = newRootItem.transform.Search("ImageRoot").gameObject;
        var rect = imageRoot.GetComponent<RectTransform>();
        rect.sizeDelta = m_GridItemSize;
        var newSize = rect.sizeDelta;
        newSize.x *= item.m_ChannelCount;
        rect.sizeDelta = newSize;
        rect.localPosition = new Vector3((m_GridItemSize.x / 2) * (item.m_ChannelCount - 1), 0, 0);
    }

    private void RemoveItem(UsedFixtureData item)
    {
        DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Remove(item);
        if(DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Count > 0)
        {
            DataContainer.Instance.CurrentUsedFixtureData = DataContainer.Instance.AllUsedFixturesRef.m_UsedFixtureDataList.Last();
        }
        else
        {
            DataContainer.Instance.CurrentUsedFixtureData = null;
        }
        UpdateItems();
    }

    private void FindClosestGrid(GameObject obj, UsedFixtureData data)
    {
        int newDmx = 1;
        float distance = float.MaxValue;
        var objecPos = obj.gameObject.transform.position;
        foreach (var item in m_FullGridItems)
        {
            var newDi = Vector3.Distance(objecPos, item.Value.gameObject.transform.position);
            if(newDi < distance)
            {
                distance = newDi;
                newDmx = item.Key;
            }
        }
        data.m_DmxStartIndex = newDmx;
        data.m_DmxStartIndex = DataContainer.Instance.FindNextFreeSpace(data.m_DmxStartIndex, data.m_ChannelCount, data);
        obj.transform.position = m_FullGridItems[data.m_DmxStartIndex].gameObject.transform.position;
        DataContainer.Instance.CurrentUsedFixtureData = data;

        Debug.LogError($"Now on {newDmx}");
        UpdateItems();
    }
}
