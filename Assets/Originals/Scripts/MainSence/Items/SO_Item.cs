using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static SO_Item;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using static UnityEditor.Progress;
#endif


/// <summary>
/// アイテムDB
/// </summary>
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    /// <summary>
    /// Itemのデータを保存するためのクラス
    /// セーブデータ保存時にも保存できる？
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        /// <summary>
        /// アイテムのID
        /// </summary>
        public int id;

        /// <summary>
        /// アイテムのプレハブ
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// アイテムのプレハブのAddressables名
        /// </summary>
        [TextArea]
        public string prefabPath;

        /// <summary>
        /// プレイヤーの位置からアイテムを生成したい位置
        /// </summary>
        public Vector3 spawnPosition;

        /// <summary>
        /// アイテムの回転数値
        /// </summary>
        public Quaternion spawnRotation;

        /// <summary>
        /// アイテムのアイコン画像
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// アイテムの種類
        /// </summary>
        public ItemType itemType;

        /// <summary>
        /// アイテムの名前
        /// </summary>
        public string itemName;

        /// <summary>
        /// アイテムの説明
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// 所持数
        /// </summary>
        public int count;

        /// <summary>
        /// 効果値
        /// </summary>
        public int effectValue;

        public ItemData(Item item)
        {
            /// <summary>
            /// アイテムのID
            /// </summary>
            id = item.id;

            /// <summary>
            /// アイテムのプレハブ
            /// </summary>
            prefab = item.prefab;

            /// <summary>
            /// アイテムのプレハブのAddressables名
            /// </summary>
            prefabPath = item.prefabPath;

            /// <summary>
            /// プレイヤーの位置からアイテムを生成したい位置
            /// </summary>
            spawnPosition = item.spawnPosition;

            /// <summary>
            /// アイテムの回転数値
            /// </summary>
            spawnRotation = item.spawnRotation;

            /// <summary>
            /// アイテムのアイコン画像
            /// </summary>
            icon = item.icon;

            /// <summary>
            /// アイテムの種類
            /// </summary>
            itemType = item.itemType;

            /// <summary>
            /// アイテムの名前
            /// </summary>
            itemName = item.itemName;

            /// <summary>
            /// アイテムの説明
            /// </summary>
            description = item.description;

            /// <summary>
            /// 所持数
            /// </summary>
            count = item.count;

            /// <summary>
            /// 効果値
            /// </summary>
            effectValue = item.effectValue;
        }
    }

    /// <summary>
    /// アイテムリスト
    /// </summary>
    public List<ItemData> itemList = new List<ItemData>();

    /// <summary>
    /// アイテムリストを返す
    /// </summary>
    /// <returns>アイテムリスト</returns>
    public List<ItemData> GetItemLists()
    {
        return itemList;
    }
 
    /// <summary>
    /// 保存しているアイテムを全て初期化する
    /// </summary>
    public void ResetItems()
    {
        itemList.Clear();
    }


    /// <summary>
    /// idでアイテムを検索するメソッド
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemById(int id)
    {
        return itemList.Find(item => item.id == id);
    }

    /// <summary>
    /// itemTypeでアイテムを検索するメソッド
    /// </summary>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public bool GetItemByType(ItemType targetType)
    {
        if (itemList == null)
        {
            Debug.LogError("itemList is null");
            return false;
        }

        //nullなアイテムを除外してチェック
        bool result = itemList.Exists(item => item != null && item.itemType == targetType);
        return result;
    }

    


    /// <summary>
    /// 使用アイテム追加
    /// </summary>
    /// <param name="newItem"></param>
    public void AddUseItem(Item newItem)
    {
        
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //アイテム新規追加
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            //インベントリに新規追加
            Inventory.instance.GetItem(itemData.id, itemData.prefabPath, itemData.spawnPosition, itemData.spawnRotation, 
                itemData.icon, itemData.itemName, itemData.description, itemData.count, itemData.effectValue);
        }
        else 
        {
            //既存アイテムの数を追加更新
            var updateItem = itemList.Find(item => item.id == newItem.id);
            updateItem.count += newItem.count;

            //インベントリに追加
            Inventory.instance.GetItem(updateItem.id, updateItem.prefabPath, updateItem.spawnPosition, updateItem.spawnRotation, 
                updateItem.icon, updateItem.itemName, updateItem.description, updateItem.count, updateItem.effectValue);
        }
    }

    /// <summary>
    /// 使用アイテムを削除する
    /// </summary>
    /// <param name="id">アイテムid</param>
    /// <param name="count">アイテムの個数</param>
    public void ReduceUseItem(int id, int count) 
    {
        if (itemList.Exists(item => item.id == id)) 
        {
            //既存アイテムの数を減少更新
            var updateItem = itemList.Find(item => item.id == id);
            updateItem.count = count;
        }
    }

    /// <summary>
    /// ドキュメント・ミステリーアイテム追加
    /// </summary>
    /// <param name="newItem">入手したアイテム</param>
    public void AddDocumentORMysteryItem(Item newItem)
    {
        //nullチェック
        if (newItem == null || newItem.gameObject == null)
        {
            Debug.LogWarning("AddDocument に null な item が渡されました！");
            return;
        }

        if (!itemList.Exists(item => item != null && item.id == newItem.id))
        {
            //アイテムを新規追加
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            if (itemData.itemType == ItemType.Document)
            {
                //ドキュメント追加
                PauseController.instance.ChangeDocumentNameText(itemData.id, itemData.itemName);
                PauseController.instance.ChangeDocumentExplanationText(itemData.description);
            }
            else
            {
                //ミステリーアイテム追加
                PauseController.instance.ChangeMysteryItemTexts(itemData.id, itemData.itemName ,itemData.description);

            }
        }
        else
        {
            Debug.Log("同じidのアイテムはすでに追加済み");
        }
    }

    /// <summary>
    /// nullアイテムを削除
    /// </summary>
    public void CleanNullItems()
    {
        int before = itemList.Count;
        itemList.RemoveAll(item => item == null);
    }
}

