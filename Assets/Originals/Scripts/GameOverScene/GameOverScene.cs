using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    [Header("ゲームオーバー画面のCanvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("ロードしたいScene名")]
    [SerializeField] private string SceneName;

    [Header("サウンド関連")]
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip gameOverSE;

    void Start()
    {
        audioSourceSE = MusicController.Instance.GetAudioSource();

        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        MusicController.Instance.PlayAudioSE(audioSourceSE, gameOverSE);
        //MusicController.instance.StopBGM();
    }

    public void OnClickedRestartGameButton()
    {
        SceneManager.LoadScene(SceneName);
    }

    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
