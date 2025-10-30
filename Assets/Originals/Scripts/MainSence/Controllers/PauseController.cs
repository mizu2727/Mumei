using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PauseController : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static PauseController instance;

    [Header("�v���C���[(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Player player;

    [Header("�S�[��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Goal goal;

    [Header("�G(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private BaseEnemy[] baseEnemy;

    [Header("�|�[�Y�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject pausePanel;

    [Header("�A�C�e���m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject viewItemsPanel;

    [Header("�h�L�������g�p�l���֘A")]
    [Header("�h�L�������g�m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject documentInventoryPanel;

    [Header("�h�L�������g�������p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject documentExplanationPanel;

    [Header("�h�L�������g���̃e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Text documentNameText;

    [Header("�h�L�������g�������e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Text documentExplanationText;


    [Header("�~�X�e���[�A�C�e���p�l���֘A")]
    [Header("�~�X�e���[�A�C�e���m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject mysteryItemInventoryPanel;

    [Header("�~�X�e���[�A�C�e�����̃{�^��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Button[] mysteryItemNameButton;

    [Header("�~�X�e���[�A�C�e�����̃e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Text[] mysteryItemNameText;

    [Header("�~�X�e���[�A�C�e���摜(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Image[] mysteryItemImage;

    [Header("�~�X�e���[�A�C�e���������e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Text[] mysteryItemExplanationText;

    [Header("�~�X�e���[�A�C�e���������p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject mysteryItemExplanationPanel;


    [Header("�I�v�V�����p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject optionPanel;

    [Header("���񑬓x�ݒ�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject mouseSensitivityPanel;

    [Header("���ʒ����ݒ�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject audioAdjustmentPanel;

    [Header("�^�C�g���֖߂�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject returnToTitlePanel;

    [Header("�t���O�֘A")]
    [Header("�|�[�Y�t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isPause = false;

    [Header("�A�C�e���m�F�p�l���{���t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isViewItemsPanel = false;

    [Header("�I�v�V�����p�l���{���t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isOptionPanel = false;

    /// <summary>
    /// ���񑬓x�ݒ�p�l���{���t���O
    /// </summary>
    private bool isViewMouseSensitivityPanel = false;

    /// <summary>
    /// ���ʒ����ݒ�p�l�������t���O
    /// </summary>
    private bool isViewAudioAdjustmentPanel = false;

    [Header("�^�C�g���֖߂�p�l���{���t���O(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public bool isReturnToTitlePanel = false;

    /// <summary>
    /// �h�L�������g�p�l���{���t���O
    /// </summary>
    private bool isDocumentPanel = false;

    /// <summary>
    /// �h�L�������g�������p�l���{���t���O
    /// </summary>
    private bool isDocumentExplanationPanel = false;

    /// <summary>
    /// �~�X�e���[�A�C�e���p�l���{���t���O
    /// </summary>
    private bool isMysteryItemPanel = false;

    /// <summary>
    /// �~�X�e���[�A�C�e���������p�l���{���t���O
    /// </summary>
    private bool isMysteryItemExplanationPanel = false;

    [Header("�`���[�g���A���p�n���}�[����t���O(�ҏW�֎~)")]
    public bool isGetHammer_Tutorial = false;

    [Header("�`���[�g���A���p���[�v����t���O(�ҏW�֎~)")]
    public bool isGetRope_Tutorial = false;

    [Header("�`���[�g���A���p�~�X�e���[�A�C�e���{������t���O(�ҏW�֎~)")]
    public bool isViewMysteryItem_Tutorial = false;


    /// <summary>
    /// �~�X�e���[�A�C�e��ID�̃��X�g
    /// </summary>
    private List<int> mysteryItemIds = new();

    /// <summary>
    /// �~�X�e���[�A�C�e�����̃��X�g
    /// </summary>
    private List<string> mysteryItemNames = new();

    /// <summary>
    /// �~�X�e���[�A�C�e���������̃��X�g
    /// </summary>
    private List<string> mysteryItemExplanations = new();

    /// <summary>
    /// �`���[�g���A���p�n���}�[ID
    /// </summary>
    private const int hammer_TutorialID = 9;

    /// <summary>
    /// �`���[�g���A���p���[�vID
    /// </summary>
    private const int rope_TutorialID = 10;

    /// <summary>
    /// �`���[�g���A���p�h�L�������gID
    /// </summary>
    private const int documentBook_TutorialID = 7;

    /// <summary>
    /// ����������̃h�L�������gID
    /// </summary>
    private const int defaultDocumentBookID = 99999;

    /// <summary>
    /// �h�L�������gID
    /// </summary>
    private int keepDocumentBookID;


    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;

    /// <summary>
    /// HomeScene
    /// </summary>
    private const string homeScene = "HomeScene";

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stage01 = "Stage01";


    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// ���ݍĐ�����Ă���BGM��ID
    /// </summary>
    private int nowPlayBGMId = 99999; 

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE�paudioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// �h�L�������g���̃{�^��SE��ID
    /// </summary>
    private readonly int documentNameButtonSEid = 3;

    /// <summary>
    /// �{�^��SE��ID
    /// </summary>
    private readonly int buttonSEid = 4;
    

    [Header("Input Actions")]
    public GameInput gameInput;

    /// <summary>
    /// �񓯊��^�X�N�̃L�����Z��
    /// �`���[�g���A������UniTask�����ҋ@���Ƀ|�[�Y��ʂ���^�C�g���֖߂�ۂ�messageText��MissingReferenceException�G���[���N����̂�h�~����p
    /// </summary>
    private CancellationTokenSource cts;

    /// <summary>
    /// ���ݍĐ�����Ă���BGM��ID���擾
    /// </summary>
    /// <returns>���ݍĐ�����Ă���BGM��ID</returns>
    public int GetNowPlayBGMId() 
    {
        return nowPlayBGMId;
    }

    /// <summary>
    /// �Ώۂ�BGM�ɐݒ肷��
    /// </summary>
    /// <param name="subjectPlayBGMId_">�Ώۂ�BGM</param>
    public void SetNowPlayBGMId(int subjectPlayBGMId_) 
    {
        nowPlayBGMId = subjectPlayBGMId_;
    }

    private void Awake()
    {
        //�V���O���g���̐ݒ�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyController();
        }

        //CancellationTokenSource��������
        cts = new CancellationTokenSource();

        gameInput = new GameInput();

        //�A�N�V�����ɃR�[���o�b�N��o�^
        gameInput.Gameplay.PressPlusButton.performed += OnPlusButtonPressed;

        //Input System��L���ɂ���
        gameInput.Enable(); 
    }

    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;

        //Input System��L���ɂ���
        gameInput.Enable();
    }

    private void OnDisable()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^����
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;

        //Input System�𖳌��ɂ���
        gameInput.Disable();

        //�񓯊��^�X�N���L�����Z��
        CancelAsyncTasks();
    }

    /// <summary>
    /// SE���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Player.instance != null)
        {
            //Player�̌��ʉ�����Ȃ��o�O��h�~�p�B�V�[���J�ڎ���Player�Q�Ƃ��X�V����
            player = Player.instance;
        }
        else
        {
            Debug.LogWarning("Player instance is null in scene: " + scene.name);
        }

        //�t���O�l��������
        isGetHammer_Tutorial = false;
        isGetRope_Tutorial = false;
        isViewMysteryItem_Tutorial = false;

        //�h�L�������gID��������
        keepDocumentBookID = defaultDocumentBookID;
    }

    /// <summary>
    /// �g�[�N�����L�����Z�����Ĕ񓯊��^�X�N�𒆒f
    /// </summary>
    public void CancelAsyncTasks()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }
    }

    private void Start()
    {
        audioSourceSE = MusicController.instance.GetAudioSource();

        //MusicController�Őݒ肳��Ă���SE�p��AudioMixerGroup��ݒ肷��
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;

        //�p�l����������ԂŔ�\���ɂ���
        //�t���O�l��������
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();

        isOptionPanel = false;
        ChangeOptionPanel();

        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();

        isGetHammer_Tutorial = false;
        isGetRope_Tutorial = false;
        isViewMysteryItem_Tutorial = false;

        //�~�X�e���[�A�C�e���̃{�^���ƃe�L�X�g��������
        InitializeMysteryItemUI();

        //�h�L�������gID��������
        keepDocumentBookID = defaultDocumentBookID;
    }


    
    public void Update()
    {
        //P�L�[orZ�L�[orEscape�L�[�Ń|�[�Y/�|�[�Y����
        if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape)) 
            && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) TogglePause();
    }

    /// <summary>
    /// �R���g���[���[��+�{�^���Ń|�[�Y/�|�[�Y����
    /// </summary>
    /// <param name="context"></param>
    private void OnPlusButtonPressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// �|�[�Y��ʂ̕\��/��\����؂�ւ���
    /// </summary>
    private void TogglePause()
    {
        //�|�[�Y���J������
        if (!player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanel && !isDocumentExplanationPanel && !isMysteryItemPanel
            && !isMysteryItemExplanationPanel && !goal.isGoalPanel && Time.timeScale != 0)
        {
            ViewPausePanel();
        }
        //�|�[�Y��������
        else if (!player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
        }
    }

    /// <summary>
    /// �}�E�X�J�[�\����\�����A�Œ���������郁�\�b�h
    /// </summary>
    void ViewMouseCorsor() 
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// �}�E�X���\���ɂ��A�Œ肷�郁�\�b�h
    /// </summary>
    void HideMouseCorsor() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// �|�[�Y
    /// </summary>
    public void ViewPausePanel() 
    {
        //�|�[�Y�t���O���I��
        isPause = true;

        //�ꎞ��~
        Time.timeScale = 0;

        //UI�̃��C���[����O���ɂ���
        pausePanel.transform.SetAsLastSibling();

        //�p�l���\��
        ChangeViewPausePanel();

        //�}�E�X�J�[�\����\�����A�Œ������
        ViewMouseCorsor();

        //���݂̃V�[�������擾���A���̖��O�ɂ���Ĉꎞ��~����BGM�����߂�
        switch (SceneManager.GetActiveScene().name) 
        {
            //HomeScene
            case homeScene:
                //HomeSceneBGM���ꎞ��~
                MusicController.instance.PauseBGM(HomeController.instance.GetAudioSourceBGM(),
                    sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());
                break;

            //Stage01
            case stage01:
                //���ݗ���Ă���BGM���X�e�[�WBGM�Ȃ̂��G�ɒǂ��Ă���BGM�Ȃ̂��𔻕ʂ���
                if (nowPlayBGMId == EnemyBGMController.instance.GetChasePlayerBGMId())
                {
                    //�v���C���[���G�ɒǂ���ۂ�BGM���ꎞ��~
                    MusicController.instance.PauseBGM(EnemyBGMController.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(EnemyBGMController.instance.GetChasePlayerBGMId()), EnemyBGMController.instance.GetChasePlayerBGMId());
                }
                else 
                {
                    //Stage01BGM���ꎞ��~
                    MusicController.instance.PauseBGM(Stage01Controller.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId()), Stage01Controller.instance.GetStage01BGMId());
                }
                break;

            default:
                Debug.LogWarning("���̑��̃V�[����");
                break;
        };

        //BGM�ꎞ��~
        //MusicController.instance.PauseBGM();

        //�Đ����̌��ʉ���S�Ĉꎞ��~���A�{�^��SE�𗬂�
        if (Player.instance != null && Player.instance.audioSourceSE != null)
        {
            MusicController.instance.PauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);
        }
        else
        {
            Debug.LogWarning("Player or AudioSource is null in ViewPausePanel");
        }

        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            if (baseEnemy[i] != null && baseEnemy[i].audioSourceSE != null)
            {
                MusicController.instance.PauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }
        }

        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
    }

    /// <summary>
    /// �|�[�Y����
    /// </summary>
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            //�ꎞ��~�J��
            Time.timeScale = 1;

            //�|�[�Y�t���O���I�t
            isPause = false;

            //�p�l����\��
            ChangeViewPausePanel();

            //�}�E�X���\���ɂ��A�Œ肷��
            HideMouseCorsor();

            //�{�^��SE�𗬂��A�C���Q�[������BGM�ESE�̈ꎞ��~��S�ĉ�������
            MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
            MusicController.instance.UnPauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);

            for (int i = 0; i < baseEnemy.Length; i++)
            {
                if (baseEnemy[i] != null) MusicController.instance.UnPauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }


            //���݂̃V�[�������擾���A���̖��O�ɂ���Ĉꎞ��~��������BGM�����߂�
            switch (SceneManager.GetActiveScene().name)
            {
                //HomeScene
                case homeScene:
                    //HomeSceneBGM���ꎞ��~����
                    MusicController.instance.UnPauseBGM(HomeController.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());
                    break;

                //Stage01
                case stage01:
                    //���݈ꎞ��~���Ă���BGM���X�e�[�WBGM�Ȃ̂��G�ɒǂ��Ă���BGM�Ȃ̂��𔻕ʂ���
                    if (nowPlayBGMId == EnemyBGMController.instance.GetChasePlayerBGMId())
                    {
                        //�v���C���[���G�ɒǂ���ۂ�BGM���ꎞ��~����
                        MusicController.instance.UnPauseBGM(EnemyBGMController.instance.GetAudioSourceBGM(),
                            sO_BGM.GetBGMClip(EnemyBGMController.instance.GetChasePlayerBGMId()), EnemyBGMController.instance.GetChasePlayerBGMId());
                    }
                    else
                    {
                        //Stage01BGM���ꎞ��~����
                        MusicController.instance.UnPauseBGM(Stage01Controller.instance.GetAudioSourceBGM(),
                            sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId()), Stage01Controller.instance.GetStage01BGMId());
                    }
                    break;

                default:
                    Debug.LogWarning("���̑��̃V�[����(UnPauseBGM)");
                    break;
            };

            //MusicController.instance.UnPauseBGM();
        }
        
    }

    /// <summary>
    /// �u�A�C�e���m�F�v�{�^������
    /// </summary>
    public void OnClickedViewItemButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y�p�l�����\���ɂ��A�A�C�e���m�F�p�l����\������
        isPause = false;
        ChangeViewPausePanel();

        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    /// <summary>
    /// �u�I�v�V�����v�{�^������
    /// </summary>
    public void OnClickedOptionButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y�p�l�����\���ɂ��A�I�v�V�����p�l����\������
        isOptionPanel = true;
        ChangeOptionPanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    /// <summary>
    /// �u���񑬓x�v�{�^������
    /// </summary>
    public void OnClickedMouseSensitivityButton() 
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //���̐ݒ�p�l�����\���ɂ���
        //���ʒ����ݒ�p�l�����\��
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //���񑬓x�ݒ�p�l����\��
        isViewMouseSensitivityPanel = true;
        ChangeMouseSensitivityPanel();
    }

    /// <summary>
    /// �u���ʒ����v�{�^������
    /// </summary>
    public void OnClickedAudioAdjustmentButton() 
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //���̐ݒ�p�l�����\���ɂ���
        //���񑬓x�ݒ�p�l�����\���ɂ���
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();


        //���ʒ����ݒ�p�l����\��
        isViewAudioAdjustmentPanel = true;
        ChangeAudioAdjustmentPanel();
    }

    /// <summary>
    /// �u�߂�v�{�^������
    /// �I�v�V�����ݒ肩��|�[�Y��ʂ֖߂�
    /// </summary>
    public void OnClickedFromOptionToPauseButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y�p�l����\��
        isPause = true;
        ChangeViewPausePanel();

        //���񑬓x�ݒ�p�l�����\��
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //���ʒ����ݒ�p�l�����\��
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //�I�v�V�����p�l�����\��
        isOptionPanel = false;
        ChangeOptionPanel();
    }

    /// <summary>
    /// �u�^�C�g���֖߂�v�{�^������
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y�p�l�����\���ɂ��A�^�C�g���֖߂�p�l����\������
        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    /// <summary>
    /// �u�͂��v����
    /// </summary>
    public void OnClickedYesButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�^�C�g����ʂ֑J��
        GameController.instance.ReturnToTitle();
    }

    /// <summary>
    /// �u�������v����
    /// </summary>
    public void OnClickedNoButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y�p�l����\���ɂ��A�^�C�g���֖߂�p�l�����\������
        isPause = true;
        ChangeViewPausePanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }


    /// <summary>
    /// �u�h�L�������g�v�{�^������
    /// </summary>
    public void OnClickedViewDocumentButton() 
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�h�L�������g�p�l����\��
        isDocumentPanel = true;
        ChangeViewDocumentPanel();

        //�~�X�e���[�A�C�e���p�l�����\��
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// �u�~�X�e���[�A�C�e���v�{�^������
    /// </summary>
    public void OnClickedViewMysteryItemButton()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        // �~�X�e���[�A�C�e���p�l����\��
        isMysteryItemPanel = true;
        ChangeViewMysteryItemPanel();

        // �h�L�������g�p�l�����\��
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //�摜�Ɛ����e�L�X�g���N���A
        if (mysteryItemImage.Length > 0)
        {
            mysteryItemImage[0].sprite = null;
            mysteryItemImage[0].enabled = false;
        }
        if (mysteryItemExplanationText.Length > 0)
        {
            mysteryItemExplanationText[0].text = "";
        }
    }


    /// <summary>
    /// �u�߂�v�{�^������
    /// �|�[�Y��ʂ֖߂�
    /// </summary>
    public void OnClickedReturnToPausePanel()
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�|�[�Y��ʂ�\��
        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        //�A�C�e���m�F�p�l�����\��
        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        //�h�L�������g�p�l�����\��
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //�~�X�e���[�A�C�e���p�l�����\��
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// �h�L�������g���̃{�^��������
    /// </summary>
    public void OnClickedDocumentNameButton() 
    {
        //�h�L�������g���̃{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(documentNameButtonSEid));

        //�h�L�������g�̐�����\��
        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();

        //���肵���h�L�������g���`���[�g���A���p�̏ꍇ
        if (keepDocumentBookID == documentBook_TutorialID) 
        {
            //�t���O�l���I��
            GameController.instance.isTutorialNextMessageFlag = true;
        }
    }

    /// <summary>
    /// �|�[�Y�p�l���̕\��/��\��
    /// </summary>
    void ChangeViewPausePanel()
    {
        if (isPause)
        {
            //�\��
            pausePanel.SetActive(true);
        }
        else
        {
            //��\��
            pausePanel.SetActive(false);
        }
    }


    /// <summary>
    /// �A�C�e���m�F�p�l���̕\��/��\��
    /// </summary>
    void ChangeViewItemsPanel() 
    {
        if (isViewItemsPanel)
        {
            //�\��
            viewItemsPanel.SetActive(true);
        }
        else
        {
            //��\��
            viewItemsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// �I�v�V�����p�l���̕\��/��\��
    /// </summary>
    void ChangeOptionPanel()
    {
        if (isOptionPanel)
        {
            //�\��
            optionPanel.SetActive(true);
        }
        else
        {
            //��\��
            optionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ���񑬓x�ݒ�p�l���̕\��/��\��
    /// </summary>
    void ChangeMouseSensitivityPanel()
    {
        if (isViewMouseSensitivityPanel)
        {
            //�\��
            mouseSensitivityPanel.SetActive(true);
        }
        else
        {
            //��\��
            mouseSensitivityPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ���ʒ����ݒ�p�l���̕\��/��\��
    /// </summary>
    void ChangeAudioAdjustmentPanel()
    {
        if (isViewAudioAdjustmentPanel)
        {
            //�\��
            audioAdjustmentPanel.SetActive(true);
        }
        else
        {
            //��\��
            audioAdjustmentPanel.SetActive(false);
        }
    }

    /// <summary>
    /// �^�C�g���֖߂�p�l���̕\��/��\��
    /// </summary>
    void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            //�\��
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            //��\��
            returnToTitlePanel.SetActive(false);
        }
    }

    /// <summary>
    /// �h�L�������g�p�l���̕\��/��\��
    /// </summary>
    void ChangeViewDocumentPanel() 
    {
        if (isDocumentPanel)
        {
            //UI�̃��C���[����O���ɂ���
            documentInventoryPanel.transform.SetAsLastSibling();

            //�\��
            documentInventoryPanel.SetActive(true);

        }
        else 
        {
            //��\��
            documentInventoryPanel.SetActive(false);

            //�h�L�������g�������p�l�����\��
            isDocumentExplanationPanel = false;
            ChangeViewDocumentExplanationPanel();
        }
    }

    /// <summary>
    /// �h�L�������g�������p�l���̕\��/��\��
    /// </summary>
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            //�\��
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            //��\��
            documentExplanationPanel.SetActive(false);
        }
    }


    /// <summary>
    /// DocumentNameText�̋L�ړ��e��ύX
    /// </summary>
    /// <param name="documentId">�擾����id</param>
    /// <param name="documentName">�ύX��̋L�ړ��e</param>
    public void ChangeDocumentNameText(int documentId, string documentName) 
    {
        //�`���[�g���A���p�h�L�������g�̏ꍇ
        if (documentId == documentBook_TutorialID) 
        {
            //�t���O�l���I��
            GameController.instance.isTutorialNextMessageFlag = true;

            //ID��ۑ�
            keepDocumentBookID = documentId;
        }

        //�V�[�����Ŏ擾�����h�L�������g�I�u�W�F�N�g�̖��O��ۑ�
        documentNameText = documentNameText.GetComponent<Text>();
        documentNameText.text = documentName;
    }

    /// <summary>
    /// DocumentExplanationText�̋L�ړ��e��ύX
    /// </summary>
    /// <param name="documentDescription"></param>
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        //�V�[�����Ŏ擾�����h�L�������g�I�u�W�F�N�g�̐�����ۑ�
        documentExplanationText = documentExplanationText.GetComponent<Text>();
        documentExplanationText.text = documentDescription;
    }

    /// <summary>
    /// �~�X�e���[�A�C�e���m�F�p�l���̕\��/��\��
    /// </summary>
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanel)
        {
            //UI�̃��C���[����O���ɂ���
            mysteryItemInventoryPanel.transform.SetAsLastSibling();

            //�\��
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            //��\��
            mysteryItemInventoryPanel.SetActive(false);

            //�~�X�e���[�A�C�e�����������\��
            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();

            //�摜�Ɛ����e�L�X�g�����Z�b�g
            if (mysteryItemImage.Length > 0)
            {
                mysteryItemImage[0].sprite = null;
                mysteryItemImage[0].enabled = false;
            }
            if (mysteryItemExplanationText.Length > 0)
            {
                mysteryItemExplanationText[0].text = "";
            }
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e���������p�l���̕\��/��\��
    /// </summary>
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            //�\��
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            //��\��
            mysteryItemExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e����UI��������
    /// </summary>
    private void InitializeMysteryItemUI()
    {
        //null�`�F�b�N
        if (mysteryItemNameButton == null || mysteryItemNameText == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        //�{�^���ɃN���b�N�C�x���g��ǉ�
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            //���[�J���ϐ��ŃC���f�b�N�X���L���v�`��
            int index = i; 

            //�N���b�N�C�x���g��ǉ�
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));

            //���肵�Ă��Ȃ��A�C�e�����̏����\����"?????????"�ɂ���
            mysteryItemNameText[i].text = "?????????";
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e�����̃{�^��������
    /// </summary>
    /// <param name="index">�C���f�b�N�X�ԍ�</param>
    public void OnClickedMysteryItemNameButton(int index)
    {
        //�{�^��SE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //�`���[�g���A���p�~�X�e���[�A�C�e����S�ē��肵���ꍇ
        if (isGetHammer_Tutorial && isGetRope_Tutorial)
        {
            //�t���O�l���I��
            isViewMysteryItem_Tutorial = true;
        }

        if (index < mysteryItemNames.Count)
        {
            //���肵���~�X�e���[�A�C�e�������X�g���ɑ��݂��邩���m�F
            string itemName = mysteryItemNames[index];
            var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

            if (item != null)
            {
                //�~�X�e���[�A�C�e�������p�l����\��
                isMysteryItemExplanationPanel = true;
                ChangeViewMysteryItemExplanationPanel();

                //�h�L�������g�����p�l�����\���ɂ���
                isDocumentExplanationPanel = false;
                ChangeViewDocumentExplanationPanel();

                //�����e�L�X�g���X�V
                if (mysteryItemExplanationText.Length > 0)
                {
                    mysteryItemExplanationText[0].text = item.description;
                }

                //�摜���X�V
                if (mysteryItemImage.Length > 0)
                {
                    mysteryItemImage[0].sprite = item.icon;
                    mysteryItemImage[0].enabled = (item.icon != null);
                }
                else
                {
                    Debug.LogWarning("mysteryItemImage �����ݒ�ł�");
                }
            }
            else
            {
                Debug.LogError($"�A�C�e�� '{itemName}' ��������܂���");
            }
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e������ǉ����AUI�ɔ��f
    /// </summary>
    /// <param name="mysteryItemID">�A�C�e��ID</param>
    /// <param name="mysteryItemName">�A�C�e����</param>
    /// <param name="mysteryItemDescription">�A�C�e������</param>
    public void ChangeMysteryItemTexts(int mysteryItemID, string mysteryItemName, string mysteryItemDescription)
    {
        //ID���X�g�ɒǉ�
        mysteryItemIds.Add(mysteryItemID);

        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            //�`���[�g���A���p�n���}�[�̏ꍇ
            if (mysteryItemIds[i] == hammer_TutorialID) 
            {
                //�t���O�l���I��
                isGetHammer_Tutorial = true;
            }

            //�`���[�g���A���p���[�v�̏ꍇ
            if (mysteryItemIds[i] == rope_TutorialID)
            {
                //�t���O�l���I��
                isGetRope_Tutorial = true;
            }
        }

        //�A�C�e�����X�g����Y������A�C�e��������
        var item = sO_Item.itemList.Find(x => x.itemName == mysteryItemName && x.itemType == ItemType.MysteryItem);
        if (item != null && !mysteryItemNames.Contains(mysteryItemName))
        {
            //�A�C�e�������X�g�ɒǉ�
            mysteryItemNames.Add(mysteryItemName);

            //�A�C�e���������X�g�ɒǉ�
            mysteryItemExplanations.Add(mysteryItemDescription);

            //UI�ɔ��f������
            UpdateMysteryItemUI();
        }
        else 
        {
            Debug.LogWarning($"MysteryItem '{mysteryItemName}' ��������Ȃ����A���łɒǉ��ς݂ł�");
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e����UI���X�V
    /// </summary>
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            if (i < mysteryItemNames.Count)
            {
                //���肵���~�X�e���[�A�C�e�������X�g���ɑ��݂��邩���m�F
                string itemName = mysteryItemNames[i];
                var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

                if (item != null)
                {
                    //�{�^���ɕ\�������e�L�X�g��"?????????"����~�X�e���[�A�C�e�����ɕύX����
                    mysteryItemNameText[i].text = itemName;

                    //�{�^���N���b�N��L��
                    mysteryItemNameButton[i].interactable = true;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //�������e�L�X�g�Ƀ~�X�e���[�A�C�e�������𔽉f������
                        mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //�~�X�e���[�A�C�e���摜�𔽉f������
                        mysteryItemImage[i].sprite = item.icon;
                        mysteryItemImage[i].enabled = (item.icon != null);
                    }
                }
                else
                {
                    Debug.LogWarning($"�A�C�e�� '{itemName}' ��������܂���");

                    //�{�^���ɕ\�������e�L�X�g��"?????????"�ɂ���
                    mysteryItemNameText[i].text = "?????????";

                    //�{�^���N���b�N�𖳌�
                    mysteryItemNameButton[i].interactable = false;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //�������e�L�X�g����ɂ���
                        mysteryItemExplanationText[i].text = "";
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //�~�X�e���[�A�C�e���摜��null����
                        mysteryItemImage[i].sprite = null;
                        mysteryItemImage[i].enabled = false;
                    }
                }
            }
            else
            {
                //�{�^���ɕ\�������e�L�X�g��"?????????"�ɂ���
                mysteryItemNameText[i].text = "?????????";

                //�{�^���N���b�N�𖳌�
                mysteryItemNameButton[i].interactable = false;

                if (i < mysteryItemExplanationText.Length)
                {
                    //�������e�L�X�g����ɂ���
                    mysteryItemExplanationText[i].text = "";
                }

                if (i < mysteryItemImage.Length)
                {
                    //�~�X�e���[�A�C�e���摜��null����
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// ���̃R���g���[���[��j������
    /// </summary>
    public void DestroyController() 
    {
        CancelAsyncTasks();
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();
        Destroy(gameObject);
    }

    /// <summary>
    /// �I�u�W�F�N�g���j�������ۂɌĂ΂��
    /// </summary>
    private void OnDestroy()
    {
        //�������̃C���X�^���X���V���O���g���C���X�^���X���g�ł���΁Astatic�ȎQ�Ƃ��N���A����
        if (instance == this)
        {
            instance = null;
        }
    }
}
