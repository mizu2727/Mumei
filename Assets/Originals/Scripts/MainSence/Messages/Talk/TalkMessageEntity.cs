using UnityEngine;

[System.Serializable]
public class TalkMessageEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// メッセージ番号
    /// </summary>
    public int number;

    /// <summary>
    /// メッセージ内容
    /// </summary>
    public string message;

    /// <summary>
    /// 話しているキャラクターの名前
    /// </summary>
    public string speakerName;

    /// <summary>
    /// 後ろを振り向くステータス（0:振り向かない、1:振り向く）
    /// </summary>
    public int isplayerBackRotateStatus;
}
