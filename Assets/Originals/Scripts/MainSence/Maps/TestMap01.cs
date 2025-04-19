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
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh mesh;
    [SerializeField] float width = 1;
    [SerializeField] float height = 1;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material mapMaterial;

    // ステージの見た目の色とサイズを指定
    [SerializeField] ObjectState WallSetting; // 壁の色とサイズ
    [SerializeField] ObjectState GroundSetting; // 地面の色とサイズ
    [SerializeField] ObjectState RoadSetting; // 道の色とサイズ

    // ステージのプレハブ
    [SerializeField] GameObject groundPrefab; // 地面のプレハブ
    [SerializeField] GameObject wallPrefab; // 壁のプレハブ
    [SerializeField] GameObject roadPrefab; // 廊下のプレハブ

    // マップ生成の初期位置
    [SerializeField] Vector3 defaultPosition; // マップ生成の初期位置

    // アイテムの設定
    [Header("アイテムの設定")]
    [SerializeField] private List<GameObject> itemPrefabs; // 複数のアイテムプレハブ
    [SerializeField] private List<int> itemGenerateNums; // 各アイテムの生成数
    [SerializeField] public Vector3 roomCenter; // 部屋の中心位置
    [SerializeField] public Vector3 roomSize; // 部屋のサイズ

    // 徘徊地点の設定
    [SerializeField] public Transform[] patrolPoint; // 徘徊地点のプレハブ

    // 敵の設定
    [SerializeField] private List<GameObject> enemyPrefabs; // 複数の敵プレハブ
    [SerializeField] private List<int> enemyGenerateNums; // 各敵の生成数

    // ゴール地点
    [SerializeField] private GameObject goalGround; // ゴール地点の地面
    [SerializeField] private Vector3 goalConnectionPoint; // ゴールとの接続点
    [SerializeField] private int connectionRoadLength = 3; // ゴールへの通路の長さ
    [SerializeField] private bool autoSelectConnectionPoint = true; // 接続点を自動選択するか
    [SerializeField] private GameObject goalObjectPrefab; // ゴールオブジェクトのプレハブ

    [SerializeField] private LayerMask groundLayer = 1 << 8; // デフォルトで「Ground」レイヤー（Layer 8）を設定


    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;

    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [SerializeField] private int mapSizeW; // マップの横サイズ
    [SerializeField] private int mapSizeH; // マップの縦サイズ
    private int[,] map; // マップの管理配列

    [SerializeField] private int roomNum; // 部屋の数
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
        // 複数のインスタンスを検出
        TestMap01[] instances = FindObjectsOfType<TestMap01>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"複数のTestMap01インスタンスが検出されました！({instances.Length}個)。このインスタンスを無効化します。");
            isDebugOffGenerate = true;
        }

        if (isDebugOffGenerate) return;

        // groundLayerの検証
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("groundLayerが空です！デフォルト値（Ground）に設定します。");
            groundLayer = LayerMask.GetMask("Ground");
        }
        Debug.Log($"groundLayer初期値: LayerMask={LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)))}, Value={groundLayer.value}");


        MapGenerate().Forget();
    }

    private async UniTask RebuildNavMeshAsync()
    {
        await UniTask.Yield();
        await UniTask.DelayFrame(10);
        if (groundSurface != null)
        {
            groundSurface.BuildNavMesh();
        }
        if (roadSurface != null)
        {
            roadSurface.BuildNavMesh();
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
            // コライダーの確認
            if (!goalGround.GetComponent<Collider>())
            {
                Debug.LogWarning("goalGround にコライダーがありません！BoxCollider を追加します。");
                var collider = goalGround.AddComponent<BoxCollider>();
                collider.size = GroundSetting.size;
            }
            goalSurface.BuildNavMesh();
            if (goalSurface.navMeshData != null)
            {
                Bounds bounds = goalSurface.navMeshData.sourceBounds;
                Debug.Log($"goalSurface NavMesh 範囲: {bounds}, goalGround 位置: {goalGround.transform.position}");
            }
            else
            {
                Debug.LogWarning("goalSurface の NavMesh データが生成されていません！");
            }
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

        GameObject ground = Instantiate(groundPrefab);
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";
        ground.transform.SetParent(groundParent.transform);

        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";
        wall.transform.SetParent(wallParent.transform);

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
        initPrefab();
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

        for (int i = 0; i < roomNum; i++)
        {
            await GeneratePatrolPointInRoomsAsync(patrolPoint[i], roomNum);
        }
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(enemyPrefabs[i], enemyGenerateNums[i]);
        }
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
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
            Debug.LogWarning("GoalGround が設定されていません！");
            return;
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
                objectParents[(int)objectType.road].transform);
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
                    objectParents[(int)objectType.road].transform);
                Debug.Log($"拡張通路を生成: {roadPos}");
            }
        }

        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);
        Debug.Log($"GoalGround を {goalGround.transform.position} に配置");

        // 位置の検証
        int goalMapX = Mathf.FloorToInt((goalGround.transform.position.x - defaultPosition.x) / GroundSetting.size.x);
        int goalMapZ = Mathf.FloorToInt((goalGround.transform.position.z - defaultPosition.z) / GroundSetting.size.z);
        Debug.Log($"GoalGround マップ位置: ({goalMapX}, {goalMapZ}), マップ範囲: ({mapSizeW}, {mapSizeH})");
        if (goalMapX < 0 || goalMapX >= mapSizeW || goalMapZ < 0 || goalMapZ >= mapSizeH)
        {
            Debug.LogWarning($"GoalGround がマップ範囲外です: ({goalMapX}, {goalMapZ})");
        }

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