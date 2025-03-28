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


    //ステージの見た目の色とサイズを指定する
    [SerializeField] ObjectState WallSetting;//壁の色とサイズを指定
    [SerializeField] ObjectState GroundSetting;//地面の色とサイズを指定
    [SerializeField] ObjectState RoadSetting;//道の色とサイズを指定

    //ステージのプレハブを指定する
    [SerializeField] GameObject groundPrefab;//地面のプレハブ
    [SerializeField] GameObject wallPrefab;//地面のプレハブ
    [SerializeField] GameObject roadPrefab;//廊下のプレハブ

    //マップ生成の初期位置を指定する
    [SerializeField] Vector3 defaultPosition;//マップを生成する初期位置を指定


    //アイテムの生成位置を指定
    [Header("アイテムの設定")]
    [SerializeField] private List<GameObject> itemPrefabs; // 複数のアイテムプレハブ
    [SerializeField] private List<int> itemGenerateNums;   // 各アイテムの生成数
    [SerializeField] public Vector3 roomCenter;//部屋の中心の位置
    [SerializeField] public Vector3 roomSize;//部屋のサイズ 


    //徘徊地点の生成位置を指定
    [SerializeField] public Transform[] patrolPoint;
    public bool isGeneratePatrolPoint = false;


    //敵の生成位置を指定
    [SerializeField] private List<GameObject> enemyPrefabs; // 複数の敵プレハブ
    [SerializeField] private List<int> enemyGenerateNums;   // 各敵の生成数


    private NavMeshSurface groundSurface;
    private NavMeshSurface roadSurface;


    [Serializable]
    class ObjectState
    {
        public Color color;
        public Vector3 size;
    }

    [SerializeField] private int mapSizeW;        // マップの横サイズ
    [SerializeField] private int mapSizeH;        // マップの縦サイズ
    private int[,] map;                           // マップの管理配列

    [SerializeField] private int roomNum;       // 部屋の数
    private int roomMin = 10;                   // 生成する部屋の最小値
    private int parentNum = 0;                  // 分割する部屋番号
    private int max = 0;                        // 最大面積
    private int roomCount;                      // 部屋カウント
    private int line = 0;                       // 分割点
    private int[,] roomStatus;                  // 部屋の管理配列

    private enum RoomStatus                     // 部屋の配列ステータス
    {
        x,// マップ座標ｘ
        y,// マップ座標ｙ
        w,// 分割した幅
        h,// 分割した高さ

        rx,// 部屋の生成位置
        ry,// 部屋の生成位置
        rw,// 部屋の幅
        rh,// 部屋の高さ
    }

    enum objectType
    {
        ground = 0,
        wall = 1,
        road = 2,
    }

    private GameObject[] mapObjects;               // マップ生成用のオブジェクト配列
    private GameObject[] objectParents;             // 各タイプ別の親オブジェクト配列

    private const int offsetWall = 2;   // 壁から離す距離
    private const int offset = 1;       // 調整用


    BaseEnemy baseEnemy;

    void Awake()
    {
        MapGenerate();
        Debug.Log("マップ生成");
        baseEnemy.Patrol();
        //isGeneratePatrolPoint = true;
    }

    // 生成用のオブジェクトを用意
    void initPrefab()
    {
        // 親オブジェクトの生成
        GameObject groundParent = new GameObject("Ground");
        GameObject wallParent = new GameObject("Wall");
        GameObject roadParent = new GameObject("Road");


        // GroundにNavMeshSurface追加
        groundSurface = groundParent.AddComponent<NavMeshSurface>();
        groundSurface.collectObjects = CollectObjects.Children;

        #if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(groundParent, 
            StaticEditorFlags.NavigationStatic);
        #endif

        // RoadにNavMeshSurface追加
        roadSurface = roadParent.AddComponent<NavMeshSurface>();
        roadSurface.collectObjects = CollectObjects.Children;
        #if UNITY_EDITOR
        GameObjectUtility.SetStaticEditorFlags(roadParent, 
            StaticEditorFlags.NavigationStatic);
        #endif


        // 配列に親オブジェクトを入れる
        objectParents = new GameObject[] { groundParent, wallParent, roadParent };


        // 迷路オブジェクトの初期化

        //地面の生成
        GameObject ground = Instantiate(groundPrefab);  
        ground.transform.localScale = GroundSetting.size;
        ground.GetComponent<Renderer>().material.color = GroundSetting.color;
        ground.name = "ground";        
        ground.transform.SetParent(groundParent.transform);


        //壁の生成
        GameObject wall = Instantiate(wallPrefab);
        wall.transform.localScale = WallSetting.size;
        wall.GetComponent<Renderer>().material.color = WallSetting.color;
        wall.name = "wall";
        wall.transform.SetParent(wallParent.transform);


        //廊下の生成
        GameObject road = Instantiate(roadPrefab);
        road.transform.localScale = RoadSetting.size;
        road.GetComponent<Renderer>().material.color = RoadSetting.color;
        road.name = "road";
        road.transform.SetParent(roadParent.transform);


        // WallオブジェクトにNavMeshObstacle追加＆Carve有効化
        var navObstacle = wall.AddComponent<NavMeshObstacle>();
        navObstacle.carving = true;


        // 配列にプレハブを入れる
        mapObjects = new GameObject[] { ground, wall, road };
    }

    private void MapGenerate()
    {
        initPrefab();

        // 部屋（StartX、StartY、幅、高さ）
        roomStatus = new int[System.Enum.GetNames(typeof(RoomStatus)).Length, roomNum];

        // フロア設定
        map = new int[mapSizeW, mapSizeH];


        // フロアの初期化
        for (int nowW = 0; nowW < mapSizeW; nowW++)
        {
            for (int nowH = 0; nowH < mapSizeH; nowH++)
            {
                // 壁を貼る
                map[nowW, nowH] = 2;
            }
        }

        // フロアを入れる
        roomStatus[(int)RoomStatus.x, roomCount] = 0;
        roomStatus[(int)RoomStatus.y, roomCount] = 0;
        roomStatus[(int)RoomStatus.w, roomCount] = mapSizeW;
        roomStatus[(int)RoomStatus.h, roomCount] = mapSizeH;

        // カウント追加
        roomCount++;

        // 部屋の数だけ分割する
        for (int splitNum = 0; splitNum < roomNum - 1; splitNum++)
        {
            // 変数初期化
            parentNum = 0;  // 分割する部屋番号
            max = 0;        // 最大面積

            // 最大の部屋番号を調べる
            for (int maxCheck = 0; maxCheck < roomNum; maxCheck++)
            {
                // 面積比較
                if (max < roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck])
                {
                    // 最大面積上書き
                    max = roomStatus[(int)RoomStatus.w, maxCheck] * roomStatus[(int)RoomStatus.h, maxCheck];

                    // 親の部屋番号セット
                    parentNum = maxCheck;
                }
            }



            //Step1 : 区域を分ける。
            //SplitPoint関数を 使用し縦に分けるか、横に分けるかを決定する
            // 取得した部屋をさらに割る
            if (SplitPoint(roomStatus[(int)RoomStatus.w, parentNum], roomStatus[(int)RoomStatus.h, parentNum]))
            {
                // 取得
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum] - line;
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum];

                // 親の部屋を整形する
                roomStatus[(int)RoomStatus.x, parentNum] += roomStatus[(int)RoomStatus.w, roomCount];
                roomStatus[(int)RoomStatus.w, parentNum] -= roomStatus[(int)RoomStatus.w, roomCount];
            }
            else
            {
                // 取得
                roomStatus[(int)RoomStatus.x, roomCount] = roomStatus[(int)RoomStatus.x, parentNum];
                roomStatus[(int)RoomStatus.y, roomCount] = roomStatus[(int)RoomStatus.y, parentNum];
                roomStatus[(int)RoomStatus.w, roomCount] = roomStatus[(int)RoomStatus.w, parentNum];
                roomStatus[(int)RoomStatus.h, roomCount] = roomStatus[(int)RoomStatus.h, parentNum] - line;

                // 親の部屋を整形する
                roomStatus[(int)RoomStatus.y, parentNum] += roomStatus[(int)RoomStatus.h, roomCount];
                roomStatus[(int)RoomStatus.h, parentNum] -= roomStatus[(int)RoomStatus.h, roomCount];
            }
            // カウントを加算
            roomCount++;
        }


        //Step2 : 区域内にランダムなサイズの部屋を生成
        // 分割した中にランダムな大きさの部屋を生成
        for (int i = 0; i < roomNum; i++)
        {
            // 生成座標の設定
            roomStatus[(int)RoomStatus.rx, i] = Random.Range(roomStatus[(int)RoomStatus.x, i] + offsetWall, (roomStatus[(int)RoomStatus.x, i] + roomStatus[(int)RoomStatus.w, i]) - (roomMin + offsetWall));
            roomStatus[(int)RoomStatus.ry, i] = Random.Range(roomStatus[(int)RoomStatus.y, i] + offsetWall, (roomStatus[(int)RoomStatus.y, i] + roomStatus[(int)RoomStatus.h, i]) - (roomMin + offsetWall));

            // 部屋の大きさを設定
            roomStatus[(int)RoomStatus.rw, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.w, i] - (roomStatus[(int)RoomStatus.rx, i] - roomStatus[(int)RoomStatus.x, i]) - offset);
            roomStatus[(int)RoomStatus.rh, i] = Random.Range(roomMin, roomStatus[(int)RoomStatus.h, i] - (roomStatus[(int)RoomStatus.ry, i] - roomStatus[(int)RoomStatus.y, i]) - offset);
        }


        //Step3 : map配列に情報を書き込み
        // マップ上書き
        for (int count = 0; count < roomNum; count++)
        {
            // 取得した部屋の確認
            for (int h = 0; h < roomStatus[(int)RoomStatus.h, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.w, count]; w++)
                {
                    // 部屋チェックポイント
                    map[w + roomStatus[(int)RoomStatus.x, count], h + roomStatus[(int)RoomStatus.y, count]] = 1;
                }

            }

            // 生成した部屋
            for (int h = 0; h < roomStatus[(int)RoomStatus.rh, count]; h++)
            {
                for (int w = 0; w < roomStatus[(int)RoomStatus.rw, count]; w++)
                {
                    map[w + roomStatus[(int)RoomStatus.rx, count], h + roomStatus[(int)RoomStatus.ry, count]] = 0;
                }
            }
        }

        // 道の生成
        int[] splitLength = new int[4];
        int roodPoint = 0;// 道を引く場所

        //Step4: ループの中で道を生成
        // 部屋から一番近い境界線を調べる(十字に調べる)
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            // 左の壁からの距離
            splitLength[0] = roomStatus[(int)RoomStatus.x, nowRoom] > 0 ?
                roomStatus[(int)RoomStatus.rx, nowRoom] - roomStatus[(int)RoomStatus.x, nowRoom] : int.MaxValue;
            // 右の壁からの距離
            splitLength[1] = (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) < mapSizeW ?
                (roomStatus[(int)RoomStatus.x, nowRoom] + roomStatus[(int)RoomStatus.w, nowRoom]) - (roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom]) : int.MaxValue;

            // 下の壁からの距離
            splitLength[2] = roomStatus[(int)RoomStatus.y, nowRoom] > 0 ?
                roomStatus[(int)RoomStatus.ry, nowRoom] - roomStatus[(int)RoomStatus.y, nowRoom] : int.MaxValue;
            // 上の壁からの距離
            splitLength[3] = (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) < mapSizeH ?
                (roomStatus[(int)RoomStatus.y, nowRoom] + roomStatus[(int)RoomStatus.h, nowRoom]) - (roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom]) : int.MaxValue;

            // マックスでない物のみ先へ
            for (int j = 0; j < splitLength.Length; j++)
            {
                if (splitLength[j] != int.MaxValue)
                {
                    // 上下左右判定
                    if (j < 2)
                    {
                        // 道を引く場所を決定
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.ry, nowRoom] + offset, roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset);

                        // マップに書き込む
                        for (int w = 1; w <= splitLength[j]; w++)
                        {
                            // 左右判定
                            if (j == 0)
                            {
                                // 左
                                map[(-w) + roomStatus[(int)RoomStatus.rx, nowRoom], roodPoint] = 2;
                            }
                            else
                            {
                                // 右
                                map[w + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;

                                // 最後
                                if (w == splitLength[j])
                                {
                                    // 一つ多く作る
                                    map[w + offset + roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset, roodPoint] = 2;
                                }
                            }
                        }
                    }
                    else
                    {
                        // 道を引く場所を決定
                        roodPoint = Random.Range(roomStatus[(int)RoomStatus.rx, nowRoom] + offset, roomStatus[(int)RoomStatus.rx, nowRoom] + roomStatus[(int)RoomStatus.rw, nowRoom] - offset);

                        // マップに書き込む
                        for (int h = 1; h <= splitLength[j]; h++)
                        {
                            // 上下判定
                            if (j == 2)
                            {
                                // 下
                                map[roodPoint, (-h) + roomStatus[(int)RoomStatus.ry, nowRoom]] = 2;
                            }
                            else
                            {
                                // 上
                                map[roodPoint, h + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;

                                // 最後
                                if (h == splitLength[j])
                                {
                                    // 一つ多く作る
                                    map[roodPoint, h + offset + roomStatus[(int)RoomStatus.ry, nowRoom] + roomStatus[(int)RoomStatus.rh, nowRoom] - offset] = 2;
                                }
                            }
                        }
                    }
                }
            }
        }

        int roadVec1 = 0;// 道の始点
        int roadVec2 = 0;// 道の終点

        // 道の接続
        for (int nowRoom = 0; nowRoom < roomNum; nowRoom++)
        {
            roadVec1 = 0;
            roadVec2 = 0;
            // 道を繋げる
            for (int roodScan = 0; roodScan < roomStatus[(int)RoomStatus.w, nowRoom]; roodScan++)
            {
                // 道を検索
                if (map[roodScan + roomStatus[(int)RoomStatus.x, nowRoom], roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    // 道の座標セット
                    if (roadVec1 == 0)
                    {
                        // 始点セット
                        roadVec1 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                    else
                    {
                        // 終点セット
                        roadVec2 = roodScan + roomStatus[(int)RoomStatus.x, nowRoom];
                    }
                }
            }
            // 道を引く
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                // 境界線を上書き
                map[roadSet, roomStatus[(int)RoomStatus.y, nowRoom]] = 2;
            }

            roadVec1 = 0;
            roadVec2 = 0;

            for (int roadScan = 0; roadScan < roomStatus[(int)RoomStatus.h, nowRoom]; roadScan++)
            {
                // 道を検索
                if (map[roomStatus[(int)RoomStatus.x, nowRoom], roadScan + roomStatus[(int)RoomStatus.y, nowRoom]] == 2)
                {
                    // 道の座標セット
                    if (roadVec1 == 0)
                    {
                        // 始点セット
                        roadVec1 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                    else
                    {
                        // 終点セット
                        roadVec2 = roadScan + roomStatus[(int)RoomStatus.y, nowRoom];
                    }
                }
            }
            // 道を引く
            for (int roadSet = roadVec1; roadSet < roadVec2; roadSet++)
            {
                // 境界線を上書き
                map[roomStatus[(int)RoomStatus.x, nowRoom], roadSet] = 2;
            }
        }

        //Step5 : 最後に配列mapをループし該当のオブジェクトを生成
        // オブジェクトを生成する
        for (int nowH = 0; nowH < mapSizeH; nowH++)
        {
            for (int nowW = 0; nowW < mapSizeW; nowW++)
            {
                // 壁の生成
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

                // 部屋の生成
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

                // 通路の生成
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

        //俳諧地点を生成
        for (int i = 0; i < roomNum; i++)
        {
            GeneratePatrolPointInRooms(patrolPoint[i], roomNum);
        }



        //敵を生成
        //敵プレハブにColliderがついている必要がある
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            GenerateObjectsInRooms(enemyPrefabs[i], enemyGenerateNums[i]);
        }


        //アイテムを生成
        //アイテムプレハブにColliderがついている必要がある
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            GenerateObjectsInRooms(itemPrefabs[i], itemGenerateNums[i]);
        }



        // NavMeshのBakeを実行
        groundSurface.BuildNavMesh();
        roadSurface.BuildNavMesh();
    }

    // 分割点のセット(int x, int y)、大きい方を分割する
    private bool SplitPoint(int x, int y)
    {
        // 分割位置の決定
        if (x > y)
        {
            line = Random.Range(roomMin + (offsetWall * 2), x - (offsetWall * 2 + roomMin));// 縦割り
            return true;
        }
        else
        {
            line = Random.Range(roomMin + (offsetWall * 2), y - (offsetWall * 2 + roomMin));// 横割り
            return false;
        }
    }

    //
    private void GeneratePatrolPointInRooms(Transform patrolPoint, int generateNum)
    {
        for (int i = 0; i < generateNum; i++)
        {
            // ランダムな部屋を選択
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
            // ランダムな部屋を選択
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

    //俳諧地点を部屋内にランダム生成
    void PlacePatrolPointInRoom(Transform patrolPoint, Vector3 roomCenter, Vector3 roomSize)
    {

        Debug.Log(patrolPoint + "生成");


        // 壁から1m離す
        float margin = 1.0f;

        // 部屋の中のランダムな位置を取得
        float x =
            Random.Range(roomCenter.x - roomSize.x / 2 + margin,
            roomCenter.x + roomSize.x / 2 - margin);

        float z =
            Random.Range(roomCenter.z - roomSize.z / 2 + margin,
            roomCenter.z + roomSize.z / 2 - margin);

        // 空中からRaycastを飛ばすため地面から少し浮かせる
        float y = roomCenter.y + 5.0f;


        Vector3 spawnPosinon = new Vector3(x, y, z);

        // レイキャストで地面の高さを検出
        if (Physics.Raycast(spawnPosinon, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 finalPos =
                hit.point + Vector3.up * (patrolPoint.transform.localScale.y * 0.5f + 0.05f);

            Instantiate(patrolPoint, finalPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("地面が見つからなかったため" + patrolPoint + "生成されませんでした");
        }
    }


    //プレハブを部屋内にランダム生成
    void PlaceObjectInRoom(GameObject prefab, Vector3 roomCenter, Vector3 roomSize)
    {

        Debug.Log(prefab + "生成");


        // 壁から1m離す
        float margin = 1.0f;

        // 部屋の中のランダムな位置を取得
        float x = 
            Random.Range(roomCenter.x - roomSize.x / 2 + margin, 
            roomCenter.x + roomSize.x / 2 - margin);

        float z = 
            Random.Range(roomCenter.z - roomSize.z / 2 + margin, 
            roomCenter.z + roomSize.z / 2 - margin);

        // 空中からRaycastを飛ばすため地面から少し浮かせる
        float y = roomCenter.y + 5.0f;


        Vector3 spawnPosinon = new Vector3(x, y, z);

        // レイキャストで地面の高さを検出
        if (Physics.Raycast(spawnPosinon, Vector3.down, out RaycastHit hit, 10f))
        {
            Vector3 finalPos = 
                hit.point + Vector3.up * (prefab.transform.localScale.y * 0.5f + 0.05f);

            Instantiate(prefab, finalPos, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("地面が見つからなかったため"+ prefab + "生成されませんでした");
        }
    }



    

}
