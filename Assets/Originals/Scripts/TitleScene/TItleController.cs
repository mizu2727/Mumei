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
    [Header("タイトル画面のCanvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("ロードしたいScene名")]
    [SerializeField] private string SceneName;

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    [Header("サウンド関連")]
    public AudioSource audioSourceBGM;
    private readonly int titleBGMid = 0; // タイトルBGMのID


    private void Awake()
    {
        Time.timeScale = 1;
        titlesCanvas.enabled = true;
        Cursor.visible = true;

        //マウスカーソルをウィンドウの外に出す
        Cursor.lockState = CursorLockMode.None;

        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();

        //audioSourceBGMを設定TODO
        //audioSourceBGM = MusicController.Instance.GetAudioSource();

        //タイトルBGMを再生TODO
        //MusicController.Instance.PlayBGM(titleBGMid);
    }

    public void OnStartButtonClicked()
    {
        //GameController.instance.playCount++;
        GameController.playCount++;
        SceneManager.LoadScene(SceneName);        
    }

    //ゲーム終了
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}