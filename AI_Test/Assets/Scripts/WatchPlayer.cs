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

    [Header("Enemy Perception Settings")]
    [SerializeField] private EnemyPerceptionSettings enemyPerceptionSettings;

    //just lazy, want to write a bit less :)
    private EnemyPerceptionSettings ai => enemyPerceptionSettings;

    Transform headBone => enemy.GetBoneTransform(HumanBodyBones.Head);

    private void Start()
    {
        if (headBone == null)
        {
            Debug.LogError("Can't find head!");
        }

        //for some reason enemy ai cannot keep Player's transform in memory, so just manually assigning it here
        if (_player != null)
        {
            //enemy_ai.SetVariableValue<Transform>("Player", _player.transform);
            enemy_ai.SetVariableValue<Animator>("PlayerAnimator", _player);
        }
        else
        {
            Debug.LogError("Player not assigned!");
        }

        enemy_ai.SetVariableValue<List<Vector3>>("GetCuriousCoordinates", ai.getCuriousCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<List<Vector3>>("GetSpottedCoordinates", ai.getSpottedCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<List<Vector3>>("SixthSenseCoordinates", ai.sixthSenseCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<Animator>("Enemy", enemy);
        enemy_ai.SetVariableValue<Transform>("HeadBone", headBone);
        enemy_ai.SetVariableValue<bool>("SixthSense", ai.sixthSense);
        enemy_ai.SetVariableValue<bool>("ImmediateSense", ai.immediateSense);
        enemy_ai.SetVariableValue<float>("SixthSenseVerticalOffset", ai.sixthSenseVerticalOffset);
    }

    /*private void Update()
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
*/
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
        if (!ai.sixthSense)
        {
            return false;
        }

        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;
        Vector3 centerBehind = headPosition + (headRotation * new Vector3(0, 0, -ai.sixthSenseVerticalOffset));

        bool inHorizontalRange = Mathf.Abs(playerLocalToHead.x) <= sixthCoordinates[0].x;

        float tEdge = Mathf.InverseLerp(sixthCoordinates[2].x, centerBehind.x, Mathf.Abs(playerLocalToHead.x));
        float maxVerticalAtZ = Mathf.Lerp(sixthCoordinates[2].z, centerBehind.z, tEdge);

        return inHorizontalRange && playerLocalToHead.z >= maxVerticalAtZ && playerLocalToHead.z <= sixthCoordinates[0].z;
    }

    private void OnDrawGizmos()
    {
        if (ai == null)
        {
            return;
        }

        Vector3 headPosition = headBone.position;
        Quaternion headRotation = headBone.rotation;

        for (int i = 0; i < 3; i++)
        {
            Vector3 localSpottedOffset = ai.getSpottedCoordinates[i];
            Vector3 inverseSpottedOffset = InvertCoordinates(localSpottedOffset);

            Vector3 localCuriousOffset = ai.getCuriousCoordinates[i];
            Vector3 inverseCuriousOffset = InvertCoordinates(localCuriousOffset);

            Vector3 localSixthOffset = ai.sixthSenseCoordinates[i];
            Vector3 inverseSixthOffset = InvertCoordinates(localSixthOffset);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(headPosition + (headRotation * localSpottedOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseSpottedOffset), 0.1f);

            //draw spotted lines
            DrawBorderLines(ref i, ref headPosition, ref headRotation, localSpottedOffset, ai.getSpottedCoordinates);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(headPosition + (headRotation * localCuriousOffset), 0.1f);
            Gizmos.DrawSphere(headPosition + (headRotation * inverseCuriousOffset), 0.1f);

            //draw curious lines
            DrawBorderLines(ref i, ref headPosition, ref headRotation, localCuriousOffset, ai.getCuriousCoordinates);

            if (ai.sixthSense)
            {
                Gizmos.color = ai.immediateSense ? Color.red : Color.yellow;
                Gizmos.DrawSphere(headPosition + (headRotation * localSixthOffset), 0.1f);
                Gizmos.DrawSphere(headPosition + (headRotation * inverseSixthOffset), 0.1f);

                Vector3 sixthSenseOrigin = headPosition + headRotation * new Vector3(0f, 0f, -ai.sixthSenseVerticalOffset);

                Gizmos.color = ai.immediateSense ? Color.red : Color.yellow;

                if (i == 2)
                {
                    Gizmos.DrawLine(headPosition + (headRotation * localSixthOffset), headPosition + (headRotation * new Vector3(0, 0, -ai.sixthSenseVerticalOffset)));
                    Gizmos.DrawLine(headPosition + (headRotation * inverseSixthOffset), headPosition + (headRotation * new Vector3(0, 0, -ai.sixthSenseVerticalOffset)));
                }
                else
                {
                    DrawBorderLines(ref i, ref headPosition, ref headRotation, localSixthOffset, ai.sixthSenseCoordinates);
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
                Gizmos.DrawWireCube(lastKnownPos.Value, Vector3.one);
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