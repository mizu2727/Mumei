using UnityEngine;


/// <summary>
/// ���[�U�[�f�[�^��ۑ����邽�߂̃N���X
/// </summary>
[System.Serializable]
public class UserData 
{
    [Header("���͂����v���C���[��(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public string playerName;

    [Header("�v���C��(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public int playCount;

    [Header("���݂̃V�[����(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public string sceneName;

    [Header("�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̊��x(�q�G�����L�[�ォ��̕ҏW�֎~)")]
    public float sensitivityValue;

    [Header("�Z�[�u����BGM����")]
    public float bGMVolume;

    [Header("�Z�[�u����SE����")]
    public float sEVolume;
}
