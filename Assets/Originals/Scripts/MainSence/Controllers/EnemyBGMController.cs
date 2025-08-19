using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBGMController : MonoBehaviour
{
    private static EnemyBGMController _instance;
    public static EnemyBGMController Instance => _instance;

    [Header("“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM")]
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

    // “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM‚ğÄ¶
    public void PlayEnemyBGM()
    {
        audioSourceEnemyBGM.Play();
        if (audioSourceEnemyBGM != null && !isDebug)
        {
            if (audioClipEnemyBGM != null)
            {
                Debug.Log("EnemyBGMController:“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMÄ¶");
                audioSourceEnemyBGM.clip = audioClipEnemyBGM;
                audioSourceEnemyBGM.loop = true;
                audioSourceEnemyBGM.Play();
            }
            else
            {
                Debug.LogWarning("[EnemyBGMController] audioClipEnemyBGM ‚ª null ‚Å‚·I");
            }
        }
    }

    // “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~
    public void PauseEnemyBGM()
    {
        Debug.Log("EnemyBGMController:“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~");
        audioSourceEnemyBGM.clip = audioClipEnemyBGM;
        audioSourceEnemyBGM.Pause();
    }

    // “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~‰ğœ
    public void UnPauseEnemyBGM()
    {
        Debug.Log("EnemyBGMController:“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~‰ğœ");
        audioSourceEnemyBGM.clip = audioClipEnemyBGM;
        audioSourceEnemyBGM.UnPause();
    }

    // “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM’â~
    public void StopEnemyBGM()
    {
        Debug.Log("EnemyBGMController:“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM’â~");
        audioSourceEnemyBGM.Stop();
    }


    // V‚µ‚¢AudioSource‚ğæ“¾‚Ü‚½‚Íì¬
    public AudioSource GetAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceSEList.Add(audioSource);
        return audioSource;
    }
}
