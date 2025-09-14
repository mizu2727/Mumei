using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PauseController : MonoBehaviour
{
    public static PauseController instance;

    [Header("プレイヤー")]
    [SerializeField] private Player player;//プレイヤー

    [Header("ゴール(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Goal goal;//ゴール

    [Header("敵(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private BaseEnemy[] baseEnemy;

    [Header("ポーズパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject pausePanel;

    [Header("アイテム確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject viewItemsPanel;

    [Header("ドキュメントパネル関連(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject documentInventoryPanel;//ドキュメント確認パネル
    [SerializeField] private GameObject documentExplanationPanel;//ドキュメント説明欄パネル
    [SerializeField] private Text documentNameText;//ドキュメント名称テキスト
    [SerializeField] private Text documentExplanationText;//ドキュメント説明欄テキスト

    [Header("ミステリーアイテムパネル関連(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mysteryItemInventoryPanel;//ミステリーアイテム確認パネル
    [SerializeField] private Button[] mysteryItemNameButton;//ミステリーアイテム名称ボタン
    [SerializeField] private Text[] mysteryItemNameText;//ミステリーアイテム名称テキスト
    [SerializeField] private Image[] mysteryItemImage;//ミステリーアイテム画像
    [SerializeField] private Text[] mysteryItemExplanationText;//ミステリーアイテム説明欄テキスト
    [SerializeField] private GameObject mysteryItemExplanationPanel;//ミステリーアイテム説明欄パネル

    [Header("オプションパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject optionPanel;

    [Header("タイトルへ戻るパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToTitlePanel;

    public bool isPause = false;
    public bool isViewItemsPanel = false;
    public bool isOptionPanel = false;
    public bool isReturnToTitlePanel = false;
    public bool isDocumentPanels = false;
    public bool isDocumentExplanationPanel = false;
    public bool isMysteryItemPanels = false;
    public bool isMysteryItemExplanationPanel = false;

    [Header("チュートリアル用フラグ(編集禁止)")]
    public bool isTutorialNextMessageFlag = false;

    [Header("チュートリアル用ハンマー入手フラグ(編集禁止)")]
    public bool isGetHammer_Tutorial = false;

    [Header("チュートリアル用ロープ入手フラグ(編集禁止)")]
    public bool isGetRope_Tutorial = false;

    [Header("チュートリアル用ミステリーアイテム閲覧入手フラグ(編集禁止)")]
    public bool isViewMysteryItem_Tutorial = false;

    private List<int> mysteryItemIds = new(); // ミステリーアイテムIDのリスト
    private List<string> mysteryItemNames = new(); // ミステリーアイテム名のリスト
    private List<string> mysteryItemExplanations = new(); // ミステリーアイテム説明欄のリスト

    // チュートリアル用ハンマーID
    private const int hammer_TutorialID = 9;

    // チュートリアル用ロープID
    private const int rope_TutorialID = 10;

    // チュートリアル用ドキュメントID
    private const int documentBook_TutorialID = 7;

    //初期化するのドキュメントID
    private const int defaultDocumentBookID = 99999;

    //ドキュメントID
    private int keepDocumentBookID;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("サウンド関連")]
    private AudioSource audioSourceSE;
    private readonly int documentNameButtonSEid = 3;//ドキュメント名称ボタンSE
    private readonly int buttonSEid = 4;//ボタンSEのID
    

    [Header("Input Actions")]
    public GameInput gameInput;


    private void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時に破棄されないようにする（必要に応じて）
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は破棄
        }

        gameInput = new GameInput();

        // アクションにコールバックを登録
        gameInput.Gameplay.PressPlusButton.performed += OnPlusButtonPressed;

        // Input Systemを有効にする
        gameInput.Enable(); 
    }

    /// <summary>
    /// Playerの効果音が鳴らないバグを防止用。Input Systemを有効にする
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        gameInput.Enable();
    }

    /// <summary>
    /// Playerの効果音が鳴らないバグを防止用。Input Systemとシーン遷移のコールバックを無効にする
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        gameInput.Disable();
    }

    /// <summary>
    /// Playerの効果音が鳴らないバグを防止用。シーン遷移時にPlayer参照を更新する
    /// </summary>
    /// <param name="scene">シーン名</param>
    /// <param name="mode">シーンモード</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Player.instance != null)
        {
            player = Player.instance;
        }
        else
        {
            Debug.LogWarning("Player instance is null in scene: " + scene.name);
        }


        isTutorialNextMessageFlag = false;

        isGetHammer_Tutorial = false;

        isGetRope_Tutorial = false;

        isViewMysteryItem_Tutorial = false;

        keepDocumentBookID = defaultDocumentBookID;
    }



    private void Start()
    {
        audioSourceSE = MusicController.Instance.GetAudioSource();

        // パネルを初期状態で非表示に
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        isOptionPanel = false;
        ChangeOptionPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();

        isTutorialNextMessageFlag = false;

        isGetHammer_Tutorial = false;

        isGetRope_Tutorial = false;

        isViewMysteryItem_Tutorial = false;

        // ミステリーアイテムのボタンとテキストを初期化
        InitializeMysteryItemUI();

        // ドキュメントIDを初期化
        keepDocumentBookID = defaultDocumentBookID;
    }


    //PキーorZキーでポーズ/ポーズ解除
    public void Update()
    {
        if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Z)) && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) TogglePause();
    }

    //コントローラーの+ボタンでポーズ/ポーズ解除
    private void OnPlusButtonPressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// ポーズ画面の表示/非表示を切り替える
    /// </summary>
    private void TogglePause()
    {
        // ポーズを開く条件
        if (!player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanels && !isDocumentExplanationPanel && !isMysteryItemPanels
            && !isMysteryItemExplanationPanel && !goal.isGoalPanel && Time.timeScale != 0)
        {
            ViewPausePanel();
        }
        // ポーズを閉じる条件
        else if (!player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
        }
    }

    //ポーズ
    public void ViewPausePanel() 
    {
        isPause = true;
        Time.timeScale = 0;
        pausePanel.transform.SetAsLastSibling();
        ChangeViewPausePanel();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MusicController.Instance.PauseBGM();

        if (Player.instance != null && Player.instance.audioSourceSE != null)
        {
            MusicController.Instance.PauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);
        }
        else
        {
            Debug.LogWarning("Player or AudioSource is null in ViewPausePanel");
        }

        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            if (baseEnemy[i] != null && baseEnemy[i].audioSourceSE != null)
            {
                MusicController.Instance.PauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }
        }

        
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
    }

    //ポーズ解除
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            Time.timeScale = 1;
            isPause = false;
            ChangeViewPausePanel();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
            MusicController.Instance.UnPauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);

            for (int i = 0; i < baseEnemy.Length; i++)
            {
                if (baseEnemy[i] != null) MusicController.Instance.UnPauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }

            MusicController.Instance.UnPauseBGM();
        }
        
    }


    //アイテム確認
    public void OnClickedViewItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isPause = false;
        ChangeViewPausePanel();


        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    //オプション設定
    public void OnClickedOptionButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isOptionPanel = true;
        ChangeOptionPanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    //オプション設定からポーズ画面へ戻る
    public void OnClickedFromOptionToPauseButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        isPause = true;
        ChangeViewPausePanel();

        isOptionPanel = false;
        ChangeOptionPanel();
    }

    //タイトルへ戻る
    public void OnClickedReturnToTitleButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    //「はい」押下
    public void OnClickedYesButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        MusicController.Instance.StopBGM();
        //GameController.instance.ReturnToTitle();
        SceneManager.LoadScene("TitleScene");
    }

    //「いいえ」押下
    public void OnClickedNoButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        isPause = true;
        ChangeViewPausePanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }



    public void OnClickedViewDocumentButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        // ドキュメントパネルを表示
        isDocumentPanels = true;
        ChangeViewDocumentPanel();

        // ミステリーアイテムパネルを非表示
        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    public void OnClickedViewMysteryItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        // ミステリーアイテムパネルを表示
        isMysteryItemPanels = true;
        ChangeViewMysteryItemPanel();

        // ドキュメントパネルを非表示
        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        // 画像と説明テキストをクリア
        if (mysteryItemImage.Length > 0)
        {
            mysteryItemImage[0].sprite = null;
            mysteryItemImage[0].enabled = false;
        }
        if (mysteryItemExplanationText.Length > 0)
        {
            mysteryItemExplanationText[0].text = "";
        }
    }


    //ポーズ画面へ戻る
    public void OnClickedReturnToPausePanel()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// ドキュメント名称ボタン押下時
    /// </summary>
    public void OnClickedDocumentNameButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(documentNameButtonSEid));

        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();

        if (keepDocumentBookID == documentBook_TutorialID) 
        {
            isTutorialNextMessageFlag = true;
        }
    }


    //ポーズパネルの表示/非表示
    void ChangeViewPausePanel()
    {
        if (isPause)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }


    //アイテム系確認パネルの表示/非表示
    void ChangeViewItemsPanel() 
    {
        if (isViewItemsPanel)
        {
            viewItemsPanel.SetActive(true);
        }
        else
        {
            viewItemsPanel.SetActive(false);
        }
    }

    //オプションパネルの表示/非表示
    void ChangeOptionPanel()
    {
        if (isOptionPanel)
        {
            optionPanel.SetActive(true);
        }
        else
        {
            optionPanel.SetActive(false);
        }
    }

    //タイトルへ戻るパネルの表示/非表示
    void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            returnToTitlePanel.SetActive(false);
        }
    }


    //ドキュメントアイテムパネルの表示/非表示
    void ChangeViewDocumentPanel() 
    {
        if (isDocumentPanels)
        {
            documentInventoryPanel.transform.SetAsLastSibling();
            documentInventoryPanel.SetActive(true);

        }
        else 
        {
            documentInventoryPanel.SetActive(false);

            isDocumentExplanationPanel = false;

            ChangeViewDocumentExplanationPanel();
        }
    }

    //ドキュメント説明欄パネルの表示/非表示
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            documentExplanationPanel.SetActive(false);
        }
    }


    /// <summary>
    /// DocumentNameTextの記載内容を変更
    /// </summary>
    /// <param name="documentId">取得したid</param>
    /// <param name="documentName">変更先の記載内容</param>
    public void ChangeDocumentNameText(int documentId, string documentName) 
    {
        // チュートリアル用ドキュメントの場合
        if (documentId == documentBook_TutorialID) 
        {
            
            isTutorialNextMessageFlag = true;
            keepDocumentBookID = documentId;
        }

        documentNameText = documentNameText.GetComponent<Text>();
        documentNameText.text = documentName;
    }

    //DocumentExplanationTextの記載内容を変更
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        documentExplanationText = documentExplanationText.GetComponent<Text>();
        documentExplanationText.text = documentDescription;
        Debug.Log("ドキュメント説明欄："+ documentExplanationText.text);
    }


    //ミステリーアイテム確認パネルの表示/非表示
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanels)
        {
            mysteryItemInventoryPanel.transform.SetAsLastSibling();
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            mysteryItemInventoryPanel.SetActive(false);

            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();

            // 画像と説明テキストをリセット
            if (mysteryItemImage.Length > 0)
            {
                mysteryItemImage[0].sprite = null;
                mysteryItemImage[0].enabled = false;
            }
            if (mysteryItemExplanationText.Length > 0)
            {
                mysteryItemExplanationText[0].text = "";
            }
        }
    }


    //ミステリーアイテム説明欄パネルの表示/非表示
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            mysteryItemExplanationPanel.SetActive(false);
        }
    }


    // ミステリーアイテムのUIを初期化
    private void InitializeMysteryItemUI()
    {
        if (mysteryItemNameButton == null || mysteryItemNameText == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        // ボタンにクリックイベントを追加
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            int index = i; // ローカル変数でインデックスをキャプチャ
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));
            mysteryItemNameText[i].text = "?????????";
        }
    }

    /// <summary>
    /// ミステリーアイテム名称ボタン押下時
    /// </summary>
    /// <param name="index"></param>
    public void OnClickedMysteryItemNameButton(int index)
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        if (isGetHammer_Tutorial && isGetRope_Tutorial)
        {
            isViewMysteryItem_Tutorial = true;
        }

        if (index < mysteryItemNames.Count)
        {
            string itemName = mysteryItemNames[index];
            var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

            if (item != null)
            {
                // ミステリーアイテム説明パネルを表示
                isMysteryItemExplanationPanel = true;
                ChangeViewMysteryItemExplanationPanel();

                // ドキュメント説明パネルを非表示にする
                isDocumentExplanationPanel = false;
                ChangeViewDocumentExplanationPanel();

                // 説明テキストを更新
                if (mysteryItemExplanationText.Length > 0)
                {
                    mysteryItemExplanationText[0].text = item.description;
                    Debug.Log($"Set explanation text to: {item.description}");
                }

                // 画像を更新
                if (mysteryItemImage.Length > 0)
                {
                    mysteryItemImage[0].sprite = item.icon;
                    mysteryItemImage[0].enabled = (item.icon != null);
                    Debug.Log($"Set image to: {(item.icon != null ? item.icon.name : "null")}");
                }
                else
                {
                    Debug.LogWarning("mysteryItemImage が未設定です");
                }

                Debug.Log($"Clicked Mystery Item: {itemName}");
            }
            else
            {
                Debug.LogError($"アイテム '{itemName}' が見つかりません");
            }
        }
    }


    // ミステリーアイテム名を追加し、UIに反映
    public void ChangeMysteryItemTexts(int mysteryItemID, string mysteryItemName, string mysteryItemDescription)
    {
        mysteryItemIds.Add(mysteryItemID);

        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            if (mysteryItemIds[i] == hammer_TutorialID) 
            {
                isGetHammer_Tutorial = true;
                Debug.Log("isGetHammer_Tutorial" + isGetHammer_Tutorial);
            }

            if (mysteryItemIds[i] == rope_TutorialID)
            {
                isGetRope_Tutorial = true;
                Debug.Log("rope_TutorialID" + rope_TutorialID);
            }
        }

        // アイテムリストから該当するアイテムを検索
        var item = sO_Item.itemList.Find(x => x.itemName == mysteryItemName && x.itemType == ItemType.MysteryItem);
        if (item != null && !mysteryItemNames.Contains(mysteryItemName))
        {
            mysteryItemNames.Add(mysteryItemName);
            mysteryItemExplanations.Add(mysteryItemDescription);
            UpdateMysteryItemUI();
        }
        else 
        {
            Debug.LogWarning($"MysteryItem '{mysteryItemName}' が見つからないか、すでに追加済みです");
        }
    }

    // ミステリーアイテムのUIを更新
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            if (i < mysteryItemNames.Count)
            {
                string itemName = mysteryItemNames[i];
                var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

                if (item != null)
                {
                    mysteryItemNameText[i].text = itemName;
                    mysteryItemNameButton[i].interactable = true;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        mysteryItemImage[i].sprite = item.icon;
                        mysteryItemImage[i].enabled = (item.icon != null);
                    }
                }
                else
                {
                    Debug.LogWarning($"アイテム '{itemName}' が見つかりません");
                    mysteryItemNameText[i].text = "?????????";
                    mysteryItemNameButton[i].interactable = false;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        mysteryItemExplanationText[i].text = "";
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        mysteryItemImage[i].sprite = null;
                        mysteryItemImage[i].enabled = false;
                    }
                }
            }
            else
            {
                mysteryItemNameText[i].text = "?????????";
                mysteryItemNameButton[i].interactable = false;

                if (i < mysteryItemExplanationText.Length)
                {
                    mysteryItemExplanationText[i].text = "";
                }

                if (i < mysteryItemImage.Length)
                {
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
    }
}
