using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

/// <summary>
/// GameOverSceneで使用する管理クラス
/// </summary>
public class GameOverScene : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameOverScene instance;

    [Header("ゲームオーバー画面のCanvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("ゲームオーバーパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject gameOverPanel;

    /// <summary>
    /// ゲームオーバーパネル閲覧フラグ
    /// </summary>
    private bool isViewGameOverPanel = false;

    [Header("タイトルへ戻るパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToTitlePanel;

    /// <summary>
    /// タイトルへ戻るパネル閲覧フラグ
    /// </summary>
    private bool isReturnToTitlePanel = false;

    /// <summary>
    /// ロードしたいScene名
    /// </summary>
    private string SceneName;

    /// <summary>
    /// AudioSource
    /// </summary>
    private AudioSource audioSourceBGM;

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// GameOverBGMのID
    /// </summary>
    private readonly int gameOverBGMId = 5;

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
        audioSourceBGM = MusicController.instance.GetAudioSource();

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        //gameOverCanvasが存在する場合
        if (gameOverCanvas != null) 
        {
            //gameOverCanvasをnullに設定
            gameOverCanvas = null;
        }

        //instanceが存在する場合
        if (instance != null)
        {
            //instanceをnullに設定
            instance = null;
        }
    }
    private void Start()
    {
        //シーン名配列インデックス番号
        switch (saveStageSceneNameArrayIndex) 
        {
            //ステージ1の場合
            case 1:
                //SceneNameをStage01に設定
                SceneName = CommonController.instance.GetStringStage01();
                break;

            //ステージ2の場合
            case 2:
                //SceneNameをStage02に設定
                SceneName = CommonController.instance.GetStringStage02();
                break;

            //ステージ3の場合
            case 3:
                //SceneNameをStage03に設定
                SceneName = CommonController.instance.GetStringStage03();  
                break;

            //ステージ4の場合
            case 4:
                //SceneNameをStage04に設定
                SceneName = CommonController.instance.GetStringStage04();
                break;

            default:
                Debug.LogError("シーン名配列インデックス番号が正しくありません。SceneNameを空に設定します。");
                break;
        }

        //シーンステータスをkGameOverSceneに設定
        GameController.instance.SetViewScene(ViewScene.kGameOverScene);

        //ゲームオーバーステータスに変更
        GameController.instance.gameModeStatus = GameModeStatus.GameOver;

        //ゲームオーバーBGM再生
        MusicController.instance.PlayNoLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameOverBGMId), gameOverBGMId);

        //ゲームオーバーUI表示
        ViewGameOverUI();
    }

    /// <summary>
    /// ゲームオーバー時のUIを表示
    /// </summary>
    private void ViewGameOverUI() 
    {
        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //ゲームオーバーパネルを表示にし、タイトルへ戻るパネルを非表示する
        isViewGameOverPanel = true;
        ChangeViewGameOverPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }

    /// <summary>
    /// ゲームオーバーパネルの表示/非表示
    /// </summary>
    private void ChangeViewGameOverPanel()
    {
        if (isViewGameOverPanel)
        {
            //表示
            gameOverPanel.SetActive(true);
        }
        else
        {
            //非表示
            gameOverPanel.SetActive(false);
        }
    }

    /// <summary>
    /// タイトルへ戻るパネルの表示/非表示
    /// </summary>
    private void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            //表示
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            //非表示
            returnToTitlePanel.SetActive(false);
        }
    }

    /// <summary>
    /// リスタートボタン押下時の処理
    /// </summary>
    public void OnClickedRestartGameButton()
    {
        //ゲームモードをPlayInGameへ変更
        GameController.instance.gameModeStatus = GameModeStatus.PlayInGame;

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //ステージのシーンをロードする
        SceneManager.LoadScene(SceneName);
    }

    /// <summary>
    /// タイトルへ戻るボタン押下時の処理
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //ゲームオーバーパネルを非表示にし、タイトルへ戻るパネルを表示する
        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isViewGameOverPanel = false;
        ChangeViewGameOverPanel();
    }

    /// <summary>
    /// 「はい」押下
    /// </summary>
    public void OnClickedYesButton()
    {
        //難易度をなしにリセット
        DifficultyLevelController.instance.SetDifficultyLevelStatus(DifficultyLevelController.DifficultyLevel.kNone);

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //TitleSceneをロードする
        SceneManager.LoadScene(CommonController.instance.GetTitleSceneName());
    }

    /// <summary>
    /// 「いいえ」押下
    /// </summary>
    public void OnClickedNoButton()
    {
        //ゲームオーバーパネルを表示にし、タイトルへ戻るパネルを非表示する
        isViewGameOverPanel = true;
        ChangeViewGameOverPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }
}