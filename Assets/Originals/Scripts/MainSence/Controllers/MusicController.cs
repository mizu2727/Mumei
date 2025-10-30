using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static MusicController instance;

    /// <summary>
    /// StageBGM�pAudioSource
    /// </summary>
    private AudioSource stageBGMAudioSource;

    /// <summary>
    /// chasePlayerBGM�pAudioSource
    /// </summary>
    private AudioSource chasePlayerBGMAudioSource;

    [Header("AudioMixer")]
    public AudioMixer audioMixer;

    [Header("AudioMixerGroupBGM")]
    public AudioMixerGroup audioMixerGroupBGM;

    [Header("AudioMixerGroupSE")]
    public AudioMixerGroup audioMixerGroupSE;

    [Header("BGMSlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider bGMSlider;

    /// <summary>
    /// BGM�ŏ�����(Slider�p)
    /// </summary>
    private const float minBGMSliderVolume = 0f;

    /// <summary>
    /// SE�ő剹��(Slider�p)
    /// </summary>
    private const float maxBGMSliderVolume = 1f;

    /// <summary>
    /// OnBGMVolumeChangedEvent
    /// </summary>
    public static event Action<float> OnBGMVolumeChangedEvent;

    /// <summary>
    /// BGM�ŏ�����(dB)
    /// </summary>
    private const float minBGMVolume = -80f;

    /// <summary>
    /// BGM�ő剹��(dB)
    /// </summary>
    private const float maxBGMVolume = 0f;

    [Header("SESlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider sESlider;

    /// <summary>
    /// SE�ŏ�����(Slider�p)
    /// </summary>
    private const float minSESliderVolume = 0f;

    /// <summary>
    /// SE�ő剹��(Slider�p)
    /// </summary>
    private const float maxSESliderVolume = 1f;

    /// <summary>
    /// OnSEVolumeChangedEvent
    /// </summary>
    public static event Action<float> OnSEVolumeChangedEvent;

    /// <summary>
    /// SE�ŏ�����(dB)
    /// </summary>
    private const float minSEVolume = -80f;

    /// <summary>
    /// SE�ő剹��(dB)
    /// </summary>
    private const float maxSEVolume = 0f;


    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;


    /// <summary>
    /// SE�paudioSource�̃��X�g
    /// </summary>
    private readonly List<AudioSource> audioSourceSEList = new ();

    [Header("�f�o�b�O�t���O")]
    [SerializeField] private bool isDebug;


    /// <summary>
    /// �X�e�[�WBGM��p��AudioSource���擾���郁�\�b�h
    /// </summary>
    /// <returns>�X�e�[�WBGM�pAudioSource</returns>
    public AudioSource GetStageBGMAudioSource()
    {
        return stageBGMAudioSource;
    }

    /// <summary>
    /// �ǐ�BGM��p��AudioSource���擾���郁�\�b�h
    /// </summary>
    /// <returns>�ǐ�BGM�pAudioSource</returns>
    public AudioSource GetChasePlayerBGMAudioSource()
    {
        return chasePlayerBGMAudioSource;
    }

    /// <summary>
    /// BGM�ŏ����ʎ擾(Slider�p)
    /// </summary>
    /// <returns>BGM�ŏ�����(Slider�p)</returns>
    public float GetMinBGMSliderVolume()
    {
        return minBGMSliderVolume;
    }

    /// <summary>
    /// BGM�ő剹�ʎ擾(Slider�p)
    /// </summary>
    /// <returns>BGM�ő剹��(Slider�p)</returns>
    public float GetMaxBGMSliderVolume()
    {
        return maxBGMSliderVolume;
    }

    /// <summary>
    /// SE�ŏ����ʎ擾(Slider�p)
    /// </summary>
    /// <returns>SE�ŏ�����(Slider�p)</returns>
    public float GetMinSESliderVolume()
    {
        return minSESliderVolume;
    }

    /// <summary>
    /// SE�ő剹�ʎ擾(Slider�p)
    /// </summary>
    /// <returns>SE�ő剹��(Slider�p)</returns>
    public float GetMaxSESliderVolume()
    {
        return maxSESliderVolume;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

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

        InitializeAudioSource();

        //BGM���~�߂�
        sO_BGM.StopAllBGM();


        if (bGMSlider != null)
        {
            //BGM�̃{�����[����bgmSlider�Œ����ł���悤�ɂ���
            bGMSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        else 
        {
            Debug.LogWarning("bGMSlider ���A�^�b�`����Ă��܂���I");
        }

        if (sESlider != null)
        {
            //SE�̃{�����[����seSlider�Œ����ł���悤�ɂ���
            sESlider.onValueChanged.AddListener(OnSEVolumeChanged);
        }
        else
        {
            Debug.LogWarning("sESlider ���A�^�b�`����Ă��܂���I");
        }
    }

    private void InitializeAudioSource() 
    {
        if (stageBGMAudioSource == null)
        {
            stageBGMAudioSource = gameObject.AddComponent<AudioSource>();
            stageBGMAudioSource.outputAudioMixerGroup = audioMixerGroupBGM;
            stageBGMAudioSource.playOnAwake = false;
            stageBGMAudioSource.loop = true; // �K�v�ɉ�����
        }

        if (chasePlayerBGMAudioSource == null)
        {
            chasePlayerBGMAudioSource = gameObject.AddComponent<AudioSource>();
            chasePlayerBGMAudioSource.outputAudioMixerGroup = audioMixerGroupBGM;
            chasePlayerBGMAudioSource.playOnAwake = false;
            chasePlayerBGMAudioSource.loop = true;
        }
    }

    /// <summary>
    /// �X���C�_�[�̒l��dB�ɕϊ����āAAudioMixer�́uBGM�v�p�����[�^�ɔ��f������
    /// </summary>
    /// <param name="value">�X���C�_�[�̒l</param>
    private void OnBGMVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, minBGMVolume, maxBGMVolume);

        //"BGMVolumeParam"��AudioMixer�Œ�`�����p�����[�^���ƈ�v���Ă���K�v������
        audioMixer.SetFloat("BGMVolumeParam", decibel);

        //�l��0 ~ 1�֕ϊ�
        OnBGMVolumeChangedEvent?.Invoke(value);
    }

    /// <summary>
    /// �X���C�_�[�̒l��dB�ɕϊ����āAAudioMixer�́uSE�v�p�����[�^�ɔ��f������
    /// </summary>
    /// <param name="value">�X���C�_�[�̒l</param>
    private void OnSEVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);
        float decibel = 20f * Mathf.Log10(value);
        decibel = Mathf.Clamp(decibel, minSEVolume, maxSEVolume);

        //"SEVolumeParam"��AudioMixer�Œ�`�����p�����[�^���ƈ�v���Ă���K�v������
        audioMixer.SetFloat("SEVolumeParam", decibel);

        //�l��0 ~ 1�֕ϊ�
        OnSEVolumeChangedEvent?.Invoke(value);
    }

    void Start()
    {
        //�f�o�b�O���[�h�̏ꍇ�������X�L�b�v
        if (isDebug) return;
    }

    /// <summary>
    /// BGM�����[�v����ōĐ�
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayLoopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //�N���b�v��ݒ�
                audioSource.clip = audioClip;

                //���[�v�Đ���L��
                audioSource.loop = true;

                //�Đ�
                audioSource.Play();

                //BGM�̏�Ԃ�Play�ɕύX
                sO_BGM.ChangeFromStopToPlayBGM(bgmId);
            }
        }
    }

    /// <summary>
    /// BGM�����[�v�Ȃ��ōĐ�
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="audioClip"></param>
    public void PlayNoLoopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                //�N���b�v��ݒ�
                audioSource.clip = audioClip;

                //���[�v�Đ��𖳌���
                audioSource.loop = false;

                //�Đ�
                audioSource.Play();

                //BGM�̏�Ԃ�Play�ɕύX
                sO_BGM.ChangeFromStopToPlayBGM(bgmId);
            }
        }
    }

    /// <summary>
    /// BGM�ꎞ��~
    /// </summary>
    public void PauseBGM(AudioSource audioSource, AudioClip audioClip, int bgmId) 
    {
        //�N���b�v��ݒ�
        audioSource.clip = audioClip;

        //���f
        audioSource.Pause();

        //BGM�̏�Ԃ�Pause�ɕύX
        sO_BGM.ChangeFromPlayToPauseBGM(bgmId);
    }

    /// <summary>
    /// BGM�ꎞ��~����
    /// </summary>
    public void UnPauseBGM(AudioSource audioSource, AudioClip audioClip, int bgmId)
    {
        //�N���b�v��ݒ�
        audioSource.clip = audioClip;

        //���f����
        audioSource.UnPause();

        //BGM�̏�Ԃ�Play�ɖ߂�
        sO_BGM.ChangeFromPauseToPlayBGM(bgmId);
    }

    /// <summary>
    /// BGM��~
    /// </summary>
    public void StopBGM(AudioSource audioSource, AudioClip audioClip, int bgmId) 
    {
        //�N���b�v��ݒ�
        audioSource.clip = audioClip;

        //���f����
        audioSource.Stop();

        //BGM�̏�Ԃ�Stop�ɕύX
        sO_BGM.ChangeFromPlayToStopBGM(bgmId);
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