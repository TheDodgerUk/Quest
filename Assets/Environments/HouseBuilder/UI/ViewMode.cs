using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public enum View
    {
        Full3D,
        Topdown,
        FirstPerson,
    }

    public enum FirstPersonType
    {
        None,
        Point,
        Orbit
    }

    public class ViewMode : MonoBehaviour
    {
        public event Action OnViewChanged;

        private readonly float FPS_HEIGHT = 5f * CoreUtils.FeetToMeters;
        private static readonly float TOPDOWN_HEIGHT = 18f;
        private readonly Vector3 TOPDOWN_ORTHOGRAPHIC_DEFAULT_POS = new Vector3(0f, TOPDOWN_HEIGHT, 0f);
        private readonly Vector3 TOPDOWN_PERSPECTIVE_DEFAULT_POS = new Vector3(0f, TOPDOWN_HEIGHT-3f, 0f);

        private Vector3 m_TopdownViewCenter = new Vector3();
        private Vector3 m_3DViewCenter = new Vector3();
        public View CurrentView { get; private set; }
        public FirstPersonType CurrentFirstPersonType { get; set; }

        public bool MouseAnchor { get; set; }

        private float m_Position3DHeight;
        private CanvasGroup m_Group;       
        private Toggle m_3DView;
        private Toggle m_TopdownView;
        private Toggle m_FirstPersonView;
        private Toggle m_Back;
        private CanvasGroup m_BackGroup;
        private TextMeshProUGUI m_CurrentModeLabel;
        private bool m_Initialised = false;

        public void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>();

            m_CurrentModeLabel = transform.Find("CurrentMode/Label").GetComponent<TextMeshProUGUI>();

            m_Back = transform.Find("TglBack").GetComponent<Toggle>();
            m_BackGroup = m_Back.GetComponent<CanvasGroup>();
            m_3DView = transform.Find("Layout Group/Tgl3DView").GetComponent<Toggle>(); //On by default , in toggle group with the other 2
            m_TopdownView = transform.Find("Layout Group/TglTopdownView").GetComponent<Toggle>(); // Off by default
            m_FirstPersonView = transform.Find("Layout Group/TglFirstPersonView").GetComponent<Toggle>(); // Off by default

            m_Back.onValueChanged.AddListener((value) => BackSelected());
            m_3DView.onValueChanged.AddListener((value) => Select3DView(value));
            m_TopdownView.onValueChanged.AddListener((value) => SelectTopdownView(value));
            m_FirstPersonView.onValueChanged.AddListener((value) => SelectFirstPersonView(value));

            CurrentView = View.Full3D;
            m_CurrentModeLabel.SetText("Full 3D View");
            CurrentFirstPersonType = FirstPersonType.Orbit;
            MouseAnchor = false;

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            HouseBuilderRoot.Instance.OnPlotModeSelected += OnPlotModeSelected;
            HouseBuilderRoot.Instance.OnStructureModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnPlacementModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnSoundModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnShowRoomModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnLightingModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnStagingModeSelected += OnContainingModeSelected;
            HouseBuilderRoot.Instance.OnModularModeSelected += (e) => OnModularModeSelected(); // fix camera
            HouseBuilderRoot.Instance.OnOSCModeSelected += OnOSCModeSelected;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;

            m_Back.group = null; // this is to keep it seperate 
            //HBGUIManager.Instance.ShowRoomSettingsMenuRef.SetVisible(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.ShowRoom);
            m_BackGroup.VisibleAndInteractive(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.ShowRoom);
            SelectTopdownView(true);
            m_Initialised = true;
        }

        private void OnEnable()
        {
            if (m_Initialised == true)
            {
                Select3DView(true);
            }
        }

        private void BackSelected()
        {
            if (HouseBuilderRoot.Instance.CurrentMode == BuilderMode.ShowRoom)
            {
                HBGUIManager.Instance.ForcePlacementMode();
            }
        }

        public void SetVisible(bool state)
        {
            m_Group.VisibleAndInteractive(state);
        }


        private void OnOSCModeSelected()
        {
            SetToOscView(true);
        }

        private void OnPlotModeSelected()
        {
            SetToOscView(false);
            var initEffect = EPOOutline.Outliner.Instance;
            /////////////initEffect.SetToFloorPlan();
            m_TopdownView.isOn = true;
            Camera.main.orthographic = true;
            Camera.main.transform.position = TOPDOWN_ORTHOGRAPHIC_DEFAULT_POS;
            Camera.main.orthographicSize = 7;

            SetVisible(false);
        }


        private void OnContainingModeSelected()
        {
            SetToOscView(false);
            var initEffect = EPOOutline.Outliner.Instance;
            initEffect.SetToStartData();
            if (Camera.main.orthographic)
            {
                m_3DView.isOn = true;
            }
            Camera.main.orthographic = false;

            SetVisible(true);
            HBGUIManager.Instance.OscSettingsMenuRef.SetVisible(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Lighting);
            //HBGUIManager.Instance.ShowRoomSettingsMenuRef.SetVisible(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.ShowRoom);
            HBGUIManager.Instance.StagingSettingsMenuRef.SetVisible(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Staging);
            m_BackGroup.VisibleAndInteractive(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.ShowRoom);
        }

        private void OnModularModeSelected()
        {
            SetToOscView(false);
            OnContainingModeSelected();
            SetVisible(false);
        }

        private void Select3DView(bool state)
        {
            SetToOscView(false);
            if (state)
            {
                var root = HouseBuilderRoot.Instance;

                root.InputContents.ModeContainerRef.ToggleShadows(true);
                root.GenerateWallsRef.ToggleShadows(true);
                root.InputCeiling.ToggleCeilingShadows(true);

                //best case scenario this position should be a distance proportional to the grid size

                var pos = Camera.main.transform.position;
                pos.y = m_Position3DHeight;
                pos.z = -m_Position3DHeight / 2;
                Camera.main.transform.position = pos;

                if (CurrentView == View.Topdown)
                {
                    Camera.main.transform.LookAt(m_TopdownViewCenter, Vector3.up);
                }
                else
                {

                    Camera.main.transform.LookAt(Vector3.zero, Vector3.up);
                }

                CurrentView = View.Full3D;
                m_CurrentModeLabel.SetText("Full 3D View");
                OnViewChanged?.Invoke();
            }
            else
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(HouseBuilderConstants.MOUSE_CENTRE_OF_SCREEN), out RaycastHit hit, 100))
                {
                    m_3DViewCenter = hit.point;
                }
            }
        }

        private void SetToOscView(bool osc)
        {
            if(osc == true)
            {
                OscController.OscControllerRoot.Instance.SetActive(true);
                HBGUIManager.Instance.SetActive(false);
                Camera.main.rect = HouseBuilderConstants.SMALL_SCREEN;
            }
            else
            {
                OscController.OscControllerRoot.Instance.SetActive(false);
                HBGUIManager.Instance.SetActive(true);
                Camera.main.rect = HouseBuilderConstants.FULL_SCREEN;
            }
        }

        private void SelectTopdownView(bool state)
        {
            if (state)
            {
                var root = HouseBuilderRoot.Instance;

                root.InputContents.ModeContainerRef.ToggleShadows(false);
                root.GenerateWallsRef.ToggleShadows(false);
                root.InputCeiling.ToggleCeilingShadows(false);

                if (CurrentView == View.Full3D)
                {
                    m_Position3DHeight = Mathf.Abs(Camera.main.transform.position.y);
                    Camera.main.transform.position = new Vector3(m_3DViewCenter.x, Camera.main.transform.position.y, m_3DViewCenter.z);
                }
                else
                {
                    Camera.main.transform.position = TOPDOWN_PERSPECTIVE_DEFAULT_POS;
                }
                
                Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);

                CurrentView = View.Topdown;
                m_CurrentModeLabel.SetText("Topdown View");
                OnViewChanged?.Invoke();
            }
            else
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(HouseBuilderConstants.MOUSE_CENTRE_OF_SCREEN), out RaycastHit hit, 100))
                {
                    m_TopdownViewCenter = hit.point;
                }
            }
        }

        private void SelectFirstPersonView(bool state)
        {
            if (state)
            {
                var root = HouseBuilderRoot.Instance;

                root.InputContents.ModeContainerRef.ToggleShadows(true);
                root.GenerateWallsRef.ToggleShadows(true);
                root.InputCeiling.ToggleCeilingShadows(true);

                if (CurrentView == View.Full3D)
                {
                    m_Position3DHeight = Mathf.Abs(Camera.main.transform.position.y);
                }
                Camera.main.transform.rotation = Quaternion.identity;
                var pos = Camera.main.transform.position;
                pos.y = FPS_HEIGHT;
                Camera.main.transform.position = pos;

                CurrentView = View.FirstPerson;
                MouseAnchor = false;
                m_CurrentModeLabel.SetText("First-Person View");
                OnViewChanged?.Invoke();
            }

            FloorAndSkyboxManager.Instance.ToggleParticleSystems(state);
        }
    }
}