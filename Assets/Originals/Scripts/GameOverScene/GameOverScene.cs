using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameOverScene : MonoBehaviour
{
    [Header("ゲームオーバー画面のCanvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("ロードしたいScene名")]
    [SerializeField] private string SceneName;

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

    void Start()
    {
        //ゲームオーバーステータスに変更
        GameController.instance.gameModeStatus = GameModeStatus.GameOver;
        audioSourceBGM = MusicController.Instance.GetAudioSource();

        //ゲームオーバーBGM再生
        MusicController.Instance.PlayNoLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameOverBGMId));

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
        GameController.instance.gameModeStatus = GameModeStatus.PlayInGame;
        SceneManager.LoadScene(SceneName);
    }

    /// <summary>
    /// タイトルへ戻るボタン押下時の処理
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
