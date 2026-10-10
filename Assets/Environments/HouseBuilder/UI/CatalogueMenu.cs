using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ContentStagingMetaData;

namespace HouseBuilder
{
    public class CatalogueGeneratedData
    {
        public GameObject GeneratedItemRef { get; private set; }

        public void SetGeneratedItem(GameObject item) => GeneratedItemRef = item;

        public CatalogueEntry CatalogueEntryRef { get; private set; }

        public void SetCatalogueEntry(CatalogueEntry item) => CatalogueEntryRef = item;

        public Interaction InteractionRef { get; private set; }

        public void SetInteraction(Interaction item)
        {
            InteractionRef = item;
            InteractionRef.SetCatalogueGeneratedData(this);
        }

        public HouseBuilderConstants.WallObjectLinkerData m_WallObjectLinkerData;
        public HouseBuilderConstants.HouseBuilderContentsJsonData m_JsonData;
    }


    public class CatalogueMenu : MonoBehaviour
    {
        private const int NUMBER_OF_ICONS_ON_SCREEN = 26;
        //private const int NUMBER_OF_FILTERS_ON_SCREEN = 9;

        struct IconData
        {
            public Vector2 cellSize;
            public int constraintCount;
        }

        private readonly IconData SoundIconData = new IconData
        {
            cellSize = new Vector2(88*3, 54),
            constraintCount = 3,
        };

        private readonly IconData NonSoundIconData = new IconData
        {
            cellSize = new Vector2(88, 88),
            constraintCount = 2,
        };


        public enum ManualMenuContents
        {
            Staging = MenuContents.Staging,
            StagingLadder = MenuContents.StagingLadder,
            StagingTrigangle = MenuContents.StagingTrigangle,
            StagingSquare = MenuContents.StagingSquare,
            StagingRectangle = MenuContents.StagingRectangle,
        }

        public enum MenuContents
        {
            Brands,
            Interior,
            Openings,
            Staging,
            StagingLadder, 
            StagingTrigangle,
            StagingSquare,
            StagingRectangle,
            Materials,
            CeilingMaterials,
            FloorMaterials,
            WallMaterials,
            Textures,
            Video,

            PortraitTextures,
            LandscapeTextures,

            PortraitVideo,
            LandscapeVideo,

            Sound,
        }

        private Transform m_Catalogue;
        private CanvasGroup m_CatalogueGroup;
        private CanvasGroup m_PlacementView;
        private CanvasGroup m_StructureView;
        private CanvasGroup m_StagingView;

        private GameObject m_FilterPrefab;
        private GameObject m_ItemPrefab;
        private GameObject m_ItemSoundPrefab;

        // filter groups
        private Transform m_PlacementFiltersGroup;
        private Transform m_StructureFiltersGroup;
        private Transform m_StagingFiltersGroup;

        private GridLayoutGroup m_GridLayoutGroup;

        private const float Y_OPEN_CATALOGUE = 0;
        private float Y_CLOSED_CATALOGUE = -242f;

        // icon group
        private Transform m_ItemsGroup;
        private Scrollbar m_ScrollbarItems;
        private Button m_BtnNextItems;
        private Button m_BtnPrevItems;

        private ToggleGroup m_PlacementFiltersTglGroup;
        private ToggleGroup m_StructureFiltersTglGroup;
        private ToggleGroup m_StagingFiltersTglGroup;

        private Toggle m_ShowHideCatalogue;
        private Toggle m_TglInterior;
        private Toggle m_TglBrands;
        private Button m_BtnLoadFile;

        private Dictionary<ConectionType, Button> m_BtnCreateStaging = new Dictionary<ConectionType, Button>();

        public List<CatalogueEntry> FullCatalogue { get; private set; }
        public List<CatalogueMenuObject> AllMenuObjects { get; private set; }
        private List<CatalogueMenuFilter> m_AllMenuFilters;

        private List<CatalogueMenuObject> m_UserMenuObjects;
        private List<string> m_SelectedFilters;

        public Action<MenuContents, string, string> OnMaterialPicked;
        public Action<string, string> OnTexturePicked;
        public Action<string, string> OnVideoPicked;
        public Action<string, string> OnSoundPicked;


        private readonly List<string> m_PlacementBaseFilters = new List<string>()
        {
            "Furniture",
            "Position",
            "Instrument",
            "Brands",
            "Pro Audio",
        };

        private readonly List<string> m_StagingBaseFilters = new List<string>()
        {
            "Staging",
        };

        //A bunch of dictionaries for now because it makes it very easy to change what is shown and when
        //These are the filters that are set up in the catalogue editor
        private Dictionary<string, string[]> m_InteriorFilters = new Dictionary<string, string[]>(){
            { "Furniture", new string[] {"Lighting", "Table", "Seating"} },
            { "Position", new string[] {"Grounded","Wall-mounted","Hanging", "SetPiece", "Misc"} },
            { "Instrument", new string[] { "Guitar", "Keyboard", "Drums" } },
            { "Pro Audio", new string[] { "Lighting", "Mixer", "Miscellaneous" } }
        };

        private Dictionary<string, string[]> m_OpeningsFilters = new Dictionary<string, string[]>(){
            { "Construction", new string[] {"Window","Door" } }
        };

        private Dictionary<string, string[]> m_StagingFilters = new Dictionary<string, string[]>(){
            { "Staging", new string[] { "Ladder", "Square", "Triangle" } }
        };

        private Dictionary<string, string[]> m_StagingFiltersLadder = new Dictionary<string, string[]>(){
            { "Staging", new string[] { "Ladder" } }
        };

        private Dictionary<string, string[]> m_StagingFiltersSquare = new Dictionary<string, string[]>(){
            { "Staging", new string[] { "Square" } }
        };

        private Dictionary<string, string[]> m_StagingFiltersRectangle = new Dictionary<string, string[]>(){
            { "Staging", new string[] { "Rectangle" } }
        };

        private Dictionary<string, string[]> m_StagingFiltersTriangle = new Dictionary<string, string[]>(){
            { "Staging", new string[] { "Triangle" } }
        };


