using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Behavior;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static UnityEngine.EventSystems.EventTrigger;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Assign Names", story: "[Enemy] gets [name] and [number]", category: "Action", id: "7c01a68c18c95afb987f6fc0acc94ccc")]
public partial class AssignNamesAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Enemy;
    [SerializeReference] public BlackboardVariable<string> Name;
    [SerializeReference] public BlackboardVariable<int> Number;

    private static int s_EnemyCounter = 0;
    private int m_EnemyNumber = 0;

    private static List<string> names = new List<string>{
        "Liam", "Olivia", "Noah", "Charlotte", "Oliver",
        "Emma", "Theodore", "Amelia", "Henry", "Sophia",
        "James", "Mia", "Elijah", "Isabella", "Mateo",
        "Evelyn", "William", "Sofia", "Lucas", "Eliana"
    };

    private static List<string> availableNames = new List<string>(names);

    protected override Status OnStart()
    {
        if (availableNames == null || availableNames.Count == 0)
        {
            availableNames = new List<string>(names);
            availableNames.Shuffle();
        }

        m_EnemyNumber = s_EnemyCounter++ % 20;
        Number.Value = m_EnemyNumber;

        if (availableNames.Count > 0)
        {
            Name.Value = availableNames[0].ToString();
            availableNames.Remove(availableNames[0]);
        }
        else
        {
            Name.Value = "Ran out of names!";
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

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}
