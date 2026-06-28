using UnityEngine;

/// <summary>
/// 放送を流すクラス
/// </summary>
public class BroadcastSpeaker : MonoBehaviour
{
    /// <summary>
    /// 放送ノイズ再生フラグ
    /// </summary>
    private bool isBroadcastNoise = false;

    /// <summary>
    /// 放送ノイズを聞いているフラグ
    /// </summary>
    private bool isListeningBroadcast = false;

    /// <summary>
    /// 放送ノイズ再生フラグを取得する
    /// </summary>
    /// <returns>放送ノイズ再生フラグ</returns>
    public bool GetIsBroadcastNoise()
    {
        return isBroadcastNoise;
    }

    /// <summary>
    /// 放送ノイズ再生フラグを設定する
    /// </summary>
    /// <param name="value">放送ノイズ再生フラグ</param>
    public void SetIsBroadcastNoise(bool value)
    {
        isBroadcastNoise = value;
    }

    /// <summary>
    /// 放送ノイズを聞いているフラグを取得する
    /// </summary>
    /// <returns>放送ノイズを聞いているフラグ</returns>
    public bool GetIsListeningBroadcast()
    {
        return isListeningBroadcast;
    }

    /// <summary>
    /// 放送ノイズを聞いているフラグを設定する
    /// </summary>
    /// <param name="value">放送ノイズを聞いているフラグ</param>
    public void SetIsListeningBroadcast(bool value)
    {
        isListeningBroadcast = value;
    }

    void Start()
    {
        //フラグの初期化
        isBroadcastNoise = false;
        isListeningBroadcast = false;
    }

    void Update()
    {
        //再生中の放送ノイズを聞いている場合
        if (isListeningBroadcast && isBroadcastNoise) 
        { 
            //TODO:専用の放送を流す。
        }
        //再生中の放送ノイズを聞いていない場合
        else if (isBroadcastNoise) 
        {
            //TODO:ノイズを流す。
        }
        else 
        {
            //放送ノイズを聞いているフラグをオフにする。
            isListeningBroadcast = false; 
        }
    }
}
