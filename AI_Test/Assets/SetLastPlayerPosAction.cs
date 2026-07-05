using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetLastPlayerPos", story: "Set [LastPlayerPos] to [Player]", category: "Action", id: "6a318c91c4ef9c8f834ac3d0c8094495")]
public partial class SetLastPlayerPosAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> LastPlayerPos;
    [SerializeReference] public BlackboardVariable<Transform> Player;

    protected override Status OnStart()
    {
        LastPlayerPos.Value = Player.Value.position;

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

