using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static GameController;

public partial class BaseEnemy
{
    /// <summary>
    /// 攻撃関連処理の初期化
    /// </summary>
    private void InitializeAttack() 
    {
        //攻撃フラグの初期化
        isAttack = false;

        //攻撃状態モードを初期化
        attackMode = AttackMode.None;
    }

    /// <summary>
    /// プレイヤーを攻撃するメソッド
    /// </summary>
    private async void AttackPlayer()
    {
        //プレイヤーが既に死亡している場合||既に攻撃している場合
        if (Player.instance.IsDead || isAttack || Player.instance == null || PlayerCamera.instance == null)
        {
            //処理をスキップ
            return;
        }

        //攻撃フラグをtrueに設定
        isAttack = true;

        //プレイヤーのHPを1減らす
        Player.instance.HP -= 1;

        //プレイヤーのHPが0以下の場合
        if (Player.instance.HP <= 0)
        {
            //プレイヤーカメラを対象のオブジェクトの方へ向ける
            await LookAtCamera();

            //シーン遷移時用データを保存
            GameController.instance.CallSaveSceneTransitionUserDataMethod();

            //プレイヤー死亡
            Player.instance.Dead();
        }
    }

    /// <summary>
    /// プレイヤーカメラを対象のオブジェクトの方へ向ける処理
    /// </summary>
    private async UniTask LookAtCamera()
    {
        Debug.Log("通常の攻撃アニメーション再生");

        //敵がプレイヤーを襲う際のエフェクト画面を表示するフラグをtrueに設定
        EnemyAttackScreen.instance.SetIsViewEnemyAttackScreen(true);

        //プレイヤーの身体のパーツを非表示にする(敵が襲う演出でプレイヤー自身が見えてしまうのを防ぐため)
        for (int i = 0; i < Player.instance.GetPlayerBodys().Length; i++)
        {
            Player.instance.GetPlayerBodys()[i].SetActive(false);
        }

        //カメラの上下左右回転をリセット
        PlayerCamera.instance.ResetCameraRotation();

        //プレイヤーカメラのX軸回転リセットフラグをtrueに設定
        PlayerCamera.instance.SetIsResetXRotate(true);

        //プレイヤーカメラのY軸回転リセットフラグをtrueに設定
        PlayerCamera.instance.SetIsResetYRotate(true);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1));

        //プレイヤーカメラのX軸回転リセットフラグをfalseに設定
        PlayerCamera.instance.SetIsResetXRotate(false);

        //プレイヤーカメラのY軸回転リセットフラグをtrueに設定
        PlayerCamera.instance.SetIsResetYRotate(false);


        //プレイヤーのRayCastが壁に当たっている場合
        if (Player.instance.GetIsRaycastHitWall())
        {
            //プレイヤーの視点を敵の方へ向ける
            Player.instance.transform.LookAt(transform.position);
        }

        //ステージBGMを停止する
        Stage01Controller.instance.StopStageBGM();

        //プレイヤーを追従するBGMを停止する
        EnemyBGMController.instance.StopChasePlayerBGM();

        //移動アニメーションオフ
        animator.SetBool(kIsRunAnimatorParameter, false);
        animator.SetBool(kIsWalkAnimatorParameter, false);


        //プレイヤーを攻撃するSEを再生
        audioSourceAttackPlayerSE.clip = sO_SE.GetSEClip(attackPlayerSEid);
        audioSourceAttackPlayerSE.loop = true;
        audioSourceAttackPlayerSE.Play();

        //NavmeshAgentの回転を無効にする
        navMeshAgent.updateRotation = false;

        //Rigidbodyの物理挙動(慣性や当たり判定の押し出し)で位置がズレるのを防ぐ
        rigidBody.isKinematic = true;

        //Rigidbodyの回転を固定する
        rigidBody.freezeRotation = true;

        //攻撃アニメーション再生
        animator.SetBool(kIsAttackAnimatorParameter, true);

        //プレイヤーの正面方向（カメラ/プレイヤーが向いている方向）を取得
        Vector3 forward = Player.instance.transform.forward;

        //Y成分(上下の傾き)を無視して水平方向のみにする
        //(壁際でPlayer.instance.transform.LookAt()が実行されると、forwardに縦方向の傾きが乗ってしまい、
        //その状態でtargetPosition.yを敵の高さに上書きすると、水平距離が1.5mより短くなって位置がズレる)
        forward.y = 0f;

        //正規化
        forward = forward.normalized;

        //距離の倍率
        float distanceMagnification = AttackDistanceFromPlayer;

        //プレイヤーの位置から前方に一定距離だけ進んだ位置を計算
        Vector3 targetPosition = Player.instance.transform.position + forward * distanceMagnification;

        //プレイヤーの高さを敵と同じにする
        targetPosition.y = transform.position.y;

        //敵をその位置に移動
        transform.position = targetPosition;

        //敵の向きをプレイヤーの方へ向ける
        transform.LookAt(Player.instance.transform.position);

        //ゲームモードをプレイヤーを攻撃する演出モードに設定
        GameController.instance.SetGameModeStatus(GameModeStatus.AttackMovieDirection);

        //遷移が完了して「AttackState」という名前のステートになるのを待つ
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(kAttackStateName));

        //攻撃アニメーションの再生速度を設定(派生クラスで変更可能。既定値は1倍)
        animator.speed = AttackAnimationSpeed;

        //停止位置(正規化時間)まで待つ(派生クラスで変更可能。既定値は0.8)
        float stopNormalizedTime = Mathf.Clamp01(AttackAnimationStopNormalizedTime);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= stopNormalizedTime);

        //アニメーションを最後で止める
        animator.speed = 0;
    }
}
