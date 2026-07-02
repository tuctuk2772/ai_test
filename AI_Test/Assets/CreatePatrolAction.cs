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
        if(Self.Value.transform.parent.GetComponent<VisualizePatrol>() == null)
        {
            Debug.LogError("Can't find visualizePatrol!");
        }

        List<GameObject> patrolPoints = Self.Value.transform.parent.GetComponent<VisualizePatrol>().patrolPoints;

        if(patrolPoints.Count == 0)
        {
            Debug.LogError("didn't transfer!");
        }

        Patrols.Value = new List<GameObject>(patrolPoints);

        Self.Value.transform.position = Patrols.Value[0].transform.position;

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

