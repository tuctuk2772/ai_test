using UnityEngine;
using UnityEngine.AI;

public class AI_Locomotion : MonoBehaviour
{
    [SerializeField] private Transform player;
    NavMeshAgent agent;
    Animator animator;

    [HideInInspector] public float id;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Time.frameCount % (20 + id) == 0)
        {
            Debug.Log($"{id}: {Time.frameCount}");
            agent.destination = player.position;
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
