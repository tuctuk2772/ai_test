using Unity.Behavior;
using UnityEngine;

public class DetectionVisual : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent enemy;
    [SerializeField] private Material assignedMaterial;

    private Material uniqueMat;
    BlackboardVariable<float> currentFill;

    private void Start()
    {
        uniqueMat = new Material(assignedMaterial);

        gameObject.GetComponent<MeshRenderer>().material = uniqueMat;

        enemy.GetVariable<float>("SuspicionMeterVisual", out currentFill);
    }

    private void Update()
    {
        uniqueMat.SetFloat("_Fill", currentFill.Value);
    }
}
