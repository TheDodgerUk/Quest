using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static OscController.DataContainer;
using static OscController.SequencePanel;
using static SliderPrefab;
using static UnityEngine.UI.Extensions.UILineRenderer;

namespace OscController
{

    public class CurveEditorPanel
    {
        
        private CurveEditorPanelMono m_CurveEditorPanelMono;

        public CurveEditorPanel(CurveEditorPanelMono curveEditorPanelMono)
        {
            m_CurveEditorPanelMono = curveEditorPanelMono;
            m_CurveEditorPanelMono.Initialise();
        }


        public void SetActive(bool active)
        {
            m_CurveEditorPanelMono.SetActive(active);
            m_CurveEditorPanelMono.m_CurrentCurveIndex = 0;
        }
        public void SetData(FixtureSliderData fixtureSliderData, int dmxNumber, SliderPrefab sliderPrefab) => m_CurveEditorPanelMono.SetData(fixtureSliderData, dmxNumber, sliderPrefab);
        public bool IsSameFixtureData(FixtureSliderData testFixture) => m_CurveEditorPanelMono.IsSameFixtureData(testFixture);
        public bool IsActive => m_CurveEditorPanelMono.isActiveAndEnabled;
        public void RefreshCurve() => m_CurveEditorPanelMono.RefreshCurve();

        public void RefreshAllCurves() => m_CurveEditorPanelMono.RefreshAllCurves();

        public int CurrentIndex => m_CurveEditorPanelMono.m_CurrentCurveIndex;
        public void SetSliderNoCallback(float time) => m_CurveEditorPanelMono.SetSlider(time);
        public ButtonText SpecailButton => m_CurveEditorPanelMono.SpecialButton;
    }


    public class CurveEditorPanelMono : MonoBehaviour
    {
        private readonly Vector3 PLACEMENT_SINGLE = new Vector3(505f, 210f, 0f);
        private readonly Vector3 PLACEMENT_WITHIN_ROOMBUILDER = new Vector3(505f, -286.3f, 0f);

        private const float OFFSET = 0.0001f;
        private const string TIME_SPACE_GRATHIC_PREFAB = "TimeLineGrathicPrefab";

        private const int REPEAT_OPTIONS_COUNT = 10; // any more than 10 and the curve goes weird!!!
        private const int REPEAT_OPTIONS_TO_REAL_NUMBER = 1;

        private const int PER_CURVE = 100;

        private PoolManagerLocalComponent<Transform> m_TimeSpacingPool;

        private Vector2 m_PanelPosition;
        private Vector2 m_LineRendererSize;
        public int m_CurrentCurveIndex = 0;

        private PoolManagerLocalComponent<ImageMouseMoveLimited> m_CirlcePool;

        private UILineRenderer m_UILineRendererRaw;
        private UILineRenderer m_UILineRendererRepeated;
        private RightClick m_RightClick;
        private FixtureSliderData m_CurrentFixtureSliderData;
        private GameObject m_FindNewPointOnCurveTestObject;
        private Dropdown m_CurveTypeDropDown;
        private Dropdown m_PresetsDropDown;
        private Dropdown m_RepeatDropDown;

        private Button m_ResetButton;
        private Button m_PlayButton;
        private Button m_CopyButton;
        private Button m_PasteButton;
        private Button m_FlipLeftRightButton;
        private Button m_FlipUpDownButton;
        private Toggle m_RepeatInvert;

        private Button m_PrevButton;
        private Button m_NextButton;
        private InputField m_CurveInputX;
        private InputField m_CurveInputY;
        public ButtonText SpecialButton { get; private set; }


        private FixtureSliderData m_CopiedFixtureSliderData;

        private GameObject m_SliderLine;
        private Slider m_Slider;
        private GameObject m_SliderHandle;
        private Text m_TxtDmxNumber;
        private int m_SliderValue = -1;
        private int m_CurrentDmxNumber;
        private SliderPrefab m_CurrentSliderPrefab;

        private IEnumerator m_Coroutine;

        private readonly List<string> TYPE_OPTIONS = new List<string>
        {
            BezierType.Straight.ToString(),
            BezierType.AnimationCurve.ToString(),
            BezierType.Square.ToString(),            
        };

        public enum PresetTypes
        {
            Sine,
            SawTooth,
            LeftLtoRightH,
            LeftHtoRightL,
        }

        private readonly List<string> PRESETS_OPTIONS = new List<string>
        {
            PresetTypes.Sine.ToString(),
            PresetTypes.SawTooth.ToString(),
            PresetTypes.LeftLtoRightH.ToString(),
            PresetTypes.LeftHtoRightL.ToString(),
        };


        private void OnEnable()
        {
            if(HouseBuilder.HouseBuilderRoot.Instance == null)
            {
                this.transform.localPosition = PLACEMENT_SINGLE;
            }
            else
            {
                this.transform.localPosition = PLACEMENT_WITHIN_ROOMBUILDER;
            }
        }

