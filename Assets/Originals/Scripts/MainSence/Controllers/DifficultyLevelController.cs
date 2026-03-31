using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using UnityEngine.EventSystems;

public class DifficultyLevelController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static DifficultyLevelController instance;

    /// <summary>
    /// HomeScene
    /// </summary>
    private const string stringHomeScene = "HomeScene";

    [Header("難易度説明(Prefabをアタッチ)")]
    [SerializeField] private DifficultyLevelExplanation difficultyLevelExplanation;

    [Header("backgroundStageImage(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Image backgroundStageImage;

    [Header("各ステージ背景画像(Prefabからアタッチすること)")]
    [SerializeField] private Sprite[] backgroundStageImageArray;

    [Header("ステージ及び難易度選択パネル(ヒエラルキー上からアタッチすること。0番目のみ空けること)")]
    [SerializeField] private GameObject stageAndDifficultyLevelChoosePanel;

    [Header("難易度選択パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject difficultyLevelChoosePanel;

    [Header("難易度悪夢ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Button nightmareLevelButton;

    [Header("難易度説明欄パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject difficultyLevelExplanationPanel;

    [Header("難易度説明テキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Text difficultyLevelExplanationText;

    [Header("ステージクリア情報パネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject stageClearInformationPanel;

    //TODO:ステージクリアフラグとクリアタイム時間を保存する変数を追加する

    /// <summary>
    /// ステージ及び難易度選択パネル閲覧フラグ
    /// </summary>
    private bool isViewStageAndDifficultyLevelChoosePanel = false;

    /// <summary>
    /// 難易度選択パネル閲覧フラグ
    /// </summary>
    private bool isViewDifficultyLevelChoosePanel = false;

    /// <summary>
    /// 難易度説明欄パネル閲覧フラグ
    /// </summary>
    private bool isDifficultyLevelExplanationPanel = false;

    /// <summary>
    /// ステージクリア情報パネル閲覧フラグ
    /// </summary>
    private bool isStageClearInformationPanel = false;

    [Header("難易度ステータス(ヒエラルキー上からの編集禁止)")]
    public DifficultyLevel difficultyLevelStatus;

    /// <summary>
    /// 難易度ステータス
    /// </summary>
    public enum DifficultyLevel
    {
        /// <summary>
        /// なし
        /// </summary>
        kNone,

        /// <summary>
        /// 
        /// </summary>
        kEasy,

        /// <summary>
        /// 
        /// </summary>
        kNormal,

        /// <summary>
        /// 悪夢
        /// </summary>
        kNightmare,
    }

    /// <summary>
    /// 難易度ステータスを取得
    /// </summary>
    /// <returns>難易度ステータス</returns>
    public DifficultyLevel GetDifficultyLevelStatus()
    {
        return difficultyLevelStatus;
    }

    /// <summary>
    /// 難易度ステータスを設定
    /// </summary>
    /// <param name="status">難易度ステータス</param>
    public void SetDifficultyLevelStatus(DifficultyLevel status)
    {
        //難易度ステータスを設定
        difficultyLevelStatus = status;

        //保存用変数に難易度ステータスを設定
        saveDifficultyLevelStatus = difficultyLevelStatus;
    }

    /// <summary>
    /// ステージ及び難易度選択パネル閲覧フラグを設定
    /// </summary>
    /// <returns>ステージ及び難易度選択パネル閲覧フラグ</returns>
    public void SetIsViewStageAndDifficultyLevelChoosePanel(bool flag) 
    {
        isViewStageAndDifficultyLevelChoosePanel = flag;
    }

    private void OnDestroy()
    {
        //stageClearInformationPanelが存在する場合
        if (stageClearInformationPanel != null) 
        {
            //stageClearInformationPanelをnullにする(メモリリークを防ぐため)
            stageClearInformationPanel = null;
        }

        //difficultyLevelExplanationTextが存在する場合
        if (difficultyLevelExplanationText != null)
        {
            //difficultyLevelExplanationTextをnullにする(メモリリークを防ぐため)
            difficultyLevelExplanationText = null;
        }

        //NightmareLevelButtonが存在する場合
        if (nightmareLevelButton != null) 
        {
            //NightmareLevelButtonをnullにする(メモリリークを防ぐため)
            nightmareLevelButton = null;
        }

        //difficultyLevelChoosePanelが存在する場合
        if (difficultyLevelChoosePanel != null) 
        {
            //difficultyLevelChoosePanelをnullにする(メモリリークを防ぐため)
            difficultyLevelChoosePanel = null;
        }

        //backgroundStageImageArrayが存在する場合
        if (backgroundStageImageArray != null) 
        {
            //backgroundStageImageArrayをnullにする(メモリリークを防ぐため)
            backgroundStageImageArray = null;
        }

        //backgroundStageImageが存在する場合
        if (backgroundStageImage != null) 
        {
            //backgroundStageImageをnullにする(メモリリークを防ぐため)
            backgroundStageImage = null;
        }

        //stageAndDifficultyLevelChoosePanelが存在する場合
        if (stageAndDifficultyLevelChoosePanel != null) 
        {
            //stageAndDifficultyLevelChoosePanelをnullにする(メモリリークを防ぐため)
            stageAndDifficultyLevelChoosePanel = null;
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
        //インスタンスが存在しない場合
        if (instance == null)
        {
            //インスタンスを設定
            instance = this;
        }
        else
        {
            //このオブジェクトを破壊する
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        //現在のシーン名によって処理を変更する
        switch (SceneManager.GetActiveScene().name)
        {
            //HomeSceneの場合
            case stringHomeScene:

                //ステージ及び難易度選択パネルが存在しない場合
                if (stageAndDifficultyLevelChoosePanel == null) 
                {
                    Debug.LogError("StageAndDifficultyLevelChoosePanelがアタッチされていません。");
                }

                //ステージ背景画像が存在しない場合
                if (backgroundStageImage == null) 
                {
                    Debug.LogError("BackgroundStageImageがアタッチされていません。");
                }

                //ステージ背景画像の配列が存在しない場合||ステージ背景画像の配列の要素数が0の場合
                if (backgroundStageImageArray == null || backgroundStageImageArray.Length == 0) 
                {
                    Debug.LogError("BackgroundStageImageArrayがアタッチされていないか、要素数が0です。");
                }

                //難易度選択パネルが存在しない場合
                if (difficultyLevelChoosePanel == null)
                {
                    Debug.LogError("DifficultyLevelChoosePanelがアタッチされていません。");
                }

                //悪夢レベルボタンが存在しない場合
                if (nightmareLevelButton == null)
                {
                    Debug.LogError("NightmareLevelButtonがアタッチされていません。");
                }

                //難易度説明欄パネルが存在しない場合
                if (difficultyLevelExplanationPanel == null) 
                {
                    Debug.LogError("DifficultyLevelExplanationPanelがアタッチされていません。");
                }

                //難易度説明テキストが存在しない場合
                if (difficultyLevelExplanationText == null) 
                {
                    Debug.LogError("DifficultyLevelExplanationTextがアタッチされていません。");
                }

                //ステージクリア情報パネルが存在しない場合
                if (stageClearInformationPanel == null) 
                {
                    Debug.LogError("stageClearInformationPanelがアタッチされていません。");
                }

                //初期化処理
                //説明文をリセット
                ResetExplanation();

                //ステージクリア情報パネルを非表示にする
                isStageClearInformationPanel = false;
                ChangeViewStageClearInformationPanel();

                //難易度選択パネルを非表示にする
                isViewDifficultyLevelChoosePanel = false;
                ChangeViewDifficultyLevelChoosePanel();

                //ステージ及び難易度選択パネルを非表示にする
                isViewStageAndDifficultyLevelChoosePanel = false;
                ChangeViewStageAndDifficultyLevelChoosePanel();
                break;

            //その他のシーンの場合
            default:
                Debug.Log("その他のシーン名");

                break;
        }
    }

    /// <summary>
    /// ステージ名ボタン押下時に呼ばれる関数
    /// </summary>
    /// <param name="number">インデックス番号</param>
    public void OnClickedStageNameButton(int number) 
    {
        //ステージシーンインデックス番号を設定
        ChangeSceneZone.instance.SetStageSceneNameArrayIndex(number);

        //ステージ及び難易度選択パネルを表示する
        isViewDifficultyLevelChoosePanel = true;
        ChangeViewDifficultyLevelChoosePanel();
    }

    /// <summary>
    /// ステージ名ボタンにカーソルが合わさった際の処理
    /// </summary>
    /// <param name="number">インデックス番号</param>
    public void OnPointerEnterStageNameButton(int number) 
    {
        //ステージ背景画像を設定する
        backgroundStageImage.sprite = backgroundStageImageArray[number];

        //ステージ背景画像の色を設定する
        backgroundStageImage.color = new Color(157, 157, 157, 220);

        //ステージクリア情報パネルを表示する
        isStageClearInformationPanel = true;
        ChangeViewStageClearInformationPanel();
    }

    /// <summary>
    /// インゲームへ戻るボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedReturnToInGameButton() 
    {
        //マウスカーソルを非表示にし、固定する
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //TODO:ステージクリアフラグとクリアタイム時間の変数をリセットする処理

        //ステージのクリア情報パネルを非表示にする
        isStageClearInformationPanel = false;
        ChangeViewStageClearInformationPanel();

        //ステージ及び難易度選択パネルを非表示にする
        isViewStageAndDifficultyLevelChoosePanel = false;
        ChangeViewStageAndDifficultyLevelChoosePanel();

        //ゲームモードステータスをInGameに設定
        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

        //ブラックアウトパネルを非表示にする
        MessageController.instance.SetIsBlackOutPanel(false);
        MessageController.instance.ViewBlackOutPanel();
    }

    /// <summary>
    /// EasyLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedEasyLevelButton()
    {
        //難易度ステータスをEasyに設定する
        SetDifficultyLevelStatus(DifficultyLevel.kEasy);

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }

    /// <summary>
    /// NormalLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNormalLevelButton() 
    {
        //難易度ステータスをNormalに設定する
        SetDifficultyLevelStatus(DifficultyLevel.kNormal);

        Debug.Log("難易度ステータス:" + GetDifficultyLevelStatus());

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }

    /// <summary>
    /// NightmareLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNightmareLevelButton() 
    {
        //難易度ステータスをNightmareに設定する
        SetDifficultyLevelStatus(DifficultyLevel.kNightmare);

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }

    /// <summary>
    /// 難易度ボタンにマウスポインタが乗ったときに呼ばれる関数
    /// </summary>
    /// <param name="number">難易度説明番号</param>
    public void OnPointerEnterDifficultyLevelButton(int number)
    {
        //難易度説明を表示
        ShowDifficultyLevelExplanation(number);
    }

    /// <summary>
    /// 難易度ボタンからマウスポインタが離れたときに呼ばれる関数
    /// </summary>
    public void OnPointerExitDifficultyLevelButton() 
    {
        //説明文をリセット
        ResetExplanation();
    }

    /// <summary>
    /// ステージ選択へ戻るボタンUI押下時の処理
    /// </summary>
    public void OnClickedReturnToStageSelectButton() 
    {
        //難易度説明パネルを非表示にする
        isDifficultyLevelExplanationPanel = false;
        ChangeViewDifficultyLevelExplanationPanel();

        //ステージ及び難易度選択パネルを非表示にする
        isViewDifficultyLevelChoosePanel = false;
        ChangeViewDifficultyLevelChoosePanel();
    }

    /// <summary>
    /// ステージ及び難易度説選択パネルの表示/非表示
    /// </summary>
    public void ChangeViewStageAndDifficultyLevelChoosePanel()
    {
        if (isViewStageAndDifficultyLevelChoosePanel)
        {
            //ステージ背景画像の色と画像内容をリセットする
            ResetBackgroundStageImage();

            //表示
            stageAndDifficultyLevelChoosePanel.SetActive(true);
        }
        else
        {
            //非表示
            stageAndDifficultyLevelChoosePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ステージ背景画像の色をリセットする
    /// </summary>
    private void ResetBackgroundStageImage() 
    {
        //ステージ背景画像の色をリセットする
        backgroundStageImage.color = new Color(0, 0, 0, 0);
    }

    /// <summary>
    /// 難易度説選択パネルの表示/非表示
    /// </summary>
    private void ChangeViewDifficultyLevelChoosePanel()
    {
        if (isViewDifficultyLevelChoosePanel)
        {
            //表示
            difficultyLevelChoosePanel.SetActive(true);
        }
        else
        {
            //非表示
            difficultyLevelChoosePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 難易度説明欄パネルの表示/非表示
    /// </summary>
    private void ChangeViewDifficultyLevelExplanationPanel()
    {
        if (isDifficultyLevelExplanationPanel)
        {
            //表示
            difficultyLevelExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            difficultyLevelExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ステージクリア情報パネルの表示/非表示
    /// </summary>
    private void ChangeViewStageClearInformationPanel() 
    {
        if (isStageClearInformationPanel) 
        {
            //表示
            stageClearInformationPanel.SetActive(true);
        }
        else 
        {
            //非表示
            stageClearInformationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 難易度説明文をリセット
    /// </summary>
    public void ResetExplanation()
    {
        //説明文をリセット
        difficultyLevelExplanationText.text = "";

        //難易度説明欄パネルを非表示にする
        isDifficultyLevelExplanationPanel = false;
        ChangeViewDifficultyLevelExplanationPanel();
    }

    /// <summary>
    /// 難易度説明を表示
    /// </summary>
    /// <param name="number">難易度説明番号</param>
    public void ShowDifficultyLevelExplanation(int number)
    {
        difficultyLevelExplanationText.text = difficultyLevelExplanation.difficultyLevelExplanation[number].explanation;

        //難易度説明欄パネルを表示にする
        isDifficultyLevelExplanationPanel = true;
        ChangeViewDifficultyLevelExplanationPanel();
    }
}
