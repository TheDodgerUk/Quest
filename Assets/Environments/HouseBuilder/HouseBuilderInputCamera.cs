using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HouseBuilder
{
    public class HouseBuilderInputCamera : MonoBehaviour
    {
        private const float MOVE_SPEED = 5;

        private float m_ScrollWheelAmount;
        private Interaction m_CurrentInteraction;

        private InteractionCameraPan m_InteractionCameraPan;
        public InteractionCameraOrbit m_InteractionCameraOrbit;
        private InteractionCameraPivot m_InteractionCameraPivot;
        private InteractionCameraPivotTopDown m_InteractionCameraPivotTopDown;

        private InteractionCameraZoomIn m_InteractionCameraZoomIn;
        private InteractionCameraZoomOut m_InteractionCameraZoomOut;

        private const float MAX_DISTANCE_FROM_OBJECT = 2.5f;


        private List<VrInteraction> m_VrInteractionList = new List<VrInteraction>();

        private const float CAMERA_RANGE = 0.5f;

        private bool m_CameraFocusing = false;
        private GameObject m_FakeFocusCameraGameObject;
        private Collider m_SelectedObjCollider;
        private GameObject m_SelectedObj;

        private Func<KeyCommand, bool> InputKey;
        private Func<MouseCommand, PressType, bool> InputMouse;

        public void Initialise()
        {
            m_InteractionCameraPan = this.AddComponent<InteractionCameraPan>();
            m_InteractionCameraPivot = this.AddComponent<InteractionCameraPivot>();
            m_InteractionCameraOrbit = this.AddComponent<InteractionCameraOrbit>();
            m_InteractionCameraZoomIn = this.AddComponent<InteractionCameraZoomIn>();
            m_InteractionCameraZoomOut = this.AddComponent<InteractionCameraZoomOut>();
            m_InteractionCameraPivotTopDown = this.AddComponent<InteractionCameraPivotTopDown>();
            CursorState.CreateInstance();
            SetUpFocusCamera();
            InputKey = HouseBuilderConstants.IsKeyboardCommand;
            InputMouse = HouseBuilderConstants.IsMouseCommand;
        }

        public void ManualUpdate()
        {
            CursorState.Instance.StartLoop();
            CursorState.Instance.SetCursorLockState();

            Cursor.lockState = CursorLockMode.None;
            m_ScrollWheelAmount = Input.mouseScrollDelta.y;

            FpsStateChange();
            NonFpsMode();
            FpsMode();
            FpsStateCusorChange();

            CursorState.Instance.EndLoop();
            CursorState.Instance.SetCursorLockState();
            if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Modular)
            {
                KeyInput();
            }
        }

        private void SetUpFocusCamera()
        {
            m_FakeFocusCameraGameObject = new GameObject("FakeFocusCameraGameObject");
            m_FakeFocusCameraGameObject.transform.SetParent(Camera.main.transform);
            m_FakeFocusCameraGameObject.SetActive(false);
        }

        private void FpsStateChange()
        {
            var viewMode = HBGUIManager.Instance.m_ViewMode;

            if (viewMode.CurrentView == View.FirstPerson)
            {
                viewMode.CurrentFirstPersonType = FirstPersonType.Orbit;
                HouseBuilderRoot.Instance.GenerateWallsRef.CutoutWallsColliders(false);

                if (InputKey(KeyCommand.FPSMouseLockedToggle))
                {
                    viewMode.MouseAnchor = !viewMode.MouseAnchor;
                }
                if (InputKey(KeyCommand.FPSMouseModifier) || HBGUIManager.Instance.m_ViewMode.MouseAnchor == true)
                {
                    viewMode.CurrentFirstPersonType = FirstPersonType.Point;
                    HouseBuilderRoot.Instance.GenerateWallsRef.CutoutWallsColliders(true);
                }
            }
            else
            {
                viewMode.CurrentFirstPersonType = FirstPersonType.None;
            }
        }

        private void FpsStateCusorChange()
        {
            var viewMode = HBGUIManager.Instance.m_ViewMode;

            if (viewMode.CurrentView == View.FirstPerson)
            {
                if (viewMode.CurrentFirstPersonType == FirstPersonType.Orbit)
                {
                    CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Orbit);
                }
                else
                {
                    CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Point);
                }

                if (InputMouse(MouseCommand.Left, PressType.Hold))
                {
                    SetOutLineNormal();
                }
            }
        }

        private void NonFpsMode()
        {
            var viewMode = HBGUIManager.Instance.m_ViewMode;

            if (viewMode.CurrentView == View.FirstPerson)
            {
                return;
            }

            HouseBuilderRoot.Instance.PanMode = false;

            if (InputKey(KeyCommand.Pan))
            {
                CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Hand);
                if(InputMouse(MouseCommand.Left, PressType.Hold))
                {
                    HouseBuilderRoot.Instance.PanMode = true;


                    m_CurrentInteraction = GetCurrentInteractionClassForPanning();
                    m_CurrentInteraction.MouseDown();
                    m_CurrentInteraction.MouseDrag();
                    m_CurrentInteraction = null;
                }
            }

            if (HouseBuilderRoot.Instance.RotateItemMode == true)
            {
                CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Rotate);
            }

            if (InputMouse(MouseCommand.Right, PressType.Hold) == false &&
                InputMouse(MouseCommand.Middle, PressType.Hold) == false &&
                m_ScrollWheelAmount == 0)
            {
                if (null != m_CurrentInteraction)
                {
                    m_CurrentInteraction.MouseUp();
                    m_CurrentInteraction = null;
                }
            }
            else
            {
                if (null == m_CurrentInteraction)
                {
                    m_CurrentInteraction = GetCurrentInteractionClassForCamera(m_ScrollWheelAmount);
                    if (null != m_CurrentInteraction)
                    {
                        if (m_CurrentInteraction is InteractionCameraOrbit)
                        {
                            CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Orbit);
                        }
                        m_CurrentInteraction.SelectObject(); // does nothing
                        m_CurrentInteraction.MouseDown(); //does nothing
                    }
                }
                else
                {
                    if (m_CurrentInteraction is InteractionCameraOrbit)
                    {
                        CursorState.Instance.SetCursorState(CursorState.CursorStateEnum.Orbit);
                    }
                    m_CurrentInteraction.MouseDrag();
                }
            }
        }

        private void FpsMode()
        {
            var viewMode = HBGUIManager.Instance.m_ViewMode;

            if (viewMode.CurrentView == View.FirstPerson && viewMode.CurrentFirstPersonType == FirstPersonType.Orbit)
            {
                var interaction = GetCurrentInteractionClassForPivot();
                interaction.MouseDrag();

                HouseBuilderRoot.Instance.InputContents.ModeContainerRef.DisableMainColliderInVR();
                MouseRight();
                HouseBuilderRoot.Instance.InputContents.ModeContainerRef.EnableMainColliderInVR();
            }
        }

        private void MouseRight()
        {
            foreach (var item in m_VrInteractionList)
            {
                item.EnableOutlineCanBeUsed = true;
                item.SetOutLine(false);
            }

            m_VrInteractionList.Clear();
            Ray ray = Camera.main.ScreenPointToRay(HouseBuilderConstants.MousePosition());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                m_VrInteractionList = hit.transform.GetComponents<VrInteraction>().ToList();
            }

            foreach (var item in m_VrInteractionList)
            {
                item.EnableOutlineCanBeUsed = true;
                item.SetOutLine(true);
            }

            if (true == InputMouse(MouseCommand.Right, PressType.Tap))
            {
                foreach (var item in m_VrInteractionList)
                {
                    ControllerDataTest data = new ControllerDataTest(hit.collider);
                    List<ControllerData> dataList = new List<ControllerData>();
                    ControllerStateInteraction fakeVRClick = new ControllerStateInteraction(data, dataList, dataList);
                    item.BeginLaser(fakeVRClick);
                }
            }
        }

        private Interaction GetCurrentInteractionClassForCamera(float scrollWheelAmount)
        {
            var viewMode = HBGUIManager.Instance.m_ViewMode;

            if (viewMode.CurrentView != View.FirstPerson)
            {
                if (InputMouse(MouseCommand.Right, PressType.Hold) && ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0))) //right
                {
                    if (viewMode.CurrentView == View.Full3D) //Not allowing camera rotation in 2D mode
                    {
                        if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Modular)
                        {
                            return GetCurrentInteractionClassForOrbit();
                        }
                        else
                        {
                            return m_InteractionCameraOrbit;
                        }
                    }
                    else
                    {
                        return GetCurrentInteractionClassForPivotTopDown();
                    }
                }

                if (InputMouse(MouseCommand.Middle, PressType.Hold)) // middle
                {
                    if (viewMode.CurrentView != View.Topdown)
                    {
                        return GetCurrentInteractionClassForPivot();
                    }
                }

                if (scrollWheelAmount > 0)
                {
                    return GetCurrentInteractionClassForZoomIn();
                }
                if (scrollWheelAmount < 0)
                {
                    return GetCurrentInteractionClassForZoomOut();
                }
                return null;
            }
            else
            {
                return GetCurrentInteractionClassForPivot();
            }
        }

        private Interaction GetCurrentInteractionClassForPanning() => m_InteractionCameraPan;

        private Interaction GetCurrentInteractionClassForPivot()
        {
            if (HBGUIManager.Instance.m_ViewMode.CurrentView == View.FirstPerson) // always pivot 
            {
                return m_InteractionCameraPivot;
            }
            else if (HBGUIManager.Instance.SceneSettingsMenuRef.IsCameraControlsSwapped() == false)
            {
                return m_InteractionCameraPivot;
            }
            else
            {
                return m_InteractionCameraOrbit;
            }
        }

        private Interaction GetCurrentInteractionClassForPivotTopDown() => m_InteractionCameraPivotTopDown;

        private Interaction GetCurrentInteractionClassForOrbit()
        {
            if (HBGUIManager.Instance.SceneSettingsMenuRef.IsCameraControlsSwapped() == false)
            {
                return m_InteractionCameraOrbit;
            }
            else
            {
                return m_InteractionCameraPivot;
            }
        }

        private Interaction GetCurrentInteractionClassForZoomIn() => m_InteractionCameraZoomIn;

        private Interaction GetCurrentInteractionClassForZoomOut() => m_InteractionCameraZoomOut;

        private void KeyInput()
        {
            if (HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions().Count == 0)
            {
                MoveCameraWithArrows();
            }
            else
            {
                MoveCameraNoArrows();
            }
        }

        private void MoveCameraNoArrows()
        {
            if (HouseBuilderConstants.IsKeyboardCommand(KeyCommand.Focus) &&
                HBGUIManager.Instance.m_ViewMode.CurrentView != View.FirstPerson)
            {
                FocusCamera();
            }
            if (InputKey(KeyCommand.Duplicate) == false)
            {
                var viewMode = HBGUIManager.Instance.m_ViewMode;

                if (InputKey(KeyCommand.MoveCameraForward) || InputKey(KeyCommand.MoveCameraBackward))
                {
                    Vector3 forward;
                    if (viewMode.CurrentView == View.Topdown)
                    {
                        forward = Camera.main.transform.up;
                    }
                    else
                    {
                        forward = Camera.main.transform.forward;
                    }
                    
                    if (viewMode.CurrentView == View.FirstPerson)
                    {
                        forward.y = 0;
                        forward.Normalize();
                    }
                    forward = InputKey(KeyCommand.MoveCameraForward) == true ? forward : -forward;
                    var nextCamPos = Camera.main.transform.position + forward * Time.deltaTime * MOVE_SPEED;

                   if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                        //HouseBuilderRoot.Instance.ManualUpdateCeilingFloors(); commented out for Bug #13903 
                    }
                }

                if (InputKey(KeyCommand.MoveCameraLeft) || InputKey(KeyCommand.MoveCameraRight))
                {
                    var right = Camera.main.transform.right;
                    right.y = 0;
                    right.Normalize();

                    right = InputKey(KeyCommand.MoveCameraLeft) == true ? right : -right;
                    var nextCamPos = Camera.main.transform.position - right * Time.deltaTime * MOVE_SPEED;

                    if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                    }
                }

                if (InputKey(KeyCommand.MoveCameraUp) || InputKey(KeyCommand.MoveCameraDown))
                {
                    var up = Vector3.up;

                    up = InputKey(KeyCommand.MoveCameraUp) == true ? -up : up;
                    var nextCamPos = Camera.main.transform.position + up * Time.deltaTime * MOVE_SPEED;

                    if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                    }
                }
            }
        }

        public void FocusCamera()
        {
            if (m_CameraFocusing == false)
            {
                m_FakeFocusCameraGameObject.gameObject.SetActive(true);
                var list = HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions();
                if (list != null)
                {
                    if (list.Count == 1)
                    {
                        m_CameraFocusing = true;
                        m_SelectedObj = list[0].gameObject;
                        m_SelectedObjCollider = m_SelectedObj.GetComponentInChildren<Collider>();
                        MoveFocusCameraTowardSelectedObject();
                    }
                }
            }
        }

        public void ManualGUIFocus()
        {
            if (HBGUIManager.Instance.m_ViewMode.CurrentView != View.FirstPerson)
            {
                if (m_CameraFocusing == false)
                {
                    m_FakeFocusCameraGameObject.gameObject.SetActive(true);
                    var list = HouseBuilderRoot.Instance.InputContents.GetCurrentInteractions();
                    if (list != null)
                    {
                        if (list.Count == 1)
                        {
                            m_CameraFocusing = true;
                            m_SelectedObj = list[0].gameObject;
                            m_SelectedObjCollider = m_SelectedObj.GetComponentInChildren<Collider>();
                            MoveFocusCameraTowardSelectedObject();
                        }
                    }
                }
            }
        }

        public IEnumerator MoveCameraWithWidget(string buttonPressed)
        {
            Vector3 nextCamPos;
            if (buttonPressed == "ForwardBtn" || buttonPressed == "BackwardBtn")
            {
                var viewMode = HBGUIManager.Instance.m_ViewMode;
                if (viewMode.CurrentView == View.Topdown)
                {
                    nextCamPos = Camera.main.transform.up;
                }
                else
                {
                    nextCamPos = Camera.main.transform.forward;
                    if (viewMode.CurrentView == View.FirstPerson)
                    {
                        nextCamPos.y = 0;
                        nextCamPos.Normalize();
                    }
                }
                nextCamPos = (buttonPressed == "ForwardBtn") ? nextCamPos : -nextCamPos;
            }
            else
            {
                nextCamPos = Camera.main.transform.right;
                nextCamPos.y = 0;
                nextCamPos.Normalize();

                nextCamPos = (buttonPressed == "RightBtn") ? nextCamPos : -nextCamPos;
            }

            bool movecam = true;
            while (movecam)
            {
                if (CheckWithinGrid(nextCamPos))
                  Camera.main.transform.position += (nextCamPos * Time.deltaTime * MOVE_SPEED);
                else
                    movecam = false;       //stop the coroutine when the camera is out of space

                yield return new WaitForEndOfFrame();
            }
        }

        private void MoveCameraWithArrows()
        {
            if (InputKey(KeyCommand.Duplicate) == false)
            {
                var viewMode = HBGUIManager.Instance.m_ViewMode;

                if (InputKey(KeyCommand.MoveCameraForward) || InputKey(KeyCommand.MoveCameraBackward) ||
                    InputKey(KeyCommand.MoveObjectForward) || InputKey(KeyCommand.MoveObjectBackward))
                {
                    Vector3 forward;
                    if (viewMode.CurrentView == View.Topdown)
                    {
                        forward = Camera.main.transform.up;
                    }
                    else
                    {
                        forward = Camera.main.transform.forward;
                    }

                    if (viewMode.CurrentView == View.FirstPerson)
                    {
                        forward.y = 0;
                        forward.Normalize();
                    }
                    forward = InputKey(KeyCommand.MoveCameraForward) || InputKey(KeyCommand.MoveObjectForward) ? forward : -forward;
                    var nextCamPos = Camera.main.transform.position + forward * Time.deltaTime * MOVE_SPEED;

                    if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                        //HouseBuilderRoot.Instance.ManualUpdateCeilingFloors(); commented out for Bug #13903 
                    }
                }

                if (InputKey(KeyCommand.MoveCameraLeft) || InputKey(KeyCommand.MoveCameraRight) ||
                    InputKey(KeyCommand.MoveObjectLeft) || InputKey(KeyCommand.MoveObjectRight))
                {
                    var right = Camera.main.transform.right;
                    right.y = 0;
                    right.Normalize();

                    right = InputKey(KeyCommand.MoveObjectLeft) || InputKey(KeyCommand.MoveCameraLeft) ? right : -right;
                    var nextCamPos = Camera.main.transform.position - right * Time.deltaTime * MOVE_SPEED;

                    if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                    }
                }

                if (InputKey(KeyCommand.MoveCameraUp) || InputKey(KeyCommand.MoveCameraDown))
                {
                    var up = Vector3.up;

                    up = InputKey(KeyCommand.MoveCameraUp) == true ? -up : up;
                    var nextCamPos = Camera.main.transform.position + up * Time.deltaTime * MOVE_SPEED;

                    if (CheckWithinGrid(nextCamPos))
                    {
                        Camera.main.transform.position = nextCamPos;
                    }
                }
            }
        }

        private bool CheckWithinGrid(Vector3 newPosition)
        {
            float width = PositionGrid.Instance.GetGridInfo().m_Width * 0.5f;
            return true;
            if (newPosition.x < -width || newPosition.x > width)
            {
                return false;
            }

            if (newPosition.z < -width || newPosition.z > width)
            {
                return false;
            }

            return true;
        }

        private void SetOutLineWhite()
        {
            var initEffect = EPOOutline.Outliner.Instance;
            initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Invalid, Color.white);
            initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Selected, Color.white);
            initEffect.SetLineColor(EPOOutline.Outliner.EnumItemSelected.Valid, Color.white);
            ////////initEffect.Data.lineThickness = 1f;
            ////////initEffect.Data.lineIntensity = 10;
            ////////initEffect.Data.fillAmount = 0;
            ////////initEffect.additiveRendering = true;
        }

        private void SetOutLineNormal()
        {
            var initEffect = EPOOutline.Outliner.Instance;
            initEffect.SetToStartData();
        }

        private void MoveFocusCameraTowardSelectedObject()
        {
            Vector3 selectedObjPos = m_SelectedObj.transform.position;
            Camera.main.transform.LookAt(selectedObjPos);
            m_FakeFocusCameraGameObject.gameObject.transform.transform.ClearLocals();
            var bounds = m_SelectedObjCollider.bounds.size;
            float ray = Mathf.Max(Mathf.Max(bounds.x, bounds.y), bounds.z);

            while (m_CameraFocusing)
            {
                int cameraStatus = CheckSelectedObjDistanceFromFocusCamera(selectedObjPos, ray);
                if (cameraStatus == 1) //camera too far from obj
                {
                    m_FakeFocusCameraGameObject.transform.position += m_FakeFocusCameraGameObject.transform.forward;  //Move camera forward
                }
                else if (cameraStatus == 0) //camera close enough from obj
                {
                    SetMainCameraFinalPosition();
                }
                else if (cameraStatus == 2)  //camera too close to obj
                {
                    m_FakeFocusCameraGameObject.transform.position -= m_FakeFocusCameraGameObject.transform.forward;
                }
            }
        }

        private int CheckSelectedObjDistanceFromFocusCamera(Vector3 pos, float ray = 1f)
        {
            float boundaries = MAX_DISTANCE_FROM_OBJECT * ray;
            var distance = Mathf.Abs(Vector3.Distance(pos, m_FakeFocusCameraGameObject.transform.position));

            if (distance < boundaries + CAMERA_RANGE && distance > boundaries - CAMERA_RANGE) //obj inside range
            {
                return 0;
            }
            else if (distance > boundaries + CAMERA_RANGE)  //obj too far
            {
                return 1;
            }
            else if (distance < (boundaries - CAMERA_RANGE)) //obj too close
            {
                return 2;
            }
            else
            {
                return -1;
            }
        }

        private void SetMainCameraFinalPosition()
        {
            Camera.main.transform.position = m_FakeFocusCameraGameObject.transform.position;
            Camera.main.transform.rotation = m_FakeFocusCameraGameObject.transform.rotation;
            m_SelectedObj = null;
            m_SelectedObjCollider = null;
            m_CameraFocusing = false;
            m_FakeFocusCameraGameObject.gameObject.SetActive(false);
        }
    }
}