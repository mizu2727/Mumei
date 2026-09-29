using UnityEngine;

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

    private void Start()
    {
        //初期化
        singTime = 0f;
    }


    protected override void Update()
    {
        base.Update();

        //無限追従モードの場合
        if (currentState == EnemyState.InfinityChase) 
        {
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
}
