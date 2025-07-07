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

    [Header("���O")]
    [SerializeField] private string playerName;

    [SerializeField]
    public string CharacterName
    {
        get => playerName;
        set => playerName = value;
    }

    [Header("���s���x")]
    [SerializeField] private float Speed = 3f;
    [SerializeField]
    public float NormalSpeed
    {
        get => Speed;
        set => Speed = value;
    }

    [Header("�_�b�V�����x")]
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

    [Header("�d��")]
    [SerializeField] private float playerGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => playerGravity;
        set => playerGravity = value;
    }

    [Header("�̗�")]
    [SerializeField] private int playerHP = 1;
    [SerializeField]
    public int HP
    {
        get => playerHP;
        set => playerHP = value;
    }

    [Header("���S����")]
    [SerializeField] private bool playerIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => playerIsDead;
        set => playerIsDead = value;
    }

    [Header("�v���C���[�������Ă��邩�𔻒�")]
    [SerializeField] private bool playerIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => playerIsMove;
        set => playerIsMove = value;
    }

    [Header("���C�g�؂�ւ�")]
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

    [Header("���̖��O")]
    [SerializeField] string aliasName = "�C�t";

    [Header("���̏����̗L��")]
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//�ړ�����

    [Header("�T�E���h�֘A")]
    private AudioSource audioSourceSE; // �v���C���[��p��AudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;

    private bool wasMovingLastFrame = false; // �O�t���[���̈ړ���Ԃ�ێ�

    [Header("�f�o�b�O���[�h")]
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
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        PlayAnimator = GetComponent<Animator>();

        if (capsuleCollider != null && characterController != null)
        {
            // CharacterController��Radius��CapsuleCollider��Radius��菬�����ݒ�i��: 0.9�{�j
            characterController.radius = capsuleCollider.radius * 0.9f;
        }
        else
        {
            Debug.LogWarning("CapsuleCollider�܂���CharacterController��������܂���B");
        }

        //�v���C���[�̏����ʒu
        playerStartPosition = transform.position;

        // MusicController����AudioSource���擾
        audioSourceSE = MusicController.Instance.GetAudioSource();
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


        // �ړ����x���擾�BShift�L�[����͂��Ă���Ԃ̓_�b�V��
        float speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? SprintSpeed : NormalSpeed;

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
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        }
        else
        {
            // ��~��: �����̃A�j���[�V�������I�t
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        // �ړ���Ԃ̕ω������m���Č��ʉ��𐧌�
        AudioClip currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? runSE : walkSE;

        if (IsMove && !wasMovingLastFrame)
        {
            // �ړ��J�n���Ɍ��ʉ����Đ�
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            // �ړ���~���Ɍ��ʉ����~
            MusicController.Instance.StopSE(audioSourceSE);
        }
        else if (IsMove && wasMovingLastFrame && MusicController.Instance.IsPlayingSE(audioSourceSE)
                 && MusicController.Instance.GetCurrentSE(audioSourceSE) != currentSE)
        {
            // �ړ����ɕ��s/�_�b�V�����؂�ւ�����ꍇ�A���ʉ���ύX
            MusicController.Instance.StopSE(audioSourceSE);
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }

        // ���݂̈ړ���Ԃ��L�^
        wasMovingLastFrame = IsMove;

    }
}
