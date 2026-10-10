using HouseBuilder;
using SteamAudio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static LoadSaveGeneratedSound;
using Vector3 = UnityEngine.Vector3;

public class SoundSimulationMenu : MonoBehaviour
{ 
    public enum MapCombinations
    {
        Occulsion,
        Occulsion_T_DirectionFactor,
        Occulsion_T_DistanceAttuenuation,
        Occulsion_T_Attuen_T_DirectionFactor,
        Occulsion_T_AirMed,
        Occulsion_T_AirMed_T_DirectionFactor, 
    }

    public enum ArithmaticEnum
    {
        Multiply,
        Add, 
        Subtract,
    }

    public class AudioTextureOptionData
    {
        public Toggle SelectedToggle;
        public Toggle InvertedToggle;
        public InputField m_FactorAmount;
    }

    public class SpeakerSelectionData
    {
        public GameObject m_Root;
        public Toggle SelectedToggle;
        public Text m_Text;
    }

    public class SplSelectionData
    {
        public GameObject m_Root;
        public Dropdown Names;
        public Dropdown Arithmetic;
        public Toggle m_Normilise;
    }


    // Keep track of stuff.
    private bool m_bMenuOpen = true;
    private bool m_bProbesVisible = false;

    // The button for this menu.
    private Button m_MenuButton;

    // Status Objects.
    private GameObject m_StatusReady;
    private GameObject m_StatusDownload;
    private GameObject m_StatusBake;
    private GameObject m_StatusRebake;
    private GameObject m_StatusUpload;

   

    private Text m_ServerInfoText;
    private Text[] m_LocalInfoObjects;
    private int m_iLocalInfoLength;

    // Quality Settings Objects.
    private Slider m_ProbeSpacingSlider;
    private InputField m_ProbeSpacingInputField;

    private Slider m_ProbeHeightSlider;
    private InputField m_ProbeHeightInputField;



    private Text m_NumberOfProbesText;
    private Text m_EstimatedBakeTimeText;
    private GameObject m_ShowProbesButton;
    private GameObject m_HideProbesButton;
    private GameObject m_CalculateTimeButton;

    // Store the quality settings.
    private float m_fProbeSpacing = 1f;
    private float m_fProbeHeight = 1.8f;


    // Probe sim time. (This is for my PC so need to adjust this after first bake and config it or something).
    private float m_fProbeSimTime = 0.002f; //0.0005f;

    // texture map menuu
    private GameObject m_TextureMapMenu;

    private Slider m_TextureMapPerMeterSlider;
    private InputField m_TextureMapPerMeterInputField;
    private int m_TextureMapPerMetre = 1;

    private Slider m_TextureMapMaxHeightSlider;
    private InputField m_TextureMapMaxHeightInputField;
    private float m_TextureMapMaxHeight = 2f;

    private Renderer m_TextureDisplayQuadRenderer;

    private Dropdown m_NumberOfCustoms;
    private Gradient m_Gradient;

    private LoadSaveGeneratedSound m_SoundSimLoadSaveRef;


    private Dictionary<TextureMapType, AudioTextureOptionData> m_AudioTextureOptionData = new Dictionary<TextureMapType, AudioTextureOptionData>();

    private const string SPEAKER_SELECTION_PREFAB = "SpeakerSelectionPrefab";
    private  PoolManager<SpeakerSelectionData> m_SpeakerSelectionPool;
    private GameObject m_SpeakerSelectionViewContent;


    private const string SPL_SELECTION_PREFAB = "SplsCombination";
    private PoolManager<SplSelectionData> m_SplSelectionPool;
    private GameObject m_CombinationDataContent;

    //
    public Image m_GradientImage;
    public Texture2D m_GradientTexture2D;
    public UnityEngine.Material m_GradientMaterial;
    public UnityEngine.Sprite m_GradientSprite;
    public List<Slider> m_SliderList = new List<Slider>();

    public void TurnOffSPLMap() => m_TextureDisplayQuadRenderer.SetActive(false);

