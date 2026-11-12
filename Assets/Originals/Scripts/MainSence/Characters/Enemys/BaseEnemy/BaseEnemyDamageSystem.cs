using UnityEngine;

public partial class BaseEnemy
{
    /// <summary>
    /// ダメージ関連を初期化する
    /// </summary>
    private void InitializeDamage() 
    {
        //ダメージを受けるフラグを初期化
        isReceiveDamage = false;

        //ダメージカウントを初期化
        receiveDamageCount = 0;
    }

    /// <summary>
    /// ダメージ関連処理
    /// </summary>
    private void DamageRelatedProcessing()
    {
        //ダメージ受けている秒数が8秒以下の場合
        if (receiveDamageCount <= kMaxReceiveDamageCount)
        {
            //ダメージを受けている秒数が0秒の場合
            if (receiveDamageCount == 0)
            {
                //ダメージアニメーション再生
                animator.SetBool(kIsDamageAnimatorParameter, true);

                //EnemyStateを保存する
                saveEnemyState = currentState;

                //移動アニメーション再生停止
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, false);

                //調査状態に移行
                currentState = EnemyState.Investigate;

                //移動停止
                navMeshAgent.isStopped = true;
            }

            //ダメージを受けている秒数を加算する
            receiveDamageCount += Time.deltaTime;
        }
        else
        {
            //移動停止解除
            navMeshAgent.isStopped = false;

            //ダメージアニメーション再生停止
            animator.SetBool(kIsDamageAnimatorParameter, false);

            //保存していたEnemyStateがChaseの場合

            if (saveEnemyState == EnemyState.Chase || saveEnemyState == EnemyState.InfinityChase)
            {
                //移動アニメーション再生
                animator.SetBool(kIsRunAnimatorParameter, true);
                animator.SetBool(kIsWalkAnimatorParameter, false);
            }
            else
            {
                //移動アニメーション再生
                animator.SetBool(kIsRunAnimatorParameter, false);
                animator.SetBool(kIsWalkAnimatorParameter, IsMove);
            }


            //ダメージ受けている秒数をリセット
            receiveDamageCount = 0;

            //ダメージを受けているフラグをオフにする
            isReceiveDamage = false;
        }
    }
}
