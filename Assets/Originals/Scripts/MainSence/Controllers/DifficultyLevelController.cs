using UnityEngine;
using static GameController;
using UnityEngine.UI;

public class DifficultyLevelController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static DifficultyLevelController instance;


    [Header("DifficultyLevelChoosePanel(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Object difficultyLevelChoosePanel;

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
        difficultyLevelStatus = status;
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


    private void Start()
    {
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
    }

    /// <summary>
    /// EasyLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedEasyLevelButton()
    {
        SetDifficultyLevelStatus(DifficultyLevel.kEasy);
    }

    /// <summary>
    /// NormalLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNormalLevelButton() 
    {
        SetDifficultyLevelStatus(DifficultyLevel.kNormal);
    }

    /// <summary>
    /// NightmareLevelButtonボタン押下時に呼ばれる関数
    /// </summary>
    public void OnClickedNightmareLevelButton() 
    {
        SetDifficultyLevelStatus(DifficultyLevel.kNightmare);
    }
}
