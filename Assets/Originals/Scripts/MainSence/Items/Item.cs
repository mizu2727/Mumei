using System;
using UnityEngine;

/// <summary>
/// �A�C�e���̎�ނ�\���񋓌^
/// </summary>
public enum ItemType
{
    /// <summary>
    /// �e�X�g�p
    /// </summary>
    Test,

    /// <summary>
    /// �g�p�A�C�e��
    /// </summary>
    UseItem,

    /// <summary>
    /// �~�X�e���[�A�C�e��
    /// </summary>
    MysteryItem,

    /// <summary>
    /// �h�L�������g
    /// </summary>
    Document,

    /// <summary>
    /// ��
    /// </summary>
    Key,
}


/// <summary>
/// DB�ŊǗ��������A�C�e���̏��ꗗ
/// </summary>
[System.Serializable]
public class Item : MonoBehaviour
{
    /// <summary>
    /// �A�C�e����ID
    /// </summary>
    public int id;

    /// <summary>
    /// �A�C�e���̃v���n�u
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// �A�C�e���̃v���n�u��Addressables��
    /// </summary>
    [TextArea]
    public string prefabPath;

    /// <summary>
    /// �v���C���[�̈ʒu����A�C�e���𐶐��������ʒu
    /// </summary>
    public Vector3 spawnPosition;

    /// <summary>
    /// �A�C�e���̉�]���l
    /// </summary>
    public Quaternion spawnRotation;

    /// <summary>
    /// �A�C�e���̃A�C�R���摜
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// �A�C�e���̎��
    /// </summary>
    public ItemType itemType;

    /// <summary>
    /// �A�C�e���̖��O
    /// </summary>
    public string itemName;

    /// <summary>
    /// �A�C�e���̐���
    /// </summary>
    [TextArea]
    public string description;

    /// <summary>
    /// ������
    /// </summary>
    public int count;

    /// <summary>
    /// ���ʒl
    /// </summary>
    public int effectValue;

    /// <summary>
    /// ID�擾
    /// </summary>
    /// <returns>ID</returns>
    public int GetId()
    {
        return id;
    }
}
