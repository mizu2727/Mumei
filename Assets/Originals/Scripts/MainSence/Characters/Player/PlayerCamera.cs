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

    private Transform playerTransform; // �v���C���[��Transform



    private void Start()
    {
        // �}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //�}�E�X���񑬓x��Slider�̍ő�l��ݒ�
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;

        // �v���C���[��Transform���擾
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance == null || Player.instance.isFallDown) return;

        // �}�E�X���x���X���C�_�[����擾
        if (mouseSensitivitySlider)
        {
            lookSensitivity = mouseSensitivitySlider.value;
            if (lookSensitivity > maxLookSensitivity) lookSensitivity = maxLookSensitivity;
        }


        //Ctrl�����Ŏ��_����������
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.playerIsBackRotate) 
        {
            if (!wasTrunLastFrame)
            {
                //�v���C���[�̓��Ŏ��E�̎ז��ɂȂ�̂�h�����߂ɃJ�����̈ʒu����������֕ύX����
                transform.localPosition = new Vector3(0, 1.5f, -0.25f);
                

                // �J�����𑦍���180�x��]�i�v���C���[�̔w��j
                transform.rotation = Quaternion.LookRotation(-playerTransform.forward, Vector3.up);

                // �}�E�X���͂𖳌���
                lookSensitivity = 0f; 

                if (mouseSensitivitySlider) mouseSensitivitySlider.value = 0f;
                wasTrunLastFrame = true;
            }
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
        {
            

            // �J�������v���C���[�̑O���ɖ߂�
            transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            if (mouseSensitivitySlider)
            {
                //�v���C���[�̓��Ŏ��E�̎ז��ɂȂ�̂�h�����߂ɃJ�����̈ʒu��O�������֕ύX����
                transform.localPosition = new Vector3(0, 1.5f, 0.1f);

                mouseSensitivitySlider.value = maxLookSensitivity / 2f;
                lookSensitivity = mouseSensitivitySlider.value;
            }
            wasTrunLastFrame = false;
        }

        if ( 0 < lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.PlayerIsBackRotate()) 
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

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

            // �J�����̏㉺��]
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // �v���C���[�̍��E��]�i�J�����̐e�I�u�W�F�N�g�j
            playerTransform.Rotate(Vector3.up * (lookX + lookX2));
        }
    }
}
