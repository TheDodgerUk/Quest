using EPOOutline;
using HouseBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ContentPlacementMetaData;

public class Interaction : MonoBehaviour
{
    public static bool SnappingEnabled { get; set; }

    public class ShaderData
    {
        public MeshRenderer m_MeshRenderer;
        public ShaderUtils.BlendMode m_RenderMode;
        public float m_Alpha;
    }


    public const float TINY_AMOUNT_OFFSET = 0.001f;
    public const int FLOAT_TO_INT = (int)((1.0f / TINY_AMOUNT_OFFSET) / 10); // will be 100

    public static bool m_ShadowsOn = true;
    public ContentPlacementMetaData PlacementMetaData { get; private set; }
    public void SetPlacement(ContentPlacementMetaData placement)
    {
        PlacementMetaData = placement;
        PlacementMetaData.CollectData(this.transform);
    }

    public CatalogueEntry CatalogueEntryRef { get; protected set; }
    public Collider MainCollider { get; protected set; }
    public List<Collider> MainColliders { get; protected set; }
    public GameObject MainColliderGameObject { get; protected set; }

    public List<Collider> m_OtherColliders = new List<Collider>();
    public EPOOutline.Outlinable m_Outline;
    public GameObject RootGameObject => this.gameObject;
    public Transform RootTransform => this.transform;
    public Bounds BoundsRef { get; set; }
    public Vector3 RealWorldBoundsCentre() => RootTransform.position + BoundsRef.center;

    private int? m_DebugFailIndex = 0;


    protected List<UnityEngine.Rendering.ShadowCastingMode> m_OriginalShadowCastingModes = null;
    protected List<ShaderData> m_OriginalShaderSettings = null;
    protected List<Light> m_LightList = null;

    public Interaction CurrentSelectedInteraction;

    public List<VrInteraction> m_VrInteractions = new List<VrInteraction>();
   
    public Interaction SnappedToInteraction { get; set; }


    private SnappingPoints m_SnappingPoints = new SnappingPoints();
    public SnappingPoints SnappingPointsRef { get { return m_SnappingPoints; } }

    private Adaptive m_Adaptive;
    public Adaptive AdaptiveRef { get { return m_Adaptive; } }
    public void CreateAdaptive() => m_Adaptive = new Adaptive();

    private Scale m_Scale;
    public Scale ScaleRef { get { return m_Scale; } }
    public void CreateScale() => m_Scale = new Scale();

    private Modular m_Modular;
    public Modular ModularRef { get { return m_Modular; } }
    public void CreateModular() => m_Modular = new Modular();

    private Lighting m_Lighting;
    public Lighting LightingRef { get { return m_Lighting; } }
    public void CreateLighting() => m_Lighting = new Lighting();

    private Staging m_Staging;
    public Staging StagingRef { get { return m_Staging; } }
    public void CreateStaging() => m_Staging = new Staging();


    private VrSliders m_VRSliders;
    public VrSliders VRSlidersRef { get { return m_VRSliders; } }
    public void CreateVRSliders() => m_VRSliders = new VrSliders();


    private VrButtons m_VRButtons;
    public VrButtons VrButtonsRef { get { return m_VRButtons; } }
    public void CreateVRButtons() => m_VRButtons = new VrButtons();


    public Rigidbody RigidbodyRef;
    private RaycastData RaycastDataRef = new RaycastData();

    public SteamSound SteamSoundRef { get; private set; }
    public CatalogueGeneratedData CatalogueGeneratedDataRef { get; private set; }
    public void SetCatalogueGeneratedData(CatalogueGeneratedData data) => CatalogueGeneratedDataRef = data;

    public int m_RootItemsGeneratedGUID = -1;

