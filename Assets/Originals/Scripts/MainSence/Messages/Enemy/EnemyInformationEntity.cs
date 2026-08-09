using UnityEngine;

[System.Serializable]
public class EnemyInformationEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// 番号
    /// </summary>
    public int number;

    /// <summary>
    /// 敵の日本語の名前
    /// </summary>
    public string nameJapanese;

    /// <summary>
    /// 敵の英語の名前
    /// </summary>
    public string nameEnglish;

    /// <summary>
    /// 敵の簡体字中国語の名前
    /// </summary>
    public string nameChinese01;

    /// <summary>
    /// 敵の繁体字中国語の名前
    /// </summary>
    public string nameChinese02;

    /// <summary>
    /// 敵のスペイン語の名前
    /// </summary>
    public string nameSpanish;

    /// <summary>
    /// 敵のポルトガル語の名前
    /// </summary>
    public string namePortuguese;

    /// <summary>
    /// 歩行音SEのID
    /// </summary>
    public int walkSEId;

    /// <summary>
    /// ダッシュ音SEのID
    /// </summary>
    public int runSEId;

    /// <summary>
    /// プレイヤーを探すSEのID
    /// </summary>
    public int findPlayerSEId;
}
