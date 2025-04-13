using UnityEngine;
using UnityEngine.UI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;


    [SerializeField] private GameObject MessagePanel;
    [SerializeField] private Text MessageText;


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

    //メッセージを表示
    public void ShowMessage(string message) 
    {
        MessageText.text = message;
        isMessagePanel = true;
        ViewMessagePanel();
    }
}
