using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Skip Frames", story: "Frame is % [enemyNumber]", category: "Conditions", id: "d87deaec7b2bcd9a5e2093dadd958662")]
public partial class SkipFramesCondition : Condition
{
    [SerializeReference] public BlackboardVariable<int> EnemyNumber;

    public override bool IsTrue()
    {
        return Time.frameCount % (20+EnemyNumber.Value) == 0;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
