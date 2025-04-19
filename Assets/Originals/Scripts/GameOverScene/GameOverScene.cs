using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    [SerializeField] private Canvas gameOverCanvas;//�Q�[���I�[�o�[��ʂ�Canvas
    [SerializeField] private string SceneName;

    void Start()
    {
        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    //��蒼��
    public void OnClickedRestartGameButton()
    {
        SceneManager.LoadScene(SceneName);
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
