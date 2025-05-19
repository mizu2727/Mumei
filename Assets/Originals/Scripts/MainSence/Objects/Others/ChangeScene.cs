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

        // 3. TestMap01 のシングルトン参照を取得
        TestMap01 testMap = null;
        for (int i = 0; i < 50; i++)
        {
            testMap = TestMap01.instance;
            if (testMap != null) break;
            await UniTask.Delay(100);
        }
        if (testMap == null)
        {
            Debug.LogError("[ChangeScene] TestMap01.instance が見つかりません。SampleScene01 に TestMap01 スクリプトがアタッチされていることを確認してください。");
            return;
        }
        Debug.Log("[ChangeScene] TestMap01.instance を取得しました。");

        // 4. マップ生成とNavMesh生成を待機（タイムアウト設定）
        int maxWaitAttempts = 100; // 最大10秒待機
        for (int i = 0; i < maxWaitAttempts; i++)
        {
            if (testMap.IsMapGenerated && testMap.IsNavMeshGenerated)
            {
                Debug.Log("[ChangeScene] マップおよびNavMesh生成が完了しました。");
                break;
            }
            Debug.Log($"[ChangeScene] マップ生成待機中: IsMapGenerated={testMap.IsMapGenerated}, IsNavMeshGenerated={testMap.IsNavMeshGenerated}");
            await UniTask.Delay(100);
        }
        if (!testMap.IsMapGenerated || !testMap.IsNavMeshGenerated)
        {
            Debug.LogError("[ChangeScene] マップまたはNavMesh生成がタイムアウトしました。");
            return;
        }

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

        // 8. プレイヤーをワープ
        await testMap.SpawnPlayerAsync();
        if (!testMap.hasPlayerSpawned)
        {
            Debug.LogError("[ChangeScene] プレイヤーのワープに失敗しました。");
            return;
        }
        Debug.Log("[ChangeScene] プレイヤーのワープが完了しました。");

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

        // 11. 敵を生成
        await testMap.SpawnEnemiesAsync();
        if (!testMap.hasEnemiesSpawned)
        {
            Debug.LogError("[ChangeScene] 敵の生成に失敗しました。");
            return;
        }
        Debug.Log("[ChangeScene] 敵の生成が完了しました。");
    }
}