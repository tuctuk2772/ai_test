using JetBrains.Annotations;
using System;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUI;

[CustomEditor(typeof(EnemyPerceptionSettings)), CanEditMultipleObjects]
public class EnemyPerceptionEditor : Editor
{
    private SerializedProperty sixthSense;
    private SerializedProperty immediateSense;
    private SerializedProperty sixthSenseVerticalOffset;
    private SerializedProperty sixthSenseHorizontal;
    private SerializedProperty sixthSenseVertical;
    private SerializedProperty sixthSenseAnglePercentage;

    private void OnEnable()
    {
        sixthSense = serializedObject.FindProperty("sixthSense");
        immediateSense = serializedObject.FindProperty("immediateSense");
        sixthSenseVerticalOffset = serializedObject.FindProperty("sixthSenseVerticalOffset");
        sixthSenseHorizontal = serializedObject.FindProperty("sixthSenseHorizontal");
        sixthSenseVertical = serializedObject.FindProperty("sixthSenseVertical");
        sixthSenseAnglePercentage = serializedObject.FindProperty("sixthSenseAnglePercentage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, "sixthSense", "immediateSense",
            "sixthSenseVerticalOffset", "sixthSenseHorizontal",
            "sixthSenseVertical", "sixthSenseAnglePercentage", "suspicionMeterMax");

        EditorGUILayout.PropertyField(sixthSense);

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(sixthSense.boolValue)))
        {
            if (group.visible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(sixthSenseVerticalOffset);
                EditorGUILayout.PropertyField(sixthSenseHorizontal);
                EditorGUILayout.PropertyField(sixthSenseVertical);
                EditorGUILayout.PropertyField(sixthSenseAnglePercentage);
                EditorGUILayout.PropertyField(immediateSense);
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}