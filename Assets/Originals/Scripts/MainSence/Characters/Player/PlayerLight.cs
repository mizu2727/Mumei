using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class PlayerLight : MonoBehaviour
{
    [Header("�v���C���[�J����(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Transform cameraTransform;

    [Header("���C�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    public GameObject playerHasLight;

    /// <summary>
    /// �G���[�h�~�p�ɒǉ��B�V�[�������[�h���ꂽ�ۂɃJ�����Q�Ƃ��X�V���邽�߂ɁASceneManager.sceneLoaded �C�x���g��o�^
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �G���[�h�~�p�ɒǉ��B���������[�N��h�����߁A�V�[���J�ڃC�x���g�̃��X�i�[���폜
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �G���[�h�~�p�ɒǉ��B�V�[�������[�h
    /// </summary>
    /// <param name="scene">�V�[����</param>
    /// <param name="mode">�V�[�����[�h</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //�V�[���J�ڌ�ɃJ�������Đݒ�
        UpdateCameraReference(); 
    }

    /// <summary>
    /// �G���[�h�~�p�ɒǉ��B�V�[���J�ڂ⏉�������ɃJ������Transform�𓮓I�Ɏ擾
    /// </summary>
    void UpdateCameraReference()
    {
        //�v���C���[�̎q�I�u�W�F�N�g����J�������擾
        Camera playerCamera = Player.instance.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }
        else
        {
            Debug.LogError("No Camera found as a child of the Player!");
        }
    }

    private void Start()
    {
        playerHasLight.SetActive(false);
        Player.instance.IsLight = false;

        //�V�[���J�n���ɃJ�������Ď擾
        UpdateCameraReference();
    }

    void Update()
    {
        //�v���C���[���S��(�v���C���[�I�u�W�F�N�g�폜��)�ɃJ�����̈ʒu���Q�Ƃł��Ȃ��Ȃ邽�߁A���f������ǉ�
        if (cameraTransform == null) return;

        //�Q�[���v���C���[�h�ȊO�̏ꍇ�A�����I�Ƀ��C�g���I�t�ɂ���
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) 
        {
            playerHasLight.SetActive(false);
            Player.instance.IsLight = false;
        }

        //�J�����̍��W�E�p�x��Ǐ]����
        TranceCamera();

        //���C�g��_��/��������
        TurnOnAndOfLight();


    }

    /// <summary>
    /// ���C�g�{�^�����������Ă��邩�𔻒肷��
    /// ���C�g�؂�ւ��cF�E1�L�[
    /// Light�c"joystick button 0"�����蓖�ĂĂ���A�R���g���[���[�ł�A�{�^���ɂȂ�
    /// </summary>
    /// <returns>true�Ń��C�g�{�^��������</returns>
    bool PlayerIsLight()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetButtonDown("Light");
    }

    /// <summary>
    /// �J�����̍��W�E�p�x��Ǐ]����
    /// </summary>
    void TranceCamera()
    {
        //���W�Ǐ]
        this.transform.position = cameraTransform.position;

        //�p�x�Ǐ]
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraTransform.rotation, 0.5f); 
    }

    /// <summary>
    /// ���C�g��_��/��������
    /// </summary>
    void TurnOnAndOfLight() 
    {
        //�v���C���[�̎q�I�u�W�F�N�g����J�������擾
        Camera playerCamera = Player.instance.GetComponentInChildren<Camera>();

        //�ʏ�v���C���[�h�̏ꍇ
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            if (PlayerIsLight() && !Player.instance.IsLight && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                //���C�g���_��
                playerHasLight.SetActive(true);
                Player.instance.IsLight = true;
            }
            else if ((PlayerIsLight() && Player.instance.IsLight) && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                //���C�g��������
                playerHasLight.SetActive(false);
                Player.instance.IsLight = false;
            }
        } 
    }
}


