using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.AI;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "SampleScene01";
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartMapGenerationAndWarpAsync().Forget();
    }

    private async UniTask StartMapGenerationAndWarpAsync()
    {
        // 1. プレイヤーのシングルトン参照を確認
        if (Player.instance == null)
        {
            Debug.LogError("[ChangeScene] Player.instance が見つかりません。SampleScene02 のプレイヤーに Player スクリプトがアタッチされていることを確認してください。");
            return;
        }
        Debug.Log("[ChangeScene] Player.instance を取得しました。");

        // 1.5 SampleScene02 のオブジェクトを非アクティブにする
        Scene currentScene = SceneManager.GetSceneByName("SampleScene02");
        if (currentScene.IsValid())
        {
            foreach (var rootObj in currentScene.GetRootGameObjects())
            {
                if (rootObj != Player.instance.gameObject) // プレイヤー以外を非アクティブに
                {
                    rootObj.SetActive(false);
                }
            }
            Debug.Log("[ChangeScene] SampleScene02 のオブジェクトを非アクティブにしました。");
        }

        // 2. SampleScene01 を非同期でロード（Additiveモード）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }
        Debug.Log("[ChangeScene] SampleScene01 のロードが完了しました。");

        

        

        // 5. デバッグログを表示
        Debug.Log("[ChangeScene] スペースキーを押下してください。");

        // 6. スペースキー入力待ち
        bool spacePressed = false;
        while (!spacePressed)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                spacePressed = true;
                Debug.Log("[ChangeScene] スペースキーが押されました。");
            }
            await UniTask.Yield();
        }

        // 7. プレイヤーを SampleScene01 にワープ（シーン切り替え処理）
        Scene targetScene = SceneManager.GetSceneByName(targetSceneName);
        if (!targetScene.IsValid())
        {
            Debug.LogError("[ChangeScene] SampleScene01 シーンが無効です。");
            return;
        }

        // シーン移動前にプレイヤーのNavMeshAgentを無効化
        NavMeshAgent playerAgent = Player.instance.GetComponent<NavMeshAgent>();
        if (playerAgent != null && playerAgent.enabled)
        {
            playerAgent.enabled = false;
            Debug.Log("[ChangeScene] プレイヤーのNavMeshAgentを無効化しました。");
        }

        // プレイヤーを SampleScene01 に移動
        Debug.Log($"[ChangeScene] プレイヤー移動前: 位置={Player.instance.transform.position}, シーン={Player.instance.gameObject.scene.name}");
        SceneManager.MoveGameObjectToScene(Player.instance.gameObject, targetScene);
        Debug.Log($"[ChangeScene] プレイヤー移動後: 位置={Player.instance.transform.position}, シーン={Player.instance.gameObject.scene.name}");

        

        // 9. SampleScene02 をアンロード
        currentScene = SceneManager.GetSceneByName("SampleScene02");
        if (currentScene.IsValid())
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(currentScene);
            while (!asyncUnload.isDone)
            {
                await UniTask.Yield();
            }
            Debug.Log("[ChangeScene] SampleScene02 をアンロードしました。");
        }

        // 10. SampleScene01 をアクティブシーンに設定
        SceneManager.SetActiveScene(targetScene);
        Debug.Log("[ChangeScene] SampleScene01 をアクティブシーンに設定しました。");

        
    }
}