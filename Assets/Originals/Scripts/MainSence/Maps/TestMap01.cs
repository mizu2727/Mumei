using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cysharp.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMap01 : MonoBehaviour
{
    public static TestMap01 Instance { get; private set; }

    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh mesh;
    [SerializeField] float width = 1;
    [SerializeField] float height = 1;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material mapMaterial;

    [SerializeField] ObjectState WallSetting;
    [SerializeField] ObjectState GroundSetting;
    [SerializeField] ObjectState RoadSetting;

    [SerializeField] GameObject groundPrefab;
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject roadPrefab;

    [SerializeField] Vector3 defaultPosition;

    [Header("アイテムの設定")]
    [SerializeField] private List<GameObject> itemPrefabs;
    [SerializeField] private List<int> itemGenerateNums;
    [SerializeField] public Vector3 roomCenter;
    [SerializeField] public Vector3 roomSize;

    [SerializeField] public Transform[] patrolPoint;

    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<int> enemyGenerateNums;

    private List<Vector3> enemyPositions = new List<Vector3>();

    [SerializeField] private float minDistanceFromEnemy = 15f;

    [SerializeField] private GameObject goalGround;
    [SerializeField] private Vector3 goalConnectionPoint;
    [SerializeField] private int connectionRoadLength = 3;
    [SerializeField] private bool autoSelectConnectionPoint = true;
    [SerializeField] private GameObject goalObjectPrefab;

    [SerializeField] private LayerMask groundLayer = 1 << 8;

    [SerializeField] private GameObject playerPrefab;

    public bool IsMapGenerated { get; private set; } = false;

    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;

    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [SerializeField] private int mapSizeW;
    [SerializeField] private int mapSizeH;
    private int[,] map;

    [SerializeField] private int roomNum;
    private int roomMin = 15;
    private int parentNum = 0;
    private int max = 0;
    private int roomCount;
    private int line = 0;
    private int[,] roomStatus;

    private enum RoomStatus
    {
        x, y, w, h, rx, ry, rw, rh
    }

    enum objectType
    {
        ground = 0,
        wall = 1,
        road = 2,
    }

    private GameObject[] mapObjects;
    private GameObject[] objectParents;
    private const int offsetWall = 2;
    private const int offset = 1;

    private bool hasPlayerSpawned = false; // プレイヤーが生成済みかを追跡

    [SerializeField] private bool isDebugOffGenerate = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[TestMap01] インスタンスが既に存在します！({Instance.gameObject.name})。このインスタンスを破棄します。");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"[TestMap01] シングルトン初期化: {gameObject.name}");

        if (mapSizeW <= 0)
        {
            mapSizeW = 50;
            Debug.LogWarning($"[TestMap01] mapSizeW が無効（{mapSizeW}）でした。デフォルト値 50 に設定します。");
        }
        if (mapSizeH <= 0)
        {
            mapSizeH = 50;
            Debug.LogWarning($"[TestMap01] mapSizeH が無効（{mapSizeH}）でした。デフォルト値 50 に設定します。");
        }
        if (roomNum <= 0)
        {
            roomNum = 5;
            Debug.LogWarning($"[TestMap01] roomNum が無効（{roomNum}）でした。デフォルト値 5 に設定します。");
        }

        if (groundLayer.value == 0)
        {
            Debug.LogWarning("[TestMap01] groundLayerが空です！デフォルト値（Ground）に設定します。");
            groundLayer = LayerMask.GetMask("Ground");
        }
        if (playerPrefab == null)
        {
            Debug.LogError("[TestMap01] プレイヤーのプレハブがインスペクターで設定されていません！");
        }

        MapGenerate().Forget();
    }

    private void Update()
    {
        // スペースキー押下でプレイヤー生成
        if (IsMapGenerated && !hasPlayerSpawned && Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPlayerAsync().Forget();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public async UniTask TriggerWarpAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] プレイヤーが存在しません！ワープできません。プレイヤー生成を確認してください。");
            return;
        }
        await WarpPlayerAsync();
    }

    private async UniTask RebuildNavMeshAsync()
    {
        await UniTask.Yield();
        await UniTask.DelayFrame(150); // 待機時間を延長してコライダー生成を待つ
        try
        {
            if (groundSurface != null)
            {
                groundSurface.BuildNavMesh();
                Debug.Log("Ground NavMesh を再構築しました");
            }
            else
            {
                Debug.LogWarning("groundSurface が null です。NavMesh生成をスキップします。");
            }
            if (roadSurface != null)
            {
                roadSurface.BuildNavMesh();
                Debug.Log("Road NavMesh を再構築しました");
            }
            else
            {
                Debug.LogWarning("roadSurface が null です。NavMesh生成をスキップします。");
            }
            if (goalGround != null)
            {
                var goalSurface = goalGround.GetComponent<NavMeshSurface>();
                if (goalSurface == null)
                {
                    goalSurface = goalGround.AddComponent<NavMeshSurface>();
                    goalSurface.collectObjects = CollectObjects.All;
#if UNITY_EDITOR
                    GameObjectUtility.SetStaticEditorFlags(goalGround, StaticEditorFlags.NavigationStatic);
#endif
                }
                if (!goalGround.GetComponent<Collider>())
                {
                    Debug.LogWarning("goalGround にコライダーがありません！BoxCollider を追加します。");
                    var collider = goalGround.AddComponent<BoxCollider>();
                    collider.size = GroundSetting.size;
                }
                goalSurface.BuildNavMesh();
                Debug.Log($"Goal NavMesh を再構築しました: {goalGround.transform.position}");
            }
            else
            {
                Debug.LogWarning("goalGround が null です。NavMesh生成をスキップします。");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"NavMesh構築中にエラーが発生しました: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void initPrefab()
    {
        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");

        groundSurface = groundParent.AddComponent<NavMeshSurface>();
        groundSurface.collectObjects = CollectObjects.All;
#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(groundParent, StaticEditorFlags.NavigationStatic);
#endif

        roadSurface = roadParent.AddComponent<NavMeshSurface>();
        roadSurface.collectObjects = CollectObjects.All;
#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(roadParent, StaticEditorFlags.NavigationStatic);
#endif

        objectParents = new GameObject[] { groundParent, wallParent, roadParent };

        // プレハブをシーン内でインスタンス化
        GameObject ground = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity, groundParent.transform);
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";

        GameObject wall = Instantiate(wallPrefab, Vector3.zero, Quaternion.identity, wallParent.transform);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";

        GameObject road = Instantiate(roadPrefab, Vector3.zero, Quaternion.identity, roadParent.transform);
        road.transform.localScale = RoadSetting.size;
        road.GetComponent<Renderer>().material.color = RoadSetting.color;
        road.name = "road";

        var navObstacle = wall.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true;

        mapObjects = new GameObject[] { ground, wall, road };
    }

    private async UniTask MapGenerate()
    {
        IsMapGenerated = false;

        // 入力値の検証
        if (roomNum <= 0)
        {
            Debug.LogError($"roomNum が無効です: {roomNum}。1以上の値を設定してください。");
            return;
        }

        initPrefab();
        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];

        for (int i = 0; i < roomNum; i++)
        {
            for (int j = 0; j < System.Enum.GetNames(typeof(RoomStatus)).Length; j++)
            {
                roomStatus[j, i] = 0;
            }
        }

        map = new int[mapSizeW, mapSizeH];
        for (int nowW = 0; nowW < mapSizeW; nowW++)
        {
            for (int nowH = 0; nowH < mapSizeH; nowH++)
            {
                map[nowW, nowH] = 2; // 壁
            }
        }

        roomStatus[(int)RoomStatus.x, roomCount] = 0;
        roomStatus[(int)RoomStatus.y, roomCount] = 0;
        roomStatus[(int)RoomStatus.w, roomCount] = mapSizeW;
        roomStatus[(int)RoomStatus.h, roomCount] = mapSizeH;
        roomCount++;

        for (int splitNum = 0; splitNum < roomNum - 1; splitNum++)
        {
            if (roomCount >= roomNum)
            {
                Debug.LogError($"roomCount ({roomCount}) が roomNum ({roomNum}) を超えました。分割を中止します。");
                break;
            }

            parentNum = 0;
            max = 0;
            for (int maxCheck = 0; maxCheck < roomNum; maxCheck++)
            {
                if (max < roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck])
                {
                    max = roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck];
                    parentNum = maxCheck;
                }
            }

            if (SplitPoint(roomStatus[(int)RoomStatus.w, parentNum], roomStatus[(int)RoomStatus.h, parentNum]))
            {
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum] - line;
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum];
                roomStatus[(int)RoomStatus.x, parentNum] += roomStatus[(int)RoomStatus.w, roomCount];
                roomStatus[(int)RoomStatus.w, parentNum] -= roomStatus[(int)RoomStatus.w, roomCount];
            }
            else
            {
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum];
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum] - line;
                roomStatus[(int)RoomStatus.y, parentNum] += roomStatus[(int)RoomStatus.h, roomCount];
                roomStatus[(int)RoomStatus.h, parentNum] -= roomStatus[(int)RoomStatus.h, roomCount];
            }
            roomCount++;
        }

        for (int i = 0; i < roomNum; i++)
        {
            roomStatus[(int)RoomStatus.rx, i] = Random.Range(roomStatus[(int)RoomStatus.x, i] + offsetWall, (roomStatus[(int)RoomStatus.x, i] + roomStatus[(int)RoomStatus.w, i]) - (roomMin + offsetWall));
            roomStatus[(int)RoomStatus.ry, i] = Random.Range(roomStatus[(int)RoomStatus.y, i] + offsetWall, (roomStatus[(int)RoomStatus.y, i] + roomStatus[(int)RoomStatus.h, i]) - (roomMin + offsetWall));
            roomStatus[(int)RoomStatus.rw, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.w, i] - (roomStatus[(int)RoomStatus.rx, i] - roomStatus[(int)RoomStatus.x, i]) - offset);
            roomStatus[(int)RoomStatus.rh, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.h, i] - (roomStatus[(int)RoomStatus.ry, i] - roomStatus[(int)RoomStatus.y, i]) - offset);
        }

        for (int count = 0; count < roomNum; count++)
        {
            for (int h = 0; h < roomStatus[(int)RoomStatus.h, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.w, count]; w++)
                {
                    int xIndex = w + roomStatus[(int)RoomStatus.x, count];
                    int yIndex = h + roomStatus[(int)RoomStatus.y, count];
                    if (xIndex >= 0 && xIndex < mapSizeW && yIndex >= 0 && yIndex < mapSizeH)
                    {
                        map[xIndex, yIndex] = 1;
                    }
                    else
                    {
                        Debug.LogWarning($"マップ範囲外アクセスを防止: xIndex={xIndex}, yIndex={yIndex}, mapSize=({mapSizeW}, {mapSizeH})");
                    }
                }
            }
            for (int h = 0; h < roomStatus[(int)RoomStatus.rh, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.rw, count]; w++)
                {
                    int xIndex = w + roomStatus[(int)RoomStatus.rx, count];
                    int yIndex = h + roomStatus[(int)RoomStatus.ry, count];
                    if (xIndex >= 0 && xIndex < mapSizeW && yIndex >= 0 && yIndex < mapSizeH)
                    {
                        map[xIndex, yIndex] = 0;
                    }
                    else
                    {
                        Debug.LogWarning($"マップ範囲外アクセスを防止: xIndex={xIndex}, yIndex={yIndex}, mapSize=({mapSizeW}, {mapSizeH})");
                    }
                }
            }
        }

        int roadVec1 = 0;
        int roadVec2 = 0;
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            roadVec1 = 0;
            roadVec2 = 0;
            for (int roodScan = 0; roodScan < roomStatus[(int)RoomStatus.w, nowRoom]; roodScan++)
            {
                if (map[roodScan + roomStatus[(int)RoomStatus.x, nowRoom], roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    if (roadVec1 == 0)
                    {
                        roadVec1 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                    else
                    {
                        roadVec2 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                }
            }
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                map[roadSet, roomStatus[(int)RoomStatus.y, nowRoom]] = 2;
            }

            roadVec1 = 0;
            roadVec2 = 0;
            for (int roadScan = 0; roadScan < roomStatus[(int)RoomStatus.h, nowRoom]; roadScan++)
            {
                if (map[roomStatus[(int)RoomStatus.x, nowRoom], roadScan + roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    if (roadVec1 == 0)
                    {
                        roadVec1 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                    else
                    {
                        roadVec2 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                }
            }
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                map[roomStatus[(int)RoomStatus.x, nowRoom], roadSet] = 2;
            }
        }

        for (int nowH = 0; nowH < mapSizeH; nowH++)
        {
            for (int nowW = 0; nowW < mapSizeW; nowW++)
            {
                if (map[nowW, nowH] == (int)objectType.wall)
                {
                    GameObject mazeObject = Instantiate(
                        mapObjects[map[nowW, nowH]],
                        new Vector3(
                            defaultPosition.x + nowW * mapObjects[map[nowW, nowH]].transform.localScale.x,
                            defaultPosition.y + ((WallSetting.size.y - 1) * 0.5f),
                            defaultPosition.z + nowH * mapObjects[map[nowW, nowH]].transform.localScale.z),
                        Quaternion.identity, objectParents[map[nowW, nowH]].transform);
                }
                else if (map[nowW, nowH] == (int)objectType.ground)
                {
                    GameObject mazeObject = Instantiate(
                        mapObjects[map[nowW, nowH]],
                        new Vector3(
                            defaultPosition.x + nowW * mapObjects[map[nowW, nowH]].transform.localScale.x,
                            defaultPosition.y,
                            defaultPosition.z + nowH * mapObjects[map[nowW, nowH]].transform.localScale.z),
                        Quaternion.identity, objectParents[map[nowW, nowH]].transform);
                }
                else if (map[nowW, nowH] == (int)objectType.road)
                {
                    GameObject mazeObject = Instantiate(
                        mapObjects[map[nowW, nowH]],
                        new Vector3(
                            defaultPosition.x + nowW * mapObjects[map[nowW, nowH]].transform.localScale.x,
                            defaultPosition.y,
                            defaultPosition.z + nowH * mapObjects[map[nowW, nowH]].transform.localScale.z),
                        Quaternion.identity, objectParents[map[nowW, nowH]].transform);
                }
            }
        }

        await RebuildNavMeshAsync();
        AddGoalConnection();

        for (int i = 0; i < roomNum; i++)
        {
            await GeneratePatrolPointInRoomsAsync(patrolPoint[i], roomNum);
        }
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            await GenerateEnemiesInRoomsAsync(enemyPrefabs[i], enemyGenerateNums[i]);
        }
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
        }


        IsMapGenerated = true;
        Debug.Log("[TestMap01] マップ生成が完了しました。スペースキーを押してプレイヤーを生成してください。");
    }

    private async UniTask SpawnPlayerAsync()
    {
        if (hasPlayerSpawned)
        {
            Debug.LogWarning("[TestMap01] プレイヤーはすでに生成されています。");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[TestMap01] プレイヤーのプレハブが設定されていません！プレイヤーを生成できません。");
            return;
        }

        Debug.Log($"[TestMap01] プレイヤー生成を開始: プレハブ={playerPrefab.name}");
        GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log($"[TestMap01] プレイヤーを生成しました: {playerObj.name}");

        hasPlayerSpawned = true;
        await WarpPlayerAsync(); // 生成後にワープ
    }


    private bool SplitPoint(int x, int y)
    {
        int minSize = roomMin + (offsetWall * 2);
        if (x < minSize && y < minSize)
        {
            Debug.LogWarning($"部屋サイズが小さすぎます。x={x}, y={y}, 最小サイズ={minSize}。分割をスキップします。");
            return false;
        }

        if (x > y)
        {
            line = Random.Range(minSize, x - minSize);
            if (line <= 0 || line >= x)
            {
                Debug.LogError($"無効な line 値: line={line}, x={x}, minSize={minSize}");
                line = Mathf.Clamp(line, minSize, x - minSize);
            }
            return true;
        }
        else
        {
            line = Random.Range(minSize, y - minSize);
            if (line <= 0 || line >= y)
            {
                Debug.LogError($"無効な line 値: line={line}, y={y}, minSize={minSize}");
                line = Mathf.Clamp(line, minSize, y - minSize);
            }
            return false;
        }
    }

    private void AddGoalConnection()
    {
        if (goalGround == null)
        {
            Debug.LogWarning("GoalGround が設定されていません！");
            return;
        }

        // goalGround がシーン内にあることを確認
        if (!goalGround.scene.IsValid() || goalGround.scene != gameObject.scene)
        {
            Debug.LogWarning("goalGround が無効なシーンに属しています。新しい goalGround をシーン内に作成します。");
            GameObject newGoalGround = Instantiate(goalGround, Vector3.zero, Quaternion.identity);
            goalGround = newGoalGround;
        }

        Transform roadParent = objectParents[(int)objectType.road].transform;
        if (!roadParent.gameObject.scene.IsValid() || roadParent.gameObject.scene != gameObject.scene)
        {
            Debug.LogWarning("Road の親オブジェクトが無効です。新しい親をシーン内に作成します。");
            GameObject newRoadParent = new GameObject("Road_Temp");
            newRoadParent.transform.SetParent(transform, false);
            roadParent = newRoadParent.transform;
        }

        Vector2Int connectPoint;
        if (autoSelectConnectionPoint)
        {
            connectPoint = FindValidConnectionPoint();
            if (connectPoint == Vector2Int.one * -1)
            {
                Debug.LogWarning("有効な接続点が見つかりませんでした！");
                return;
            }
            goalConnectionPoint = new Vector3(connectPoint.x, 0, connectPoint.y);
        }
        else
        {
            connectPoint = new Vector2Int((int)goalConnectionPoint.x, (int)goalConnectionPoint.z);
        }

        int connectX = connectPoint.x;
        int connectZ = connectPoint.y;
        if (connectX < 0 || connectX >= mapSizeW || connectZ < 0 || connectZ >= mapSizeH)
        {
            Debug.LogWarning($"接続点 ({connectX}, {connectZ}) がマップ範囲外です！");
            return;
        }

        if (map[connectX, connectZ] != (int)objectType.ground && map[connectX, connectZ] != (int)objectType.road)
        {
            Debug.LogWarning($"接続点 ({connectX}, {connectZ}) は部屋または通路ではありません！");
            return;
        }

        ClearPathToGoal(connectX, connectZ);

        for (int i = 1; i <= connectionRoadLength; i++)
        {
            int roadX = connectX + i;
            if (roadX >= mapSizeW) break;
            Vector3 roadPos = new Vector3(
                defaultPosition.x + roadX * RoadSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + connectZ * RoadSetting.size.z);
            GameObject road = Instantiate(
                mapObjects[(int)objectType.road],
                roadPos,
                Quaternion.identity,
                roadParent);
            if (!road.GetComponent<Collider>())
            {
                Debug.LogWarning($"道路オブジェクト {road.name} にコライダーがありません！BoxColliderを追加します。");
                var collider = road.AddComponent<BoxCollider>();
                collider.size = RoadSetting.size;
            }
        }

        int extendedRoads = connectionRoadLength - (mapSizeW - connectX - 1);
        if (extendedRoads > 0)
        {
            for (int i = 0; i < extendedRoads; i++)
            {
                Vector3 roadPos = new Vector3(
                    defaultPosition.x + (mapSizeW + i) * RoadSetting.size.x,
                    defaultPosition.y,
                    defaultPosition.z + connectZ * RoadSetting.size.z);
                GameObject road = Instantiate(
                    mapObjects[(int)objectType.road],
                    roadPos,
                    Quaternion.identity,
                    roadParent);
                if (!road.GetComponent<Collider>())
                {
                    Debug.LogWarning($"拡張道路オブジェクト {road.name} にコライダーがありません！BoxColliderを追加します。");
                    var collider = road.AddComponent<BoxCollider>();
                    collider.size = RoadSetting.size;
                }
                Debug.Log($"拡張通路を生成: {roadPos}");
            }
        }

        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);
        Debug.Log($"GoalGround を {goalGround.transform.position} に配置");

        if (!goalGround.GetComponent<Collider>())
        {
            Debug.LogWarning($"goalGround にコライダーがありません！BoxColliderを追加します。");
            var collider = goalGround.AddComponent<BoxCollider>();
            collider.size = GroundSetting.size;
        }

        int goalMapX = Mathf.FloorToInt((goalGround.transform.position.x - defaultPosition.x) / GroundSetting.size.x);
        int goalMapZ = Mathf.FloorToInt((goalGround.transform.position.z - defaultPosition.z) / GroundSetting.size.z);
        Debug.Log($"GoalGround マップ位置: ({goalMapX}, {goalMapZ}), マップ範囲: ({mapSizeW}, {mapSizeH})");

        if (goalObjectPrefab != null)
        {
            Vector3 goalObjectPosition = goalGround.transform.position + Vector3.up * 0.1f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(goalObjectPosition, out navHit, 20.0f, NavMesh.AllAreas))
            {
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    navHit.position,
                    Quaternion.identity,
                    goalGround.transform);
                Debug.Log($"ゴールオブジェクトを {navHit.position} に生成しました");
            }
            else
            {
                Debug.LogWarning($"NavMesh 上の位置が見つからなかったため、ゴールオブジェクトを生成できませんでした: {goalObjectPosition}");
                Vector3 fallbackPosition = goalGround.transform.position + Vector3.up * 0.1f;
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    fallbackPosition,
                    Quaternion.identity,
                    goalGround.transform);
                Debug.Log($"フォールバック: ゴールオブジェクトを {fallbackPosition} に生成しました（NavMeshなし）");
            }
        }
        else
        {
            Debug.LogWarning("ゴールオブジェクトのプレハブが設定されていません！");
        }
    }

    private async UniTask GenerateEnemiesInRoomsAsync(GameObject prefab, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            Vector3 center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2.0f) * GroundSetting.size.z
            );
            Vector3 size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z
            );

            Vector3 enemyPosition = await PlaceEnemyAsync(prefab, center, size);
            if (enemyPosition != Vector3.zero)
            {
                enemyPositions.Add(enemyPosition);
                Debug.Log($"敵を {enemyPosition} に生成しました");
            }
        }
    }

    private async UniTask<Vector3> PlaceEnemyAsync(GameObject prefab, Vector3 roomCenter, Vector3 roomSize)
    {
        string objectName = prefab.name;
        float margin = 1.0f;
        int maxAttempts = 5;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            float y = roomCenter.y + 5.0f;
            Vector3 spawnPosition = new Vector3(x, y, z);

            int mapX = Mathf.FloorToInt((x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((z - defaultPosition.z) / GroundSetting.size.z);
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH || map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"位置 ({x}, {z}) は地面ではありません。試行 {attempt + 1}/{maxAttempts}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                Vector3 finalPos = hit.point + Vector3.up * (prefab.transform.localScale.y * 0.5f + 0.05f);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
                {
                    Instantiate(prefab, navHit.position, Quaternion.identity);
                    return navHit.position;
                }
                else
                {
                    Debug.LogWarning($"NavMesh上の位置が見つからず {objectName} を生成できませんでした: {finalPos}");
                }
            }
            else
            {
                Debug.LogWarning($"地面検出失敗: 位置={spawnPosition}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }
        Debug.LogError($"最大試行回数を超えても {objectName} を生成できませんでした");
        return Vector3.zero;
    }

    private async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] Player.instance が null です！ワープできません。");
            return;
        }

        if (!Player.instance.gameObject.activeInHierarchy)
        {
            Debug.LogError($"[TestMap01] Player.instance ({Player.instance.name}) が非アクティブです！ワープできません。");
            return;
        }

        int maxRoomAttempts = 10;
        bool playerWarped = false;

        for (int roomAttempt = 0; roomAttempt < maxRoomAttempts; roomAttempt++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            Vector3 center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2.0f) * GroundSetting.size.z
            );
            Vector3 size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z
            );

            Vector3 warpPosition = await FindWarpPositionAsync(center, size);
            if (warpPosition != Vector3.zero)
            {
                bool isFarEnough = true;
                foreach (Vector3 enemyPos in enemyPositions)
                {
                    float distance = Vector3.Distance(warpPosition, enemyPos);
                    if (distance < minDistanceFromEnemy)
                    {
                        isFarEnough = false;
                        Debug.Log($"[TestMap01] ワープ位置 {warpPosition} が敵 {enemyPos} に近すぎます（距離: {distance} < {minDistanceFromEnemy}）");
                        break;
                    }
                }

                if (isFarEnough)
                {
                    NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
                    if (agent != null && agent.isOnNavMesh)
                    {
                        agent.enabled = false;
                        Debug.Log($"[TestMap01] NavMeshAgent を一時的に無効化: {Player.instance.name}");
                    }

                    Player.instance.transform.position = warpPosition;
                    Debug.Log($"[TestMap01] プレイヤー初期位置: {warpPosition} （{Player.instance.name} をワープしました）");

                    if (agent != null)
                    {
                        agent.enabled = true;
                        NavMeshHit navHit;
                        if (NavMesh.SamplePosition(warpPosition, out navHit, 10.0f, NavMesh.AllAreas))
                        {
                            Vector3 adjustedNavPos = navHit.position + Vector3.up * 0.1f;
                            agent.Warp(adjustedNavPos);
                            Debug.Log($"[TestMap01] NavMeshAgent を {adjustedNavPos} にワープしました。");
                        }
                        else
                        {
                            Debug.LogWarning($"[TestMap01] ワープ位置 {warpPosition} はNavMesh上にありません。NavMeshAgentのワープをスキップします。");
                            Player.instance.transform.position = warpPosition;
                        }
                    }

                    playerWarped = true;
                    break;
                }
            }
        }

        if (!playerWarped)
        {
            Debug.LogWarning("[TestMap01] 適切なワープ位置が見つかりませんでした。フォールバック位置にワープします。");

            Vector3 fallbackPosition = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, 0] + roomStatus[(int)RoomStatus.rw, 0] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y + (Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.6f),
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, 0] + roomStatus[(int)RoomStatus.rh, 0] / 2.0f) * GroundSetting.size.z
            );

            int mapX = Mathf.FloorToInt((fallbackPosition.x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((fallbackPosition.z - defaultPosition.z) / GroundSetting.size.z);
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH || map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogError($"[TestMap01] フォールバック位置 {fallbackPosition} は無効です。mapX={mapX}, mapZ={mapZ}, map[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? (objectType)map[mapX, mapZ] : "範囲外")}");
                return;
            }

            NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.enabled = false;
                Debug.Log($"[TestMap01] NavMeshAgent を一時的に無効化（フォールバック）: {Player.instance.name}");
            }

            Player.instance.transform.position = fallbackPosition;
            Debug.Log($"[TestMap01] プレイヤー初期位置（フォールバック）: {fallbackPosition} （{Player.instance.name} をワープしました）");

            if (agent != null)
            {
                agent.enabled = true;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(fallbackPosition, out navHit, 10.0f, NavMesh.AllAreas))
                {
                    Vector3 adjustedNavPos = navHit.position + Vector3.up * 0.1f;
                    agent.Warp(adjustedNavPos);
                    Debug.Log($"[TestMap01] NavMeshAgent をフォールバック位置 {adjustedNavPos} にワープしました。");
                }
                else
                {
                    Debug.LogWarning($"[TestMap01] フォールバック位置 {fallbackPosition} もNavMesh上にありません。NavMeshAgentのワープをスキップします。");
                }
            }
        }
    }


    private async UniTask<Vector3> FindWarpPositionAsync(Vector3 roomCenter, Vector3 roomSize)
    {
        float margin = 1.0f;
        int maxAttempts = 20;
        float yOffset = Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.5f + 0.1f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            float y = roomCenter.y + 5.0f;
            Vector3 spawnPosition = new Vector3(x, y, z);

            int mapX = Mathf.FloorToInt((x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((z - defaultPosition.z) / GroundSetting.size.z);

            // マップ範囲外チェック
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH)
            {
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 位置 ({x}, {z}) はマップ範囲外です。");
                continue;
            }

            // 地面チェック
            if (map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 位置 ({x}, {z}) は地面ではありません。map[{mapX}, {mapZ}]={(objectType)map[mapX, mapZ]}");
                continue;
            }

            // レイキャストで地面を確認
            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                // ヒットしたオブジェクトが正しい地面か確認
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: ヒットしたオブジェクト {hit.collider.gameObject.name} はGroundレイヤーではありません。");
                    continue;
                }

                Vector3 finalPos = hit.point + Vector3.up * yOffset;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
                {
                    Vector3 adjustedPos = navHit.position + Vector3.up * 0.1f;
                    // ワープ位置が壁と衝突しないか確認
                    if (!Physics.CheckSphere(adjustedPos, 0.5f, LayerMask.GetMask("Wall")))
                    {
                        Debug.Log($"試行 {attempt + 1}/{maxAttempts}: ワープ位置 {adjustedPos} を見つけました（地面ヒット: {hit.point}, コライダー: {hit.collider.gameObject.name})");
                        return adjustedPos;
                    }
                    else
                    {
                        Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: ワープ位置 {adjustedPos} が壁と衝突しています。");
                    }
                }
                else
                {
                    Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: NavMesh上の位置が見つかりませんでした: {finalPos}");
                }
            }
            else
            {
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 地面検出失敗: 位置={spawnPosition}, LayerMask={LayerMask.LayerToName(groundLayer.value)}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 10f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"最大試行回数({maxAttempts})を超えてもワープ位置を見つけることができませんでした。roomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }

    private Vector2Int FindValidConnectionPoint()
    {
        List<Vector2Int> validPoints = new List<Vector2Int>();
        for (int x = 1; x < mapSizeW - 1; x++)
        {
            for (int z = 1; z < mapSizeH - 1; z++)
            {
                if (map[x, z] == (int)objectType.ground || map[x, z] == (int)objectType.road)
                {
                    bool isClear = true;
                    for (int i = 1; i <= connectionRoadLength && x + i < mapSizeW; i++)
                    {
                        if (map[x + i, z] == (int)objectType.wall)
                        {
                            isClear = false;
                            break;
                        }
                    }
                    if (isClear)
                    {
                        validPoints.Add(new Vector2Int(x, z));
                    }
                }
            }
        }

        if (validPoints.Count == 0)
        {
            Debug.LogWarning("有効な接続点が見つかりませんでした！壁に隣接しない部屋/通路を試してください。");
            return Vector2Int.one * -1;
        }
        return validPoints[Random.Range(0, validPoints.Count)];
    }

    private void ClearPathToGoal(int startX, int startZ)
    {
        for (int i = 1; i <= connectionRoadLength && startX + i < mapSizeW; i++)
        {
            int roadX = startX + i;
            if (map[roadX, startZ] == (int)objectType.wall)
            {
                map[roadX, startZ] = (int)objectType.road;
                Debug.Log($"壁を道路に変更: ({roadX}, {startZ})");
            }
        }
    }

    private async UniTask GeneratePatrolPointInRoomsAsync(Transform patrolPoint, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            Vector3 center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2.0f) * GroundSetting.size.z
            );
            Vector3 size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z
            );

            await PlaceObjectAsync(patrolPoint.gameObject, center, size, isPatrolPoint: true);
        }
    }

    private async UniTask GenerateObjectsInRoomsAsync(GameObject prefab, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            Vector3 center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2.0f) * GroundSetting.size.z
            );
            Vector3 size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z
            );
            await PlaceObjectAsync(prefab, center, size, isPatrolPoint: false);
        }
    }

    private async UniTask PlaceObjectAsync(GameObject prefab, Vector3 roomCenter, Vector3 roomSize, bool isPatrolPoint)
    {
        string objectName = prefab.name;

        float margin = 1.0f;
        int maxAttempts = 5;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            float y = roomCenter.y + 5.0f;
            Vector3 spawnPosition = new Vector3(x, y, z);

            int mapX = Mathf.FloorToInt((x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((z - defaultPosition.z) / GroundSetting.size.z);
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH || map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"位置 ({x}, {z}) は地面ではありません。試行 {attempt + 1}/{maxAttempts}, map[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? map[mapX, mapZ] : "範囲外")}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                Debug.Log($"地面検出成功: ヒット位置={hit.point}, オブジェクト={hit.collider.gameObject.name}, LayerMask={LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)))}");
                Vector3 finalPos = hit.point + Vector3.up * (isPatrolPoint ? 0.05f : prefab.transform.localScale.y * 0.5f + 0.05f);
                if (isPatrolPoint)
                {
                    NavMeshHit navHit;
                    if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
                    {
                        Instantiate(prefab, navHit.position, Quaternion.identity);
                        return;
                    }
                    else
                    {
                        Debug.LogWarning($"NavMesh上の位置が見つからず {objectName} を生成できませんでした: {finalPos}");
                    }
                }
                else
                {
                    Instantiate(prefab, finalPos, Quaternion.identity);
                    Debug.Log($"{objectName} を {finalPos} に生成しました");
                    return;
                }
            }
            else
            {
                string layerName = groundLayer.value == 0 ? "空" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)));
                Debug.LogWarning($"地面検出失敗: 位置={spawnPosition}, LayerMask={layerName}, LayerMaskValue={groundLayer.value}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }
        Debug.LogError($"最大試行回数を超えても {objectName} を生成できませんでした");
    }
}