        private Dictionary<string, string[]> m_BrandsFilters = new Dictionary<string, string[]>(){
            { "Brands", new string[] {"Behringer","Midas","Turbosound","Tannoy","Lake","Klarkteknik","Labgruppen", "TCElectronic" } }
        };

        private static string[] SYSTEM_CUSTOM = new string[] { "System", "Custom" };

        private Dictionary<string, string[]> m_TexturesFilters = new Dictionary<string, string[]>(){
            { ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Portrait.ToString(), SYSTEM_CUSTOM  },
            { ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Landscape.ToString(), SYSTEM_CUSTOM  },
        };

        private Dictionary<string, string[]> m_TexturePortraitFilters = new Dictionary<string, string[]>(){
            { ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Portrait.ToString(), SYSTEM_CUSTOM }
        };

        private Dictionary<string, string[]> m_TextureLandscapeFilters = new Dictionary<string, string[]>(){
            { ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Landscape.ToString(), SYSTEM_CUSTOM }
        };

        private Dictionary<string, string[]> m_VideoFilters = new Dictionary<string, string[]>(){
            { ContentVideoPlayerMetaData.OrientationType.Portrait.ToString(), SYSTEM_CUSTOM  },
            { ContentVideoPlayerMetaData.OrientationType.Landscape.ToString(), SYSTEM_CUSTOM  },
        };

        private Dictionary<string, string[]> m_VideoPortraitFilters = new Dictionary<string, string[]>(){
            { ContentVideoPlayerMetaData.OrientationType.Portrait.ToString(), SYSTEM_CUSTOM  },
        };

        private Dictionary<string, string[]> m_VideoLandscapeFilters = new Dictionary<string, string[]>(){
            { ContentVideoPlayerMetaData.OrientationType.Landscape.ToString(), SYSTEM_CUSTOM  },
        };

        private Dictionary<string, string[]> m_SoundFilters = new Dictionary<string, string[]>(){
            { "Sound", SYSTEM_CUSTOM  },
        };


        private Dictionary<string, string[]> m_MaterialsFilters = new Dictionary<string, string[]>(){
            { "Ceiling", new string[] {"Concrete", "Metal", "Painted" } },
            { "Wall", new string[] {"Brick","Concrete","Metal","Painted","Wallpaper" } },
            { "Floor", new string[] {"Carpet","Concrete","Marble","Metallic","Wood" } }
        };



        private Dictionary<string, string[]> m_CeilingMaterials = new Dictionary<string, string[]>(){
            { "Ceiling", new string[] {"Concrete", "Metal", "Painted" } }
        };

        private Dictionary<string, string[]> m_WallMaterials = new Dictionary<string, string[]>(){
            { "Wall", new string[] {"Brick","Concrete","Metal","Painted","Wallpaper" } }
        };

        private Dictionary<string, string[]> m_FloorMaterials = new Dictionary<string, string[]>(){
            { "Floor", new string[] {"Carpet","Concrete","Marble","Metallic","Wood" } }
        };

        private Dictionary<string, MenuContents> m_MaterialsLinkageFilters = new Dictionary<string, MenuContents>(){
            { "Ceiling", MenuContents.CeilingMaterials },
            { "Wall", MenuContents.WallMaterials },
            { "Floor", MenuContents.FloorMaterials }
        };
        public enum CatalogueMenuObjectType
        {
            Material,
            Texture,
            Video,
            CatalogueMenuItem,
            Sound,
        }


        private CatalogueMenuObjectType m_CatalogueMenuObjectType = CatalogueMenuObjectType.Material;

