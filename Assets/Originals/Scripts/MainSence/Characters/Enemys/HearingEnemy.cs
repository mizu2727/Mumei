using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

/// <summary>
/// 周囲の音を聞き取ることができる敵クラス
/// </summary>
public class HearingEnemy : BaseEnemy
{
    [Header("難易度Easyの場合のダッシュ音の検知範囲(直接調整すること)")]
    [SerializeField] private float easySoundDetectionRange;

    [Header("難易度Normalの場合のダッシュ音の検知範囲(直接調整すること)")]
    [SerializeField] private float normalSoundDetectionRange;

    [Header("難易度Nightmareの場合のダッシュ音の検知範囲(直接調整すること)")]
    [SerializeField] private float nightmareSoundDetectionRange;

    /// <summary>
    /// ダッシュ音の検知範囲
    /// </summary>
    private float soundDetectionRange;


    [Header("難易度Easyの場合のダッシュ音の調査時間(直接調整すること)")]
    [SerializeField] private float easySoundInvestigateDuration;

    [Header("難易度Normalの場合のダッシュ音の調査時間(直接調整すること)")]
    [SerializeField] private float normalSoundInvestigateDuration;

    [Header("難易度Nightmareの場合のダッシュ音の調査時間(直接調整すること)")]
    [SerializeField] private float nightmareSoundInvestigateDuration;

    /// <summary>
    /// ダッシュ音の調査時間
    /// </summary>
    private float soundInvestigateDuration;

    /// <summary>
    /// 調査時間カウンター
    /// </summary>
    private float soundInvestigateTimer = 0f;

    /// <summary>
    /// 最後に聞こえたプレイヤーのダッシュ音の位置
    /// </summary>
    private Vector3 lastHeardSoundPosition;

    /// <summary>
    /// ダッシュ音を調査するフラグ
    /// </summary>
    private bool isInvestigatingSound = false;

    /// <summary>
    /// 難易度Easyの場合の特定の放送SEの検知範囲
    /// </summary>
    private const float easyBroadcastSoundDetectionRange = 80;

    /// <summary>
    /// 難易度Normalの場合の特定の放送SEの検知範囲
    /// </summary>
    private const float normalBroadcastSoundDetectionRange = 100;

    /// <summary>
    /// 難易度Nightmareの場合の特定の放送SEの検知範囲
    /// </summary>
    private const float nightmareBroadcastSoundDetectionRange = 120;

    /// <summary>
    /// 特定の放送SEの検知範囲
    /// </summary>
    private float broadcastSoundDetectionRange;

    /// <summary>
    /// 放送スピーカー音を調査するフラグ
    /// </summary>
    private bool isInvestigatingBroadcastSound = false;


    /*
     * Start()を作成してしまうとエラーが発生するため、ここには使用しないこと。
     */

    protected override  void Update()
    {
        //ゲームがプレイ中でない、またはプレイヤーが死亡している場合
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null 
            || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;

            //処理をスキップ
            return;
        }

