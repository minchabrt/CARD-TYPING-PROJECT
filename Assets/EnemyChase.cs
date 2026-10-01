using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    NavMeshAgent agent;
    Transform player;

    [SerializeField] float rotationSpeed = 8f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindFirstObjectByType<FPMovement>().transform;

        agent.updateRotation = false; // sami rotiramo, agent to ne radi za nas
    }

    void Update()
    {
        agent.SetDestination(player.position);
        FacePlayer();
    }

    void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // da se ne naginje gore/dole ka igracu

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}