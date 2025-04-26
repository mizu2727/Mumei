using UnityEngine;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;
    public static MusicController Instance
    {
        get
        {
            if (_instance == null)
            {
                // �V�����Q�[���I�u�W�F�N�g�𐶐�
                GameObject musicControllerObj = new GameObject("MusicController");
                _instance = musicControllerObj.AddComponent<MusicController>();
                DontDestroyOnLoad(musicControllerObj);
                Debug.Log("[MusicController] �x���������� MusicController �𐶐����܂����B");
            }
            return _instance;
        }
    }

    [SerializeField] private AudioSource audioSourceBGM;//BGM
    private AudioSource audioSourceSE = null;//AudioSourece
    [SerializeField] private bool isDebug;//�f�o�b�O���[�h

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

        audioSourceSE = gameObject.AddComponent<AudioSource>();
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
            if (audioClip != null)
            {
                audioSourceSE.PlayOneShot(audioClip);
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip �� null �ł��I");
            }
        }
        else
        {
            Debug.LogWarning($"[MusicController] ���ʉ��Đ����s: audioSourceSE={(audioSourceSE != null ? "�ݒ�ς�" : "null")}, isDebug={isDebug}, audioClip={(audioClip != null ? audioClip.name : "null")}");
        }
    }
}