        public void Initialise()
        {
            m_PanelPosition = this.GetComponent<RectTransform>().sizeDelta;
            m_UILineRendererRaw = this.transform.SearchComponent<UILineRenderer>("UI LineRendererRaw");
            m_UILineRendererRaw.BezierSegmentsPerCurve = PER_CURVE;
            m_LineRendererSize = m_UILineRendererRaw.GetComponent<RectTransform>().sizeDelta;
            m_UILineRendererRepeated = this.transform.SearchComponent<UILineRenderer>("UI LineRendererRepeated");


            GameObject obj = CachedObjects.Instance.InstantiateCachedGameObject("CurveGrabPointPrefab", m_UILineRendererRaw.gameObject.transform.parent.gameObject);
            obj.ForceComponent<ImageMouseMoveLimited>();
            m_CirlcePool = new PoolManagerLocalComponent<ImageMouseMoveLimited>(obj, this.transform);
            Destroy(obj);


            m_FindNewPointOnCurveTestObject = new GameObject("m_FindNewPointOnCurveTestObject");
            m_FindNewPointOnCurveTestObject.transform.SetParent(m_UILineRendererRaw.transform);

            m_RightClick = m_UILineRendererRaw.gameObject.ForceComponent<RightClick>();
            m_RightClick.onRightClick.AddListener(() =>
            {
                var newCurveEntry = FindNewCurvePoint();

                newCurveEntry.y = Mathf.Clamp(newCurveEntry.y, 0f, (float)OscControllerRoot.MAX_DMX_VALUE);
                m_CurrentFixtureSliderData.m_AnimationCurveRaw.AddKey(newCurveEntry.x, newCurveEntry.y);
                for (int i = 0; i < m_CurrentFixtureSliderData.m_AnimationCurveRaw.keys.Length; i++)
                {
                    if (m_CurrentFixtureSliderData.m_AnimationCurveRaw.keys[i].time == newCurveEntry.x)
                    {
                        m_CurrentCurveIndex = i;
                    }
                }

                RefreshCurve();
            });

            InitialiseButtons();

            GameObject timeSpacingItemPrefab = CachedObjects.Instance.InstantiateCachedGameObject(TIME_SPACE_GRATHIC_PREFAB, this.transform);
            m_TimeSpacingPool = new PoolManagerLocalComponent<Transform>(timeSpacingItemPrefab, this.transform);
            Destroy(timeSpacingItemPrefab);
            m_TimeSpacingPool.DeSpawnAll();
            ScaleTimeSpaceing();

            m_Slider = this.transform.SearchComponent<Slider>("Slider");
            m_SliderLine = this.transform.SearchComponent<Transform>("SliderLine").gameObject;
            m_SliderHandle = m_Slider.transform.SearchComponent<Transform>("Handle").gameObject;
            MoveHandle();

            m_Slider.onValueChanged.AddListener((value) =>
            {
                m_SliderValue = PlayCurveValue(m_SliderValue, value);
                MoveHandle();
            });
        }

        private void InitialiseButtons()
        {
            InitialiseBottomButtons();
            InitialiseTopButtons();
        }

