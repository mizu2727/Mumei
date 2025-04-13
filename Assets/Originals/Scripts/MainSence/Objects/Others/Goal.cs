using Mono.Cecil.Cil;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;



public class Goal : MonoBehaviour
{
    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;

    [SerializeField] public int anserItemId;//正解用のアイテムid


    [SerializeField] private GameObject selectMysteryItemPanel;
    [SerializeField] private Button[] selectMysteryItemNameButton;//ミステリーアイテム名称ボタン
    [SerializeField] private Image[] selectMysteryItemImage;//ミステリーアイテム画像

    public bool isSelectMysteryItemPanel;

    private void Start()
    {
        isSelectMysteryItemPanel = false;
        ViewSelectMysteryItemPanel();
    }

    public async void GoalCheck()
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
            string message = "ドキュメントを集めてください";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }


        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            string message = "ミステリーアイテムを集めてください";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        string message = "このドキュメントに関係するアイテムを選択せよ";

        MessageController.instance.ShowMessage(message);


    }

    public void ViewSelectMysteryItemPanel()
    {
        if (isSelectMysteryItemPanel)
        {
            selectMysteryItemPanel.SetActive(true);
        }
        else
        {
            selectMysteryItemPanel.SetActive(false);
        }
    }
}
