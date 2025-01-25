using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]  public float mouseSensitivity = 100f;//�}�E�X���x
    private float xRotation = 0f;//�J������X����]�p�x

    void Start()
    {
        // �}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //�}�E�X�̈ړ�
        float mouseX = 
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        xRotation -= mouseY;

        //���_�̉�]�͈͂�-90�x����90�x�ɐ�������
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //��]�p�x���X�V
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //���̃X�N���v�g�̐e�I�u�W�F�N�g����]������
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
