using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(VisualizePatrol))]
public class PatrolPointsEditor : Editor
{
    private ReorderableList list;

    private SerializedProperty enemyReference;
    private SerializedProperty patrolType;

    private void OnEnable()
    {
        enemyReference = serializedObject.FindProperty("enemy");
        patrolType = serializedObject.FindProperty("patrolPathType");

        SerializedProperty listProperty = serializedObject.FindProperty("patrolPoints");
        list = new ReorderableList(serializedObject, listProperty, true, true, true, true);

        if (list.count <= 0)
        {
            VisualizePatrol patrol = (VisualizePatrol)target;

            patrol.SpawnFreeformPatrolPoint();

            serializedObject.ApplyModifiedProperties();
        }

        list.onAddCallback += (ReorderableList tempList) =>
        {
            VisualizePatrol patrol = (VisualizePatrol)target;

            patrol.SpawnFreeformPatrolPoint();

            serializedObject.ApplyModifiedProperties();
        };

        list.onRemoveCallback += (ReorderableList tempList) =>
        {
            VisualizePatrol patrol = (VisualizePatrol)target;

            if(list.count > 1)
            {
                patrol.DestroyLastPatrolPoint();
            }

            serializedObject.ApplyModifiedProperties();
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
        EditorGUILayout.PropertyField(patrolType);

        EditorGUILayout.Space();

        PatrolPathType currentType = (PatrolPathType)patrolType.intValue;

        if(currentType == PatrolPathType.Freeform)
        {
            list.DoLayoutList();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
