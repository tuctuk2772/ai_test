using UnityEngine;
using UnityEditor;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Unity.Collections.LowLevel.Unsafe;

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

            if (prop.name == "enemyPerceptionSettings")
            {
                EditorGUILayout.PropertyField(prop, GUIContent.none);
                continue;
            }

            if (prop.name == "suspicionMeter")
                continue;

            if (prop.name == "newSuspicionMeter")
                continue;

            using (new EditorGUI.DisabledScope(prop.name == "m_Script"))
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        EditorGUILayout.Space();
        //DrawSuspicionMeterSlider();
        DrawNewSuspicionMeterSlider();

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

    private void DrawNewSuspicionMeterSlider()
    {
        SerializedProperty suspicionProp = serializedObject.FindProperty("newSuspicionMeter");

        EditorGUILayout.LabelField("Suspicion Meter", EditorStyles.boldLabel);
        Vector3 value = suspicionProp.vector3Value;

        float min = value.x, max = value.y, threshold = value.z;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"{min:F1}", GUILayout.Width(40));
        Rect sliderRect = GUILayoutUtility.GetRect(50, EditorGUIUtility.singleLineHeight, GUILayout.ExpandWidth(true));

        GUIStyle rightAlign = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight,
            contentOffset = new Vector2(-10f, 0f)
        };

        EditorGUILayout.LabelField($"{max:F1}", rightAlign, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        DrawTripleHandleSlider(sliderRect, ref min, ref max, ref threshold, suspicionMin, suspicionMax);

        Vector3 newValue = new Vector3(min, max, threshold);
        if (newValue != value)
        {
            suspicionProp.vector3Value = newValue;
        }
    }

    private static void DrawTripleHandleSlider(Rect rect, ref float min, ref float max, ref float threshold, float limitMin, float limitMax)
    {
        int minId = GUIUtility.GetControlID(FocusType.Passive);
        int maxId = GUIUtility.GetControlID(FocusType.Passive);
        int thresholdId = GUIUtility.GetControlID(FocusType.Passive);

        const float trackHeight = 2f;
        const float handleWidth = 10f;
        const float handleHeight = 10f;

        float trackY = rect.yMax - handleHeight * 0.5f - 2f;
        Rect trackRect = new Rect(rect.x, trackY - trackHeight * 0.5f, rect.width, trackHeight);

        float ValueToX(float v) => Mathf.Lerp(rect.x, rect.xMax, Mathf.InverseLerp(limitMin, limitMax, v));
        float XToValue(float x) => Mathf.Clamp(Mathf.Lerp(limitMin, limitMax, Mathf.InverseLerp(rect.x, rect.xMax, x)), limitMin, limitMax);

        float minX = ValueToX(min);
        float maxX = ValueToX(max);
        float thresholdX = ValueToX(threshold);

        Rect minHandleRect = new Rect(minX - handleWidth * 0.5f, trackY - handleHeight * 0.5f, handleWidth, handleHeight);
        Rect maxHandleRect = new Rect(maxX - handleWidth * 0.5f, trackY - handleHeight * 0.5f, handleWidth, handleHeight);
        Rect threshHandleRect = new Rect(thresholdX - handleWidth * 0.5f, trackY - handleHeight * 0.5f, handleWidth, handleHeight);

        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(trackRect, Color.gray4);

            Rect selectedRect = new Rect(minX + 1f, trackY - handleHeight * 0.5f, Mathf.Max(0f, thresholdX - minX), handleHeight);
            EditorGUI.DrawRect(selectedRect, Color.gray6);

            Rect curiousRect = new Rect(thresholdX, trackY - handleHeight * 0.5f, Mathf.Max(0f, maxX - thresholdX - 1f), handleHeight);
            EditorGUI.DrawRect(curiousRect, Color.yellow);

            Handles.color = Color.gray6;
            Handles.DrawSolidArc(new Vector3(minHandleRect.x + handleWidth * 0.5f, trackY), Vector3.forward, Vector2.up, 180f, handleHeight * 0.5f);

            Handles.color = Color.yellow;
            Handles.DrawSolidArc(new Vector3(threshHandleRect.x + handleWidth * 0.5f, trackY), Vector3.forward, Vector2.up, 360f, handleHeight * 0.5f);

            Handles.color = Color.gray1;
            Handles.DrawWireArc(new Vector3(threshHandleRect.x + handleWidth * 0.5f, trackY), Vector3.forward, Vector2.up, 360f, handleHeight * 0.5f);

            Rect label = new Rect(new Vector2(curiousRect.x - handleWidth, curiousRect.y - handleHeight * 2f), threshHandleRect.size * 2f);
            EditorGUI.LabelField(label, $"{threshold:F1}");

            Handles.color = Color.yellow;
            Handles.DrawSolidArc(new Vector3(maxHandleRect.x + handleWidth * 0.5f, trackY), Vector3.forward, Vector2.down, 180f, handleHeight * 0.5f);
        }

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
                if (threshHandleRect.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = thresholdId;
                    e.Use();
                }
                else if (minHandleRect.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = minId;
                    e.Use();
                }
                else if (maxHandleRect.Contains(e.mousePosition))
                {
                    GUIUtility.hotControl = maxId;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == minId)
                {
                    min = Mathf.Clamp(XToValue(e.mousePosition.x), limitMin, max);
                    threshold = Mathf.Clamp(threshold, min, max);
                    GUI.changed = true;
                    e.Use();
                }
                else if (GUIUtility.hotControl == maxId)
                {
                    max = Mathf.Clamp(XToValue(e.mousePosition.x), min, limitMax);
                    threshold = Mathf.Clamp(threshold, min, max);
                    GUI.changed = true;
                    e.Use();
                }
                else if (GUIUtility.hotControl == thresholdId)
                {
                    threshold = Mathf.Clamp(XToValue(e.mousePosition.x), min, max);
                    GUI.changed = true;
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == minId || GUIUtility.hotControl == maxId || GUIUtility.hotControl == thresholdId)
                {
                    GUIUtility.hotControl = 0;
                    e.Use();
                }
                break;
        }
    }
}