    public virtual void Awake()
    {
        this.gameObject.layer = Layers.DefaultLayer;

        BoundsRef = this.gameObject.GetEncapsulatingBounds();

        if (PlacementMetaData == null)
        {
            PlacementMetaData = new ContentPlacementMetaData();
        }
        
        MainColliders = new List<Collider>();
        var colliderHolder = this.transform.Find(GlobalConsts.CollisionObj);
        if (colliderHolder != null)
        {
            MainColliders = colliderHolder.GetComponentsInChildren<Collider>().ToList();
            MainCollider = colliderHolder.GetComponentInChildren<Collider>();
            MainColliderGameObject = MainCollider.gameObject;
        }


        if (this is InteractionGameObject || this is InteractionWallGeneratedPoint || this is InteractionCeilingAndFloor)
        {
            SteamSoundRef = new SteamSound(this);
        }

        RigidbodyRef = GetComponentInChildren<Rigidbody>(true);
        if (MainCollider != null)
        {
            m_OtherColliders = GetComponentsInChildren<Collider>(true).ToList();
            foreach (var item in MainColliders)
            {
                m_OtherColliders.Remove(item);
            }


            foreach (var col in m_OtherColliders)
            {
                if (col.isTrigger != true)
                {
                    if (this.name.StartsWith(HouseBuilderConstants.CEILING_NAME) == false && this.name.StartsWith(HouseBuilderConstants.FLOOR_NAME) == false)
                    {
                        Debug.LogWarning($"{this.name} sub collider is not set true forcing it");
                    }
                    if (col is MeshCollider)
                    {
                        MeshCollider meshCollider = (MeshCollider)col;
                        if(meshCollider.convex == true)
                        {
                            meshCollider.isTrigger = true;
                        }                   
                    }
                    else
                    {
                        col.isTrigger = true;
                    }

                }

            }

            if ((this is InteractionWallGeneratedPoint || this is InteractionWallGeneratedWhole) == false)
            {

                if (MainCollider is BoxCollider)
                {
                    BoxCollider box = (BoxCollider)MainCollider;
                    if (box != null)
                    {
                        if (MainColliders.Count != 1)
                        {
                            if (box.center != Vector3.zero)
                            {
                                Debug.LogError($"box.center has to be Vector3.zero, and its the gameobject that moves: {this.name}", this);
                            }
                        }
                        if (box.isTrigger == false)
                        {
                            Debug.LogError($"box was not set to trigger {this.name}", this);
                            box.isTrigger = true;
                        }
                    }
                }
                else if (MainCollider is MeshCollider)
                {
                    MeshCollider mesh = (MeshCollider)MainCollider;
                    if (mesh != null)
                    {
                        if (mesh.convex == false)
                        {
                            Debug.LogError($"mesh was not set to convex, forced: {this.name}:     {this.gameObject.GetGameObjectPath()}", this);
                            mesh.convex = true;
                        }
                        if (mesh.isTrigger == false)
                        {
                            Debug.LogError($"mesh was not set to trigger {this.name}:     {this.gameObject.GetGameObjectPath()}", this);
                            mesh.isTrigger = true;
                        }
                    }
                }
            }
        }

        CollectStartingShadersData();
        CollectLightData();
    }



    protected void CollectLightData()
    {
        m_LightList = RootGameObject.GetComponentsInChildren<Light>().ToList();
        foreach (var item in m_LightList)
        {
            if (item.shadows != LightShadows.Soft)
            {
                Debug.LogWarning($"{this.name} light changing to LightShadows.Soft", item);
                item.shadows = LightShadows.Soft;
            }
        }
    }

    protected void CollectStartingShadersData()
    {
        var list = this.GetComponentsInChildren<MeshRenderer>().ToList();

        m_OriginalShaderSettings = new List<ShaderData>();

        for (int i = 0; i < list.Count; i++)
        {
            foreach (var mat in list[i].materials)
            {
                mat.shader = Shader.Find(mat.shader.name);
                ShaderData newSetting = new ShaderData();
                newSetting.m_MeshRenderer = list[i];
                newSetting.m_RenderMode = ShaderUtils.GetMode(newSetting.m_MeshRenderer.material);

                if (newSetting.m_MeshRenderer.material.HasProperty("_Color") == true)
                {
                    newSetting.m_Alpha = newSetting.m_MeshRenderer.material.color.a;
                }
                m_OriginalShaderSettings.Add(newSetting);
            }
        }
    }

    [InspectorButton]
    public void SetOriginalRenderMode()
    {
        m_OriginalShaderSettings.RemoveAll(e => e.m_MeshRenderer == null);
        foreach (var item in m_OriginalShaderSettings)
        {
            item.m_MeshRenderer.enabled = true;
            var mats = item.m_MeshRenderer.materials;
            for (int matIndex = 0; matIndex < mats.Length; matIndex++)
            {
                ShaderUtils.SetMode(ref mats[matIndex], item.m_RenderMode, item.m_Alpha);
            }
        }
    }

    public void EnableRenderer(bool enable)
    {
        m_OriginalShaderSettings.RemoveAll(e => e.m_MeshRenderer == null);
        foreach (var item in m_OriginalShaderSettings)
        {
            item.m_MeshRenderer.enabled = enable;
        }
    }

    public bool HasLights => (m_LightList.Count > 0);
    public void EnableLights(bool enable)
    {
        m_LightList.ForEach(e => e.enabled = enable);
    }

    public void DisableOutlineOverrideRender()
    {
        if(PlacementMetaData.m_OutlineOverrideRenderer != null)
        {
            PlacementMetaData.m_OutlineOverrideRenderer.enabled = false;
        }
    }



    public void SetLightsColour(Color colour)
    {
        m_LightList.ForEach(e => e.color = colour);
    }

