using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

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

    [Header("DifficultyLevelChoosePanel(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject difficultyLevelChoosePanel;

    [Header("NightmareLevelButton(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Button nightmareLevelButton;


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
    /// DifficultyLevelChoosePanelを取得する
    /// </summary>
    /// <returns>DifficultyLevelChoosePanel</returns>
    public GameObject GetDifficultyLevelChoosePanel()
    {
        return difficultyLevelChoosePanel;
    }


    private void OnDestroy()
    {
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

                break;

            //その他のシーンの場合
            default:
                Debug.LogWarning("その他のシーン名");

                break;
        }
    }

    /// <summary>
    /// EasyLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedEasyLevelButton()
    {
        SetDifficultyLevelStatus(DifficultyLevel.kEasy);

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }

    /// <summary>
    /// NormalLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNormalLevelButton() 
    {
        SetDifficultyLevelStatus(DifficultyLevel.kNormal);

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }

    /// <summary>
    /// NightmareLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNightmareLevelButton() 
    {
        SetDifficultyLevelStatus(DifficultyLevel.kNightmare);

        //ステージシーンに遷移する処理を開始する
        ChangeSceneZone.instance.ChangeStageScene();
    }
}
