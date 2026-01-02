using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

public class HearingEnemy : BaseEnemy
{
    [Header("ダッシュ音の検知範囲")]
    [SerializeField] private float soundDetectionRange = 50f;

    [Header("ダッシュ音の調査時間")]
    [SerializeField] private float soundInvestigateDuration = 10f;

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


    protected override async void Update()
    {
        //ゲームがプレイ中でない、またはプレイヤーが死亡している場合は処理をスキップ
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null 
            || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;
            return;
        }


        

        //プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint.position);

        //プレイヤーのダッシュ音を検知
        if (!isInvestigatingSound && Player.instance.IsDash && !Player.instance.GetIsPlayerHidden()
            && distanceToPlayer <= soundDetectionRange)
        {
            //追従モード以外の場合
            if (currentState != EnemyState.Chase)
            {
                //ノイズ画面を表示
                noiseScreenPanel.SetActive(true);
                Debug.Log("ダッシュ音を検知したため、ノイズ画面を表示");
            }

            Debug.Log("ダッシュ音を検知");

            //音の位置を記録
            lastHeardSoundPosition = targetPoint.position;
            isInvestigatingSound = true;
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

                    //ダッシュ音調査中の場合
                    if (isInvestigatingSound)
                    {


                        //歩行アニメーション再生
                        animator.SetBool("isRun", false);
                        animator.SetBool("isWalk", IsEnemyMoving());

                        navMeshAgent.speed = NormalSpeed;

                        soundInvestigateTimer += Time.deltaTime;

                        //プレイヤーが視野に入った場合、追従状態に移行
                        if (IsPlayerInFront())
                        {
                            currentState = EnemyState.Chase;
                            lastKnownPlayerPosition = targetPoint.position;
                            isInvestigatingSound = false;

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
                            isAlertMode = false;

                            Debug.Log("調査状態から通常徘徊状態へ02");
                        }
                        //プレイヤーが近くにいる場合、警戒状態に移行
                        else if (distanceToPlayer <= alertRange)
                        {
                            currentState = EnemyState.Alert;
                            isInvestigatingSound = false;

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