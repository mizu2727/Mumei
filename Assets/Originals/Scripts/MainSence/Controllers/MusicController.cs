using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    //インスタンス
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [Header("BGM")]
    [SerializeField] private AudioSource audioSourceBGM;

    [Header("BGMクリップ")]
    [SerializeField] private AudioClip[] audioClipBGM;


    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioClip番号
    /// </summary>
    public int audioClipnum = 0;

    /// <summary>
    /// audioClip番号(保存用)
    /// </summary>
    private int keepAudioClipnum = 999;


    /// <summary>
    /// SE用audioSourceのリスト
    /// </summary>
    private readonly List<AudioSource> audioSourceSEList = new ();

    [Header("デバッグフラグ")]
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

        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン遷移時に無効なAudioSourceをクリーンアップ
        audioSourceSEList.RemoveAll(audioSource => audioSource == null || !audioSource.gameObject.activeInHierarchy);

        //BGMを止める
        sO_BGM.StopAllBGM();
    }

    void Start()
    {
        //デバッグモードの場合処理をスキップ
        if (isDebug) return;

        audioClipnum = 0;
        PlayBGM();
    }

    /// <summary>
    /// BGM再生
    /// </summary>
    public void PlayBGM()
    {
        if (!isDebug)
        {
            keepAudioClipnum = audioClipnum;
            audioSourceBGM.clip = audioClipBGM[audioClipnum];
            audioSourceBGM.loop = true;
            audioSourceBGM.Play();
        }
    }

    /// <summary>
    /// BGM一時停止
    /// </summary>
    public void PauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.Pause();
    }

    /// <summary>
    /// BGM一時停止解除
    /// </summary>
    public void UnPauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.UnPause();
    }

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM()
    {
        keepAudioClipnum = 999;
        audioSourceBGM.Stop();
    }

    /// <summary>
    ///  新しいAudioSourceを取得または作成
    /// </summary>
    /// <returns></returns>
    public AudioSource GetAudioSource()
    {
        //使用していない有効なAudioSourceを探す
        foreach (var audioSource in audioSourceSEList)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy && !audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        //新しいAudioSourceを作成
        var audioSourceNew = gameObject.AddComponent<AudioSource>();
        audioSourceNew.playOnAwake = false;
        audioSourceSEList.Add(audioSourceNew);
        return audioSourceNew;
    }

    /// <summary>
    /// SE再生
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //効果音クリップを設定
                audioSource.clip = audioClip;

                //ループ再生を無効化
                audioSource.loop = false;

                //効果音を再生
                audioSource.Play(); 
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip が null です！");
            }
        }
    }

    /// <summary>
    /// SEを一瞬だけ再生
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public async void PlayMomentAudioSE(AudioSource audioSource, AudioClip audioClip)
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
                Debug.LogWarning("[PlayMomentAudioSE] audioClip が null です！");
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

        StopSE(audioSource);
    }

    /// <summary>
    /// SEをループ再生
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
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

    /// <summary>
    /// SEを一時停止
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
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
    }

    /// <summary>
    /// SEの一時停止解除
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
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
    }

    /// <summary>
    /// SE停止
    /// </summary>
    /// <param name="audioSource"></param>
    public void StopSE(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// 指定されたAudioSourceが再生中か確認
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    public bool IsPlayingSE(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// 現在のSEクリップを取得
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    public AudioClip GetCurrentSE(AudioSource audioSource)
    {
        return audioSource != null ? audioSource.clip : null;
    }
}