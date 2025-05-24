using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private string SceneName;

    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip gameOverSE;

    void Start()
    {
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
