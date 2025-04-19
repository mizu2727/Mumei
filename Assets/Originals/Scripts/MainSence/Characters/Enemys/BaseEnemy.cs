using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy: MonoBehaviour,CharacterInterface
{
    //
    [SerializeField] private Player player;

    //NavMeshAgent���擾
    NavMeshAgent navMeshAgent;

    //�Ǐ]�������I�u�W�F�N�g(Hieralchy���̃I�u�W�F�N�g���A�^�b�`����)
    [SerializeField] public Transform tagetPoint;

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

    [SerializeField] private float enemyDetectionRange = 10f;
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
        if (player.IsDead) return;

        player.HP -= 1;

        if (player.HP <= 0) player.Dead() ;

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

        if (navMeshAgent.isOnNavMesh)
        {
            //Debug.Log("NavMeshAgent �� NavMesh ��ɔz�u����Ă��܂��I");
        }
        else
        {
            Debug.LogError("NavMeshAgent �� NavMesh ��ɂ���܂���I");
        }

        //�ړ���L����
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        // NavMesh ��̓K�؂Ȉʒu�Ɉړ�
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            Debug.Log("�G�̏����ʒu�𒲐�: " + hit.position);
        }

        navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
        maxPositionNumber = testMap01.patrolPoint.Length;
        Debug.Log("patrolPoint�̐�: " + maxPositionNumber);

        if (maxPositionNumber > 0)
        {
            navMeshAgent.destination = testMap01.patrolPoint[positionNumber].position;
            Debug.Log("�����p�j�n�_��" + testMap01.patrolPoint[positionNumber].position);
        }
        else
        {
            Debug.LogError("patrolPoint���������Ă��܂���I");
        }
    }

    void Update()
    {
        if (player.IsDead || player == null || testMap01 == null) return;

        //�v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, tagetPoint.position);

        //�v���C���[�����͈̔͂ɓ������ꍇ�̏���
        if (distance <= DetectionRange)
        {
            ChasePlayer();
        }
        else
        {
            //Debug.Log($"���ݒn: {transform.position}");
            //Debug.Log($"�ړI�n: {navMeshAgent.destination}");
            //Debug.Log($"�ړI�n�܂ł̋���: {navMeshAgent.remainingDistance}");
            //Debug.Log($"�ړ����x: {navMeshAgent.velocity}");
            //Debug.Log($"NavMeshAgent�̏��: {navMeshAgent.pathStatus}");

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
            player = other.GetComponent<Player>();
            if (player != null && !player.IsDead)
            {
                Attack();
            }
        }
    }
}
