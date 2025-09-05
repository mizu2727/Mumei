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
    [SerializeField] private AudioClip openSE;
    private readonly int openSEid = 6; // ドアを開けるSEのID
    [SerializeField] private AudioClip closeSE;
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
        // AudioSourceの取得と検証
        if (audioSourceSE == null || !audioSourceSE)
        {
            if (MusicController.Instance != null)
            {
                audioSourceSE = MusicController.Instance.GetAudioSource();
                if (audioSourceSE != null)
                {
                    audioSourceSE.playOnAwake = false;
                }
                else
                {
                    Debug.LogError("MusicControllerからAudioSourceを取得できませんでした。");
                }
            }
            else
            {
                Debug.LogError("MusicController.Instanceが見つかりません。");
            }
        }
    }

    private void Start()
    {
        // AudioSourceの初期化
        InitializeAudioSource();
    }

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

    //ドアを開ける
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
        DoorSE();
    }

    //ドアを閉める
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
        DoorSE();
    }


    //ドアの開閉の効果音
    void DoorSE() 
    {
        // 効果音制御
        AudioClip currentSE = (isOpenDoor) ? openSE : closeSE;

        // プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // 距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        MusicController.Instance.PlayAudioSE(audioSourceSE, currentSE);

        //音量を設定
        audioSourceSE.volume = volume;
    }


    // 距離に基づく音量を計算するメソッド
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