        private void InitialiseBottomButtons()
        {
            m_CurveTypeDropDown = this.transform.SearchComponent<Dropdown>("CurveTypeDropdown");
            m_CurveTypeDropDown.ClearOptions();
            m_CurveTypeDropDown.AddOptions(TYPE_OPTIONS);
            m_CurveTypeDropDown.onValueChanged.AddListener((value) =>
            {
                Enum.TryParse(TYPE_OPTIONS[value], out BezierType bezierType);
                if (m_CurrentFixtureSliderData != null)
                {
                    m_CurrentFixtureSliderData.m_BezierType = bezierType;
                    RefreshCurve();
                }
            });


            m_PresetsDropDown = this.transform.SearchComponent<Dropdown>("PresetsDropdown");
            m_PresetsDropDown.ClearOptions();
            m_PresetsDropDown.AddOptions(PRESETS_OPTIONS);
            m_PresetsDropDown.onValueChanged.AddListener((value) =>
            {
                Enum.TryParse(PRESETS_OPTIONS[value], out PresetTypes presetTypes);
                if (m_CurrentFixtureSliderData != null)
                {
                    PresetCurve(presetTypes);
                    RefreshCurve();
                }
            });

            m_RepeatDropDown = this.transform.SearchComponent<Dropdown>("RepeatDropdown");
            m_RepeatDropDown.ClearOptions();

            List<string> numberToShow = new List<string>();
            for(int i = 0; i < REPEAT_OPTIONS_COUNT; i++)
            {
                numberToShow.Add((i + REPEAT_OPTIONS_TO_REAL_NUMBER).ToString());
            }
            m_RepeatDropDown.AddOptions(numberToShow);
            m_RepeatDropDown.onValueChanged.AddListener((value) =>
            {
                int repeatAmount = value + REPEAT_OPTIONS_TO_REAL_NUMBER;
                m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber = repeatAmount;
                RefreshCurve();
            });


            m_ResetButton = this.transform.SearchComponent<Button>("ResetButton");
            m_ResetButton.onClick.AddListener(() =>
            {
                if (m_Coroutine != null)
                {
                    m_Slider.value = 0;
                    m_SliderValue = -1;

                    StopCoroutine(m_Coroutine);
                    m_Coroutine = null;
                
                    m_SliderValue = PlayCurveValue(m_SliderValue, 0);
                }
            });

            m_PlayButton = this.transform.SearchComponent<Button>("PlayButton");
            m_PlayButton.onClick.AddListener(() =>
            {
                if (m_Coroutine == null)
                {
                    var CurrentSceneData = DataContainer.Instance.CurrentSequenceData;
                    var playTime = CurrentSceneData.m_DurationTime;

                    m_Coroutine = PlayAnimationCurves(CurrentSceneData, playTime);
                    StartCoroutine(m_Coroutine);
                }               
            });

            m_RepeatInvert = this.transform.SearchComponent<Toggle>("RepeatInvert");
            m_RepeatInvert.onValueChanged.AddListener((value) =>
            {
                Debug.LogError("RepeatInvert change linkage of repeated so invert then not then invert");
                m_CurrentFixtureSliderData.m_RepeatInvert = value;
                RefreshCurve();
            });

            m_CopyButton = this.transform.SearchComponent<Button>("CopyButton");
            m_CopyButton.onClick.AddListener(() =>
            {
                CopyCurve();
            });

            m_PasteButton = this.transform.SearchComponent<Button>("PasteButton");
            m_PasteButton.onClick.AddListener(() =>
            {
                PasteCurve();
            });


            m_FlipLeftRightButton = this.transform.SearchComponent<Button>("FlipLeftRight");
            m_FlipLeftRightButton.onClick.AddListener(() =>
            {
                FlipLeftRight();
            });

            m_FlipUpDownButton = this.transform.SearchComponent<Button>("FlipUpDown");
            m_FlipUpDownButton.onClick.AddListener(() =>
            {
                FlipUpDown();
            });

            m_TxtDmxNumber = this.transform.SearchComponent<Text>("DmxNumber");
        }

        private void InitialiseTopButtons()
        {
            m_PrevButton = this.transform.SearchComponent<Button>("PrevNode");
            m_PrevButton.onClick.AddListener(() =>
            {
                m_CurrentCurveIndex = m_CurrentCurveIndex.WrapDecrement(m_CurrentFixtureSliderData.m_AnimationCurveRaw.length);
                RefreshCurve();
            });

            m_NextButton = this.transform.SearchComponent<Button>("NextNode");
            m_NextButton.onClick.AddListener(() =>
            {
                m_CurrentCurveIndex = m_CurrentCurveIndex.WrapIncrement(m_CurrentFixtureSliderData.m_AnimationCurveRaw.length);
                RefreshCurve();
            });

            m_CurveInputX = this.transform.SearchComponent<InputField>("InputFieldX");
            m_CurveInputX.onEndEdit.AddListener((amount) =>
            {
                float amountFloat = float.Parse(amount);
                amountFloat = Mathf.Clamp(amountFloat, 0f, DataContainer.Instance.CurrentSequenceData.m_DurationTime);
                m_CurveInputX.SetTextWithoutNotify(amountFloat.ToString("0.00"));
                Keyframe curve = m_CurrentFixtureSliderData.m_AnimationCurveRaw[m_CurrentCurveIndex];
                curve.time = amountFloat;
                m_CurrentFixtureSliderData.m_AnimationCurveRaw.MoveKey(m_CurrentCurveIndex, curve);
                RefreshCurve();
            });

            m_CurveInputY = this.transform.SearchComponent<InputField>("InputFieldY");
            m_CurveInputY.onEndEdit.AddListener((amount) =>
            {
                int amountInt = int.Parse(amount);
                amountInt = Mathf.Clamp(amountInt, 0, OscControllerRoot.MAX_DMX_VALUE);
                m_CurveInputY.SetTextWithoutNotify(amountInt.ToString());
                Keyframe curve = m_CurrentFixtureSliderData.m_AnimationCurveRaw[m_CurrentCurveIndex];
                curve.value = amountInt;
                m_CurrentFixtureSliderData.m_AnimationCurveRaw.MoveKey(m_CurrentCurveIndex, curve);
                RefreshCurve();
            });



            SpecialButton = new ButtonText(this.transform.SearchComponent<Button>("Special").gameObject);
            SpecialButton.ButtonRef.onClick.AddListener(() =>
            {
                if (m_CurrentSliderPrefab.m_WheelData != null)
                {
                    OscControllerRoot.Instance.WheelPanelRef.SetActive(!OscControllerRoot.Instance.WheelPanelRef.IsActive);
                    OscControllerRoot.Instance.WheelPanelRef.SetWheelData(m_CurrentSliderPrefab, SpecialButton.ButtonRef.transform);
                }
            });
        }

