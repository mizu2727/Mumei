using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ユーザーデータを保存するためのクラス
/// </summary>
[System.Serializable]
public class UserData 
{
    [Header("入力したプレイヤー名(ヒエラルキー上からの編集禁止)")]
    public string playerName;

    [Header("プレイ回数(ヒエラルキー上からの編集禁止)")]
    public int playCount;

    [Header("現在のシーン名(ヒエラルキー上からの編集禁止)")]
    public string sceneName;

    [Header("マウス/ゲームパッドの右スティックの感度(ヒエラルキー上からの編集禁止)")]
    public float sensitivityValue;

    [Header("セーブするBGM音量")]
    public float bGMVolume;

    [Header("セーブするSE音量")]
    public float sEVolume;

    [Header("セーブする明るさの値")]
    public float _brightnessValue;

    [Header("セーブするフルスクリーンフラグ値")]
    public bool isFullScreen;

    [Header("セーブする画面解像度の配列インデックス番号")]
    public int resolutionArrayIndexNumber;

    [Header("セーブするOperationPanel手動閲覧フラグ")]
    public bool isSelfViewOperationPanel;

    [Header("セーブするUseItemTextPanel手動閲覧フラグ")]
    public bool isSelfViewUseItemTextPanel;

    [Header("セーブするCompassTextPanel手動閲覧フラグ")]
    public bool isSelfViewCompassTextPanel;

    [Header("セーブするシーン名配列インデックス番号")]
    public int _stageSceneNameArrayIndex;

    [Header("セーブする難易度ステータス")]
    public DifficultyLevelController.DifficultyLevel _difficultyLevelStatus;

    [Header("セーブするステージクリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<StageClearData> stageClearList = new ();

    [Header("セーブするデモステージ01難易度クリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<DemoStage01DifficultyLevelClearData> demoStage01DifficultyLevelClearDataList = new();

    [Header("セーブするステージ01難易度クリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage01DifficultyLevelClearData> stage01DifficultyLevelClearDataList = new();

    [Header("セーブするステージ02難易度クリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage02DifficultyLevelClearData> stage02DifficultyLevelClearDataList = new();

    [Header("セーブするステージ03難易度クリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage03DifficultyLevelClearData> stage03DifficultyLevelClearDataList = new();

    [Header("セーブするステージ04難易度クリアステータスリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage04DifficultyLevelClearData> stage04DifficultyLevelClearDataList = new();

    [Header("セーブするデモステージ01難易度毎クリアタイムリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<DemoStage01DifficultyLevelClearTimeData> demoStage01DifficultyLevelClearTimeDataList = new();

    [Header("セーブするステージ01難易度毎クリアタイムリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage01DifficultyLevelClearTimeData> stage01DifficultyLevelClearTimeDataList = new();

    [Header("セーブするステージ02難易度毎クリアタイムリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage02DifficultyLevelClearTimeData> stage02DifficultyLevelClearTimeDataList = new();

    [Header("セーブするステージ03難易度毎クリアタイムリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage03DifficultyLevelClearTimeData> stage03DifficultyLevelClearTimeDataList = new();

    [Header("セーブするステージ04難易度毎クリアタイムリスト(JsonUtilityで保存可能な形式への変換用List)")]
    public List<Stage04DifficultyLevelClearTimeData> stage04DifficultyLevelClearTimeDataList = new();

    [Header("セーブする言語ステータス")]
    public LanguageController.LanguageStatus _languageStatusStatus;
}

/// <summary>
/// Dictionary型のステージクリアステータスを保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class StageClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のデモステージ01難易度クリアステータス配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class DemoStage01DifficultyLevelClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のステージ01難易度クリアステータス配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage01DifficultyLevelClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のステージ02難易度クリアステータス配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage02DifficultyLevelClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のステージ03難易度クリアステータス配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage03DifficultyLevelClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のステージ04難易度クリアステータス配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage04DifficultyLevelClearData
{
    public string key;
    public int value;
}

/// <summary>
/// Dictionary型のデモステージ01難易度毎クリアタイム配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class DemoStage01DifficultyLevelClearTimeData
{
    public string key;
    public string value;
}

/// <summary>
/// Dictionary型のステージ01難易度毎クリアタイム配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage01DifficultyLevelClearTimeData
{
    public string key;
    public string value;
}

/// <summary>
/// Dictionary型のステージ02難易度毎クリアタイム配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage02DifficultyLevelClearTimeData
{
    public string key;
    public string value;
}

/// <summary>
/// Dictionary型のステージ03難易度毎クリアタイム配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage03DifficultyLevelClearTimeData
{
    public string key;
    public string value;
}

/// <summary>
/// Dictionary型のステージ04難易度毎クリアタイム配列を保存するためのクラス
/// JsonUtilityで保存可能な形式への変換のために作成
/// </summary>
[System.Serializable]
public class Stage04DifficultyLevelClearTimeData
{
    public string key;
    public string value;
}