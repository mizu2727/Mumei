using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage01Controller : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Stage01Controller instance;

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// Stage01BGMのID
    /// </summary>
    private readonly int stage01BGMId = 2;



    /// <summary>
    /// AudioSourceBGMを取得する
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// Stage01BGMのIDを取得する
    /// </summary>
    /// <returns>Stage01BGMのID</returns>
    public int GetStage01BGMId()
    {
        return stage01BGMId;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM音量変更時のイベント登録
        //MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //BGM音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateBGMVolume(float volume)
    {
        //NullReferenceExceptionを防ぐ用
        if (audioSourceBGM == null)
        {
            Debug.LogWarning("Stage01Controller: audioSourceBGM が未設定のため音量変更をスキップしました。");
            return;
        }

        audioSourceBGM.volume = volume;
    }

    /// <summary>
    /// シーン遷移時に処理を呼び出す関数
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGMを設定
        audioSourceBGM = MusicController.instance.GetStageBGMAudioSource();

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;

        //BGM音量変更時のイベント登録
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //Stage01BGMを流す。現在再生中のBGMを設定する。
        PlayStage01BGM();
    }

    /// <summary>
    /// Stage01BGMを流す。現在再生中のBGMを設定する。
    /// </summary>
    public void PlayStage01BGM() 
    {
        //Stage01BGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(stage01BGMId), stage01BGMId);

        //現在再生中のBGMをStage01BGMに設定する
        PauseController.instance.SetNowPlayBGMId(stage01BGMId);
    }

}