        private void ScaleTimeSpaceing()
        {
            var CurrentSceneData = DataContainer.Instance.CurrentSequenceData;
            if (CurrentSceneData != null)
            {
                m_TimeSpacingPool.DeSpawnAll();

                float spacingAmount = m_LineRendererSize.x / CurrentSceneData.m_DurationTime;
                for (int i = 0; i < CurrentSceneData.m_DurationTime.Ceil(); i++)
                {
                    var spaceItem = m_TimeSpacingPool.SpawnObject();
                    RectTransform rect = spaceItem.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector3(spacingAmount, 20, 0);

                    spaceItem.transform.localPosition = m_UILineRendererRaw.transform.localPosition;
                    spaceItem.transform.localPosition += new Vector3((spacingAmount / 2) + ( i * spacingAmount), 10f, 0f);
                    spaceItem.transform.localScale = Vector3.one;
                }
            }

            MoveHandle();
        }

        public void SetSlider(float time)
        {
            m_Slider.SetValueWithoutNotify(time);
            MoveHandle();
        }


        private void MoveHandle()
        {
            if (m_SliderLine != null && m_SliderHandle != null)
            {

                Transform originalParent = m_SliderLine.transform.parent;
                m_SliderLine.transform.SetParent(m_SliderHandle.transform);
                m_SliderLine.transform.localPosition = Vector3.zero;
                m_SliderLine.transform.localPosition = new Vector3(0f, -175f, 0f);
                m_SliderLine.transform.SetParent(originalParent);

                m_UILineRendererRepeated.transform.SetAsLastSibling();
                m_UILineRendererRaw.transform.SetAsLastSibling();
            }
        }


        public bool IsSameFixtureData(FixtureSliderData test) => test == m_CurrentFixtureSliderData;

        private void PresetCurve(PresetTypes presetTypes)
        {
            AnimationCurve curve = m_CurrentFixtureSliderData.m_AnimationCurveRaw;
            AnimationCurve newCurve = new AnimationCurve();
            float durationTime = DataContainer.Instance.CurrentSequenceData.m_DurationTime;
            Keyframe newKey = new Keyframe();
            switch (presetTypes)
            {
                case PresetTypes.Sine:
                    m_CurrentFixtureSliderData.m_BezierType = BezierType.AnimationCurve;

                    newKey = new Keyframe
                    {
                        time = 0,
                        value = 0
                    };
                    newCurve.AddKey(newKey);

                    newKey = new Keyframe
                    {
                        time = (durationTime / 2f),
                        value = OscControllerRoot.MAX_DMX_VALUE
                    };
                    newCurve.AddKey(newKey);

                    newKey = new Keyframe
                    {
                        time = durationTime,
                        value = 0
                    };
                    newCurve.AddKey(newKey);
                    m_CurrentFixtureSliderData.m_AnimationCurveRaw = newCurve;
                    break;
                case PresetTypes.SawTooth:
                    m_CurrentFixtureSliderData.m_BezierType = BezierType.Straight;
                    break;

                case PresetTypes.LeftLtoRightH:

                    newKey = new Keyframe
                    {
                        time = 0,
                        value = 0
                    };
                    newCurve.AddKey(newKey);

                    newKey = new Keyframe
                    {
                        time = durationTime,
                        value = OscControllerRoot.MAX_DMX_VALUE
                    };
                    newCurve.AddKey(newKey);
                    m_CurrentFixtureSliderData.m_AnimationCurveRaw = newCurve;
                    m_CurrentFixtureSliderData.m_BezierType = BezierType.AnimationCurve;
                    break;

                case PresetTypes.LeftHtoRightL:

                    newKey = new Keyframe
                    {
                        time = 0,
                        value = OscControllerRoot.MAX_DMX_VALUE
                    };
                    newCurve.AddKey(newKey);

                    newKey = new Keyframe
                    {
                        time = durationTime,
                        value = 0
                    };
                    newCurve.AddKey(newKey);
                    m_CurrentFixtureSliderData.m_AnimationCurveRaw = newCurve;
                    m_CurrentFixtureSliderData.m_BezierType = BezierType.AnimationCurve;
                    break;
                default:
                    break;
            }

        }

        private void FlipLeftRight()
        {
            AnimationCurve existingCurve = m_CurrentFixtureSliderData.m_AnimationCurveRaw;
            AnimationCurve newCurve = new AnimationCurve();
            for (int i = 0; i < existingCurve.length; i++)
            {
                Keyframe newFrame = new Keyframe
                {
                    time = existingCurve[i].time,
                    value = existingCurve[(existingCurve.length - 1) - i].value
                };
                newCurve.AddKey(newFrame);
            }
            m_CurrentFixtureSliderData.m_AnimationCurveRaw = newCurve;
            RefreshCurve();
        }


