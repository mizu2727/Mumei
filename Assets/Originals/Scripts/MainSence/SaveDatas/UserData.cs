using UnityEngine;


/// <summary>
/// ユーザーデータを保存するためのクラス
/// </summary>
[System.Serializable]
public class UserData 
{
    [Header("入力したプレイヤー名")]
    public string playerName;

    [Header("プレイ回数")]
    public int playCount;

    [Header("現在のシーン名")]
    public string sceneName;
}
