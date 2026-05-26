using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 移動する壁のクラス
/// </summary>
public class MoveWall : MonoBehaviour
{
    [Header("goal(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Goal goal;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// 現在再生中であるかを示すフラグ
    /// </summary>
    private bool isPlayingMoveWallSE = false;

    /// <summary>
    /// audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// SEの音量倍率
    /// </summary>
    private const float magnificationSeVolume = 0.5f;

    /// <summary>
    /// 壁が移動するSEのID
    /// </summary>
    private readonly int moveWallSEid = 20;

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時にAudioSourceを再設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //現在再生中フラグをオフ
        isPlayingMoveWallSE = false;
    }

    
    void Update()
    {
        //ゴール壁移動中フラグがオンの場合
        if (goal.GetIsMovingGoalWall() && !isPlayingMoveWallSE)
        {
            //壁が移動するSEを再生
            MoveWallSE();
        }
        else 
        {
            //壁が移動するSEを停止
            audioSourceSE.Stop();

            //現在再生中フラグをオフ
            isPlayingMoveWallSE = false;
        }
    }

    /// <summary>
    /// 壁が移動する効果音
    /// </summary>
    void MoveWallSE()
    {
        //SEの音量に倍率をかけて設定
        audioSourceSE.volume *= magnificationSeVolume;

        //ループ再生を有効にする
        audioSourceSE.loop = true;

        //壁が移動するSEを再生
        audioSourceSE.PlayOneShot(sO_SE.GetSEClip(moveWallSEid));

        //現在再生中フラグをオン
        isPlayingMoveWallSE = true;

        //なぜか連続でログが出現してしまう
        Debug.Log("壁が移動するSEを再生");
    }
}
