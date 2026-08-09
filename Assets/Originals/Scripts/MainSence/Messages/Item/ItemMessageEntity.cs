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
    /// 簡体字中国語アイテム名
    /// </summary>
    public string itemNameChinese01;

    /// <summary>
    /// 簡体字中国語アイテム名のサイズ
    /// </summary>
    public int itemNameSizeChinese01;

    /// <summary>
    /// 簡体字中国語アイテム説明
    /// </summary>
    public string itemDescriptionChinese01;

    /// <summary>
    /// 簡体字中国語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizeChinese01;

    /// <summary>
    /// 繁体字中国語アイテム名
    /// </summary>
    public string itemNameChinese02;

    /// <summary>
    /// 繁体字中国語アイテム名のサイズ
    /// </summary>
    public int itemNameSizeChinese02;

    /// <summary>
    /// 繁体字中国語アイテム説明
    /// </summary>
    public string itemDescriptionChinese02;

    /// <summary>
    /// 繁体字中国語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizeChinese02;

    /// <summary>
    /// スペイン語アイテム名
    /// </summary>
    public string itemNameSpanish;

    /// <summary>
    /// スペイン語アイテム名のサイズ
    /// </summary>
    public int itemNameSizeSpanish;

    /// <summary>
    /// スペイン語アイテム説明
    /// </summary>
    public string itemDescriptionSpanish;

    /// <summary>
    /// スペイン語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizeSpanish;

    /// <summary>
    /// ポルトガル語アイテム名
    /// </summary>
    public string itemNamePortuguese;

    /// <summary>
    /// ポルトガル語アイテム名のサイズ
    /// </summary>
    public int itemNameSizePortuguese;

    /// <summary>
    /// ポルトガル語アイテム説明
    /// </summary>
    public string itemDescriptionPortuguese;

    /// <summary>
    /// ポルトガル語アイテム説明のサイズ
    /// </summary>
    public int itemDescriptionSizePortuguese;

    /// <summary>
    /// メモ
    /// </summary>
    public string memo;
}
