using UnityEngine;

[System.Serializable]
public class DifficultyLevelExplanationEntity
{
    //Excelデータの1行目と同じパラメータ

    /// <summary>
    /// 難易度説明番号
    /// </summary>
    public int number;

    /// <summary>
    /// 難易度説明
    /// </summary>
    public string explanation;

    /// <summary>
    /// 難易度説明を日本語で表示する際のサイズ
    /// </summary>
    public int explanationSizeJapanese;

    /// <summary>
    /// 英語での難易度説明
    /// </summary>
    public string explanationEnglish;

    /// <summary>
    /// 難易度説明を英語で表示する際のサイズ
    /// </summary>
    public int explanationSizeEnglish;
}
