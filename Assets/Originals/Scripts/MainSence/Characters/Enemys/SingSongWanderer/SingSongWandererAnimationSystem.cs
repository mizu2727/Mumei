using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using static GameController;
using static Player;

public partial class SingSongWanderer
{
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

        //ハイドポイントの種類に応じた補助距離(ベッドの下の場合は専用の距離)
        float checkDistance = hiddenObject.IsUnderBed() ? bedFoundDistance : foundDistance;

        //補助距離が無効の場合
        if (checkDistance <= 0f)
        {
            //処理をスキップ
            return;
        }

        //水平距離と高さの差を計算
        Vector3 diff = hiddenObject.transform.position - transform.position;
        float heightDiff = Mathf.Abs(diff.y);
        diff.y = 0f;

        //一定距離まで近づいた場合
        if (diff.magnitude <= checkDistance && heightDiff <= kFoundVerticalRange)
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
        if (isFoundSequenceRunning || GameController.instance.GetGameModeStatus() != GameModeStatus.PlayInGame)
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

        //無限追従の演出中の登録を解除(発見演出で赤い画面・BGMを止めるため)
        SetInfinityChasePresentation(false);

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
            GameController.instance.SetGameModeStatus(GameModeStatus.AttackMovieDirection);

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

            //ゴミ箱の場合、プレイヤーの視点を真上に固定する(覗き込んでくる敵を見上げる)
            if (hidePointType == HiddenObject.HidePointType.TrashCan && isLockPlayerViewUpOnTrashCan
                && PlayerCamera.instance != null)
            {
                PlayerCamera.instance.SetLookUpLock(true);
            }

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

            //視点の真上固定を解除(途中でキャンセルされた場合に固定が残らないようにする)
            if (PlayerCamera.instance != null)
            {
                PlayerCamera.instance.SetLookUpLock(false);
            }
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

            case HiddenObject.HidePointType.UnderBed:
                return bedPeekSetting;

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
        for (int i = 0; i < Player.instance.GetPlayerBodys().Length; i++)
        {
            if (Player.instance.GetPlayerBodys()[i] != null)
            {
                Player.instance.GetPlayerBodys()[i].SetActive(false);
            }
        }

        //うつ伏せ(ベッドの下)で隠れている場合
        if (Player.instance.GetIsProneHidden() && PlayerCamera.instance != null)
        {
            //うつ伏せの姿勢のまま、覗き込んでくる敵の頭(無い場合は敵本体)を目で追わせる
            //(プレイヤー本体を回転させるとベッドから身体がはみ出すため、カメラだけを向ける)
            Transform lookTarget = GetHeadTransform();
            PlayerCamera.instance.SetProneLookTarget(lookTarget != null ? lookTarget : transform);

            //処理をスキップ
            return;
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
        //頭のTransformを取得
        GetHeadTransform();

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
    /// 頭のTransformを取得する(未設定でHumanoidの場合は自動取得)
    /// </summary>
    /// <returns>頭のTransform(取得できない場合はnull)</returns>
    private Transform GetHeadTransform()
    {
        if (headTransform == null && animator != null && animator.isHuman)
        {
            headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        }

        return headTransform;
    }

    /// <summary>
    /// 襲う演出(独自の襲うアニメーション)
    /// </summary>
    /// <param name="token">キャンセルトークン</param>
    private async UniTask PlayHideAttackAsync(CancellationToken token)
    {
        //襲うSEを再生
        if (kHideAttackSEid >= 0 && sO_SE != null)
        {
            AudioClip clip = sO_SE.GetSEClip(kHideAttackSEid);
            if (clip != null)
            {
                audioSourceHideAttackSE = CreateSEAudioSourceIfNeeded(audioSourceHideAttackSE);
                audioSourceHideAttackSE.clip = clip;
                audioSourceHideAttackSE.loop = true;
                audioSourceHideAttackSE.Play();
            }
        }

        //襲うアニメーション再生
        SetAnimatorBoolSafe(kStringHideAttackAnimatorParameter, true);

        //襲うアニメーションのステートに遷移するまで待つ
        bool isReached = await WaitAnimatorStateAsync(kStringHideAttackStateName, token);
        if (!isReached)
        {
            return;
        }

        //再生速度を設定
        animator.speed = kHideAttackAnimationSpeed;

        //停止位置(正規化時間)まで待つ(途中で別ステートへ抜けた場合も終了)
        float stopNormalizedTime = Mathf.Clamp01(kHideAttackStopNormalizedTime);
        await UniTask.WaitUntil(() =>
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return !info.IsName(kStringHideAttackStateName) || info.normalizedTime >= stopNormalizedTime;
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
