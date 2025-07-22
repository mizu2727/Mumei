using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using static GameController;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    [Header("�^�C�g����ʂ�Canvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("���[�h������Scene��")]
    [SerializeField] private string SceneName;


    private void Awake()
    {
        Time.timeScale = 1;
        titlesCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);
    }

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(SceneName);        
    }

    //�Q�[���I��
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}