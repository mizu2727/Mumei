using UnityEngine;
using System.Collections.Generic;


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
        return itemList.Find(item => item.id == id);
    }
}
