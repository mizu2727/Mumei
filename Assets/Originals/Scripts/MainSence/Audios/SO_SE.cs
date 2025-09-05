using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SE��DB
/// </summary>
[CreateAssetMenu(fileName = "SO_SE", menuName = "Scriptable Objects/SO_SE")]
public class SO_SE : ScriptableObject
{
    [System.Serializable]
    public class SEData
    {
        [Header("SE��ID")]
        public int id;

        [Header("SE��AudioClip")]
        public AudioClip seAudioClip;

        [Header("SE�̖��O")]
        [TextArea]
        public string seName;

        [Header("SE�̐���")]
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
    /// �w�肵��ID�Ɋ�Â���AudioClip���擾���郁�\�b�h
    /// </summary>
    /// <param name="seId">id</param>
    /// <returns></returns>
    public AudioClip GetSEClip(int seId)
    {
        if (seList == null || seList.Count == 0)
        {
            Debug.LogWarning("SO_SE �� seList ����܂���null�ł��B");
            return null;
        }

        SEData seData = seList.Find(se => se.id == seId);
        if (seData != null && seData.seAudioClip != null)
        {
            return seData.seAudioClip;
        }
        else
        {
            Debug.LogWarning($"ID {seId} �̌��ʉ���������܂���B");
            return null;
        }
    }
}
