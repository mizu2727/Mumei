using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class Player : MonoBehaviour, CharacterInterface
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Player instance { get; private set; }

    /// <summary>
    /// キャラクターコントローラー
    /// </summary>
    CharacterController characterController;

    /// <summary>
    /// アニメーション
    /// </summary>
    private Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }

    [Header("名前(ヒエラルキー上での編集禁止)")]

    [SerializeField]
    public string CharacterName
    {
        get => GameController.playerName; 
        set => GameController.playerName = value;

    }

    [Header("歩行速度")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField]
    public float NormalSpeed
    {
        get => walkSpeed;
        set => walkSpeed = value;
    }

    [Header("ダッシュ速度")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField]
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }

    /// <summary>
    /// 移動速度の現在値
    /// </summary>
    float speed;

    [Header("スタミナSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider staminaSlider;

    /// <summary>
    /// スタミナ最大値
    /// </summary>
    private const float kMaxStamina = 100f;

    /// <summary>
    /// スタミナの現在値
    /// </summary>
    private float stamina;

    /// <summary>
    /// デフォルトのスタミナ消費値
    /// </summary>
    private const float kDefaultStaminaConsumeRatio = 25.0f;

    /// <summary>
    /// スタミナ消費値
    /// </summary>
    private float staminaConsumeRatio = 25.0f;

    /// <summary>
    /// スタミナ回復値
    /// </summary>
    private const float kStaminaRecoveryRatio = 20f;

    /// <summary>
    /// スタミナ使用可能フラグ
    /// </summary>
    private bool isStamina;

    [Header("検知範囲")]
    [SerializeField] private float playerDetectionRange = 10f;
    [SerializeField]
    public float DetectionRange
    {
        get => playerDetectionRange;
        set => playerDetectionRange = value;
    }

    [Header("重力")]
    [SerializeField] private float playerGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => playerGravity;
        set => playerGravity = value;
    }

    /// <summary>
    /// デフォルトの体力
    /// </summary>
    private const int kDefaultHP = 1;

    [Header("体力")]
    [SerializeField] private int playerHP = 1;
    [SerializeField]
    public int HP
    {
        get => playerHP;
        set => playerHP = value;
    }

    [Header("死亡フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool playerIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => playerIsDead;
        set => playerIsDead = value;
    }

    [Header("プレイヤー移動フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool playerIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => playerIsMove;
        set => playerIsMove = value;
    }

    [Header("プレイヤーダッシュフラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool playerIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => playerIsDash;
        set => playerIsDash = value;
    }

    [Header("後ろを向くフラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] public bool playerIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => playerIsBackRotate;
        set => playerIsBackRotate = value;
    }

    [Header("ライト切り替えフラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] private bool playerIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => playerIsLight;
        set => playerIsLight = value;
    }

    /// <summary>
    /// 死亡
    /// </summary>
    public void Dead()
    {
        IsDead = true;

        //移動方向をリセット
        moveDirection = Vector3.zero;

        //アニメーションを停止
        if (animator != null)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }

        //効果音を停止
        if (audioSourceSE != null)
        {
            audioSourceSE.Stop();
        }

        

        //ゲームオーバー画面へ遷移
        GameController.instance.ViewGameOver();

        //プレイヤー削除
        DestroyPlayer();
    }

    /// <summary>
    /// 攻撃
    /// </summary>
    public void Attack()
    {
        Debug.Log("Player Attack!");
    }

    [Header("初期位置")]
    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => playerStartPosition;
        set => playerStartPosition = value;
    }

    /// <summary>
    /// 仮の名前
    /// </summary>
    [SerializeField] const string aliasName = "イフ";

    [Header("鍵の所持フラグ(ヒエラルキー上での編集禁止)")]
    [SerializeField] public bool isHoldKey = false;

    /// <summary>
    /// 移動方向
    /// </summary>
    Vector3 moveDirection = Vector3.zero;

    /// <summary>
    /// 移動境界値
    /// </summary>
    private const float kMovingBoundaryValue = 0.01f;


    /// <summary>
    /// 180度回転ベクトル
    /// </summary>
    private Vector3 rotate180 = new Vector3(0, 180f, 0);

    /// <summary>
    /// 回転速度倍率
    /// </summary>
    private const float kRotationSpeedMagnification = 0.5f;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("歩行音・ダッシュ音用のaudioSource(ヒエラルキー上での編集禁止)")]
    public AudioSource audioSourceSE;

    /// <summary>
    /// 歩行音のID
    /// </summary>
    private readonly int walkSEid = 0;

    /// <summary>
    /// ダッシュ音のID
    /// </summary>
    private readonly int runSEid = 1;

    /// <summary>
    /// 現在再生中の効果音
    /// </summary>
    private AudioClip currentSE;

    /// <summary>
    /// 前フレームの移動状態フラグ
    /// </summary>
    private bool wasMovingLastFrame = false;


    [Header("プレイヤーが倒れているフラグ(ヒエラルキー上での編集禁止)")]
    public bool isFallDown = false;

    [Header("デバッグモード")]
    public bool isDebug = false;


    [Header("タグ・レイヤー関連")]
    [SerializeField] string doorTag = "Door";
    [SerializeField] string doorPartsTag = "DoorParts";


    /// <summary>
    /// IgnoreObject
    /// </summary>
    private IgnoreObject ignoreObject;

    /// <summary>
    /// 対象の開閉したいドア
    /// </summary>
    GameObject gameObjectDoor;


    /// <summary>
    /// kCharacterControllerRadius(要調整)
    /// </summary>
    private const float kCharacterControllerRadius = 0.9f;


    /// <summary>
    /// デフォルトのスタミナ消費値を取得
    /// </summary>
    /// <returns>デフォルトのスタミナ消費値</returns>
    public float GetDefaultStaminaConsumeRatio() 
    {
        return kDefaultStaminaConsumeRatio;
    }

    /// <summary>
    /// スタミナ消費値を設定
    /// </summary>
    /// <param name="specifiedStaminaConsumeRatio">スタミナ消費値</param>
    public void SetStaminaConsumeRatio(float specifiedStaminaConsumeRatio)
    {
        staminaConsumeRatio = specifiedStaminaConsumeRatio;
    }

    /// <summary>
    /// 現在再生中の効果音を取得
    /// </summary>
    /// <returns>現在再生中の効果音</returns>
    public AudioClip GetCurrentSE() 
    {
        return currentSE;
    }

    private void Awake()
    {
        //インスタンス
        if (instance != null && instance != this)
        {
            DestroyPlayer();
            return;
        }
        instance = this;


        Debug.Log("Player生成");
    }

    /// <summary>
    /// 移動入力があるかどうかを判定
    /// </summary>
    /// <returns>移動しているならtrue</returns>
    public bool IsPlayerMoving()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        return Mathf.Abs(moveX) > kMovingBoundaryValue || Mathf.Abs(moveZ) > kMovingBoundaryValue;
    }

    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    void OnDestroy()
    {
        //インスタンス破棄
        if (instance == this)
        {
            instance = null;
        }
    }


    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;

    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// シーン遷移時に設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceを設定
        InitializeAudioSource();

        //GameController からstaminaSliderを取得
        //画面遷移時にstaminaSliderの数値が変化しないバグを防ぐ用
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = kMaxStamina;
                staminaSlider.value = stamina;
            }
        }

        //Stage01Sceneに遷移した際に位置をリセット
        if (scene.name == "Stage01Scene")
        {
            //CharacterControllerを一時的に無効にして位置をリセット
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            transform.position = playerStartPosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            moveDirection = Vector3.zero;
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            //プレイヤーの状態を初期化
            IsDead = false;
            playerHP = kDefaultHP;
            stamina = kMaxStamina;
            isStamina = true;
        }
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

    private void Start()
    {
        IsDead = false;

        //コンポーネントの取得
        characterController = GetComponent<CharacterController>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        PlayAnimator = GetComponent<Animator>();

        if (capsuleCollider != null && characterController != null)
        {
            //CharacterControllerのRadiusをCapsuleColliderのRadiusより小さく設定（例: 0.9倍）
            characterController.radius = capsuleCollider.radius * kCharacterControllerRadius;
        }
        else
        {
            Debug.LogWarning("CapsuleColliderまたはCharacterControllerが見つかりません。");
        }

        //プレイヤーの初期位置
        playerStartPosition = transform.position;

        //AudioSourceの初期化
        InitializeAudioSource();

        //スタミナ最大値の初期化
        stamina = kMaxStamina;

        //GameControllerからstaminaSliderを取得
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
        }

        //スタミナSliderの最大値を設定
        if (staminaSlider)
        {
            staminaSlider.maxValue = kMaxStamina;
            staminaSlider.value = stamina;
        }

        isStamina = true;

        playerIsBackRotate = false;
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //AudioSourceを取得
        //(別の効果音が鳴っている間に敵の効果音が鳴らないバグを防止する用)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    private void Update()
    {
        //デバッグ時でQキー押下すると死亡する
        if (isDebug && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("プレイヤー死亡(デバッグモード)");
            Dead();
        }

        //デバッグ時でCキー押下するとクリア画面へ遷移する
        if (isDebug && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("クリア(デバッグモード)");
            SceneManager.LoadScene("GameClearScene");
        }

        //ストーリーモードでプレイヤーを回転させる
        if (playerIsBackRotate && (GameController.instance.gameModeStatus == GameModeStatus.Story)) PlayerTurn();

        //通常のプレイ以外の場合、処理をスキップ
        if (playerIsDead || Time.timeScale == 0 || isFallDown || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) 
        {
            //移動方向をリセット
            moveDirection = Vector3.zero;
            return;
        } 

        //PauseControllerがないSceneでnullチェックエラーを回避するために、個別でわけている
        if (PauseController.instance.isPause) return;


        //ダッシュ判定
        PlayerDashOrWalk();

        //前後左右の入力から、移動のためのベクトルを計算
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // カメラのTransformを取得
        Transform cameraTransform = Camera.main.transform;

        // カメラのforwardベクトルをXZ平面に投影
        Vector3 cameraForwardXZ = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;


        //移動方向を計算
        Vector3 move;


        if (playerIsBackRotate)
        {
            // プレイヤーの体の向きを基準に移動方向を計算
            Vector3 playerForwardXZ = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 playerRightXZ = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

            // プレイヤーの前方/後方（moveZ）と右/左（moveX）を基に移動ベクトルを計算
            move = playerRightXZ * moveX + playerForwardXZ * moveZ;
        }
        else
        {
            // 通常時（カメラの向きに沿って移動）
            move = cameraRightXZ * moveX + cameraForwardXZ * moveZ;
        }

        // isGrounded は地面にいるかどうかを判定する
        if (characterController.isGrounded)
        {
            moveDirection = move * speed;

            // 接地時にY方向の移動をリセット
            moveDirection.y = 0f;
        }
        else
        {
            // 重力を効かせる
            moveDirection = move + new Vector3(0, moveDirection.y, 0);
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        //Moveは指定したベクトルだけ移動させる命令
        characterController.Move(moveDirection * Time.deltaTime);

        //移動判定
        IsMove = IsPlayerMoving();

        //スタミナ管理
        PlayerStaminaManage();


        //アニメーションの制御
        if (IsMove)
        {
            //移動中:Shiftに応じて走行または歩行する
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));

        }
        else
        {
            //停止中:両方のアニメーションをオフ
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        //audioSourceSEの状態を確認
        if (audioSourceSE == null || !audioSourceSE)
        {
            Debug.LogWarning("audioSourceSEがnullまたはMissingです。再設定を試みます。");
            InitializeAudioSource();
            if (audioSourceSE == null)
            {
                Debug.LogError("audioSourceSEを再設定できませんでした。");
                return;
            }
        }


        //移動状態の変化を検知して効果音を制御
        currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash") ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);



        if (IsMove && !wasMovingLastFrame)
        {
            //移動開始時に効果音を再生
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            //移動停止時に効果音を停止
            StopPlayerSE(audioSourceSE);
        }
        else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
        {
            //移動中に歩行/ダッシュが切り替わった場合、効果音を変更
            audioSourceSE.Stop();
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }

        //現在の移動状態を記録
        wasMovingLastFrame = IsMove;


        //後ろを向いているかを判定
        playerIsBackRotate = PlayerIsBackRotate();

    }


    /// <summary>
    /// ダッシュ判定
    /// </summary>
    void PlayerDashOrWalk() 
    {
        //Shiftキー・Xボタンを入力している間はダッシュ
        //Dash…"joystick button 4"を割り当て。コントローラーではLボタンになる
        if (IsMove && isStamina && Time.timeScale == 1 
            && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash")))
        {
            //ダッシュ開始
            IsDash = true;

            speed = SprintSpeed;

        }
        else
        {
            //ダッシュ終了
            IsDash = false;

            speed = NormalSpeed;
        }
    }

    /// <summary>
    /// スタミナ管理
    /// </summary>
    void PlayerStaminaManage() 
    {
        if (IsDash)
        {
            if (0 < stamina)
            {
                //ダッシュ中はスタミナを消費
                stamina -= staminaConsumeRatio * Time.deltaTime;
            }
            else
            {
                IsDash = false;

                //ここでスピードを落とす
                //スタミナ切れでダッシュ不可
                isStamina = false;

                speed = NormalSpeed;
            }
        }
        else if (stamina < kMaxStamina)
        {
            //ダッシュしていないときはスタミナを回復
            stamina += kStaminaRecoveryRatio * Time.deltaTime;  
        }
        else if (kMaxStamina <= stamina)
        {
            //スタミナ復活でダッシュ可能
            stamina = kMaxStamina;
            isStamina = true;
        }

        if (staminaSlider)
        {
            //スライダーに値を反映
            staminaSlider.value = stamina;
        }
    }

    /// <summary>
    /// SEを停止する
    /// </summary>
    /// <param name="subjectAudioSourceSE">対象のSE</param>
    public void StopPlayerSE(AudioSource subjectAudioSourceSE) 
    {
        subjectAudioSourceSE.Stop();
    }

    /// <summary>
    /// 振り返り判定
    /// </summary>
    /// <returns>Ctrl押下でtrue</returns>
    public bool PlayerIsBackRotate() 
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    /// <summary>
    /// プレイヤー回転
    /// </summary>
    public void PlayerTurn() 
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.Story)
        {
            //プレイヤーの向きを180度ゆっくり回転させる
            if (transform.rotation.y < 0) transform.Rotate(rotate180 * (Time.deltaTime * kRotationSpeedMagnification));
            else playerIsBackRotate = false;
        }
    }

    /// <summary>
    /// プレイヤーワープ処理
    /// </summary>
    /// <param name="x">X座標</param>
    /// <param name="y">Y座標</param>
    /// <param name="z">Z座標</param>
    public void PlayerWarp(float x, float y, float z) 
    {
        //CharacterControllerを一時的に無効にする
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        //transform.positionを直接設定してワープする場合、一時的にCharacterControllerを一時的に無効にする必要がある
        transform.position = new Vector3(x, y, z);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        //CharacterControllerを再度有効にする
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    /// <summary>
    /// プレイヤー削除処理
    /// </summary>
    public void DestroyPlayer() 
    {
        Destroy(gameObject);
    }


    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">コライダー</param>
    private void OnTriggerEnter(Collider collider)
    {
        //DoorTagのオブジェクトが触れた場合
        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;

            //子オブジェクトのコンポーネントを取得
            ignoreObject = gameObjectDoor.GetComponentInChildren<IgnoreObject>();

            //子オブジェクトのタグがdoorPartsTagの場合
            if (ignoreObject.CompareTag(doorPartsTag)) 
            {
                //コライダーを無効化
                ignoreObject.GetMeshCollider().enabled = false;
            }
        }
    }

    /// <summary>
    /// オブジェクトのコライダーから離れた場合の処理
    /// </summary>
    /// <param name="collider">コライダー</param>
    private void OnTriggerExit(Collider collider)
    {
        //DoorTagのオブジェクトから離れた場合
        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;

            //子オブジェクトのコンポーネントを取得
            ignoreObject = gameObjectDoor.GetComponentInChildren<IgnoreObject>();

            //子オブジェクトのタグがdoorPartsTagの場合
            if (ignoreObject.CompareTag(doorPartsTag))
            {
                //コライダーを有効化
                ignoreObject.GetMeshCollider().enabled = true;
            }
        }
    }
}
