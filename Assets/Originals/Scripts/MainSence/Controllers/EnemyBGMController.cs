using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBGMController : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static EnemyBGMController instance;

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stage01 = "Stage01";

    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// �v���C���[���G�ɒǂ���ۂ�BGM��ID
    /// </summary>
    private readonly int chasePlayerBGMId = 4;

    /// <summary>
    /// �ۑ��pAudioSourceBGM
    /// </summary>
    private AudioSource keepAudioSourceBGM;

    /// <summary>
    /// �ۑ��pAudioClipBGM
    /// </summary>
    private AudioClip keepAudioClipBGM;

    /// <summary>
    /// �ۑ��pAudioBGMID
    /// </summary>
    private int keepAudioBGMId = 999;

    /// <summary>
    /// AudioSourceBGM���擾����
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// �v���C���[���G�ɒǂ���ۂ�BGM��ID���擾
    /// </summary>
    /// <returns>�v���C���[���G�ɒǂ���ۂ�BGM��ID</returns>
    public int GetChasePlayerBGMId()
    {
        return chasePlayerBGMId;
    }

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

        //BGM���ʕύX���̃C�x���g�o�^����
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
    private void UpdateBGMVolume(float volume)
    {
        if (audioSourceBGM == null)
        {
            Debug.LogWarning("EnemyBGMController: audioSourceBGM �����ݒ�̂��߉��ʕύX���X�L�b�v���܂����B");
            return;
        }

        audioSourceBGM.volume = volume;
    }

    /// <summary>
    /// �V�[���J�ڎ��ɏ������Ăяo���֐�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGM��ݒ�
        audioSourceBGM = MusicController.instance.GetChasePlayerBGMAudioSource();

        //MusicController�Őݒ肳��Ă���BGM�p��AudioMixerGroup��ݒ肷��
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //AudioSource�̏�����
        InitializeAudioSource();
    }

    /// <summary>
    /// �v���C���[��Ǐ]����BGM�𗬂��B���ݍĐ���BGM��ݒ肷��B
    /// </summary>
    public void PlayChasePlayerBGM() 
    {
        //BGM���Đ�
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(chasePlayerBGMId), chasePlayerBGMId);

        //���ݍĐ�����BGM��ChasePlayerBGM�ɐݒ肷��
        PauseController.instance.SetNowPlayBGMId(chasePlayerBGMId);
    }

    /// <summary>
    /// �X�e�[�WBGM����v���C���[��Ǐ]����BGM�֐؂�ւ���
    /// </summary>
    public void ChangeBGMFromStageBGMToChasePlayerBGM() 
    {
        //���݂̃V�[�������擾���A���̖��O�ɂ���Ĉꎞ��~����X�e�[�WBGM�����߂�
        switch (SceneManager.GetActiveScene().name)
        {
            //Stage01
            case stage01:

                //�ꎞ��~����BGM�̏���ۑ�����
                keepAudioSourceBGM = Stage01Controller.instance.GetAudioSourceBGM();
                keepAudioClipBGM = sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId());
                keepAudioBGMId = Stage01Controller.instance.GetStage01BGMId();
                break;

            default:
                Debug.LogWarning("���̑��̃V�[����");
                break;
        }

        //�X�e�[�WBGM���ꎞ��~
        MusicController.instance.PauseBGM(keepAudioSourceBGM,keepAudioClipBGM, keepAudioBGMId);

        //�v���C���[��Ǐ]����BGM��BGM�X�e�[�g��Play�ȊO�̏ꍇ
        if (sO_BGM.CheckBGMState(chasePlayerBGMId) != BGMState.Play)
        {
            //�v���C���[��Ǐ]����BGM�𗬂�
            PlayChasePlayerBGM();
        }
    }

    /// <summary>
    /// �v���C���[��Ǐ]����BGM����X�e�[�WBGM�֐؂�ւ���
    /// </summary>
    public void ChangeBGMFromChasePlayerBGMToStageBGM()
    {
        //�v���C���[��Ǐ]����BGM���~
        MusicController.instance.StopBGM(audioSourceBGM, sO_BGM.GetBGMClip(chasePlayerBGMId), chasePlayerBGMId);

        //�X�e�[�WBGM���ꎞ��~����
        MusicController.instance.UnPauseBGM(keepAudioSourceBGM, keepAudioClipBGM, keepAudioBGMId);

        //���ݍĐ�����BGM��ݒ肷��
        PauseController.instance.SetNowPlayBGMId(keepAudioBGMId);
    }
}
