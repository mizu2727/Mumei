using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMP_Ruby;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static Player;
using static UnityEditor.Progress;

/// <summary>
/// ポーズ画面管理クラス
/// </summary>
public partial class PauseController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PauseController instance;

    /// <summary>
    /// TutorialClearStatus(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringTutorialClearStatus = "TutorialClearStatus";

    /// <summary>
    /// デモ版用静声に熱する彷徨う者のステータス(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringDemoVeinVainWandererStatus = "DemoVeinVainWanderer";

    /// <summary>
    /// 静声に熱する彷徨う者のステータス(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringVeinVainWandererStatus = "VeinVainWanderer";

    /// <summary>
    /// 微美しき魅忘の彷徨う者のステータス(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringBeauteousBewilderWandererStatus = "BeauteousBewilderWanderer";

    /// <summary>
    /// 唄歌う彷徨う者のステータス(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringSingSongWandererStatus = "SingSongWanderer";


    [Header("プレイヤー(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Player player;

    [Header("ゴール(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Goal goal;

    [Header("敵(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private BaseEnemy[] baseEnemy;


    /*-----------------------------------------------------------
     * ポーズパネル関連(アイテム確認パネルの子パネルも含む)
     ----------------------------------------------------------*/

    [Header("ポーズパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject pausePanel;

    /// <summary>
    /// pausePanelを取得
    /// </summary>
    /// <returns>pausePanel</returns>
    public GameObject GetPausePanel()
    {
        return pausePanel;
    }


    /*-----------------------------------------------------------
     * アイテム確認パネル関連(アイテム確認パネルの子パネルも含む)
     ----------------------------------------------------------*/

    [Header("アイテム確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject viewItemsPanel;

    Color32 kDefaultViewItemsPanelColor = new Color32(0, 0, 0, 210);

    Color32 kViewItemsPanelBlackColor = new Color32(0, 0, 0, 255);

    /*-----------------------------------------------------------
     * アーカイブパネル関連(アーカイブパネルの子パネルも含む)
     ----------------------------------------------------------*/

    [Header("アーカイブパネル関連")]
    [Header("アーカイブパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject archivePanel;

    [Header("アーカイブボタンテキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text archiveButtonText;

    /// <summary>
    /// アーカイブボタンテキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby archiveButtonTextRubyComponent;

    [Header("彷徨う者関連パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject wandererPanel;

    [Header("彷徨う者ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject wandererButton;

    /// <summary>
    /// 彷徨う者ボタンを表示/非表示する
    /// </summary>
    /// <param name="isActive">表示/非表示</param>
    public void SetWandererButtonActive(bool isActive)
    {
        wandererButton.SetActive(isActive);
    }

    [Header("彷徨う者ボタンテキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text wandererButtonText;

    /// <summary>
    /// 彷徨う者ボタンテキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby wandererButtonTextRubyComponent;

    [Header("彷徨う者名称ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Button[] wandererNameButton;

    [Header("彷徨う者名称テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text[] wandererNameText;

    /// <summary>
    /// 彷徨う者名称テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby[] wandererNameTextRubyComponent;

    /// <summary>
    /// デフォルトの彷徨う者名称テキストサイズ
    /// </summary>
    private const int kDefaultWandererNameTextSize = 14;

    [Header("彷徨う者説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject wandererExplanationPanel;

    [Header("彷徨う者説明欄テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text[] wandererExplanationText;

    /// <summary>
    /// 彷徨う者説明欄テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby[] wandererExplanationTextRubyComponent;

    /// <summary>
    /// デフォルトの彷徨う者名称テキストサイズ
    /// </summary>
    private const int kDefaultWandererExplanationTextSize = 14;


    /*-----------------------------------------------------------
     * ドキュメントパネル関連(ドキュメントパネルの子パネルも含む)
     ----------------------------------------------------------*/

    [Header("ドキュメントパネル関連")]
    [Header("ドキュメント確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject documentInventoryPanel;

    [Header("ドキュメント説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject documentExplanationPanel;

    [Header("ドキュメント名称テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text documentNameText;

    /// <summary>
    /// ドキュメント名称テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby documentNameTextRubyComponent;

    [Header("ドキュメント説明欄テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text documentExplanationText;

    /// <summary>
    /// ドキュメント説明欄テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby documentExplanationTextRubyComponent;

    /// <summary>
    /// デフォルトのドキュメント名称テキストサイズ
    /// </summary>
    private const int kDefultDocumentNameTextSize = 14;


    /*-----------------------------------------------------------
     * ミステリーアイテムパネル関連(ミステリーアイテムパネルの子パネルも含む)
     ----------------------------------------------------------*/

    [Header("ミステリーアイテムパネル関連")]
    [Header("ミステリーアイテム確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mysteryItemInventoryPanel;

    [Header("ミステリーアイテム名称ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Button[] mysteryItemNameButton;

    [Header("ミステリーアイテム名称テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text[] mysteryItemNameText;

    /// <summary>
    /// ミステリーアイテム名称テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby[] mysteryItemNameTextRubyComponent;

    /// <summary>
    /// デフォルトのミステリーアイテム名称テキストサイズ
    /// </summary>
    private const int kDefultMysteryItemNameTextSize = 14;

    [Header("ミステリーアイテム画像(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Image[] mysteryItemImage;

    [Header("ミステリーアイテム説明欄テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private TMP_Text[] mysteryItemExplanationText;

    /// <summary>
    /// ミステリーアイテム説明欄テキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby[] mysteryItemExplanationTextRubyComponent;

    [Header("ミステリーアイテム説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mysteryItemExplanationPanel;


    /*-----------------------------------------------------------
     * リタイア関連
     -----------------------------------------------------------*/

    [Header("リタイアパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject retirePanel;

    [Header("ステージ選択へ戻るパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToSelectStagePanel;

    [Header("ステージ選択へ戻るボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToSelectStageButton;

    [Header("タイトルへ戻るパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToTitlePanel;


    /*-----------------------------------------------------------
     * フラグ関連
     -----------------------------------------------------------*/
    [Header("フラグ関連")]
    [Header("ポーズフラグ(ヒエラルキー上からの編集禁止)")]
    public bool isPause = false;

    /// <summary>
    /// アイテム確認パネル閲覧フラグ
    /// </summary>
    private bool isViewItemsPanel = false;

    /// <summary>
    /// アイテム確認パネル閲覧フラグを取得する
    /// </summary>
    /// <returns>アイテム確認パネル閲覧フラグ</returns>
    public bool GetIsViewItemsPanel()
    {
        return isViewItemsPanel;
    }

    /// <summary>
    /// リタイアパネル閲覧フラグ
    /// </summary>
    private bool isRetirePanel = false;

    /// <summary>
    /// ステージ選択へ戻るパネル閲覧フラグ
    /// </summary>
    private bool isReturnToSelectStagePanel = false;

    /// <summary>
    /// タイトルへ戻るパネル閲覧フラグ
    /// </summary>
    private bool isReturnToTitlePanel = false;

    /// <summary>
    /// タイトルへ戻るパネル閲覧フラグを取得する
    /// </summary>
    /// <returns>タイトルへ戻るパネル閲覧フラグ</returns>
    public bool IsReturnToTitlePanel()
    {
        return isReturnToTitlePanel;
    }

    /// <summary>
    /// アーカイブパネル閲覧フラグ
    /// </summary>
    private bool isArchivePanel = false;

    /// <summary>
    /// 彷徨う者パネル閲覧フラグ
    /// </summary>
    private bool isWandererPanel = false;

    /// <summary>
    /// 彷徨う者説明欄パネル閲覧フラグ
    /// </summary>
    private bool isWandererExplanationPanel = false;

    /// <summary>
    /// ドキュメントパネル閲覧フラグ
    /// </summary>
    private bool isDocumentPanel = false;

    /// <summary>
    /// ドキュメント説明欄パネル閲覧フラグ
    /// </summary>
    private bool isDocumentExplanationPanel = false;

    /// <summary>
    /// ミステリーアイテムパネル閲覧フラグ
    /// </summary>
    private bool isMysteryItemPanel = false;

    /// <summary>
    /// ミステリーアイテム説明欄パネル閲覧フラグ
    /// </summary>
    private bool isMysteryItemExplanationPanel = false;

    [Header("チュートリアル用ハンマー入手フラグ(編集禁止)")]
    public bool isGetHammer_Tutorial = false;

    [Header("チュートリアル用ロープ入手フラグ(編集禁止)")]
    public bool isGetRope_Tutorial = false;

    [Header("チュートリアル用ミステリーアイテム閲覧入手フラグ(編集禁止)")]
    public bool isViewMysteryItem_Tutorial = false;


    /// <summary>
    /// 彷徨う者関連情報IDのリスト
    /// </summary>
    private List<int> enemyInformationIds = new();

    /// <summary>
    /// デフォルトの1つ目の彷徨う者関連情報IDと連動するアイテムIDの値
    /// </summary>
    private const int kDefaultInterlockingOfFirstEnemyInformationIdAndItemId = 28;

    /// <summary>
    /// 彷徨う者関連情報名のリスト
    /// </summary>
    private List<string> enemyInformationNames = new();

    /// <summary>
    /// 彷徨う者関連情報説明欄のリスト
    /// </summary>
    private List<string> enemyInformationExplanations = new();

    /// <summary>
    /// 彷徨う者関連情報を未取得状態の場合のキー値
    /// </summary>
    private const int kDefaultSaveEnemyInformationKey = 99999;


    /// <summary>
    /// ミステリーアイテムIDのリスト
    /// </summary>
    private List<int> mysteryItemIds = new();

    /// <summary>
    /// ミステリーアイテム名のリスト
    /// </summary>
    private List<string> mysteryItemNames = new();

    /// <summary>
    /// ミステリーアイテム説明欄のリスト
    /// </summary>
    private List<string> mysteryItemExplanations = new();

    /// <summary>
    /// チュートリアル用ハンマーID
    /// </summary>
    private const int hammer_TutorialID = 9;

    /// <summary>
    /// チュートリアル用ロープID
    /// </summary>
    private const int rope_TutorialID = 10;

    /// <summary>
    /// チュートリアル用ドキュメントID
    /// </summary>
    private const int documentBook_TutorialID = 7;

    /// <summary>
    /// 初期化するのドキュメントID
    /// </summary>
    private const int defaultDocumentBookID = 99999;

    /// <summary>
    /// ドキュメントID
    /// </summary>
    private int keepDocumentBookID;


    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("ボタンテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ItemMessage itemMessage;

    /// <summary>
    /// アイテム入手前の名称表示用テキスト
    /// </summary>
    private string defaultItemName = "?????????";


    /// <summary>
    /// 現在再生されているBGMのID
    /// </summary>
    private int nowPlayBGMId = 99999;

    /// <summary>
    /// 対象のBGMに設定する
    /// </summary>
    /// <param name="subjectPlayBGMId_">対象のBGM</param>
    public void SetNowPlayBGMId(int subjectPlayBGMId_)
    {
        nowPlayBGMId = subjectPlayBGMId_;
    }

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE用audioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ドキュメント名称ボタンSEのID
    /// </summary>
    private readonly int documentNameButtonSEid = 3;

    /// <summary>
    /// ボタンSEのID
    /// </summary>
    private readonly int buttonSEid = 4;
    

    [Header("Input Actions")]
    public GameInput gameInput;

    /// <summary>
    /// 非同期タスクのキャンセル
    /// チュートリアル内のUniTask処理待機中にポーズ画面からタイトルへ戻る際のmessageTextでMissingReferenceExceptionエラーが起こるのを防止する用
    /// </summary>
    private CancellationTokenSource cts;


    /// <summary>
    /// オブジェクトが破棄される際に呼ばれる
    /// </summary>
    private void OnDestroy()
    {
        //goalが存在する場合
        if(goal != null)
        {
            //goalをnullにする
            goal = null;
        }

        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            //baseEnemyが存在する場合
            if (baseEnemy[i] != null)
            {
                //baseEnemyをnullにする
                baseEnemy[i] = null;
            }
        }

        //pausePanelが存在する場合
        if (pausePanel != null)
        {
            //pausePanelをnullにする
            pausePanel = null;
        }

        //viewItemsPanelが存在する場合
        if (viewItemsPanel != null)
        {
            //viewItemsPanelをnullにする
            viewItemsPanel = null;
        }

        if (wandererExplanationTextRubyComponent != null)
        {
            for (int i = 0; i < wandererExplanationTextRubyComponent.Length; i++)
            {
                //wandererExplanationTextRubyComponentが存在する場合
                if (wandererExplanationTextRubyComponent[i] != null)
                {
                    //wandererExplanationTextRubyComponentをnullにする
                    wandererExplanationTextRubyComponent[i] = null;
                }
            }
        }

        if (wandererNameTextRubyComponent != null)
        {
            for (int i = 0; i < wandererNameTextRubyComponent.Length; i++)
            {
                //wandererNameTextRubyComponentが存在する場合
                if (wandererNameTextRubyComponent[i] != null)
                {
                    //wandererNameTextRubyComponentをnullにする
                    wandererNameTextRubyComponent[i] = null;
                }
            }
        }

        //wandererButtonTextが存在する場合
        if (wandererButtonText != null) 
        {
            //wandererButtonTextをnullにする
            wandererButtonText = null;
        }

        //wandererButtonTextRubyComponentが存在する場合
        if (wandererButtonTextRubyComponent != null)
        {
            //wandererButtonTextRubyComponentをnullにする
            wandererButtonTextRubyComponent = null;
        }

        //wandererButtonが存在する場合
        if (wandererButton != null)
        {
            //wandererButtonをnullにする
            wandererButton = null;
        }

        //wandererPanelが存在する場合
        if (wandererPanel != null)
        {
            //wandererPanelをnullにする
            wandererPanel = null;
        }

        //archiveButtonTextが存在する場合
        if (archiveButtonText != null) 
        {
            //archiveButtonTextをnullにする
            archiveButtonText = null;
        }

        //archiveButtonTextRubyComponentが存在する場合
        if (archiveButtonTextRubyComponent != null)
        {
            //archiveButtonTextRubyComponentをnullにする
            archiveButtonTextRubyComponent = null;
        }

        //archivePanelが存在する場合
        if (archivePanel != null)
        {
            //archivePanelをnullにする
            archivePanel = null;
        }

        //documentNameTextRubyComponentが存在する場合
        if (documentNameTextRubyComponent != null)
        {
            //documentNameTextRubyComponentをnullにする
            documentNameTextRubyComponent = null;
        }

        //documentNameTextが存在する場合
        if (documentNameText != null)
        {
            //documentNameTextをnullにする
            documentNameText = null;
        }

        //documentExplanationTextRubyComponentが存在する場合
        if (documentExplanationTextRubyComponent != null)
        {
            //documentExplanationTextRubyComponentをnullにする
            documentExplanationTextRubyComponent = null;
        }

        //documentExplanationTextが存在する場合
        if (documentExplanationText != null)
        {
            //documentExplanationTextをnullにする
            documentExplanationText = null;
        }

        //documentExplanationPanelが存在する場合
        if (documentExplanationPanel != null)
        {
            //documentExplanationPanelをnullにする
            documentExplanationPanel = null;
        }

        //documentInventoryPanelが存在する場合
        if (documentInventoryPanel != null) 
        {
            //documentInventoryPanelをnullにする
            documentInventoryPanel = null;
        }

        if (mysteryItemExplanationTextRubyComponent != null) 
        {
            for (int i = 0; i < mysteryItemExplanationTextRubyComponent.Length; i++)
            {
                //mysteryItemExplanationTextRubyComponentが存在する場合
                if (mysteryItemExplanationTextRubyComponent[i] != null)
                {
                    //mysteryItemExplanationTextRubyComponentをnullにする
                    mysteryItemExplanationTextRubyComponent[i] = null;
                }
            }
        }

        if (mysteryItemExplanationText != null) 
        {
            for (int i = 0; i < mysteryItemExplanationText.Length; i++)
            {
                //mysteryItemExplanationTextが存在する場合
                if (mysteryItemExplanationText[i] != null)
                {
                    //mysteryItemExplanationTextをnullにする
                    mysteryItemExplanationText[i] = null;
                }
            }
        }

        if (mysteryItemNameTextRubyComponent != null)
        {
            for (int i = 0; i < mysteryItemNameTextRubyComponent.Length; i++)
            {
                //mysteryItemNameTextRubyComponentが存在する場合
                if (mysteryItemNameTextRubyComponent[i] != null)
                {
                    //mysteryItemNameTextRubyComponentをnullにする
                    mysteryItemNameTextRubyComponent[i] = null;
                }
            }
        }


        if (mysteryItemNameText != null) 
        {
            for (int i = 0; i < mysteryItemNameText.Length; i++)
            {
                //mysteryItemNameTextが存在する場合
                if (mysteryItemNameText[i] != null)
                {
                    //mysteryItemNameTextをnullにする
                    mysteryItemNameText[i] = null;
                }
            }
        }


        for (int i = 0; i < mysteryItemImage.Length; i++)
        {
            //mysteryItemImageが存在する場合
            if (mysteryItemImage[i] != null)
            {
                //mysteryItemImageをnullにする
                mysteryItemImage[i] = null;
            }
        }

        for (int i = 0; i < mysteryItemNameButton.Length; i++) 
        {
            //mysteryItemNameButtonが存在する場合
            if (mysteryItemNameButton[i] != null)
            {
                //mysteryItemNameButtonをnullにする
                mysteryItemNameButton[i] = null;
            }
        }

        //mysteryItemInventoryPanelが存在する場合
        if (mysteryItemExplanationPanel != null) 
        {
            //mysteryItemInventoryPanelをnullにする
            mysteryItemExplanationPanel = null;
        }

        //mysteryItemInventoryPanelが存在する場合
        if (mysteryItemInventoryPanel != null) 
        {
            //mysteryItemInventoryPanelをnullにする
            mysteryItemInventoryPanel = null;
        }

        //returnToTitlePanelが存在する場合
        if (returnToTitlePanel != null) 
        {
            //returnToTitlePanelをnullにする
            returnToTitlePanel = null;
        }

        //returnToSelectStageButtonが存在する場合
        if (returnToSelectStageButton != null) 
        {
            //returnToSelectStageButtonをnullにする
            returnToSelectStageButton = null;
        }

        //returnToSelectStagePanelが存在する場合
        if (returnToSelectStagePanel != null) 
        {
            //returnToSelectStagePanelをnullにする
            returnToSelectStagePanel = null;
        }

        //retirePanelが存在する場合
        if (retirePanel != null) 
        {
            //retirePanelをnullにする
            retirePanel = null;
        }

        //もしこのインスタンスがシングルトンインスタンス自身であれば、staticな参照をクリアする
        if (instance == this)
        {
            instance = null;
        }

        //インスタンスが存在する場合
        if (instance != null)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }
    private void Awake()
    {
        //シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyController();
        }

        //CancellationTokenSourceを初期化
        cts = new CancellationTokenSource();

        gameInput = new GameInput();

        //アクションにコールバックを登録
        gameInput.Gameplay.PressPlusButton.performed += OnPlusButtonPressed;

        //Input Systemを有効にする
        gameInput.Enable(); 
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;

        //Input Systemを有効にする
        gameInput.Enable();
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;

        //Input Systemを無効にする
        gameInput.Disable();

        //非同期タスクをキャンセル
        CancelAsyncTasks();
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Player.instance != null)
        {
            //Playerの効果音が鳴らないバグを防止用。シーン遷移時にPlayer参照を更新する
            player = Player.instance;
        }
        else
        {
            Debug.LogWarning("Player instance is null in scene: " + scene.name);
        }

        //フラグ値を初期化
        isGetHammer_Tutorial = false;
        isGetRope_Tutorial = false;
        isViewMysteryItem_Tutorial = false;

        //ドキュメントIDを初期化
        keepDocumentBookID = defaultDocumentBookID;
    }

    /// <summary>
    /// トークンをキャンセルして非同期タスクを中断
    /// </summary>
    public void CancelAsyncTasks()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }
    }

    private void Start()
    {
        audioSourceSE = MusicController.instance.GetAudioSource();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;

        if (documentNameText != null) 
        {
            //ドキュメントオブジェクトのTextMeshProRubyコンポーネントを取得する
            documentNameTextRubyComponent = documentNameText.GetComponent<TMP_Ruby.TextMeshProRuby>();

            //ドキュメント名称サイズを初期化する
            documentNameText.fontSize = kDefultDocumentNameTextSize;

            //ドキュメント名称を初期化
            documentNameText.text = defaultItemName;
            documentNameTextRubyComponent.Text = documentNameText.text;
        }

        //彷徨う者ボタンが存在する場合
        if (wandererButton != null) 
        {
            //チュートリアルのストーリーを閲覧済みの場合
            if (saveViewStoryStatusArray[stringTutorialClearStatus] == 1)
            {
                //彷徨う者ボタンを表示する
                wandererButton.SetActive(true);
            }
            else
            {
                //彷徨う者ボタンを非表示にする
                wandererButton.SetActive(false);
            }
        }
            

        //現在のシーン名がHomeSceneの場合
        if (CommonController.instance.GetHomeSceneName() == SceneManager.GetActiveScene().name)
        {
            //アイテム確認パネルの色をデフォルトの色に戻す
            viewItemsPanel.GetComponent<Image>().color = kDefaultViewItemsPanelColor;
        }
        else 
        {
            //アイテム確認パネルの色を黒にする
            viewItemsPanel.GetComponent<Image>().color = kViewItemsPanelBlackColor;
        }

        //ステージ選択へ戻るボタンが存在する場合
        if (returnToSelectStageButton != null) 
        {
            //現在のシーン名がHomeSceneの場合||現在のシーン名がHome02Sceneの場合
            if (CommonController.instance.GetHomeSceneName() == SceneManager.GetActiveScene().name
                || CommonController.instance.GetHome02SceneName() == SceneManager.GetActiveScene().name)
            {
                //ステージ選択へ戻るボタンを非表示にする
                returnToSelectStageButton.SetActive(false);
            }
            else
            {
                //ステージ選択へ戻るボタンを表示する
                returnToSelectStageButton.SetActive(true);
            }
        }


        //パネルを初期状態で非表示にする
        //フラグ値を初期化
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isArchivePanel = false;
        ChangeViewArchivePanel();

        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();

        isReturnToSelectStagePanel = false;
        ChangeReturnToSelectStagePanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();

        isRetirePanel = false;
        ChangeViewRetirePanel();

        isGetHammer_Tutorial = false;
        isGetRope_Tutorial = false;
        isViewMysteryItem_Tutorial = false;

        //彷徨う者関連情報IDのリストを事前に作成
        enemyInformationIds = Enumerable.Repeat(kDefaultSaveEnemyInformationKey, wandererNameButton.Length).ToList();

        //彷徨う者関連情報名のリストを事前に作成
        enemyInformationNames = Enumerable.Repeat(defaultItemName, wandererNameButton.Length).ToList();

        //彷徨う者関連情報説明欄のリストを事前に作成
        enemyInformationExplanations = Enumerable.Repeat("", wandererNameButton.Length).ToList();

        //彷徨う者関連情報のボタンとテキストを初期化
        InitializeWandererItemUI();

        //ミステリーアイテムのボタンとテキストを初期化
        InitializeMysteryItemUI();

        //ドキュメントIDを初期化
        keepDocumentBookID = defaultDocumentBookID;
    }


    
    public void Update()
    {
        //PキーorZキーorEscapeキーでポーズ/ポーズ解除
        if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape)) 
            && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.GetDieMode() != DieMode.Die) TogglePause();
    }

    /// <summary>
    /// コントローラーの+ボタンでポーズ/ポーズ解除
    /// </summary>
    /// <param name="context"></param>
    private void OnPlusButtonPressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// ポーズ画面の表示/非表示を切り替える
    /// </summary>
    private void TogglePause()
    {
        //ポーズを開く条件
        if (!player.IsDead && !isPause && !isViewItemsPanel && !isArchivePanel
            && !isDocumentPanel && !isDocumentExplanationPanel && !isMysteryItemPanel
            && !isMysteryItemExplanationPanel && !isRetirePanel
            && (CommonController.instance.GetHome02SceneName() == SceneManager.GetActiveScene().name || !goal.isGoalPanel) 
            && Time.timeScale != 0)
        {
            ViewPausePanel();
        }
        //ポーズを閉じる条件
        else if (!player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
        }
        //アイテム確認パネルを開いている場合||リタイアパネルを開いている場合||オプションパネルを開いている場合||
        else if (isViewItemsPanel || isRetirePanel || OptionUIController.instance.GetIsOptionPanel()) 
        {
            //各パネルを閉じる
            //アイテム確認パネルを非表示
            isViewItemsPanel = false;
            ChangeViewItemsPanel();

            //アーカイブパネルを非表示
            isArchivePanel = false;
            ChangeViewArchivePanel();

            //ドキュメントパネルを非表示
            isDocumentPanel = false;
            ChangeViewDocumentPanel();

            //ミステリーアイテムパネルを非表示
            isMysteryItemPanel = false;
            ChangeViewMysteryItemPanel();

            //リタイアパネルを非表示
            isRetirePanel = false;
            ChangeViewRetirePanel();

            //オプションパネルが表示されている場合は閉じる
            OptionUIController.instance.OnClickedCloseOptionButton();
        }
    }

    /// <summary>
    /// マウスカーソルを表示し、固定を解除するメソッド
    /// </summary>
    void ViewMouseCorsor() 
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// マウスを非表示にし、固定するメソッド
    /// </summary>
    void HideMouseCorsor() 
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// ポーズ
    /// </summary>
    public void ViewPausePanel() 
    {
        //ポーズフラグをオン
        isPause = true;

        //一時停止
        Time.timeScale = 0;

        //UIのレイヤーを手前側にする
        pausePanel.transform.SetAsLastSibling();

        //パネル表示
        ChangeViewPausePanel();

        //マウスカーソルを表示し、固定を解除
        ViewMouseCorsor();

        //再生中の効果音を全て一時停止し、ボタンSEを流す
        //PlayerのSE一時停止
        if (Player.instance != null && Player.instance.audioSourceSE != null)
        {
            MusicController.instance.PauseSE(Player.instance.audioSourceSE, Player.instance.GetCurrentSE());
        }
        else
        {
            Debug.LogWarning("Player or AudioSource is null in ViewPausePanel");
        }

        //敵のSE一時停止
        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            if (baseEnemy[i] != null && baseEnemy[i].GetAudioSourceSE() != null 
                && baseEnemy[i].GetAudioSourceFindPlayerSE() != null)
            {
                MusicController.instance.PauseSE(baseEnemy[i].GetAudioSourceSE(), baseEnemy[i].GetCurrentSE());

                //プレイヤーを探すSE再生中フラグがオンの場合のみ一時停止
                if (baseEnemy[i].GetAudioSourceFindPlayerSE().isPlaying) 
                {
                    MusicController.instance.PauseSE(baseEnemy[i].GetAudioSourceFindPlayerSE(), baseEnemy[i].GetAudioSourceFindPlayerSE().clip);
                } 
            }
        }

        //ボタンSEを流す
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
    }

    /// <summary>
    /// ポーズ解除
    /// </summary>
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            //一時停止開所
            Time.timeScale = 1;

            //ポーズフラグをオフ
            isPause = false;

            //パネル非表示
            ChangeViewPausePanel();

            //マウスを非表示にし、固定する
            HideMouseCorsor();

            //ボタンSEを流し、インゲーム内のSEの一時停止を全て解除する
            MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
            MusicController.instance.UnPauseSE(Player.instance.audioSourceSE, Player.instance.GetCurrentSE());

            //敵のSE一時停止解除
            for (int i = 0; i < baseEnemy.Length; i++)
            {
                if (baseEnemy[i] != null && baseEnemy[i].GetAudioSourceSE() != null
                    && baseEnemy[i].GetAudioSourceFindPlayerSE() != null) 
                { 
                    MusicController.instance.UnPauseSE(baseEnemy[i].GetAudioSourceSE(), baseEnemy[i].GetCurrentSE());

                    //プレイヤーを探すSE再生中フラグがオンの場合のみ一時停止解除
                    if (baseEnemy[i].GetAudioSourceFindPlayerSE().isPlaying) 
                    {
                        MusicController.instance.UnPauseSE(baseEnemy[i].GetAudioSourceFindPlayerSE(), baseEnemy[i].GetAudioSourceFindPlayerSE().clip);
                    }            
                }
            }
        }
        
    }

    /// <summary>
    /// 「アイテム確認」ボタン押下
    /// </summary>
    public void OnClickedViewItemButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを非表示にし、アイテム確認パネルを表示する
        isPause = false;
        ChangeViewPausePanel();

        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    /// <summary>
    /// ステージ選択へ戻るパネル・タイトルへ戻るパネル内の「はい」押下
    /// </summary>
    public void OnClickedYesButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ステージ選択へ戻るパネルが表示されている場合
        if (isReturnToSelectStagePanel) 
        {
            //ステージ選択画面へ遷移
            GameController.instance.ReturnToSelectStage();
        }
        //タイトルへ戻るパネルが表示されている場合
        else if (isReturnToTitlePanel)
        {
            //タイトル画面へ遷移
            GameController.instance.ReturnToTitle();
        }
    }

    /// <summary>
    /// ステージ選択へ戻るパネル・タイトルへ戻るパネル内の「いいえ」押下
    /// </summary>
    public void OnClickedNoButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //リタイアパネルを表示
        isRetirePanel = true;
        ChangeViewRetirePanel();

        //ステージ選択へ戻るパネルを非表示にする
        isReturnToSelectStagePanel = false;
        ChangeReturnToSelectStagePanel();

        //タイトルへ戻るパネルを非表示にする
        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }

    /// <summary>
    /// 「アーカイブ」ボタン押下
    /// </summary>
    public void OnClickedViewArchivePanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //アーカイブパネルを表示
        isArchivePanel = true;
        ChangeViewArchivePanel();

        //ドキュメントパネルを非表示
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //ミステリーアイテムパネルを非表示
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// 「彷徨う者」ボタン押下
    /// </summary>
    public void OnClickedViewWandererPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //彷徨う者パネルを表示
        isWandererPanel = true;
        ChangeViewWandererPanel();

        //「彷徨う者」ボタンを非表示にする
        wandererButton.SetActive(false);

        //TODO:アーカイブパネル内の他のパネルを非表示
    }

    /// <summary>
    /// 「ドキュメント」ボタン押下
    /// </summary>
    public void OnClickedViewDocumentButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ドキュメントパネルを表示
        isDocumentPanel = true;
        ChangeViewDocumentPanel();

        //アーカイブパネルを非表示
        isArchivePanel = false;
        ChangeViewArchivePanel();

        //ミステリーアイテムパネルを非表示
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// 「ミステリーアイテム」ボタン押下
    /// </summary>
    public void OnClickedViewMysteryItemButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        // ミステリーアイテムパネルを表示
        isMysteryItemPanel = true;
        ChangeViewMysteryItemPanel();

        //アーカイブパネルを非表示
        isArchivePanel = false;
        ChangeViewArchivePanel();

        // ドキュメントパネルを非表示
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //画像と説明テキストをクリア
        if (mysteryItemImage.Length > 0)
        {
            mysteryItemImage[0].sprite = null;
            mysteryItemImage[0].enabled = false;
        }

        //説明テキストが重なるのを防止するため、全ての説明テキストを一旦クリアする
        for (int i = 0; i < mysteryItemExplanationText.Length; i++)
        {
            if (mysteryItemExplanationText.Length > 0)
            {
                mysteryItemExplanationText[i].text = "";
                mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
            }
        }
    }


    /// <summary>
    /// 「戻る」ボタン押下
    /// ポーズ画面へ戻る
    /// </summary>
    public void OnClickedReturnToPausePanel()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズ画面を表示
        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        //アイテム確認パネルを非表示
        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        //アーカイブパネルを非表示
        isArchivePanel = false;
        ChangeViewArchivePanel();

        //ドキュメントパネルを非表示
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //ミステリーアイテムパネルを非表示
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();

        //リタイアパネルを非表示
        isRetirePanel = false;
        ChangeViewRetirePanel();
    }

    /// <summary>
    /// ドキュメント名称ボタン押下時
    /// </summary>
    public void OnClickedDocumentNameButton() 
    {
        //ドキュメント名称ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(documentNameButtonSEid));

        //テキスト内容を変更する
        SettingLanguageText();

        //ドキュメントを入手している場合
        if (keepDocumentBookID != defaultDocumentBookID) 
        {
            //ドキュメントの説明を表示
            isDocumentExplanationPanel = true;
            ChangeViewDocumentExplanationPanel();
        }

        //入手したドキュメントがチュートリアル用の場合
        if (keepDocumentBookID == documentBook_TutorialID) 
        {
            //フラグ値をオン
            GameController.instance.SetIsTutorialNextMessageFlag(true);
        }
    }

    /// <summary>
    /// ポーズパネルの表示/非表示
    /// </summary>
    public void ChangeViewPausePanel()
    {
        //フラグ値がtrueの場合
        if (isPause)
        {
            //表示
            pausePanel.SetActive(true);
        }
        else
        {
            //非表示
            pausePanel.SetActive(false);
        }
    }


    /// <summary>
    /// アイテム確認パネルの表示/非表示
    /// </summary>
    private void ChangeViewItemsPanel() 
    {
        if (isViewItemsPanel)
        {
            //表示
            viewItemsPanel.SetActive(true);
        }
        else
        {
            //非表示
            viewItemsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// アーカイブパネルの表示/非表示
    /// </summary>
    private void ChangeViewArchivePanel()
    {
        //アーカイブパネルが存在しない場合
        if (archivePanel == null)
        {
            //処理をスキップ
            return;
        }

        if (isArchivePanel)
        {
            //UIのレイヤーを手前側にする
            archivePanel.transform.SetAsLastSibling();

            //テキスト内容を変更する
            SettingLanguageText();

            //アーカイブパネルを表示
            archivePanel.SetActive(true);

            /*----------------------------------------------------------
             *TODO:アーカイブパネル内の子ボタンを全て表示にする処理を追加する 
             ----------------------------------------------------------*/

            //チュートリアルのストーリーを閲覧済みの場合
            if (saveViewStoryStatusArray[stringTutorialClearStatus] == 1) 
            {
                //「彷徨う者」ボタンを表示にする
                wandererButton.SetActive(true);
            }
        }
        else
        {
            //アーカイブパネルを非表示
            archivePanel.SetActive(false);

            /*----------------------------------------------------------
             *TODO:アーカイブパネル内の子パネルを非表示にする処理を追加する 
             ----------------------------------------------------------*/

            //彷徨う者パネルを非表示
            isWandererPanel = false;
            ChangeViewWandererPanel();
        }
    }

    /// <summary>
    /// 彷徨う者名称ボタン押下時
    /// </summary>
    /// <param name="index">インデックス番号</param>
    public void OnClickedWandererNameButton(int index)
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //インデックス番号が名称ボタンの数より小さい場合
        if (index < enemyInformationNames.Count)
        {
            //既に取得済みの彷徨う者関連情報の場合
            if (enemyInformationIds[index] != kDefaultSaveEnemyInformationKey)
            {
                //彷徨う者説明パネルを表示
                isWandererExplanationPanel = true;
                ChangeViewWandererExplanationPanel();

                //説明テキストを更新
                if (wandererExplanationText.Length > 0)
                {
                    //説明テキストが重なるのを防止するため、全ての説明テキストを一旦クリアする
                    for (int i = 0; i < wandererExplanationText.Length; i++)
                    {
                        if (wandererExplanationText.Length > 0)
                        {
                            wandererExplanationText[i].text = "";
                            wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                        }
                    }

                    //言語ステータスに応じて、テキストを変更する
                    switch (LanguageController.instance.GetLanguageStatus())
                    {
                        //日本語
                        case LanguageController.LanguageStatus.kJapanese:

                            //彷徨う者説明欄に日本語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionJapanese;

                            //彷徨う者説明テキストサイズを日本語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizeJapanese;
                            break;

                        //英語
                        case LanguageController.LanguageStatus.kEnglish:

                            //彷徨う者説明欄に英語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionEnglish;

                            //彷徨う者説明テキストサイズを英語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizeEnglish;
                            break;

                        //簡体字中国語
                        case LanguageController.LanguageStatus.kSimplifiedChinese:

                            //彷徨う者説明欄に簡体字中国語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionChinese01;

                            //彷徨う者説明テキストサイズを簡体字中国語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizeChinese01;
                            break;

                        //繁体字中国語
                        case LanguageController.LanguageStatus.kTraditionalChinese:

                            //彷徨う者説明欄に繁体字中国語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionChinese02;

                            //彷徨う者説明テキストサイズを繁体字中国語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizeChinese02;
                            break;

                        //スペイン語
                        case LanguageController.LanguageStatus.kSpanish:

                            //彷徨う者説明欄にスペイン語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSpanish;

                            //彷徨う者説明テキストサイズをスペイン語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizeSpanish;
                            break;

                        //ポルトガル語
                        case LanguageController.LanguageStatus.kPortuguese:

                            //彷徨う者説明欄にポルトガル語用の説明テキストを設定する
                            wandererExplanationText[0].text = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionPortuguese;

                            //彷徨う者説明テキストサイズをポルトガル語用に設定する
                            wandererExplanationText[0].fontSize = itemMessage.itemMessage[enemyInformationIds[index]].itemDescriptionSizePortuguese;
                            break;

                        default:
                            Debug.LogWarning("その他の言語ステータス");
                            break;
                    }

                    wandererExplanationTextRubyComponent[0].Text = wandererExplanationText[0].text;
                }
            }
            else
            {
                Debug.LogWarning($"彷徨う者関連情報アイテム '{enemyInformationNames[index]}' が見つかりません");
            }
        }
    }

    /// <summary>
    /// ミステリーアイテム名称ボタン押下時
    /// </summary>
    /// <param name="index">インデックス番号</param>
    public void OnClickedMysteryItemNameButton(int index)
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //チュートリアル用ミステリーアイテムを全て入手した場合
        if (isGetHammer_Tutorial && isGetRope_Tutorial)
        {
            //フラグ値をオン
            isViewMysteryItem_Tutorial = true;
        }

        if (index < mysteryItemNames.Count)
        {
            //入手したミステリーアイテムがリスト内に存在するかを確認
            string itemName = mysteryItemNames[index];
            SO_Item.ItemData item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

            if (item != null)
            {
                //ミステリーアイテム説明パネルを表示
                isMysteryItemExplanationPanel = true;
                ChangeViewMysteryItemExplanationPanel();

                //ドキュメント説明パネルを非表示にする
                isDocumentExplanationPanel = false;
                ChangeViewDocumentExplanationPanel();

                //説明テキストを更新
                if (mysteryItemExplanationText.Length > 0)
                {
                    //説明テキストが重なるのを防止するため、全ての説明テキストを一旦クリアする
                    for (int i = 0; i < mysteryItemExplanationText.Length; i++)
                    {
                        if (mysteryItemExplanationText.Length > 0)
                        {
                            mysteryItemExplanationText[i].text = "";
                            mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
                        }
                    }
                    
                    //言語ステータスに応じて、テキストを変更する
                    switch (LanguageController.instance.GetLanguageStatus()) 
                    {
                        //日本語
                        case LanguageController.LanguageStatus.kJapanese:

                            //ミステリーアイテム説明欄に日本語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionJapanese;

                            //ミステリーアイテム説明テキストサイズを日本語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeJapanese;
                            break;

                        //英語
                        case LanguageController.LanguageStatus.kEnglish:

                            //ミステリーアイテム説明欄に英語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionEnglish;

                            //ミステリーアイテム説明テキストサイズを英語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeEnglish;
                            break;

                        //簡体字中国語
                        case LanguageController.LanguageStatus.kSimplifiedChinese:

                            //ミステリーアイテム説明欄に簡体字中国語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionChinese01;

                            //ミステリーアイテム説明テキストサイズを簡体字中国語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese01;
                            break;

                        //繁体字中国語
                        case LanguageController.LanguageStatus.kTraditionalChinese:

                            //ミステリーアイテム説明欄に繁体字中国語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionChinese02;

                            //ミステリーアイテム説明テキストサイズを繁体字中国語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese02;
                            break;

                        //スペイン語
                        case LanguageController.LanguageStatus.kSpanish:

                            //ミステリーアイテム説明欄にスペイン語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionSpanish;

                            //ミステリーアイテム説明テキストサイズをスペイン語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeSpanish;
                            break;

                        //ポルトガル語
                        case LanguageController.LanguageStatus.kPortuguese:

                            //ミステリーアイテム説明欄にポルトガル語用の説明テキストを設定する
                            mysteryItemExplanationText[0].text = itemMessage.itemMessage[item.id].itemDescriptionPortuguese;

                            //ミステリーアイテム説明テキストサイズをポルトガル語用に設定する
                            mysteryItemExplanationText[0].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizePortuguese;
                            break;

                        default:
                            Debug.LogWarning("その他の言語ステータス");
                            break;
                    }

                    mysteryItemExplanationTextRubyComponent[0].Text = mysteryItemExplanationText[0].text;
                }

                //画像を更新
                if (mysteryItemImage.Length > 0)
                {
                    mysteryItemImage[0].sprite = item.icon;
                    mysteryItemImage[0].enabled = (item.icon != null);
                }
                else
                {
                    Debug.LogWarning("mysteryItemImage が未設定です");
                }
            }
            else
            {
                Debug.LogError($"アイテム '{itemName}' が見つかりません");
            }
        }
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    public void SettingLanguageText() 
    {
        //ドキュメントの言語設定を行う
        SettingLanguageDocumentText();

        //ミステリーアイテムの言語設定を行う
        SettingLanguageMysteryItemText();

        //彷徨う者関連情報の言語設定を行う
        SettingLanguageWandererInformationText();
    }

    /// <summary>
    /// このコントローラーを破棄する
    /// </summary>
    public void DestroyController() 
    {
        CancelAsyncTasks();
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();
        Destroy(gameObject);
    }
}
