using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look Around", story: "Check if [agent] can see [player]", category: "Action/Find", id: "c9a9c2e49deb770d66f2ce9445b8f598")]
public partial class LookAroundAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Player;
    [SerializeReference] public BlackboardVariable<Detection> CurrentDetection;
    [SerializeReference] public BlackboardVariable<Int32> EnemyNumber;

    [SerializeReference] public BlackboardVariable<List<Vector3>> GetCuriousCoordinates;
    [SerializeReference] public BlackboardVariable<List<Vector3>> GetSpottedCoordinates;
    [SerializeReference] public BlackboardVariable<List<Vector3>> SixthSenseCoordinates;

    [SerializeReference] public BlackboardVariable<Transform> HeadBone;

    [SerializeReference] public BlackboardVariable<bool> SixthSense, ImmediateSense;
    [SerializeReference] public BlackboardVariable<float> SixthSenseVerticalOffset;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Time.frameCount % (20 + EnemyNumber.Value) != 0)
        {
            return Status.Running;
        }

        Vector3 headPosition = HeadBone.Value.position;
        Quaternion headRotation = HeadBone.Value.rotation;

        Vector3 playerWorldPos = Player.Value.position;
        Vector3 playerLocalToHead = Quaternion.Inverse(headRotation) * (playerWorldPos - headPosition);

        if (TrapCheck(0, playerLocalToHead, GetSpottedCoordinates.Value) || TrapCheck(1, playerLocalToHead, GetSpottedCoordinates.Value))
        {
            CurrentDetection.Value = Detection.Spotted;
        }
        else if (TrapCheck(0, playerLocalToHead, GetCuriousCoordinates.Value) || TrapCheck(1, playerLocalToHead, GetCuriousCoordinates.Value))
        {
            CurrentDetection.Value = Detection.Curious;
        }
        else if (PentCheck(playerLocalToHead, SixthSenseCoordinates.Value))
        {
            CurrentDetection.Value = ImmediateSense.Value ? Detection.Spotted : Detection.Curious;
        }
        else
        {
            CurrentDetection.Value = CurrentDetection.Value == Detection.Searching ? Detection.Searching : Detection.Idle;
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



    protected override void OnEnd()
    {

    }
}

