using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AdjustLastKnownPlayerPos", story: "Adjust [LastPlayerPos]", category: "Action", id: "425eed00f5b6d522290f542c63924fb4")]
public partial class AdjustLastKnownPlayerPosAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> LastPlayerPos;

    protected override Status OnStart()
    {
        while (Physics.Raycast(LastPlayerPos.Value, Vector3.down, Mathf.Infinity))
        {
            //Debug.Log($"{patrolPoint.name} hit!");
            LastPlayerPos.Value += Vector3.down * 0.15f;
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

