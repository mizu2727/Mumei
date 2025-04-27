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
    public static TestMap01 instance { get; private set; }

    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Mesh mesh;
    [SerializeField] float width = 1;
    [SerializeField] float height = 1;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material mapMaterial;

    // �X�e�[�W�̌����ڂ̐F�ƃT�C�Y���w��
    [SerializeField] ObjectState WallSetting; // �ǂ̐F�ƃT�C�Y
    [SerializeField] ObjectState GroundSetting; // �n�ʂ̐F�ƃT�C�Y
    [SerializeField] ObjectState RoadSetting; // ���̐F�ƃT�C�Y

    // �X�e�[�W�̃v���n�u
    [SerializeField] GameObject groundPrefab; // �n�ʂ̃v���n�u
    [SerializeField] GameObject wallPrefab; // �ǂ̃v���n�u
    [SerializeField] GameObject roadPrefab; // �L���̃v���n�u

    // �}�b�v�����̏����ʒu
    [SerializeField] Vector3 defaultPosition; // �}�b�v�����̏����ʒu

    // �A�C�e���̐ݒ�
    [Header("�A�C�e���̐ݒ�")]
    [SerializeField] private List<GameObject> itemPrefabs; // �����̃A�C�e���v���n�u
    [SerializeField] private List<int> itemGenerateNums; // �e�A�C�e���̐�����
    [SerializeField] public Vector3 roomCenter; // �����̒��S�ʒu
    [SerializeField] public Vector3 roomSize; // �����̃T�C�Y

    // �p�j�n�_�̐ݒ�
    [SerializeField] public Transform[] patrolPoint; // �p�j�n�_�̃v���n�u

    // �G�̐ݒ�
    [SerializeField] private List<GameObject> enemyPrefabs; // �����̓G�v���n�u
    [SerializeField] private List<int> enemyGenerateNums; // �e�G�̐�����

    // �S�[���n�_
    [SerializeField] private GameObject goalGroundPrefab; // �S�[���n�ʂ̃v���n�u
    [SerializeField] private GameObject goalGround; // // �V�[�����̃S�[���n�ʁi�C���X�^���X�j
    [SerializeField] private Vector3 goalConnectionPoint; // �S�[���Ƃ̐ڑ��_
    [SerializeField] private int connectionRoadLength = 3; // �S�[���ւ̒ʘH�̒���
    [SerializeField] private bool autoSelectConnectionPoint = true; // �ڑ��_�������I�����邩
    [SerializeField] private GameObject goalObjectPrefab; // �S�[���I�u�W�F�N�g�̃v���n�u

    [SerializeField] private LayerMask groundLayer = 1 << 8; // �f�t�H���g�ŁuGround�v���C���[�iLayer 8�j��ݒ�


    // �}�b�v�����������������ǂ������Ǘ�����t���O
    public bool IsMapGenerated { get; private set; }

    // �v���C���[�����łɃX�|�[���������ǂ������Ǘ�����t���O
    private bool hasPlayerSpawned = false;

    // �v���C���[�ƓG�̍ŏ������i���[�v���ɓG�Ƌ߂����Ȃ��悤�ɂ���j
    [SerializeField] private float minDistanceFromEnemy = 5f;

    // �G�̈ʒu���L�^���郊�X�g�i���[�v���ɓG�Ƃ̋������`�F�b�N���邽�߁j
    private List<Vector3> enemyPositions = new List<Vector3>();


    // �G�����łɃX�|�[���������ǂ������Ǘ�����t���O
    private bool hasEnemiesSpawned = false;

    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;

    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [SerializeField] private int mapSizeW; // �}�b�v�̉��T�C�Y
    [SerializeField] private int mapSizeH; // �}�b�v�̏c�T�C�Y
    private int[,] map; // �}�b�v�̊Ǘ��z��

    [SerializeField] private int roomNum; // �����̐�
    private int roomMin = 10; // �����̍ŏ��l
    private int parentNum = 0; // �������镔���ԍ�
    private int max = 0; // �ő�ʐ�
    private int roomCount; // �����J�E���g
    private int line = 0; // �����_
    private int[,] roomStatus; // �����̊Ǘ��z��

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

    private GameObject[] mapObjects; // �}�b�v�����p�̃I�u�W�F�N�g�z��
    private GameObject[] objectParents; // �e�^�C�v�ʂ̐e�I�u�W�F�N�g�z��
    private const int offsetWall = 2; // �ǂ��痣������
    private const int offset = 1; // �����p

    [SerializeField] private bool isDebugOffGenerate = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // �����̃C���X�^���X�����o
        TestMap01[] instances = FindObjectsOfType<TestMap01>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"������TestMap01�C���X�^���X�����o����܂����I({instances.Length}��)�B���̃C���X�^���X�𖳌������܂��B");
            isDebugOffGenerate = true;
        }

        if (isDebugOffGenerate) return;

        // groundLayer�̌���
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("groundLayer����ł��I�f�t�H���g�l�iGround�j�ɐݒ肵�܂��B");
            groundLayer = LayerMask.GetMask("Ground");
        }
        int layerIndex = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));
        string layerName = (groundLayer.value != 0 && layerIndex >= 0 && layerIndex < 32)
            ? LayerMask.LayerToName(layerIndex)
            : $"�����ȃ��C���[ (Value={groundLayer.value})";
        Debug.Log($"groundLayer�����l: LayerMask={layerName}, Value={groundLayer.value}");

        MapGenerate().Forget();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void Update()
    {
        if (!IsMapGenerated)
        {
            Debug.Log("[TestMap01] �}�b�v�������܂��������Ă��܂���B");
            return;
        }

        if (!hasPlayerSpawned && !hasEnemiesSpawned && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[TestMap01] �X�y�[�X�L�[��������܂����B�v���C���[�ƓG�𐶐����܂��B");
            SpawnPlayerAndEnemiesAsync().Forget();
        }
    }

    private async UniTask RebuildNavMeshAsync()
    {
        await UniTask.Yield();
        await UniTask.DelayFrame(10);

        if (groundSurface != null)
        {
            groundSurface.BuildNavMesh();
            if (groundSurface.navMeshData != null)
            {
                Bounds bounds = groundSurface.navMeshData.sourceBounds;
                Debug.Log($"[TestMap01] groundSurface NavMesh �\�z����: �͈�={bounds}");
            }
            else
            {
                Debug.LogError("[TestMap01] groundSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
            }
        }
        else
        {
            Debug.LogError("[TestMap01] groundSurface ��null�ł��I");
        }

        if (roadSurface != null)
        {
            roadSurface.BuildNavMesh();
            if (roadSurface.navMeshData != null)
            {
                Bounds bounds = roadSurface.navMeshData.sourceBounds;
                Debug.Log($"[TestMap01] roadSurface NavMesh �\�z����: �͈�={bounds}");
            }
            else
            {
                Debug.LogError("[TestMap01] roadSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
            }
        }
        else
        {
            Debug.LogError("[TestMap01] roadSurface ��null�ł��I");
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
                Debug.LogWarning("[TestMap01] goalGround �ɃR���C�_�[������܂���IBoxCollider ��ǉ����܂��B");
                var collider = goalGround.AddComponent<BoxCollider>();
                collider.size = GroundSetting.size;
            }
            goalSurface.BuildNavMesh();
            if (goalSurface.navMeshData != null)
            {
                Bounds bounds = goalSurface.navMeshData.sourceBounds;
                Debug.Log($"[TestMap01] goalSurface NavMesh �\�z����: �͈�={bounds}, �ʒu={goalGround.transform.position}");
            }
            else
            {
                Debug.LogError("[TestMap01] goalSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
            }
        }
        else
        {
            Debug.LogError("[TestMap01] goalGround ��null�ł��I");
        }
    }



    void initPrefab()
    {
        // �����̐e�I�u�W�F�N�g������Δj������i�V�[�����܂����Ȃ��悤�Ɂj
        if (objectParents != null)
        {
            foreach (var parent in objectParents)
            {
                if (parent != null)
                {
                    Destroy(parent);
                }
            }
        }

        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");

        // �V�[���ɐe�I�u�W�F�N�g���֘A�t����i�i������h���j
        groundParent.transform.SetParent(transform);
        wallParent.transform.SetParent(transform);
        roadParent.transform.SetParent(transform);

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

        // �v���n�u�̌���
        if (groundPrefab == null || wallPrefab == null || roadPrefab == null)
        {
            Debug.LogError("�v���n�u���ݒ肳��Ă��܂���IgroundPrefab, wallPrefab, roadPrefab���m�F���Ă��������B");
            return;
        }

        // �v���n�u���V�[�����̃I�u�W�F�N�g�łȂ����Ƃ��m�F
        if (groundPrefab.scene.IsValid() || wallPrefab.scene.IsValid() || roadPrefab.scene.IsValid())
        {
            Debug.LogError("�v���n�u���V�[�����̃I�u�W�F�N�g�ł��I�v���n�u�A�Z�b�g���w�肵�Ă��������B");
            return;
        }

        // �v���n�u�̃C���X�^���X�� 
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

        // ���������ꂽ�I�u�W�F�N�g�̏�Ԃ��m�F
        foreach (var obj in mapObjects)
        {
            Debug.Log($"mapObjects[{obj.name}] �V�[����: {obj.scene.IsValid()}, �ʒu: {obj.transform.position}");
        }
    }

    private async UniTask MapGenerate()
    {
        // �}�b�v�����J�n���Ƀt���O��false�ɐݒ�
        IsMapGenerated = false;

        initPrefab();
        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];
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


        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
        }

        // �}�b�v����������Ƀt���O��true�ɐݒ�
        IsMapGenerated = true;
        Debug.Log("[TestMap01] �}�b�v�������������܂����B�X�y�[�X�L�[�������ăv���C���[�𐶐����Ă��������B");
    }

    // �v���C���[���[�v�ƓG���������ԂɎ��s
    private async UniTask SpawnPlayerAndEnemiesAsync()
    {
        if (hasPlayerSpawned || hasEnemiesSpawned)
        {
            Debug.LogWarning("[TestMap01] �v���C���[�܂��͓G�����łɐ�������Ă��܂��B�������X�L�b�v���܂��B");
            return;
        }

        Debug.Log("[TestMap01] �v���C���[�ƓG�̐������J�n���܂��B");

        // �v���C���[���X�|�[��
        await SpawnPlayerAsync();

        // �v���C���[�X�|�[����ɓG�𐶐�
        if (hasPlayerSpawned && !hasEnemiesSpawned)
        {
            enemyPositions.Clear();
            Debug.Log($"[TestMap01] �G�����J�n: �v���n�u��={enemyPrefabs.Count}, ���������X�g={string.Join(", ", enemyGenerateNums)}");

            int totalEnemiesGenerated = 0;
            for (int i = 0; i < enemyPrefabs.Count; i++)
            {
                if (i >= enemyGenerateNums.Count || enemyPrefabs[i] == null)
                {
                    Debug.LogWarning($"[TestMap01] �G�v���n�u[{i}]�������܂��͐����������ݒ�ł��B�X�L�b�v���܂��B");
                    continue;
                }
                int generateNum = enemyGenerateNums[i];
                Debug.Log($"[TestMap01] �G�v���n�u {enemyPrefabs[i].name} �� {generateNum} �̐������܂��B");
                await GenerateEnemiesInRoomsAsync(enemyPrefabs[i], generateNum);
                totalEnemiesGenerated += generateNum;
            }

            hasEnemiesSpawned = true;
            Debug.Log($"[TestMap01] �G�̐������������܂����B��������: {totalEnemiesGenerated}");
        }
        else
        {
            Debug.LogWarning("[TestMap01] �v���C���[�̃X�|�[���Ɏ��s�������߁A�G�������X�L�b�v���܂����B");
        }
    }


    // �v���C���[���X�|�[������񓯊����\�b�h
    public async UniTask SpawnPlayerAsync()
    {
        if (hasPlayerSpawned)
        {
            Debug.LogWarning("[TestMap01] �v���C���[�͂��łɃX�|�[�����Ă��܂��B");
            return;
        }

        await WarpPlayerAsync();
        hasPlayerSpawned = true;
        Debug.Log("[TestMap01] �v���C���[�̃X�|�[�����������܂����B");
    }

    // ���J���\�b�h�Ƃ��ă��[�v������񋟁iTitleController����Ăяo����悤�Ɂj
    public async UniTask TriggerWarpAsync()
    {
        //await SpawnPlayerAsync();
    }

    // �v���C���[�������_���ȕ����Ƀ��[�v������
    private async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] Player.instance �� null �ł��I���[�v�ł��܂���B");
            return;
        }

        if (!Player.instance.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[TestMap01] Player.instance ({Player.instance.name}) ����A�N�e�B�u�ł��B�A�N�e�B�u�����܂��B");
            Player.instance.gameObject.SetActive(true);
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
            Vector3 fallbackPosition = defaultPosition + new Vector3(5f, 0.6f, 5f); // ���S�Ȉʒu�ɐݒ�
            Player.instance.transform.position = fallbackPosition;
            Debug.Log($"[TestMap01] �v���C���[�����ʒu�i�t�H�[���o�b�N�j: {fallbackPosition}");
        }
    }

    // ���[�v�ʒu����������񓯊����\�b�h
    private async UniTask<Vector3> FindWarpPositionAsync(Vector3 roomCenter, Vector3 roomSize)
    {
        float margin = 1.0f;
        int maxAttempts = 20;
        float yOffset = Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.5f + 0.1f;

        // groundLayer�̌���
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("[TestMap01] groundLayer����ł��B�f�t�H���g��Ground���C���[�iLayer 8�j��ݒ肵�܂��B");
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
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �̓}�b�v�͈͊O�ł��B");
                continue;
            }

            if (map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �͒n�ʂł͂���܂���Bmap[{mapX}, {mapZ}]={(objectType)map[mapX, mapZ]}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
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
                // ���C���[�C���f�b�N�X�����S�Ɏ擾
                int layerIndex = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));
                string layerName = (groundLayer.value != 0 && layerIndex >= 0 && layerIndex < 32)
                    ? LayerMask.LayerToName(layerIndex)
                    : $"�����ȃ��C���[ (Value={groundLayer.value})";
                Debug.LogWarning($"���s {attempt + 1}/{maxAttempts}: �n�ʌ��o���s: �ʒu={spawnPosition}, LayerMask={layerName}");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 10f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"�ő厎�s��({maxAttempts})�𒴂��Ă����[�v�ʒu�������邱�Ƃ��ł��܂���ł����BroomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }

    // �G�𐶐����郁�\�b�h�i�G�̈ʒu���L�^�j
    private async UniTask GenerateEnemiesInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap01] �G�v���n�u��null�ł��I");
            return;
        }

        Debug.Log($"[TestMap01] �G {prefab.name} �̐������J�n: ������={generateNum}");
        int enemiesGenerated = 0;

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

            Vector3 position = await PlaceObjectAsync(prefab, center, size, isPatrolPoint: false);
            if (position != Vector3.zero)
            {
                enemyPositions.Add(position);
                GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
                enemy.name = $"{prefab.name}_{enemiesGenerated}";
                BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
                if (enemyScript != null && Player.instance != null)
                {
                    enemyScript.tagetPoint = Player.instance.transform;
                    Debug.Log($"[TestMap01] �G {enemy.name} �� {position} �ɐ����B�v���C���[��Transform��ݒ�: {Player.instance.transform.position}");
                }
                else
                {
                    Debug.LogError($"[TestMap01] �G {enemy.name} �̐����Ɏ��s�܂���Player.instance��null�ł��B");
                }
                enemiesGenerated++;
            }
            else
            {
                Debug.LogWarning($"[TestMap01] �G�̐����ʒu��������܂���ł����B�����C���f�b�N�X: {roomIndex}");
            }
        }

        Debug.Log($"[TestMap01] �G {prefab.name} �̐�������: ������={enemiesGenerated}/{generateNum}");
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
            Debug.LogError("[TestMap01] goalGround���ݒ肳��Ă��܂���I�v���n�u�܂��̓V�[�����̃I�u�W�F�N�g��ݒ肵�Ă��������B");
            return;
        }

        if (!goalGround.scene.IsValid())
        {
            Debug.LogError("[TestMap01] goalGround���V�[�����̃I�u�W�F�N�g�ł͂���܂���I�V�[�����ŃC���X�^���X�����Ă��������B");
            return;
        }

        Vector2Int connectPoint;
        if (autoSelectConnectionPoint)
        {
            connectPoint = FindValidConnectionPoint();
            if (connectPoint == Vector2Int.one * -1)
            {
                Debug.LogWarning("[TestMap01] �L���Ȑڑ��_��������܂���ł����I");
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
            Debug.LogWarning($"[TestMap01] �ڑ��_ ({connectX}, {connectZ}) ���}�b�v�͈͊O�ł��I");
            return;
        }

        if (map[connectX, connectZ] != (int)objectType.ground && map[connectX, connectZ] != (int)objectType.road)
        {
            Debug.LogWarning($"[TestMap01] �ڑ��_ ({connectX}, {connectZ}) �͕����܂��͒ʘH�ł͂���܂���I");
            return;
        }

        ClearPathToGoal(connectX, connectZ);

        // objectParents���V�[�����̃I�u�W�F�N�g�ł��邱�Ƃ��m�F
        if (objectParents[(int)objectType.road] == null || !objectParents[(int)objectType.road].scene.IsValid())
        {
            Debug.LogError("[TestMap01] objectParents[road]���V�[�����̃I�u�W�F�N�g�ł͂���܂���IinitPrefab���m�F���Ă��������B");
            return;
        }

        // �ʘH�̐���
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

        // �g���ʘH�̐���
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
                Debug.Log($"[TestMap01] �g���ʘH�𐶐�: {roadPos}");
            }
        }

        // �S�[���n�_�̔z�u
        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);
        Debug.Log($"[TestMap01] GoalGround �� {goalGround.transform.position} �ɔz�u");

        // �S�[���I�u�W�F�N�g�̐���
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
                Debug.Log($"[TestMap01] �S�[���I�u�W�F�N�g�� {navHit.position} �ɐ������܂���");
            }
            else
            {
                Debug.LogWarning("[TestMap01] NavMesh ��̈ʒu��������Ȃ��������߁A�S�[���I�u�W�F�N�g�𐶐��ł��܂���ł���");
                Vector3 fallbackPosition = goalGround.transform.position + Vector3.up * 0.1f;
                GameObject goalObject = Instantiate(
                    goalObjectPrefab,
                    fallbackPosition,
                    Quaternion.identity);
                goalObject.name = "GoalObject_Fallback";
                goalObject.transform.SetParent(goalGround.transform, false);
                Debug.Log($"[TestMap01] �t�H�[���o�b�N: �S�[���I�u�W�F�N�g�� {fallbackPosition} �ɐ������܂����iNavMesh�Ȃ��j");
            }
        }
        else
        {
            Debug.LogWarning("[TestMap01] �S�[���I�u�W�F�N�g�̃v���n�u���ݒ肳��Ă��܂���I");
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

    // PlaceObjectAsync ���C�����Ĕz�u�ʒu��Ԃ��悤�ɂ���
    private async UniTask<Vector3> PlaceObjectAsync(GameObject prefab, Vector3 roomCenter, Vector3 roomSize, bool isPatrolPoint)
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
                        return navHit.position;
                    }
                    else
                    {
                        Debug.LogWarning($"NavMesh��̈ʒu�������炸 {objectName} �𐶐��ł��܂���ł���: {finalPos}");
                    }
                }
                else
                {
                    GameObject obj = Instantiate(prefab, finalPos, Quaternion.identity);
                    Debug.Log($"{objectName} �� {finalPos} �ɐ������܂���");
                    return finalPos;
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
        return Vector3.zero;
    }
}