        private void FlipUpDown()
        {
            AnimationCurve curve = m_CurrentFixtureSliderData.m_AnimationCurveRaw;
            for (int i = 0; i < curve.length; i++)
            {
                var key = curve[i];
                key.value = OscControllerRoot.MAX_DMX_VALUE - key.value;
                curve.MoveKey(i, key);
            }
            RefreshCurve();
        }

        private void CopyCurve()
        {
            m_CopiedFixtureSliderData =  m_CurrentFixtureSliderData;
            RefreshCurve();
        }

        private void PasteCurve()
        {
            if(m_CopiedFixtureSliderData == null)
            {
                return;
            }
            if (m_CopiedFixtureSliderData.m_AnimationCurveRaw != null)
            {
                AnimationCurve newCurve = new AnimationCurve();
                for (int i = 0; i < m_CopiedFixtureSliderData.m_AnimationCurveRaw.length; i++)
                {
                    newCurve.AddKey(m_CopiedFixtureSliderData.m_AnimationCurveRaw[i]);
                }
                m_CurrentFixtureSliderData.m_AnimationCurveRaw = newCurve;
                m_CurrentFixtureSliderData.m_BezierType = m_CopiedFixtureSliderData.m_BezierType;
                m_CurrentFixtureSliderData.m_UseAnimationCurve = m_CopiedFixtureSliderData.m_UseAnimationCurve;
                m_CurrentFixtureSliderData.m_RepeatInvert = m_CopiedFixtureSliderData.m_RepeatInvert;
                m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber = m_CopiedFixtureSliderData.m_AnimationCurveRepeatNumber;
            }
            RefreshCurve();
        }



        IEnumerator PlayAnimationCurves(OscSequenceData data, float playTime)
        {
            Debug.LogError("Playng Curve Started");
            int currentValue = -1;
            float timer = 0;
            while (timer < playTime)
            {
                currentValue = PlayCurveValue(currentValue, timer);
                yield return new WaitForEndOfFrame();
                timer += Time.deltaTime;
                m_Slider.SetValueWithoutNotify(timer);
                MoveHandle();
            }

            // play the last item
            timer = playTime;
            currentValue = PlayCurveValue(currentValue, timer);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            m_Slider.SetValueWithoutNotify(timer);
            MoveHandle();

            Debug.LogError("Playng Curve Finished");
        }

        private int PlayCurveValue(int currentValue, float timer)
        {
            byte value = OscControllerRoot.Instance.PlayDataRef.GetDmxValue(timer, m_CurrentFixtureSliderData);
            if (currentValue != value)
            {
                currentValue = value;
                int fullDmxIndex = m_CurrentFixtureSliderData.m_FixtureData.m_DmxStartIndex + m_CurrentFixtureSliderData.m_ChannelIndex;
                var slider = OscControllerRoot.Instance.SliderPanelRef.FindSliderPrefab(fullDmxIndex);
                OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(currentValue, fullDmxIndex, slider);
            }

            return currentValue;
        }

        private Vector2 FindNewCurvePoint()
        {
            float currentTime = 0;

            Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            float distance = float.MaxValue;
            Vector2 closestPosition = Vector2.one;
            Vector2 newKeyValue = Vector2.one;
            float maxTime = m_CurrentFixtureSliderData.m_AnimationCurveRaw.keys[m_CurrentFixtureSliderData.m_AnimationCurveRaw.length - 1].time;

            while (currentTime < maxTime)
            {
                var value = m_CurrentFixtureSliderData.m_AnimationCurveRaw.Evaluate(currentTime);

                Vector2 newTestPlacement = new Vector2(currentTime, value);
                Vector2 testPoint = new Vector2(currentTime / maxTime, (value / OscControllerRoot.MAX_DMX_VALUE));
                m_FindNewPointOnCurveTestObject.transform.localPosition = testPoint * m_LineRendererSize;

                float tempDis = Vector2.Distance(m_FindNewPointOnCurveTestObject.transform.position, mousePosition);
                if (tempDis < distance)
                {
                    distance = tempDis;
                    closestPosition = testPoint;
                    newKeyValue = newTestPlacement;
                }
                currentTime += OFFSET;
            }
            return newKeyValue;
        }


        public void RefreshAllCurves()
        {
            // weird issue, i need to do it twice, i not like it TODO
            // thats why i flat coding it, and needs tobe looked at a later stage
            // maxTime comes out at last time, second time around it uses the new time
            OscSequenceData currentSceneData = DataContainer.Instance.CurrentSequenceData;
            for (int i = 0; i < currentSceneData.m_SliderData.Count; i++)
            {
                FixtureSliderData fixture = currentSceneData.m_SliderData[i];
                SetData(fixture, fixture.m_DmxValue, null);
            }
        }

        public void RefreshCurve() => SetData(m_CurrentFixtureSliderData, m_CurrentDmxNumber, m_CurrentSliderPrefab);


