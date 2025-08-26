using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SaveLoad : MonoBehaviour
{
    UserData userData;


    private void Update()
    {

    }

    /// <summary>
    /// �f�[�^��ۑ����郁�\�b�h
    /// </summary>
    public void SaveUserData() 
    {
        //userData = GetComponent<UserData>();

        //���[�U�[�f�[�^��ۑ�����N���X
        userData = new UserData() 
        {
            playerName = GameController.playerName,
            playCount = GameController.playCount,

            //���݂̃V�[�������擾���ĕۑ�
            sceneName = SceneManager.GetActiveScene().name,
        };

        //���[�U�[�f�[�^��JSON�`���ŕۑ�
        //UserData�I�u�W�F�N�g��JSON������ɕϊ�
        string json = JsonUtility.ToJson(userData, true);

        Debug.Log("Json�`���Ńf�[�^��ۑ��������e:" + json);

        //PlayerPrefs�ɕۑ�
        PlayerPrefs.SetString("PlayerUserData", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// �f�[�^�����[�h���郁�\�b�h
    /// </summary>
    public void LoadUserData() 
    {
        if (PlayerPrefs.HasKey("PlayerUserData")) 
        {
            //PlayerPrefs����JSON��������擾
            string josn = PlayerPrefs.GetString("PlayerUserData");

            //JSON�������UserData�I�u�W�F�N�g�ɕϊ�
            UserData userData = JsonUtility.FromJson<UserData>(josn);           

            SceneManager.LoadScene(userData.sceneName);

            //�e�p�����[�^�[�Ƀ��[�U�[�f�[�^��ݒ�
            GameController.playerName = userData.playerName;
            GameController.playCount = ++userData.playCount;
        }
        else
        {
            Debug.LogWarning("PlayerUserData�����݂��܂���");
        }
    }

    /// <summary>
    /// �ۑ������f�[�^�����������郁�\�b�h
    /// </summary>
    public void ResetUserData()
    {
        // ����̃L�[�̃f�[�^���폜����ꍇ
        if (PlayerPrefs.HasKey("PlayerUserData"))
        {
            PlayerPrefs.DeleteKey("PlayerUserData");
            Debug.Log("PlayerUserData�����������܂���");
        }

        // �ύX��ۑ�����
        PlayerPrefs.Save();
    }
}
