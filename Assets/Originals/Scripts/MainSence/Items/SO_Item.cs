using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;


//�A�C�e��DB
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    public List<Item> itemList = new ();


    //�@�A�C�e�����X�g��Ԃ�
    public List<Item> GetItemLists()
    {
        return itemList;
    }


    // id�ŃA�C�e�����������郁�\�b�h
    public Item GetItemById(int id)
    {
        Debug.Log(itemList.Find(item => item.id == id));
        return itemList.Find(item => item.id == id);
    }

    //�A�C�e���ǉ�
    public void AddItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //�A�C�e���V�K�ǉ�
            itemList.Add(newItem);
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

    //�h�L�������g�ǉ�
    public void AddDocument(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //�h�L�������g�V�K�ǉ�
            itemList.Add(newItem);
            Debug.Log($"�h�L�������g {newItem.id} ��+ {newItem.count}�V�K�ǉ�");
        }
        else
        {
            Debug.LogError($"{newItem.id}�����łɏ������Ă��܂�");
        }
    }

    //�~�X�e���[�A�C�e���ǉ�
    public void AddMysteryItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //�~�X�e���[�A�C�e���V�K�ǉ�
            itemList.Add(newItem);
            Debug.Log($"�~�X�e���[�A�C�e�� {newItem.id} ��+ {newItem.count}�V�K�ǉ�");
        }
        else
        {
            Debug.LogError($"{newItem.id}�����łɏ������Ă��܂�");
        }
    }
}
