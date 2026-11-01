using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static Player;



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

    [Header("ゴールパネル内の「戻る」ボタン(ヒエラルキー上からアタッチする必要がある。コントローラー操作用)")]
    [SerializeField] private Button returnToInGameButton;

    [Header("ゴール壁(ヒエラルキーからアタッチする必要がある)")]
    [SerializeField] private GameObject goalWall;

    /// <summary>
    /// ゴール壁の移動速度
    /// </summary>
    private float currentGoalWallMoveSpeed;

    /// <summary>
    /// ステージ1ゴール壁の移動速度
    /// </summary>
    private const float stage01GoalWallMoveSpeed = 0.25f;

    /// <summary>
    /// ステージ2ゴール壁の移動速度
    /// </summary>
    private const float stage02GoalWallMoveSpeed = 0.5f;

    /// <summary>
    /// ステージ3ゴール壁の移動速度
    /// </summary>
    private const float stage03GoalWallMoveSpeed = 0.5f;

    /// <summary>
    /// ステージ4ゴール壁の移動速度
    /// </summary>
    private const float stage04GoalWallMoveSpeed = 0.5f;

    /// <summary>
    /// メモ
    /// ステージ1の場合：(X:56,Y:10,Z:68)
    /// </summary>
    [Header("移動後のゴール壁位置(子オブジェクトに鳴っていない場合の位置を設定する必要がある)")]
    [SerializeField] private Vector3 movedGoalWallPosition;


    /// <summary>
    /// Stage01(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage01Scene = "Stage01";

    /// <summary>
    /// Stage02(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage02Scene = "Stage02";

    /// <summary>
    /// Stage03(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage03Scene = "Stage03";

    /// <summary>
    /// Stage04(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage04Scene = "Stage04";

    /// <summary>
    /// 正解のミステリーアイテムを選択したかどうかのフラグ
    /// </summary>
    private bool isChoosedAnser;

    /// <summary>
    /// ゴール壁が移動中であるかどうかのフラグ
    /// </summary>
    private bool isMovingGoalWall;

    /// <summary>
    /// ゴールパネルフラグ
    /// </summary>
    private bool isGoalPanel;

    /// <summary>
    /// ゴールパネルフラグを取得する
    /// </summary>
    /// <returns>ゴールパネルフラグ</returns>
    public bool GetIsGoalPanel()
    {
        return isGoalPanel;
    }

    /// <summary>
    /// ゴールパネルフラグを設定する
    /// </summary>
    /// <param name="value">ゴールパネルフラグ</param>
    public void SetIsGoalPanel(bool value)
    {
        isGoalPanel = value;
    }

    [Header("チュートリアルフラグ(チュートリアルステージでオンになる)")]
    [SerializeField] public bool isTutorial;

    /// <summary>
    /// ゴール壁が移動中であるかどうかのフラグを取得する
    /// </summary>
    /// <returns>ゴール壁が移動中であるかどうかのフラグ</returns>
    public bool GetIsMovingGoalWall() 
    {
        return isMovingGoalWall;
    }

    /// <summary>
    /// ゴールパネルを閉じたフレーム番号
    /// </summary>
    private int lastClosedGoalPanelFrame = -100;

    /// <summary>
    /// ゴールパネルを閉じた後、再度開けないようにするフレーム数
    /// (コントローラーの×/Aで「戻る」を押すと、同じボタン入力でPlayerInteractのインタラクトも反応し、
    /// 閉じた直後に再度ゴールパネルが開いてしまうのを防ぐため)
    /// </summary>
    private const int kReopenBlockFrames = 1;


    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        for (int i = 0; i < selectMysteryItemImage.Length; i++) 
        {
            //ミステリーアイテム画像が存在する場合
            if (selectMysteryItemImage[i] != null)
            {
                //ミステリーアイテム画像をnullにする
                selectMysteryItemImage[i].sprite = null;
                selectMysteryItemImage[i].enabled = false;
            }
        }

        for (int i = 0; i < selectMysteryItemButton.Length; i++) 
        {
            //ミステリーアイテムボタンが存在する場合
            if (selectMysteryItemButton[i] != null) 
            {
                //ミステリーアイテムボタンをnullにする
                selectMysteryItemButton[i] = null;
            }
        }

        //ゴールパネルが存在する場合
        if (GoalPanel != null) 
        {
            //ゴールパネルをnullにする
            GoalPanel = null;
        }
        
        //ゴール壁が存在する場合
        if (goalWall != null) 
        {
            //ゴール壁をnullにする
            goalWall = null;
        }

        //「戻る」ボタンをnullにする
        returnToInGameButton = null;
    }

    private void Start()
    {
        //正解のミステリーアイテムを選択したフラグをオフにする
        isChoosedAnser = false;

        //ゴール壁移動中フラグをオンにする
        isMovingGoalWall = false;

        //ゴールパネルを非表示
        isGoalPanel = false;
        ViewGoalPanel();

        //ゴールパネル内のミステリーアイテムのUIを初期化
        InitializeSelectMysteryItemUI();

        //マウスホバー・コントローラー選択時に、ミステリーアイテム画像を半透明にするイベントを設定
        //(selectMysteryItemImageはInitializeSelectMysteryItemUI内で取得するため、その後に呼ぶ)
        if (CommonController.instance != null)
        {
            CommonController.instance.SetupButtonHoverEvents(selectMysteryItemButton, selectMysteryItemImage);
        }
    }


    private void Update()
    {
        //正解のミステリーアイテムを選択している場合
        if (isChoosedAnser && isMovingGoalWall) 
        {
            //ゴール壁のTransformコンポーネントを取得
            Transform moveObjectTransform = goalWall.GetComponent<Transform>();

            //ゴール壁を目的の位置に移動
            moveObjectTransform.position = Vector3.Lerp(moveObjectTransform.position, movedGoalWallPosition, currentGoalWallMoveSpeed * Time.deltaTime);

            //ゴール壁が目的の位置に到達した場合
            if (moveObjectTransform.position == movedGoalWallPosition) 
            {
                //ゴール壁移動中フラグをオフにする
                isMovingGoalWall = false;
            }
        }

        //B / ○でUIの「戻る」
        if (GamepadInput.IsCancelPressed() && isGoalPanel
            && GameController.instance.GetGameModeStatus() == GameModeStatus.PlayInGame && Player.instance.GetDieMode() != DieMode.Die)
        {
            OnClickedReturnToInGameButton();

            return;
        }
    }

    /// <summary>
    /// プレイヤーがゴールオブジェクトに触れた場合の処理
    /// </summary>
    public async void GoalCheck()
    {
        //ゴールパネルを閉じた直後の場合
        //(×/Aで「戻る」を押した入力で、同じフレーム内にインタラクトが反応して再度開くのを防ぐ)
        if (Time.frameCount - lastClosedGoalPanelFrame <= kReopenBlockFrames)
        {
            //処理をスキップ
            return;
        }

        //アイテムのnullチェック
        sO_Item.CleanNullItems();

        //既に正解のミステリーアイテムを選択している場合
        if (isChoosedAnser) 
        {
            //処理をスキップ
            return;
        }

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
    private void MysteryItemCheck() 
    {
        //ミステリーアイテムを選択する旨のメッセージを表示
        isGoalPanel = true;
        MessageController.instance.ShowGoalMessage(3);

        //MysteryItemはUpdateSelectMysteryItemUIで処理するため、ここでは保持しない
        List<SO_Item.ItemData> mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
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
            //プレイヤー効果音を停止
            MusicController.instance.StopSE(Player.instance.audioSourceSE);

            //一時停止
            Time.timeScale = 0;

            //マウスカーソルを有効にし、固定を解除
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

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

            //コントローラーでの選択枠の移動先を設定(ミステリーアイテムの入手数でボタンの押せる・押せないが変わるため、表示のたびに設定する)
            SetupGoalPanelNavigation();

            //コントローラー接続時は最初のボタンを選択する
            if (GamepadInput.IsConnected)
            {
                StartCoroutine(SelectFirstGoalPanelButtonNextFrame());
            }
        }
        else
        {
            //ゴールパネル内のボタンが選択されたままの場合は選択を解除する
            //(Deselectイベントが呼ばれるため、ミステリーアイテム画像の半透明も元に戻る)
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != null
                && eventSystem.currentSelectedGameObject.transform.IsChildOf(GoalPanel.transform))
            {
                eventSystem.SetSelectedGameObject(null);
            }

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
    /// コントローラー(十字キー・スティック)での選択枠の移動先を設定する
    /// (プレハブ側のNavigation設定に関係なく、ミステリーアイテムと「戻る」ボタンの間を確実に移動できるようにする)
    ///
    ///   [アイテム1] ←→ [アイテム2] ←→ ...
    ///         ↓↑
    ///      [ 戻る ]
    /// </summary>
    private void SetupGoalPanelNavigation()
    {
        //押せるミステリーアイテムボタンを並び順に取得
        List<Button> activeButtons = new List<Button>();
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (selectMysteryItemButton[i] != null && selectMysteryItemButton[i].interactable)
            {
                activeButtons.Add(selectMysteryItemButton[i]);
            }
        }

        //ミステリーアイテムボタン:左右→隣のアイテム、下→「戻る」ボタン
        for (int i = 0; i < activeButtons.Count; i++)
        {
            Selectable left = i > 0 ? activeButtons[i - 1] : null;
            Selectable right = i < activeButtons.Count - 1 ? activeButtons[i + 1] : null;
            SetExplicitNavigation(activeButtons[i], null, returnToInGameButton, left, right);
        }

        //「戻る」ボタン:上→最初のアイテム
        Selectable firstItem = activeButtons.Count > 0 ? activeButtons[0] : null;
        SetExplicitNavigation(returnToInGameButton, firstItem, null, null, null);
    }

    /// <summary>
    /// ボタンの移動先を手動(Explicit)で設定する
    /// </summary>
    /// <param name="target">設定するボタン</param>
    /// <param name="up">上の移動先(無い場合はnull)</param>
    /// <param name="down">下の移動先(無い場合はnull)</param>
    /// <param name="left">左の移動先(無い場合はnull)</param>
    /// <param name="right">右の移動先(無い場合はnull)</param>
    private void SetExplicitNavigation(Selectable target, Selectable up, Selectable down, Selectable left, Selectable right)
    {
        //ボタンが設定されていない場合
        if (target == null)
        {
            //処理をスキップ
            return;
        }

        //Navigationは構造体のため、コピーして書き換えてから代入し直す
        Navigation navigation = target.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        navigation.selectOnLeft = left;
        navigation.selectOnRight = right;
        target.navigation = navigation;
    }

    /// <summary>
    /// 1フレーム待ってから、ゴールパネルの最初のボタン(押せる最初のミステリーアイテム。無ければ「戻る」ボタン)を選択する
    /// (yield return nullはTime.timeScale=0でも進むため、一時停止中でも動作する)
    /// </summary>
    private IEnumerator SelectFirstGoalPanelButtonNextFrame()
    {
        yield return null;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || !isGoalPanel)
        {
            yield break;
        }

        //既にゴールパネル内のボタンが選択されている場合はそのままにする
        GameObject current = eventSystem.currentSelectedGameObject;
        if (current != null && current.activeInHierarchy && current.transform.IsChildOf(GoalPanel.transform))
        {
            yield break;
        }

        //押せる最初のミステリーアイテムボタンを選択
        foreach (Button button in selectMysteryItemButton)
        {
            if (button != null && button.IsInteractable() && button.gameObject.activeInHierarchy)
            {
                eventSystem.SetSelectedGameObject(button.gameObject);
                yield break;
            }
        }

        //ミステリーアイテムが無い場合は「戻る」ボタンを選択
        if (returnToInGameButton != null && returnToInGameButton.IsInteractable() && returnToInGameButton.gameObject.activeInHierarchy)
        {
            eventSystem.SetSelectedGameObject(returnToInGameButton.gameObject);
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

        //閉じたフレームを記録(同じボタン入力で再度開かないようにするため)
        lastClosedGoalPanelFrame = Time.frameCount;

        //チュートリアルの場合、ストーリーモードへ変更(ゴールオブジェクトへのインタラクト操作を高速で行うと、ゲーム進行不可になるバグを防ぐ用)
        if (GameController.instance.GetIsTutorialGoalFlag()) 
        {
            //ストーリーモードへ変更
            GameController.instance.SetGameModeStatus(GameModeStatus.Story);
        }
            
        //メッセージテキストを空にする
        MessageController.instance.ResetMessage();

        //チュートリアルの場合、チュートリアル終了の旨のメッセージを表示する
        if (GameController.instance.GetIsTutorialGoalFlag())
        {
            await MessageController.instance.ShowSystemMessage(14); 
        }
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
        List<SO_Item.ItemData> mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        if (index < mysteryItems.Count && mysteryItems[index] != null)
        {
            //正解のミステリーアイテムであるかを判定
            if (mysteryItems[index].id == anserItemId)
            {
                //現在のシーン名を取得
                switch (SceneManager.GetActiveScene().name) 
                {
                    //ステージ1の場合
                    case stringStage01Scene:

                        //ステージ1のゴール壁移動速度を設定
                        currentGoalWallMoveSpeed = stage01GoalWallMoveSpeed;
                        break;

                    //ステージ2の場合
                    case stringStage02Scene:

                        //ステージ2のゴール壁移動速度を設定
                        currentGoalWallMoveSpeed = stage02GoalWallMoveSpeed;
                        break;

                    //ステージ3の場合
                    case stringStage03Scene:

                        //ステージ3のゴール壁移動速度を設定
                        currentGoalWallMoveSpeed = stage03GoalWallMoveSpeed;
                        break;

                    //ステージ4の場合
                    case stringStage04Scene:

                        //ステージ4のゴール壁移動速度を設定
                        currentGoalWallMoveSpeed = stage04GoalWallMoveSpeed;
                        break;
                }


                //正解のミステリーアイテムを選択したフラグをオンにする
                isChoosedAnser = true;

                //ゴール壁移動中フラグをオンにする
                isMovingGoalWall = true;

                //パネルを非表示にする(戻るボタン押下時と同じ処理を発動)
                OnClickedReturnToInGameButton();
            }
            //正解のミステリーアイテム(チュートリアル版)であるかを判定
            else if (mysteryItems[index].id == anserTutorialItemId) 
            {
                //チュートリアル終了後の会話を進める
                GameController.instance.SetIsTutorialGoalFlag(true);
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
        List<SO_Item.ItemData> mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

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
