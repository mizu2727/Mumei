using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

/// <summary>
/// GameClearSceneで使用する管理クラス
/// </summary>
public class GameClearController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameClearController instance;

    [Header("ゲームクリア画面のCanvas")]
    [SerializeField] public Canvas gameClearCanvas;

    /// <summary>
    /// アンケートURL
    /// </summary>
    private const string questionnaireURL = "https://docs.google.com/forms/d/13qMmaottZOaX7lg4lu8kzzvfE5aXFS3kNgPPoCmxI6M/viewform";

    /// <summary>
    /// XのURL
    /// </summary>
    private const string X_URL = "https://x.com/Tomanegi0707";

    /// <summary>
    /// BlueSkyのURL
    /// </summary>
    private const string BlueSky_URL = "https://bsky.app/profile/tomanegi0707.bsky.social";

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
    /// GameClearSceneBGMのID
    /// </summary>
    private readonly int gameClearSceneBGMId = 3;


    /// <summary>
    /// TitleSceneのシーン名
    /// </summary>
    const string stringTitleScene = "TitleScene";

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        //gameClearCanvasが存在する場合
        if (gameClearCanvas != null)
        {
            //gameClearCanvasをnullに設定
            gameClearCanvas = null;
        }

        //instanceが存在する場合
        if (instance != null)
        {
            //instanceをnullに設定
            instance = null;
        }
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();
    }

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
    public int GetGameClearSceneBGMId()
    {
        return gameClearSceneBGMId;
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
        audioClipBGM = sO_BGM.GetBGMClip(gameClearSceneBGMId);

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }


    void Start()
    {
        //シーンステータスをkGameClearSceneに設定
        GameController.instance.SetViewScene(ViewScene.kGameClearScene);

        //ゲームモードステータスをStoryに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
        HiddenGameClearUI();

        //ゲームクリアシーンBGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameClearSceneBGMId), gameClearSceneBGMId);
    }

    /// <summary>
    /// タイトルへ戻る
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //タイトルへ戻る
        SceneManager.LoadScene(stringTitleScene);
    }

    /// <summary>
    /// ゲームクリア時のUIを表示
    /// </summary>
    public void ViewGameClearUI() 
    {
        gameClearCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// ゲームクリア時のUIを非表示
    /// </summary>
    void HiddenGameClearUI()
    {
        gameClearCanvas.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// アンケートURLを開く
    /// </summary>
    public void OnClickedQuestionnaire_Button() 
    {
        Application.OpenURL(questionnaireURL);
    }

    /// <summary>
    /// XのURLを開く
    /// </summary>
    public void OnClickedX_Button()
    {
        Application.OpenURL(X_URL);
    }

    /// <summary>
    /// BlueSkyのURLを開く
    /// </summary>
    public void OnClickedBlueSky_Button()
    {
        Application.OpenURL(BlueSky_URL);
    }
}
