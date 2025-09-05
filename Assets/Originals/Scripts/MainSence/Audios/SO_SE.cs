using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SEのDB
/// </summary>
[CreateAssetMenu(fileName = "SO_SE", menuName = "Scriptable Objects/SO_SE")]
public class SO_SE : ScriptableObject
{
    [System.Serializable]
    public class SEData
    {
        [Header("SEのID")]
        public int id;

        [Header("SEのAudioClip")]
        public AudioClip seAudioClip;

        [Header("SEの名前")]
        [TextArea]
        public string seName;

        [Header("SEの説明")]
        [TextArea]
        public string description;
        public SEData(int id, AudioClip seAudioClip, string clipPath, string seName, string description)
        {
            this.id = id;
            this.seAudioClip = seAudioClip;
            this.seName = seName;
            this.description = description;
        }
    }

    public List<SEData> seList = new ();


    /// <summary>
    /// 指定したIDに基づいてAudioClipを取得するメソッド
    /// </summary>
    /// <param name="seId">id</param>
    /// <returns></returns>
    public AudioClip GetSEClip(int seId)
    {
        if (seList == null || seList.Count == 0)
        {
            Debug.LogWarning("SO_SE の seList が空またはnullです。");
            return null;
        }

        SEData seData = seList.Find(se => se.id == seId);
        if (seData != null && seData.seAudioClip != null)
        {
            return seData.seAudioClip;
        }
        else
        {
            Debug.LogWarning($"ID {seId} の効果音が見つかりません。");
            return null;
        }
    }
}