        public void SetData(FixtureSliderData fixtureSliderData, int dmxNumber, SliderPrefab sliderPrefab)
        {
            if (fixtureSliderData == null)
            {
                return;
            }

            if (m_CurrentFixtureSliderData != fixtureSliderData)
            {
                OscControllerRoot.Instance.WheelPanelRef.SetActive(false);
                m_CurrentCurveIndex = 0;
            }
            m_CurrentFixtureSliderData = fixtureSliderData;


            ScaleTimeSpaceing();
            OscSequenceData currentSceneData = DataContainer.Instance.CurrentSequenceData;
            m_Slider.minValue = 0;
            m_Slider.maxValue = currentSceneData.m_DurationTime;

            m_CurrentDmxNumber = dmxNumber;
            m_TxtDmxNumber.text = m_CurrentDmxNumber.ToString();
            m_CurrentSliderPrefab = null;
            if (sliderPrefab != null)
            {
                m_CurrentSliderPrefab = sliderPrefab;
            }
            else
            {
                Debug.LogError("sliderPrefab == null");
            }


            Keyframe currentKeyframe = m_CurrentFixtureSliderData.m_AnimationCurveRaw[m_CurrentCurveIndex];
            m_CurveInputX.text = currentKeyframe.time.ToString("0.00");
            m_CurveInputY.text = ((int)currentKeyframe.value).ToString();
            m_CurveInputX.interactable = ((m_CurrentCurveIndex != 0) && (m_CurrentCurveIndex != m_CurrentFixtureSliderData.m_AnimationCurveRaw.length - 1)); // false if its the ends

            if (m_CurrentSliderPrefab != null)
            {
                m_CurrentSliderPrefab.m_Slider.SetValueWithoutNotify(currentKeyframe.value);
                // cant be as the colour looks wrong if do m_SpecialButton.interactable = (m_CurrentSliderPrefab.m_WheelData != null);

                ////////// this coudl be whats breaking shit 
                ///////////////////OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(currentKeyframe.value, m_CurrentDmxNumber, m_CurrentSliderPrefab, SpecialButton);
            }
            m_CirlcePool.DeSpawnAll();


            int dropDownIndex = TYPE_OPTIONS.FindIndex(e => e == m_CurrentFixtureSliderData.m_BezierType.ToString());
            m_CurveTypeDropDown.SetValueWithoutNotify(dropDownIndex);
            m_RepeatDropDown.SetValueWithoutNotify(m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber - 1);
            m_RepeatInvert.SetIsOnWithoutNotify(m_CurrentFixtureSliderData.m_RepeatInvert);



            if (m_CurrentFixtureSliderData.m_AnimationCurveRaw.length < 2)
            {
                Debug.LogError("Error found length not right ");
                // if for some reason there is an error, reset
                m_CurrentFixtureSliderData.m_AnimationCurveRaw = new AnimationCurve();
                m_CurrentFixtureSliderData.m_AnimationCurveRaw.AddKey(0, 0);
                m_CurrentFixtureSliderData.m_AnimationCurveRaw.AddKey(currentSceneData.m_DurationTime, 0);

                m_CurrentFixtureSliderData.m_AnimationCurveRepeated = new AnimationCurve();
                m_CurrentFixtureSliderData.m_AnimationCurveRepeated.AddKey(0, 0);
                m_CurrentFixtureSliderData.m_AnimationCurveRepeated.AddKey(currentSceneData.m_DurationTime, 0);
            }

            AnimationCurve curveRaw = m_CurrentFixtureSliderData.m_AnimationCurveRaw;
            OrderKeyFrames(ref curveRaw);
            MoveKeyFrames(ref curveRaw, currentSceneData);

            AnimationCurve curveRepeated = CreateRepeatedAnimationCurve(curveRaw);
            float maxTime;
            Vector2[] pointsRaw;
            Vector2[] pointsRepeated;
            CreateRawLineRenderFromCurve(curveRaw, out maxTime, out pointsRaw);
            CreateReapeatedLineRenderFromCurve(curveRaw, curveRepeated, out maxTime, out pointsRepeated);
            CreateRawGrabPointsForCurve(curveRaw, maxTime, pointsRaw);

            OscControllerRoot.Instance.SliderPanelRef.CopyAllMultipleCurves(m_CurrentFixtureSliderData, sliderPrefab);
        }

