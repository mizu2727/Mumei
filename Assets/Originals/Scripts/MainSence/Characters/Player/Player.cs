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

    [SerializeField] string aliasName = "�C�t";//���̖��O

    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//�ړ�����

    public bool isDebug = false;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log($"[Player] �V���O���g��������: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[Player] �d���C���X�^���X��j��: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    public bool IsPlayerMoving()
    {
        // �ړ����͂����邩�ǂ����𔻒�
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

        //�R���|�[�l���g�̎擾
        characterController = GetComponent<CharacterController>();
        PlayAnimator = GetComponent<Animator>();

        //�v���C���[�̏����ʒu
        playerStartPosition = transform.position;
    }


    private void Update()
    {
        if (isDebug && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("�v���C���[���S(�f�o�b�O���[�h)");
            Dead();
        }

        if (isDebug && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("�N���A(�f�o�b�O���[�h)");
            SceneManager.LoadScene("GameClearScene");
        }


        if (playerIsDead) return;


        // �ړ����x���擾�B��Shift�L�[����͂��Ă���Ԃ̓_�b�V��
        float speed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : NormalSpeed;

        // �O�㍶�E�̓��͂���A�ړ��̂��߂̃x�N�g�����v�Z
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        //�ړ��������v�Z
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // isGrounded �͒n�ʂɂ��邩�ǂ����𔻒肷��
        if (characterController.isGrounded)
        {
            moveDirection = move * speed;
        }
        else
        {
            // �d�͂���������
            moveDirection = move + new Vector3(0, moveDirection.y, 0);
            moveDirection.y -= Gravity * Time.deltaTime;
        }

        // Move �͎w�肵���x�N�g�������ړ������閽��
        characterController.Move(moveDirection * Time.deltaTime);


        IsMove = IsPlayerMoving();


        // �A�j���[�V�����̐���
        if (IsMove)
        {
            // �ړ���: LeftShift�ɉ����đ��s�܂��͕��s
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift));
        }
        else
        {
            // ��~��: �����̃A�j���[�V�������I�t
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }
    }
}
