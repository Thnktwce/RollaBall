using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public Transform targetPlayer;        // ������� ������
    public Transform secondEnemy;         // ������� ������� �����
    private NavMeshAgent enemyNavMeshAgent; // ����� ��� ���������

    public float avoidanceRadius = 5f;    // ������, � ������� ���� ����� �������� ������ ������
    public float distanceToWall = 1.0f;  // ���������� �� ����� ��� �����������, � ������� ���� ������ �����������

    void Start()
    {
        enemyNavMeshAgent = GetComponent<NavMeshAgent>(); // ������������� ������

        // ������� ������ �� ���� "Player", ���� ���������� targetPlayer �� ���� ������ � ����������
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
            // ������ �������� �����
            Vector3 targetPosition = targetPlayer.position;

            if (secondEnemy != null)
            {
                // �������� �� ���������� �� ������� ����� ��� ��������� ������������
                float distanceToSecondEnemy = Vector3.Distance(transform.position, secondEnemy.position);
                if (distanceToSecondEnemy < avoidanceRadius)
                {
                    Vector3 avoidanceDirection = transform.position - secondEnemy.position;
                    targetPosition = targetPlayer.position + avoidanceDirection.normalized * avoidanceRadius;
                }
            }

            // ���� ������ ������� ��������� � ������� � ������ �����������
            NavMeshPath path = new NavMeshPath();
            enemyNavMeshAgent.CalculatePath(targetPosition, path);

            // ���������, ���� �� ����
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                enemyNavMeshAgent.SetDestination(targetPosition); // ���� ������, ������������� �������� �����
            }
            else
            {
                // ���� ���� �� ������ (��������, ���� ������������), �������� ����� �������� ����
                AvoidObstacles();
            }
        }
        else
        {
            Debug.LogError("Target Player is not assigned and not found in the scene.");
        }
    }

    // ����� ��� ������ ����������� (��������, ����)
    void AvoidObstacles()
    {
        // ���������, ��� ����� ������ ���� �����������
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, distanceToWall))
        {
            // ���� ����������� ����� ������, �������� ������ ���
            Vector3 avoidanceDirection = Vector3.Reflect(transform.forward, hit.normal); // ���������� ���������
            Vector3 newTargetPosition = transform.position + avoidanceDirection * avoidanceRadius;
            enemyNavMeshAgent.SetDestination(newTargetPosition); // ������������� ����� ����, ����� �������� �����������
        }
        else
        {
            // ���� ��� �����������, ���������� ��������� � ������
            enemyNavMeshAgent.SetDestination(targetPlayer.position);
        }
    }
}
