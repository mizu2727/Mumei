using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;



public class Goal : MonoBehaviour
{
    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("正解用のアイテムid")]
    [SerializeField] private int anserItemId;

    [Header("正解用のチュートリアルアイテムid")]
    [SerializeField] private int anserTutorialItemId;

    [Header("ゴールパネル(ヒエラルキー上のパネルをアタッチする必要がある)")]
    [SerializeField] private GameObject GoalPanel;

    [Header("ミステリーアイテム名称ボタン(ヒエラルキー上のボタンをアタッチする必要がある)")]
    [SerializeField] private Button[] selectMysteryItemButton;

    [Header("ミステリーアイテム画像(ヒエラルキー上の画像をアタッチする必要がある)")]
    [SerializeField] private Image[] selectMysteryItemImage;

    [Header("ゴールフラグ")]
    public bool isGoalPanel;

    [Header("チュートリアルフラグ(チュートリアルステージでオンになる)")]
    [SerializeField] public bool isTutorial;



    private void Start()
    {
        //ゴールパネルを非表示
        isGoalPanel = false;
        ViewGoalPanel();

        //ゴールパネル内のミステリーアイテムのUIを初期化
        InitializeSelectMysteryItemUI();
    }

    /// <summary>
    /// プレイヤーがゴールオブジェクトに触れた場合の処理
    /// </summary>
    public async void GoalCheck()
    {
        //アイテムのnullチェック
        sO_Item.CleanNullItems();

        //ドキュメントを入手していない場合
        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {
            //ドキュメントが必要である旨のメッセージを表示し、処理をスキップ
            MessageController.instance.ShowGoalMessage(1);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();
            return;
        }

        //ミステリーアイテムを入手していない場合
        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            //ミステリーアイテムが必要である旨のメッセージを表示し、処理をスキップ
            MessageController.instance.ShowGoalMessage(2);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();
            return;
        }
        else 
        {
            //ミステリーアイテムを選択からの処理へ
            MysteryItemCheck();
        } 
    }

    /// <summary>
    /// ミステリーアイテムを選択からの処理
    /// </summary>
    void MysteryItemCheck() 
    {
        //ミステリーアイテムを選択する旨のメッセージを表示
        isGoalPanel = true;
        MessageController.instance.ShowGoalMessage(3);

        //MysteryItemはUpdateSelectMysteryItemUIで処理するため、ここでは保持しない
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
        //ゴールパネルを表示
        ViewGoalPanel();

    }

    /// <summary>
    /// ゴールパネルの表示/非表示
    /// </summary>
    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            //一時停止
            Time.timeScale = 0;

            //マウスカーソルを有効にし、固定を解除
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            //パネルを表示
            GoalPanel.SetActive(true);

            //CanvasGroupの状態を確認
            CanvasGroup canvasGroup = GoalPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            //画像を更新
            UpdateSelectMysteryItemUI();
        }
        else
        {
            //マウスカーソルを非表示にし、固定する
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GoalPanel.SetActive(false);

            //画像を非表示にする
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

    /// <summary>
    /// 「戻る」ボタン押下
    /// </summary>
    public async void OnClickedReturnToInGameButton() 
    {
        //一時停止解除
        Time.timeScale = 1;

        //パネル非表示
        isGoalPanel = false;
        ViewGoalPanel();

        //メッセージテキストを空にする
        MessageController.instance.ResetMessage();

        //チュートリアルの場合、チュートリアル終了の旨のメッセージを表示する
        if (GameController.instance.isTutorialGoalFlag) await MessageController.instance.ShowSystemMessage(14);
    }

    /// <summary>
    /// ミステリーアイテムのUIを初期化
    /// </summary>
    private void InitializeSelectMysteryItemUI()
    {
        //Image配列を動的に取得（ボタンの子要素から）
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            //ボタン自身のImageを確認
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                //ボタンのraycastを有効化
                buttonImage.raycastTarget = true;
            }

            //子要素のImageを取得
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} に子要素の Image コンポーネントが見つかりません");
            }
            else
            {
                //画像のクリックを無効化
                selectMysteryItemImage[i].raycastTarget = false;

                //初期状態で非表示にする
                selectMysteryItemImage[i].enabled = false; 
            }

            //ボタンの状態を設定
            selectMysteryItemButton[i].interactable = false;

            //クリックイベントを追加
            int index = i;

            //既存リスナーをクリア
            selectMysteryItemButton[i].onClick.RemoveAllListeners(); 

            //新規リスナーを追加
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }       
    }

    /// <summary>
    /// ミステリーアイテム画像を押下した場合の処理
    /// </summary>
    /// <param name="index"></param>
    public void OnClickedselectMysteryItemButton(int index)
    {
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        if (index < mysteryItems.Count && mysteryItems[index] != null)
        {
            //正解のミステリーアイテムであるかを判定
            if (mysteryItems[index].id == anserItemId)
            {
                //正解時の処理

                //プレイヤーを削除
                Player.instance.DestroyPlayer();
                
                //画面遷移
                SceneManager.LoadScene("GameClearScene");
            }
            //正解のミステリーアイテム(チュートリアル版)であるかを判定
            else if (mysteryItems[index].id == anserTutorialItemId) 
            {
                //チュートリアル終了後の会話を進める
                GameController.instance.isTutorialGoalFlag = true;
                OnClickedReturnToInGameButton();
            }
            else
            {
                //不正解時の処理
                MessageController.instance.ShowGoalMessage(4);
            }
        }
    }

    /// <summary>
    /// ミステリーアイテムのUIを更新
    /// </summary>
    private void UpdateSelectMysteryItemUI()
    {
        //itemListからMysteryItemを取得
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i < mysteryItems.Count && mysteryItems[i] != null)
            {
                //ボタンクリックを有効
                selectMysteryItemButton[i].interactable = true;
                Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    //ミステリーアイテム画像を設定
                    selectMysteryItemImage[i].sprite = mysteryItems[i].icon;
                    selectMysteryItemImage[i].enabled = (mysteryItems[i].icon != null);
                    selectMysteryItemImage[i].color = Color.white;
                }
            }
            else
            {
                //ボタンクリックを無効
                selectMysteryItemButton[i].interactable = false;

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    //ミステリーアイテム画像をnullにする
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }
}