        //難易度に応じてそれぞれの設定を変更する
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus())
        {
            //難易度Easyの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //難易度Easyの用ダッシュ音の検知範囲を設定
                soundDetectionRange = easySoundDetectionRange;

                //難易度Easyの用ダッシュ音の調査時間を設定
                soundInvestigateDuration = easySoundInvestigateDuration;

                //難易度Easyの用特定の放送SEの検知範囲を設定
                broadcastSoundDetectionRange = easyBroadcastSoundDetectionRange;

                break;

            //難易度Normalの場合(デバッグ用にkNoneも追加)
            case DifficultyLevelController.DifficultyLevel.kNormal:
            case DifficultyLevelController.DifficultyLevel.kNone:

                //難易度Normalの用ダッシュ音の検知範囲を設定
                soundDetectionRange = normalSoundDetectionRange;

                //難易度Normalの用ダッシュ音の調査時間を設定
                soundInvestigateDuration = normalSoundInvestigateDuration;

                //難易度Normalの用特定の放送SEの検知範囲を設定
                broadcastSoundDetectionRange = normalBroadcastSoundDetectionRange;

                break;

            //難易度Nightmareの場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //難易度Nightmareの用ダッシュ音の検知範囲を設定
                soundDetectionRange = nightmareSoundDetectionRange;

                //難易度Nightmareの用ダッシュ音の調査時間を設定
                soundInvestigateDuration = nightmareSoundInvestigateDuration;

                //難易度Nightmareの用特定の放送SEの検知範囲を設定
                broadcastSoundDetectionRange = nightmareBroadcastSoundDetectionRange;

                break;
        }

        //プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint.position);

        //放送スピーカーとの距離を計算し、範囲内のスピーカーを検知する
        if (!isInvestigatingSound && !isInvestigatingBroadcastSound && BroadcastController.instance != null
            && currentState != EnemyState.Investigate && currentState != EnemyState.Chase)
        {
            //放送スピーカーのTransformのリストを取得
            List<Transform> speakerTransformList = BroadcastController.instance.GetBroadcastSpeakerTransformList();

            //最も近いスピーカーを見つけるための変数
            float closestDistance = float.MaxValue;
            Transform closestSpeaker = null;

            //放送スピーカーのTransformのリストをループして、最も近いスピーカーを見つける
            foreach (Transform speakerTransform in speakerTransformList)
            {
                //放送スピーカーのTransformがnullの場合
                if (speakerTransform == null) 
                { 
                    //スキップ
                    continue;
                }

                //各スピーカーのコンポーネントを取得
                BroadcastSpeaker currentSpeaker = speakerTransform.GetComponent<BroadcastSpeaker>();

                //特定の放送が流れているスピーカーのみを対象にする
                if (currentSpeaker != null && currentSpeaker.GetIsListeningBroadcast()) 
                {
                    //スピーカーとの距離を計算
                    float distanceToSpeaker = Vector3.Distance(transform.position, speakerTransform.position);

                    //特定の放送が流れているスピーカーとの距離をログ出力
                    //Debug.Log($"[デバッグ] 特定の放送中のスピーカー({speakerTransform.name})との距離: {distanceToSpeaker} (検知範囲: {broadcastSoundDetectionRange})");

                    //最も近いスピーカーを更新
                    if (distanceToSpeaker < closestDistance)
                    {
                        closestDistance = distanceToSpeaker;
                        closestSpeaker = speakerTransform;
                    }
                }
            }

            //スピーカーのコンポーネントを取得
            BroadcastSpeaker speaker = closestSpeaker != null ? closestSpeaker.GetComponent<BroadcastSpeaker>() : null;

            //最も近いスピーカーが検知範囲内にある場合&&そのスピーカーの放送ノイズを聞いているフラグがオンの場合
            //&&プレイヤーが隠れていない場合&&追従モード以外の場合、調査状態に移行
            if (closestSpeaker != null && closestDistance <= broadcastSoundDetectionRange && speaker != null 
                && speaker.GetIsListeningBroadcast() && !Player.instance.GetIsPlayerHidden() 
                && currentState != EnemyState.Investigate && currentState != EnemyState.Chase)
            {
                Debug.Log("放送スピーカー音を検知");

                //最も近いスピーカーの位置を記録
                //lastHeardSoundPosition = closestSpeaker.position;

                //プレイヤーの位置へ移動
                lastHeardSoundPosition = targetPoint.position;

                
                isInvestigatingBroadcastSound = true;
                soundInvestigateTimer = 0f;

                //調査状態に移行
                currentState = EnemyState.Investigate;

                //スピーカーの位置へ移動
                navMeshAgent.SetDestination(lastHeardSoundPosition);
            }
        }

        //(プレイヤーのダッシュ音を検知||音を鳴らしてしまった場合)&&追従モード以外の場合
        //プレイヤー追従時にダッシュ音を検知してしまうと追従状態から調査状態に移行してしまうため、追従モード以外の場合に限定する
        if ((Player.instance.IsDash || Player.instance.GetIsMakeSound()) && !isInvestigatingSound && !Player.instance.GetIsPlayerHidden()
            && distanceToPlayer <= soundDetectionRange && currentState != EnemyState.Chase)
        {
            //ノイズ画面を表示
            noiseScreenPanel.SetActive(true);

            Debug.Log("ダッシュ音を検知");

            //音の位置を記録
            lastHeardSoundPosition = targetPoint.position;
            isInvestigatingSound = true;
            isInvestigatingBroadcastSound = false;
            soundInvestigateTimer = 0f;

            //ダッシュ音を検知した場合、調査状態に移行
            currentState = EnemyState.Investigate;

            //音の位置へ移動
            navMeshAgent.SetDestination(lastHeardSoundPosition);
        }
        else 
        {
            //ノイズ画面を非表示
            noiseScreenPanel.SetActive(false);
        }

        //TODO:

        //状態ごとの処理
        switch (currentState)
        {
            case EnemyState.Patrol:
            case EnemyState.Alert:
            case EnemyState.Chase:

                //BaseEnemy.csのUpdate関数を呼び出す
                base.Update();
            break;

            //調査状態
            case EnemyState.Investigate:

                //ダッシュ音調査中、または放送スピーカー音調査中の場合
                if (isInvestigatingSound || isInvestigatingBroadcastSound)
                {
                    //歩行アニメーション再生
                    animator.SetBool(kIsRunAnimatorParameter, false);
                    animator.SetBool(kIsWalkAnimatorParameter, IsEnemyMoving());

                    navMeshAgent.speed = NormalSpeed;

                    soundInvestigateTimer += Time.deltaTime;

                    //プレイヤーが視野に入った場合、追従状態に移行
                    if (IsPlayerInFront())
                    {
                        currentState = EnemyState.Chase;
                        lastKnownPlayerPosition = targetPoint.position;
                        isInvestigatingSound = false;
                        isInvestigatingBroadcastSound = false;

                        //画面を赤く表示
                        playerFoundPanel.SetActive(true);

                        Debug.Log("調査状態から追従状態へ02");
                    }
                    //調査時間が経過、または目的地に到達した場合、通常徘徊に戻る
                    else if (soundInvestigateTimer >= soundInvestigateDuration
                        || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                    {
                        currentState = EnemyState.Patrol;
                        isInvestigatingSound = false;
                        isInvestigatingBroadcastSound = false;
                        isAlertMode = false;

                        Debug.Log("調査状態から通常徘徊状態へ02");
                    }
                    //プレイヤーが近くにいる場合、警戒状態に移行
                    else if (distanceToPlayer <= alertRange)
                    {
                        currentState = EnemyState.Alert;
                        isInvestigatingSound = false;
                        isInvestigatingBroadcastSound = false;

                        //画面を元に戻す
                        playerFoundPanel.SetActive(false);

                        Debug.Log("調査状態から警戒圏内状態へ02");
                    }
                }
                else
                {
                    //BaseEnemy.csのUpdate関数を呼び出す
                    base.Update();
                }
            break;
        }
    }
}