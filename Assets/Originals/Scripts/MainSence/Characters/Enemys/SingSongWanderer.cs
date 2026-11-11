using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using static GameController;
using static Player;

/// <summary>
/// 唄歌う彷徨う者
/// </summary>
public partial class SingSongWanderer : HearingEnemy
{
    [Header("デバッグ用無限追従フラグ(製品版ではオフにすること)")]
    [SerializeField] private bool debugInfinityChaseFlag;

    /*---------------------------------------------
     * 歌(walkSE)関連
     --------------------------------------------*/

    [Header("歌(walkSE)を再生する、プレイヤーとの距離(m)")]
    [SerializeField] private float singZoneDistance = 10.0f;

    /// <summary>
    /// 最後まで再生したと判定する許容誤差(秒)
    /// (再生が止まった時点の再生位置が「曲の長さ - この値」以上なら最後まで聞いたとみなす)
    /// </summary>
    private const float kSongEndTolerance = 1.0f;

    /// <summary>
    /// 歌(walkSE)用のaudioSource
    /// (BaseEnemyの移動音用audioSourceとは別にし、他の敵の移動音処理に影響させない)
    /// </summary>
    private AudioSource audioSourceSong;

    /// <summary>
    /// 歌を再生中フラグ(一時停止中も含む)
    /// </summary>
    private bool isSinging = false;

    /// <summary>
    /// 歌を一時停止中フラグ(ポーズ中など)
    /// </summary>
    private bool isSongPaused = false;

    /// <summary>
    /// 前フレームまでの歌の再生位置(秒)
    /// </summary>
    private float lastSongTime = 0f;

    /// <summary>
    /// 歌を最後まで聞いたフラグ(一度trueになったら無限追従を続ける)
    /// </summary>
    private bool isSongFinished = false;

    /// <summary>
    /// 歌(walkSE)が見つからない警告を出したフラグ(毎フレーム警告を出さないため)
    /// </summary>
    private bool isSongMissingWarned = false;

    /// <summary>
    /// BaseEnemyの移動音の自動再生を行うか
    /// 歌い終わるまではwalkSEを歌としてこのクラスで制御し、歌い終わった後(無限追従)はBaseEnemyの走る音に任せる
    /// </summary>
    protected override bool UseDefaultMoveSE => isSongFinished;


    /// <summary>
    /// 通常の攻撃アニメーションを停止させる位置
    /// </summary>
    private const float kAttackAnimationStopNormalizedTime = 1f;

    /// <summary>
    /// BaseEnemyの攻撃アニメーション停止位置をこの敵専用の値に差し替える
    /// </summary>
    protected override float AttackAnimationStopNormalizedTime => kAttackAnimationStopNormalizedTime;


    /*---------------------------------------------
     * 隠れているプレイヤーの発見演出関連
     --------------------------------------------*/

    /// <summary>
    /// ハイドポイントの種類ごとの覗き込み設定
    /// </summary>
    [Serializable]
    public class PeekSetting
    {
        [Tooltip("ハイドポイントの中心から前方へ何m離れた位置にワープするか(敵が覗き込む位置が未設定の場合のみ使用)")]
        public float frontDistance = 0.8f;

        [Tooltip("ワープ後の自身の高さの補正(m)")]
        public float bodyHeightOffset = 0f;

        [Tooltip("覗き込む高さ(ハイドポイントの位置からの高さ(m))。顔はこの位置を向く")]
        public float peekHeight = 0.3f;

        [Tooltip("顔の向きの追加補正(度)  X:+で下向き  Y:+で右向き  Z:首の傾き")]
        public Vector3 headAngleOffset = Vector3.zero;

        [Tooltip("覗き込みアニメーションのAnimatorパラメータ名(Bool)")]
        public string peekAnimatorParameter = "isHideAttack";
    }


    [Header("宝箱を覗き込む設定")]
    [SerializeField]
    private PeekSetting chestPeekSetting = new PeekSetting
    {
        frontDistance = 0.8f,
        bodyHeightOffset = 0f,
        peekHeight = 0.2f,
        headAngleOffset = new Vector3(10f, 0f, 0f),
        peekAnimatorParameter = "isPeekChest",
    };

