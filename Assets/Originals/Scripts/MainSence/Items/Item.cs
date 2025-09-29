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
}


/// <summary>
/// DBで管理したいアイテムの情報一覧
/// </summary>
[System.Serializable]
public class Item : MonoBehaviour
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

    /// <summary>
    /// ID取得
    /// </summary>
    /// <returns>ID</returns>
    public int GetId()
    {
        return id;
    }
}
