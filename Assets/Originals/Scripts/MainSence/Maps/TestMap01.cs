using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cysharp.Threading.Tasks;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestMap01 : MonoBehaviour
{
    public static TestMap01 instance { get; private set; }

    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh mesh;
    [SerializeField] float width = 1;
    [SerializeField] float height = 1;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material mapMaterial;


    [Header("ステージの見た目の色とサイズを指定")]
    [SerializeField] ObjectState WallSetting; // 壁の色とサイズ
    [SerializeField] ObjectState GroundSetting; // 地面の色とサイズ
    [SerializeField] ObjectState RoadSetting; // 道の色とサイズ


    [Header("ステージのプレハブ")]
    [SerializeField] GameObject groundPrefab; // 地面のプレハブ
    [SerializeField] GameObject wallPrefab; // 壁のプレハブ
    [SerializeField] GameObject roadPrefab; // 廊下のプレハブ


    [Header("マップ生成の初期位置")]
    [SerializeField] Vector3 defaultPosition;

    [Header("アイテムの設定")]
    [SerializeField] private List<GameObject> itemPrefabs; // 複数のアイテムプレハブ
    [SerializeField] private List<int> itemGenerateNums; // 各アイテムの生成数
    [SerializeField] public Vector3 roomCenter; // 部屋の中心位置
    [SerializeField] public Vector3 roomSize; // 部屋のサイズ


    [Header("徘徊地点の設定(徘徊地点のプレハブを設定すること)")]
    [SerializeField] private GameObject[] patrolPointPrefabs; // 徘徊地点プレハブのリスト
    [HideInInspector] public Transform[] patrolPoint; // 動的に生成された徘徊地点
    [Header("徘徊地点の親オブジェクト")]
    private GameObject patrolPointParent; // 徘徊地点の親オブジェクト

    // 徘徊地点を格納するリスト（動的生成用）
    private List<Transform> patrolPointsList = new List<Transform>();

    [Header("敵の設定")]
    [SerializeField] private List<GameObject> enemyPrefabs; // 複数の敵プレハブ
    [SerializeField] private List<int> enemyGenerateNums; // 各敵の生成数


    [Header("ゴール")]
    [SerializeField] private GameObject goalGroundPrefab; // ゴール地面のプレハブ
    [SerializeField] private GameObject goalGround; // // シーン内のゴール地面（インスタンス）
    [SerializeField] private Vector3 goalConnectionPoint; // ゴールとの接続点
    [SerializeField] private int connectionRoadLength = 3; // ゴールへの通路の長さ
    [SerializeField] private bool autoSelectConnectionPoint = true; // 接続点を自動選択するか
    [SerializeField] private GameObject goalObjectPrefab; // ゴールオブジェクトのプレハブ


    [Header("デフォルトで「Ground」レイヤーを設定")]
    [SerializeField] private LayerMask groundLayer = 1 << 8;


    [Header("マップ生成完了フラグ")]
    public bool IsMapGenerated { get; private set; }

    [Header("マップ生成中フラグ")]
    private bool isGeneratingMap = false;


    [Header("プレイヤー生成完了フラグ")]
    private bool hasPlayerSpawned = false;


    [Header("プレイヤーと敵の最小距離（ワープ時に敵と近すぎないようにする）")]
    [SerializeField] private float minDistanceFromEnemy = 5f;


    [Header("敵の位置を記録するリスト（ワープ時に敵との距離をチェックするため）")]
    private List<Vector3> enemyPositions = new List<Vector3>();



    [Header("敵生成完了フラグ")]
    private bool hasEnemiesSpawned = false;

    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;

    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [Header("マップの横サイズ")]
    [SerializeField] private int mapSizeW;

    [Header("マップの縦サイズ")]
    [SerializeField] private int mapSizeH;

    [Header("マップの管理配列")]
    private int[,] map;

    [Header("部屋の数")]
    [SerializeField] private int roomNum;

    private int roomMin = 10; // 部屋の最小値
    private int parentNum = 0; // 分割する部屋番号
    private int max = 0; // 最大面積
    private int roomCount; // 部屋カウント
    private int line = 0; // 分割点
    private int[,] roomStatus; // 部屋の管理配列

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

    private GameObject[] mapObjects; // マップ生成用のオブジェクト配列
    private GameObject[] objectParents; // 各タイプ別の親オブジェクト配列
    private const int offsetWall = 2; // 壁から離す距離
    private const int offset = 1; // 調整用

    [SerializeField] private bool isDebugOffGenerate = false;

    void Awake()
    {
        // シングルトンのチェック
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[TestMap01] 別のインスタンスがすでに存在します: {instance.gameObject.name}。このインスタンスを破棄します: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        instance = this;

        // シーン内の他のTestMap01コンポーネントを検索
        TestMap01[] instances = FindObjectsOfType<TestMap01>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"[TestMap01] シーン内に複数のTestMap01インスタンスが検出されました！({instances.Length}個)");
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != this)
                {
                    Debug.LogWarning($"[TestMap01] 他のインスタンスを破棄します: {instances[i].gameObject.name}");
                    Destroy(instances[i].gameObject);
                }
            }
        }

        //デバッグモードの場合、以降の処理をスキップ
        if (isDebugOffGenerate) return;

        // groundLayerが空の場合、（Ground）に設定
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Ground");

        int layerIndex = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));

        // マップ生成を一度だけ実行
        MapGenerate().Forget();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (!IsMapGenerated) return;

        if (!hasPlayerSpawned && !hasEnemiesSpawned && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[TestMap01] スペースキーが押されました。プレイヤーと敵を生成します。");
            SpawnPlayerAndEnemiesAsync().Forget();
        }
    }

    async UniTask RebuildNavMeshAsync()
    {
        await UniTask.DelayFrame(10);

        if (groundSurface != null)
        {
            groundSurface.BuildNavMesh();
            Debug.Log(groundSurface.navMeshData != null
                ? $"[TestMap01] groundSurface NavMesh 構築完了: 範囲={groundSurface.navMeshData.sourceBounds}"
                : "[TestMap01] groundSurface のNavMeshデータ生成に失敗しました！");
        }
        else Debug.LogError("[TestMap01] groundSurface がnullです！");

        if (roadSurface != null)
        {
            roadSurface.BuildNavMesh();
            Debug.Log(roadSurface.navMeshData != null
                ? $"[TestMap01] roadSurface NavMesh 構築完了: 範囲={roadSurface.navMeshData.sourceBounds}"
                : "[TestMap01] roadSurface のNavMeshデータ生成に失敗しました！");
        }
        else Debug.LogError("[TestMap01] roadSurface がnullです！");

        if (goalGround != null)
        {
            var goalSurface = goalGround.GetComponent<NavMeshSurface>() ?? goalGround.AddComponent<NavMeshSurface>();
            goalSurface.collectObjects = CollectObjects.All;
#if UNITY_EDITOR
            GameObjectUtility.SetStaticEditorFlags(goalGround, StaticEditorFlags.NavigationStatic);
#endif
            if (!goalGround.GetComponent<Collider>())
            {
                Debug.LogWarning("[TestMap01] goalGround にコライダーがありません！BoxCollider を追加します。");
                goalGround.AddComponent<BoxCollider>().size = GroundSetting.size;
            }
            goalSurface.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
            goalSurface.BuildNavMesh();
            Debug.Log(goalSurface.navMeshData != null
                ? $"[TestMap01] goalSurface NavMesh 構築完了: 範囲={goalSurface.navMeshData.sourceBounds}, 位置={goalGround.transform.position}"
                : "[TestMap01] goalSurface のNavMeshデータ生成に失敗しました！");
        }
        else Debug.LogError("[TestMap01] goalGround がnullです！");
    }



    void initPrefab()
    {
        // 既存の親オブジェクトがあれば破棄
        if (objectParents != null)
        {
            foreach (var parent in objectParents.Where(p => p != null))
            {
                Destroy(parent);
            }
            objectParents = null;
        }

        // 徘徊地点の親オブジェクトがあれば破棄
        if (patrolPointParent != null)
        {
            Destroy(patrolPointParent);
            patrolPointParent = null;
        }

        // 既存の親オブジェクト生成（Ground, Wall, Road）
        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");

        // 徘徊地点の親オブジェクトを生成
        patrolPointParent = new GameObject("PatrolPoints");
        patrolPointParent.transform.SetParent(transform); // TestMap01の子として設定

        // シーンに親オブジェクトを関連付ける
        groundParent.transform.SetParent(transform);
        wallParent.transform.SetParent(transform);
        roadParent.transform.SetParent(transform);

        // NavMeshSurfaceの設定（既存コード）
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

        // プレハブの検証（既存コード）
        if (groundPrefab == null || wallPrefab == null || roadPrefab == null)
        {
            Debug.LogError("プレハブが設定されていません！groundPrefab, wallPrefab, roadPrefabを確認してください。");
            return;
        }

        // プレハブがシーン内のオブジェクトでないことを確認
        if (groundPrefab.scene.IsValid() || wallPrefab.scene.IsValid() || roadPrefab.scene.IsValid())
        {
            Debug.LogError("プレハブがシーン内のオブジェクトです！プレハブアセットを指定してください。");
            return;
        }

        // 地面プレハブのインスタンス化（以下、既存コードをそのまま使用）
        GameObject ground = Instantiate(groundPrefab);
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";
        ground.transform.SetParent(groundParent.transform);
        ground.layer = LayerMask.NameToLayer("Ground");
        if (!ground.GetComponent<Collider>())
        {
            Debug.LogWarning("[TestMap01] groundPrefab にコライダーがありません。BoxColliderを追加します。");
            ground.AddComponent<BoxCollider>().size = GroundSetting.size;
        }

        // 壁プレハブのインスタンス化
        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";
        wall.transform.SetParent(wallParent.transform);

        // 道プレハブのインスタンス化
        GameObject road = Instantiate(roadPrefab);
        road.transform.localScale = RoadSetting.size;
        road.GetComponent<Renderer>().material.color = RoadSetting.color;
        road.name = "road";
        road.transform.SetParent(roadParent.transform);

        var navObstacle = wall.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true;

        mapObjects = new GameObject[] { ground, wall, road };
    }

    private async UniTask MapGenerate()
    {
        // すでにマップ生成中または生成済みの場合は実行しない
        if (isGeneratingMap || IsMapGenerated) return;

        isGeneratingMap = true;

        // マップ生成開始時にフラグをfalseに設定
        IsMapGenerated = false;

        initPrefab();

        if (mapObjects == null || mapObjects.Any(obj => obj == null))
        {
            Debug.LogError("[TestMap01] initPrefab が失敗しました。マップ生成を中断します。");
            isGeneratingMap = false;
            return;
        }

        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];
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
                    map[w + roomStatus[(int)RoomStatus.x, count], h + roomStatus[(int)RoomStatus.y, count]] = 1;
                }
            }
            for (int h = 0; h < roomStatus[(int)RoomStatus.rh, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.rw, count]; w++)
                {
                    map[w + roomStatus[(int)RoomStatus.rx, count], h + roomStatus[(int)RoomStatus.ry, count]] = 0;
                }
            }
        }

        int[] splitLength = new int[4];
        int roodPoint = 0;
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            splitLength[0] = roomStatus[(int)RoomStatus.x, nowRoom] > 0 ? roomStatus[(int)RoomStatus.rx, nowRoom] - roomStatus[(int)RoomStatus.x, nowRoom] : int.MaxValue;
            splitLength[1] = (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) < mapSizeW ? (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) - (roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom]) : int.MaxValue;
            splitLength[2] = roomStatus[(int)RoomStatus.y, nowRoom] > 0 ? roomStatus[(int)RoomStatus.ry, nowRoom] - roomStatus[(int)RoomStatus.y, nowRoom] : int.MaxValue;
            splitLength[3] = (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) < mapSizeH ? (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) - (roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom]) : int.MaxValue;

            for (int j = 0; j < splitLength.Length; j++)
            {
                if (splitLength[j] != int.MaxValue)
                {
                    if (j < 2)
                    {
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.ry, nowRoom] + offset, roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset);
                        for (int w = 1; w <= splitLength[j]; w++)
                        {
                            if (j == 0)
                            {
                                map[(-w) + roomStatus[(int)RoomStatus.rx, nowRoom], roodPoint] = 2;
                            }
                            else
                            {
                                map[w + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;
                                if (w == splitLength[j])
                                {
                                    map[w + offset + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.rx, nowRoom] + offset, roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset);
                        for (int h = 1; h <= splitLength[j]; h++)
                        {
                            if (j == 2)
                            {
                                map[roodPoint, (-h) + roomStatus[(int)RoomStatus.ry, nowRoom]] = 2;
                            }
                            else
                            {
                                map[roodPoint, h + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;
                                if (h == splitLength[j])
                                {
                                    map[roodPoint, h + offset + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;
                                }
                            }
                        }
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

        // 徘徊地点リストをクリア
        patrolPointsList.Clear();

        // 徘徊地点を各プレハブごとに生成
        for (int i = 0; i < patrolPointPrefabs.Length; i++)
        {
            if (patrolPointPrefabs[i] != null)
            {
                await GeneratePatrolPointInRoomsAsync(patrolPointPrefabs[i].transform, roomNum);
            }
            else
            {
                Debug.LogWarning($"[TestMap01] patrolPointPrefabs[{i}] がnullです。スキップします。");
            }
        }

        // 生成された徘徊地点をpatrolPoint配列に設定
        patrolPoint = patrolPointsList.ToArray();
        Debug.Log($"[TestMap01] patrolPoint配列に{patrolPoint.Length}個の徘徊地点を設定しました。");


        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
        }

        // マップ生成完了後にフラグをtrueに設定
        IsMapGenerated = true;

        isGeneratingMap = false;

        Debug.Log("[TestMap01] マップ生成が完了しました。スペースキーを押してプレイヤーを生成してください。");
    }

    // プレイヤーワープと敵生成を順番に実行
    private async UniTask SpawnPlayerAndEnemiesAsync()
    {
        //プレイヤーまたは敵がすでに生成されている場合、処理をスキップ
        if (hasPlayerSpawned || hasEnemiesSpawned) return;

        // プレイヤーをスポーン
        await SpawnPlayerAsync();

        if (!hasPlayerSpawned || hasEnemiesSpawned) return;

        enemyPositions.Clear();
        Debug.Log($"[TestMap01] 敵生成開始: プレハブ数={enemyPrefabs.Count}, 生成数リスト={string.Join(", ", enemyGenerateNums)}");

        if (enemyPrefabs.Count == 0 || enemyGenerateNums.Count == 0)
        {
            Debug.LogError("[TestMap01] 敵プレハブまたは生成数が設定されていません！");
            return;
        }

        int totalEnemiesGenerated = 0;
        int maxIndex = Mathf.Min(enemyPrefabs.Count, enemyGenerateNums.Count);
        for (int i = 0; i < maxIndex; i++)
        {
            if (enemyPrefabs[i] == null || enemyGenerateNums[i] <= 0) continue;
            Debug.Log($"[TestMap01] 敵プレハブ {enemyPrefabs[i].name} を {enemyGenerateNums[i]} 体生成します。");
            await GenerateEnemiesInRoomsAsync(enemyPrefabs[i], enemyGenerateNums[i]);
            totalEnemiesGenerated += enemyGenerateNums[i];
        }

        hasEnemiesSpawned = true;
        Debug.Log($"[TestMap01] 敵の生成が完了しました。総生成数: {totalEnemiesGenerated}");
    }


    // プレイヤーをスポーンする非同期メソッド
    public async UniTask SpawnPlayerAsync()
    {
        if (hasPlayerSpawned)
        {
            Debug.LogWarning("[TestMap01] プレイヤーはすでにスポーンしています。");
            return;
        }

        await WarpPlayerAsync();
        hasPlayerSpawned = true;
    }

    // 公開メソッドとしてワープ処理を提供（TitleControllerから呼び出せるように）
    public async UniTask TriggerWarpAsync()
    {
        //await SpawnPlayerAsync();
    }

    // プレイヤーをランダムな部屋にワープさせる
    async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] Player.instance が null です！ワープできません。");
            return;
        }

        if (!Player.instance.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[TestMap01] Player.instance ({Player.instance.name}) が非アクティブです。アクティブ化します。");
            Player.instance.gameObject.SetActive(true);
        }

        const int maxRoomAttempts = 10;
        bool playerWarped = false;

        for (int attempt = 0; attempt < maxRoomAttempts; attempt++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            Vector3 center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2f) * GroundSetting.size.z);
            Vector3 size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z);

            Vector3 warpPosition = await FindWarpPositionAsync(center, size);
            if (warpPosition == Vector3.zero) continue;

            bool isFarEnough = enemyPositions.All(enemyPos => Vector3.Distance(warpPosition, enemyPos) >= minDistanceFromEnemy);
            if (!isFarEnough) continue;

            var agent = Player.instance.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh) agent.enabled = false;

            Player.instance.transform.position = warpPosition;

            if (agent != null)
            {
                agent.enabled = true;
                if (NavMesh.SamplePosition(warpPosition, out var navHit, 10f, NavMesh.AllAreas))
                {
                    var adjustedNavPos = navHit.position + Vector3.up * 0.1f;
                    agent.Warp(adjustedNavPos);
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

        if (!playerWarped)
        {
            var fallbackPosition = defaultPosition + new Vector3(5f, 0.6f, 5f);
            Player.instance.transform.position = fallbackPosition;
        }
    }

    // ワープ位置を検索する非同期メソッド
    private async UniTask<Vector3> FindWarpPositionAsync(Vector3 roomCenter, Vector3 roomSize)
    {
        float margin = 1.0f;
        int maxAttempts = 20;
        float yOffset = Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.5f + 0.1f;

        // groundLayerの検証
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("[TestMap01] groundLayerが空です。デフォルトでGroundレイヤー（Layer 8）を設定します。");
            groundLayer = LayerMask.GetMask("Ground");
        }

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            float y = roomCenter.y + 5.0f;
            Vector3 spawnPosition = new Vector3(x, y, z);

            int mapX = Mathf.FloorToInt((x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((z - defaultPosition.z) / GroundSetting.size.z);

            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH)
            {
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 位置 ({x}, {z}) はマップ範囲外です。");
                continue;
            }

            if (map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 位置 ({x}, {z}) は地面ではありません。map[{mapX}, {mapZ}]={(objectType)map[mapX, mapZ]}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
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
                    if (!Physics.CheckSphere(adjustedPos, 0.5f, LayerMask.GetMask("Wall")))
                    {
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
                // レイヤーインデックスを安全に取得
                int layerIndex = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));
                string layerName = (groundLayer.value != 0 && layerIndex >= 0 && layerIndex < 32)
                    ? LayerMask.LayerToName(layerIndex)
                    : $"無効なレイヤー (Value={groundLayer.value})";
                Debug.LogWarning($"試行 {attempt + 1}/{maxAttempts}: 地面検出失敗: 位置={spawnPosition}, LayerMask={layerName}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 10f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"最大試行回数({maxAttempts})を超えてもワープ位置を見つけることができませんでした。roomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }

    // 敵を生成するメソッド（敵の位置を記録）
    async UniTask GenerateEnemiesInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap01] 敵プレハブがnullです！");
            return;
        }

        int enemiesGenerated = 0;

        for (int i = 0; i < generateNum; i++)
        {
            int roomIndex = Random.Range(0, roomStatus.GetLength(1));
            var center = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2f) * GroundSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2f) * GroundSetting.size.z);
            var size = new Vector3(
                roomStatus[(int)RoomStatus.rw, roomIndex] * GroundSetting.size.x,
                GroundSetting.size.y,
                roomStatus[(int)RoomStatus.rh, roomIndex] * GroundSetting.size.z);

            var position = await PlaceObjectAsync(prefab, center, size, false);
            if (position == Vector3.zero) continue;

            enemyPositions.Add(position);
            var enemy = Instantiate(prefab, position, Quaternion.identity);
            enemy.name = $"{prefab.name}_{enemiesGenerated}";
            var enemyScript = enemy.GetComponent<BaseEnemy>();
            if (enemyScript != null && Player.instance != null)
                enemyScript.tagetPoint = Player.instance.transform;
            enemiesGenerated++;
        }
    }


    private bool SplitPoint(int x, int y)
    {
        if (x > y)
        {
            line = Random.Range(roomMin + (offsetWall * 2), x - (offsetWall * 2 + roomMin));
            return true;
        }
        else
        {
            line = Random.Range(roomMin + (offsetWall * 2), y - (offsetWall * 2 + roomMin));
            return false;
        }
    }

    private void AddGoalConnection()
    {
        if (goalGround == null)
        {
            Debug.LogError("[TestMap01] goalGroundが設定されていません！プレハブまたはシーン内のオブジェクトを設定してください。");
            return;
        }

        if (!goalGround.scene.IsValid())
        {
            Debug.LogError("[TestMap01] goalGroundがシーン内のオブジェクトではありません！シーン内でインスタンス化してください。");
            return;
        }

        Vector2Int connectPoint;
        if (autoSelectConnectionPoint)
        {
            connectPoint = FindValidConnectionPoint();
            if (connectPoint == Vector2Int.one * -1)
            {
                Debug.LogWarning("[TestMap01] 有効な接続点が見つかりませんでした！");
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
            Debug.LogWarning($"[TestMap01] 接続点 ({connectX}, {connectZ}) がマップ範囲外です！");
            return;
        }

        if (map[connectX, connectZ] != (int)objectType.ground && map[connectX, connectZ] != (int)objectType.road)
        {
            Debug.LogWarning($"[TestMap01] 接続点 ({connectX}, {connectZ}) は部屋または通路ではありません！");
            return;
        }

        ClearPathToGoal(connectX, connectZ);

        // objectParentsがシーン内のオブジェクトであることを確認
        if (objectParents[(int)objectType.road] == null || !objectParents[(int)objectType.road].scene.IsValid())
        {
            Debug.LogError("[TestMap01] objectParents[road]がシーン内のオブジェクトではありません！initPrefabを確認してください。");
            return;
        }

        // 通路の生成
        for (int i = 1; i <= connectionRoadLength && connectX + i < mapSizeW; i++)
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
                Quaternion.identity);
            road.name = $"Road_{roadX}_{connectZ}";
            road.transform.SetParent(objectParents[(int)objectType.road].transform, false);
        }

        // 拡張通路の生成
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
                    Quaternion.identity);
                road.name = $"ExtendedRoad_{mapSizeW + i}_{connectZ}";
                road.transform.SetParent(objectParents[(int)objectType.road].transform, false);
            }
        }

        // ゴール地点の配置
        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);

        // ゴールオブジェクトの生成
        if (goalObjectPrefab != null)
        {
            Vector3 goalObjectPosition = goalGround.transform.position + Vector3.up * 0.1f;
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(goalObjectPosition, out navHit, 20.0f, NavMesh.AllAreas))
            {
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    navHit.position,
                    Quaternion.identity);
                goalObject.name = "GoalObject";
                goalObject.transform.SetParent(goalGround.transform, false);
            }
            else
            {
                Debug.LogWarning("[TestMap01] NavMesh 上の位置が見つからなかったため、ゴールオブジェクトを生成できませんでした");
                Vector3 fallbackPosition = goalGround.transform.position + Vector3.up * 0.1f;
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    fallbackPosition,
                    Quaternion.identity);
                goalObject.name = "GoalObject_Fallback";
                goalObject.transform.SetParent(goalGround.transform, false);
            }
        }
        else
        {
            Debug.LogWarning("[TestMap01] ゴールオブジェクトのプレハブが設定されていません！");
        }
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

                    if (isClear) validPoints.Add(new Vector2Int(x, z));
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
            }
        }
    }

    private async UniTask GeneratePatrolPointInRoomsAsync(Transform patrolPoint, int generateNum)
    {
        if (patrolPoint == null)
        {
            Debug.LogError("[TestMap01] 徘徊地点プレハブがnullです！");
            return;
        }

        if (patrolPointParent == null)
        {
            Debug.LogError("[TestMap01] 徘徊地点の親オブジェクトがnullです！initPrefabを確認してください。");
            return;
        }

        int pointsGenerated = 0;

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

            Vector3 position = await PlaceObjectAsync(patrolPoint.gameObject, center, size, isPatrolPoint: true);
            if (position != Vector3.zero)
            {
                GameObject obj = Instantiate(patrolPoint.gameObject, position, Quaternion.identity);
                obj.name = $"{patrolPoint.name}_{pointsGenerated}";
                obj.transform.SetParent(patrolPointParent.transform); // 親をPatrolPointsに設定
                patrolPointsList.Add(obj.transform); // リストに追加
                pointsGenerated++;
            }
            else
            {
                Debug.LogWarning($"[TestMap01] 徘徊地点の生成位置が見つかりませんでした。部屋インデックス: {roomIndex}");
            }
        }

        Debug.Log($"[TestMap01] 徘徊地点の生成が完了しました。生成数: {pointsGenerated}");
    }

    private async UniTask GenerateObjectsInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap01] アイテムプレハブがnullです！");
            return;
        }

        int itemsGenerated = 0;

        List<(int index, int groundArea)> roomAreas = new List<(int, int)>();
        for (int i = 0; i < roomStatus.GetLength(1); i++)
        {
            int groundArea = roomStatus[(int)RoomStatus.rw, i] * roomStatus[(int)RoomStatus.rh, i];
            roomAreas.Add((i, groundArea));
        }
        roomAreas.Sort((a, b) => b.groundArea.CompareTo(a.groundArea));

        for (int i = 0; i < generateNum; i++)
        {
            bool placed = false;
            for (int j = 0; j < Mathf.Min(3, roomAreas.Count); j++)
            {
                int roomIndex = roomAreas[j].index;
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

                Vector3 position = await PlaceObjectAsync(prefab, center, size, isPatrolPoint: false);
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
                // フォールバック：最大面積の部屋の中心に配置
                int roomIndex = roomAreas[0].index;
                Vector3 center = new Vector3(
                    defaultPosition.x + (roomStatus[(int)RoomStatus.rx, roomIndex] + roomStatus[(int)RoomStatus.rw, roomIndex] / 2.0f) * GroundSetting.size.x,
                    defaultPosition.y + prefab.transform.localScale.y * 0.5f + 0.05f,
                    defaultPosition.z + (roomStatus[(int)RoomStatus.ry, roomIndex] + roomStatus[(int)RoomStatus.rh, roomIndex] / 2.0f) * GroundSetting.size.z
                );
                GameObject obj = Instantiate(prefab, center, Quaternion.identity);
                obj.name = $"{prefab.name}_{itemsGenerated}_Fallback";
                itemsGenerated++;
            }
        }
    }

    private async UniTask<Vector3> PlaceObjectAsync(GameObject prefab, Vector3 roomCenter, Vector3 roomSize, bool isPatrolPoint)
    {
        string objectName = prefab.name;
        float margin = 0.5f; // マージンを小さくして配置可能な範囲を拡大
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
                Debug.LogWarning($"[TestMap01] 試行 {attempt + 1}/{maxAttempts}: 位置 ({x}, {z}) は地面ではありません。map[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? (objectType)map[mapX, mapZ] : "範囲外")} ({objectName})");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    Debug.LogWarning($"[TestMap01] 試行 {attempt + 1}/{maxAttempts}: ヒットしたオブジェクト {hit.collider.gameObject.name} はGroundレイヤーではありません ({objectName})");
                    continue;
                }

                Vector3 finalPos = hit.point + Vector3.up * (isPatrolPoint ? 0.05f : prefab.transform.localScale.y * 0.5f + 0.05f);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
                {
                    Debug.Log($"[TestMap01] 試行 {attempt + 1}/{maxAttempts}: 配置位置 {navHit.position} を見つけました (NavMesh, {objectName})");
                    return navHit.position;
                }
                else
                {
                    // NavMeshが見つからない場合、Raycastのヒット位置を使用
                    Debug.Log($"[TestMap01] 試行 {attempt + 1}/{maxAttempts}: NavMeshが見つかりませんでしたが、地面位置 {finalPos} を使用します ({objectName})");
                    return finalPos;
                }
            }
            else
            {
                string layerName = groundLayer.value == 0 ? "空" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)));
                Debug.LogWarning($"[TestMap01] 試行 {attempt + 1}/{maxAttempts}: 地面検出失敗: 位置={spawnPosition}, LayerMask={layerName}, LayerMaskValue={groundLayer.value} ({objectName})");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"[TestMap01] 最大試行回数({maxAttempts})を超えても {objectName} の配置位置を見つけられませんでした。roomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }
}