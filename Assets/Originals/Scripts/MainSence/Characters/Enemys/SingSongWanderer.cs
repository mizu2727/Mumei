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
public class SingSongWanderer : HearingEnemy
{
    /// <summary>
    /// 歌う時間
    /// </summary>
    private float singTime;

    /// <summary>
    /// 最大の歌う時間
    /// </summary>
    private const float kMaxSingTime = 35.0f;


    /// <summary>
    /// 歌う範囲の距離
    /// </summary>
    private const float kSingZoneDistance = 10.0f;


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
        public string peekAnimatorParameter = "";
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

    [Header("ゴミ箱を覗き込む設定(宝箱と覗く高さ・顔の向きが異なる)")]
    [SerializeField]
    private PeekSetting trashCanPeekSetting = new PeekSetting
    {
        frontDistance = 0.6f,
        bodyHeightOffset = 0f,
        peekHeight = 0.6f,
        headAngleOffset = new Vector3(25f, 0f, 15f),
        peekAnimatorParameter = "isPeekTrashCan",
    };

    [Header("ロッカーを覗き込む設定")]
    [SerializeField]
    private PeekSetting lockerPeekSetting = new PeekSetting
    {
        frontDistance = 0.9f,
        bodyHeightOffset = 0f,
        peekHeight = 1.0f,
        headAngleOffset = Vector3.zero,
        peekAnimatorParameter = "isPeekLocker",
    };

    [Header("「みーつけた」音声のSE ID(SO_SEに登録したIDを設定すること)")]
    [SerializeField] private int foundVoiceSEid = 30;

    [Header("覗き込んでから襲うまでの最低時間(秒)。音声の方が長い場合は音声が終わるまで待つ")]
    [SerializeField] private float minPeekDuration = 1.5f;

    [Header("襲うアニメーションのAnimatorパラメータ名(Bool)")]
    [SerializeField] private string hideAttackAnimatorParameter = "isHideAttack";

    [Header("襲うアニメーションのステート名")]
    [SerializeField] private string hideAttackStateName = "HideAttack";

    [Header("襲うアニメーションの再生速度(1 = 通常速度)")]
    [SerializeField] private float hideAttackAnimationSpeed = 1.0f;

    [Header("襲うアニメーションを停止させる位置(正規化時間 0～1)")]
    [SerializeField] private float hideAttackStopNormalizedTime = 0.8f;

    [Header("襲う時のSE ID(既定はBaseEnemyと同じ16)")]
    [SerializeField] private int hideAttackSEid = 16;

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

    /// <summary>
    /// 発見演出中フラグを取得
    /// </summary>
    /// <returns>発見演出中フラグ</returns>
    public bool GetIsFoundSequenceRunning()
    {
        return isFoundSequenceRunning;
    }


    /*
     * ※Start()を定義するとBaseEnemyのStart()が呼ばれなくなる(Unityは最派生クラスのStartのみ呼ぶ)ため、
     *   初期化はAwake()で行うこと。
     */
    private void Awake()
    {
        //初期化
        singTime = 0f;
        isFoundSequenceRunning = false;
        isHeadLookActive = false;
        currentHeadWeight = 0f;
    }


    protected override void Update()
    {
        //発見演出中の場合
        if (isFoundSequenceRunning)
        {
            //処理をスキップ
            return;
        }

        base.Update();

        //プレイヤーが存在しない場合||通常プレイ以外の場合(プレイヤー死亡後のエラー防止)
        if (Player.instance == null || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame)
        {
            //処理をスキップ
            return;
        }

        //無限追従モードの場合
        if (currentState == EnemyState.InfinityChase)
        {
            //隠れているハイドポイントに近づいたかを判定(接触判定の補助)
            CheckFoundHiddenPlayerByDistance();

            //処理をスキップ
            return;
        }

        //プレイヤーが、歌う範囲内にいる場合
        if (Vector3.Distance(transform.position, Player.instance.transform.position) <= kSingZoneDistance)
        {
            //歌う時間を加算する
            singTime += Time.deltaTime;

            //全て歌い終わった場合
            if (singTime >= kMaxSingTime)
            {
                //歌う時間をリセットする
                singTime = 0f;

                //無限追従モードに切り替える
                currentState = EnemyState.InfinityChase;
            }
        }
        //プレイヤーが、歌う範囲外にいる場合
        else
        {
            //歌う時間をリセットする
            singTime = 0f;
        }
    }

