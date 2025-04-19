using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy: MonoBehaviour,CharacterInterface
{
    //
    [SerializeField] private Player player;

    //NavMeshAgentを取得
    NavMeshAgent navMeshAgent;

    //追従したいオブジェクト(Hieralchy内のオブジェクトをアタッチする)
    [SerializeField] public Transform tagetPoint;

    //徘徊
    [SerializeField] private TestMap01 testMap01;//プレハブ化したオブジェクトをアタッチ
    private int positionNumber = 0;
    private int maxPositionNumber;

    //この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になる
    //要調整が必要
    [SerializeField] private float findPatrolPointRange = 2500f;

    private Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }


    [SerializeField] private string enemyName;

    [SerializeField]
    public string CharacterName
    {
        get => enemyName;
        set => enemyName = value;
    }


    [SerializeField] private float Speed = 4f;
    [SerializeField]
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }


    [SerializeField] private float dashSpeed = 5f;
    [SerializeField]
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }

    [SerializeField] private float enemyDetectionRange = 10f;
    [SerializeField]
    public float DetectionRange
    {
        get => enemyDetectionRange;
        set => enemyDetectionRange = value;
    }

    [SerializeField] private float enemyGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => enemyGravity;
        set => enemyGravity = value;
    }

    [SerializeField] private int enemyHP = 1;
    [SerializeField]
    public int HP
    {
        get => enemyHP;
        set => enemyHP = value;
    }

    [SerializeField] private bool enemyIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => enemyIsDead;
        set => enemyIsDead = value;
    }

    [SerializeField] private bool enemyIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => enemyIsMove;
        set => enemyIsMove = value;
    }

    public void Dead()
    {
        Debug.Log("Enemy Dead");
    }


    //攻撃
    public void Attack()
    {
        if (player.IsDead) return;

        player.HP -= 1;

        if (player.HP <= 0) player.Dead() ;

    }


    [SerializeField] private Vector3 enemyStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => enemyStartPosition;
        set => enemyStartPosition = value;
    }

    //追従
    void ChasePlayer() 
    {
        //追従用の目的地を設定
        navMeshAgent.SetDestination(tagetPoint.position);
        Debug.Log("プレイヤー追従");
    }

    //次の俳諧地点を決める
    void NextPosition() 
    {
        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = testMap01.patrolPoint[positionNumber].position;

        NavMeshHit hit;

        //目的地がNaveMeshの範囲内にあるかを判定する
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas)) 
        {
            navMeshAgent.destination = hit.position;
            //Debug.Log("目的地をNavMesh上に補正: " + hit.position);
        }
        else
        {
            Debug.LogError("目的地がNavMeshの範囲外です: " + targetPos);

            // **ランダムな位置を探索する**
            Vector3 randomPos = targetPos + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            if (NavMesh.SamplePosition(randomPos, out hit, 10.0f, NavMesh.AllAreas))
            {
                navMeshAgent.destination = hit.position;
                Debug.LogWarning("代わりにNavMesh上のランダムな位置に移動: " + hit.position);
            }
            else
            {
                Debug.LogError("適切なNavMesh位置が見つかりませんでした");
            }
        }
    }

    void Start() 
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent.isOnNavMesh)
        {
            //Debug.Log("NavMeshAgent は NavMesh 上に配置されています！");
        }
        else
        {
            Debug.LogError("NavMeshAgent が NavMesh 上にありません！");
        }

        //移動を有効化
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        // NavMesh 上の適切な位置に移動
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            Debug.Log("敵の初期位置を調整: " + hit.position);
        }

        navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
        maxPositionNumber = testMap01.patrolPoint.Length;
        Debug.Log("patrolPointの数: " + maxPositionNumber);

        if (maxPositionNumber > 0)
        {
            navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
            Debug.Log("初期徘徊地点は" + testMap01.patrolPoint[positionNumber].position);
        }
        else
        {
            Debug.LogError("patrolPointが一つも作られていません！");
        }
    }

    void Update()
    {
        if (player.IsDead || player == null || testMap01 == null) return;

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, tagetPoint.position);

        //プレイヤーが一定の範囲に入った場合の処理
        if (distance <= DetectionRange)
        {
            ChasePlayer();
        }
        else
        {
            //Debug.Log($"現在地: {transform.position}");
            //Debug.Log($"目的地: {navMeshAgent.destination}");
            //Debug.Log($"目的地までの距離: {navMeshAgent.remainingDistance}");
            //Debug.Log($"移動速度: {navMeshAgent.velocity}");
            //Debug.Log($"NavMeshAgentの状態: {navMeshAgent.pathStatus}");

            //敵と俳諧地点の距離が指定の値の範囲内の場合の処理
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                NextPosition();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            if (player != null && !player.IsDead)
            {
                Attack();
            }
        }
    }
}
