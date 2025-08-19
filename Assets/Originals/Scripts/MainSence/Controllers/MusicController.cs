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

    [Header("BGMƒNƒŠƒbƒv")]
    [SerializeField] private AudioClip[] audioClipBGM;

    public int audioClipnum = 0;

    private int keepAudioClipnum = 999;

    //[Header("“G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM")]
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

    // BGMÄ¶
    public void PlayBGM()
    {


        if (!isDebug)
        {
            //”Ô†‚ª“¯‚¶BGM‚ğ˜A‘±‚ÅÄ¶‚·‚é‚Ì‚ğ‘j~‚·‚é
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
                Debug.LogWarning("”Ô†‚ª“¯‚¶BGM‚ğ˜A‘±‚ÅÄ¶‚µ‚æ‚¤‚Æ‚µ‚Ä‚¢‚Ü‚·");
            }
        }
    }

    // BGMˆê’â~
    public void PauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.Pause();
    }

    // BGMˆê’â~‰ğœ
    public void UnPauseBGM()
    {
        audioSourceBGM.clip = audioClipBGM[audioClipnum];
        audioSourceBGM.UnPause();
    }

    // BGM’â~
    public void StopBGM()
    {
        keepAudioClipnum = 999;
        audioSourceBGM.Stop();
    }

    //// “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM‚ğÄ¶
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
    //            Debug.LogWarning("[MusicController] audioClipEnemyBGM ‚ª null ‚Å‚·I");
    //        }
    //    }
    //}

    //// “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~
    //public void PauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.Pause();
    //}

    //// “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGMˆê’â~‰ğœ
    //public void UnPauseEnemyBGM()
    //{
    //    audioSourceEnemyBGM.clip = audioClipEnemyBGM;
    //    audioSourceEnemyBGM.UnPause();
    //}

    //// “G‚É’Ç‚í‚ê‚Ä‚¢‚é‚ÌBGM’â~
    //public void StopEnemyBGM()
    //{
    //    audioSourceEnemyBGM.Stop();
    //}


    // V‚µ‚¢AudioSource‚ğæ“¾‚Ü‚½‚Íì¬
    public AudioSource GetAudioSource()
    {
        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSourceSEList.Add(audioSource);
        return audioSource;
    }

    // SEÄ¶
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
                Debug.LogWarning("[MusicController] audioClip ‚ª null ‚Å‚·I");
            }
        }
    }

    // SE‚ğˆêu‚Ì‚İÄ¶
    public async void PlayMomentAudioSE(AudioSource audioSource, AudioClip audioClip)
    {
        if (audioSource != null && !isDebug)
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.loop = false;
                audioSource.Play();
                Debug.Log("SE‚ğˆêu‚Ì‚İÄ¶ƒXƒ^[ƒg");
            }
            else
            {
                Debug.LogWarning("[PlayMomentAudioSE] audioClip ‚ª null ‚Å‚·I");
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

        StopSE(audioSource);
        Debug.Log("SE‚ğˆêu‚Ì‚İÄ¶’â~");
    }

    // SE‚ğƒ‹[ƒvÄ¶
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
                Debug.LogWarning("[MusicController] loop—p‚ÌaudioClip ‚ª null ‚Å‚·I");
            }
        }
    }

    // SE‚ğˆê’â~
    public void PauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Pause();
        Debug.Log("SEˆê’â~");
    }

    // SE‚Ìˆê’â~‰ğœ
    public void UnPauseSE(AudioSource audioSource, AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.UnPause();
        Debug.Log("SEˆê’â~‰ğœ");
    }

    // SE’â~
    public void StopSE(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }

    // w’è‚³‚ê‚½AudioSource‚ªÄ¶’†‚©Šm”F
    public bool IsPlayingSE(AudioSource audioSource)
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // Œ»İ‚ÌSEƒNƒŠƒbƒv‚ğæ“¾
    public AudioClip GetCurrentSE(AudioSource audioSource)
    {
        return audioSource != null ? audioSource.clip : null;
    }
}