using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Create Patrol", story: "[Self] create [patrols]", category: "Action", id: "0e61067961dca8750ff6cfad0c66b81c")]
public partial class CreatePatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Patrols;
    protected override Status OnStart()
    {
        List<GameObject> patrolPoints = Self.Value.transform.parent.GetComponent<VisualizePatrol>().patrolPoints;

        Patrols.Value = patrolPoints;

        Self.Value.transform.position = patrolPoints[0].transform.position;

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