    [InspectorButton]
    public void SetRenderModeFade()
    {
        for (int i = 0; i < m_OriginalShaderSettings.Count; i++)
        {
            if (m_OriginalShaderSettings[i] != null && m_OriginalShaderSettings[i].m_MeshRenderer != null && m_OriginalShaderSettings[i].m_MeshRenderer.materials != null)
            {
                var mats = m_OriginalShaderSettings[i].m_MeshRenderer.materials;
                for (int matIndex = 0; matIndex < mats.Length; matIndex++)
                {
                    ShaderUtils.SetMode(ref mats[matIndex], ShaderUtils.BlendMode.Fade, HouseBuilderConstants.GAMEOBJECT_FADE_AMOUNT);
                }
            }
        }
    }

    public void SetRenderModeFadeNonSound()
    {
        if(this.SteamSoundRef.AudioSoundType == ContentSteamAudioMetaData.SoundEnumType.None && this.PlacementMetaData.Placement.IsCutout() == false)
        {
            SetRenderModeFade();
        }
    }

    public void SetRenderModeFadeNonLighting()
    {
        if (this.LightingRef == null && this.PlacementMetaData.Placement.IsCutout() == false)
        {
            SetRenderModeFade();
        }
    }

    public void SetRenderModeFadeNonStaging()
    {
        if (this.StagingRef == null)
        {
            SetRenderModeFade();
        }
    }


    public static void SetGeneratedMeshRenderer(Renderer render)
    {
        if (Interaction.m_ShadowsOn == true)
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
        }
        else
        {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }


    public static bool MousePointOnFloor(out Vector3 pos)
    {
        Ray rayRef = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
        RaycastHit validRaycastHit = new RaycastHit();
        RaycastHit[] hit = Physics.RaycastAll(rayRef, Mathf.Infinity).OrderBy(h => h.distance).ToArray();

        for (int i = 0; i < hit.Length; i++)
        {
            if (null != hit[i].collider.gameObject.GetComponent<InteractionFloorPlan>())
            {
                validRaycastHit = hit[i];
                pos = validRaycastHit.point;
                return true;
            }
        }
        pos = validRaycastHit.point;
        return false;
    }

    public bool GetMouseAsWorldPointForceZero(bool testCorners, out Vector3 pos) => InternalRayToPoint(HouseBuilder.PositionGrid.Instance.MouseToRay(), true, testCorners, out pos, out GameObject initialHitGameObject);
    public bool RotationCheck(Vector3 offset, Vector3 rayDirection) => InternalRayToPoint(new Ray(RootTransform.position + offset, rayDirection), false, true, out Vector3 pos, out GameObject initialHitGameObject, isKeyboardInput:true);

    public bool KeyboardMove(Vector3 positionInRealWorld, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject)
    {
        Ray mouseRay = HouseBuilder.PositionGrid.Instance.KeyboardToRay(this, positionInRealWorld);
        return InternalRayToPoint(mouseRay, forceToZero: false, testCorners: true, out gameObjectNewPosition, out initialHitGameObject, isKeyboardInput:true);
    }

    public bool GetMouseAsWorldPoint(bool testCorners, Vector3? lastValidPlace, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject)
    {
        if (lastValidPlace != null)
        {
            Vector3 last = (Vector3)lastValidPlace;
            Ray mouseRay = HouseBuilder.PositionGrid.Instance.MouseToRay();
            mouseRay.origin += last;
            return InternalRayToPoint(mouseRay, false, testCorners, out gameObjectNewPosition, out initialHitGameObject);
        }
        else
        {
            Ray mouseRay = HouseBuilder.PositionGrid.Instance.MouseToRay();
            return InternalRayToPoint(mouseRay, false, testCorners, out gameObjectNewPosition, out initialHitGameObject);
        }
    }

