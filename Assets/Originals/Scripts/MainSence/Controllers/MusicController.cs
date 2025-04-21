using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    public static AudioSource instance;//インスタンス化

    [SerializeField] private AudioSource audioSourceBGM;//BGM
    private AudioSource audioSourceSE = null;//AudioSourece
    [SerializeField] private bool isDebug;//デバッグモード

    void Start()
    {
        if (isDebug) return;
        PlayBGM();
    }

    // BGM再生
    public void PlayBGM()
    {
        audioSourceBGM.Play();
    }

    // BGM一時停止
    public void PauseBGM()
    {
        audioSourceBGM.Pause();
    }

    // BGM一時停止解除
    public void UnPauseBGM()
    {
        audioSourceBGM.UnPause();
    }

    //BGM停止
    public void StopBGM()
    {
        audioSourceBGM.Stop();
    }

    //SEを鳴らす
    public void PlayAudioSE(AudioClip audioClip)
    {
        if (audioSourceSE != null && !isDebug)
        {
            audioSourceSE.PlayOneShot(audioClip);
        }
        else
        {
            Debug.Log("No Setting audioSourceSE!");
        }
    }
}
