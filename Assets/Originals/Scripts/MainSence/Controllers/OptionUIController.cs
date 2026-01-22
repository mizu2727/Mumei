using UnityEngine;
using UnityEngine.Animations.Rigging;
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

    [Header("明るさ調整設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject brightnessAdjustmentPanel;

    [Header("スクリーン調整設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject screenAdjustmentPanel;

    [Header("説明関係設定パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject explanationPanel;


    [Header("説明関係ボタン(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject explanationButton;


    /// <summary>
    /// オプションパネル閲覧フラグ
    /// </summary>
    private bool isOptionPanel = false;

    /// <summary>
    /// マウス感度設定パネル閲覧フラグ
    /// </summary>
    private bool isViewMouseSensitivityPanel = false;

    /// <summary>
    /// 音量調整設定パネル閲覧フラグ
    /// </summary>
    private bool isViewAudioAdjustmentPanel = false;

    /// <summary>
    /// 明るさ調整設定パネル閲覧フラグ
    /// </summary>
    private bool isViewBrightnessAdjustmentPanel = false;

    /// <summary>
    /// スクリーン調整設定パネル閲覧フラグ
    /// </summary>
    private bool isViewScreenAdjustmentPanel = false;

    /// <summary>
    /// 説明関係設定パネル閲覧フラグ
    /// </summary>
    private bool isViewExplanationPanel = false;

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

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();

        //現在のシーン名がTitleSceneの場合
        if (stringTitleScene == SceneManager.GetActiveScene().name)
        {
            //説明ボタンを非表示にする
            explanationButton.SetActive(false);
        }
        else 
        {
            //説明ボタンを表示する
            explanationButton.SetActive(true);
        }
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

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();


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

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();


        //音量調整設定パネルを表示
        isViewAudioAdjustmentPanel = true;
        ChangeAudioAdjustmentPanel();
    }

    /// <summary>
    /// 「暗さ」ボタン押下
    /// </summary>
    public void OnClickedBrightnessAdjustmentButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //感度設定パネルを非表示にする
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();


        //明るさ調整設定パネルを表示
        isViewBrightnessAdjustmentPanel = true;
        ChangeBrightnessAdjustmentPanel();
    }

    /// <summary>
    /// 「画面」ボタン押下時の処理
    /// </summary>
    public void OnClickedScreenAdjustmentButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //感度設定パネルを非表示にする
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();


        //スクリーン調整設定パネルを表示
        isViewScreenAdjustmentPanel = true;
        ChangeScreenAdjustmentPanel();
    }

    /// <summary>
    /// 「説明」ボタン押下時の処理
    /// </summary>
    public void OnClickedExplanationButton()
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //他の設定パネルを非表示にする
        //感度設定パネルを非表示にする
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();


        //説明関係設定パネルを表示
        isViewExplanationPanel = true;
        ChangeExplanationPanel();
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
            PauseController.instance.GetPausePanel().transform.SetAsLastSibling();
            PauseController.instance.isPause = true;
            PauseController.instance.ChangeViewPausePanel();
        }

        //旋回速度設定パネルを非表示
        isViewMouseSensitivityPanel = false;
        ChangeMouseSensitivityPanel();

        //音量調整設定パネルを非表示
        isViewAudioAdjustmentPanel = false;
        ChangeAudioAdjustmentPanel();

        //明るさ調整設定パネルを非表示
        isViewBrightnessAdjustmentPanel = false;
        ChangeBrightnessAdjustmentPanel();

        //スクリーン調整設定パネルを非表示
        isViewScreenAdjustmentPanel = false;
        ChangeScreenAdjustmentPanel();

        //説明関係設定パネルを非表示
        isViewExplanationPanel = false;
        ChangeExplanationPanel();

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
    /// 感度設定パネルの表示/非表示
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

    /// <summary>
    /// 明るさ調整設定パネルの表示/非表示
    /// </summary>
    void ChangeBrightnessAdjustmentPanel()
    {
        //フラグ値がオンの場合
        if (isViewBrightnessAdjustmentPanel)
        {
            //表示
            brightnessAdjustmentPanel.SetActive(true);
        }
        else
        {
            //非表示
            brightnessAdjustmentPanel.SetActive(false);
        }
    }

    /// <summary>
    /// スクリーン調整設定パネルの表示/非表示
    /// </summary>
    void ChangeScreenAdjustmentPanel()
    {
        //フラグ値がオンの場合
        if (isViewScreenAdjustmentPanel)
        {
            //表示
            screenAdjustmentPanel.SetActive(true);
        }
        else
        {
            //非表示
            screenAdjustmentPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 説明関係設定パネルの表示/非表示
    /// </summary>
    private void ChangeExplanationPanel()
    {
        //現在のシーン名がTitleSceneの場合
        if (stringTitleScene == SceneManager.GetActiveScene().name)
        {
            //処理をスキップ
            return;
        }

        //フラグ値がオンの場合
        if (isViewExplanationPanel)
        {
            //表示
            explanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            explanationPanel.SetActive(false);
        }
    }
}