    /// <summary>
    /// 設定情報は決定済み
    /// </summary>
    [Header("ゴミ箱を覗き込む設定(宝箱と覗く高さ・顔の向きが異なる)")]
    [SerializeField]
    private PeekSetting trashCanPeekSetting = new PeekSetting
    {
        frontDistance = 0.7f,
        bodyHeightOffset = 0f,
        peekHeight = 0.8f,
        headAngleOffset = new Vector3(5f, 0f, 5f),
        peekAnimatorParameter = "isPeekTrashCan",
    };

    /// <summary>
    /// 設定情報は決定済み
    /// </summary>
    [Header("ロッカーを覗き込む設定")]
    [SerializeField]
    private PeekSetting lockerPeekSetting = new PeekSetting
    {
        frontDistance = 0.9f,
        bodyHeightOffset = 0f,
        peekHeight = 1.0f,
        headAngleOffset = Vector3.zero,
        peekAnimatorParameter = "isHideAttack",
    };

    [Header("ベッドの下を覗き込む設定(ベッドは大きいため、ハイドポイント側で敵が覗き込む位置を設定することを推奨)")]
    [SerializeField]
    private PeekSetting bedPeekSetting = new PeekSetting
    {
        frontDistance = 1.3f,
        bodyHeightOffset = 0f,
        peekHeight = 0.2f,
        headAngleOffset = new Vector3(20f, 0f, 0f),
        peekAnimatorParameter = "isPeekBed",
    };

    [Header("ベッドの下に隠れている場合の接触判定の補助距離(m)。ベッドは大きく中心まで近づけないため長めにする。0で無効")]
    [SerializeField] private float bedFoundDistance = 2.0f;

    /// <summary>
    /// ゴミ箱を覗き込む時、プレイヤーの視点を真上に固定するフラグ
    /// </summary>
    private bool isLockPlayerViewUpOnTrashCan = true;

    /// <summary>
    /// 隠れているプレイヤーを見つけた際の音声SE
    /// </summary>
    private readonly int foundVoiceSEid = 30;

    [Header("覗き込んでから襲うまでの最低時間(秒)。音声の方が長い場合は音声が終わるまで待つ")]
    [SerializeField] private float minPeekDuration = 1.5f;

    /// <summary>
    /// 隠れたプレイヤーを襲うアニメーションのAnimatorパラメータ名(Bool)
    /// </summary>
    private const string kStringHideAttackAnimatorParameter = "isHideAttack";

    /// <summary>
    /// 隠れたプレイヤーを襲うアニメーションのステート名
    /// </summary>
    private const string kStringHideAttackStateName = "HideAttack";

    /// <summary>
    /// 隠れたプレイヤーを襲うアニメーションの再生速度(1 = 通常速度)
    /// </summary>
    private const float kHideAttackAnimationSpeed = 1.0f;

    /// <summary>
    /// 隠れたプレイヤーを襲うアニメーションを停止させる位置(正規化時間 0～1)
    /// </summary>
    private const float kHideAttackStopNormalizedTime = 1f;

    /// <summary>
    /// 隠れたプレイヤーを襲う時のSE ID(既定はBaseEnemyと同じ16)
    /// </summary>
    private const int kHideAttackSEid = 16;

    [Header("接触判定の補助距離(m)。NavMeshの都合でハイドポイントに触れられない場合用。0で無効")]
    [SerializeField] private float foundDistance = 1.2f;

    [Header("頭のTransform(任意。未設定でHumanoidの場合は自動取得)")]
    [SerializeField] private Transform headTransform;

    [Header("頭を覗き込む方向へ向ける強さ(0～1)")]
    [SerializeField, Range(0f, 1f)] private float headLookWeight = 1.0f;

    [Header("頭の向きを切り替える速さ(1秒あたりのウェイト変化量)")]
    [SerializeField] private float headTurnSpeed = 4.0f;

    /// <summary>
    /// 補助距離判定で許容する高さの差(m)(別の階のハイドポイントを拾わないため)
    /// </summary>
    private const float kFoundVerticalRange = 2.0f;

    /// <summary>
    /// アニメーションのステート遷移を待つ最大時間(秒)(Animator未設定で止まり続けるのを防ぐ)
    /// </summary>
    private const float kAnimatorStateWaitTimeout = 3.0f;

    /// <summary>
    /// 床の高さを探す範囲(m)
    /// </summary>
    private const float kGroundSampleRange = 2.0f;

