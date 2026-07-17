using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;


//明るさ調整管理クラス
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
        //スライダーが存在する場合
        if (brightnessAdjustmentSlider)
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
