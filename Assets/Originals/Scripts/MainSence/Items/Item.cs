using System;
using UnityEngine;

// アイテムの種類を表す列挙型
public enum ItemType
{
    Test,       // テスト用
    UseItem,    // 使用アイテム
    MysteryItem,// ゴールに必要なアイテム
    Document,   //ドキュメント
    Key         //鍵
}


//DBで管理したいアイテムの情報一覧
[System.Serializable]
public class Item : MonoBehaviour
{
    public int id;             // アイテムのID
    public GameObject prefab;  // アイテムのプレハブ
    [TextArea]
    public string prefabPath;　//アイテムのプレハブのAddressables名
    public Vector3 spawnPosition; //プレイヤーの位置からアイテムを生成したい位置
    public Quaternion spawnRotation; //アイテムの回転数値
    public Sprite icon;        // アイテムのアイコン画像
    public ItemType itemType;  // アイテムの種類
    public string itemName;    // アイテムの名前
    [TextArea]
    public string description; // アイテムの説明
    public int count;          // 所持数
    public int effectValue;    // 効果値


    public int GetId()
    {
        return id;
    }

    
}
