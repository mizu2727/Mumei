using UnityEngine;
using UnityEngine.AI;
using static GameController;

/// <summary>
/// Navmesh・移動関連の処理をまとめたクラス
/// </summary>
public partial class BaseEnemy
{
    /// <summary>
    /// NavMesh関連の初期化
    /// </summary>
    private void InitializeNavMesh() 
    {
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

        //加速度を設定
        navMeshAgent.acceleration = 20f;

        //停止距離を設定
        navMeshAgent.stoppingDistance = 0f;

        //モデルに合わせて調整すること
        navMeshAgent.baseOffset = 0f;

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
        if (patrolPoint == null || patrolPoint.Length == 0)
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

        //難易度に応じてそれぞれの設定を変更する
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus())
        {
            //難易度Easyの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //通常移動速度を難易度Easy用に設定
                Speed = easyMoveSpeed;

                //ダッシュ移動速度を難易度Easy用に設定
                dashSpeed = easyDashSpeed;

                //プレイヤーを視野できる距離を難易度Easy用に設定
                enemyDetectionRange = easyEnemyDetectionRange;
                break;

            //難易度Normal用の場合(デバッグ用にkNoneも追加)
            case DifficultyLevelController.DifficultyLevel.kNormal:
            case DifficultyLevelController.DifficultyLevel.kNone:

                //通常移動速度を難易度Normal用に設定
                Speed = defaultMoveSpeed;

                //ダッシュ移動速度を難易度Normal用に設定
                dashSpeed = defaultDashSpeed;

                //プレイヤーを視野できる距離を難易度Normal用に設定
                enemyDetectionRange = normalEnemyDetectionRange;
                break;

            //難易度Nightmare用の場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //通常移動速度を難易度Nightmare用に設定
                Speed = nightmareMoveSpeed;

                //ダッシュ移動速度を難易度Nightmare用に設定
                dashSpeed = nightmareDashSpeed;

                //プレイヤーを視野できる距離を難易度Nightmare用に設定
                enemyDetectionRange = nightmareEnemyDetectionRange;
                break;
        }

        //経路計算用のNavMeshPathを生成
        workPath = new NavMeshPath();
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

        //プレイヤーとの距離を計算
        Vector3 directionToPlayer = targetPoint.position - transform.position;
        distanceToPlayer = directionToPlayer.magnitude;

        //視線の上下ピッチ角度を反映した方向(階段昇降時は傾斜に沿って上下に傾く)
        Vector3 sightForward = Quaternion.AngleAxis(currentSightPitchAngle, transform.right) * transform.forward;

        //角度を計算
        float angle = Vector3.Angle(sightForward, directionToPlayer.normalized);

