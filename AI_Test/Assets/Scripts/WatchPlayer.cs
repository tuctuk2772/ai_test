using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.UIElements;

public class WatchPlayer : MonoBehaviour
{
    [HideInInspector] public int enemyNumber;

    [SerializeField] private Animator enemy;
    [SerializeField] private BehaviorGraphAgent enemy_ai;
    [SerializeField] private Animator _player;

    private Detection local_ai_detection;

    [Header("Memory")]
    [SerializeField, Range(0, 10)] private float memoryDuration;
    private bool justLostTrack = false;
    private float currentTimeStarted;

    [Header("Get Spotted")]
    [SerializeField, Range(0, 2)] private float getSpottedVerticalOffset = 1f;
    [SerializeField, Range(0, 30)] private float getSpottedVerticalMaxDistance = 7f;
    [SerializeField, Range(0, 10)] private float getSpottedHorizontalMaxDistance = 2f;
    [SerializeField, Range(0, 1)] private float getSpottedHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] private float getSpottedVerticalMaxPercentage = 0.66f;

    [Header("Get Curious")]
    [SerializeField, Range(0, 2)] private float getCuriousVerticalOffset = 0f;
    [SerializeField, Range(5, 50)] private float getCuriousVerticalMaxDistance = 15f;
    [SerializeField, Range(2, 15)] private float getCuriousHorizontalMaxDistance = 5f;
    [SerializeField, Range(0, 1)] private float getCuriousHorizontalMaxPercentage = 0.75f;
    [SerializeField, Range(0, 1)] private float getCuriousVerticalMaxPercentage = 0.66f;

    [Header("Sixth Sense")]
    [SerializeField] private bool sixthSense = true;
    [SerializeField] private bool immediateSense = false;
    [SerializeField, Range(0, 2)] private float sixthSenseVerticalOffset = 0.25f;
    [SerializeField, Range(0, 5)] private float sixthSenseHorizontal = 2f;
    [SerializeField, Range(0, 5)] private float sixthSenseVertical = 1f;
    [SerializeField, Range(0, 1)] private float sixthSenseAnglePercentage = 0.75f;

    [Header("Temp")]
    private Vector3[] getCuriousCoordinates = new Vector3[3];
    private Vector3[] getSpottedCoordinates = new Vector3[3];
    private Vector3[] sixthSenseCoordinates = new Vector3[3];

    Transform headBone => enemy.GetBoneTransform(HumanBodyBones.Head);

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

    private void Start()
    {
        if (headBone == null)
        {
            Debug.LogError("Can't find head!");
        }

        BuildCoordinates();

    }

    private void Update()
    {
        //remember player position if just spotted
        if (justLostTrack && Time.time < currentTimeStarted + memoryDuration)
        {
            return;
        }

        enemy_ai.GetVariable<Detection>("Detection", out BlackboardVariable<Detection> currentDetection);

        if (justLostTrack && Time.time >= currentTimeStarted + memoryDuration)
        {
            justLostTrack = false;
            enemy_ai.SetVariableValue<Detection>("Detection", Detection.Searching);
            return;
        }

        //runs only 3 times per second
        if (Time.frameCount % (20 + enemyNumber) != 0) return;

        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        Vector3 playerWorldPos = _player.transform.position;
        Vector3 playerLocalToHead = Quaternion.Inverse(headRotation) * (playerWorldPos - headPosition);

        if (TrapCheck(0, playerLocalToHead, getSpottedCoordinates) || TrapCheck(1, playerLocalToHead, getSpottedCoordinates))
        {
            local_ai_detection = Detection.Spotted;
            justLostTrack = true;
            currentTimeStarted = Time.time;
        }
        else if (TrapCheck(0, playerLocalToHead, getCuriousCoordinates) || TrapCheck(1, playerLocalToHead, getCuriousCoordinates))
        {
            //debug_ai_detection = Detection.Curious;
        }
        else if (PentCheck(playerLocalToHead, sixthSenseCoordinates))
        {
            local_ai_detection = immediateSense ? Detection.Spotted : Detection.Curious;
        }
        else
        {
            local_ai_detection = currentDetection == Detection.Searching ? Detection.Searching : Detection.Idle;
        }

        enemy_ai.SetVariableValue<Detection>("Detection", local_ai_detection);
    }

    //coordinates to calculate correctly are 0 and 1, because it is reflected horizontally
    private bool TrapCheck(int coordinateNumber, Vector3 playerLocalToHead, Vector3[] coordinates)
    {
        bool inVerticalRange = playerLocalToHead.z > coordinates[coordinateNumber].z && playerLocalToHead.z < coordinates[coordinateNumber + 1].z;

        float tEdge = Mathf.InverseLerp(coordinates[coordinateNumber].z, coordinates[coordinateNumber + 1].z, playerLocalToHead.z);
        float maxHorizontalAtZ = Mathf.Lerp(coordinates[coordinateNumber].x, coordinates[coordinateNumber + 1].x, tEdge);

        return inVerticalRange && Mathf.Abs(playerLocalToHead.x) <= maxHorizontalAtZ;
    }

    private bool PentCheck(Vector3 playerLocalToHead, Vector3[] sixthCoordinates)
    {
        if (!sixthSense)
        {
            return false;
        }

        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;
        Vector3 centerBehind = headPosition + (headRotation * new Vector3(0, 0, -sixthSenseVerticalOffset));

        bool inHorizontalRange = Mathf.Abs(playerLocalToHead.x) <= sixthCoordinates[0].x;

        float tEdge = Mathf.InverseLerp(sixthCoordinates[2].x, centerBehind.x, Mathf.Abs(playerLocalToHead.x));
        float maxVerticalAtZ = Mathf.Lerp(sixthCoordinates[2].z, centerBehind.z, tEdge);

        return inHorizontalRange && playerLocalToHead.z >= maxVerticalAtZ && playerLocalToHead.z <= sixthCoordinates[0].z;
    }

    private void OnDrawGizmos()
    {
        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        BuildCoordinates();

        for (int i = 0; i < 3; i++)
        {
            Vector3 localSpottedOffset = getSpottedCoordinates[i];
            Vector3 inverseSpottedOffset = InvertCoordinates(localSpottedOffset);

            Vector3 localCuriousOffset = getCuriousCoordinates[i];
            Vector3 inverseCuriousOffset = InvertCoordinates(localCuriousOffset);

            Vector3 localSixthOffset = sixthSenseCoordinates[i];
            Vector3 inverseSixthOffset = InvertCoordinates(localSixthOffset);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(headPosition + (headRotation * localSpottedOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseSpottedOffset), 0.1f);

            //draw spotted lines
            DrawBorderLines(ref i, ref headPosition, ref headRotation, localSpottedOffset, getSpottedCoordinates);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(headPosition + (headRotation * localCuriousOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseCuriousOffset), 0.1f);

            //draw curious lines
            DrawBorderLines(ref i, ref headPosition, ref headRotation, localCuriousOffset, getCuriousCoordinates);

            if (sixthSense)
            {
                Gizmos.color = immediateSense ? Color.red : Color.yellow;
                Gizmos.DrawSphere(headPosition + (headRotation * localSixthOffset), 0.1f);
                Gizmos.DrawSphere(headPosition + (headRotation * inverseSixthOffset), 0.1f);

                Vector3 sixthSenseOrigin = headPosition + headRotation * new Vector3(0f, 0f, -sixthSenseVerticalOffset);

                Gizmos.color = immediateSense ? Color.red : Color.yellow;

                if (i == 2)
                {
                    Gizmos.DrawLine(headPosition + (headRotation * localSixthOffset), headPosition + (headRotation * new Vector3(0, 0, -sixthSenseVerticalOffset)));
                    Gizmos.DrawLine(headPosition + (headRotation * inverseSixthOffset), headPosition + (headRotation * new Vector3(0, 0, -sixthSenseVerticalOffset)));
                }
                else
                {
                    DrawBorderLines(ref i, ref headPosition, ref headRotation, localSixthOffset, sixthSenseCoordinates);
                }
            }
        }

        Gizmos.color = Color.magenta;
        DebugDrawRangeLines();

        Gizmos.color = Color.black;

        if (Application.isPlaying)
        {
            enemy_ai.GetVariable<Vector3>("LastPlayerPos", out BlackboardVariable<Vector3> lastKnownPos);
            if (lastKnownPos != null)
            {
                Gizmos.DrawWireSphere(lastKnownPos.Value, 0.5f);
            }
        }
    }

    private Vector3 InvertCoordinates(Vector3 input)
    {
        return new Vector3(-input.x, input.y, input.z);
    }

    private void DrawBorderLines(ref int i, ref Vector3 headPosition, ref Quaternion headRotation, Vector3 offset, Vector3[] coordinates)
    {
        switch (i)
        {
            case 0:
                Gizmos.DrawLine(headPosition + (headRotation * offset), headPosition + (headRotation * InvertCoordinates(offset)));
                Gizmos.DrawLine(headPosition + (headRotation * offset), headPosition + (headRotation * coordinates[(i + 1) % 3]));
                Gizmos.DrawLine(headPosition + (headRotation * InvertCoordinates(offset)), headPosition + (headRotation * InvertCoordinates(coordinates[(i + 1) % 3])));
                break;
            case 1:
                Gizmos.DrawLine(headPosition + (headRotation * offset), headPosition + (headRotation * coordinates[(i + 1) % 3]));
                Gizmos.DrawLine(headPosition + (headRotation * InvertCoordinates(offset)), headPosition + (headRotation * InvertCoordinates(coordinates[(i + 1) % 3])));
                break;
            case 2:
                Gizmos.DrawLine(headPosition + (headRotation * offset), headPosition + (headRotation * InvertCoordinates(offset)));
                break;
            default:
                Debug.LogError("DrawBorderLines failed!");
                break;
        }
    }

    private void DebugDrawRangeLines()
    {
        if (local_ai_detection == Detection.Curious || local_ai_detection == Detection.Spotted)
        {
            Gizmos.color = local_ai_detection == Detection.Spotted ? Color.red : Color.yellow;

            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.Head).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftUpperArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftLowerArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightUpperArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.Chest).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightUpperLeg).position);
            Gizmos.DrawLine(enemy.GetBoneTransform(HumanBodyBones.Head).position, _player.GetBoneTransform(HumanBodyBones.RightLowerLeg).position);
        }
    }
}