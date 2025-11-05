using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static GameController;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    [Header("アニメーション")]
    public Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }

    [Header("名前")]
    [SerializeField] private string enemyName;

    [SerializeField]
    public string CharacterName
    {
        get => enemyName;
        set => enemyName = value;
    }

    [Header("通常移動速度")]
    [SerializeField] private float Speed = 4f;
    [SerializeField]
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }

    [Header("ダッシュ時の移動速度")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField]
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }

    [Header("検知範囲")]
    [SerializeField] private float enemyDetectionRange = 40f;
    [SerializeField]
    public float DetectionRange
    {
        get => enemyDetectionRange;
        set => enemyDetectionRange = value;
    }

    [Header("重力")]
    [SerializeField] private float enemyGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => enemyGravity;
        set => enemyGravity = value;
    }

    [Header("HP")]
    [SerializeField] private int enemyHP = 1;
    [SerializeField]
    public int HP
    {
        get => enemyHP;
        set => enemyHP = value;
    }

    [Header("死亡フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool enemyIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => enemyIsDead;
        set => enemyIsDead = value;
    }

    [Header("移動フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool enemyIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => enemyIsMove;
        set => enemyIsMove = value;
    }

    [Header("ダッシュフラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool enemyIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => enemyIsDash;
        set => enemyIsDash = value;
    }

    [Header("振り返りフラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool enemyIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => enemyIsBackRotate;
        set => enemyIsBackRotate = value;
    }

    [Header("照明フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool enemyIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => enemyIsLight;
        set => enemyIsLight = value;
    }

    /// <summary>
    /// 死亡メソッド
    /// </summary>
    public void Dead()
    {
        Debug.Log("Enemy Dead");
    }


    /// <summary>
    /// プレイヤーを攻撃するメソッド
    /// </summary>
    public void Attack()
    {
        if (Player.instance.IsDead) return;

        Player.instance.HP -= 1;

        if (Player.instance.HP <= 0) Player.instance.Dead();

    }

    [Header("初期位置")]
    [SerializeField] private Vector3 enemyStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => enemyStartPosition;
        set => enemyStartPosition = value;
    }

    /// <summary>
    /// 衝突地点を記録
    /// </summary>
    private Vector3 lastCollisionPoint;

   /// <summary>
   /// 移動時の状態
   /// </summary>
    public enum EnemyState
    {
        Patrol,      // 通常徘徊
        Alert,       // 警戒（プレイヤー発見、視線なし）
        Chase,       // 追従（プレイヤー視認）
        Investigate  // 調査（プレイヤーを見失った位置に向かう）
    }

    [Header("EnemyState(ヒエラルキー上での編集禁止)")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("プレイヤーの最後の既知の位置(ヒエラルキー上での編集禁止)")]
    public Vector3 lastKnownPlayerPosition;

    /// <summary>
    /// 調査時間カウンター
    /// </summary>
    private float investigateTimer = 0f;

    /// <summary>
    /// 調査する時間（秒）
    /// </summary>
    private float investigateDuration = 5f;

    [Header("navMeshAgent(ヒエラルキー上での編集禁止)")]
    public NavMeshAgent navMeshAgent;

    [Header("徘徊関連")]
    [Header("徘徊地点を見つける範囲(この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になるため、要調整が必要)")]
    [SerializeField] private float findPatrolPointRange = 10f;

    [Header("警戒範囲(プレイヤーとの距離)")]
    [SerializeField] public float alertRange = 15f;

    [Header("徘徊地点のTransform配列")]
    [SerializeField] private Transform[] patrolPoint;

    /// <summary>
    /// 徘徊地点の要素番号
    /// </summary>
    private int positionNumber = 0;

    /// <summary>
    /// 徘徊地点の最大要素番号
    /// </summary>
    private int maxPositionNumber;


    [Header("検知・視線関連")] 
    [Header("追従したいオブジェクト(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;

    [Header("視野角")]
    [SerializeField] private float fieldOfViewAngle = 60f;

    [Header("SphereCastの球の半径")]
    [SerializeField] private float sphereCastRadius = 0.5f;

    [Header("検知対象のレイヤー（Playerを設定すること）")]
    [SerializeField] private LayerMask detectionLayer;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("歩行音・ダッシュ音用のaudioSource(ヒエラルキー上での編集禁止)")]
    public AudioSource audioSourceSE;

    /// <summary>
    /// 歩行音のID
    /// </summary>
    private readonly int walkSEid = 7;

    /// <summary>
    /// ダッシュ音のID
    /// </summary>
    private readonly int runSEid = 8;

    /// <summary>
    /// プレイヤーを探す音用のaudioSource
    /// </summary>
    private AudioSource audioSourceFindPlayerSE;

    /// <summary>
    /// プレイヤーを探す音のID
    /// </summary>
    private readonly int findPlayerSEid = 9;

    [Header("現在再生中の効果音(ヒエラルキー上での編集禁止)")]
    public AudioClip currentSE;

    [Header("走る音の再生速度(要調整)")]
    [SerializeField] private float runSEPitch = 2f;


    [Header("サウンドの距離関連(要調整)")]
    [Header("音量が最大になる距離")]
    [SerializeField] private float maxSoundDistance = 10f;

    [Header("音量が最小になる距離")]
    [SerializeField] private float minSoundDistance = 20f;

    [Header("最大音量")]
    [SerializeField] private float maxVolume = 1.0f;

    [Header("最小音量")]
    [SerializeField] private float minVolume = 0.0f;

    /// <summary>
    /// マスター音量
    /// </summary>
    private float masterSEVolume = 1.0f;


    [Header("プレイヤー発見時のパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject playerFoundPanel;


    /// <summary>
    /// 前フレームの移動状態フラグ
    /// </summary>
    private bool wasMovingLastFrame = false;

    [Header("プレイヤーが視野内にいるかを判定(ヒエラルキー上での編集禁止)")]
    public bool isAlertMode = false;


    [Header("タグ・レイヤー関連")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] string wallTag = "Wall";
    [SerializeField] string doorTag = "Door";


    /// <summary>
    /// ドア
    /// </summary>
    private Door door;

    /// <summary>
    /// 対象の開閉したいドア
    /// </summary>
    GameObject gameObjectDoor;

    /// <summary>
    /// オブジェクトが生成されたタイミングで一回だけ呼ばれる関数。初期化しておきたい情報を書く
    /// </summary>
    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時にAudioSourceを再設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Player.instanceが見つかりません。targetPointを設定できません。");
            navMeshAgent.isStopped = true;
        }
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        if (MusicController.instance != null)
        {
            audioSourceSE = MusicController.instance.GetAudioSource();
            if (audioSourceSE != null)
            {
                audioSourceSE.playOnAwake = false;
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] MusicControllerからAudioSourceを取得できませんでした。");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] MusicController.instanceが見つかりません。");
        }

        audioSourceFindPlayerSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceFindPlayerSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;

        //マスター音量を同期
        masterSEVolume = MusicController.instance.sESlider.value;
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        masterSEVolume = volume;
        audioSourceFindPlayerSE.volume = volume;
    }

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();

        //専用の新しいAudioSourceを取得
        //(別の効果音が鳴っている間に敵の効果音が鳴らないバグを防止する用)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgentがアタッチされていません！");
            return;
        }

        //targetPointをPlayer.instanceのTransformに設定
        //(シーン遷移した後にプレイヤーのtransformがnullになるエラーを防止する用)
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Player.instanceが見つかりません。targetPointを設定できません。");
            navMeshAgent.isStopped = true;
            return;
        }

        // NavMeshAgentの初期化

        navMeshAgent.isStopped = false;

        //位置をNavMeshAgentに任せる
        navMeshAgent.updatePosition = true;

        //回転をNavMeshAgentに任せる
        navMeshAgent.updateRotation = true;

        //回転速度を適切に設定すること
        navMeshAgent.angularSpeed = 360f;

        //モデルに合わせて調整すること
        navMeshAgent.baseOffset = 0f; 

        //モデルの回転を初期化
        transform.rotation = Quaternion.identity;

        //NavMesh上に配置されているか確認
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

        //徘徊地点の初期化
        if ( patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] testMap01またはpatrolPointが設定されていません！");
            return;
        }

        //patrolPointの各要素をnullチェック
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
    }

    /// <summary>
    /// プレイヤーが視野内にいるかをチェックする
    /// </summary>
    /// <returns>プレイヤーオブジェクトのLayerがヒットするとtrue</returns>
    public bool IsPlayerInFront()
    {
        //プレイヤーをnullチェック
        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] targetPointがnullです。");
            return false;
        }

        //プレイヤーとの処理と角度を計算
        Vector3 directionToPlayer = targetPoint.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        //プレイヤーが視野角内かつ検知範囲内にいるか
        if (distanceToPlayer <= enemyDetectionRange && angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;

            //視線の開始位置(敵の視線の高さ)
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;

            //プレイヤーの中心
            Vector3 targetPosition = targetPoint.position + Vector3.up * 0.5f;

            //Raycastでヒットしたオブジェクトのレイヤーをチェック
            if (Physics.SphereCast(rayOrigin, sphereCastRadius, directionToPlayer.normalized, out hit, enemyDetectionRange, detectionLayer))
            {
                if (hit.collider.CompareTag(playerTag))
                {
                    //視線経路上のすべてのオブジェクトをチェック
                    RaycastHit[] hits = Physics.RaycastAll(rayOrigin, directionToPlayer.normalized, distanceToPlayer);
                    foreach (var rayHit in hits)
                    {
                        //壁にヒットした場合
                        if (rayHit.collider.CompareTag(wallTag))
                        {
                            //プレイヤーの視認失敗
                            return false;
                        }

                        //ドアにヒットした場合
                        if (rayHit.collider.CompareTag(doorTag))
                        {
                            Door door = rayHit.collider.GetComponent<Door>();

                            //ドアが閉まっている場合
                            if (door != null && !door.isOpenDoor)
                            {
                                //プレイヤーの視認失敗
                                return false;
                            }
                        }
                    }

                    //プレイヤーの視認成功
                    return true;
                }
            }
        }

        //プレイヤーの視認失敗
        return false;
    }

    /// <summary>
    /// プレイヤーを追従する
    /// </summary>
    void ChasePlayer()
    {
        //プレイヤーをnullチェック
        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPointがnullです。");
            navMeshAgent.isStopped = true;
            return;
        }

        //プレイヤーの位置を取得
        navMeshAgent.SetDestination(targetPoint.position);

        //ダッシュ
        navMeshAgent.speed = dashSpeed;
        navMeshAgent.isStopped = false;
    }
 
    /// <summary>
    /// 次の俳諧地点を決める
    /// </summary>
    void NextPosition()
    {
        //徘徊地点をnullチェック
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

    /// <summary>
    /// 移動しているかを判定する
    /// </summary>
    /// <returns>移動中ならtrue</returns>
    public bool IsEnemyMoving()
    {
        //NavMeshAgentが有効で、経路が存在し、停止しておらず、速度がある場合に移動中と判定
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            return navMeshAgent.hasPath && !navMeshAgent.isStopped && navMeshAgent.velocity.magnitude > 0.1f;
        }
        return false;
    }

    /// <summary>
    /// オブジェクトのコリジョンと衝突した場合の処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトのコリジョン</param>
    private void OnCollisionEnter(Collision collision)
    {

        //壁に触れた場合
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag(wallTag))
        {
            //速度を0に設定して停止させる
            navMeshAgent.velocity = Vector3.zero;

            //NavMeshAgentの移動を停止
            navMeshAgent.isStopped = true;

            //停止アニメーションを再生
            animator.SetBool("isRun", false); 
            animator.SetBool("isWalk", false);

            //衝突地点を記録
            lastCollisionPoint = collision.contacts[0].point;

            //0.5秒後に方向転換
            Invoke("ChangeDirection", 0.5f); 
        }

        //プレイヤーに触れた場合
        if (collision.gameObject.CompareTag(playerTag))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                //プレイヤーを攻撃
                Attack();
            }
        }

        //ドアに触れた場合
        if (collision.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collision.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                //ドアを開ける
                door.OpenDoor();
            }
        }
    }

    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">貫通したオブジェクトのコライダー</param>
    private void OnTriggerEnter(Collider collider)
    {

        //プレイヤーに触れた場合
        if (collider.gameObject.CompareTag(playerTag))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                //プレイヤーを攻撃
                Attack();
            }
        }

        //ドアに触れた場合
        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                //ドアを開ける
                door.OpenDoor();
            }
        }
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    void ChangeDirection()
    {
        //navMeshAgentが有効であるか
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            //NavMeshAgentの停止を解除
            navMeshAgent.isStopped = false;

            //現在位置から少し離れたランダムな方向を探す
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
                //移動先が見つからない場合、現在の目的地を再設定する
                if (navMeshAgent.path.corners.Length > 1)
                {
                    navMeshAgent.SetDestination(navMeshAgent.path.corners[navMeshAgent.path.corners.Length - 1]);
                    Debug.LogWarning($"[{gameObject.name}] ランダムな移動先が見つからないため、現在の目的地を再設定");
                }
                else
                {
                    //移動先が見つからない場合、最も近いNavMeshの端を探す
                    if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                    {
                        navMeshAgent.SetDestination(edgeHit.position);
                        Debug.LogWarning($"[{gameObject.name}] 最寄りのNavMeshの端に移動");
                    }
                    else
                    {
                        Debug.LogError($"[{gameObject.name}] 有効な回避先が見つかりませんでした。");
                    }
                }
            }
        }
    }

    protected virtual async void Update()
    {
        //通常プレイ以外のモードも場合、処理をスキップ
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) return;

        //プレイヤーが死亡している場合、処理をスキップ
        if (Player.instance == null || Player.instance.IsDead  || targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update処理をスキップ: Player={Player.instance}, tagetPoint={targetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        //移動中かどうかを判定
        IsMove = IsEnemyMoving();

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        //状態遷移と処理
        switch (currentState)
        {
            //通常徘徊状態
            case EnemyState.Patrol:

                //歩行アニメーションを再生
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);

                navMeshAgent.speed = Speed;

                //警戒音を停止
                audioSourceFindPlayerSE.Stop();

                //プレイヤーが視野内にいるかをチェック
                if (distance <= alertRange)
                {
                    if (IsPlayerInFront())
                    {
                        //プレイヤーが視野内にいる場合、追従状態に移行
                        //一瞬だけ実行したい処理もここに記載する
                        currentState = EnemyState.Chase;
                        isAlertMode = true;
                        lastKnownPlayerPosition = targetPoint.position;

                        //ステージBGMからプレイヤーを追従するBGMへ切り替える
                        EnemyBGMController.instance.ChangeBGMFromStageBGMToChasePlayerBGM();

                        //画面を赤く表示
                        playerFoundPanel.SetActive(true);

                        Debug.Log("通常徘徊状態から追従状態へ");
                    }
                    else
                    {

                        //プレイヤーが視野内にいるが視線がない場合、警戒圏内状態に移行
                        currentState = EnemyState.Alert;
                        isAlertMode = true;

                        Debug.Log("通常徘徊状態から警戒圏内状態へ");
                    }
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    //次の徘徊先を設定
                    NextPosition();
                }
                break;

            //警戒圏内状態
            case EnemyState.Alert:

                //歩行アニメーションを再生
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);

                navMeshAgent.speed = Speed;

                //警戒音を再生
                audioSourceFindPlayerSE.clip = sO_SE.GetSEClip(findPlayerSEid);
                audioSourceFindPlayerSE.loop = true;
                audioSourceFindPlayerSE.Play();

                if (IsPlayerInFront())
                {

                    //プレイヤーが視野内にいる場合、追従状態に移行
                    //一瞬だけ実行したい処理もここに記載する
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;

                    //ステージBGMからプレイヤーを追従するBGMへ切り替える
                    EnemyBGMController.instance.ChangeBGMFromStageBGMToChasePlayerBGM();

                    //画面を赤く表示
                    playerFoundPanel.SetActive(true);

                    Debug.Log("警戒圏内状態から追従状態へ");
                }
                else if (distance > alertRange)
                {

                    //プレイヤーが視野外の場合、通常徘徊状態に移行
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //画面の色を元に戻す
                    playerFoundPanel.SetActive(false);

                    //プレイヤーを追従するBGMからステージBGMへ切り替える
                    EnemyBGMController.instance.ChangeBGMFromChasePlayerBGMToStageBGM();

                    Debug.Log("警戒圏内状態から通常徘徊状態へ");
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    //次の徘徊先を設定
                    NextPosition();
                }
                break;

        　　 //追従状態
            case EnemyState.Chase:
                //警戒音を停止
                audioSourceFindPlayerSE.Stop();

                //ダッシュアニメーションを再生
                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);

                navMeshAgent.speed = dashSpeed;

                //プレイヤーを追従
                ChasePlayer();

                if (!IsPlayerInFront())
                {
                    //プレイヤーが視野外に出た場合、調査状態に移行
                    currentState = EnemyState.Investigate;
                    investigateTimer = 0f;
                    navMeshAgent.SetDestination(lastKnownPlayerPosition);

                    Debug.Log("追従状態から調査状態へ");
                }
                else
                {
                    //プレイヤーが視野内にいる場合、追従状態を続ける
                    lastKnownPlayerPosition = targetPoint.position;

                    await UniTask.Delay(TimeSpan.FromSeconds(0.3));

                    //画面の色を元に戻す(プレイヤー死亡後に発生するエラーを防止する用にif文を追加)
                    if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) playerFoundPanel.SetActive(false);
                }
                break;

            //調査状態
            case EnemyState.Investigate:

                //画面の色を元に戻す
                playerFoundPanel.SetActive(false);

                //警戒音を停止
                audioSourceFindPlayerSE.Stop();

                //歩行アニメーション再生
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);

                navMeshAgent.speed = Speed;


                investigateTimer += Time.deltaTime;
                if (IsPlayerInFront())
                {
                    //プレイヤーが視野内に戻った場合、追従状態へ移行
                    //(調査状態から追従状態へ切り替える場合は、敵に追われている時のBGMはそのまま流すこと)
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;

                    Debug.Log("調査状態から追従状態へ");
                }
                else if (investigateTimer >= investigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                {
                    //調査時間が経過した場合、通常徘徊状態へ移行
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //プレイヤーを追従するBGMからステージBGMへ切り替える
                    EnemyBGMController.instance.ChangeBGMFromChasePlayerBGMToStageBGM();

                    Debug.Log("調査状態から通常徘徊状態へ");
                }
                else if (distance <= alertRange)
                {

                    //プレイヤーが視野内にいる場合、警戒圏内状態へ移行
                    //(調査状態から警戒圏内状態へ切り替える場合は、敵に追われている時のBGMはそのまま流すこと)
                    currentState = EnemyState.Alert;

                    Debug.Log("調査状態から警戒圏内状態へ");
                }
                break;
        }

        //移動時の効果音処理(プレイヤー死亡後に発生するエラーを防止する用にif文を追加)
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            // 効果音制御
            currentSE = (currentState == EnemyState.Chase) ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);

            //距離ベースの相対音量（0～1）
            float relativeVolume = CalculateVolumeBasedOnDistance(distance);

            //最終音量 = マスターSE音量 × 相対音量
            float finalVolume = masterSEVolume * relativeVolume;
            
            if (IsMove && !wasMovingLastFrame)
            {
                //移動音再生(走る音の場合、ピッチを調整)
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;
                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;
                audioSourceSE.volume = finalVolume;
                audioSourceSE.Play();
            }
            else if (!IsMove && wasMovingLastFrame)
            {
                //移動音停止
                audioSourceSE.Stop();

                // 停止時にピッチをリセット
                audioSourceSE.pitch = 1.0f;
            }
            else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
            {
                //移動音停止
                audioSourceSE.Stop();

                //移動音再生(走る音の場合、ピッチを調整)
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;
                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;
                audioSourceSE.volume = finalVolume;
                audioSourceSE.Play();
            }
            else if (IsMove && audioSourceSE.isPlaying)
            {
                //移動中は最終音量を継続的に更新
                audioSourceSE.volume = finalVolume;
            }

            wasMovingLastFrame = IsMove;
        }        
    }

    /// <summary>
    /// ギズモを描画
    /// </summary>
    private void OnDrawGizmos()
    {
        //視野範囲の可視化
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 leftRay = Quaternion.Euler(0, -halfFOV, 0) * transform.forward * enemyDetectionRange;
        Vector3 rightRay = Quaternion.Euler(0, halfFOV, 0) * transform.forward * enemyDetectionRange;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, leftRay);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, rightRay);

        //SphereCastの範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, sphereCastRadius);
        if (targetPoint != null)
        {
            Gizmos.DrawWireSphere(targetPoint.position + Vector3.up * 1.0f, sphereCastRadius);

            // Linecastの経路を可視化
            Gizmos.color = Color.red;
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = targetPoint.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(rayOrigin, targetPos);
        }
    }

    /// <summary>
    /// 距離に基づく音量を計算するメソッド
    /// </summary>
    /// <param name="distance">プレイヤーとの距離</param>
    /// <returns>音量</returns>
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            //最大音量
            return maxVolume;
        }
        else if (distance >= minSoundDistance)
        {
            //最小音量
            return minVolume; 
        }
        else
        {
            //距離に基づいて音量を調整
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}