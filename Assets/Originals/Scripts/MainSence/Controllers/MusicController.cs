using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    public static AudioSource instance;//�C���X�^���X��

    [SerializeField] private AudioSource audioSourceBGM;//BGM
    private AudioSource audioSourceSE = null;//AudioSourece
    [SerializeField] private bool isDebug;//�f�o�b�O���[�h

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

    //BGM��~
    public void StopBGM()
    {
        audioSourceBGM.Stop();
    }

    //SE��炷
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
