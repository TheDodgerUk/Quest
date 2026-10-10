using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ContentPlacementMetaData;

public class RaycastData 
{
    private const int MAX_CORNERS = 8;
    public const float TINY_AMOUNT_OFFSET = 0.001f;

    public Ray[] m_Raycast = new Ray[MAX_CORNERS];
    public float[] m_RaycastLength = new float[MAX_CORNERS];
    public int m_RaycastArrayLength = MAX_CORNERS;

    Interaction m_Interaction;

    #region CalculateRaycasts
    public void CalculateRaycasts(HouseBuilder.InteractionWallGeneratedPoint wallPointRef, Interaction interaction)
    {
        m_Interaction = interaction;
        FillRayCastDirection(-m_Interaction.RootTransform.up);

        if (m_Interaction.PlacementMetaData.Placement.IsWall() == true)
        {
            CreateWallPlacementRays(m_Interaction.PlacementMetaData.Placement, wallPointRef);
        }

        if (m_Interaction.PlacementMetaData.Placement == EnumPlacement.Ceiling)
        {
            CreateCeilingRays();
            FillRayCastDirection(m_Interaction.RootTransform.up);
        }
    }



    private void FillRayCastDirection(Vector3 direction)
    {
        for (int i = 0; i < 8; i++)
        {
            m_Raycast[i].direction = direction;
        }
    }


    private void CreateWallPlacementRays(EnumPlacement placement, HouseBuilder.InteractionWallGeneratedPoint wallPointRef)
    {
        float wallThickness = wallPointRef.GeneratedWholeRef.FloorPlanJsonDataRef.m_RoomData.m_Depth;
        BoxCollider boxCollider = (BoxCollider)m_Interaction.MainCollider;
        if (placement.IsCutout())
        {
            CreatWallCutoutRays(boxCollider, wallThickness, placement);
        }
        else
        {
            if (placement == EnumPlacement.WallHanging)
            {
                CreatWallHangingRays(boxCollider, wallThickness, placement);
            }
            else
            {
                CreatWallMountedRays(boxCollider, wallThickness, placement);
            }
        }
    }


