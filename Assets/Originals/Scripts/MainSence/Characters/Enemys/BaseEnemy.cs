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
    public Transform[] patrolPoint;
    private int positionNumber = 0;
    private int maxPositionNumber;

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
    }

    //���̔o�~�n�_�����߂�
    void NextPosition() 
    {
        positionNumber = Random.Range(0, maxPositionNumber);

        navMeshAgent.destination = patrolPoint[positionNumber].position;

        Debug.Log("���̔o�~�n�_�̗v�f�ԍ���" + positionNumber);
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.destination = patrolPoint[positionNumber].position;
        maxPositionNumber = patrolPoint.Length;
        Debug.Log("�ő�̔o�~�n�_�̗v�f�ԍ���" + patrolPoint[maxPositionNumber - 1]);
    }

    void Update()
    {
        if (player.IsDead || player == null) return;

        //�v���C���[�Ƃ̋����𑪒�
        float distance = Vector3.Distance(transform.position, tagetPoint.position);

        //�v���C���[�����͈̔͂ɓ������ꍇ�̏���
        if (distance <= DetectionRange)
        {
            ChasePlayer();
        }
        else
        {
            //�G�Ɣo�~�n�_�̋������w��̒l�͈͓̔��̏ꍇ�̏���
            if (navMeshAgent.remainingDistance < 0.5f)
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
