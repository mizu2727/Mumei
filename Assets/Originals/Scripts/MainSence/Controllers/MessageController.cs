using Cysharp.Threading.Tasks;
using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("���b�Z�[�W�p�l���֘A(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;

    [Header("���b�Z�[�W�������X�s�[�h�B���l���������قǑf��������")]
    [SerializeField] private float writeSpeed = 0;
    private bool isWrite = false;//�����Ă�r���ł��邩�𔻒�

    [Header("��b���b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private TalkMessage talkMessage;

    [Header("�S�[�����b�Z�[�W(Prefab���A�^�b�`)")]
    [SerializeField] private GoalMessage goalMessage;

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

    //�S�[�����b�Z�[�W��\��
    public void ShowGoalMessage(int number) 
    {
        Debug.Log("�S�[�����b�Z�[�W�X�^�[�g");
        messageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }
}
