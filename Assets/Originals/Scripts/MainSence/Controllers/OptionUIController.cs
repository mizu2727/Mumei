using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// 「オプション」ボタン内のUIを管理するクラス
/// </summary>
public class OptionUIController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static OptionUIController instance;

    [Header("オプションパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject optionPanel;

    [Header("マウス感度設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject mouseSensitivityPanel;

    [Header("音量調整設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject audioAdjustmentPanel;


    /// <summary>
    /// オプションパネル閲覧フラグ
    /// </summary>
    private bool isOptionPanel = false;

    /// <summary>
    /// 旋回速度設定パネル閲覧フラグ
    /// </summary>
    private bool isViewMouseSensitivityPanel = false;

    /// <summary>
    /// 音量調整設定パネル説欄フラグ
    /// </summary>
    private bool isViewAudioAdjustmentPanel = false;

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
    const string stringTitleScene = "TitleScene";

    /// <summary>
    /// オプションパネル閲覧フラグを取得
    /// </summary>
    /// <returns>オプションパネル閲覧フラグ</returns>
    public bool GetIsOptionPanel() 
    {
        return isOptionPanel;
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
        //オプションパネルを非表示
        isOptionPanel = false;
        ChangeOptionPanel();

        //感度設定パネルを非表示
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

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
        //インスタンスがnullの場合
        if (instance == null)
        {
            //インスタンス生成
            instance = this;

            //シーン遷移してもインスタンスが破棄されないようにする
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //インスタンスを破棄
            Destroy(this.gameObject);
        }
    }


    private void Start()
    {
        InitializeAudioSource();
    }

    /// <summary>
    /// 「オプション」ボタン押下
    /// </summary>
    public void OnClickedOptionButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //オプションパネルを表示する
        isOptionPanel = true;
        ChangeOptionPanel();

        //現在のシーン名がTitleSceneの場合
        if (SceneManager.GetActiveScene().name == stringTitleScene)
        {
            //タイトルパネルを非表示にする
            TitleController.instance.titlePanel.SetActive(false);
        }
        else 
        {
            //ポーズパネルを非表示
            PauseController.instance.isPause = false;
            PauseController.instance.ChangeViewPausePanel();
        }
    }

    /// <summary>
    /// 「感度」ボタン押下
    /// </summary>
    public void OnClickedMouseSensitivityButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //感度設定パネルを表示
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
        //感度設定パネルを非表示にする
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();


        //音量調整設定パネルを表示
        isViewAudioAdjustmentPanel = true;
        ChangeAudioAdjustmentPanel();
    }

    /// <summary>
    /// 「戻る」ボタン押下
    /// オプション設定画面を閉じる
    /// </summary>
    public void OnClickedCloseOptionButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));


        //現在のシーン名がTitleSceneの場合
        if (SceneManager.GetActiveScene().name == stringTitleScene)
        {
            //タイトルパネルを表示にする
            TitleController.instance.titlePanel.SetActive(true);
        }
        else
        {
            //ポーズパネルを表示
            PauseController.instance.isPause = true;
            PauseController.instance.ChangeViewPausePanel();
        }

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
    /// オプションパネルの表示/非表示
    /// </summary>
    void ChangeOptionPanel()
    {
        //フラグ値がオンの場合
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
        //フラグ値がオンの場合
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
        //フラグ値がオンの場合
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
}
