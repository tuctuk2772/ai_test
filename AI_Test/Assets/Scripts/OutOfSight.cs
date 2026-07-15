using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.Behavior;

public class OutOfSight : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private BehaviorGraphAgent enemy_ai;

    [SerializeField] private EnemyPerceptionSettings enemyPerceptionSettings;

    Vector3[] ogGetCuriousPoints, ogGetSpottedPoints;

    private void Start()
    {
        ogGetCuriousPoints = enemyPerceptionSettings.getCuriousCoordinates;
        ogGetSpottedPoints = enemyPerceptionSettings.getSpottedCoordinates;
    }

    private void Update()
    {
        animator.enabled = isVisible();
        Debug_AssignUpdatedCoordinates(isBehind() ? 1f : 2f);
    }

    private void Debug_AssignUpdatedCoordinates(float dividedFactor)
    {
        Vector3[] newGetCuriousPoints = new Vector3[3], newGetSpottedPoints = new Vector3[3];

        for (int i = 0; i < ogGetCuriousPoints.Length; i++)
        {
            newGetCuriousPoints[i] = ogGetCuriousPoints[i] / dividedFactor;
        }

        for (int i = 0; i < ogGetSpottedPoints.Length; i++)
        {
            newGetSpottedPoints[i] = ogGetSpottedPoints[i] / dividedFactor;
        }

        enemyPerceptionSettings.getCuriousCoordinates = newGetCuriousPoints;
        enemyPerceptionSettings.getSpottedCoordinates = newGetSpottedPoints;

        enemy_ai.SetVariableValue<List<Vector3>>("GetSpottedCoordinates", newGetSpottedPoints.ToList<Vector3>());
        enemy_ai.SetVariableValue<List<Vector3>>("GetCuriousCoordinates", newGetCuriousPoints.ToList<Vector3>());
    }

    private void OnDisable()
    {
        enemyPerceptionSettings.getCuriousCoordinates = ogGetCuriousPoints;
        enemyPerceptionSettings.getSpottedCoordinates = ogGetSpottedPoints;
    }

    private bool isVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        return planes.All(plane => plane.GetDistanceToPoint(transform.position) >= 0) 
            || Vector3.Distance(transform.position, mainCamera.transform.position) < 5f;
    }

    private bool isBehind()
    {
        Vector3 toObject = transform.position - mainCamera.transform.position;
        return Vector3.Dot(mainCamera.transform.forward, toObject) >= 0;
    }

    private void OnDrawGizmos()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Plane[] invertedPlanes = new Plane[6];

        for (int i = 0; i < planes.Length; i++)
        {
            invertedPlanes[i] = new Plane(-planes[i].normal, -planes[i].distance);
        }

        foreach (Plane plane in invertedPlanes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(plane.normal, Vector3.zero);
        }
    }
}
