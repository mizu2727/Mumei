using UnityEngine;
using static Player;

public partial class BaseEnemy
{
    /// <summary>
    /// オブジェクトのコリジョンと衝突した場合の処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトのコリジョン</param>
    private void OnCollisionEnter(Collision collision)
    {
        //派生クラスへ接触したオブジェクトを通知
        OnTouchObject(collision.gameObject);

        //壁に触れた場合
        if (collision.gameObject.layer == LayerMask.NameToLayer(CommonController.instance.GetWallLayer()) || collision.gameObject.CompareTag(CommonController.instance.GetWallTag()))
        {
            //追従状態の場合||無限追従の場合
            if (currentState == EnemyState.Chase || currentState == EnemyState.InfinityChase)
            {
                //NavMeshAgent自体の経路探索に任せるため、以下の強制停止処理をスキップする
                //(※returnすると下のプレイヤー・ドア判定まで飛ばしてしまうため、ifで分岐する)
            }
            //進行方向の壁に正面からぶつかった場合のみ停止して方向転換する
            //(床・天井や、壁を横に擦っただけの接触では止めない)
            else if (!isWallAvoidWaiting && IsHeadOnWallCollision(collision))
            {
                //方向転換待ちフラグをtrueに設定
                isWallAvoidWaiting = true;

                //速度を0に設定して停止させる
                navMeshAgent.velocity = Vector3.zero;

                //NavMeshAgentの移動を停止
                navMeshAgent.isStopped = true;

                //停止アニメーションを再生
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, false);

                //衝突地点を記録
                lastCollisionPoint = collision.GetContact(0).point;

                //多重登録を防いでから、一定時間後に方向転換
                CancelInvoke(nameof(ChangeDirection));
                Invoke(nameof(ChangeDirection), kWallAvoidWaitTime);
            }
        }

        //プレイヤーに触れた場合&&ダメージを受けていない場合&&プレイヤーが隠れていない場合
        if (collision.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) && !isReceiveDamage && !Player.instance.GetIsPlayerHidden())
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                //攻撃モードを開始する
                attackMode = AttackMode.StartAttack;

                //プレイヤーの死亡状態モードを死亡へ設定
                Player.instance.SetDieMode(DieMode.Die);
            }
        }

        //ドアに触れた場合
        if (collision.gameObject.CompareTag(CommonController.instance.GetDoorTag()))
        {
            gameObjectDoor = collision.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            //鍵がかかっていないドア系が閉まっている場合
            if ((!door.GetIsNeedKeyDoor() && !door.isOpenDoor && !door.GetIsSlidingDoor())
                || (!door.GetIsNeedKeyDoor() && !door.isOpenDoor && door.GetIsSlidingDoor()) && !door.IsSliding())
            {
                //ドアを開ける
                door.OpenDoor();
            }

            //鍵がかかっていないスライドドアが開いている場合
            if (!door.GetIsNeedKeyDoor() && door.isOpenDoor && door.GetIsSlidingDoor() && !door.IsSliding())
            {
                //ドアを閉める
                door.CloseDoor();
            }
        }

        //階段に接触した場合
        if (collision.gameObject.CompareTag(CommonController.instance.GetStairGroundTag()))
        {
            //階段昇降処理を開始
            isOnStairs = true;
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        //階段から離れた場合
        if (collision.gameObject.CompareTag(CommonController.instance.GetStairGroundTag()))
        {
            //階段昇降処理を終了(視線ピッチを平常状態へ戻す)
            isOnStairs = false;
        }
    }


    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">貫通したオブジェクトのコライダー</param>
    private void OnTriggerEnter(Collider collider)
    {
        //派生クラスへ接触したオブジェクトを通知
        OnTouchObject(collider.gameObject);

        //プレイヤーに触れた場合&&ダメージを受けていない場合&&プレイヤーが隠れていない場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) && !isReceiveDamage && !Player.instance.GetIsPlayerHidden())
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                //攻撃モードを開始する
                attackMode = AttackMode.StartAttack;

                //プレイヤーの死亡状態モードを死亡へ設定
                Player.instance.SetDieMode(DieMode.Die);
            }
        }

        //ドアに触れた場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetDoorTag()))
        {
            gameObjectDoor = collider.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            //鍵がかかっていないドア系が閉まっている場合
            if ((!door.GetIsNeedKeyDoor() && !door.isOpenDoor && !door.GetIsSlidingDoor())
                || (!door.GetIsNeedKeyDoor() && !door.isOpenDoor && door.GetIsSlidingDoor()) && !door.IsSliding())
            {
                //ドアを開ける
                door.OpenDoor();
            }

            //鍵がかかっていないスライドドアが開いている場合
            if (!door.GetIsNeedKeyDoor() && door.isOpenDoor && door.GetIsSlidingDoor() && !door.IsSliding())
            {
                //ドアを閉める
                door.CloseDoor();
            }
        }

        //階段に接触した場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetStairGroundTag()))
        {
            //階段昇降処理を開始
            isOnStairs = true;
        }
    }

    /// <summary>
    /// オブジェクトのコライダー内に触れている場合の処理
    /// </summary>
    /// <param name="collider">離れたオブジェクトのコライダー</param>
    private void OnTriggerStay(Collider collider)
    {
        //派生クラスへ接触したオブジェクトを通知
        OnTouchObject(collider.gameObject);

        //プレイヤーに触れた場合&&ダメージを受けていない場合&&プレイヤーが隠れていない場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) && !isReceiveDamage && !Player.instance.GetIsPlayerHidden())
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                //攻撃モードを開始する
                attackMode = AttackMode.StartAttack;

                //プレイヤーの死亡状態モードを死亡へ設定
                Player.instance.SetDieMode(DieMode.Die);
            }
        }

        //階段に触れている場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetStairGroundTag()))
        {
            //階段昇降処理を開始
            isOnStairs = true;
        }
    }

    /// <summary>
    /// オブジェクトのコライダーから離れた場合の処理
    /// </summary>
    /// <param name="collider">離れたオブジェクトのコライダー</param>
    private void OnTriggerExit(Collider collider)
    {
        //階段から離れた場合
        if (collider.gameObject.CompareTag(CommonController.instance.GetStairGroundTag()))
        {
            //階段昇降処理を終了(視線ピッチを平常状態へ戻す)
            isOnStairs = false;
        }
    }
}
