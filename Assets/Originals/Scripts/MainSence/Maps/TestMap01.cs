using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.AI;
using Unity.AI.Navigation;


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


    //�X�e�[�W�̌����ڂ̐F�ƃT�C�Y���w�肷��
    [SerializeField] ObjectState WallSetting;//�ǂ̐F�ƃT�C�Y���w��
    [SerializeField] ObjectState GroundSetting;//�n�ʂ̐F�ƃT�C�Y���w��
    [SerializeField] ObjectState RoadSetting;//���̐F�ƃT�C�Y���w��

    //�X�e�[�W�̃v���n�u���w�肷��
    [SerializeField] GameObject groundPrefab;//�n�ʂ̃v���n�u
    [SerializeField] GameObject wallPrefab;//�n�ʂ̃v���n�u
    [SerializeField] GameObject roadPrefab;//�L���̃v���n�u

    //�}�b�v�����̏����ʒu���w�肷��
    [SerializeField] Vector3 defaultPosition;//�}�b�v�𐶐����鏉���ʒu���w��


    //�A�C�e���̐����ʒu���w��
    [Header("�A�C�e���̐ݒ�")]
    [SerializeField] private List<GameObject> itemPrefabs; // �����̃A�C�e���v���n�u
    [SerializeField] private List<int> itemGenerateNums;   // �e�A�C�e���̐�����
    [SerializeField] public Vector3 roomCenter;//�����̒��S�̈ʒu
    [SerializeField] public Vector3 roomSize;//�����̃T�C�Y 


    //�p�j�n�_�̐����ʒu���w��
    [SerializeField] public Transform[] patrolPoint;



    //�G�̐����ʒu���w��
    [SerializeField] private List<GameObject> enemyPrefabs; // �����̓G�v���n�u
    [SerializeField] private List<int> enemyGenerateNums;   // �e�G�̐�����


    //�S�[���n�_
    [SerializeField] private GameObject goalGround; // �S�[���n�_�̒n��
    [SerializeField] private Vector3 goalConnectionPoint; // �S�[���Ƃ̐ڑ��_�i�C���X�y�N�^�[�Őݒ�A�����I�����͖����j
    [SerializeField] private int connectionRoadLength = 3; // �S�[���ւ̒ʘH�̒���
    [SerializeField] private bool autoSelectConnectionPoint = true; // �ڑ��_�������I�����邩
    [SerializeField] private GameObject goalObjectPrefab; // �S�[���I�u�W�F�N�g�̃v���n�u


    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;


    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [SerializeField] private int mapSizeW;        // �}�b�v�̉��T�C�Y
    [SerializeField] private int mapSizeH;        // �}�b�v�̏c�T�C�Y
    private int[,] map;                           // �}�b�v�̊Ǘ��z��

    [SerializeField] private int roomNum;       // �����̐�
    private int roomMin = 10;                   // �������镔���̍ŏ��l
    private int parentNum = 0;                  // �������镔���ԍ�
    private int max = 0;                        // �ő�ʐ�
    private int roomCount;                      // �����J�E���g
    private int line = 0;                       // �����_
    private int[,] roomStatus;                  // �����̊Ǘ��z��

    private enum RoomStatus                     // �����̔z��X�e�[�^�X
    {
        x,// �}�b�v���W��
        y,// �}�b�v���W��
        w,// ����������
        h,// ������������

        rx,// �����̐����ʒu
        ry,// �����̐����ʒu
        rw,// �����̕�
        rh,// �����̍���
    }

    enum objectType
    {
        ground = 0,
        wall = 1,
        road = 2,
    }

    private GameObject[] mapObjects;               // �}�b�v�����p�̃I�u�W�F�N�g�z��
    private GameObject[] objectParents;             // �e�^�C�v�ʂ̐e�I�u�W�F�N�g�z��

    private const int offsetWall = 2;   // �ǂ��痣������
    private const int offset = 1;       // �����p


    BaseEnemy baseEnemy;

    [SerializeField] private bool isDebugOffGenerate = false;

    void Awake()
    {
        if (isDebugOffGenerate) return;
        MapGenerate();
        Debug.Log("�}�b�v����");
    }

    IEnumerator RebuildNavMesh()
    {
        yield return new WaitForEndOfFrame(); // �t���[���I����Ɏ��s
        groundSurface.BuildNavMesh();
        roadSurface.BuildNavMesh();
    }
    void Start()
    {
        StartCoroutine(RebuildNavMesh());
    }

    // �����p�̃I�u�W�F�N�g��p��
    void initPrefab()
    {
        // �e�I�u�W�F�N�g�̐���
        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");


        // Ground��NavMeshSurface�ǉ�
        groundSurface = groundParent.AddComponent<NavMeshSurface>();
        groundSurface.collectObjects = CollectObjects.All;

#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(groundParent, 
            StaticEditorFlags.NavigationStatic);
        #endif

        // Road��NavMeshSurface�ǉ�
        roadSurface = roadParent.AddComponent<NavMeshSurface>();
        roadSurface.collectObjects = CollectObjects.All;

