using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class HomeController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static HomeController instance;

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


    [Header("タイトルへ戻るボタン(ヒエラルキー上からアタッチすること(バグNo.Er001への一時的な措置))")]
    [SerializeField] private GameObject returnToTitlePanel;


    [Header("wall_Tutorial(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public GameObject wall_Tutorial;

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
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGMを設定
        audioSourceBGM = MusicController.instance.GetAudioSource();

        //audioClipBGMを設定
        audioClipBGM = sO_BGM.GetBGMClip(homeSceneBGMId);

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
        GameController.instance.ResetParams();

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();

        //バグNo.Er001への一時的な措置
        returnToTitlePanel.SetActive(false);
    }

    private void Start()
    {
        //ホームシーンBGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(homeSceneBGMId), homeSceneBGMId);
    }
}
