//using EnvironmentHelpers;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;
//using UnityEngine;
//using UnityEngine.UI;
//using static HouseBuilder.HouseBuilderConstants;
//using static HouseBuilder.HouseBuilderInputCeiling;

//namespace HouseBuilder
//{
//    public class CeilingFloorMaterialPanel : GenericItemChoicePanel
//    {

//        public HouseBuilderFloorPlanJsonData m_FloorPlanJsonData = new HouseBuilderFloorPlanJsonData();

//        private Slider m_WallsTextureTilingSlider;
//        private Text m_WallsTextureTilingText;

//        Action<HouseBuilderFloorPlanJsonData> m_SaveCallBack;


//        Button m_Right;
//        Button m_Left;
//        private int m_CurrentIndex = 0;

//        protected override string MATERIAL_DIRECTORY => HouseBuilderConstants.CEILING_TEXTURE_DIRECTORY;
//        protected virtual ModeContainer<InteractionCeilingAndFloor> ModeRef => HouseBuilderRoot.Instance.InputCeiling.CeilingModeContainerRefSolid;
//        protected virtual PanelTypeEnum PanelType => PanelTypeEnum.Ceiling;
//        private MaterialData m_MaterialData;

//        public override void Initialise()
//        {
//            this.gameObject.SetActive(true);

//            m_WallsTextureTilingSlider = transform.SearchComponent<Slider>("CeilingTextureTilingSlider");
//            m_WallsTextureTilingText = transform.SearchComponent<Text>("CeilingTextureTilingText");


//            m_Right = transform.SearchComponent<Button>("Right");
//            m_Left = transform.SearchComponent<Button>("Left");

//            base.Initialise();

//            new UIButtonHoverToolTip(m_Right, "Right", () =>
//            {
//                int count = HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList.Count;
//                m_CurrentIndex = m_CurrentIndex.WrapIncrement(count);
//                SetCorrectState();
//            });

//            new UIButtonHoverToolTip(m_Left, "Left", () =>         
//            {
//                int count = HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList.Count;
//                m_CurrentIndex = m_CurrentIndex.WrapDecrement(count);
//                SetCorrectState();
//            });


//            m_CurrentIndex = 0;
//            m_WallsTextureTilingSlider.minValue = 1.0f;
//            m_WallsTextureTilingSlider.maxValue = 10.0f;
//        }



//        private void SetCorrectState()
//        {
//            ShowOutline(m_CurrentIndex);
//            if (HouseBuilderRoot.Instance != null && HouseBuilderRoot.Instance.InputCeiling != null)
//            {
//                HouseBuilderRoot.Instance.InputCeiling.ShowSolidCeilingFloor(PanelType, m_CurrentIndex);
//                SetCallBacks(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[m_CurrentIndex].m_StructureJsonData, OnChangeCompleted);
//            }
//        }


//        private void OnEnable()
//        {
//            if (PositionGrid.Instance != null)
//            {
//                PositionGrid.Instance.SetTempGridState(false);
//            }
//            m_CurrentIndex = 0;
//            SetCorrectState();
//        }


//        private void OnDisable()
//        {
//            if (PositionGrid.Instance != null)
//            {
//                PositionGrid.Instance.ShowCurrentGridState();
//            }
//            HideOutline();
//        }

//        private void ShowOutline(int index)
//        {
//            if (null != HouseBuilderRoot.Instance && null != HouseBuilderRoot.Instance.InputFloorPlan)
//            {
//                SetCallBacks(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[index].m_StructureJsonData, OnChangeCompleted);
//                var inter = ModeRef.GetAt(m_CurrentIndex);
//                HideOutline();
//                inter.ManualSelect();
//            }
//        }

//        private void HideOutline()
//        {
//            if (null != HouseBuilderRoot.Instance && null != HouseBuilderRoot.Instance.InputFloorPlan)
//            {
//                foreach (var item in ModeRef.GetList)
//                {
//                    item.ManualUnSelect();
//                }
//            }
//        }

//        private void SetCallBacks(HouseBuilderFloorPlanJsonData data, Action<HouseBuilderFloorPlanJsonData> SaveCallBack)
//        {
//            if (PanelType == PanelTypeEnum.Ceiling)
//            {
//                m_MaterialData = m_FloorPlanJsonData.m_CeilingData;
//            }
//            else
//            {
//                m_MaterialData = m_FloorPlanJsonData.m_FloorData;
//            }

//            Deinitialise();
//            m_FloorPlanJsonData = data;
//            m_SaveCallBack = SaveCallBack;
//            foreach (var textureButton in m_ButtonList)
//            {
//                textureButton.onClick.RemoveAllListeners();
//                textureButton.onClick.AddListener(() =>
//                {
//                    m_MaterialData.m_MaterialName = textureButton.name;
//                    ReGenerate();
//                });
//            }

//            m_WallsTextureTilingSlider.onValueChanged.RemoveAllListeners();
//            m_WallsTextureTilingSlider.value = m_MaterialData.m_TilingAmount;
//            m_WallsTextureTilingText.text = m_WallsTextureTilingSlider.value.ToString("0.00");
//            m_WallsTextureTilingSlider.onValueChanged.AddListener((amount) =>
//            {
//                m_MaterialData.m_TilingAmount = amount;
//                m_WallsTextureTilingText.text = amount.ToString("0.00");
//                ReGenerate();
//            });

//        }

//        private void OnChangeCompleted(HouseBuilderConstants.HouseBuilderFloorPlanJsonData data)
//        {
//            HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[m_CurrentIndex].m_StructureJsonData = data;
//            ReGenerate();
//        }

//        private void FindCorrectWallToReselect(int wallNumber)
//        {
//            var items = FindObjectsOfType<InteractionWallGeneratedWhole>();
//            foreach (var item in items)
//            {
//                string resultString = Regex.Match(item.name, @"\d+").Value;
//                if (Int32.Parse(resultString) == wallNumber)
//                {
//                    HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(item);
//                    break;
//                }
//            }
//        }

//        private void ReGenerate()
//        {
//            Debug.Log("ReGenerate");
//            HouseBuilderRoot.Instance.InputCeiling.GenerateCeilingsAndFloorsForRooms(null);

//                ShowOutline(m_CurrentIndex);
//                HouseBuilderRoot.Instance.InputCeiling.ShowSolidCeilingFloor(PanelType, m_CurrentIndex);
//                SetCallBacks(HouseBuilderRoot.Instance.InputFloorPlan.ModeContainerRef.GetList[m_CurrentIndex].m_StructureJsonData, OnChangeCompleted);
//        }

//        protected override void Deinitialise()
//        {
//            foreach(var item in m_ButtonList)
//            {
//                item.onClick.RemoveAllListeners();
//            }
//        }

//    }

//}