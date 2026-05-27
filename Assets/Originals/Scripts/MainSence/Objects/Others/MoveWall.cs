using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// 移動する壁のクラス
/// </summary>
public class MoveWall : MonoBehaviour
{
    [Header("goal(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Goal goal;

    /// <summary>
    /// プレイヤーとの距離
    /// </summary>
    private float distanceToPlayer;

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
    /// 音量が最大になる距離
    /// </summary>
    private float maxSoundDistance = 10f;

    /// <summary>
    /// 音量が最小になる距離
    /// </summary>
    private float minSoundDistance = 50f;

    /// <summary>
    /// 最大音量
    /// </summary>
    private const float maxVolume = 1.0f;

    /// <summary>
    /// 最小音量
    /// </summary>
    private const float minVolume = 0.0f;

    /// <summary>
    /// マスター音量
    /// </summary>
    private float masterSEVolume = 1.0f;

    /// <summary>
    /// 壁が移動するSEのID
    /// </summary>
    private readonly int moveWallSEid = 20;

    private void OnEnable()
    {
        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
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

        //マスター音量を同期
        masterSEVolume = MusicController.instance.sESlider.value;
    }

    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //現在再生中フラグをオフ
        isPlayingMoveWallSE = false;

        audioSourceSE.playOnAwake = false;
    }

    
    void Update()
    {
        //壁が移動している場合
        if (goal.GetIsMovingGoalWall())
        {
            //再生中フラグがオフの場合
            if (!isPlayingMoveWallSE)
            {
                //まだ再生していなければ、再生を開始する
                StartMoveWallSE();
            }

            //再生中は、毎フレームの距離に応じて音量だけを更新する
            UpdateVolumeBasedOnDistance();
        }
        //壁が移動していない場合
        else
        {
            //再生中フラグがオンの場合
            if (isPlayingMoveWallSE)
            {
                //壁が移動するSEを停止
                audioSourceSE.Stop();

                //現在再生中フラグをオフ
                isPlayingMoveWallSE = false;
            }
        }
    }

    /// <summary>
    /// 壁が移動する効果音の再生を開始する
    /// </summary>
    void StartMoveWallSE()
    {
        //壁が移動するSEを設定
        audioSourceSE.clip = sO_SE.GetSEClip(moveWallSEid);

        //ループ再生を無効にする
        audioSourceSE.loop = false;

        //初回の音量を計算して再生
        UpdateVolumeBasedOnDistance();
        audioSourceSE.Play();

        // 現在再生中フラグをオン
        isPlayingMoveWallSE = true;
    }

    /// <summary>
    /// プレイヤーとの距離に応じて音量を常に更新する
    /// </summary>
    void UpdateVolumeBasedOnDistance()
    {
        //プレイヤーが存在しない場合||audioSourceSEが存在しない場合
        if (Player.instance == null || audioSourceSE == null) 
        {
            //処理をスキップ
            return; 
        }

        //プレイヤーとの距離を測定
        distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);

        //最終音量 = マスターSE音量 × 距離ベースの相対音量（0～1）
        audioSourceSE.volume = masterSEVolume * CalculateVolumeBasedOnDistance(distanceToPlayer);
    }

    /// <summary>
    /// 距離に基づく音量を計算するメソッド
    /// </summary>
    /// <param name="distance">プレイヤーとの距離</param>
    /// <returns>音量</returns>
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            //最大音量
            return maxVolume;
        }
        else if (distance >= minSoundDistance)
        {
            //最小音量
            return minVolume;
        }
        else
        {
            //距離に基づいて音量を調整
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}
