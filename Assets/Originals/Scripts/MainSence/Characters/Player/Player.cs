using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

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
    [SerializeField] private string playerName;

    [SerializeField]
    public string CharacterName
    {
        get => playerName;
        set => playerName = value;
    }

    [Header("歩行速度")]
    [SerializeField] private float Speed = 3f;
    [SerializeField]
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }

    [Header("ダッシュ速度")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField]
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }

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
    [SerializeField] string aliasName = "イフ";

    [Header("鍵の所持の有無")]
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//移動方向

    [Header("サウンド関連")]
    private AudioSource audioSourceSE; // プレイヤー専用のAudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;

    private bool wasMovingLastFrame = false; // 前フレームの移動状態を保持

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


        if (playerIsDead) return;


        // 移動速度を取得。Shiftキーを入力している間はダッシュ
        float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? SprintSpeed : NormalSpeed;

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


        // アニメーションの制御
        if (IsMove)
        {
            // 移動中: LeftShiftに応じて走行または歩行
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        }
        else
        {
            // 停止中: 両方のアニメーションをオフ
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        // 移動状態の変化を検知して効果音を制御
        AudioClip currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? runSE : walkSE;

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

    }
}
