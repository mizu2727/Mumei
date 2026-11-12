using Unity.VisualScripting;
using UnityEngine;
using static GameController;

public partial class BaseEnemy
{
    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        if (MusicController.instance != null)
        {
            audioSourceSE = MusicController.instance.GetAudioSource();

            if (audioSourceSE != null)
            {
                audioSourceSE.playOnAwake = false;
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] MusicControllerからAudioSourceを取得できませんでした。");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] MusicController.instanceが見つかりません。");
        }

        audioSourceFindPlayerSE = gameObject.AddComponent<AudioSource>();
        audioSourceAttackPlayerSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceFindPlayerSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceAttackPlayerSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;

        //マスター音量を同期
        masterSEVolume = MusicController.instance.sESlider.value;


        //専用の新しいAudioSourceを取得
        //(別の効果音が鳴っている間に敵の効果音が鳴らないバグを防止する用)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }
    }

    /// <summary>
    /// WalkまたはDashのSEを再生するメソッド
    /// </summary>
    private void PlayWalkOrDashSE(float targetDistance) 
    {
        //移動時の効果音処理(プレイヤー死亡後に発生するエラーを防止する用にif文を追加)
        //(派生クラスで移動音を独自に制御する場合は実行しない)
        if (UseDefaultMoveSE && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame)
        {
            // 効果音制御
            currentSE = (currentState == EnemyState.Chase || currentState == EnemyState.InfinityChase) ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);

            //距離ベースの相対音量（0～1）
            float relativeVolume = CalculateVolumeBasedOnDistance(targetDistance);

            //最終音量 = マスターSE音量 × 相対音量
            float finalVolume = masterSEVolume * relativeVolume;

            if (IsMove && !wasMovingLastFrame)
            {
                //移動音再生(走る音の場合、ピッチを調整)
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;
                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;
                audioSourceSE.volume = finalVolume;
                audioSourceSE.Play();
            }
            else if (!IsMove && wasMovingLastFrame)
            {
                //移動音停止
                audioSourceSE.Stop();

                // 停止時にピッチをリセット
                audioSourceSE.pitch = 1.0f;
            }
            else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
            {
                //移動音停止
                audioSourceSE.Stop();

                //移動音再生(走る音の場合、ピッチを調整)
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;
                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;
                audioSourceSE.volume = finalVolume;
                audioSourceSE.Play();
            }
            else if (IsMove && audioSourceSE.isPlaying)
            {
                //移動中は最終音量を継続的に更新
                audioSourceSE.volume = finalVolume;
            }

            wasMovingLastFrame = IsMove;
        }
    }

    /// <summary>
    /// 距離に基づく音量を計算するメソッド
    /// </summary>
    /// <param name="distance">プレイヤーとの距離</param>
    /// <returns>音量</returns>
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        //プレイヤーとの高さの差を測定
        float heightDifference = Mathf.Abs(transform.position.y - targetPoint.position.y);

        //高さの差が音が届く最大高さ差を超えている場合
        if (heightDifference > maxSoundVerticalRange)
        {
            //最小音量(音が届かない)
            return minVolume;
        }


        if (distance <= maxSoundDistance)
        {
            //最大音量
            return maxVolume;
        }
        else if (distance >= minSoundDistance)
        {
            //最小音量
            return minVolume;
        }
        else
        {
            //距離に基づいて音量を調整
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }

    /// <summary>
    /// 警戒音再生メソッド
    /// </summary>
    protected void PlayFindPlayerSE()
    {
        //ポーズ中ではない場合
        if (Time.timeScale == 1)
        {
            //プレイヤーを探すSE再生中フラグをtrue
            isPlayFindPlayerSE = true;

            //警戒音を再生
            audioSourceFindPlayerSE.clip = sO_SE.GetSEClip(findPlayerSEid);
            audioSourceFindPlayerSE.loop = true;
            audioSourceFindPlayerSE.Play();
        }
    }

    /// <summary>
    /// 警戒音停止メソッド
    /// </summary>
    protected void StopFindPlayerSE()
    {
        //プレイヤーを探すSE再生中フラグをfalse
        isPlayFindPlayerSE = false;

        audioSourceFindPlayerSE.Stop();
    }
}
