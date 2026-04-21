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
