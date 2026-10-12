using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FloatToFloat", story: "Set [float] to [value]", category: "Action", id: "e19264bceab673f1de522fac9f60281d")]
public partial class FloatToFloatAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Float;
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        Float.Value = Value.Value;
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

