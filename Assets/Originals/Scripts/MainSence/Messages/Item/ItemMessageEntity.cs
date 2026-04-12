using UnityEngine;

[System.Serializable]
public class ItemMessageEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// アイテムID(参照用)
    /// </summary>
    public int itemId;

    /// <summary>
    /// アイテムプレハブのAddressables名
    /// </summary>
    public string itemPrefabPath;

    /// <summary>
    /// プレイヤーの位置からアイテムを生成したい位置X
    /// </summary>
    public float spawnPositionX;

    /// <summary>
    /// プレイヤーの位置からアイテムを生成したい位置Y
    /// </summary>
    public float spawnPositionY;

    /// <summary>
    /// プレイヤーの位置からアイテムを生成したい位置Z
    /// </summary>
    public float spawnPositionZ;

    /// <summary>
    /// アイテムの回転数値X
    /// </summary>
    public float spawnRotationX;

    /// <summary>
    /// アイテムの回転数値Y
    /// </summary>
    public float spawnRotationY;

    /// <summary>
    /// アイテムの回転数値Z
    /// </summary>
    public float spawnRotationZ;

    /// <summary>
    /// 日本語アイテム名
    /// </summary>
    public string itemNameJapanese;

    /// <summary>
    /// 日本語アイテム名のサイズ
    /// </summary>
    public int itemNameSizeJapanese;

    /// <summary>
    /// 日本語アイテム説明
    /// </summary>
    public string itemDescriptionJapanese;

    /// <summary>
    /// 日本語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizeJapanese;

    /// <summary>
    /// 所持数
    /// </summary>
    public int itemCount;

    /// <summary>
    /// 効果値
    /// </summary>
    public int itemEffectValue;

    /// <summary>
    /// 英語アイテム名
    /// </summary>
    public string itemNameEnglish;

    /// <summary>
    /// 英語アイテム名のサイズ
    /// </summary>
    public int itemNameSizeEnglish;

    /// <summary>
    /// 英語アイテム説明
    /// </summary>
    public string itemDescriptionEnglish;

    /// <summary>
    /// 英語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizeEnglish;

    /// <summary>
    /// メモ
    /// </summary>
    public string memo;
}
