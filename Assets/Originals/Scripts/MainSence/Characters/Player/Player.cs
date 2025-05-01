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

    [SerializeField] private string playerName;

    [SerializeField]
    public string CharacterName
    {
        get => playerName;
        set => playerName = value;
    }

    [SerializeField] private float Speed = 3f;
    [SerializeField]
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }

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

    [SerializeField] private float playerGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => playerGravity;
        set => playerGravity = value;
    }

    [SerializeField] private int playerHP = 1;
    [SerializeField]
    public int HP
    {
        get => playerHP;
        set => playerHP = value;
    }

    [SerializeField] private bool playerIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => playerIsDead;
        set => playerIsDead = value;
    }

    [SerializeField] private bool playerIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => playerIsMove;
        set => playerIsMove = value;
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

    [SerializeField] string aliasName = "イフ";//仮の名前

    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//移動方向

    public bool isDebug = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log($"[Player] シングルトン初期化: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Player] 重複インスタンスを破棄: {gameObject.name}");
            Destroy(gameObject);
        }
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
        PlayAnimator = GetComponent<Animator>();

        //プレイヤーの初期位置
        playerStartPosition = transform.position;
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


        // 移動速度を取得。左Shiftキーを入力している間はダッシュ
        float speed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : NormalSpeed;

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
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift));
        }
        else
        {
            // 停止中: 両方のアニメーションをオフ
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }
    }
}
