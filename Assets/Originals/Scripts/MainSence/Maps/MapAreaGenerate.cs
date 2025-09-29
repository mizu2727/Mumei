
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapAreaGenerate : MonoBehaviour
{
    [Header("マップエリアを格納する(ヒエラルキー上のマップエリアをアタッチすること)")]
    [SerializeField] private List <GameObject> areaPrefabList;

    [Header("ランダムに選ばれたマップエリアが格納される(ヒエラルキー上からのアタッチ禁止)")]
    public List<GameObject> useMapAreaList = new ();

    [Header("マップエリア生成地点のTransform配列(ヒエラルキー上のマップエリア生成地点をアタッチすること)")]
    [SerializeField] private Transform[] mapAreaPoint;


    [Header("アイテムを格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> itemPrefabList;

    [Header("ランダムに選ばれたアイテムが格納される(ヒエラルキー上からのアタッチ禁止)")]
    public List<GameObject> useItemList = new();

    [Header("アイテム生成地点のTransform配列(ヒエラルキー上のDrawerスクリプトのdrawerItemTransformをアタッチすること)")]
    [SerializeField] private Transform[] itemPoint;

    void Start()
    {
        //マップをランダムに配置
        MapGenerate();

        //アイテムをランダム配置
        ItemGenerate();
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
