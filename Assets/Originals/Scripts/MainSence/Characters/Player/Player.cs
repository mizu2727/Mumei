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
    [SerializeField] const  float maxStamina = 100f;

    //�X�^�~�i�̌��ݒl
    float stamina;

    [Header("�X�^�~�i����l")]
    [SerializeField] public float staminaConsumeRatio = 50f;

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
        // ���S�t���O�𗧂Ă�
        IsDead = true;

        // �ړ����������Z�b�g
        moveDirection = Vector3.zero;

        // �A�j���[�V�������~
        if (animator != null)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }

        // ���ʉ����~
        if (audioSourceSE != null)
        {
            audioSourceSE.Stop();
        }

        GameController.instance.ViewGameOver();
        //SceneManager.LoadScene("GameOverScene");
        DestroyPlayer();
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

    
    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("�T�E���h�֘A")]
    public AudioSource audioSourceSE; // �v���C���[��p��AudioSource

    // ���ʉ���ID�iSO_SE �� seList �ɑΉ��j
    private readonly int walkSEid = 0; // ���s����ID
    private readonly int runSEid = 1;  // �_�b�V������ID

    [Header("���ݍĐ����̌��ʉ�")]
    public AudioClip currentSE;

    private bool wasMovingLastFrame = false; // �O�t���[���̈ړ���Ԃ�ێ�


    [Header("�v���C���[���|��Ă��邩�𔻒�")]
    public bool isFallDown = false;

    [Header("�f�o�b�O���[�h")]
    public bool isDebug = false;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyPlayer();
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

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ肷�邽�߂̃C�x���g�o�^����
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();

        // GameController ���� staminaSlider ���Ď擾
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = maxStamina;
                staminaSlider.value = stamina;
            }
        }

        // Stage01Scene�ɑJ�ڂ����ۂɈʒu�����Z�b�g
        if (scene.name == "Stage01Scene")
        {
            // CharacterController���ꎞ�I�ɖ����ɂ��Ĉʒu�����Z�b�g
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            transform.position = playerStartPosition;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            moveDirection = Vector3.zero; // �ړ����������Z�b�g
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            // �v���C���[�̏�Ԃ�������
            IsDead = false;
            playerHP = 1; // HP�������l�Ƀ��Z�b�g
            stamina = maxStamina; // �X�^�~�i�����Z�b�g
            isStamina = true;
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

        // AudioSource�̏�����
        InitializeAudioSource();

        //�X�^�~�i�ő�l�̏�����
        stamina = maxStamina;

        // GameController����staminaSlider���擾
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
        }

        // �X�^�~�iSlider�̍ő�l��ݒ�
        if (staminaSlider)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = stamina;
        }

        isStamina = true;

        playerIsBackRotate = false;
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {
        //��p�̐V����AudioSource���擾
        //(�ʂ̌��ʉ������Ă���ԂɃh�G�̌��ʉ�����Ȃ��o�O��h�~����p)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }
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

        if (playerIsDead || Time.timeScale == 0 || isFallDown || GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) 
        {
            //�ړ����������Z�b�g
            moveDirection = Vector3.zero;
            return;
        } 

        //PauseController���Ȃ�Scene��null�`�F�b�N�G���[��������邽�߂ɁA�ʂł킯�Ă���
        if (PauseController.instance.isPause) return;


        //�_�b�V������
        PlayerDashOrWalk();

        // �O�㍶�E�̓��͂���A�ړ��̂��߂̃x�N�g�����v�Z
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // �J������Transform���擾
        Transform cameraTransform = Camera.main.transform;

        // �J������forward�x�N�g����XZ���ʂɓ��e
        Vector3 cameraForwardXZ = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;


        //�ړ��������v�Z
        Vector3 move;


        if (playerIsBackRotate)
        {
            // �v���C���[�̑̂̌�������Ɉړ��������v�Z
            Vector3 playerForwardXZ = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            Vector3 playerRightXZ = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

            // �v���C���[�̑O��/����imoveZ�j�ƉE/���imoveX�j����Ɉړ��x�N�g�����v�Z
            move = playerRightXZ * moveX + playerForwardXZ * moveZ;
        }
        else
        {
            // �ʏ펞�i�J�����̌����ɉ����Ĉړ��j
            move = cameraRightXZ * moveX + cameraForwardXZ * moveZ;
        }

        // isGrounded �͒n�ʂɂ��邩�ǂ����𔻒肷��
        if (characterController.isGrounded)
        {
            moveDirection = move * speed;

            // �ڒn����Y�����̈ړ������Z�b�g
            moveDirection.y = 0f;
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


        // audioSourceSE�̏�Ԃ��m�F
        if (audioSourceSE == null || !audioSourceSE)
        {
            Debug.LogWarning("audioSourceSE��null�܂���Missing�ł��B�Đݒ�����݂܂��B");
            InitializeAudioSource();
            if (audioSourceSE == null)
            {
                Debug.LogError("audioSourceSE���Đݒ�ł��܂���ł����B");
                return;
            }
        }


        // �ړ���Ԃ̕ω������m���Č��ʉ��𐧌�
        currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash") ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);



        if (IsMove && !wasMovingLastFrame)
        {
            // �ړ��J�n���Ɍ��ʉ����Đ�
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            // �ړ���~���Ɍ��ʉ����~
            audioSourceSE.Stop();
        }
        else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
        {
            // �ړ����ɕ��s/�_�b�V�����؂�ւ�����ꍇ�A���ʉ���ύX
            audioSourceSE.Stop();
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }

        // ���݂̈ړ���Ԃ��L�^
        wasMovingLastFrame = IsMove;


        //���������Ă��邩�𔻒�
        playerIsBackRotate = PlayerIsBackRotate();

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
    }

    /// <summary>
    /// �v���C���[���[�v����
    /// </summary>
    /// <param name="x">X���W</param>
    /// <param name="y">Y���W</param>
    /// <param name="z">Z���W</param>
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

    /// <summary>
    /// �v���C���[�폜����
    /// </summary>
    public void DestroyPlayer() 
    {
        Debug.Log("Player���폜���܂���");
        Destroy(gameObject);
    }
}
