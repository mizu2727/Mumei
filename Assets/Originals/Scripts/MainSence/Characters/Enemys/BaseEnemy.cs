using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random; // System.Random と UnityEngine.Random の衝突を避ける

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    // NavMeshAgentを取得
    NavMeshAgent navMeshAgent;

    // 追従したいオブジェクト
    public Transform tagetPoint;

    // 徘徊
    [SerializeField] private TestMap01 testMap01;//プレハブ化したオブジェクトをアタッチ
    private int positionNumber = 0;
    private int maxPositionNumber;

    // この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になる
    // 要調整が必要
    [SerializeField] private float findPatrolPointRange = 10f;

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

        if (Player.instance.HP <= 0) Player.instance.Dead();

    }


    [SerializeField] private Vector3 enemyStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => enemyStartPosition;
        set => enemyStartPosition = value;
    }


    private Vector3 lastCollisionPoint;

    //SE
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    [SerializeField] private AudioClip findPlayerSE;


    private bool wasMovingLastFrame = false; // 前フレームの移動状態を保持


    // 追従
    void ChasePlayer()
    {
        // 追従用の目的地を設定
        navMeshAgent.SetDestination(tagetPoint.position);
        Debug.Log("プレイヤー追従");
    }

    // 次の俳諧地点を決める
    void NextPosition()
    {
        if (testMap01.patrolPoint == null || testMap01.patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPointが無効です！移動を停止します。");
            navMeshAgent.isStopped = true;
            return;
        }

        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = testMap01.patrolPoint[positionNumber].position;

        NavMeshHit hit;

        // 目的地がNaveMeshの範囲内にあるかを判定する
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
        {
            navMeshAgent.destination = hit.position;
            Debug.Log($"[{gameObject.name}] 次の徘徊地点: {hit.position} (インデックス: {positionNumber})");
        }
        else
        {
            Debug.LogError("目的地がNavMeshの範囲外です: " + targetPos);

            // フォールバック：別の徘徊地点を試す
            for (int i = 0; i < testMap01.patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i + 1) % testMap01.patrolPoint.Length;
                Vector3 fallbackPos = testMap01.patrolPoint[nextIndex].position;
                if (NavMesh.SamplePosition(fallbackPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
                {
                    navMeshAgent.destination = hit.position;
                    positionNumber = nextIndex;
                    Debug.Log($"[{gameObject.name}] フォールバック先の徘徊地点に移動: {hit.position} (インデックス: {nextIndex})");
                    return;
                }
            }

            // 最後の手段：現在位置の近くのNavMesh上の位置を探す
            if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
            {
                navMeshAgent.destination = edgeHit.position;
                Debug.LogWarning($"[{gameObject.name}] 有効な徘徊地点が見つからず、最寄りのNavMeshの端に移動: {edgeHit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 有効なNavMesh位置が見つかりませんでした。移動を停止します。");
                navMeshAgent.isStopped = true;
            }
        }
    }

    public bool IsEnemyMoving()
    {
        // NavMeshAgentが有効で、経路が存在し、停止しておらず、速度がある場合に移動中と判定
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            return navMeshAgent.hasPath && !navMeshAgent.isStopped && navMeshAgent.velocity.magnitude > 0.1f;
        }
        return false;
    }

    // 衝突判定と処理を追加
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[{gameObject.name}] OnCollisionEnter が呼ばれました。衝突相手: {collision.gameObject.name}");
        Debug.Log($"[{gameObject.name}] 何かと衝突しました: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Tag: {collision.gameObject.tag}");

        // 衝突したオブジェクトが "Wall" レイヤーまたは "Wall" タグを持っているか確認
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"[{gameObject.name}] 壁と衝突しました。速度を0に設定し、方向転換を試みます。");
            navMeshAgent.velocity = Vector3.zero; // 速度を0に設定して停止させる
            navMeshAgent.isStopped = true; // NavMeshAgentの移動を停止
            animator.SetBool("isRun", false); // 停止アニメーションを再生
            animator.SetBool("isWalk", false);
            lastCollisionPoint = collision.contacts[0].point; // 衝突地点を記録
            Invoke("ChangeDirection", 0.5f); // 0.5秒後に方向転換
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }

    void ChangeDirection()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            // NavMeshAgentの停止を解除
            navMeshAgent.isStopped = false;

            // 現在位置から少し離れたランダムな方向を試す
            Vector3 randomDirection = Random.insideUnitSphere.normalized * 3f;
            Vector3 newTarget = transform.position + randomDirection;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
                Debug.Log($"[{gameObject.name}] 衝突後、ランダムな新しい目的地を設定: {hit.position}");
            }
            else
            {
                // それでも見つからなければ、現在の目的地を再設定してみる
                if (navMeshAgent.path.corners.Length > 1)
                {
                    navMeshAgent.SetDestination(navMeshAgent.path.corners[navMeshAgent.path.corners.Length - 1]);
                    Debug.LogWarning($"[{gameObject.name}] ランダムな移動先が見つからず、現在の目的地を再設定");
                }
                else
                {
                    // それもなければ、最も近い NavMesh の端を探す
                    if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                    {
                        navMeshAgent.SetDestination(edgeHit.position);
                        Debug.LogWarning($"[{gameObject.name}] 最寄りの NavMesh の端に移動");
                    }
                    else
                    {
                        Debug.LogError($"[{gameObject.name}] 有効な回避先が見つかりませんでした。");
                    }
                }
            }
        }
    }


    [SerializeField] private float raycastDistance = 1.0f; // 地面までの距離をRaycastで確認する距離
    [SerializeField] private LayerMask groundLayer; // 地面として判定するLayer

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();


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
        navMeshAgent.baseOffset = -0.1f; //baseOffsetを調整

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

        // patrolPointの各要素を検証
        for (int i = 0; i < testMap01.patrolPoint.Length; i++)
        {
            if (testMap01.patrolPoint[i] == null)
            {
                Debug.LogError($"[{gameObject.name}] patrolPoint[{i}] がnullです！停止します。");
                navMeshAgent.isStopped = true;
                return;
            }
        }


        maxPositionNumber = testMap01.patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] 初期徘徊地点: {testMap01.patrolPoint[positionNumber].position}");
    }

    void Update()
    {
        if (Player.instance.IsDead || Player.instance == null || testMap01 == null) return;

        // 移動中かどうかを判定
        IsMove = IsEnemyMoving();

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, tagetPoint.position);
        Debug.Log($"[{gameObject.name}] プレイヤーとの距離: {distance}, 検知範囲: {DetectionRange}, 現在位置: {transform.position}, プレイヤー位置: {tagetPoint.position}");

        // プレイヤーが一定の範囲に入った場合の処理
        if (distance <= DetectionRange)
        {
            ChasePlayer();

            // 追従中はRunアニメーションを再生
            animator.SetBool("isRun", IsMove);
            animator.SetBool("isWalk", false); // Walkを無効化


        }
        else
        {
            // 徘徊中はWalkアニメーションを再生
            animator.SetBool("isRun", false); // Runを無効化
            animator.SetBool("isWalk", IsMove);

            // 敵と俳徊地点の距離が指定の値の範囲内の場合の処理
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                NextPosition();
            }
        }

        // 移動状態の変化を検知して効果音を制御
        AudioClip currentSE = distance <= DetectionRange ? runSE : walkSE;

        if (IsMove && !wasMovingLastFrame)
        {
            // 移動開始時に効果音を再生
            MusicController.Instance.LoopPlayAudioSE(currentSE);
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            // 移動停止時に効果音を停止
            MusicController.Instance.StopSE(walkSE);
            MusicController.Instance.StopSE(runSE);
        }
        else if (IsMove && wasMovingLastFrame && MusicController.Instance.IsPlayingSE()
            && MusicController.Instance.GetCurrentSE() != currentSE)
        {
            // 移動中に歩行/ダッシュが切り替わった場合、効果音を変更
            MusicController.Instance.StopSE(walkSE);
            MusicController.Instance.StopSE(runSE);
            MusicController.Instance.LoopPlayAudioSE(currentSE);
        }

        // 現在の移動状態を記録
        wasMovingLastFrame = IsMove;



        // Raycastで地面との距離をチェックし、NavMeshAgentのbaseOffsetを調整する
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            // 自身の中心と地面の距離を調整
            navMeshAgent.baseOffset = -hit.distance;
            Debug.Log($"[{gameObject.name}] 地面との距離: {hit.distance}, baseOffset: {navMeshAgent.baseOffset}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 地面を検知できませんでした。baseOffsetは調整されません。");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }
}