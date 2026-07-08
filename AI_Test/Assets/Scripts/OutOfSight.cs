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

    private List<Vector3> getCuriousCoordinates, getSpottedCoordinates;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        animator.enabled = isBehind();
    }

    private bool isVisible()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        return planes.All(plane => plane.GetDistanceToPoint(transform.position)>=0);
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

        foreach(Plane plane in invertedPlanes)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(plane.normal, Vector3.zero);
        }
    }
}
