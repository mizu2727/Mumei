using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    [Header("�}�E�X���x")]
    [SerializeField]  public float mouseSensitivity = 100f;

    [Header("�}�E�X�̐��񑬓x��Slider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("�}�E�X���x�ő�l")]
    [SerializeField] float maxMouseSensitivity = 500f;

    //�}�E�X�̉��ړ�
    float mouseX;

    //�}�E�X�̏c�ړ�
    float mouseY;

    [Header("�J������X����]�p�x")]
    private float xRotation = 0f;//�J������X����]�p�x

    [Header("�J������X����]�͈�")]
    [SerializeField] private float xRotationRange = 45f ;



    private void Start()
    {
        // �}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //�}�E�X���񑬓x��Slider�̍ő�l��ݒ�
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxMouseSensitivity;
    }

    private void Update()
    {
        
        if (maxMouseSensitivity <= mouseSensitivitySlider.value) mouseSensitivitySlider.value = maxMouseSensitivity;

        mouseSensitivity = mouseSensitivitySlider.value;

        if ( 0 < mouseSensitivity) 
        {
            //�}�E�X�̈ړ�
            mouseX =
                Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY =
                Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        


        xRotation -= mouseY;

        //���_�̉�]�͈͂�-A�x����B�x�ɐ�������
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //��]�p�x���X�V
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //���̃X�N���v�g�̐e�I�u�W�F�N�g����]������
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
