using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

public class HearingEnemy : BaseEnemy
{
    [Header("�_�b�V�����̌��m�͈�")]
    [SerializeField] private float soundDetectionRange = 50f; // �_�b�V���������m���鋗��

    [Header("�_�b�V�����̒�������")]
    [SerializeField] private float soundInvestigateDuration = 10f; // �_�b�V�����̒�������

    private float soundInvestigateTimer = 0f; // �������Ԃ̃J�E���^�[
    private Vector3 lastHeardSoundPosition; // �Ō�ɕ��������_�b�V�����̈ʒu
    private bool isInvestigatingSound = false; // �_�b�V�����𒲍������ǂ���

    protected override async void Update()
    {
        // �Q�[�����v���C���łȂ��A�܂��̓v���C���[�����S���Ă���ꍇ�͏������X�L�b�v
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        // �v���C���[�Ƃ̋������v�Z
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint.position);

        // �v���C���[�̃_�b�V���������m
        //&& currentState != EnemyState.Chase
        if (!isInvestigatingSound && Player.instance.IsDash && distanceToPlayer <= soundDetectionRange)
        {
            // �_�b�V���������m�����ꍇ�A������ԂɈڍs
            currentState = EnemyState.Investigate;
            lastHeardSoundPosition = targetPoint.position; // ���̈ʒu���L�^
            isInvestigatingSound = true;
            soundInvestigateTimer = 0f;
            navMeshAgent.SetDestination(lastHeardSoundPosition); // ���̈ʒu�Ɍ�����
            Debug.Log($"[{gameObject.name}] �v���C���[�̃_�b�V���������m�I�ʒu: {lastHeardSoundPosition}");
        }

        // ��Ԃ��Ƃ̏���
        switch (currentState)
        {
            case EnemyState.Patrol:
            case EnemyState.Alert:
            case EnemyState.Chase:
                // BaseEnemy �� Update ���W�b�N���Ăяo��
                base.Update();
                break;

            case EnemyState.Investigate:
                if (isInvestigatingSound)
                {
                    // �_�b�V�����̒�����
                    animator.SetBool("isRun", false);
                    animator.SetBool("isWalk", IsEnemyMoving());
                    navMeshAgent.speed = NormalSpeed;

                    soundInvestigateTimer += Time.deltaTime;

                    // �v���C���[������ɓ������ꍇ�A�Ǐ]��ԂɈڍs
                    if (IsPlayerInFront())
                    {
                        currentState = EnemyState.Chase;
                        lastKnownPlayerPosition = targetPoint.position;
                        isInvestigatingSound = false;
                        playerFoundPanel.SetActive(true);
                        Debug.Log($"[{gameObject.name}] �������Ƀv���C���[�𔭌��I�Ǐ]��ԂɈڍs");
                    }
                    // �������Ԃ��o�߁A�܂��͖ړI�n�ɓ��B�����ꍇ�A�ʏ�p�j�ɖ߂�
                    else if (soundInvestigateTimer >= soundInvestigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                    {
                        currentState = EnemyState.Patrol;
                        isInvestigatingSound = false;
                        isAlertMode = false;
                        Debug.Log($"[{gameObject.name}] �_�b�V�����̒����I���A�ʏ�p�j�ɖ߂�");
                    }
                    // �v���C���[���߂��ɂ���ꍇ�A�x����ԂɈڍs
                    else if (distanceToPlayer <= alertRange)
                    {
                        currentState = EnemyState.Alert;
                        isInvestigatingSound = false;
                        Debug.Log($"[{gameObject.name}] �������Ƀv���C���[���߂��ɂ���A�x����ԂɈڍs");
                    }
                }
                else
                {
                    // �ʏ�̒�����ԁiBaseEnemy �̏������p���j
                    base.Update();
                }
                break;
        }
    }
}