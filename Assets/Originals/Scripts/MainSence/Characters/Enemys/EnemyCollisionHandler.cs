//廃止予定
using UnityEngine;
using UnityEngine.AI;

public class EnemyCollisionHandler : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("方向転換までの遅延時間")]
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
        // 衝突したオブジェクトが "Wall" レイヤーまたは "Wall" タグを持っているか確認
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            if (Time.time > lastCollisionTime + changeDirectionDelay)
            {
                lastCollisionTime = Time.time;
                // NavMeshエージェントが移動中の場合
                if (agent.velocity.magnitude > 0.1f && agent.isActiveAndEnabled)
                {
                    // 移動を少し停止させる
                    agent.isStopped = true;
                    // 少し待ってから新しいランダムな方向へ移動させる
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
            // 現在位置からランダムな方向の少し先の位置を新しい目的地とする
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            Vector3 newTarget = transform.position + randomDirection;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // NavMesh 上に新しい目的地が見つからなかった場合のフォールバック処理
                // 例: 現在位置から少し離れたランダムな NavMesh 上の点を探す
                if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                {
                    agent.SetDestination(edgeHit.position);
                }
            }
        }
    }
}