using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGMの再生状態
/// </summary>
public enum  BGMState
{
    /// <summary>
    /// 再生停止
    /// </summary>
    Stop,

    /// <summary>
    /// 再生中
    /// </summary>
    Play,

    /// <summary>
    /// 一時停止中
    /// </summary>
    Pause,  
}


[CreateAssetMenu(fileName = "SO_BGM", menuName = "Scriptable Objects/SO_BGM")]
public class SO_BGM : ScriptableObject
{
    [System.Serializable]
    public class BGMData 
    {
        [Header("BGMのID")]
        public int id;

        [Header("BGMのAudioClip")]
        public AudioClip bgmAudioClip;

        [Header("BGMの名前")]
        [TextArea]
        public string bgmName;

        [Header("BGMの説明")]
        [TextArea]
        public string description;

        [Header("BGMの再生状態")]
        public BGMState bgmState;

        public BGMData(int id, AudioClip bgmAudioClip, string bgmName, string description, BGMState bgmState)
        {
            this.id = id;
            this.bgmAudioClip = bgmAudioClip;
            this.bgmName = bgmName;
            this.description = description;
            this.bgmState = bgmState;
        }
    }

    //BGMのリスト
    public List<BGMData> bgmList = new();

    /// <summary>
    /// 指定したIDに基づいてAudioClipを取得するメソッド
    /// </summary>
    /// <param name="bgmId">取得したid</param>
    /// <returns>AudioClip</returns>
    public AudioClip GetBGMClip(int bgmId)
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM の bgmList が空またはnullです。(GetBGMClip)");
            return null;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmAudioClip != null)
        {
            return bgmData.bgmAudioClip;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} のBGMが見つかりません。");
            return null;
        }
    }

    /// <summary>
    /// BGMリスト内の全てのBGMの状態をStopに変更するメソッド
    /// </summary>
    public void StopAllBGM() 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM の bgmList が空またはnullのものが存在します。");
            return ;
        }

        foreach (var bgmData in bgmList)
        {
            bgmData.bgmState = BGMState.Stop;
        }
    }


    /// <summary>
    /// BGMの状態をStopからPlayに変更するメソッド
    /// </summary>
    /// <param name="bgmId">対象BGMのid</param>
    /// <returns>trueならBGMStateをPlay / falseならBGMStateをStop</returns>
    public BGMState ChangeFromStopToPlayBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM の bgmList が空またはnullです。BGMStateをStopの状態に維持します。");
            return BGMState.Stop;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Stop)
        {
            return BGMState.Play;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} のBGMが見つかりません。BGMStateをStopの状態に維持します。");
            return BGMState.Stop;
        }
    }

    /// <summary>
    /// BGMの状態をPlayからPauseに変更するメソッド
    /// </summary>
    /// <param name="bgmId">対象BGMのid</param>
    /// <returns>trueならBGMStateをPause / falseならBGMStateをPlay</returns>
    public BGMState ChangeFromPlayToPauseBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM の bgmList が空またはnullです。BGMStateをPlayの状態に維持します。(ChangeFromPlayToPauseBGM)");
            return BGMState.Play;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Play)
        {
            return BGMState.Pause;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} のBGMが見つかりません。BGMStateをPlayの状態に維持します。(ChangeFromPlayToPauseBGM)");
            return BGMState.Play;
        }
    }

    /// <summary>
    /// BGMの状態をPlayからStopに変更するメソッド
    /// </summary>
    /// <param name="bgmId">対象BGMのid</param>
    /// <returns>trueならBGMStateをStop / falseならBGMStateをPlay</returns>
    public BGMState ChangeFromPlayToStopBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM の bgmList が空またはnullです。BGMStateをPlayの状態に維持します。(ChangeFromPlayToStopBGM)");
            return BGMState.Play;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Play)
        {
            return BGMState.Stop;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} のBGMが見つかりません。BGMStateをPlayの状態に維持します。(ChangeFromPlayToStopBGM)");
            return BGMState.Play;
        }
    }
}
