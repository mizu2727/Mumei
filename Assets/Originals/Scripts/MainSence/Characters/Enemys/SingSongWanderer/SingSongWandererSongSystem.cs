using UnityEngine;

public partial class SingSongWanderer
{
    /*---------------------------------------------
     * 歌(walkSE)の制御
     --------------------------------------------*/

    /// <summary>
    /// 歌(walkSE)の再生・停止・最後まで聞いたかの判定
    /// ・プレイヤーが近づいたら最初から再生
    /// ・途中で離れたら停止(再度近づいたら最初から再生し直す)
    /// ・最後まで聞いたら無限追従モードへ移行
    /// </summary>
    private void UpdateSong()
    {
        //ポーズ中の場合
        if (Time.timeScale == 0f || (PauseController.instance != null && PauseController.instance.GetIsPause()))
        {
            //歌を一時停止(ポーズ中に歌が進んで終わってしまうのを防ぐ)
            PauseSong();

            //処理をスキップ
            return;
        }

        //一時停止していた場合は再開
        ResumeSong();

        //歌を再生中の場合
        if (isSinging)
        {
            //再生が続いている場合
            if (audioSourceSong != null && audioSourceSong.isPlaying)
            {
                //再生位置を記録
                lastSongTime = audioSourceSong.time;

                //音量をSE音量に合わせる(設定画面で変更された場合に反映するため)
                UpdateSongVolume();
            }
            //再生が止まった場合(このクラスで止めていない = 曲の終わりまで再生された)
            else
            {
                //最後まで再生されたか(曲の終わり付近まで進んでいたか)を判定
                AudioClip clip = audioSourceSong != null ? audioSourceSong.clip : null;
                bool isReachedEnd = clip != null && lastSongTime >= clip.length - kSongEndTolerance;

                //歌を停止状態にする
                StopSong();

                //最後まで聞いた場合
                if (isReachedEnd)
                {
                    //無限追従モードへ移行
                    FinishSong();

                    //処理をスキップ
                    return;
                }
            }
        }

        //プレイヤーとの距離が近いかを判定
        bool isPlayerNear = Vector3.Distance(transform.position, Player.instance.transform.position) <= singZoneDistance;

        //近づいた場合&&歌っていない場合
        if (isPlayerNear && !isSinging)
        {
            //最初から歌う
            StartSong();
        }
        //離れた場合&&歌っている場合
        else if (!isPlayerNear && isSinging)
        {
            //歌を停止(次に近づいた時は最初から)
            StopSong();
        }
    }

    /// <summary>
    /// 歌(walkSE)を最初から再生する
    /// </summary>
    private void StartSong()
    {
        //walkSEを取得
        AudioClip clip = sO_SE != null ? sO_SE.GetSEClip(WalkSEid) : null;
        if (clip == null)
        {
            if (!isSongMissingWarned)
            {
                Debug.LogWarning($"[{gameObject.name}] 歌(walkSE ID:{WalkSEid})が見つかりません。EnemyInformationのwalkSEIdを確認してください。");
                isSongMissingWarned = true;
            }
            return;
        }

        //歌用のaudioSourceを準備
        audioSourceSong = CreateSEAudioSourceIfNeeded(audioSourceSong);
        audioSourceSong.clip = clip;
        audioSourceSong.loop = false;
        audioSourceSong.pitch = 1.0f;

        //最初から再生
        audioSourceSong.time = 0f;
        audioSourceSong.Play();

        isSinging = true;
        isSongPaused = false;
        lastSongTime = 0f;
    }

    /// <summary>
    /// 歌(walkSE)を停止する(次は最初から再生)
    /// </summary>
    private void StopSong()
    {
        if (audioSourceSong != null)
        {
            audioSourceSong.Stop();
        }

        isSinging = false;
        isSongPaused = false;
        lastSongTime = 0f;
    }

    /// <summary>
    /// 歌(walkSE)を一時停止する
    /// </summary>
    private void PauseSong()
    {
        if (isSinging && !isSongPaused && audioSourceSong != null && audioSourceSong.isPlaying)
        {
            audioSourceSong.Pause();
            isSongPaused = true;
        }
    }

    /// <summary>
    /// 一時停止した歌(walkSE)を再開する
    /// </summary>
    private void ResumeSong()
    {
        if (isSinging && isSongPaused && audioSourceSong != null)
        {
            audioSourceSong.UnPause();
            isSongPaused = false;
        }
    }

    /// <summary>
    /// 歌の音量をSE音量に合わせる
    /// </summary>
    private void UpdateSongVolume()
    {
        if (audioSourceSong != null && MusicController.instance != null && MusicController.instance.sESlider != null)
        {
            audioSourceSong.volume = MusicController.instance.sESlider.value;
        }
    }

    /// <summary>
    /// 歌っている間は通常徘徊状態へ戻す
    /// </summary>
    private void ReturnToPatrolWhileSinging()
    {
        //通常徘徊状態に移行
        currentState = EnemyState.Patrol;
        isAlertMode = false;

        //警戒音を停止
        StopFindPlayerSE();

        //画面エフェクトを非表示
        if (playerFoundPanel != null) playerFoundPanel.SetActive(false);
        if (noiseScreenPanel != null) noiseScreenPanel.SetActive(false);

        //現在の目的地(プレイヤー・音の位置)を破棄し、次フレームで徘徊先を選び直させる
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }
    }

    /// <summary>
    /// 歌を最後まで聞いた場合の処理(無限追従モードへ移行)
    /// </summary>
    private void FinishSong()
    {
        //歌を最後まで聞いたフラグをオンにする
        isSongFinished = true;

        //無限追従の演出(赤い画面・追従BGM)を開始
        PlayInfinityChasePresentation();

        //無限追従モードに切り替える
        currentState = EnemyState.InfinityChase;

        Debug.Log($"[{gameObject.name}] 歌を最後まで聞いたため、無限追従モードへ移行");
    }
}
