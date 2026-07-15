using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look Around", story: "Check if [agent] can see [player]", category: "Action/Find", id: "c9a9c2e49deb770d66f2ce9445b8f598")]
public partial class LookAroundAction : Action
{
    //note - this can be optimized further, i want to eventually have one raycast per frame as opposed to overloading with 10 raycasts, like Splinter Cell Blacklist

    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Player;
    [SerializeReference] public BlackboardVariable<Animator> Player_Animator;
    [SerializeReference] public BlackboardVariable<Detection> CurrentDetection;
    [SerializeReference] public BlackboardVariable<Int32> EnemyNumber;

    [SerializeReference] public BlackboardVariable<List<Vector3>> GetCuriousCoordinates;
    [SerializeReference] public BlackboardVariable<List<Vector3>> GetSpottedCoordinates;
    [SerializeReference] public BlackboardVariable<List<Vector3>> SixthSenseCoordinates;

    [SerializeReference] public BlackboardVariable<Transform> HeadBone;

    [SerializeReference] public BlackboardVariable<bool> SixthSense, ImmediateSense;
    [SerializeReference] public BlackboardVariable<float> SixthSenseVerticalOffset;

    [SerializeReference] public BlackboardVariable<float> suspicionMeterMax;
    [SerializeReference] public BlackboardVariable<float> suspicionMeterVisual;

    private int playerLayer;
    private float currentSuspicionMeter = 0f;

    private bool suspicionGrowing = false;

    private struct DetectionZone
    {
        public Detection detectionOutcome;
        public Func<Vector3, bool> InZone;
    }

    private List<DetectionZone> ZonePriorities()
    {
        return new List<DetectionZone>()
        {
            new DetectionZone
        {
            detectionOutcome = Detection.Spotted,
            InZone = pos => TrapCheck(0, pos, GetSpottedCoordinates.Value) || TrapCheck(1, pos, GetSpottedCoordinates.Value)
        },
        new DetectionZone
        {
            detectionOutcome = Detection.Curious,
            InZone = pos => TrapCheck(0, pos, GetCuriousCoordinates.Value) || TrapCheck(1, pos, GetCuriousCoordinates.Value)
        },
        new DetectionZone
        {
            detectionOutcome = ImmediateSense.Value ? Detection.Spotted : Detection.Curious,
            InZone = pos => PentCheck(pos, SixthSenseCoordinates.Value)
        }
        };
    }

    protected override Status OnStart()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        /*        if (Time.frameCount % (20 + EnemyNumber.Value) != 0 && !suspicionGrowing)
                {
                    return Status.Running;
                }*/

        if (Player.Value == null)
        {
            Debug.LogError("Player not assigned!");
            return Status.Running;
        }

        Vector3 headPosition = HeadBone.Value.position;
        Quaternion headRotation = HeadBone.Value.rotation;

        Vector3 playerWorldPos = Player.Value.position;
        Vector3 playerLocalToHead = Quaternion.Inverse(headRotation) * (playerWorldPos - headPosition);

        Detection candidateDetection = CurrentDetection.Value == Detection.Searching ? Detection.Searching : Detection.Idle;

        foreach (var zone in ZonePriorities())
        {
            if (zone.InZone(playerLocalToHead))
            {
                candidateDetection = zone.detectionOutcome;
                break;
            }
        }

        if (PlayerSeen(candidateDetection))
        {
            CurrentDetection.Value = candidateDetection;
        }

