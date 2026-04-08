using UnityEngine;

[System.Serializable]
public class SystemMessageEntity
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
    /// InputPlayerNameFieldに関するステータス（0:何もなし、1:InputPlayerNameFieldを表示、2:InputPlayerNameFieldを再度表示）
    /// </summary>
    public int isInputPlayerNameFieldStatus;

    /// <summary>
    /// 参照したいSystemMessage番号
    /// </summary>
    public int showSystemMessageNumber;

    /// <summary>
    /// ノイズSEを流すステータス（0:流さない、3:流すパターンその3）
    /// </summary>
    public int isPlaySoundNoiseStatus;

    /// <summary>
    /// TalkMessageStatus関連フラグ（0:何もなし、1:TalkMessageStatusを表示その1、2:TalkMessageStatusを表示その2）
    /// </summary>
    public int isShowTalkMessageStatus;

    /// <summary>
    /// 参照したいTalkMessage番号
    /// </summary>
    public int ShowTalkMessageNumber;

    /// <summary>
    /// チュートリアルフラグ（0:何もなし、4:ドキュメント(チュートリアル版)入手後の処理 5:ドキュメント(チュートリアル版)閲覧後の処理、
    /// 6:ミステリーアイテム(チュートリアル版)入手後の処理 7:ミステリーアイテム(チュートリアル版)閲覧後の処理、
    /// 8:チュートリアル終了）
    /// </summary>
    public int isTutorialStatus;
}