        //プレイヤーが視野角内にいる&&検知範囲内にいる場合
        if (distanceToPlayer <= enemyDetectionRange && angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;

            //視線の開始位置(敵の視線の高さ)
            Vector3 rayOrigin = transform.position + Vector3.up * kEyeHeight;

            //プレイヤーの中心
            Vector3 targetPosition = targetPoint.position + Vector3.up * 0.5f;

            //Raycastでヒットしたオブジェクトのレイヤーをチェック
            if (Physics.SphereCast(rayOrigin, kSphereCastRadius, directionToPlayer.normalized, out hit, enemyDetectionRange, detectionLayer))
            {
                if (hit.collider.CompareTag(CommonController.instance.GetPlayerTag()))
                {
                    //視線経路上のすべてのオブジェクトをチェック
                    RaycastHit[] hits = Physics.RaycastAll(rayOrigin, directionToPlayer.normalized, distanceToPlayer);
                    foreach (var rayHit in hits)
                    {
                        //壁にヒットした場合
                        if (rayHit.collider.CompareTag(CommonController.instance.GetWallTag()))
                        {
                            //プレイヤーの視認失敗
                            return false;
                        }

                        //ドアにヒットした場合
                        if (rayHit.collider.CompareTag(CommonController.instance.GetDoorTag()))
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

                    //プレイヤーが隠れている場合の処理
                    if (Player.instance.GetIsPlayerHidden())
                    {
                        //プレイヤーの視認失敗
                        return false;
                    }

                    //唄歌う彷徨う者以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer)
                    {
                        //ステージBGMからプレイヤーを追従するBGMへ切り替える
                        EnemyBGMController.instance.ChangeBGMFromStageBGMToChasePlayerBGM();
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
    private void ChasePlayer()
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
    private void NextPosition()
    {
        //徘徊地点をnullチェック
        if (patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPointが無効です！");
            navMeshAgent.isStopped = true;
            return;
        }

        //通常プレイモード&&パネルがnull以外の場合
        if (GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && playerFoundPanel != null)
        {
            //赤い画面を非表示
            HidePlayerFoundPanel();
        }

        //ランダムな俳諧地点を選択
        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = patrolPoint[positionNumber].position;

        NavMeshHit hit;

        //NavMesh上の位置を確認
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
        {
            //最後まで到達できる経路かを確認する
            //(到達できない徘徊地点を選ぶと、経路の途中(NavMeshの切れ目)で止まってしまうため)
            if (TrySetCompletePatrolDestination(positionNumber, hit.position))
            {
                return;
            }

            //ランダムに選んだ地点に到達できない場合、他の徘徊地点を順番に試す
            for (int i = 1; i < patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i) % patrolPoint.Length;
                if (patrolPoint[nextIndex] == null) continue;

                if (NavMesh.SamplePosition(patrolPoint[nextIndex].position, out NavMeshHit nextHit, findPatrolPointRange, NavMesh.AllAreas)
                    && TrySetCompletePatrolDestination(nextIndex, nextHit.position))
                {
                    return;
                }
            }

            //どの徘徊地点にも完全な経路が無い場合は、従来通り行ける所まで向かう
            Debug.LogWarning($"[{gameObject.name}] 完全な経路で到達できる徘徊地点がありません。NavMeshの繋がりを確認してください。");
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
    /// 徘徊地点までの経路が完全(PathComplete)な場合のみ目的地に設定する
    /// </summary>
    /// <param name="index">徘徊地点の要素番号</param>
    /// <param name="navMeshPosition">NavMesh上に補正した徘徊地点の位置</param>
    /// <returns>設定できた場合true</returns>
    private bool TrySetCompletePatrolDestination(int index, Vector3 navMeshPosition)
    {
        if (workPath == null) workPath = new NavMeshPath();

        //経路を計算
        if (navMeshAgent.CalculatePath(navMeshPosition, workPath) && workPath.status == NavMeshPathStatus.PathComplete)
        {
            navMeshAgent.SetPath(workPath);
            positionNumber = index;
            return true;
        }

        //到達できない徘徊地点をログに出す(NavMeshが途切れている場所の特定用)
        Debug.LogWarning($"[{gameObject.name}] 徘徊地点「{patrolPoint[index].name}」へ到達できません(経路:{workPath.status})。" +
                         $"現在地:{transform.position} 目的地:{navMeshPosition}");
        return false;
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
    /// オブジェクトに接触した場合に呼ばれる(派生クラスで独自の接触処理を行う用)
    /// OnCollisionEnter / OnTriggerEnter / OnTriggerStay から呼ばれる
    /// </summary>
    /// <param name="touchedObject">接触したオブジェクト</param>
    protected virtual void OnTouchObject(GameObject touchedObject)
    {
    }

    /// <summary>
    /// 方向転換
    /// </summary>
    private void ChangeDirection()
    {
        //方向転換待ちフラグを解除(どの分岐でも必ず解除する)
        isWallAvoidWaiting = false;

        //navMeshAgentが有効でない、またはNavMesh上にいない場合は何もしない
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        //ダメージ中はDamageRelatedProcessing()側で停止・再開を管理するため、ここでは再開しない
        if (isReceiveDamage)
        {
            return;
        }

        //NavMeshAgentの停止を解除
        navMeshAgent.isStopped = false;

        //スタック検知をリセット(停止していた0.5秒をスタックとして数えないため)
        ResetStuckCheck();

        //待機中に追従状態または無限追従へ切り替わっていた場合は、ChasePlayer()の目的地設定に任せる
        if (currentState == EnemyState.Chase || currentState == EnemyState.InfinityChase)
        {
            return;
        }

        if (workPath == null) workPath = new NavMeshPath();

        //壁から離れる方向(水平方向のみ)
        Vector3 awayFromWall = transform.position - lastCollisionPoint;
        awayFromWall.y = 0f;
        if (awayFromWall.sqrMagnitude < 0.0001f)
        {
            awayFromWall = -transform.forward;
            awayFromWall.y = 0f;
        }
        awayFromWall.Normalize();

        //壁から離れる方向を中心に±90度の範囲でランダムな地点を探す
        //(※元のRandom.insideUnitSphereはY成分を含むため、上下の別の床をサンプリングしてしまうことがあった)
        for (int i = 0; i < kChangeDirectionTryCount; i++)
        {
            float angle = Random.Range(-90f, 90f);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * awayFromWall;
            Vector3 newTarget = transform.position + direction * kChangeDirectionDistance;

            //NavMesh上の位置を確認(高さの違う床を拾わないよう、サンプリング半径は小さめ)
            if (NavMesh.SamplePosition(newTarget, out NavMeshHit hit, 1.5f, NavMesh.AllAreas)
                && Mathf.Abs(hit.position.y - transform.position.y) < 1.0f
                && navMeshAgent.CalculatePath(hit.position, workPath)
                && workPath.status == NavMeshPathStatus.PathComplete)
            {
                navMeshAgent.SetPath(workPath);
                return;
            }
        }

        //回避先が見つからない場合は、到達可能な徘徊地点を選び直す
        Debug.LogWarning($"[{gameObject.name}] 壁衝突後の回避先が見つからないため、徘徊地点を選び直します。位置:{transform.position}");
        NextPosition();
    }

    /// <summary>
    /// 進行方向の壁に正面からぶつかったかを判定する
    /// </summary>
    /// <param name="collision">衝突情報</param>
    /// <returns>正面衝突の場合true</returns>
    private bool IsHeadOnWallCollision(Collision collision)
    {
        if (navMeshAgent == null || collision.contactCount == 0)
        {
            return false;
        }

        //NavMeshAgentが進もうとしている方向(水平)
        Vector3 moveDirection = navMeshAgent.desiredVelocity;
        moveDirection.y = 0f;

        //ほぼ動いていない場合は正面衝突とみなさない
        if (moveDirection.sqrMagnitude < 0.01f)
        {
            return false;
        }
        moveDirection.Normalize();

        for (int i = 0; i < collision.contactCount; i++)
        {
            //接触面の法線(相手のコライダーから自分へ向かう向き)
            Vector3 normal = collision.GetContact(i).normal;

            //法線が上下を向いている = 床や天井との接触なので無視する
            //(床と壁が一体のメッシュでWallレイヤー/タグになっている道などで、段差や継ぎ目を踏むたびに停止していたのを防ぐ)
            if (Mathf.Abs(normal.y) > 0.5f)
            {
                continue;
            }

            normal.y = 0f;
            normal.Normalize();

            //進行方向と法線が向かい合っている(=壁に向かって進んでいる)場合のみ正面衝突
            //(横に擦っただけの接触はNavMeshAgentの回避に任せる)
            if (Vector3.Dot(moveDirection, normal) < -0.5f)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// スタック検知の状態をリセットする
    /// </summary>
    private void ResetStuckCheck()
    {
        stuckCheckTimer = 0f;
        stuckTimer = 0f;
        stuckCheckLastPosition = transform.position;
    }

    /// <summary>
    /// 移動停止が意図しないものである場合に、NavMeshAgentの停止を解除する
    /// (isStoppedがtrueのまま残って動かなくなるのを防ぐ)
    /// </summary>
    private void EnsureAgentResumed()
    {
        //壁衝突の方向転換待ち中は意図的に停止しているので解除しない
        if (isWallAvoidWaiting)
        {
            return;
        }

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = false;
        }
    }

    /// <summary>
    /// スタック(経路はあるのに進めていない状態)を検知して復帰させる
    /// </summary>
    private void CheckStuck()
    {
        //通常プレイ以外、攻撃中、ダメージ中はチェックしない
        //(Update内のawait後に遅れて呼ばれた場合でも、攻撃演出中にワープしないようにする)
        if (GameController.instance.GetGameModeStatus() != GameModeStatus.PlayInGame
            || attackMode != AttackMode.None || isAttack || isReceiveDamage)
        {
            ResetStuckCheck();
            return;
        }

        //意図的に停止している、経路計算中、経路が無い場合はチェックしない
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh
            || isWallAvoidWaiting || navMeshAgent.isStopped || navMeshAgent.pathPending || !navMeshAgent.hasPath)
        {
            ResetStuckCheck();
            return;
        }

        //目的地にほぼ到着している場合は、到着処理(NextPosition等)に任せる
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f)
        {
            ResetStuckCheck();
            return;
        }

        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer < stuckCheckInterval)
        {
            return;
        }

        //前回チェック時からの水平移動量
        Vector3 moved = transform.position - stuckCheckLastPosition;
        moved.y = 0f;

        stuckCheckLastPosition = transform.position;
        stuckCheckTimer = 0f;

        if (moved.magnitude < stuckMoveThreshold)
        {
            //進めていない時間を加算
            stuckTimer += stuckCheckInterval;
        }
        else
        {
            //進めているのでリセット
            stuckTimer = 0f;
            stuckRecoverCount = 0;
        }

        //一定時間進めていない場合、スタックとみなして復帰処理
        if (stuckTimer >= stuckTimeToRecover)
        {
            RecoverFromStuck();
        }
    }

    /// <summary>
    /// スタックからの復帰処理
    /// </summary>
    private void RecoverFromStuck()
    {
        stuckRecoverCount++;

        if (isLogStuck)
        {
            Debug.LogWarning($"[{gameObject.name}] スタック検知({stuckRecoverCount}回目) 状態:{currentState} " +
                             $"位置:{transform.position} Agent内部位置:{navMeshAgent.nextPosition} 目的地:{navMeshAgent.destination} " +
                             $"経路:{navMeshAgent.pathStatus} 残距離:{navMeshAgent.remainingDistance:F2}");
        }

        //Rigidbodyに残っている速度を消す(物理の押し戻しとNavMeshAgentの移動が喧嘩するのを防ぐ)
        if (rigidBody != null && !rigidBody.isKinematic)
        {
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
        }

        //Transformの位置とNavMeshAgentの内部位置のズレを解消するため、NavMesh上の最寄り位置へワープ
        //(Warpすると経路が消えるので、この後で必ず目的地を設定し直す)
        Vector3 savedDestination = navMeshAgent.destination;
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
        }

        navMeshAgent.isStopped = false;

        switch (currentState)
        {
            //追従状態・無限追従:次フレームのChasePlayer()でプレイヤーへの経路が再設定される
            case EnemyState.Chase:
            case EnemyState.InfinityChase:
                navMeshAgent.SetDestination(savedDestination);
                break;

            //調査状態:1回目は同じ地点へ再挑戦、それでもダメなら徘徊に戻す
            case EnemyState.Investigate:
                if (stuckRecoverCount <= 1)
                {
                    navMeshAgent.SetDestination(savedDestination);
                }
                else
                {
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //唄歌う彷徨う者以外の場合
                    if (enemyNameStatus != EnemyName.SingSongWanderer)
                    {
                        ChangeBGMToStageIfAllowed();
                    }

                    NextPosition();
                }
                break;

            //徘徊・警戒状態:別の徘徊地点を選び直す
            default:
                NextPosition();
                break;
        }

        ResetStuckCheck();
    }

    /// <summary>
    /// ギズモを描画
    /// </summary>
    private void OnDrawGizmos()
    {
        //視野範囲の可視化
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;

        //1. 敵の現在の向き（Y軸回転）に、視線のピッチ角（X軸回転）を合成したQuat（回転）を作る
        float pitch = Application.isPlaying ? currentSightPitchAngle : 0f;
        Quaternion sightRotation = transform.rotation * Quaternion.Euler(pitch, 0, 0);

        //2. 合成した回転から左右の視野角（±halfFOV）を計算
        Vector3 leftRay = (sightRotation * Quaternion.Euler(0, -halfFOV, 0)) * Vector3.forward * enemyDetectionRange;
        Vector3 rightRay = (sightRotation * Quaternion.Euler(0, halfFOV, 0)) * Vector3.forward * enemyDetectionRange;

        //視線の開始位置（目線の高さ）
        Vector3 eyePosition = transform.position + Vector3.up * kEyeHeight;

        //描画
        Gizmos.DrawRay(eyePosition, leftRay);
        Gizmos.DrawRay(eyePosition, rightRay);

        //SphereCastの範囲
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePosition, kSphereCastRadius);

        if (targetPoint != null)
        {
            Gizmos.DrawWireSphere(targetPoint.position + Vector3.up * 1.0f, kSphereCastRadius);

            // Linecastの経路を可視化
            Gizmos.color = Color.red;
            Vector3 targetPos = targetPoint.position + Vector3.up * 0.5f;
            Gizmos.DrawLine(eyePosition, targetPos);
        }
    }
}
