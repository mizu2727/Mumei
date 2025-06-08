using UnityEngine;
using System.Collections.Generic;


//アイテムDB
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    
    //Itemのデータを保存するためのクラス
    //セーブデータ保存時にも保存できる？
    [System.Serializable]
    public class ItemData
    {
        public int id;             // アイテムのID
        public GameObject prefab;  // アイテムのプレハブ
        public Sprite icon;        // アイテムのアイコン画像
        public ItemType itemType;  // アイテムの種類
        public string itemName;    // アイテムの名前
        [TextArea]
        public string description; // アイテムの説明
        public int count;          // 所持数
        public int effectValue;    // 効果値

        public ItemData(Item item)
        {
            id = item.id;
            prefab = item.prefab;
            icon = item.icon;
            itemType = item.itemType;
            itemName = item.itemName;
            description = item.description;
            count = item.count;
            effectValue = item.effectValue;
        }
    }

    public List<ItemData> itemList = new List<ItemData>();

    //　アイテムリストを返す
    public List<ItemData> GetItemLists()
    {
        return itemList;
    }


    // 保存しているアイテムを全て初期化する
    public void ResetItems()
    {
        itemList.Clear();
    }


    // idでアイテムを検索するメソッド
    public ItemData GetItemById(int id)
    {
        return itemList.Find(item => item.id == id);
    }

    //itemTypeでアイテムを検索するメソッド
    public bool GetItemByType(ItemType targetType)
    {
        if (itemList == null)
        {
            Debug.LogError("itemList is null");
            return false;
        }

        // null なアイテムを除外してチェック
        bool result = 
            itemList.Exists(item => item != null && item.itemType == targetType);
        return result;
    }

    


    //アイテム追加
    public void AddItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //アイテム新規追加
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);
            Debug.Log($"アイテム {newItem.id} を+ {newItem.count}新規追加");
        }
        else 
        {
            // 既存アイテムの数を追加更新
            var updateItem = itemList.Find(item => item.id == newItem.id);
            updateItem.count += newItem.count;
            Debug.Log($"アイテム {updateItem.id} の数を追加更新。所持数： {updateItem.count}");
        }
    }

    //ドキュメント・ミステリーアイテム追加
    public void AddDocumentORMysteryItem(Item newItem)
    {
        if (newItem == null || newItem.gameObject == null)
        {
            Debug.LogWarning("AddDocument に null な item が渡されました！");
            return;
        }

        Debug.Log($"追加しようとしているアイテム: {newItem.id}, 現在のitemList数: {itemList.Count}");
        if (!itemList.Exists(item => item != null && item.id == newItem.id))
        {
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            if (itemData.itemType == ItemType.Document)
            {
                PauseController.instance.ChangeDocumentNameText(itemData.itemName);
                PauseController.instance.ChangeDocumentExplanationText(itemData.description);
            }
            else 
            {
                PauseController.instance.ChangeMysteryItemTexts(itemData.itemName ,itemData.description);

            }
            Debug.Log($"アイテムを追加: {itemData.id}, 新しいitemList数: {itemList.Count}");
        }
        else
        {
            Debug.Log("同じidのアイテムはすでに追加済み");
        }
    }

    

    //nullアイテムを削除
    public void CleanNullItems()
    {
        int before = itemList.Count;
        itemList.RemoveAll(item => item == null);
        Debug.Log($"null を削除しました: {before - itemList.Count} 件");
    }
}

