using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameOverScene : MonoBehaviour
{
    [Header("ゲームオーバー画面のCanvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("ロードしたいScene名")]
    [SerializeField] private string SceneName;

    [Header("サウンド関連")]
    private AudioSource audioSourceBGM;
    [SerializeField] private AudioClip gameOverBGM;


    void Start()
    {
        GameController.instance.gameModeStatus = GameModeStatus.GameOver;
        audioSourceBGM = MusicController.Instance.GetAudioSource();
        MusicController.Instance.PlayNoLoopBGM(audioSourceBGM, gameOverBGM);
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
