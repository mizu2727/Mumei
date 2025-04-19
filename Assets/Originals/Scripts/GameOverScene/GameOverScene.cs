using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    [SerializeField] private Canvas gameOverCanvas;//ゲームオーバー画面のCanvas
    [SerializeField] private string SceneName;

    void Start()
    {
        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    //やり直す
    public void OnClickedRestartGameButton()
    {
        SceneManager.LoadScene(SceneName);
    }

    //タイトルへ戻る
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
