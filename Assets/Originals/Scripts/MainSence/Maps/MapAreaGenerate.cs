
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


/// <summary>
/// マップエリアランダム配置クラス
/// </summary>
public class MapAreaGenerate : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static MapAreaGenerate instance;

    [Header("マップエリアを格納する(ヒエラルキー上のマップエリアをアタッチすること)")]
    [SerializeField] private List <GameObject> areaPrefabList;

    [Header("ランダムに選ばれたマップエリアが格納される(ヒエラルキー上からのアタッチ禁止。シャッフルされたマップ確認用)")]
    public List<GameObject> useMapAreaList = new ();

    [Header("マップエリア生成地点のTransform配列(ヒエラルキー上のマップエリア生成地点をアタッチすること)")]
    [SerializeField] private Transform[] mapAreaPoint;


    [Header("アイテムを格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> itemPrefabList;

    [Header("ランダムに選ばれたアイテムが格納される(ヒエラルキー上からのアタッチ禁止。シャッフルされたアイテム確認用)")]
    public List<GameObject> useItemList = new();

    [Header("アイテム生成地点のTransform配列(ヒエラルキー上のDrawerスクリプトのdrawerItemTransformをアタッチすること)")]
    [SerializeField] private Transform[] itemPoint;

    /// <summary>
    /// 地面オブジェクト
    /// </summary>
    [SerializeField] private StageGround stageGround;

    /// <summary>
    /// navMeshSurface
    /// </summary>
    [SerializeField] private NavMeshSurface navMeshSurface;


    [Header("敵の位置(ヒエラルキー上からアタッチすること。ステージライトとの距離測定で必要)")]
    [SerializeField] public Transform[] baseEnemyTransformArray;

    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    void OnDestroy()
    {
        for(int i = 0; i < areaPrefabList.Count; i++)
        {
            //マップエリアが存在する場合
            if (areaPrefabList[i] != null)
            {
                //マップエリアをnullに設定
                areaPrefabList[i] = null;
            }
        }

        for(int i = 0; i < useMapAreaList.Count; i++)
        {
            //シャッフルされたマップエリアが存在する場合
            if (useMapAreaList[i] != null)
            {
                //シャッフルされたマップエリアをnullに設定
                useMapAreaList[i] = null;
            }
        }

        for(int i = 0; i < mapAreaPoint.Length; i++)
        {
            //マップエリア生成地点が存在する場合
            if (mapAreaPoint[i] != null)
            {
                //マップエリア生成地点をnullに設定
                mapAreaPoint[i] = null;
            }
        }

        for(int i = 0; i < itemPrefabList.Count; i++)
        {
            //アイテムが存在する場合
            if (itemPrefabList[i] != null)
            {
                //アイテムをnullに設定
                itemPrefabList[i] = null;
            }
        }

        for(int i = 0; i < useItemList.Count; i++)
        {
            //シャッフルされたアイテムが存在する場合
            if (useItemList[i] != null)
            {
                //シャッフルされたアイテムをnullに設定
                useItemList[i] = null;
            }
        }

        for(int i = 0; i < itemPoint.Length; i++)
        {
            //アイテム生成地点が存在する場合
            if (itemPoint[i] != null)
            {
                //アイテム生成地点をnullに設定
                itemPoint[i] = null;
            }
        }

        for(int i = 0; i < baseEnemyTransformArray.Length; i++)
        {
            //敵の位置が存在する場合
            if (baseEnemyTransformArray[i] != null)
            {
                //敵の位置をnullに設定
                baseEnemyTransformArray[i] = null;
            }
        }

        //インスタンスが存在する場合
        if (instance == this)
        {
            //インスタンスをnullに設定
            instance = null;
        }
    }

    private void Awake()
    {
        //インスタンスが存在しない場合
        if (instance == null)
        {
            //インスタンスを自身に設定
            instance = this;
        }
        else
        {
            //インスタンスが既に存在する場合、自身を破棄
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //マップをランダムに配置
        MapGenerate();

        //アイテムをランダム配置
        ItemGenerate();


        stageGround.Build(navMeshSurface);
    }

    /// <summary>
    /// マップをランダム配置するメソッド
    /// </summary>
    void MapGenerate() 
    {
        //areaPrefabListのコピーを作成
        List<GameObject> shuffledMapAreaPrefabList = new List<GameObject>(areaPrefabList);

        //areaPrefabListをシャッフル
        ShuffleList(shuffledMapAreaPrefabList);

        // シャッフルされたリストを格納
        useMapAreaList.AddRange(shuffledMapAreaPrefabList);


        for (int i = 0; i < useMapAreaList.Count; i++)
        {
            //マップをランダム配置する
            useMapAreaList[i].transform.position = mapAreaPoint[i].position;
        }
    }

    /// <summary>
    /// アイテムをランダム配置するメソッド
    /// </summary>
    void ItemGenerate()
    {
        //areaPrefabListのコピーを作成
        List<GameObject> shuffledItemPrefabList = new List<GameObject>(itemPrefabList);

        //areaPrefabListをシャッフル
        ShuffleList(shuffledItemPrefabList);

        //シャッフルされたリストを格納
        useItemList.AddRange(shuffledItemPrefabList);

        for (int i = 0; i < useItemList.Count; i++)
        {
            //アイテムをランダム配置する
            useItemList[i].transform.position = itemPoint[i].position;
        }

        for (int i = 0; i < useItemList.Count; i++)
        {
            //アイテム生成地点にDrawerコンポーネントがあるか確認
            Drawer drawer = itemPoint[i].GetComponent<Drawer>();

            //nullチェック
            if (drawer != null)
            {
                //アイテムを生成する際、位置情報をitemPoint[i].positionではなく、Drawerの親のTransformに合わせる
                GameObject newItem = Instantiate(useItemList[i]);

                //アイテムの位置をitemPointに合わせる
                newItem.transform.position = itemPoint[i].position;

                //生成したアイテムをDrawerにアタッチ
                drawer.SetItemTransform(newItem.transform);
            }
            else
            {
                Debug.LogError(itemPoint[i].name + "にDrawerコンポーネントが見つかりませんでした！");
            }
        }
    }

    /// <summary>
    /// リストの要素をランダムにシャッフルするメソッド
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
