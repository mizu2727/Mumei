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
    /// �C���X�^���X
    /// </summary>
    public static Player instance { get; private set; }

    /// <summary>
    /// �L�����N�^�[�R���g���[���[
    /// </summary>
    CharacterController characterController;

    /// <summary>
    /// �A�j���[�V����
    /// </summary>
    private Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }

    [Header("���O(�q�G�����L�[��ł̕ҏW�֎~)")]

    [SerializeField]
    public string CharacterName
    {
        get => GameController.playerName; 
        set => GameController.playerName = value;

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

    /// <summary>
    /// �ړ����x�̌��ݒl
    /// </summary>
    float speed;

    [Header("�X�^�~�iSlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider staminaSlider;

    [Header("�X�^�~�i�ő�l")]
    [SerializeField] const  float maxStamina = 100f;

    /// <summary>
    /// �X�^�~�i�̌��ݒl
    /// </summary>
    float stamina;

    [Header("�X�^�~�i����l")]
    [SerializeField] public float staminaConsumeRatio = 50f;

    [Header("�X�^�~�i�񕜒l")]
    [SerializeField] float staminaRecoveryRatio = 20f;

    /// <summary>
    /// �X�^�~�i�g�p�\�t���O
    /// </summary>
    bool isStamina;

    [Header("���m�͈�")]
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

    [Header("���S�t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] private bool playerIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => playerIsDead;
        set => playerIsDead = value;
    }

    [Header("�v���C���[�ړ��t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] private bool playerIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => playerIsMove;
        set => playerIsMove = value;
    }

    [Header("�v���C���[�_�b�V���t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] private bool playerIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => playerIsDash;
        set => playerIsDash = value;
    }

    [Header("���������t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] public bool playerIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => playerIsBackRotate;
        set => playerIsBackRotate = value;
    }

    [Header("���C�g�؂�ւ��t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] private bool playerIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => playerIsLight;
        set => playerIsLight = value;
    }

    /// <summary>
    /// ���S
    /// </summary>
    public void Dead()
    {
        IsDead = true;

        //�ړ����������Z�b�g
        moveDirection = Vector3.zero;

        //�A�j���[�V�������~
        if (animator != null)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }

        //���ʉ����~
        if (audioSourceSE != null)
        {
            audioSourceSE.Stop();
        }

        //�Q�[���I�[�o�[��ʂ֑J��
        GameController.instance.ViewGameOver();

        //�v���C���[�폜
        DestroyPlayer();
    }

    /// <summary>
    /// �U��
    /// </summary>
    public void Attack()
    {
        Debug.Log("Player Attack!");
    }

    [Header("�����ʒu")]
    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => playerStartPosition;
        set => playerStartPosition = value;
    }

    /// <summary>
    /// ���̖��O
    /// </summary>
    [SerializeField] const string aliasName = "�C�t";

    [Header("���̏����t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    [SerializeField] public bool isHoldKey = false;

    /// <summary>
    /// �ړ�����
    /// </summary>
    Vector3 moveDirection = Vector3.zero;

    
    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("���s���E�_�b�V�����p��audioSource(�q�G�����L�[��ł̕ҏW�֎~)")]
    public AudioSource audioSourceSE;

    /// <summary>
    /// ���s����ID
    /// </summary>
    private readonly int walkSEid = 0;

    /// <summary>
    /// �_�b�V������ID
    /// </summary>
    private readonly int runSEid = 1; 

    [Header("���ݍĐ����̌��ʉ�(�q�G�����L�[��ł̕ҏW�֎~)")]
    public AudioClip currentSE;

    /// <summary>
    /// �O�t���[���̈ړ���ԃt���O
    /// </summary>
    private bool wasMovingLastFrame = false;


    [Header("�v���C���[���|��Ă���t���O(�q�G�����L�[��ł̕ҏW�֎~)")]
    public bool isFallDown = false;

    [Header("�f�o�b�O���[�h")]
    public bool isDebug = false;


    [Header("�^�O�E���C���[�֘A")]
    [SerializeField] string doorTag = "Door";
    [SerializeField] string doorPartsTag = "DoorParts";


    /// <summary>
    /// IgnoreObject
    /// </summary>
    private IgnoreObject ignoreObject;

    /// <summary>
    /// �Ώۂ̊J�������h�A
    /// </summary>
    GameObject gameObjectDoor;

    private void Awake()
    {
        //�C���X�^���X
        if (instance != null && instance != this)
        {
            DestroyPlayer();
            return;
        }
        instance = this;
    }

    /// <summary>
    /// �ړ����͂����邩�ǂ����𔻒�
    /// </summary>
    /// <returns>�ړ����Ă���Ȃ�true</returns>
    public bool IsPlayerMoving()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        return Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f;
    }

    /// <summary>
    /// �I�u�W�F�N�g���j�󂳂ꂽ�ۂɌĂ΂��֐�
    /// </summary>
    void OnDestroy()
    {
        //�C���X�^���X�j��
        if (instance == this)
        {
            instance = null;
        }
    }


    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;

    }

    private void OnDisable()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^����
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// �V�[���J�ڎ��ɐݒ�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSource��ݒ�
        InitializeAudioSource();

        //GameController ����staminaSlider���擾
        //��ʑJ�ڎ���staminaSlider�̐��l���ω����Ȃ��o�O��h���p
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
            if (staminaSlider != null)
            {
                staminaSlider.maxValue = maxStamina;
                staminaSlider.value = stamina;
            }
        }

        //Stage01Scene�ɑJ�ڂ����ۂɈʒu�����Z�b�g
        if (scene.name == "Stage01Scene")
        {
            //CharacterController���ꎞ�I�ɖ����ɂ��Ĉʒu�����Z�b�g
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

            //�v���C���[�̏�Ԃ�������
            IsDead = false;
            playerHP = 1;
            stamina = maxStamina;
            isStamina = true;
        }
    }

    /// <summary>
    /// SE���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
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

        //�R���|�[�l���g�̎擾
        characterController = GetComponent<CharacterController>();
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        PlayAnimator = GetComponent<Animator>();

        if (capsuleCollider != null && characterController != null)
        {
            //CharacterController��Radius��CapsuleCollider��Radius��菬�����ݒ�i��: 0.9�{�j
            characterController.radius = capsuleCollider.radius * 0.9f;
        }
        else
        {
            Debug.LogWarning("CapsuleCollider�܂���CharacterController��������܂���B");
        }

        //�v���C���[�̏����ʒu
        playerStartPosition = transform.position;

        //AudioSource�̏�����
        InitializeAudioSource();

        //�X�^�~�i�ő�l�̏�����
        stamina = maxStamina;

        //GameController����staminaSlider���擾
        if (GameController.instance != null)
        {
            staminaSlider = GameController.instance.staminaSlider;
        }

        //�X�^�~�iSlider�̍ő�l��ݒ�
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
        //AudioSource���擾
        //(�ʂ̌��ʉ������Ă���ԂɓG�̌��ʉ�����Ȃ��o�O��h�~����p)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        //MusicController�Őݒ肳��Ă���SE�p��AudioMixerGroup��ݒ肷��
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    private void Update()
    {
        //�f�o�b�O����Q�L�[��������Ǝ��S����
        if (isDebug && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("�v���C���[���S(�f�o�b�O���[�h)");
            Dead();
        }

        //�f�o�b�O����C�L�[��������ƃN���A��ʂ֑J�ڂ���
        if (isDebug && Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("�N���A(�f�o�b�O���[�h)");
            SceneManager.LoadScene("GameClearScene");
        }

        //�X�g�[���[���[�h�Ńv���C���[����]������
        if (playerIsBackRotate && (GameController.instance.gameModeStatus == GameModeStatus.Story)) PlayerTurn();

        //�ʏ�̃v���C�ȊO�̏ꍇ�A�������X�L�b�v
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

        //�O�㍶�E�̓��͂���A�ړ��̂��߂̃x�N�g�����v�Z
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

        //Move�͎w�肵���x�N�g�������ړ������閽��
        characterController.Move(moveDirection * Time.deltaTime);

        //�ړ�����
        IsMove = IsPlayerMoving();

        //�X�^�~�i�Ǘ�
        PlayerStaminaManage();


        //�A�j���[�V�����̐���
        if (IsMove)
        {
            //�ړ���:Shift�ɉ����đ��s�܂��͕��s����
            animator.SetBool("isRun", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));
            animator.SetBool("isWalk", !Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash"));

        }
        else
        {
            //��~��:�����̃A�j���[�V�������I�t
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);

        }


        //audioSourceSE�̏�Ԃ��m�F
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


        //�ړ���Ԃ̕ω������m���Č��ʉ��𐧌�
        currentSE = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetButton("Dash") ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);



        if (IsMove && !wasMovingLastFrame)
        {
            //�ړ��J�n���Ɍ��ʉ����Đ�
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }
        else if (!IsMove && wasMovingLastFrame)
        {
            //�ړ���~���Ɍ��ʉ����~
            audioSourceSE.Stop();
        }
        else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
        {
            //�ړ����ɕ��s/�_�b�V�����؂�ւ�����ꍇ�A���ʉ���ύX
            audioSourceSE.Stop();
            audioSourceSE.clip = currentSE;
            audioSourceSE.loop = true;
            audioSourceSE.Play();
        }

        //���݂̈ړ���Ԃ��L�^
        wasMovingLastFrame = IsMove;


        //���������Ă��邩�𔻒�
        playerIsBackRotate = PlayerIsBackRotate();

    }


    /// <summary>
    /// �_�b�V������
    /// </summary>
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

    /// <summary>
    /// �X�^�~�i�Ǘ�
    /// </summary>
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

                //�����ŃX�s�[�h�𗎂Ƃ�
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

    /// <summary>
    /// �U��Ԃ蔻��
    /// </summary>
    /// <returns>Ctrl������true</returns>
    public bool PlayerIsBackRotate() 
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    /// <summary>
    /// �v���C���[��]
    /// </summary>
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
        //CharacterController���ꎞ�I�ɖ����ɂ���
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        //transform.position�𒼐ڐݒ肵�ă��[�v����ꍇ�A�ꎞ�I��CharacterController���ꎞ�I�ɖ����ɂ���K�v������
        transform.position = new Vector3(x, y, z);
        transform.rotation = Quaternion.Euler(0, 0, 0);

        //CharacterController���ēx�L���ɂ���
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
        Destroy(gameObject);
    }


    /// <summary>
    /// �I�u�W�F�N�g�̃R���C�_�[���ђʂ����ꍇ�̏���
    /// </summary>
    /// <param name="collider">�R���C�_�[</param>
    private void OnTriggerEnter(Collider collider)
    {
        //DoorTag�̃I�u�W�F�N�g���G�ꂽ�ꍇ
        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;

            //�q�I�u�W�F�N�g�̃R���|�[�l���g���擾
            ignoreObject = gameObjectDoor.GetComponentInChildren<IgnoreObject>();

            //�q�I�u�W�F�N�g�̃^�O��doorPartsTag�̏ꍇ
            if (ignoreObject.CompareTag(doorPartsTag)) 
            {
                //�R���C�_�[�𖳌���
                ignoreObject.GetMeshCollider().enabled = false;
            }
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g�̃R���C�_�[���痣�ꂽ�ꍇ�̏���
    /// </summary>
    /// <param name="collider">�R���C�_�[</param>
    private void OnTriggerExit(Collider collider)
    {
        //DoorTag�̃I�u�W�F�N�g���痣�ꂽ�ꍇ
        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;

            //�q�I�u�W�F�N�g�̃R���|�[�l���g���擾
            ignoreObject = gameObjectDoor.GetComponentInChildren<IgnoreObject>();

            //�q�I�u�W�F�N�g�̃^�O��doorPartsTag�̏ꍇ
            if (ignoreObject.CompareTag(doorPartsTag))
            {
                //�R���C�_�[��L����
                ignoreObject.GetMeshCollider().enabled = true;
            }
        }
    }
}
