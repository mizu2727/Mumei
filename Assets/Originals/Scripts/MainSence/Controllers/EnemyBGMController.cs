using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBGMController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static EnemyBGMController instance;

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stage01 = "Stage01";

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// プレイヤーが敵に追われる際のBGMのID
    /// </summary>
    private readonly int chasePlayerBGMId = 4;

    /// <summary>
    /// 保存用AudioSourceBGM
    /// </summary>
    private AudioSource keepAudioSourceBGM;

    /// <summary>
    /// 保存用AudioClipBGM
    /// </summary>
    private AudioClip keepAudioClipBGM;

    /// <summary>
    /// 保存用AudioBGMID
    /// </summary>
    private int keepAudioBGMId = 999;

    /// <summary>
    /// AudioSourceBGMを取得する
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// プレイヤーが敵に追われる際のBGMのIDを取得
    /// </summary>
    /// <returns>プレイヤーが敵に追われる際のBGMのID</returns>
    public int GetChasePlayerBGMId()
    {
        return chasePlayerBGMId;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM音量変更時のイベント登録
        //MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //BGM音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateBGMVolume(float volume)
    {
        //NullReferenceExceptionを防ぐ用
        if (audioSourceBGM == null)
        {
            Debug.LogWarning("EnemyBGMController: audioSourceBGM が未設定のため音量変更をスキップしました。");
            return;
        }

        audioSourceBGM.volume = volume;
    }

    /// <summary>
    /// シーン遷移時に処理を呼び出す関数
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGMを設定
        audioSourceBGM = MusicController.instance.GetChasePlayerBGMAudioSource();

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;

        //BGM音量変更時のイベント登録
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// プレイヤーを追従するBGMを流す。現在再生中BGMを設定する。
    /// </summary>
    public void PlayChasePlayerBGM() 
    {
        //BGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(chasePlayerBGMId), chasePlayerBGMId);

        //現在再生中のBGMをChasePlayerBGMに設定する
        PauseController.instance.SetNowPlayBGMId(chasePlayerBGMId);
    }

    /// <summary>
    /// ステージBGMからプレイヤーを追従するBGMへ切り替える
    /// </summary>
    public void ChangeBGMFromStageBGMToChasePlayerBGM() 
    {
        //現在のシーン名を取得し、その名前によって一時停止するステージBGMを決める
        switch (SceneManager.GetActiveScene().name)
        {
            //Stage01
            case stage01:

                //一時停止するBGMの情報を保存する
                keepAudioSourceBGM = Stage01Controller.instance.GetAudioSourceBGM();
                keepAudioClipBGM = sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId());
                keepAudioBGMId = Stage01Controller.instance.GetStage01BGMId();
                break;

            default:
                Debug.LogWarning("その他のシーン名");
                break;
        }

        //ステージBGMを一時停止
        MusicController.instance.PauseBGM(keepAudioSourceBGM,keepAudioClipBGM, keepAudioBGMId);

        //プレイヤーを追従するBGMのBGMステートがPlay以外の場合
        if (sO_BGM.CheckBGMState(chasePlayerBGMId) != BGMState.Play)
        {
            //プレイヤーを追従するBGMを流す
            PlayChasePlayerBGM();
        }
    }

    /// <summary>
    /// プレイヤーを追従するBGMからステージBGMへ切り替える
    /// </summary>
    public void ChangeBGMFromChasePlayerBGMToStageBGM()
    {
        //プレイヤーを追従するBGMを停止
        MusicController.instance.StopBGM(audioSourceBGM, sO_BGM.GetBGMClip(chasePlayerBGMId), chasePlayerBGMId);

        //ステージBGMを一時停止解除
        MusicController.instance.UnPauseBGM(keepAudioSourceBGM, keepAudioClipBGM, keepAudioBGMId);

        //現在再生中のBGMを設定する
        PauseController.instance.SetNowPlayBGMId(keepAudioBGMId);
    }
}
