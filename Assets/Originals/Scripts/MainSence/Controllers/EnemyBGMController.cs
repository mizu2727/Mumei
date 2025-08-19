using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBGMController : MonoBehaviour
{
    private static EnemyBGMController _instance;
    public static EnemyBGMController Instance => _instance;

    [Header("�G�ɒǂ��Ă��鎞��BGM")]
    [SerializeField] private AudioSource audioSourceEnemyBGM;

    [SerializeField] private AudioClip audioClipEnemyBGM;

    private readonly List<AudioSource> audioSourceSEList = new();


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

        audioSourceEnemyBGM = GetComponent<AudioSource>();
        if (audioSourceEnemyBGM == null)
        {
            audioSourceEnemyBGM = gameObject.AddComponent<AudioSource>();
        }

        audioClipEnemyBGM = GetComponent<AudioClip>();
    }

    // �G�ɒǂ��Ă��鎞��BGM���Đ�
    public void PlayEnemyBGM()
    {
        audioSourceEnemyBGM.Play();
        if (audioSourceEnemyBGM != null && !isDebug)
        {
            if (audioClipEnemyBGM != null)
            {
                Debug.Log("EnemyBGMController:�G�ɒǂ��Ă��鎞��BGM�Đ�");
                audioSourceEnemyBGM.clip = audioClipEnemyBGM;
                audioSourceEnemyBGM.loop = true;
                audioSourceEnemyBGM.Play();
            }
            else
            {
                Debug.LogWarning("[EnemyBGMController] audioClipEnemyBGM �� null �ł��I");
            }
        }
    }

    // �G�ɒǂ��Ă��鎞��BGM�ꎞ��~
    public void PauseEnemyBGM()
    {
        Debug.Log("EnemyBGMController:�G�ɒǂ��Ă��鎞��BGM�ꎞ��~");
        audioSourceEnemyBGM.clip = audioClipEnemyBGM;
        audioSourceEnemyBGM.Pause();
    }

    // �G�ɒǂ��Ă��鎞��BGM�ꎞ��~����
    public void UnPauseEnemyBGM()
    {
        Debug.Log("EnemyBGMController:�G�ɒǂ��Ă��鎞��BGM�ꎞ��~����");
        audioSourceEnemyBGM.clip = audioClipEnemyBGM;
        audioSourceEnemyBGM.UnPause();
    }

    // �G�ɒǂ��Ă��鎞��BGM��~
    public void StopEnemyBGM()
    {
        Debug.Log("EnemyBGMController:�G�ɒǂ��Ă��鎞��BGM��~");
        audioSourceEnemyBGM.Stop();
    }


    // �V����AudioSource���擾�܂��͍쐬
    public AudioSource GetAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceSEList.Add(audioSource);
        return audioSource;
    }
}
