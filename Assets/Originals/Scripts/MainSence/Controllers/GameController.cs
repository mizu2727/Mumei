using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameController instance;

    [Header("Prefab内のGameControllerの子オブジェクトをアタッチすること")]
    [SerializeField] private SaveLoad saveLoad;

    [Header("ゲームモードのステータス(ヒエラルキー上からの編集禁止)")]
    public GameModeStatus gameModeStatus;

    [Header("メッセージテキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public Text messageText;

    [Header("チュートリアル用ドキュメント")]
    [SerializeField] public GameObject tutorialDocument;

    [Header("チュートリアル用ミステリーアイテム関連")]
    [SerializeField] public GameObject tutorialMysteryItem01;
    [SerializeField] public GameObject tutorialMysteryItem02;

    [Header("チュートリアル用アイテム親オブジェクト")]
    [SerializeField] public GameObject tutorialItems;

    [Header("チュートリアル用フラグ(ヒエラルキー上からの編集禁止)")]
    public bool isTutorialNextMessageFlag = false;

    [Header("チュートリアル用ゴールフラグ(ヒエラルキー上からの編集禁止)")]
    public bool isTutorialGoalFlag = false;

    [Header("PlayerスタミナSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider staminaSlider;

    [Header("PlayerCameraマウス/ゲームパッドの右スティックの旋回速度のSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider mouseSensitivitySlider;


    [Header("Playerの使用アイテムインベントリパネル関連")]
    [Header("使用アイテムパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject useItemPanel;

    [Header("使用アイテム所持カウントテキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text useItemCountText;

    [Header("使用アイテム画像(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Image useItemImage;

    [Header("使用アイテムテキスト確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject useItemTextPanel;

    [Header("使用アイテム名テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text useItemNameText;

    [Header("使用アイテム説明テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text useItemExplanationText;


    [Header("セーブ・ロードしたい変数関連")]
    [Header("セーブするプレイヤー名")]
    public static string playerName;

    [Header("セーブするプレイ回数")]
    public static int playCount = 0;



    /// <summary>
    /// ゲームモードステータス
    /// </summary>
    public enum  GameModeStatus
    {
        /// <summary>
        /// ストーリーモード
        /// </summary>
        Story,

        /// <summary>
        /// 通常プレイモード
        /// </summary>
        PlayInGame,

        /// <summary>
        /// プレイヤーを操作しないモード
        /// </summary>
        StopInGame,

        /// <summary>
        /// ゲームオーバーモード
        /// </summary>
        GameOver,
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //パラメーターリセット
        ResetParams();
    }

    /// <summary>
    /// パラメーターリセット
    /// </summary>
    public void ResetParams() 
    {
        isTutorialNextMessageFlag = false;
        isTutorialGoalFlag = false;
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);


        Time.timeScale = 1;
    }

    private void Start()
    {
        //パラメーターリセット
        ResetParams();
    }

    /// <summary>
    /// ゲームモードのステータスを設定
    /// </summary>
    /// <param name="status">ゲームモードステータス</param>
    public void SetGameModeStatus(GameModeStatus status) 
    {
        gameModeStatus = status;

        //ストーリーモードの場合
        if (gameModeStatus == GameModeStatus.Story) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// データを保存するメソッドを呼び出す(製品版で使用する)
    /// </summary>
    public void CallSaveUserDataMethod() 
    {
        saveLoad.SaveUserData();
    }

    /// <summary>
    /// データをロードするメソッドを呼び出す(製品版で使用する)
    /// </summary>
    public void CallLoadUserDataMethod() 
    {
        saveLoad.LoadUserData();
    }

    /// <summary>
    /// データを初期化するメソッドを呼び出す(製品版で使用する)
    /// </summary>
    public void CallRestDataMethod() 
    {
        saveLoad.ResetUserData();
    }

    /// <summary>
    /// ゲームオーバー画面へ遷移する
    /// </summary>
    public void ViewGameOver() 
    {
        if (Player.instance.IsDead) 
        {
            SceneManager.LoadScene("GameOverScene");
        }
    }

    /// <summary>
    /// タイトル画面へ戻る
    /// </summary>
    public void ReturnToTitle() 
    {
        //MessageControllerの非同期タスクをキャンセル
        if (MessageController.instance != null)
        {
            MessageController.instance.CancelAsyncTasks();
            MessageController.instance.DestroyController();
        }

        //プレイヤー削除・タイトルシーンへ遷移
        if (Player.instance != null)
        {
            Player.instance.DestroyPlayer();
        }
        SceneManager.LoadScene("TitleScene");

        //PauseControllerを削除
        if (PauseController.instance != null)
        {
            PauseController.instance.DestroyController();
        }

    }
}
