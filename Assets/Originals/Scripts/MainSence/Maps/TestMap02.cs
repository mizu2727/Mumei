using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cysharp.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMap02 : MonoBehaviour
{
    public static TestMap02 instance { get; private set; }

    [Header("ステージプレハブの設定")]
    [SerializeField] private List<GameObject> stagePrefabs; // ステージプレハブのリスト
    [SerializeField] private float stageSpacing = 10f; // プレハブ間の距離
    [SerializeField] private Vector2Int gridSize = new Vector2Int(5, 5); // グリッドサイズ（5x5）

    [Header("マップ生成の初期位置")]
    [SerializeField] private Vector3 defaultPosition;

    [Header("アイテムの設定")]
    [SerializeField] private List<GameObject> itemPrefabs;
    [SerializeField] private List<int> itemGenerateNums;

    [Header("徘徊地点の設定")]
    [SerializeField] private GameObject[] patrolPointPrefabs;
    [HideInInspector] public Transform[] patrolPoint;
    private GameObject patrolPointParent;
    private List<Transform> patrolPointsList = new List<Transform>();

    [Header("敵の設定")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<int> enemyGenerateNums;
    [SerializeField] private float minDistanceFromEnemy = 5f;
    private List<Vector3> enemyPositions = new List<Vector3>();

    [Header("ゴール")]
    [SerializeField] private GameObject goalGroundPrefab;
    [SerializeField] private GameObject goalGround;
    [SerializeField] private GameObject goalObjectPrefab;

    [Header("デフォルトで「Ground」レイヤーを設定")]
    [SerializeField] private LayerMask groundLayer = 1 << 8;

    [Header("デバッグ設定")]
    [SerializeField] private bool isDebugOffGenerate = false;
    [SerializeField] private bool debugGenerateEnemies = true; // 敵の生成フラグ
    [SerializeField] private bool debugGenerateItems = true; // アイテムの生成フラグ
    [SerializeField] private bool debugGeneratePatrolPoints = true; // 徘徊地点の生成フラグ
    [SerializeField] private bool debugGenerateGoal = true; // ゴールの生成フラグ

    [Header("フラグ")]
    public bool IsMapGenerated { get; private set; }
    private bool isGeneratingMap = false;
    public bool IsNavMeshGenerated { get; private set; }
    public bool hasPlayerSpawned = false;
    public bool hasEnemiesSpawned = false;

    private NavMeshSurface[] stageSurfaces;
    private List<(Vector3 center, Vector3 size)> rooms; // プレハブから取得した部屋情報

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[TestMap02] 別のインスタンスがすでに存在します: {instance.gameObject.name}。このインスタンスを破棄します: {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Ground");

        if (!isDebugOffGenerate)
        {
            MapGenerate().Forget();
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private async UniTask MapGenerate()
    {
        if (isGeneratingMap || IsMapGenerated) return;
        isGeneratingMap = true;
        IsMapGenerated = false;

        if (stagePrefabs == null || stagePrefabs.Count == 0 || stagePrefabs.Any(p => p == null))
        {
            Debug.LogError("[TestMap02] ステージプレハブが設定されていないか、nullが含まれています。");
            isGeneratingMap = false;
            return;
        }

        GameObject stagesParent = new GameObject("Stages");
        stagesParent.transform.position = defaultPosition;
        stagesParent.transform.SetParent(transform);

        List<NavMeshSurface> surfaces = new List<NavMeshSurface>();
        rooms = new List<(Vector3 center, Vector3 size)>();

        // 5x5グリッドにプレハブを配置
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                GameObject prefab = stagePrefabs[Random.Range(0, stagePrefabs.Count)];
                Vector3 position = defaultPosition + new Vector3(x * stageSpacing, 0, z * stageSpacing);
                GameObject stage = Instantiate(prefab, position, Quaternion.identity, stagesParent.transform);
                stage.name = $"Stage_{x}_{z}";

                NavMeshSurface surface = stage.GetComponent<NavMeshSurface>();
                if (surface == null)
                {
                    surface = stage.AddComponent<NavMeshSurface>();
                    surface.collectObjects = CollectObjects.Children;
                    surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
#if UNITY_EDITOR
                    GameObjectUtility.SetStaticEditorFlags(stage, StaticEditorFlags.NavigationStatic);
#endif
                }
                surfaces.Add(surface);

                // プレハブ内の部屋情報を取得（"Room"タグ付きオブジェクト）
                foreach (Transform child in stage.GetComponentsInChildren<Transform>())
                {
                    if (child.CompareTag("Room"))
                    {
                        Vector3 worldPos = child.position;
                        Vector3 size = child.localScale; // スケールでサイズを表現
                        rooms.Add((worldPos, size));
                    }
                }
            }
        }

        // 部屋情報が取得できなかった場合のフォールバック
        if (rooms.Count == 0)
        {
            Debug.LogWarning("[TestMap02] 部屋情報が見つかりませんでした。デフォルトの部屋を追加します。");
            rooms.Add((defaultPosition + new Vector3(gridSize.x * stageSpacing / 2, 0, gridSize.y * stageSpacing / 2), new Vector3(stageSpacing, 1, stageSpacing)));
        }

        stageSurfaces = surfaces.ToArray();
        await RebuildNavMeshAsync();

        // 徘徊地点の親オブジェクトを初期化
        if (debugGeneratePatrolPoints)
        {
            patrolPointParent = new GameObject("PatrolPoints");
            patrolPointParent.transform.SetParent(transform);
            patrolPointParent.transform.position = defaultPosition;
        }

        // ゴール配置
        if (!debugGenerateGoal)
        {
            AddGoalConnection();
        }
        else
        {
            Debug.Log("[TestMap02] デバッグ設定によりゴールの生成をスキップしました。");
        }

        // 徘徊地点の生成
        if (!debugGeneratePatrolPoints)
        {
            patrolPointsList.Clear();
            for (int i = 0; i < patrolPointPrefabs.Length; i++)
            {
                if (patrolPointPrefabs[i] != null)
                {
                    await GeneratePatrolPointInRoomsAsync(patrolPointPrefabs[i].transform, patrolPointPrefabs.Length);
                }
            }
            patrolPoint = patrolPointsList.ToArray();
        }
        else
        {
            Debug.Log("[TestMap02] デバッグ設定により徘徊地点の生成をスキップしました。");
        }

        // アイテムの生成
        if (!debugGenerateItems)
        {
            for (int i = 0; i < itemPrefabs.Count; i++)
            {
                await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
            }
        }
        else
        {
            Debug.Log("[TestMap02] デバッグ設定によりアイテムの生成をスキップしました。");
        }

        // 敵の生成
        if (!debugGenerateEnemies)
        {
            await SpawnEnemiesAsync();
        }
        else
        {
            Debug.Log("[TestMap02] デバッグ設定により敵の生成をスキップしました。");
        }

        IsMapGenerated = true;
        isGeneratingMap = false;
        Debug.Log("[TestMap02] マップ生成が完了しました。スペースキーを押してプレイヤーを生成してください。");
    }

    private async UniTask RebuildNavMeshAsync()
    {
        await UniTask.DelayFrame(10);
        foreach (var surface in stageSurfaces)
        {
            if (surface != null)
            {
                surface.BuildNavMesh();
                await UniTask.WaitUntil(() => surface.navMeshData != null, cancellationToken: this.GetCancellationTokenOnDestroy());
                Debug.Log(surface.navMeshData != null
                    ? $"[TestMap02] NavMesh 構築完了: 範囲={surface.navMeshData.sourceBounds}, 位置={surface.transform.position}"
                    : "[TestMap02] NavMeshデータ生成に失敗しました！");
            }
        }
        IsNavMeshGenerated = true;
        Debug.Log("[TestMap02] NavMesh 生成が完了しました。");
    }

    public async UniTask SpawnPlayerAsync()
    {
        if (hasPlayerSpawned)
        {
            Debug.LogWarning("[TestMap02] プレイヤーはすでにスポーンしています。");
            return;
        }
        await WarpPlayerAsync();
        if (hasPlayerSpawned)
        {
            Debug.Log("[TestMap02] プレイヤーのスポーンが完了しました。");
        }
        else
        {
            Debug.LogError("[TestMap02] プレイヤーのスポーンに失敗しました。");
        }
    }

    public async UniTask SpawnEnemiesAsync()
    {
        if (hasEnemiesSpawned)
        {
            Debug.LogWarning("[TestMap02] 敵はすでにスポーンしています。");
            return;
        }
        enemyPositions.Clear();
        int totalEnemiesGenerated = 0;
        int maxIndex = Mathf.Min(enemyPrefabs.Count, enemyGenerateNums.Count);
        for (int i = 0; i < maxIndex; i++)
        {
            if (enemyPrefabs[i] == null || enemyGenerateNums[i] <= 0) continue;
            await GenerateEnemiesInRoomsAsync(enemyPrefabs[i], enemyGenerateNums[i]);
            totalEnemiesGenerated += enemyGenerateNums[i];
        }
        hasEnemiesSpawned = true;
        Debug.Log($"[TestMap02] 敵の生成が完了しました。総生成数: {totalEnemiesGenerated}");
    }

    private async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap02] Player.instance が null です！ワープできません。");
            hasPlayerSpawned = false;
            return;
        }
        if (!CheckNavMeshExists())
        {
            Debug.LogWarning("[TestMap02] NavMesh がまだ生成されていません。ワープ処理を延期します。");
            await DelayedWarpAsync();
            return;
        }
        NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("[TestMap02] Player に NavMeshAgent コンポーネントがアタッチされていません。");
            hasPlayerSpawned = false;
            return;
        }

        Vector3 initialPosition = Player.instance.transform.position;
        const int maxRoomAttempts = 50;
        bool playerWarped = false;

        for (int attempt = 0; attempt < maxRoomAttempts; attempt++)
        {
            int roomIndex = Random.Range(0, rooms.Count);
            var (center, size) = rooms[roomIndex];
            Vector3 warpPosition = await FindWarpPositionAsync(center, size);
            if (warpPosition == Vector3.zero) continue;
            if (Vector3.Distance(warpPosition, initialPosition) < 0.1f) continue;
            if (debugGenerateEnemies && enemyPositions.Any(enemyPos => Vector3.Distance(warpPosition, enemyPos) < minDistanceFromEnemy)) continue;

            if (agent.enabled)
            {
                agent.enabled = false;
                await UniTask.NextFrame();
            }
            Player.instance.transform.position = warpPosition;
            await UniTask.NextFrame();
            agent.enabled = true;
            await UniTask.NextFrame();

            bool warpSuccess = agent.Warp(warpPosition);
            if (!warpSuccess || !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(warpPosition, out var navHit, 5.0f, NavMesh.AllAreas))
                {
                    warpPosition = navHit.position;
                    agent.Warp(warpPosition);
                }
                else
                {
                    Player.instance.transform.position = warpPosition;
                    agent.enabled = false;
                    playerWarped = true;
                    hasPlayerSpawned = true;
                    break;
                }
            }

            Vector3 finalPosition = Player.instance.transform.position;
            if (NavMesh.SamplePosition(finalPosition, out var finalNavHit, 2.0f, NavMesh.AllAreas))
            {
                Player.instance.transform.position = finalNavHit.position;
                agent.Warp(finalNavHit.position);
            }
            playerWarped = true;
            hasPlayerSpawned = true;
            Debug.Log($"[TestMap02] プレイヤーをワープしました: {finalPosition}");
            break;
        }

        if (!playerWarped)
        {
            Vector3 fallbackPosition = defaultPosition + new Vector3(5f, 0.6f, 5f);
            if (NavMesh.SamplePosition(fallbackPosition, out var navHit, 10f, NavMesh.AllAreas))
            {
                fallbackPosition = navHit.position + Vector3.up * 0.1f;
            }
            if (agent.enabled)
            {
                agent.enabled = false;
                await UniTask.NextFrame();
            }
            Player.instance.transform.position = fallbackPosition;
            await UniTask.NextFrame();
            agent.enabled = true;
            await UniTask.NextFrame();
            bool warpSuccess = agent.Warp(fallbackPosition);
            hasPlayerSpawned = warpSuccess && agent.isOnNavMesh;
            Debug.Log(hasPlayerSpawned
                ? $"[TestMap02] フォールバックワープ成功: {fallbackPosition}"
                : $"[TestMap02] フォールバックワープに失敗: {fallbackPosition}, 直接配置");
        }
    }

    private async UniTask<Vector3> FindWarpPositionAsync(Vector3 roomCenter, Vector3 roomSize)
    {
        float margin = 0.5f;
        int maxAttempts = 100;
        float yOffset = Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.5f + 0.1f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            Vector3 spawnPosition = new Vector3(x, roomCenter.y + 5.0f, z);

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Vector3 finalPos = hit.point + Vector3.up * yOffset;
                    if (NavMesh.SamplePosition(finalPos, out var navHit, 5.0f, NavMesh.AllAreas))
                    {
                        Vector3 adjustedPos = navHit.position + Vector3.up * 0.1f;
                        if (!Physics.CheckSphere(adjustedPos, 0.5f, LayerMask.GetMask("Wall")))
                        {
                            return adjustedPos;
                        }
                    }
                }
            }
            await UniTask.Yield();
        }

        Vector3 fallbackPos = roomCenter + Vector3.up * yOffset;
        Debug.LogWarning($"[TestMap02] ワープ位置が見つかりませんでした。フォールバック位置を使用: {fallbackPos}");
        return fallbackPos;
    }

    private async UniTask GenerateEnemiesInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap02] 敵プレハブがnullです！");
            return;
        }

        int enemiesGenerated = 0;
        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, rooms.Count);
            var (center, size) = rooms[roomIndex];
            Vector3 position = await PlaceObjectAsync(prefab, center, size, false);
            if (position == Vector3.zero) continue;

            enemyPositions.Add(position);
            var enemy = Instantiate(prefab, position, Quaternion.identity);
            enemy.name = $"{prefab.name}_{enemiesGenerated}";
            var enemyScript = enemy.GetComponent<BaseEnemy>();
            if (enemyScript != null && Player.instance != null)
                enemyScript.targetPoint = Player.instance.transform;
            enemiesGenerated++;
        }
    }

    private void AddGoalConnection()
    {
        if (goalGroundPrefab == null)
        {
            Debug.LogWarning("[TestMap02] ゴールプレハブが設定されていません。");
            return;
        }

        Vector3 goalPos = defaultPosition + new Vector3((gridSize.x - 1) * stageSpacing, 0, (gridSize.y - 1) * stageSpacing);
        goalGround = Instantiate(goalGroundPrefab, goalPos, Quaternion.identity, transform);
        goalGround.name = "GoalGround";

        if (goalObjectPrefab != null)
        {
            Vector3 goalObjPos = goalGround.transform.position + Vector3.up * 0.1f;
            if (NavMesh.SamplePosition(goalObjPos, out var navHit, 20.0f, NavMesh.AllAreas))
            {
                GameObject goalObj = Instantiate(goalObjectPrefab, navHit.position, Quaternion.identity, goalGround.transform);
                goalObj.name = "GoalObject";
            }
            else
            {
                GameObject goalObj = Instantiate(goalObjectPrefab, goalObjPos, Quaternion.identity, goalGround.transform);
                goalObj.name = "GoalObject_Fallback";
            }
        }
    }

    private async UniTask GeneratePatrolPointInRoomsAsync(Transform patrolPoint, int generateNum)
    {
        if (patrolPoint == null || patrolPointParent == null)
        {
            Debug.LogError("[TestMap02] 徘徊地点プレハブまたは親オブジェクトがnullです！");
            return;
        }

        int pointsGenerated = 0;
        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, rooms.Count);
            var (center, size) = rooms[roomIndex];
            Vector3 position = await PlaceObjectAsync(patrolPoint.gameObject, center, size, true);
            if (position == Vector3.zero) continue;

            GameObject obj = Instantiate(patrolPoint.gameObject, position, Quaternion.identity);
            obj.name = $"{patrolPoint.name}_{pointsGenerated}";
            obj.transform.SetParent(patrolPointParent.transform);
            patrolPointsList.Add(obj.transform);
            pointsGenerated++;
        }
        Debug.Log($"[TestMap02] 徘徊地点の生成が完了しました。生成数: {pointsGenerated}");
    }

    private async UniTask GenerateObjectsInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap02] アイテムプレハブがnullです！");
            return;
        }

        int itemsGenerated = 0;
        var roomAreas = rooms.Select((room, index) => (index, area: room.size.x * room.size.z))
                            .OrderByDescending(r => r.area).ToList();

        for (int i = 0; i < generateNum; i++)
        {
            bool placed = false;
            for (int j = 0; j < Mathf.Min(3, roomAreas.Count); j++)
            {
                int roomIndex = roomAreas[j].index;
                var (center, size) = rooms[roomIndex];
                Vector3 position = await PlaceObjectAsync(prefab, center, size, false);
                if (position != Vector3.zero)
                {
                    GameObject obj = Instantiate(prefab, position, Quaternion.identity);
                    obj.name = $"{prefab.name}_{itemsGenerated}";
                    itemsGenerated++;
                    placed = true;
                    break;
                }
            }
            if (!placed && roomAreas.Count > 0)
            {
                int roomIndex = roomAreas[0].index;
                var (center, size) = rooms[roomIndex];
                Vector3 position = center + Vector3.up * (prefab.transform.localScale.y * 0.5f + 0.05f);
                GameObject obj = Instantiate(prefab, position, Quaternion.identity);
                obj.name = $"{prefab.name}_{itemsGenerated}_Fallback";
                itemsGenerated++;
            }
        }
    }

    private async UniTask<Vector3> PlaceObjectAsync(GameObject prefab, Vector3 roomCenter, Vector3 roomSize, bool isPatrolPoint)
    {
        string objectName = prefab.name;
        float margin = 0.5f;
        int maxAttempts = 5;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            Vector3 spawnPosition = new Vector3(x, roomCenter.y + 5.0f, z);

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Vector3 finalPos = hit.point + Vector3.up * (isPatrolPoint ? 0.05f : prefab.transform.localScale.y * 0.5f + 0.05f);
                    if (NavMesh.SamplePosition(finalPos, out var navHit, 5.0f, NavMesh.AllAreas))
                    {
                        return navHit.position;
                    }
                    return finalPos;
                }
            }
            await UniTask.Yield();
        }

        Debug.LogError($"[TestMap02] 最大試行回数({maxAttempts})を超えても {objectName} の配置位置を見つけられませんでした。roomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }

    private bool CheckNavMeshExists()
    {
        Vector3 checkPosition = defaultPosition + new Vector3(gridSize.x * stageSpacing / 2, 0, gridSize.y * stageSpacing / 2);
        NavMeshHit hit;
        return NavMesh.SamplePosition(checkPosition, out hit, 1.0f, NavMesh.AllAreas);
    }

    private async UniTask DelayedWarpAsync()
    {
        int maxWaitAttempts = 50;
        int waitIntervalMs = 100;
        for (int attempt = 0; attempt < maxWaitAttempts; attempt++)
        {
            if (CheckNavMeshExists())
            {
                Debug.Log("[TestMap02] NavMesh生成が確認されました。ワープを再試行します。");
                await WarpPlayerAsync();
                return;
            }
            Debug.Log($"[TestMap02] NavMesh生成待機中... 試行 {attempt + 1}/{maxWaitAttempts}");
            await UniTask.Delay(waitIntervalMs, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
        Debug.LogError("[TestMap02] NavMesh生成がタイムアウトしました。ワープをキャンセルします。");
        hasPlayerSpawned = false;
    }
}