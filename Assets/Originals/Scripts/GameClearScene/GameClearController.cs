using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameClearController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameClearController instance;

    [Header("ゲームクリア画面のCanvas")]
    [SerializeField] public Canvas gameClearCanvas;

    [Header("アンケートURL")]
    [SerializeField] private string questionnaireURL = "https://docs.google.com/forms/d/13qMmaottZOaX7lg4lu8kzzvfE5aXFS3kNgPPoCmxI6M/viewform";

    [Header("XのURL")]
    [SerializeField] private string X_URL = "https://x.com/Tomanegi0707";

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }


    void Start()
    {
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
        HiddenGameClearUI();
    }

    /// <summary>
    /// タイトルへ戻る
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ゲームクリア時のUIを表示
    /// </summary>
    public void ViewGameClearUI() 
    {
        gameClearCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// ゲームクリア時のUIを非表示
    /// </summary>
    void HiddenGameClearUI()
    {
        gameClearCanvas.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// アンケートURLを開く
    /// </summary>
    public void OnClickedQuestionnaire_Button() 
    {
        Application.OpenURL(questionnaireURL);
    }

    /// <summary>
    /// XのURLを開く
    /// </summary>
    public void OnClickedX_Button()
    {
        Application.OpenURL(X_URL);
    }
}
