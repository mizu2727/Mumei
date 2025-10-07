using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x")]
    //[SerializeField]  public float lookSensitivity = 500f;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̐��񑬓x��Slider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    //[SerializeField] public Slider mouseSensitivitySlider;

    /// <summary>
    /// �}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x�ő�l
    /// </summary>
    //const float maxLookSensitivity = 1000f;

    /// <summary>
    /// �}�E�X�̉��ړ�
    /// </summary>
    float lookX;

    /// <summary>
    /// �}�E�X�̏c�ړ�
    /// </summary>
    float lookY;

    /// <summary>
    /// �Q�[���p�b�h�̉E�X�e�B�b�N�̉��ړ�
    /// </summary>
    float lookX2;

    /// <summary>
    /// �Q�[���p�b�h�̉E�X�e�B�b�N�̏c�ړ�
    /// </summary>
    float lookY2;

    [Header("�J������X����]�p�x")]
    private float xRotation = 0f;

    [Header("�J������X����]�͈�")]
    [SerializeField] private float xRotationRange = 45f ;

    /// <summary>
    /// �O�t���[���̌��������t���O
    /// </summary>
    private bool wasTrunLastFrame = false;

    /// <summary>
    /// �v���C���[��Transform
    /// </summary>
    private Transform playerTransform;


    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ��ɐݒ�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //�v���C���[������U������ƃv���C���[�̓��Ŏ��E���ז��ɂȂ�s���h�~����p
        //if (GameController.instance.mouseSensitivitySlider != null) mouseSensitivitySlider = GameController.instance.mouseSensitivitySlider;
        //else Debug.LogError("GameController��mouseSensitivitySlider���ݒ肳��Ă��܂���");
    }

    private void Start()
    {
        //�}�E�X�J�[�\�����\���ɂ��A�ʒu���Œ�
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //�}�E�X���񑬓x��Slider�̍ő�l��ݒ�
        //if (GameController.instance.mouseSensitivitySlider) GameController.instance.mouseSensitivitySlider.maxValue = maxLookSensitivity;

        //�v���C���[��Transform���擾
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance == null || Player.instance.isFallDown) return;

        //�}�E�X���x���X���C�_�[����擾
        //if (GameController.instance.mouseSensitivitySlider)
        //{
        //    GameController.instance.lookSensitivity = GameController.instance.mouseSensitivitySlider.value;
        //    if (GameController.instance.lookSensitivity > maxLookSensitivity) GameController.instance.lookSensitivity = maxLookSensitivity;
        //}


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
                GameController.lookSensitivity = 0f; 

                if (GameController.instance.mouseSensitivitySlider) GameController.instance.mouseSensitivitySlider.value = 0f;
                wasTrunLastFrame = true;
            }
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
        {
            // �J�������v���C���[�̑O���ɖ߂�
            transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            if (GameController.instance.mouseSensitivitySlider)
            {
                //�v���C���[�̓��Ŏ��E�̎ז��ɂȂ�̂�h�����߂ɃJ�����̈ʒu��O�������֕ύX����
                transform.localPosition = new Vector3(0, 1.5f, 0.1f);

                GameController.instance.mouseSensitivitySlider.value = GameController.instance.maxLookSensitivity / 2f;
                GameController.lookSensitivity = GameController.instance.mouseSensitivitySlider.value;
            }
            wasTrunLastFrame = false;
        }

        if ( 0 < GameController.lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.PlayerIsBackRotate()) 
        {
            //�}�E�X�̈ړ�
            lookX = Input.GetAxis("Mouse X") * GameController.lookSensitivity * Time.deltaTime;
            lookY = Input.GetAxis("Mouse Y") * GameController.lookSensitivity * Time.deltaTime;

            //�Q�[���p�b�h�̉E�X�e�B�b�N�̈ړ�
            //Mouse X2�cAxis����"4th axis (Joysticks)"��I���B�R���g���[���[�ł͉E�X�e�B�b�N�ɂȂ�
            lookX2 = Input.GetAxis("Mouse X2") * GameController.lookSensitivity * Time.deltaTime;

            //Mouse Y2�cAxis����"5th axis (Joysticks)"��I���B�R���g���[���[�ł͉E�X�e�B�b�N�ɂȂ�
            lookY2 = Input.GetAxis("Mouse Y2") * GameController.lookSensitivity * Time.deltaTime;

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

            // �J�����̏㉺��]
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // �v���C���[�̍��E��]�i�J�����̐e�I�u�W�F�N�g�j
            playerTransform.Rotate(Vector3.up * (lookX + lookX2));
        }
    }

    /// <summary>
    /// �J�����̉�]�����Z�b�g����
    /// </summary>
    public void ResetCameraRotation()
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.Story) 
        {
            //�㉺��]�����Z�b�g
            xRotation = 0f;

            //�J�����̃��[�J����]��������Ԃɖ߂�
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
        }
    }
}
