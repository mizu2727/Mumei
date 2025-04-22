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

    [Header("�A�C�e���̐ݒ�")]
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

    private bool hasPlayerSpawned = false; // �v���C���[�������ς݂���ǐ�

    [SerializeField] private bool isDebugOffGenerate = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[TestMap01] �C���X�^���X�����ɑ��݂��܂��I({Instance.gameObject.name})�B���̃C���X�^���X��j�����܂��B");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log($"[TestMap01] �V���O���g��������: {gameObject.name}");

        if (mapSizeW <= 0)
        {
            mapSizeW = 50;
            Debug.LogWarning($"[TestMap01] mapSizeW �������i{mapSizeW}�j�ł����B�f�t�H���g�l 50 �ɐݒ肵�܂��B");
        }
        if (mapSizeH <= 0)
        {
            mapSizeH = 50;
            Debug.LogWarning($"[TestMap01] mapSizeH �������i{mapSizeH}�j�ł����B�f�t�H���g�l 50 �ɐݒ肵�܂��B");
        }
        if (roomNum <= 0)
        {
            roomNum = 5;
            Debug.LogWarning($"[TestMap01] roomNum �������i{roomNum}�j�ł����B�f�t�H���g�l 5 �ɐݒ肵�܂��B");
        }

        if (groundLayer.value == 0)
        {
            Debug.LogWarning("[TestMap01] groundLayer����ł��I�f�t�H���g�l�iGround�j�ɐݒ肵�܂��B");
            groundLayer = LayerMask.GetMask("Ground");
        }
        if (playerPrefab == null)
        {
            Debug.LogError("[TestMap01] �v���C���[�̃v���n�u���C���X�y�N�^�[�Őݒ肳��Ă��܂���I");
        }

        MapGenerate().Forget();
    }

    private void Update()
    {
        // �X�y�[�X�L�[�����Ńv���C���[����
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
            Debug.LogError("[TestMap01] �v���C���[�����݂��܂���I���[�v�ł��܂���B�v���C���[�������m�F���Ă��������B");
            return;
        }
        await WarpPlayerAsync();
    }

    private async UniTask RebuildNavMeshAsync()
    {
        await UniTask.Yield();
        await UniTask.DelayFrame(150); // �ҋ@���Ԃ��������ăR���C�_�[������҂�
        try
        {
            if (groundSurface != null)
            {
                groundSurface.BuildNavMesh();
                Debug.Log("Ground NavMesh ���č\�z���܂���");
            }
            else
            {
                Debug.LogWarning("groundSurface �� null �ł��BNavMesh�������X�L�b�v���܂��B");
            }
            if (roadSurface != null)
            {
                roadSurface.BuildNavMesh();
                Debug.Log("Road NavMesh ���č\�z���܂���");
            }
            else
            {
                Debug.LogWarning("roadSurface �� null �ł��BNavMesh�������X�L�b�v���܂��B");
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
                    Debug.LogWarning("goalGround �ɃR���C�_�[������܂���IBoxCollider ��ǉ����܂��B");
                    var collider = goalGround.AddComponent<BoxCollider>();
                    collider.size = GroundSetting.size;
                }
                goalSurface.BuildNavMesh();
                Debug.Log($"Goal NavMesh ���č\�z���܂���: {goalGround.transform.position}");
            }
            else
            {
                Debug.LogWarning("goalGround �� null �ł��BNavMesh�������X�L�b�v���܂��B");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"NavMesh�\�z���ɃG���[���������܂���: {ex.Message}\n{ex.StackTrace}");
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

        // �v���n�u���V�[�����ŃC���X�^���X��
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

        // ���͒l�̌���
        if (roomNum <= 0)
        {
            Debug.LogError($"roomNum �������ł�: {roomNum}�B1�ȏ�̒l��ݒ肵�Ă��������B");
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
                map[nowW, nowH] = 2; // ��
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
                Debug.LogError($"roomCount ({roomCount}) �� roomNum ({roomNum}) �𒴂��܂����B�����𒆎~���܂��B");
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
                        Debug.LogWarning($"�}�b�v�͈͊O�A�N�Z�X��h�~: xIndex={xIndex}, yIndex={yIndex}, mapSize=({mapSizeW}, {mapSizeH})");
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
                        Debug.LogWarning($"�}�b�v�͈͊O�A�N�Z�X��h�~: xIndex={xIndex}, yIndex={yIndex}, mapSize=({mapSizeW}, {mapSizeH})");
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
        Debug.Log("[TestMap01] �}�b�v�������������܂����B�X�y�[�X�L�[�������ăv���C���[�𐶐����Ă��������B");
    }

    private async UniTask SpawnPlayerAsync()
    {
        if (hasPlayerSpawned)
        {
            Debug.LogWarning("[TestMap01] �v���C���[�͂��łɐ�������Ă��܂��B");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("[TestMap01] �v���C���[�̃v���n�u���ݒ肳��Ă��܂���I�v���C���[�𐶐��ł��܂���B");
            return;
        }

        Debug.Log($"[TestMap01] �v���C���[�������J�n: �v���n�u={playerPrefab.name}");
        GameObject playerObj = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Debug.Log($"[TestMap01] �v���C���[�𐶐����܂���: {playerObj.name}");

        hasPlayerSpawned = true;
        await WarpPlayerAsync(); // ������Ƀ��[�v
    }


    private bool SplitPoint(int x, int y)
    {
        int minSize = roomMin + (offsetWall * 2);
        if (x < minSize && y < minSize)
        {
            Debug.LogWarning($"�����T�C�Y�����������܂��Bx={x}, y={y}, �ŏ��T�C�Y={minSize}�B�������X�L�b�v���܂��B");
            return false;
        }

        if (x > y)
        {
            line = Random.Range(minSize, x - minSize);
            if (line <= 0 || line >= x)
            {
                Debug.LogError($"������ line �l: line={line}, x={x}, minSize={minSize}");
                line = Mathf.Clamp(line, minSize, x - minSize);
            }
            return true;
        }
        else
        {
            line = Random.Range(minSize, y - minSize);
            if (line <= 0 || line >= y)
            {
                Debug.LogError($"������ line �l: line={line}, y={y}, minSize={minSize}");
                line = Mathf.Clamp(line, minSize, y - minSize);
            }
            return false;
        }
    }

    private void AddGoalConnection()
    {
        if (goalGround == null)
        {
            Debug.LogWarning("GoalGround ���ݒ肳��Ă��܂���I");
            return;
        }

        // goalGround ���V�[�����ɂ��邱�Ƃ��m�F
        if (!goalGround.scene.IsValid() || goalGround.scene != gameObject.scene)
        {
            Debug.LogWarning("goalGround �������ȃV�[���ɑ����Ă��܂��B�V���� goalGround ���V�[�����ɍ쐬���܂��B");
            GameObject newGoalGround = Instantiate(goalGround, Vector3.zero, Quaternion.identity);
            goalGround = newGoalGround;
        }

        Transform roadParent = objectParents[(int)objectType.road].transform;
        if (!roadParent.gameObject.scene.IsValid() || roadParent.gameObject.scene != gameObject.scene)
        {
            Debug.LogWarning("Road �̐e�I�u�W�F�N�g�������ł��B�V�����e���V�[�����ɍ쐬���܂��B");
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
                Debug.LogWarning("�L���Ȑڑ��_��������܂���ł����I");
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
            Debug.LogWarning($"�ڑ��_ ({connectX}, {connectZ}) ���}�b�v�͈͊O�ł��I");
            return;
        }

        if (map[connectX, connectZ] != (int)objectType.ground && map[connectX, connectZ] != (int)objectType.road)
        {
            Debug.LogWarning($"�ڑ��_ ({connectX}, {connectZ}) �͕����܂��͒ʘH�ł͂���܂���I");
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
                Debug.LogWarning($"���H�I�u�W�F�N�g {road.name} �ɃR���C�_�[������܂���IBoxCollider��ǉ����܂��B");
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
                    Debug.LogWarning($"�g�����H�I�u�W�F�N�g {road.name} �ɃR���C�_�[������܂���IBoxCollider��ǉ����܂��B");
                    var collider = road.AddComponent<BoxCollider>();
                    collider.size = RoadSetting.size;
                }
                Debug.Log($"�g���ʘH�𐶐�: {roadPos}");
            }
        }

        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);
        Debug.Log($"GoalGround �� {goalGround.transform.position} �ɔz�u");

        if (!goalGround.GetComponent<Collider>())
        {
            Debug.LogWarning($"goalGround �ɃR���C�_�[������܂���IBoxCollider��ǉ����܂��B");
            var collider = goalGround.AddComponent<BoxCollider>();
            collider.size = GroundSetting.size;
        }

        int goalMapX = Mathf.FloorToInt((goalGround.transform.position.x - defaultPosition.x) / GroundSetting.size.x);
        int goalMapZ = Mathf.FloorToInt((goalGround.transform.position.z - defaultPosition.z) / GroundSetting.size.z);
        Debug.Log($"GoalGround �}�b�v�ʒu: ({goalMapX}, {goalMapZ}), �}�b�v�͈�: ({mapSizeW}, {mapSizeH})");

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
                Debug.Log($"�S�[���I�u�W�F�N�g�� {navHit.position} �ɐ������܂���");
            }
            else
            {
                Debug.LogWarning($"NavMesh ��̈ʒu��������Ȃ��������߁A�S�[���I�u�W�F�N�g�𐶐��ł��܂���ł���: {goalObjectPosition}");
                Vector3 fallbackPosition = goalGround.transform.position + Vector3.up * 0.1f;
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    fallbackPosition,
                    Quaternion.identity,
                    goalGround.transform);
                Debug.Log($"�t�H�[���o�b�N: �S�[���I�u�W�F�N�g�� {fallbackPosition} �ɐ������܂����iNavMesh�Ȃ��j");
            }
        }
        else
        {
            Debug.LogWarning("�S�[���I�u�W�F�N�g�̃v���n�u���ݒ肳��Ă��܂���I");
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
                Debug.Log($"�G�� {enemyPosition} �ɐ������܂���");
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
                Debug.LogWarning($"�ʒu ({x}, {z}) �͒n�ʂł͂���܂���B���s {attempt + 1}/{maxAttempts}");
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
                    Debug.LogWarning($"NavMesh��̈ʒu�������炸 {objectName} �𐶐��ł��܂���ł���: {finalPos}");
                }
            }
            else
            {
                Debug.LogWarning($"�n�ʌ��o���s: �ʒu={spawnPosition}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }
        Debug.LogError($"�ő厎�s�񐔂𒴂��Ă� {objectName} �𐶐��ł��܂���ł���");
        return Vector3.zero;
    }

    private async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] Player.instance �� null �ł��I���[�v�ł��܂���B");
            return;
        }

        if (!Player.instance.gameObject.activeInHierarchy)
        {
            Debug.LogError($"[TestMap01] Player.instance ({Player.instance.name}) ����A�N�e�B�u�ł��I���[�v�ł��܂���B");
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
                        Debug.Log($"[TestMap01] ���[�v�ʒu {warpPosition} ���G {enemyPos} �ɋ߂����܂��i����: {distance} < {minDistanceFromEnemy}�j");
                        break;
                    }
                }

                if (isFarEnough)
                {
                    NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
                    if (agent != null && agent.isOnNavMesh)
                    {
                        agent.enabled = false;
                        Debug.Log($"[TestMap01] NavMeshAgent ���ꎞ�I�ɖ�����: {Player.instance.name}");
                    }

                    Player.instance.transform.position = warpPosition;
                    Debug.Log($"[TestMap01] �v���C���[�����ʒu: {warpPosition} �i{Player.instance.name} �����[�v���܂����j");

                    if (agent != null)
                    {
                        agent.enabled = true;
                        NavMeshHit navHit;
                        if (NavMesh.SamplePosition(warpPosition, out navHit, 10.0f, NavMesh.AllAreas))
                        {
                            Vector3 adjustedNavPos = navHit.position + Vector3.up * 0.1f;
                            agent.Warp(adjustedNavPos);
                            Debug.Log($"[TestMap01] NavMeshAgent �� {adjustedNavPos} �Ƀ��[�v���܂����B");
                        }
                        else
                        {
                            Debug.LogWarning($"[TestMap01] ���[�v�ʒu {warpPosition} ��NavMesh��ɂ���܂���BNavMeshAgent�̃��[�v���X�L�b�v���܂��B");
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
            Debug.LogWarning("[TestMap01] �K�؂ȃ��[�v�ʒu��������܂���ł����B�t�H�[���o�b�N�ʒu�Ƀ��[�v���܂��B");

            Vector3 fallbackPosition = new Vector3(
                defaultPosition.x + (roomStatus[(int)RoomStatus.rx, 0] + roomStatus[(int)RoomStatus.rw, 0] / 2.0f) * GroundSetting.size.x,
                defaultPosition.y + (Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.6f),
                defaultPosition.z + (roomStatus[(int)RoomStatus.ry, 0] + roomStatus[(int)RoomStatus.rh, 0] / 2.0f) * GroundSetting.size.z
            );

            int mapX = Mathf.FloorToInt((fallbackPosition.x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((fallbackPosition.z - defaultPosition.z) / GroundSetting.size.z);
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH || map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogError($"[TestMap01] �t�H�[���o�b�N�ʒu {fallbackPosition} �͖����ł��BmapX={mapX}, mapZ={mapZ}, map[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? (objectType)map[mapX, mapZ] : "�͈͊O")}");
                return;
            }

            NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.enabled = false;
                Debug.Log($"[TestMap01] NavMeshAgent ���ꎞ�I�ɖ������i�t�H�[���o�b�N�j: {Player.instance.name}");
            }

            Player.instance.transform.position = fallbackPosition;
            Debug.Log($"[TestMap01] �v���C���[�����ʒu�i�t�H�[���o�b�N�j: {fallbackPosition} �i{Player.instance.name} �����[�v���܂����j");

            if (agent != null)
            {
                agent.enabled = true;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(fallbackPosition, out navHit, 10.0f, NavMesh.AllAreas))
                {
                    Vector3 adjustedNavPos = navHit.position + Vector3.up * 0.1f;
                    agent.Warp(adjustedNavPos);
                    Debug.Log($"[TestMap01] NavMeshAgent ���t�H�[���o�b�N�ʒu {adjustedNavPos} �Ƀ��[�v���܂����B");
                }
                else
                {
                    Debug.LogWarning($"[TestMap01] �t�H�[���o�b�N�ʒu {fallbackPosition} ��NavMesh��ɂ���܂���BNavMeshAgent�̃��[�v���X�L�b�v���܂��B");
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

            // �}�b�v�͈͊O�`�F�b�N
            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH)
            {
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �̓}�b�v�͈͊O�ł��B");
                continue;
            }

            // �n�ʃ`�F�b�N
            if (map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �͒n�ʂł͂���܂���Bmap[{mapX}, {mapZ}]={(objectType)map[mapX, mapZ]}");
                continue;
            }

            // ���C�L���X�g�Œn�ʂ��m�F
            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                // �q�b�g�����I�u�W�F�N�g���������n�ʂ��m�F
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �q�b�g�����I�u�W�F�N�g {hit.collider.gameObject.name} ��Ground���C���[�ł͂���܂���B");
                    continue;
                }

                Vector3 finalPos = hit.point + Vector3.up * yOffset;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
                {
                    Vector3 adjustedPos = navHit.position + Vector3.up * 0.1f;
                    // ���[�v�ʒu���ǂƏՓ˂��Ȃ����m�F
                    if (!Physics.CheckSphere(adjustedPos, 0.5f, LayerMask.GetMask("Wall")))
                    {
                        Debug.Log($"���s {attempt + 1}/{maxAttempts}: ���[�v�ʒu {adjustedPos} �������܂����i�n�ʃq�b�g: {hit.point}, �R���C�_�[: {hit.collider.gameObject.name})");
                        return adjustedPos;
                    }
                    else
                    {
                        Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: ���[�v�ʒu {adjustedPos} ���ǂƏՓ˂��Ă��܂��B");
                    }
                }
                else
                {
                    Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: NavMesh��̈ʒu��������܂���ł���: {finalPos}");
                }
            }
            else
            {
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �n�ʌ��o���s: �ʒu={spawnPosition}, LayerMask={LayerMask.LayerToName(groundLayer.value)}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 10f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"�ő厎�s��({maxAttempts})�𒴂��Ă����[�v�ʒu�������邱�Ƃ��ł��܂���ł����BroomCenter={roomCenter}, roomSize={roomSize}");
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
            Debug.LogWarning("�L���Ȑڑ��_��������܂���ł����I�ǂɗאڂ��Ȃ�����/�ʘH�������Ă��������B");
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
                Debug.Log($"�ǂ𓹘H�ɕύX: ({roadX}, {startZ})");
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
                Debug.LogWarning($"�ʒu ({x}, {z}) �͒n�ʂł͂���܂���B���s {attempt + 1}/{maxAttempts}, map[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? map[mapX, mapZ] : "�͈͊O")}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                Debug.Log($"�n�ʌ��o����: �q�b�g�ʒu={hit.point}, �I�u�W�F�N�g={hit.collider.gameObject.name}, LayerMask={LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)))}");
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
                        Debug.LogWarning($"NavMesh��̈ʒu�������炸 {objectName} �𐶐��ł��܂���ł���: {finalPos}");
                    }
                }
                else
                {
                    Instantiate(prefab, finalPos, Quaternion.identity);
                    Debug.Log($"{objectName} �� {finalPos} �ɐ������܂���");
                    return;
                }
            }
            else
            {
                string layerName = groundLayer.value == 0 ? "��" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)));
                Debug.LogWarning($"�n�ʌ��o���s: �ʒu={spawnPosition}, LayerMask={layerName}, LayerMaskValue={groundLayer.value}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }
        Debug.LogError($"�ő厎�s�񐔂𒴂��Ă� {objectName} �𐶐��ł��܂���ł���");
    }
}