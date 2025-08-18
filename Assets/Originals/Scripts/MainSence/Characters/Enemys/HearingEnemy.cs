using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

public class HearingEnemy : BaseEnemy
{
    [Header("ダッシュ音の検知範囲")]
    [SerializeField] private float soundDetectionRange = 50f; // ダッシュ音を検知する距離

    [Header("ダッシュ音の調査時間")]
    [SerializeField] private float soundInvestigateDuration = 10f; // ダッシュ音の調査時間

    private float soundInvestigateTimer = 0f; // 調査時間のカウンター
    private Vector3 lastHeardSoundPosition; // 最後に聞こえたダッシュ音の位置
    private bool isInvestigatingSound = false; // ダッシュ音を調査中かどうか

    protected override async void Update()
    {
        // ゲームがプレイ中でない、またはプレイヤーが死亡している場合は処理をスキップ
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        // プレイヤーとの距離を計算
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint.position);

        // プレイヤーのダッシュ音を検知
        //&& currentState != EnemyState.Chase
        if (!isInvestigatingSound && Player.instance.IsDash && distanceToPlayer <= soundDetectionRange)
        {
            // ダッシュ音を検知した場合、調査状態に移行
            currentState = EnemyState.Investigate;
            lastHeardSoundPosition = targetPoint.position; // 音の位置を記録
            isInvestigatingSound = true;
            soundInvestigateTimer = 0f;
            navMeshAgent.SetDestination(lastHeardSoundPosition); // 音の位置に向かう
            Debug.Log($"[{gameObject.name}] プレイヤーのダッシュ音を検知！位置: {lastHeardSoundPosition}");
        }

        // 状態ごとの処理
        switch (currentState)
        {
            case EnemyState.Patrol:
            case EnemyState.Alert:
            case EnemyState.Chase:
                // BaseEnemy の Update ロジックを呼び出す
                base.Update();
                break;

            case EnemyState.Investigate:
                if (isInvestigatingSound)
                {
                    // ダッシュ音の調査中
                    animator.SetBool("isRun", false);
                    animator.SetBool("isWalk", IsEnemyMoving());
                    navMeshAgent.speed = NormalSpeed;

                    soundInvestigateTimer += Time.deltaTime;

                    // プレイヤーが視野に入った場合、追従状態に移行
                    if (IsPlayerInFront())
                    {
                        currentState = EnemyState.Chase;
                        lastKnownPlayerPosition = targetPoint.position;
                        isInvestigatingSound = false;
                        playerFoundPanel.SetActive(true);
                        Debug.Log($"[{gameObject.name}] 調査中にプレイヤーを発見！追従状態に移行");
                    }
                    // 調査時間が経過、または目的地に到達した場合、通常徘徊に戻る
                    else if (soundInvestigateTimer >= soundInvestigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                    {
                        currentState = EnemyState.Patrol;
                        isInvestigatingSound = false;
                        isAlertMode = false;
                        Debug.Log($"[{gameObject.name}] ダッシュ音の調査終了、通常徘徊に戻る");
                    }
                    // プレイヤーが近くにいる場合、警戒状態に移行
                    else if (distanceToPlayer <= alertRange)
                    {
                        currentState = EnemyState.Alert;
                        isInvestigatingSound = false;
                        Debug.Log($"[{gameObject.name}] 調査中にプレイヤーが近くにいる、警戒状態に移行");
                    }
                }
                else
                {
                    // 通常の調査状態（BaseEnemy の処理を継承）
                    base.Update();
                }
                break;
        }
    }
}