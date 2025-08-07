using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x")]
    [SerializeField]  public float lookSensitivity = 100f;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̐��񑬓x��Slider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x�ő�l")]
    [SerializeField] float maxLookSensitivity = 500f;

    //�}�E�X�̉��ړ�
    float lookX;

    //�}�E�X�̏c�ړ�
    float lookY;

    //�Q�[���p�b�h�̉E�X�e�B�b�N�̉��ړ�
    float lookX2;

    //�Q�[���p�b�h�̉E�X�e�B�b�N�̏c�ړ�
    float lookY2;

    [Header("�J������X����]�p�x")]
    private float xRotation = 0f;//�J������X����]�p�x

    [Header("�J������X����]�͈�")]
    [SerializeField] private float xRotationRange = 45f ;

    // �O�t���[���̌���������Ԃ�ێ�
    private bool wasTrunLastFrame = false; 



    private void Start()
    {
        // �}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //�}�E�X���񑬓x��Slider�̍ő�l��ݒ�
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;


        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance.isFallDown) return;


        //Ctrl�����Ŏ��_����������
        if(GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.playerIsBackRotate && !wasTrunLastFrame) 
        {
            //�v���C���[�����������Ă��鎞�̓}�E�X���x��0�ɂ���
            lookSensitivity = 0f;
            mouseSensitivitySlider.value = 0f;

            //�v���C���[�ƈꏏ�ɃJ������180�x��]������
            Player.instance.PlayerTurn();
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && mouseSensitivitySlider.value == 0f && wasTrunLastFrame)
        {
            //�v���C���[���O�������Ă��鎞�̓}�E�X���x�����ɖ߂�
            mouseSensitivitySlider.value = maxLookSensitivity / 2f;
        }

        wasTrunLastFrame = Player.instance.playerIsBackRotate;

        if (maxLookSensitivity <= mouseSensitivitySlider.value) mouseSensitivitySlider.value = maxLookSensitivity;

        lookSensitivity = mouseSensitivitySlider.value;

        if ( 0 < lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            //�}�E�X�̈ړ�
            lookX =
                Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            lookY =
                Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

            //�Q�[���p�b�h�̉E�X�e�B�b�N�̈ړ�

            //Mouse X2�cAxis����"4th axis (Joysticks)"��I���B�R���g���[���[�ł͉E�X�e�B�b�N�ɂȂ�
            lookX2 =
                Input.GetAxis("Mouse X2") * lookSensitivity * Time.deltaTime;

            //Mouse Y2�cAxis����"5th axis (Joysticks)"��I���B�R���g���[���[�ł͉E�X�e�B�b�N�ɂȂ�
            lookY2 =
                Input.GetAxis("Mouse Y2") * lookSensitivity * Time.deltaTime;
        }
        


        xRotation -= lookY;

        xRotation -= lookY2;

        //���_�̉�]�͈͂�-A�x����B�x�ɐ�������
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //��]�p�x���X�V
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //���̃X�N���v�g�̐e�I�u�W�F�N�g����]������
        transform.parent.Rotate(Vector3.up * lookX);

        transform.parent.Rotate(Vector3.up * lookX2);
    }
}
