using Mono.Cecil.Cil;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;



public class Goal : MonoBehaviour
{
    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;

    [SerializeField] public int anserItemId;//正解用のアイテムid


    [SerializeField] private GameObject GoalPanel;
    [SerializeField] private Button[] selectMysteryItemButton;//ミステリーアイテム名称ボタン
    [SerializeField] private Image[] selectMysteryItemImage;//ミステリーアイテム画像

    public bool isGoalPanel;

    private SO_Item.ItemData mysteryItem; // MysteryItem タイプのアイテムを保持

    private void Start()
    {
        isGoalPanel = false;
        ViewGoalPanel();

        InitializeSelectMysteryItemUI();
    }

    public async void GoalCheck()
    {
        sO_Item.CleanNullItems();

        Debug.Log($"GoalCheck 実行時のitemList数: {sO_Item.itemList.Count}");
        for (int i = 0; i < sO_Item.itemList.Count; i++)
        {
            var item = sO_Item.itemList[i];
            if (item != null)
            {
                Debug.Log($"itemList[{i}]: Name={item.itemName}, Type={item.itemType}, Icon={(item.icon != null ? item.icon.name : "null")}");
            }
            else
            {
                Debug.LogWarning($"itemList[{i}] に null が含まれています！");
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

        // MysteryItem を検索して保持
        mysteryItem = sO_Item.itemList.Find(item => item != null && item.itemType == ItemType.MysteryItem);


        if (mysteryItem == null)
        {
            Debug.LogWarning("MysteryItem が見つかりませんでした");
        }
        else
        {
            Debug.Log($"MysteryItem が見つかりました: {mysteryItem.itemName}");
        }


        isGoalPanel = true;
        ViewGoalPanel();

    }

    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            GoalPanel.SetActive(true);

            // 画像を更新
            UpdateSelectMysteryItemUI(); // 画像表示を更新
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GoalPanel.SetActive(false);

            // 画像を非表示にする（必要に応じて）
            for (int i = 0; i < selectMysteryItemImage.Length; i++)
            {
                if (selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }

    public void OnClickedReturnToInGameButton() 
    {
        isGoalPanel = false;
        ViewGoalPanel();

        MessageController.instance.ResetMessage();
    }


    // ミステリーアイテムのUIを初期化
    private void InitializeSelectMysteryItemUI()
    {
        if (selectMysteryItemButton == null)
        {
            Debug.LogError("mysteryItemNameButton is not assigned!");
            return;
        }

        // Image 配列を動的に取得（ボタンの子要素から）
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            // ボタン自身の Image を無効化
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = false;
                Debug.Log($"Button {i} の Image を無効化しました");
            }

            // 子要素の Image を取得
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} に Image コンポーネントが見つかりません");
            }
            else
            {
                selectMysteryItemImage[i].raycastTarget = false; // 画像のクリックを無効化

            }
        }

        // ボタンにクリックイベントを追加
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            int index = i;
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }
    }

    public void OnClickedselectMysteryItemButton(int index)
    {
        if (mysteryItem != null && index == 0) // 最初のボタンのみ処理
        {
            Debug.Log($"Clicked Mystery Item: {mysteryItem.itemName}");
            // 正解判定を行う場合は、anserItemId を使用
            if (mysteryItem.id == anserItemId)
            {
                Debug.Log("正解のアイテムが選択されました！");
            }
            else
            {
                Debug.Log("不正解のアイテムが選択されました");
            }
        }
    }


    

    // ミステリーアイテムのUIを更新
    private void UpdateSelectMysteryItemUI()
    {
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i == 0 && mysteryItem != null && mysteryItem.itemType == ItemType.MysteryItem)
            {
                selectMysteryItemButton[i].interactable = true;
                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    Debug.Log($"Setting image for {mysteryItem.itemName}, Icon: {(mysteryItem.icon != null ? mysteryItem.icon.name : "null")}");
                    selectMysteryItemImage[i].sprite = mysteryItem.icon;
                    selectMysteryItemImage[i].enabled = (mysteryItem.icon != null);
                    selectMysteryItemImage[i].color = Color.white; // アルファ値を強制的に1に
                    Debug.Log($"Image {i} - Enabled: {selectMysteryItemImage[i].enabled}, Sprite: {(selectMysteryItemImage[i].sprite != null ? selectMysteryItemImage[i].sprite.name : "null")}, Alpha: {selectMysteryItemImage[i].color.a}, Scale: {selectMysteryItemImage[i].transform.localScale}");
                }
            }
            else
            {
                selectMysteryItemButton[i].interactable = false;
                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }


}
