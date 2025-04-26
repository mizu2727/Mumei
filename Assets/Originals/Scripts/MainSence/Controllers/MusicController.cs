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
                // 新しいゲームオブジェクトを生成
                GameObject musicControllerObj = new GameObject("MusicController");
                _instance = musicControllerObj.AddComponent<MusicController>();
                DontDestroyOnLoad(musicControllerObj);
                Debug.Log("[MusicController] 遅延初期化で MusicController を生成しました。");
            }
            return _instance;
        }
    }

    [SerializeField] private AudioSource audioSourceBGM;//BGM
    private AudioSource audioSourceSE = null;//AudioSourece
    [SerializeField] private bool isDebug;//デバッグモード

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
            if (audioClip != null)
            {
                audioSourceSE.PlayOneShot(audioClip);
            }
            else
            {
                Debug.LogWarning("[MusicController] audioClip が null です！");
            }
        }
        else
        {
            Debug.LogWarning($"[MusicController] 効果音再生失敗: audioSourceSE={(audioSourceSE != null ? "設定済み" : "null")}, isDebug={isDebug}, audioClip={(audioClip != null ? audioClip.name : "null")}");
        }
    }
}
