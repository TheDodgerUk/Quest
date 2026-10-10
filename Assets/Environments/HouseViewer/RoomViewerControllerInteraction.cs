using HouseBuilder;
using HouseViewer;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomViewerControllerInteraction : MonoBehaviour
{
    [SerializeField]
    private  float SCALE = 10f;
    [SerializeField]
    private  float ROTATION = 300f;
    [SerializeField]
    private  float POSITION = 1f;
    /// <summary>
    /// notes
    /// 2 hands inwards  you get bigger
    /// 2 hand positions with head, for rotation, like grabbing the wjole object
    /// 
    /// </summary>
    public enum ControlMode
    {
        Interaction,
        Demeos,
    }


    private ControlMode m_ControlMode = ControlMode.Interaction;
    public ControlMode CurrentMode
    {
        get { return m_ControlMode; }
        set
        {
            if (value != m_ControlMode)
            {
                m_ControlMode = value;
                ControlModeChanged();
            }
        }
    }


    public enum DemeosMode
    {
        Teleport,
        Highlight,
        Move
    }

    private DemeosMode m_DemeosMode = DemeosMode.Teleport;
    public DemeosMode CurrentDemeosMode
    {
        get { return m_DemeosMode; }
        set
        {
            if (value != m_DemeosMode)
            {
                m_DemeosMode = value;
                this.WaitForFrames(3, () =>
                {
                    DemeosModeChanged();
                });
                
            }
        }
    }


    private Vector3 m_InteractionPosition;



    private float m_InitialScale = 1f;
    private Quaternion m_InitialRotation;
    private Vector3 m_InitialCameraPosition;
    private Vector3 m_InitialMidddleOfControllers;

    

    public class HandInfo
    {
        public Handedness m_Hand = Handedness.Left;
        public Vector3 m_InitialControllerLocalPosition;
        public Vector3 m_InitialControllerWorldPosition;
        public bool m_Grip = false;
        public ControllerStateInteraction m_Controller;
        public ControllerData m_ControllerData;
        public GameObject m_FakeHand;
        public ParticleSystem m_Highlight;
        public GameObject m_DemeosModeGameRoot;
        public Dictionary<DemeosMode, Transform> m_DemeosModeGameObjects = new Dictionary<DemeosMode,Transform>();
    }


    private HandInfo m_LeftHand = new HandInfo();
    private HandInfo m_RightHand = new HandInfo();


    public enum DisplayOn
    {
        None,
        Left,
        Right,
    }
    private DisplayOn m_DisplayOn = DisplayOn.None;


    public class HandMenu
    {
        public GameObject m_Grow;
        public GameObject m_Sound;
        public GameObject m_Load;

        public GameObject m_Root;
    }


    private HandMenu m_HandMenu = new HandMenu();

    private void Awake()
    {
        InputManagerVR.Instance.AnySubscription.BtnPrimary.Begin += DemeosTriggerChange;
        InputManagerVR.Instance.AnySubscription.Grab.Begin += DemeosGrabBegin;
        InputManagerVR.Instance.AnySubscription.Grab.Update += DemeosGrabUpdate;
        InputManagerVR.Instance.AnySubscription.Grab.End += DemeosGrabEnd;



        var rig = CameraControllerVR.Instance.RigRoot.GetComponentInChildren<Rigidbody>(true);
        Destroy(rig);

        m_LeftHand.m_FakeHand = new GameObject("m_FakeHand Left");
        m_RightHand.m_FakeHand = new GameObject("m_FakeHand Right");

        m_LeftHand.m_FakeHand.transform.SetParent(CameraControllerVR.Instance.MainCamera.gameObject.transform);
        m_RightHand.m_FakeHand.transform.SetParent(CameraControllerVR.Instance.MainCamera.gameObject.transform);

        /////////////////////////CameraControllerVR.Instance.SetController(CameraControllerVR.ControllerType.Hand);
        InputManagerVR.Instance.IterateControllers(OnControllerAdded);
        InputManagerVR.Instance.ControllerAdded -= OnControllerAdded;
        InputManagerVR.Instance.ControllerAdded += OnControllerAdded;

        ControlModeChanged(false);
    }

    private void OnControllerAdded(ControllerData controller)
    {
        var tracked = controller as TrackedControllerData;
        if (tracked != null)
        {
            if (m_HandMenu.m_Root == null)
            {
                m_HandMenu.m_Root = CachedObjects.Instance.InstantiateCachedGameObject("MenuTest");
                m_HandMenu.m_Sound = m_HandMenu.m_Root.SearchComponent<Transform>("Sound").gameObject;
                m_HandMenu.m_Load = m_HandMenu.m_Root.SearchComponent<Transform>("Load").gameObject;
                m_HandMenu.m_Grow = m_HandMenu.m_Root.SearchComponent<Transform>("Grow").gameObject;

                m_HandMenu.m_Grow.gameObject.ForceComponent<Photon.Pun.PhotonView>();
                var grow = m_HandMenu.m_Grow.gameObject.ForceComponent<VrInteractionButtonLatched>();
                grow.Initialise(1f, 0.1f, (newMode) =>
                {
                    CurrentMode = (ControlMode)(int)newMode;
                    Debug.LogError($"m_ControlMode  {CurrentMode.ToString()}");
                });
                var hoverGrow = m_HandMenu.m_Grow.gameObject.ForceComponent<VrInteractionHoverScale>();
                hoverGrow.Initialise(0.5f, 1.3f);



                m_HandMenu.m_Load.gameObject.ForceComponent<Photon.Pun.PhotonView>();
                var load = m_HandMenu.m_Load.gameObject.ForceComponent<VrInteractionClickCallBack>();
                load.AddCallback((amount) =>
                {
                    HouseViewerRoot.Instance.RoomChoice();
                    Debug.LogError("Clicked VrInteractionClickCallBack");
                });
                var hoverLoad = m_HandMenu.m_Load.gameObject.ForceComponent<VrInteractionHoverScale>();
                hoverLoad.Initialise(0.5f, 1.3f);


                m_HandMenu.m_Sound.gameObject.ForceComponent<Photon.Pun.PhotonView>();
                var sound = m_HandMenu.m_Sound.gameObject.ForceComponent<VrInteractionButtonToggle>();
                sound.Initialise(1f, 0.1f, (ttt) =>
                {
                    Debug.LogError("Clicked VrInteractionButtonToggle");
                });
                var hoverSound = m_HandMenu.m_Sound.gameObject.ForceComponent<VrInteractionHoverScale>();
                hoverSound.Initialise(0.5f, 1.3f);
            }


            if (tracked.Hand == Handedness.Left)
            {
                AssignHandItems(m_LeftHand, controller);

                m_LeftHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_LeftHand.m_Highlight.gameObject, new Vector3(0f, 0f, 0.12f), Quaternion.identity);
                m_LeftHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_LeftHand.m_DemeosModeGameRoot, new Vector3(0f, 0f, -0.12f), Quaternion.identity);

            }
            else
            {
                AssignHandItems(m_RightHand, controller);

                m_RightHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_RightHand.m_Highlight.gameObject, new Vector3(0f, 0f, 0.12f), Quaternion.identity);
                m_RightHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_RightHand.m_DemeosModeGameRoot, new Vector3(0f, 0f, -0.12f), Quaternion.identity);
            }
 
        }
    }

    private void AssignHandItems(HandInfo hand, ControllerData data)
    {
        hand.m_ControllerData = data;
        var hight = CachedObjects.Instance.InstantiateCachedGameObject("HighLight");
        hand.m_Highlight = hight.GetComponent<ParticleSystem>();
        var main = hand.m_Highlight.main;
        main.playOnAwake = false;


        // TODO Tom wants the name Demeos removed 
        hand.m_DemeosModeGameRoot = CachedObjects.Instance.InstantiateCachedGameObject("DemeosMode");
        hand.m_DemeosModeGameObjects.Add(DemeosMode.Teleport, hand.m_DemeosModeGameRoot.transform.SearchComponent<Transform>("Teleport"));
        hand.m_DemeosModeGameObjects.Add(DemeosMode.Highlight, hand.m_DemeosModeGameRoot.transform.SearchComponent<Transform>("Highlight"));
        hand.m_DemeosModeGameObjects.Add(DemeosMode.Move, hand.m_DemeosModeGameRoot.transform.SearchComponent<Transform>("Move"));

        hand.m_DemeosModeGameObjects[DemeosMode.Teleport].AddComponent<VrInteractionClickCallBack>().AddCallback((amount) => DemeosTriggerChange());
        hand.m_DemeosModeGameObjects[DemeosMode.Highlight].AddComponent<VrInteractionClickCallBack>().AddCallback((amount) => DemeosTriggerChange());
        hand.m_DemeosModeGameObjects[DemeosMode.Move].AddComponent<VrInteractionClickCallBack>().AddCallback((amount) => DemeosTriggerChange());
    }

    private void ControlModeChanged(bool useBlink = true)
    {
        m_LeftHand.m_Highlight.Stop();
        m_RightHand.m_Highlight.Stop();

        switch (CurrentMode)
        {
            case ControlMode.Interaction:
                CameraControllerVR.Instance.RigRoot.transform.position = m_InteractionPosition;
                CameraControllerVR.Instance.RigRoot.transform.SetScale(1f);

                CameraControllerVR.Instance.ToggleBlink(useBlink, () =>
                {
                    CameraControllerVR.Instance.RigRoot.transform.SetScale(1f);
                    CameraControllerVR.Instance.ToggleBlink(false);
                });
                break;
            case ControlMode.Demeos:
                m_InteractionPosition = CameraControllerVR.Instance.RigRoot.transform.position;
                DemeosModeChanged();
                break;
            default:
                break;
        }
    }

    internal void DemeosHighLightBegin(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        if(CurrentMode == ControlMode.Demeos && CurrentDemeosMode == DemeosMode.Highlight)
        {
            if(interaction.Main.Hand == Handedness.Left)
            {
                m_LeftHand.m_Highlight.Play();
            }
            else
            {
                m_RightHand.m_Highlight.Play();
            }
        }
        
    }
    internal void DemeosHighLightEnd(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        if (CurrentMode == ControlMode.Demeos && CurrentDemeosMode == DemeosMode.Highlight)
        {
            if (interaction.Main.Hand == Handedness.Left)
            {
                m_LeftHand.m_Highlight.Stop();
            }
            else
            {
                m_RightHand.m_Highlight.Stop();
            }
        }
    }

    internal void DemeosTriggerChange(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        DemeosTriggerChange();
    }

    internal void DemeosTriggerChange()
    {
        int myEnumMemberCount = Enum.GetNames(typeof(DemeosMode)).Length;
        int enumInt = (int)CurrentDemeosMode;
        enumInt = enumInt.WrapIncrement(myEnumMemberCount);
        CurrentDemeosMode = (DemeosMode)enumInt;
    }


    private void DemeosModeChanged()
    {
        m_LeftHand.m_Highlight.Stop();
        m_RightHand.m_Highlight.Stop();

        foreach (var item in m_LeftHand.m_DemeosModeGameObjects)
        {
            item.Value.SetActive(false);
        }

        foreach (var item in m_RightHand.m_DemeosModeGameObjects)
        {
            item.Value.SetActive(false);
        }

        m_LeftHand.m_DemeosModeGameObjects[m_DemeosMode].SetActive(true);
        m_RightHand.m_DemeosModeGameObjects[m_DemeosMode].SetActive(true);

        InputManagerVR.Instance.AnySubscription.Interact.Begin -= DemeosHighLightBegin;
        InputManagerVR.Instance.AnySubscription.Interact.End -= DemeosHighLightEnd;
        switch (m_DemeosMode)
        {
            case DemeosMode.Teleport:
                Debug.LogError("Teleport");

                break;
            case DemeosMode.Highlight:
                Debug.LogError("Heighlight");
                InputManagerVR.Instance.AnySubscription.Interact.Begin += DemeosHighLightBegin;
                InputManagerVR.Instance.AnySubscription.Interact.End += DemeosHighLightEnd;
                break;
            case DemeosMode.Move:
                Debug.LogError("Move Item");
                break;
            default:
                break;
        }
    }

    private void DemoModTelported()
    {
        if (CurrentMode == ControlMode.Demeos)
        {
            CurrentMode = ControlMode.Interaction;
        }
    }

    private void UpdateLocalData(HandInfo hand)
    {
        if (CurrentMode != ControlMode.Demeos)
        {
            return;
        }
        if (hand.m_Grip == true)
        {
            hand.m_FakeHand.transform.position = hand.m_Controller.Main.WristTransform.transform.position;
            hand.m_FakeHand.transform.rotation = hand.m_Controller.Main.WristTransform.transform.rotation;
        }
    }


    public void DemeosGrabBegin(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        if (CurrentMode != ControlMode.Demeos)
        {
            return;
        }
        m_InitialScale = CameraControllerVR.Instance.RigRoot.transform.localScale.y;
        m_InitialRotation = CameraControllerVR.Instance.RigRoot.transform.rotation;
        m_InitialCameraPosition = CameraControllerVR.Instance.RigRoot.transform.position;
        m_InitialMidddleOfControllers = Vector3.Lerp(m_LeftHand.m_InitialControllerWorldPosition, m_RightHand.m_InitialControllerWorldPosition, 0.5f);
        if (interaction.Main.Hand == Handedness.Left)
        {
            DemeosGrabBegin(interaction, m_LeftHand);
        }
        else
        {
            DemeosGrabBegin(interaction, m_RightHand); 
        }
    }


    private void DemeosGrabBegin(ControllerStateInteraction interaction, HandInfo hand)
    {
        if (CurrentMode != ControlMode.Demeos)
        {
            return;
        }
        hand.m_Controller = interaction;
        hand.m_Grip = true;
        UpdateLocalData(hand);      
        hand.m_InitialControllerLocalPosition = hand.m_FakeHand.transform.localPosition;
        hand.m_InitialControllerWorldPosition = hand.m_FakeHand.transform.position;
    }

    public void DemeosGrabUpdate(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        if (CurrentMode != ControlMode.Demeos)
        {
            return;
        }
        UpdateLocalData(m_LeftHand);
        UpdateLocalData(m_RightHand);       

        if (m_LeftHand.m_Grip == true && m_RightHand.m_Grip == true)
        {
            // scale
            float originalScaleDistance = m_LeftHand.m_InitialControllerLocalPosition.x - m_RightHand.m_InitialControllerLocalPosition.x;
            float newScaleDistance = m_LeftHand.m_FakeHand.transform.localPosition.x - m_RightHand.m_FakeHand.transform.localPosition.x;           
            float newScale = m_InitialScale + ((newScaleDistance - originalScaleDistance)  * SCALE);

            newScale = newScale.Clamp(1f, 100f);
            //Debug.LogError($"or {originalScaleDistance}, new {newScaleDistance}, m_Scale {newScale}");
            CameraControllerVR.Instance.RigRoot.transform.SetScale(newScale);


            // rotation
            float originalRotationDistance = m_LeftHand.m_InitialControllerLocalPosition.z - m_RightHand.m_InitialControllerLocalPosition.z;
            float newRotationDistance = m_LeftHand.m_FakeHand.transform.localPosition.z - m_RightHand.m_FakeHand.transform.localPosition.z;
            float rotateAmount = (originalRotationDistance - newRotationDistance) * ROTATION;
            //Debug.LogError($"original {originalRotationDistance }, right {newRotationDistance}, rotateAmount {rotateAmount}        , left{m_LeftHand.m_FakeHand.transform.localPosition.z}, right{m_RightHand.m_FakeHand.transform.localPosition.z}");

            CameraControllerVR.Instance.RigRoot.transform.rotation = m_InitialRotation;
            CameraControllerVR.Instance.RigRoot.transform.position = m_InitialCameraPosition;
            CameraControllerVR.Instance.RigRoot.transform.RotateAround(m_InitialMidddleOfControllers, Vector3.up, rotateAmount);

        }
        else
        {
            HandInfo currentHand = m_RightHand;
            if (interaction.Main.Hand == Handedness.Left)
            {
                currentHand = m_LeftHand;
            }

            Vector3 amount = (currentHand.m_FakeHand.transform.localPosition - currentHand.m_InitialControllerLocalPosition);

            amount = Quaternion.AngleAxis(CameraControllerVR.Instance.RigRoot.transform.localEulerAngles.y, Vector3.up) * amount;
            amount *= POSITION;
            amount.y = -amount.y; // invert
            amount *= CameraControllerVR.Instance.RigRoot.transform.localScale.x;
            Vector3 newPosition = m_InitialCameraPosition + amount;


            // limit height
            newPosition.y = Mathf.Max(newPosition.y, CameraControllerVR.Instance.RigRoot.transform.localScale.x -1f);
            CameraControllerVR.Instance.RigRoot.transform.position = newPosition;
        }
    }

    public void DemeosGrabEnd(ControllerStateInteraction interaction, bool sendPhotonMessage)
    {
        if (CurrentMode != ControlMode.Demeos)
        {
            return;
        }
        if (interaction.Main.Hand == Handedness.Left)
        {
            m_LeftHand.m_Grip = false;
        }
        if (interaction.Main.Hand == Handedness.Right)
        {
            m_RightHand.m_Grip = false;
        }
    }


    private void Update()
    {
        if (m_HandMenu.m_Root == null)
        {
            return;
        }

        MainMenuUpdate();
    }

    private void MainMenuUpdate()
    {
        bool showLeft = false;
        bool showRight = false;

        var data = InputManagerVR.Instance.GetController(Handedness.Left);
        if (data != null)
        {
            float amount = data.WristTransform.localEulerAngles.z;
            showLeft = amount.IsInRange(90 - 30, 90 + 30);
            ////Debug.LogError($"origin Left x  local {data.Origin.localEulerAngles}, show {showLeft}", data.Origin);

        }

        var dataRight = InputManagerVR.Instance.GetController(Handedness.Right);
        if (dataRight != null)
        {
            float amount = dataRight.WristTransform.localEulerAngles.z;
            showRight = amount.IsInRange(270 - 30, 270 + 30);
            ////Debug.LogError($"origin Right x {amount},  show {showRight}", dataRight.Origin);
        }

        switch (m_DisplayOn)
        {
            case DisplayOn.None:
                if (showLeft == true)
                {
                    UpdateCubes(showLeft, Handedness.Left);
                }
                else
                {
                    UpdateCubes(showRight, Handedness.Right);
                }
                break;
            case DisplayOn.Left:
                UpdateCubes(showLeft, Handedness.Left);
                break;
            case DisplayOn.Right:
                UpdateCubes(showRight, Handedness.Right);
                break;
            default:
                break;
        }
    }

    private void UpdateCubes(bool enable, Handedness hand)
    {
        if(m_LeftHand.m_Grip == true || m_RightHand.m_Grip)
        {
            m_HandMenu.m_Root.SetActive(false); // the menu cant be displayed when gripping 
            return;
        }

        if(enable == false)
        {
            m_DisplayOn = DisplayOn.None;
        }
        else
        {
            if(hand == Handedness.Left)
            {
                m_DisplayOn = DisplayOn.Left;
            }
            else
            {
                m_DisplayOn = DisplayOn.Right;
            }
        }
        m_HandMenu.m_Root.SetActive(enable);
        if(enable == true)
        {
            if(hand == Handedness.Left)
            {

                m_LeftHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_HandMenu.m_Root, new Vector3(0.066f, -0.064f, -0.05f), Quaternion.Euler(new Vector3(220, 0, -90)));
                m_LeftHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_LeftHand.m_Highlight.gameObject, new Vector3(0.066f, -0.064f, -0.05f), Quaternion.identity);
            }
            else
            {
                m_RightHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_HandMenu.m_Root, new Vector3(-0.066f, -0.064f, -0.05f), Quaternion.Euler(new Vector3(220, 0, 90)));
                m_RightHand.m_ControllerData.AddModelAttachmentToRaycastRoot(m_LeftHand.m_Highlight.gameObject, new Vector3(0.066f, -0.064f, -0.05f), Quaternion.identity);
            }
        }
        else
        {
        }

    }

}
