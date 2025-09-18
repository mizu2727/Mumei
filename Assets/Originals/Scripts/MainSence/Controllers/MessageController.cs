using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static UnityEngine.Rendering.DebugUI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("���b�Z�[�W�p�l���֘A(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] public Text messageText;
    [SerializeField] private GameObject CheckInputNamePanel;
    [SerializeField] private Text CheckInputNameText;

    [Header("�u���b�N�A�E�g(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject blackOutPanel;

    [Header("���b�Z�[�W�������X�s�[�h�B���l���������قǑf��������")]
    [SerializeField] private float writeSpeed = 0;
    private bool isWrite = false;//�����Ă�r���ł��邩�𔻒�

    [Header("�V�X�e�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private SystemMessage systemMessage;

    [Header("�V�X�e�����b�Z�[�W�\���@�\(Prefab���A�^�b�`)")]
    [SerializeField] private ShowSystemMessage showSystemMessage;

    [Header("��b���b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private TalkMessage talkMessage;

    [Header("��b���b�Z�[�W�\���@�\(Prefab���A�^�b�`)")]
    [SerializeField] private ShowTalkMessage showTalkMessage;

    [Header("�S�[�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("�v���C���[�̖��O����(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] public InputField inputPlayerNameField;

    [Header("�v���C���[�J����(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private PlayerCamera playerCamera;

    [Header("�S�[��(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] public Goal goal;

    [Header("wall_Tutorial(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject wall_Tutorial;

    [Header("�C���x���g�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private InventoryMessage inventoryMessage;

    [Header("���b�Z�[�W�p�l������")]
    public bool isMessagePanel = false;

    [Header("�u���b�N�A�E�g����")]
    public bool isBlackOutPanel = false;


    [Header("�T�E���h�֘A")]
    [SerializeField] private AudioClip noiseSE;
    private AudioSource audioSourceSE;

    // �񓯊��^�X�N�̃L�����Z���p(�`���[�g���A������UniTask�����ҋ@���Ƀ|�[�Y��ʂ���^�C�g���֖߂�ۂ�messageText��MissingReferenceException�G���[���N����̂�h�~����p)
    private CancellationTokenSource cts; 

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // �񓯊��^�X�N���L�����Z��
        CancelAsyncTasks(); 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (messageText == null)
        {
            Debug.LogWarning("MessageController��messageText��null�̂��߁AmessageText = messageText�����s");
            messageText = GameController.instance.messageText;
        }

        if (messageText != null) messageText = GameController.instance.messageText;
        else Debug.LogError("GameController��messageText���ݒ肳��Ă��܂���");

        if (messageText == null)
        {
            Debug.LogError("MessageController��messageText��null");
        }
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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyController();
        }

        // CancellationTokenSource��������
        cts = new CancellationTokenSource(); 

        ResetMessage();

        
        if(inputPlayerNameField != null) 
        {
            inputPlayerNameField = inputPlayerNameField.GetComponent<InputField>();
            inputPlayerNameField.gameObject.SetActive(false);
        }

        if (CheckInputNamePanel != null) 
        {
            CheckInputNamePanel.SetActive(false);
        }

        if (CheckInputNameText != null) 
        {
            CheckInputNameText.text = "";
        }


        isBlackOutPanel = false;
    }

    private void Start()
    {
        //MusicController��Awake�֐��̏�����ɌĂ΂��悤�ɂ��邽�߁A
        //Start�֐�����AudioSource���擾����
        audioSourceSE = MusicController.Instance.GetAudioSource();

        //if (messageText == null) 
        //{
        //    Debug.LogWarning("messageText��null�̂��߁AmessageText = messageText�����s");
        //    messageText = messageText;
        //} 

        //if (messageText != null) messageText = messageText;
        //else Debug.LogError("GameController��messageText���ݒ肳��Ă��܂���");
    }

    //���b�Z�[�W�p�l���̕\���E��\��
    public void ViewMessagePanel()
    {
        if (isMessagePanel)
        {
            messagePanel.SetActive(true);
        }
        else
        {
            messagePanel.SetActive(false);
        }
    }

    //�u���b�N�A�E�g�p�l���̕\���E��\��
    public void ViewBlackOutPanel()
    {
        if (isBlackOutPanel)
        {
            blackOutPanel.SetActive(true);
        }
        else
        {
            blackOutPanel.SetActive(false);
        }
    }

    //���b�Z�[�W�����Z�b�g
    public void ResetMessage()
    {
        messageText.color = Color.white;
        messageText.text = "";
        isMessagePanel = false;
        ViewMessagePanel();
    }

    //�e�L�X�g���ꕶ�����\��
    async void Write(string s)
    {
        // ���ɏ������ݒ��̏ꍇ�́A�������Ȃ�
        if (isWrite) return;

        isWrite = true;
        messageText.text = ""; // ����e�L�X�g���N���A���Ă��珑���n�߂�

        for (int i = 0; i < s.Length; i++)
        {
            // �������ݑ��x��0�̏ꍇ�A��C�ɕ\��
            if (writeSpeed <= 0)
            {
                messageText.text = s;
                break;
            }

            messageText.text += s.Substring(i, 1);
            await UniTask.Delay(TimeSpan.FromSeconds(writeSpeed));
        }
        isWrite = false;
    }

    //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W��\��
    async UniTask ShowNextMessage() 
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        //�|�[�Y�p�l���E�S�[���p�l�����J���Ă���Ԃ́A���̃��b�Z�[�W��i�߂Ȃ�
        if (goal != null && PauseController.instance != null) 
        {
            if (PauseController.instance.isPause || goal.isGoalPanel)
            {
                await ShowNextMessage();
            }
        }     
    }



    //��b���b�Z�[�W��\��
    public async UniTask ShowTalkMessage(int number)
    {
        // ���Ƀ��b�Z�[�W�������Ă�r���ł���ꍇ�́A�ȍ~�̏����𒆒f
        if (isWrite)
        {
            writeSpeed = 0; // �������ݑ��x���グ�č����\��
            return;
        }

        //�O�̃��b�Z�[�W�������Ă�r���ł��邩�𔻒f�B�����r���Ȃ�true
        if (Time.timeScale == 1)
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //�b���Ă���l���J�i���̏ꍇ�A���͂̐F���V�A���F�ɐݒ�
            if (talkMessage.talkMessage[number].speakerName == "�J�i��") messageText.color = Color.cyan;
            else messageText.color = Color.white;
            


            //�G�N�Z���f�[�^�^.���X�g�^[�ԍ�].�J������
            Write(talkMessage.talkMessage[number].message);

            //�`���[�g���A���̕ǂ������ăS�[���I�u�W�F�N�g��������悤�ɂ���
            if (wall_Tutorial != null && number == 47) wall_Tutorial.SetActive(false);

            await ShowNextMessage();

                switch (number)
                {
                    case 2:
                        messageText.text = "";
                        number++;

                        Player.instance.playerIsBackRotate = true;

                        await UniTask.Delay(TimeSpan.FromSeconds(3));

                        //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                        showTalkMessage.ShowGameTalkMessage(number);
                        //Debug.Log("Player.instance.playerName" + Player.instance.playerName);

                        break;

                    case 11:
                    case 19:
                    case 60:
                        messageText.text = "";
                        number++;

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        MusicController.Instance.PauseBGM();

                        MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                        // �F��ԐF�ɐݒ�
                        messageText.color = Color.red;

                        Write(talkMessage.talkMessage[number].message);

                        await ShowNextMessage();


                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        MusicController.Instance.UnPauseBGM();

                        messageText.text = "";
                        number++;

                        showTalkMessage.ShowGameTalkMessage(number);
                        break;

                    case 27:
                    case 33:
                        messageText.text = "";
                        number++;

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                        showTalkMessage.ShowGameTalkMessage(number);
                        break;


                    //��b�I�����ă`���[�g���A���ɓ���
                    case 36:
                        ResetMessage();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        isBlackOutPanel = true;
                        ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        Kaname.instance.WarpPostion(1, 0.505f, 7);

                        ////�`���[�g���A���p�h�L�������g��\��
                        GameController.instance.tutorialDocument.SetActive(true);

                        isBlackOutPanel = false;
                        ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        //�V�X�e�����b�Z�[�W
                        showSystemMessage.ShowGameSystemMessage(9);

                        break;

                    case 42:
                        ResetMessage();

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        showSystemMessage.ShowGameSystemMessage(10);


                        break;

                    case 43:
                        ResetMessage();

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        showSystemMessage.ShowGameSystemMessage(11);


                        break;

                    //�`���[�g���A���~�X�e���[�A�C�e����\��
                    case 45:
                        ResetMessage();

                        GameController.instance.tutorialMysteryItem01.SetActive(true);
                        GameController.instance.tutorialMysteryItem02.SetActive(true);

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        showSystemMessage.ShowGameSystemMessage(12);

                        break;

                    case 46:
                        ResetMessage();

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        showSystemMessage.ShowGameSystemMessage(13);


                        break;


                    case 48:
                        ResetMessage();

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        showSystemMessage.ShowGameSystemMessage(14);

                        break;

                    case 68:
                        ResetMessage();

                        Debug.Log("�`���[�g���A����b�I��");

                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        //�X�e�[�W1�ֈړ�
                        SceneManager.LoadScene("Stage01");

                        break;

                    case 70:
                        messageText.text = "";
                        number++;

                        isBlackOutPanel = false;
                        ViewBlackOutPanel();

                        //�s����BGM�𗬂�
                        //MusicController.Instance.PlayBGM();

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                        showTalkMessage.ShowGameTalkMessage(number);

                        break;

                    case 86:
                        messageText.text = "";
                        number++;

                        //�s����BGM���~�߂�
                        MusicController.Instance.StopBGM();



                        isBlackOutPanel = true;
                        ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                        // �F��ԐF�ɐݒ�
                        messageText.color = Color.red;

                        Write(talkMessage.talkMessage[number].message);

                        await ShowNextMessage();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        ResetMessage();


                        Debug.Log(showTalkMessage);
                        GameClearController.instance.ViewGameClearUI();

                        isBlackOutPanel = false;
                        ViewBlackOutPanel();

                        break;



                    //���b�Z�[�W�ԍ��ɑΉ����Ă��郁�b�Z�[�W���L�ځ����̃��b�Z�[�W�ԍ���p��
                    default:
                        messageText.text = "";
                        number++;

                        //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                        showTalkMessage.ShowGameTalkMessage(number);
                        break;
                }
        }
    }


    //�V�X�e�����b�Z�[�W��\��
    public async UniTask ShowSystemMessage(int number)
    {
        // ���Ƀ��b�Z�[�W�������Ă�r���ł���ꍇ�́A�ȍ~�̏����𒆒f
        if (isWrite)
        {
            writeSpeed = 0; // �������ݑ��x���グ�č����\��
            return;
        }

        //�O�̃��b�Z�[�W�������Ă�r���ł��邩�𔻒f�B�����r���Ȃ�true
        if (Time.timeScale == 1)
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //�G�N�Z���f�[�^�^.���X�g�^[�ԍ�].�J������
            Write(systemMessage.systemMessage[number].message);

            switch (number)
            {
                //���O����UI��\��
                case 3:
                    await ShowNextMessage();

                    ResetMessage();

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;

                    inputPlayerNameField.gameObject.SetActive(true);

                    break;

                //���O���͐����Ɉ�������������ɁA������x���O����UI��\��
                case 4:
                    await ShowNextMessage();

                    ResetMessage();
                    showSystemMessage.ShowGameSystemMessage(3);

                    break;

                case 6:
                    await ShowNextMessage();

                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // �F��ԐF�ɐݒ�
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await ShowNextMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //�z�[���ֈړ�
                    SceneManager.LoadScene("HomeScene");
                    break;

                //��b���b�Z�[�W��\��
                case 9:
                    await ShowNextMessage();

                    ResetMessage();

                    showTalkMessage.ShowGameTalkMessage(38);

                    break;

                case 10:
                    //�h�L�������g(�`���[�g���A����)���肵���烁�b�Z�[�W�����߂�
                    await UniTask.WaitUntil(() => GameController.instance.isTutorialNextMessageFlag);
                    GameController.instance.isTutorialNextMessageFlag = false;

                    //���N���b�N�c�h�L�������g���葀��̐���
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(43);

                    break;

                case 11:
                    //�h�L�������g(�`���[�g���A����)���{����Ƀ|�[�Y���������烁�b�Z�[�W�����߂�
                    await UniTask.WaitUntil(() => GameController.instance.isTutorialNextMessageFlag && !PauseController.instance.isPause && !PauseController.instance.isViewItemsPanel);
                    GameController.instance.isTutorialNextMessageFlag = false;

                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(44);

                    break;

                case 12:
                    //�~�X�e���[�A�C�e��(�`���[�g���A����)���肵���烁�b�Z�[�W�����߂�
                    await UniTask.WaitUntil(() => PauseController.instance.isGetHammer_Tutorial && PauseController.instance.isGetRope_Tutorial);

                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(46);

                    break;

                case 13:
                    //�~�X�e���[�A�C�e��(�`���[�g���A����)���{����Ƀ|�[�Y���������烁�b�Z�[�W�����߂�
                    await UniTask.WaitUntil(() =>  !PauseController.instance.isPause 
                    && !PauseController.instance.isViewItemsPanel && PauseController.instance.isViewMysteryItem_Tutorial);
                    PauseController.instance.isViewMysteryItem_Tutorial = false;
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(47);

                    break;

                //�`���[�g���A���I��
                case 14:
                    //�`���[�g���A���p�S�[���̉{���I�������烁�b�Z�[�W�����߂�
                    Debug.Log("MessageController�`���[�g���A���p�S�[���̉{���O");
                    await UniTask.WaitUntil(() => !PauseController.instance.isPause
                    && GameController.instance.isTutorialGoalFlag);
                    Debug.Log("MessageController�`���[�g���A���p�S�[���̉{���I��");
                    GameController.instance.isTutorialGoalFlag = false;

                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    wall_Tutorial.SetActive(true);

                    Kaname.instance.WarpPostion(1, 0.505f, 2);

                    Player.instance.PlayerWarp(1, 0.562f, 0);

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    // �J�����̊p�x�����Z�b�g
                    if (Player.instance != null)
                    {
                        if (playerCamera != null)
                        {
                            // �J�����̏㉺��]�����Z�b�g
                            playerCamera.ResetCameraRotation();
                            // �v���C���[�̌������J�����̐��ʂɓ����i�K�v�ɉ����āj
                            Player.instance.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        }
                    }

                    GameController.instance.tutorialItems.SetActive(false);

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showSystemMessage.ShowGameSystemMessage(number);

                    break;


                case 15:
                    await ShowNextMessage();

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showTalkMessage.ShowGameTalkMessage(49);

                    break;


                //���b�Z�[�W�ԍ��ɑΉ����Ă��郁�b�Z�[�W���L�ځ����̃��b�Z�[�W�ԍ���p��
                default:
                    await ShowNextMessage();

                    messageText.text = "";
                    number++;

                    //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                    showSystemMessage.ShowGameSystemMessage(number);
                    break;
            }
        }
    }

    //�S�[�����b�Z�[�W��\��
    public void ShowGoalMessage(int number) 
    {
        Debug.Log("�S�[�����b�Z�[�W�X�^�[�g");
        messageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }

    /// <summary>
    /// �C���x���g�����b�Z�[�W��\��
    /// </summary>
    /// <param name="number">���b�Z�[�W�ԍ�</param>
    public void ShowInventoryMessage(int number) 
    {
        Debug.Log("�C���x���g�����b�Z�[�W�X�^�[�g"); 
        messageText.text = inventoryMessage.inventoryMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }

    /// <summary>
    /// �v���C���[�̖��O�̓��͂����������ۂɌĂ΂��
    /// </summary>
    /// <param name="playerName">���͂����v���C���[��</param>
    public void SavePlayerName(string playerName)
    {
        if ((1 < playerName.Length) && (playerName.Length < 11)) 
        {
            //���O���ꎞ�I�ɕۑ�
            inputPlayerNameField.text = playerName;

            //�m�F�p�e�L�X�g�ɓ��͂������O��\��
            CheckInputNameText.text = inputPlayerNameField.text + " �ł�낵���ł����H";

            //���͓��e�m�F�p�l����\��
            CheckInputNamePanel.SetActive(true);

            inputPlayerNameField.gameObject.SetActive(false);       
        }
        else
        {
            inputPlayerNameField.gameObject.SetActive(false);
            showSystemMessage.ShowGameSystemMessage(4);
        }
    }

    /// <summary>
    /// ���͓��e�m�F�p�l���Łu�͂��v�{�^���������ꂽ�ۂɌĂ΂��
    /// </summary>
    public void OnClickedYesCheckInputNameButton() 
    {
        //���O��ۑ�
        GameController.playerName = inputPlayerNameField.text;

        CheckInputNamePanel.SetActive(false);

        //���̃V�X�e�����b�Z�[�W��\��
        showSystemMessage.ShowGameSystemMessage(5);
    }

    /// <summary>
    /// ���͓��e�m�F�p�l���Łu�������v�{�^���������ꂽ�ۂɌĂ΂��
    /// </summary>
    public void OnClickedNoCheckInputNameButton() 
    {
        CheckInputNamePanel.SetActive(false);
        inputPlayerNameField.gameObject.SetActive(true);
    }

    /// <summary>
    /// ���̃R���g���[���[��j������
    /// </summary>
    public void DestroyController()
    {
        CancelAsyncTasks();
        messageText.text = "";
        Destroy(gameObject);
    }

    /// <summary>
    /// �I�u�W�F�N�g���j�������ۂɌĂ΂��
    /// </summary>
    private void OnDestroy()
    {
        // �������̃C���X�^���X���V���O���g���C���X�^���X���g�ł���΁A
        // static�ȎQ�Ƃ��N���A����
        if (instance == this)
        {
            instance = null;
            Debug.Log("MessageController static�ȎQ�Ƃ��N���A");
        }
    }
}
