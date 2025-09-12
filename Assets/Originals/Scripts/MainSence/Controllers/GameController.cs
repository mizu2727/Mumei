using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("Prefab内のGameControllerの子オブジェクトをアタッチすること")]
    [SerializeField] private SaveLoad saveLoad;

    [Header("ゲームモードのステータス")]
    public GameModeStatus gameModeStatus;

    [Header("チュートリアル用ドキュメント")]
    [SerializeField] public GameObject tutorialDocument;

    [Header("チュートリアル用ミステリーアイテム関連")]
    [SerializeField] public GameObject tutorialMysteryItem01;
    [SerializeField] public GameObject tutorialMysteryItem02;

    [Header("チュートリアル用アイテム親オブジェクト")]
    [SerializeField] public GameObject tutorialItems;

    [Header("PlayerスタミナSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider staminaSlider;

    [Header("PlayerCameraマウス/ゲームパッドの右スティックの旋回速度のSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("Playerの使用アイテムインベントリパネル関連(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject useItemPanel;//使用アイテム確認パネル
    [SerializeField] public Text useItemCountText;//使用アイテム所持カウントテキスト
    [SerializeField] public Image useItemImage;//使用アイテム画像
    [SerializeField] public GameObject useItemTextPanel;//使用アイテムテキスト確認パネル
    [SerializeField] public Text useItemNameText;//使用アイテム名テキスト
    [SerializeField] public Text useItemExplanationText;//使用アイテム説明テキスト

    //[Header("チュートリアル用ゴール")]
    //[SerializeField] public GameObject tutorialGoal;

    [Header("セーブするプレイヤー名")]
    public static string playerName;

    [Header("セーブするプレイ回数")]
    public static int playCount = 0;

    


    public enum  GameModeStatus
    {
        Story,
        PlayInGame,
        StopInGame,
        GoalGameMode,
        GameOver,
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        Time.timeScale = 1;
    }

    private void Update()
    {

    }

    //ゲームモードのステータスを設定
    public void SetGameModeStatus(GameModeStatus status) 
    {
        gameModeStatus = status;

        if (gameModeStatus == GameModeStatus.Story) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// データを保存するメソッドを呼び出す(仮)
    /// </summary>
    public void CallSaveUserDataMethod() 
    {
        saveLoad.SaveUserData();
    }

    /// <summary>
    /// データをロードするメソッドを呼び出す(仮)
    /// </summary>
    public void CallLoadUserDataMethod() 
    {
        saveLoad.LoadUserData();
    }

    /// <summary>
    /// データを初期化するメソッドを呼び出す(仮)
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

}