        internal void Initialise(Dictionary<string, GameObject> contentTypes)
        {
            m_CatalogueGroup = gameObject.GetComponent<CanvasGroup>();
            m_CatalogueGroup.FixBrokenMasks();
            m_Catalogue = gameObject.GetComponent<Transform>();

            m_FilterPrefab = contentTypes["TglFilter"];
            m_ItemPrefab = contentTypes["CatItem"];
            m_ItemSoundPrefab = contentTypes["CatSoundItem"];

            // Structure components
            var structureView = this.transform.Find("StructureView");
            m_StructureView = structureView.GetComponent<CanvasGroup>();
            m_StructureView.FixBrokenMasks();
            m_StructureFiltersGroup = structureView.Find("StructureFilters/Viewport/Content");
            m_StructureFiltersTglGroup = m_StructureFiltersGroup.GetComponent<ToggleGroup>();
            //m_StructureFiltersScrollbar = structureView.Find("StructureFilters/Scrollbar").GetComponent<Scrollbar>();

            //m_StructureFiltersGroup
            var placementView = this.transform.Find("PlacementView");
            m_PlacementView = placementView.GetComponent<CanvasGroup>();
            m_PlacementView.FixBrokenMasks();
            m_PlacementFiltersGroup = placementView.Find("PlacementFilters/Viewport/Content");
            m_PlacementFiltersTglGroup = m_PlacementFiltersGroup.GetComponent<ToggleGroup>();


            var stagingView = this.transform.Find("StagingView");
            m_StagingView = stagingView.GetComponent<CanvasGroup>();
            m_StagingView.FixBrokenMasks();
            m_StagingFiltersGroup = stagingView.Find("PlacementFilters/Viewport/Content");
            m_StagingFiltersTglGroup = m_StagingFiltersGroup.GetComponent<ToggleGroup>();



            // icon group
            m_ItemsGroup = transform.Find("ItemView/BrowseScrollView/Viewport/Content");

            m_GridLayoutGroup = m_ItemsGroup.GetComponent<GridLayoutGroup>();

            m_ScrollbarItems = transform.Find("ItemView/BrowseScrollView/Scrollbar/").GetComponent<Scrollbar>();

            m_BtnNextItems = transform.Find("ItemView/BtnNextItems").GetComponent<Button>();
            m_BtnPrevItems = transform.Find("ItemView/BtnPreviousItems").GetComponent<Button>();

            m_ShowHideCatalogue = transform.Find("OnOffTgl").GetComponent<Toggle>();
            m_ShowHideCatalogue.onValueChanged.AddListener((value) => OpenCatalogue(value));

            m_BtnNextItems.onClick.AddListener(() =>
            {
                float amount = CalculateScrollPrecentage(NUMBER_OF_ICONS_ON_SCREEN, m_ItemsGroup);
                this.Create<ValueTween>(0.3f, EaseType.CubicOut, () => { CheckButtonsItems(); })
                .Initialise(m_ScrollbarItems.value, m_ScrollbarItems.value + amount, (f) =>
                {
                    f = f.Clamp(0, 1);
                    m_ScrollbarItems.value = f;
                });
            });

            m_BtnPrevItems.onClick.AddListener(() =>
            {
                float amount = CalculateScrollPrecentage(NUMBER_OF_ICONS_ON_SCREEN, m_ItemsGroup);
                this.Create<ValueTween>(0.3f, EaseType.CubicOut, () => { CheckButtonsItems(); })
                .Initialise(m_ScrollbarItems.value, m_ScrollbarItems.value - amount, (f) =>
                {
                    f = f.Clamp(0, 1);
                    m_ScrollbarItems.value = f;
                });
            });


            m_TglInterior = placementView.Find("TglGroup/TglInterior").GetComponent<Toggle>();
            m_TglBrands = placementView.Find("TglGroup/TglBrands").GetComponent<Toggle>();

            m_TglInterior.onValueChanged.AddListener((value) => ToggleInteriorItems(value));
            m_TglBrands.onValueChanged.AddListener((value) => ToggleBrandsItems(value));

            m_BtnLoadFile = placementView.Find("TglGroup/UploadBtn").GetComponent<Button>();
            m_BtnLoadFile.onClick.AddListener(HouseBuilderRoot.Instance.ImportFromDirectory);


            m_BtnCreateStaging.Add(ConectionType.Ladder, stagingView.Find("TglGroup/CreateStartLadder").GetComponent<Button>());
            m_BtnCreateStaging.Add(ConectionType.Triangle, stagingView.Find("TglGroup/CreateStartTriangle").GetComponent<Button>());
            m_BtnCreateStaging.Add(ConectionType.Square, stagingView.Find("TglGroup/CreateStartSquare").GetComponent<Button>());
            m_BtnCreateStaging.Add(ConectionType.Rectangle, stagingView.Find("TglGroup/CreateStartRectangle").GetComponent<Button>());

            foreach(var item in m_BtnCreateStaging)
            {
                m_BtnCreateStaging[item.Key].onClick.AddListener(() =>
                {
                    CreateStagingStartPoint(item.Key, (item) =>
                    {
                        HouseBuilderRoot.Instance.InputStaging.CreateStagingStartPoint(item);
                    });
                    Debug.LogError($"m_BtnCreateStaging {item.Key}");
                });
            }      


            FullCatalogue = Catalogue.Instance.GetCatalogue;
            AllMenuObjects = new List<CatalogueMenuObject>();
            m_AllMenuFilters = new List<CatalogueMenuFilter>();

            m_UserMenuObjects = new List<CatalogueMenuObject>();
            m_SelectedFilters = new List<string>();

            HouseBuilderRoot.Instance.OnAllAssetsLoaded += OnAllAssetsLoaded;
            HouseBuilderRoot.Instance.OnModeTransitionBegin += OnModeTransitionBegin;
            HouseBuilderRoot.Instance.OnStructureModeSelected += OnStructureModeSelected;
            HouseBuilderRoot.Instance.OnPlacementModeSelected += OnPlacementModeSelected;
            HouseBuilderRoot.Instance.OnSoundModeSelected += OnSoundModeSelected;
            HouseBuilderRoot.Instance.OnLightingModeSelected += OnLightModeSelected;
            HouseBuilderRoot.Instance.OnStagingModeSelected += OnStagingModeSelected;
            HouseBuilderRoot.Instance.OnModularModeSelected += (e) => OnModularModeSelected();

            HBGUIManager.Instance.WallSettingsMenuRef.PostInitialise(this, OnWallColourSettingsToggled);
            HBGUIManager.Instance.CeilingSettingsMenuRef.PostInitialise(this, OnCeilingColourSettingsToggled);
            HBGUIManager.Instance.FloorSettingsMenuRef.PostInitialise(this, OnFloorColourSettingsToggled);
            HBGUIManager.Instance.ItemSettingsMenuRef.PostInitialise(this);
            HBGUIManager.Instance.SoundSettingsMenuRef.PostInitialise(this);
            HBGUIManager.Instance.LightingSettingsMenuRef.PostInitialise(this);
            HBGUIManager.Instance.StagingSettingsMenuRef.PostInitialise(this);
            

            HBGUIManager.Instance.m_StructureSwitchMenu.OnStructureSwitchChanged += OnStructureSwitchChanged;

            HouseBuilderRoot.Instance.m_InitialisationsToWaitForTotal++;
            CheckButtonsItems();
            //CheckButtonsFilters();

            // needs to be done for screen resolutions // on my screen which is 3840x2160 , its -424
            Y_CLOSED_CATALOGUE = m_Catalogue.position.y;

            Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
        }

        private void OnEnvironmentLoadingComplete()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
            HBGUIManager.Instance.StagingSettingsMenuRef.InitCreateStartButtons(m_BtnCreateStaging);
        }

        private void CheckButtonsItems() => CheckButtons(m_ItemsGroup, m_ScrollbarItems, m_BtnPrevItems, m_BtnNextItems, NUMBER_OF_ICONS_ON_SCREEN);
 
        private void CheckButtons(Transform group, Scrollbar scrollBar, Button prev, Button next, int maxNumberOnScreen)
        {
            var directChildren = group.GetDirectChildren();
            directChildren.RemoveAll(e => e.gameObject.activeSelf == false);

            if (directChildren.Count > maxNumberOnScreen)
            {
                next.interactable = (scrollBar.value == 1 ? false : true);
                prev.interactable = (scrollBar.value == 0 ? false : true);
            }
            else
            {
                next.interactable = false;
                prev.interactable = false;
            }
        }

