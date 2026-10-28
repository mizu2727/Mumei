using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// コントローラー(EventSystem)でスライダーを選択中に、スライダーの色を変更するクラス
/// 1つのスライダーにつき1つ作成し、各コントローラーのUpdate関数からUpdateFocusColorを呼ぶ
/// (MonoBehaviourではないため、アタッチ不要)
/// </summary>
public class SliderFocusColorChanger
{
    /// <summary>
    /// 選択中の色(ボタンの文字の黄色と同じ色)
    /// </summary>
    public static readonly Color kDefaultFocusedColor = new Color(239f / 255f, 227f / 255f, 26f / 255f, 1f);

    /// <summary>
    /// 選択中の色
    /// </summary>
    private readonly Color focusedColor;

    /// <summary>
    /// 元の色を保存したスライダー
    /// </summary>
    private Slider cachedSlider;

    /// <summary>
    /// 元の色を保存した見出し文字
    /// </summary>
    private Graphic cachedLabel;

    /// <summary>
    /// スライダーのつまみ(Handle)の画像
    /// </summary>
    private Image handleImage;

    /// <summary>
    /// スライダーの塗り(Fill)の画像
    /// </summary>
    private Image fillImage;

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
    /// スライダー選択中フラグ
    /// </summary>
    private bool isFocused = false;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="focusedColor">選択中の色</param>
    public SliderFocusColorChanger(Color focusedColor)
    {
        this.focusedColor = focusedColor;
    }

    /// <summary>
    /// コンストラクタ(選択中の色はボタンの文字と同じ黄色)
    /// </summary>
    public SliderFocusColorChanger() : this(kDefaultFocusedColor)
    {
    }

    /// <summary>
    /// スライダーが選択中かを判定し、色を切り替える(毎フレーム呼ぶ)
    /// </summary>
    /// <param name="slider">対象のスライダー</param>
    /// <param name="label">一緒に色を変える見出し文字(不要な場合はnull)</param>
    public void UpdateFocusColor(Slider slider, Graphic label = null)
    {
        //スライダーが存在しない場合
        if (slider == null)
        {
            //処理をスキップ
            return;
        }

        //スライダー・見出し文字が変わった場合は元の色を保存し直す
        if (slider != cachedSlider || label != cachedLabel)
        {
            Cache(slider, label);
        }

        //EventSystemで選択中のオブジェクトがスライダー&&スライダーが表示されている場合
        bool isNowFocused = EventSystem.current != null
            && EventSystem.current.currentSelectedGameObject == slider.gameObject
            && slider.gameObject.activeInHierarchy;

        //状態が変わっていない場合
        if (isNowFocused == isFocused)
        {
            //処理をスキップ(毎フレーム色を書き換えないようにするため)
            return;
        }

        isFocused = isNowFocused;
        ApplyColor(isNowFocused);
    }

    /// <summary>
    /// スライダーのつまみ・塗り・見出し文字の元の色を保存する
    /// </summary>
    /// <param name="slider">対象のスライダー</param>
    /// <param name="label">見出し文字</param>
    private void Cache(Slider slider, Graphic label)
    {
        //前のスライダーが選択中の色のままの場合は元の色に戻す
        if (isFocused)
        {
            ApplyColor(false);
            isFocused = false;
        }

        cachedSlider = slider;
        cachedLabel = label;

        //つまみの画像と元の色を保存
        handleImage = slider.handleRect != null ? slider.handleRect.GetComponent<Image>() : null;
        if (handleImage != null)
        {
            defaultHandleColor = handleImage.color;
        }

        //塗りの画像と元の色を保存
        fillImage = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
        if (fillImage != null)
        {
            defaultFillColor = fillImage.color;
        }

        //見出し文字の元の色を保存
        if (label != null)
        {
            defaultLabelColor = label.color;
        }
    }

    /// <summary>
    /// 選択中の色・元の色に切り替える
    /// </summary>
    /// <param name="isFocusedColor">true:選択中の色 / false:元の色</param>
    private void ApplyColor(bool isFocusedColor)
    {
        //つまみの色を変更
        if (handleImage != null)
        {
            handleImage.color = isFocusedColor ? focusedColor : defaultHandleColor;
        }

        //塗りの色を変更
        if (fillImage != null)
        {
            fillImage.color = isFocusedColor ? focusedColor : defaultFillColor;
        }

        //見出し文字の色を変更
        if (cachedLabel != null)
        {
            cachedLabel.color = isFocusedColor ? focusedColor : defaultLabelColor;
        }
    }

    /// <summary>
    /// 参照を解放する(オブジェクト破棄時に呼ぶ)
    /// </summary>
    public void Release()
    {
        cachedSlider = null;
        cachedLabel = null;
        handleImage = null;
        fillImage = null;
        isFocused = false;
    }
}
