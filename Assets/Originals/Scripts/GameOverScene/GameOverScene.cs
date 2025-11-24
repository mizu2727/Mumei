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

    [Header("ロードしたいScene名")]
    [SerializeField] private string SceneName;

    /// <summary>
    /// TitleSceneのシーン名
    /// </summary>
    const string stringTitleScene = "TitleScene";

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


    void Start()
    {
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
    void ViewGameOverUI() 
    {
        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
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
        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //TitleSceneをロードする
        SceneManager.LoadScene(stringTitleScene);
    }
}
