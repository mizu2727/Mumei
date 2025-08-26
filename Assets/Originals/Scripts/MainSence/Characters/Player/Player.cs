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

    [Header("���O")]
    //[SerializeField] public string playerName;

    [SerializeField]
    public string CharacterName
    {
        get => GameController.playerName; 
        set => GameController.playerName = value;

        //get => playerName;
        //set => playerName = value;
    }

    [Header("���s���x")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField]
    public float NormalSpeed
    {
        get => walkSpeed;
        set => walkSpeed = value;
    }

    [Header("�_�b�V�����x")]
    [SerializeField] private float dashSpeed = 5f;
    [SerializeField]
    public float SprintSpeed
    {
        get => dashSpeed;
        set => dashSpeed = value;
    }

    //�ړ����x�̌��ݒl
    float speed;

    [Header("�X�^�~�iSlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider staminaSlider;

    [Header("�X�^�~�i�ő�l")]
    [SerializeField] float maxStamina = 100f;

    //�X�^�~�i�̌��ݒl
    float stamina;

    [Header("�X�^�~�i����l")]
    [SerializeField] float staminaConsumeRatio = 50f;

    [Header("�X�^�~�i�񕜒l")]
    [SerializeField] float staminaRecoveryRatio = 20f;

    //�X�^�~�i�̎g�p���\�ł��邩�𔻒�
    bool isStamina;

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

    [Header("�v���C���[���_�b�V�����Ă��邩�𔻒�")]
    [SerializeField] private bool playerIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => playerIsDash;
        set => playerIsDash = value;
    }

    [Header("�v���C���[�����������Ă��邩�𔻒�")]
    [SerializeField] public bool playerIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => playerIsBackRotate;
        set => playerIsBackRotate = value;
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
    [SerializeField] const string aliasName = "�C�t";

    [Header("���̏����̗L��")]
    [SerializeField] public bool isHoldKey = false;

    Vector3 moveDirection = Vector3.zero;//�ړ�����

    [Header("�T�E���h�֘A")]
    public AudioSource audioSourceSE; // �v���C���[��p��AudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    public AudioClip currentSE;//���ݍĐ����̌��ʉ�

    private bool wasMovingLastFrame = false; // �O�t���[���̈ړ���Ԃ�ێ�


    [Header("�v���C���[���|��Ă��邩�𔻒�")]
    public bool isFallDown = false;

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

        //�X�^�~�i���ݒl�̏�����
        stamina = maxStamina;

        //�X�^�~�iSliderSlider�̍ő�l��ݒ�
        if (staminaSlider) staminaSlider.maxValue = maxStamina;

        isStamina = true;

        playerIsBackRotate = false;
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

        if (playerIsBackRotate && (GameController.instance.gameModeStatus == GameModeStatus.Story)) PlayerTurn();

        if (playerIsDead  || Time.timeScale == 0 || isFallDown || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) return;

        //PauseController���Ȃ�Scene��null�`�F�b�N�G���[��������邽�߂ɁA�ʂł킯�Ă���
        if (PauseController.instance.isPause) return;


        //�_�b�V������
        PlayerDashOrWalk();

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

        //�X�^�~�i�Ǘ�
        PlayerStaminaManage();


        // �A�j���[�V�����̐���
        if (IsMove)
        {
            // �ړ���: LeftShift�ɉ����đ��s�܂��͕��s
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));

        }
        else
        {
            // ��~��: �����̃A�j���[�V�������I�t
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        // �ړ���Ԃ̕ω������m���Č��ʉ��𐧌�
        currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash") ? runSE : walkSE;



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


        //���������Ă��邩�𔻒�
        if (PlayerIsBackRotate()) playerIsBackRotate = true;
        else playerIsBackRotate = false;

    }

    //�_�b�V������
    void PlayerDashOrWalk() 
    {
        //Shift�L�[�EX�{�^������͂��Ă���Ԃ̓_�b�V��
        //Dash�c"joystick button 4"�����蓖�āB�R���g���[���[�ł�L�{�^���ɂȂ�
        if (IsMove && isStamina && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash")))
        {
            //�_�b�V���J�n
            IsDash = true;

            speed = SprintSpeed;

        }
        else
        {
            //�_�b�V���I��
            IsDash = false;

            speed = NormalSpeed;
        }
    }

    //�X�^�~�i�Ǘ�
    void PlayerStaminaManage() 
    {
        if (IsDash)
        {
            if (0 < stamina)
            {
                //�_�b�V�����̓X�^�~�i������
                stamina -= staminaConsumeRatio * Time.deltaTime;
            }
            else
            {
                IsDash = false;

                // �����ŃX�s�[�h�𗎂Ƃ�

                //�X�^�~�i�؂�Ń_�b�V���s��
                isStamina = false;

                speed = NormalSpeed;
            }
        }
        else if (stamina < maxStamina)
        {
            //�_�b�V�����Ă��Ȃ��Ƃ��̓X�^�~�i����
            stamina += staminaRecoveryRatio * Time.deltaTime;  
        }
        else if (maxStamina <= stamina)
        {
            //�X�^�~�i�����Ń_�b�V���\
            stamina = maxStamina;
            isStamina = true;
        }

        if (staminaSlider)
        {
            //�X���C�_�[�ɒl�𔽉f
            staminaSlider.value = stamina;
        }
    }

    //Ctrl����؂�ւ�
    public bool PlayerIsBackRotate() 
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public void PlayerTurn() 
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.Story)
        {
            //�v���C���[�̌�����180�x��������]������
            if (transform.rotation.y < 0) transform.Rotate(new Vector3(0, 180f, 0) * (Time.deltaTime * 0.5f));
            else playerIsBackRotate = false;
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame)
        {
            //�v���C���[�̌�����180�x��]������
            transform.Rotate(0, 180, 0);
        } 
    }

    public void PlayerWarp(float x, float y, float z) 
    {
        // CharacterController���ꎞ�I�ɖ����ɂ���
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        // transform.position�𒼐ڐݒ肵�ă��[�v����ꍇ�A�ꎞ�I��CharacterController���ꎞ�I�ɖ����ɂ���K�v������
        transform.position = new Vector3(x, y, z);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // CharacterController���ēx�L���ɂ���
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}
