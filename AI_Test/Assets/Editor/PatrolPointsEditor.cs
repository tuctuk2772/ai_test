using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(VisualizePatrol))]
public class PatrolPointsEditor : Editor
{
    private ReorderableList list;

    private SerializedProperty enemyReference;
    private SerializedProperty patrolType;
    private SerializedProperty rectangleDimensions;

    private void OnEnable()
    {
        enemyReference = serializedObject.FindProperty("enemy");
        patrolType = serializedObject.FindProperty("patrolPathType");
        rectangleDimensions = serializedObject.FindProperty("rectangleDimensions");

        PatrolPathType currentType = (PatrolPathType)patrolType.intValue;


        SerializedProperty listProperty = serializedObject.FindProperty("patrolPoints");
        list = new ReorderableList(serializedObject, listProperty, true, true, true, true);

        if (list.count <= 0 && currentType == PatrolPathType.Freeform)
        {
            VisualizePatrol patrol = (VisualizePatrol)target;
            patrol.SpawnFreeformPatrolPoint();
            serializedObject.Update();
        }

        list.onAddCallback += (ReorderableList tempList) =>
        {
            VisualizePatrol patrol = (VisualizePatrol)target;

            patrol.SpawnFreeformPatrolPoint();

            serializedObject.Update();
        };

        list.onRemoveCallback += (ReorderableList tempList) =>
        {
            VisualizePatrol patrol = (VisualizePatrol)target;

            if (list.count > 1)
            {
                patrol.DestroyLastPatrolPoint();
            }

            serializedObject.Update();
        };

        list.drawElementCallback = DrawElementCallback;
        list.draggable = false;

        list.drawHeaderCallback += (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Patrol Points");
        };
    }

    private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);

        rect.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(rect, element, GUIContent.none);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(enemyReference);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(patrolType);
        bool patrolTypeChanged = EditorGUI.EndChangeCheck();

        EditorGUILayout.Space();

        PatrolPathType currentType = (PatrolPathType)patrolType.intValue;

        if (patrolTypeChanged)
        {
            serializedObject.ApplyModifiedProperties();
            VisualizePatrol patrol = (VisualizePatrol)target;

            if (currentType == PatrolPathType.Rectangle)
            {
                patrol.GenerateRectanglePoints();
            }
            else
            {
                patrol.ResetToFreeform();
            }
            serializedObject.Update();
        }

        switch (currentType)
        {
            case PatrolPathType.Freeform:
                list.DoLayoutList();
                break;

            case PatrolPathType.Rectangle:
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Rectangle Dimensions", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(rectangleDimensions, GUIContent.none);
                bool dimensionsChanged = EditorGUI.EndChangeCheck();

                EditorGUILayout.EndVertical();

                if (dimensionsChanged)
                {
                    serializedObject.ApplyModifiedProperties();
                    VisualizePatrol patrol = (VisualizePatrol)target;
                    patrol.GenerateRectanglePoints();
                    serializedObject.Update();
                }
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
