using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{

    [Header("ドアの開閉の判定")]
    public bool isOpenDoor = false;

    [Header("鍵が掛かっているかを判定")]
    [SerializeField] public bool isNeedKeyDoor = false;

    [Header("ドア開閉時の回転角度")]
    [SerializeField] float openDirenctionValue = 90.0f;//ドアを開ける角度
    [SerializeField] float closeDirenctionValue = 0.0f;//ドアを閉じる角度

    //プレイヤー
    private Player player;

    [Header("ドア間との距離を測定したいオブジェクトをアタッチ(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("サウンド関連")]
    private AudioSource audioSourceSE;//Door専用のAudioSource
    private readonly int openSEid = 6; // ドアを開けるSEのID
    private readonly int closeSEid = 5; // ドアを閉めるSEのID

    [Header("サウンドの距離関連(要調整)")]
    [SerializeField] private float maxSoundDistance = 10f; // 音量が最大になる距離
    [SerializeField] private float minSoundDistance = 20f; // 音量が最小になる距離
    [SerializeField] private float maxVolume = 1.0f; // 最大音量
    [SerializeField] private float minVolume = 0.0f; // 最小音量

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定するためのイベント登録解除
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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
    }

    private void Start()
    {
        // AudioSourceの初期化
        InitializeAudioSource();

        //PlayerシングルトンからTransformを取得
        //(シーン遷移した後にプレイヤーのtransformがnullになるエラーを防止する用)
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError("Player.instanceが見つかりません。シーンにPlayerオブジェクトが存在することを確認してください。");
        }
    }

    /// <summary>
    /// ドアの開閉システム
    /// </summary>
    public void DoorSystem() 
    {
        if (isOpenDoor)
        {
            CloseDoor();
        }
        else 
        {
            //鍵が必要な場合
            if (isNeedKeyDoor && player.isHoldKey)
            {
                Debug.Log("解錠しました。");
                player.isHoldKey = false;
                isNeedKeyDoor = false;
            }
            else if (!isNeedKeyDoor)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("施錠されている。");
            }
        }
    }

    /// <summary>
    /// ドアを開ける
    /// </summary>
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
        DoorSE();
    }

    /// <summary>
    /// ドアを閉める
    /// </summary>
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
        DoorSE();
    }


    /// <summary>
    /// ドアの開閉の効果音
    /// </summary>
    void DoorSE() 
    {
        if (targetPoint == null)
        {
            Debug.LogWarning("targetPointがnullです。プレイヤーオブジェクトを正しく設定してください。");
            return;
        }

        // 効果音制御
        AudioClip currentSE = (isOpenDoor) ? sO_SE.GetSEClip(openSEid) : sO_SE.GetSEClip(closeSEid);

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // 距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        // PlayOneShotを使用して、移動音と競合しないように単発再生
        audioSourceSE.PlayOneShot(currentSE, volume);

        //音量を設定
        audioSourceSE.volume = volume;
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
            // 最大音量
            return maxVolume; 
        }
        else if (distance >= minSoundDistance)
        {
            // 最小音量
            return minVolume; 
        }
        else
        {
            // 距離に基づいて線形補間
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}