        private float CalculateScrollPrecentage(int numberItemsOnScreen, Transform contentGroup)
        {
            var directChildren = contentGroup.GetDirectChildren();
            directChildren.RemoveAll(e => e.gameObject.activeSelf == false);

            int count = directChildren.Count - numberItemsOnScreen;
            count = count.Clamp(0, count);
            float amount = 0;
            if (count != 0)
            {
                amount = ((float)numberItemsOnScreen) / ((float)count);
                amount = amount.Clamp(0, 1);
            }
            return amount;
        }

        private void SetVisible(bool state)
        {
            m_CatalogueGroup.VisibleAndInteractive(state);
        }

        private void OpenCatalogue(bool state)
        {
            float startPos = state ? Y_CLOSED_CATALOGUE : Y_OPEN_CATALOGUE;
            float closePos = state ? Y_OPEN_CATALOGUE: Y_CLOSED_CATALOGUE;
            Vector3 catPos = m_Catalogue.position;

            this.Create<ValueTween>(0.8f, EaseType.ExpoOut, () => 
            {}).Initialise(startPos, closePos, (f) =>
            {
                catPos.y = f; 
                m_Catalogue.position = catPos;
            });
        }

        private void OnAllAssetsLoaded()
        {
            if (AllMenuObjects.Count > 0)
            {
                foreach (var item in AllMenuObjects)
                {
                    Destroy(item.gameObject);
                }
            }

            TaskAction task = new TaskAction(FullCatalogue.Count, () =>
            {
                HouseBuilderRoot.Instance.m_InitialisationsToWaitForSoFar++;
            });
            for (int i = 0; i < FullCatalogue.Count; i++)
            {
                AllMenuObjects.Add(GenerateMenuItem(FullCatalogue[i], task));
            }

            foreach (KeyValuePair<string, string[]> pair in m_MaterialsFilters)
            {
                var menuContents = m_MaterialsLinkageFilters[pair.Key];
                Core.AssetBundlesRef.MaterialAssetBundleRef.GetItemList(this, pair.Key, (materials) =>
                {
                    for (int i = 0; i < materials.Count; i++)
                    {
                        var split = materials[i].Split('_').ToList();
                        string subFilter = split[1].First().ToString().ToUpper() + split[1].Substring(1);

                        AllMenuObjects.Add(GenerateMenuMaterial(menuContents, materials[i], pair.Key, subFilter));
                    }
                });
            }

            foreach (KeyValuePair<string, string[]> pair in m_TexturesFilters)
            {
                Core.AssetBundlesRef.Texture2DAssetBundleRef.GetItemList(this, pair.Key, (textures) =>
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        var split = textures[i].Split('_').ToList();
                        string subFilter = split[1].First().ToString().ToUpper() + split[1].Substring(1);

                        AllMenuObjects.Add(GenerateMenuTexture(textures[i], pair.Key, subFilter));
                    }
                });
            }


            foreach (KeyValuePair<string, string[]> pair in m_SoundFilters)
            {
                Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItemList(this, (audioClip) =>
                {
                    for (int i = 0; i < audioClip.Count; i++)
                    {
                        var split = audioClip[i].Split('_').ToList();
                        if (split.Count > 1)
                        {
                            AllMenuObjects.Add(GenerateMenuSound(audioClip[i], "Sound", split[0]));
                        }
                    }
                });
            }


            foreach (KeyValuePair<string, string[]> pair in m_TexturesFilters)
            {
                Core.AssetBundlesRef.VideoClipAssetBundleRef.GetItemList(this, pair.Key, (textures) =>
                {
                    for (int i = 0; i < textures.Count; i++)
                    {
                        var split = textures[i].Split('_').ToList();
                        string subFilter = split[1].First().ToString().ToUpper() + split[1].Substring(1);

                        AllMenuObjects.Add(GenerateMenuVideo(textures[i], pair.Key, subFilter));
                    }
                });
            }


            LoadCustomUserItems(false);

            RemoveCurrentFilters();

            LoadFilters(MenuContents.Interior);
            ShowRelevantObjects();
        }



        public void LoadCustomUserItems(bool checkItems)
        {
            if (checkItems == true)
            {
                if (m_UserMenuObjects != null)
                {
                    for (int i = 0; i < m_UserMenuObjects.Count; i++)
                    {
                        var item = m_UserMenuObjects[i];
                        AllMenuObjects.Remove(m_UserMenuObjects[i]);
                        Destroy(item);
                    }
                }
            }

            if (Core.Network.Users.CurrentUser != null)
            {
                Core.Network.GetCustomFileList((listOfItems) =>
                {
                    if (listOfItems != null)
                    {
                        listOfItems = listOfItems.Distinct().ToList();
                        for (int i = 0; i < listOfItems.Count; i++)
                        {
                            string itemName = listOfItems[i];
                            var split = itemName.Split('_').ToList();
                            if (itemName.EndsWith("png", StringComparison.CurrentCultureIgnoreCase) || itemName.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase) || itemName.EndsWith("mp3", StringComparison.CurrentCultureIgnoreCase) || itemName.EndsWith("wav", StringComparison.CurrentCultureIgnoreCase))
                            {
                                string subFilter = "";

                                if (split.Count > 1)
                                {
                                    subFilter = split[1].First().ToString().ToUpper() + split[1].Substring(1);
                                }
                                if (itemName.EndsWith("png", StringComparison.CurrentCultureIgnoreCase) || itemName.EndsWith("jpg", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    Core.Network.GetCustomFileTexture(itemName, (texture) =>
                                    {
                                        m_UserMenuObjects.Add(GenerateMenuTexture(itemName, split.First(), subFilter, texture));
                                        AllMenuObjects.Add(GenerateMenuTexture(itemName, split.First(), subFilter, texture));
                                    });
                                }
                                else if (itemName.EndsWith("mp3", StringComparison.CurrentCultureIgnoreCase) || itemName.EndsWith("wav", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    m_UserMenuObjects.Add(GenerateMenuSound(itemName, "Sound", "Custom")); //m_SoundFilters
                                    AllMenuObjects.Add(GenerateMenuSound(itemName, "Sound", "Custom"));  //m_SoundFilters
                                }
                            }
                        }
                    }
                });
            }
        }

        private void OnModeTransitionBegin()
        {
            SetVisible(false);
        }

        private void OnStructureModeSelected()
        {
            VisualiseStructureView(true);
            VisualisePlacementView(false);
            VisualiseStagingView(false);
        }

        private void OnPlacementModeSelected()
        {
            VisualiseStructureView(false);
            VisualisePlacementView(true);
            VisualiseStagingView(false);
            SetVisible(true);

            m_TglInterior.isOn = true;
            ToggleInteriorItems(true); // just in case the button is already on, to fire the event again
        }


        private void OnSoundModeSelected()
        {
            VisualiseStructureView(true);
            VisualisePlacementView(false);
            VisualiseStagingView(false);
            SetVisible(true);

            m_TglInterior.isOn = true;
            ToggleSoundItems(true); // just in case the button is already on, to fire the event again
        }


        private void OnLightModeSelected()
        {
            VisualiseStructureView(false);
            VisualisePlacementView(false);
            VisualiseStagingView(false);
            SetVisible(false);
        }

        private void OnStagingModeSelected()
        {
            VisualiseStructureView(false);
            VisualisePlacementView(false);
            VisualiseStagingView(true);
            SetVisible(true);
            ToggleStagingItems(true); // just in case the button is already on, to fire the event again
        }


        private void OnModularModeSelected()
        {
            VisualiseStructureView(false);
            VisualisePlacementView(false);
            VisualiseStagingView(false);
            SetVisible(false);
        }

        private void VisualiseStructureView(bool state)
        {
            m_StructureView.alpha = state ? 1 : 0;
            m_StructureView.interactable = state;
            m_StructureView.blocksRaycasts = state;
        }

        private void VisualisePlacementView(bool state)
        {
            m_PlacementView.alpha = state ? 1 : 0;
            m_PlacementView.interactable = state;
            m_PlacementView.blocksRaycasts = state;
        }

        private void VisualiseStagingView(bool state)
        {
            m_StagingView.alpha = state ? 1 : 0;
            m_StagingView.interactable = state;
            m_StagingView.blocksRaycasts = state;
        }


        public void ToggleItemTextures(bool state, ContentMaterialChangeMetaData.MaterialItemData.OrientationType orientationType)
        {
            if (state)
            {

                switch (orientationType)
                {
                    case ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Portrait:
                        LoadFilters(MenuContents.PortraitTextures);
                        break;
                    case ContentMaterialChangeMetaData.MaterialItemData.OrientationType.Landscape:
                        LoadFilters(MenuContents.LandscapeTextures);
                        break;
                    default:
                        Debug.LogError($"{orientationType} has not been impimented");
                        break;
                }
                ShowRelevantObjects();
            }
            else
            {
                OnPlacementModeSelected();
            }
        }


        public void ToggleItemVideos(bool state, ContentVideoPlayerMetaData.OrientationType orientationType)
        {
            if (state)
            {
                switch (orientationType)
                {
                    case ContentVideoPlayerMetaData.OrientationType.Portrait:
                        LoadFilters(MenuContents.PortraitVideo);
                        break;
                    case ContentVideoPlayerMetaData.OrientationType.Landscape:
                        LoadFilters(MenuContents.LandscapeVideo);
                        break;
                }
               ShowRelevantObjects();
            }
            else
            {
                OnPlacementModeSelected();
            }
        }


        private void ToggleInteriorItems(bool state)
        {
            if (state)
            {
               LoadFilters(MenuContents.Interior);
               ShowRelevantObjects();
            }
        }

        private void ToggleStagingItems(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.Staging);
                ShowRelevantObjects();
            }
        }


        public void ManualSetFilter(ManualMenuContents filter)
        {
            LoadFilters((MenuContents)(int)filter);
            ShowRelevantObjects();
        }
        private void ToggleBrandsItems(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.Brands);
                ShowRelevantObjects();
            }
        }

        private void OnWallColourSettingsToggled(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.WallMaterials);
                ShowRelevantObjects();
            }
        }

        private void OnCeilingColourSettingsToggled(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.CeilingMaterials);
                ShowRelevantObjects();
            }
        }

        private void OnFloorColourSettingsToggled(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.FloorMaterials);
                ShowRelevantObjects();
            }
        }

        private void ToggleSoundItems(bool state)
        {
            if (state)
            {
                LoadFilters(MenuContents.Sound);
                ShowRelevantObjects();
                SetRelevantObjectsInteractable(false);
            }
        }

        public void CreateStagingStartPoint(ConectionType conectionType, Action<CatalogueGeneratedData> item)
        {
            var entry = Core.Catalogue.GetEntry(StagingSettingsMenu.STAGE_START_ITEMS[conectionType]);
            TaskAction task = new TaskAction(1, () =>
            {
                var last = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList.Last();
                last.InteractionRef.RootGameObject.transform.position = Vector3.zero;
                item?.Invoke(last);
                last.InteractionRef.GetComponentInChildren<Renderer>().enabled = false; // turn off renderer
                Camera.main.gameObject.MoveToLookAtRealive(last.InteractionRef.RootGameObject);
            });
            LoadSaveHouseContentsData.GenerateCataloguePrefab(
                this, //MonoBehaviour mono
                0,
                entry, //CatalogueEntry entry
                null, //HouseBuilderContentsJsonData dataRef
                HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AddGameObject, //Action< CatalogueGeneratedData > addContainer
                GenerateCatalogueType.CreateInGame, //GenerateCatalogueType generateCatalogueType,
                task, //TaskAction task
                   (justAddedItem) =>
                   {
                       if (justAddedItem != null)
                       {
                           GenerateSubItems(justAddedItem.GeneratedItemRef, entry);
                       }
                       else
                       {
                           Debug.LogError($"Error creating {entry.ShortName}   {entry.Guid}");
                       }
                   });
        
        }

        private CatalogueMenuItem GenerateMenuItem(CatalogueEntry entry, TaskAction task)
        {
            var gameObject = Instantiate(m_ItemPrefab, m_ItemsGroup);
            var newItem = gameObject.ForceComponent<CatalogueMenuItem>();
            newItem.Initialise(entry, task, () =>
             {
                 HBGUIManager.Instance.ItemSettingsMenuRef.SetActive(false, false);
                 var item = HouseBuilderRoot.Instance.InputContents.GetLastInteraction();

                 var currentMode = HouseBuilderRoot.Instance.InputContents.CurrentInputMode;
                 if (item != null && (currentMode == HouseBuilderInput.InputMode.Placing) && (HouseBuilderRoot.Instance.CurrentMode != BuilderMode.Staging))
                 {
                     HouseBuilderRoot.Instance.InputContents.ModeContainerRef.Delete(item.gameObject);
                 }

                 LoadSaveHouseContentsData.GenerateCataloguePrefab(
                     this, //MonoBehaviour mono
                     0,
                     entry, //CatalogueEntry entry
                     null, //HouseBuilderContentsJsonData dataRef
                     HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AddGameObject, //Action< CatalogueGeneratedData > addContainer
                     GenerateCatalogueType.CreateInGame, //GenerateCatalogueType generateCatalogueType,
                     null, //TaskAction task
                        (justAddedItem) =>
                        {
                            if (justAddedItem != null)
                            {
                                GenerateSubItems(justAddedItem.GeneratedItemRef, entry);
                            }
                            else
                            {
                                Debug.LogError($"Error creating {entry.ShortName}   {entry.Guid}");
                            }
                        });
             });
            return newItem;
        }

        private void GenerateSubItems(GameObject root, CatalogueEntry entry)
        {
            Interaction rootInteraction = root.GetComponent<Interaction>();


            List<CatalogueGeneratedData> AddedItems = new List<CatalogueGeneratedData>();
            TaskAction task = new TaskAction(entry.m_SubItems.Count, () =>
            {
                HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(null);
                HouseBuilderRoot.Instance.InputContents.CleanUp();
                var rootInteraction = root.GetComponent<Interaction>();
                if (rootInteraction.PlacementMetaData.Placement != ContentPlacementMetaData.EnumPlacement.SetToZero)
                {
                    HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(root.GetComponent<Interaction>());
                }
                HouseBuilderRoot.Instance.RecorderRef.Record(forceRecord:true);
            });

            
            for (int i = 0; i < entry.m_SubItems.Count; i++)
            {
                GameObject oldGameObject = root.transform.Search(entry.m_SubItems[i].m_ObjectName).gameObject;
                var subItemEntry = Core.Catalogue.GetCatalogue.Find(e => e.Guid == entry.m_SubItems[i].m_GUID);


                LoadSaveHouseContentsData.GenerateCataloguePrefab(
                    Core.Mono, //MonoBehaviour mono
                    0, 
                    subItemEntry, //CatalogueEntry entry
                    null, //HouseBuilderContentsJsonData dataRef
                    HouseBuilderRoot.Instance.InputContents.ModeContainerRef.AddGameObject, //Action< CatalogueGeneratedData > addContainer
                    GenerateCatalogueType.CreateInGame, //GenerateCatalogueType generateCatalogueType,
                    null, //TaskAction task
                    (justAddedItem) =>
                    {
                        Debug.Log($"GenerateSubItems  oldGameObject {oldGameObject.name}   justAddedItem {justAddedItem.CatalogueEntryRef.FullName}");
                        ReplaceGameObject(rootInteraction, oldGameObject, justAddedItem);
                        AddedItems.Add(justAddedItem);
                        task.Increment();
                    }); //Action<CatalogueGeneratedData> callback
            }
        }


        private CatalogueMenuMaterial GenerateMenuMaterial(MenuContents menuContents, string materialName, string group, string filter)
        {
            var gameObject = Instantiate(m_ItemPrefab, m_ItemsGroup);
            var newMaterialItem = gameObject.ForceComponent<CatalogueMenuMaterial>();

            newMaterialItem.Initialise(materialName, group, filter, () =>
             {
                 Debug.Log("GenerateMenuMaterial clicked");
                 OnMaterialPicked?.Invoke(menuContents, materialName, filter);
             });

            return newMaterialItem;
        }

        private CatalogueMenuTexture GenerateMenuTexture(string textureName, string group, string filter)
        {
            var gameObject = Instantiate(m_ItemPrefab, m_ItemsGroup);
            var newTextureItem = gameObject.ForceComponent<CatalogueMenuTexture>();

            newTextureItem.Initialise(textureName, group, filter, () =>
            {
                Debug.Log("GenerateMenuTexture clicked");
                OnTexturePicked?.Invoke(textureName, filter);
            });

            return newTextureItem;
        }

        private CatalogueMenuVideo GenerateMenuVideo(string textureName, string group, string filter)
        {
            var gameObject = Instantiate(m_ItemPrefab, m_ItemsGroup);
            var newTextureItem = gameObject.ForceComponent<CatalogueMenuVideo>();

            newTextureItem.Initialise(textureName, group, filter, () =>
            {
                Debug.Log("GenerateMenuVideo clicked");
                OnVideoPicked?.Invoke(textureName, filter);
            });

            return newTextureItem;
        }

        private CatalogueMenuTexture GenerateMenuTexture(string textureName, string group, string filter, Texture texture)
        {
            var gameObject = Instantiate(m_ItemPrefab, m_ItemsGroup);
            var newTextureItem = gameObject.ForceComponent<CatalogueMenuTexture>();

            newTextureItem.Initialise(textureName, group, filter, texture, () =>
            {
                Debug.Log("GenerateMenuTexture clicked");
                OnTexturePicked?.Invoke(textureName, filter);
            });

            return newTextureItem;
        }


        private CatalogueMenuSound GenerateMenuSound(string textureName, string group, string filter)
        {
            var gameObject = Instantiate(m_ItemSoundPrefab, m_ItemsGroup);
            var newTextureItem = gameObject.ForceComponent<CatalogueMenuSound>();

            newTextureItem.Initialise(textureName, group, filter, () =>
            {
                Debug.Log("GenerateMenuSound clicked");
                OnSoundPicked?.Invoke(textureName, filter);
            });

            return newTextureItem;
        }



        private CatalogueMenuFilter GenerateFilter(string group, string name)
        {
            var gameObject = Instantiate(m_FilterPrefab);
            gameObject.name = $"{name}";

            if (m_PlacementBaseFilters.Contains(group))
            {
                gameObject.transform.SetParent(m_PlacementFiltersGroup);
                gameObject.GetComponent<Toggle>().group = m_PlacementFiltersTglGroup;
            }
            else if (m_StagingBaseFilters.Contains(group))
            {
                gameObject.transform.SetParent(m_StagingFiltersGroup);
                gameObject.GetComponent<Toggle>().group = m_StagingFiltersTglGroup;
            }
            else
            {
                gameObject.transform.SetParent(m_StructureFiltersGroup);
                gameObject.GetComponent<Toggle>().group = m_StructureFiltersTglGroup;
            }



            var newFilter = gameObject.AddComponent<CatalogueMenuFilter>();

            newFilter.Initialise(group, name, (value) =>
            {
                OnFilterToggled(group + "/" + name, value);
            });
            return newFilter;
        }

        private void ReplaceGameObject(Interaction root, GameObject old, CatalogueGeneratedData justAddedItem)
        {
            HouseBuilderRoot.Instance.InputContents.SetCurrentInteraction(null);
            var pos = old.transform.localPosition;
            var rot = old.transform.localRotation;
            var parent = old.transform.parent;

            justAddedItem.GeneratedItemRef.transform.SetParent(parent);
            justAddedItem.GeneratedItemRef.transform.localPosition = pos;
            justAddedItem.GeneratedItemRef.transform.localRotation = rot;

            justAddedItem.m_JsonData.m_ParentGeneratedGUID = root.m_RootItemsGeneratedGUID;

            //HouseBuilderRoot.Instance.ParentItem(justAddedItem);
            old.SetActive(false);
            Debug.Log($"ReplaceGameObject {old.GetGameObjectPath()}");
            Destroy(old);
        }


        private void ShowRelevantObjects()
        {
            for (int i = 0; i < AllMenuObjects.Count; i++)
            {
                var item = AllMenuObjects[i];
                item.SetActive(false);

                for (int k = 0; k < m_AllMenuFilters.Count; k++)
                {
                    string filter = m_AllMenuFilters[k].m_FilterName;
                    if (item.MatchSingleFilter(filter))
                    {
                        if(IsItemValid(item) == true)
                        {
                            item.SetActive(true);
                        }
                    }
                }
            }

            m_ScrollbarItems.value = 0;
            CheckButtonsItems();
            //OnlyShowValidFilters();
            //CheckButtonsFilters();

            //CheckButtonsFilters();
            SetRelevantObjectsInteractable(true);
            ChangeGridSize();
        }


        public void SetRelevantObjectsInteractable(bool interactable )
        {
            if (AllMenuObjects != null)
            {
                foreach(var item in AllMenuObjects)
                {
                    if(item != null && item.m_Button != null)
                    {
                        item.m_Button.interactable = interactable;
                    }
                }
            }
        }

        private void ChangeGridSize()
        {
            var size = m_GridLayoutGroup.cellSize;
            if (m_CatalogueMenuObjectType == CatalogueMenuObjectType.Sound)
            {
                size = SoundIconData.cellSize;
                m_GridLayoutGroup.constraintCount = SoundIconData.constraintCount;
            }
            else
            {
                size = NonSoundIconData.cellSize;
                m_GridLayoutGroup.constraintCount = NonSoundIconData.constraintCount;
            }
            m_GridLayoutGroup.cellSize = size;
        }


        private bool IsItemValid(CatalogueMenuObject item)
        {
            switch (m_CatalogueMenuObjectType)
            {
                case CatalogueMenuObjectType.Material:
                    if (item is CatalogueMenuMaterial)
                    {
                        return true;
                    }
                    break;
                case CatalogueMenuObjectType.Texture:
                    if (item is CatalogueMenuTexture)
                    {
                        return true;
                    }
                    break;
                case CatalogueMenuObjectType.Video:
                    if (item is CatalogueMenuVideo)
                    {
                        return true;
                    }
                    break;
                case CatalogueMenuObjectType.CatalogueMenuItem:
                    if (item is CatalogueMenuItem)
                    {
                        return true;
                    }
                    break;
                case CatalogueMenuObjectType.Sound:
                    if (item is CatalogueMenuSound)
                    {
                        return true;
                    }
                    break;
                    
                default:
                    break;
            }
            return false;
        }

        private void LoadFilters(MenuContents menuContents)
        {
            RemoveCurrentFilters();

            Dictionary<string, string[]> targetFilters = new Dictionary<string, string[]>();

            switch (menuContents)
            {
                case MenuContents.Interior:
                    targetFilters = m_InteriorFilters;
                    break;

                case MenuContents.Openings:
                    targetFilters = m_OpeningsFilters;
                    break;


                case MenuContents.Staging:
                    targetFilters = m_StagingFilters;
                    break;

                case MenuContents.StagingLadder:
                    targetFilters = m_StagingFiltersLadder;
                    break;

                case MenuContents.StagingTrigangle:
                    targetFilters = m_StagingFiltersTriangle;
                    break;

                case MenuContents.StagingSquare:
                    targetFilters = m_StagingFiltersSquare;
                    break;

                case MenuContents.StagingRectangle:
                    targetFilters = m_StagingFiltersRectangle;
                    break;

                case MenuContents.Brands:
                    targetFilters = m_BrandsFilters;
                    break;

                case MenuContents.Textures:
                    targetFilters = m_TexturesFilters;
                    break;

                case MenuContents.Video:
                    targetFilters = m_VideoFilters;
                    break;

                case MenuContents.Materials:
                    targetFilters = m_MaterialsFilters;
                    break;

                case MenuContents.CeilingMaterials:
                    targetFilters = m_CeilingMaterials;
                    break;

                case MenuContents.WallMaterials:
                    targetFilters = m_WallMaterials;
                    break;

                case MenuContents.FloorMaterials:
                    targetFilters = m_FloorMaterials;
                    break;

                case MenuContents.PortraitTextures:
                    targetFilters = m_TexturePortraitFilters;
                    break;

                case MenuContents.LandscapeTextures:
                    targetFilters = m_TextureLandscapeFilters;
                    break;

                case MenuContents.PortraitVideo:
                    targetFilters = m_VideoPortraitFilters;
                    break;

                case MenuContents.LandscapeVideo:
                    targetFilters = m_VideoLandscapeFilters;
                    break;

                case MenuContents.Sound:
                    targetFilters = m_SoundFilters;
                    break;

                default:
                    Debug.LogError($"{menuContents} has not been impimented");
                    break;

            }

            switch (menuContents)
            {
                case MenuContents.Brands:
                case MenuContents.Interior:
                case MenuContents.Openings:
                case MenuContents.Staging:
                    m_CatalogueMenuObjectType = CatalogueMenuObjectType.CatalogueMenuItem;
                    break;
              
                case MenuContents.Materials:
                case MenuContents.CeilingMaterials:
                case MenuContents.FloorMaterials:
                case MenuContents.WallMaterials:
                    m_CatalogueMenuObjectType = CatalogueMenuObjectType.Material;
                    break;

                case MenuContents.Textures:
                case MenuContents.PortraitTextures:
                case MenuContents.LandscapeTextures:
                    m_CatalogueMenuObjectType = CatalogueMenuObjectType.Texture;
                    break;

                case MenuContents.Video:
                case MenuContents.PortraitVideo:
                case MenuContents.LandscapeVideo:
                    m_CatalogueMenuObjectType = CatalogueMenuObjectType.Video;
                    break;

                case MenuContents.Sound:
                    m_CatalogueMenuObjectType = CatalogueMenuObjectType.Sound;
                    break;
            }

            
            foreach (KeyValuePair<string, string[]> pair in targetFilters)
            {
                foreach (string item in pair.Value)
                {
                    m_AllMenuFilters.Add(GenerateFilter(pair.Key, item));
                }
            }
        }

        private void OnFilterToggled(string fullName, bool state)
        {
            if (state)
            {
                if (!m_SelectedFilters.Contains(fullName))
                    m_SelectedFilters.Add(fullName);
            }
            else
            {
                if (m_SelectedFilters.Contains(fullName))
                    m_SelectedFilters.Remove(fullName);
            }

            ApplyFiltersToObjects();
            RefreshFilters();
        }

        private void RefreshFilters()
        {
            if (m_SelectedFilters.Count == 0)
            {
                m_AllMenuFilters.ForEach(e => e.SetActive(true));

                ShowRelevantObjects();
                //OnlyShowValidFilters();
            }
            else
            {
                //OnlyShowValidFilters();
            }
            //CheckButtonsFilters();
        }

        private void OnlyShowValidFilters()
        {
            m_AllMenuFilters.ForEach(e => e.SetActive(false));

            var visibleItems = AllMenuObjects.FindAll((e) => e.m_IsActive == true);

            for (int i = 0; i < m_AllMenuFilters.Count; i++)
            {
                var filter = m_AllMenuFilters[i];

                for (int k = 0; k < visibleItems.Count; k++)
                {
                    if (visibleItems[k].MatchSingleFilter(filter.m_FilterName))
                    {
                        filter.SetActive(true);
                    }
                }
            }
        }

        private void ApplyFiltersToObjects()
        {
            for (int i = 0; i < AllMenuObjects.Count; i++)
            {
                var item = AllMenuObjects[i];

                if (m_SelectedFilters.Count == 0 || (item.MatchFilters(m_SelectedFilters) && IsItemValid(item)))
                {
                    item.SetActive(true);
                }
                else
                {
                    item.SetActive(false);
                }
            }
        }

        private void RemoveCurrentFilters()
        {
            m_SelectedFilters.Clear();

            if (m_AllMenuFilters.Count > 0)
            {
                foreach (var filter in m_AllMenuFilters)
                {
                    Destroy(filter.gameObject);
                }
            }
            m_AllMenuFilters.Clear();
        }

        internal void OnStructureSwitchChanged(StructureType currentSelection)
        {
            switch (currentSelection)
            {
                case StructureType.None:
                    SetVisible(false);
                    break;

                case StructureType.Ceiling:
                    LoadFilters(MenuContents.CeilingMaterials);
                    ShowRelevantObjects();
                    SetVisible(true);
                    break;

                case StructureType.Openings:
                    LoadFilters(MenuContents.Openings);
                    ShowRelevantObjects();
                    SetVisible(true);
                    break;

                case StructureType.Staging:
                    LoadFilters(MenuContents.Staging);
                    ShowRelevantObjects();
                    SetVisible(true);
                    break;

                case StructureType.Walls:
                    LoadFilters(MenuContents.WallMaterials);
                    ShowRelevantObjects();
                    SetVisible(true);
                    break;

                case StructureType.Floor:
                    LoadFilters(MenuContents.FloorMaterials);
                    ShowRelevantObjects();
                    SetVisible(true);
                    break;
            }
        }

        private void OnDestroy()
        {
            Core.Environment.OnEnvironmentLoadingComplete -= OnAllAssetsLoaded;
        }

        public List<CatalogueMenuObject> GetPossibleItems()
        {
            if (AllMenuObjects != null)
            {
                return AllMenuObjects;
            }
            else
            {
                Debug.LogError("List is empty");
                return null;
            }
        }
    }
}