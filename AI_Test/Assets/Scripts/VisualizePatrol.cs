using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public enum PatrolPathType
{
    Freeform, Rectangle
}

public class VisualizePatrol : MonoBehaviour
{
    public List<GameObject> patrolPoints = new();

    [SerializeField] private PatrolPathType patrolPathType;
    [SerializeField] private GameObject enemy;

    [SerializeField] private Vector2 rectangleDimensions;

    public void SpawnFreeformPatrolPoint()
    {
        GameObject patrolPoint = new GameObject();
        patrolPoints.Add(patrolPoint);
        patrolPoint.name = $"PatrolPoint_{patrolPoints.Count - 1}";
        patrolPoint.transform.parent = transform;
        patrolPoint.transform.localPosition = (patrolPoints.Count - 1) * Vector3.forward;
    }

    public void DestroyLastPatrolPoint()
    {
        GameObject destroyObject = patrolPoints[patrolPoints.Count - 1];

        patrolPoints.Remove(destroyObject);
#if UNITY_EDITOR
        DestroyImmediate(destroyObject);
#endif
    }

    public void GenerateRectanglePoints()
    {
        foreach (GameObject patrol in patrolPoints)
        {
#if UNITY_EDITOR
            DestroyImmediate(patrol);
#endif
        }

        patrolPoints.Clear();

        SpawnRectanglePoint(0, new Vector2(0, 0));
        SpawnRectanglePoint(1, new Vector2(rectangleDimensions.x, 0));
        SpawnRectanglePoint(2, rectangleDimensions);
        SpawnRectanglePoint(3, new Vector2(0, rectangleDimensions.y));
    }

    public void ResetToFreeform()
    {
        foreach (GameObject patrol in patrolPoints)
        {
#if UNITY_EDITOR
            DestroyImmediate(patrol);
#endif
        }
        patrolPoints.Clear();

        GameObject patrolPoint = new GameObject("PatrolPoint_0");
        patrolPoint.transform.parent = transform;
        patrolPoint.transform.localPosition = Vector3.zero;
        patrolPoints.Add(patrolPoint);
    }

    private void SpawnRectanglePoint(int i, Vector2 dimensions)
    {
        GameObject patrolPoint = new GameObject();
        patrolPoints.Add(patrolPoint);
        patrolPoint.transform.parent = transform;
        patrolPoint.transform.localPosition = new Vector3(dimensions.x, 0f, dimensions.y);
        patrolPoint.name = $"PatrolPoint_{i}";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (patrolPoints.Count < 0 || enemy == null)
        {
            return;
        }

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
#endif
}
