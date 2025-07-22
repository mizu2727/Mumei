using UnityEngine;

interface CharacterInterface
{
    //�A�j���[�V�����Đ�
    Animator PlayAnimator { get; set; }

    //���O
    string CharacterName { get; set; }

    //�ʏ�̈ړ����x
    float NormalSpeed { get; set; }

    //�_�b�V�����̈ړ����x
    float SprintSpeed { get; set; }

    //�L�����N�^�[���o�͈�
    float DetectionRange { get; set; }

    //�d��
    float Gravity { get; set; }

    //HP
    int HP { get; set; }

    //���S����
    bool IsDead { get; set; }

    //���S
    void Dead();

    //�ړ�����
    bool IsMove { get; set; }

    //�_�b�V������
    bool IsDash { get; set; }

    //�������������
    bool IsBackRotate { get; set; }

    //���C�g�̐؂�ւ�
    bool IsLight { get; set; }

    //�U��
    void Attack();

    //�L�����N�^�[�̏����ʒu
    Vector3 StartPosition { get; set; }
}
