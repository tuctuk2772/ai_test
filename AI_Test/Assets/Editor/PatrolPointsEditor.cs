using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(VisualizePatrol))]
public class PatrolPointsEditor : Editor
{
    private ReorderableList list;
    private SerializedProperty enemyReference;

    private void OnEnable()
    {
        enemyReference = serializedObject.FindProperty("enemy");

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

        list.drawHeaderCallback += (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Patrol Points");
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(enemyReference);
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
