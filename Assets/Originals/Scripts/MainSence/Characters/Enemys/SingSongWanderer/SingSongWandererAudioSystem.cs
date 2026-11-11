using UnityEngine;

public partial class SingSongWanderer
{
    /// <summary>
    /// 無限追従の演出(赤い画面・追従BGM)を開始する
    /// 他の敵がステージBGMへ戻したり赤い画面を消したりしないよう、演出中として登録する
    /// </summary>
    private void PlayInfinityChasePresentation()
    {
        //無限追従の演出中として登録
        SetInfinityChasePresentation(true);

        //画面を赤く表示
        if (playerFoundPanel != null)
        {
            playerFoundPanel.SetActive(true);
        }

        //ステージBGMからプレイヤーを追従するBGMへ切り替える
        if (EnemyBGMController.instance != null)
        {
            EnemyBGMController.instance.ChangeBGMFromStageBGMToChasePlayerBGM();
        }
    }

    /// <summary>
    /// 演出用にBGM・移動音・警戒音を停止する
    /// </summary>
    private void StopSoundsForDirection()
    {
        //警戒音を停止
        StopFindPlayerSE();

        //歌を停止
        StopSong();

        //移動音を停止
        AudioSource moveSE = GetAudioSourceSE();
        if (moveSE != null)
        {
            moveSE.Stop();
        }

        //ステージBGMを停止する
        if (Stage01Controller.instance != null)
        {
            Stage01Controller.instance.StopStageBGM();
        }

        //プレイヤーを追従するBGMを停止する
        if (EnemyBGMController.instance != null)
        {
            EnemyBGMController.instance.StopChasePlayerBGM();
        }
    }

    /// <summary>
    /// 「みーつけた」音声を再生する
    /// </summary>
    /// <returns>音声の長さ(秒)</returns>
    private float PlayFoundVoice()
    {
        if (foundVoiceSEid < 0 || sO_SE == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 「みーつけた」音声のSE IDが未設定です。");
            return 0f;
        }

        AudioClip clip = sO_SE.GetSEClip(foundVoiceSEid);
        if (clip == null)
        {
            return 0f;
        }

        audioSourceFoundVoice = CreateSEAudioSourceIfNeeded(audioSourceFoundVoice);
        audioSourceFoundVoice.loop = false;
        audioSourceFoundVoice.PlayOneShot(clip);

        return clip.length;
    }

    /// <summary>
    /// SE用のAudioSourceが無ければ生成する
    /// </summary>
    /// <param name="source">現在のAudioSource</param>
    /// <returns>使用するAudioSource</returns>
    private AudioSource CreateSEAudioSourceIfNeeded(AudioSource source)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            //2Dで再生(演出中は距離に関係なくはっきり聞こえるようにする)
            source.spatialBlend = 0f;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroup・音量を反映
        if (MusicController.instance != null)
        {
            source.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
            if (MusicController.instance.sESlider != null)
            {
                source.volume = MusicController.instance.sESlider.value;
            }
        }

        return source;
    }
}
