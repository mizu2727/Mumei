using Cysharp.Threading.Tasks;
using System;
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
    [SerializeField] private Text messageText;

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

    [Header("�S�[��(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] public Goal goal;

    [Header("���b�Z�[�W�p�l������")]
    public bool isMessagePanel = false;

    [Header("�u���b�N�A�E�g����")]
    public bool isBlackOutPanel = false;


    [Header("�T�E���h�֘A")]
    [SerializeField] private AudioClip noiseSE;
    private AudioSource audioSourceSE;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        ResetMessage();

        audioSourceSE = MusicController.Instance.GetAudioSource();

        inputPlayerNameField = inputPlayerNameField.GetComponent<InputField>();
        inputPlayerNameField.gameObject.SetActive(false);

        isBlackOutPanel = false;
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
        writeSpeed = 0;
        isWrite = true;

        for (int i = 0; i < s.Length; i++)
        {
            messageText.text += s.Substring(i, 1);
            await UniTask.Delay(TimeSpan.FromSeconds(writeSpeed));
        }
        isWrite = false;
    }

    //��b���b�Z�[�W��\��
    public async UniTask ShowTalkMessage(int number)
    {
        Debug.Log("��b�X�^�[�g");

        //�O�̃��b�Z�[�W�������Ă�r���ł��邩�𔻒f�B�����r���Ȃ�true
        if (isWrite) writeSpeed = 0;
        else if (Time.timeScale == 1)
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //�b���Ă���l���J�i���̏ꍇ�A���͂̐F���V�A���F�ɐݒ�
            if (talkMessage.talkMessage[number].speakerName == "�J�i��") messageText.color = Color.cyan;
            else messageText.color = Color.white;
            


            //�G�N�Z���f�[�^�^.���X�g�^[�ԍ�].�J������
            Write(talkMessage.talkMessage[number].message);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            switch (number)
            {
                case 2:
                    messageText.text = "";
                    number++;

                    Player.instance.playerIsBackRotate = true;

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                    showTalkMessage.ShowGameTalkMessage(number);



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
                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));


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

                    goal.OnTutorial();

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

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                    showTalkMessage.ShowGameTalkMessage(number);

                    break;

                case 86:
                    messageText.text = "";
                    number++;

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // �F��ԐF�ɐݒ�
                    messageText.color = Color.red;

                    Write(talkMessage.talkMessage[number].message);
                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

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
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //�O�̃��b�Z�[�W�������Ă�r���ł��邩�𔻒f�B�����r���Ȃ�true
        if (isWrite) writeSpeed = 0;
        else if (Time.timeScale == 1)
        {
            if (number == 15 && goal.isTutorial) 
            {
                goal.OffTutorial();
            } 

            isMessagePanel = true;
            ViewMessagePanel();

            //�G�N�Z���f�[�^�^.���X�g�^[�ԍ�].�J������
            Write(systemMessage.systemMessage[number].message);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            switch (number)
            {
                //���O����UI��\��
                case 3:
                    ResetMessage();

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;

                    inputPlayerNameField.gameObject.SetActive(true);

                    break;

                //���O���͐����Ɉ�������������ɁA������x���O����UI��\��
                case 4:
                    ResetMessage();
                    showSystemMessage.ShowGameSystemMessage(3);

                    break;

                case 6:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // �F��ԐF�ɐݒ�
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //�z�[���ֈړ�
                    SceneManager.LoadScene("HomeScene");
                    break;

                //��b���b�Z�[�W��\��
                case 9:
                    ResetMessage();

                    showTalkMessage.ShowGameTalkMessage(38);

                    break;

                case 10:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(43);

                    break;

                case 11:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(44);

                    break;

                case 12:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(46);

                    break;

                case 13:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(47);

                    break;

                //�`���[�g���A���I��
                case 15:
                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    Kaname.instance.WarpPostion(1, 0.505f, 2);

                    Player.instance.PlayerWarp(1, 0.562f, 0);

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    GameController.instance.tutorialItems.SetActive(false);

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));         

                    showTalkMessage.ShowGameTalkMessage(49);

                    break;


                //���b�Z�[�W�ԍ��ɑΉ����Ă��郁�b�Z�[�W���L�ځ����̃��b�Z�[�W�ԍ���p��
                default:
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

    //�v���C���[�̖��O�̓��͂����������ۂɌĂ΂��
    public void SavePlayerName(string playerName)
    {
        if ((1 < playerName.Length) && (playerName.Length < 11)) 
        {
            //���O��ۑ�
            inputPlayerNameField.text = playerName;

            inputPlayerNameField.gameObject.SetActive(false);

            showSystemMessage.ShowGameSystemMessage(5);
        }
        else
        {
            inputPlayerNameField.gameObject.SetActive(false);
            showSystemMessage.ShowGameSystemMessage(4);
        }
    }  
}