#if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(roadParent, 
            StaticEditorFlags.NavigationStatic);
        #endif


        // �z��ɐe�I�u�W�F�N�g������
        objectParents = new GameObject[] { groundParent, wallParent, roadParent };


        // ���H�I�u�W�F�N�g�̏�����

        //�n�ʂ̐���
        GameObject ground = Instantiate(groundPrefab);  
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";        
        ground.transform.SetParent(groundParent.transform);


        //�ǂ̐���
        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";
        wall.transform.SetParent(wallParent.transform);


        //�L���̐���
        GameObject road = Instantiate(roadPrefab);
        road.transform.localScale = RoadSetting.size;
        road.GetComponent<Renderer>().material.color = RoadSetting.color;
        road.name = "road";
        road.transform.SetParent(roadParent.transform);


        // Wall�I�u�W�F�N�g��NavMeshObstacle�ǉ���Carve�L����
        var navObstacle = wall.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true;


        // �z��Ƀv���n�u������
        mapObjects = new GameObject[] { ground, wall, road };
    }

    private void MapGenerate()
    {
        initPrefab();


        // �}�b�v�����R�[�h�i���������A���������A�ʘH�����j
        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];
        map = new int[mapSizeW, mapSizeH];

        // �t���A�̏�����
        for (int nowW = 0; nowW < mapSizeW; nowW++)
        {
            for (int nowH = 0; nowH < mapSizeH; nowH++)
            {
                map[nowW, nowH] = 2; // ��
            }
        }


        // �����iStartX�AStartY�A���A�����j
        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];

        // �t���A�ݒ�
        map = new int[mapSizeW, mapSizeH];


        // �t���A�̏�����
        for (int nowW = 0; nowW < mapSizeW; nowW++)
        {
            for (int nowH = 0; nowH < mapSizeH; nowH++)
            {
                // �ǂ�\��
                map[nowW, nowH] = 2;
            }
        }

        // �t���A������
        roomStatus[(int)RoomStatus.x, roomCount] = 0;
        roomStatus[(int)RoomStatus.y, roomCount] = 0;
        roomStatus[(int)RoomStatus.w, roomCount] = mapSizeW;
        roomStatus[(int)RoomStatus.h, roomCount] = mapSizeH;

        // �J�E���g�ǉ�
        roomCount++;

        // �����̐�������������
        for (int splitNum = 0; splitNum < roomNum - 1; splitNum++)
        {
            // �ϐ�������
            parentNum = 0;  // �������镔���ԍ�
            max = 0;        // �ő�ʐ�

            // �ő�̕����ԍ��𒲂ׂ�
            for (int maxCheck = 0; maxCheck < roomNum; maxCheck++)
            {
                // �ʐϔ�r
                if (max < roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck])
                {
                    // �ő�ʐϏ㏑��
                    max = roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck];

                    // �e�̕����ԍ��Z�b�g
                    parentNum = maxCheck;
                }
            }



            //Step1 : ���𕪂���B
            //SplitPoint�֐��� �g�p���c�ɕ����邩�A���ɕ����邩�����肷��
            // �擾��������������Ɋ���
            if (SplitPoint(roomStatus[(int)RoomStatus.w, parentNum], roomStatus[(int)RoomStatus.h, parentNum]))
            {
                // �擾
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum] - line;
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum];

                // �e�̕����𐮌`����
                roomStatus[(int)RoomStatus.x, parentNum] += roomStatus[(int)RoomStatus.w, roomCount];
                roomStatus[(int)RoomStatus.w, parentNum] -= roomStatus[(int)RoomStatus.w, roomCount];
            }
            else
            {
                // �擾
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum];
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum] - line;

                // �e�̕����𐮌`����
                roomStatus[(int)RoomStatus.y, parentNum] += roomStatus[(int)RoomStatus.h, roomCount];
                roomStatus[(int)RoomStatus.h, parentNum] -= roomStatus[(int)RoomStatus.h, roomCount];
            }
            // �J�E���g�����Z
            roomCount++;
        }


        //Step2 : �����Ƀ����_���ȃT�C�Y�̕����𐶐�
        // �����������Ƀ����_���ȑ傫���̕����𐶐�
        for (int i = 0; i < roomNum; i++)
        {
            // �������W�̐ݒ�
            roomStatus[(int)RoomStatus.rx, i] = Random.Range(roomStatus[(int)RoomStatus.x, i] + offsetWall, (roomStatus[(int)RoomStatus.x, i] + roomStatus[(int)RoomStatus.w, i]) - (roomMin + offsetWall));
            roomStatus[(int)RoomStatus.ry, i] = Random.Range(roomStatus[(int)RoomStatus.y, i] + offsetWall, (roomStatus[(int)RoomStatus.y, i] + roomStatus[(int)RoomStatus.h, i]) - (roomMin + offsetWall));

            // �����̑傫����ݒ�
            roomStatus[(int)RoomStatus.rw, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.w, i] - (roomStatus[(int)RoomStatus.rx, i] - roomStatus[(int)RoomStatus.x, i]) - offset);
            roomStatus[(int)RoomStatus.rh, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.h, i] - (roomStatus[(int)RoomStatus.ry, i] - roomStatus[(int)RoomStatus.y, i]) - offset);
        }


        //Step3 : map�z��ɏ�����������
        // �}�b�v�㏑��
        for (int count = 0; count < roomNum; count++)
        {
            // �擾���������̊m�F
            for (int h = 0; h < roomStatus[(int)RoomStatus.h, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.w, count]; w++)
                {
                    // �����`�F�b�N�|�C���g
                    map[w + roomStatus[(int)RoomStatus.x, count], h + roomStatus[(int)RoomStatus.y, count]] = 1;
                }

            }

            // ������������
            for (int h = 0; h < roomStatus[(int)RoomStatus.rh, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.rw, count]; w++)
                {
                    map[w + roomStatus[(int)RoomStatus.rx, count], h + roomStatus[(int)RoomStatus.ry, count]] = 0;
                }
            }
        }

        // ���̐���
        int[] splitLength = new int[4];
        int roodPoint = 0;// ���������ꏊ

        //Step4: ���[�v�̒��œ��𐶐�
        // ���������ԋ߂����E���𒲂ׂ�(�\���ɒ��ׂ�)
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            // ���̕ǂ���̋���
            splitLength[0] = roomStatus[(int)RoomStatus.x, nowRoom] > 0 ?
                roomStatus[(int)RoomStatus.rx, nowRoom] - roomStatus[(int)RoomStatus.x, nowRoom] : int.MaxValue;
            // �E�̕ǂ���̋���
            splitLength[1] = (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) < mapSizeW ?
                (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) - (roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom]) : int.MaxValue;

            // ���̕ǂ���̋���
            splitLength[2] = roomStatus[(int)RoomStatus.y, nowRoom] > 0 ?
                roomStatus[(int)RoomStatus.ry, nowRoom] - roomStatus[(int)RoomStatus.y, nowRoom] : int.MaxValue;
            // ��̕ǂ���̋���
            splitLength[3] = (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) < mapSizeH ?
                (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) - (roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom]) : int.MaxValue;

            // �}�b�N�X�łȂ����̂ݐ��
            for (int j = 0; j < splitLength.Length; j++)
            {
                if (splitLength[j] != int.MaxValue)
                {
                    // �㉺���E����
                    if (j < 2)
                    {
                        // ���������ꏊ������
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.ry, nowRoom] + offset, roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset);

                        // �}�b�v�ɏ�������
                        for (int w = 1; w <= splitLength[j]; w++)
                        {
                            // ���E����
                            if (j == 0)
                            {
                                // ��
                                map[(-w) + roomStatus[(int)RoomStatus.rx, nowRoom], roodPoint] = 2;
                            }
                            else
                            {
                                // �E
                                map[w + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;

                                // �Ō�
                                if (w == splitLength[j])
                                {
                                    // ��������
                                    map[w + offset + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        // ���������ꏊ������
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.rx, nowRoom] + offset, roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset);

                        // �}�b�v�ɏ�������
                        for (int h = 1; h <= splitLength[j]; h++)
                        {
                            // �㉺����
                            if (j == 2)
                            {
                                // ��
                                map[roodPoint, (-h) + roomStatus[(int)RoomStatus.ry, nowRoom]] = 2;
                            }
                            else
                            {
                                // ��
                                map[roodPoint, h + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;

                                // �Ō�
                                if (h == splitLength[j])
                                {
                                    // ��������
                                    map[roodPoint, h + offset + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;
                                }
                            }
                        }
                    }
                }
            }
        }

        int roadVec1 = 0;// ���̎n�_
        int roadVec2 = 0;// ���̏I�_

        // ���̐ڑ�
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            roadVec1 = 0;
            roadVec2 = 0;
            // �����q����
            for (int roodScan = 0; roodScan < roomStatus[(int)RoomStatus.w, nowRoom]; roodScan++)
            {
                // ��������
                if (map[roodScan + roomStatus[(int)RoomStatus.x, nowRoom], roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    // ���̍��W�Z�b�g
                    if (roadVec1 == 0)
                    {
                        // �n�_�Z�b�g
                        roadVec1 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                    else
                    {
                        // �I�_�Z�b�g
                        roadVec2 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                }
            }
            // ��������
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                // ���E�����㏑��
                map[roadSet, roomStatus[(int)RoomStatus.y, nowRoom]] = 2;
            }

            roadVec1 = 0;
            roadVec2 = 0;

            for (int roadScan = 0; roadScan < roomStatus[(int)RoomStatus.h, nowRoom]; roadScan++)
            {
                // ��������
                if (map[roomStatus[(int)RoomStatus.x, nowRoom], roadScan + roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    // ���̍��W�Z�b�g
                    if (roadVec1 == 0)
                    {
                        // �n�_�Z�b�g
                        roadVec1 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                    else
                    {
                        // �I�_�Z�b�g
                        roadVec2 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                }
            }
            // ��������
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                // ���E�����㏑��
                map[roomStatus[(int)RoomStatus.x, nowRoom], roadSet] = 2;
            }
        }

        //Step5 : �Ō�ɔz��map�����[�v���Y���̃I�u�W�F�N�g�𐶐�
        // �I�u�W�F�N�g�𐶐�����
        for (int nowH = 0; nowH < mapSizeH; nowH++)
        {
            for (int nowW = 0; nowW < mapSizeW; nowW++)
            {
                // �ǂ̐���
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

                // �����̐���
                if (map[nowW, nowH] == (int)objectType.ground)
                {
                    GameObject mazeObject = Instantiate(
                        mapObjects[map[nowW, nowH]],
                        new Vector3(
                            defaultPosition.x + nowW * mapObjects[map[nowW, nowH]].transform.localScale.x,
                            defaultPosition.y,
                            defaultPosition.z + nowH * mapObjects[map[nowW, nowH]].transform.localScale.z),
                        Quaternion.identity, objectParents[map[nowW, nowH]].transform);
                }

                // �ʘH�̐���
                if (map[nowW, nowH] == (int)objectType.road)
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


        //�S�[���n�_�ւ̒ʘH��ǉ�
        AddGoalConnection();


        // �I�u�W�F�N�g����
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

        // �S�[���n�_��NavMesh�𓝍�
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
            goalSurface.BuildNavMesh();
            Debug.Log("goalSurface �� NavMesh ���\�z���܂����I");
        }



        // NavMesh��Bake�����s
        if (groundSurface != null)
        {
            groundSurface.BuildNavMesh();
            Debug.Log("groundSurface �� NavMesh ���č\�z���܂����I");
        }
        else
        {
            Debug.LogError("groundSurface ���ݒ肳��Ă��܂���I");
        }

        if (roadSurface != null)
        {
            roadSurface.BuildNavMesh();
            Debug.Log("roadSurface �� NavMesh ���č\�z���܂����I");
        }
        else
        {
            Debug.LogError("roadSurface ���ݒ肳��Ă��܂���I");
        }

        Bounds bounds = groundSurface.navMeshData.sourceBounds;
        Debug.Log("NavMesh�͈�: " + bounds);

        //�o�~�n�_�𐶐�
        for (int i = 0; i < roomNum; i++)
        {
            GeneratePatrolPointInRooms(patrolPoint[i], roomNum);
        }

        //�G�𐶐�
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            GenerateObjectsInRooms(enemyPrefabs[i], enemyGenerateNums[i]);
        }


        //�A�C�e���𐶐�
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            GenerateObjectsInRooms(itemPrefabs[i], itemGenerateNums[i]);
        }
    }

    // �����_�̃Z�b�g(int x, int y)�A�傫�����𕪊�����
    private bool SplitPoint(int x, int y)
    {
        // �����ʒu�̌���
        if (x > y)
        {
            line = Random.Range(roomMin + (offsetWall * 2), x - (offsetWall * 2 + roomMin));// �c����
            return true;
        }
        else
        {
            line = Random.Range(roomMin + (offsetWall * 2), y - (offsetWall * 2 + roomMin));// ������
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

        // �ڑ��_�������I��
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
            Debug.Log($"�����I�������ڑ��_: ({goalConnectionPoint.x}, {goalConnectionPoint.z})");
        }
        else
        {
            connectPoint = new Vector2Int((int)goalConnectionPoint.x, (int)goalConnectionPoint.z);
        }

        // �ڑ��_�̌���
        int connectX = connectPoint.x;
        int connectZ = connectPoint.y;
        if (connectX < 0 || connectX >= mapSizeW || connectZ < 0 || connectZ >= mapSizeH)
        {
            Debug.LogWarning($"�ڑ��_ ({connectX}, {connectZ}) ���}�b�v�͈͊O�ł��I");
            return;
        }

        // �ڑ��_�������܂��͒ʘH�ł��邱�Ƃ��m�F
        if (map[connectX, connectZ] != (int)objectType.ground && map[connectX, connectZ] != (int)objectType.road)
        {
            Debug.LogWarning($"�ڑ��_ ({connectX}, {connectZ}) �͕����܂��͒ʘH�ł͂���܂���I");
            return;
        }

        // �ʘH�p�X���N���A�i�ǂ𓹘H�ɕύX�j
        ClearPathToGoal(connectX, connectZ);

        // �ʘH�𐶐��i�E�����j
        for (int i = 1; i <= connectionRoadLength; i++)
        {
            int roadX = connectX + i;
            if (roadX >= mapSizeW) break; // �}�b�v�͈͊O�̓X�L�b�v

            Vector3 roadPos = new Vector3(
                defaultPosition.x + roadX * RoadSetting.size.x,
                defaultPosition.y,
                defaultPosition.z + connectZ * RoadSetting.size.z);
            GameObject road = Instantiate(
                mapObjects[(int)objectType.road],
                roadPos,
                Quaternion.identity,
                objectParents[(int)objectType.road].transform);
            Debug.Log($"�ʘH�𐶐�: {roadPos}");
        }

        // �}�b�v�͈͊O�ɒǉ��̒ʘH�𐶐��i�K�v�ɉ����āj
        int extendedRoads = connectionRoadLength - (mapSizeW - connectX - 1);
        if (extendedRoads > 0)
        {
            for (int i = 0; i < extendedRoads; i++)
            {
                Vector3 roadPos = new Vector3(
                    defaultPosition.x + (mapSizeW + i) * RoadSetting.size.x,
                    defaultPosition.y, // goalGround �Ɠ�������
                    defaultPosition.z + connectZ * RoadSetting.size.z);
                GameObject road = Instantiate(
                    mapObjects[(int)objectType.road],
                    roadPos,
                    Quaternion.identity,
                    objectParents[(int)objectType.road].transform);
                Debug.Log($"�g���ʘH�𐶐�: {roadPos}");
            }
        }

        // �S�[���n�_�̈ʒu�𒲐�
        int totalRoadLength = Mathf.Min(connectionRoadLength, mapSizeW - connectX - 1) + (extendedRoads > 0 ? extendedRoads : 0);
        goalGround.transform.position = new Vector3(
            defaultPosition.x + (connectX + totalRoadLength) * RoadSetting.size.x,
            defaultPosition.y,
            defaultPosition.z + connectZ * RoadSetting.size.z);
        Debug.Log($"GoalGround �� {goalGround.transform.position} �ɔz�u");

        // goalGround �� NavMesh ���\�z
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
            goalSurface.BuildNavMesh();
            Debug.Log("goalSurface �� NavMesh ���\�z���܂����I");

            // NavMesh �͈̔͂��f�o�b�O
            if (goalSurface.navMeshData != null)
            {
                Bounds bounds = goalSurface.navMeshData.sourceBounds;
                Debug.Log($"goalSurface NavMesh �͈�: {bounds}");
            }
            else
            {
                Debug.LogWarning("goalSurface �� NavMesh �f�[�^����������Ă��܂���I");
            }
        }

        // �S�[���I�u�W�F�N�g�̐���
        if (goalObjectPrefab != null)
        {
            Vector3 goalObjectPosition = goalGround.transform.position + Vector3.up * 0.1f; // �n�ʂ���킸���ɕ�������
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(goalObjectPosition, out navHit, 5.0f, NavMesh.AllAreas))
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
                // �t�H�[���o�b�N: NavMesh �Ȃ��Œ��ڔz�u
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


    // �L���Ȑڑ��_�i�����܂��͒ʘH�A�ǂ��痣�ꂽ�ʒu�j��������
    private Vector2Int FindValidConnectionPoint()
    {
        List<Vector2Int> validPoints = new List<Vector2Int>();

        // �}�b�v���𑖍����ĕ����܂��͒ʘH�����W
        for (int x = 1; x < mapSizeW - 1; x++) // �[�������
        {
            for (int z = 1; z < mapSizeH - 1; z++)
            {
                if (map[x, z] == (int)objectType.ground || map[x, z] == (int)objectType.road)
                {
                    // �E�����ɕǂ��Ȃ����`�F�b�N
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

        // �����_���ɑI��
        return validPoints[Random.Range(0, validPoints.Count)];
    }


    // �ʘH�p�X���N���A�i�ǂ𓹘H�ɕύX�j
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



    //
    private void GeneratePatrolPointInRooms(Transform patrolPoint, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            // �����_���ȕ�����I��
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

            PlacePatrolPointInRoom(patrolPoint, center, size);
        }
    }

    //
    private void GenerateObjectsInRooms(GameObject prefab, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            // �����_���ȕ�����I��
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

            PlaceObjectInRoom(prefab, center, size);
        }
    }

    //�o�~�n�_�𕔉����Ƀ����_������
    void PlacePatrolPointInRoom(Transform patrolPoint, Vector3 roomCenter, Vector3 roomSize)
    {
        Debug.Log(patrolPoint + "����");


        // �ǂ���1m����
        float margin = 1.0f;

        // �����̒��̃����_���Ȉʒu���擾
        float x =
            Random.Range(roomCenter.x - roomSize.x / 2 + margin,
            roomCenter.x + roomSize.x / 2 - margin);

        float z =
            Random.Range(roomCenter.z - roomSize.z / 2 + margin,
            roomCenter.z + roomSize.z / 2 - margin);

        // �󒆂���Raycast���΂����ߒn�ʂ��班����������
        float y = roomCenter.y + 5.0f;

        Vector3 spawnPosition = new Vector3(x, y, z);

        if (Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 finalPos = hit.point + Vector3.up * 0.05f;

            // NavMesh��̈ʒu�ɏC��
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(finalPos, out navHit, 2.0f, NavMesh.AllAreas))
            {
                Instantiate(patrolPoint, navHit.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("NavMesh��̈ʒu�������炸 patrolPoint �𐶐��ł��܂���ł���: " + finalPos);
            }
        }
        else
        {
            Debug.LogWarning("�n�ʂ�������Ȃ���������"+patrolPoint+"�𐶐��ł��܂���ł���: " + spawnPosition);
        }
    }


    //�v���n�u�𕔉����Ƀ����_������
    void PlaceObjectInRoom(GameObject prefab, Vector3 roomCenter, Vector3 roomSize)
    {

        Debug.Log(prefab + "����");


        // �ǂ���1m����
        float margin = 1.0f;

        // �����̒��̃����_���Ȉʒu���擾
        float x = 
            Random.Range(roomCenter.x - roomSize.x / 2 + margin, 
            roomCenter.x + roomSize.x / 2 - margin);

        float z = 
            Random.Range(roomCenter.z - roomSize.z / 2 + margin, 
            roomCenter.z + roomSize.z / 2 - margin);

        // �󒆂���Raycast���΂����ߒn�ʂ��班����������
        float y = roomCenter.y + 5.0f;


        Vector3 spawnPosinon = new Vector3(x, y, z);

        // ���C�L���X�g�Œn�ʂ̍��������o
        if (Physics.Raycast(spawnPosinon, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 finalPos = 
                hit.point + Vector3.up * (prefab.transform.localScale.y * 0.5f + 0.05f);

            Instantiate(prefab, finalPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("�n�ʂ�������Ȃ���������"+ prefab + "��������܂���ł���");
        }
    }
}
