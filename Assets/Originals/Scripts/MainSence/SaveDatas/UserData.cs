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
}
