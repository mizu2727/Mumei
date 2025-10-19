using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

public class HearingEnemy : BaseEnemy
{
    [Header("�_�b�V�����̌��m�͈�")]
    [SerializeField] private float soundDetectionRange = 50f;

    [Header("�_�b�V�����̒�������")]
    [SerializeField] private float soundInvestigateDuration = 10f;

    /// <summary>
    /// �������ԃJ�E���^�[
    /// </summary>
    private float soundInvestigateTimer = 0f;

    /// <summary>
    /// �Ō�ɕ��������v���C���[�̃_�b�V�����̈ʒu
    /// </summary>
    private Vector3 lastHeardSoundPosition;

    /// <summary>
    /// �_�b�V�����𒲍�����t���O
    /// </summary>
    private bool isInvestigatingSound = false;

    protected override async void Update()
    {
        //�Q�[�����v���C���łȂ��A�܂��̓v���C���[�����S���Ă���ꍇ�͏������X�L�b�v
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;
            return;
        }

        //�v���C���[�Ƃ̋������v�Z
        float distanceToPlayer = Vector3.Distance(transform.position, targetPoint.position);

        //�v���C���[�̃_�b�V���������m
        if (!isInvestigatingSound && Player.instance.IsDash && distanceToPlayer <= soundDetectionRange)
        {
            //�_�b�V���������m�����ꍇ�A������ԂɈڍs
            currentState = EnemyState.Investigate;

            //���̈ʒu���L�^
            lastHeardSoundPosition = targetPoint.position;
            isInvestigatingSound = true;
            soundInvestigateTimer = 0f;

            //���̈ʒu�ֈړ�
            navMeshAgent.SetDestination(lastHeardSoundPosition);
        }

        //��Ԃ��Ƃ̏���
        switch (currentState)
        {
            case EnemyState.Patrol:
            case EnemyState.Alert:
            case EnemyState.Chase:

                //BaseEnemy.cs��Update�֐����Ăяo��
                base.Update();
                break;

            //�������
            case EnemyState.Investigate:

                //�_�b�V�����������̏ꍇ
                if (isInvestigatingSound)
                {
                    

                    //���s�A�j���[�V�����Đ�
                    animator.SetBool("isRun", false);
                    animator.SetBool("isWalk", IsEnemyMoving());

                    navMeshAgent.speed = NormalSpeed;

                    soundInvestigateTimer += Time.deltaTime;

                    //�v���C���[������ɓ������ꍇ�A�Ǐ]��ԂɈڍs
                    if (IsPlayerInFront())
                    {
                        currentState = EnemyState.Chase;
                        lastKnownPlayerPosition = targetPoint.position;
                        isInvestigatingSound = false;

                        //��ʂ�Ԃ��\��
                        playerFoundPanel.SetActive(true);

                        Debug.Log("������Ԃ���Ǐ]��Ԃ�02");
                    }
                    //�������Ԃ��o�߁A�܂��͖ړI�n�ɓ��B�����ꍇ�A�ʏ�p�j�ɖ߂�
                    else if (soundInvestigateTimer >= soundInvestigateDuration || (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending))
                    {
                        currentState = EnemyState.Patrol;
                        isInvestigatingSound = false;
                        isAlertMode = false;

                        Debug.Log("������Ԃ���ʏ�p�j��Ԃ�02");
                    }
                    //�v���C���[���߂��ɂ���ꍇ�A�x����ԂɈڍs
                    else if (distanceToPlayer <= alertRange)
                    {
                        currentState = EnemyState.Alert;
                        isInvestigatingSound = false;

                        //��ʂ����ɖ߂�
                        playerFoundPanel.SetActive(false);

                        Debug.Log("������Ԃ���x��������Ԃ�02");
                    }
                }
                else
                {
                    //BaseEnemy.cs��Update�֐����Ăяo��
                    base.Update();
                }
                break;
        }
    }
}