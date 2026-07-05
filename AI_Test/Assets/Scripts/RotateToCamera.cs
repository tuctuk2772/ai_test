using UnityEngine;

public class RotateToCamera : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public void Update()
    {
        Vector3 v = mainCamera.transform.position - transform.position;
        v.x = v.z = 0.0f;

        transform.LookAt(mainCamera.transform.position - v);
    }
}
