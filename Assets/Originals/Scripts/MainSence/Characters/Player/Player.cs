using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, CharacterInterface
{

    CharacterController characterConttroller;

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

    [SerializeField] private float Speed  = 3f;
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

    private void Start()
    {
        IsDead = false;

        //コンポーネントの取得
        characterConttroller = GetComponent<CharacterController>();
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
        if (characterConttroller.isGrounded)
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
        characterConttroller.Move(moveDirection  * Time.deltaTime);

        // 移動のアニメーション
        //animator.SetFloat("MoveSpeed", move.magnitude);

        
    }
}
