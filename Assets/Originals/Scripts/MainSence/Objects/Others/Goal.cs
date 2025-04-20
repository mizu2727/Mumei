using Mono.Cecil.Cil;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class Goal : MonoBehaviour
{
    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;

    [SerializeField] public int anserItemId;//正解用のアイテムid


    [SerializeField] private GameObject GoalPanel;//コールパネル
    [SerializeField] private Button[] selectMysteryItemButton;//ミステリーアイテム名称ボタン
    [SerializeField] private Image[] selectMysteryItemImage;//ミステリーアイテム画像

    public bool isGoalPanel;

    private void Start()
    {
        isGoalPanel = false;
        ViewGoalPanel();

        InitializeSelectMysteryItemUI();
    }

    public async void GoalCheck()
    {
        sO_Item.CleanNullItems();

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
        string message = "ドキュメントに関係するアイテムを選択せよ";

        MessageController.instance.ShowMessage(message);

        // MysteryItemはUpdateSelectMysteryItemUIで処理するため、ここでは保持しない
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
        isGoalPanel = true;
        ViewGoalPanel();

    }

    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            GoalPanel.SetActive(true);

            // CanvasGroup の状態を確認
            CanvasGroup canvasGroup = GoalPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            // 画像を更新
            UpdateSelectMysteryItemUI();
        }
        else
        {
            Time.timeScale = 1;
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
        // Image 配列を動的に取得（ボタンの子要素から）
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            // ボタン自身の Image を確認
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true; // ボタンの raycast を有効化
            }

            // 子要素の Image を取得
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} に子要素の Image コンポーネントが見つかりません");
            }
            else
            {
                selectMysteryItemImage[i].raycastTarget = false; // 画像のクリックを無効化
                selectMysteryItemImage[i].enabled = false; // 初期状態で非表示
            }

            // ボタンの状態を設定
            selectMysteryItemButton[i].interactable = false;

            // クリックイベントを追加
            int index = i;
            selectMysteryItemButton[i].onClick.RemoveAllListeners(); // 既存リスナーをクリア
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }

        
    }

    public void OnClickedselectMysteryItemButton(int index)
    {
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        if (index < mysteryItems.Count && mysteryItems[index] != null)
        {
            if (mysteryItems[index].id == anserItemId)
            {
                Debug.Log("正解のアイテムが選択されました！");
                SceneManager.LoadScene("GameClearScene");
            }
            else
            {
                Debug.Log("不正解のアイテムが選択されました");
                // 不正解時の処理（例：メッセージ表示）
            }
        }
    }

    // ミステリーアイテムのUIを更新
    private void UpdateSelectMysteryItemUI()
    {
        // itemListからMysteryItemを取得
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i < mysteryItems.Count && mysteryItems[i] != null)
            {
                selectMysteryItemButton[i].interactable = true;
                Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = mysteryItems[i].icon;
                    selectMysteryItemImage[i].enabled = (mysteryItems[i].icon != null);
                    selectMysteryItemImage[i].color = Color.white;
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
