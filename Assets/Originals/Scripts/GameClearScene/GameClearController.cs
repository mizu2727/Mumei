using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameClearController : MonoBehaviour
{
    public static GameClearController instance;

    [Header("�Q�[���N���A��ʂ�Canvas")]
    [SerializeField] public Canvas gameClearCanvas;

    [Header("�A���P�[�gURL")]
    [SerializeField] private string questionnaireURL = "https://docs.google.com/forms/d/1xdBHlH7TKpKQc7KNpDPFqtSgjhFX4BW1OZQjSmdZVZA/viewform";

    [Header("X��URL")]
    [SerializeField] private string X_URL = "https://x.com/Tomanegi0707";

    private void Awake()
    {
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
        gameClearCanvas.enabled = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //await MessageController.instance.ShowSystemMessage(70);
    }

    /// <summary>
    /// �^�C�g���֖߂�
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }


    public void ViewGameClearUI() 
    {
        gameClearCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
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
