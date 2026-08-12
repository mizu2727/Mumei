using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using static GameController;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// タイトル画面を管理するクラス
/// </summary>
public class TitleController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static TitleController instance;


    /// <summary>
    /// TutorialClearStatus(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringTutorialClearStatus = "TutorialClearStatus";

    /// <summary>
    /// DemoStage01(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringDemoStage01 = "DemoStage01";

    /// <summary>
    /// Stage01(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringStage01 = "Stage01";


    [Header("タイトル画面のCanvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("タイトルパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject titlePanel;

    /// <summary>
    /// タイトルパネルを取得する
    /// </summary>
    /// <returns>タイトルパネル</returns>
    public GameObject GetTitlePanel()
    {
        return titlePanel;
    }


    /*----------------------------------------------------------------
     * BGM関連
     ---------------------------------------------------------------*/

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


    /*----------------------------------------------------------------
     * SE関連
     ---------------------------------------------------------------*/

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE用audioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ボタンSEのID
    /// </summary>
    private readonly int buttonSEid = 4;

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM音量変更時のイベント登録
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
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
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
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
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        //BGM音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;

        //titlesCanvasを安全に削除
        DestroySafe(ref titlesCanvas);

        //titlePanelを安全に削除
        DestroySafe(ref titlePanel);

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
        audioSourceBGM = MusicController.instance.GetAudioSource();

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;

        //AudioSourceSEを取得
        audioSourceSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceSE.playOnAwake = false;
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

        //デモ版用のステージ01を既にクリアしている場合||製品版用のステージ01を既にクリアしている場合
        if ((GameController.instance.GetIsDemoPlayFlag() && saveStageClearStatusArray[stringDemoStage01] == 1)
            || (!GameController.instance.GetIsDemoPlayFlag() && saveStageClearStatusArray[stringStage01] == 1)) 
        {
            //チュートリアルのストーリを閲覧したことを保存する
            saveViewStoryStatusArray[stringTutorialClearStatus] = 1;
        }
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

        //チュートリアルのストーリーを閲覧していない場合
        if (saveViewStoryStatusArray[stringTutorialClearStatus] == 0)
        {
            //OpeningSceneをロードする
            SceneManager.LoadScene(CommonController.instance.GetOpeningSceneName());
        }
        //既にチュートリアルのストーリーを閲覧している場合
        else
        {
            //Home02SceneNameをロードする
            SceneManager.LoadScene(CommonController.instance.GetHome02SceneName());
        }
    }

    /// <summary>
    /// 「データ」押下時の処理
    /// </summary>
    public void OnDataButtonClicked()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //タイトルパネルを非表示にする
        titlePanel.SetActive(false);

        //タイトル画面内のステージ及び難易度情報パネルを表示にする
        DifficultyLevelController.instance.SetIsViewStageAndDifficultyLevelPanel(true);
        DifficultyLevelController.instance.ChangeViewStageAndDifficultyLevelPanel();
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
}