using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameOverScene : MonoBehaviour
{
    [Header("�Q�[���I�[�o�[��ʂ�Canvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("���[�h������Scene��")]
    [SerializeField] private string SceneName;

    [Header("�T�E���h�֘A")]
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
    /// �Q�[���I�[�o�[����UI��\��
    /// </summary>
    void ViewGameOverUI() 
    {
        gameOverCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }


    /// <summary>
    /// ���X�^�[�g�{�^���������̏���
    /// </summary>
    public void OnClickedRestartGameButton()
    {
        GameController.instance.gameModeStatus = GameModeStatus.PlayInGame;
        SceneManager.LoadScene(SceneName);
    }

    /// <summary>
    /// �^�C�g���֖߂�{�^���������̏���
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
