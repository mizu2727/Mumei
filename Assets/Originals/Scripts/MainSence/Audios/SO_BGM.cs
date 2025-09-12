using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM�̍Đ����
/// </summary>
public enum  BGMState
{
    /// <summary>
    /// �Đ���~
    /// </summary>
    Stop,

    /// <summary>
    /// �Đ���
    /// </summary>
    Play,

    /// <summary>
    /// �ꎞ��~��
    /// </summary>
    Pause,  
}


[CreateAssetMenu(fileName = "SO_BGM", menuName = "Scriptable Objects/SO_BGM")]
public class SO_BGM : ScriptableObject
{
    [System.Serializable]
    public class BGMData 
    {
        [Header("BGM��ID")]
        public int id;

        [Header("BGM��AudioClip")]
        public AudioClip bgmAudioClip;

        [Header("BGM�̖��O")]
        [TextArea]
        public string bgmName;

        [Header("BGM�̐���")]
        [TextArea]
        public string description;

        [Header("BGM�̍Đ����")]
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

    //BGM�̃��X�g
    public List<BGMData> bgmList = new();

    /// <summary>
    /// �w�肵��ID�Ɋ�Â���AudioClip���擾���郁�\�b�h
    /// </summary>
    /// <param name="bgmId">�擾����id</param>
    /// <returns>AudioClip</returns>
    public AudioClip GetBGMClip(int bgmId)
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM �� bgmList ����܂���null�ł��B(GetBGMClip)");
            return null;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmAudioClip != null)
        {
            return bgmData.bgmAudioClip;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} ��BGM��������܂���B");
            return null;
        }
    }

    /// <summary>
    /// BGM���X�g���̑S�Ă�BGM�̏�Ԃ�Stop�ɕύX���郁�\�b�h
    /// </summary>
    public void StopAllBGM() 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM �� bgmList ����܂���null�̂��̂����݂��܂��B");
            return ;
        }

        foreach (var bgmData in bgmList)
        {
            bgmData.bgmState = BGMState.Stop;
        }
    }


    /// <summary>
    /// BGM�̏�Ԃ�Stop����Play�ɕύX���郁�\�b�h
    /// </summary>
    /// <param name="bgmId">�Ώ�BGM��id</param>
    /// <returns>true�Ȃ�BGMState��Play / false�Ȃ�BGMState��Stop</returns>
    public BGMState ChangeFromStopToPlayBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM �� bgmList ����܂���null�ł��BBGMState��Stop�̏�ԂɈێ����܂��B");
            return BGMState.Stop;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Stop)
        {
            return BGMState.Play;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} ��BGM��������܂���BBGMState��Stop�̏�ԂɈێ����܂��B");
            return BGMState.Stop;
        }
    }

    /// <summary>
    /// BGM�̏�Ԃ�Play����Pause�ɕύX���郁�\�b�h
    /// </summary>
    /// <param name="bgmId">�Ώ�BGM��id</param>
    /// <returns>true�Ȃ�BGMState��Pause / false�Ȃ�BGMState��Play</returns>
    public BGMState ChangeFromPlayToPauseBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM �� bgmList ����܂���null�ł��BBGMState��Play�̏�ԂɈێ����܂��B(ChangeFromPlayToPauseBGM)");
            return BGMState.Play;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Play)
        {
            return BGMState.Pause;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} ��BGM��������܂���BBGMState��Play�̏�ԂɈێ����܂��B(ChangeFromPlayToPauseBGM)");
            return BGMState.Play;
        }
    }

    /// <summary>
    /// BGM�̏�Ԃ�Play����Stop�ɕύX���郁�\�b�h
    /// </summary>
    /// <param name="bgmId">�Ώ�BGM��id</param>
    /// <returns>true�Ȃ�BGMState��Stop / false�Ȃ�BGMState��Play</returns>
    public BGMState ChangeFromPlayToStopBGM(int bgmId) 
    {
        if (bgmList == null || bgmList.Count == 0)
        {
            Debug.LogWarning("SO_BGM �� bgmList ����܂���null�ł��BBGMState��Play�̏�ԂɈێ����܂��B(ChangeFromPlayToStopBGM)");
            return BGMState.Play;
        }

        BGMData bgmData = bgmList.Find(bgm => bgm.id == bgmId);
        if (bgmData != null && bgmData.bgmState == BGMState.Play)
        {
            return BGMState.Stop;
        }
        else
        {
            Debug.LogWarning($"ID {bgmId} ��BGM��������܂���BBGMState��Play�̏�ԂɈێ����܂��B(ChangeFromPlayToStopBGM)");
            return BGMState.Play;
        }
    }
}
