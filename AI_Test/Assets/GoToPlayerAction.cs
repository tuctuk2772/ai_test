using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Go To Player", story: "Agent moves towards [player]", category: "Action/Navigation", id: "8b532d8a18a3d66f336be56759748601")]
public partial class GoToPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Player;
    [SerializeReference] public BlackboardVariable<Int32> enemyNumber;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        /*if (Time.frameCount % (20 + enemyNumber) == 0)
        {
            Debug.Log("updated!");
            return Status.Success;
        }
        else
        {
            return Status.Failure;
        }*/

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

