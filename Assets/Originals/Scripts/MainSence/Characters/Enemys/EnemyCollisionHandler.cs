//�p�~�\��
using UnityEngine;
using UnityEngine.AI;

public class EnemyCollisionHandler : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("�����]���܂ł̒x������")]
    public float changeDirectionDelay = 1f;
    private float lastCollisionTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component not found on this GameObject.");
            enabled = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // �Փ˂����I�u�W�F�N�g�� "Wall" ���C���[�܂��� "Wall" �^�O�������Ă��邩�m�F
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time > lastCollisionTime + changeDirectionDelay)
            {
                lastCollisionTime = Time.time;
                // NavMesh�G�[�W�F���g���ړ����̏ꍇ
                if (agent.velocity.magnitude > 0.1f && agent.isActiveAndEnabled)
                {
                    // �ړ���������~������
                    agent.isStopped = true;
                    // �����҂��Ă���V���������_���ȕ����ֈړ�������
                    Invoke("ResumeAndChangeDirection", 0.5f);
                }
            }
        }
    }

    void ResumeAndChangeDirection()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            // ���݈ʒu���烉���_���ȕ����̏�����̈ʒu��V�����ړI�n�Ƃ���
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            Vector3 newTarget = transform.position + randomDirection;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // NavMesh ��ɐV�����ړI�n��������Ȃ������ꍇ�̃t�H�[���o�b�N����
                // ��: ���݈ʒu���班�����ꂽ�����_���� NavMesh ��̓_��T��
                if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                {
                    agent.SetDestination(edgeHit.position);
                }
            }
        }
    }
}