    private bool InternalRayToPoint(Ray rayRef, bool forceToZero, bool testCorners, out Vector3 gameObjectNewPosition, out GameObject initialHitGameObject, bool isKeyboardInput = false)
    {
        SnappedToInteraction = null;
        initialHitGameObject = null;


        gameObjectNewPosition = RootTransform.position;



        RaycastHit validMouseRaycastHit = new RaycastHit();
        bool validRayHitPoint = false;
        Vector3 originalRayDirection = rayRef.direction;
        if (this.PlacementMetaData.Placement == EnumPlacement.Ceiling && isKeyboardInput == false)
        {
            rayRef = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
        }

        // reset
        if (m_Outline != null) // needed for plot mode
        {
            m_Outline.ItemSelected = Outliner.EnumItemSelected.Valid;
        }
  

        // get hitpoint for direct mouse position
        if (true == RayCastDetection(forceToZero, rayRef, float.PositiveInfinity, ref validMouseRaycastHit, ref initialHitGameObject))
        {
            validRayHitPoint = true;
            CurrentSelectedInteraction = ColourTheOutline(initialHitGameObject, true);
            gameObjectNewPosition = validMouseRaycastHit.point;

        }

        Vector3 reversedRayDirection = -rayRef.direction;
        if (this is InteractionGameObjectMove)
        {
            NonWallItemsCheck(ref rayRef, originalRayDirection, forceToZero, ref gameObjectNewPosition, ref initialHitGameObject, ref validMouseRaycastHit);
        }

        WallChecks(ref gameObjectNewPosition, ref validMouseRaycastHit);

        MountToAll(ref gameObjectNewPosition, ref validMouseRaycastHit);

        ColourTheOutline(initialHitGameObject, false);


        if (SnappingEnabled == true)
        {
            if (this is InteractionGameObjectMove)
            {
                SnappingPointsRef.SnappingWallObjects(this, ref gameObjectNewPosition);
                SnappingPointsRef.SnappingGroundObjects(this, ref gameObjectNewPosition, validRayHitPoint);
            }
        }

        //// if ray cast downwards
        //// it will reverse from the impact point, towards ceiling
        #region Ceiling
        if ((this.PlacementMetaData.Placement == EnumPlacement.Ceiling) && originalRayDirection.y < 0)
        {
            if (false == Physics.Raycast(validMouseRaycastHit.point, reversedRayDirection, out validMouseRaycastHit))
            {
                m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
                return validRayHitPoint;
            }
            rayRef.origin = validMouseRaycastHit.point;
            rayRef.origin += (-reversedRayDirection * TINY_AMOUNT_OFFSET); // move downwards vector // reverse the reserve
            rayRef.direction = reversedRayDirection;

            initialHitGameObject = validMouseRaycastHit.collider.gameObject;
            if (true == RayCastDetection(forceToZero, rayRef, float.PositiveInfinity, ref validMouseRaycastHit, ref initialHitGameObject))
            {
                validRayHitPoint = true;
                CurrentSelectedInteraction = ColourTheOutline(initialHitGameObject, true);
                gameObjectNewPosition = validMouseRaycastHit.point;
            }
        }

        #endregion Ceiling


        if (forceToZero == true)
        {
            gameObjectNewPosition.y = 0;
        }
        if (this is InteractionGameObjectMove)
        {
            RootTransform.position = gameObjectNewPosition;
        }

        if (initialHitGameObject == null) // hit nothing so invalid
        {
            if (m_Outline != null)
            {
                m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
            }
        }
        else
        {
            if (true == testCorners)
            {
                TestCorners(forceToZero, gameObjectNewPosition, initialHitGameObject, validMouseRaycastHit);
            }
        }



        if (validRayHitPoint == false)
        {
            //Debug.Log($"Valid click ? - {validRayHitPoint}");
            RayCastTester.Instance.SetRay(rayRef, 10);
        }
        return validRayHitPoint;
    }

    private void NonWallItemsCheck(ref Ray rayRef, Vector3 originalRayDirection, bool forceToZero, ref Vector3 gameObjectNewPosition, ref GameObject initialHitGameObject, ref RaycastHit validMouseRaycastHit)
    {
        RaycastHit validBoxRaycastHit = new RaycastHit();
        // check ground floor items in the possible way 
        if (PlacementMetaData.Placement.IsWall() == false)
        {
            // casts box shape off the item its self(collider shape)  along the same line as the mouse
            if (true == RayCastBoxDetection(forceToZero, rayRef, ref validBoxRaycastHit, ref initialHitGameObject))
            {
                Vector3 point = validMouseRaycastHit.point;
                point.y = validBoxRaycastHit.point.y;
                validMouseRaycastHit.point = point;
                gameObjectNewPosition = validMouseRaycastHit.point;
            }
            else
            {
                return;
            }

            // the box collider will hit something, maybe ground, but  prob an object in the way
            // the height of the impact is used

            var origin = gameObjectNewPosition;
            origin.y = Camera.main.transform.position.y;
            rayRef.origin = origin;

            if(PlacementMetaData.Placement == EnumPlacement.Ceiling)
            {
                if(originalRayDirection.y < 0) // ray downwards
                {
                    rayRef.direction = Vector3.down;  // raycast downwards 
                }
                else
                {
                    rayRef.direction = Vector3.up; // raycast updwards if looking upwards for ceiling  object 
                }
            }
            else
            {
                rayRef.direction = Vector3.down;
            }
            
            RayCastTester.Instance.SetRay(rayRef, 1);


            // this was to do another test , but if there is a object above it , like a shelf, it will put it on top of that 

            ////////////RaycastHit validBoxRaycastHitSecond = new RaycastHit();
            ////////////// The ray is now directly down at the mouse hit point but at height of the box collider hit, 
            ////////////// this checks every factor for putting object in right place, and hitting other objects
            ////////////if (true == RayCastBoxDetection(forceToZero, rayRef, ref validBoxRaycastHitSecond, ref initialHitGameObject))
            ////////////{
            ////////////    Vector3 point = validMouseRaycastHit.point;
            ////////////    point.y = validBoxRaycastHitSecond.point.y;
            ////////////    validMouseRaycastHit.point = point;
            ////////////    gameObjectNewPosition = validMouseRaycastHit.point;
            ////////////    if (validBoxRaycastHitSecond.distance <= 0) // this means it hits the object, but its was inside the object which does not give any useful information
            ////////////    {
            ////////////        RaycastHit second = new RaycastHit();
            ////////////        origin = rayRef.origin;
            ////////////        origin.y = Camera.main.transform.position.y;
            ////////////        rayRef.origin = origin;
            ////////////        RayCastBoxDetectionSecond(rayRef, float.PositiveInfinity, ref validBoxRaycastHitSecond, ref second); // raycasts and finds validBoxRaycastHitSecond, but this time with valid information

            ////////////        point = validMouseRaycastHit.point;
            ////////////        point.y = second.point.y;
            ////////////        validMouseRaycastHit.point = point;
            ////////////        gameObjectNewPosition = validMouseRaycastHit.point;
            ////////////    }
            ////////////}
        }
    }


