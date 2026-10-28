using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

/// <summary>
/// 明るさ調整管理クラス
/// </summary>
public class BrightnessAdjustmentController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static BrightnessAdjustmentController instance;

    /// <summary>
    /// 一番明るい数値
    /// </summary>
    const float kTheBrightestValue = 0.0f;

    /// <summary>
    /// 一番暗い数値
    /// </summary>
    const float kTheDarkestValue = 0.2f;

    /// <summary>
    /// デフォルトの明るさ
    /// </summary>
    const float kDefaultBrightnessValue = 0.075f;


    [Header("明るさのSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider brightnessAdjustmentSlider;

    /// <summary>
    /// 黒色
    /// </summary>
    Color blackColor = new Color32(0, 0, 0, 255);


    /*-------------------------------------------------------------
     * コントローラーでスライダーを選択中の色変更関連
     -------------------------------------------------------------*/

    [Header("コントローラーで選択中のスライダーの色(ボタンの文字の黄色と同じ色)")]
    [SerializeField] private Color sliderFocusedColor = new Color(239f / 255f, 227f / 255f, 26f / 255f, 1f);

    [Header("選択中に一緒に色を変える見出し文字(任意。TextMeshPro・Textどちらでも可)")]
    [SerializeField] private Graphic brightnessLabelGraphic;

    /// <summary>
    /// スライダーのつまみ(Handle)の画像
    /// </summary>
    private Image sliderHandleImage;

    /// <summary>
    /// スライダーの塗り(Fill)の画像
    /// </summary>
    private Image sliderFillImage;

    /// <summary>
    /// つまみの元の色
    /// </summary>
    private Color defaultHandleColor;

    /// <summary>
    /// 塗りの元の色
    /// </summary>
    private Color defaultFillColor;

    /// <summary>
    /// 見出し文字の元の色
    /// </summary>
    private Color defaultLabelColor;

    /// <summary>
    /// 元の色を保存したスライダー(スライダーが差し替えられた場合に保存し直すため)
    /// </summary>
    private Slider cachedSlider;

    /// <summary>
    /// スライダー選択中フラグ
    /// </summary>
    private bool isSliderFocused = false;

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

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        //brightnessAdjustmentSliderが存在する場合
        if (brightnessAdjustmentSlider != null)
        {
            //brightnessAdjustmentSliderをnullに設定
            brightnessAdjustmentSlider = null;
        }

        //スライダー選択中の色変更関連の参照をnullに設定
        brightnessLabelGraphic = null;
        sliderHandleImage = null;
        sliderFillImage = null;
        cachedSlider = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Environmentタブ内のOtherSettings内のFog欄内を設定
    /// </summary>
    public void ApplyCustomFogSettings()
    {
        //Fogを有効にする
        RenderSettings.fog = true;

        //Fog欄内のColorを設定
        RenderSettings.fogColor = blackColor;

        //Fog欄内のModeを設定
        RenderSettings.fogMode = FogMode.ExponentialSquared;
    }

    /// <summary>
    /// 明るさのスライダーに関する設定
    /// </summary>
    public void ApplyBrightnessAdjustmentSlider()
    {
        //一番明るい時の数値をスライダーの最大値として設定
        brightnessAdjustmentSlider.maxValue = kTheBrightestValue;

        //一番暗い時の数値をスライダーの最小値として設定
        brightnessAdjustmentSlider.minValue = -kTheDarkestValue;
    }


    private void Update()
    {
        //コントローラーでスライダーを選択中の場合、色を変更する
        UpdateSliderFocusColor();

        //スライダーが存在する場合&&明るさ調整パネルを開いている場合
        if (brightnessAdjustmentSlider && OptionUIController.instance.GetIsViewBrightnessAdjustmentPanel())
        {

            //スライダーの値をFog欄内のDensityへ設定
            RenderSettings.fogDensity = brightnessAdjustmentSlider.value;

            //セーブ用明るさの値をスライダーから取得
            SaveBrightnessValue();
            //brightnessValue = RenderSettings.fogDensity;

            //値が最大値を超えないように制限
            if (brightnessValue > brightnessAdjustmentSlider.maxValue) 
            {
                brightnessValue = brightnessAdjustmentSlider.maxValue;
            }

            //値が最小値を超えないように制限
            if (brightnessValue < brightnessAdjustmentSlider.minValue)
            {
                brightnessValue = brightnessAdjustmentSlider.minValue;
            }
        }
    }

    /// <summary>
    /// スライダーのつまみ・塗りの画像と元の色を保存する
    /// </summary>
    private void CacheSliderGraphics()
    {
        //既に保存済みのスライダーの場合
        if (cachedSlider == brightnessAdjustmentSlider)
        {
            //処理をスキップ
            return;
        }

        cachedSlider = brightnessAdjustmentSlider;
        isSliderFocused = false;

        //つまみの画像と元の色を保存
        sliderHandleImage = brightnessAdjustmentSlider.handleRect != null
            ? brightnessAdjustmentSlider.handleRect.GetComponent<Image>() : null;
        if (sliderHandleImage != null)
        {
            defaultHandleColor = sliderHandleImage.color;
        }

        //塗りの画像と元の色を保存
        sliderFillImage = brightnessAdjustmentSlider.fillRect != null
            ? brightnessAdjustmentSlider.fillRect.GetComponent<Image>() : null;
        if (sliderFillImage != null)
        {
            defaultFillColor = sliderFillImage.color;
        }

        //見出し文字の元の色を保存
        if (brightnessLabelGraphic != null)
        {
            defaultLabelColor = brightnessLabelGraphic.color;
        }
    }

    /// <summary>
    /// スライダーが選択中(フォーカス中)かを判定し、色を切り替える
    /// </summary>
    private void UpdateSliderFocusColor()
    {
        //スライダーが存在しない場合
        if (brightnessAdjustmentSlider == null)
        {
            //処理をスキップ
            return;
        }

        //つまみ・塗りの元の色を保存
        CacheSliderGraphics();

        //EventSystemで選択中のオブジェクトがスライダー&&スライダーが表示されている場合
        bool isFocused = EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == brightnessAdjustmentSlider.gameObject
            && brightnessAdjustmentSlider.gameObject.activeInHierarchy;

        //状態が変わっていない場合
        if (isFocused == isSliderFocused)
        {
            //処理をスキップ(毎フレーム色を書き換えないようにするため)
            return;
        }

        isSliderFocused = isFocused;
        ApplySliderFocusColor(isFocused);
    }

    /// <summary>
    /// スライダーの色を選択中の色・元の色に切り替える
    /// </summary>
    /// <param name="isFocused">true:選択中の色 / false:元の色</param>
    private void ApplySliderFocusColor(bool isFocused)
    {
        //つまみの色を変更
        if (sliderHandleImage != null)
        {
            sliderHandleImage.color = isFocused ? sliderFocusedColor : defaultHandleColor;
        }

        //塗りの色を変更
        if (sliderFillImage != null)
        {
            sliderFillImage.color = isFocused ? sliderFocusedColor : defaultFillColor;
        }

        //見出し文字の色を変更
        if (brightnessLabelGraphic != null)
        {
            brightnessLabelGraphic.color = isFocused ? sliderFocusedColor : defaultLabelColor;
        }
    }

    /// <summary>
    /// セーブ用明るさの値をスライダーから取得
    /// </summary>
    public void SaveBrightnessValue() 
    {
        //セーブ用明るさの値をスライダーから取得
        //brightnessValue = RenderSettings.fogDensity;
        brightnessValue = brightnessAdjustmentSlider.value;
    }


    /// <summary>
    /// 保存されている明るさの値をスライダーへ復元する
    /// (起動時・シーン遷移時に呼び出すこと)
    /// </summary>
    /// <param name="value">復元する明るさの値(GameController.brightnessValue)</param>
    public void RestoreBrightnessValue(float value)
    {
        //スライダーが存在しない場合
        if (brightnessAdjustmentSlider == null) 
        {
            //処理をスキップ
            return; 
        }

        /*
         * 下記の順序で処理することが重要
         * 1: ApplyBrightnessAdjustmentSlider()で明るさスライダーの最小・最大値を確定させる
         * 2:保存されていた明るさの値を設定する
         * 3: ApplyCustomFogSettings()でFog欄内の設定を適用する
         */

        //明るさスライダーの最小・最大値を設定
        ApplyBrightnessAdjustmentSlider();

        //その後に保存されていた値を設定
        brightnessAdjustmentSlider.value = value;

        //Fog欄内の設定を適用
        ApplyCustomFogSettings();
    }
}
