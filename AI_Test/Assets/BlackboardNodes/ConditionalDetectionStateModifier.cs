using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Conditional Detection State", story: "Check [currentSuspicionMeter] with [SuspicionMeter]", category: "Flow/Conditional", id: "796c1c9a589d2239fda1322ba8731c84")]
public partial class ConditionalDetectionStateModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<float> CurrentSuspicionMeter;
    [SerializeReference] public BlackboardVariable<Vector3> SuspicionMeter;

    [SerializeReference] public BlackboardVariable<Detection> currentDetection;

    protected override Status OnStart()
    {
        if(CurrentSuspicionMeter.Value >= SuspicionMeter.Value.z)
        {

        }


        if(CurrentSuspicionMeter.Value >= SuspicionMeter.Value.y)
        {
            currentDetection.Value = Detection.Curious;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

