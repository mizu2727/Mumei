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
    /// メモ欄
    /// </summary>
    public string memo;
}
