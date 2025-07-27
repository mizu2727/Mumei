using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class GameClearController : MonoBehaviour
{
    public static GameClearController instance;

    [SerializeField] public Canvas gameClearCanvas;//ゲームクリア画面のCanvas

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

    //タイトルへ戻る
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
}