    public void Initialise(Button soundSimButton)
    {
        // Get references.
        m_SoundSimLoadSaveRef = Core.Audio.LoadSaveGeneratedSoundRef;

        // Get the button for this menu, and hide it until we're in sound mode.
        m_MenuButton = soundSimButton;
        HouseBuilderRoot.Instance.OnSoundModeSelected += ShowSoundSimButton;
        HouseBuilderRoot.Instance.OnPlotModeSelected += HideSoundSimButton;
        HouseBuilderRoot.Instance.OnStructureModeSelected += HideSoundSimButton;
        HouseBuilderRoot.Instance.OnPlacementModeSelected += HideSoundSimButton;
        HouseBuilderRoot.Instance.OnLightingModeSelected += HideSoundSimButton;
        HouseBuilderRoot.Instance.OnStagingModeSelected += HideSoundSimButton;
        HouseBuilderRoot.Instance.OnOSCModeSelected += HideSoundSimButton;
        HideSoundSimButton();

        // Find and setup the various buttons and menu items.
        Transform[] children = this.GetComponentsInChildren<Transform>(true);
        string sName;
        List<Text> localInfoObjects = new List<Text>();
        for (int i = 0; i < children.Length; i++)
        {
            sName = children[i].name;

            // Menu open/close buttons.
            if (sName == "Quit Button")
            {
                children[i].GetComponent<Button>().onClick.AddListener(OpenCloseMenu);
            }
            else if (sName == "Close Sound Sim Settings")
            {
                children[i].GetComponent<Button>().onClick.AddListener(OpenCloseMenu);
            }
            // Status stuff.
            else if (sName == "Status Ready")
            {
                m_StatusReady = children[i].gameObject;
            }
            else if (sName == "Status Download")
            {
                m_StatusDownload = children[i].gameObject;
            }
            else if (sName == "Status Bake")
            {
                m_StatusBake = children[i].gameObject;
            }
            else if (sName == "Status Rebake")
            {
                m_StatusRebake = children[i].gameObject;
            }
            else if (sName == "Status Upload")
            {
                m_StatusUpload = children[i].gameObject;
            }
            else if (sName == "Download Data Button")
            {
                children[i].GetComponent<Button>().onClick.AddListener(DownloadData);
            }
            else if (sName == "Upload Data Button")
            {
                children[i].GetComponent<Button>().onClick.AddListener(UploadData);
            }
            else if (sName == "Server Info")
            {
                // So far there's only one of these, if it changes do the same list thing as the Local Info button.
                m_ServerInfoText = children[i].GetComponent<Text>();
            }
            else if (sName == "Local Info")
            {
                localInfoObjects.Add(children[i].GetComponent<Text>());
            }
            // Quality settings stuff.
            else if (sName == "Calculate Info Button")
            {
                m_CalculateTimeButton = children[i].gameObject;
                m_CalculateTimeButton.GetComponent<Button>().onClick.AddListener(UpdateTimeEstimate);
            }
            else if (sName == "Probe Distance Slider")
            {
                m_ProbeSpacingSlider = children[i].GetComponent<Slider>();
                m_ProbeSpacingSlider.onValueChanged.AddListener(UpdateProbeSpacing);
            }
            else if (sName == "Probe Distance Input Field")
            {
                m_ProbeSpacingInputField = children[i].GetComponent<InputField>();
                m_ProbeSpacingInputField.onEndEdit.AddListener(UpdateProbeSpacing);
            }
            else if (sName == "Probe Height Slider")
            {
                m_ProbeHeightSlider = children[i].GetComponent<Slider>();
                m_ProbeHeightSlider.onValueChanged.AddListener(UpdateProbeHeight);
            }
            else if (sName == "Probe Height Input Field")
            {
                m_ProbeHeightInputField = children[i].GetComponent<InputField>();
                m_ProbeHeightInputField.onEndEdit.AddListener(UpdateProbeHeight);
            }
            else if (sName == "Number of Probes Text")
            {
                m_NumberOfProbesText = children[i].GetComponent<Text>();
            }
            else if (sName == "Estimated Time Text")
            {
                m_EstimatedBakeTimeText = children[i].GetComponent<Text>();
            }
            // Baking and probe buttons.
            else if (sName == "Show Probes Button")
            {
                m_ShowProbesButton = children[i].gameObject;
                m_ShowProbesButton.GetComponent<Button>().onClick.AddListener(ShowProbes);
            }
            else if (sName == "Hide Probes Button")
            {
                m_HideProbesButton = children[i].gameObject;
                m_HideProbesButton.GetComponent<Button>().onClick.AddListener(HideProbes);
            }
            else if (sName == "Bake Data Button")
            {
                children[i].GetComponent<Button>().onClick.AddListener(GenerateSound);
            }

            // audio textureMap
            else if (sName == "TextureMapsMenu")
            {
                m_TextureMapMenu = children[i].gameObject;
                m_TextureMapMenu.SetActive(false);
            }
            else if (sName == "CreateTextureMapsMenu")
            {
                m_TextureMapMenu = children[i].gameObject;
                children[i].GetComponent<Button>().onClick.AddListener(OpenTextureMapMenu);
            }
            else if (sName == "CloseTextureMapMenu")
            {
                children[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    m_TextureMapMenu.SetActive(false);
                    UpdateStatus();
                });
            }
            else if (sName == "TextureDisplayQuad")
            {
                GameObject obj = children[i].gameObject;
                m_TextureDisplayQuadRenderer = obj.GetComponent<Renderer>();
                m_TextureDisplayQuadRenderer.transform.SetParent(null);
                m_TextureDisplayQuadRenderer.SetActive(false);              
            }
        }

        var textureMapsMenu = this.transform.SearchComponent<Transform>("TextureMapsMenu");
        AssignTextureMapsMenu(textureMapsMenu);

        var gradientHolder = this.transform.SearchComponent<Transform>("GradientHolder");
        AssignGradientMenu(gradientHolder);

        m_LocalInfoObjects = localInfoObjects.ToArray();
        m_iLocalInfoLength = m_LocalInfoObjects.Length;


        // this is done so you still have mouse and cmaera control over this item
        //  if (IsMouseOverGUI())  HouseBuilderRoot.cs

