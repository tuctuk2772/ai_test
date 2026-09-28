using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Lower Detection", story: "Lower [meter] to right below [state]", category: "Action", id: "ce9a2918353a81050bc0b52d87f0af05")]
public partial class LowerDetectionAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Meter;
    [SerializeReference] public BlackboardVariable<Detection> State;
    [SerializeReference] public BlackboardVariable<Vector3> DetectionVariables;

    protected override Status OnStart()
    {
        float changedMeterValue = Meter.Value;

        switch (State.Value)
        {
            case Detection.Idle:
                changedMeterValue = DetectionVariables.Value.x - 0.01f;
                break;
            case Detection.Curious:
                changedMeterValue = DetectionVariables.Value.z - 0.01f;
                break;
            case Detection.Spotted:
                changedMeterValue = DetectionVariables.Value.y - 0.01f;
                break;
        }

        Meter.Value = changedMeterValue;

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

