using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WatchPlayer)), CanEditMultipleObjects]
public class EnemyVisualEditor : Editor
{
    private const float suspicionMin = 0f;
    private const float suspicionMax = 10f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if(prop.name == "enemyPerceptionSettings")
            {
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                continue;
            }

            if (prop.name == "suspicionMeter")
                continue;

            using (new EditorGUI.DisabledScope(prop.name == "m_Script"))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        EditorGUILayout.Space();
        DrawSuspicionMeterSlider();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSuspicionMeterSlider()
    {
        SerializedProperty suspicionProp = serializedObject.FindProperty("suspicionMeter");
        Vector2 value = suspicionProp.vector2Value;

        float min = value.x;
        float max = value.y;

        EditorGUILayout.LabelField("Suspicion Meter", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{min:F1}", GUILayout.Width(70));
        EditorGUILayout.MinMaxSlider(ref min, ref max, suspicionMin, suspicionMax);
        EditorGUILayout.LabelField($"{max:F1}", GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        Vector2 newValue = new Vector2(min, max);
        if (newValue != value)
        {
            suspicionProp.vector2Value = newValue;
        }
    }
}