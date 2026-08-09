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

    /// <summary>
    /// 簡体字中国語での難易度説明
    /// </summary>
    public string explanationChinese01;

    /// <summary>
    /// 難易度説明を簡体字中国語で表示する際のサイズ
    /// </summary>
    public int explanationSizeChinese01;

    /// <summary>
    /// 繁体字中国語での難易度説明
    /// </summary>
    public string explanationChinese02;

    /// <summary>
    /// 難易度説明を繁体字中国語で表示する際のサイズ
    /// </summary>
    public int explanationSizeChinese02;

    /// <summary>
    /// スペイン語での難易度説明
    /// </summary>
    public string explanationSpanish;

    /// <summary>
    /// 難易度説明をスペイン語で表示する際のサイズ
    /// </summary>
    public int explanationSizeSpanish;

    /// <summary>
    /// ポルトガル語での難易度説明
    /// </summary>
    public string explanationPortuguese;

    /// <summary>
    /// 難易度説明をポルトガル語で表示する際のサイズ
    /// </summary>
    public int explanationSizePortuguese;
}
