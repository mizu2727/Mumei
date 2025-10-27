using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using static GameController;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    [Header("�^�C�g����ʂ�Canvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("���[�h������Scene��")]
    [SerializeField] private string SceneName;

    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// �^�C�g��BGM��ID
    /// </summary>
    private readonly int titleBGMid = 0;

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
        //audioSourceBGM��ݒ�
        audioSourceBGM = MusicController.Instance.GetAudioSource();

        //MusicController�Őݒ肳��Ă���BGM�p��AudioMixerGroup��ݒ肷��
        audioSourceBGM.outputAudioMixerGroup = MusicController.Instance.audioMixerGroupBGM;
    }

    private void Awake()
    {
        Time.timeScale = 1;
        titlesCanvas.enabled = true;
        Cursor.visible = true;

        //�}�E�X�J�[�\�����E�B���h�E�̊O�ɏo��
        Cursor.lockState = CursorLockMode.None;

        //�Q�[�����[�h�X�e�[�^�X��StopInGame�ɕύX
        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

        
    }

    private void Start()
    {
        //�^�C�g��BGM���Đ�
        MusicController.Instance.PlayNoLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(titleBGMid), titleBGMid);
    }

    /// <summary>
    /// �u�X�^�[�g�v�������̏���
    /// </summary>
    public void OnStartButtonClicked()
    {
        //GameController.instance.playCount++;
        GameController.playCount++;
        SceneManager.LoadScene(SceneName);        
    }

    /// <summary>
    /// �Q�[���I������
    /// </summary>
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}