        private void CreateRawGrabPointsForCurve(AnimationCurve curve, float maxTime, Vector2[] points)
        {
            float localMaxTime = maxTime;
            int curveLength = curve.length;
            if (m_CurveTypeDropDown.value == 2) // Square  TYPE_OPTIONS
            {
                curveLength -= 1; // dont show last circle as cannot see that 
            }
            for (int i = 0; i < curveLength; i++)
            {
                int localIndex = i;

                var circle = m_CirlcePool.SpawnObject(m_UILineRendererRaw.transform);

                if (m_CurrentCurveIndex == i)
                {
                    circle.GetComponent<Image>().color = Color.red;
                }
                else
                {
                    circle.GetComponent<Image>().color = Color.blue;
                }

                LimitMovement(curve, i, circle, maxTime);

                circle.transform.localPosition = points[i] * m_LineRendererSize;

                circle.OnRightClick.RemoveAllListeners();
                circle.OnRightClick.AddListener(() =>
                {
                    if (localIndex != 0 && localIndex != curve.length - 1) // not ends
                    {
                        Debug.LogError("Remove key");
                        curve.RemoveKey(localIndex);
                        RefreshCurve();
                    }
                    else
                    {
                        Debug.LogError("Cannot Remove key");
                    }
                });

                circle.OnPointerHeld.RemoveAllListeners();
                circle.OnPointerHeld.AddListener(() =>
                {
                    m_CurrentCurveIndex = localIndex;
                    DataContainer.Instance.CurrentAnimationCurve = curve;
                    var localPos = circle.transform.localPosition;
                    var relative = localPos / m_LineRendererSize;
                    Keyframe newKey = new Keyframe
                    {
                        time = relative.x * localMaxTime,
                        value = relative.y * OscControllerRoot.MAX_DMX_VALUE
                    };
                    OscControllerRoot.Instance.SliderPanelRef.DisplayVisualsAndSendSlideData(newKey.value, m_CurrentDmxNumber, m_CurrentSliderPrefab, SpecialButton);
                    curve.MoveKey(localIndex, newKey);
                    RefreshCurve();
                });

                circle.OnPointerEnd.RemoveAllListeners();
                circle.OnPointerEnd.AddListener(() =>
                {
                    RefreshCurve();
                });

            }
        }

        private void CreateRawLineRenderFromCurve(AnimationCurve curve, out float maxTime, out Vector2[] points)
        {
            maxTime = curve[curve.length - 1].time;
            points = new Vector2[curve.length];
            for (int i = 0; i < curve.length; i++)
            {
                points[i] = new Vector2(curve[i].time / maxTime, (curve[i].value / OscControllerRoot.MAX_DMX_VALUE));
                points[i].x /= m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber;
            }
            m_UILineRendererRaw.transform.localScale = Vector3.one;
            m_UILineRendererRaw.Points = points;
            m_UILineRendererRaw.BezierMode = m_CurrentFixtureSliderData.m_BezierType;
            m_UILineRendererRaw.color = Color.red;
        }


        private void CreateReapeatedLineRenderFromCurve(AnimationCurve curveRaw, AnimationCurve curveRepeted, out float maxTime, out Vector2[] points)
        {
            m_UILineRendererRepeated.enabled = (curveRaw.length != curveRepeted.length);

            maxTime = curveRepeted[curveRepeted.length - 1].time;
            points = new Vector2[curveRepeted.length];
            for (int i = 0; i < curveRepeted.length; i++)
            {
                points[i] = new Vector2(curveRepeted[i].time / maxTime, (curveRepeted[i].value / OscControllerRoot.MAX_DMX_VALUE));
            }
            m_UILineRendererRepeated.transform.localScale = Vector3.one;
            m_UILineRendererRepeated.Points = points;
            m_UILineRendererRepeated.BezierMode = m_CurrentFixtureSliderData.m_BezierType;
            m_UILineRendererRepeated.color = Color.blue;
        }

        private AnimationCurve CreateRepeatedAnimationCurve(AnimationCurve curve)
        {
            if (m_CurrentFixtureSliderData.m_RepeatInvert == false)
            {
                float maxTime = curve[curve.length - 1].time;
                AnimationCurve newCurve = new AnimationCurve();
                for (int i = 0; i < m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber; i++)
                {
                    for (int j = 0; j < curve.length; j++)
                    {
                        Keyframe raw = curve[j];
                        Keyframe newKey = new Keyframe();
                        newKey.time = raw.time / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber;
                        newKey.time += i * (maxTime / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber);
                        if (j == 0 && i != 0) // not first repeat start but all others
                        {
                            newKey.time += OFFSET;
                        }
                        newKey.value = raw.value;
                        newCurve.AddKey(newKey);
                    }
                }
                m_CurrentFixtureSliderData.m_AnimationCurveRepeated = newCurve;
                return m_CurrentFixtureSliderData.m_AnimationCurveRepeated;
            }
            else
            {
                float maxTime = curve[curve.length - 1].time;

                AnimationCurve repeatCurveForward = new AnimationCurve();
                for (int i = 1; i < curve.length; i++) // miss the fitst item 
                {
                    Keyframe raw = curve[i];
                    Keyframe newKey = new Keyframe();
                    newKey.time = raw.time;
                    newKey.value = raw.value;
                    repeatCurveForward.AddKey(newKey);
                }

                AnimationCurve repeatCurveBackward = new AnimationCurve();
                for (int i = 0; i < repeatCurveForward.length; i++)
                {
                    Keyframe raw = curve[i];
                    Keyframe newKey = new Keyframe();
                    newKey.time = maxTime - raw.time;
                    newKey.value = raw.value;
                    repeatCurveBackward.AddKey(newKey);
                }
                OrderKeyFrames(ref repeatCurveBackward);



                AnimationCurve newCurve = new AnimationCurve();
                for (int j = 0; j < curve.length; j++)
                {
                        Keyframe raw = curve[j];
                        Keyframe newKey = new Keyframe();
                        newKey.time = raw.time / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber;
                        newKey.time += 0 * (maxTime / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber);
                        newKey.value = raw.value;
                        newCurve.AddKey(newKey);
                }

                for(int i = 1; i < m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber; i++) // not the first one as thats taken carre of above
                {
                    if (i % 2 == 0) // even number
                    {
                        AddToCurve(maxTime, repeatCurveForward, newCurve, i);
                    }
                    else
                    {

                        AddToCurve(maxTime, repeatCurveBackward, newCurve, i);
                    }
                }
                OrderKeyFrames(ref newCurve);
                m_CurrentFixtureSliderData.m_AnimationCurveRepeated = newCurve;
                return m_CurrentFixtureSliderData.m_AnimationCurveRepeated;
            }
        }

