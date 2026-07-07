using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static GameController;

public class Door : MonoBehaviour
{
    /*--------------------------------------
    * 開閉フラグ関連の変数
    ---------------------------------------*/

    [Header("ドアの開閉フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isOpenDoor = false;


    /*--------------------------------------
    * 鍵関連の変数
    ---------------------------------------*/

    [Header("鍵が掛かっているかのフラグ")]
    [SerializeField] private bool isNeedKeyDoor = false;

    /// <summary>
    /// 鍵が掛かっているかのフラグを取得する関数
    /// </summary>
    /// <returns>鍵が掛かっているかのフラグ</returns>
    public bool GetIsNeedKeyDoor()
    {
        return isNeedKeyDoor;
    }

    /*--------------------------------------
    * 回転角度関連の変数
    ---------------------------------------*/

    [Header("ドア開閉時の回転角度")]
    [Header("ドアを開ける角度")]
    [SerializeField] float openDirenctionValue = 90.0f;

    [Header("ドアを閉じる角度")]
    [SerializeField] float closeDirenctionValue = 0.0f;


    /* -------------------------
    * スライド式ドア関連
    * -------------------------*/

    [Header("ドアのスライドタイプ(ヒエラルキー上から直接編集すること)")]
    public SlideType slideType;

    /// <summary>
    /// スライドタイプの列挙型
    /// </summary>
    public enum SlideType
    {
        /// <summary>
        /// 縦
        /// </summary>
        Vertical,

        /// <summary>
        /// 横
        /// </summary>
        Beside,
    }

    [Header("スライド式ドアフラグ(ONにすると横スライド開閉になる)")]
    [SerializeField] private bool isSlidingDoor = false;

    [Header("スライド方向(ローカル座標のX軸方向に移動。負値で逆方向)")]
    [SerializeField] private float slideDistance = 2.0f;

    [Header("スライド速度")]
    [SerializeField] private float slideSpeed = 3.0f;

    /// <summary>
    /// スライドドアの開いた位置(ワールド座標)
    /// </summary>
    private Vector3 slideOpenPosition;

    /// <summary>
    /// スライドドアの閉じた位置(ワールド座標)
    /// </summary>
    private Vector3 slideClosedPosition;

    /// <summary>
    /// スライド中フラグ
    /// </summary>
    private bool isSliding = false;

    /// <summary>
    /// スライド目標座標
    /// </summary>
    private Vector3 slideTargetPosition;


    /*--------------------------------------
    * プレイヤー関連の変数
    ---------------------------------------*/

    /// <summary>
    /// プレイヤー
    /// </summary>
    private Player player;

    [Header("ドア間との距離を測定したいオブジェクトをアタッチ(ヒエラルキー上のプレイヤーをアタッチすること)")]
    [SerializeField] public Transform targetPoint;


    /*--------------------------------------
    * SE関連の変数
    ---------------------------------------*/

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

    /// <summary>
    /// スライド式ドアを開閉する用SEのID
    /// </summary>
    private readonly int slideOpenSEid = 24;


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

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }


    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    void OnDestroy() 
    {
        //targetPointが存在する場合
        if (targetPoint != null)
        {
            //targetPointをnullに設定
            targetPoint = null;
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
            Debug.LogWarning("Player.instanceが見つかりません。シーンにPlayerオブジェクトが存在することを確認してください。");
        }

        //スライドドアの場合
        if (isSlidingDoor)
        {
            //開閉位置をスタート時のローカルX軸方向を基準に事前計算
            slideClosedPosition = transform.position;

            //スライドが縦方向の場合
            if (slideType == SlideType.Vertical) 
            {
                //スライドドアの素材の元々の向きが正常でないため、transform.upを使用してスライド方向を計算
                slideOpenPosition = transform.position + transform.up * slideDistance;
            }
            //スライドが横方向の場合
            else if (slideType == SlideType.Beside) 
            {
                //transform.rightを使用してスライド方向を計算
                slideOpenPosition = transform.position + transform.right * slideDistance;
            }


        }
    }

    private void Update()
    {
        //スライド中フラグがオンの場合
        if (isSliding)
        {
            //現在の位置から目標座標へ扉をスライド移動させる
            transform.position = Vector3.MoveTowards(transform.position, slideTargetPosition, slideSpeed * Time.deltaTime);

            //目標座標に到達する場合
            if (Vector3.Distance(transform.position, slideTargetPosition) < 0.001f)
            {
                //スライド終了
                transform.position = slideTargetPosition;
                isSliding = false;
            }
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

        //スライドドアの場合
        if (isSlidingDoor)
        {
            //スライドドア：横方向へスライド開始
            StartSlide(slideOpenPosition);
        }
        else
        {
            //回転ドア：従来の回転処理
            transform.Rotate(0, openDirenctionValue, 0);
        }

        DoorSE();
    }

    /// <summary>
    /// ドアを閉める
    /// </summary>
    void CloseDoor()
    {
        isOpenDoor = false;

        //スライドドアの場合
        if (isSlidingDoor)
        {
            //スライドドア：元の位置へスライド開始
            StartSlide(slideClosedPosition);
        }
        else
        {
            //回転ドア：従来の回転処理
            transform.Rotate(0, closeDirenctionValue, 0);
        }

        DoorSE();
    }


    /// <summary>
    /// スライドを開始する処理
    /// </summary>
    /// <param name="targetPos">移動先のワールド座標</param>
    private void StartSlide(Vector3 targetPos)
    {
        slideTargetPosition = targetPos;
        isSliding = true;
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

        AudioClip currentSE;

        //スライドドアの場合
        if (isSlidingDoor)
        {
            //スライド用のSE
            currentSE = sO_SE.GetSEClip(slideOpenSEid);
        }
        else
        {
            //通常の開閉SE
            currentSE = isOpenDoor ? sO_SE.GetSEClip(openSEid) : sO_SE.GetSEClip(closeSEid);
        }

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
