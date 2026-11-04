using UnityEngine;
using UnityEngine.UI;

namespace ArcCanvas
{
    /// <summary>
    /// Auto-built runtime control panel. Defaults to Simple view: a shape
    /// dropdown, ONE "Curve Amount" slider, and preset buttons - matching the
    /// familiar single-slider curved-canvas workflow. An "Advanced" toggle
    /// reveals per-shape fields, quality, curve direction, and the optional
    /// depth multiplier for anyone who wants full control.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("ArcCanvas/Arc Canvas Runtime Controller")]
    public class ArcCanvasRuntimeController : MonoBehaviour
    {
        public ArcCanvasSettings settings;
        public float minRadius = 200f, maxRadius = 2000f;

        Transform _root;
        Transform _advancedContainer;
        Text _advancedButtonLabel;
        bool _advancedVisible;
        ArcShape _lastBuiltShape;

        void Start()
        {
            if (settings == null) settings = GetComponentInParent<ArcCanvasSettings>();
            if (settings != null) BuildUI();
        }

        void Update()
        {
            if (settings != null && _advancedVisible && settings.shape != _lastBuiltShape)
                RebuildAdvancedFields();
        }

        void BuildUI()
        {
            var panel = new GameObject("ArcControlPanel", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(Image));
            panel.transform.SetParent(transform, false);
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 260);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 6;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            _root = panel.transform;

            AddDropdown(_root, "Shape", System.Enum.GetNames(typeof(ArcShape)), (int)settings.shape, i =>
            {
                settings.shape = (ArcShape)i;
                if (_advancedVisible) RebuildAdvancedFields();
            });

            AddSlider(_root, "Curve Amount", 0f, 1f, settings.curveAmount, v => settings.curveAmount = v);

            AddPresetButtonsRow(_root);

            var advBtnGO = new GameObject("AdvancedToggle", typeof(RectTransform), typeof(Image), typeof(Button));
            advBtnGO.transform.SetParent(_root, false);
            advBtnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 32);
            advBtnGO.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
            var advLabelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            advLabelGO.transform.SetParent(advBtnGO.transform, false);
            _advancedButtonLabel = advLabelGO.GetComponent<Text>();
            _advancedButtonLabel.text = "Show Advanced Controls";
            _advancedButtonLabel.alignment = TextAnchor.MiddleCenter;
            _advancedButtonLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _advancedButtonLabel.color = Color.white;
            var advLabelRt = advLabelGO.GetComponent<RectTransform>();
            advLabelRt.anchorMin = Vector2.zero; advLabelRt.anchorMax = Vector2.one; advLabelRt.offsetMin = Vector2.zero; advLabelRt.offsetMax = Vector2.zero;
            advBtnGO.GetComponent<Button>().onClick.AddListener(ToggleAdvanced);

