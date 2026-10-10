using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OscController
{
    public class MenuPanel : MonoBehaviour
    {

        public Action OnLoadSaveSelected;
        public Action OnUserSelected;

        public enum ModeEnum
        {
            Map,
            Sequence,
            TimeLine,
            RunTimeSequenceLine
        }

        public enum DisplayStyleEnum
        {
            Simple,
            Advanced,
        }


        private ModeEnum m_ModeEnum = ModeEnum.Map;
        public ModeEnum Mode
        {
            get
            {
                return m_ModeEnum;
            }
            set
            {
                m_ModeEnum = value;
                Refresh();
            }
        }

        private DisplayStyleEnum m_DisplayStyle = DisplayStyleEnum.Advanced;
        public DisplayStyleEnum DisplayStyle
        {
            get { return m_DisplayStyle; }
            set
            {
                m_DisplayStyle = value;
                Refresh();
            }
        }

        public void Initialise()
        {
            this.SetActive(true);
            var btnReturn = this.transform.Search("Return").GetComponent<Button>();
            if (HouseBuilder.HouseBuilderRoot.Instance == null)
            {
                btnReturn.SetActive(false);
            }
            else
            {
                btnReturn.onClick.AddListener(() =>
                {
                    HouseBuilder.HouseBuilderRoot.Instance.CurrentMode = HouseBuilder.BuilderMode.Lighting;
                });
            }


            this.transform.Search("MapButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Mode = ModeEnum.Map;
            });

            this.transform.Search("SceneButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Mode = ModeEnum.Sequence;
            });

            this.transform.Search("TimeLineButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Mode = ModeEnum.TimeLine;
            });

            this.transform.Search("RuntimeButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                Mode = ModeEnum.RunTimeSequenceLine;
            });


            var user = this.transform.Search("User").GetComponent<Button>();
            user.onClick.AddListener(() =>
            {
                OnUserSelected?.Invoke();
            });

            if (HouseBuilder.HouseBuilderRoot.Instance != null)
            {
                user.SetActive(false);
            }


            this.transform.Search("LoadSave").GetComponent<Button>().onClick.AddListener(() =>
            {
                OnLoadSaveSelected?.Invoke();
            });


            this.transform.Search("Simple").GetComponent<Button>().onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                this.DisplayStyle = MenuPanel.DisplayStyleEnum.Simple;
            }));


            this.transform.Search("Advanced").GetComponent<Button>().onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                this.DisplayStyle = MenuPanel.DisplayStyleEnum.Advanced;
                Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_HEADER_HEADER, "play");
                Core.PhotonGenericRef.SendGenericHeaderAndData("DDDD", "play1122222");
                Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_HEADER_HEADER, "play11");
            }));

            this.transform.Search("Settings").GetComponent<Button>().onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                OscControllerRoot.Instance.SettingsPanelRef.SetActive(true);
                OscControllerRoot.Instance.SettingsPanelRef.FillInData(DataContainer.Instance.GenericDataDataRef);
            }));

            MapPressed();
        }

        private void OnEnable()
        {
            MapPressed();
        }

        private void Refresh()
        {
            Core.PhotonGenericRef.SendGenericHeader(OscControllerRoot.GENERIC_UNSELECT_ALL_DMX_MESSAGE);
            OscControllerRoot.Instance.PlayDataRef.ClearAllDmx();
            switch (Mode)
            {
                case ModeEnum.Map:
                    MapPressed();
                    break;
                case ModeEnum.Sequence:
                    SequencePressed();
                    break;;
                case ModeEnum.TimeLine:
                    TimeLinePressed();
                    break;
                case ModeEnum.RunTimeSequenceLine:
                    RunTimeSequenceLinePressed();
                    break;
                default:
                    break;
            }
        }

        private void MapPressed()
        {
            if(OscControllerRoot.Instance.GridPanelRef == null)
            {
                return;
            }
            OscControllerRoot.Instance.GridPanelRef.SetActive(true);
            OscControllerRoot.Instance.FixtureChoicePanelRef.SetActive(true);
            OscControllerRoot.Instance.SequencePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();

            OscControllerRoot.Instance.SequencePanelOptionsRef.SetActive(false);

            OscControllerRoot.Instance.TimeLinePanelRef.SetActive(false);
            OscControllerRoot.Instance.TimeLinePanelOptionsRef.SetActive(false);
            OscControllerRoot.Instance.SettingsPanelRef.SetActive(false);
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            OscControllerRoot.Instance.SliderPanelRef.SetActive(false);

            OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(false);
            CoverBackground(turnoff: false);
            ShowSlider(false);          
        }

        private void SequencePressed()
        {
            OscControllerRoot.Instance.GridPanelRef.SetActive(false);
            OscControllerRoot.Instance.FixtureChoicePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequencePanelRef.SetActive(true);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();

            OscControllerRoot.Instance.SequencePanelRef.SetButtonStates(true);
            OscControllerRoot.Instance.TimeLinePanelRef.SetActive(false);
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            OscControllerRoot.Instance.SliderPanelRef.SetActive(true);
            OscControllerRoot.Instance.SliderPanelRef.SetCorrectPanelSize();

            OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(false);
            CoverBackground(turnoff: true);
            ShowSlider(true);
        }

        private void TimeLinePressed()
        {
            OscControllerRoot.Instance.GridPanelRef.SetActive(false);
            OscControllerRoot.Instance.FixtureChoicePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequencePanelRef.SetActive(true);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.SetActive(false);

            OscControllerRoot.Instance.SequencePanelRef.SetButtonStates(false);
            OscControllerRoot.Instance.TimeLinePanelRef.SetActive(true);
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            OscControllerRoot.Instance.SliderPanelRef.SetActive(false);

            OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(false);
            CoverBackground(turnoff: true);
            ShowSlider(false);
        }

        private void RunTimeSequenceLinePressed()
        {
            OscControllerRoot.Instance.GridPanelRef.SetActive(false);
            OscControllerRoot.Instance.FixtureChoicePanelRef.SetActive(false);
            OscControllerRoot.Instance.SequencePanelRef.SetActive(true);
            OscControllerRoot.Instance.SequenceRunTimePanelRef.SetActive(true);

            OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();
            OscControllerRoot.Instance.SequenceRunTimePanelRef.CreateDisplaySequenceItems();

            OscControllerRoot.Instance.SequencePanelRef.SetButtonStates(false);
            OscControllerRoot.Instance.TimeLinePanelRef.SetActive(false);
            OscControllerRoot.Instance.CurveEditorPanelRef.SetActive(false);
            OscControllerRoot.Instance.SliderPanelRef.SetActive(false);

            OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(false);
            CoverBackground(turnoff: true);
            ShowSlider(false);
        }


        private void ShowSlider(bool show)
        {
            if (show == true)
            {
                if (DisplayStyle == DisplayStyleEnum.Simple)
                {
                    OscControllerRoot.Instance.SliderPanelRef.SetActive(false);
                    OscControllerRoot.Instance.SliderSimplePanelRef.SetActive(true);
                    OscControllerRoot.Instance.SliderSimpleChoicePanelRef.SetActive(true);
                }
                else
                {
                    OscControllerRoot.Instance.SliderPanelRef.SetActive(true);
                    OscControllerRoot.Instance.SliderSimplePanelRef.SetActive(false);
                    OscControllerRoot.Instance.SliderSimpleChoicePanelRef.SetActive(false);
                }
            }
            else
            {
                OscControllerRoot.Instance.SliderPanelRef.SetActive(false);
                OscControllerRoot.Instance.SliderSimplePanelRef.SetActive(false);
                OscControllerRoot.Instance.SliderSimpleChoicePanelRef.SetActive(false);
            }

            OscControllerRoot.Instance.MacroPanelRef.SetActiveAndAssign(false);
        }

        private void CoverBackground(bool turnoff)
        {
            OscControllerRoot.Instance.BackGroundImageFull.SetActive(true);
            if (HouseBuilder.HouseBuilderRoot.Instance != null && turnoff == true)
            {
                //if in HouseBuilder, this will render the image which will cover up any lingering images because 2 cameras , the ui and room 
                this.WaitForEndOfFrame(() =>
                {
                    OscControllerRoot.Instance.BackGroundImageFull.SetActive(false);
                });
            }
        }
    }
}
