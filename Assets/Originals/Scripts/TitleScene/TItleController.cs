using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    [Header("�^�C�g����ʂ�Canvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("���[�h������Scene��")]
    [Header("�f���pScene���FAbandonedAsylum01")]
    [SerializeField] private string SceneName;

    [Header("Scene���[�h����")]
    [SerializeField] private int sceneRoadTime = 1;

    [Header("�}�b�v���[�h����")]
    [SerializeField] private float mapRoadTime = 1f;

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
            if (TestMap01.instance != null)
            {
                Debug.Log($"[TitleController] TestMap01.Instance ��������܂���: {TestMap01.instance.gameObject.name}");
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
            await UniTask.DelayFrame(sceneRoadTime, cancellationToken: cancellationTokenSource.Token); // �ҋ@���Ԃ�����

            // TestMap01.Instance ���擾�i�ő�500�t���[���ҋ@�j
            TestMap01 mapGenerator = null;
            for (int i = 0; i < mapRoadTime; i++)
            {
                mapGenerator = TestMap01.instance;
                if (mapGenerator != null && mapGenerator.gameObject.scene.IsValid())
                {
                    Debug.Log($"[TitleController] TestMap01.Instance �� {i} �t���[���ڂŎ擾: {mapGenerator.gameObject.name}");
                    break;
                }
                await UniTask.DelayFrame(1, cancellationToken: cancellationTokenSource.Token);
            }

            if (mapGenerator == null || !mapGenerator.gameObject.scene.IsValid())
            {
                Debug.LogError($"[TitleController] TestMap01.Instance ��������܂���A�܂��̓V�[���������ł��I�V�[�� '{SceneName}' �� TestMap01 ���A�^�b�`���ꂽ GameObject �����݂��邩�m�F���Ă��������B");
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