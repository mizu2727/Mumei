using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    //�C���X�^���X
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [Header("BGM")]
    [SerializeField] private AudioSource audioSourceBGM;

    [Header("BGM�N���b�v")]
    [SerializeField] private AudioClip[] audioClipBGM;


    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioClip�ԍ�
    /// </summary>
    public int audioClipnum = 0;

    /// <summary>
    /// audioClip�ԍ�(�ۑ��p)
    /// </summary>
    private int keepAudioClipnum = 999;


    /// <summary>
    /// SE�paudioSource�̃��X�g
    /// </summary>
    private readonly List<AudioSource> audioSourceSEList = new ();

    [Header("�f�o�b�O�t���O")]
    [SerializeField] private bool isDebug;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        audioSourceBGM = GetComponent<AudioSource>();


        if (audioSourceBGM == null)
        {
            Debug.LogWarning("AudioSourceBGM not found, adding one.");
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        }

        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // �V�[���J�ڎ��ɖ�����AudioSource���N���[���A�b�v
        audioSourceSEList.RemoveAll(audioSource => audioSource == null || !audioSource.gameObject.activeInHierarchy);

        //BGM���~�߂�
        sO_BGM.StopAllBGM();
    }

    void Start()
    {
        //�f�o�b�O���[�h�̏ꍇ�������X�L�b�v
        if (isDebug) return;

        audioClipnum = 0;
        PlayBGM();
    }

    /// <summary>
    /// BGM�Đ�
    /// </summary>
    public void PlayBGM()
    {
        if (!isDebug)
        {
            keepAudioClipnum = audioClipnum;
            audioSourceBGM.clip = audioClipBGM[audioClipnum];
            audioSourceBGM.loop = true;
            audioSourceBGM.Play();
        }
    }

    /// <summary>
    /// BGM�ꎞ��~
    /// </summary>
    public void PauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.Pause();
    }

    /// <summary>
    /// BGM�ꎞ��~����
    /// </summary>
    public void UnPauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.UnPause();
    }

    /// <summary>
    /// BGM��~
    /// </summary>
    public void StopBGM()
    {
        keepAudioClipnum = 999;
        audioSourceBGM.Stop();
    }

    /// <summary>
    ///  �V����AudioSource���擾�܂��͍쐬
    /// </summary>
    /// <returns></returns>
    public AudioSource GetAudioSource()
    {
        //�g�p���Ă��Ȃ��L����AudioSource��T��
        foreach (var audioSource in audioSourceSEList)
        {
            if (audioSource != null && audioSource.gameObject.activeInHierarchy && !audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        //�V����AudioSource���쐬
        var audioSourceNew = gameObject.AddComponent<AudioSource>();
        audioSourceNew.playOnAwake = false;
        audioSourceSEList.Add(audioSourceNew);
        return audioSourceNew;
    }

    /// <summary>
    /// SE�Đ�
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //���ʉ��N���b�v��ݒ�
                audioSource.clip = audioClip;

                //���[�v�Đ��𖳌���
                audioSource.loop = false;

                //���ʉ����Đ�
                audioSource.Play(); 
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip �� null �ł��I");
            }
        }
    }

    /// <summary>
    /// SE����u�����Đ�
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public async void PlayMomentAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("[PlayMomentAudioSE] audioClip �� null �ł��I");
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

        StopSE(audioSource);
    }

    /// <summary>
    /// SE�����[�v�Đ�
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void LoopPlayAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("[MusicController] loop�p��audioClip �� null �ł��I");
            }
        }
    }

    /// <summary>
    /// SE���ꎞ��~
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("PauseSE�caudioSource :" + audioSource);
            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("PauseSE�caudioClip :" + audioClip);
            return;
        }
        audioSource.clip = audioClip;
        audioSource.Pause();
    }

    /// <summary>
    /// SE�̈ꎞ��~����
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void UnPauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("UnPauseSE�caudioSource :" + audioSource);
            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("UnPauseSE�caudioClip :" + audioClip);
            return;
        }
        audioSource.clip = audioClip;
        audioSource.UnPause();
    }

    /// <summary>
    /// SE��~
    /// </summary>
    /// <param name="audioSource"></param>
    public void StopSE(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// �w�肳�ꂽAudioSource���Đ������m�F
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    public bool IsPlayingSE(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    /// <summary>
    /// ���݂�SE�N���b�v���擾
    /// </summary>
    /// <param name="audioSource"></param>
    /// <returns></returns>
    public AudioClip GetCurrentSE(AudioSource audioSource)
    {
        return audioSource != null ? audioSource.clip : null;
    }
}