using TMPro;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.UI;

public class DEBUG_UpdateDetection : MonoBehaviour
{
    [SerializeField] private BehaviorGraphAgent enemy;

    private TextMeshPro currentText;
    BlackboardVariable<float> currentFill;

    private void Start()
    {
        enemy.GetVariable<float>("SuspicionMeterVisual", out currentFill);
        currentText = transform.GetComponent<TextMeshPro>();

        if (currentText == null) Debug.LogError("Text not found!");
    }

    private void Update()
    {
        if (currentText == null) return;

        currentText.text = (Mathf.Round(currentFill.Value * 100f) / 100f).ToString();
    }
}
