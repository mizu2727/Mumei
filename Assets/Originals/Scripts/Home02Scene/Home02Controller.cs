using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

/// <summary>
/// Home02Sceneで使用する管理クラス
/// </summary>
public class Home02Controller : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Home02Controller instance;

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// audioClipBGM
    /// </summary>
    private AudioClip audioClipBGM;

    /// <summary>
    /// HomeSceneBGMのID
    /// </summary>
    private readonly int homeSceneBGMId = 1;


    /// <summary>
    /// AudioSourceBGMを取得する
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// AudioClipBGMを取得する
    /// </summary>
    /// <returns>AudioClipBGM</returns>
    public AudioClip GetAudioClipBGM()
    {
        return audioClipBGM;
    }

    /// <summary>
    /// HomeSceneBGMのIDを取得する
    /// </summary>
    /// <returns>HomeSceneBGMのID</returns>
    public int GetHomeSceneBGMId()
    {
        return homeSceneBGMId;
    }


    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM音量変更時のイベント登録
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateBGMVolume(float volume)
    {
        if (audioSourceBGM != null)
        {
            audioSourceBGM.volume = volume;
        }
    }

    /// <summary>
    /// シーン遷移時に処理を呼び出す関数
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    private void OnDestroy()
    {
        //BGM音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;


        //インスタンスが存在する場合
        if (instance != null)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }

    /// <summary>
    /// オブジェクト等を安全に破棄する関数
    /// </summary>
    /// <typeparam name="T">型のテンプレート</typeparam>
    /// <param name="obj">オブジェクト</param>
    /// <param name="t">リセット数値</param>
    private void DestroySafe<T>(ref T obj, float t = 0) where T : Object
    {
        if (obj != null)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Object.Destroy(obj, t);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
#else
            Object.Destroy(obj, t);
#endif
            obj = null;
        }
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGMを設定
        audioSourceBGM = MusicController.instance.GetOtherBGMAudioSource();

        //audioClipBGMを設定
        audioClipBGM = sO_BGM.GetBGMClip(homeSceneBGMId);

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }


    void Awake()
    {
        //インスタンスがnullの場合
        if (instance == null)
        {
            //インスタンス生成
            instance = this;
        }
        else
        {
            //ゲームオブジェクト破棄
            Destroy(gameObject);
        }

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();
    }

    private void Start()
    {
        //シーンステータスをkHome02Sceneに設定
        GameController.instance.SetViewScene(ViewScene.kHome02Scene);

        //ゲームモードステータスをPlayInGameに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

        //BGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(homeSceneBGMId), homeSceneBGMId);
    }
}
