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
    /// 日本語メッセージサイズ
    /// </summary>
    public int messageSizeJapanese;

    /// <summary>
    /// 英語メッセージ
    /// </summary>
    public string messageEnglish;

    /// <summary>
    /// 話しているキャラクターの名前の英語表記
    /// </summary>
    public string speakerNameEnglish;

    /// <summary>
    /// 英語メッセージサイズ
    /// </summary>
    public int messageSizeEnglish;

    /// <summary>
    /// 後ろを振り向くステータス（0:振り向かない、1:振り向く）
    /// </summary>
    public int isplayerBackRotateStatus;

    /// <summary>
    /// ノイズSEを流すステータス（0:流さない、1:流すパターンその1　2:流すパターンその2）
    /// </summary>
    public int isPlaySoundNoiseStatus;

    /// <summary>
    /// 待ち時間を発生させるステータス（0:待ち時間なし、1:待ち時間あり）
    /// </summary>
    public int isWaitStatus;

    /// <summary>
    /// 待ち時間の長さ（秒）
    /// </summary>
    public int waitTime;

    /// <summary>
    /// チュートリアルフラグ（0:何もなし、1:チュートリアルフラグあり 2:チュートリアル中にプレイヤーを操作できる 3:チュートリアル用ミステリーアイテム表示）
    /// </summary>
    public int isTutorialStatus;

    /// <summary>
    /// 参照したいSystemMessage番号
    /// </summary>
    public int showSystemMessageNumber;

    /// <summary>
    /// 会話終了ステータス（0:会話終了なし、1:会話終了パターンその1）
    /// </summary>
    public int isEndStatus;

    /// <summary>
    /// ブラックアウト関連ステータス（0:なにもしない、2:ブラックアウト解除）
    /// </summary>
    public int isBlackOutStatus;
}
