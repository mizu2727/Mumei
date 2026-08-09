using UnityEngine;

[System.Serializable]
public class UITextMessageEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// メッセージ番号
    /// </summary>
    public int number;

    /// <summary>
    /// 日本語メッセージ内容
    /// </summary>
    public string messageJapanese;

    /// <summary>
    /// 日本語メッセージサイズ
    /// </summary>
    public int messageSizeJapanese;

    /// <summary>
    /// 英語メッセージ内容
    /// </summary>
    public string messageEnglish;

    /// <summary>
    /// 英語メッセージサイズ
    /// </summary>
    public int messageSizeEnglish;

    /// <summary>
    /// 簡体字中国語メッセージ内容
    /// </summary>
    public string messageChinese01;

    /// <summary>
    /// 簡体字中国語メッセージサイズ
    /// </summary>
    public int messageSizeChinese01;

    /// <summary>
    /// 繁体字中国語メッセージ内容
    /// </summary>
    public string messageChinese02;

    /// <summary>
    /// 繁体字中国語メッセージサイズ
    /// </summary>
    public int messageSizeChinese02;

    /// <summary>
    /// スペイン語メッセージ内容
    /// </summary>
    public string messageSpanish;

    /// <summary>
    /// スペイン語メッセージサイズ
    /// </summary>
    public int messageSizeSpanish;

    /// <summary>
    /// ポルトガル語メッセージ内容
    /// </summary>
    public string messagePortuguese;

    /// <summary>
    /// ポルトガル語メッセージサイズ
    /// </summary>
    public int messageSizePortuguese;

    /// <summary>
    /// メモ欄
    /// </summary>
    public string memo;
}
