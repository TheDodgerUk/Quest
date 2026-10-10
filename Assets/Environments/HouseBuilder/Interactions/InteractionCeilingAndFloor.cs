using HouseBuilder;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static HouseBuilder.HouseBuilderConstants;

public class InteractionCeilingAndFloor : Interaction
{
    private MeshDoubleSidedData m_MeshData;
    private InteractionWallWhole m_InteractionWallWhole;

    public void Initialise(EPOOutline.Outlinable outLine, InteractionWallWhole interactionWallWhole)
    {
        m_Outline = outLine;
        this.gameObject.layer = Layers.VrInteractionLayer;
        m_InteractionWallWhole = interactionWallWhole;
    }

    public override void SelectObject()
    {
        m_Outline.SetOutLine(true);
        HouseBuilderRoot.Instance.OnNewCeilingFloorSelected(m_InteractionWallWhole);
    }

    public override void UnSelectObject()
    {
        m_Outline.SetOutLine(false);
        HouseBuilderRoot.Instance.OnCeilingFloorDeselected(m_InteractionWallWhole);
    }

    public void SetLookingDown()
    {
        CheckLinkage();
        m_MeshData.SetLookingDown();
    }

    public void SetLookingUp()
    {
        CheckLinkage();
        m_MeshData.SetLookingUp();
    }

    public void SetLookingViaCamera()
    {
        CheckLinkage();
        float pos = Camera.main.transform.position.y;
        if (pos > m_MeshData.m_Height)
        {
            m_MeshData.SetLookingDown();
        }
        else
        {
            m_MeshData.SetLookingUp();
        }
    }

    private void CheckLinkage()
    {
        if (m_MeshData == null)
        {
            m_MeshData = GetComponent<MeshDoubleSidedData>();
        }
    }
}
