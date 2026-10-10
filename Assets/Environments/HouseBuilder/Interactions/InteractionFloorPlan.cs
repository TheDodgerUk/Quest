using HouseBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;


public class InteractionFloorPlan : Interaction
{
    public void Initialise()
    {
        Core.Environment.OnEnvironmentLoadingComplete += InternalInitialise;
        if (null != HouseBuilderRoot.Instance)
        {
           // this.gameObject.layer = Layers.FXLayer;
        }
    }

    public override void Awake()
    {
        Initialise();
    }

    private void InternalInitialise()
    {
    }

    public override void MouseDown()
    {
    }

    public static InteractionWallWhole BeginNewWall(GameObject holder)
    {
        GameObject newWallCreation = new GameObject("WallCreation");
        Debug.Log("new WallCreation");
        InteractionWallWhole WallCreation = newWallCreation.ForceComponent<InteractionWallWhole>();
        WallCreation.Initialise(holder);
        return WallCreation;
    }


    private static void SaveData(HouseBuilderFloorPlanJsonData Data)
    {
        Debug.LogError(Data);
    }

    public static void CreateNewPartOffWall(Interaction existingWall)
    {
        Vector3 pos;
        if (true == Interaction.MousePointOnFloor(out pos))
        {
            existingWall.MouseDown();
        }
    }


    public override void MouseDrag()
    {
    }

    public override void MouseUp()
    {
    }

}
