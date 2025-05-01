using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("�}�E�X���x")]
    [SerializeField]  public float mouseSensitivity = 100f;

    [Header("�J������X����]�p�x")]
    private float xRotation = 0f;//�J������X����]�p�x

    [Header("�J������X����]�͈�")]
    [SerializeField] private float xRotationRange = 90f ;

    private void Start()
    {
        // �}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //�}�E�X�̈ړ�
        float mouseX = 
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        xRotation -= mouseY;

        //���_�̉�]�͈͂�-A�x����B�x�ɐ�������
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //��]�p�x���X�V
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //���̃X�N���v�g�̐e�I�u�W�F�N�g����]������
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
