using UnityEngine;
using UnityEngine.UI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("���b�Z�[�W�p�l��(�q�G�����L�[�ォ��A�^�b�`����K�v������)")]
    [SerializeField] private GameObject MessagePanel;
    [SerializeField] private Text MessageText;


    [Header("�S�[�����b�Z�[�W�֘A(Prefab���A�^�b�`)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("���b�Z�[�W�p�l������")]
    public bool isMessagePanel = false;


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
    }

    public void Start()
    {
        ResetMessage();
    }

    public void ViewMessagePanel() 
    {
        if (isMessagePanel)
        {
            MessagePanel.SetActive(true);
        }
        else
        {
            MessagePanel.SetActive(false);
        }
    }

    //���b�Z�[�W�����Z�b�g
    public void ResetMessage() 
    {
        MessageText.text = "";
        isMessagePanel = false;
        ViewMessagePanel();
    }

    //�S�[�����b�Z�[�W��\��
    public void ShowGoalMessage(int number) 
    {
        MessageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }
}
