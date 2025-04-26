using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy: MonoBehaviour,CharacterInterface
{
    //NavMeshAgent���擾
    NavMeshAgent navMeshAgent;

    //�Ǐ]�������I�u�W�F�N�g
    public Transform tagetPoint;

    //�p�j
    [SerializeField] private TestMap01 testMap01;//�v���n�u�������I�u�W�F�N�g���A�^�b�`
    private int positionNumber = 0;
    private int maxPositionNumber;

    //���̒l����������Ɯp�j�n�_�������炸�A�L�������NaveMesh�͈̔͊O�ɂȂ�
    //�v�������K�v
    [SerializeField] private float findPatrolPointRange = 2500f;

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

    [SerializeField] private float enemyDetectionRange = 100f;
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

        if (Player.instance.HP <= 0) Player.instance.Dead() ;

    }


    [SerializeField] private Vector3 enemyStartPosition;
    [SerializeField]
    public Vector3 StartPosition
    {
        get => enemyStartPosition;
        set => enemyStartPosition = value;
    }

    //�Ǐ]
    void ChasePlayer() 
    {
        //�Ǐ]�p�̖ړI�n��ݒ�
        navMeshAgent.SetDestination(tagetPoint.position);
        Debug.Log("�v���C���[�Ǐ]");
    }

    //���̔o�~�n�_�����߂�
    void NextPosition() 
    {
        positionNumber = Random.Range(0, maxPositionNumber);
        Vector3 targetPos = testMap01.patrolPoint[positionNumber].position;

        NavMeshHit hit;

        //�ړI�n��NaveMesh�͈͓̔��ɂ��邩�𔻒肷��
        if (NavMesh.SamplePosition(targetPos, out hit, findPatrolPointRange, NavMesh.AllAreas)) 
        {
            navMeshAgent.destination = hit.position;
            //Debug.Log("�ړI�n��NavMesh��ɕ␳: " + hit.position);
        }
        else
        {
            Debug.LogError("�ړI�n��NavMesh�͈̔͊O�ł�: " + targetPos);

            // **�����_���Ȉʒu��T������**
            Vector3 randomPos = targetPos + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
            if (NavMesh.SamplePosition(randomPos, out hit, 10.0f, NavMesh.AllAreas))
            {
                navMeshAgent.destination = hit.position;
                Debug.LogWarning("�����NavMesh��̃����_���Ȉʒu�Ɉړ�: " + hit.position);
            }
            else
            {
                Debug.LogError("�K�؂�NavMesh�ʒu��������܂���ł���");
            }
        }
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgent���A�^�b�`����Ă��܂���I");
            return;
        }

        // NavMeshAgent�̏�����
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

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
        if (testMap01 == null || testMap01.patrolPoint == null || testMap01.patrolPoint.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] testMap01�܂���patrolPoint���ݒ肳��Ă��܂���I");
            return;
        }

        maxPositionNumber = testMap01.patrolPoint.Length;
        positionNumber = Random.Range(0, maxPositionNumber);
        navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
        Debug.Log($"[{gameObject.name}] �����p�j�n�_: {testMap01.patrolPoint[positionNumber].position}");
    }

    void Update()
    {
        if (Player.instance.IsDead || Player.instance == null || testMap01 == null) return;

        // �K�{�R���|�[�l���g�̃`�F�b�N
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            Debug.LogWarning($"[{gameObject.name}] NavMeshAgent�������܂��͑��݂��܂���B");
            return;
        }
        if (Player.instance == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Player.instance��null�ł��B");
            return;
        }
        if (Player.instance.IsDead)
        {
            Debug.Log($"[{gameObject.name}] �v���C���[�����ɂ܂����B�Ǐ]���~�B");
            return;
        }
        if (testMap01 == null)
        {
            Debug.LogWarning($"[{gameObject.name}] testMap01��null�ł��B");
            return;
        }
        if (tagetPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] tagetPoint��null�ł��B�v���C���[��Transform��ݒ肵�Ă��������B");
            return;
        }

        //�v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, tagetPoint.position);
        Debug.Log($"[{gameObject.name}] �v���C���[�Ƃ̋���: {distance}, ���m�͈�: {DetectionRange}, ���݈ʒu: {transform.position}, �v���C���[�ʒu: {tagetPoint.position}");

        //�v���C���[�����͈̔͂ɓ������ꍇ�̏���
        if (distance <= DetectionRange)
        {
            ChasePlayer();
        }
        else
        {
            //�G�Ɣo�~�n�_�̋������w��̒l�͈͓̔��̏ꍇ�̏���
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
            {
                NextPosition();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Player.instance = other.GetComponent<Player>();
            if (Player.instance != null && !Player.instance.IsDead)
            {
                Attack();
            }
        }
    }
}