    /// <summary>
    /// アニメーション適用後に頭の向きを上書きする
    /// </summary>
    private void LateUpdate()
    {
        //頭を向けていない&&ウェイトが0の場合||頭のTransformが無い場合
        if ((!isHeadLookActive && currentHeadWeight <= 0f) || headTransform == null)
        {
            //処理をスキップ
            return;
        }

        //ウェイトを滑らかに変化させる
        float targetWeight = isHeadLookActive ? headLookWeight : 0f;
        currentHeadWeight = Mathf.MoveTowards(currentHeadWeight, targetWeight, headTurnSpeed * Time.deltaTime);

        //頭から覗き込む位置への方向
        Vector3 direction = headLookTarget - headTransform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        //覗き込む方向 + 顔の向きの補正 + 骨の軸の差分
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, transform.up)
            * Quaternion.Euler(currentHeadAngleOffset)
            * headRotationOffset;

        //アニメーションの頭の向きとブレンド
        headTransform.rotation = Quaternion.Slerp(headTransform.rotation, lookRotation, currentHeadWeight);
    }


    /*---------------------------------------------
     * ハイドポイントの接触判定
     --------------------------------------------*/

    /// <summary>
    /// オブジェクトに接触した場合の処理(BaseEnemyから呼ばれる)
    /// </summary>
    /// <param name="touchedObject">接触したオブジェクト</param>
    protected override void OnTouchObject(GameObject touchedObject)
    {
        //発見演出中||無限追従モード以外||null の場合
        if (isFoundSequenceRunning || currentState != EnemyState.InfinityChase || touchedObject == null)
        {
            //処理をスキップ
            return;
        }

        //接触したオブジェクト(またはその親)のハイドポイントを取得
        //(コライダーがハイドポイントの子オブジェクトに付いている場合にも対応)
        HiddenObject hiddenObject = touchedObject.GetComponentInParent<HiddenObject>();

        //ハイドポイント以外の場合
        if (hiddenObject == null)
        {
            //処理をスキップ
            return;
        }

        //発見演出を開始
        TryStartFoundSequence(hiddenObject);
    }

    /// <summary>
    /// 隠れているハイドポイントに一定距離まで近づいたかを判定する(接触判定の補助)
    /// </summary>
    private void CheckFoundHiddenPlayerByDistance()
    {
        //補助距離が無効||プレイヤーが隠れていない場合
        if (foundDistance <= 0f || !Player.instance.GetIsPlayerHidden())
        {
            //処理をスキップ
            return;
        }

        //プレイヤーの親(隠れているハイドポイント)を取得
        Transform playerParent = Player.instance.transform.parent;
        if (playerParent == null)
        {
            return;
        }

        HiddenObject hiddenObject = playerParent.GetComponent<HiddenObject>();
        if (hiddenObject == null)
        {
            return;
        }

        //水平距離と高さの差を計算
        Vector3 diff = hiddenObject.transform.position - transform.position;
        float heightDiff = Mathf.Abs(diff.y);
        diff.y = 0f;

        //一定距離まで近づいた場合
        if (diff.magnitude <= foundDistance && heightDiff <= kFoundVerticalRange)
        {
            //発見演出を開始
            TryStartFoundSequence(hiddenObject);
        }
    }

    /// <summary>
    /// 条件を満たしていれば発見演出を開始する
    /// </summary>
    /// <param name="hiddenObject">接触したハイドポイント</param>
    private void TryStartFoundSequence(HiddenObject hiddenObject)
    {
        //発見演出中||通常プレイ以外の場合
        if (isFoundSequenceRunning || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame)
        {
            return;
        }

        //プレイヤーが存在しない||死亡している場合
        if (Player.instance == null || Player.instance.IsDead)
        {
            return;
        }

        //このハイドポイントの子オブジェクトにプレイヤーが存在しない場合
        if (!hiddenObject.IsPlayerHiddenInside())
        {
            return;
        }

        //扉の開閉シーケンス中(出入りの途中)の場合
        if (hiddenObject.GetIsDoorSequenceRunning())
        {
            return;
        }

        //発見演出中フラグをオンにする
        isFoundSequenceRunning = true;

        //プレイヤーの死亡状態モードを死亡へ設定(ハイドポイントから出られなくする)
        Player.instance.SetDieMode(DieMode.Die);

        Debug.Log($"[{gameObject.name}] 隠れているプレイヤーを発見:{hiddenObject.name}({hiddenObject.GetHidePointType()})");

        //発見演出を開始
        FoundHiddenPlayerSequenceAsync(hiddenObject, this.GetCancellationTokenOnDestroy()).Forget();
    }


    /*---------------------------------------------
     * 発見演出
     --------------------------------------------*/

    /// <summary>
    /// 隠れているプレイヤーを発見した時の演出
    /// ワープ → (ロッカーの場合は扉を開ける) → 覗き込み+「みーつけた」 → 襲う → プレイヤー死亡
    /// </summary>
    /// <param name="hiddenObject">プレイヤーが隠れているハイドポイント</param>
    /// <param name="token">キャンセルトークン</param>
    private async UniTaskVoid FoundHiddenPlayerSequenceAsync(HiddenObject hiddenObject, CancellationToken token)
    {
        try
        {
            //ハイドポイントの種類に応じた覗き込み設定を取得
            HiddenObject.HidePointType hidePointType = hiddenObject.GetHidePointType();
            PeekSetting setting = GetPeekSetting(hidePointType);

            //ゲームモードをプレイヤーを攻撃する演出モードに設定(他の敵・プレイヤーの処理を止める)
            GameController.instance.gameModeStatus = GameModeStatus.AttackMovieDirection;

            //画面エフェクトを非表示
            if (playerFoundPanel != null) playerFoundPanel.SetActive(false);
            if (noiseScreenPanel != null) noiseScreenPanel.SetActive(false);

            //移動を停止
            StopMovementForDirection();

            //BGM・移動音・警戒音を停止
            StopSoundsForDirection();

            //ハイドポイントの前方へワープ
            WarpToPeekPosition(hiddenObject, setting);

            //プレイヤーの視点を敵の方へ向ける
            await SetupPlayerViewAsync(token);

            //ロッカーの場合、扉のみを開ける(プレイヤーは外に出さない)
            if (hidePointType == HiddenObject.HidePointType.Locker)
            {
                float doorWaitTime = hiddenObject.OpenDoorByEnemy();
                if (doorWaitTime > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(doorWaitTime), cancellationToken: token);
                }
            }

            //覗き込みアニメーションを再生
            animator.speed = 1.0f;
            SetAnimatorBoolSafe(setting.peekAnimatorParameter, true);

            //頭を覗き込む方向へ向ける(覗く高さ・顔の向きを反映)
            StartHeadLook(hiddenObject, setting);

            //「みーつけた」音声を再生
            float voiceLength = PlayFoundVoice();

            //音声が終わるまで(最低minPeekDuration秒)覗き込む
            await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(voiceLength, minPeekDuration)), cancellationToken: token);

            //覗き込みを終了
            SetAnimatorBoolSafe(setting.peekAnimatorParameter, false);
            isHeadLookActive = false;

            //襲う演出
            await PlayHideAttackAsync(token);

            //プレイヤーが既に存在しない場合
            if (Player.instance == null)
            {
                return;
            }

            //見つかった時点で即死させる
            Player.instance.HP = 0;

            //シーン遷移時用データを保存
            GameController.instance.CallSaveSceneTransitionUserDataMethod();

            //プレイヤー死亡
            Player.instance.Dead();
        }
        catch (OperationCanceledException)
        {
            //オブジェクト破棄やシーン遷移によるキャンセルは正常系のため何もしない
        }
    }

    /// <summary>
    /// ハイドポイントの種類に応じた覗き込み設定を取得
    /// </summary>
    /// <param name="hidePointType">ハイドポイントの種類</param>
    /// <returns>覗き込み設定</returns>
    private PeekSetting GetPeekSetting(HiddenObject.HidePointType hidePointType)
    {
        switch (hidePointType)
        {
            case HiddenObject.HidePointType.TrashCan:
                return trashCanPeekSetting;

            case HiddenObject.HidePointType.Locker:
                return lockerPeekSetting;

            case HiddenObject.HidePointType.Chest:
            default:
                return chestPeekSetting;
        }
    }

    /// <summary>
    /// 演出用に移動を停止する
    /// </summary>
    private void StopMovementForDirection()
    {
        if (navMeshAgent != null)
        {
            if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.ResetPath();
            }

            //位置・回転をNavMeshAgentに任せない(ワープ後にスクリプトで位置を決めるため)
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }

        if (rigidBody != null)
        {
            if (!rigidBody.isKinematic)
            {
                rigidBody.linearVelocity = Vector3.zero;
                rigidBody.angularVelocity = Vector3.zero;
            }

            //物理挙動(慣性や当たり判定の押し出し)で位置がズレるのを防ぐ
            rigidBody.isKinematic = true;
            rigidBody.freezeRotation = true;
        }

        //移動アニメーションオフ
        animator.SetBool(kIsRunAnimatorParameter, false);
        animator.SetBool(kIsWalkAnimatorParameter, false);
    }

    /// <summary>
    /// 演出用にBGM・移動音・警戒音を停止する
    /// </summary>
    private void StopSoundsForDirection()
    {
        //警戒音を停止
        StopFindPlayerSE();

        //移動音を停止
        AudioSource moveSE = GetAudioSourceSE();
        if (moveSE != null)
        {
            moveSE.Stop();
        }

        //ステージBGMを停止する
        if (Stage01Controller.instance != null)
        {
            Stage01Controller.instance.StopStageBGM();
        }

        //プレイヤーを追従するBGMを停止する
        if (EnemyBGMController.instance != null)
        {
            EnemyBGMController.instance.StopChasePlayerBGM();
        }
    }

    /// <summary>
    /// ハイドポイントの前方(覗き込む位置)へワープする
    /// </summary>
    /// <param name="hiddenObject">ハイドポイント</param>
    /// <param name="setting">覗き込み設定</param>
    private void WarpToPeekPosition(HiddenObject hiddenObject, PeekSetting setting)
    {
        Vector3 warpPosition;
        Quaternion warpRotation;

        Transform peekPoint = hiddenObject.GetEnemyPeekPoint();

        //覗き込む位置がハイドポイント側で設定されている場合
        if (peekPoint != null)
        {
            warpPosition = peekPoint.position;

            Vector3 peekForward = peekPoint.forward;
            peekForward.y = 0f;
            warpRotation = peekForward.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(peekForward.normalized) : peekPoint.rotation;
        }
        //未設定の場合はハイドポイントの前方へ自動配置
        else
        {
            //ハイドポイントの前方(HiddenObjectでプレイヤーが出てくる方向と同じ)
            Vector3 hideForward = hiddenObject.transform.forward;
            hideForward.y = 0f;

            //前方が取れない場合(真上・真下を向いている等)は自身がいる方向を前方とする
            if (hideForward.sqrMagnitude < 0.0001f)
            {
                hideForward = transform.position - hiddenObject.transform.position;
                hideForward.y = 0f;
            }
            hideForward.Normalize();

            warpPosition = hiddenObject.transform.position + hideForward * setting.frontDistance;

            //床の高さに合わせる(XZは変えずにYのみ採用)
            if (NavMesh.SamplePosition(warpPosition, out NavMeshHit hit, kGroundSampleRange, NavMesh.AllAreas))
            {
                warpPosition.y = hit.position.y;
            }
            else
            {
                warpPosition.y = transform.position.y;
            }

            //ハイドポイントの方を向く
            warpRotation = Quaternion.LookRotation(-hideForward);
        }

        //高さを補正
        warpPosition.y += setting.bodyHeightOffset;

        //ワープ
        transform.SetPositionAndRotation(warpPosition, warpRotation);
    }

    /// <summary>
    /// プレイヤーの視点を敵の方へ向ける(BaseEnemyの襲う演出と同様の手順)
    /// </summary>
    /// <param name="token">キャンセルトークン</param>
    private async UniTask SetupPlayerViewAsync(CancellationToken token)
    {
        if (Player.instance == null)
        {
            return;
        }

        //敵がプレイヤーを襲う際のエフェクト画面を表示
        if (EnemyAttackScreen.instance != null)
        {
            EnemyAttackScreen.instance.SetIsViewEnemyAttackScreen(true);
        }

        //プレイヤーの身体のパーツを非表示にする(演出でプレイヤー自身が見えてしまうのを防ぐため)
        for (int i = 0; i < Player.instance.playerBodys.Length; i++)
        {
            if (Player.instance.playerBodys[i] != null)
            {
                Player.instance.playerBodys[i].SetActive(false);
            }
        }

        if (PlayerCamera.instance != null)
        {
            //カメラの上下左右回転をリセット
            PlayerCamera.instance.ResetCameraRotation();
            PlayerCamera.instance.SetIsResetXRotate(true);
            PlayerCamera.instance.SetIsResetYRotate(true);

            await UniTask.Delay(TimeSpan.FromSeconds(0.1), cancellationToken: token);

            PlayerCamera.instance.SetIsResetXRotate(false);
            PlayerCamera.instance.SetIsResetYRotate(false);
        }

        if (Player.instance == null)
        {
            return;
        }

        //プレイヤー(ハイドポイントの中)から敵の方へ水平に向ける
        Vector3 toEnemy = transform.position - Player.instance.transform.position;
        toEnemy.y = 0f;
        if (toEnemy.sqrMagnitude > 0.0001f)
        {
            Player.instance.transform.rotation = Quaternion.LookRotation(toEnemy.normalized);
        }
    }

    /// <summary>
    /// 頭を覗き込む方向へ向け始める
    /// </summary>
    /// <param name="hiddenObject">ハイドポイント</param>
    /// <param name="setting">覗き込み設定</param>
    private void StartHeadLook(HiddenObject hiddenObject, PeekSetting setting)
    {
        //頭のTransformが未設定でHumanoidの場合は自動取得
        if (headTransform == null && animator != null && animator.isHuman)
        {
            headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        if (headTransform == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 頭のTransformが見つからないため、顔の向きの補正を行いません。");
            return;
        }

        //覗き込む位置(覗く高さ)
        headLookTarget = hiddenObject.transform.position + Vector3.up * setting.peekHeight;

        //顔の向きの追加補正
        currentHeadAngleOffset = setting.headAngleOffset;

        //自身の向きに対する頭の回転の差分を記録
        headRotationOffset = Quaternion.Inverse(transform.rotation) * headTransform.rotation;

        currentHeadWeight = 0f;
        isHeadLookActive = true;
    }

    /// <summary>
    /// 「みーつけた」音声を再生する
    /// </summary>
    /// <returns>音声の長さ(秒)</returns>
    private float PlayFoundVoice()
    {
        if (foundVoiceSEid < 0 || sO_SE == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 「みーつけた」音声のSE IDが未設定です。");
            return 0f;
        }

        AudioClip clip = sO_SE.GetSEClip(foundVoiceSEid);
        if (clip == null)
        {
            return 0f;
        }

        audioSourceFoundVoice = CreateSEAudioSourceIfNeeded(audioSourceFoundVoice);
        audioSourceFoundVoice.loop = false;
        audioSourceFoundVoice.PlayOneShot(clip);

        return clip.length;
    }

    /// <summary>
    /// 襲う演出(独自の襲うアニメーション)
    /// </summary>
    /// <param name="token">キャンセルトークン</param>
    private async UniTask PlayHideAttackAsync(CancellationToken token)
    {
        //襲うSEを再生
        if (hideAttackSEid >= 0 && sO_SE != null)
        {
            AudioClip clip = sO_SE.GetSEClip(hideAttackSEid);
            if (clip != null)
            {
                audioSourceHideAttackSE = CreateSEAudioSourceIfNeeded(audioSourceHideAttackSE);
                audioSourceHideAttackSE.clip = clip;
                audioSourceHideAttackSE.loop = true;
                audioSourceHideAttackSE.Play();
            }
        }

        //襲うアニメーション再生
        SetAnimatorBoolSafe(hideAttackAnimatorParameter, true);

        //襲うアニメーションのステートに遷移するまで待つ
        bool isReached = await WaitAnimatorStateAsync(hideAttackStateName, token);
        if (!isReached)
        {
            return;
        }

        //再生速度を設定
        animator.speed = hideAttackAnimationSpeed;

        //停止位置(正規化時間)まで待つ(途中で別ステートへ抜けた場合も終了)
        float stopNormalizedTime = Mathf.Clamp01(hideAttackStopNormalizedTime);
        await UniTask.WaitUntil(() =>
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return !info.IsName(hideAttackStateName) || info.normalizedTime >= stopNormalizedTime;
        }, cancellationToken: token);

        //アニメーションを止める
        animator.speed = 0f;
    }

    /// <summary>
    /// 指定のステートになるまで待つ(タイムアウトあり)
    /// </summary>
    /// <param name="stateName">ステート名</param>
    /// <param name="token">キャンセルトークン</param>
    /// <returns>ステートに遷移できた場合true</returns>
    private async UniTask<bool> WaitAnimatorStateAsync(string stateName, CancellationToken token)
    {
        float elapsed = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            elapsed += Time.deltaTime;
            if (elapsed >= kAnimatorStateWaitTimeout)
            {
                Debug.LogWarning($"[{gameObject.name}] ステート「{stateName}」に遷移しませんでした。Animatorの遷移設定を確認してください。");
                return false;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        return true;
    }

    /// <summary>
    /// SE用のAudioSourceが無ければ生成する
    /// </summary>
    /// <param name="source">現在のAudioSource</param>
    /// <returns>使用するAudioSource</returns>
    private AudioSource CreateSEAudioSourceIfNeeded(AudioSource source)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            //2Dで再生(演出中は距離に関係なくはっきり聞こえるようにする)
            source.spatialBlend = 0f;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroup・音量を反映
        if (MusicController.instance != null)
        {
            source.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
            if (MusicController.instance.sESlider != null)
            {
                source.volume = MusicController.instance.sESlider.value;
            }
        }

        return source;
    }

    /// <summary>
    /// Animatorにパラメータが存在する場合のみBoolを設定する(未作成時の警告を分かりやすくする)
    /// </summary>
    /// <param name="parameterName">パラメータ名</param>
    /// <param name="value">値</param>
    private void SetAnimatorBoolSafe(string parameterName, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
            {
                animator.SetBool(parameterName, value);
                return;
            }
        }

        Debug.LogWarning($"[{gameObject.name}] Animatorにパラメータ「{parameterName}」(Bool)がありません。");
    }
}
