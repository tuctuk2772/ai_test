using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "OutOfSightCounter", story: "Start [CurrentMemoryTimer]", category: "Action", id: "634b4be488b1c225d78203dc8edb8fd7")]
public partial class OutOfSightCounterAction : Action
{
    [SerializeReference] public BlackboardVariable<float> ActiveMemoryDuration;
    [SerializeReference] public BlackboardVariable<float> CurrentMemoryTimer;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        CurrentMemoryTimer.Value += Time.deltaTime;
        return Status.Success;
    }

    protected override void OnEnd()
    {

    }
}

