using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 放送を流すクラス
/// </summary>
public class BroadcastSpeaker : MonoBehaviour
{
    /// <summary>
    /// 放送ノイズ再生フラグ
    /// </summary>
    private bool isBroadcastNoise = false;

    /// <summary>
    /// 放送ノイズ再生フラグを取得する
    /// </summary>
    /// <returns>放送ノイズ再生フラグ</returns>
    public bool GetIsBroadcastNoise()
    {
        return isBroadcastNoise;
    }

    /// <summary>
    /// 放送ノイズ再生フラグを設定する
    /// </summary>
    /// <param name="value">放送ノイズ再生フラグ</param>
    public void SetIsBroadcastNoise(bool value)
    {
        isBroadcastNoise = value;
    }

    /// <summary>
    /// 放送ノイズを聞いているフラグ
    /// </summary>
    private bool isListeningBroadcast = false;

    /// <summary>
    /// 放送ノイズを聞いているフラグを取得する
    /// </summary>
    /// <returns>放送ノイズを聞いているフラグ</returns>
    public bool GetIsListeningBroadcast()
    {
        return isListeningBroadcast;
    }

    /// <summary>
    /// 放送ノイズを聞いているフラグを設定する
    /// </summary>
    /// <param name="value">放送ノイズを聞いているフラグ</param>
    public void SetIsListeningBroadcast(bool value)
    {
        isListeningBroadcast = value;
    }


    /*--------------------------------------
    * プレイヤー関連の変数
    ---------------------------------------*/

    /// <summary>
    /// プレイヤー
    /// </summary>
    private Player player;

    [Header("放送スピーカーとの距離を測定したいオブジェクトをアタッチ(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;

    /*--------------------------------------
    * SE関連の変数
    ---------------------------------------*/

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// ノイズ系専用のAudioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// 放送ノイズSEのID
    /// </summary>
    private readonly int broadcastNoiseSEid = 25;

    /// <summary>
    /// 指定の放送SEのID
    /// </summary>
    private readonly int broadcastSEid = 26;

    [Header("サウンドの距離関連(要調整)")]
    [Header("音量が最大になる距離")]
    [SerializeField] private float maxSoundDistance = 10f;

    [Header("音量が最小になる距離")]
    [SerializeField] private float minSoundDistance = 20f;

    [Header("最大音量")]
    [SerializeField] private float maxVolume = 1.0f;

    [Header("最小音量")]
    [SerializeField] private float minVolume = 0.0f;


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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();
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

        //ループ再生をオンにする
        audioSourceSE.loop = true;

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }


    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //フラグの初期化
        isBroadcastNoise = false;
        isListeningBroadcast = false;
    }

    void Update()
    {
        //再生中の放送ノイズを聞いている場合
        if (isBroadcastNoise) 
        {
            //放送を流す。
            PlayBroadcast();
        }
        else 
        {
            //放送ノイズを聞いているフラグをオフにする。
            isListeningBroadcast = false;


            if (audioSourceSE.isPlaying)
            {
                audioSourceSE.Stop();
            }
        }
    }

    /// <summary>
    /// ノイズor特定の放送を流す処理
    /// </summary>
    private void PlayBroadcast() 
    {
        //nullチェック
        if (targetPoint == null)
        {
            Debug.LogWarning("targetPointがnullです。プレイヤーオブジェクトを正しく設定してください。");
            return;
        }

        AudioClip currentSE;

        //再生中の放送ノイズを聞いている場合
        if (isListeningBroadcast)
        {
            //指定の放送用のSE
            currentSE = sO_SE.GetSEClip(broadcastSEid);

            Debug.Log("特定の放送を流す");
        }
        else
        {
            //放送ノイズSE
            currentSE = sO_SE.GetSEClip(broadcastNoiseSEid);

            //Debug.Log("放送ノイズを流す");
        }

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        //距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        //PlayOneShotを使用して、移動音と競合しないように単発再生
        //audioSourceSE.PlayOneShot(currentSE, volume);

        //音量を設定
        audioSourceSE.volume = volume;

        //現在再生中のクリップと異なる、または再生されていない場合のみ再生を開始する
        if (audioSourceSE.clip != currentSE || !audioSourceSE.isPlaying) 
        {
            audioSourceSE.clip = currentSE;
            audioSourceSE.Play();
        }
    }

    /// <summary>
    /// 距離に基づく音量を計算するメソッド
    /// </summary>
    /// <param name="distance">対象オブジェクトの距離</param>
    /// <returns></returns>
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
