using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static GameController;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    [Header("�G�̃X�e�[�^�X")]
    public Animator animator;
    public Animator PlayAnimator
    {
        get => animator;
        set => animator = value;
    }


    [SerializeField] private string enemyName;

    [SerializeField]
    public string CharacterName
    {
        get => enemyName;
        set => enemyName = value;
    }


    [SerializeField] private float Speed = 4f;
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

    
    [SerializeField]
    public float DetectionRange
    {
        get => enemyDetectionRange;
        set => enemyDetectionRange = value;
    }

    [SerializeField] private float enemyGravity = 10f;
    [SerializeField]
    public float Gravity
    {
        get => enemyGravity;
        set => enemyGravity = value;
    }

    [SerializeField] private int enemyHP = 1;
    [SerializeField]
    public int HP
    {
        get => enemyHP;
        set => enemyHP = value;
    }

    [SerializeField] private bool enemyIsDead = false;
    [SerializeField]
    public bool IsDead
    {
        get => enemyIsDead;
        set => enemyIsDead = value;
    }

    [SerializeField] private bool enemyIsMove = true;
    [SerializeField]
    public bool IsMove
    {
        get => enemyIsMove;
        set => enemyIsMove = value;
    }

    [SerializeField] private bool enemyIsDash = true;
    [SerializeField]
    public bool IsDash
    {
        get => enemyIsDash;
        set => enemyIsDash = value;
    }

    [SerializeField] private bool enemyIsBackRotate = false;
    [SerializeField]
    public bool IsBackRotate
    {
        get => enemyIsBackRotate;
        set => enemyIsBackRotate = value;
    }

    [SerializeField] private bool enemyIsLight = true;
    [SerializeField]
    public bool IsLight
    {
        get => enemyIsLight;
        set => enemyIsLight = value;
    }

    public void Dead()
    {
        Debug.Log("Enemy Dead");
    }


    //�U��
    public void Attack()
    {
        if (Player.instance.IsDead) return;

        Player.instance.HP -= 1;

        if (Player.instance.HP <= 0) Player.instance.Dead();

    }


    [SerializeField] private Vector3 enemyStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => enemyStartPosition;
        set => enemyStartPosition = value;
    }


    private Vector3 lastCollisionPoint;

   
    public enum EnemyState
    {
        Patrol,      // �ʏ�p�j
        Alert,       // �x���i�v���C���[�����A�����Ȃ��j
        Chase,       // �Ǐ]�i�v���C���[���F�j
        Investigate  // �����i�v���C���[�����������ʒu�Ɍ������j
    }

    [Header("EnemyState(�q�G�����L�[��ł̕ҏW�֎~)")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("�v���C���[�̍Ō�̊��m�̈ʒu(�q�G�����L�[��ł̕ҏW�֎~)")]
    public Vector3 lastKnownPlayerPosition;

    private float investigateTimer = 0f; // �������ԃJ�E���^�[
    private float investigateDuration = 5f; // �������鎞�ԁi�b�j



    [Header("NavMesh�֘A(�q�G�����L�[��ł̕ҏW�֎~)")]
    public NavMeshAgent navMeshAgent;

    [Header("�p�j�n�_��������͈�(���̒l����������Ɯp�j�n�_�������炸�A�L�������NaveMesh�͈̔͊O�ɂȂ邽�߁A�v�������K�v)")]
    [SerializeField] private float findPatrolPointRange = 10f;

    [Header("�G�̌��m�͈�")]
    [SerializeField] private float enemyDetectionRange = 100f;

    [Header("�x���͈�(�v���C���[�Ƃ̋���)")]
    [SerializeField] public float alertRange = 15f;


    [Header("�p�j�֘A")]

    [Header("(�v���n�u�������I�u�W�F�N�g���A�^�b�`���邱��)")]
    [SerializeField] private TestMap01 testMap01;

    [Header("�p�j�n�_��Transform�z��")]
    [SerializeField] public Transform[] patrolPoint;
    private int positionNumber = 0;
    private int maxPositionNumber;


    [Header("���m�E�����֘A")]
    // 
    [Header("�Ǐ]�������I�u�W�F�N�g(�q�G�����L�[��̃v���C���[���A�^�b�`���邱��)")]
    [SerializeField] public Transform targetPoint;

    [Header("����p")]
    [SerializeField] private float fieldOfViewAngle = 60f;

    [Header("SphereCast�̋��̔��a")]
    [SerializeField] private float sphereCastRadius = 0.5f;

    [Header("���m�Ώۂ̃��C���[�iPlayer��ݒ肷�邱�Ɓj")]
    [SerializeField] private LayerMask detectionLayer;

    [Header("��Q���̃��C���[�iWall�Ȃǂ�ݒ肷�邱�Ɓj")]
    [SerializeField] private LayerMask obstacleLayer;


    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("�T�E���h�֘A")]
    public AudioSource audioSourceSE; 
    private readonly int walkSEid = 7; // ���s����ID
    private readonly int runSEid = 8;  // �_�b�V������ID
    public AudioSource audioSourceFindPlayerSE;
    private readonly int findPlayerSEid = 9;  // �_�b�V������ID

    [Header("���ݍĐ����̌��ʉ�")]
    public AudioClip currentSE;//���ݍĐ����̌��ʉ�

    [Header("���鉹�̍Đ����x(�v����)")]
    [SerializeField] private float runSEPitch = 2f;

    [Header("�T�E���h�̋����֘A(�v����)")]
    [SerializeField] private float maxSoundDistance = 10f; // ���ʂ��ő�ɂȂ鋗��
    [SerializeField] private float minSoundDistance = 20f; // ���ʂ��ŏ��ɂȂ鋗��
    [SerializeField] private float maxVolume = 1.0f; // �ő剹��
    [SerializeField] private float minVolume = 0.0f; // �ŏ�����


    [Header("�v���C���[�������̃p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public GameObject playerFoundPanel;




    private bool wasMovingLastFrame = false; // �O�t���[���̈ړ���Ԃ�ێ�

    [Header("�v���C���[��������ɂ��邩�𔻒�(�q�G�����L�[��ł̕ҏW�֎~)")]
    public bool isAlertMode = false;


    [Header("�^�O�E���C���[�֘A")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] string wallTag = "Wall";
    [SerializeField] string doorTag = "Door";


    private Door door;
    GameObject gameObjectDoor;

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
    /// 
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Player.instance��������܂���BtargetPoint��ݒ�ł��܂���B");
            navMeshAgent.isStopped = true;
        }
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {
        if (MusicController.Instance != null)
        {
            audioSourceSE = MusicController.Instance.GetAudioSource();
            if (audioSourceSE != null)
            {
                audioSourceSE.playOnAwake = false;
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] MusicController����AudioSource���擾�ł��܂���ł����B");
            }
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] MusicController.Instance��������܂���B");
        }

        audioSourceFindPlayerSE = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();

        //��p�̐V����AudioSource���擾
        //(�ʂ̌��ʉ������Ă���ԂɓG�̌��ʉ�����Ȃ��o�O��h�~����p)
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgent���A�^�b�`����Ă��܂���I");
            return;
        }

        //targetPoint��Player.instance��Transform�ɐݒ�
        //(�V�[���J�ڂ�����Ƀv���C���[��transform��null�ɂȂ�G���[��h�~����p)
        if (Player.instance != null)
        {
            targetPoint = Player.instance.transform;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Player.instance��������܂���BtargetPoint��ݒ�ł��܂���B");
            navMeshAgent.isStopped = true;
            return;
        }

        // NavMeshAgent�̏�����
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true; // ��]��NavMeshAgent�ɔC����
        navMeshAgent.angularSpeed = 360f; // ��]���x��K�؂ɐݒ�
        navMeshAgent.baseOffset = 0f; // ���f���ɍ��킹�Ē���

        // ���f���̉�]���������i�K�v�ɉ����āj
        transform.rotation = Quaternion.identity;

        // NavMesh��ɔz�u����Ă��邩�m�F
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[{gameObject.name}] NavMesh��ɂ���܂���B�ʒu��␳���܂��B");
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] NavMesh�ʒu�␳�Ɏ��s���܂����B�ʒu: {transform.position}");
            }
        }

        // �p�j�n�_�̏�����
        if ( patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] testMap01�܂���patrolPoint���ݒ肳��Ă��܂���I");
            return;
        }

        // patrolPoint�̊e�v�f������
        for (int i = 0; i < patrolPoint.Length; i++)
        {
            if (patrolPoint[i] == null)
            {
                Debug.LogError($"[{gameObject.name}] patrolPoint[{i}] ��null�ł��I��~���܂��B");
                navMeshAgent.isStopped = true;
                return;
            }
        }

        //�o�~�n�_�̏�����
        maxPositionNumber = patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] �����p�j�n�_: {patrolPoint[positionNumber].position}");
    }

    //�v���C���[��������ɂ��邩���`�F�b�N����
    public bool IsPlayerInFront()
    {

        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] targetPoint��null�ł��B");
            return false;
        }

        //�v���C���[�Ƃ̏����Ɗp�x���v�Z
        Vector3 directionToPlayer = targetPoint.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angle = Vector3.Angle(transform.forward, directionToPlayer.normalized);

        // �v���C���[������p�������m�͈͓��ɂ��邩
        if (distanceToPlayer <= enemyDetectionRange && angle <= fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // �����̊J�n�ʒu
            if (Physics.SphereCast(rayOrigin, sphereCastRadius, directionToPlayer.normalized, out hit, enemyDetectionRange, detectionLayer))
            {
                if (hit.collider.CompareTag(playerTag))
                {
                    // ��Q�����`�F�b�N
                    if (!Physics.Linecast(rayOrigin, targetPoint.position + Vector3.up * 1.0f, obstacleLayer))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;
    }

    //�v���C���[��Ǐ]����
    void ChasePlayer()
    {
        if (targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPoint��null�ł��B");
            navMeshAgent.isStopped = true;
            return;
        }

        //�v���C���[�̈ʒu���擾
        navMeshAgent.SetDestination(targetPoint.position);
        navMeshAgent.speed = dashSpeed;
        navMeshAgent.isStopped = false;
    }

    // ���̔o�~�n�_�����߂�
    void NextPosition()
    {
        if (patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPoint�������ł��I");
            navMeshAgent.isStopped = true;
            return;
        }

        //�����_���Ȕo�~�n�_��I��
        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = patrolPoint[positionNumber].position;
        NavMeshHit hit;

        //NavMesh��̈ʒu���m�F
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
        {
            navMeshAgent.destination = hit.position;
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] �ړI�n��NavMesh�͈̔͊O: {targetPos}");
            for (int i = 0; i < patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i + 1) % patrolPoint.Length;
                Vector3 fallbackPos = patrolPoint[nextIndex].position;
                if (NavMesh.SamplePosition(fallbackPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
                {
                    navMeshAgent.destination = hit.position;
                    positionNumber = nextIndex;
                    return;
                }
            }

            if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
            {
                navMeshAgent.destination = edgeHit.position;
                Debug.LogWarning($"[{gameObject.name}] �Ŋ���NavMesh�̒[�Ɉړ�: {edgeHit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] �L����NavMesh�ʒu��������܂���ł����B");
                navMeshAgent.isStopped = true;
            }
        }
    }


    public bool IsEnemyMoving()
    {
        // NavMeshAgent���L���ŁA�o�H�����݂��A��~���Ă��炸�A���x������ꍇ�Ɉړ����Ɣ���
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            return navMeshAgent.hasPath && !navMeshAgent.isStopped && navMeshAgent.velocity.magnitude > 0.1f;
        }
        return false;
    }

    // �Փ˔���Ə�����ǉ�
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[{gameObject.name}] �Փˌ��o: {collision.gameObject.name}, �^�O: {collision.gameObject.tag}");

        // �Փ˂����I�u�W�F�N�g�� "Wall" ���C���[�܂��� "Wall" �^�O�������Ă��邩�m�F
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag(wallTag))
        {
            navMeshAgent.velocity = Vector3.zero; // ���x��0�ɐݒ肵�Ē�~������
            navMeshAgent.isStopped = true; // NavMeshAgent�̈ړ����~
            animator.SetBool("isRun", false); // ��~�A�j���[�V�������Đ�
            animator.SetBool("isWalk", false);
            lastCollisionPoint = collision.contacts[0].point; // �Փ˒n�_���L�^
            Invoke("ChangeDirection", 0.5f); // 0.5�b��ɕ����]��
        }


        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"[{gameObject.name}] �v���C���[�ƏՓˁI HP���������J�n");
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }

        if (collision.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collision.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            Debug.Log("�G���h�A�ƏՓ�");
            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                door.OpenDoor();
            }
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log($"[{gameObject.name}] �Փˌ��o: {collider.gameObject.name}, �^�O: {collider.gameObject.tag},����2");

        if (collider.gameObject.CompareTag(playerTag))
        {
            Debug.Log($"[{gameObject.name}] �v���C���[�ƏՓ�2�I HP���������J�n");
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }

        if (collider.gameObject.CompareTag(doorTag))
        {
            gameObjectDoor = collider.gameObject;
            door = gameObjectDoor.GetComponent<Door>();

            if (door == null)
            {
                Debug.LogError($"[{gameObject.name}] �h�A�I�u�W�F�N�g({gameObjectDoor.name})��Door�R���|�[�l���g��������܂���I");
                return;
            }

            Debug.Log("�G���h�A�ƏՓ�2");
            if (!door.isNeedKeyDoor && !door.isOpenDoor)
            {
                door.OpenDoor();
            }
        }
    }

    //�����]��
    void ChangeDirection()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            // NavMeshAgent�̒�~������
            navMeshAgent.isStopped = false;

            // ���݈ʒu���班�����ꂽ�����_���ȕ���������
            Vector3 randomDirection = Random.insideUnitSphere.normalized * 3f;
            Vector3 newTarget = transform.position + randomDirection;

            //NavMesh��̈ʒu���m�F
            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                // ����ł�������Ȃ���΁A���݂̖ړI�n���Đݒ肵�Ă݂�
                if (navMeshAgent.path.corners.Length > 1)
                {
                    navMeshAgent.SetDestination(navMeshAgent.path.corners[navMeshAgent.path.corners.Length - 1]);
                    Debug.LogWarning($"[{gameObject.name}] �����_���Ȉړ��悪�����炸�A���݂̖ړI�n���Đݒ�");
                }
                else
                {
                    // ������Ȃ���΁A�ł��߂� NavMesh �̒[��T��
                    if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                    {
                        navMeshAgent.SetDestination(edgeHit.position);
                        Debug.LogWarning($"[{gameObject.name}] �Ŋ��� NavMesh �̒[�Ɉړ�");
                    }
                    else
                    {
                        Debug.LogError($"[{gameObject.name}] �L���ȉ��悪������܂���ł����B");
                    }
                }
            }
        }
    }

    protected virtual async void Update()
    {
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) return;

        if (Player.instance == null || Player.instance.IsDead  || targetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update�������X�L�b�v: Player={Player.instance}, tagetPoint={targetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        // �ړ������ǂ����𔻒�
        IsMove = IsEnemyMoving();

        // �v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, targetPoint.position);

        // ��ԑJ�ڂƏ���
        switch (currentState)
        {
            //�ʏ�p�j���
            case EnemyState.Patrol:
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                //�x�������~
                audioSourceFindPlayerSE.Stop();

                //�v���C���[��������ɂ��邩���`�F�b�N
                if (distance <= alertRange)
                {
                    if (IsPlayerInFront())
                    {
                        //�v���C���[��������ɂ���ꍇ�A�Ǐ]��ԂɈڍs
                        //��u�������s�����������������ɋL�ڂ���
                        currentState = EnemyState.Chase;
                        isAlertMode = true;
                        lastKnownPlayerPosition = targetPoint.position;

                        //�G�ɒǂ��Ă��鎞��BGM���Đ�
                        MusicController.Instance.audioClipnum = 0;
                        MusicController.Instance.StopBGM();
                        MusicController.Instance.audioClipnum = 1;
                        MusicController.Instance.PlayBGM();

                        //MusicController.Instance.StopBGM();
                        //EnemyBGMController.Instance.PlayEnemyBGM();

                        playerFoundPanel.SetActive(true);
                    }
                    else
                    {

                        //�v���C���[��������ɂ��邪�������Ȃ��ꍇ�A�x��������ԂɈڍs
                        currentState = EnemyState.Alert;
                        isAlertMode = true;
                        
                    }
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    NextPosition();
                }
                break;

            //�x���������
            case EnemyState.Alert:
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                Debug.Log("�x���������01_01");

                //�x�������Đ�
                audioSourceFindPlayerSE.clip = sO_SE.GetSEClip(findPlayerSEid);
                audioSourceFindPlayerSE.loop = true;
                audioSourceFindPlayerSE.Play();

                if (IsPlayerInFront())
                {
                    Debug.Log("�x���������01_02");

                    //�v���C���[��������ɂ���ꍇ�A�Ǐ]��ԂɈڍs
                    //��u�������s�����������������ɋL�ڂ���
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;

                    //�G�ɒǂ��Ă��鎞��BGM���Đ�
                    MusicController.Instance.audioClipnum = 0;
                    MusicController.Instance.StopBGM();
                    MusicController.Instance.audioClipnum = 1;
                    MusicController.Instance.PlayBGM();

                    //MusicController.Instance.StopBGM();
                    //EnemyBGMController.Instance.PlayEnemyBGM();

                    playerFoundPanel.SetActive(true);
                }
                else if (distance > alertRange)
                {
                    Debug.Log("�x���������01_03");

                    //�v���C���[������O�̏ꍇ�A�ʏ�p�j��ԂɈڍs
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //�G�ɒǂ��Ă��鎞��BGM���~
                    MusicController.Instance.audioClipnum = 1;
                    MusicController.Instance.StopBGM();
                    MusicController.Instance.audioClipnum = 0;
                    MusicController.Instance.PlayBGM();
                }
                else if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    Debug.Log("�x���������01_04");
                    NextPosition();
                }
                break;

        �@�@ //�Ǐ]���
            case EnemyState.Chase:
                //�x�������~
                audioSourceFindPlayerSE.Stop();

                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);
                navMeshAgent.speed = dashSpeed;
                ChasePlayer();

                if (!IsPlayerInFront())
                {
                    //�v���C���[������O�ɏo���ꍇ�A������ԂɈڍs
                    currentState = EnemyState.Investigate;
                    investigateTimer = 0f;
                    navMeshAgent.SetDestination(lastKnownPlayerPosition);
                }
                else
                {
                    //�v���C���[��������ɂ���ꍇ�A�Ǐ]��Ԃ𑱂���
                    lastKnownPlayerPosition = targetPoint.position;

                    await UniTask.Delay(TimeSpan.FromSeconds(0.3));

                    //�v���C���[���S��ɔ�������G���[��h�~����p��if����ǉ�
                    if(GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) playerFoundPanel.SetActive(false);
                }
                break;

            //�������
            case EnemyState.Investigate:
                //�x�������~
                audioSourceFindPlayerSE.Stop();

                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                navMeshAgent.speed = Speed;

                Debug.Log("�������01_01");

                investigateTimer += Time.deltaTime;
                if (IsPlayerInFront())
                {
                    Debug.Log("�������01_02");
                    //�v���C���[��������ɖ߂����ꍇ�A�Ǐ]��Ԃֈڍs
                    currentState = EnemyState.Chase;
                    lastKnownPlayerPosition = targetPoint.position;

                    //�G�ɒǂ��Ă��鎞��BGM���Đ�
                    MusicController.Instance.audioClipnum = 0;
                    MusicController.Instance.StopBGM();
                    MusicController.Instance.audioClipnum = 1;
                    MusicController.Instance.PlayBGM();
                }
                else if (investigateTimer >= investigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                {
                    Debug.Log("�������01_03");
                    //�������Ԃ��o�߂����ꍇ�A�ʏ�p�j��Ԃֈڍs
                    currentState = EnemyState.Patrol;
                    isAlertMode = false;

                    //�G�ɒǂ��Ă��鎞��BGM���~(�����̏���������Ȃ�)
                    MusicController.Instance.audioClipnum = 1;
                    MusicController.Instance.StopBGM();
                    MusicController.Instance.audioClipnum = 0;
                    MusicController.Instance.PlayBGM();

                    //EnemyBGMController.Instance.StopEnemyBGM();
                    //MusicController.Instance.PlayBGM();

                }
                else if (distance <= alertRange)
                {
                    Debug.Log("�������01_04");

                    //�v���C���[��������ɂ���ꍇ�A�x��������Ԃֈڍs
                    currentState = EnemyState.Alert;

                    //�G�ɒǂ��Ă��鎞��BGM���~
                    MusicController.Instance.audioClipnum = 1;
                    MusicController.Instance.StopBGM();
                    MusicController.Instance.audioClipnum = 0;
                    MusicController.Instance.PlayBGM();
                }
                break;
        }

        //�v���C���[���S��ɔ�������G���[��h�~����p��if����ǉ�
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            // ���ʉ�����
            currentSE = (currentState == EnemyState.Chase) ? sO_SE.GetSEClip(runSEid) : sO_SE.GetSEClip(walkSEid);

            // �����Ɋ�Â����ʌv�Z
            float volume = CalculateVolumeBasedOnDistance(distance);

            if (IsMove && !wasMovingLastFrame)
            {
                // ���鉹�̏ꍇ�A�s�b�`�𒲐�
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;

                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;

                //���ʂ�ݒ�
                audioSourceSE.volume = volume;

                audioSourceSE.Play();
            }
            else if (!IsMove && wasMovingLastFrame)
            {
                audioSourceSE.Stop();

                // ��~���Ƀs�b�`�����Z�b�g
                audioSourceSE.pitch = 1.0f;
            }
            else if (IsMove && wasMovingLastFrame && audioSourceSE.clip != currentSE)
            {
                audioSourceSE.Stop();

                // ���鉹�̏ꍇ�A�s�b�`�𒲐�
                audioSourceSE.pitch = (currentSE == sO_SE.GetSEClip(runSEid)) ? runSEPitch : 1.0f;
                audioSourceSE.clip = currentSE;
                audioSourceSE.loop = true;
                audioSourceSE.volume = volume;
                audioSourceSE.Play();
            }
            else if (IsMove && audioSourceSE.isPlaying)
            {
                // �ړ����ɉ��ʂ��p���I�ɍX�V
                audioSourceSE.volume = volume;
            }

            wasMovingLastFrame = IsMove;
        }        
    }


    private void OnDrawGizmos()
    {
        // ����͈͂̉���
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 leftRay = Quaternion.Euler(0, -halfFOV, 0) * transform.forward * enemyDetectionRange;
        Vector3 rightRay = Quaternion.Euler(0, halfFOV, 0) * transform.forward * enemyDetectionRange;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, leftRay);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, rightRay);

        // SphereCast�͈̔�
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, sphereCastRadius);
        if (targetPoint != null)
        {
            Gizmos.DrawWireSphere(targetPoint.position + Vector3.up * 1.0f, sphereCastRadius);
        }
    }

    // �����Ɋ�Â����ʂ��v�Z���郁�\�b�h
    private float CalculateVolumeBasedOnDistance(float distance)
    {
        if (distance <= maxSoundDistance)
        {
            return maxVolume; // �ő剹��
        }
        else if (distance >= minSoundDistance)
        {
            return minVolume; // �ŏ�����
        }
        else
        {
            // �����Ɋ�Â��Đ��`���
            float t = (distance - maxSoundDistance) / (minSoundDistance - maxSoundDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }
    }
}