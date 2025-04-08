using Mono.Cecil.Cil;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;


    public bool isDebugGoal = false;

    private void Start()
    {

    }

    public void GoalCheck()
    {
        sO_Item.CleanNullItems();

        Debug.Log($"GoalCheck 実行時のitemList数: {sO_Item.itemList.Count}");
        foreach (var item in sO_Item.itemList)
        {
            if (item != null)
            {
                Debug.Log($"itemList内: {item.itemName} - {item.itemType}");
            }
            else
            {
                Debug.LogWarning("itemList に null が含まれています！");
            }
        }

        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {
            Debug.Log("ドキュメントが必要だ！");
            return;
        }

        Debug.Log("ドキュメントが見つかりました！");

        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            Debug.Log("ミステリーアイテムが必要だ！");
            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        Debug.Log("このドキュメントに関係するアイテムを選択せよ");

        if (isDebugGoal) 
        {
         
        }

    }
}
