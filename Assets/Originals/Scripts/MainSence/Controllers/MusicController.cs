using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static MusicController instance;

    /// <summary>
    /// StageBGM用AudioSource
    /// </summary>
    private AudioSource stageBGMAudioSource;

    /// <summary>
    /// chasePlayerBGM用AudioSource
    /// </summary>
    private AudioSource chasePlayerBGMAudioSource;

    [Header("AudioMixer")]
    public AudioMixer audioMixer;

    [Header("AudioMixerGroupBGM")]
    public AudioMixerGroup audioMixerGroupBGM;

    [Header("AudioMixerGroupSE")]
    public AudioMixerGroup audioMixerGroupSE;

    [Header("BGMSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider bGMSlider;

    /// <summary>
    /// BGM最小音量(Slider用)
    /// </summary>
    private const float minBGMSliderVolume = 0f;

    /// <summary>
    /// SE最大音量(Slider用)
    /// </summary>
    private const float maxBGMSliderVolume = 1f;

    /// <summary>
    /// OnBGMVolumeChangedEvent
    /// </summary>
    public static event Action<float> OnBGMVolumeChangedEvent;

    /// <summary>
    /// BGM最小音量(dB)
    /// </summary>
    private const float minBGMVolume = -80f;

    /// <summary>
    /// BGM最大音量(dB)
    /// </summary>
    private const float maxBGMVolume = 0f;

    [Header("SESlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider sESlider;

    /// <summary>
    /// SE最小音量(Slider用)
    /// </summary>
    private const float minSESliderVolume = 0f;

    /// <summary>
    /// SE最大音量(Slider用)
    /// </summary>
    private const float maxSESliderVolume = 1f;

    /// <summary>
    /// OnSEVolumeChangedEvent
    /// </summary>
    public static event Action<float> OnSEVolumeChangedEvent;

    /// <summary>
    /// SE最小音量(dB)
    /// </summary>
    private const float minSEVolume = -80f;

    /// <summary>
    /// SE最大音量(dB)
    /// </summary>
    private const float maxSEVolume = 0f;


    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;


    /// <summary>
    /// SE用audioSourceのリスト
    /// </summary>
    private readonly List<AudioSource> audioSourceSEList = new ();

    [Header("デバッグフラグ")]
    [SerializeField] private bool isDebug;


    /// <summary>
    /// ステージBGM専用のAudioSourceを取得するメソッド
    /// </summary>
    /// <returns>ステージBGM用AudioSource</returns>
    public AudioSource GetStageBGMAudioSource()
    {
        return stageBGMAudioSource;
    }

    /// <summary>
    /// 追跡BGM専用のAudioSourceを取得するメソッド
    /// </summary>
    /// <returns>追跡BGM用AudioSource</returns>
    public AudioSource GetChasePlayerBGMAudioSource()
    {
        return chasePlayerBGMAudioSource;
    }

    /// <summary>
    /// BGM最小音量取得(Slider用)
    /// </summary>
    /// <returns>BGM最小音量(Slider用)</returns>
    public float GetMinBGMSliderVolume()
    {
        return minBGMSliderVolume;
    }

    /// <summary>
    /// BGM最大音量取得(Slider用)
    /// </summary>
    /// <returns>BGM最大音量(Slider用)</returns>
    public float GetMaxBGMSliderVolume()
    {
        return maxBGMSliderVolume;
    }

    /// <summary>
    /// SE最小音量取得(Slider用)
    /// </summary>
    /// <returns>SE最小音量(Slider用)</returns>
    public float GetMinSESliderVolume()
    {
        return minSESliderVolume;
    }

    /// <summary>
    /// SE最大音量取得(Slider用)
    /// </summary>
    /// <returns>SE最大音量(Slider用)</returns>
    public float GetMaxSESliderVolume()
    {
        return maxSESliderVolume;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

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

        InitializeAudioSource();

        //BGMを止める
        sO_BGM.StopAllBGM();


        if (bGMSlider != null)
        {
            //BGMのボリュームをbgmSliderで調整できるようにする
            bGMSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        else 
        {
            Debug.LogWarning("bGMSlider がアタッチされていません！");
        }

        if (sESlider != null)
        {
            //SEのボリュームをseSliderで調整できるようにする
            sESlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }
        else
        {
            Debug.LogWarning("sESlider がアタッチされていません！");
        }
    }

    private void InitializeAudioSource() 
    {
        if (stageBGMAudioSource == null)
        {
            stageBGMAudioSource = gameObject.AddComponent<AudioSource>();
            stageBGMAudioSource.outputAudioMixerGroup = audioMixerGroupBGM;
            stageBGMAudioSource.playOnAwake = false;
            stageBGMAudioSource.loop = true; // 必要に応じて
        }

        if (chasePlayerBGMAudioSource == null)
        {
            chasePlayerBGMAudioSource = gameObject.AddComponent<AudioSource>();
            chasePlayerBGMAudioSource.outputAudioMixerGroup = audioMixerGroupBGM;
            chasePlayerBGMAudioSource.playOnAwake = false;
            chasePlayerBGMAudioSource.loop = true;
        }
    }

    /// <summary>
    /// スライダーの値をdBに変換して、AudioMixerの「BGM」パラメータに反映させる
    /// </summary>
    /// <param name="value">スライダーの値</param>
    private void OnBGMVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, minBGMVolume, maxBGMVolume);

        //"BGMVolumeParam"はAudioMixerで定義したパラメータ名と一致している必要がある
        audioMixer.SetFloat("BGMVolumeParam", decibel);

        //値を0 ~ 1へ変換
        OnBGMVolumeChangedEvent?.Invoke(value);
    }

    /// <summary>
    /// スライダーの値をdBに変換して、AudioMixerの「SE」パラメータに反映させる
    /// </summary>
    /// <param name="value">スライダーの値</param>
    private void OnSEVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, minSEVolume, maxSEVolume);

        //"SEVolumeParam"はAudioMixerで定義したパラメータ名と一致している必要がある
        audioMixer.SetFloat("SEVolumeParam", decibel);

        //値を0 ~ 1へ変換
        OnSEVolumeChangedEvent?.Invoke(value);
    }

    void Start()
    {
        //デバッグモードの場合処理をスキップ
        if (isDebug) return;
    }

    /// <summary>
    /// BGMをループありで再生
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayLoopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //クリップを設定
                audioSource.clip = audioClip;

                //ループ再生を有効
                audioSource.loop = true;

                //再生
                audioSource.Play();

                //BGMの状態をPlayに変更
                sO_BGM.ChangeFromStopToPlayBGM(bgmId);
            }
        }
    }

    /// <summary>
    /// BGMをループなしで再生
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayNoLoopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //クリップを設定
                audioSource.clip = audioClip;

                //ループ再生を無効化
                audioSource.loop = false;

                //再生
                audioSource.Play();

                //BGMの状態をPlayに変更
                sO_BGM.ChangeFromStopToPlayBGM(bgmId);
            }
        }
    }

    /// <summary>
    /// BGM一時停止
    /// </summary>
    public void PauseBGM(AudioSource audioSource, AudioClip audioClip, int bgmId) 
    {
        //クリップを設定
        audioSource.clip = audioClip;

        //中断
        audioSource.Pause();

        //BGMの状態をPauseに変更
        sO_BGM.ChangeFromPlayToPauseBGM(bgmId);
    }

    /// <summary>
    /// BGM一時停止解除
    /// </summary>
    public void UnPauseBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        //クリップを設定
        audioSource.clip = audioClip;

        //中断解除
        audioSource.UnPause();

        //BGMの状態をPlayに戻す
        sO_BGM.ChangeFromPauseToPlayBGM(bgmId);
    }

    /// <summary>
    /// BGM停止
    /// </summary>
    public void StopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId) 
    {
        //クリップを設定
        audioSource.clip = audioClip;

        //中断解除
        audioSource.Stop();

        //BGMの状態をStopに変更
        sO_BGM.ChangeFromPlayToStopBGM(bgmId);
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