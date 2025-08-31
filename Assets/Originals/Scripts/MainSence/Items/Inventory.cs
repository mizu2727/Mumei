//廃止→再利用予定
using System.Collections.Generic;
using UnityEngine;
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
        keepItemId = noneItemId;
        keepItemCount = minKeepItemCount;
        useItemCountText.text = keepItemCount.ToString();
        useItemImage.sprite = null;
        useItemImage.color = new Color(255, 255, 255, 0.05f);
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
    /// インベントリ内にアイテムを追加
    /// </summary>
    /// <param name="id">アイテムid</param>
    /// <param name="icon">アイテムの画像</param>
    /// <param name="itemName">アイテムの名前</param>
    /// <param name="count">アイテムの個数</param>
    public void GetItem(int id, Sprite icon, string itemName, int count)
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

            sO_Item.ReduceUseItem(keepItemId, keepItemCount);

            Debug.Log("id:" + keepItemId + "のkeepItemCount(使用アイテム(減少)):" + keepItemCount);

            if (keepItemCount == minKeepItemCount)
            {
                keepItemId = noneItemId;
                useItemImage.sprite = null;
                useItemImage.color = new Color(255, 255, 255, 0.05f);
            }
        }
        else 
        {
            Debug.Log("使用できるアイテムitemがないようだ");
        }
        
    }
}
