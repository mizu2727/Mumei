using Cysharp.Threading.Tasks;
using System;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static Player;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public partial class BaseEnemy : MonoBehaviour, CharacterInterface
{
    [Header("敵の情報(Prefabをアタッチ)")]
    [SerializeField] private EnemyInformation enemyInformation;


    [Header("彷徨う者の名前のステータス(直接編集すること)")]
    [SerializeField] protected EnemyName enemyNameStatus;

    /// <summary>
    /// 彷徨う者の名前
    /// </summary>
    protected enum EnemyName
    {
        /// <summary>
        /// 静声に熱する彷徨う者
        /// </summary>
        VeinVainWanderer,

        /// <summary>
        /// 微美しき魅忘の彷徨う者
        /// </summary>
        BeauteousBewilderWanderer,

        /// <summary>
        /// 唄歌う彷徨う者
        /// </summary>
        SingSongWanderer,
    }


    [Header("アニメーション")]
    public Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }

    /// <summary>
    /// ステート名:Attack
    /// </summary>
    private const string kAttackStateName = "Attack";

    /// <summary>
    /// "isWalk"
    /// </summary>
    protected const string kIsWalkAnimatorParameter = "isWalk";

    /// <summary>
    /// "isRun"
    /// </summary>
    protected const string kIsRunAnimatorParameter = "isRun";

    /// <summary>
    /// "isAttack"
    /// </summary>
    private const string kIsAttackAnimatorParameter = "isAttack";

    /// <summary>
    /// "isDamage"
    /// </summary>
    private const string kIsDamageAnimatorParameter = "isDamage";

    /// <summary>
    /// 名前
    /// </summary>
    private string nameJapanese;

    public string CharacterName
    {
        get => nameJapanese;
        set => nameJapanese = value;
    }

    /// <summary>
    /// 名前(英語)
    /// </summary>
    private string nameEnglish;


    [Header("難易度Easyの場合の通常移動速度(直接調整すること)")]
    [SerializeField] private float easyMoveSpeed;

    [Header("難易度Normalの場合の通常移動速度(直接調整すること)")]
    [SerializeField] private float defaultMoveSpeed;

    [Header("難易度Nightmareの場合の通常移動速度(直接調整すること)")]
    [SerializeField] private float nightmareMoveSpeed;

    /// <summary>
    /// 通常移動速度
    /// </summary>
    private float Speed;
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }


    [Header("難易度Easyの場合のダッシュ移動速度(直接調整すること)")]
    [SerializeField] private float easyDashSpeed;

    [Header("難易度Normalの場合のダッシュ移動速度(直接調整すること)")]
    [SerializeField] private float defaultDashSpeed;

    [Header("難易度Nightmareの場合のダッシュ移動速度(直接調整すること)")]
    [SerializeField] private float nightmareDashSpeed;

    /// <summary>
    /// ダッシュ時の移動速度
    /// </summary>
    private float dashSpeed;
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }


    [Header("難易度Easyの場合のプレイヤーを視野できる距離(直接調整すること)")]
    [SerializeField] private float easyEnemyDetectionRange;

    [Header("難易度Normalの場合のプレイヤーを視野できる距離(直接調整すること)")]
    [SerializeField] private float normalEnemyDetectionRange;

    [Header("難易度Nightmareの場合のプレイヤーを視野できる距離(直接調整すること)")]
    [SerializeField] private float nightmareEnemyDetectionRange;

    /// <summary>
    /// プレイヤーを視野できる距離
    /// </summary>
    private float enemyDetectionRange;
    [SerializeField]
    public float DetectionRange
    {
        get => enemyDetectionRange;
        set => enemyDetectionRange = value;
    }

    /// <summary>
    /// 重力
    /// </summary>
    private float enemyGravity = 10f;
    public float Gravity
    {
        get => enemyGravity;
        set => enemyGravity = value;
    }

    /// <summary>
    /// HP
    /// </summary>
    private int enemyHP = 1;
    public int HP
    {
        get => enemyHP;
        set => enemyHP = value;
    }

    /// <summary>
    /// 死亡フラグ
    /// </summary>
    private bool enemyIsDead = false;
    public bool IsDead
    {
        get => enemyIsDead;
        set => enemyIsDead = value;
    }

    /// <summary>
    /// 移動フラグ
    /// </summary>
    private bool enemyIsMove = true;
    public bool IsMove
    {
        get => enemyIsMove;
        set => enemyIsMove = value;
    }

    /// <summary>
    /// ダッシュフラグ
    /// </summary>
    private bool enemyIsDash = true;
    public bool IsDash
    {
        get => enemyIsDash;
        set => enemyIsDash = value;
    }

    /// <summary>
    /// 振り返りフラグ
    /// </summary>
    private bool enemyIsBackRotate = false;
    public bool IsBackRotate
    {
        get => enemyIsBackRotate;
        set => enemyIsBackRotate = value;
    }

    /// <summary>
    /// 照明フラグ
    /// </summary>
    private bool enemyIsLight = true;
    public bool IsLight
    {
        get => enemyIsLight;
        set => enemyIsLight = value;
    }

    /// <summary>
    /// ダメージを受けるフラグ
    /// </summary>
    private bool isReceiveDamage;

    /// <summary>
    /// ダメージカウント
    /// </summary>
    private float receiveDamageCount;

    /// <summary>
    /// 最大ダメージカウント
    /// </summary>
    private const float kMaxReceiveDamageCount = 8.0f;

    /// <summary>
    /// 死亡メソッド
    /// </summary>
    public void Dead()
    {
        Debug.Log("Enemy Dead");
    }

    /// <summary>
    /// Rigidbody
    /// </summary>
    protected Rigidbody rigidBody;


    /// <summary>
    /// 攻撃状態モード
    /// </summary>
    private AttackMode attackMode;

    enum AttackMode
    {
        /// <summary>
        /// 何もない
        /// </summary>
        None,

        /// <summary>
        /// 攻撃モード開始
        /// </summary>
        StartAttack,
    }


    /// <summary>
    /// プレイヤーを攻撃するメソッド
    /// </summary>
    public void Attack()
    {
        Debug.Log("敵の攻撃");
    }

    [Header("自身のzoomOutGameObject(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject zoomOutGameObject;

    [Header("自身のzoomOutGameObjectHideObject(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject zoomOutGameObjectHideObject;

    /// <summary>
    /// 攻撃フラグ
    /// </summary>
    private bool isAttack = false;

    /// <summary>
    /// プレイヤーを検知(視認・音)して警戒・追従・調査を開始できるか
    /// 敵ごとに変えたい場合は派生クラスでoverrideする(既定はtrue)
    /// </summary>
    protected virtual bool CanDetectPlayer => true;

    /// <summary>
    /// BaseEnemyの移動音(歩行音・ダッシュ音)の自動再生を行うか
    /// 移動音を派生クラス側で独自に制御したい場合はoverrideしてfalseを返す(既定はtrue)
    /// </summary>
    protected virtual bool UseDefaultMoveSE => true;

    /*----------------------
     * 無限追従の演出(追従BGM・赤い画面)の保護
     ---------------------*/

    /// <summary>
    /// 無限追従の演出中の敵の一覧
    /// (追従BGMと赤い画面は全ての敵で共有しているため、無限追従中は他の敵がステージBGMへ戻したり赤い画面を消したりしないようにする)
    /// </summary>
    private static readonly System.Collections.Generic.HashSet<BaseEnemy> infinityChasePresentationEnemies
        = new System.Collections.Generic.HashSet<BaseEnemy>();

    /// <summary>
    /// 無限追従の演出中の敵が存在するか
    /// </summary>
    /// <returns>存在する場合true</returns>
    protected static bool IsInfinityChasePresentationActive()
    {
        //破棄済みの敵を除外(シーン遷移などで残らないようにする)
        infinityChasePresentationEnemies.RemoveWhere(enemy => enemy == null);

        return infinityChasePresentationEnemies.Count > 0;
    }

    /// <summary>
    /// 自身を無限追従の演出中として登録・解除する
    /// </summary>
    /// <param name="isActive">trueで登録、falseで解除</param>
    protected void SetInfinityChasePresentation(bool isActive)
    {
        if (isActive)
        {
            infinityChasePresentationEnemies.Add(this);
        }
        else
        {
            infinityChasePresentationEnemies.Remove(this);
        }
    }

    /// <summary>
    /// 追従BGMからステージBGMへ切り替える(無限追従の演出中は切り替えない)
    /// </summary>
    protected void ChangeBGMToStageIfAllowed()
    {
        //無限追従の演出中の場合
        if (IsInfinityChasePresentationActive())
        {
            //処理をスキップ
            return;
        }

        if (EnemyBGMController.instance != null)
        {
            EnemyBGMController.instance.ChangeBGMFromChasePlayerBGMToStageBGM();
        }
    }

    /// <summary>
    /// 赤い画面を非表示にする(無限追従の演出中は非表示にしない)
    /// </summary>
    protected void HidePlayerFoundPanel()
    {
        //無限追従の演出中の場合
        if (IsInfinityChasePresentationActive())
        {
            //処理をスキップ
            return;
        }

        if (playerFoundPanel != null)
        {
            playerFoundPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 攻撃アニメーションの再生速度(1 = 通常速度)
    /// 敵ごとに変えたい場合は派生クラスでoverrideする
    /// </summary>
    protected virtual float AttackAnimationSpeed => 1.0f;

    /// <summary>
    /// 攻撃アニメーションを停止させる位置(正規化時間 0～1)
    /// 敵ごとに変えたい場合は派生クラスでoverrideする
    /// </summary>
    protected virtual float AttackAnimationStopNormalizedTime => 0.8f;

    /// <summary>
    /// 攻撃演出時に、プレイヤーの正面から何m離れた位置に敵を配置するか
    /// 敵ごとに変えたい場合は派生クラスでoverrideする
    /// </summary>
    protected virtual float AttackDistanceFromPlayer => 1.5f;


    /// <summary>
    /// 初期位置
    /// </summary>
    private Vector3 enemyStartPosition;
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
        /// <summary>
        /// 通常徘徊
        /// </summary>
        Patrol,

        /// <summary>
        /// 警戒（プレイヤー発見、視線なし）
        /// </summary>
        Alert,

        /// <summary>
        ///  追従（プレイヤー視認）
        /// </summary>
        Chase,

        /// <summary>
        /// 調査（プレイヤーを見失った位置に向かう）
        /// </summary>
        Investigate,

        /// <summary>
        /// 無限追従
        /// </summary>
        InfinityChase,
    }

    /// <summary>
    /// EnemyState
    /// </summary>
    protected EnemyState currentState = EnemyState.Patrol;

    /// <summary>
    /// 保存用EnemyState
    /// </summary>
    protected EnemyState saveEnemyState;

    /// <summary>
    /// プレイヤーの最後の既知の位置
    /// </summary>
    protected Vector3 lastKnownPlayerPosition;

    /// <summary>
    /// 調査ステートへの移行までの時間
    /// </summary>
    private float changeToInvestigateTimer;

    /// <summary>
    /// 指定の調査ステートへの移行までの時間
    /// </summary>
    private const float kChangeToInvestigateDuration = 3.0f;

    /// <summary>
    /// 調査時間カウンター
    /// </summary>
    private float investigateTimer = 0f;

    /// <summary>
    /// 調査する時間（秒）
    /// </summary>
    private float investigateDuration = 5f;

    /// <summary>
    /// navMeshAgent
    /// </summary>
    protected NavMeshAgent navMeshAgent;

    [Header("徘徊関連")]
    [Header("徘徊地点を見つける範囲(この値が狭すぎると徘徊地点が見つからず、広すぎるとNaveMeshの範囲外になるため、要調整が必要)")]
    [SerializeField] private float findPatrolPointRange = 10f;

    [Header("警戒範囲(プレイヤーとの距離)")]
    [SerializeField] protected float alertRange = 15f;

    /// <summary>
    /// プレイヤーの現在地との距離
    /// </summary>
    private float distanceToPlayer;

    [Header("徘徊地点のTransform配列(ヒエラルキー上からアタッチすること)")]
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

    /// <summary>
    /// 敵の視線の高さ（プレイヤー検知、階段判定、ギズモ描画で共通使用）
    /// </summary>
    private const float kEyeHeight = 1.5f;


    /// <summary>
    /// SphereCastの球の半径
    /// </summary>
    private const float kSphereCastRadius = 0.1f;

    [Header("検知対象のレイヤー（Playerを設定すること）")]
    [SerializeField] private LayerMask detectionLayer;


    /*----------------------
     * 階段関連の処理
     ---------------------*/

    [Header("階段関連")]
    /// <summary>
    /// 階段レイヤー
    /// </summary>
    private LayerMask stairsLayer;

    [Header("視線の上下ピッチ角度の補間速度(要調整)")]
    [SerializeField] private float sightPitchSmoothSpeed = 5.0f;

    /// <summary>
    /// 階段に乗っているかどうかのフラグ
    /// </summary>
    private bool isOnStairs = false;

    /// <summary>
    /// 現在の視線の上下ピッチ角度(度)
    /// </summary>
    private float currentSightPitchAngle;

    /// <summary>
    /// 傾斜サンプリング用の前後オフセット距離。急な階段でもレイが階段内に収まるよう、前後のサンプリング距離を短めに設定（前後nメートル）
    /// </summary>
    private const float kStairsSampleOffset = 0.4f;

    /// <summary>
    /// 高低差があっても確実に地面（階段）に届くレイの長さ
    /// </summary>
    private const float kStairsEayLength = 5.0f;


    /*----------------------
     * SE関連の処理
     ---------------------*/

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// 歩行音・ダッシュ音用のaudioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// 歩行音のID
    /// </summary>
    private int walkSEid;

    /// <summary>
    /// ダッシュ音のID
    /// </summary>
    private int runSEid;

    /// <summary>
    /// 歩行音のIDを取得(派生クラスで移動音を独自に制御する用)
    /// </summary>
    protected int WalkSEid => walkSEid;

    /// <summary>
    /// プレイヤーを探す音用のaudioSource
    /// </summary>
    private AudioSource audioSourceFindPlayerSE;

    /// <summary>
    /// プレイヤーを探す音のID
    /// </summary>
    private int findPlayerSEid;

    /// <summary>
    /// プレイヤーを襲う音用のaudioSource
    /// </summary>
    private AudioSource audioSourceAttackPlayerSE;

    /// <summary>
    /// プレイヤーを襲う音のID
    /// </summary>
    private readonly int attackPlayerSEid = 16;

    /// <summary>
    /// 現在再生中の効果音
    /// </summary>
    private AudioClip currentSE;

    [Header("走る音の再生速度(直接調整すること)")]
    [SerializeField] private float runSEPitch;


    /// <summary>
    /// 音量が最大になる距離
    /// </summary>
    private float maxSoundDistance = 10f;

    /// <summary>
    /// 音量が最小になる距離
    /// </summary>
    private float minSoundDistance = 50f;

    /// <summary>
    /// 音が届く最大高さ差（Y軸制限）
    /// </summary>
    protected const float maxSoundVerticalRange = 10.0f;

    /// <summary>
    /// 最大音量
    /// </summary>
    private const float maxVolume = 1.0f;

    /// <summary>
    /// 最小音量
    /// </summary>
    private const float minVolume = 0.0f;

    /// <summary>
    /// マスター音量
    /// </summary>
    private float masterSEVolume = 1.0f;


    [Header("プレイヤー発見時のパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject playerFoundPanel;


    [Header("音検知時のパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] protected GameObject noiseScreenPanel;


    /// <summary>
    /// 前フレームの移動状態フラグ
    /// </summary>
    private bool wasMovingLastFrame = false;


    [Header("プレイヤーが視野内にいるかを判定(ヒエラルキー上での編集禁止)")]
    public bool isAlertMode = false;

    /// <summary>
    /// プレイヤーを探すSE再生中フラグ
    /// </summary>
    private bool isPlayFindPlayerSE;


    /// <summary>
    /// ドア
    /// </summary>
    private Door door;

    /// <summary>
    /// 対象の開閉したいドア
    /// </summary>
    GameObject gameObjectDoor;


    /*----------------------
     * スタック(移動停止)対策関連
     ---------------------*/

    [Header("スタック検知関連")]
    [Header("移動量をチェックする間隔(秒)")]
    [SerializeField] private float stuckCheckInterval = 0.5f;

    [Header("チェック間隔内にこの距離(m)以上動いていなければ「進めていない」とみなす")]
    [SerializeField] private float stuckMoveThreshold = 0.15f;

    [Header("進めていない状態がこの秒数続いたらスタックと判定して復帰処理を行う")]
    [SerializeField] private float stuckTimeToRecover = 1.5f;

    [Header("スタック復帰時にログを出す(原因調査用)")]
    [SerializeField] private bool isLogStuck = true;

    /// <summary>
    /// 移動量チェック用タイマー
    /// </summary>
    private float stuckCheckTimer;

    /// <summary>
    /// 進めていない状態が続いている時間
    /// </summary>
    private float stuckTimer;

    /// <summary>
    /// 前回チェック時の位置
    /// </summary>
    private Vector3 stuckCheckLastPosition;

    /// <summary>
    /// 連続でスタック復帰した回数
    /// </summary>
    private int stuckRecoverCount;

    /// <summary>
    /// 壁に衝突して方向転換待ちをしているフラグ
    /// (この間だけ意図的にisStoppedをtrueにしている)
    /// </summary>
    private bool isWallAvoidWaiting = false;

    /// <summary>
    /// 壁衝突後、方向転換するまでの待機時間
    /// </summary>
    private const float kWallAvoidWaitTime = 0.5f;

    /// <summary>
    /// 壁衝突時の方向転換で探すランダム地点までの距離
    /// </summary>
    private const float kChangeDirectionDistance = 3.0f;

    /// <summary>
    /// 方向転換先を探す試行回数
    /// </summary>
    private const int kChangeDirectionTryCount = 8;

    /// <summary>
    /// 経路計算用(毎回newしないよう使い回す)
    /// </summary>
    private NavMeshPath workPath;

    /// <summary>
    /// ダメージを受けるフラグを取得
    /// </summary>
    /// <returns>ダメージを受けるフラグ</returns>
    public bool GetIsReceiveDamage()
    {
        return isReceiveDamage;
    }

    /// <summary>
    /// ダメージを受けるフラグを設定する
    /// </summary>
    /// <param name="valueFlag">ダメージを受けるフラグ</param>
    public void SetIsReceiveDamage(bool valueFlag)
    {
        isReceiveDamage = valueFlag;
    }

    /// <summary>
    /// 歩行音・ダッシュ音用のaudioSourceを取得
    /// </summary>
    /// <returns>歩行音・ダッシュ音用のaudioSource</returns>
    public AudioSource GetAudioSourceSE()
    {
        return audioSourceSE;
    }

    /// <summary>
    /// 現在再生中の効果音を取得
    /// </summary>
    /// <returns>現在再生中の効果音</returns>
    public AudioClip GetCurrentSE()
    {
        return currentSE;
    }

    /// <summary>
    /// プレイヤーを探す音用のaudioSourceを取得
    /// </summary>
    /// <returns>プレイヤーを探す音用のaudioSource</returns>
    public AudioSource GetAudioSourceFindPlayerSE()
    {
        return audioSourceFindPlayerSE;
    }

    /// <summary>
    /// プレイヤーを探す音のIDを取得
    /// </summary>
    /// <returns>プレイヤーを探す音のID</returns>
    public int GetFindPlayerSEid()
    {
        return findPlayerSEid;
    }

    /// <summary>
    /// プレイヤーを探すSE再生中フラグを取得
    /// </summary>
    /// <returns>プレイヤーを探すSE再生中フラグ</returns>
    public bool GetIsPlayFindPlayerSE()
    {
        return isPlayFindPlayerSE;
    }


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

    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        masterSEVolume = volume;
        audioSourceFindPlayerSE.volume = volume;
        audioSourceAttackPlayerSE.volume = volume;
    }

    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    void OnDestroy()
    {
        //無限追従の演出中の登録を解除
        infinityChasePresentationEnemies.Remove(this);

        for (int i = 0; i < patrolPoint.Length; i++)
        {
            //patrolPointが存在する場合
            if (patrolPoint[i] != null)
            {
                //patrolPointをnullに設定
                patrolPoint[i] = null;
            }
        }

        //targetPointが存在する場合
        if (targetPoint != null)
        {
            //targetPointをnullに設定
            targetPoint = null;
        }

        //playerFoundPanelが存在する場合
        if (playerFoundPanel != null)
        {
            //playerFoundPanelをnullに設定
            playerFoundPanel = null;
        }

        //noiseScreenPanelが存在する場合
        if (noiseScreenPanel != null)
        {
            //noiseScreenPanelをnullに設定
            noiseScreenPanel = null;
        }

        //zoomOutGameObjectが存在する場合
        if (zoomOutGameObject != null)
        {
            //zoomOutGameObjectをnullに設定
            zoomOutGameObject = null;
        }
    }

    void Start()
    {
        //Audio関連の初期化
        InitializeAudioSource();

        //アニメーションのAnimatorを取得
        PlayAnimator = GetComponent<Animator>();

        //NavMesh関連の初期化
        InitializeNavMesh();

        //モデルの回転を初期化
        transform.rotation = Quaternion.identity;

        //階段レイヤーを取得
        stairsLayer = LayerMask.GetMask(CommonController.instance.GetStairGroundLayer());

        //スタック検知の初期化
        ResetStuckCheck();

        //プレイヤーを探すSE再生中フラグの初期化
        isPlayFindPlayerSE = false;

        //攻撃関連の初期化
        InitializeAttack();

        //rigidbodyを取得
        rigidBody = GetComponent<Rigidbody>();

        //ダメージ関連を初期化する
        InitializeDamage();

        //彷徨う者の名前のステータスに応じて各設定をする
        switch (enemyNameStatus)
        {
            //静声に熱する彷徨う者
            case EnemyName.VeinVainWanderer:

                //名前を設定
                nameJapanese = enemyInformation.enemyInformation[1].nameJapanese;
                nameEnglish = enemyInformation.enemyInformation[1].nameEnglish;

                //SEのIDを設定
                walkSEid = enemyInformation.enemyInformation[1].walkSEId;
                runSEid = enemyInformation.enemyInformation[1].runSEId;
                findPlayerSEid = enemyInformation.enemyInformation[1].findPlayerSEId;

                break;

            //微美しき魅忘の彷徨う者
            case EnemyName.BeauteousBewilderWanderer:

                //名前を設定
                nameJapanese = enemyInformation.enemyInformation[2].nameJapanese;
                nameEnglish = enemyInformation.enemyInformation[2].nameEnglish;

                //SEのIDを設定
                walkSEid = enemyInformation.enemyInformation[2].walkSEId;
                runSEid = enemyInformation.enemyInformation[2].runSEId;
                findPlayerSEid = enemyInformation.enemyInformation[1].findPlayerSEId;

                break;

            //唄歌う彷徨う者
            case EnemyName.SingSongWanderer:
                //名前を設定
                nameJapanese = enemyInformation.enemyInformation[3].nameJapanese;
                nameEnglish = enemyInformation.enemyInformation[3].nameEnglish;
                //SEのIDを設定
                walkSEid = enemyInformation.enemyInformation[3].walkSEId;
                runSEid = enemyInformation.enemyInformation[3].runSEId;
                findPlayerSEid = enemyInformation.enemyInformation[1].findPlayerSEId;
                break;


            default:
                Debug.LogError($"[{gameObject.name}] enemyNameStatusが未設定です。");
                break;
        }
    }

    protected virtual async void Update()
    {
        //通常プレイ以外のモードの場合
        if (GameController.instance.GetGameModeStatus() != GameModeStatus.PlayInGame)
        {
            //処理をスキップ
            return;
        }

        //プレイヤーが死亡している場合、処理をスキップ
        if (Player.instance == null || Player.instance.IsDead || targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update処理をスキップ: Player={Player.instance}, tagetPoint={targetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        //ダメージを受けた場合
        if (isReceiveDamage)
        {
            //ダメージ関連処理開始
            DamageRelatedProcessing();

            return;
        }

        //攻撃モードが開始された場合
        if (attackMode == AttackMode.StartAttack)
        {
            //プレイヤーを攻撃
            AttackPlayer();

            return;
        }

        //移動中かどうかを判定
        IsMove = IsEnemyMoving();

        //階段昇降時の視線上下ピッチ角度を更新
        UpdateStairsSightPitch();

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        //状態遷移と処理
        switch (currentState)
        {
            //通常徘徊状態
            case EnemyState.Patrol:

                //意図しない停止(isStoppedのまま)を解除
                EnsureAgentResumed();

                //歩行アニメーションを再生
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, IsMove);

                navMeshAgent.speed = Speed;

                //警戒音を停止
                StopFindPlayerSE();

                //プレイヤーが視野内にいるかをチェック(検知できない敵の場合は徘徊を続ける)
                if (CanDetectPlayer && distance <= alertRange)
                {
                    if (IsPlayerInFront())
                    {
                        //プレイヤーが視野内にいる場合、追従状態に移行
                        //一瞬だけ実行したい処理もここに記載する
                        currentState = EnemyState.Chase;
                        isAlertMode = true;
                        lastKnownPlayerPosition = targetPoint.position;

                        //唄歌う彷徨う者以外の場合&&通常プレイモード&&パネルがnull以外の場合
                        if (enemyNameStatus != EnemyName.SingSongWanderer && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && playerFoundPanel != null)
                        {
                            //画面を赤く表示
                            playerFoundPanel.SetActive(true);
                        }

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
                    //唄歌う彷徨う者以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer) 
                    {
                        //プレイヤーを追従するBGMからステージBGMへ切り替える
                        ChangeBGMToStageIfAllowed();
                    } 

                    //次の徘徊先を設定
                    NextPosition();
                }
                break;

            //警戒圏内状態
            case EnemyState.Alert:

                //意図しない停止(isStoppedのまま)を解除
                EnsureAgentResumed();

                //歩行アニメーションを再生
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, IsMove);

                //速度を設定
                navMeshAgent.speed = Speed;

                //警戒音を再生
                PlayFindPlayerSE();

                if (IsPlayerInFront())
                {
                    //プレイヤーが視野内にいる場合、追従状態に移行
                    //一瞬だけ実行したい処理もここに記載する
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;

                    //唄歌う彷徨う者以外の場合&&通常プレイモード&&パネルがnull以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && playerFoundPanel != null)
                    {
                        //画面を赤く表示
                        playerFoundPanel.SetActive(true);
                    }

                    Debug.Log("警戒圏内状態から追従状態へ");
                }
                else if (distance > alertRange)
                {
                    //プレイヤーが視野外の場合、通常徘徊状態に移行
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //通常プレイモード&&パネルがnull以外の場合
                    if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && playerFoundPanel != null)
                    {
                        //赤い画面を非表示
                        HidePlayerFoundPanel();
                    }

                    //唄歌う彷徨う者以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer) 
                    {
                        //プレイヤーを追従するBGMからステージBGMへ切り替える
                        ChangeBGMToStageIfAllowed();
                    }

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
                StopFindPlayerSE();

                //ダッシュアニメーションを再生
                animator.SetBool(kIsRunAnimatorParameter, true);
                animator.SetBool(kIsWalkAnimatorParameter, false);

                navMeshAgent.speed = dashSpeed;

                //プレイヤーを追従
                ChasePlayer();

                //追従状態を続ける
                lastKnownPlayerPosition = targetPoint.position;

                //プレイヤーが視野外に出た場合
                if (!IsPlayerInFront())
                {
                    //プレイヤーが視野外にいる場合、タイマーを進める
                    changeToInvestigateTimer += Time.deltaTime;

                    //調査ステートへの移行時間が一定時間を超えた場合
                    if (changeToInvestigateTimer > kChangeToInvestigateDuration)
                    {
                        //画面の色を元に戻す
                        HidePlayerFoundPanel();

                        //調査ステートへの移行時間をリセット
                        changeToInvestigateTimer = 0f;

                        //調査タイマーをリセット
                        investigateTimer = 0f;

                        //プレイヤーの最後の既知の位置に向かう
                        navMeshAgent.SetDestination(lastKnownPlayerPosition);

                        //調査状態に移行
                        currentState = EnemyState.Investigate;

                        Debug.Log("追従状態から調査状態へ");
                    }
                }
                //プレイヤーが視野内にいる場合
                else
                {
                    //調査ステートへの移行時間をリセット
                    changeToInvestigateTimer = 0f;

                    await UniTask.Delay(TimeSpan.FromSeconds(0.3));

                    //画面の色を元に戻す(プレイヤー死亡後に発生するエラーを防止する用にif文を追加)
                    //通常プレイモード&&パネルがnull以外の場合
                    if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && playerFoundPanel != null)
                    {
                        HidePlayerFoundPanel();
                    }
                }
                break;

            //調査状態
            case EnemyState.Investigate:

                //意図しない停止(isStoppedのまま)を解除
                EnsureAgentResumed();

                //画面の色を元に戻す
                HidePlayerFoundPanel();

                //警戒音を停止
                StopFindPlayerSE();

                //歩行アニメーション再生
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, IsMove);

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

                    //唄歌う彷徨う者以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer) 
                    {
                        //プレイヤーを追従するBGMからステージBGMへ切り替える
                        ChangeBGMToStageIfAllowed();
                    }

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

            //無限追従状態
            case EnemyState.InfinityChase:
                //警戒音を停止
                StopFindPlayerSE();

                //ダッシュアニメーションを再生
                animator.SetBool(kIsRunAnimatorParameter, true);
                animator.SetBool(kIsWalkAnimatorParameter, false);

                navMeshAgent.speed = dashSpeed;

                //プレイヤーを追従
                ChasePlayer();

                //追従状態を続ける
                lastKnownPlayerPosition = targetPoint.position;

                //調査ステートへの移行時間をリセット
                changeToInvestigateTimer = 0f;
                break;
        }

        //スタック(経路はあるのに進めていない状態)を検知して復帰させる
        CheckStuck();

        //WalkまたはDashのSEを再生するメソッド
        PlayWalkOrDashSE(distance);
    }
}