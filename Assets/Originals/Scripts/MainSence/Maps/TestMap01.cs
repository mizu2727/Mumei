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


    [Header("�X�e�[�W�̌����ڂ̐F�ƃT�C�Y���w��")]
    [SerializeField] ObjectState WallSetting; // �ǂ̐F�ƃT�C�Y
    [SerializeField] ObjectState GroundSetting; // �n�ʂ̐F�ƃT�C�Y
    [SerializeField] ObjectState RoadSetting; // ���̐F�ƃT�C�Y


    [Header("�X�e�[�W�̃v���n�u")]
    [SerializeField] GameObject groundPrefab; // �n�ʂ̃v���n�u
    [SerializeField] GameObject wallPrefab; // �ǂ̃v���n�u
    [SerializeField] GameObject roadPrefab; // �L���̃v���n�u


    [Header("�}�b�v�����̏����ʒu")]
    [SerializeField] Vector3 defaultPosition;

    [Header("�A�C�e���̐ݒ�")]
    [SerializeField] private List<GameObject> itemPrefabs; // �����̃A�C�e���v���n�u
    [SerializeField] private List<int> itemGenerateNums; // �e�A�C�e���̐�����
    [SerializeField] public Vector3 roomCenter; // �����̒��S�ʒu
    [SerializeField] public Vector3 roomSize; // �����̃T�C�Y


    [Header("�p�j�n�_�̐ݒ�(�p�j�n�_�̃v���n�u��ݒ肷�邱��)")]
    [SerializeField] private GameObject[] patrolPointPrefabs; // �p�j�n�_�v���n�u�̃��X�g
    [HideInInspector] public Transform[] patrolPoint; // ���I�ɐ������ꂽ�p�j�n�_
    [Header("�p�j�n�_�̐e�I�u�W�F�N�g")]
    private GameObject patrolPointParent; // �p�j�n�_�̐e�I�u�W�F�N�g

    // �p�j�n�_���i�[���郊�X�g�i���I�����p�j
    private List<Transform> patrolPointsList = new List<Transform>();

    [Header("�G�̐ݒ�")]
    [SerializeField] private List<GameObject> enemyPrefabs; // �����̓G�v���n�u
    [SerializeField] private List<int> enemyGenerateNums; // �e�G�̐�����


    [Header("�S�[��")]
    [SerializeField] private GameObject goalGroundPrefab; // �S�[���n�ʂ̃v���n�u
    [SerializeField] private GameObject goalGround; // // �V�[�����̃S�[���n�ʁi�C���X�^���X�j
    [SerializeField] private Vector3 goalConnectionPoint; // �S�[���Ƃ̐ڑ��_
    [SerializeField] private int connectionRoadLength = 3; // �S�[���ւ̒ʘH�̒���
    [SerializeField] private bool autoSelectConnectionPoint = true; // �ڑ��_�������I�����邩
    [SerializeField] private GameObject goalObjectPrefab; // �S�[���I�u�W�F�N�g�̃v���n�u


    [Header("�f�t�H���g�ŁuGround�v���C���[��ݒ�")]
    [SerializeField] private LayerMask groundLayer = 1 << 8;


    [Header("�}�b�v���������t���O")]
    public bool IsMapGenerated { get; private set; }

    [Header("�}�b�v�������t���O")]
    private bool isGeneratingMap = false;

    [Header("NavMesh���������t���O")]
    public bool IsNavMeshGenerated { get; private set; }


    [Header("�v���C���[���������t���O")]
    public bool hasPlayerSpawned = false;


    [Header("�v���C���[�ƓG�̍ŏ������i���[�v���ɓG�Ƌ߂����Ȃ��悤�ɂ���j")]
    [SerializeField] private float minDistanceFromEnemy = 5f;


    [Header("�G�̈ʒu���L�^���郊�X�g�i���[�v���ɓG�Ƃ̋������`�F�b�N���邽�߁j")]
    private List<Vector3> enemyPositions = new List<Vector3>();



    [Header("�G���������t���O")]
    public bool hasEnemiesSpawned = false;

    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;

    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [Header("�}�b�v�̉��T�C�Y")]
    [SerializeField] private int mapSizeW;

    [Header("�}�b�v�̏c�T�C�Y")]
    [SerializeField] private int mapSizeH;

    [Header("�}�b�v�̊Ǘ��z��")]
    private int[,] map;

    [Header("�����̐�")]
    [SerializeField] private int roomNum;

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
        // �V���O���g���̃`�F�b�N
        if (instance != null && instance != this)
        {
            Debug.LogWarning($"[TestMap01] �ʂ̃C���X�^���X�����łɑ��݂��܂�: {instance.gameObject.name}�B���̃C���X�^���X��j�����܂�: {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        instance = this;

        // �V�[�����̑���TestMap01�R���|�[�l���g������
        TestMap01[] instances = FindObjectsOfType<TestMap01>();
        if (instances.Length > 1)
        {
            Debug.LogWarning($"[TestMap01] �V�[�����ɕ�����TestMap01�C���X�^���X�����o����܂����I({instances.Length}��)");
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != this)
                {
                    Debug.LogWarning($"[TestMap01] ���̃C���X�^���X��j�����܂�: {instances[i].gameObject.name}");
                    Destroy(instances[i].gameObject);
                }
            }
        }

        //�f�o�b�O���[�h�̏ꍇ�A�ȍ~�̏������X�L�b�v
        if (isDebugOffGenerate) return;

        // groundLayer����̏ꍇ�A�iGround�j�ɐݒ�
        if (groundLayer.value == 0) groundLayer = LayerMask.GetMask("Ground");

        int layerIndex = Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2));

        // �}�b�v��������x�������s
        MapGenerate().Forget();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    async UniTask RebuildNavMeshAsync()
    {
        await UniTask.DelayFrame(10);

        if (groundSurface != null)
        {
            groundSurface.BuildNavMesh();
            await UniTask.WaitUntil(() => groundSurface.navMeshData != null, cancellationToken: this.GetCancellationTokenOnDestroy());
            Debug.Log(groundSurface.navMeshData != null
                ? $"[TestMap01] groundSurface NavMesh �\�z����: �͈�={groundSurface.navMeshData.sourceBounds}, �ʒu={groundSurface.transform.position}"
                : "[TestMap01] groundSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
        }
        else
        {
            Debug.LogError("[TestMap01] groundSurface �� null �ł��I");
        }

        if (roadSurface != null)
        {
            roadSurface.BuildNavMesh();
            await UniTask.WaitUntil(() => roadSurface.navMeshData != null, cancellationToken: this.GetCancellationTokenOnDestroy());
            Debug.Log(roadSurface.navMeshData != null
                ? $"[TestMap01] roadSurface NavMesh �\�z����: �͈�={roadSurface.navMeshData.sourceBounds}, �ʒu={roadSurface.transform.position}"
                : "[TestMap01] roadSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
        }
        else
        {
            Debug.LogError("[TestMap01] roadSurface �� null �ł��I");
        }

        if (goalGround != null)
        {
            var goalSurface = goalGround.GetComponent<NavMeshSurface>() ?? goalGround.AddComponent<NavMeshSurface>();
            goalSurface.collectObjects = CollectObjects.Children;
            goalSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
#if UNITY_EDITOR
            GameObjectUtility.SetStaticEditorFlags(goalGround, StaticEditorFlags.NavigationStatic);
#endif
            if (!goalGround.GetComponent<Collider>())
            {
                Debug.LogWarning("[TestMap01] goalGround �ɃR���C�_�[������܂���IBoxCollider ��ǉ����܂��B");
                goalGround.AddComponent<BoxCollider>().size = GroundSetting.size;
            }
            goalSurface.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
            goalSurface.BuildNavMesh();
            Debug.Log(goalSurface.navMeshData != null
                ? $"[TestMap01] goalSurface NavMesh �\�z����: �͈�={goalSurface.navMeshData.sourceBounds}, �ʒu={goalGround.transform.position}"
                : "[TestMap01] goalSurface ��NavMesh�f�[�^�����Ɏ��s���܂����I");
        }
        else
        {
            Debug.LogError("[TestMap01] goalGround ��null�ł��I");
        }

        // NavMesh ����������������Ă��邩�m�F
        Vector3 testPos = defaultPosition + new Vector3(mapSizeW * GroundSetting.size.x / 2, 1f, mapSizeH * GroundSetting.size.z / 2);
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(testPos, out navHit, 10f, NavMesh.AllAreas))
        {
            Debug.Log($"[TestMap01] NavMesh �����m�F: �T���v���ʒu={navHit.position}");
        }
        else
        {
            Debug.LogError($"[TestMap01] NavMesh �����m�F���s: �e�X�g�ʒu={testPos}");
            // �ǉ��̃f�o�b�O���
            Debug.Log($"[TestMap01] �}�b�v�T�C�Y: mapSizeW={mapSizeW}, mapSizeH={mapSizeH}, GroundSetting.size={GroundSetting.size}");
            Debug.Log($"[TestMap01] defaultPosition={defaultPosition}");
            if (groundSurface != null)
            {
                Debug.Log($"[TestMap01] groundSurface �͈̔�: {groundSurface.navMeshData?.sourceBounds}");
            }
            if (roadSurface != null)
            {
                Debug.Log($"[TestMap01] roadSurface �͈̔�: {roadSurface.navMeshData?.sourceBounds}");
            }
        }

        IsNavMeshGenerated = true;
        Debug.Log("[TestMap01] NavMesh �������������܂����B");
    }



    void initPrefab()
    {
        // �����̐e�I�u�W�F�N�g������Δj��
        if (objectParents != null)
        {
            foreach (var parent in objectParents.Where(p => p != null))
            {
                Destroy(parent);
            }
            objectParents = null;
        }

        // �p�j�n�_�̐e�I�u�W�F�N�g������Δj��
        if (patrolPointParent != null)
        {
            Destroy(patrolPointParent);
            patrolPointParent = null;
        }

        // �����̐e�I�u�W�F�N�g�����iGround, Wall, Road�j
        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");

        // �e�I�u�W�F�N�g�̈ʒu�� defaultPosition �ɍ��킹��
        groundParent.transform.position = defaultPosition;
        wallParent.transform.position = defaultPosition;
        roadParent.transform.position = defaultPosition;

        // �p�j�n�_�̐e�I�u�W�F�N�g�𐶐�
        patrolPointParent = new GameObject("PatrolPoints");
        patrolPointParent.transform.SetParent(transform);
        patrolPointParent.transform.position = defaultPosition;

        // �V�[���ɐe�I�u�W�F�N�g���֘A�t����
        groundParent.transform.SetParent(transform);
        wallParent.transform.SetParent(transform);
        roadParent.transform.SetParent(transform);

        // NavMeshSurface�̐ݒ�
        groundSurface = groundParent.AddComponent<NavMeshSurface>();
        groundSurface.collectObjects = CollectObjects.Children; // Children �ɕύX���Ĕ͈͂�����
        groundSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(groundParent, StaticEditorFlags.NavigationStatic);
#endif

        roadSurface = roadParent.AddComponent<NavMeshSurface>();
        roadSurface.collectObjects = CollectObjects.Children; // Children �ɕύX
        roadSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
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

        // �n�ʃv���n�u�̃C���X�^���X��
        GameObject ground = Instantiate(groundPrefab);
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";
        ground.transform.SetParent(groundParent.transform);
        ground.layer = LayerMask.NameToLayer("Ground");
        if (!ground.GetComponent<Collider>())
        {
            Debug.LogWarning("[TestMap01] groundPrefab �ɃR���C�_�[������܂���BBoxCollider��ǉ����܂��B");
            ground.AddComponent<BoxCollider>().size = GroundSetting.size;
        }

        // �ǃv���n�u�̃C���X�^���X��
        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";
        wall.transform.SetParent(wallParent.transform);

        // ���v���n�u�̃C���X�^���X��
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
        // ���łɃ}�b�v�������܂��͐����ς݂̏ꍇ�͎��s���Ȃ�
        if (isGeneratingMap || IsMapGenerated) return;

        isGeneratingMap = true;

        // �}�b�v�����J�n���Ƀt���O��false�ɐݒ�
        IsMapGenerated = false;

        initPrefab();

        if (mapObjects == null || mapObjects.Any(obj => obj == null))
        {
            Debug.LogError("[TestMap01] initPrefab �����s���܂����B�}�b�v�����𒆒f���܂��B");
            isGeneratingMap = false;
            return;
        }

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

        // �p�j�n�_���X�g���N���A
        patrolPointsList.Clear();

        // �p�j�n�_���e�v���n�u���Ƃɐ���
        for (int i = 0; i < patrolPointPrefabs.Length; i++)
        {
            if (patrolPointPrefabs[i] != null)
            {
                await GeneratePatrolPointInRoomsAsync(patrolPointPrefabs[i].transform, roomNum);
            }
            else
            {
                Debug.LogWarning($"[TestMap01] patrolPointPrefabs[{i}] ��null�ł��B�X�L�b�v���܂��B");
            }
        }

        // �������ꂽ�p�j�n�_��patrolPoint�z��ɐݒ�
        patrolPoint = patrolPointsList.ToArray();
        Debug.Log($"[TestMap01] patrolPoint�z���{patrolPoint.Length}�̜p�j�n�_��ݒ肵�܂����B");


        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            await GenerateObjectsInRoomsAsync(itemPrefabs[i], itemGenerateNums[i]);
        }

        // �}�b�v����������Ƀt���O��true�ɐݒ�
        IsMapGenerated = true;

        isGeneratingMap = false;

        Debug.Log("[TestMap01] �}�b�v�������������܂����B�X�y�[�X�L�[�������ăv���C���[�𐶐����Ă��������B");

        // �}�b�v�̏�Ԃ����O�o��
        string mapDebug = "";
        for (int h = 0; h < mapSizeH; h++)
        {
            for (int w = 0; w < mapSizeW; w++)
            {
                mapDebug += map[w, h] + " ";
            }
            mapDebug += "\n";
        }
        Debug.Log($"[TestMap01] �������ꂽ�}�b�v:\n{mapDebug}");
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
        if (hasPlayerSpawned)
        {
            Debug.Log("[TestMap01] �v���C���[�̃X�|�[�����������܂����B");
        }
        else
        {
            Debug.LogError("[TestMap01] �v���C���[�̃X�|�[���Ɏ��s���܂����B");
        }
    }

    // �G���X�|�[������V�����񓯊����\�b�h
    public async UniTask SpawnEnemiesAsync()
    {
        if (hasEnemiesSpawned)
        {
            Debug.LogWarning("[TestMap01] �G�͂��łɃX�|�[�����Ă��܂��B");
            return;
        }

        enemyPositions.Clear();
        Debug.Log($"[TestMap01] �G�����J�n: �v���n�u��={enemyPrefabs.Count}, ���������X�g={string.Join(", ", enemyGenerateNums)}");

        if (enemyPrefabs.Count == 0 || enemyGenerateNums.Count == 0)
        {
            Debug.LogError("[TestMap01] �G�v���n�u�܂��͐��������ݒ肳��Ă��܂���I");
            return;
        }

        int totalEnemiesGenerated = 0;
        int maxIndex = Mathf.Min(enemyPrefabs.Count, enemyGenerateNums.Count);
        for (int i = 0; i < maxIndex; i++)
        {
            if (enemyPrefabs[i] == null || enemyGenerateNums[i] <= 0) continue;
            Debug.Log($"[TestMap01] �G�v���n�u {enemyPrefabs[i].name} �� {enemyGenerateNums[i]} �̐������܂��B");
            await GenerateEnemiesInRoomsAsync(enemyPrefabs[i], enemyGenerateNums[i]);
            totalEnemiesGenerated += enemyGenerateNums[i];
        }

        hasEnemiesSpawned = true;
        Debug.Log($"[TestMap01] �G�̐������������܂����B��������: {totalEnemiesGenerated}");
    }


    // ���J���\�b�h�Ƃ��ă��[�v������񋟁iTitleController����Ăяo����悤�Ɂj
    public async UniTask TriggerWarpAsync()
    {
        //await SpawnPlayerAsync();
    }

    // �v���C���[�������_���ȕ����Ƀ��[�v������
    async UniTask WarpPlayerAsync()
    {
        if (Player.instance == null)
        {
            Debug.LogError("[TestMap01] Player.instance �� null �ł��I���[�v�ł��܂���B");
            hasPlayerSpawned = false;
            return;
        }

        // NavMesh �̑��݊m�F
        if (!CheckNavMeshExists())
        {
            Debug.LogWarning("[TestMap01] NavMesh ���܂���������Ă��܂���B���[�v�������������܂��B");
            await DelayedWarpAsync();
            return;
        }

        NavMeshAgent agent = Player.instance.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("[TestMap01] Player �� NavMeshAgent �R���|�[�l���g���A�^�b�`����Ă��܂���B");
            hasPlayerSpawned = false;
            return;
        }

        Vector3 initialPosition = Player.instance.transform.position;
        Debug.Log($"[TestMap01] ���[�v�O�̃v���C���[�ʒu: {initialPosition}");

        const int maxRoomAttempts = 50;
        bool playerWarped = false;
        Vector3 warpPosition = Vector3.zero;

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

            warpPosition = await FindWarpPositionAsync(center, size);
            if (warpPosition == Vector3.zero)
            {
                Debug.LogWarning($"[TestMap01] ����{roomIndex}�ł̃��[�v�ʒu�����Ɏ��s");
                continue;
            }

            if (Vector3.Distance(warpPosition, initialPosition) < 0.1f)
            {
                Debug.LogWarning($"[TestMap01] ���[�v�ʒu {warpPosition} �����݈ʒu�Ƃقړ����ł��B�ʂ̈ʒu�������܂��B");
                continue;
            }

            bool isFarEnough = enemyPositions.Count == 0 || enemyPositions.All(enemyPos => Vector3.Distance(warpPosition, enemyPos) >= minDistanceFromEnemy);
            if (!isFarEnough)
            {
                Debug.LogWarning($"[TestMap01] ���[�v�ʒu {warpPosition} ���G�ɋ߂����܂�");
                continue;
            }

            // NavMeshAgent �����Z�b�g���ă��[�v
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
            Debug.Log($"[TestMap01] NavMeshAgent.Warp ����: {warpSuccess}, ���[�v�ʒu: {warpPosition}, isOnNavMesh: {agent.isOnNavMesh}");

            if (!warpSuccess || !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(warpPosition, out var navHit, 5.0f, NavMesh.AllAreas))
                {
                    warpPosition = navHit.position;
                    agent.Warp(warpPosition);
                    Debug.Log($"[TestMap01] �Ď��s���[�v����: {navHit.position}, isOnNavMesh: {agent.isOnNavMesh}");
                }
                else
                {
                    Debug.LogWarning($"[TestMap01] NavMeshAgent.Warp �Ɏ��s�܂���NavMesh��ɂ���܂���: {warpPosition}");
                    // NavMesh ���g�킸���ڔz�u
                    Player.instance.transform.position = warpPosition;
                    agent.enabled = false; // NavMeshAgent �𖳌���
                    Debug.Log($"[TestMap01] NavMesh ���[�v���s�̂��ߒ��ڔz�u: {warpPosition}");
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
                Debug.Log($"[TestMap01] ���[�v��ʒu��NavMesh��ɒ���: {finalNavHit.position}, isOnNavMesh: {agent.isOnNavMesh}");
            }
            else
            {
                Debug.LogWarning($"[TestMap01] ���[�v��ʒu {finalPosition} ��NavMesh��ɂ���܂���");
                continue;
            }

            playerWarped = true;
            hasPlayerSpawned = true;
            Debug.Log($"[TestMap01] �v���C���[�����[�v���܂���: {finalPosition}");
            break;
        }

        if (!playerWarped)
        {
            var fallbackPosition = defaultPosition + new Vector3(5f, 0.6f, 5f);
            if (NavMesh.SamplePosition(fallbackPosition, out var navHit, 10f, NavMesh.AllAreas))
            {
                fallbackPosition = navHit.position + Vector3.up * 0.1f;
                Debug.Log($"[TestMap01] �t�H�[���o�b�N�ʒu��NavMesh��ɒ���: {fallbackPosition}");
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
            Debug.Log($"[TestMap01] �t�H�[���o�b�N NavMeshAgent.Warp ����: {warpSuccess}, �ʒu: {fallbackPosition}");

            if (warpSuccess && agent.isOnNavMesh)
            {
                hasPlayerSpawned = true;
                Debug.Log($"[TestMap01] �t�H�[���o�b�N���[�v����: {fallbackPosition}");
            }
            else
            {
                Debug.LogWarning($"[TestMap01] �t�H�[���o�b�N���[�v�Ɏ��s: {fallbackPosition}, ���ڔz�u���܂�");
                Player.instance.transform.position = fallbackPosition;
                agent.enabled = false; // NavMeshAgent �𖳌���
                hasPlayerSpawned = true;
            }
        }
    }

    // ���[�v�ʒu����������񓯊����\�b�h
    private async UniTask<Vector3> FindWarpPositionAsync(Vector3 roomCenter, Vector3 roomSize)
    {
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
            Debug.LogWarning("[TestMap01] groundLayer����ł����B�f�t�H���g��Ground���C���[�ɐݒ肵�܂����B");
        }

        float margin = 0.5f;
        int maxAttempts = 100;
        float yOffset = Player.instance != null ? Player.instance.transform.localScale.y * 0.5f + 0.1f : 0.5f + 0.1f;

        Debug.Log($"[TestMap01] FindWarpPositionAsync: roomCenter={roomCenter}, roomSize={roomSize}");

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(roomCenter.x - roomSize.x / 2 + margin, roomCenter.x + roomSize.x / 2 - margin);
            float z = Random.Range(roomCenter.z - roomSize.z / 2 + margin, roomCenter.z + roomSize.z / 2 - margin);
            float y = defaultPosition.y + GroundSetting.size.y + 2.0f;
            Vector3 spawnPosition = new Vector3(x, y, z);

            Debug.Log($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �����ʒu={spawnPosition}");

            int mapX = Mathf.FloorToInt((x - defaultPosition.x) / GroundSetting.size.x);
            int mapZ = Mathf.FloorToInt((z - defaultPosition.z) / GroundSetting.size.z);

            if (mapX < 0 || mapX >= mapSizeW || mapZ < 0 || mapZ >= mapSizeH)
            {
                Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �̓}�b�v�͈͊O�ł��B");
                continue;
            }

            if (map[mapX, mapZ] != (int)objectType.ground)
            {
                Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �͒n�ʂł͂���܂���Bmap[{mapX}, {mapZ}]={(objectType)map[mapX, mapZ]}");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �q�b�g�����I�u�W�F�N�g {hit.collider.gameObject.name} ��Ground���C���[�ł͂���܂��� (���C���[: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
                    continue;
                }

                Vector3 finalPos = hit.point + Vector3.up * yOffset;
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 5.0f, NavMesh.AllAreas))
                {
                    Vector3 adjustedPos = navHit.position + Vector3.up * 0.1f;
                    if (!Physics.CheckSphere(adjustedPos, 0.5f, LayerMask.GetMask("Wall")))
                    {
                        Debug.Log($"[TestMap01] ���[�v�ʒu�����o: {adjustedPos}");
                        return adjustedPos;
                    }
                    else
                    {
                        Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: ���[�v�ʒu {adjustedPos} ���ǂƏՓ˂��Ă��܂��B");
                    }
                }
                else
                {
                    Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: NavMesh��̈ʒu��������܂���ł���: {finalPos}");
                }
            }
            else
            {
                string layerName = groundLayer.value == 0 ? "��" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)));
                Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �n�ʌ��o���s: �ʒu={spawnPosition}, LayerMask={layerName}, LayerMaskValue={groundLayer.value}, map[{mapX},{mapZ}]={(objectType)map[mapX, mapZ]}");
                Debug.DrawRay(spawnPosition, Vector3.down * 20f, Color.red, 10f);
            }
            await UniTask.Yield();
        }

        // �t�H�[���o�b�N�ʒu��ݒ�
        Vector3 fallbackPos = roomCenter + Vector3.up * (GroundSetting.size.y + yOffset);
        Debug.LogWarning($"[TestMap01] �ő厎�s��({maxAttempts})�𒴂��Ă����[�v�ʒu�������邱�Ƃ��ł��܂���ł����B�t�H�[���o�b�N�ʒu���g�p: {fallbackPos}");
        return fallbackPos;
    }

    // �G�𐶐����郁�\�b�h�i�G�̈ʒu���L�^�j
    async UniTask GenerateEnemiesInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap01] �G�v���n�u��null�ł��I");
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
            }
        }

        // �S�[���n�_�̔z�u
        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);

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

                    if (isClear) validPoints.Add(new Vector2Int(x, z));
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
            }
        }
    }

    private async UniTask GeneratePatrolPointInRoomsAsync(Transform patrolPoint, int generateNum)
    {
        if (patrolPoint == null)
        {
            Debug.LogError("[TestMap01] �p�j�n�_�v���n�u��null�ł��I");
            return;
        }

        if (patrolPointParent == null)
        {
            Debug.LogError("[TestMap01] �p�j�n�_�̐e�I�u�W�F�N�g��null�ł��IinitPrefab���m�F���Ă��������B");
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
                obj.transform.SetParent(patrolPointParent.transform); // �e��PatrolPoints�ɐݒ�
                patrolPointsList.Add(obj.transform); // ���X�g�ɒǉ�
                pointsGenerated++;
            }
            else
            {
                Debug.LogWarning($"[TestMap01] �p�j�n�_�̐����ʒu��������܂���ł����B�����C���f�b�N�X: {roomIndex}");
            }
        }

        Debug.Log($"[TestMap01] �p�j�n�_�̐������������܂����B������: {pointsGenerated}");
    }

    private async UniTask GenerateObjectsInRoomsAsync(GameObject prefab, int generateNum)
    {
        if (prefab == null)
        {
            Debug.LogError("[TestMap01] �A�C�e���v���n�u��null�ł��I");
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
                // �t�H�[���o�b�N�F�ő�ʐς̕����̒��S�ɔz�u
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
        float margin = 0.5f; // �}�[�W�������������Ĕz�u�\�Ȕ͈͂��g��
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
                Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �ʒu ({x}, {z}) �͒n�ʂł͂���܂���Bmap[{mapX}, {mapZ}]={(mapX >= 0 && mapX < mapSizeW && mapZ >= 0 && mapZ < mapSizeH ? (objectType)map[mapX, mapZ] : "�͈͊O")} ({objectName})");
                continue;
            }

            if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                {
                    Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �q�b�g�����I�u�W�F�N�g {hit.collider.gameObject.name} ��Ground���C���[�ł͂���܂��� ({objectName})");
                    continue;
                }

                Vector3 finalPos = hit.point + Vector3.up * (isPatrolPoint ? 0.05f : prefab.transform.localScale.y * 0.5f + 0.05f);
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(finalPos, out navHit, 5.0f, NavMesh.AllAreas))
                {
                    Debug.Log($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �z�u�ʒu {navHit.position} �������܂��� (NavMesh, {objectName})");
                    return navHit.position;
                }
                else
                {
                    // NavMesh��������Ȃ��ꍇ�ARaycast�̃q�b�g�ʒu���g�p
                    Debug.Log($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: NavMesh��������܂���ł������A�n�ʈʒu {finalPos} ���g�p���܂� ({objectName})");
                    return finalPos;
                }
            }
            else
            {
                string layerName = groundLayer.value == 0 ? "��" : LayerMask.LayerToName(Mathf.FloorToInt(Mathf.Log(groundLayer.value, 2)));
                Debug.LogWarning($"[TestMap01] ���s {attempt + 1}/{maxAttempts}: �n�ʌ��o���s: �ʒu={spawnPosition}, LayerMask={layerName}, LayerMaskValue={groundLayer.value} ({objectName})");
                Debug.DrawRay(spawnPosition, Vector3.down * 10f, Color.red, 5f);
            }
            await UniTask.Yield();
        }

        Debug.LogError($"[TestMap01] �ő厎�s��({maxAttempts})�𒴂��Ă� {objectName} �̔z�u�ʒu���������܂���ł����BroomCenter={roomCenter}, roomSize={roomSize}");
        return Vector3.zero;
    }

    private bool CheckNavMeshExists()
    {
        // �K���Ȉʒu�i�}�b�v�̒��S�Ȃǁj�� NavMesh ��̈ʒu���擾�ł��邩����
        Vector3 checkPosition = defaultPosition + new Vector3(mapSizeW * GroundSetting.size.x / 2, 0, mapSizeH * GroundSetting.size.z / 2);
        NavMeshHit hit;
        return NavMesh.SamplePosition(checkPosition, out hit, 1.0f, NavMesh.AllAreas);
    }

    private Vector3 warpPosition; // �N���X���Ő錾

    // ���[�v�������������郁�\�b�h
    async UniTask DelayedWarpAsync()
    {
        Debug.Log("[TestMap01] ���[�v�������������ANavMesh������ҋ@���܂��B");
        int maxWaitAttempts = 50; // �ő�ҋ@���s��
        int waitIntervalMs = 100; // �ҋ@�Ԋu�i�~���b�j

        for (int attempt = 0; attempt < maxWaitAttempts; attempt++)
        {
            if (CheckNavMeshExists())
            {
                Debug.Log("[TestMap01] NavMesh�������m�F����܂����B���[�v���Ď��s���܂��B");
                await WarpPlayerAsync(); // �Ď��s
                return;
            }
            Debug.Log($"[TestMap01] NavMesh�����ҋ@��... ���s {attempt + 1}/{maxWaitAttempts}");
            await UniTask.Delay(waitIntervalMs, cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        Debug.LogError("[TestMap01] NavMesh�������^�C���A�E�g���܂����B���[�v���L�����Z�����܂��B");
        hasPlayerSpawned = false;
    }
}