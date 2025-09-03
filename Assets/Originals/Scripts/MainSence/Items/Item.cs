using System;
using UnityEngine;

// �A�C�e���̎�ނ�\���񋓌^
public enum ItemType
{
    Test,       // �e�X�g�p
    UseItem,    // �g�p�A�C�e��
    MysteryItem,// �S�[���ɕK�v�ȃA�C�e��
    Document,   //�h�L�������g
    Key         //��
}


//DB�ŊǗ��������A�C�e���̏��ꗗ
[System.Serializable]
public class Item : MonoBehaviour
{
    public int id;             // �A�C�e����ID
    public GameObject prefab;  // �A�C�e���̃v���n�u
    [TextArea]
    public string prefabPath;�@//�A�C�e���̃v���n�u��Addressables��
    public Vector3 spawnPosition; //�v���C���[�̈ʒu����A�C�e���𐶐��������ʒu
    public Quaternion spawnRotation; //�A�C�e���̉�]���l
    public Sprite icon;        // �A�C�e���̃A�C�R���摜
    public ItemType itemType;  // �A�C�e���̎��
    public string itemName;    // �A�C�e���̖��O
    [TextArea]
    public string description; // �A�C�e���̐���
    public int count;          // ������
    public int effectValue;    // ���ʒl


    public int GetId()
    {
        return id;
    }

    
}
