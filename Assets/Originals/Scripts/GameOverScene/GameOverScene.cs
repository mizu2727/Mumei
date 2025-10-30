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

    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM���ʕύX���̃C�x���g�o�^
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void OnDisable()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^����
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
    private void UpdateBGMVolume(float volume)
    {
        if (audioSourceBGM != null)
        {
            audioSourceBGM.volume = volume;
        }
    }

    /// <summary>
    /// �V�[���J�ڎ��ɏ������Ăяo���֐�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        //AudioSource�̏�����
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource() 
    {
        audioSourceBGM = MusicController.instance.GetAudioSource();

        //MusicController�Őݒ肳��Ă���BGM�p��AudioMixerGroup��ݒ肷��
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }


    void Start()
    {
        //�Q�[���I�[�o�[�X�e�[�^�X�ɕύX
        GameController.instance.gameModeStatus = GameModeStatus.GameOver;

        //�Q�[���I�[�o�[BGM�Đ�
        MusicController.instance.PlayNoLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameOverBGMId), gameOverBGMId);

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
