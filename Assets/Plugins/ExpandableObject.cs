using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
#endif

public class ExpandableAttribute : PropertyAttribute
{
    public ExpandableAttribute() { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ExpandableAttribute), true)]
public class ExpandableAttributeDrawer : PropertyDrawer
{
    private enum BackgroundStyles { None, HelpBox, Darken, Lighten }
    private static readonly BackgroundStyles BackgroundStyle = BackgroundStyles.HelpBox;
    private static readonly Color DarkColour = new Color(0.0f, 0.0f, 0.0f, 0.2f);
    private static readonly Color LightColour = new Color(1.0f, 1.0f, 1.0f, 0.2f);

    private static readonly bool ShowScriptField = false;
    private const float InnerSpacing = 6.0f;
    private const float OuterSpacing = 4.0f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float totalHeight = EditorGUIUtility.singleLineHeight;

        if (property.objectReferenceValue == null)
        {
            return totalHeight;
        }

        if (property.isExpanded == false)
        {
            return totalHeight;
        }

        var targetObject = new SerializedObject(property.objectReferenceValue);
        if (targetObject == null)
        {
            return totalHeight;
        }

        var field = targetObject.GetIterator();
        field.NextVisible(true);

        if (ShowScriptField == true)
        {
            totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        while (field.NextVisible(true) == true)
        {
            totalHeight += EditorGUI.GetPropertyHeight(field, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        totalHeight += InnerSpacing * 2;
        totalHeight += OuterSpacing * 2;

        return totalHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect fieldRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };

        EditorGUI.PropertyField(fieldRect, property, label, true);
        if (property.objectReferenceValue == null)
        {
            return;
        }

        property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, GUIContent.none, true);
        if (property.isExpanded == false)
        {
            return;
        }

        var targetObject = new SerializedObject(property.objectReferenceValue);
        if (targetObject == null)
        {
            return;
        }

        var propertyRects = new List<Rect>();
        var marchingRect = new Rect(fieldRect);

        var bodyRect = new Rect(fieldRect);
        bodyRect.xMin += EditorGUI.indentLevel * 18;
        bodyRect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
            + OuterSpacing;

        SerializedProperty field = targetObject.GetIterator();
        field.NextVisible(true);

        marchingRect.y += InnerSpacing + OuterSpacing;

        if (ShowScriptField == true)
        {
            propertyRects.Add(marchingRect);
            marchingRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        while (field.NextVisible(true))
        {
            marchingRect.y += marchingRect.height + EditorGUIUtility.standardVerticalSpacing;
            marchingRect.height = EditorGUI.GetPropertyHeight(field, true);
            propertyRects.Add(marchingRect);
        }

        marchingRect.y += InnerSpacing;

        bodyRect.yMax = marchingRect.yMax;

        DrawBackground(bodyRect);

        EditorGUI.indentLevel++;
        {
            int index = 0;
            field = targetObject.GetIterator();
            field.NextVisible(true);

            if (ShowScriptField == true)
            {
                //Show the disabled script field
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(propertyRects[index], field, true);
                EditorGUI.EndDisabledGroup();
                index++;
            }

            //Replacement for "editor.OnInspectorGUI ();" so we have more control on how we draw the editor
            while (field.NextVisible(true))
            {
                try
                {
                    EditorGUI.PropertyField(propertyRects[index], field, true);
                }
                catch (StackOverflowException)
                {
                    field.objectReferenceValue = null;
                    Debug.LogError("Detected self-nesting cauisng a StackOverflowException, avoid using the same " +
                        "object iside a nested structure.");
                }

                index++;
            }
        }
        targetObject.ApplyModifiedProperties();
        EditorGUI.indentLevel--;
    }

    private void DrawBackground(Rect rect)
    {
        switch (BackgroundStyle)
        {
            case BackgroundStyles.HelpBox:
                EditorGUI.HelpBox(rect, "", MessageType.None);
                break;

            case BackgroundStyles.Darken:
                EditorGUI.DrawRect(rect, DarkColour);
                break;

            case BackgroundStyles.Lighten:
                EditorGUI.DrawRect(rect, LightColour);
                break;
        }
    }
}
#endif