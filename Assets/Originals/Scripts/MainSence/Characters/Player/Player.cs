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

    [SerializeField] string aliasName = "�C�t";//���̖��O

    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//�ړ�����

    public bool isDebug = false;

    private void Start()
    {
        IsDead = false;

        //�R���|�[�l���g�̎擾
        characterConttroller = GetComponent<CharacterController>();
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
        if (characterConttroller.isGrounded)
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
        characterConttroller.Move(moveDirection  * Time.deltaTime);

        // �ړ��̃A�j���[�V����
        //animator.SetFloat("MoveSpeed", move.magnitude);

        
    }
}
