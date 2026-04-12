using System;
using UnityEngine;

/// <summary>
/// アイテムの種類を表す列挙型
/// </summary>
public enum ItemType
{
    /// <summary>
    /// テスト用
    /// </summary>
    Test,

    /// <summary>
    /// 使用アイテム
    /// </summary>
    UseItem,

    /// <summary>
    /// ミステリーアイテム
    /// </summary>
    MysteryItem,

    /// <summary>
    /// ドキュメント
    /// </summary>
    Document,

    /// <summary>
    /// 鍵
    /// </summary>
    Key,

    /// <summary>
    /// コンパス
    /// </summary>
    Compass,

    /// <summary>
    /// プレイヤーライト
    /// </summary>
    PlayerLight,
}


/// <summary>
/// DBで管理したいアイテムの情報一覧
/// </summary>
[System.Serializable]
public class Item : MonoBehaviour
{
    [Header("ボタンテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ItemMessage itemMessage;

    /// <summary>
    /// アイテムのID
    /// </summary>
    [SerializeField] private int id;

    /// <summary>
    /// アイテムのプレハブ
    /// </summary>
    [SerializeField] private GameObject prefab;

    /// <summary>
    /// アイテムのプレハブのAddressables名
    /// </summary>
    private string prefabPath;

    /// <summary>
    /// プレイヤーの位置からアイテムを生成したい位置
    /// </summary>
    private Vector3 spawnPosition;

    /// <summary>
    /// アイテムの回転数値
    /// </summary>
    private Quaternion spawnRotation;

    /// <summary>
    /// アイテムのアイコン画像
    /// </summary>
    [SerializeField] private Sprite icon;

    /// <summary>
    /// アイテムの種類
    /// </summary>
    [SerializeField] private ItemType itemType;

    /// <summary>
    /// アイテムの名前
    /// </summary>
    private string itemName;

    /// <summary>
    /// アイテムの説明
    /// </summary>
    private string description;

    /// <summary>
    /// 所持数
    /// </summary>
    private int count;

    /// <summary>
    /// 効果値
    /// </summary>
    private int effectValue;

    /// <summary>
    /// ID取得
    /// </summary>
    /// <returns>ID</returns>
    public int GetId()
    {
        return id;
    }
    /// <summary>
    /// プレハブを取得
    /// </summary>
    /// <returns>プレハブ</returns>
    public GameObject GetPrefab() 
    {
        return prefab;
    }

    /// <summary>
    /// プレハブのAddressables名を取得
    /// </summary>
    /// <returns>プレハブのAddressables名</returns>
    public string GetPrefabPath() 
    {
        return prefabPath;
    }

    public Vector3 GetSpawnPosition() 
    {
        return spawnPosition;
    }

    /// <summary>
    /// アイテムの回転数値を取得
    /// </summary>
    /// <returns>アイテムの回転数値</returns>
    public Quaternion GetSpawnRotation() 
    {
        return spawnRotation;
    }

    /// <summary>
    /// アイコン画像を取得
    /// </summary>
    /// <returns>アイコン画像</returns>
    public Sprite GetIcon() 
    {
        return icon;
    }

    /// <summary>
    /// アイテムの種類を取得
    /// </summary>
    /// <returns>アイテムの種類</returns>
    public ItemType GetItemType() 
    {
        return itemType;
    }

    /// <summary>
    /// アイテム名を取得
    /// </summary>
    /// <returns>アイテム名</returns>
    public string GetItemName() 
    {
        return itemName;
    }

    /// <summary>
    /// アイテム説明を取得
    /// </summary>
    /// <returns>アイテム説明</returns>
    public string Description() 
    {
        return description;
    }

    /// <summary>
    /// 所持数を取得
    /// </summary>
    /// <returns>所持数</returns>
    public int GetCount() 
    {
        return count;
    }

    /// <summary>
    /// 効果値を取得
    /// </summary>
    /// <returns>効果値</returns>
    public int GetEffectValue() 
    {
        return effectValue;
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    public  void SettingLanguageText() 
    {
        //アイテムのプレハブのAddressables名を設定する
        prefabPath = itemMessage.itemMessage[id].itemPrefabPath;

        //アイテムの生成位置を設定する
        spawnPosition = new Vector3(itemMessage.itemMessage[id].spawnPositionX
            , itemMessage.itemMessage[id].spawnPositionY, itemMessage.itemMessage[id].spawnPositionZ);

        //アイテムの回転数値を設定する
        spawnRotation = Quaternion.Euler(itemMessage.itemMessage[id].spawnRotationX
            , itemMessage.itemMessage[id].spawnRotationY, itemMessage.itemMessage[id].spawnRotationZ);


        //所持数を設定する
        count = itemMessage.itemMessage[id].itemCount;

        //効果値を設定する
        effectValue = itemMessage.itemMessage[id].itemEffectValue;

        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus()) 
        {
            case LanguageController.LanguageStatus.kJapanese:

                //日本語アイテム名を設定する
                itemName = itemMessage.itemMessage[id].itemNameJapanese;

                //日本語アイテム説明を設定する
                description = itemMessage.itemMessage[id].itemDescriptionJapanese;
                break;

            case LanguageController.LanguageStatus.kEnglish:

                //英語アイテムを設定する
                itemName = itemMessage.itemMessage[id].itemNameEnglish;

                //英語アイテム説明名を設定する
                description = itemMessage.itemMessage[id].itemDescriptionEnglish;
                break;
        }
    }
}
