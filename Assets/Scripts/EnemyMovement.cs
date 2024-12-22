using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform targetPlayer;        // ѕозици€ игрока
    public Transform secondEnemy;         // ѕозици€ второго врага
    private NavMeshAgent enemyNavMeshAgent; // јгент дл€ навигации

    public float avoidanceRadius = 5f;    // –адиус, в котором враг будет избегать других врагов
    public float distanceToWall = 1.0f;  // –ассто€ние до стены или преп€тстви€, с которым враг должен столкнутьс€

    void Start()
    {
        enemyNavMeshAgent = GetComponent<NavMeshAgent>(); // »нициализаци€ агента

        // Ќаходим игрока по тегу "Player", если переменна€ targetPlayer не была задана в инспекторе
        if (targetPlayer == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                targetPlayer = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player not found. Please assign the player to the targetPlayer field.");
            }
        }
    }

    void Update()
    {
        if (targetPlayer != null)
        {
            // Ћогика движени€ врага
            Vector3 targetPosition = targetPlayer.position;

            if (secondEnemy != null)
            {
                // ѕроверка на рассто€ние до второго врага дл€ избегани€ столкновений
                float distanceToSecondEnemy = Vector3.Distance(transform.position, secondEnemy.position);
                if (distanceToSecondEnemy < avoidanceRadius)
                {
                    Vector3 avoidanceDirection = transform.position - secondEnemy.position;
                    targetPosition = targetPlayer.position + avoidanceDirection.normalized * avoidanceRadius;
                }
            }

            // ƒаем агенту задание двигатьс€ к позиции с учетом преп€тствий
            NavMeshPath path = new NavMeshPath();
            enemyNavMeshAgent.CalculatePath(targetPosition, path);

            // ѕровер€ем, есть ли путь
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                enemyNavMeshAgent.SetDestination(targetPosition); // ѕуть найден, устанавливаем конечную точку
            }
            else
            {
                // ≈сли путь не найден (например, путь заблокирован), пытаемс€ найти обходной путь
                AvoidObstacles();
            }
        }
        else
        {
            Debug.LogError("Target Player is not assigned and not found in the scene.");
        }
    }

    // ћетод дл€ обхода преп€тствий (например, стен)
    void AvoidObstacles()
    {
        // ѕровер€ем, что перед врагом есть преп€тствие
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceToWall))
        {
            // ≈сли преп€тствие перед врагом, пытаемс€ обойти его
            Vector3 avoidanceDirection = Vector3.Reflect(transform.forward, hit.normal); // »спользуем отражение
            Vector3 newTargetPosition = transform.position + avoidanceDirection * avoidanceRadius;
            enemyNavMeshAgent.SetDestination(newTargetPosition); // ”станавливаем новую цель, чтобы избежать преп€тстви€
        }
        else
        {
            // ≈сли нет преп€тствий, продолжаем двигатьс€ к игроку
            enemyNavMeshAgent.SetDestination(targetPlayer.position);
        }
    }
}
