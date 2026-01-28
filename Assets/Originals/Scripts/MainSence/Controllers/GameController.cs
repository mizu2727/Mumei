using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    /// <summary>
    /// チュートリアル用フラグ
    /// </summary>
    private bool isTutorialNextMessageFlag = false;

    /// <summary>
    /// チュートリアル用ゴールフラグ
    /// </summary>
    private bool isTutorialGoalFlag = false;

    [Header("PlayerスタミナSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider staminaSlider;

    [Header("PlayerCameraマウス/ゲームパッドの右スティックの旋回速度のSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    /// <summary>
    /// マウス/ゲームパッドの右スティックの感度最大値
    /// </summary>
    private float maxLookSensitivity = 20f;


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


    [Header("ブラックアウト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public GameObject blackOutPanel;


    [Header("画面解像度テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text resolutionText;


    [Header("セーブ・ロードしたい変数関連")]
    [Header("セーブするプレイヤー名(ヒエラルキー上からの編集禁止)")]
    public static string playerName;

    [Header("セーブするプレイ回数(ヒエラルキー上からの編集禁止)")]
    public static int playCount = 0;

    [Header("マウス/ゲームパッドの右スティックの感度")]
    public static float lookSensitivity = 3.0f;

    [Header("セーブするBGM音量")]
    public static float bGMVolume = 1;

    [Header("セーブするSE音量")]
    public static float sEVolume = 1;

    [Header("セーブする明るさの値")]
    public static float brightnessValue = 0.075f;

    [Header("セーブするフルスクリーンフラグ値")]
    public static bool isFullScreen = true;

    [Header("セーブする画面解像度の配列インデックス番号")]
    public static int resolutionArrayIndexNumber = 3;

    [Header("セーブするOperationPanel手動閲覧フラグ")]
    public static bool isSaveSelfViewOperationPanel = true;

    [Header("セーブするUseItemTextPanel手動閲覧フラグ")]
    public static bool isSaveSelfViewUseItemTextPanel = true;

    [Header("セーブするCompassTextPanel手動閲覧フラグ")]
    public static bool isSaveSelfViewCompassTextPanel = true;

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

    /// <summary>
    /// 表示するシーンステータスの列挙型
    /// </summary>
    public enum ViewScene 
    {
        /// <summary>
        /// なし
        /// </summary>
        kNone,

        /// <summary>
        /// TitleScene
        /// </summary>
        kTitleScene,

        /// <summary>
        /// OpeningScene
        /// </summary>
        kOpeningScene,

        /// <summary>
        /// HomeScene
        /// </summary>
        kHomeScene,

        /// <summary>
        /// GameOverScene
        /// </summary>
        kGameOverScene,

        /// <summary>
        /// GameClearScene
        /// </summary>
        kGameClearScene,

        /// <summary>
        /// Stage01
        /// </summary>
        kStage01,
    }

    /// <summary>
    /// 表示するシーンステータス
    /// </summary>
    private ViewScene viewScene;

    /// <summary>
    /// TitleSceneのシーン名
    /// </summary>
    const string stringTitleScene = "TitleScene";

    /// <summary>
    /// GameOverSceneのシーン名
    /// </summary>
    const string stringGameOverScene = "GameOverScene";

    /// <summary>
    /// 表示するシーンステータスを取得
    /// </summary>
    /// <returns>表示するシーンステータス</returns>
    public ViewScene GetViewScene() 
    {
        return viewScene;
    }

    /// <summary>
    /// チュートリアルフラグを取得
    /// </summary>
    /// <returns>チュートリアルフラグ</returns>
    public bool GetIsTutorialNextMessageFlag() 
    {
        return isTutorialNextMessageFlag;
    }

    /// <summary>
    /// チュートリアルフラグを設定
    /// </summary>
    /// <param name="subjectFlag">チュートリアルフラグ</param>
    public void SetIsTutorialNextMessageFlag(bool subjectFlag) 
    {
        isTutorialNextMessageFlag = subjectFlag;
    }

    /// <summary>
    /// チュートリアル用ゴールフラグを取得
    /// </summary>
    /// <returns>チュートリアル用ゴールフラグ</returns>
    public bool GetIsTutorialGoalFlag() 
    {
        return isTutorialGoalFlag;
    }

    /// <summary>
    /// チュートリアル用ゴールフラグを設定
    /// </summary>
    /// <param name="subjectFlag">チュートリアル用ゴールフラグ</param>
    public void SetIsTutorialGoalFlag(bool subjectFlag) 
    {
        isTutorialGoalFlag = subjectFlag;
    }

    /// <summary>
    /// 表示したいシーンステータスを設定
    /// </summary>
    /// <param name="targetScene">表示したいシーンステータス</param>
    public void SetViewScene(ViewScene targetScene) 
    {
        viewScene = targetScene;
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


        //シーン遷移時用データをロード
        CallLoadSceneTransitionUserDataMethod();

        //マウス感度が存在する場合
        if (mouseSensitivitySlider != null)
        {
            //マウス感度を保存した値に設定
            mouseSensitivitySlider.value = lookSensitivity;
        }

        //BGMスライダーが存在する場合
        if (MusicController.instance.bGMSlider != null)
        {
            //BGMを保存した値に設定
            MusicController.instance.bGMSlider.value = bGMVolume;

            //現在のシーン名がTitleSceneの場合(ステージ系シーンへ遷移した際に発生するエラーを防ぐためにこのif分を追加)
            if (SceneManager.GetActiveScene().name == stringTitleScene) 
            {
                MusicController.instance.OnBGMVolumeChanged(bGMVolume);
            }
        }

        //SEスライダーが存在する場合
        if (MusicController.instance.sESlider != null)
        {
            //SEを保存した値に設定
            MusicController.instance.sESlider.value = sEVolume;

            //現在のシーン名がTitleSceneの場合(ステージ系シーンへ遷移した際に発生するエラーを防ぐためにこのif分を追加)
            if (SceneManager.GetActiveScene().name == stringTitleScene)
            {
                MusicController.instance.OnSEVolumeChanged(sEVolume);
            } 
        }

        //明るさ調整スライダーが存在する場合
        if (BrightnessAdjustmentController.instance.brightnessAdjustmentSlider != null) 
        {
            //明るさを保存した値に設定
            BrightnessAdjustmentController.instance.brightnessAdjustmentSlider.value = brightnessValue;
            Debug.Log("ロードした明るさの値:" + brightnessValue);
        }

        //画面解像度テキストが存在する場合
        if (resolutionText != null) 
        {
            //解像度の配列インデックス番号を保存した値に設定
            ScreenAspect.instance.SetResolutionArrayIndex(resolutionArrayIndexNumber);

            //フルスクリーンフラグ値を保存した値に設定
            ScreenAspect.instance.SetFullScreenFlag(isFullScreen);
        }

        //OperationExplanationControllerが存在する場合
        if (OperationExplanationController.instance != null)
        {
            //OperationPanelが存在する場合
            if (OperationExplanationController.instance.GetOperationPanel() != null)
            {
                //OperationPanel手動閲覧フラグを保存した値に設定
                OperationExplanationController.instance.SetIsSelfViewOperationPanel(isSaveSelfViewOperationPanel);
                Debug.Log("ロードしたOperationPanel手動閲覧フラグ:" + isSaveSelfViewOperationPanel);
            }

            //UseItemTextPanelが存在する場合
            if (OperationExplanationController.instance.GetUseItemTextPanel() != null)
            {
                //UseItemTextPanel手動閲覧フラグを保存した値に設定
                OperationExplanationController.instance.SetIsSelfViewUseItemTextPanel(isSaveSelfViewUseItemTextPanel);
                Debug.Log("ロードしたUseItemTextPanel手動閲覧フラグ:" + isSaveSelfViewUseItemTextPanel);
            }


            //CompassTextPanelが存在する場合
            if (OperationExplanationController.instance.GetCompassTextPanel() != null)
            {
                //CompassTextPanel手動閲覧フラグを保存した値に設定
                OperationExplanationController.instance.SetIsSelfViewCompassTextPanel(isSaveSelfViewCompassTextPanel);
                Debug.Log("ロードしたCompassTextPanel手動閲覧フラグ:" + isSaveSelfViewCompassTextPanel);
            }
        }
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



        //マウス旋回速度のSliderの最大値を設定
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;

        //BGM音量のSliderの最大値を設定
        if (MusicController.instance.bGMSlider) MusicController.instance.bGMSlider.maxValue = MusicController.instance.GetMaxBGMSliderVolume();

        //SE音量のSliderの最大値を設定
        if (MusicController.instance.sESlider) MusicController.instance.sESlider.maxValue = MusicController.instance.GetMaxSESliderVolume();

        //Fog欄内・明るさスライダーの最大値と最小値を設定
        if (BrightnessAdjustmentController.instance.brightnessAdjustmentSlider) 
        {
            //Environmentタブ内のOtherSettings内のFog欄内を設定
            BrightnessAdjustmentController.instance.ApplyCustomFogSettings();

            //明るさのスライダーに関する設定
            BrightnessAdjustmentController.instance.ApplyBrightnessAdjustmentSlider();
        }

        //リセット(デバッグ用。新しいシーン遷移時用データ用パラメータが追加された場合に一度実行すること)
        //CallRestDataMethod();
    }

    private void Update()
    {
        //マウス感度をスライダーから取得
        if (mouseSensitivitySlider)
        {
            lookSensitivity = mouseSensitivitySlider.value;

            //最大値を超えないように制限
            if (lookSensitivity > maxLookSensitivity) lookSensitivity = maxLookSensitivity;
        }

        //セーブ用BGM音量をスライダーから取得
        if (MusicController.instance.bGMSlider)
        {
            bGMVolume = MusicController.instance.bGMSlider.value;

            //BGM音量が最大値を超えないように制限
            if (bGMVolume > MusicController.instance.GetMaxBGMSliderVolume()) bGMVolume = MusicController.instance.GetMaxBGMSliderVolume();

            //BGM音量が最小値未満にならないように制限
            if (bGMVolume < MusicController.instance.GetMinBGMSliderVolume()) bGMVolume = MusicController.instance.GetMinBGMSliderVolume();
        }

        //セーブ用SE音量をスライダーから取得
        if (MusicController.instance.sESlider)
        {
            sEVolume = MusicController.instance.sESlider.value;

            //SE音量が最大値を超えないように制限
            if (sEVolume > MusicController.instance.GetMaxSESliderVolume()) sEVolume = MusicController.instance.GetMaxSESliderVolume();

            //SE音量が最小値未満にならないように制限
            if (sEVolume < MusicController.instance.GetMinSESliderVolume()) sEVolume = MusicController.instance.GetMinSESliderVolume();
        }
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
    /// シーン遷移時用データを保存するメソッドを呼び出す
    /// </summary>
    public void CallSaveSceneTransitionUserDataMethod()
    {
        saveLoad.SaveSceneTransitionUserData();
    }

    /// <summary>
    /// シーン遷移時用データをロードするメソッドを呼び出す
    /// </summary>
    public void CallLoadSceneTransitionUserDataMethod()
    {
        saveLoad.LoadSceneTransitionUserData();
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
        // シーン遷移前に非同期タスクをキャンセル
        if (MessageController.instance != null)
        {
            MessageController.instance.CancelAsyncTasks();
        }

        if (Player.instance.IsDead) 
        {
            //シーン遷移時用データを保存
            CallSaveSceneTransitionUserDataMethod();

            //ゲームオーバーシーンをロードする
            SceneManager.LoadScene(stringGameOverScene);
        }
    }

    /// <summary>
    /// タイトル画面へ戻る
    /// </summary>
    public void ReturnToTitle() 
    {
        //シーン遷移時用データを保存
        CallSaveSceneTransitionUserDataMethod();

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
        SceneManager.LoadScene(stringTitleScene);

        //PauseControllerを削除
        if (PauseController.instance != null)
        {
            PauseController.instance.DestroyController();
        }

    }


    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        //メッセージテキストが存在する場合
        if (messageText != null)
        {
            //メッセージテキストをnullに設定
            messageText = null;
        }

        //チュートリアル用ドキュメントが存在する場合
        if (tutorialDocument != null)
        {
            //チュートリアル用ドキュメントをnullに設定
            tutorialDocument = null;
        }

        //チュートリアル用ミステリーアイテム01が存在する場合
        if (tutorialMysteryItem01 != null)
        {
            //チュートリアル用ミステリーアイテム01をnullに設定
            tutorialMysteryItem01 = null;
        }

        //チュートリアル用ミステリーアイテム02が存在する場合
        if (tutorialMysteryItem02 != null)
        {
            //チュートリアル用ミステリーアイテム02をnullに設定
            tutorialMysteryItem02 = null;
        }

        //チュートリアル用アイテム親オブジェクトが存在する場合
        if (tutorialItems != null)
        {
            //チュートリアル用アイテム親オブジェクトをnullに設定
            tutorialItems = null;
        }

        //スタミナSliderが存在する場合
        if (staminaSlider != null)
        {
            //スタミナSliderをnullに設定
            staminaSlider = null;
        }

        //マウス感度Sliderが存在する場合
        if (mouseSensitivitySlider != null)
        {
            //マウス感度Sliderをnullに設定
            mouseSensitivitySlider = null;
        }

        //使用アイテムパネルが存在する場合
        if (useItemPanel != null)
        {
            //使用アイテムパネルをnullに設定
            useItemPanel = null;
        }

        //使用アイテム所持カウントテキストが存在する場合
        if (useItemCountText != null)
        {
            //使用アイテム所持カウントテキストをnullに設定
            useItemCountText = null;
        }

        //使用アイテム画像が存在する場合
        if (useItemImage != null)
        {
            //使用アイテム画像をnullに設定
            useItemImage = null;
        }

        //使用アイテムテキスト確認パネルが存在する場合
        if (useItemTextPanel != null)
        {
            //使用アイテムテキスト確認パネルをnullに設定
            useItemTextPanel = null;
        }

        //使用アイテム名テキストが存在する場合
        if (useItemNameText != null)
        {
            //使用アイテム名テキストをnullに設定
            useItemNameText = null;
        }

        //使用アイテム説明テキストが存在する場合
        if (useItemExplanationText != null)
        {
            //使用アイテム説明テキストをnullに設定
            useItemExplanationText = null;
        }

        //ブラックアウトパネルが存在する場合
        if (blackOutPanel != null)
        {
            //ブラックアウトパネルをnullに設定
            blackOutPanel = null;
        }

        //画面解像度テキストが存在する場合
        if (resolutionText != null)
        {
            //画面解像度テキストをnullに設定
            resolutionText = null;
        }
    }
}
