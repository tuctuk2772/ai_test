using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UniversalFunctions;

using Action = Unity.Behavior.Action;

/*todo -
 * - decouple increase and decrease detection
 * - decouple visual conversion from regular detecton float (two separate Blackboard Variables)
 * - convert PlayerSeen to float - it's not a binary system
 *      - switching states should be in behaviour tree
 * - move trap/pent checks into separate script
*/

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

    [SerializeReference] public BlackboardVariable<Vector3> suspicionMeter;
    [SerializeReference] public BlackboardVariable<float> currentSuspicionMeter;
    [SerializeReference] public BlackboardVariable<float> suspicionMeterVisual;

    private int playerLayer;
    //private float currentSuspicionMeter = 0f;
    private float maxDurationBeforeSpotted = 1f;

    private bool suspicionGrowing = false;

    #region ZoneSetup
    private struct DetectionZone
    {
        public Detection detectionOutcome;
        public List<Vector3> zoneCoordinates;
        public Func<Vector3, bool> InZone;
    }

    //priorities from top to bottom
    private List<DetectionZone> ZonePriorities()
    {
        return new List<DetectionZone>()
        {
            new DetectionZone
        {
            detectionOutcome = Detection.Spotted,
            zoneCoordinates = GetSpottedCoordinates.Value,
            InZone = pos => TrapCheck(0, pos, GetSpottedCoordinates.Value) || TrapCheck(1, pos, GetSpottedCoordinates.Value)
        },
        new DetectionZone
        {
            detectionOutcome = Detection.Curious,
            zoneCoordinates = GetCuriousCoordinates.Value,
            InZone = pos => TrapCheck(0, pos, GetCuriousCoordinates.Value) || TrapCheck(1, pos, GetCuriousCoordinates.Value)
        },
        new DetectionZone
        {
            detectionOutcome = ImmediateSense.Value ? Detection.Spotted : Detection.Curious,
            zoneCoordinates = SixthSenseCoordinates.Value,
            InZone = pos => PentCheck(pos, SixthSenseCoordinates.Value)
        }
        };
    }
    #endregion

    protected override Status OnStart()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        Debug.Log(suspicionMeter.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //checks every 20 frames, creates some issues rn
        /*if (Time.frameCount % (20 + EnemyNumber.Value) != 0 && !suspicionGrowing)
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

        //by default, ai doesn't see player
        Detection candidateDetection = CurrentDetection.Value == Detection.Searching ? Detection.Searching : Detection.Idle;

        List<Vector3> candidateCoordinates = new(); //check this later, this might cause issues

        foreach (var zone in ZonePriorities())
        {
            if (zone.InZone(playerLocalToHead))
            {
                candidateDetection = zone.detectionOutcome;
                candidateCoordinates = zone.zoneCoordinates;
                break;
            }
        }

        if (candidateDetection == Detection.Idle || candidateDetection == Detection.Searching)
        {
            GradualSuspicionReduction();
            suspicionMeterVisual.Value = currentSuspicionMeter / maxDurationBeforeSpotted;
            return Status.Running;
        }

        //if (PlayerSeen(candidateDetection, ref candidateCoordinates))
        //{
        //    Debug.Log(candidateDetection);
        //}

        if (PlayerSeen(candidateDetection, ref candidateCoordinates))
        {
            CurrentDetection.Value = candidateDetection;
        }

        return Status.Running;
    }



    #region CoordinateChecks
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

    #endregion

    #region BodySetup
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
    #endregion

    //the final output is based on a timer, not automatically detected
    private bool PlayerSeen(Detection outcomeDetection, ref List<Vector3> zoneCoordinates)
    {
        Vector3 origin = HeadBone.Value.position;

        //there has to be a way to lock these references in, right? but maybe because you have to grab current positions idk
        Body[] targets = new Body[] //priority for specific body parts
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

        int visibilityValue = 0; //different locations on the body have different visibilityValues

        int amountOfBonesSeen = 0; //amount of bones is important to divide all visible bones to get averageDistance
        float averageDistance = 0f;

        for (int i = 0; i < targets.Length; i++)
        {
            Body targetBone = targets[i];

            if (Mathf.Abs(targetBone.verticalOffset) > 3f)
            {
                continue;
            }

            //how do you do this one raycast frame by frame instead of all at once?
            if (Physics.Raycast(origin, targetBone.direction, out RaycastHit hit, targetBone.distance))
            {
                visibilityValue += hit.collider.gameObject.layer == playerLayer ? targetBone.seenValue : 0;
                amountOfBonesSeen++;
                averageDistance += targetBone.distance;
            }
        }

        //NEED TO SAVE THIS UP HERE, IMPORTANT
        float oldMaxDurationBeforeSpotted = maxDurationBeforeSpotted;

        //by default, the max duration is set to the max suspicion meter
        maxDurationBeforeSpotted = suspicionMeter.Value.y;

        suspicionGrowing = visibilityValue > 5 ? true : false;

        //player is not seen at all or not enough
        if (amountOfBonesSeen == 0 || !suspicionGrowing)
        {
            GradualSuspicionReduction();
        }
        else
        {
            averageDistance /= amountOfBonesSeen;

            float oldSuspicionMeterValue = currentSuspicionMeter;

            //the closer the enemy is to the player, the faster the player is detected,
            //but it is a static buildup if it's just curious
            if (outcomeDetection == Detection.Spotted)
            {
                maxDurationBeforeSpotted = _UniversalFunctions.ConvertRangeNewValue(
                    oldMin: Mathf.Abs(zoneCoordinates[0].z),
                    oldMax: Mathf.Abs(zoneCoordinates[2].z),
                    newMin: suspicionMeter.Value.x,
                    newMax: suspicionMeter.Value.y,
                    oldValue: averageDistance);
            }

            currentSuspicionMeter.Value = _UniversalFunctions.ConvertRangeNewValue(
                        oldMin: 0,
                        oldMax: oldMaxDurationBeforeSpotted,
                        newMin: 0,
                        newMax: maxDurationBeforeSpotted,
                        oldValue: oldSuspicionMeterValue
                        );

            currentSuspicionMeter.Value += Time.deltaTime;
        }

        //converts to range 0 thru 1
        float adjustedCurrentSuspicionMeter = _UniversalFunctions.ConvertRangeNewValue(
                        oldMin: 0,
                        oldMax: maxDurationBeforeSpotted,
                        newMin: 0,
                        newMax: 1,
                        oldValue: currentSuspicionMeter
                        );

        suspicionMeterVisual.Value = adjustedCurrentSuspicionMeter;

        return adjustedCurrentSuspicionMeter >= 1f;
    }

    private void GradualSuspicionReduction()
    {
        //for the future, I'd like the speed of this to go down more consistantly, but this is fine for now
        if (currentSuspicionMeter > 0)
        {
            currentSuspicionMeter.Value -= (maxDurationBeforeSpotted * 0.25f) * Time.deltaTime;
        }

        //prevents accidental negative numbers
        if (currentSuspicionMeter < 0)
        {
            currentSuspicionMeter.Value = 0;
        }
    }

    protected override void OnEnd()
    {

    }
}

