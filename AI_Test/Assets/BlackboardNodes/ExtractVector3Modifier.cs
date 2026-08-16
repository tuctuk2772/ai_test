using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Extract Vector3", story: "[Float] is greater than [Vector3]", category: "Flow/Conditional", id: "f5e090e12f3a17544632d750f8b73a30")]
public partial class ExtractVector3Modifier : Composite
{
    [SerializeReference] public BlackboardVariable<float> Float;
    [SerializeReference] public BlackboardVariable<Vector3> Vector3;

    protected override Status OnStart()
    {
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

