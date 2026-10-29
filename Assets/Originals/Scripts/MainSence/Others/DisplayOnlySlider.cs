using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 表示専用のスライダーにするクラス(体力・スタミナゲージ等)
/// ・コントローラー(十字キー・スティック)で選択枠が合わないようにする
/// ・マウス・コントローラーでバーを動かせないようにする
/// ・スクリプトからのvalue変更(ゲージの増減)は今まで通り反映される
/// 対象のSliderと同じオブジェクトにアタッチする
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Slider))]
public class DisplayOnlySlider : MonoBehaviour
{
    /// <summary>
    /// 対象のスライダー
    /// </summary>
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        ApplyDisplayOnlySettings();
    }

    private void OnEnable()
    {
        //表示されるたびに設定を確認する(他のスクリプトから設定を戻された場合に備えるため)
        ApplyDisplayOnlySettings();
    }

    private void LateUpdate()
    {
        //万が一選択された場合は選択を解除する
        DeselectIfSelected();
    }

    private void OnDestroy()
    {
        //参照をnullに設定(メモリリークを防ぐため)
        slider = null;
    }

    /// <summary>
    /// 表示専用の設定を適用する
    /// </summary>
    private void ApplyDisplayOnlySettings()
    {
        //スライダーが存在しない場合
        if (slider == null)
        {
            //処理をスキップ
            return;
        }

        //1.十字キー・スティックで選択枠が移動してこないようにする
        Navigation navigation = slider.navigation;
        navigation.mode = Navigation.Mode.None;
        slider.navigation = navigation;

        //2.選択時・無効時の色変化をなくす(interactableをオフにしても見た目が変わらないようにするため)
        slider.transition = Selectable.Transition.None;

        //3.マウス・コントローラーで値を動かせないようにする
        //(GamepadUISelectorの自動選択対象からも外れる)
        slider.interactable = false;

        //4.マウスのクリック・ドラッグ判定を無くす(ゲージの裏にあるUIの邪魔にならないようにするため)
        foreach (Graphic graphic in slider.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        //既に選択されている場合は選択を解除する
        DeselectIfSelected();
    }

    /// <summary>
    /// このスライダーが選択されている場合、選択を解除する
    /// </summary>
    private void DeselectIfSelected()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject == gameObject)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }
}
