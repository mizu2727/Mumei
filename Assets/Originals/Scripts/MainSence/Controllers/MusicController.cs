using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;
    public static MusicController Instance => _instance;

    [Header("BGM")]
    [SerializeField] private AudioSource audioSourceBGM;

    [Header("BGM�N���b�v")]
    [SerializeField] private AudioClip[] audioClipBGM;

    public int audioClipnum = 0;

    private int keepAudioClipnum = 999;

    //[Header("�G�ɒǂ��Ă��鎞��BGM")]
    //[SerializeField] private AudioSource audioSourceEnemyBGM;

    //[SerializeField] private AudioClip audioClipEnemyBGM;

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
        //if (audioSourceBGM == null)
        //{
        //    audioSourceBGM = gameObject.AddComponent<AudioSource>();
        //}
        //audioClipBGM = GetComponent<AudioClip>();


        //audioSourceEnemyBGM = GetComponent<AudioSource>();
        //if (audioSourceEnemyBGM == null)
        //{
        //    audioSourceEnemyBGM = gameObject.AddComponent<AudioSource>();
        //}

        //audioClipEnemyBGM = GetComponent<AudioClip>();
    }

    void Start()
    {
        if (isDebug) return;

        audioClipnum = 0;
        PlayBGM();
    }

    // BGM�Đ�
    public void PlayBGM()
    {


        if (!isDebug)
        {
            //�ԍ�������BGM��A���ōĐ�����̂�j�~����
            if (keepAudioClipnum != audioClipnum)
            {
                keepAudioClipnum = audioClipnum;
                audioSourceBGM.clip = audioClipBGM[audioClipnum];
                audioSourceBGM.loop = true;
                audioSourceBGM.Play();
                Debug.Log("keepAudioClipnum = " + keepAudioClipnum);
            }
            else 
            {
                Debug.LogWarning("�ԍ�������BGM��A���ōĐ����悤�Ƃ��Ă��܂�");
            }
        }
    }

    // BGM�ꎞ��~
    public void PauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.Pause();
    }

    // BGM�ꎞ��~����
    public void UnPauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.UnPause();
    }

    // BGM��~
    public void StopBGM()
    {
        keepAudioClipnum = 999;
        audioSourceBGM.Stop();
    }

    //// �G�ɒǂ��Ă��鎞��BGM���Đ�
    //public void PlayEnemyBGM()
    //{
    //    if (audioSourceEnemyBGM != null && !isDebug)
    //    {
    //        if (audioClipEnemyBGM != null)
    //        {
    //            audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //            audioSourceEnemyBGM.loop = true;
    //            audioSourceEnemyBGM.Play();
    //        }
    //        else
    //        {
    //            Debug.LogWarning("[MusicController] audioClipEnemyBGM �� null �ł��I");
    //        }
    //    }
    //}

    //// �G�ɒǂ��Ă��鎞��BGM�ꎞ��~
    //public void PauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.Pause();
    //}

    //// �G�ɒǂ��Ă��鎞��BGM�ꎞ��~����
    //public void UnPauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.UnPause();
    //}

    //// �G�ɒǂ��Ă��鎞��BGM��~
    //public void StopEnemyBGM()
    //{
    //    audioSourceEnemyBGM.Stop();
    //}


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