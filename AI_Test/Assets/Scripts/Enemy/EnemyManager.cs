using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<AI_Locomotion> enemies = new();

    private void Awake()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            AI_Locomotion enemy = enemies[i];

            enemy.id = i;
        }
    }
}
