using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static GameController instance;

    [Header("Prefab����GameController�̎q�I�u�W�F�N�g���A�^�b�`���邱��")]
    [SerializeField] private SaveLoad saveLoad;

    [Header("�Q�[�����[�h�̃X�e�[�^�X(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public GameModeStatus gameModeStatus;

    [Header("���b�Z�[�W�e�L�X�g(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] public Text messageText;

    [Header("�`���[�g���A���p�h�L�������g")]
    [SerializeField] public GameObject tutorialDocument;

    [Header("�`���[�g���A���p�~�X�e���[�A�C�e���֘A")]
    [SerializeField] public GameObject tutorialMysteryItem01;
    [SerializeField] public GameObject tutorialMysteryItem02;

    [Header("�`���[�g���A���p�A�C�e���e�I�u�W�F�N�g")]
    [SerializeField] public GameObject tutorialItems;

    [Header("�`���[�g���A���p�t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isTutorialNextMessageFlag = false;

    [Header("�`���[�g���A���p�S�[���t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isTutorialGoalFlag = false;

    [Header("Player�X�^�~�iSlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider staminaSlider;

    [Header("PlayerCamera�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̐��񑬓x��Slider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x�ő�l(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public float maxLookSensitivity = 100f;


    [Header("Player�̎g�p�A�C�e���C���x���g���p�l���֘A")]
    [Header("�g�p�A�C�e���p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public GameObject useItemPanel;

    [Header("�g�p�A�C�e�������J�E���g�e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Text useItemCountText;

    [Header("�g�p�A�C�e���摜(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Image useItemImage;

    [Header("�g�p�A�C�e���e�L�X�g�m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public GameObject useItemTextPanel;

    [Header("�g�p�A�C�e�����e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Text useItemNameText;

    [Header("�g�p�A�C�e�������e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Text useItemExplanationText;


    [Header("�Z�[�u�E���[�h�������ϐ��֘A")]
    [Header("�Z�[�u����v���C���[��(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public static string playerName;

    [Header("�Z�[�u����v���C��(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public static int playCount = 0;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x")]
    public static float lookSensitivity = 50f;

    [Header("�Z�[�u����BGM����")]
    public static float bGMVolume = 1;

    [Header("�Z�[�u����SE����")]
    public static float sEVolume = 1;

    /// <summary>
    /// �Q�[�����[�h�X�e�[�^�X
    /// </summary>
    public enum  GameModeStatus
    {
        /// <summary>
        /// �X�g�[���[���[�h
        /// </summary>
        Story,

        /// <summary>
        /// �ʏ�v���C���[�h
        /// </summary>
        PlayInGame,

        /// <summary>
        /// �v���C���[�𑀍삵�Ȃ����[�h
        /// </summary>
        StopInGame,

        /// <summary>
        /// �Q�[���I�[�o�[���[�h
        /// </summary>
        GameOver,
    }

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //�p�����[�^�[���Z�b�g
        ResetParams();


        //�V�[���J�ڎ��p�f�[�^�����[�h
        CallLoadSceneTransitionUserDataMethod();

        if (scene.name != "GameOverScene" && mouseSensitivitySlider != null)
        {
            //�}�E�X���x��ۑ������l�ɐݒ�
            mouseSensitivitySlider.value = lookSensitivity;
        }

        if (MusicController.instance.bGMSlider != null)
        {
            //BGM��ۑ������l�ɐݒ�
            MusicController.instance.bGMSlider.value = bGMVolume;
        }

        if (MusicController.instance.sESlider != null)
        {
            //SE��ۑ������l�ɐݒ�
            MusicController.instance.sESlider.value = sEVolume;
        }
    }

    /// <summary>
    /// �p�����[�^�[���Z�b�g
    /// </summary>
    public void ResetParams() 
    {
        isTutorialNextMessageFlag = false;
        isTutorialGoalFlag = false;
    }

    private void Awake()
    {
        //�C���X�^���X����
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);


        Time.timeScale = 1;
    }

    private void Start()
    {
        //�p�����[�^�[���Z�b�g
        ResetParams();

        //�}�E�X���񑬓x��Slider�̍ő�l��ݒ�
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;

        //BGM���ʂ�Slider�̍ő�l��ݒ�
        if (MusicController.instance.bGMSlider) MusicController.instance.bGMSlider.maxValue = MusicController.instance.GetMaxBGMSliderVolume();

        //SE���ʂ�Slider�̍ő�l��ݒ�
        if (MusicController.instance.sESlider) MusicController.instance.sESlider.maxValue = MusicController.instance.GetMaxSESliderVolume();
    }

    private void Update()
    {
        //�}�E�X���x���X���C�_�[����擾
        if (mouseSensitivitySlider)
        {
            lookSensitivity = mouseSensitivitySlider.value;

            //�ő�l�𒴂��Ȃ��悤�ɐ���
            if (lookSensitivity > maxLookSensitivity) lookSensitivity = maxLookSensitivity;
        }

        //�Z�[�u�pBGM���ʂ��X���C�_�[����擾
        if (MusicController.instance.bGMSlider)
        {
            bGMVolume = MusicController.instance.bGMSlider.value;

            //BGM���ʂ��ő�l�𒴂��Ȃ��悤�ɐ���
            if (bGMVolume > MusicController.instance.GetMaxBGMSliderVolume()) bGMVolume = MusicController.instance.GetMaxBGMSliderVolume();

            //BGM���ʂ��ŏ��l�����ɂȂ�Ȃ��悤�ɐ���
            if (bGMVolume < MusicController.instance.GetMinBGMSliderVolume()) bGMVolume = MusicController.instance.GetMinBGMSliderVolume();
        }

        //�Z�[�u�pSE���ʂ��X���C�_�[����擾
        if (MusicController.instance.sESlider)
        {
            sEVolume = MusicController.instance.sESlider.value;

            //SE���ʂ��ő�l�𒴂��Ȃ��悤�ɐ���
            if (sEVolume > MusicController.instance.GetMaxSESliderVolume()) sEVolume = MusicController.instance.GetMaxSESliderVolume();

            //SE���ʂ��ŏ��l�����ɂȂ�Ȃ��悤�ɐ���
            if (sEVolume < MusicController.instance.GetMinSESliderVolume()) sEVolume = MusicController.instance.GetMinSESliderVolume();
        }
    }


    /// <summary>
    /// �Q�[�����[�h�̃X�e�[�^�X��ݒ�
    /// </summary>
    /// <param name="status">�Q�[�����[�h�X�e�[�^�X</param>
    public void SetGameModeStatus(GameModeStatus status) 
    {
        gameModeStatus = status;

        //�X�g�[���[���[�h�̏ꍇ
        if (gameModeStatus == GameModeStatus.Story) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// �f�[�^��ۑ����郁�\�b�h���Ăяo��(���i�łŎg�p����)
    /// </summary>
    public void CallSaveUserDataMethod() 
    {
        saveLoad.SaveUserData();
    }

    /// <summary>
    /// �f�[�^�����[�h���郁�\�b�h���Ăяo��(���i�łŎg�p����)
    /// </summary>
    public void CallLoadUserDataMethod() 
    {
        saveLoad.LoadUserData();
    }

    /// <summary>
    /// �V�[���J�ڎ��p�f�[�^��ۑ����郁�\�b�h���Ăяo��
    /// </summary>
    public void CallSaveSceneTransitionUserDataMethod()
    {
        saveLoad.SaveSceneTransitionUserData();
    }

    /// <summary>
    /// �V�[���J�ڎ��p�f�[�^�����[�h���郁�\�b�h���Ăяo��
    /// </summary>
    public void CallLoadSceneTransitionUserDataMethod()
    {
        saveLoad.LoadSceneTransitionUserData();
    }

    /// <summary>
    /// �f�[�^�����������郁�\�b�h���Ăяo��(���i�łŎg�p����)
    /// </summary>
    public void CallRestDataMethod() 
    {
        saveLoad.ResetUserData();
    }

    /// <summary>
    /// �Q�[���I�[�o�[��ʂ֑J�ڂ���
    /// </summary>
    public void ViewGameOver() 
    {
        // �V�[���J�ڑO�ɔ񓯊��^�X�N���L�����Z��
        if (MessageController.instance != null)
        {
            MessageController.instance.CancelAsyncTasks();
        }

        if (Player.instance.IsDead) 
        {
            //�V�[���J�ڎ��p�f�[�^��ۑ�
            CallSaveSceneTransitionUserDataMethod();

            SceneManager.LoadScene("GameOverScene");
        }
    }

    /// <summary>
    /// �^�C�g����ʂ֖߂�
    /// </summary>
    public void ReturnToTitle() 
    {
        //MessageController�̔񓯊��^�X�N���L�����Z��
        if (MessageController.instance != null)
        {
            MessageController.instance.CancelAsyncTasks();
            MessageController.instance.DestroyController();
        }

        //�v���C���[�폜�E�^�C�g���V�[���֑J��
        if (Player.instance != null)
        {
            Player.instance.DestroyPlayer();
        }
        SceneManager.LoadScene("TitleScene");

        //PauseController���폜
        if (PauseController.instance != null)
        {
            PauseController.instance.DestroyController();
        }

    }
}