        private void AddToCurve(float maxTime, AnimationCurve repeatCurve, AnimationCurve newCurve, int i)
        {
            for (int j = 0; j < repeatCurve.length; j++)
            {
                Keyframe raw = repeatCurve[j];
                Keyframe newKey = new Keyframe();
                newKey.time = raw.time / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber;
                newKey.time += i * (maxTime / m_CurrentFixtureSliderData.m_AnimationCurveRepeatNumber);
                newKey.value = raw.value;
                newCurve.AddKey(newKey);
            }
        }

        private void OrderKeyFrames(ref AnimationCurve curve)
        {
            List<Keyframe> keys = new List<Keyframe>();
            for (int c = 0; c < curve.length; c++)
            {
                keys.Add(curve[c]);
            }

            keys.Sort(delegate (Keyframe x, Keyframe y)
            {
                return x.time.CompareTo(y.time); // this sorts by time
            });

            foreach (var item in keys) // remove all keys 
            {
                curve.RemoveKey(0);
            }

            foreach (var item in keys) // add all keys back in, but in correct order
            {
                curve.AddKey(item);
            }
        }


        private void MoveKeyFrames(ref AnimationCurve curve, OscSequenceData currentSceneData)
        {
            if (currentSceneData != null)
            {
                int lastIndex = curve.length - 1;
                Keyframe lastKeyFrame = curve.keys[lastIndex];
                float orginalLastTime = lastKeyFrame.time;

                // move all 
                if (orginalLastTime != currentSceneData.m_DurationTime)
                {                  
                    float timeMuliplyer = currentSceneData.m_DurationTime / orginalLastTime;
                    // move all others
                    for (int i = 0; i < curve.length; i++)
                    {
                        if (i != 0 && i != lastIndex) // weird way to do this, but was having issues before 
                        {
                            Keyframe currentKeyFrame = curve.keys[i];
                            currentKeyFrame.time *= timeMuliplyer;
                            curve.MoveKey(i, currentKeyFrame);
                        }
                    }

                    lastKeyFrame.time = currentSceneData.m_DurationTime;
                    curve.MoveKey(lastIndex, lastKeyFrame);
                }
            }
        }

        private void LimitMovement(AnimationCurve curve, int i, ImageMouseMoveLimited newCurve, float maxTime)
        {
            if (i == 0)
            {
                newCurve.SetLocalValidArea(Vector3.zero, new Vector3(0, m_LineRendererSize.y, 0));
            }
            else if (i == (curve.length - 1))
            {
                newCurve.SetLocalValidArea(new Vector3(m_LineRendererSize.x, 0, 0), new Vector3(m_LineRendererSize.x, m_LineRendererSize.y, 0));
            }
            else
            {
                float xMin = (curve[i - 1].time / maxTime) + OFFSET;
                float xMax = (curve[i + 1].time / maxTime) - OFFSET;
                Vector3 min = new Vector3(xMin * m_LineRendererSize.x, 0, 0);
                Vector3 max = new Vector3(xMax * m_LineRendererSize.x, m_LineRendererSize.y, 0);
                newCurve.SetLocalValidArea(min, max);
            }
        }

        private void UpdataData(UILineRenderer line, AnimationCurve curve, float maxTime)
        {
            Vector2[] points = new Vector2[curve.length];
            for (int i = 0; i < curve.length; i++)
            {
                points[i] = new Vector2((curve[i].time / maxTime), (curve[i].value / OscControllerRoot.MAX_DMX_VALUE)); 
            }
            line.Points = points;
        }

    }
}
