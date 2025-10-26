using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameOverScene : MonoBehaviour
{
    [Header("�Q�[���I�[�o�[��ʂ�Canvas")]
    [SerializeField] private Canvas gameOverCanvas;

    [Header("���[�h������Scene��")]
    [SerializeField] private string SceneName;

    /// <summary>
    /// AudioSource
    /// </summary>
    private AudioSource audioSourceBGM;

    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// GameOverBGM��ID
    /// </summary>
    private readonly int gameOverBGMId = 5;

    void Start()
    {
        //�Q�[���I�[�o�[�X�e�[�^�X�ɕύX
        GameController.instance.gameModeStatus = GameModeStatus.GameOver;
        audioSourceBGM = MusicController.Instance.GetAudioSource();

        //�Q�[���I�[�o�[BGM�Đ�
        MusicController.Instance.PlayNoLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameOverBGMId));

        //�Q�[���I�[�o�[UI�\��
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
