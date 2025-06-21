using UnityEngine;
using UnityEngine.UI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("メッセージパネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject MessagePanel;
    [SerializeField] private Text MessageText;


    [Header("ゴールメッセージ関連(Prefabをアタッチ)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("メッセージパネル判定")]
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

    //メッセージをリセット
    public void ResetMessage() 
    {
        MessageText.text = "";
        isMessagePanel = false;
        ViewMessagePanel();
    }

    //ゴールメッセージを表示
    public void ShowGoalMessage(int number) 
    {
        MessageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }
}
