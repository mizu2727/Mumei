using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class Goal : MonoBehaviour
{
    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("正解用のアイテムid")]
    [SerializeField] public int anserItemId;

    [Header("ゴールパネル(ヒエラルキー上のパネルをアタッチする必要がある)")]
    [SerializeField] private GameObject GoalPanel;

    [Header("ミステリーアイテム名称ボタン(ヒエラルキー上のボタンをアタッチする必要がある)")]
    [SerializeField] private Button[] selectMysteryItemButton;

    [Header("ミステリーアイテム画像(ヒエラルキー上の画像をアタッチする必要がある)")]
    [SerializeField] private Image[] selectMysteryItemImage;

    [Header("ゴール判定")]
    public bool isGoalPanel;

    [Header("チュートリアル判定(チュートリアルステージでオンになる)")]
    [SerializeField] public bool isTutorial;



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

            MessageController.instance.ShowGoalMessage(1);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }


        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            MessageController.instance.ShowGoalMessage(2);

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
        isGoalPanel = true;
        MessageController.instance.ShowGoalMessage(3);

        // MysteryItemはUpdateSelectMysteryItemUIで処理するため、ここでは保持しない
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
        
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

    public async void OnClickedReturnToInGameButton() 
    {
        Time.timeScale = 1;
        isGoalPanel = false;
        ViewGoalPanel();

        if(isTutorial) await MessageController.instance.ShowSystemMessage(14);
        else MessageController.instance.ResetMessage();
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
            //正解のミステリーアイテムであるかを判定
            if (mysteryItems[index].id == anserItemId)
            {
                Debug.Log("isTutorial :000" + isTutorial);

                if (isTutorial)
                {
                    Debug.Log("チュートリアルクリア");
                    MessageController.instance.ShowGoalMessage(5);
                }
                else 
                {
                    Debug.Log("isTutorial :" + isTutorial);

                    //正解時の処理
                    SceneManager.LoadScene("GameClearScene");
                }      
            }
            else
            {
                //不正解時の処理
                MessageController.instance.ShowGoalMessage(4);
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

    public void OnTutorial() 
    {
        isTutorial = true;
        Debug.Log("isTutorial = " + isTutorial);
    }

    public void OffTutorial()
    {
        isTutorial = false;
        Debug.Log("isTutorial は " + isTutorial);
    }
}