        return Status.Running;
    }

    

    //coordinates to calculate correctly are 0 and 1, because it is reflected horizontally
    private bool TrapCheck(int coordinateNumber, Vector3 playerLocalToHead, List<Vector3> coordinates)
    {
        bool inVerticalRange = playerLocalToHead.z > coordinates[coordinateNumber].z && playerLocalToHead.z < coordinates[coordinateNumber + 1].z;

        float tEdge = Mathf.InverseLerp(coordinates[coordinateNumber].z, coordinates[coordinateNumber + 1].z, playerLocalToHead.z);
        float maxHorizontalAtZ = Mathf.Lerp(coordinates[coordinateNumber].x, coordinates[coordinateNumber + 1].x, tEdge);

        return inVerticalRange && Mathf.Abs(playerLocalToHead.x) <= maxHorizontalAtZ;
    }

    private bool PentCheck(Vector3 playerLocalToHead, List<Vector3> sixthCoordinates)
    {
        if (!SixthSense)
        {
            return false;
        }

        Vector3 headPosition = HeadBone.Value.position;
        Quaternion headRotation = HeadBone.Value.rotation;
        Vector3 centerBehind = headPosition + (headRotation * new Vector3(0, 0, -SixthSenseVerticalOffset.Value));

        bool inHorizontalRange = Mathf.Abs(playerLocalToHead.x) <= sixthCoordinates[0].x;

        float tEdge = Mathf.InverseLerp(sixthCoordinates[2].x, centerBehind.x, Mathf.Abs(playerLocalToHead.x));
        float maxVerticalAtZ = Mathf.Lerp(sixthCoordinates[2].z, centerBehind.z, tEdge);

        return inHorizontalRange && playerLocalToHead.z >= maxVerticalAtZ && playerLocalToHead.z <= sixthCoordinates[0].z;
    }

    private struct Body
    {
        public Vector3 bonePosition;
        public float distance;

        public Vector3 direction;
        public float verticalOffset;
        public int seenValue;
    }

    private Body CreateBody(ref Vector3 origin, HumanBodyBones bone, int value)
    {
        Vector3 bonePosition = Player_Animator.Value.GetBoneTransform(bone).position;
        Vector3 direction = bonePosition - origin;
        float distance = direction.magnitude;
        direction.Normalize();

        return new Body
        {
            bonePosition = bonePosition,
            seenValue = value,

            direction = direction,
            distance = distance,
            verticalOffset = Mathf.Abs(bonePosition.y - origin.y)
        };
    }

    private bool PlayerSeen(Detection outcomeDetection)
    {
        Vector3 origin = HeadBone.Value.position;

        //priority for specific body parts
        Body[] targets = new Body[]
        {
            CreateBody(ref origin, HumanBodyBones.Head, 3),
            CreateBody(ref origin, HumanBodyBones.LeftUpperArm, 2),
            CreateBody(ref origin, HumanBodyBones.LeftLowerArm, 1),
            CreateBody(ref origin, HumanBodyBones.RightUpperArm, 2),
            CreateBody(ref origin, HumanBodyBones.RightLowerArm, 1),
            CreateBody(ref origin, HumanBodyBones.Chest, 3),
            CreateBody(ref origin, HumanBodyBones.LeftUpperLeg, 2),
            CreateBody(ref origin, HumanBodyBones.LeftLowerLeg, 1),
            CreateBody(ref origin, HumanBodyBones.RightUpperLeg, 2),
            CreateBody(ref origin, HumanBodyBones.RightLowerLeg, 1),
        };

        //how do you do this one raycast frame by frame?

        int visibilityValue = 0;

        int amountOfBonesSeen = 0;
        float averageDistance = 0f;

        for (int i = 0; i < targets.Length; i++)
        {
            Body targetBone = targets[i];

            if (Mathf.Abs(targetBone.verticalOffset) > 3f)
            {
                continue;
            }

            if (Physics.Raycast(origin, targetBone.direction, out RaycastHit hit, targetBone.distance))
            {
                visibilityValue += hit.collider.gameObject.layer == playerLayer ? targetBone.seenValue : 0;
                amountOfBonesSeen++;
                averageDistance += targetBone.distance;
            }
        }

        averageDistance /= amountOfBonesSeen;

        suspicionGrowing = visibilityValue > 5 ? true : false;

        if (suspicionGrowing)
        {
            currentSuspicionMeter += Time.deltaTime;
        }
        else
        {
            //suspicion currently snaps back down
            if(currentSuspicionMeter > 0)
            {
                currentSuspicionMeter -= 0.25f * Time.deltaTime;
                suspicionMeterVisual.Value -= currentSuspicionMeter;
            }
            return false;
        }

        float detectionSuspicionMeter = suspicionMeterMax.Value;

        if (outcomeDetection == Detection.Spotted)
        {
            float distanceRatio = Mathf.Clamp01(averageDistance / GetCuriousCoordinates.Value[2].z);

            detectionSuspicionMeter = distanceRatio * 0.75f * suspicionMeterMax.Value;
        }

        suspicionMeterVisual.Value = Mathf.Clamp01(currentSuspicionMeter/detectionSuspicionMeter);

        return currentSuspicionMeter >= detectionSuspicionMeter;
    }

    protected override void OnEnd()
    {

    }
}

