using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy: MonoBehaviour,CharacterInterface
{
    //NavMeshAgentを取得
    NavMeshAgent navMeshAgent;

    //追従したいオブジェクト
    public Transform tagetPoint;

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

    [SerializeField] private float enemyDetectionRange = 100f;
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
        if (Player.instance.IsDead) return;

        Player.instance.HP -= 1;

        if (Player.instance.HP <= 0) Player.instance.Dead() ;

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
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgentがアタッチされていません！");
            return;
        }

        // NavMeshAgentの初期化
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        // NavMesh上に配置されているか確認
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[{gameObject.name}] NavMesh上にありません。位置を補正します。");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
                Debug.Log($"[{gameObject.name}] 初期位置をNavMesh上に補正: {hit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] NavMesh位置補正に失敗しました。位置: {transform.position}");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] NavMesh上に配置されています。位置: {transform.position}");
        }

        // 徘徊地点の初期化
        if (testMap01 == null || testMap01.patrolPoint == null || testMap01.patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] testMap01またはpatrolPointが設定されていません！");
            return;
        }

        maxPositionNumber = testMap01.patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] 初期徘徊地点: {testMap01.patrolPoint[positionNumber].position}");
    }

    void Update()
    {
        if (Player.instance.IsDead || Player.instance == null || testMap01 == null) return;

        // 必須コンポーネントのチェック
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            Debug.LogWarning($"[{gameObject.name}] NavMeshAgentが無効または存在しません。");
            return;
        }
        if (Player.instance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Player.instanceがnullです。");
            return;
        }
        if (Player.instance.IsDead)
        {
            Debug.Log($"[{gameObject.name}] プレイヤーが死にました。追従を停止。");
            return;
        }
        if (testMap01 == null)
        {
            Debug.LogWarning($"[{gameObject.name}] testMap01がnullです。");
            return;
        }
        if (tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPointがnullです。プレイヤーのTransformを設定してください。");
            return;
        }

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, tagetPoint.position);
        Debug.Log($"[{gameObject.name}] プレイヤーとの距離: {distance}, 検知範囲: {DetectionRange}, 現在位置: {transform.position}, プレイヤー位置: {tagetPoint.position}");

        //プレイヤーが一定の範囲に入った場合の処理
        if (distance <= DetectionRange)
        {
            ChasePlayer();
        }
        else
        {
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
            //Player.instance = other.GetComponent<Player>();
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }
}