    /// <summary>
    /// 発見演出中フラグ
    /// </summary>
    private bool isFoundSequenceRunning = false;

    /// <summary>
    /// プレイヤーを検知(視認・音)して警戒・追従・調査を開始できるか
    /// 歌い終わるまでは検知せず、徘徊しながら歌うだけにする
    /// ※ダメージ中は検知しない(HearingEnemyの音の調査処理がダメージ処理より優先され、
    ///   ダメージ中に状態が切り替わったり赤い画面が消えたりするのを防ぐ)
    /// </summary>
    protected override bool CanDetectPlayer => isSongFinished && !GetIsReceiveDamage();

    /// <summary>
    /// 「みーつけた」音声用のaudioSource
    /// </summary>
    private AudioSource audioSourceFoundVoice;

    /// <summary>
    /// 襲う時のSE用のaudioSource
    /// </summary>
    private AudioSource audioSourceHideAttackSE;

    /// <summary>
    /// 頭を向けるフラグ
    /// </summary>
    private bool isHeadLookActive = false;

    /// <summary>
    /// 頭を向ける位置
    /// </summary>
    private Vector3 headLookTarget;

    /// <summary>
    /// 頭の向きの追加補正
    /// </summary>
    private Vector3 currentHeadAngleOffset;

    /// <summary>
    /// 自身の向きに対する頭の回転の差分(骨の軸の向きがモデルごとに違っても正しく向けるため)
    /// </summary>
    private Quaternion headRotationOffset = Quaternion.identity;

    /// <summary>
    /// 現在の頭を向けるウェイト
    /// </summary>
    private float currentHeadWeight = 0f;


    /*
     * ※Start()を定義するとBaseEnemyのStart()が呼ばれなくなる(Unityは最派生クラスのStartのみ呼ぶ)ため、
     *   初期化はAwake()で行うこと。
     *   OnEnable()/OnDisable()も同様にBaseEnemy側が呼ばれなくなるため、このクラスでは定義しないこと。
     */
    private void Awake()
    {
        //初期化
        isSinging = false;
        isSongPaused = false;
        lastSongTime = 0f;
        isSongFinished = false;
        isSongMissingWarned = false;
        isFoundSequenceRunning = false;
        isHeadLookActive = false;
        currentHeadWeight = 0f;

        //デバッグ用の無限追従フラグがオンの場合、
        if (debugInfinityChaseFlag) 
        {
            //歌を最後まで聞いたフラグをオンにする
            isSongFinished = true;
        }
    }


    protected override void Update()
    {
        //発見演出中の場合
        if (isFoundSequenceRunning)
        {
            //処理をスキップ
            return;
        }

        //歌い終わった後に無限追従モード以外になっていた場合
        //(BaseEnemyのダメージ処理は状態を調査状態に変えたまま戻さないため、ダメージ終了後に無限追従へ戻す)
        if (isSongFinished)
        {
            if (currentState != EnemyState.InfinityChase && !GetIsReceiveDamage())
            {
                //無限追従モードに戻す
                currentState = EnemyState.InfinityChase;

                //無限追従の演出(赤い画面・追従BGM)を戻す
                PlayInfinityChasePresentation();
            }
        }
        //歌い終わる前に警戒・追従・調査状態になっていた場合(ダメージ処理で調査状態へ移行した場合など)
        else if (currentState == EnemyState.Alert || currentState == EnemyState.Chase || currentState == EnemyState.Investigate)
        {
            //通常徘徊状態へ戻す
            ReturnToPatrolWhileSinging();
        }

        base.Update();

        //プレイヤーが存在しない場合||通常プレイ以外の場合(プレイヤー死亡後のエラー防止)
        if (Player.instance == null || GameController.instance.GetGameModeStatus() != GameModeStatus.PlayInGame)
        {
            //歌を一時停止
            PauseSong();

            //処理をスキップ
            return;
        }

        //歌い終わった場合(無限追従モード、またはダメージ中)
        if (isSongFinished)
        {
            //無限追従モードの場合
            if (currentState == EnemyState.InfinityChase)
            {
                //隠れているハイドポイントに近づいたかを判定(接触判定の補助)
                CheckFoundHiddenPlayerByDistance();
            }

            //処理をスキップ
            return;
        }

        //歌(walkSE)の再生・停止・最後まで聞いたかの判定
        UpdateSong();
    }
}
