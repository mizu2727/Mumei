using UnityEngine;

interface CharacterInterface
{
    /// <summary>
    /// �A�j���[�V�����Đ�
    /// </summary>
    Animator PlayAnimator { get; set; }

    /// <summary>
    /// �L�����N�^�[��
    /// </summary>
    string CharacterName { get; set; }

    /// <summary>
    /// �ʏ�̈ړ����x
    /// </summary>
    float NormalSpeed { get; set; }

    /// <summary>
    /// �_�b�V�����̈ړ����x
    /// </summary>
    float SprintSpeed { get; set; }

    /// <summary>
    /// �L�����N�^�[���o�͈�
    /// </summary>
    float DetectionRange { get; set; }

    /// <summary>
    /// �d��
    /// </summary>
    float Gravity { get; set; }
    
    /// <summary>
    /// HP
    /// </summary>
    int HP { get; set; }

    /// <summary>
    /// ���S�t���O
    /// </summary>
    bool IsDead { get; set; }

    /// <summary>
    /// ���S���\�b�h
    /// </summary>
    void Dead();

    /// <summary>
    /// �ړ�����
    /// </summary>
    bool IsMove { get; set; }

    /// <summary>
    /// �_�b�V������
    /// </summary>
    bool IsDash { get; set; }

    /// <summary>
    /// ���������t���O
    /// </summary>
    bool IsBackRotate { get; set; }

    /// <summary>
    /// ���C�g�̐؂�ւ��t���O
    /// </summary>
    bool IsLight { get; set; }

    /// <summary>
    /// �U�����郁�\�b�h
    /// </summary>
    void Attack();

    /// <summary>
    /// �L�����N�^�[�̏����ʒu
    /// </summary>
    Vector3 StartPosition { get; set; }
}
