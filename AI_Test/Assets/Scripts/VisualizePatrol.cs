using System.Collections.Generic;
using UnityEngine;

public class VisualizePatrol : MonoBehaviour
{
    [HideInInspector] public List<GameObject> patrolPoints = new();
    [SerializeField] private GameObject enemy;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        patrolPoints.Clear();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("PatrolP"))
            {
                patrolPoints.Add(child.gameObject);
            }
        }

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (i == 0 && !Application.isPlaying)
            {
                enemy.transform.position = patrolPoints[i].transform.position;
            }

            Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[(i + 1) % patrolPoints.Count].transform.position);
        }
    }
}
#endif
