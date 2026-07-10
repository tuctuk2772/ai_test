using UnityEngine;

[CreateAssetMenu(fileName = "EnemyPerceptionSettings", menuName = "Behavior/EnemyPerceptionSettings")]
public class EnemyPerceptionSettings : ScriptableObject
{
    [Header("Get Spotted")]
    [SerializeField, Range(0, 2)] public float getSpottedVerticalOffset = 1f;
    [SerializeField, Range(0, 30)] public float getSpottedVerticalMaxDistance = 7f;
    [SerializeField, Range(0, 10)] public float getSpottedHorizontalMaxDistance = 2f;
    [SerializeField, Range(0, 1)] public float getSpottedHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] public float getSpottedVerticalMaxPercentage = 0.66f;

    [Header("Get Curious")]
    [SerializeField, Range(0, 2)] public float getCuriousVerticalOffset = 0f;
    [SerializeField, Range(5, 50)] public float getCuriousVerticalMaxDistance = 15f;
    [SerializeField, Range(2, 15)] public float getCuriousHorizontalMaxDistance = 5f;
    [SerializeField, Range(0, 1)] public float getCuriousHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] public float getCuriousVerticalMaxPercentage = 0.66f;

    [Header("Sixth Sense")]
    [SerializeField] public bool sixthSense = true;
    [SerializeField] public bool immediateSense = false;
    [SerializeField, Range(0, 2)] public float sixthSenseVerticalOffset = 0.25f;
    [SerializeField, Range(0, 5)] public float sixthSenseHorizontal = 2f;
    [SerializeField, Range(0, 5)] public float sixthSenseVertical = 1f;
    [SerializeField, Range(0, 1)] public float sixthSenseAnglePercentage = 0.75f;

    [Header("")]

    [HideInInspector] public Vector3[] getCuriousCoordinates = new Vector3[3];
    [HideInInspector] public Vector3[] getSpottedCoordinates = new Vector3[3];
    [HideInInspector] public Vector3[] sixthSenseCoordinates = new Vector3[3];

    private void OnValidate()
    {
        BuildCoordinates();
    }

    private void BuildCoordinates()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 localSpottedOffset = Vector3.zero;
            Vector3 localCuriousOffset = Vector3.zero;
            Vector3 localSixthOffset = Vector3.zero;

            switch (i)
            {
                case 0:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVerticalOffset);
                    break;
                case 1:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance, 0f, getSpottedVerticalMaxDistance * getSpottedHorizontalMaxPercentage + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance, 0f, getCuriousVerticalMaxDistance * getCuriousHorizontalMaxPercentage + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
                case 2:
                    localSpottedOffset = new Vector3(getSpottedHorizontalMaxDistance * getSpottedVerticalMaxPercentage, 0f, getSpottedVerticalMaxDistance + getSpottedVerticalOffset);
                    localCuriousOffset = new Vector3(getCuriousHorizontalMaxDistance * getCuriousVerticalMaxPercentage, 0f, getCuriousVerticalMaxDistance + getCuriousVerticalOffset);
                    localSixthOffset = new Vector3(sixthSenseHorizontal * sixthSenseAnglePercentage, 0f, -sixthSenseVertical - sixthSenseVerticalOffset);
                    break;
                default:
                    Debug.LogError("Something went wrong while building");
                    break;
            }

            Vector3 inverseSpottedOffset = new Vector3(-localSpottedOffset.x, 0f, localSpottedOffset.z);
            Vector3 inverseCuriousOffset = new Vector3(-localCuriousOffset.x, 0f, localCuriousOffset.z);
            Vector3 inverseSixthOffset = new Vector3(-localSixthOffset.x, 0f, localSixthOffset.z);

            getSpottedCoordinates[i] = localSpottedOffset;
            getCuriousCoordinates[i] = localCuriousOffset;
            sixthSenseCoordinates[i] = localSixthOffset;
        }
    }
}
