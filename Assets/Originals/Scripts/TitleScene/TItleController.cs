using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using static GameController;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static TitleController instance;

    [Header("タイトル画面のCanvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("タイトルパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject titlePanel;

    /// <summary>
    /// OpeningScene
    /// </summary>
    private string stringOpeningScene = "OpeningScene";

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// タイトルBGMのID
    /// </summary>
    private readonly int titleBGMId = 0;

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

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }

    private void Awake()
    {
        //インスタンスがnullの場合
        if (instance == null)
        {
            //インスタンス生成
            instance = this;
        }
        else
        {
            //インスタンスを破棄
            Destroy(this.gameObject);
        }

        //シーンステータスをkTitleSceneに設定
        GameController.instance.SetViewScene(ViewScene.kTitleScene);

        Time.timeScale = 1;
        titlesCanvas.enabled = true;
        Cursor.visible = true;

        //マウスカーソルをウィンドウの外に出す
        Cursor.lockState = CursorLockMode.None;

        //ゲームモードステータスをStopInGameに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

    }

    private void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //タイトルBGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(titleBGMId), titleBGMId);
    }

    /// <summary>
    /// 「スタート」押下時の処理
    /// </summary>
    public void OnStartButtonClicked()
    {
        //GameController.instance.playCount++;
        GameController.playCount++;

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //OpeningSceneをロードする
        SceneManager.LoadScene(stringOpeningScene);        
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    public void EndGame()
    {
        //明るさ設定を保存
        BrightnessAdjustmentController.instance.SaveBrightnessValue();

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //ゲーム終了
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        //titlesCanvasが存在する場合
        if (titlesCanvas != null) 
        {
            //titlesCanvasをnullにする
            titlesCanvas = null;
        }

        //titlePanelが存在する場合
        if (titlePanel != null) 
        {
            //titlePanelをnullにする
            titlePanel = null;
        }

        //インスタンスが存在する場合
        if (instance == this)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }
}