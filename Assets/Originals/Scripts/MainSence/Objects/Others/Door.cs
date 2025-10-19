using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{

    [Header("ドアの開閉フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isOpenDoor = false;

    [Header("鍵が掛かっているかのフラグ")]
    [SerializeField] public bool isNeedKeyDoor = false;

    [Header("ドア開閉時の回転角度")]
    [Header("ドアを開ける角度")]
    [SerializeField] float openDirenctionValue = 90.0f;

    [Header("ドアを閉じる角度")]
    [SerializeField] float closeDirenctionValue = 0.0f;

    /// <summary>
    /// プレイヤー
    /// </summary>
    private Player player;


    [Header("ドア間との距離を測定したいオブジェクトをアタッチ(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// Door専用のAudioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ドアを開けるSEのID
    /// </summary>
    private readonly int openSEid = 6;

    /// <summary>
    /// ドアを閉めるSEのID
    /// </summary>
    private readonly int closeSEid = 5;

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
    }

    private void OnDisable()
    {
        //シーン遷移時にAudioSourceを再設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
    }

    private void Start()
    {
        //AudioSourceの初期化
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
            //ドアを閉じる
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
                //ドアを開ける
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
        //nullチェック
        if (targetPoint == null)
        {
            Debug.LogWarning("targetPointがnullです。プレイヤーオブジェクトを正しく設定してください。");
            return;
        }

        //効果音制御
        AudioClip currentSE = (isOpenDoor) ? sO_SE.GetSEClip(openSEid) : sO_SE.GetSEClip(closeSEid);

        //プレイヤーとの距離を測定
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        //距離に基づく音量計算
        float volume = CalculateVolumeBasedOnDistance(distance);

        //PlayOneShotを使用して、移動音と競合しないように単発再生
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
