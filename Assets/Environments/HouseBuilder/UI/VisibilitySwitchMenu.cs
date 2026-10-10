using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
namespace HouseBuilder
{
    public class VisibilitySwitchMenu : MonoBehaviour
    {
        private CanvasGroup m_Group;
        private Toggle[] m_Toggles = new Toggle[4];
        //private TextMeshProUGUI[] m_Labels = new TextMeshProUGUI[4];

        private readonly Color32 LABEL_NORMAL = new Color32(102, 164, 255, 255);
        private readonly Color32 LABEL_SELECTED = new Color32(145, 197, 236, 255);

        public void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>();

            m_Toggles[0] = transform.Find("ToggleGroup/TglCeiling").GetComponent<Toggle>();
            m_Toggles[1] = transform.Find("ToggleGroup/TglOpenings").GetComponent<Toggle>();
            m_Toggles[2] = transform.Find("ToggleGroup/TglWalls").GetComponent<Toggle>();
            m_Toggles[3] = transform.Find("ToggleGroup/TglFloor").GetComponent<Toggle>();

            m_Toggles[0].onValueChanged.AddListener((value) => UpdateVisuals());
            m_Toggles[1].onValueChanged.AddListener((value) => UpdateVisuals());
            m_Toggles[2].onValueChanged.AddListener((value) => UpdateVisuals());
            m_Toggles[3].onValueChanged.AddListener((value) => UpdateVisuals());

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;

            HouseBuilderRoot.Instance.OnModeTransitionBegin += OnModeTransitionBegin;
            HouseBuilderRoot.Instance.OnPlacementModeSelected += OnVisibilityModeSelected;

            ConsoleExtra.Log($"Bug #14673: Removing Invisibility functionality into furniture mode,     state = false;    ", null, ConsoleExtraEnum.EDebugType.StartUp);
            m_Toggles[1].interactable = false;
        }

        private void SetVisible(bool state)
        {
            state = false;

            m_Group.VisibleAndInteractive(state);
        }

        private void OnEnvironmentLoadingComplete()
        {
            //And other stuff here for pre-load
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        }

        private void OnModeTransitionBegin()
        {
            SetVisible(false);
        }

        private void OnVisibilityModeSelected()
        {
            //not sure if we want to reset state of the toggles on mode switch
            //if we do, its just .isOn = true for each of the toggles
            m_Toggles[0].isOn = true;
            m_Toggles[1].isOn = true;
            m_Toggles[2].isOn = true;
            m_Toggles[3].isOn = true;
            SetVisible(true);
        }

        public void UpdateVisuals()
        {
            HouseBuilderRoot.Instance.InputCeiling.ToggleTemporaryAllCeilingVisibility(m_Toggles[0].isOn);
            HouseBuilderRoot.Instance.InputCeiling.ToggleTemporaryAllFloorVisibility(m_Toggles[3].isOn);
            UpdateWalls();
        }

        private void UpdateWalls()
        {
            foreach (var room in HouseBuilderRoot.Instance.GenerateWallsRef.GeneratedWholeList)
            {
                foreach (var wallpart in room.m_WallGeneratedPointList)
                {
                    if (m_Toggles[2].isOn == true)
                    {
                        var list = wallpart.GetComponentsInChildren<MeshRenderer>().ToList();
                        list.ForEach(e => e.shadowCastingMode = ShadowCastingMode.TwoSided);
                    }
                    else
                    {
                        var list = wallpart.GetComponentsInChildren<MeshRenderer>().ToList();
                        list.ForEach(e => e.shadowCastingMode = ShadowCastingMode.ShadowsOnly);
                    }
                }
            }
        }
    }
}