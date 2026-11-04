#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ArcCanvas.EditorTools
{
    [CustomEditor(typeof(ArcCanvasSettings))]
    public class ArcCanvasSettingsEditor : UnityEditor.Editor
    {
        SerializedProperty _shape, _shapeParams, _quality, _curveBlend, _autoInject, _autoWorldSpace, _simpleMode, _curveAmount;
        bool _advancedFoldout;

        void OnEnable()
        {
            _shape = serializedObject.FindProperty("shape");
            _shapeParams = serializedObject.FindProperty("shapeParams");
            _quality = serializedObject.FindProperty("quality");
            _curveBlend = serializedObject.FindProperty("curveBlend");
            _autoInject = serializedObject.FindProperty("autoInjectChildren");
            _autoWorldSpace = serializedObject.FindProperty("autoForceWorldSpace");
            _simpleMode = serializedObject.FindProperty("simpleMode");
            _curveAmount = serializedObject.FindProperty("curveAmount");
        }

        public override void OnInspectorGUI()
        {
            var settings = (ArcCanvasSettings)target;
            serializedObject.Update();

            if (settings.IsWorldSpaceIssue && !settings.autoForceWorldSpace)
            {
                EditorGUILayout.HelpBox(
                    "This Canvas is not World Space. The curve is invisible without " +
                    "camera perspective. Enable 'Auto Force World Space' in Advanced below.",
                    MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();

            // --- Shape picker: always visible, it's the one decision everyone makes first ---
            EditorGUILayout.PropertyField(_shape, new GUIContent("Shape"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_simpleMode, new GUIContent("Simple Mode"));

            if (_simpleMode.boolValue)
            {
                // --- THE one slider ---
                EditorGUILayout.Slider(_curveAmount, 0f, 1f, new GUIContent("Curve Amount"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Subtle")) settings.ApplyPreset_SubtleCurve();
                    if (GUILayout.Button("Wrap Around")) settings.ApplyPreset_WrapAroundViewer();
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Curved Monitor")) settings.ApplyPreset_CurvedMonitor();
                    if (GUILayout.Button("Space Helmet")) settings.ApplyPreset_SpaceHelmet();
                }
                if (GUILayout.Button("Flatten")) settings.ApplyPreset_Flat();
            }
            else
            {
                DrawAdvancedShapeFields((ArcShape)_shape.enumValueIndex);

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_curveBlend, new GUIContent("Curve Blend (flat <-> curved)"));
            }

            EditorGUILayout.Space();
            _advancedFoldout = EditorGUILayout.Foldout(_advancedFoldout, "Advanced", true);
            if (_advancedFoldout)
            {
                EditorGUI.indentLevel++;

                if (_simpleMode.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "These are driven automatically by Curve Amount while Simple Mode is " +
                        "on. Turn Simple Mode off above to set them directly.", MessageType.None);
                    using (new EditorGUI.DisabledScope(true))
                        DrawAdvancedShapeFields((ArcShape)_shape.enumValueIndex);
                }

                EditorGUILayout.PropertyField(_quality, new GUIContent("Quality (subdivisions)"));

                EditorGUILayout.Space();
                var useCustomDepthProp = _shapeParams.FindPropertyRelative("useCustomDepth");
                EditorGUILayout.PropertyField(useCustomDepthProp, new GUIContent("Use Custom Depth (optional)"));
                if (useCustomDepthProp.boolValue)
                    DrawChild("depthMultiplier", "Depth Multiplier");

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_autoInject, new GUIContent("Auto-Inject Children"));
                EditorGUILayout.PropertyField(_autoWorldSpace, new GUIContent("Auto Force World Space"));

                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                settings.NotifyChanged();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Child Elements")) settings.RefreshChildEffects();
                if (GUILayout.Button("Force Rebuild Now")) settings.NotifyChanged();
            }
        }

        void DrawAdvancedShapeFields(ArcShape shape)
        {
            if (ArcCanvasShapeFieldMap.UsesRadius(shape))
                DrawChild("radius", "Radius");

            DrawChild("curveDirection", "Curve Direction (concave / convex)");

            if (ArcCanvasShapeFieldMap.UsesWave(shape))
            {
                DrawChild("waveAmplitude", "Wave Amplitude");
                DrawChild("waveFrequency", "Wave Frequency");
            }
            if (ArcCanvasShapeFieldMap.UsesConeTaper(shape))
            {
                DrawChild("coneTaper", "Cone Taper (0=cylinder, 1=point)");
            }
            if (ArcCanvasShapeFieldMap.UsesSpiral(shape))
            {
                DrawChild("spiralTurns", "Spiral Turns");
                DrawChild("spiralHeightPerTurn", "Spiral Height Per Turn");
            }
            if (ArcCanvasShapeFieldMap.UsesEllipse(shape))
            {
                DrawChild("ellipseRadiusX", "Ellipse Radius X");
                DrawChild("ellipseRadiusZ", "Ellipse Radius Z");
            }
        }

        void DrawChild(string relativeName, string label)
        {
            var prop = _shapeParams.FindPropertyRelative(relativeName);
            if (prop != null) EditorGUILayout.PropertyField(prop, new GUIContent(label));
        }
    }
}
#endif
