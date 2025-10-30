using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage01Controller : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static Stage01Controller instance;

    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// Stage01BGM��ID
    /// </summary>
    private readonly int stage01BGMId = 2;



    /// <summary>
    /// AudioSourceBGM���擾����
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// Stage01BGM��ID���擾����
    /// </summary>
    /// <returns>Stage01BGM��ID</returns>
    public int GetStage01BGMId()
    {
        return stage01BGMId;
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
        //NullReferenceException��h���p
        if (audioSourceBGM == null)
        {
            Debug.LogWarning("Stage01Controller: audioSourceBGM �����ݒ�̂��߉��ʕύX���X�L�b�v���܂����B");
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
        audioSourceBGM = MusicController.instance.GetStageBGMAudioSource();

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

    void Start()
    {
        //AudioSource�̏�����
        InitializeAudioSource();

        //Stage01BGM�𗬂��B���ݍĐ�����BGM��ݒ肷��B
        PlayStage01BGM();
    }

    /// <summary>
    /// Stage01BGM�𗬂��B���ݍĐ�����BGM��ݒ肷��B
    /// </summary>
    public void PlayStage01BGM() 
    {
        //Stage01BGM���Đ�
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(stage01BGMId), stage01BGMId);

        //���ݍĐ�����BGM��Stage01BGM�ɐݒ肷��
        PauseController.instance.SetNowPlayBGMId(stage01BGMId);
    }
}
