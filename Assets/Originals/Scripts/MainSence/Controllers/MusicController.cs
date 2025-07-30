using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [SerializeField] private AudioSource audioSourceBGM;
    private readonly List<AudioSource> audioSourceSEList = new ();
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
            audioSourceBGM = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (isDebug) return;
        PlayBGM();
    }

    // BGM�Đ�
    public void PlayBGM()
    {
        audioSourceBGM.Play();
    }

    // BGM�ꎞ��~
    public void PauseBGM()
    {
        audioSourceBGM.Pause();
    }

    // BGM�ꎞ��~����
    public void UnPauseBGM()
    {
        audioSourceBGM.UnPause();
    }

    // BGM��~
    public void StopBGM()
    {
        audioSourceBGM.Stop();
    }

    // �V����AudioSource���擾�܂��͍쐬
    public AudioSource GetAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceSEList.Add(audioSource);
        return audioSource;
    }

    // SE�Đ�
    public void PlayAudioSE(AudioSource audioSource, AudioClip audioClip)
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
                Debug.LogWarning("[MusicController] audioClip �� null �ł��I");
            }
        }
    }

    // SE����u�̂ݍĐ�
    public async void PlayMomentAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
                Debug.Log("SE����u�̂ݍĐ��X�^�[�g");
            }
            else
            {
                Debug.LogWarning("[PlayMomentAudioSE] audioClip �� null �ł��I");
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

        StopSE(audioSource);
        Debug.Log("SE����u�̂ݍĐ���~");
    }

    // SE�����[�v�Đ�
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

    // SE���ꎞ��~
    public void PauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Pause();
        Debug.Log("SE�ꎞ��~");
    }

    // SE�̈ꎞ��~����
    public void UnPauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.UnPause();
        Debug.Log("SE�ꎞ��~����");
    }

    // SE��~
    public void StopSE(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    // �w�肳�ꂽAudioSource���Đ������m�F
    public bool IsPlayingSE(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // ���݂�SE�N���b�v���擾
    public AudioClip GetCurrentSE(AudioSource audioSource)
    {
        return audioSource != null ? audioSource.clip : null;
    }
}