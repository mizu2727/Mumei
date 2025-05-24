using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random; // System.Random �� UnityEngine.Random �̏Փ˂������

public class BaseEnemy : MonoBehaviour, CharacterInterface
{
    // �G�̃X�e�[�^�X
    [Header("Enemy Stats")]
    private Animator animator;
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


    // NavMesh�֘A
    [Header("NavMesh Settings")]
    NavMeshAgent navMeshAgent;
    // ���̒l����������Ɯp�j�n�_�������炸�A�L�������NaveMesh�͈̔͊O�ɂȂ�
    // �v�������K�v
    [SerializeField] private float findPatrolPointRange = 10f;
    [SerializeField] private float enemyDetectionRange = 100f;
    [SerializeField] private float alertRange = 15f;
    [SerializeField] private float sightRange = 10f;
    [SerializeField] private LayerMask sightLayer;
    [SerializeField] private float raycastDistance = 5.0f;
    [SerializeField] private LayerMask groundLayer; // �n�ʂƂ��Ĕ��肷��Layer


    // �p�j�֘A
    [Header("Patrol Settings")]
    [SerializeField] private TestMap01 testMap01;//�v���n�u�������I�u�W�F�N�g���A�^�b�`
    [SerializeField] public Transform[] patrolPoint;

    private int positionNumber = 0;
    private int maxPositionNumber;


    // ���m�E�����֘A
    [Header("Detection Settings")]
    // �Ǐ]�������I�u�W�F�N�g
    public Transform tagetPoint;//�q�G�����L�[��̃v���C���[���A�^�b�`����




    // �T�E���h�֘A
    [Header("Audio Settings")]
    private AudioSource audioSourceSE; // �G��p��AudioSource
    [SerializeField] private AudioClip walkSE;
    [SerializeField] private AudioClip runSE;
    [SerializeField] private AudioClip findPlayerSE;



    private bool wasMovingLastFrame = false; // �O�t���[���̈ړ���Ԃ�ێ�



    
    private bool isAlertMode = false;



