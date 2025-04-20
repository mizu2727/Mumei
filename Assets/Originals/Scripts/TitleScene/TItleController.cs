using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    [SerializeField] private Canvas titlesCanvas;
    [SerializeField] private string SceneName;
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        Time.timeScale = 1;
        cancellationTokenSource = new CancellationTokenSource();
        SceneManager.sceneLoaded += OnSceneLoaded; // シーンロードイベントを登録
    }

    private void Start()
    {
        titlesCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneName)
        {
            Debug.Log($"[TitleController] シーン '{scene.name}' がロードされました。TestMap01.Instance をチェックします。");
            if (TestMap01.Instance != null)
            {
                Debug.Log($"[TitleController] TestMap01.Instance が見つかりました: {TestMap01.Instance.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[TitleController] TestMap01.Instance が null です。シーン '{scene.name}' に TestMap01 が存在するか確認してください。");
            }
        }
    }

    public async void OnStartButtonClicked()
    {
        try
        {
            // シーンを非同期でロード
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
            while (!asyncLoad.isDone)
            {
                await UniTask.Yield(cancellationToken: cancellationTokenSource.Token);
            }

            // シーンが完全にロードされるまで待機
            await UniTask.DelayFrame(10, cancellationToken: cancellationTokenSource.Token);

            // TestMap01.Instance を取得（最大200フレーム待機）
            TestMap01 mapGenerator = null;
            for (int i = 0; i < 200; i++)
            {
                mapGenerator = TestMap01.Instance;
                if (mapGenerator != null)
                {
                    Debug.Log($"[TitleController] TestMap01.Instance を {i} フレーム目で取得: {mapGenerator.gameObject.name}");
                    break;
                }
                await UniTask.DelayFrame(1, cancellationToken: cancellationTokenSource.Token);
            }

            if (mapGenerator == null)
            {
                Debug.LogError($"[TitleController] TestMap01.Instance が見つかりません！シーン '{SceneName}' に TestMap01 がアタッチされた GameObject が存在するか確認してください。");
                return;
            }

            // マップ生成が完了するまで待機
            await UniTask.WaitUntil(() => mapGenerator.IsMapGenerated, cancellationToken: cancellationTokenSource.Token);

            // プレイヤーのワープを実行
            await mapGenerator.TriggerWarpAsync();
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning($"[TitleController] OnStartButtonClicked がキャンセルされました。");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TitleController] OnStartButtonClicked で例外が発生: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // イベントを解除
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}