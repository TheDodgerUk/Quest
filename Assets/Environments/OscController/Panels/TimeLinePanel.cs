using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static OscController.DataContainer;
using static OscController.DataContainer.OscAllUsedFixtures;
using static OscController.OscControllerRoot;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{

    public class TimeLinePanel : MonoBehaviour
    {
        private const int WAVE_SIZE_SECONDS = 10;
        public const int OFFSET_FOR_SLIDER = 18;  // handle is not at the end
        public int OFFSET_FOR_SLIDER_WAVE => ((PIXELS_PER_SECOND / 2) * WAVE_SIZE_SECONDS) + OFFSET_FOR_SLIDER;
        public const int OFFSET_FOR_SLIDER_NUMBER = 10;
        public const int OFFSET_FOR_SLIDER_GRATHICS = 18;
        public const int OFFSET_FOR_SLIDER_DELTA_SIZE = 20;

        


        private const string HEADER_PREFAB = "TimeLineHeaderPrefab";
        private const string ITEM_PREFAB = "TimeLineItemPrefab";

        private const string TIME_SPACE_WAVE_PREFAB = "TimeLineWavePrefab";
        private const string TIME_SPACE_GRATHIC_PREFAB = "TimeLineGrathic10SecondsPrefab";
        private const string TIME_NUMBER_PREFAB = "TimeLineNumberPrefab";
        private const string TIME_HIGHTLIGHT_PREFAB = "TimeLineHightlightPrefab";

 
        [SerializeField]
        private const int PIXELS_PER_SECOND_START = 100;
        private int PIXELS_PER_SECOND = 100;
        private const int MIN_PIXELS_PER_SECOND = 20;
        private const int MAX_PIXELS_PER_SECOND = 300;

        private const int MAX_MINUTES = 10;
        public const int MAX_SECONDS = 60 * MAX_MINUTES;
        private int MAX_PIXELS => (MAX_SECONDS * PIXELS_PER_SECOND);

        private const int GROUP_HEIGHT = 55;
       
        

        private const int PIXELS_PER_ZOOM = 100;
        private const int START_GROUP_HEIGHTS = 400;

        private readonly Color DISABLED_COLOR = new Color(0f, 0f, 0f, 0.5f);
        private readonly Color ALPHA_COLOR = new Color(1f, 1f, 1f, 0f);

        private Button m_AddGroup;
        private Button m_AddBlankGroup;

        private Button m_SelectAll;
        private Button m_SelectNone;

        private Button m_Stop;
        private Button m_Reset;
        private Button m_Play;

        private TMPro.TMP_InputField m_SliderTimeInput;
        private TMPro.TMP_InputField m_SceneStartTimeInput;

        private ScrollRect m_ScrollViewLeftGroup;
        private ToggleGroup m_ToggleViewLeftGroup;
        private RectTransform m_ContentViewLeftGroup;

        private ScrollRect m_ScrollViewMain;
        private RectTransform m_ScrollViewMainRectTransform;
        private ToggleGroup m_ToggleViewMain;
        private RectTransform m_ContentViewMain;

        private ScrollRect m_ScrollViewSlider;
        private Scrollbar m_ScrollbarViewMain;
        private RectTransform m_ContentSlider;
        private Slider m_TimeSlider;
        private GameObject m_SliderHandle;
        private GameObject m_SliderLine;
        private RectTransform m_SliderLineRectTransform;
        private Slider m_ZoomSlider;

        private RectTransform m_SliderRect;

        private PoolManagerLocalComponent<Text> m_TimeNumberPool;
        private PoolManagerLocalComponent<RectTransform> m_TimeSpacingPool;
        private PoolManager<CreateWaveForm.WavePoolObject> m_LeftTimeWavePool;
        private PoolManager<CreateWaveForm.WavePoolObject> m_RightTimeWavePool;

        private int m_GroupIndexSelected = 0;
        private bool m_Initialised = false;
        private CreateWaveForm m_LeftCreateWaveForm = new CreateWaveForm();
        private CreateWaveForm m_RightCreateWaveForm = new CreateWaveForm();

        private IEnumerator m_CoroutineNumberText;
        private string CurrentAudioName = "";

        public void Initialise()
        {
            this.transform.FindAndFixMasks();
            m_ScrollViewLeftGroup = this.transform.SearchComponent<ScrollRect>("Scroll ViewGroup");
            m_ContentViewLeftGroup = m_ScrollViewLeftGroup.transform.SearchComponent<RectTransform>("Content");
            m_ToggleViewLeftGroup = m_ScrollViewLeftGroup.GetComponentInChildren<ToggleGroup>();
            m_ToggleViewLeftGroup.allowSwitchOff = true;


            m_ScrollViewMain = this.transform.SearchComponent<ScrollRect>("Scroll ViewMain");
            m_ScrollViewMainRectTransform = m_ScrollViewMain.GetComponent<RectTransform>();
            m_ScrollbarViewMain = m_ScrollViewMain.transform.SearchComponent<Scrollbar>("Scrollbar Horizontal");
            m_ContentViewMain = m_ScrollViewMain.transform.SearchComponent<RectTransform>("Content");
            m_ToggleViewMain = m_ScrollViewMain.GetComponentInChildren<ToggleGroup>();
            m_ToggleViewMain.allowSwitchOff = true;


            m_SceneStartTimeInput = this.transform.SearchComponent<TMPro.TMP_InputField>("SceneStartTime");
            m_SceneStartTimeInput.onSubmit.AddListener((value) =>
            {
                if (DataContainer.Instance.CurrentGroupLinkageData != null)
                {
                    float amount = float.Parse(value);
                    amount = Mathf.Clamp(amount, 0, MAX_SECONDS);
                    TimeLineTxtPosition(amount); // sets it
                    SetPositionFromStartTime(amount, DataContainer.Instance.CurrentGroupLinkageData);
                }
            });

            m_SliderTimeInput = this.transform.SearchComponent<TMPro.TMP_InputField>("SliderTimeInput");
            m_SliderTimeInput.onSubmit.AddListener((value) =>
            {
                m_TimeSlider.value = float.Parse(value);
            });

            m_ScrollViewSlider = this.transform.SearchComponent<ScrollRect>("Scroll ViewSlider");
            m_ScrollViewSlider.vertical = false;
            m_ScrollViewSlider.horizontal = false;
            m_ContentSlider = m_ScrollViewSlider.transform.SearchComponent<RectTransform>("Content");





            m_TimeSlider = m_ScrollViewSlider.transform.GetComponentInChildren<Slider>(true);
            m_SliderRect = m_TimeSlider.GetComponent<RectTransform>();

            m_SliderHandle = m_TimeSlider.transform.SearchComponent<Transform>("Handle").gameObject;
            m_SliderLine = this.transform.SearchComponent<Transform>("SliderLine").gameObject;
            m_SliderLineRectTransform = m_SliderLine.GetComponent<RectTransform>();


            Vector2 correctAnchorPoint = new Vector2(0f, 1f);
            var itemPrefab = CachedObjects.Instance.GetPrefab(ITEM_PREFAB);
            var rect = itemPrefab.GetComponent<RectTransform>();
            rect.anchorMin = correctAnchorPoint;
            rect.anchorMax = correctAnchorPoint;

            var header = CachedObjects.Instance.GetPrefab(HEADER_PREFAB);
            rect = header.GetComponent<RectTransform>();
            rect.anchorMin = correctAnchorPoint;
            rect.anchorMax = correctAnchorPoint;


            var grathic = CachedObjects.Instance.GetPrefab(TIME_SPACE_GRATHIC_PREFAB);
            grathic.GetComponent<Image>().raycastTarget = false;
            rect = grathic.GetComponent<RectTransform>();
            rect.anchorMin = correctAnchorPoint;
            rect.anchorMax = correctAnchorPoint;


            var number = CachedObjects.Instance.GetPrefab(TIME_NUMBER_PREFAB);
            number.GetComponent<Text>().raycastTarget = false;
            rect = number.GetComponent<RectTransform>();
            rect.anchorMin = correctAnchorPoint;
            rect.anchorMax = correctAnchorPoint;

            var wave = CachedObjects.Instance.GetPrefab(TIME_SPACE_WAVE_PREFAB);
            wave.GetComponent<RawImage>().raycastTarget = false;
            rect = wave.GetComponent<RectTransform>();
            rect.anchorMin = correctAnchorPoint;
            rect.anchorMax = correctAnchorPoint;

            var hightlight = CachedObjects.Instance.GetPrefab(TIME_HIGHTLIGHT_PREFAB);
            hightlight.GetComponent<Image>().raycastTarget = false;
            

            m_TimeSlider.maxValue = MAX_SECONDS;
            m_ContentViewLeftGroup.sizeDelta = new Vector2(m_ContentViewLeftGroup.sizeDelta.x, START_GROUP_HEIGHTS);
            m_ContentViewMain.sizeDelta = new Vector2(MAX_PIXELS, START_GROUP_HEIGHTS);
            m_ContentSlider.sizeDelta = new Vector2(MAX_PIXELS, START_GROUP_HEIGHTS);
            m_SliderRect.sizeDelta = new Vector2(MAX_PIXELS, m_SliderRect.sizeDelta.y);
            m_TimeSlider.value = 0f;


            var timeItem = CachedObjects.Instance.InstantiateCachedGameObject(TIME_NUMBER_PREFAB, m_ScrollViewMain.transform);
            m_TimeNumberPool = new PoolManagerLocalComponent<Text>(timeItem, m_ContentSlider.transform);
            Destroy(timeItem);
            m_TimeNumberPool.DeSpawnAll();

            var timeSpacingItem = CachedObjects.Instance.GetPrefab(TIME_SPACE_GRATHIC_PREFAB);
            m_TimeSpacingPool = new PoolManagerLocalComponent<RectTransform>(timeSpacingItem, m_ContentSlider.transform);
            m_TimeSpacingPool.DeSpawnAll();

            var waveItem = CachedObjects.Instance.GetPrefab(TIME_SPACE_WAVE_PREFAB);

            m_LeftTimeWavePool = new PoolManager<CreateWaveForm.WavePoolObject>(waveItem, this.gameObject, CreateWaveForm.CreateNewWave, CreateWaveForm.onSpawn, CreateWaveForm.onDeSpawn);
            m_RightTimeWavePool = new PoolManager<CreateWaveForm.WavePoolObject>(waveItem, this.gameObject, CreateWaveForm.CreateNewWave, CreateWaveForm.onSpawn, CreateWaveForm.onDeSpawn);
            m_LeftTimeWavePool.DeSpawnAll();
            m_RightTimeWavePool.DeSpawnAll();

            MoveSliderGroupToCorrectPlace();


            m_AddGroup = this.transform.SearchComponent<Button>("AddGroup");
            m_AddGroup.onClick.AddListener(() =>
            {
                OscGroupData newGroup = new OscGroupData();
                DataContainer.Instance.TimeLineDataRef.Add(newGroup);
                if (DataContainer.Instance.TimeLineDataRef.Count > 1)
                {
                    Debug.LogError($"DataContainer.Instance.TimeLineDataRef  {DataContainer.Instance.TimeLineDataRef.Count}");
                }


                newGroup.m_IsBlank = false;
                InitialiseHeaderItem(newGroup, DataContainer.Instance.TimeLineDataRef.LastIndex());
                DeleteAllItems();
                InitialiseLoadedItems();
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            });

            m_AddBlankGroup = this.transform.SearchComponent<Button>("BlankGroup");
            m_AddBlankGroup.onClick.AddListener(() =>
            {
                OscGroupData newGroup = new OscGroupData();
                DataContainer.Instance.TimeLineDataRef.Add(newGroup);
                if (DataContainer.Instance.TimeLineDataRef.Count > 1)
                {
                    Debug.LogError($"DataContainer.Instance.TimeLineDataRef  {DataContainer.Instance.TimeLineDataRef.Count}");
                }
                newGroup.m_IsBlank = true;
                InitialiseHeaderItem(newGroup, DataContainer.Instance.TimeLineDataRef.LastIndex());
                DeleteAllItems();
                InitialiseLoadedItems();
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            });

            m_SelectAll = this.transform.SearchComponent<Button>("AllButton");
            m_SelectAll.onClick.AddListener(() =>
            {
                Debug.LogError("AllButton");
                DataContainer.Instance.TimeLineDataRef.ForEach(e => e.m_SelectedToggle.isOn = true);
            });

            m_SelectNone = this.transform.SearchComponent<Button>("NoneButton");
            m_SelectNone.onClick.AddListener(() =>
            {
                Debug.LogError("NoneButton");
                DataContainer.Instance.TimeLineDataRef.ForEach(e => e.m_SelectedToggle.isOn = false);
            });

            m_ZoomSlider = this.transform.SearchComponent<Slider>("ZoomSlider");
            m_ZoomSlider.minValue = MIN_PIXELS_PER_SECOND;
            m_ZoomSlider.maxValue = MAX_PIXELS_PER_SECOND;
            m_ZoomSlider.wholeNumbers = true;
            m_ZoomSlider.SetValueWithoutNotify(PIXELS_PER_SECOND);
            m_ZoomSlider.onValueChanged.AddListener((value) =>
            {
                PIXELS_PER_SECOND = (int)value;
                ScaleAllItems();
            });

            var zoomHandle = m_ZoomSlider.gameObject.ForceComponent<ImageMouseBase>();
            zoomHandle.OnPointerEnd.AddListener(() =>
            {
                ScaleAllItems();
            });

            m_Stop = this.transform.SearchComponent<Button>("Stop");
            m_Stop.onClick.AddListener(() =>
            {                
                if (HouseBuilder.HouseBuilderRoot.Instance != null) // seperate progream
                {
                    HouseBuilder.HouseBuilderRoot.Instance.StopAllSoundAndLighting();
                }
                else
                {
                    Core.PhotonGenericRef.SendGenericHeader(OscControllerRoot.GENERIC_STOP_ALL_MESSAGE);
                    OscControllerRoot.Instance.PlayDataRef.StopTimeLine();
                }
                InitialiseLoadedItems();
                ReCalcuateAllStartTimesAndSizes();

                OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();
                OscControllerRoot.Instance.AudioSourceRef.Stop();
            });

            m_Reset = this.transform.SearchComponent<Button>("Reset");
            m_Reset.onClick.AddListener(() =>
            {
                m_ScrollbarViewMain.value = 0; // set to start
                if (HouseBuilder.HouseBuilderRoot.Instance != null) // seperate progream
                {
                    HouseBuilder.HouseBuilderRoot.Instance.ResetAllSoundAndLighting();
                }
                else
                {
                    Core.PhotonGenericRef.SendGenericHeader(OscControllerRoot.GENERIC_RESET_ALL_MESSAGE);
                    ReCalcuateAllStartTimesAndSizes();
                    OscControllerRoot.Instance.PlayDataRef.ResetTimeLine();
                }
                OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();
                OscControllerRoot.Instance.AudioSourceRef.Stop();
                OscControllerRoot.Instance.AudioSourceRef.time = 0;
            });

            m_Play = this.transform.SearchComponent<Button>("Play");
            m_Play.onClick.AddListener(() =>
            {
                OscControllerRoot.Instance.SequenceRunTimePanelRef.StopAllReapeats();
                float value = GetTimeLineValue();
                if (DataContainer.Instance.GenericDataDataRef.m_SoundSource == SoundSource.RoomBuilder)
                {
                    if (HouseBuilder.HouseBuilderRoot.Instance != null) // seperate progream
                    {
                        HouseBuilder.HouseBuilderRoot.Instance.PlayAllSoundAndLighting(value);
                    }
                    else
                    {
                        Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE, value.ToString());
                        ReCalcuateAllStartTimesAndSizes();
                        OscControllerRoot.Instance.PlayDataRef.PlayTimeLine();
                    }
                }
                else
                {
                    if (HouseBuilder.HouseBuilderRoot.Instance != null) // seperate progream
                    {
                        HouseBuilder.HouseBuilderRoot.Instance.PlayAllLighting(value); 
                    }
                    else
                    {
                        Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_PLAY_MUSIC_MESSAGE, value.ToString());
                        ReCalcuateAllStartTimesAndSizes();
                        OscControllerRoot.Instance.PlayDataRef.PlayTimeLine();
                    }
                    OscControllerRoot.Instance.AudioSourceRef.time = value;
                    OscControllerRoot.Instance.AudioSourceRef.Play();
                }
            });


            Button convert = this.transform.SearchComponent<Button>("Convert");
            convert.onClick.AddListener(() =>
            {
                OscGenericPromptMenu.Instance.ShowYesNoMenu("Do you want to convert recorded data", () =>
                {
                    foreach (var item in DataContainer.Instance.TimeLineDataRef)
                    {
                        if (item.m_IsBlank == false && item.m_IsRecorded == true)
                        {
                            item.m_IsRecorded = false;
                        }
                    }
                    InitialiseLoadedItems();
                }, null);
            });


            Button deleteRuntime = this.transform.SearchComponent<Button>("DeleteRuntime");
            deleteRuntime.onClick.AddListener(() =>
            {
                OscGenericPromptMenu.Instance.ShowYesNoMenu("Do you want to delete recorded data", () =>
                {
                    DataContainer.Instance.TimeLineDataRef.RemoveAll(e => e.m_IsRecorded == true);
                    InitialiseLoadedItems();
                }, null);

            });


            m_TimeSlider.onValueChanged.AddListener((time) =>
            {
                MoveTimeLine();
                m_SliderTimeInput.SetTextWithoutNotify(m_TimeSlider.value.ToString("0.00"));
                OscControllerRoot.Instance.PlayDataRef.PlayTimeOnTimeLine(time);
                if (time == 0)
                {
                    OscControllerRoot.Instance.PlayDataRef.ClearAllDmx();
                }
            });


            var timeSlider = m_TimeSlider.transform.gameObject.ForceComponent<ImageMouseBase>();
            timeSlider.OnPointerEnd.AddListener(() =>
            {
                m_SliderTimeInput.SetTextWithoutNotify(m_TimeSlider.value.ToString("0.00"));
                OscControllerRoot.Instance.PlayDataRef.PlayTimeLineFastUntil(m_TimeSlider.value);
            });


            m_ScrollViewMain.onValueChanged.AddListener((amount) =>
            {
                MoveTimeLine();
            });

            DataContainer.Instance.OnSequenceDataChange += FilterHeaders;
            ScreenSizeChange(new Vector2(Screen.width, Screen.height));
            Core.OnScreenSizeChange += ScreenSizeChange;
            m_Initialised = true;       
        }

        public void ResetSliders()
        {
            m_ScrollbarViewMain.SetValueWithoutNotify(0);
            m_TimeSlider.SetValueWithoutNotify(0);
            if (this.isActiveAndEnabled == true)
            {
                this.WaitForFrames(5, () =>
                {
                // the wait is to make sure the ui is moved to correct place
                m_ZoomSlider.value = PIXELS_PER_SECOND_START;
                });
            }
        }

        private void MoveSliderGroupToCorrectPlace()
        {
            if(m_TimeSlider == null)
            {
                return;
            }

            float value = m_TimeSlider.value;
            float scroll = m_ScrollbarViewMain.value;
            if (value == 0 && scroll == 0)
            {
                m_TimeSlider.SetValueWithoutNotify(0);

                m_ScrollbarViewMain.SetValueWithoutNotify(0);


                Vector3[] v = new Vector3[4];
                m_ScrollViewMainRectTransform.GetWorldCorners(v);

                float diff = m_SliderHandle.transform.position.x - v[0].x;
                m_ScrollViewSlider.transform.position -= new Vector3(diff, 0, 0);

                m_TimeSlider.SetValueWithoutNotify(value);

                m_ScrollbarViewMain.SetValueWithoutNotify(scroll);
            }
            //Debug.LogError($"handle {m_SliderHandle.transform.position}");
            //for (var i = 0; i < 4; i++)
            //{
            //    Debug.LogError("World Corner " + i + " : " + v[i]);
            //}
            //Debug.LogError($"Diff {diff}");
        }

        private void OnEnable()
        {
            if (m_Initialised == true)
            {              
                ScaleAllItems();
                CreateAndAssignWave(DataContainer.Instance.GenericDataDataRef);
            }
        }

        private void ScreenSizeChange(Vector2 size)
        {
            ResetSliders();
            MoveSliderGroupToCorrectPlace();
        }

        public void FilterHeaders(OscSequenceData obj)
        {
            foreach (var item in DataContainer.Instance.TimeLineDataRef)
            {
                bool valid = (obj.m_OptionsName == item.m_OptionsName);
                if (item.m_AddButton != null)
                {
                    item.m_AddButton.interactable = valid;
                    if (obj.m_OptionsType == OscControllerRoot.OPTIONS_ALL)
                    {
                        item.m_AddButton.interactable = true; // force true
                    }
                }
            }
        }

        public float SetTimeLineMaxValue(float max) => m_TimeSlider.maxValue = max;
        public float GetTimeLineValue() => m_TimeSlider.value;
        public void SetTimeLineValue(float value)
        {
            //////////////   COMENTED OUT AS I WANT TO USE ZOOM 
            //////////////
            ////////////// still not right but getting a lot closer


            //////if (m_SliderHandle.transform.position.x >= (Screen.width * 0.8f))
            //////{
            //////    float place = m_SliderHandle.transform.localPosition.x + (m_SliderRect.sizeDelta.x / 2);
            //////    float percentage = (place / m_SliderRect.sizeDelta.x);


            //////    // these magic numbers found to work , still need more checking though 
            //////    float extraOffset = 100 - PIXELS_PER_SECOND;
            //////    extraOffset /= 10000;
            //////    m_ScrollbarViewMain.value = percentage - 0.02f - (extraOffset);
            //////}
            ///
            m_TimeSlider.SetValueWithoutNotify(value);
            MoveTimeLine();
        }

        private void ScaleText()
        {
            int spacingLengthWave = 0;
            int spacingLengthNormal = MAX_SECONDS;
            int spacingLengthGrathicsNormal = (MAX_SECONDS/ WAVE_SIZE_SECONDS) +1;
            if (DataContainer.Instance.GenericDataDataRef.m_AudioClip != null)
            {
                spacingLengthWave = ((int)DataContainer.Instance.GenericDataDataRef.m_AudioClip.length) + 1;
                spacingLengthNormal = spacingLengthWave;
                spacingLengthWave /= WAVE_SIZE_SECONDS;
                spacingLengthWave += 1;
            }


            m_TimeSpacingPool.DeSpawnAll();
            Vector3 sizeDelta = new Vector3(PIXELS_PER_SECOND * WAVE_SIZE_SECONDS, 20, 0);
            for (int i = 0; i < (spacingLengthGrathicsNormal); i++) //MAX_SECONDS
            {
                var rect = m_TimeSpacingPool.SpawnObject();
                rect.sizeDelta = sizeDelta;
                rect.transform.localPosition = new Vector3((i * PIXELS_PER_SECOND * WAVE_SIZE_SECONDS) + OFFSET_FOR_SLIDER_GRATHICS, -20f, 0);// -20, offset so can see it 
                rect.transform.localScale = Vector3.one;
            }


            // this because its very very slow
            if(m_CoroutineNumberText !=null)
            {
                StopCoroutine(m_CoroutineNumberText);
            }
            m_CoroutineNumberText = NumberText(spacingLengthNormal);
            Core.Mono.StartCoroutine(m_CoroutineNumberText);



            
            m_LeftTimeWavePool.DeSpawnAll();
            Vector3 sizeDeltaWave = new Vector3(PIXELS_PER_SECOND * WAVE_SIZE_SECONDS, m_ScrollViewMainRectTransform.sizeDelta.y / 2, 0);
            for (int i = 0; i < spacingLengthWave; i++)
            {
                var waveItem = m_LeftTimeWavePool.SpawnObject();
                waveItem.m_RectTransform.transform.SetParent(m_ContentSlider.transform);
                
                var rect = waveItem.m_RectTransform;
                rect.sizeDelta = sizeDeltaWave;
                waveItem.m_RawImage.transform.localPosition = new Vector3((i * PIXELS_PER_SECOND * WAVE_SIZE_SECONDS) + (OFFSET_FOR_SLIDER_WAVE), -150f, 0);// -20, offset so can see it 
                waveItem.m_RawImage.transform.localScale = Vector3.one;
            }

            m_RightTimeWavePool.DeSpawnAll();
            for (int i = 0; i < spacingLengthWave; i++)
            {
                var waveItem = m_RightTimeWavePool.SpawnObject();
                waveItem.m_RectTransform.transform.SetParent(m_ContentSlider.transform);

                var rect = waveItem.m_RectTransform;
                rect.sizeDelta = sizeDeltaWave;
                waveItem.m_RawImage.transform.localPosition = new Vector3((i * PIXELS_PER_SECOND * WAVE_SIZE_SECONDS) + (OFFSET_FOR_SLIDER_WAVE), -350f, 0);// -20, offset so can see it 
                waveItem.m_RawImage.transform.localScale = Vector3.one;
            }


            m_TimeSlider.transform.SetAsLastSibling();   
        }


        /// <summary>
        /// this moved to Coroutine as its very very very slow 
        /// </summary>
        private IEnumerator NumberText(int spacingLengthNormal)
        {
            yield return null;
            m_TimeNumberPool.DeSpawnAll();
            for (int i = 0; i < (spacingLengthNormal); i++)
            {
                yield return null;                              // needed for StopCoroutine   
                var textItem = m_TimeNumberPool.SpawnObject();
                textItem.text = i.ToString();
                textItem.transform.localPosition = new Vector3((i * PIXELS_PER_SECOND) + OFFSET_FOR_SLIDER_NUMBER, -5f, 0);// -5, offset so can see it 
                textItem.transform.localScale = Vector3.one;
            }

        }
        
        public void CreateAndAssignWave(OscGenericData data)
        {
            if (string.IsNullOrEmpty(data.m_AudioName) == false)
            {
                if(CurrentAudioName == data.m_AudioName)
                {
                    return;
                }
                CurrentAudioName = data.m_AudioName;
                Core.Network.GetCustomFileAudioClip(data.m_AudioName, (clip) =>
                {
                    DataContainer.Instance.GenericDataDataRef.m_AudioClip = clip;
                    ScaleText();
                    TaskAction compleate = new TaskAction(2, () =>
                    {
                        if (OscControllerRoot.Instance.isActiveAndEnabled == true)
                        {
                            OSCAssetSyncMenu.Instance.SetVisible(false);
                        }
                    });
                    var leftList = m_LeftTimeWavePool.GetPublicList();
                    Vector2 maxSize = new Vector2(MAX_PIXELS_PER_SECOND * WAVE_SIZE_SECONDS, m_ScrollViewMainRectTransform.sizeDelta.y);

                    m_LeftCreateWaveForm.CreateAndAssignSplitSeconds(leftList, WAVE_SIZE_SECONDS, 0, clip, Color.red, ALPHA_COLOR, maxSize, (isbeingCreated) =>
                    {
                        if (isbeingCreated == true)
                        {
                            if (OscControllerRoot.Instance.isActiveAndEnabled == true)
                            {
                                OSCAssetSyncMenu.Instance.SetVisible(true);
                                OSCAssetSyncMenu.Instance.SetHeaderText("Creating waveforms");
                                OSCAssetSyncMenu.Instance.SetActiveSpinning();
                            }

                        }
                    }, compleate.Increment);

                    var rightList = m_RightTimeWavePool.GetPublicList();
                    m_RightCreateWaveForm.CreateAndAssignSplitSeconds(rightList, WAVE_SIZE_SECONDS, 1, clip, Color.red, ALPHA_COLOR, maxSize, null, compleate.Increment);
                });
            }
            else
            {
                var left = m_LeftTimeWavePool.GetPublicList();
                foreach (var item in left)
                {
                    item.m_RawImage.SetActive(false);
                }
                var right = m_RightTimeWavePool.GetPublicList();
                foreach (var item in right)
                {
                    item.m_RawImage.SetActive(false);
                }
            }
        }

        private void InitialiseHeaderItem(OscGroupData newGroup, int groupIndex)
        {
            OscGroupData localGroupData = newGroup;

            localGroupData.m_MainGameObject = CachedObjects.Instance.InstantiateCachedGameObject(HEADER_PREFAB, m_ToggleViewLeftGroup.transform);

            localGroupData.m_MainGameObjectImage = localGroupData.m_MainGameObject.GetComponent<Image>();
            localGroupData.m_BlankImage = localGroupData.m_MainGameObject.transform.SearchComponent<Image>("BlankImage");
            localGroupData.m_BlankImage.SetActive(newGroup.m_IsBlank);

            localGroupData.m_BtnOptions = localGroupData.m_MainGameObject.transform.SearchComponent<Button>("Options");
            localGroupData.m_TxtOptions = localGroupData.m_BtnOptions.transform.SearchComponent<Text>("Text");

            localGroupData.m_InputFieldName = localGroupData.m_MainGameObject.transform.SearchComponent<InputField>("InputField");
            localGroupData.m_AddButton = localGroupData.m_MainGameObject.transform.SearchComponent<Button>("AddButton");
            localGroupData.m_UpButton = localGroupData.m_MainGameObject.transform.SearchComponent<Button>("UpButton");
            localGroupData.m_DownButton = localGroupData.m_MainGameObject.transform.SearchComponent<Button>("DownButton");
            localGroupData.m_SelectedToggle = localGroupData.m_MainGameObject.transform.SearchComponent<Toggle>("SelectedToggle");


            localGroupData.m_MainGameObject.transform.localPosition = new Vector3(140, (-GROUP_HEIGHT * (groupIndex + 1)), 0);  // +1 so there is always an amout, so never get -GROUP_HEIGHT * (0)
            localGroupData.AssignTextOptions();

            float possibleMax = ((DataContainer.Instance.TimeLineDataRef.Count + 2) * GROUP_HEIGHT); // +2 to make sure the size is always bigger than it needs to be 
            float height = Mathf.Max(START_GROUP_HEIGHTS, possibleMax);
            m_ContentViewLeftGroup.sizeDelta = new Vector2(m_ContentViewLeftGroup.sizeDelta.x, height);
            m_ContentViewMain.sizeDelta = new Vector2(MAX_PIXELS, height);

            SetDisabledColor(localGroupData.m_AddButton);
            OptionsChoice(groupIndex, localGroupData);



            var item = CachedObjects.Instance.InstantiateCachedGameObject(TIME_HIGHTLIGHT_PREFAB, m_ToggleViewMain.transform);
            item.transform.localPosition = localGroupData.m_MainGameObject.transform.localPosition;
            var hightlight = item.transform.GetComponent<RectTransform>();
            hightlight.sizeDelta = new Vector2(MAX_PIXELS * 2, hightlight.sizeDelta.y);

            localGroupData.m_AddButton.onClick.AddListener(() =>
            {
                var buttonIndex = DataContainer.Instance.TimeLineDataRef.FindIndex(e => e.m_MainGameObject.gameObject == localGroupData.m_MainGameObject.gameObject);
                m_GroupIndexSelected = buttonIndex;

                OscGroupData.GroupLinkageData newGroupLink = new OscGroupData.GroupLinkageData();

                var scene = DataContainer.Instance.CurrentSequenceData;
                DataContainer.Instance.TimeLineDataRef[m_GroupIndexSelected].m_GroupLinkageData.Add(newGroupLink);
                InitialiseSequenceToGroup(localGroupData, newGroupLink, scene, m_GroupIndexSelected);
                FindCorrectStartTime(groupIndex, newGroupLink);
                ScaleAllItems();
                HighlightGroupLink(newGroupLink);
                TimeLineTxtPosition(newGroupLink.m_StartTime);
                TCPMessageToSelectModel(localGroupData);
                OptionsChoice(groupIndex, localGroupData);
                HighLightHeader(localGroupData);
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            });


            var rightClickOnAddButton = localGroupData.m_AddButton.gameObject.ForceComponent<RightClick>();
            rightClickOnAddButton.onRightClick.AddListener(() =>
            {
                DeleteThisRow(localGroupData);
            });
            
            rightClickOnAddButton = localGroupData.m_BlankImage.gameObject.ForceComponent<RightClick>();
            rightClickOnAddButton.onRightClick.AddListener(() =>
            {
                DeleteThisRow(localGroupData);
            });



            var fakeButtonOnMainObject = localGroupData.m_MainGameObject.gameObject.ForceComponent<FakeButton>();
            fakeButtonOnMainObject.onClick.AddListener(() =>
            {
                HighLightHeader(localGroupData);
                TCPMessageToSelectModel(localGroupData);
            });


            SetDisabledColor(localGroupData.m_UpButton);
            localGroupData.m_UpButton.interactable = DataContainer.Instance.TimeLineDataRef.CanMoveUp(groupIndex);
            if(localGroupData.m_IsBlank == true && localGroupData.m_IsRecorded)
            {
                localGroupData.m_UpButton.interactable = false;
            }
            localGroupData.m_UpButton.onClick.AddListener(() =>
            {
                HighLightHeader(localGroupData);
                DataContainer.Instance.TimeLineDataRef.MoveUp(groupIndex);
                DeleteAllItems();
                InitialiseLoadedItems();
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            });

            SetDisabledColor(localGroupData.m_DownButton);
            localGroupData.m_DownButton.interactable = DataContainer.Instance.TimeLineDataRef.CanMoveDown(groupIndex);
            if (localGroupData.m_IsBlank == true && localGroupData.m_IsRecorded)
            {
                localGroupData.m_UpButton.interactable = false;
            }
            localGroupData.m_DownButton.onClick.AddListener(() =>
            {
                HighLightHeader(localGroupData);
                DataContainer.Instance.TimeLineDataRef.MoveDown(groupIndex);
                DeleteAllItems();
                InitialiseLoadedItems();
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            });

            var fakeButtonOnFieldName = localGroupData.m_InputFieldName.gameObject.ForceComponent<FakeButton>();
            fakeButtonOnFieldName.onClick.AddListener(() =>
            {
                HighLightHeader(localGroupData);
                TCPMessageToSelectModel(localGroupData);
            });

            localGroupData.m_SelectedToggle.isOn = localGroupData.m_IsSelected;
            localGroupData.m_SelectedToggle.onValueChanged.AddListener((value) =>
            {
                localGroupData.m_IsSelected = value;
            });


            localGroupData.m_InputFieldName.text = localGroupData.m_InputFieldNameText;
            localGroupData.m_InputFieldName.onValueChanged.AddListener((text) =>
            {
                localGroupData.m_InputFieldNameText = text;
            });
        }

        private void DeleteThisRow(OscGroupData localGroupData)
        {
            HighLightHeader(localGroupData);
            OscGenericPromptMenu.Instance.ShowYesNoMenu("Delete This Row?", () =>
            {
                var item = DataContainer.Instance.TimeLineDataRef.Find(e => e.m_MainGameObject == localGroupData.m_MainGameObject);

                DeleteAllItems();
                DataContainer.Instance.TimeLineDataRef.Remove(item);
                InitialiseLoadedItems();
                FilterHeaders(DataContainer.Instance.CurrentSequenceData);
            }, null);
        }

        private void SetDisabledColor(Button item)
        {
            // set correct color
            ColorBlock colorVar = item.colors;
            colorVar.disabledColor = DISABLED_COLOR;
            item.colors = colorVar;
        }

        private void OptionsChoice(int groupIndex, OscGroupData localGroupData)
        {
            localGroupData.m_BtnOptions.onClick.RemoveAllListeners();
            var btn = localGroupData.m_BtnOptions.transform.gameObject.ForceComponent<FakeButton>();
            btn.onClick.RemoveAllListeners();

            bool hasChildren = (DataContainer.Instance.TimeLineDataRef[groupIndex].m_GroupLinkageData.Count != 0);
            if (hasChildren == true)
            {
                btn.onClick.AddListener(() =>
                {
                    HighLightHeader(localGroupData);
                    TCPMessageToSelectModel(localGroupData);
                });
            }
            else
            {
                localGroupData.m_BtnOptions.onClick.AddListener(() =>
                {
                    OscControllerRoot.Instance.TimeLinePanelOptionsRef.SetActive(true);
                    OscControllerRoot.Instance.TimeLinePanelOptionsRef.FillInData(localGroupData);
                    HighLightHeader(localGroupData);
                    TCPMessageToSelectModel(localGroupData);
                });
            }
        }

        private void TCPMessageToSelectModel(OscGroupData oscGroupData)
        {
            if (oscGroupData.m_OptionsName != OscControllerRoot.OPTIONS_ALL)
            {
                Core.PhotonGenericRef.SendGenericHeaderAndData(OscControllerRoot.GENERIC_SELECT_DMX_MESSAGE, oscGroupData.m_OptionsDmxStart.ToString());
            }
            else
            {
                Core.PhotonGenericRef.SendGenericHeader(OscControllerRoot.GENERIC_SELECT_ALL_DMX_MESSAGE);
            }
        }

        private void FindCorrectStartTime(int groupIndex, OscGroupData.GroupLinkageData newGroupLink)
        {
            float amount = 0;
            int lastIndex = DataContainer.Instance.TimeLineDataRef.LastIndex();
            if (lastIndex >= groupIndex)
            {
                var items = DataContainer.Instance.TimeLineDataRef[groupIndex];
                amount = DataContainer.Instance.TimeLineDataRef[groupIndex].FindLastEndTime();
            }
            newGroupLink.m_StartTime = amount;
        }

        private void DeleteGroup(OscGroupData data)
        {
            for (int j = 0; j < data.m_GroupLinkageData.Count; j++)
            {
                var link = data.m_GroupLinkageData[j];
                Destroy(link.m_GameObjectItem);
            }
            Destroy(data.m_MainGameObject);
        }

        public void DeleteAllItems()
        {
            for (int i = 0; i < DataContainer.Instance.TimeLineDataRef.Count; i++)
            {
                DeleteGroup(DataContainer.Instance.TimeLineDataRef[i]);
            }
        }

        public void InitialiseLoadedItems()
        {

            var children = m_ToggleViewMain.transform.GetDirectChildren();
            for(int i = 0; i < children.Count;i++)
            {
                Destroy(children[i].gameObject);
            }

            children = m_ToggleViewLeftGroup.transform.GetDirectChildren();
            for (int i = 0; i < children.Count; i++)
            {
                Destroy(children[i].gameObject);
            }


            for (int i = 0; i < DataContainer.Instance.TimeLineDataRef.Count; i++)
            {
                OscGroupData item = DataContainer.Instance.TimeLineDataRef[i];
                InitialiseHeaderItem(item, i);

                for (int j = 0; j < item.m_GroupLinkageData.Count; j++)
                {
                    OscGroupData.GroupLinkageData link = item.m_GroupLinkageData[j];
                    int index = DataContainer.Instance.SequenceDataRef.FindIndex(e => e == link.m_Sequence);
                    if (index != -1)
                    {
                        link.m_SequenceIndex = index;
                    }

                    if (DataContainer.Instance.SequenceDataRef.ContainsIndex(link.m_SequenceIndex) == true)
                    {
                        OscSequenceData Sequence = DataContainer.Instance.SequenceDataRef[link.m_SequenceIndex];
                        InitialiseSequenceToGroup(item, link, Sequence, i);
                    }
                    else
                    {
                        Debug.LogError($"THIS SHOULD NEVER HAPPEN DataContainer.Instance.SequenceDataRef.ContainsIndex {DataContainer.Instance.TimeLineDataRef.Count}   link.m_SequenceIndex { link.m_SequenceIndex}");
                    }
                }

            }
            ScaleAllItems();
        }

        private void TimeLineTxtPosition(float time)
        {
            m_SceneStartTimeInput.SetTextWithoutNotify(time.ToString("0.00"));
        }

        private void CalculateStartTimeFromPosition(OscGroupData.GroupLinkageData data)
        {
            var pos = data.m_Item.transform.localPosition.x;
            pos /= PIXELS_PER_SECOND;
            data.m_StartTime = pos;
        }

        private void SetPositionFromStartTime(float startTime, OscGroupData.GroupLinkageData data)
        {
            data.m_StartTime = startTime;
            float x = data.m_StartTime;
            x *= PIXELS_PER_SECOND;
            data.m_Item.transform.localPosition = new Vector3(x, data.m_Item.transform.localPosition.y, data.m_Item.transform.localPosition.z);
        }

        private void InitialiseSequenceToGroup(OscGroupData groupData, OscGroupData.GroupLinkageData newGroupLink, OscSequenceData sequence, int groupIndex)
        {
            int localGroupIndex = groupIndex;
            OscGroupData.GroupLinkageData localNewGroupLink = newGroupLink;
            var item = CachedObjects.Instance.InstantiateCachedGameObject(ITEM_PREFAB, m_ToggleViewMain.transform);

            newGroupLink.m_GameObjectItem = item;
            newGroupLink.m_Image = newGroupLink.m_GameObjectItem.GetComponentInChildren<Image>(true);
            newGroupLink.m_RectTransform = newGroupLink.m_Image.GetComponent<RectTransform>();
            newGroupLink.m_Sequence = sequence;
            newGroupLink.m_SequenceDisplayName = item.GetComponentInChildren<Text>();
            newGroupLink.m_SequenceDisplayName.text = sequence.m_SequenceName;
            newGroupLink.m_OscGroupData = groupData;

            var limited = item.ForceComponent<ImageMouseMoveLimited>();
            newGroupLink.m_Item = limited;

            var Y = DataContainer.Instance.TimeLineDataRef[localGroupIndex].m_MainGameObject.transform.localPosition.y;
            Vector3 min = new Vector3(0, Y, 0);
            Vector3 max = new Vector3(MAX_PIXELS, Y, 0);

            Vector3 beginPosition = Vector3.zero;

            limited.SetLocalValidArea(min, max);
            limited.OnPointerBegin.AddListener(() =>
            {              
                HighlightGroupLink(newGroupLink);
                m_ScrollViewMain.enabled = false;
                TimeLineTxtPosition(newGroupLink.m_StartTime);
                beginPosition = limited.transform.localPosition;
                Debug.LogError($"start   {limited.transform.position}   {newGroupLink.m_StartTime}");
            });

            limited.OnPointerEnd.AddListener(() =>
            {
                m_ScrollViewMain.enabled = true;
                ReCalcuateAllStartTimesAndSizes();

                if (IsIsInValidPlace(localGroupIndex, localNewGroupLink) == false)
                {
                    limited.transform.localPosition = beginPosition;
                    ReCalcuateAllStartTimesAndSizes();
                }
                Debug.LogError($"end   {limited.transform.position}   {newGroupLink.m_StartTime}");
            });

            limited.OnPointerHeld.AddListener(() =>
            {
                CalculateStartTimeFromPosition(newGroupLink);
                TimeLineTxtPosition(newGroupLink.m_StartTime);
            });

            limited.OnRightClick.AddListener(() =>
            {
                OscGenericPromptMenu.Instance.ShowYesNoMenu("Delete This Item?", () =>
                {
                    foreach (var group in DataContainer.Instance.TimeLineDataRef)
                    {
                        for (int groupItemIndex = 0; groupItemIndex < group.m_GroupLinkageData.Count; groupItemIndex++)
                        {
                            if (group.m_GroupLinkageData[groupItemIndex] == localNewGroupLink)
                            {
                                Destroy(group.m_GroupLinkageData[groupItemIndex].m_GameObjectItem);
                                group.m_GroupLinkageData.Remove(group.m_GroupLinkageData[groupItemIndex]);
                                return;
                            }
                        }
                    }
                }, null);
            });
        }


        private void HighLightHeader(OscGroupData header)
        {
            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                group.m_MainGameObjectImage.color = OscControllerRoot.UN_SELECTED;
            }
            header.m_MainGameObjectImage.color = OscControllerRoot.SELECTED;
        }


        private void HighlightGroupLink(OscGroupData.GroupLinkageData newGroupLink)
        {
            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                for (int groupItemIndex = 0; groupItemIndex < group.m_GroupLinkageData.Count; groupItemIndex++)
                {
                    if (group.m_GroupLinkageData[groupItemIndex] == newGroupLink)
                    {
                        group.m_GroupLinkageData[groupItemIndex].m_Image.color = OscControllerRoot.SELECTED;
                        DataContainer.Instance.CurrentGroupLinkageData = newGroupLink;
                        TCPMessageToSelectModel(group);
                    }
                    else
                    {
                        group.m_GroupLinkageData[groupItemIndex].m_Image.color = OscControllerRoot.UN_SELECTED;
                    }
                }
            }

            OscControllerRoot.Instance.SequencePanelRef.TurnOnSelf(newGroupLink.m_Sequence, sendMessage: false);
        }

        private void SetCorrectoffsetPosition(float time, GameObject newRootItem)
        {
            var imageRoot = newRootItem.transform.Search("ImageRoot").gameObject;
            var rect = imageRoot.GetComponent<RectTransform>();
            var newSize = rect.sizeDelta;
            newSize.x = time * PIXELS_PER_SECOND;
            rect.sizeDelta = newSize;
            float x = (newSize.x / 2f);
            rect.localPosition = new Vector3(x, rect.localPosition.y, 0);
        }

        public void ReCalcuateAllStartTimesAndSizes()
        {
            foreach (var item in DataContainer.Instance.TimeLineDataRef)
            {
                foreach (var group in item.m_GroupLinkageData)
                {
                    if (group.m_GameObjectItem != null)
                    {
                        SetCorrectoffsetPosition(group.m_Sequence.m_DurationTime, group.m_Item.gameObject);
                        CalculateStartTimeFromPosition(group);
                    }
                }
            }
        }

        public bool IsIsInValidPlace(int linkageIndex, OscGroupData.GroupLinkageData self)
        {
            var root = DataContainer.Instance.TimeLineDataRef[linkageIndex];
            float selfStart = self.m_StartTime;
            float selfEnd = self.m_StartTime + self.m_Sequence.m_DurationTime;
            foreach (var item in root.m_GroupLinkageData)
            {
                if(item == self)
                {
                    continue;
                }

                float testStart = item.m_StartTime;
                float testEnd = item.m_StartTime + item.m_Sequence.m_DurationTime;



                bool n1 = Contains(selfStart, testStart, testEnd);
                bool n2 = Contains(selfEnd, testStart, testEnd);

                if (Contains(selfStart, testStart, testEnd) == true || Contains(selfEnd, testStart, testEnd) == true)
                {
                    return false;
                }
            }
            return true;
        }


        private bool Contains(float testItem, float min, float max)
        {
            return (testItem > min && testItem < max);
        }


    private void MoveTimeLine()
        {
            float offset = m_SliderLineRectTransform.sizeDelta.y / 2f;
            Transform originalParent = m_SliderLine.transform.parent;
            m_SliderLine.transform.SetParent(m_SliderHandle.transform);
            m_SliderLine.transform.localPosition = Vector3.zero;
            m_SliderLine.transform.localPosition = new Vector3(0f, -offset, 0f);
            m_SliderLine.transform.SetParent(originalParent);

            //////////bool show = (m_SliderLine.transform.position.x >= m_SliderGroupTopLeft.x);
            //////////m_SliderLine.SetActive(show);
            m_SliderTimeInput.SetTextWithoutNotify(m_TimeSlider.value.ToString("0.00"));
        }

        public void ScaleAllItems()
        {
            for (int i = 0; i < DataContainer.Instance.TimeLineDataRef.Count; i++)
            {
                var groupItem = DataContainer.Instance.TimeLineDataRef[i];
                float Y = groupItem.m_MainGameObject.transform.localPosition.y;
                for (int j = 0; j < groupItem.m_GroupLinkageData.Count; j++)
                {
                    var link = groupItem.m_GroupLinkageData[j];
                    link.m_GameObjectItem.transform.localPosition = new Vector3((link.m_StartTime * PIXELS_PER_SECOND), Y, 0);
                }
            }


            //m_ScrollViewSlider.transform.position = new Vector3(m_ScrollViewMain.transform.position.x - OFFSET_FOR_SLIDER, m_ScrollViewSlider.transform.position.y, 0);
            MoveSliderGroupToCorrectPlace();
            ScaleText();
            ReCalcuateAllStartTimesAndSizes();
            m_ContentSlider.sizeDelta = new Vector2(MAX_PIXELS + (OFFSET_FOR_SLIDER_DELTA_SIZE), m_ContentSlider.sizeDelta.y);
            m_SliderRect.sizeDelta = new Vector2(MAX_PIXELS, m_SliderRect.sizeDelta.y);
            m_ContentViewMain.sizeDelta = new Vector2(MAX_PIXELS, START_GROUP_HEIGHTS);
            m_SliderRect.sizeDelta = new Vector2(MAX_PIXELS, m_SliderRect.sizeDelta.y);


            float possibleMax = ((DataContainer.Instance.TimeLineDataRef.Count + 2) * GROUP_HEIGHT); // +2 to make sure the size is always bigger than it needs to be 
            float height = Mathf.Max(START_GROUP_HEIGHTS, possibleMax);
            m_ContentViewLeftGroup.sizeDelta = new Vector2(m_ContentViewLeftGroup.sizeDelta.x, height);
            m_ContentViewMain.sizeDelta = new Vector2(MAX_PIXELS, height);
            MoveTimeLine();
            UpdateSceneNames();
        }

        public void UpdateSceneNames()
        {
            foreach (var group in DataContainer.Instance.TimeLineDataRef)
            {
                foreach (var itemInGroup in group.m_GroupLinkageData)
                {
                    if (itemInGroup.m_SequenceDisplayName != null && itemInGroup.m_Sequence != null)
                    {
                        float width = itemInGroup.m_RectTransform.sizeDelta.x;
                        width -= 100f;
                        if (width < 0)
                        {
                            itemInGroup.m_SequenceDisplayName.text = itemInGroup.m_Sequence.m_SequenceName;
                        }
                        else
                        {
                            // this is a quick test of makeing duplicate screen information 
                            int length = itemInGroup.m_Sequence.m_SequenceName.Length;

                            float pixels = itemInGroup.m_SequenceDisplayName.pixelsPerUnit;
                            int size = (int)(pixels * length);


                            string space = "";
                            for (int i = 0; i < itemInGroup.m_Sequence.m_SequenceName.Length; i++)
                            {
                                space += " ";
                            }


                            itemInGroup.m_SequenceDisplayName.text = "";
                            int numberOfItems = Mathf.FloorToInt(width / size) / 3;

                            for (int i = 0; i < numberOfItems; i++)
                            {
                                itemInGroup.m_SequenceDisplayName.text += $"{itemInGroup.m_Sequence.m_SequenceName}{space}";
                            }
                            // just addd the name with no spaces
                            itemInGroup.m_SequenceDisplayName.text += $"{itemInGroup.m_Sequence.m_SequenceName}";
                        }
                    }
                }
            }
        }

    }

}


