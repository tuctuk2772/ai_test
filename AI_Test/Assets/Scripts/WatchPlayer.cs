using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

public class WatchPlayer : MonoBehaviour
{
    [HideInInspector] public int enemyNumber;

    [SerializeField] private Animator enemy;
    [SerializeField] private BehaviorGraphAgent enemy_ai;
    [SerializeField] private Animator _player;

    [Header("Memory")]
    [SerializeField, Range(0, 10)] private float memoryDuration;
    private bool justLostTrack = false;
    private float currentTimeStarted;

    [Header("Suspicion")]
    [SerializeField] private Vector2 suspicionMeter;

    //[SerializeField, Range(0, 10), InspectorName("Suspicion Meter Max (sec)")] private float suspicionMeterMax;
    //[SerializeField, Range(0, 5), InspectorName("Suspicion Meter Min (sec)")] private float suspicionMeterMin;

    [Header("Enemy Perception Settings")]
    [SerializeField, InspectorName("")] private EnemyPerceptionSettings enemyPerceptionSettings;

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
            enemy_ai.SetVariableValue<Transform>("Player", _player.transform);
            enemy_ai.SetVariableValue<Animator>("PlayerAnimator", _player);
        }
        else
        {
            Debug.LogError("Player not assigned!");
        }

        //suspicion meter rounding
        Vector2 newSuspicionMeter = new Vector2(Mathf.Round(suspicionMeter.x * 10f)/10f,Mathf.Round(suspicionMeter.y * 10f)/10f);
        suspicionMeter = newSuspicionMeter;

        enemy_ai.SetVariableValue<List<Vector3>>("GetCuriousCoordinates", ai.getCuriousCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<List<Vector3>>("GetSpottedCoordinates", ai.getSpottedCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<List<Vector3>>("SixthSenseCoordinates", ai.sixthSenseCoordinates.ToList<Vector3>());
        enemy_ai.SetVariableValue<Animator>("Enemy", enemy);
        enemy_ai.SetVariableValue<Transform>("HeadBone", headBone);
        enemy_ai.SetVariableValue<bool>("SixthSense", ai.sixthSense);
        enemy_ai.SetVariableValue<bool>("ImmediateSense", ai.immediateSense);
        enemy_ai.SetVariableValue<float>("SixthSenseVerticalOffset", ai.sixthSenseVerticalOffset);
        enemy_ai.SetVariableValue<Vector2>("SuspicionMeter", suspicionMeter);
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
        //DebugDrawRangeLines();

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