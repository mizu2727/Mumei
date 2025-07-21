using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("���b�Z�[�W�p�l���֘A(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;

    [Header("���b�Z�[�W�������X�s�[�h�B���l���������قǑf��������")]
    [SerializeField] private float writeSpeed = 0;
    private bool isWrite = false;//�����Ă�r���ł��邩�𔻒�

    [Header("�V�X�e�����b�Z�[�W�\���@�\(Prefab���A�^�b�`)")]
    [SerializeField] private ShowSystemMessage showSystemMessage;

    [Header("��b���b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private TalkMessage talkMessage;

    [Header("�V�X�e�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private SystemMessage systemMessage;

    [Header("�S�[�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("�v���C���[�̖��O����(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] public InputField inputPlayerNameField;

    [Header("���b�Z�[�W�p�l������")]
    public bool isMessagePanel = false;

    [Header("�f�o�b�O�p�֘A")]
    [SerializeField] private TestShowMessage testShowMessage;


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

        inputPlayerNameField = inputPlayerNameField.GetComponent<InputField>();
        inputPlayerNameField.gameObject.SetActive(false);
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
        else
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //�G�N�Z���f�[�^�^.���X�g�^[�ԍ�].�J������
            Write(talkMessage.talkMessage[number].message);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            switch (number)
            {
                //�e��b�ɂ����āA��ԍŌ�ɕ\������ׂ��e�L�X�g��\���B���̃��b�Z�[�W�ԍ��̗p�ӂ͍s��Ȃ�
                case 2:
                    //�X�y�[�X�L�[�����ŉ�b�I��
                    ResetMessage();

                    break;

                //���b�Z�[�W�ԍ��ɑΉ����Ă��郁�b�Z�[�W���L�ځ����̃��b�Z�[�W�ԍ���p��
                default:
                    messageText.text = "";
                    number++;

                    //�X�y�[�X�L�[�����Ŏ��̃��b�Z�[�W������
                    testShowMessage.TestShowTalkMessage(number);
                    break;
            }
        }
    }


    //�V�X�e�����b�Z�[�W��\��
    public async UniTask ShowSystemMessage(int number)
    {
        Debug.Log("�V�X�e�����b�Z�[�W��\��");

        //�O�̃��b�Z�[�W�������Ă�r���ł��邩�𔻒f�B�����r���Ȃ�true
        if (isWrite) writeSpeed = 0;
        else
        {
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

                    inputPlayerNameField.gameObject.SetActive(true);

                    break;

                case 6:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    // �F��ԐF�ɐݒ�
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    break ;


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
        if (playerName.Length < 11) 
        {
            //���O��ۑ�
            inputPlayerNameField.text = playerName;

            inputPlayerNameField.gameObject.SetActive(false);

            showSystemMessage.ShowGameSystemMessage(5);
        }
        else
        {
            showSystemMessage.ShowGameSystemMessage(4);
        }
    }  
}
