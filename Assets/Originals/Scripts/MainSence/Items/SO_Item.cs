using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static SO_Item;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using static UnityEditor.Progress;
#endif


/// <summary>
/// �A�C�e��DB
/// </summary>
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    /// <summary>
    /// Item�̃f�[�^��ۑ����邽�߂̃N���X
    /// �Z�[�u�f�[�^�ۑ����ɂ��ۑ��ł���H
    /// </summary>
    [System.Serializable]
    public class ItemData
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

        public ItemData(Item item)
        {
            /// <summary>
            /// �A�C�e����ID
            /// </summary>
            id = item.id;

            /// <summary>
            /// �A�C�e���̃v���n�u
            /// </summary>
            prefab = item.prefab;

            /// <summary>
            /// �A�C�e���̃v���n�u��Addressables��
            /// </summary>
            prefabPath = item.prefabPath;

            /// <summary>
            /// �v���C���[�̈ʒu����A�C�e���𐶐��������ʒu
            /// </summary>
            spawnPosition = item.spawnPosition;

            /// <summary>
            /// �A�C�e���̉�]���l
            /// </summary>
            spawnRotation = item.spawnRotation;

            /// <summary>
            /// �A�C�e���̃A�C�R���摜
            /// </summary>
            icon = item.icon;

            /// <summary>
            /// �A�C�e���̎��
            /// </summary>
            itemType = item.itemType;

            /// <summary>
            /// �A�C�e���̖��O
            /// </summary>
            itemName = item.itemName;

            /// <summary>
            /// �A�C�e���̐���
            /// </summary>
            description = item.description;

            /// <summary>
            /// ������
            /// </summary>
            count = item.count;

            /// <summary>
            /// ���ʒl
            /// </summary>
            effectValue = item.effectValue;
        }
    }

    /// <summary>
    /// �A�C�e�����X�g
    /// </summary>
    public List<ItemData> itemList = new List<ItemData>();

    /// <summary>
    /// �A�C�e�����X�g��Ԃ�
    /// </summary>
    /// <returns>�A�C�e�����X�g</returns>
    public List<ItemData> GetItemLists()
    {
        return itemList;
    }
 
    /// <summary>
    /// �ۑ����Ă���A�C�e����S�ď���������
    /// </summary>
    public void ResetItems()
    {
        itemList.Clear();
    }


    /// <summary>
    /// id�ŃA�C�e�����������郁�\�b�h
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemData GetItemById(int id)
    {
        return itemList.Find(item => item.id == id);
    }

    /// <summary>
    /// itemType�ŃA�C�e�����������郁�\�b�h
    /// </summary>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public bool GetItemByType(ItemType targetType)
    {
        if (itemList == null)
        {
            Debug.LogError("itemList is null");
            return false;
        }

        //null�ȃA�C�e�������O���ă`�F�b�N
        bool result = itemList.Exists(item => item != null && item.itemType == targetType);
        return result;
    }

    


    /// <summary>
    /// �g�p�A�C�e���ǉ�
    /// </summary>
    /// <param name="newItem"></param>
    public void AddUseItem(Item newItem)
    {
        
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //�A�C�e���V�K�ǉ�
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            //�C���x���g���ɐV�K�ǉ�
            Inventory.instance.GetItem(itemData.id, itemData.prefabPath, itemData.spawnPosition, itemData.spawnRotation, 
                itemData.icon, itemData.itemName, itemData.description, itemData.count, itemData.effectValue);
        }
        else 
        {
            //�����A�C�e���̐���ǉ��X�V
            var updateItem = itemList.Find(item => item.id == newItem.id);
            updateItem.count += newItem.count;

            //�C���x���g���ɒǉ�
            Inventory.instance.GetItem(updateItem.id, updateItem.prefabPath, updateItem.spawnPosition, updateItem.spawnRotation, 
                updateItem.icon, updateItem.itemName, updateItem.description, updateItem.count, updateItem.effectValue);
        }
    }

    /// <summary>
    /// �g�p�A�C�e�����폜����
    /// </summary>
    /// <param name="id">�A�C�e��id</param>
    /// <param name="count">�A�C�e���̌�</param>
    public void ReduceUseItem(int id, int count) 
    {
        if (itemList.Exists(item => item.id == id)) 
        {
            //�����A�C�e���̐��������X�V
            var updateItem = itemList.Find(item => item.id == id);
            updateItem.count = count;
        }
    }

    /// <summary>
    /// �h�L�������g�E�~�X�e���[�A�C�e���ǉ�
    /// </summary>
    /// <param name="newItem">���肵���A�C�e��</param>
    public void AddDocumentORMysteryItem(Item newItem)
    {
        //null�`�F�b�N
        if (newItem == null || newItem.gameObject == null)
        {
            Debug.LogWarning("AddDocument �� null �� item ���n����܂����I");
            return;
        }

        if (!itemList.Exists(item => item != null && item.id == newItem.id))
        {
            //�A�C�e����V�K�ǉ�
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            if (itemData.itemType == ItemType.Document)
            {
                //�h�L�������g�ǉ�
                PauseController.instance.ChangeDocumentNameText(itemData.id, itemData.itemName);
                PauseController.instance.ChangeDocumentExplanationText(itemData.description);
            }
            else
            {
                //�~�X�e���[�A�C�e���ǉ�
                PauseController.instance.ChangeMysteryItemTexts(itemData.id, itemData.itemName ,itemData.description);

            }
        }
        else
        {
            Debug.Log("����id�̃A�C�e���͂��łɒǉ��ς�");
        }
    }

    /// <summary>
    /// null�A�C�e�����폜
    /// </summary>
    public void CleanNullItems()
    {
        int before = itemList.Count;
        itemList.RemoveAll(item => item == null);
    }
}

