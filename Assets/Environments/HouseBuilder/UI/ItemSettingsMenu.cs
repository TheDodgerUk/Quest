using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HouseBuilder
{
    public class ItemSettingsMenu : MonoBehaviour
    {
        public class CombinedMetadata
        {
            public MetaDataType m_MetaDataType;
            public MetaData m_MetaData;
        }

        public class DuplicateData
        {
            public CatalogueGeneratedData m_CatalogueGeneratedData;
            public int m_index;
        }

        public Action<bool> OnColourSettingsToggled;
        private CanvasGroup m_Group;
        public bool m_IsGroupActive;
        private Transform m_Rotation;
        private Transform m_RotationExtra;
        private Transform m_Adaptive;

        private Transform m_Scale;       
        private Transform m_MultipleDuplicate;
        private Toggle m_TglMultipleDuplication;
        private Toggle m_TglRotation;
        private Toggle m_TglAdaptive;
        private Toggle m_TglScale;
        private CanvasGroup m_RotationPanel;
        private CanvasGroup m_ModulationPanel;
        private CanvasGroup m_ScalePanel;
        private CanvasGroup m_MultipleDuplicationPanel;

        private Button m_BtnDuplicateItem;
        private Button m_BtnDeleteItem;
        private Slider m_ScaleSlider;

        private Button m_BtnModularItem;
        private float MIN_SCALE_SLIDER_VALUE = 0.5f;
        private float MAX_SCALE_SLIDER_VALUE = 5f; 


        private TMP_InputField m_InputRotation;
        private TextMeshProUGUI m_RotationPlaceholder;

        private TMP_InputField m_InputRotationExtra;
        private TextMeshProUGUI m_RotationPlaceholderExtra;

        private Button m_AdaptiveUp;
        private Button m_AdaptiveDown;
        private Button m_AdaptiveLeft;
        private Button m_AdaptiveRight;

        private Button m_WidgetMoveBtn;
        private Button m_WidgetRotateBtn;



        private List<InteractionGameObject> m_CurrentSelections = null;

        private SimpleDictionaryList<Interaction, CombinedMetadata> m_MetaList = new SimpleDictionaryList<Interaction, CombinedMetadata>();

        private bool m_IsTexturingOn = false;
        private bool m_IsVideoingOn = false;


        private Duplication DuplicationRef = new Duplication();

        internal void Initialise(Dictionary<string, GameObject> m_ContentTypes)
        {
            m_Group = transform.GetComponent<CanvasGroup>(); //hidden by default
            m_Group.FixBrokenMasks();

            m_IsGroupActive = false;
            m_BtnDuplicateItem = transform.Find("BtnDuplicateItem").GetComponent<Button>(); //shown by default
            m_BtnDeleteItem = transform.Find("BtnDeleteItem").GetComponent<Button>(); //shown by default
            m_Rotation = transform.Find("RotateItem");
            m_Adaptive = transform.Find("ExpandItem");
            m_RotationExtra = transform.SearchComponent<Transform>("RotateItemExtra");

            m_Scale = transform.Find("ScaleItem");
            m_MultipleDuplicate = transform.Find("MultipleDuplicate");

            m_TglMultipleDuplication = m_MultipleDuplicate.GetComponentInChildren<Toggle>();
            m_TglRotation = transform.Find("RotateItem/Toggle").GetComponent<Toggle>(); //off by default
            m_TglAdaptive = transform.Find("ExpandItem/Toggle").GetComponent<Toggle>(); //off by default
            m_TglScale = transform.Find("ScaleItem/Toggle").GetComponent<Toggle>(); //off by default

            m_BtnModularItem = transform.SearchComponent<Button>("BtnModular");//off by default

            Transform rotationPanel = transform.Find("RotateItem/Content");
            Transform modulationPanel = transform.Find("ExpandItem/Content");

            Transform scalePanel = transform.Find("ScaleItem/ContentScale");
            m_RotationPanel = rotationPanel.GetComponent<CanvasGroup>(); //hidden by default
            m_ModulationPanel = modulationPanel.GetComponent<CanvasGroup>(); //hidden by default
            m_ScalePanel = scalePanel.GetComponent<CanvasGroup>(); //hidden by default

            Transform multipleDuplicatePanel = transform.Find("MultipleDuplicate/Content");
            m_MultipleDuplicationPanel = multipleDuplicatePanel.GetComponent<CanvasGroup>(); //hidden by default


            m_InputRotation = rotationPanel.Find("InputRotate").GetComponent<TMP_InputField>();
            m_RotationPlaceholder = rotationPanel.Find("InputRotate/Text Area/Placeholder").GetComponent<TextMeshProUGUI>();

            m_InputRotationExtra = rotationPanel.SearchComponent<TMP_InputField>("InputRotateExtra");
            m_RotationPlaceholderExtra = m_InputRotationExtra.transform.SearchComponent<TextMeshProUGUI>("Placeholder");

            m_AdaptiveUp = modulationPanel.transform.Find("BtnUp").GetComponent<Button>();
            m_AdaptiveDown = modulationPanel.transform.Find("BtnDown").GetComponent<Button>();
            m_AdaptiveLeft = modulationPanel.transform.Find("BtnLeft").GetComponent<Button>();
            m_AdaptiveRight = modulationPanel.transform.Find("BtnRight").GetComponent<Button>();

            m_ScaleSlider = scalePanel.GetComponentInChildren<Slider>();
            m_ScaleSlider.onValueChanged.AddListener(delegate { ScaleSelectedObject(); }) ;
            m_BtnModularItem.onClick.AddListener(OpenModular);

            m_TglMultipleDuplication.onValueChanged.AddListener((value) => SetMultipleDuplicationSettingsActive(value));
            m_TglRotation.onValueChanged.AddListener((value) => SetRotationSettingsActive(value));
            m_TglAdaptive.onValueChanged.AddListener((value) => SetModulationSettingsActive(value));
            m_InputRotation.onEndEdit.AddListener((value) => SetItemRotation(value));
            m_InputRotationExtra.onEndEdit.AddListener((value) => SetItemRotationExtra(value));
            //m_TglScale.onValueChanged.AddListener((value) => SetScaleSettingActive(value));

            m_AdaptiveUp.onClick.AddListener(() => ModulateObject(Vector3.up));
            m_AdaptiveDown.onClick.AddListener(() => ModulateObject(Vector3.down));
            m_AdaptiveLeft.onClick.AddListener(() => ModulateObject(Vector3.left));
            m_AdaptiveRight.onClick.AddListener(() => ModulateObject(Vector3.right));

            m_BtnDuplicateItem.onClick.AddListener(() => DuplicationRef.DuplicateItems(m_CurrentSelections[0], m_CurrentSelections));
            m_BtnDeleteItem.onClick.AddListener(() => DeleteItem());
            DuplicationRef.SetupMulipleDuplicate(m_MultipleDuplicationPanel, m_CurrentSelections);


            m_WidgetMoveBtn = this.transform.SearchComponent<Button>("WidgetMoveBtn");
            m_WidgetMoveBtn.onClick.AddListener(() => HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Move);

            m_WidgetRotateBtn = this.transform.SearchComponent<Button>("WidgetRotateBtn");
            m_WidgetRotateBtn.onClick.AddListener(() => HouseBuilderRoot.Instance.TransformGizmoRef.transformType = RuntimeGizmos.TransformType.Rotate);

            HouseBuilderRoot.Instance.OnObjectSelected += (state, data) =>
            {
                if (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Sound && HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Staging)
                {
                    m_CurrentSelections = data;
                    SetActive(state, false);
                }
            };

            HouseBuilderRoot.Instance.OnModeTransitionBegin += () => SetActive(false, true);

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void OpenModular() => HouseBuilderRoot.Instance.CurrentMode = BuilderMode.Modular;

        public void PostInitialise(CatalogueMenu catalogue) //Delayed because catalogue must be initialised before this
        {
            catalogue.OnTexturePicked += OnTexturePicked;
            catalogue.OnVideoPicked += OnVideoPicked;
        }

        private void OnEnvironmentLoadingComplete()
        { 
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            HouseBuilderRoot.Instance.InputContents.OnDeleteKeyPressed += () => DeleteItem();
            HouseBuilderRoot.Instance.InputContents.OnDuplicateKeyPressed += () => DuplicationRef.DuplicateItems(m_CurrentSelections[0], m_CurrentSelections);
        }


        public void SetActive(bool state, bool manual)
        {
            if(HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Sound || HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Lighting || HouseBuilderRoot.Instance.CurrentMode == BuilderMode.Staging)
            {
                state = false;
            }
            // sets the basics for m_MultipleDuplicate
            DuplicationRef.DestroyTemporaryDuplicateList();
            if (state)
            {
                DuplicationRef.SetCurrentSelection(m_CurrentSelections);
                DuplicationRef.SetDistanceVaribles();
            }

            if (state && HouseBuilderRoot.Instance.IsMouseOverGUI() && !manual) return;
            if (HouseBuilderRoot.Instance.InputContents.CurrentInputMode == HouseBuilderInput.InputMode.Placing) return;
            if (state)
            {
                m_MultipleDuplicate.gameObject.SetActive(m_CurrentSelections.Count == 1);
                m_BtnDuplicateItem.gameObject.SetActive(state);

                CollectMetadata();
                var firstItem = m_CurrentSelections.FirstOrDefault();
                if (firstItem.PlacementMetaData.Placement.IsWall())
                {
                    m_Rotation.gameObject.SetActive(false);
                    m_RotationExtra.gameObject.SetActive(false);
                }
                else
                {
                    m_RotationExtra.gameObject.SetActive(firstItem.PlacementMetaData.m_ExtraPivotPointGameObject != null);
                    SetInputRotationExtraFieldPlaceholder();

                    m_Rotation.gameObject.SetActive(true);
                    SetInputRotationFieldPlaceholder();                   
                }

                int metadataCount = m_MetaList.GetKeys().Count;
                if (metadataCount != 0)
                {
                    bool showModular = (metadataCount == CountMetadata(MetaDataType.ContentModular));
                    m_BtnModularItem.SetActive(showModular == true && m_CurrentSelections.Count == 1);

                    bool showScaleTgl = (metadataCount == CountMetadata(MetaDataType.ContentScale));
                    m_Scale.SetActive(showScaleTgl);

                    bool showMaterialColours = (metadataCount == CountMetadata(MetaDataType.ContentMaterialChange));
                    bool showAdaptiveTgl = (metadataCount == CountMetadata(MetaDataType.ContentAdaptive));

                    m_IsTexturingOn = AreAllCurrentInteractionsTextureable(out ContentMaterialChangeMetaData.MaterialItemData.OrientationType orientationTypeMaterialChange);
                    m_IsVideoingOn = AreAllCurrentInteractionsVideoTextureable(out ContentVideoPlayerMetaData.OrientationType orientationTypeVideoChange);
                    bool showLightColours = (metadataCount == CountLightCanEditMetadata());

                    SetColoursActive(showMaterialColours, showLightColours);

                    if (showAdaptiveTgl)
                    {                      
                        m_Adaptive.gameObject.SetActive(true);
                        SetupAdaptiveIcons();
                    }
                    else
                    {
                        m_Adaptive.gameObject.SetActive(false);
                    }



                    if (m_IsTexturingOn)
                    {
                        HBGUIManager.Instance.CatalogueMenuRef.ToggleItemTextures(true, orientationTypeMaterialChange);
                    }

                    if (m_IsVideoingOn)
                    {
                        HBGUIManager.Instance.CatalogueMenuRef.ToggleItemVideos(true, orientationTypeVideoChange);
                    }
                }
                else
                {
                    m_IsTexturingOn = false; // just in case who knows
                    m_IsVideoingOn = false;
                    m_Scale.SetActive(false);
                }

                if(m_CurrentSelections[0].PlacementMetaData.Method == ContentPlacementMetaData.EnumMethod.Widget)
                {
                    m_WidgetMoveBtn.SetActive(true);
                    m_WidgetRotateBtn.SetActive(true);
                    m_Rotation.SetActive(false);
                }
                else
                {
                    m_WidgetMoveBtn.SetActive(false);
                    m_WidgetRotateBtn.SetActive(false);
                }

            }
            else
            {
                m_BtnModularItem.SetActive(false);
                m_TglRotation.isOn = false;
                m_TglAdaptive.isOn = false;
                m_TglMultipleDuplication.isOn = false;

                SetScaleSettingActive(false);
                SetRotationSettingsActive(false);
                SetColoursActive(false, false);
                SetModulationSettingsActive(false);
                SetMultipleDuplicationSettingsActive(false);

                if (m_IsTexturingOn)
                {
                    HBGUIManager.Instance.CatalogueMenuRef.ToggleItemTextures(false, ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Portrait);
                }
                if (m_IsVideoingOn)
                {
                    HBGUIManager.Instance.CatalogueMenuRef.ToggleItemVideos(false, ContentVideoPlayerMetaData.OrientationType.Landscape);
                }
                

            }

            m_IsGroupActive = state;
            m_Group.VisibleAndInteractive(state);
        }

        private void SetRotationSettingsActive(bool state)
        {
            m_RotationPanel.VisibleAndInteractive(state);
        }

        private void SetModulationSettingsActive(bool state)
        {
            m_ModulationPanel.VisibleAndInteractive(state);
        }

        private void SetMultipleDuplicationSettingsActive(bool state)
        {
            m_MultipleDuplicationPanel.VisibleAndInteractive(state);
        }

        private void SetScaleSettingActive(bool state)
        {
            m_ScalePanel.VisibleAndInteractive(state);

            //TO DO: fix => look at delete code!
            if (state)
            {
                var objscale = m_CurrentSelections[0].CurrentSelectedInteraction.ScaleRef.GetScale();
                m_ScaleSlider.value = objscale.x;
            }
        }

        private void ScaleSelectedObject()
        {
            m_CurrentSelections[0].ScaleRef.SetScale(m_ScaleSlider.value);
        }

        private void SetColoursActive(bool showMaterialColours, bool showLightColours)
        {
            bool state = showMaterialColours || showLightColours;

            if (state)
            {
                if (showLightColours == true)
                {
                    ColorPickerManager.Instance.SetAmbientLightDefaultColours();
                }
                else
                {
                    ColorPickerManager.Instance.SetFurnitureDefaultColours();
                }
                ColorPickerManager.OnColourChanged = OnColourPicked;
            }
            else
            {
                ColorPickerManager.OnColourChanged = null;
            }

            OnColourSettingsToggled?.Invoke(state);
        }

        private void CollectMetadata()
        {
            m_MetaList.ClearAll();
            foreach (var interaction in m_CurrentSelections)
            {
                if (null != interaction.CatalogueEntryRef)
                {
                    if (interaction.CatalogueEntryRef.GetLoadedMetaData().Count != 0)
                    {
                        // loaded data
                        foreach (var item in interaction.CatalogueEntryRef.GetLoadedMetaData())
                        {
                            AddMetaData(interaction, item);
                        }
                    }
                    else
                    {
                        // from FBX files 
                        foreach (var item in interaction.CatalogueEntryRef.GetAllMetaData())
                        {
                            AddMetaData(interaction, item);
                        }
                    }
                }
            }
        }

        private void AddMetaData(InteractionGameObject interaction, MetaData item)
        {
            CombinedMetadata comb = new CombinedMetadata();
            comb.m_MetaDataType = item.GetMetaDataType();
            comb.m_MetaData = item;
            m_MetaList.AddToList(interaction, comb);
        }


        private int CountMetadata(MetaDataType metaDataType)
        {
            int count = 0;
            foreach (var item in m_MetaList.GetKeys())
            {
                var valueList = m_MetaList.GetValuesList(item);
                var found = valueList.Find(e => e.m_MetaDataType == metaDataType);
                if (found != null)
                {
                    count++;
                }
            }
            return count;
        }

        private MetaData GetMetadata(MetaDataType metaDataType)
        {
            foreach (var item in m_MetaList.GetKeys())
            {
                var valueList = m_MetaList.GetValuesList(item);
                var found = valueList.Find(e => e.m_MetaDataType == metaDataType);
                if (found != null)
                {
                    return found.m_MetaData;
                }
            }
            return null;
        }

        private int CountLightCanEditMetadata()
        {
            int count = 0;
            foreach (var item in m_MetaList.GetKeys())
            {
                var valueList = m_MetaList.GetValuesList(item);
                var found = valueList.Find(e => e.m_MetaDataType == MetaDataType.ContentLight);
                if (found != null && ((ContentLightMetaData)found.m_MetaData).m_CanEdit == true)
                {
                    count++;
                }
            }
            return count;
        }


        private void SetupAdaptiveIcons()
        {
            var first = m_MetaList.GetKeys()[0];
            var axisField = first.AdaptiveRef.ContentAdaptiveMetaDataRef.UnPackAxisField();

            if (axisField.Count == 2)
            {
                m_AdaptiveUp.gameObject.SetActive(true);
                m_AdaptiveDown.gameObject.SetActive(true);
                m_AdaptiveLeft.gameObject.SetActive(true);
                m_AdaptiveRight.gameObject.SetActive(true);
            }
            else
            {
                switch (axisField[0])
                {
                    case Axis.X:
                        m_AdaptiveUp.gameObject.SetActive(false);
                        m_AdaptiveDown.gameObject.SetActive(false);
                        m_AdaptiveLeft.gameObject.SetActive(true);
                        m_AdaptiveRight.gameObject.SetActive(true);
                        break;

                    case Axis.Y:
                    case Axis.Z:
                        m_AdaptiveUp.gameObject.SetActive(true);
                        m_AdaptiveDown.gameObject.SetActive(true);
                        m_AdaptiveLeft.gameObject.SetActive(false);
                        m_AdaptiveRight.gameObject.SetActive(false);
                        break;
                }
            }
        }

        private void ModulateObject(Vector3 direction)
        {
            var first = m_MetaList.GetKeys()[0];
            var axisField = first.AdaptiveRef.ContentAdaptiveMetaDataRef.UnPackAxisField();

            foreach (var item in m_MetaList.GetKeys())
            {
                item.AdaptiveRef.GetAmount1And2(out int amount1, out int amount2);

                if (direction == Vector3.up)
                {
                    item.AdaptiveRef.Modulate(amount1, ++amount2);
                }
                else if (direction == Vector3.down)
                {
                    item.AdaptiveRef.Modulate(amount1, --amount2);
                }
                else if (direction == Vector3.left)
                {
                    item.AdaptiveRef.Modulate(--amount1, amount2);
                }
                else if (direction == Vector3.right)
                {
                    item.AdaptiveRef.Modulate(++amount1, amount2);
                }
            }

            if (first.PlacementMetaData.Placement.IsCutout())
            {
                HouseBuilderRoot.Instance.RefreshWall();
            }
        }

        private void DeleteItem()
        {
            var inputContentsRef = HouseBuilderRoot.Instance.InputContents;

            bool isCutout = HouseBuilderRoot.Instance.InputContents.GetLastInteraction().PlacementMetaData.Placement.IsCutout();
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                var children = m_CurrentSelections[i].GetComponentsInChildren<InteractionGameObjectMove>().ToList(); ;
                children.Remove((InteractionGameObjectMove)m_CurrentSelections[i]);
                for (int childIndex = 0; childIndex < children.Count; childIndex++)
                {
                    inputContentsRef.DeleteItem(children[childIndex], true);
                }
                inputContentsRef.DeleteItem(m_CurrentSelections[i], true);
            }

            this.WaitForEndOfFrame(() =>
            {
                HouseBuilderRoot.Instance.InputContents.CleanUp();
                HouseBuilderRoot.Instance.RecorderRef.Record();
            });

            HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(null);

            if (isCutout == true)
            {
                HouseBuilderRoot.Instance.RefreshWall();
            }

            SetActive(false, true);
        }


        private void OnColourPicked(Color col)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                MaterialChangeMetaData(col, item);
                LightData(col, item);
            }
            HouseBuilderRoot.Instance.RecorderRef.Record();
            Debug.Log(col);
        }

        private void LightData(Color col, InteractionGameObject item)
        {
            if (item.HasLights)
            {
                item.SetLightsColour(col);
            }
        }

        private void MaterialChangeMetaData(Color col, InteractionGameObject item)
        {
            var valList = m_MetaList.GetValuesList(item);
            var rawMatData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentMaterialChange);
            if (rawMatData != null)
            {
                ContentMaterialChangeMetaData data = (ContentMaterialChangeMetaData)rawMatData.m_MetaData;

                // Anthony I think this is where i need to specify only the first material should be changed if the item is a pictureframe ?
                //or maybe in ContentMaterialChangeMetaData
                foreach (var matData in data.m_MaterialItemData)
                {
                    if (matData.m_MaterialType == ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Colour)
                    {
                        ContentMaterialChangeMetaData.SetColourStatic(item.gameObject, matData.m_MaterialGameObjectName, matData.m_MaterialIndex, col);
                    }
                }

                var rawModData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentAdaptive);
                if (rawModData != null)
                {
                    foreach (var matData in data.m_MaterialItemData)
                    {
                        var listOfNames = item.gameObject.transform.SearchListWithIncrementNumber(matData.m_MaterialGameObjectName);
                        foreach (var trans in listOfNames)
                        {
                            if (matData.m_MaterialType == ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Colour)
                            {
                                ContentMaterialChangeMetaData.SetColourStatic(item.gameObject, trans.name, matData.m_MaterialIndex, col);
                            }
                        }
                    }
                }
            }
        }

        private bool AreAllCurrentInteractionsTextureable(out ContentMaterialChangeMetaData.MaterialItemData.OrientationType orientationType)
        {
            orientationType = ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Landscape;
            int count = 0;
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var valList = m_MetaList.GetValuesList(item);
                var rawMatData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentMaterialChange);

                if (rawMatData != null)
                {
                    ContentMaterialChangeMetaData data = (ContentMaterialChangeMetaData)rawMatData.m_MetaData;

                    foreach (var matData in data.m_MaterialItemData)
                    {
                        if (matData.m_MaterialType == ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Texture)
                        {
                            orientationType = matData.m_OrientationType;
                            count++;
                        }
                    }
                }
            }
            return (count == m_CurrentSelections.Count);
        }

        private bool AreAllCurrentInteractionsVideoTextureable(out ContentVideoPlayerMetaData.OrientationType orientationType)
        {
            orientationType = ContentVideoPlayerMetaData.OrientationType.Landscape;
            int count = 0;
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var valList = m_MetaList.GetValuesList(item);
                var rawMatData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentVideoPlayer);

                if (rawMatData != null)
                {
                    ContentVideoPlayerMetaData data = (ContentVideoPlayerMetaData)rawMatData.m_MetaData;
                    orientationType = data.m_OrientationType;
                    count++;
                }
            }

            return (count == m_CurrentSelections.Count);
        }


        private void OnTexturePicked(string textureName, string filter)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var valList = m_MetaList.GetValuesList(item);
                var rawMatData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentMaterialChange);
                if (rawMatData != null)
                {
                    ContentMaterialChangeMetaData data = (ContentMaterialChangeMetaData)rawMatData.m_MetaData;

                    foreach (var matData in data.m_MaterialItemData)
                    {
                        if (matData.m_MaterialType == ContentMaterialChangeMetaData.MaterialItemData.MaterialType.Texture)
                        {
                            ContentMaterialChangeMetaData.SetTexture(m_CurrentSelections[i].RootGameObject,
                                matData.m_MaterialGameObjectName, matData.m_MaterialIndex, textureName, matData.m_TextureMap, null);
                        }
                    }
                }
            }
        }


        private void OnVideoPicked(string videoName, string filter)
        {
            for (int i = 0; i < m_CurrentSelections.Count; i++)
            {
                InteractionGameObject item = m_CurrentSelections[i];
                var valList = m_MetaList.GetValuesList(item);
                var rawMatData = valList.Find(e => e.m_MetaDataType == MetaDataType.ContentVideoPlayer);
                if (rawMatData != null)
                {
                    var vid = item.GetComponentInChildren<VrInterationVideoPlayer>();
                    vid.VideoName = videoName;
                }
            }
        }


        private void SetItemRotation(string value)
        {
            if (true == float.TryParse(value, out float newRotation))
            {
                for (int i = 0; i < m_CurrentSelections.Count; i++)
                {
                    var currentItem = m_CurrentSelections[i];
                    currentItem.PlacementMetaData.m_MountToAllRotation = newRotation;
                    Transform trans = currentItem.transform;
                    trans.localRotation = Quaternion.identity;
                    trans.Rotate(new Vector3(0, currentItem.PlacementMetaData.m_MountToAllRotation, 0), Space.Self);
                    HouseBuilderRoot.Instance.RecorderRef.Record();
                }
            }
        }

        private void SetInputRotationFieldPlaceholder()
        {
            m_InputRotation.text = null; // this is so m_RotationPlaceholder is shown
            float currentRotation = m_CurrentSelections[0].transform.eulerAngles.y;
            m_RotationPlaceholder.SetText(currentRotation.ToString());
        }

        private void SetItemRotationExtra(string value)
        {
            if (true == float.TryParse(value, out float newRotation))
            {
                ContentPlacementMetaData placementMetaData = null;
                for (int i = 0; i < m_CurrentSelections.Count; i++)
                {
                    placementMetaData = m_CurrentSelections[i].PlacementMetaData;
                    placementMetaData.m_ExtraPivotRotation = newRotation;

                    switch (placementMetaData.m_ExtraPivotPointAxis)
                    {
                        case Axis.X:
                            placementMetaData.m_ExtraPivotPointGameObject.transform.localRotation = Quaternion.Euler(placementMetaData.m_ExtraPivotRotation, 0, 0);
                            break;
                        case Axis.Y:
                            placementMetaData.m_ExtraPivotPointGameObject.transform.localRotation = Quaternion.Euler(0, placementMetaData.m_ExtraPivotRotation, 0);
                            break;
                        case Axis.Z:
                            placementMetaData.m_ExtraPivotPointGameObject.transform.localRotation = Quaternion.Euler(0, 0, placementMetaData.m_ExtraPivotRotation);
                            break;
                        default:
                            break;
                    }
                    HouseBuilderRoot.Instance.RecorderRef.Record();
                }
            }
        }

        private void SetInputRotationExtraFieldPlaceholder()
        {
            m_InputRotationExtra.SetTextWithoutNotify(m_CurrentSelections.FirstOrDefault().PlacementMetaData.m_ExtraPivotRotation.ToString());
        }

    }
}