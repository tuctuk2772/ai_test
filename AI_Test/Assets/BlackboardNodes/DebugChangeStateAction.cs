using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DEBUG_ChangeState", story: "Change [state] to [x]", category: "Action", id: "72e9763325f81bc395dd15fa60ddffa4")]
public partial class DebugChangeStateAction : Action
{
    [SerializeReference] public BlackboardVariable<Detection> State;
    [SerializeReference] public BlackboardVariable<Detection> X;

    protected override Status OnStart()
    {
        State.Value = X.Value;

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

