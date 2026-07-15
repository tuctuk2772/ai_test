using Unity.Behavior;
using UnityEngine;

public class DetectionVisual : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent enemy;
    [SerializeField] private Material assignedMaterial;

    Material uniqueMat;

    [SerializeField] BlackboardVariable<float> currentFill;

    private void Start()
    {
        uniqueMat = new Material(assignedMaterial);
        
        gameObject.GetComponent<MeshRenderer>().material = uniqueMat;
    }

    private void Update()
    {
        enemy.GetVariable<float>("SuspicionMeterVisual", out currentFill);

        uniqueMat.SetFloat("_Fill", currentFill.Value);
    }
}
