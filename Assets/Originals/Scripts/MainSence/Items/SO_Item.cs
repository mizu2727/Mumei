using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;


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
        Debug.Log(itemList.Find(item => item.id == id));
        return itemList.Find(item => item.id == id);
    }

    //アイテム追加
    public void AddItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //アイテム新規追加
            itemList.Add(newItem);
            Debug.Log($"アイテム {newItem.id} を+ {newItem.count}新規追加");
        }
        else 
        {
            // 既存アイテムの数を追加更新
            var updateItem = itemList.Find(item => item.id == newItem.id);
            updateItem.count += newItem.count;
            Debug.Log($"アイテム {updateItem.id} の数を追加更新。所持数： {updateItem.count}");
        }
    }

    //ドキュメント追加
    public void AddDocument(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //ドキュメント新規追加
            itemList.Add(newItem);
            Debug.Log($"ドキュメント {newItem.id} を+ {newItem.count}新規追加");
        }
        else
        {
            Debug.LogError($"{newItem.id}をすでに所持しています");
        }
    }

    //ミステリーアイテム追加
    public void AddMysteryItem(Item newItem)
    {
        if (!itemList.Exists(item => item.id == newItem.id))
        {
            //ミステリーアイテム新規追加
            itemList.Add(newItem);
            Debug.Log($"ミステリーアイテム {newItem.id} を+ {newItem.count}新規追加");
        }
        else
        {
            Debug.LogError($"{newItem.id}をすでに所持しています");
        }
    }
}
