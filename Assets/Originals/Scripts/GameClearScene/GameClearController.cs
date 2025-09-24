using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameClearController : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static GameClearController instance;

    [Header("�Q�[���N���A��ʂ�Canvas")]
    [SerializeField] public Canvas gameClearCanvas;

    [Header("�A���P�[�gURL")]
    [SerializeField] private string questionnaireURL = "https://docs.google.com/forms/d/13qMmaottZOaX7lg4lu8kzzvfE5aXFS3kNgPPoCmxI6M/viewform";

    [Header("X��URL")]
    [SerializeField] private string X_URL = "https://x.com/Tomanegi0707";

    private void Awake()
    {
        //�C���X�^���X����
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
    /// �^�C�g���֖߂�
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// �Q�[���N���A����UI��\��
    /// </summary>
    public void ViewGameClearUI() 
    {
        gameClearCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    /// <summary>
    /// �Q�[���N���A����UI���\��
    /// </summary>
    void HiddenGameClearUI()
    {
        gameClearCanvas.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// �A���P�[�gURL���J��
    /// </summary>
    public void OnClickedQuestionnaire_Button() 
    {
        Application.OpenURL(questionnaireURL);
    }

    /// <summary>
    /// X��URL���J��
    /// </summary>
    public void OnClickedX_Button()
    {
        Application.OpenURL(X_URL);
    }
}