    private void CreateCeilingRays()
    {
        m_RaycastArrayLength = 4;

        BoxCollider boxCollider = (BoxCollider)m_Interaction.MainCollider;

        //bottom of item
        m_Raycast[0].origin = (new Vector3(-boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * 0.5f);
        m_Raycast[1].origin = (new Vector3(boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * 0.5f);
        m_Raycast[2].origin = (new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f);
        m_Raycast[3].origin = (new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f);



        // rotate to correct position
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin = m_Interaction.RootTransform.TransformPoint(m_Raycast[i].origin);
        }


        OffsetRaycastsViaParentsLocalPositions(m_RaycastArrayLength, boxCollider);

        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_RaycastLength[i] = float.PositiveInfinity;
        }

        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin += (-m_Raycast[i].direction * TINY_AMOUNT_OFFSET);
        }
    }


    private void CreatWallCutoutRays(BoxCollider boxCollider, float wallThickness, EnumPlacement placement)
    {
        m_RaycastArrayLength = 8;

        BoxCollider self = (BoxCollider)m_Interaction.MainCollider;
        float selfColliderSize = self.size.z;
        wallThickness = Mathf.Max(wallThickness, selfColliderSize);
        wallThickness *= 1.1f;

        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_RaycastLength[i] = wallThickness + (TINY_AMOUNT_OFFSET * 2); // this is for the offset later on 
        }


        m_Raycast[0].origin = (new Vector3(-boxCollider.size.x, -boxCollider.size.y, -wallThickness * 2) * 0.5f);
        m_Raycast[1].origin = (new Vector3(boxCollider.size.x, -boxCollider.size.y, -wallThickness * 2) * 0.5f);
        m_Raycast[2].origin = (new Vector3(-boxCollider.size.x, boxCollider.size.y, -wallThickness * 2) * 0.5f);
        m_Raycast[3].origin = (new Vector3(boxCollider.size.x, boxCollider.size.y, -wallThickness * 2) * 0.5f);

        m_Raycast[4].origin = (new Vector3(boxCollider.size.x, -boxCollider.size.y, wallThickness) * 0.5f);
        m_Raycast[5].origin = (new Vector3(-boxCollider.size.x, -boxCollider.size.y, wallThickness) * 0.5f);
        m_Raycast[6].origin = (new Vector3(boxCollider.size.x, boxCollider.size.y, wallThickness) * 0.5f);
        m_Raycast[7].origin = (new Vector3(-boxCollider.size.x, boxCollider.size.y, wallThickness) * 0.5f);

        if (placement.IsWallAndGrounded() == true)
        {
            // raise the bottom part of collider up a bit , will compensate if artist is slightly off with collider
            m_Raycast[0].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            m_Raycast[1].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            m_Raycast[4].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            m_Raycast[5].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
        }

        // rotate to correct position
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin = m_Interaction.RootTransform.TransformPoint(m_Raycast[i].origin);
        }

        OffsetRaycastsViaParentsLocalPositions(m_RaycastArrayLength, boxCollider);

        //// just move it a smidge more 
        for (int i = 0; i < 4; i++)
        {
            m_Raycast[i].origin -= (m_Interaction.RootTransform.forward * TINY_AMOUNT_OFFSET);
        }
        for (int i = 4; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin += (m_Interaction.RootTransform.forward * TINY_AMOUNT_OFFSET);
        }

        for (int i = 0; i < 4; i++)
        {
            m_Raycast[i].direction = m_Interaction.RootTransform.forward; // ones in front , face backwards
        }
        for (int i = 4; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].direction = -m_Interaction.RootTransform.forward; // ones in front , face backwards
        }


        if (placement.IsWallAndGrounded() == true)
        {
            // raise the bottom part of collider up a bit , will compensate if artist is slightly off with collider
            while (m_Raycast[0].origin.y <= 0)
            {
                m_Raycast[0].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            }

            while (m_Raycast[1].origin.y <= 0)
            {
                m_Raycast[1].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            }

            while (m_Raycast[4].origin.y <= 0)
            {
                m_Raycast[4].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            }

            while (m_Raycast[5].origin.y <= 0)
            {
                m_Raycast[5].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            }
        }
    }

    private void OffsetRaycastsViaParentsLocalPositions(int numerOfRaycasts, Collider collider)
    {
        for (int i = 0; i < numerOfRaycasts; i++)
        {
            var trans = collider.transform;
            while (m_Interaction.RootTransform != trans)
            {
                m_Raycast[i].origin += trans.localPosition.z * m_Interaction.RootTransform.forward;
                m_Raycast[i].origin += trans.localPosition.y * m_Interaction.RootTransform.up;
                m_Raycast[i].origin += trans.localPosition.x * m_Interaction.RootTransform.right;
                trans = trans.parent.transform;
            }
        }
    }


    private void CreatWallHangingRays(BoxCollider boxCollider, float wallThickness, EnumPlacement placement)
    {
        m_RaycastArrayLength = 2;
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_RaycastLength[i] = ((boxCollider.size.z) + wallThickness);
        }

        m_Raycast[0].origin = (new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f);
        m_Raycast[1].origin = (new Vector3(-boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f);

        AdjustWallItemRaycastPoints(boxCollider, placement);
    }

    private void CreatWallMountedRays(BoxCollider boxCollider, float wallThickness, EnumPlacement placement)
    {
        m_RaycastArrayLength = 4;
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_RaycastLength[i] = ((boxCollider.size.z) + wallThickness);
        }

        m_Raycast[0].origin = (new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f);
        m_Raycast[1].origin = (new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f);
        m_Raycast[2].origin = (new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f);
        m_Raycast[3].origin = (new Vector3(-boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f);

        AdjustWallItemRaycastPoints(boxCollider, placement);
    }

    private void AdjustWallItemRaycastPoints(BoxCollider boxCollider, EnumPlacement placement)
    {
        // rotate to correct position
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin = m_Interaction.RootTransform.TransformPoint(m_Raycast[i].origin);
        }

        OffsetRaycastsViaParentsLocalPositions(m_RaycastArrayLength, boxCollider);

        if (placement.IsWallAndGrounded() == true)
        {
            // raise the bottom part of collider up a bit , will compensate if artist is slightly off with collider
            m_Raycast[0].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
            m_Raycast[1].origin += Vector3.up * (TINY_AMOUNT_OFFSET * 2);
        }


        // just move it a smidge more 
        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].origin += (m_Interaction.RootTransform.forward * TINY_AMOUNT_OFFSET);
        }

        for (int i = 0; i < m_RaycastArrayLength; i++)
        {
            m_Raycast[i].direction = -m_Interaction.RootTransform.forward;
        }
    }

    #endregion


}
