using HouseBuilder;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionGameObject : Interaction
{
    public void SetCatalogueEntry(CatalogueEntry entry)
    {

        CatalogueEntryRef = entry;

        var data = CatalogueEntryRef.GetMetaData<ContentPlacementMetaData>();
        if (data == null)
        {
            data = new ContentPlacementMetaData();
        }
        this.SetPlacement(data);
        m_Outline = gameObject.ForceComponent<EPOOutline.Outlinable>();


        if (this.PlacementMetaData.m_OutlineOverrideGameObject != null)
        {
            m_Outline.Initialise(this.PlacementMetaData.m_OutlineOverrideGameObject);
        }
        else
        {
            m_Outline.Initialise();
        }

        var modData = CatalogueEntryRef.GetMetaData<ContentAdaptiveMetaData>();
        if (modData != null)
        {
            var box = (BoxCollider)MainCollider;
            box.size = BoundsRef.size;
        }

        SnappingPointsRef.SetupPoints(this.RootGameObject, PlacementMetaData, MainCollider);
    }

    public override void Awake()
    {
        base.Awake();
    }


    public override void SelectObject()
    {
        List<InteractionGameObject> obj = new List<InteractionGameObject>();

        switch (HouseBuilderRoot.Instance.CurrentMode)
        {
            case BuilderMode.Plot:
            case BuilderMode.Structure:
            case BuilderMode.Placement:
            case BuilderMode.Modular:
                foreach (var item in HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions())
                {
                    obj.Add((InteractionGameObject)item);
                }
                break;
            case BuilderMode.Sound:
                foreach (var item in HouseBuilderRoot.Instance.InputSound.GetCurrentInteractions())
                {
                    obj.Add((InteractionGameObject)item);
                }
                break;
            case BuilderMode.Lighting:
                foreach (var item in HouseBuilderRoot.Instance.InputLighting.GetCurrentInteractions())
                {
                    obj.Add((InteractionGameObject)item);
                }
                break;
            default:
                break;
        }


        HouseBuilderRoot.Instance.OnNewObjectSelected(obj);
        base.SelectObject();
    }

    public override void UnSelectObject()
    {
        HouseBuilderRoot.Instance.OnObjectDeselected();
        base.UnSelectObject();      
    }


    public void DeInitialiseOutline()
    {
        /////////m_Outline.DeInitialise();
    }

    [InspectorButton]
    public void RecalculateCollider()
    {      
        var box = (BoxCollider)MainCollider;

        // record correct placement
        Vector3 realPos = RootTransform.position;
        Quaternion realRot = RootTransform.rotation;

        // set to zero and get bounds
        RootTransform.position = Vector3.zero;
        RootTransform.rotation = Quaternion.identity;

        // do all stuff needed
        BoundsRef = this.gameObject.GetEncapsulatingBounds();
        box.transform.parent.ClearLocals();
        box.transform.localRotation = Quaternion.identity;
        box.transform.localPosition = new Vector3(0, BoundsRef.center.y, 0);
        box.center = Vector3.zero;
        box.size = BoundsRef.size;

        //place back to correct position
        RootTransform.position = realPos;
        RootTransform.rotation = realRot;
        m_Outline.Initialise();

        CollectStartingShadersData();
        CollectLightData();
    }
}
