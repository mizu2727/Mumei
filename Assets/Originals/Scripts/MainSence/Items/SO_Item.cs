using UnityEngine;
using System.Collections.Generic;


//アイテムDB
[CreateAssetMenu(fileName = "SO_Item", menuName = "Scriptable Objects/SO_Item")]
public class SO_Item : ScriptableObject
{
    public List<Item> itemList = new ();


    //　アイテムリストを返す
    public List<Item> GetItemLists()
    {
        return itemList;
    }


    // idでアイテムを検索するメソッド
    public Item GetItemById(int id)
    {
        return itemList.Find(item => item.id == id);
    }
}
