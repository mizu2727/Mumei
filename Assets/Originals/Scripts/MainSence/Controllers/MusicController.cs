using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [Header("BGM")]
    [SerializeField] private AudioSource audioSourceBGM;

    [Header("BGMクリップ")]
    [SerializeField] private AudioClip[] audioClipBGM;

    public int audioClipnum = 0;

    private int keepAudioClipnum = 999;

    //[Header("敵に追われている時のBGM")]
    //[SerializeField] private AudioSource audioSourceEnemyBGM;

    //[SerializeField] private AudioClip audioClipEnemyBGM;

    private readonly List<AudioSource> audioSourceSEList = new ();


    [SerializeField] private bool isDebug;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSourceBGM = GetComponent<AudioSource>();


        if (audioSourceBGM == null)
        {
            Debug.LogWarning("AudioSourceBGM not found, adding one.");
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        }
        Debug.Log($"MusicController initialized. Instance: {_instance}, GameObject: {gameObject.name}");

        // シーン遷移時のクリーンアップハンドラを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン遷移時に無効なAudioSourceをクリーンアップ
        audioSourceSEList.RemoveAll(audioSource => audioSource == null || !audioSource.gameObject.activeInHierarchy);
        Debug.Log($"シーン {scene.name} に遷移。audioSourceSEList の有効なAudioSource数: {audioSourceSEList.Count}");
    }

    void Start()
    {
        if (isDebug) return;

        audioClipnum = 0;
        PlayBGM();
    }




    // BGM再生
    public void PlayBGM()
    {


        if (!isDebug)
        {
            //番号が同じBGMを連続で再生するのを阻止する
            if (keepAudioClipnum != audioClipnum)
            {
                keepAudioClipnum = audioClipnum;
                audioSourceBGM.clip = audioClipBGM[audioClipnum];
                audioSourceBGM.loop = true;
                audioSourceBGM.Play();
                Debug.Log("keepAudioClipnum = " + keepAudioClipnum);
            }
            else 
            {
                Debug.LogWarning("番号が同じBGMを連続で再生しようとしています");
            }
        }
    }

    // BGM一時停止
    public void PauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.Pause();
    }

    // BGM一時停止解除
    public void UnPauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.UnPause();
    }

    // BGM停止
    public void StopBGM()
    {
        keepAudioClipnum = 999;
        audioSourceBGM.Stop();
    }

    //// 敵に追われている時のBGMを再生
    //public void PlayEnemyBGM()
    //{
    //    if (audioSourceEnemyBGM != null && !isDebug)
    //    {
    //        if (audioClipEnemyBGM != null)
    //        {
    //            audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //            audioSourceEnemyBGM.loop = true;
    //            audioSourceEnemyBGM.Play();
    //        }
    //        else
    //        {
    //            Debug.LogWarning("[MusicController] audioClipEnemyBGM が null です！");
    //        }
    //    }
    //}

    //// 敵に追われている時のBGM一時停止
    //public void PauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.Pause();
    //}

    //// 敵に追われている時のBGM一時停止解除
    //public void UnPauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.UnPause();
    //}

    //// 敵に追われている時のBGM停止
    //public void StopEnemyBGM()
    //{
    //    audioSourceEnemyBGM.Stop();
    //}


    // 新しいAudioSourceを取得または作成
    public AudioSource GetAudioSource()
    {
        // 使用中でない有効なAudioSourceを探す
        foreach (var audioSource in audioSourceSEList)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy && !audioSource.isPlaying)
            {
                Debug.Log($"既存の未使用AudioSourceを再利用: {audioSource.GetInstanceID()}");
                return audioSource;
            }
        }

        // 新しいAudioSourceを作成
        var audioSourceNew = gameObject.AddComponent<AudioSource>();
        audioSourceNew.playOnAwake = false;
        audioSourceSEList.Add(audioSourceNew);
        Debug.Log($"新しいAudioSourceを作成: {audioSourceNew.GetInstanceID()}");
        return audioSourceNew;
    }

    // SE再生
    public void PlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip; // 効果音クリップを設定
                audioSource.loop = false; // ループ再生を無効化
                audioSource.Play(); // 効果音を再生
                Debug.Log($"Playing SE: {audioClip.name} on AudioSource: {audioSource.GetInstanceID()}");
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip が null です！");
            }
        }
        else
        {
            Debug.LogWarning($"[MusicController] AudioSource is null or debug mode is enabled (isDebug: {isDebug})");
        }
    }

    // SEを一瞬のみ再生
    public async void PlayMomentAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
                Debug.Log("SEを一瞬のみ再生スタート");
            }
            else
            {
                Debug.LogWarning("[PlayMomentAudioSE] audioClip が null です！");
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

        StopSE(audioSource);
        Debug.Log("SEを一瞬のみ再生停止");
    }

    // SEをループ再生
    public void LoopPlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("[MusicController] loop用のaudioClip が null です！");
            }
        }
    }

    // SEを一時停止
    public void PauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("PauseSE…audioSource :" + audioSource);
            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("PauseSE…audioClip :" + audioClip);
            return;
        }
        audioSource.clip = audioClip;
        audioSource.Pause();
        Debug.Log("SE一時停止");
    }

    // SEの一時停止解除
    public void UnPauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("UnPauseSE…audioSource :" + audioSource);
            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("UnPauseSE…audioClip :" + audioClip);
            return;
        }
        audioSource.clip = audioClip;
        audioSource.UnPause();
        Debug.Log("SE一時停止解除");
    }

    // SE停止
    public void StopSE(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    // 指定されたAudioSourceが再生中か確認
    public bool IsPlayingSE(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // 現在のSEクリップを取得
    public AudioClip GetCurrentSE(AudioSource audioSource)
    {
        return audioSource != null ? audioSource.clip : null;
    }
}