using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class SnappingPoints
    {
        private const float JUST_AWAY_FROM_EDGE = 0.9f;
        private const float IS_SAME_ANGLE = 1;

        private  List<GameObject> m_SnappingList = new List<GameObject>();
        private GameObject m_SnappingPointHolder;

        public bool EnableSnapping { get; set; }

        public void SetupPoints(GameObject root, ContentPlacementMetaData placementMetaData, Collider mainCollider)
        {
            CreateHolder(root);

            EnableSnapping = true;
            var enumItems = placementMetaData.UnPackSnappingPointField();
            foreach (var enumItem in enumItems)
            {
                Vector3 centre = mainCollider.bounds.center;
                Vector3 extends = mainCollider.bounds.extents;
                if (placementMetaData.Placement.IsWall())
                {
                    WallPoints(root, enumItem, centre, extends);
                }
                else
                {
                    if (placementMetaData.Placement.IsGroundedIncludingWall())
                    {
                        GroundedPoints(root, enumItem, centre, extends);
                    }
                }
            }
        }

        public void SetupPointsForWall(GameObject root, ContentPlacementMetaData.SnappingPointEnum data)
        {
            CreateHolder(root);
            GroundedPoints(root, data, Vector3.zero, Vector3.zero);
            GroundedPoints(root, data, Vector3.zero, Vector3.zero);
        }

        private void CreateHolder(GameObject root)
        {
            if (m_SnappingPointHolder == null)
            {
                m_SnappingPointHolder = new GameObject("SnappingPointHolder");
                m_SnappingPointHolder.transform.SetParent(root.transform);
            }
        }

        private void WallPoints(GameObject root, ContentPlacementMetaData.SnappingPointEnum enumItem, Vector3 centre, Vector3 extends)
        {
            if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Right || enumItem == ContentPlacementMetaData.SnappingPointEnum.Left)
            {
                List<Vector3> right = new List<Vector3>();
                float positionY = extends.y * JUST_AWAY_FROM_EDGE;
                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Right)
                {
                    right.Add(new Vector3(extends.x, centre.y + positionY, 0));
                    right.Add(new Vector3(extends.x, centre.y, 0));
                    right.Add(new Vector3(extends.x, centre.y - positionY, 0));
                    SetSnapping(root, right, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Right);
                }

                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Left)
                {
                    List<Vector3> left = new List<Vector3>();
                    left.Add(new Vector3(-extends.x, centre.y + positionY, 0));
                    left.Add(new Vector3(-extends.x, centre.y, 0));
                    left.Add(new Vector3(-extends.x, centre.y - positionY, 0));
                    SetSnapping(root, left, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Left);
                }
            }

            if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleY || enumItem == ContentPlacementMetaData.SnappingPointEnum.Top || enumItem == ContentPlacementMetaData.SnappingPointEnum.Bottom)
            {
                float positionTopY = +extends.y;
                float positionX = extends.x * JUST_AWAY_FROM_EDGE;
                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleY || enumItem == ContentPlacementMetaData.SnappingPointEnum.Top)
                {
                    List<Vector3> up = new List<Vector3>();
                    up.Add(new Vector3(centre.x + positionX, positionTopY, 0));
                    up.Add(new Vector3(centre.x, positionTopY, 0));
                    up.Add(new Vector3(centre.x - positionX, positionTopY, 0));
                    SetSnapping(root, up, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Top);
                }

                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleY || enumItem == ContentPlacementMetaData.SnappingPointEnum.Bottom)
                {
                    float positionBottomY = -extends.y;
                    List<Vector3> down = new List<Vector3>();
                    down.Add(new Vector3(centre.x + positionX, positionBottomY, 0));
                    down.Add(new Vector3(centre.x, positionBottomY, 0));
                    down.Add(new Vector3(centre.x - positionX, positionBottomY, 0));
                    SetSnapping(root, down, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Bottom);
                }
            }
        }

        private void GroundedPoints(GameObject root, ContentPlacementMetaData.SnappingPointEnum enumItem, Vector3 centre, Vector3 extends)
        {
            float groundLevel = centre.y - extends.y;
            groundLevel += 0.01f;
            if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Right || enumItem == ContentPlacementMetaData.SnappingPointEnum.Left)
            {
                float positionZ = extends.z * JUST_AWAY_FROM_EDGE;
                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Right)
                {
                    List<Vector3> right = new List<Vector3>();
                    right.Add(new Vector3(extends.x, groundLevel, positionZ));
                    right.Add(new Vector3(extends.x, groundLevel, 0));
                    right.Add(new Vector3(extends.x, groundLevel, -positionZ));
                    SetSnapping(root, right, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Right);
                }

                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleX || enumItem == ContentPlacementMetaData.SnappingPointEnum.Left)
                {
                    List<Vector3> left = new List<Vector3>();
                    left.Add(new Vector3(-extends.x, groundLevel, positionZ));
                    left.Add(new Vector3(-extends.x, groundLevel, 0));
                    left.Add(new Vector3(-extends.x, groundLevel, -positionZ));
                    SetSnapping(root, left, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Left);
                }
            }

            if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleZ || enumItem == ContentPlacementMetaData.SnappingPointEnum.Forward || enumItem == ContentPlacementMetaData.SnappingPointEnum.Back)
            {
                float forward = extends.z;
                float positionX = extends.x * JUST_AWAY_FROM_EDGE;

                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleZ || enumItem == ContentPlacementMetaData.SnappingPointEnum.Forward)
                {
                    List<Vector3> up = new List<Vector3>();
                    up.Add(new Vector3(positionX, groundLevel, forward));
                    up.Add(new Vector3(0, groundLevel, forward));
                    up.Add(new Vector3(-positionX, groundLevel, forward));
                    SetSnapping(root, up, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Forward);
                }

                if (enumItem == ContentPlacementMetaData.SnappingPointEnum.MiddleZ || enumItem == ContentPlacementMetaData.SnappingPointEnum.Back)
                {
                    float backwards = -extends.z;
                    List<Vector3> down = new List<Vector3>();
                    down.Add(new Vector3(positionX, groundLevel, backwards));
                    down.Add(new Vector3(0, groundLevel, backwards));
                    down.Add(new Vector3(-positionX, groundLevel, backwards));
                    SetSnapping(root, down, ContentPlacementMetaData.SnappingPositionsEnum.Snapping_Position_Backward);
                }
            }
        }

        private void SetSnapping(GameObject root, List<Vector3> pos, ContentPlacementMetaData.SnappingPositionsEnum posEnum)
        {
            if (m_SnappingList.Find(e => e.name == posEnum.ToString()) != null)
            {
                // if already been added before 
                return;
            }

            foreach (var item in pos)
            {
                GameObject SnappingPoint = new GameObject(posEnum.ToString());
                SnappingPoint.transform.SetParent(m_SnappingPointHolder.transform);

                Vector3 direction = ContentPlacementMetaData.FaceDirection(root, posEnum);
                SnappingPoint.transform.rotation = Quaternion.LookRotation(direction);
                SnappingPoint.transform.localPosition = item;
                m_SnappingList.Add(SnappingPoint);
            }
        }


        public void SnappingWallObjects(Interaction owner, ref Vector3 gameObjectNewPosition)
        {
            if (EnableSnapping == false)
            {
                return;
            }
            if (owner.RootTransform == null)
            {
                return;
            }
            if (owner.PlacementMetaData.Placement.IsWall() == false)
            {
                return;
            }
            if (owner.PlacementMetaData.m_SnapToOthers == false)
            {
                return;
            }

            foreach (var thisPoint in m_SnappingList)
            {
                Collider[] allHitObjects = FindClosestCollidersForDistance(owner, thisPoint);

                foreach (var hitObject in allHitObjects)
                {
                    var hitInteraction = hitObject.GetComponentInParent<InteractionGameObjectMove>();

                    if (hitInteraction == null || hitInteraction.RootGameObject == owner.RootGameObject)
                    {
                        continue;
                    }

                    foreach (var hitSnapPoint in hitInteraction.SnappingPointsRef.m_SnappingList)
                    {
                        if (ContentPlacementMetaData.Match(owner.RootGameObject, thisPoint.name, hitInteraction.RootGameObject, hitSnapPoint.name) == true)
                        {
                            var snapAmount = (hitSnapPoint.transform.position - thisPoint.transform.position);

                            Vector3 planeHit = Vector3.zero;
                            if (InputUtility.LinePlaneIntersectionDualDirection(out planeHit, thisPoint.transform.position, thisPoint.transform.right, hitSnapPoint.transform.right, hitSnapPoint.transform.position) == true)
                            {
                                snapAmount = (planeHit - thisPoint.transform.position);
                            }
                            else if (InputUtility.LinePlaneIntersectionDualDirection(out planeHit, thisPoint.transform.position, -thisPoint.transform.right, -hitSnapPoint.transform.right, hitSnapPoint.transform.position) == true)
                            {
                                snapAmount = (planeHit - thisPoint.transform.position);
                            }

                            gameObjectNewPosition += snapAmount;
                            owner.RootTransform.position += snapAmount;
                            owner.SnappedToInteraction = hitInteraction;
                            return;
                        }
                    }

                }
            }
        }


        public void SnappingGroundObjects(Interaction owner, ref Vector3 gameObjectNewPosition, bool validRayHitPoint)
        {
            if (EnableSnapping == false)
            {
                return;
            }
            if (owner.RootTransform == null)
            {
                return;
            }
            if (owner.PlacementMetaData.Placement.IsWall() == true)
            {
                return;
            }
            if (owner.PlacementMetaData.m_SnapToOthers == false)
            {
                return;
            }

            if (validRayHitPoint == true)
            {
                owner.RootTransform.position = gameObjectNewPosition; // this sorts out all the weird issues
            }

            if (FindClosestItems(owner, out RaycastHit rayHit, out GameObject from) == true)
            {
                foreach (var item in owner.SnappedToInteraction.SnappingPointsRef.m_SnappingList)
                {
                    if (HasValidSnappingPointAttached(item, rayHit) == true)
                    {
                        var snapAmount = (rayHit.point - from.transform.position);
                        gameObjectNewPosition += snapAmount;

                        float angle = float.MaxValue;
                        foreach (var snapPoint in m_SnappingList)
                        {
                            float a = Vector3.Angle(-snapPoint.transform.forward, rayHit.normal);
                            if (a < Mathf.Abs(angle))
                            {
                                angle = a;
                                if (Vector3.Cross(snapPoint.transform.forward, rayHit.normal).y < 0)
                                {
                                    angle = -a;
                                };

                            }
                        }
                        owner.RootTransform.Rotate(Vector3.up, -angle);
                        // 
                        return;
                    }
                }
            }
        }

        private bool HasValidSnappingPointAttached(GameObject obj, RaycastHit ray)
        {
            var angle = Vector3.Angle(obj.transform.forward, ray.normal);
            return (angle < IS_SAME_ANGLE);
        }

        private bool FindClosestItems(Interaction owner, out RaycastHit rayHitObjectReturn, out GameObject gameObjectFrom)
        {
            rayHitObjectReturn = new RaycastHit();
            gameObjectFrom = null;

            var distance = float.MaxValue;
            foreach (var thisPoint in this.m_SnappingList) // loop though all the snap points on object i am moving 
            {
                // get objects the snap point i hit
                Collider[] everythigHit = FindClosestCollidersForDistance(owner, thisPoint);

                foreach (var hitObject in everythigHit)
                {
                    // make sure its not my self
                    Interaction hitInteraction = hitObject.GetComponentInParent<InteractionGameObjectMove>();
                    if (hitInteraction == null || hitInteraction.RootGameObject == owner.RootGameObject)
                    {
                        hitInteraction = hitObject.GetComponentInParent<HouseBuilder.InteractionWallGeneratedPoint>();
                        if (hitInteraction == null)
                        {
                            continue;
                        }
                    }


                    if (ContentPlacementMetaData.RaycastHit(thisPoint, hitInteraction, out RaycastHit rayHitObject) == true)
                    {
                        float thisDistance = Vector3.Distance(rayHitObject.point, thisPoint.transform.position);
                        if (thisDistance < distance)
                        {
                            distance = thisDistance;
                            gameObjectFrom = thisPoint;
                            owner.SnappedToInteraction = hitInteraction;
                            rayHitObjectReturn = rayHitObject;
                        }
                    }
                }
            }
            return (owner.SnappedToInteraction != null);
        }


        private Collider[] FindClosestCollidersForDistance(Interaction owner, GameObject thisPoint)
        {
            Interaction closest = owner;
            var allHitObjects = Physics.OverlapSphere(thisPoint.transform.position, owner.PlacementMetaData.m_SnappingDistance);
            foreach (var hitObject in allHitObjects)
            {
                var hitInteraction = hitObject.GetComponentInParent<InteractionGameObjectMove>();

                if (hitInteraction == null || hitInteraction.RootGameObject == owner.RootGameObject)
                {
                    continue;
                }

                // get closest as some will be tiny 
                if (hitInteraction.PlacementMetaData.m_SnappingDistance < closest.PlacementMetaData.m_SnappingDistance)
                {
                    closest = hitInteraction;
                }
            }


            if (closest != owner)
            {
                allHitObjects = Physics.OverlapSphere(thisPoint.transform.position, owner.PlacementMetaData.m_SnappingDistance);
            }

            return allHitObjects;
        }

    }
}
