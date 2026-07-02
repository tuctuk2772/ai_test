using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum PatrolPathType
{
    Freeform, Rectangle
}

public class VisualizePatrol : MonoBehaviour
{
    [HideInInspector] public List<GameObject> patrolPoints = new();

    [SerializeField] private PatrolPathType patrolPathType;
    [SerializeField] private GameObject enemy;

    public void SpawnFreeformPatrolPoint()
    {
        GameObject patrolPoint = new GameObject();
        patrolPoints.Add(patrolPoint);
        patrolPoint.name = $"PatrolPoint_{patrolPoints.Count-1}";
        patrolPoint.transform.parent = transform;
    }

    public void DestroyLastPatrolPoint()
    {
        GameObject destroyObject = patrolPoints[patrolPoints.Count - 1];

        patrolPoints.Remove(destroyObject);
#if UNITY_EDITOR
        DestroyImmediate(destroyObject);
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (i == 0 && !Application.isPlaying)
            {
                enemy.transform.position = patrolPoints[i].transform.position;
            }

            Gizmos.DrawWireSphere(patrolPoints[i].transform.position, 0.5f);
            Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[(i + 1) % patrolPoints.Count].transform.position);
        }
    }
}
#endif