    private void WallChecks(ref Vector3 gameObjectNewPosition, ref RaycastHit validRaycastHit)
    {
        if (this.PlacementMetaData.Placement.IsWall())
        {
            gameObjectNewPosition = validRaycastHit.point;
            float rot = Vector3.SignedAngle(Vector3.forward, validRaycastHit.normal, Vector3.up);
            RootTransform.position = validRaycastHit.point;      // this overrides the grid
            RootTransform.rotation = Quaternion.Euler(0, rot, 0); // Rotate to correct direction
        }

        if (this.PlacementMetaData.Placement.IsWallAndGrounded() == true)
        {
            // TODO very hacky 
            gameObjectNewPosition.y = 0;
            validRaycastHit.point = gameObjectNewPosition;
            RootTransform.position = validRaycastHit.point;
        }
    }

    private void MountToAll(ref Vector3 gameObjectNewPosition, ref RaycastHit validRaycastHit)
    {
        if (this.PlacementMetaData.Placement == EnumPlacement.MountToAll)
        {
            Vector3 newDirection = RootTransform.TransformDirection(validRaycastHit.normal);
            var angle = Vector3.Angle(Vector3.up, validRaycastHit.normal);

            RootTransform.transform.up = validRaycastHit.normal;
            RootTransform.Rotate(new Vector3(0, PlacementMetaData.m_MountToAllRotation, 0), Space.Self);
        }
    }
    

    private void TestCorners(bool forceToZero, Vector3 gameObjectNewPosition, GameObject hitGameObject, RaycastHit validRaycastHit)
    {
        InteractionWallGeneratedPoint wallPointRef = null;
        if (this.PlacementMetaData.Placement.IsWall() == true)
        {
            wallPointRef = hitGameObject.GetComponent<InteractionWallGeneratedPoint>();
            bool isWall = (null != wallPointRef);
            if (false == isWall)
            {
                if ((hitGameObject.name != HouseBuilder.HouseBuilderConstants.GENERATED_UPPER_HULL) && (hitGameObject.name != HouseBuilder.HouseBuilderConstants.GENERATED_LOWER_HULL))
                {
                    m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
                    return;
                }
            }
        }

        if (forceToZero == true)
        {
            return;
        }

        RaycastDataRef.CalculateRaycasts(wallPointRef, this);

        m_DebugFailIndex = null;

        if (SnappedToInteraction == null)
        {
            if (this is InteractionGameObjectRotate)
            {
                var item = this.GetComponent<InteractionGameObjectMove>();
                SnappedToInteraction = item.SnappedToInteraction;
            }
        }
        // this is a copy paste job, it simple  to read this way 
        if (PlacementMetaData.Placement.IsWall() == false)
        {
            if (PlacementMetaData.Placement == EnumPlacement.Ceiling)
            {
                AllCornersCeiling(forceToZero, gameObjectNewPosition, hitGameObject, validRaycastHit);
            }
            else
            {
                // this is done via normal collision 
            }
        }
        else
        {
            AllCornersWallItems(forceToZero, hitGameObject, validRaycastHit);
        }
    }

    public void TestWallObjectIsStillValid(InteractionWallGeneratedPoint generatedWallPoint)
    {
        RaycastDataRef.CalculateRaycasts(generatedWallPoint, this);
        RaycastHit test = new RaycastHit();
        AllCornersWallItems(false, generatedWallPoint.RootGameObject, test);
    }

