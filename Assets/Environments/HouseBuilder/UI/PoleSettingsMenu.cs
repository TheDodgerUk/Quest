using UnityEngine;
using UnityEngine.UI;

namespace HouseBuilder
{
    public enum PlotElement
    {
        Point,
        DashesBetweenPoles,
    }

    public class PoleSettingsMenu : MonoBehaviour
    {
        private readonly Vector3 CURSOR_OFFSET = new Vector3(0f, 46f, 0f);

        private RectTransform m_RectTransform;
        private CanvasGroup m_Group;
        private Button m_BtnNew;
        private Button m_BtnExtend;
        private Button m_BtnSplit;
        private Toggle m_TglShowHide;
        private Button m_BtnDelete;
        private Button m_BtnBreak;
        private InteractionWallPoint m_CurrentPointData;
        private InteractionWallWhole m_CurrentWallData;
        private PlotElement m_CurrentSelection = PlotElement.Point; //default doesnt matter
        private Vector3 m_MousePoint;

        private Vector3 m_DottedWallStartPoint;
        private Vector3 m_DottedWallEndPoint;
        private int m_CurrentIndex;
        private int m_NextIndex;

        internal void Initialise()
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_RectTransform = transform.GetComponent<RectTransform>();
            m_BtnNew = transform.Find("BtnNew").GetComponent<Button>(); //shown by default
            m_BtnExtend = transform.Find("BtnExtend").GetComponent<Button>(); //shown by default
            m_BtnSplit = transform.Find("BtnSplit").GetComponent<Button>(); //shown by default
            m_TglShowHide = transform.Find("TglShowHide").GetComponent<Toggle>(); //shown by default
            m_BtnDelete = transform.Find("BtnDelete").GetComponent<Button>(); //shown by default
            m_BtnBreak = transform.Find("BtnBreak").GetComponent<Button>(); //shown by default

            m_BtnNew.onClick.AddListener(() => New());
            m_BtnExtend.onClick.AddListener(() => Extend());
            m_BtnSplit.onClick.AddListener(() => Split());
            m_TglShowHide.onValueChanged.AddListener((value) => ToggleShowHide(value));
            m_BtnDelete.onClick.AddListener(() => Delete());
            m_BtnBreak.onClick.AddListener(() => Break());

            //TODO:rename this event so its more clear what it is OR split in to base and subclasses
            HouseBuilderRoot.Instance.OnPoleSelected += (state, pointData, wallData, selectionType, mousePoint) =>
            {
                m_CurrentPointData = pointData;
                m_CurrentWallData = wallData;
                m_CurrentSelection = selectionType;
                m_MousePoint = mousePoint;
                SetVisible(state, false);
            };

            HouseBuilderRoot.Instance.OnModeTransitionBegin += () => SetVisible(false, true);
        }

        private void SetVisible(bool state, bool manual)
        {
            if (state && HouseBuilderRoot.Instance.IsMouseOverGUI() && !manual) return;
            if (HouseBuilderRoot.Instance.InputContents.CurrentInputMode == HouseBuilderInput.InputMode.Placing) return;

            m_Group.VisibleAndInteractive(state);

            m_BtnNew.gameObject.SetActive(false);
            m_BtnExtend.gameObject.SetActive(false);
            m_BtnSplit.gameObject.SetActive(false);
            m_TglShowHide.gameObject.SetActive(false);
            m_BtnDelete.gameObject.SetActive(false);
            m_BtnBreak.gameObject.SetActive(false);

            if (state)
            {
                ShowOptionsPerSelection();
                m_RectTransform.transform.position = Input.mousePosition + CURSOR_OFFSET;
            }
        }

        private void ShowOptionsPerSelection()
        {
            var thisPoint = m_CurrentPointData;
            var wall = m_CurrentWallData;
            var firstItem = wall.m_WallPointList.First();
            var lastItem = wall.m_WallPointList.Last();

            if (m_CurrentSelection == PlotElement.Point)
            {
                if ((firstItem == thisPoint || lastItem == thisPoint)
                    && wall.m_StructureJsonData.m_IsWallLooped == false)
                {
                    m_BtnNew.gameObject.SetActive(true);
                    m_BtnExtend.gameObject.SetActive(true);
                    m_BtnDelete.gameObject.SetActive(true);
                }
                else
                {
                    m_BtnNew.gameObject.SetActive(true);
                    m_BtnDelete.gameObject.SetActive(true);
                }
            }
            else if (m_CurrentSelection == PlotElement.DashesBetweenPoles)
            {
                m_DottedWallStartPoint = thisPoint.transform.position;
                m_CurrentIndex = wall.m_WallPointList.FindIndex(e => e == thisPoint);

                m_NextIndex = m_CurrentIndex.WrapIncrement(wall.m_WallPointList);
                m_DottedWallEndPoint = wall.m_WallPointList[m_NextIndex].transform.position;

                if ((firstItem == thisPoint || lastItem == thisPoint)
                    && wall.m_StructureJsonData.m_IsWallLooped == false)
                {
                    m_BtnSplit.gameObject.SetActive(true);
                    m_TglShowHide.gameObject.SetActive(true);
                    m_BtnNew.gameObject.SetActive(true);
                }
                else
                {
                    m_BtnSplit.gameObject.SetActive(true);
                    m_TglShowHide.gameObject.SetActive(true);
                    m_BtnBreak.gameObject.SetActive(true);
                    m_BtnNew.gameObject.SetActive(true);
                }
            }

            m_TglShowHide.SetIsOnWithoutNotify(HouseBuilderRoot.Instance.InputFloorPlan.GetHideState(m_CurrentIndex));
        }

        private void New()
        {
            if (m_CurrentSelection == PlotElement.Point)
            {
                HouseBuilderRoot.Instance.InputFloorPlan.OnAddNewRoomStartCallback(m_CurrentPointData.transform.position);
            }
            else if (m_CurrentSelection == PlotElement.DashesBetweenPoles)
            {
                var pos = CoreUtils.ClosestPointOnLine(m_DottedWallStartPoint, m_DottedWallEndPoint, m_MousePoint);
                HouseBuilderRoot.Instance.InputFloorPlan.OnAddNewRoomStartCallback(pos);
            }
        }

        private void Extend()
        {
            HouseBuilderRoot.Instance.InputFloorPlan.OnEditCallback(m_CurrentPointData, m_CurrentWallData);
        }

        private void Split()
        {
            var pos = CoreUtils.ClosestPointOnLine(m_DottedWallStartPoint, m_DottedWallEndPoint, m_MousePoint);
            HouseBuilderRoot.Instance.InputFloorPlan.OnSplitCallback(m_NextIndex, pos);
        }

        private void ToggleShowHide(bool value)
        {
            HouseBuilderRoot.Instance.InputFloorPlan.OnHideCallback(m_CurrentIndex);
        }

        private void Break()
        {
            HouseBuilderRoot.Instance.InputFloorPlan.OnBreakCallback(m_CurrentPointData);
            SetVisible(false, true); //done here because otherwise with OnPoleDeselected the menu closes only until i move the mouse away from the button
        }

        private void Delete()
        {
            HouseBuilderRoot.Instance.InputFloorPlan.OnDeleteCallback();
            SetVisible(false, true); //done here because otherwise with OnPoleDeselected the menu closes only until i move the mouse away from the button
        }
    }
}