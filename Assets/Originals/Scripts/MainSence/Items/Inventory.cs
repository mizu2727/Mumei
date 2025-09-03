using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static SO_Item;


public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("使用アイテムパネル関連(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject useItemPanel;//使用アイテム確認パネル
    [SerializeField] private Text useItemCountText;//使用アイテム所持カウントテキスト
    [SerializeField] private Image useItemImage;//使用アイテム画像


    //アイテムID管理(アイテムインベントリ複数枠分用)
    private readonly List<int> idList = new();

    [Header("アイテムID管理(アイテムインベントリ1枠分用。編集禁止)")]
    public int keepItemId;

    //アイテム未所持時のID(アイテムインベントリ1枠分用)
    private const int noneItemId = 99999;

    //アイテムのプレハブのAddressables名
    private string keepItemPrefabPath;

    //アイテムのプレハブのAddressables名(空白)
    private const string noneItemPrefabPath = "";

    //アイテム生成位置
    private Vector3 keepItemSpawnPosition;

    //アイテム生成位置(デフォルト)
    private Vector3 defaultItemSpawnPosition = new Vector3(0, 0, 0);

    //アイテムの回転数値
    private Quaternion keepItemSpawnRotation;

    //アイテムの回転数値(デフォルト)
    private Quaternion defaultItemSpawnRotation = Quaternion.identity;

    //アイテム所持数管理(アイテムインベントリ複数枠分用)
    private readonly List<int> countList = new();

    //アイテム所持数管理(アイテムインベントリ1枠分用)
    private int keepItemCount;

    private const int minKeepItemCount = 0;

    //アイテムリストのインデックス番号
    int checkIndex;




    private Player player;

    private void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時に破棄されないようにする（必要に応じて）
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は破棄
        }
    }

    void Start()
    {
        player = GetComponent<Player>();
        ResetInventoryItem();
    }

    void Update()
    {
        if (UseInventoryItem()) UseItem();
    }

    /// <summary>
    /// 右クリックでインベントリアイテム使用する関数
    /// </summary>
    /// <returns>true / false</returns>
    bool UseInventoryItem()
    {
        return Input.GetMouseButtonDown(1);
    }

    /// <summary>
    /// インベントリにアイテムを追加
    /// </summary>
    /// <param name="id">アイテムid</param>
    /// <param name="path">アイテムのパス</param>
    /// <param name="position">アイテムの生成位置</param>
    /// <param name="rotation">アイテムの回転</param>
    /// <param name="icon">アイテムの画像</param>
    /// <param name="itemName">アイテム名</param>
    /// <param name="count">アイテム個数</param>
    public void GetItem(int id, string path, Vector3 position, Quaternion rotation, Sprite icon, string itemName, int count)
    {
        //リストの中にアイテムが何番目に存在するのかを確認
        //存在しない場合は-1を返す
        //checkIndex = idList.IndexOf(id);

        //インベントリに新規追加する処理
        //checkIndex == -1
        if (keepItemId == noneItemId)
        {

            //　アイテムidを設定
            idList.Add(id);

            keepItemId = id;
            keepItemPrefabPath = path;
            keepItemSpawnPosition = position;
            keepItemSpawnRotation = rotation;
            useItemImage.sprite = icon;
            useItemImage.color = new Color(255, 255, 255, 1);

            //　アイテム所持数を設定
            countList.Add(count);
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            Debug.Log("id:" + id + "のアイテムitemを" + count + "個新規追加");
            Debug.Log("keepItemCount(使用アイテム):" + keepItemCount);
            
        }
        else
        {
            //アイテム所持数を追加
            countList[checkIndex] += count;
            keepItemCount = count;
            useItemCountText.text = count.ToString();


            Debug.Log("id:" + id + "のアイテムitemを" + count + "個へ増加");

            Debug.Log("keepItemCount(使用アイテム):" + keepItemCount);
        }
    }

    /// <summary>
    /// アイテムを使用する
    /// </summary>
    public void UseItem() 
    {
        if (minKeepItemCount < keepItemCount)
        {
            Debug.Log("インベントリアイテムitemを使用");
            --keepItemCount;
            useItemCountText.text = keepItemCount.ToString();

            ActivationUseItem(keepItemId);

            sO_Item.ReduceUseItem(keepItemId, keepItemCount);
            

            Debug.Log("id:" + keepItemId + "のkeepItemCount(使用アイテム(減少)):" + keepItemCount);

            if (keepItemCount == minKeepItemCount)
            {
                ResetInventoryItem();
            }
        }
        else 
        {
            Debug.Log("使用できるアイテムitemがないようだ");
        }
    }

    /// <summary>
    /// 使用したアイテムのIDによって、それぞれの処理を行う
    /// </summary>
    /// <param name="keepItemId">アイテムID</param>
    void ActivationUseItem(int keepItemId) 
    {
        //テスト用使用アイテム①
        if (keepItemId == 995) 
        {
            // ローカル座標をワールド座標に変換
            Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
            Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

            // Addressablesを使用してプレハブをステージ上に非同期生成
            Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
        }

    }

    /// <summary>
    /// インベントリをリセットする
    /// </summary>
    void ResetInventoryItem() 
    {
        keepItemId = noneItemId;
        keepItemCount = minKeepItemCount;
        useItemCountText.text = keepItemCount.ToString();
        keepItemPrefabPath = noneItemPrefabPath;
        keepItemSpawnPosition = defaultItemSpawnPosition;
        keepItemSpawnRotation = defaultItemSpawnRotation;
        useItemImage.sprite = null;
        useItemImage.color = new Color(255, 255, 255, 0.05f);
    }
}
