using UnityEngine;
using System.Collections.Generic;


//�A�C�e��DB
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    
    //Item�̃f�[�^��ۑ����邽�߂̃N���X
    //�Z�[�u�f�[�^�ۑ����ɂ��ۑ��ł���H
    [System.Serializable]
    public class ItemData
    {
        public int id;             // �A�C�e����ID
        public GameObject prefab;  // �A�C�e���̃v���n�u
        public Sprite icon;        // �A�C�e���̃A�C�R���摜
        public ItemType itemType;  // �A�C�e���̎��
        public string itemName;    // �A�C�e���̖��O
        [TextArea]
        public string description; // �A�C�e���̐���
        public int count;          // ������
        public int effectValue;    // ���ʒl

        public ItemData(Item item)
        {
            id = item.id;
            prefab = item.prefab;
            icon = item.icon;
            itemType = item.itemType;
            itemName = item.itemName;
            description = item.description;
            count = item.count;
            effectValue = item.effectValue;
        }
    }

    public List<ItemData> itemList = new List<ItemData>();

    //�@�A�C�e�����X�g��Ԃ�
    public List<ItemData> GetItemLists()
    {
        return itemList;
    }


    // �ۑ����Ă���A�C�e����S�ď���������
    public void ResetItems()
    {
        itemList.Clear();
    }


    // id�ŃA�C�e�����������郁�\�b�h
    public ItemData GetItemById(int id)
    {
        return itemList.Find(item => item.id == id);
    }

    //itemType�ŃA�C�e�����������郁�\�b�h
    public bool GetItemByType(ItemType targetType)
    {
        if (itemList == null)
        {
            Debug.LogError("itemList is null");
            return false;
        }

        // null �ȃA�C�e�������O���ă`�F�b�N
        bool result = 
            itemList.Exists(item => item != null && item.itemType == targetType);
        return result;
    }

    


    //�A�C�e���ǉ�
    public void AddItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //�A�C�e���V�K�ǉ�
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);
            Debug.Log($"�A�C�e�� {newItem.id} ��+ {newItem.count}�V�K�ǉ�");
        }
        else 
        {
            // �����A�C�e���̐���ǉ��X�V
            var updateItem = itemList.Find(item => item.id == newItem.id);
            updateItem.count += newItem.count;
            Debug.Log($"�A�C�e�� {updateItem.id} �̐���ǉ��X�V�B�������F {updateItem.count}");
        }
    }

    //�h�L�������g�E�~�X�e���[�A�C�e���ǉ�
    public void AddDocumentORMysteryItem(Item newItem)
    {
        if (newItem == null || newItem.gameObject == null)
        {
            Debug.LogWarning("AddDocument �� null �� item ���n����܂����I");
            return;
        }

        Debug.Log($"�ǉ����悤�Ƃ��Ă���A�C�e��: {newItem.id}, ���݂�itemList��: {itemList.Count}");
        if (!itemList.Exists(item => item != null && item.id == newItem.id))
        {
            ItemData itemData = new ItemData(newItem);
            itemList.Add(itemData);

            if (itemData.itemType == ItemType.Document)
            {
                PauseController.instance.ChangeDocumentNameText(itemData.itemName);
                PauseController.instance.ChangeDocumentExplanationText(itemData.description);
            }
            else 
            {
                PauseController.instance.ChangeMysteryItemTexts(itemData.itemName ,itemData.description);

            }
            Debug.Log($"�A�C�e����ǉ�: {itemData.id}, �V����itemList��: {itemList.Count}");
        }
        else
        {
            Debug.Log("����id�̃A�C�e���͂��łɒǉ��ς�");
        }
    }

    

    //null�A�C�e�����폜
    public void CleanNullItems()
    {
        int before = itemList.Count;
        itemList.RemoveAll(item => item == null);
        Debug.Log($"null ���폜���܂���: {before - itemList.Count} ��");
    }
}