            var advContainerGO = new GameObject("AdvancedFields", typeof(RectTransform), typeof(VerticalLayoutGroup));
            advContainerGO.transform.SetParent(_root, false);
            var advLayout = advContainerGO.GetComponent<VerticalLayoutGroup>();
            advLayout.spacing = 6;
            advLayout.childControlWidth = true;
            advLayout.childControlHeight = true;
            _advancedContainer = advContainerGO.transform;
            advContainerGO.SetActive(false);
        }

        void ToggleAdvanced()
        {
            _advancedVisible = !_advancedVisible;
            _advancedContainer.gameObject.SetActive(_advancedVisible);
            _advancedButtonLabel.text = _advancedVisible ? "Hide Advanced Controls" : "Show Advanced Controls";

            if (_advancedVisible)
            {
                settings.simpleMode = false;
                RebuildAdvancedFields();
            }
            else
            {
                settings.simpleMode = true;
            }
        }

        void AddPresetButtonsRow(Transform parent)
        {
            var row1 = new GameObject("PresetsRow1", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row1.transform.SetParent(parent, false);
            row1.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            AddButton(row1.transform, "Subtle", settings.ApplyPreset_SubtleCurve);
            AddButton(row1.transform, "Wrap", settings.ApplyPreset_WrapAroundViewer);
            AddButton(row1.transform, "Monitor", settings.ApplyPreset_CurvedMonitor);

            var row2 = new GameObject("PresetsRow2", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row2.transform.SetParent(parent, false);
            row2.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            AddButton(row2.transform, "Helmet", settings.ApplyPreset_SpaceHelmet);
            AddButton(row2.transform, "Flat", settings.ApplyPreset_Flat);
        }

        void AddButton(Transform parent, string label, System.Action onClick)
        {
            var go = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 32);
            go.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.8f, 0.9f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick());

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(go.transform, false);
            var text = labelGO.GetComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 12;
            text.color = Color.white;
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        }

        void RebuildAdvancedFields()
        {
            _lastBuiltShape = settings.shape;
            foreach (Transform child in _advancedContainer)
                Destroy(child.gameObject);

            var shape = settings.shape;

            AddDropdown(_advancedContainer, "Curve Direction", System.Enum.GetNames(typeof(ArcCurveDirection)), (int)settings.shapeParams.curveDirection, i =>
            { var p = settings.shapeParams; p.curveDirection = (ArcCurveDirection)i; settings.shapeParams = p; });

            if (ArcCanvasShapeFieldMap.UsesRadius(shape))
            {
                AddSlider(_advancedContainer, "Radius", minRadius, maxRadius, settings.shapeParams.radius, v =>
                { var p = settings.shapeParams; p.radius = v; settings.shapeParams = p; });
            }

            if (ArcCanvasShapeFieldMap.UsesWave(shape))
            {
                AddSlider(_advancedContainer, "Wave Amplitude", 0f, 200f, settings.shapeParams.waveAmplitude, v =>
                { var p = settings.shapeParams; p.waveAmplitude = v; settings.shapeParams = p; });
                AddSlider(_advancedContainer, "Wave Frequency", 0.1f, 8f, settings.shapeParams.waveFrequency, v =>
                { var p = settings.shapeParams; p.waveFrequency = v; settings.shapeParams = p; });
            }

            if (ArcCanvasShapeFieldMap.UsesConeTaper(shape))
            {
                AddSlider(_advancedContainer, "Cone Taper", 0f, 1f, settings.shapeParams.coneTaper, v =>
                { var p = settings.shapeParams; p.coneTaper = v; settings.shapeParams = p; });
            }

            if (ArcCanvasShapeFieldMap.UsesSpiral(shape))
            {
                AddSlider(_advancedContainer, "Spiral Turns", 0.25f, 5f, settings.shapeParams.spiralTurns, v =>
                { var p = settings.shapeParams; p.spiralTurns = v; settings.shapeParams = p; });
                AddSlider(_advancedContainer, "Spiral Height/Turn", 0f, 800f, settings.shapeParams.spiralHeightPerTurn, v =>
                { var p = settings.shapeParams; p.spiralHeightPerTurn = v; settings.shapeParams = p; });
            }

            if (ArcCanvasShapeFieldMap.UsesEllipse(shape))
            {
                AddSlider(_advancedContainer, "Ellipse Radius X", minRadius, maxRadius, settings.shapeParams.ellipseRadiusX, v =>
                { var p = settings.shapeParams; p.ellipseRadiusX = v; settings.shapeParams = p; });
                AddSlider(_advancedContainer, "Ellipse Radius Z", minRadius * 0.25f, maxRadius, settings.shapeParams.ellipseRadiusZ, v =>
                { var p = settings.shapeParams; p.ellipseRadiusZ = v; settings.shapeParams = p; });
            }

            AddSlider(_advancedContainer, "Quality (subdivisions)", 1, 32, settings.quality, v => settings.quality = Mathf.RoundToInt(v));

            AddDepthToggleSection(_advancedContainer);
        }

        void AddDepthToggleSection(Transform parent)
        {
            var row = new GameObject("DepthToggleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);

            var toggleGO = new GameObject("DepthToggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggleGO.transform.SetParent(row.transform, false);
            toggleGO.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            var depthToggle = toggleGO.GetComponent<Toggle>();
            depthToggle.isOn = settings.shapeParams.useCustomDepth;

            var checkmarkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmarkGO.transform.SetParent(toggleGO.transform, false);
            checkmarkGO.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.3f);
            var checkRt = checkmarkGO.GetComponent<RectTransform>();
            checkRt.anchorMin = Vector2.zero; checkRt.anchorMax = Vector2.one; checkRt.offsetMin = Vector2.zero; checkRt.offsetMax = Vector2.zero;
            depthToggle.graphic = checkmarkGO.GetComponent<Image>();

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(row.transform, false);
            var text = labelGO.GetComponent<Text>();
            text.text = "Use Custom Depth (optional)";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.fontSize = 13;

            var depthFieldsContainer = new GameObject("DepthFields", typeof(RectTransform), typeof(VerticalLayoutGroup));
            depthFieldsContainer.transform.SetParent(parent, false);

            RebuildDepthField(depthFieldsContainer.transform);
            depthToggle.onValueChanged.AddListener(isOn =>
            {
                var p = settings.shapeParams;
                p.useCustomDepth = isOn;
                settings.shapeParams = p;
                RebuildDepthField(depthFieldsContainer.transform);
            });
        }

        void RebuildDepthField(Transform container)
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
            if (!settings.shapeParams.useCustomDepth) return;
            AddSlider(container, "Depth Multiplier", 0f, 3f, settings.shapeParams.depthMultiplier, v =>
            { var p = settings.shapeParams; p.depthMultiplier = v; settings.shapeParams = p; });
        }

        void AddSlider(Transform parent, string label, float min, float max, float def, UnityEngine.Events.UnityAction<float> onChange)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform), typeof(VerticalLayoutGroup));
            row.transform.SetParent(parent, false);

            var text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(row.transform, false);
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.fontSize = 13;

            var sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGO.transform.SetParent(row.transform, false);
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 20);
            var slider = sliderGO.GetComponent<Slider>();

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGO.transform, false);
            bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one; bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.25f); fillAreaRt.anchorMax = new Vector2(1, 0.75f);
            fillAreaRt.offsetMin = new Vector2(5, 0); fillAreaRt.offsetMax = new Vector2(-5, 0);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            fill.GetComponent<Image>().color = new Color(0.2f, 0.7f, 1f, 1f);
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0, 1); fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGO.transform, false);
            var handleAreaRt = handleArea.GetComponent<RectTransform>();
            handleAreaRt.anchorMin = Vector2.zero; handleAreaRt.anchorMax = Vector2.one; handleAreaRt.offsetMin = Vector2.zero; handleAreaRt.offsetMax = Vector2.zero;

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            handle.GetComponent<Image>().color = Color.white;
            handle.GetComponent<RectTransform>().sizeDelta = new Vector2(14, 20);

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();

            slider.minValue = min; slider.maxValue = max; slider.value = def;
            slider.onValueChanged.AddListener(onChange);
        }

        void AddDropdown(Transform parent, string label, string[] options, int def, UnityEngine.Events.UnityAction<int> onChange)
        {
            var row = new GameObject(label + "DropdownRow", typeof(RectTransform), typeof(VerticalLayoutGroup));
            row.transform.SetParent(parent, false);

            var text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(row.transform, false);
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.fontSize = 13;

            var dropdownGO = new GameObject("Dropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
            dropdownGO.transform.SetParent(row.transform, false);
            dropdownGO.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 30);
            dropdownGO.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
            var dropdown = dropdownGO.GetComponent<Dropdown>();

            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(dropdownGO.transform, false);
            var dLabel = labelGO.GetComponent<Text>();
            dLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dLabel.color = Color.white;
            dLabel.alignment = TextAnchor.MiddleLeft;
            var dLabelRt = labelGO.GetComponent<RectTransform>();
            dLabelRt.anchorMin = Vector2.zero; dLabelRt.anchorMax = Vector2.one;
            dLabelRt.offsetMin = new Vector2(10, 0); dLabelRt.offsetMax = new Vector2(-10, 0);
            dropdown.captionText = dLabel;

            dropdown.options.Clear();
            foreach (var o in options) dropdown.options.Add(new Dropdown.OptionData(o));
            dropdown.value = def;
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(onChange);
        }
    }
}
