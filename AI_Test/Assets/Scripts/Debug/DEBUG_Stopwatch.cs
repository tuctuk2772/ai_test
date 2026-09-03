using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Behavior;

public class DEBUG_Stopwatch : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private BehaviorGraphAgent enemy;

    private BlackboardVariable<float> currentSuspicion;

    float currentSuspicionValue;
    private float currentTime = 0f;

    private void Start()
    {
        enemy.GetVariable<float>("CurrentSuspicionMeter", out currentSuspicion);
    }

    private void Update()
    {
        if(currentSuspicion.Value == 0)
        {
            return;
        }

        currentTime += Time.deltaTime;

        text.text = currentTime.ToString();
    }
}
