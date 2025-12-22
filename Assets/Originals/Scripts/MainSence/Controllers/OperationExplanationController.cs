using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class OperationExplanationController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static OperationExplanationController instance;


    [Header("OperationPanel(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject operationPanel;

    [Header("UseItemTextPanel(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject useItemTextPanel;

    [Header("CompassTextPanel(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject compassTextPanel;


    [Header("CompassText(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject compassText;

    [Header("SelfViewCompassTextPanelButton(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject selfViewCompassTextPanelButton;

    [Header("SelfHiddenCompassTextPanelButton(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject selfHiddenCompassTextPanelButton;


    /// <summary>
    /// OperationPanel手動閲覧フラグ
    /// </summary>
    private bool isSelfViewOperationPanel = true;

    /// <summary>
    /// UseItemTextPanel手動閲覧フラグ
    /// </summary>
    private bool isSelfViewUseItemTextPanel = true;

    /// <summary>
    /// CompassTextPanel手動閲覧フラグ
    /// </summary>
    private bool isSelfViewCompassTextPanel = true;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE用audioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ボタンSEのID
    /// </summary>
    private readonly int buttonSEid = 4;

    /// <summary>
    /// TitleSceneのシーン名
    /// </summary>
    private const string stringTitleScene = "TitleScene";

    /// <summary>
    /// OpeningSceneのシーン名
    /// </summary>
    private const string stringOpeningScene = "OpeningScene";

    /// <summary>
    /// HomeScene
    /// </summary>
    private const string stringHomeScene = "HomeScene";

    /// <summary>
    /// OperationPanelを取得する
    /// </summary>
    /// <returns>OperationPanel</returns>
    public GameObject GetOperationPanel()
    {
        return operationPanel;
    }

    /// <summary>
    /// UseItemTextPanelを取得する
    /// </summary>
    /// <returns>UseItemTextPanel</returns>
    public GameObject GetUseItemTextPanel()
    {
        return useItemTextPanel;
    }

    /// <summary>
    /// CompassTextPanelを取得する
    /// </summary>
    /// <returns>CompassTextPanel</returns>
    public GameObject GetCompassTextPanel()
    {
        return compassTextPanel;
    }

    /// <summary>
    /// OperationPanel手動閲覧フラグを取得する
    /// </summary>
    /// <returns>OperationPanel手動閲覧フラグ</returns>
    public bool GetIsSelfViewOperationPanel() 
    {
        return isSelfViewOperationPanel;
    }

    /// <summary>
    /// OperationPanel手動閲覧フラグを設定する
    /// </summary>
    /// <param name="value">OperationPanel手動閲覧フラグ</param>
    public void SetIsSelfViewOperationPanel(bool value)
    {
        isSelfViewOperationPanel = value;
        ChangeOperationPanel();
    }

    /// <summary>
    /// UseItemTextPanel手動閲覧フラグを取得する
    /// </summary>
    /// <returns>UseItemTextPanel手動閲覧フラグ</returns>
    public bool GetIsSelfViewUseItemTextPanel()
    {
        return isSelfViewUseItemTextPanel;
    }

    /// <summary>
    /// UseItemTextPanel手動閲覧フラグを設定する
    /// </summary>
    /// <param name="value">UseItemTextPanel手動閲覧フラグ</param>
    public void SetIsSelfViewUseItemTextPanel(bool value)
    {
        isSelfViewUseItemTextPanel = value;
        ChangeUseItemTextPanel();
    }

    /// <summary>
    /// CompassTextPanel手動閲覧フラグを取得する
    /// </summary>
    /// <returns>CompassTextPanel手動閲覧フラグ</returns>
    public bool GetIsSelfViewCompassTextPanel() 
    {
        return isSelfViewCompassTextPanel;
    }

    /// <summary>
    /// CompassTextPanel手動閲覧フラグを設定する
    /// </summary>
    /// <param name="value">CompassTextPanel手動閲覧フラグ</param>
    public void SetIsSelfViewCompassTextPanel(bool value)
    {
        isSelfViewCompassTextPanel = value;
        ChangeCompassTextPanel();
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
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
        
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //AudioSourceを取得
        audioSourceSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceSE.playOnAwake = false;
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //現在のシーン名がTitleSceneの場合||OpeningSceneの場合
        if (stringTitleScene == SceneManager.GetActiveScene().name || stringOpeningScene == SceneManager.GetActiveScene().name)
        {
            //処理をスキップ
            return;
        }

        //現在のシーン名がHomeSceneの場合
        if (stringHomeScene == SceneManager.GetActiveScene().name)
        {
            //compassTextを非表示にする
            compassText.SetActive(false);

            //コンパス説明表示ボタンを非表示にする
            selfViewCompassTextPanelButton.SetActive(false);

            //コンパス説明非表示ボタンを非表示にする
            selfHiddenCompassTextPanelButton.SetActive(false);
        }
        else
        {
            //compassTextを表示にする
            compassText.SetActive(true);

            //コンパス説明表示ボタンを表示にする
            selfViewCompassTextPanelButton.SetActive(true);

            //コンパス説明非表示ボタンを表示にする
            selfHiddenCompassTextPanelButton.SetActive(true);
        }
    }

    /// <summary>
    /// OperationPanelの表示・非表示切り替え処理
    /// </summary>
    private void ChangeOperationPanel() 
    {
        //セーブ用のフラグ値に保存する
        isSaveSelfViewOperationPanel = isSelfViewOperationPanel;

        //OperationPanelがアタッチされていない場合
        if (operationPanel == null) 
        {
            //処理を抜ける
            return;
        }

        //フラグ値がオンの場合
        if (isSelfViewOperationPanel)
        {
            //表示
            operationPanel.SetActive(true);
        }
        else
        {
            //非表示
            operationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// UseItemTextPanelの表示・非表示切り替え処理
    /// </summary>
    private void ChangeUseItemTextPanel()
    {
        //セーブ用のフラグ値に保存する
        isSaveSelfViewUseItemTextPanel = isSelfViewUseItemTextPanel;

        //UseItemTextPanelがアタッチされていない場合
        if (useItemTextPanel == null)
        {
            //処理を抜ける
            return;
        }

        //フラグ値がオンの場合
        if (isSelfViewUseItemTextPanel)
        {
            //表示
            useItemTextPanel.SetActive(true);
        }
        else
        {
            //非表示
            useItemTextPanel.SetActive(false);
        }
    }

    /// <summary>
    /// CompassTextPanelの表示・非表示切り替え処理
    /// </summary>
    private void ChangeCompassTextPanel()
    {
        //セーブ用のフラグ値に保存する
        isSaveSelfViewCompassTextPanel = isSelfViewCompassTextPanel;

        //CompassTextPanelがアタッチされていない場合
        if (compassTextPanel == null)
        {
            //処理を抜ける
            return;
        }

        //フラグ値がオンの場合
        if (isSelfViewCompassTextPanel)
        {
            //表示
            compassTextPanel.SetActive(true);
        }
        else
        {
            //非表示
            compassTextPanel.SetActive(false);
        }
    }


    /// <summary>
    /// 「表示」ボタン押下時の処理
    /// OperationPanelを表示
    /// </summary>
    public void OnClickedSelfViewOperationPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //OperationPanelを表示
        isSelfViewOperationPanel = true;
        ChangeOperationPanel();
    }

    /// <summary>
    /// 「非表示」ボタン押下時の処理
    /// OperationPanelを非表示する
    /// </summary>
    public void OnClickedSelfHiddenOperationPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //OperationPanelを非表示
        isSelfViewOperationPanel = false;
        ChangeOperationPanel();
    }

    /// <summary>
    /// 「表示」ボタン押下時の処理
    /// UseItemTextPanelを表示
    /// </summary>
    public void OnClickedSelfViewUseItemTextPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //UseItemTextPanelを表示
        isSelfViewUseItemTextPanel = true;
        ChangeUseItemTextPanel();
    }

    /// <summary>
    /// 「非表示」ボタン押下時の処理
    /// UseItemTextPanelを非表示する
    /// </summary>
    public void OnClickedSelfHiddenUseItemTextPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //UseItemTextPanelを非表示
        isSelfViewUseItemTextPanel = false;
        ChangeUseItemTextPanel();
    }

    /// <summary>
    /// 「表示」ボタン押下時の処理
    /// CompassTextPanelを表示
    /// </summary>
    public void OnClickedSelfViewCompassTextPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //CompassTextPanelを表示
        isSelfViewCompassTextPanel = true;
        ChangeCompassTextPanel();
    }

    /// <summary>
    /// 「非表示」ボタン押下時の処理
    /// CompassTextPanelを非表示する
    /// </summary>
    public void OnClickedSelfHiddenCompassTextPanelButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //CompassTextPanelを非表示
        isSelfViewCompassTextPanel = false;
        ChangeCompassTextPanel();
    }
}
