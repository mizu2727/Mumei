using UnityEngine;

public class ScrollObject : MonoBehaviour
{
    [Header("�X�N���[���J�n�n�_")]
    public float startPosition;

    [Header("�X�N���[���I���n�_")]
    public float endPosition;

    [Header("�X�N���[�����x")]
    public float speed;

    [Header("�X�N���[���̌����̔���(isDirection��true�Ȃ�E�����Afalse�Ȃ獶����)")]
    public bool isDirection;

    //�X�N���[���̌����̌���
    private float direction = 1;

    private void Start()
    {
        if (!isDirection) direction = -1;
    }

    void Update()
    {
        //�I�u�W�F�N�g���w��̑��x�ƌ����ŃX�N���[��������
        transform.Translate(speed * Time.deltaTime * direction, 0, 0);

        //�I�u�W�F�N�g���I���n�_�܂ŃX�N���[���������𔻒�
        if (transform.position.x <= endPosition) Scroll();
    }

    //�I�u�W�F�N�g�̈ʒu���J�n�n�_�֎w�肷��
    void Scroll()
    {
        float difference = transform.position.x - endPosition;
        Vector3 resetPosition = transform.position;
        resetPosition.x = startPosition + difference;
        transform.position = resetPosition;
    }
}