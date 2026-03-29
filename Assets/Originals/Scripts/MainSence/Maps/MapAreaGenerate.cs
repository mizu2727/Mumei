
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


    [Header("ドキュメントとミステリーアイテムを格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> documentAndMysteryItemPrefabList;

    [Header("コンパスを格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> compassPrefabList;

    [Header("スタミナ増強剤を格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> staminaEnhancerPrefabList;

    [Header("難易度Easyのスタミナ増強剤格納数")]
    [SerializeField] private int easyStaminaEnhancerCount;

    [Header("デフォルト(難易度Normal)のスタミナ増強剤格納数")]
    [SerializeField] private int defaultStaminaEnhancerCount;

    [Header("難易度Nightmareのスタミナ増強剤格納数")]
    [SerializeField] private int nightmareStaminaEnhancerCount;

    /// <summary>
    /// スタミナ増強剤格納数
    /// </summary>
    private int staminaEnhancerCount;

    [Header("ダミーアイテムオブジェクトを格納する(ヒエラルキー上のアイテムをアタッチすること。空のEmptyPrefabも格納すること。)")]
    [SerializeField] private List<GameObject> dammyItemPrefabList;

    /// <summary>
    /// シャッフル予定のアイテムを格納する
    /// </summary>
    private List<GameObject> shuffledItemPrefabList;

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

        for(int i = 0; i < shuffledItemPrefabList.Count; i++)
        {
            //アイテムが存在する場合
            if (shuffledItemPrefabList[i] != null)
            {
                //アイテムをnullに設定
                shuffledItemPrefabList[i] = null;
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
        //documentAndMysteryItemPrefabListのコピーを作成
        shuffledItemPrefabList = new List<GameObject>(documentAndMysteryItemPrefabList);

        //コンパスを全アイテム格納リストに追加
        shuffledItemPrefabList.AddRange(compassPrefabList);

        //難易度に応じてアイテムの格納数を変更する
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus()) 
        {
            //難易度Easyの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //スタミナ増強剤の格納数を難易度Easy用に設定
                staminaEnhancerCount = easyStaminaEnhancerCount;
                break;

            //難易度Normal用の場合(デバッグ用にkNoneも追加)
            case DifficultyLevelController.DifficultyLevel.kNormal:
            case DifficultyLevelController.DifficultyLevel.kNone:

                //スタミナ増強剤の格納数をデフォルト用に設定
                staminaEnhancerCount = defaultStaminaEnhancerCount;
                break;

            //難易度Nightmare用の場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:
                //スタミナ増強剤の格納数を難易度Nightmare用に設定
                staminaEnhancerCount = nightmareStaminaEnhancerCount;
                break; 
        }


        //スタミナ増強剤を指定の数だけ全アイテム格納リストに追加
        for (int i = 0; i < staminaEnhancerCount; i++)
        {
            //スタミナ増強剤を全アイテム格納リストに追加
            shuffledItemPrefabList.AddRange(staminaEnhancerPrefabList);
        }
        

        //引き出しの数がshuffledItemPrefabList数より多い場合
        if (shuffledItemPrefabList.Count < itemPoint.Length)
        {
            //引き出しの数ととshuffledItemPrefabList.Count数の差分数の差分数だけダミーアイテムオブジェクトを全アイテム格納リストに追加
            for (int i = 0; i < itemPoint.Length - shuffledItemPrefabList.Count; i++)
            {
                //ダミーアイテムオブジェクトを全アイテム格納リストに追加
                shuffledItemPrefabList.AddRange(dammyItemPrefabList);
            }
        }

        //allItemListをシャッフル
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
