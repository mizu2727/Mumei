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

    [Header("セーブするステージクリアステータス配列(JsonUtilityで保存可能な形式への変換用List)")]
    public List<StageClearData> stageClearList = new ();
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
