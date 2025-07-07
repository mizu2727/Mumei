using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    [Header("敵のステータス")]
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

    [SerializeField] private bool enemyIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => enemyIsLight;
        set => enemyIsLight = value;
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


    private enum EnemyState
    {
        Patrol,      // 通常徘徊
        Alert,       // 警戒（プレイヤー発見、視線なし）
        Chase,       // 追従（プレイヤー視認）
        Investigate  // 調査（プレイヤーを見失った位置に向かう）
    }


    private EnemyState currentState = EnemyState.Patrol;
    private Vector3 lastKnownPlayerPosition; // プレイヤーの最後の既知の位置
    private float investigateTimer = 0f; // 調査時間カウンター
    private float investigateDuration = 5f; // 調査する時間（秒）



    [Header("NavMesh関連")]
    NavMeshAgent navMeshAgent;

    [Header("徘徊地点を見つける範囲(この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になるため、要調整が必要)")]
    [SerializeField] private float findPatrolPointRange = 10f;

    [Header("敵の検知範囲")]
    [SerializeField] private float enemyDetectionRange = 100f;

    [Header("警戒範囲(プレイヤーとの距離)")]
    [SerializeField] private float alertRange = 15f;


    [Header("徘徊関連")]

    [Header("(プレハブ化したオブジェクトをアタッチすること)")]
    [SerializeField] private TestMap01 testMap01;

    [Header("徘徊地点のTransform配列")]
    [SerializeField] public Transform[] patrolPoint;
    private int positionNumber = 0;
    private int maxPositionNumber;


    [Header("検知・視線関連")]
    // 
    [Header("追従したいオブジェクト(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;

    [Header("視野角")]
    [SerializeField] private float fieldOfViewAngle = 60f;

    [Header("SphereCastの球の半径")]
    [SerializeField] private float sphereCastRadius = 0.5f;

    [Header("検知対象のレイヤー（Playerを設定すること）")]
    [SerializeField] private LayerMask detectionLayer;

    [Header("障害物のレイヤー（Wallなどを設定すること）")]
    [SerializeField] private LayerMask obstacleLayer;



    [Header("サウンド関連")]
    private AudioSource audioSourceSE; 
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    [SerializeField] private AudioClip findPlayerSE;

    [Header("走る音の再生速度(要調整)")]
    [SerializeField] private float runSEPitch = 2f;

    [Header("サウンドの距離関連(要調整)")]
    [SerializeField] private float maxSoundDistance = 10f; // 音量が最大になる距離
    [SerializeField] private float minSoundDistance = 20f; // 音量が最小になる距離
    [SerializeField] private float maxVolume = 1.0f; // 最大音量
    [SerializeField] private float minVolume = 0.0f; // 最小音量



    private bool wasMovingLastFrame = false; // 前フレームの移動状態を保持
 
    private bool isAlertMode = false;


    [Header("タグ・レイヤー関連")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] string wallTag = "Wall";
    [SerializeField] string doorTag = "Door";


    private Door door;
    GameObject gameObjectDoor;

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();
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
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] NavMesh位置補正に失敗しました。位置: {transform.position}");
            }
        }

        // 徘徊地点の初期化
        if ( patrolPoint == null || patrolPoint.Length == 0)
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

        //俳諧地点の初期化
        maxPositionNumber = patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] 初期徘徊地点: {patrolPoint[positionNumber].position}");
    }

    //プレイヤーが視野内にいるかをチェックする
    private bool IsPlayerInFront()
    {

        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] targetPointがnullです。");
            return false;
        }

        //プレイヤーとの処理と角度を計算
        Vector3 directionToPlayer = targetPoint.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        // プレイヤーが視野角内かつ検知範囲内にいるか
        if (distanceToPlayer <= enemyDetectionRange && angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // 視線の開始位置
            if (Physics.SphereCast(rayOrigin, sphereCastRadius, directionToPlayer.normalized, out hit, enemyDetectionRange, detectionLayer))
            {
                if (hit.collider.CompareTag(playerTag))
                {
                    // 障害物をチェック
                    if (!Physics.Linecast(rayOrigin, targetPoint.position + Vector3.up * 1.0f, obstacleLayer))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    //プレイヤーを追従する
    void ChasePlayer()
    {
        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPointがnullです。");
            navMeshAgent.isStopped = true;
            return;
        }

        //プレイヤーの位置を取得
        navMeshAgent.SetDestination(targetPoint.position);
        navMeshAgent.speed = dashSpeed;
        navMeshAgent.isStopped = false;
    }

    // 次の俳諧地点を決める
    void NextPosition()
    {
        if (patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPointが無効です！");
            navMeshAgent.isStopped = true;
            return;
        }

        //ランダムな俳諧地点を選択
        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = patrolPoint[positionNumber].position;
        NavMeshHit hit;

        //NavMesh上の位置を確認
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
        {
            navMeshAgent.destination = hit.position;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 目的地がNavMeshの範囲外: {targetPos}");
            for (int i = 0; i < patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i + 1) % patrolPoint.Length;
                Vector3 fallbackPos = patrolPoint[nextIndex].position;
                if (NavMesh.SamplePosition(fallbackPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
                {
                    navMeshAgent.destination = hit.position;
                    positionNumber = nextIndex;
                    return;
                }
            }

            if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
            {
                navMeshAgent.destination = edgeHit.position;
                Debug.LogWarning($"[{gameObject.name}] 最寄りのNavMeshの端に移動: {edgeHit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 有効なNavMesh位置が見つかりませんでした。");
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
        Debug.Log($"[{gameObject.name}] 衝突検出: {collision.gameObject.name}, タグ: {collision.gameObject.tag}");

        // 衝突したオブジェクトが "Wall" レイヤーまたは "Wall" タグを持っているか確認
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag(wallTag))
        {
            navMeshAgent.velocity = Vector3.zero; // 速度を0に設定して停止させる
            navMeshAgent.isStopped = true; // NavMeshAgentの移動を停止
            animator.SetBool("isRun", false); // 停止アニメーションを再生
            animator.SetBool("isWalk", false);
            lastCollisionPoint = collision.contacts[0].point; // 衝突地点を記録
            Invoke("ChangeDirection", 0.5f); // 0.5秒後に方向転換
        }


        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"[{gameObject.name}] プレイヤーと衝突！ HP減少処理開始");
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }

        if (collision.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collision.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            Debug.Log("敵がドアと衝突");
            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                door.OpenDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"[{gameObject.name}] 衝突検出: {collider.gameObject.name}, タグ: {collider.gameObject.tag},その2");

        if (collider.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"[{gameObject.name}] プレイヤーと衝突2！ HP減少処理開始");
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }

        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            Debug.Log("敵がドアと衝突2");
            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                door.OpenDoor();
            }
        }
    }

    //方向転換
    void ChangeDirection()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            // NavMeshAgentの停止を解除
            navMeshAgent.isStopped = false;

            // 現在位置から少し離れたランダムな方向を試す
            Vector3 randomDirection = Random.insideUnitSphere.normalized * 3f;
            Vector3 newTarget = transform.position + randomDirection;

            //NavMesh上の位置を確認
            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
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

    void Update()
    {
        if (Player.instance == null || Player.instance.IsDead  || targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update処理をスキップ: Player={Player.instance}, tagetPoint={targetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        // 移動中かどうかを判定
        IsMove = IsEnemyMoving();

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // 状態遷移と処理
        switch (currentState)
        {
            //通常徘徊
            case EnemyState.Patrol:
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                //プレイヤーが視野内にいるかをチェック
                if (distance <= alertRange)
                {
                    if (IsPlayerInFront())
                    {
                        //プレイヤーが視野内にいる場合、追従状態に移行
                        currentState = EnemyState.Chase;
                        isAlertMode = true;
                        lastKnownPlayerPosition = targetPoint.position;
                        MusicController.Instance.LoopPlayAudioSE(audioSourceSE, findPlayerSE);
                    }
                    else
                    {
                        //プレイヤーが視野内にいるが視線がない場合、警戒状態に移行
                        currentState = EnemyState.Alert;
                        isAlertMode = true;
                        MusicController.Instance.LoopPlayAudioSE(audioSourceSE, findPlayerSE);
                    }
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    NextPosition();
                }
                break;

            //警戒状態
            case EnemyState.Alert:
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                if (IsPlayerInFront())
                {
                    //プレイヤーが視野内にいる場合、追従状態に移行
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;
                }
                else if (distance > alertRange)
                {
                    //プレイヤーが視野外の場合、通常徘徊に移行
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    NextPosition();
                }
                break;

        　　 //追従状態
            case EnemyState.Chase:
                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);
                navMeshAgent.speed = dashSpeed;
                ChasePlayer();

                if (!IsPlayerInFront())
                {
                    //プレイヤーが視野外に出た場合、調査状態に移行
                    currentState = EnemyState.Investigate;
                    investigateTimer = 0f;
                    navMeshAgent.SetDestination(lastKnownPlayerPosition);
                }
                else
                {
                    //プレイヤーが視野内にいる場合、追従状態を続ける
                    lastKnownPlayerPosition = targetPoint.position;
                }
                break;

            //調査状態
            case EnemyState.Investigate:
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                investigateTimer += Time.deltaTime;
                if (IsPlayerInFront())
                {
                    //プレイヤーが視野内に戻った場合、追従状態へ移行
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;
                }
                else if (investigateTimer >= investigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                {
                    //調査時間が経過した場合、通常徘徊へ移行
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;
                }
                else if (distance <= alertRange)
                {
                    //プレイヤーが視野内にいる場合、警戒状態へ移行
                    currentState = EnemyState.Alert;
                }
                break;
        }

        // 効果音制御
        AudioClip currentSE = (currentState == EnemyState.Chase) ? runSE : walkSE;

        // 距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        if (IsMove && !wasMovingLastFrame)
        {
            // 走る音の場合、ピッチを調整
            audioSourceSE.pitch = (currentSE == runSE) ? runSEPitch : 1.0f;

            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);

            //音量を設定
            audioSourceSE.volume = volume;
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            MusicController.Instance.StopSE(audioSourceSE);

            // 停止時にピッチをリセット
            audioSourceSE.pitch = 1.0f; 
        }
        else if (IsMove && wasMovingLastFrame && MusicController.Instance.IsPlayingSE(audioSourceSE)
                 && MusicController.Instance.GetCurrentSE(audioSourceSE) != currentSE)
        {
            // 走る音の場合、ピッチを調整
            audioSourceSE.pitch = (currentSE == runSE) ? runSEPitch : 1.0f;

            MusicController.Instance.StopSE(audioSourceSE);
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);

            audioSourceSE.volume = volume;
        }
        else if (IsMove && MusicController.Instance.IsPlayingSE(audioSourceSE))
        {
            // 移動中に音量を継続的に更新
            audioSourceSE.volume = volume; 
        }

        wasMovingLastFrame = IsMove;
    }


    private void OnDrawGizmos()
    {
        // 視野範囲の可視化
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 leftRay = Quaternion.Euler(0, -halfFOV, 0) * transform.forward * enemyDetectionRange;
        Vector3 rightRay = Quaternion.Euler(0, halfFOV, 0) * transform.forward * enemyDetectionRange;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, leftRay);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, rightRay);

        // SphereCastの範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, sphereCastRadius);
        if (targetPoint != null)
        {
            Gizmos.DrawWireSphere(targetPoint.position + Vector3.up * 1.0f, sphereCastRadius);
        }
    }

    // 距離に基づく音量を計算するメソッド
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            return maxVolume; // 最大音量
        }
        else if (distance >= minSoundDistance)
        {
            return minVolume; // 最小音量
        }
        else
        {
            // 距離に基づいて線形補間
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}