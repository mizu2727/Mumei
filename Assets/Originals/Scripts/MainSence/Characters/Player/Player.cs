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
    public static Player instance { get; private set; }

    CharacterController characterController;

    private Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }

    [Header("名前")]
    //[SerializeField] public string playerName;

    [SerializeField]
    public string CharacterName
    {
        get => GameController.playerName; 
        set => GameController.playerName = value;

        //get => playerName;
        //set => playerName = value;
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

    //移動速度の現在値
    float speed;

    [Header("スタミナSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider staminaSlider;

    [Header("スタミナ最大値")]
    [SerializeField] float maxStamina = 100f;

    //スタミナの現在値
    float stamina;

    [Header("スタミナ消費値")]
    [SerializeField] float staminaConsumeRatio = 50f;

    [Header("スタミナ回復値")]
    [SerializeField] float staminaRecoveryRatio = 20f;

    //スタミナの使用が可能であるかを判定
    bool isStamina;

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

    [Header("体力")]
    [SerializeField] private int playerHP = 1;
    [SerializeField]
    public int HP
    {
        get => playerHP;
        set => playerHP = value;
    }

    [Header("死亡判定")]
    [SerializeField] private bool playerIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => playerIsDead;
        set => playerIsDead = value;
    }

    [Header("プレイヤーが動いているかを判定")]
    [SerializeField] private bool playerIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => playerIsMove;
        set => playerIsMove = value;
    }

    [Header("プレイヤーがダッシュしているかを判定")]
    [SerializeField] private bool playerIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => playerIsDash;
        set => playerIsDash = value;
    }

    [Header("プレイヤーが後ろを向いているかを判定")]
    [SerializeField] public bool playerIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => playerIsBackRotate;
        set => playerIsBackRotate = value;
    }

    [Header("ライト切り替え")]
    [SerializeField] private bool playerIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => playerIsLight;
        set => playerIsLight = value;
    }

    public void Dead()
    {
        SceneManager.LoadScene("GameOverScene");
        //Destroy(gameObject);
    }

    public void Attack()
    {
        Debug.Log("Player Attack!");
    }

    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => playerStartPosition;
        set => playerStartPosition = value;
    }

    [Header("仮の名前")]
    [SerializeField] const string aliasName = "イフ";

    [Header("鍵の所持の有無")]
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//移動方向

    [Header("サウンド関連")]
    public AudioSource audioSourceSE; // プレイヤー専用のAudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    public AudioClip currentSE;//現在再生中の効果音

    private bool wasMovingLastFrame = false; // 前フレームの移動状態を保持


    [Header("プレイヤーが倒れているかを判定")]
    public bool isFallDown = false;

    [Header("デバッグモード")]
    public bool isDebug = false;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public bool IsPlayerMoving()
    {
        // 移動入力があるかどうかを判定
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        return Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
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
            // CharacterControllerのRadiusをCapsuleColliderのRadiusより小さく設定（例: 0.9倍）
            characterController.radius = capsuleCollider.radius * 0.9f;
        }
        else
        {
            Debug.LogWarning("CapsuleColliderまたはCharacterControllerが見つかりません。");
        }

        //プレイヤーの初期位置
        playerStartPosition = transform.position;

        // MusicControllerからAudioSourceを取得
        audioSourceSE = MusicController.Instance.GetAudioSource();

        //スタミナ現在値の初期化
        stamina = maxStamina;

        //スタミナSliderSliderの最大値を設定
        if (staminaSlider) staminaSlider.maxValue = maxStamina;

        isStamina = true;

        playerIsBackRotate = false;
    }


    private void Update()
    {
        if (isDebug && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("プレイヤー死亡(デバッグモード)");
            Dead();
        }

        if (isDebug && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("クリア(デバッグモード)");
            SceneManager.LoadScene("GameClearScene");
        }

        if (playerIsBackRotate && (GameController.instance.gameModeStatus == GameModeStatus.Story)) PlayerTurn();

        if (playerIsDead  || Time.timeScale == 0 || isFallDown || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) return;

        //PauseControllerがないSceneでnullチェックエラーを回避するために、個別でわけている
        if (PauseController.instance.isPause) return;


        //ダッシュ判定
        PlayerDashOrWalk();

        // 前後左右の入力から、移動のためのベクトルを計算
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        //移動方向を計算
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // isGrounded は地面にいるかどうかを判定する
        if (characterController.isGrounded)
        {
            moveDirection = move * speed;
        }
        else
        {
            // 重力を効かせる
            moveDirection = move + new Vector3(0, moveDirection.y, 0);
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        // Move は指定したベクトルだけ移動させる命令
        characterController.Move(moveDirection * Time.deltaTime);


        IsMove = IsPlayerMoving();

        //スタミナ管理
        PlayerStaminaManage();


        // アニメーションの制御
        if (IsMove)
        {
            // 移動中: LeftShiftに応じて走行または歩行
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));

        }
        else
        {
            // 停止中: 両方のアニメーションをオフ
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        // 移動状態の変化を検知して効果音を制御
        currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash") ? runSE : walkSE;



        if (IsMove && !wasMovingLastFrame)
        {
            // 移動開始時に効果音を再生
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            // 移動停止時に効果音を停止
            MusicController.Instance.StopSE(audioSourceSE);
        }
        else if (IsMove && wasMovingLastFrame && MusicController.Instance.IsPlayingSE(audioSourceSE)
                 && MusicController.Instance.GetCurrentSE(audioSourceSE) != currentSE)
        {
            // 移動中に歩行/ダッシュが切り替わった場合、効果音を変更
            MusicController.Instance.StopSE(audioSourceSE);
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }

        // 現在の移動状態を記録
        wasMovingLastFrame = IsMove;


        //後ろを向いているかを判定
        if (PlayerIsBackRotate()) playerIsBackRotate = true;
        else playerIsBackRotate = false;

    }

    //ダッシュ判定
    void PlayerDashOrWalk() 
    {
        //Shiftキー・Xボタンを入力している間はダッシュ
        //Dash…"joystick button 4"を割り当て。コントローラーではLボタンになる
        if (IsMove && isStamina && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash")))
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

    //スタミナ管理
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

                // ここでスピードを落とす

                //スタミナ切れでダッシュ不可
                isStamina = false;

                speed = NormalSpeed;
            }
        }
        else if (stamina < maxStamina)
        {
            //ダッシュしていないときはスタミナを回復
            stamina += staminaRecoveryRatio * Time.deltaTime;  
        }
        else if (maxStamina <= stamina)
        {
            //スタミナ復活でダッシュ可能
            stamina = maxStamina;
            isStamina = true;
        }

        if (staminaSlider)
        {
            //スライダーに値を反映
            staminaSlider.value = stamina;
        }
    }

    //Ctrl操作切り替え
    public bool PlayerIsBackRotate() 
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public void PlayerTurn() 
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.Story)
        {
            //プレイヤーの向きを180度ゆっくり回転させる
            if (transform.rotation.y < 0) transform.Rotate(new Vector3(0, 180f, 0) * (Time.deltaTime * 0.5f));
            else playerIsBackRotate = false;
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame)
        {
            //プレイヤーの向きを180度回転させる
            transform.Rotate(0, 180, 0);
        } 
    }

    public void PlayerWarp(float x, float y, float z) 
    {
        // CharacterControllerを一時的に無効にする
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // transform.positionを直接設定してワープする場合、一時的にCharacterControllerを一時的に無効にする必要がある
        transform.position = new Vector3(x, y, z);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // CharacterControllerを再度有効にする
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}
