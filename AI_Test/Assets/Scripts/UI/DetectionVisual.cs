using Unity.Behavior;
using UnityEngine;

public class DetectionVisual : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent enemy;
    [SerializeField] private Material assignedMaterial;

    private Material uniqueMat;
    BlackboardVariable<float> currentFill;
    BlackboardVariable<Vector3> meter;

    private void Start()
    {
        uniqueMat = new Material(assignedMaterial);

        gameObject.GetComponent<MeshRenderer>().material = uniqueMat;

        enemy.GetVariable<float>("CurrentSuspicionMeter", out currentFill);
        enemy.GetVariable<Vector3>("SuspicionMeter", out meter);

        uniqueMat.SetVector("_SuspicionMeter", meter.Value);
    }

    private void Update()
    {
        //it takes a couple of frames for the suspicion meter to be accurate
        if(uniqueMat.GetVector("_SuspicionMeter") == Vector4.zero)
        {
            uniqueMat.SetVector("_SuspicionMeter", meter.Value);
        }

        uniqueMat.SetFloat("_Fill", currentFill.Value);
    }
}
