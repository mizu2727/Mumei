using UnityEngine;

[System.Serializable]
public class GoalMessageEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// メッセージ番号
    /// </summary>
    public int number;

    /// <summary>
    /// 日本語メッセージ内容
    /// </summary>
    public string message;

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
}
