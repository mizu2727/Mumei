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
        SceneManager.sceneLoaded += OnSceneLoaded; // �V�[�����[�h�C�x���g��o�^
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
            Debug.Log($"[TitleController] �V�[�� '{scene.name}' �����[�h����܂����BTestMap01.Instance ���`�F�b�N���܂��B");
            if (TestMap01.Instance != null)
            {
                Debug.Log($"[TitleController] TestMap01.Instance ��������܂���: {TestMap01.Instance.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[TitleController] TestMap01.Instance �� null �ł��B�V�[�� '{scene.name}' �� TestMap01 �����݂��邩�m�F���Ă��������B");
            }
        }
    }

    public async void OnStartButtonClicked()
    {
        try
        {
            // �V�[����񓯊��Ń��[�h
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
            while (!asyncLoad.isDone)
            {
                await UniTask.Yield(cancellationToken: cancellationTokenSource.Token);
            }

            // �V�[�������S�Ƀ��[�h�����܂őҋ@
            await UniTask.DelayFrame(10, cancellationToken: cancellationTokenSource.Token);

            // TestMap01.Instance ���擾�i�ő�200�t���[���ҋ@�j
            TestMap01 mapGenerator = null;
            for (int i = 0; i < 200; i++)
            {
                mapGenerator = TestMap01.Instance;
                if (mapGenerator != null)
                {
                    Debug.Log($"[TitleController] TestMap01.Instance �� {i} �t���[���ڂŎ擾: {mapGenerator.gameObject.name}");
                    break;
                }
                await UniTask.DelayFrame(1, cancellationToken: cancellationTokenSource.Token);
            }

            if (mapGenerator == null)
            {
                Debug.LogError($"[TitleController] TestMap01.Instance ��������܂���I�V�[�� '{SceneName}' �� TestMap01 ���A�^�b�`���ꂽ GameObject �����݂��邩�m�F���Ă��������B");
                return;
            }

            // �}�b�v��������������܂őҋ@
            await UniTask.WaitUntil(() => mapGenerator.IsMapGenerated, cancellationToken: cancellationTokenSource.Token);

            // �v���C���[�̃��[�v�����s
            await mapGenerator.TriggerWarpAsync();
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning($"[TitleController] OnStartButtonClicked ���L�����Z������܂����B");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[TitleController] OnStartButtonClicked �ŗ�O������: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // �C�x���g������
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