using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [SerializeField] private AudioSource audioSourceBGM;
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
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (isDebug) return;
        PlayBGM();
    }

    // BGM再生
    public void PlayBGM()
    {
        audioSourceBGM.Play();
    }

    // BGM一時停止
    public void PauseBGM()
    {
        audioSourceBGM.Pause();
    }

    // BGM一時停止解除
    public void UnPauseBGM()
    {
        audioSourceBGM.UnPause();
    }

    // BGM停止
    public void StopBGM()
    {
        audioSourceBGM.Stop();
    }

    // 新しいAudioSourceを取得または作成
    public AudioSource GetAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceSEList.Add(audioSource);
        return audioSource;
    }

    // SE再生
    public void PlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip が null です！");
            }
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
        audioSource.clip = audioClip;
        audioSource.Pause();
        Debug.Log("SE一時停止");
    }

    // SEの一時停止解除
    public void UnPauseSE(AudioSource audioSource, AudioClip audioClip)
    {
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