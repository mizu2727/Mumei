using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PauseController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PauseController instance;

    [Header("プレイヤー(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Player player;

    [Header("ゴール(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Goal goal;

    [Header("敵(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private BaseEnemy[] baseEnemy;

    [Header("ポーズパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject pausePanel;

    [Header("アイテム確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject viewItemsPanel;

    [Header("ドキュメントパネル関連")]
    [Header("ドキュメント確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject documentInventoryPanel;

    [Header("ドキュメント説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject documentExplanationPanel;

    [Header("ドキュメント名称テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text documentNameText;

    [Header("ドキュメント説明欄テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text documentExplanationText;


    [Header("ミステリーアイテムパネル関連")]
    [Header("ミステリーアイテム確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mysteryItemInventoryPanel;

    [Header("ミステリーアイテム名称ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Button[] mysteryItemNameButton;

    [Header("ミステリーアイテム名称テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text[] mysteryItemNameText;

    [Header("ミステリーアイテム画像(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Image[] mysteryItemImage;

    [Header("ミステリーアイテム説明欄テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text[] mysteryItemExplanationText;

    [Header("ミステリーアイテム説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mysteryItemExplanationPanel;


    [Header("オプションパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject optionPanel;

    [Header("旋回速度設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mouseSensitivityPanel;

    [Header("音量調整設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject audioAdjustmentPanel;

    [Header("タイトルへ戻るパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject returnToTitlePanel;

    [Header("フラグ関連")]
    [Header("ポーズフラグ(ヒエラルキー上からの編集禁止)")]
    public bool isPause = false;

    [Header("アイテム確認パネル閲覧フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isViewItemsPanel = false;

    [Header("オプションパネル閲覧フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isOptionPanel = false;

    /// <summary>
    /// 旋回速度設定パネル閲覧フラグ
    /// </summary>
    private bool isViewMouseSensitivityPanel = false;

    /// <summary>
    /// 音量調整設定パネル説欄フラグ
    /// </summary>
    private bool isViewAudioAdjustmentPanel = false;

    [Header("タイトルへ戻るパネル閲覧フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isReturnToTitlePanel = false;

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

    /// <summary>
    /// HomeScene
    /// </summary>
    private const string homeScene = "HomeScene";

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stage01 = "Stage01";


    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// 現在再生されているBGMのID
    /// </summary>
    private int nowPlayBGMId = 99999; 

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
    /// 現在再生されているBGMのIDを取得
    /// </summary>
    /// <returns>現在再生されているBGMのID</returns>
    public int GetNowPlayBGMId() 
    {
        return nowPlayBGMId;
    }

    /// <summary>
    /// 対象のBGMに設定する
    /// </summary>
    /// <param name="subjectPlayBGMId_">対象のBGM</param>
    public void SetNowPlayBGMId(int subjectPlayBGMId_) 
    {
        nowPlayBGMId = subjectPlayBGMId_;
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

        //パネルを初期状態で非表示にする
        //フラグ値を初期化
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();

        isOptionPanel = false;
        ChangeOptionPanel();

        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();

        isGetHammer_Tutorial = false;
        isGetRope_Tutorial = false;
        isViewMysteryItem_Tutorial = false;

        //ミステリーアイテムのボタンとテキストを初期化
        InitializeMysteryItemUI();

        //ドキュメントIDを初期化
        keepDocumentBookID = defaultDocumentBookID;
    }


    
    public void Update()
    {
        //PキーorZキーorEscapeキーでポーズ/ポーズ解除
        if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Escape)) 
            && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) TogglePause();
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
        if (!player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanel && !isDocumentExplanationPanel && !isMysteryItemPanel
            && !isMysteryItemExplanationPanel && !goal.isGoalPanel && Time.timeScale != 0)
        {
            ViewPausePanel();
        }
        //ポーズを閉じる条件
        else if (!player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
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

        //現在のシーン名を取得し、その名前によって一時停止するBGMを決める
        switch (SceneManager.GetActiveScene().name) 
        {
            //HomeScene
            case homeScene:
                //HomeSceneBGMを一時停止
                MusicController.instance.PauseBGM(HomeController.instance.GetAudioSourceBGM(),
                    sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());
                break;

            //Stage01
            case stage01:
                //現在流れているBGMがステージBGMなのか敵に追われているBGMなのかを判別する
                if (nowPlayBGMId == EnemyBGMController.instance.GetChasePlayerBGMId())
                {
                    //プレイヤーが敵に追われる際のBGMを一時停止
                    MusicController.instance.PauseBGM(EnemyBGMController.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(EnemyBGMController.instance.GetChasePlayerBGMId()), EnemyBGMController.instance.GetChasePlayerBGMId());
                }
                else 
                {
                    //Stage01BGMを一時停止
                    MusicController.instance.PauseBGM(Stage01Controller.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId()), Stage01Controller.instance.GetStage01BGMId());
                }
                break;

            default:
                Debug.LogWarning("その他のシーン名");
                break;
        };

        //BGM一時停止
        //MusicController.instance.PauseBGM();

        //再生中の効果音を全て一時停止し、ボタンSEを流す
        if (Player.instance != null && Player.instance.audioSourceSE != null)
        {
            MusicController.instance.PauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);
        }
        else
        {
            Debug.LogWarning("Player or AudioSource is null in ViewPausePanel");
        }

        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            if (baseEnemy[i] != null && baseEnemy[i].audioSourceSE != null)
            {
                MusicController.instance.PauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }
        }

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

            //ボタンSEを流し、インゲーム内のBGM・SEの一時停止を全て解除する
            MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
            MusicController.instance.UnPauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);

            for (int i = 0; i < baseEnemy.Length; i++)
            {
                if (baseEnemy[i] != null) MusicController.instance.UnPauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }


            //現在のシーン名を取得し、その名前によって一時停止解除するBGMを決める
            switch (SceneManager.GetActiveScene().name)
            {
                //HomeScene
                case homeScene:
                    //HomeSceneBGMを一時停止解除
                    MusicController.instance.UnPauseBGM(HomeController.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());
                    break;

                //Stage01
                case stage01:
                    //現在一時停止しているBGMがステージBGMなのか敵に追われているBGMなのかを判別する
                    if (nowPlayBGMId == EnemyBGMController.instance.GetChasePlayerBGMId())
                    {
                        //プレイヤーが敵に追われる際のBGMを一時停止解除
                        MusicController.instance.UnPauseBGM(EnemyBGMController.instance.GetAudioSourceBGM(),
                            sO_BGM.GetBGMClip(EnemyBGMController.instance.GetChasePlayerBGMId()), EnemyBGMController.instance.GetChasePlayerBGMId());
                    }
                    else
                    {
                        //Stage01BGMを一時停止解除
                        MusicController.instance.UnPauseBGM(Stage01Controller.instance.GetAudioSourceBGM(),
                            sO_BGM.GetBGMClip(Stage01Controller.instance.GetStage01BGMId()), Stage01Controller.instance.GetStage01BGMId());
                    }
                    break;

                default:
                    Debug.LogWarning("その他のシーン名(UnPauseBGM)");
                    break;
            };

            //MusicController.instance.UnPauseBGM();
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
    /// 「オプション」ボタン押下
    /// </summary>
    public void OnClickedOptionButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを非表示にし、オプションパネルを表示する
        isOptionPanel = true;
        ChangeOptionPanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    /// <summary>
    /// 「旋回速度」ボタン押下
    /// </summary>
    public void OnClickedMouseSensitivityButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //旋回速度設定パネルを表示
        isViewMouseSensitivityPanel = true;
        ChangeMouseSensitivityPanel();
    }

    /// <summary>
    /// 「音量調整」ボタン押下
    /// </summary>
    public void OnClickedAudioAdjustmentButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //旋回速度設定パネルを非表示にする
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();


        //音量調整設定パネルを表示
        isViewAudioAdjustmentPanel = true;
        ChangeAudioAdjustmentPanel();
    }

    /// <summary>
    /// 「戻る」ボタン押下
    /// オプション設定からポーズ画面へ戻る
    /// </summary>
    public void OnClickedFromOptionToPauseButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを表示
        isPause = true;
        ChangeViewPausePanel();

        //旋回速度設定パネルを非表示
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //オプションパネルを非表示
        isOptionPanel = false;
        ChangeOptionPanel();
    }

    /// <summary>
    /// 「タイトルへ戻る」ボタン押下
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを非表示にし、タイトルへ戻るパネルを表示する
        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    /// <summary>
    /// 「はい」押下
    /// </summary>
    public void OnClickedYesButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //タイトル画面へ遷移
        GameController.instance.ReturnToTitle();
    }

    /// <summary>
    /// 「いいえ」押下
    /// </summary>
    public void OnClickedNoButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //ポーズパネルを表示にし、タイトルへ戻るパネルを非表示する
        isPause = true;
        ChangeViewPausePanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
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

        // ドキュメントパネルを非表示
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //画像と説明テキストをクリア
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

        //ドキュメントパネルを非表示
        isDocumentPanel = false;
        ChangeViewDocumentPanel();

        //ミステリーアイテムパネルを非表示
        isMysteryItemPanel = false;
        ChangeViewMysteryItemPanel();
    }

    /// <summary>
    /// ドキュメント名称ボタン押下時
    /// </summary>
    public void OnClickedDocumentNameButton() 
    {
        //ドキュメント名称ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(documentNameButtonSEid));

        //ドキュメントの説明を表示
        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();

        //入手したドキュメントがチュートリアル用の場合
        if (keepDocumentBookID == documentBook_TutorialID) 
        {
            //フラグ値をオン
            GameController.instance.isTutorialNextMessageFlag = true;
        }
    }

    /// <summary>
    /// ポーズパネルの表示/非表示
    /// </summary>
    void ChangeViewPausePanel()
    {
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
    void ChangeViewItemsPanel() 
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
    /// オプションパネルの表示/非表示
    /// </summary>
    void ChangeOptionPanel()
    {
        if (isOptionPanel)
        {
            //表示
            optionPanel.SetActive(true);
        }
        else
        {
            //非表示
            optionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 旋回速度設定パネルの表示/非表示
    /// </summary>
    void ChangeMouseSensitivityPanel()
    {
        if (isViewMouseSensitivityPanel)
        {
            //表示
            mouseSensitivityPanel.SetActive(true);
        }
        else
        {
            //非表示
            mouseSensitivityPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 音量調整設定パネルの表示/非表示
    /// </summary>
    void ChangeAudioAdjustmentPanel()
    {
        if (isViewAudioAdjustmentPanel)
        {
            //表示
            audioAdjustmentPanel.SetActive(true);
        }
        else
        {
            //非表示
            audioAdjustmentPanel.SetActive(false);
        }
    }

    /// <summary>
    /// タイトルへ戻るパネルの表示/非表示
    /// </summary>
    void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            //表示
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            //非表示
            returnToTitlePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ドキュメントパネルの表示/非表示
    /// </summary>
    void ChangeViewDocumentPanel() 
    {
        if (isDocumentPanel)
        {
            //UIのレイヤーを手前側にする
            documentInventoryPanel.transform.SetAsLastSibling();

            //表示
            documentInventoryPanel.SetActive(true);

        }
        else 
        {
            //非表示
            documentInventoryPanel.SetActive(false);

            //ドキュメント説明欄パネルを非表示
            isDocumentExplanationPanel = false;
            ChangeViewDocumentExplanationPanel();
        }
    }

    /// <summary>
    /// ドキュメント説明欄パネルの表示/非表示
    /// </summary>
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            //表示
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
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
        //チュートリアル用ドキュメントの場合
        if (documentId == documentBook_TutorialID) 
        {
            //フラグ値をオン
            GameController.instance.isTutorialNextMessageFlag = true;

            //IDを保存
            keepDocumentBookID = documentId;
        }

        //シーン内で取得したドキュメントオブジェクトの名前を保存
        documentNameText = documentNameText.GetComponent<Text>();
        documentNameText.text = documentName;
    }

    /// <summary>
    /// DocumentExplanationTextの記載内容を変更
    /// </summary>
    /// <param name="documentDescription"></param>
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        //シーン内で取得したドキュメントオブジェクトの説明を保存
        documentExplanationText = documentExplanationText.GetComponent<Text>();
        documentExplanationText.text = documentDescription;
    }

    /// <summary>
    /// ミステリーアイテム確認パネルの表示/非表示
    /// </summary>
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanel)
        {
            //UIのレイヤーを手前側にする
            mysteryItemInventoryPanel.transform.SetAsLastSibling();

            //表示
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            //非表示
            mysteryItemInventoryPanel.SetActive(false);

            //ミステリーアイテム説明欄を非表示
            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();

            //画像と説明テキストをリセット
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

    /// <summary>
    /// ミステリーアイテム説明欄パネルの表示/非表示
    /// </summary>
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            //表示
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            mysteryItemExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ミステリーアイテムのUIを初期化
    /// </summary>
    private void InitializeMysteryItemUI()
    {
        //nullチェック
        if (mysteryItemNameButton == null || mysteryItemNameText == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        //ボタンにクリックイベントを追加
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            //ローカル変数でインデックスをキャプチャ
            int index = i; 

            //クリックイベントを追加
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));

            //入手していないアイテム名の初期表示を"?????????"にする
            mysteryItemNameText[i].text = "?????????";
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
            var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

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
                    mysteryItemExplanationText[0].text = item.description;
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
    /// ミステリーアイテム名を追加し、UIに反映
    /// </summary>
    /// <param name="mysteryItemID">アイテムID</param>
    /// <param name="mysteryItemName">アイテム名</param>
    /// <param name="mysteryItemDescription">アイテム説明</param>
    public void ChangeMysteryItemTexts(int mysteryItemID, string mysteryItemName, string mysteryItemDescription)
    {
        //IDリストに追加
        mysteryItemIds.Add(mysteryItemID);

        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            //チュートリアル用ハンマーの場合
            if (mysteryItemIds[i] == hammer_TutorialID) 
            {
                //フラグ値をオン
                isGetHammer_Tutorial = true;
            }

            //チュートリアル用ロープの場合
            if (mysteryItemIds[i] == rope_TutorialID)
            {
                //フラグ値をオン
                isGetRope_Tutorial = true;
            }
        }

        //アイテムリストから該当するアイテムを検索
        var item = sO_Item.itemList.Find(x => x.itemName == mysteryItemName && x.itemType == ItemType.MysteryItem);
        if (item != null && !mysteryItemNames.Contains(mysteryItemName))
        {
            //アイテム名リストに追加
            mysteryItemNames.Add(mysteryItemName);

            //アイテム説明リストに追加
            mysteryItemExplanations.Add(mysteryItemDescription);

            //UIに反映させる
            UpdateMysteryItemUI();
        }
        else 
        {
            Debug.LogWarning($"MysteryItem '{mysteryItemName}' が見つからないか、すでに追加済みです");
        }
    }

    /// <summary>
    /// ミステリーアイテムのUIを更新
    /// </summary>
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            if (i < mysteryItemNames.Count)
            {
                //入手したミステリーアイテムがリスト内に存在するかを確認
                string itemName = mysteryItemNames[i];
                var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

                if (item != null)
                {
                    //ボタンに表示されるテキストを"?????????"からミステリーアイテム名に変更する
                    mysteryItemNameText[i].text = itemName;

                    //ボタンクリックを有効
                    mysteryItemNameButton[i].interactable = true;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //説明欄テキストにミステリーアイテム説明を反映させる
                        mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //ミステリーアイテム画像を反映させる
                        mysteryItemImage[i].sprite = item.icon;
                        mysteryItemImage[i].enabled = (item.icon != null);
                    }
                }
                else
                {
                    Debug.LogWarning($"アイテム '{itemName}' が見つかりません");

                    //ボタンに表示されるテキストを"?????????"にする
                    mysteryItemNameText[i].text = "?????????";

                    //ボタンクリックを無効
                    mysteryItemNameButton[i].interactable = false;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //説明欄テキストを空にする
                        mysteryItemExplanationText[i].text = "";
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //ミステリーアイテム画像をnullする
                        mysteryItemImage[i].sprite = null;
                        mysteryItemImage[i].enabled = false;
                    }
                }
            }
            else
            {
                //ボタンに表示されるテキストを"?????????"にする
                mysteryItemNameText[i].text = "?????????";

                //ボタンクリックを無効
                mysteryItemNameButton[i].interactable = false;

                if (i < mysteryItemExplanationText.Length)
                {
                    //説明欄テキストを空にする
                    mysteryItemExplanationText[i].text = "";
                }

                if (i < mysteryItemImage.Length)
                {
                    //ミステリーアイテム画像をnullする
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
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

    /// <summary>
    /// オブジェクトが破棄される際に呼ばれる
    /// </summary>
    private void OnDestroy()
    {
        //もしこのインスタンスがシングルトンインスタンス自身であれば、staticな参照をクリアする
        if (instance == this)
        {
            instance = null;
        }
    }
}