    void ChasePlayer()
    {
        if (tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPoint��null�ł��B�v���C���[��ݒ肵�Ă��������B");
            return;
        }
        navMeshAgent.SetDestination(tagetPoint.position);
        navMeshAgent.speed = dashSpeed; // �Ǐ]���̓_�b�V�����x
        navMeshAgent.isStopped = false;
        Debug.Log($"[{gameObject.name}] �v���C���[�Ǐ]: �ړI�n {tagetPoint.position}");
    }

    // ���̔o�~�n�_�����߂�
    void NextPosition()
    {
        //testMap01.patrolPoint
        if (patrolPoint == null || patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] patrolPoint�������ł��I�ړ����~���܂��B");
            navMeshAgent.isStopped = true;
            return;
        }

        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = patrolPoint[positionNumber].position;

        NavMeshHit hit;

        // �ړI�n��NaveMesh�͈͓̔��ɂ��邩�𔻒肷��
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
        {
            navMeshAgent.destination = hit.position;
            Debug.Log($"[{gameObject.name}] ���̜p�j�n�_: {hit.position} (�C���f�b�N�X: {positionNumber})");
        }
        else
        {
            Debug.LogError("�ړI�n��NavMesh�͈̔͊O�ł�: " + targetPos);

            // �t�H�[���o�b�N�F�ʂ̜p�j�n�_������
            for (int i = 0; i < patrolPoint.Length; i++)
            {
                int nextIndex = (positionNumber + i + 1) % patrolPoint.Length;
                Vector3 fallbackPos = patrolPoint[nextIndex].position;
                if (NavMesh.SamplePosition(fallbackPos, out hit, findPatrolPointRange, NavMesh.AllAreas))
                {
                    navMeshAgent.destination = hit.position;
                    positionNumber = nextIndex;
                    Debug.Log($"[{gameObject.name}] �t�H�[���o�b�N��̜p�j�n�_�Ɉړ�: {hit.position} (�C���f�b�N�X: {nextIndex})");
                    return;
                }
            }

            // �Ō�̎�i�F���݈ʒu�̋߂���NavMesh��̈ʒu��T��
            if (NavMesh.FindClosestEdge(transform.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
            {
                navMeshAgent.destination = edgeHit.position;
                Debug.LogWarning($"[{gameObject.name}] �L���Ȝp�j�n�_�������炸�A�Ŋ���NavMesh�̒[�Ɉړ�: {edgeHit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] �L����NavMesh�ʒu��������܂���ł����B�ړ����~���܂��B");
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
        Debug.Log($"[{gameObject.name}] OnCollisionEnter ���Ă΂�܂����B�Փˑ���: {collision.gameObject.name}");
        Debug.Log($"[{gameObject.name}] �����ƏՓ˂��܂���: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}, Tag: {collision.gameObject.tag}");

        // �Փ˂����I�u�W�F�N�g�� "Wall" ���C���[�܂��� "Wall" �^�O�������Ă��邩�m�F
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log($"[{gameObject.name}] �ǂƏՓ˂��܂����B���x��0�ɐݒ肵�A�����]�������݂܂��B");
            navMeshAgent.velocity = Vector3.zero; // ���x��0�ɐݒ肵�Ē�~������
            navMeshAgent.isStopped = true; // NavMeshAgent�̈ړ����~
            animator.SetBool("isRun", false); // ��~�A�j���[�V�������Đ�
            animator.SetBool("isWalk", false);
            lastCollisionPoint = collision.contacts[0].point; // �Փ˒n�_���L�^
            Invoke("ChangeDirection", 0.5f); // 0.5�b��ɕ����]��
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }

    void ChangeDirection()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            // NavMeshAgent�̒�~������
            navMeshAgent.isStopped = false;

            // ���݈ʒu���班�����ꂽ�����_���ȕ���������
            Vector3 randomDirection = Random.insideUnitSphere.normalized * 3f;
            Vector3 newTarget = transform.position + randomDirection;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newTarget, out hit, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
                Debug.Log($"[{gameObject.name}] �Փˌ�A�����_���ȐV�����ړI�n��ݒ�: {hit.position}");
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

    

    void Start()
    {
        PlayAnimator = GetComponent<Animator>();


        // MusicController����AudioSource���擾
        audioSourceSE = MusicController.Instance.GetAudioSource();

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgent���A�^�b�`����Ă��܂���I");
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
                Debug.Log($"[{gameObject.name}] �����ʒu��NavMesh��ɕ␳: {hit.position}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] NavMesh�ʒu�␳�Ɏ��s���܂����B�ʒu: {transform.position}");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] NavMesh��ɔz�u����Ă��܂��B�ʒu: {transform.position}");
        }

        // �p�j�n�_�̏�����
        if (testMap01 == null || patrolPoint == null || patrolPoint.Length == 0)
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


        maxPositionNumber = patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] �����p�j�n�_: {patrolPoint[positionNumber].position}");
    }

    void Update()
    {
        if (Player.instance == null || Player.instance.IsDead || testMap01 == null || tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Update�������X�L�b�v: Player={Player.instance}, testMap01={testMap01}, tagetPoint={tagetPoint}");
            navMeshAgent.isStopped = true;
            return;
        }

        // �ړ������ǂ����𔻒�
        IsMove = IsEnemyMoving();

        // �v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, tagetPoint.position);
        
        // �x�����[�h����
        if (distance <= alertRange)
        {
            if (!isAlertMode)
            {
                Debug.Log($"[{gameObject.name}] �x�����[�h�J�n: �v���C���[�Ƃ̋��� {distance}");
                isAlertMode = true;
                MusicController.Instance.LoopPlayAudioSE(audioSourceSE, findPlayerSE); // �v���C���[��������SE
            }

            if (CanSeePlayer())
            {
                ChasePlayer();
                animator.SetBool("isRun", true);
                animator.SetBool("isWalk", false);
            }
            else
            {
                animator.SetBool("isRun", false);
                animator.SetBool("isWalk", IsMove);
                if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
                {
                    NextPosition();
                }
            }
        }
        else
        {
            if (isAlertMode)
            {
                Debug.Log($"[{gameObject.name}] �x�����[�h����: �v���C���[�Ƃ̋��� {distance}");
                isAlertMode = false;
            }
            animator.SetBool("isRun", false);
            animator.SetBool("isWalk", IsMove);
            navMeshAgent.speed = Speed; // �ʏ푬�x�ɖ߂�
            if (!navMeshAgent.pathPending && (navMeshAgent.remainingDistance < 0.5f || !navMeshAgent.hasPath))
            {
                NextPosition();
            }
        }

        // ���ʉ�����
        AudioClip currentSE = isAlertMode && CanSeePlayer() ? runSE : walkSE;

        if (IsMove && !wasMovingLastFrame)
        {
            // �ړ��J�n���ɓK�؂Ȍ��ʉ����Đ�
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
            // �ړ����ɏ�Ԃ��؂�ւ�����ꍇ�A���ʉ���ύX
            MusicController.Instance.StopSE(audioSourceSE);
            MusicController.Instance.LoopPlayAudioSE(audioSourceSE, currentSE);
        }

        // ���݂̈ړ���Ԃ��L�^
        wasMovingLastFrame = IsMove;



        // �n�ʂƂ̋�������
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundLayer))
        {
            navMeshAgent.baseOffset = -hit.distance;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] �n�ʂ����m�ł��܂���ł����B�ʒu: {transform.position}");
            navMeshAgent.baseOffset = -0.1f;
        }
    }

    bool CanSeePlayer()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = (tagetPoint.position - transform.position).normalized;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f; // �����������������ē�������o��

        // Raycast�̕`��i�΁F�q�b�g�A�ԁF�q�b�g�Ȃ��j
        if (Physics.Raycast(rayOrigin, directionToPlayer, out hit, sightRange, sightLayer))
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * hit.distance, Color.green, 0.1f);
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log($"[{gameObject.name}] �v���C���[�����F: ���� {hit.distance}");
                return true;
            }
            else
            {
                Debug.Log($"[{gameObject.name}] ��������Q���ɎՂ�ꂽ: {hit.collider.name}");
                Debug.DrawRay(rayOrigin, directionToPlayer * hit.distance, Color.red, 0.1f);
            }
        }
        else
        {
            Debug.DrawRay(rayOrigin, directionToPlayer * sightRange, Color.red, 0.1f);
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }
}