    private void AllCornersCeiling(bool forceToZero, Vector3 gameObjectNewPosition, GameObject hitGameObject, RaycastHit validRaycastHit)
    {
        GameObject cornerHitGameObject = null;
        for (int i = 0; i < RaycastDataRef.m_RaycastArrayLength; i++)
        {
            bool valid = RayCastDetection(forceToZero, RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i], ref validRaycastHit, ref cornerHitGameObject);
            bool hitSnapping = false;
            if (SnappedToInteraction != null)
            {
                hitSnapping = (cornerHitGameObject == SnappedToInteraction.RootGameObject || hitGameObject == SnappedToInteraction.RootGameObject);
            }

            int point1 = (int)(gameObjectNewPosition.y * FLOAT_TO_INT);     // this sorts out the floating point errors you get 
            int point2 = (int)(validRaycastHit.point.y * FLOAT_TO_INT);     // this sorts out the floating point errors you get 
            if ((valid == false) || (point1 != point2))
            {
                if (hitSnapping == false)
                {
                    m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
                    RayCastDetection(forceToZero, RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i], ref validRaycastHit, ref cornerHitGameObject); // this is so i can follow through why it was invalid
                    m_DebugFailIndex = i;
                    RayCastTester.Instance.SetRay(RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i]);
                    return;
                }
            }
        }
    }

    private void AllCornersWallItems(bool forceToZero, GameObject hitGameObject, RaycastHit validRaycastHit)
    {
        GameObject cornerHitGameObject = null;
        for (int i = 0; i < RaycastDataRef.m_RaycastArrayLength; i++)
        {
            bool valid = RayCastDetection(forceToZero, RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i], ref validRaycastHit, ref cornerHitGameObject);
            bool hitSnapping = false;
            if (SnappedToInteraction != null)
            {
                hitSnapping = (cornerHitGameObject == SnappedToInteraction.RootGameObject || hitGameObject == SnappedToInteraction.RootGameObject);
            }
            if ((valid == false) || (hitGameObject != cornerHitGameObject))
            {
                if (hitSnapping == false)
                {
                    m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
                    bool debugTest = RayCastDetection(forceToZero, RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i], ref validRaycastHit, ref cornerHitGameObject); // this is so i can follow through why it was invalid
                    m_DebugFailIndex = i;
                    RayCastTester.Instance.SetRay(RaycastDataRef.m_Raycast[i], RaycastDataRef.m_RaycastLength[i]);
                    return;
                }
            }
        }
    }

    private bool RayCastDetection(bool forceToZero, Ray ray, float raycastLength, ref RaycastHit validRaycastHit, ref GameObject initialHitGameObject)
    {
        initialHitGameObject = null;
        RaycastHit[] hit = Physics.RaycastAll(ray, raycastLength).OrderBy(h => h.distance).ToArray();
        return InternalRayCastDetection(forceToZero, hit, ref validRaycastHit, ref initialHitGameObject);
    }

    private bool RayCastBoxDetection(bool forceToZero, Ray ray, ref RaycastHit validRaycastHit, ref GameObject initialHitGameObject)
    {
        initialHitGameObject = null;
        RaycastHit[] hit = RigidbodySweepTest(ray);
        return InternalRayCastDetection(forceToZero, hit, ref validRaycastHit, ref initialHitGameObject);
    }

    private void RayCastBoxDetectionSecond(Ray ray, float raycastLength, ref RaycastHit originalHit, ref RaycastHit returnHit)
    {
        RaycastHit[] hit = RigidbodySweepTest(ray);

        for (int i = 0; i < hit.Length; i++)
        {
            // non Ceiling objects get placed though Ceiling objects
            if (PlacementMetaData.Placement != EnumPlacement.Ceiling)
            {
                var otherHit = hit[i].collider.gameObject.GetComponentInParent<InteractionGameObject>();
                if (otherHit != null && otherHit.PlacementMetaData.Placement == EnumPlacement.Ceiling)
                {
                    continue;
                }
            }

            if (originalHit.collider == hit[i].collider)
            {
                returnHit = hit[i];
                return;
            }
        }
    }

    private RaycastHit[] RigidbodySweepTest(Ray ray)
    {
        RootTransform.position = ray.origin;
        RaycastHit[] hit = new UnityEngine.RaycastHit[0];
        if (RigidbodyRef != null)
        {
            hit = RigidbodyRef.SweepTestAll(ray.direction).OrderBy(h => h.distance).ToArray();
        }


        // this needs to happen because of Floor20m Floor40m Floor60m Floor80m
        // select one can change meshCollider to Convex and see how thick it becomes 
        if (hit == null || hit.Length == 0)
        {
            var hitList = Physics.BoxCastAll(ray.origin, BoundsRef.extents, ray.direction, this.transform.rotation).ToList();
            if (this is InteractionGameObject)
            {
                var hitSingle = hitList.FindAll(e => e.collider.gameObject.GetComponent<InteractionFloorPlan>() != null);
                if (hitSingle != null && hitSingle.Count != 0)
                {
                    List<RaycastHit> temp = new List<RaycastHit>();
                    temp.Add(hitSingle[0]);
                    hit = temp.ToArray();
                }
            }
            if (this is InteractionWallWhole)
            {
                var hitSingle = hitList.FindAll(e => e.collider.gameObject.GetComponent<InteractionWallWhole>() != null);
                if (hitSingle != null && hitSingle.Count != 0)
                {
                    List<RaycastHit> temp = new List<RaycastHit>();
                    temp.Add(hitSingle[0]);
                    hit = temp.ToArray();
                }
            }
        }
        return hit;
    }

    private bool InternalRayCastDetection(bool forceToZero, RaycastHit[] hit, ref RaycastHit validRaycastHit, ref GameObject initialHitGameObject)
    {
        for (int i = 0; i < hit.Length; i++)
        {
            // this has to be first 
            initialHitGameObject = hit[i].collider.gameObject;

            // ignore generated walls cutout
            if (initialHitGameObject.name == HouseBuilder.HouseBuilderConstants.GENERATED_UPPER_HULL || initialHitGameObject.name == HouseBuilder.HouseBuilderConstants.GENERATED_LOWER_HULL)
            {
                continue;
            }

            // non Ceiling objects get placed though Ceiling objects
            if (PlacementMetaData.Placement != EnumPlacement.Ceiling)
            {
                var otherHit = hit[i].collider.gameObject.GetComponentInParent<InteractionGameObject>();
                if (otherHit != null && otherHit.PlacementMetaData.Placement == EnumPlacement.Ceiling)
                {
                    continue;
                }
            }

            if(hit[i].collider.gameObject.layer == Layers.ControllerLeftLayer || hit[i].collider.gameObject.layer == Layers.ControllerRightLayer)
            {
                continue;
            }

            // Bug #14921: weird issues with "jumping" ceiling items 
            //https://dev.azure.com/MusicTribe/MI_R00398/_workitems/edit/14921/
            // ceiling objects avoid  colliding
            if (PlacementMetaData.Placement == EnumPlacement.Ceiling)
            {
                var otherHit = hit[i].collider.gameObject.GetComponentInParent<Interaction>(); // get hit object
                if (otherHit != null && otherHit.PlacementMetaData != null)
                {
                    if (otherHit.PlacementMetaData.Placement != EnumPlacement.Ceiling)  // if its not a ceiling object  then ...
                    {
                        if ((otherHit is InteractionCeilingAndFloor) == false && ((otherHit is InteractionFloorPlan) == false))  // if its not a floor or ceiling  continue on , 
                        {
                            continue;
                        }
                    }
                }
            }


            initialHitGameObject = HouseBuilderConstants.GetValidGameObject(hit[i].collider.gameObject);

            if (initialHitGameObject == this.gameObject)
            {
                continue;
            }

            var childrenInteractions = this.GetComponentsInChildren<Interaction>(true).ToList();
            childrenInteractions.Remove(this);
            List<GameObject> newList = new List<GameObject>();
            foreach (var inter in childrenInteractions)
            {
                newList.Add(inter.gameObject);
            }
            newList.Remove(this.gameObject);

            if (newList.Contains(initialHitGameObject))
            {
                continue;
            }

            if (initialHitGameObject == null)
            {
                Debug.LogError($"initialHitGameObject is null, should not be, find the issues");
            }


            // non wall items will not be placed on top of the wall 
            if(this.PlacementMetaData.Placement.IsWall() == false)
            {
                if(initialHitGameObject.GetComponent<InteractionWallGeneratedPoint>() != null)
                {
                    if (m_Outline != null)
                    {
                        m_Outline.ItemSelected = EPOOutline.Outliner.EnumItemSelected.Invalid;
                    }
                    continue;
                }

                // dont try to put object on a cutout stuff
                var obj = initialHitGameObject.GetComponent<Interaction>();
                if (obj != null && obj.PlacementMetaData != null)
                {
                    if(obj.PlacementMetaData.Placement.IsCutout() == true || (obj.PlacementMetaData.Placement.IsWall() == true  && obj.PlacementMetaData.Placement.IsWallStackable() == false))
                    {
                        continue;
                    }
                }
            }

            if (forceToZero == true)
            {
                if (null != initialHitGameObject.GetComponent<InteractionFloorPlan>())
                {
                    validRaycastHit = hit[i];
                    return true;
                }
            }
            else
            {
                validRaycastHit = hit[i];
                return true;
            }
        }
        return false;
    }