        Core.Environment.OnEnvironmentLoadingComplete += OnEnvironmentLoadingComplete;
    }

    private void AssignGradientMenu(Transform root)
    {
        m_GradientImage = root.SearchComponent<Image>("GradientImage");
        m_GradientMaterial = new UnityEngine.Material(m_GradientImage.material);
        m_GradientTexture2D = new Texture2D(100, 1);
        m_GradientSprite = Sprite.Create(m_GradientTexture2D, new Rect(0.0f, 0.0f, m_GradientTexture2D.width, m_GradientTexture2D.height), new Vector2(m_GradientTexture2D.width / 2, m_GradientTexture2D.height / 2));

        m_SliderList = root.GetComponentsInChildren<Slider>(true).ToList();
        foreach(var item in m_SliderList)
        {
            item.onValueChanged.AddListener((value) => UpdateGradientImage());
        }
        UpdateGradientImage();
    }

    private void UpdateGradientImage()
    {
        /////////progress *= 0.85f;
        m_Gradient = new Gradient();
        GradientColorKey[] gck = new[]
        {
                new GradientColorKey(Color.red, 1f),
                new GradientColorKey(Color.yellow, m_SliderList[2].value),
                new GradientColorKey(Color.green, m_SliderList[1].value),
                new GradientColorKey(Color.cyan, m_SliderList[0].value),
                new GradientColorKey(Color.blue, 0f)
            };


        GradientAlphaKey[] gak = new[]
        {
                new GradientAlphaKey(0, 0f),
                new GradientAlphaKey(1, 1f)
            };

        m_Gradient.SetKeys(gck, gak);


        for (int i = 0; i < 100; i++)
        {
            float percentage = i;
            percentage /= 100;
            var colour = GetGradient(percentage);
            m_GradientTexture2D.SetPixel(i, 0, colour);
        }
        m_GradientTexture2D.Apply();
        m_GradientImage.material = m_GradientMaterial;

        m_GradientSprite = Sprite.Create(m_GradientTexture2D, new Rect(0.0f, 0.0f, m_GradientTexture2D.width, m_GradientTexture2D.height), new Vector2(m_GradientTexture2D.width / 2, m_GradientTexture2D.height / 2));
        m_GradientImage.sprite = m_GradientSprite;
        m_GradientImage.color = Color.white;
    }

    private void AssignTextureMapsMenu(Transform root)
    {

        var speakerContent = root.SearchComponent<Transform>("SpeakerSelectionView");
        m_SpeakerSelectionViewContent = speakerContent.SearchComponent<Transform>("Content").gameObject;

        var splContent = root.SearchComponent<Transform>("CombinationSelectionView");
        m_CombinationDataContent = splContent.SearchComponent<Transform>("Content").gameObject;

        var SeperateHolder = root.SearchComponent<Transform>("SeperateHolder");
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("occlusionFactor"), TextureMapType.occlusionFactor);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("propagationDelay"), TextureMapType.propagationDelay);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("distanceAttenuation"), TextureMapType.distanceAttenuation);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("directivityFactor"), TextureMapType.directivityFactor);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("airAbsorptionLow"), TextureMapType.airAbsorptionLow);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("airAbsorptionMid"), TextureMapType.airAbsorptionMid);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("airAbsorptionHigh"), TextureMapType.airAbsorptionHigh);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("transmissionFactorLow"), TextureMapType.transmissionFactorLow);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("transmissionFactorMid"), TextureMapType.transmissionFactorMid);
        AddAudioTextureOptionData(SeperateHolder.SearchComponent<Transform>("transmissionFactorHigh"), TextureMapType.transmissionFactorHigh);

        SeperateHolder.SearchComponent<Button>("DisplayMap").onClick.AddListener(DisplayTextureMap);



        var dropDownHolder = root.SearchComponent<Transform>("DropDownHolder");
        dropDownHolder.SearchComponent<Button>("DisplayMap").onClick.AddListener(DisplayCustomTextureMap);

        // create numbers
        m_NumberOfCustoms = dropDownHolder.SearchComponent<Dropdown>("NumberOfOptions");
        m_NumberOfCustoms.ClearOptions();
        string[] numbers = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        m_NumberOfCustoms.AddOptions(numbers.ToList());
        m_NumberOfCustoms.onValueChanged.AddListener((value) =>
        {
            m_SplSelectionPool.DeSpawnAll();
            for(int i = 0; i < value+1; i++)
            {
                m_SplSelectionPool.SpawnObject();
            }
        });


        m_TextureMapPerMeterSlider = root.SearchComponent<Slider>("Texture Map Per Meter Slider");
        m_TextureMapPerMeterSlider.minValue = 1f;
        m_TextureMapPerMeterSlider.maxValue = 25f;
        m_TextureMapPerMeterSlider.wholeNumbers = true;
        m_TextureMapPerMeterSlider.onValueChanged.AddListener(UpdateTextureMapPerMetre);

        m_TextureMapPerMeterInputField = root.SearchComponent<InputField>("Texture Map Per Meter Input Field");
        m_TextureMapPerMeterInputField.onEndEdit.AddListener(UpdateTextureMapPerMetre);

        m_TextureMapMaxHeightSlider = root.SearchComponent<Slider>("Texture Map Max Height Slider");
        m_TextureMapMaxHeightSlider.minValue = 0.1f;
        m_TextureMapMaxHeightSlider.maxValue = 25f;
        m_TextureMapMaxHeightSlider.wholeNumbers = false;
        m_TextureMapMaxHeightSlider.onValueChanged.AddListener(UpdateTextureMapMaxHeight);

        m_TextureMapMaxHeightInputField = root.SearchComponent<InputField>("Texture Map Max Height Input Field");
        m_TextureMapMaxHeightInputField.onEndEdit.AddListener(UpdateTextureMapMaxHeight);

        root.SearchComponent<Button>("HideMap").onClick.AddListener(HideTextureMap);
        root.SearchComponent<Button>("CreateMap").onClick.AddListener(CreateTextureMaps);
    }

    private void OnEnvironmentLoadingComplete()
    {
        Core.Environment.OnEnvironmentLoadingComplete -= OnEnvironmentLoadingComplete;
        var prefab = CachedObjects.Instance.GetPrefab(SPEAKER_SELECTION_PREFAB);
        if (prefab == null)
        {
            Debug.LogError($"SPEAKER_SELECTION_PREFAB not found : {SPEAKER_SELECTION_PREFAB}");
        }
        prefab.gameObject.transform.localScale = Vector3.one;
        m_SpeakerSelectionPool = new PoolManager<SpeakerSelectionData>(prefab, this.gameObject, onSpeakerCreate, onSpeakerSpawn, onSpeakerDespawn);

        var prefabSpl = CachedObjects.Instance.GetPrefab(SPL_SELECTION_PREFAB);
        m_SplSelectionPool = new PoolManager<SplSelectionData>(prefabSpl, this.gameObject, onSplCreate, onSplSpawn, onSplDespawn);
        m_SplSelectionPool.SpawnObject();

        UpdateStatus();
        UpdateQualitySettings();

        // Close the menu.
        OpenCloseMenu();
    }

    private void OpenTextureMapMenu()
    {
        m_TextureMapMenu.SetActive(true);
        m_SpeakerSelectionPool.DeSpawnAll();
        var data = CollectAudioGroups();
        foreach (var item in data.GetKeys())
        {
            var obj = m_SpeakerSelectionPool.SpawnObject();
            obj.m_Text.text = item.CatalogueEntryRef.ShortName;
        }

        m_TextureMapMaxHeightSlider.maxValue = HouseBuilderRoot.Instance.gameObject.GetEncapsulatingBounds().max.y;
        m_TextureMapMaxHeightSlider.value = m_TextureMapMaxHeightSlider.maxValue;
    }

    public SimpleDictionaryList<Interaction, SteamAudioSource>  CollectAudioGroups()
    {
        SimpleDictionaryList<Interaction, SteamAudioSource> data = new SimpleDictionaryList<Interaction, SteamAudioSource>();
        foreach (var item in HouseBuilderRoot.Instance.InputContents.ModeContainerRef.GetList)
        {
            if(item.InteractionRef.PlacementMetaData.m_OutlineOverrideRenderer != null)
            {
                var audio = item.InteractionRef.GetComponentsInChildren<SteamAudioSource>();
                if(audio.Length != 0)
                {
                    data.AddToList(item.InteractionRef, audio.ToList());
                }
            }
        }

        
        var all = FindObjectsOfType<SteamAudioSource>();
        for (int i = 0; i < all.Length; i++)
        {
            var allUsedSteamAudioSources = data.GetValuesAll();
            if (allUsedSteamAudioSources.Contains(all[i]) == false)
            {
                var interaction = all[i].GetComponentInParent<Interaction>();
                data.AddToList(interaction, all[i]);
            }
        }

        SimpleDictionaryList<Interaction, SteamAudioSource> returnData = new SimpleDictionaryList<Interaction, SteamAudioSource>();
        for (int i = 0; i < all.Length; i++)
        {
            var key = data.GetKey(all[i]);
            if(returnData.Count(key) == 0)
            {
                returnData.AddToList(key, data.GetList(key));
            }
        }

        return returnData;
    }

    private SpeakerSelectionData onSpeakerCreate(GameObject obj)
    {
        SpeakerSelectionData item = new SpeakerSelectionData();
        item.m_Root = obj;
        item.SelectedToggle = item.m_Root.GetComponentInChildren<Toggle>();
        item.m_Text = item.m_Root.GetComponentInChildren<Text>();
        return item;
    }


    private SpeakerSelectionData onSpeakerSpawn(SpeakerSelectionData arg)
    {
        arg.m_Root.transform.SetParent(m_SpeakerSelectionViewContent.transform);
        arg.SelectedToggle.isOn = true;
        arg.m_Root.SetActive(true);
        return arg;
    }

    private SpeakerSelectionData onSpeakerDespawn(SpeakerSelectionData arg)
    {
        arg.m_Root.transform.SetParent(this.transform);
        arg.m_Root.SetActive(false);
        return arg;
    }


    private SplSelectionData onSplCreate(GameObject obj)
    {
        SplSelectionData item = new SplSelectionData();
        item.m_Root = obj;
        item.m_Normilise = item.m_Root.GetComponentInChildren<Toggle>();
        item.Names = item.m_Root.transform.SearchComponent<Dropdown>("Name");
        item.Names.ClearOptions();
        List<string> names = Enum.GetNames(typeof(TextureMapType)).ToList();
        item.Names.AddOptions(names);

        item.Arithmetic = item.m_Root.transform.SearchComponent<Dropdown>("Arithmetic");
        item.Arithmetic.ClearOptions();
        string[] symbols = {"*", "+", "-"};
        item.Arithmetic.AddOptions(symbols.ToList());
        return item;
    }


    private SplSelectionData onSplSpawn(SplSelectionData arg)
    {
        arg.m_Root.transform.SetParent(m_CombinationDataContent.transform);
        arg.m_Root.SetActive(true);
        return arg;
    }

    private SplSelectionData onSplDespawn(SplSelectionData arg)
    {
        arg.m_Root.transform.SetParent(this.transform);
        arg.m_Root.SetActive(false);
        return arg;
    }


    private void AddAudioTextureOptionData(Transform obj, TextureMapType textureMapType) => AddAudioTextureOptionData(obj.gameObject, textureMapType);

    private void AddAudioTextureOptionData(GameObject obj, TextureMapType textureMapType)
    {
        AudioTextureOptionData newData = new AudioTextureOptionData();
        newData.SelectedToggle = obj.transform.SearchComponent<Toggle>("Toggle");
        newData.InvertedToggle = obj.transform.SearchComponent<Toggle>("InvertToggle");
        newData.m_FactorAmount = obj.GetComponentInChildren<InputField>(true);
        m_AudioTextureOptionData.Add(textureMapType, newData);
        newData.SelectedToggle.isOn = false;
        newData.InvertedToggle.isOn = false;
        if(textureMapType == TextureMapType.occlusionFactor)
        {
            newData.SelectedToggle.isOn = true;
        }
    }

    private void HideTextureMap()
    {
        var interactions = FindObjectsOfType<InteractionGameObjectMove>().ToList();
        var heatMapRootGameObject = interactions.FindLast(e => e.SteamSoundRef.MetaDataRef.m_SplGameObject != null);
        interactions.RemoveAll(e => e.SteamSoundRef.MetaDataRef == null);
        if (heatMapRootGameObject != null && heatMapRootGameObject.SteamSoundRef.MetaDataRef.m_SplGameObject != null)
        {
            heatMapRootGameObject.SteamSoundRef.MetaDataRef.m_SplGameObject.SetActive(false);
        }

        m_TextureDisplayQuadRenderer.SetActive(false);
    }

    private void DisplayTextureMap()
    {
        m_TextureDisplayQuadRenderer.SetActive(true);

        SplTextureMap.SplTextureData textureData = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
        if (textureData != null)
        {
            var probPosition = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position;


            m_TextureDisplayQuadRenderer.transform.position = new Vector3(probPosition.x, 0.1f, probPosition.z);
            m_TextureDisplayQuadRenderer.transform.SetScale(textureData.TextureSize.x, textureData.TextureSize.z, textureData.TextureSize.z);

            int amountX = textureData.TextureSizeX * textureData.AmountPerMetre;
            int amountZ = textureData.TextureSizeZ * textureData.AmountPerMetre;
            var newImage = new Texture2D(amountX, amountZ, TextureFormat.ARGB32, false);

            var listOfChoisenSpeakers = m_SpeakerSelectionPool.GetPublicList();
            var allTextureData = Core.Audio.AudioSimulationSetupRef.GetTextureDataAll();


            // collect all data
            int soundsToUseCount = 0;
            float[,] combinedAllData = new float[amountX, amountZ];
            List<float[,]> seperateItems = new List<float[,]>();

            var allGroups = CollectAudioGroups();
            for (int i = 0; i < allGroups.GetKeys().Count; i++)
            {
                if (listOfChoisenSpeakers[i].SelectedToggle.isOn == true)
                {
                    var keyValues = allGroups.GetValues(i);
                    for (int groupCount = 0; groupCount < keyValues.Count; groupCount++)
                    {
                        seperateItems.Add(new float[amountX, amountZ]);
                    }
                }
            }




            bool isAnythingSelected = false;
            for (int i = 0; i < allGroups.GetKeys().Count; i++)
            {
                if (listOfChoisenSpeakers[i].SelectedToggle.isOn == true)
                {
                    isAnythingSelected = true;
                    break;
                }
            }

            if (isAnythingSelected == false)
            {
                GenericPromptMenu.Instance.ShowOkayMenu("No items have been selected");
                return;
            }

            int seperateItemsIndex = 0;
            for (int i = 0; i < allGroups.GetKeys().Count; i++)
            {
                if (listOfChoisenSpeakers[i].SelectedToggle.isOn == true)
                {
                    soundsToUseCount++;

                    int allTextureDataIndex = 0;
                    for (int j = 0; j < i; j++)
                    {
                        allTextureDataIndex += allGroups.GetValues(j).Count;
                    }

                    for (int currentGroup = 0; currentGroup < allGroups.GetValues(i).Count; currentGroup++)
                    {
                        var rawData = CombineData(allTextureData[allTextureDataIndex + currentGroup], false);
                        if (rawData.Length != 0)
                        {
                            for (int x = 0; x < amountX; x++)
                            {
                                for (int z = 0; z < amountZ; z++)
                                {
                                    combinedAllData[x, z] += rawData[x, z];
                                    seperateItems[seperateItemsIndex][x, z] = rawData[x, z];
                                }
                            }
                        }
                        seperateItemsIndex++;
                    }

                }
            }

            // medium
            for (int x = 0; x < amountX; x++)
            {
                for (int z = 0; z < amountZ; z++)
                {
                    var dataPoint = combinedAllData[x, z];
                    dataPoint /= soundsToUseCount; // divide by the sounds systems being used
                    newImage.SetPixel(x, z, GetGradient(dataPoint));
                }
            }

            UseMax(amountX, amountZ, newImage, seperateItems);
            ApplyTexture(newImage);
        }
    }


    private void DisplayCustomTextureMap()
    {

        m_TextureDisplayQuadRenderer.SetActive(true);

        SplTextureMap.SplTextureData textureData = Core.Audio.AudioSimulationSetupRef.GetTextureDataFirst();
        if (textureData != null)
        {
            var probPosition = Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position;


            m_TextureDisplayQuadRenderer.transform.position = new Vector3(probPosition.x, 0.1f, probPosition.z);
            m_TextureDisplayQuadRenderer.transform.SetScale(textureData.TextureSize.x, textureData.TextureSize.z, textureData.TextureSize.z);

            int amountX = textureData.TextureSizeX * textureData.AmountPerMetre;
            int amountZ = textureData.TextureSizeZ * textureData.AmountPerMetre;
            var newImage = new Texture2D(amountX, amountZ, TextureFormat.ARGB32, false);

            var listOfChoisenSpeakers = m_SpeakerSelectionPool.GetPublicList();
            var allTextureData = Core.Audio.AudioSimulationSetupRef.GetTextureDataAll();


            // collect all data
            float[,] combinedAllData = new float[amountX, amountZ];
            Dictionary<TextureMapType, float[,]> seperateItems = new Dictionary<TextureMapType, float[,]>();
            foreach (TextureMapType item in Enum.GetValues(typeof(TextureMapType)))
            {
                seperateItems.Add(item, new float[amountX, amountZ]);
            }


            var allGroups = CollectAudioGroups();
            for (int index = 0; index < allGroups.GetKeys().Count; index++)
            {
                if (listOfChoisenSpeakers[index].SelectedToggle.isOn == true)
                {


                    int allTextureDataIndex = 0;
                    for (int j = 0; j < index; j++)
                    {
                        allTextureDataIndex += allGroups.GetValues(j).Count;
                    }

                    for (int currentGroup = 0; currentGroup < allGroups.GetValues(index).Count; currentGroup++)
                    {
                        int correctIndex = allTextureDataIndex + currentGroup;

                        var publicList = m_SplSelectionPool.GetPublicList();
                        foreach(var item in publicList)
                        {
                            TextureMapType textureMapType = (TextureMapType)item.Names.value;
                            TextureMax(textureMapType, amountX, amountZ, correctIndex, allTextureData, seperateItems);
                        }
                    }
                }
            }

            var publicListOther = m_SplSelectionPool.GetPublicList();

            TextureMapType textureMapType1 = (TextureMapType)publicListOther[0].Names.value;
            combinedAllData = seperateItems[textureMapType1];
            for(int i = 1; i< publicListOther.Count; i++)
            {
                textureMapType1 = (TextureMapType)publicListOther[i].Names.value;
                ArithmaticEnum arithmaticEnum = (ArithmaticEnum)publicListOther[i].Arithmetic.value;
                bool normalise = publicListOther[i].m_Normilise.isOn;

                TextureArithmatic(normalise, arithmaticEnum, textureMapType1, amountX, amountZ, seperateItems, combinedAllData);               
            }

            CombinedItems(amountX, amountZ, newImage, combinedAllData);
            ApplyTexture(newImage);
        }
    }


    private void TextureArithmatic(bool normalise, ArithmaticEnum arithmaticEnum, TextureMapType textureMapType, int amountX, int amountZ, Dictionary<TextureMapType, float[,]> seperateItems, float[,] combinedAllData)
    {
        float[,] rawData = seperateItems[textureMapType];

        if (rawData.Length != 0)
        {           
            float maxValue = 1f;
            if (normalise == true)
            {
                maxValue = 0f;
                for (int x = 0; x < amountX; x++)
                {
                    for (int z = 0; z < amountZ; z++)
                    {
                        float value = rawData[x, z];
                        maxValue = Mathf.Max(maxValue, value);
                    }
                }               
            }


            for (int x = 0; x < amountX; x++)
            {
                for (int z = 0; z < amountZ; z++)
                {
                    float value = rawData[x, z];
                    if (value >= 0)
                    {
                        switch (arithmaticEnum)
                        {
                            case ArithmaticEnum.Multiply:
                                combinedAllData[x, z] *= (value / maxValue);
                                break;
                            case ArithmaticEnum.Add:
                                combinedAllData[x, z] += (value / maxValue);
                                break;
                            case ArithmaticEnum.Subtract:
                                combinedAllData[x, z] -= (value / maxValue);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

        }
    }
    

    private void TextureMax(TextureMapType textureMapType, int amountX, int amountZ, int index, List<SplTextureMap.SplTextureData> allTextureData,  Dictionary<TextureMapType, float[,]> seperateItems)
    {
        float[,] rawData = allTextureData[index].m_RawTextureData[textureMapType];
        if (rawData != null && rawData.Length != 0)
        {
            for (int x = 0; x < amountX; x++)
            {
                for (int z = 0; z < amountZ; z++)
                {
                    seperateItems[textureMapType][x, z] = Mathf.Max(seperateItems[textureMapType][x, z], rawData[x, z]);
                }
            }
        }
    }




    private void UseMax(int amountX, int amountZ, Texture2D newImage, List<float[,]> seperateItems)
    {
        // use the max amount there 
        for (int x = 0; x < amountX; x++)
        {
            for (int z = 0; z < amountZ; z++)
            {
                float value = 0;

                foreach (var item in seperateItems)
                {
                    value = Mathf.Max(value, item[x, z]);
                }
                newImage.SetPixel(x, z, GetGradient(value));
            }
        }
    }

    private void CombinedItems(int amountX, int amountZ, Texture2D newImage, float[,] combinedItems)
    {
        // use the max amount there 
        for (int x = 0; x < amountX; x++)
        {
            for (int z = 0; z < amountZ; z++)
            {
                float value = combinedItems[x, z];
                newImage.SetPixel(x, z, GetGradient(value));
            }
        }
    }

    private void ApplyTexture(Texture2D newImage)
    {
        newImage.Apply();

        var interactions = FindObjectsOfType<InteractionGameObjectMove>().ToList();
        interactions.RemoveAll(e => e.SteamSoundRef.MetaDataRef == null);
        var heatMapRootGameObject = interactions.FindLast(e => e.SteamSoundRef.MetaDataRef.m_SplGameObject != null);
        if (heatMapRootGameObject != null && heatMapRootGameObject.SteamSoundRef.MetaDataRef.m_SplGameObject != null)
        {
            heatMapRootGameObject.SteamSoundRef.MetaDataRef.m_SplMeshRenderer.material.mainTexture = newImage;
            heatMapRootGameObject.SteamSoundRef.MetaDataRef.m_SplGameObject.SetActive(true);

            m_TextureDisplayQuadRenderer.material.mainTexture = newImage;
            m_TextureDisplayQuadRenderer.SetActive(false);
        }
        else
        {
            m_TextureDisplayQuadRenderer.material.mainTexture = newImage;
            m_TextureDisplayQuadRenderer.SetActive(true);
        }
    }

    private float[,] CombineData(SplTextureMap.SplTextureData textureData, bool forceAll)
    {
        int amountX = textureData.TextureSizeX * textureData.AmountPerMetre;
        int amountZ = textureData.TextureSizeZ * textureData.AmountPerMetre;

        float[,] combinedData = new float[amountX, amountZ];
        int totalAmount = 0;
        foreach (var option in m_AudioTextureOptionData)
        {
            if(forceAll || option.Value.SelectedToggle.isOn == true)
            {
                int factorAmount = int.Parse(option.Value.m_FactorAmount.text);
                factorAmount = Mathf.Max(0, factorAmount);
                if (factorAmount > 0)
                {
                    if (textureData.m_RawTextureData.ContainsKey(option.Key) == true)
                    {
                        var rawItem = textureData.m_RawTextureData[option.Key];
                        totalAmount += factorAmount;

                        for (int amountIndex = 0; amountIndex < factorAmount; amountIndex++)
                        {
                            for (int x = 0; x < amountX; x++)
                            {
                                for (int z = 0; z < amountZ; z++)
                                {
                                    var dataPoint = rawItem[x, z];
                                    if (option.Value.InvertedToggle.isOn == false)  // invert the amount to see the differnce
                                    {
                                        if (dataPoint > -10)
                                        {
                                            combinedData[x, z] += dataPoint;
                                        }
                                    }
                                    else
                                    {
                                        if (dataPoint > -10)
                                        {
                                            combinedData[x, z] += (1f - dataPoint);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"m_AudioTextureOptionData does not have key: {option.Key}");
                    }
                }
            }
        }

        for (int x = 0; x < amountX; x++)
        {
            for (int z = 0; z < amountZ; z++)
            {
                combinedData[x, z] /= totalAmount;
            }
        }
        return combinedData;
    }

    public Color GetGradient(float percentage) =>  m_Gradient.Evaluate(percentage);






    private void HideSoundSimButton()
    {
        m_MenuButton.gameObject.SetActive(false);
    }


    private void ShowSoundSimButton()
    {
        m_MenuButton.gameObject.SetActive(true);
    }


    public void OpenCloseMenu()
    {
        m_bMenuOpen = !m_bMenuOpen;
        this.gameObject.SetActive(m_bMenuOpen);

        if (true == m_bMenuOpen)
        {
            UpdateStatus();
        }
    }


    /// <summary>
    /// Check and update the status of the audio simulation.
    /// </summary>
    private void UpdateStatus()
    {
        m_StatusReady.SetActive(false);
        m_StatusDownload.SetActive(false);
        m_StatusBake.SetActive(false);
        m_StatusRebake.SetActive(false);
        m_StatusUpload.SetActive(false);

        m_SoundSimLoadSaveRef.GetLocalJsonVersion((localData) =>
        {
            m_SoundSimLoadSaveRef.GetNetworkJsonVersion((networkData) =>
            {
                // If there is no data, we need to bake audio sim data.
                if ((null == localData) && (null == networkData))
                {
                    m_StatusBake.SetActive(true);
                }
                // If there is no local data, but there is network data, we need to download.
                else if ((null == localData) && (null != networkData))
                {
                    m_StatusDownload.SetActive(true);
                    StatusUpdateServerInfo(networkData);
                }
                // If there is local data, but no network data, we need to upload.
                else if ((null != localData) && (null == networkData))
                {
                    m_StatusUpload.SetActive(true);
                    StatusUpdateLocalInfo(localData);
                }
                else
                {
                    // If there's data on both we need to figure out which is newest.
                    DateTime localTime = DateTime.FromBinary(localData.DateTimeBinary);
                    DateTime networkTime = DateTime.FromBinary(networkData.DateTimeBinary);

                    // If the local is newer, we need to upload.
                    if (localTime > networkTime)
                    {
                        m_StatusUpload.SetActive(true);
                        StatusUpdateLocalInfo(localData);
                    }
                    // If the network one is newer, we need to download.
                    else if (networkTime > localTime)
                    {
                        m_StatusDownload.SetActive(true);
                        StatusUpdateServerInfo(networkData);
                    }
                    // else, we don't care.
                }

                // Now we need to check for changes, as we might not want to download if we need to bake:

                // If the current hash doesn't match the local data, tell them to re-bake instead.
                if ((null != localData) && (localData.CurrentHashCode != SteamAudioManager.GetSingleton().CurrentHashCode))
                {
                    m_StatusDownload.SetActive(false);
                    m_StatusUpload.SetActive(false);
                    m_StatusRebake.SetActive(true);
                }
                // If there's network data, but no local, and the network data is out of date, tell them to bake instead.
                else if ((null != networkData) && (null == localData) && (networkData.CurrentHashCode != SteamAudioManager.GetSingleton().CurrentHashCode))
                {
                    m_StatusDownload.SetActive(false);
                    m_StatusBake.SetActive(true);
                }
                // Otherwise, all is good.
                else if (null != localData)
                {
                    m_StatusReady.SetActive(true);
                    StatusUpdateLocalInfo(localData);
                }
            });
        });
    }


    private void StatusUpdateLocalInfo(SoundBundleVersion data)
    {
        string sText = $"(Date: {DateTime.FromBinary(data.DateTimeBinary)}, Probe Spacing: {data.HorizontalSpacing} metres, Probe Height: {data.HeightAboveFloor} metres, Size: {data.TotalFileSizeMB} mb)";

        for (int i = 0; i < m_iLocalInfoLength; i++)
        {
            m_LocalInfoObjects[i].text = sText;
        }
    }


    private void StatusUpdateServerInfo(SoundBundleVersion data)
    {
        m_ServerInfoText.text = $"(Date: {DateTime.FromBinary(data.DateTimeBinary)}, Probe Spacing: {data.HorizontalSpacing} metres, Probe Height: {data.HeightAboveFloor} metres, Size: {data.TotalFileSizeMB} mb)";
    }


    public void UpdateProbeSpacing(string sSpacing) => UpdateProbeSpacing(float.Parse(sSpacing));
    public void UpdateProbeHeight(string sHeight) => UpdateProbeHeight(float.Parse(sHeight));
    public void UpdateTextureMapPerMetre(string perMetre) => UpdateTextureMapPerMetre(int.Parse(perMetre));
    public void UpdateTextureMapMaxHeight(string maxHeight) => UpdateTextureMapMaxHeight(float.Parse(maxHeight));

    public void UpdateProbeSpacing(float fSpacing)
    {
        m_fProbeSpacing = fSpacing;
        UpdateQualitySettings();
    }

    public void UpdateProbeHeight(float fHeight)
    {
        m_fProbeHeight = fHeight;
        UpdateQualitySettings();
    }

    public void UpdateTextureMapPerMetre(float perMetre)
    {
        m_TextureMapPerMetre = (int)perMetre;
        UpdateQualitySettings();
    }

    public void UpdateTextureMapMaxHeight(float maxHeight)
    {
        m_TextureMapMaxHeight = (float)maxHeight;
        UpdateQualitySettings();
    }

    /// <summary>
    /// Check and update the status of the quality settings.
    /// </summary>
    private void UpdateQualitySettings()
    {
        // Update the settings on the Steam Audio probe box.
        AudioSimulationSetup.s_Singleton.SteamAudioProbeBoxRef.horizontalSpacing = m_fProbeSpacing;
        AudioSimulationSetup.s_Singleton.SteamAudioProbeBoxRef.heightAboveFloor = m_fProbeHeight;

        // Update the settings on the menu.
        m_ProbeSpacingSlider.value = m_fProbeSpacing;
        m_ProbeSpacingInputField.text = m_fProbeSpacing.ToString("F2");

        m_ProbeHeightSlider.value = m_fProbeHeight;
        m_ProbeHeightInputField.text = m_fProbeHeight.ToString("F2");

        m_TextureMapPerMetre = Mathf.Min(m_TextureMapPerMetre, (int)m_TextureMapPerMeterSlider.maxValue);
        m_TextureMapPerMeterSlider.value = m_TextureMapPerMetre;
        m_TextureMapPerMeterInputField.text = m_TextureMapPerMetre.ToString();

        m_TextureMapMaxHeight = Mathf.Min(m_TextureMapMaxHeight, (int)m_TextureMapMaxHeightSlider.maxValue);
        m_TextureMapMaxHeightSlider.value = m_TextureMapMaxHeight;
        m_TextureMapMaxHeightInputField.text = m_TextureMapMaxHeight.ToString();
        // Clear anything tied to the old quality settings.
        HideProbes();
        m_NumberOfProbesText.text = "-";
        m_EstimatedBakeTimeText.text = "-";
        m_CalculateTimeButton.SetActive(true);
    }


    /// <summary>
    /// Updates the estimated time for sound sim baking and displays it in the menu.
    /// </summary>
    private void UpdateTimeEstimate()
    {
        m_CalculateTimeButton.SetActive(false);

        // Work out sim time.
        Core.Audio.AudioSimulationSetupRef.CreateVisualProbes((iProbeCount) => 
        {
            m_NumberOfProbesText.text = iProbeCount.ToString();

            int iTime = 10; // 10 seconds for initial setup.
            // A couple of milliseconds per probe, times the amount of sources (this will need changing to selected sources) plus the reverb.
            iTime += (int)(iProbeCount * m_fProbeSimTime) * (FindObjectsOfType<SteamAudioSource>().Length + 1); // The + 1 is for the reverb.

            TimeSpan estimatedTime = new TimeSpan(0, 0, iTime);

            m_EstimatedBakeTimeText.text = estimatedTime.ToString();

        }, false); // The false stops it rendering the probes, which speeds this whole thing up a lot.
    }


    private void UploadData()
    {
        m_SoundSimLoadSaveRef.ConfirmUploadGeneratedSound();
        OpenCloseMenu();
    }


    private void DownloadData()
    {
        m_SoundSimLoadSaveRef.ConfirmUpdateGeneratedSound(null);
        OpenCloseMenu();
    }


  

    /// <summary>
    /// Show the audio probes in the scene.
    /// </summary>
    private void ShowProbes()
    {
        m_bProbesVisible = true;
        Core.Audio.AudioSimulationSetupRef.CreateVisualProbes(null);  //////Core.Audio.AudioSimulationSetupRef.CreateVisualProbes(null);
        m_ShowProbesButton.SetActive(false);
        m_HideProbesButton.SetActive(true);
        OpenCloseMenu();
    }

    private void CreateTextureMaps()
    {
        GenericPromptMenu.Instance.ShowYesNoMenu("Are you sure you want to re-create", () =>
        {
            Stopwatch timer = Stopwatch.RealTime();
            AssetSyncMenu.Instance.SetVisible(true);
            AssetSyncMenu.Instance.SetProgressBar();

            AssetSyncMenu.Instance.SetHeaderText("Creating Maps");
            AssetSyncMenu.Instance.SetProgressBarText("Gathering Data");
            AssetSyncMenu.Instance.ShowCancel(() =>
            {
                Core.Audio.AudioSimulationSetupRef.StopCreateTextureMaps();
                SetVisibleFalseMaps();
            });


            var sources = FindObjectsOfType<SteamAudioSource>().ToList();
            if (sources.Count == 0)
            {
                GenericPromptMenu.Instance.ShowOkayMenu("There are no SteamAudioSource's in the build.");
            }
            else
            {
                Core.Audio.AudioSimulationSetupRef.CreateTextureMaps(m_TextureMapPerMetre, m_TextureMapMaxHeight, SetProgressBarTextMaps, SetProgressMaps, () =>
                {
                    SetVisibleFalseMaps();
                    timer.Stop();
                    timer.ToReadable();
                    GenericPromptMenu.Instance.ShowOkayMenu($"Maps are created took :{timer.ToReadable()}", null);
                    Core.Audio.LoadSaveGeneratedSoundRef.WriteJsonFileLocally();
                    DisplayTextureMap();
                });
            }
        });

    }

    private void SetProgressBarTextMaps(string info)
    {
        AssetSyncMenu.Instance.SetProgressBarText($"ETA: {info}");
    }

    private void SetProgressMaps(float amount)
    {
        AssetSyncMenu.Instance.SetProgress(amount);
    }

    private void SetVisibleFalseMaps()
    {
        AssetSyncMenu.Instance.SetActive(false);
    }


    /// <summary>
    /// Hide the audio probes in the scene.
    /// </summary>
    private void HideProbes()
    {
        if (true == m_bProbesVisible)
        {
            m_bProbesVisible = false;
            Core.Audio.AudioSimulationSetupRef.DeleteVisualProbes();
            m_ShowProbesButton.SetActive(true);
            m_HideProbesButton.SetActive(false);
        }
    }


    /// <summary>
    /// Trigger baking the audio data for the current scene.
    /// </summary>
    private void GenerateSound()
    {
        GenericPromptMenu.Instance.ShowYesNoMenu("Are you sure you want to re-Bake", () =>
        {
            AssetSyncMenu.Instance.SetVisible(true);
            AssetSyncMenu.Instance.SetProgressBar();
            AssetSyncMenu.Instance.SetHeaderText("Init Baking");
            Core.Mono.WaitForFrame(() => // without this, the AssetSyncMenu is not shown for a few seconds
            {
                // Close the menu.
                OpenCloseMenu();

                Debug.LogError("TODO: Need a box to come up, with all the sound sources and select wheich one to generate for");


                Bounds contentBounds = HouseBuilderRoot.Instance.InputContents.ModeContainerRef.Holder.GetEncapsulatingBounds();
                Bounds wallbounds = HouseBuilderRoot.Instance.GenerateWallsRef.Holder.GetEncapsulatingBounds();

                contentBounds.Encapsulate(wallbounds);

                Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.position = contentBounds.center;
                Core.Audio.AudioSimulationSetupRef.SteamAudioProbeBoxRef.transform.SetScale(contentBounds.size.x + 1f, contentBounds.size.y, contentBounds.size.z + 1f);
                Core.Audio.AudioSimulationSetupRef.m_ProbBoxSize = contentBounds.size;
                Core.Audio.AudioSimulationSetupRef.m_ProbBoxCenter = contentBounds.center;
                Core.Audio.AudioSimulationSetupRef.AssignAudioName();

                Core.Audio.AudioSimulationSetupRef.DeleteVisualProbes();

                // Trigger the audio baking.
                Core.Audio.AudioSimulationSetupRef.DeleteVisualProbes();
                Core.Audio.AudioSimulationSetupRef.CreateBakeAudio(AssetSyncMenu.Instance.SetHeaderText, AssetSyncMenu.Instance.SetProgressBarText, AssetSyncMenu.Instance.SetProgress, () =>
                {
                    AssetSyncMenu.Instance.SetVisible(false);
                });
            });
        });
    }
}
