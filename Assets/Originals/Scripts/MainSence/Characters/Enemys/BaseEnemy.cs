using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random; // System.Random と UnityEngine.Random の衝突を避ける

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    // 敵のステータス
    [Header("Enemy Stats")]
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


    // NavMesh関連
    [Header("NavMesh Settings")]
    NavMeshAgent navMeshAgent;
    // この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になる
    // 要調整が必要
    [SerializeField] private float findPatrolPointRange = 10f;
    [SerializeField] private float enemyDetectionRange = 100f;
    [SerializeField] private float alertRange = 15f;
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private LayerMask sightLayer;
    [SerializeField] private float raycastDistance = 5.0f;
    [SerializeField] private LayerMask groundLayer; // 地面として判定するLayer


    // 徘徊関連
    [Header("Patrol Settings")]
    [SerializeField] private TestMap01 testMap01;//プレハブ化したオブジェクトをアタッチ
    [SerializeField] public Transform[] patrolPoint;

    private int positionNumber = 0;
    private int maxPositionNumber;


    // 検知・視線関連
    [Header("Detection Settings")]
    // 追従したいオブジェクト
    public Transform tagetPoint;//ヒエラルキー上のプレイヤーをアタッチする




    // サウンド関連
    [Header("Audio Settings")]
    private AudioSource audioSourceSE; // 敵専用のAudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    [SerializeField] private AudioClip findPlayerSE;



    private bool wasMovingLastFrame = false; // 前フレームの移動状態を保持



    
    private bool isAlertMode = false;



    void ChasePlayer()
    {
        if (tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPointがnullです。プレイヤーを設定してください。");
            return;
        }
        navMeshAgent.SetDestination(tagetPoint.position);
        navMeshAgent.speed = dashSpeed; // 追従時はダッシュ速度
        navMeshAgent.isStopped = false;
        Debug.Log($"[{gameObject.name}] プレイヤー追従: 目的地 {tagetPoint.position}");
    }

    // 次の俳諧地点を決める
    void NextPosition()
    {
        //testMap01.patrolPoint
        if (patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPointが無効です！移動を停止します。");
            navMeshAgent.isStopped = true;
            return;
        }

        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = patrolPoint[positionNumber].position;

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
            for (int i = 0; i < patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i + 1) % patrolPoint.Length;
                Vector3 fallbackPos = patrolPoint[nextIndex].position;
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

    

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();


        // MusicControllerからAudioSourceを取得
        audioSourceSE = MusicController.Instance.GetAudioSource();

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgentがアタッチされていません！");
            return;
        }

        // NavMeshAgentの初期化
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true; // 回転をNavMeshAgentに任せる
        navMeshAgent.angularSpeed = 360f; // 回転速度を適切に設定
        navMeshAgent.baseOffset = 0f; // モデルに合わせて調整

        // モデルの回転を初期化（必要に応じて）
        transform.rotation = Quaternion.identity;

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
        if (testMap01 == null || patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] testMap01またはpatrolPointが設定されていません！");
            return;
        }

        // patrolPointの各要素を検証
        for (int i = 0; i < patrolPoint.Length; i++)
        {
            if (patrolPoint[i] == null)
            {
                Debug.LogError($"[{gameObject.name}] patrolPoint[{i}] がnullです！停止します。");
                navMeshAgent.isStopped = true;
                return;
            }
        }


        maxPositionNumber = patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] 初期徘徊地点: {patrolPoint[positionNumber].position}");
    }

    void Update()
    {
        if (Player.instance == null || Player.instance.IsDead || testMap01 == null || tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update処理をスキップ: Player={Player.instance}, testMap01={testMap01}, tagetPoint={tagetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        // 移動中かどうかを判定
        IsMove = IsEnemyMoving();

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, tagetPoint.position);
        
        // 警戒モード判定
        if (distance <= alertRange)
        {
            if (!isAlertMode)
            {
                Debug.Log($"[{gameObject.name}] 警戒モード開始: プレイヤーとの距離 {distance}");
                isAlertMode = true;
                MusicController.Instance.LoopPlayAudioSE(audioSourceSE, findPlayerSE); // プレイヤー発見時のSE
            }

            if (CanSeePlayer())
            {
                ChasePlayer();
                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);
            }
            else
            {
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    NextPosition();
                }
            }
        }
        else
        {
            if (isAlertMode)
            {
                Debug.Log($"[{gameObject.name}] 警戒モード解除: プレイヤーとの距離 {distance}");
                isAlertMode = false;
            }
            animator.SetBool("isRun", false);
            animator.SetBool("isWalk", IsMove);
            navMeshAgent.speed = Speed; // 通常速度に戻す
            if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
            {
                NextPosition();
            }
        }

        // 効果音制御
        AudioClip currentSE = isAlertMode && CanSeePlayer() ? runSE : walkSE;

        if (IsMove && !wasMovingLastFrame)
        {
            // 移動開始時に適切な効果音を再生
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            // 移動停止時に効果音を停止
            MusicController.Instance.StopSE(audioSourceSE);
        }
        else if (IsMove && wasMovingLastFrame && MusicController.Instance.IsPlayingSE(audioSourceSE)
                 && MusicController.Instance.GetCurrentSE(audioSourceSE) != currentSE)
        {
            // 移動中に状態が切り替わった場合、効果音を変更
            MusicController.Instance.StopSE(audioSourceSE);
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }

        // 現在の移動状態を記録
        wasMovingLastFrame = IsMove;



        // 地面との距離調整
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            navMeshAgent.baseOffset = -hit.distance;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 地面を検知できませんでした。位置: {transform.position}");
            navMeshAgent.baseOffset = -0.1f;
        }
    }

    bool CanSeePlayer()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (tagetPoint.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f; // 視線を少し高くして頭部から出す

        // Raycastの描画（緑：ヒット、赤：ヒットなし）
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, sightRange, sightLayer))
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * hit.distance, Color.green, 0.1f);
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log($"[{gameObject.name}] プレイヤーを視認: 距離 {hit.distance}");
                return true;
            }
            else
            {
                Debug.Log($"[{gameObject.name}] 視線が障害物に遮られた: {hit.collider.name}");
                Debug.DrawRay(rayOrigin, directionToPlayer * hit.distance, Color.red, 0.1f);
            }
        }
        else
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * sightRange, Color.red, 0.1f);
        }
        return false;
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