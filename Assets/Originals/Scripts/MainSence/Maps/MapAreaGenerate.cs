
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static LanguageController;
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


    /// <summary>
    /// Item.cs
    /// </summary>
    private Item item;

    [Header("ドキュメントとミステリーアイテムを格納する(Prefabをアタッチすること。)")]
    [SerializeField] private List<GameObject> documentAndMysteryItemPrefabList;

    [Header("コンパスを格納する(Prefabをアタッチすること。)")]
    [SerializeField] private List<GameObject> compassPrefabList;

    [Header("スタミナ増強剤を格納する(Prefabをアタッチすること。)")]
    [SerializeField] private List<GameObject> staminaEnhancerPrefabList;

    [Header("難易度Easyのスタミナ増強剤格納数")]
    [SerializeField] private int easyStaminaEnhancerCount;

    [Header("難易度Normalのスタミナ増強剤格納数")]
    [SerializeField] private int normalStaminaEnhancerCount;

    [Header("難易度Nightmareのスタミナ増強剤格納数")]
    [SerializeField] private int nightmareStaminaEnhancerCount;

    /// <summary>
    /// スタミナ増強剤格納数
    /// </summary>
    private int staminaEnhancerCount;

    [Header("クラッカーを格納する(Prefabをアタッチすること。)")]
    [SerializeField] private List<GameObject> crackerPrefabList;

    [Header("難易度Easyのスタミナクラッカー格納数")]
    [SerializeField] private int easyCrackerCount;

    [Header("難易度Normalのスタミナクラッカー格納数")]
    [SerializeField] private int normalCrackerCount;

    [Header("難易度Nightmareのクラッカー格納数")]
    [SerializeField] private int nightmareCrackerCount;

    /// <summary>
    /// クラッカー格納数
    /// </summary>
    private int crackerCount;

    [Header("ダミーアイテムオブジェクトを格納する(Prefabをアタッチすること。)")]
    [SerializeField] private List<GameObject> dammyItemPrefabList;

    /// <summary>
    /// シャッフル予定のアイテムを格納する
    /// </summary>
    private List<GameObject> shuffledItemPrefabList;

    [Header("ランダムに選ばれたアイテムが格納される(ヒエラルキー上からのアタッチ禁止。シャッフルされたアイテム確認用)")]
    [SerializeField] private List<GameObject> useItemList = new();

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


    [Header("難易度Easyでの敵の位置(ヒエラルキー上からアタッチすること。ステージライトとの距離測定で必要)")]
    [SerializeField] private Transform[] easyBaseEnemyTransformArray;

    [Header("難易度Normalでの敵の位置(ヒエラルキー上からアタッチすること。ステージライトとの距離測定で必要)")]
    [SerializeField] private Transform[] normalBaseEnemyTransformArray;

    [Header("難易度Nightmareでの敵の位置(ヒエラルキー上からアタッチすること。ステージライトとの距離測定で必要)")]
    [SerializeField] private Transform[] nightmareBaseEnemyTransformArray;

    [Header("敵の位置(ヒエラルキー上からのアタッチ禁止。格納結果確認用。ステージライトとの距離測定で必要)")]
    [SerializeField] private Transform[] baseEnemyTransformArray;


    [Header("Easy用の隠れる地点のTransform配列(ヒエラルキー上のHiddeObjectをアタッチすること)")]
    [SerializeField] private Transform[] easyHideObject;

    [Header("Normal用の隠れる地点のTransform配列(ヒエラルキー上のHiddeObjectをアタッチすること)")]
    [SerializeField] private Transform[] normalHideObject;


    /// <summary>
    /// 敵の位置の配列を取得
    /// </summary>
    /// <returns>敵の位置の配列</returns>
    public Transform[] GetBaseEnemyTransformArray() 
    {
        return baseEnemyTransformArray;
    }

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

        //難易度に応じてオブジェクトを表示/非表示するメソッド
        SettingViewObjects();

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

        //クラッカーを全アイテム格納リストに追加
        shuffledItemPrefabList.AddRange(crackerPrefabList);

        //難易度に応じてアイテムの格納数を変更する
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus()) 
        {
            //難易度Easyの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //スタミナ増強剤の格納数を難易度Easy用に設定
                staminaEnhancerCount = easyStaminaEnhancerCount;

                //クラッカーの格納数を難易度Easy用に設定
                crackerCount = easyCrackerCount;
                break;

            //難易度Normal用の場合(デバッグ用にkNoneも追加)
            case DifficultyLevelController.DifficultyLevel.kNormal:
            case DifficultyLevelController.DifficultyLevel.kNone:

                //スタミナ増強剤の格納数をデフォルト用に設定
                staminaEnhancerCount = normalStaminaEnhancerCount;

                //クラッカーの格納数を難易度Normal用に設定
                crackerCount = normalCrackerCount;
                break;

            //難易度Nightmare用の場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //スタミナ増強剤の格納数を難易度Nightmare用に設定
                staminaEnhancerCount = nightmareStaminaEnhancerCount;

                //クラッカーの格納数を難易度Nightmare用に設定
                crackerCount = nightmareCrackerCount;
                break; 
        }


        //スタミナ増強剤を指定の数だけ全アイテム格納リストに追加
        for (int i = 0; i < staminaEnhancerCount; i++)
        {
            //スタミナ増強剤を全アイテム格納リストに追加
            shuffledItemPrefabList.AddRange(staminaEnhancerPrefabList);
        }

        //クラッカーを指定の数だけ全アイテム格納リストに追加
        for (int i = 0; i < crackerCount; i++)
        {
            //スタミナ増強剤を全アイテム格納リストに追加
            shuffledItemPrefabList.AddRange(crackerPrefabList);
        }

        for (int i = 0; i < shuffledItemPrefabList.Count; i++)
        {
            //アイテムのテキスト関連を設定する
            shuffledItemPrefabList[i] = SettingLanguageText(shuffledItemPrefabList[i]);
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
    /// 言語を設定する
    /// </summary>
    /// <param name="targetItem">アイテム</param>
    private GameObject SettingLanguageText(GameObject targetItem)
    {
        //Itemコンポーネントを取得
        item = targetItem.GetComponent<Item>();

        //ItemPrefabのテキスト関連を設定する
        item.SettingLanguageText();

        targetItem = item.gameObject;

        return targetItem;
    }

    /// <summary>
    /// 引き出し内のアイテムの言語を変更するメソッド
    /// </summary>
    public void ChangeLanguageText() 
    {
        for (int i = 0; i < useItemList.Count; i++) 
        {
            //Drawerコンポーネントを取得
            Drawer drawer = itemPoint[i].GetComponent<Drawer>();

            //引き出し内のアイテムのテキスト関連を設定する
            drawer.SettingLanguageText();
        }
    }

    /// <summary>
    /// 難易度に応じてオブジェクトを表示/非表示するメソッド
    /// </summary>
    void SettingViewObjects() 
    {
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus()) 
        {
            //Easyの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //難易度Easy用の隠れる地点を表示
                SettingEasyHideObject(true);

                //難易度Normal用の隠れる地点を表示
                SettingNormalHideObject(true);

                //難易度Easy用の敵の位置が設定されている場合
                if (easyBaseEnemyTransformArray.Length != 0) 
                {
                    //難易度Easy用に敵をを表示
                    for (int i = 0; i < easyBaseEnemyTransformArray.Length; i++)
                    {
                        easyBaseEnemyTransformArray[i].gameObject.SetActive(true);
                    }

                    //難易度Easy用の敵の位置を設定
                    baseEnemyTransformArray = easyBaseEnemyTransformArray;
                }

                

                break;

            //Normalの場合
            case DifficultyLevelController.DifficultyLevel.kNormal:

                //難易度Easy用の隠れる地点を非表示にする
                SettingEasyHideObject(false);

                //難易度Normal用の隠れる地点を表示
                SettingNormalHideObject(true);


                //難易度Normal用の敵の位置が設定されている場合
                if (normalBaseEnemyTransformArray.Length != 0)
                {
                    //難易度Normal用に敵をを表示
                    for (int i = 0; i < normalBaseEnemyTransformArray.Length; i++)
                    {
                        normalBaseEnemyTransformArray[i].gameObject.SetActive(true);
                    }

                    //難易度Normal用の敵の位置を設定
                    baseEnemyTransformArray = normalBaseEnemyTransformArray;
                }

                break;

            //Nightmareの場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //難易度Easy用の隠れる地点を非表示にする
                SettingEasyHideObject(false);

                //難易度Normal用の隠れる地点を非表示にする
                SettingNormalHideObject(false);


                //難易度Nightmare用の敵の位置が設定されている場合
                if (nightmareBaseEnemyTransformArray.Length != 0)
                {
                    //難易度Nightmare用に敵をを表示
                    for (int i = 0; i < nightmareBaseEnemyTransformArray.Length; i++)
                    {
                        nightmareBaseEnemyTransformArray[i].gameObject.SetActive(true);
                    }

                    //難易度Nightmare用の敵の位置を設定
                    baseEnemyTransformArray = nightmareBaseEnemyTransformArray;
                }

                break;


            default:
                Debug.Log("その他の難易度(デバッグ用)");
                
                //難易度Easy用の敵の位置が設定されている場合
                if (easyBaseEnemyTransformArray.Length != 0)
                {
                    //難易度Easy用に敵をを表示
                    for (int i = 0; i < easyBaseEnemyTransformArray.Length; i++)
                    {
                        easyBaseEnemyTransformArray[i].gameObject.SetActive(true);
                    }

                    //難易度Easy用の敵の位置を設定
                    baseEnemyTransformArray = easyBaseEnemyTransformArray;
                }
                

                /*
                //難易度Normal用の敵の位置が設定されている場合
                if (normalBaseEnemyTransformArray.Length != 0)
                {
                    //難易度Normal用に敵をを表示
                    for (int i = 0; i < normalBaseEnemyTransformArray.Length; i++)
                    {
                        normalBaseEnemyTransformArray[i].gameObject.SetActive(true);
                    }

                    //難易度Normal用の敵の位置を設定
                    baseEnemyTransformArray = normalBaseEnemyTransformArray;
                }
                */

                

                break;
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

    /// <summary>
    /// 難易度Easy用のHideObjectを表示/非表示するメソッド
    /// </summary>
    /// <param name="targetEnable">表示/非表示</param>
    private void SettingEasyHideObject(bool targetEnable) 
    {
        //Easy用の隠れる地点が設定されている場合
        if (easyHideObject.Length != 0)
        {
            //難易度Easy用の隠れる地点を表示/非表示
            for (int i = 0; i < easyHideObject.Length; i++)
            {
                easyHideObject[i].gameObject.SetActive(targetEnable);
            }
        }
        else
        {
            Debug.LogWarning("難易度Easy用の隠れる地点が設定されていません。");
        }
    }

    /// <summary>
    /// 難易度Normal用のHideObjectを表示/非表示するメソッド
    /// </summary>
    /// <param name="targetEnable">表示/非表示</param>
    private void SettingNormalHideObject(bool targetEnable)
    {
        //Normal用の隠れる地点が設定されている場合
        if (normalHideObject.Length != 0)
        {
            //難易度Normal用の隠れる地点を表示/非表示
            for (int i = 0; i < normalHideObject.Length; i++)
            {
                normalHideObject[i].gameObject.SetActive(targetEnable);
            }
        }
        else
        {
            Debug.LogWarning("難易度Normal用の隠れる地点が設定されていません。");
        }
    }
}
