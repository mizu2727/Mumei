using UnityEngine;

public partial class BaseEnemy
{
    /// <summary>
    /// 階段昇降時の傾斜（前後の高低差）から、視線を上下させるピッチ角度（度）を算出する
    /// </summary>
    /// <returns>算出したピッチ角度（上向きはマイナス値、下向きはプラス値、傾斜なしは0f）</returns>
    private float CalculateStairsSightPitch()
    {
        // --- 照射位置の算出 ---
        // 敵の現在位置から目線の高さ（+Y）をベースに、前方および後方へオフセットした位置
        Vector3 forwardSamplePos = transform.position + transform.forward * kStairsSampleOffset + Vector3.up * kEyeHeight;
        Vector3 backwardSamplePos = transform.position - transform.forward * kStairsSampleOffset + Vector3.up * kEyeHeight;

        // --- レイキャストの実行 ---
        // 前後それぞれの位置から真下（Vector3.down）に向けて階段レイヤー（stairsLayer）へ照射
        bool hitForward = Physics.Raycast(forwardSamplePos, Vector3.down, out RaycastHit forwardHit, kStairsEayLength, stairsLayer);
        bool hitBackward = Physics.Raycast(backwardSamplePos, Vector3.down, out RaycastHit backwardHit, kStairsEayLength, stairsLayer);

        // ==========================================
        // メイン処理：前後のレイが両方ヒットした場合
        // ==========================================
        if (hitForward && hitBackward)
        {
            // 高低差（Y座標の差）を計算
            // ※上り階段（前方が高い）の際、視線を「上（マイナス角度）」に向けるため backward - forward の順で引く
            float heightDifference = backwardHit.point.y - forwardHit.point.y;

            // 前後のサンプリング点同士の水平距離（前後 sampleOffset × 2）
            float horizontalDistance = kStairsSampleOffset * 2f;

            // アークタンジェント（Atan2）で高さと底辺から角度（ラジアン）を求め、Rad2Degで「度」に変換して返す
            return Mathf.Atan2(heightDifference, horizontalDistance) * Mathf.Rad2Deg;
        }

        // ==========================================
        // 予備処理：階段の登り口・降り口などで片方しかヒットしなかった場合
        // ==========================================
        // 敵の足元直下（中央）の射出位置を設定してレイキャストを実行
        Vector3 centerSamplePos = transform.position + Vector3.up * kEyeHeight;
        bool hitCenter = Physics.Raycast(centerSamplePos, Vector3.down, out RaycastHit centerHit, kStairsEayLength, stairsLayer);

        if (hitCenter)
        {
            // 前方だけヒットしている場合（階段の降り口や踊り場への入り口など）
            if (hitForward)
            {
                // 中央と前方の高低差から角度を算出
                float heightDiff = centerHit.point.y - forwardHit.point.y;
                return Mathf.Atan2(heightDiff, kStairsSampleOffset) * Mathf.Rad2Deg;
            }
            // 後方だけヒットしている場合（階段の登り口など）
            else if (hitBackward)
            {
                //後方と中央の高低差から角度を算出
                float heightDiff = backwardHit.point.y - centerHit.point.y;
                return Mathf.Atan2(heightDiff, kStairsSampleOffset) * Mathf.Rad2Deg;
            }
        }

        // どのレイキャストも有効な判定にならなかった場合は傾斜なし（0度）とする
        return 0f;
    }

    /// <summary>
    /// 階段昇降時の視線の上下ピッチ角度を更新する
    /// (階段の昇降移動自体はNavMeshAgentに任せ、ここではプレイヤー検知用の視線角度のみを更新する)
    /// </summary>
    private void UpdateStairsSightPitch()
    {
        //階段に乗っている場合は傾斜から算出した角度、乗っていない場合は0度を目標角度にする
        float targetPitchAngle = isOnStairs ? CalculateStairsSightPitch() : 0f;

        //目標角度へ滑らかに補間
        currentSightPitchAngle = Mathf.Lerp(currentSightPitchAngle, targetPitchAngle, Time.deltaTime * sightPitchSmoothSpeed);
    }
}
