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
        /// 彷徨う者の情報アイテムID
        /// </summary>
        public int enemyInformationMessageId;

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
            id = item.GetId();

            /// <summary>
            /// アイテムのプレハブ
            /// </summary>
            prefab = item.GetPrefab();

            /// <summary>
            /// アイテムのプレハブのAddressables名
            /// </summary>
            prefabPath = item.GetPrefabPath();

            /// <summary>
            /// プレイヤーの位置からアイテムを生成したい位置
            /// </summary>
            spawnPosition = item.GetSpawnPosition();

            /// <summary>
            /// アイテムの回転数値
            /// </summary>
            spawnRotation = item.GetSpawnRotation();

            /// <summary>
            /// アイテムのアイコン画像
            /// </summary>
            icon = item.GetIcon();

            /// <summary>
            /// アイテムの種類
            /// </summary>
            itemType = item.GetItemType();

            /// <summary>
            /// 彷徨う者の情報アイテムID
            /// </summary>
            enemyInformationMessageId = item.GetEnemyInformationMessageId();

            /// <summary>
            /// アイテムの名前
            /// </summary>
            itemName = item.GetItemName();

            /// <summary>
            /// アイテムの説明
            /// </summary>
            description = item.Description();

            /// <summary>
            /// 所持数
            /// </summary>
            count = item.GetCount();

            /// <summary>
            /// 効果値
            /// </summary>
            effectValue = item.GetEffectValue();
        }
    }

    /// <summary>
    /// アイテムリスト
    /// </summary>
    public List<ItemData> itemList = new List<ItemData>();

    /// <summary>
    /// アイテムリストを取得する
    /// </summary>
    /// <returns>アイテムリスト</returns>
    public List<ItemData> GetItemLists()
    {
        return itemList;
    }


    /// <summary>
    /// 永続的に保存する彷徨う者の情報リスト
    /// </summary>
    public List<ItemData> enemyInformationList = new List<ItemData>();

    /// <summary>
    /// 永続的に保存する彷徨う者の情報リストを取得する
    /// </summary>
    /// <returns>永続的に保存する彷徨う者の情報リスト</returns>
    public List<ItemData> GetEnemyInformationList()
    {
        return enemyInformationList;
    }


    /// <summary>
    /// 保存しているアイテムを全て初期化する
    /// </summary>
    public void ResetItems()
    {
        itemList.Clear();
    }


    /// <summary>
    /// 永続的に保存している彷徨う者の情報リストを全て初期化する
    /// </summary>
    public void ResetEnemyInformationList()
    {
        enemyInformationList.Clear();
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
    /// アイテム名をidで検索して変更するメソッド
    /// </summary>
    /// <param name="id">アイテムID</param>
    /// <param name="newName">アイテム名</param>
    public void SetItemName(int id, string newName) 
    {
        if (itemList.Exists(item => item.id == id)) 
        {
            ItemData updateItem = itemList.Find(item => item.id == id);
            updateItem.itemName = newName;
        }
    }

    /// <summary>
    /// 彷徨う者の情報名をidで検索して変更するメソッド
    /// </summary>
    /// <param name="id">彷徨う者の情報ID</param>
    /// <param name="newName">彷徨う者の情報名</param>
    public void SetEnemyInformationItemName(int id, string newName)
    {
        if (enemyInformationList.Exists(item => item.id == id))
        {
            ItemData updateEnemyInformation = enemyInformationList.Find(item => item.id == id);
            updateEnemyInformation.itemName = newName;
        }
    }


    /// <summary>
    /// アイテム説明をidで検索して変更するメソッド
    /// </summary>
    /// <param name="id">アイテムID</param>
    /// <param name="newDescription">アイテム説明</param>
    public void SetItemDescription(int id, string newDescription) 
    {
        if (itemList.Exists(item => item.id == id)) 
        {
            ItemData updateItem = itemList.Find(item => item.id == id);
            updateItem.description = newDescription;
        }
    }

    /// <summary>
    /// 彷徨う者の情報の説明をidで検索して変更するメソッド
    /// </summary>
    /// <param name="id">彷徨う者の情報ID</param>
    /// <param name="newDescription">彷徨う者の情報の説明</param>
    public void SetEnemyInformationItemDescription(int id, string newDescription)
    {
        if (enemyInformationList.Exists(item => item.id == id))
        {
            ItemData updateEnemyInformation = enemyInformationList.Find(item => item.id == id);
            updateEnemyInformation.description = newDescription;
        }
    }


    /// <summary>
    /// 使用アイテム追加
    /// </summary>
    /// <param name="newItem"></param>
    public void AddUseItem(Item newItem)
    {
        
        if (!itemList.Exists(item => item.id == newItem.GetId()))
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
            ItemData updateItem = itemList.Find(item => item.id == newItem.GetId());
            updateItem.count += newItem.GetCount();

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
            ItemData updateItem = itemList.Find(item => item.id == id);
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
            Debug.LogWarning("ドキュメント・ミステリーアイテム追加処理でnullitemが渡された。");
            return;
        }

        if (!itemList.Exists(item => item != null && item.id == newItem.GetId()))
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
            Debug.Log("ドキュメント・ミステリーアイテム追加処理で同じidのアイテムはすでに追加済み");
        }
    }

    /// <summary>
    /// 彷徨う者の情報追加
    /// </summary>
    /// <param name="newEnemyInformation">入手した彷徨う者の情報</param>
    public void AddEnemyInformationItem(Item newEnemyInformation)
    {
        //nullチェック
        if (newEnemyInformation == null || newEnemyInformation.gameObject == null)
        {
            Debug.LogWarning("彷徨う者の情報追加処理でnullitemが渡された。");
            return;
        }

        if (!enemyInformationList.Exists(item => item != null && item.id == newEnemyInformation.GetId()))
        {
            //リストに新規追加
            ItemData newEnemyInformationData = new ItemData(newEnemyInformation);
            enemyInformationList.Add(newEnemyInformationData);

            if (newEnemyInformationData.itemType == ItemType.EnemyInforｍation)
            {
                //TODO:彷徨う者の情報追加
            }
        }
        else
        {
            Debug.Log("彷徨う者の情報追加処理で同じidのアイテムはすでに追加済み");
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