#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < RaycastDataRef.m_RaycastArrayLength; i++)
        {
            Gizmos.DrawSphere(RaycastDataRef.m_Raycast[i].origin, 0.01f);
            Gizmos.DrawRay(RaycastDataRef.m_Raycast[i].origin, RaycastDataRef.m_Raycast[i].direction * 1f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(this.transform.position, 0.05f);

        if (m_DebugFailIndex != null)
        {
            Gizmos.color = Color.red;
            int index = (int)m_DebugFailIndex;
            Gizmos.DrawSphere(RaycastDataRef.m_Raycast[index].origin, 0.01f);
            Gizmos.DrawRay(RaycastDataRef.m_Raycast[index].origin, RaycastDataRef.m_Raycast[index].direction * RaycastDataRef.m_RaycastLength[index]);
        }
    }
#endif


    private Interaction ColourTheOutline(GameObject hitGameObject, bool forceValidFirst)
    {
        Interaction returnInteraction = null;

        if (null == hitGameObject) return returnInteraction;
        if (null == m_Outline) return returnInteraction;

        returnInteraction = hitGameObject.GetComponent<Interaction>();
        if (forceValidFirst == true)
        {
            m_Outline.ItemSelected = Outliner.EnumItemSelected.Valid;
        }

        if (null == returnInteraction) return returnInteraction;

        if (returnInteraction.PlacementMetaData == null)
        {
            return returnInteraction;
        }

        if (returnInteraction.PlacementMetaData.Placement == EnumPlacement.NotStackable && this.PlacementMetaData.Placement.IsWall() == false)
        {
            if (this.PlacementMetaData.Placement != EnumPlacement.MountToAll)
            {
                m_Outline.ItemSelected = Outliner.EnumItemSelected.Invalid;
            }
        }

        return returnInteraction;
    }

    public virtual void SelectObject()
    {
        if (null != m_Outline)
        {
            m_Outline.SetOutLine(true);
            m_Outline.ItemSelected = Outliner.EnumItemSelected.Selected;
        }
    }

    public virtual void UnSelectObject()
    {
        if (null != m_Outline)
        {
            m_Outline.SetOutLine(false);
        }
    }

    public void ForceSelectObject()
    {
        if (null != m_Outline)
        {
            m_Outline.SetOutLine(true);
            m_Outline.ItemSelected = Outliner.EnumItemSelected.Selected;
        }
    }

    public void ForceUnSelectObject()
    {
        if (null != m_Outline)
        {
            m_Outline.SetOutLine(false);
        }
    }




    public virtual void MouseDown() { }
    public virtual void MouseDrag() { }
    public virtual void KeyboardMove(Vector3 positionInRealWorld) { }
    public virtual void KeyboardRotate(float amount) { }
    public virtual void MouseUp() { }

    #region ShadowSettings

    protected void SetShadowsOn()
    {
        if (m_Outline != null)
        {
            m_Outline.RenderersRef.RemoveAll(e => e == null);
            for (int i = 0; i < m_OriginalShadowCastingModes.Count; i++)
            {
                if (m_Outline.RenderersRef.ContainsIndex(i) == true)
                {
                    m_Outline.RenderersRef[i].shadowCastingMode = m_OriginalShadowCastingModes[i];
                }
            }
            m_ShadowsOn = true;
        }
    }

    protected void SetShadowsOff()
    {
        if (m_Outline != null)
        {
            m_Outline.RenderersRef.RemoveAll(e => e == null);
            m_Outline.RenderersRef.ForEach(r => r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off);
            m_ShadowsOn = false;
        }
    }

    public void SetShadowsToCurrentState() => SetShadows(m_ShadowsOn);

    public void SetShadows(bool on)
    {
        StoreOriginalShadowCastingModes();
        if (on)
            SetShadowsOn();
        else
            SetShadowsOff();
    }

    protected virtual void StoreOriginalShadowCastingModes()
    {
        if (m_Outline != null)
        {
            m_Outline.RenderersRef.RemoveAll(e => e == null);
            if (m_OriginalShadowCastingModes == null || m_OriginalShadowCastingModes.Count == 0)
            {
                m_OriginalShadowCastingModes = new List<UnityEngine.Rendering.ShadowCastingMode>();
                foreach (var item in m_Outline.RenderersRef)
                {
                    m_OriginalShadowCastingModes.Add(item.shadowCastingMode);
                }
            }
        }
    }
    #endregion

    public void OnDestroy()
    {
        if (m_Lighting != null)
        {
            m_Lighting.OnDestroy();
        }
    }

}
