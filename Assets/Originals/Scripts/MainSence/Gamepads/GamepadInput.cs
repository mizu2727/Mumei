using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// ゲームパッド(Xbox / PlayStation / Switch Pro)入力をまとめたクラス
/// Input SystemのGamepadクラスを使うため、各コントローラーを同じボタン配置(南/東/北ボタン等)として扱える
///
/// ※Switch Proコントローラーは任天堂の慣習に合わせ「A(右)=決定 / B(下)=戻る」に入れ替える
///   (useNintendoButtonLabelをfalseにすると、Xboxと同じ位置(下=決定)のままになる)
///
///                       Xbox        PS          Switch Pro
/// ■ストーリーモード
///   会話を進める      … A / RT      × / R2      A / ZR
/// ■それ以外のモード
///   移動              … 左スティック
///   視点移動          … 右スティック
///   ダッシュ          … LT          L2          ZL        (長押し)
///   インタラクト/決定 … A           ×           A
///   アイテム使用      … RT          R2          ZR
///   ライト点滅        … LB          L1          L
///   振り返る          … RB          R1          R         (長押し)
///   アイテムを捨てる  … Y           △          X         (長押し)
///   ポーズ/ポーズ解除 … Menu        OPTIONS     +
///   UIの「戻る」      … B           ○          B
/// </summary>
public static class GamepadInput
{
    /// <summary>
    /// Switch Proコントローラー使用時、任天堂の慣習(A=決定 / B=戻る)に合わせて決定・戻るを入れ替えるか
    /// </summary>
    public static bool useNintendoButtonLabel = true;

    /// <summary>
    /// 現在接続されているゲームパッド(未接続の場合はnull)
    /// </summary>
    private static Gamepad Pad => Gamepad.current;

    /*-------------------------------------------------------------
     * コントローラーの抜き差し関連
     -------------------------------------------------------------*/

    /// <summary>
    /// コントローラーが接続された瞬間に呼ばれるイベント
    /// 例:GamepadInput.OnGamepadConnected += (pad) => { ... };
    /// </summary>
    public static event Action<Gamepad> OnGamepadConnected;

    /// <summary>
    /// コントローラーが切断された瞬間に呼ばれるイベント
    /// 例:GamepadInput.OnGamepadDisconnected += (pad) => { ... };
    /// </summary>
    public static event Action<Gamepad> OnGamepadDisconnected;

    /// <summary>
    /// ゲーム起動時の初期化(エディタで「Enter Play Mode Options」によりstatic変数が残る場合に備えてリセットする)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetDeviceChangeEvents()
    {
        OnGamepadConnected = null;
        OnGamepadDisconnected = null;
        InputSystem.onDeviceChange -= HandleDeviceChange;
    }

    /// <summary>
    /// ゲーム起動時に、デバイスの接続状態の変化を監視する処理を登録する
    /// (シーンにアタッチしなくても自動で動く)
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterDeviceChangeEvent()
    {
        //二重登録を防ぐため、一度解除してから登録する
        InputSystem.onDeviceChange -= HandleDeviceChange;
        InputSystem.onDeviceChange += HandleDeviceChange;
    }

    /// <summary>
    /// デバイスの接続状態が変わった時に呼ばれる
    /// </summary>
    /// <param name="device">対象のデバイス</param>
    /// <param name="change">変化の種類</param>
    private static void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        //ゲームパッド以外(キーボード・マウス等)は無視
        Gamepad gamepad = device as Gamepad;
        if (gamepad == null)
        {
            return;
        }

        switch (change)
        {
            //接続された(初めて接続・抜いて差し直した・Bluetoothが再接続した)
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                OnGamepadConnected?.Invoke(gamepad);
                break;

            //切断された(ケーブルを抜いた・電池切れ・Bluetoothが切れた)
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                OnGamepadDisconnected?.Invoke(gamepad);
                break;
        }
    }

    /// <summary>
    /// ゲームパッドが接続されているか
    /// </summary>
    public static bool IsConnected => Gamepad.current != null;

    /// <summary>
    /// 現在のゲームパッドがSwitch Proコントローラーか
    /// (プラットフォームによってはSwitchProControllerHIDクラスが存在しないため、レイアウト名で判定する)
    /// </summary>
    public static bool IsSwitchProController => Pad != null && Pad.layout.Contains("SwitchPro");

    /// <summary>
    /// 決定・戻るを任天堂の配置(A=右 / B=下)で扱うか
    /// </summary>
    public static bool IsNintendoLayout => useNintendoButtonLabel && IsSwitchProController;

    /// <summary>
    /// 決定ボタン(Xbox:A / PS:× / Switch:A)
    /// </summary>
    private static ButtonControl ConfirmButton => IsNintendoLayout ? Pad.buttonEast : Pad.buttonSouth;

    /// <summary>
    /// 戻るボタン(Xbox:B / PS:○ / Switch:B)
    /// </summary>
    private static ButtonControl BackButton => IsNintendoLayout ? Pad.buttonSouth : Pad.buttonEast;

    /// <summary>
    /// 移動入力(左スティック)を取得
    /// </summary>
    /// <returns>移動入力(-1から1)</returns>
    public static Vector2 GetMove()
    {
        return Pad != null ? Pad.leftStick.ReadValue() : Vector2.zero;
    }

    /// <summary>
    /// 視点移動入力(右スティック)を取得
    /// </summary>
    /// <returns>視点移動入力(-1から1)</returns>
    public static Vector2 GetLook()
    {
        return Pad != null ? Pad.rightStick.ReadValue() : Vector2.zero;
    }

    /// <summary>
    /// ダッシュ(LT / L2 / ZL)を押しているか
    /// </summary>
    public static bool IsDashHeld()
    {
        return Pad != null && Pad.leftTrigger.isPressed;
    }

    /// <summary>
    /// インタラクト・決定(A / × / A)を押した瞬間か
    /// </summary>
    public static bool IsInteractPressed()
    {
        return Pad != null && ConfirmButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 会話を進める(決定ボタン または RT / R2 / ZR)を押した瞬間か
    /// </summary>
    public static bool IsStoryAdvancePressed()
    {
        return Pad != null && (ConfirmButton.wasPressedThisFrame || Pad.rightTrigger.wasPressedThisFrame);
    }

    /// <summary>
    /// アイテム使用(RT / R2 / ZR)を押した瞬間か
    /// </summary>
    public static bool IsUseItemPressed()
    {
        return Pad != null && Pad.rightTrigger.wasPressedThisFrame;
    }

    /// <summary>
    /// ライト点滅(LB / L1 / L)を押した瞬間か
    /// </summary>
    public static bool IsLightPressed()
    {
        return Pad != null && Pad.leftShoulder.wasPressedThisFrame;
    }

    /// <summary>
    /// 振り返る(RB / R1 / R)を押しているか
    /// </summary>
    public static bool IsBackRotateHeld()
    {
        return Pad != null && Pad.rightShoulder.isPressed;
    }

    /// <summary>
    /// アイテムを捨てる(Y / △ / X ※いずれも上のボタン)を押しているか(長押し判定は呼び出し側で行う)
    /// </summary>
    public static bool IsDropItemHeld()
    {
        return Pad != null && Pad.buttonNorth.isPressed;
    }

    /// <summary>
    /// ポーズ(Menu / OPTIONS / +)を押した瞬間か
    /// </summary>
    public static bool IsPausePressed()
    {
        return Pad != null && Pad.startButton.wasPressedThisFrame;
    }

    /// <summary>
    /// UIの「戻る」(B / ○ / B)を押した瞬間か
    /// </summary>
    public static bool IsCancelPressed()
    {
        return Pad != null && BackButton.wasPressedThisFrame;
    }

    /// <summary>
    /// UI操作用の入力(十字キー・左スティック・決定)があったか
    /// </summary>
    public static bool HasNavigationInput()
    {
        if (Pad == null)
        {
            return false;
        }

        return Pad.dpad.ReadValue().sqrMagnitude > 0.25f
            || Pad.leftStick.ReadValue().sqrMagnitude > 0.25f
            || ConfirmButton.wasPressedThisFrame;
    }

    /// <summary>
    /// 絶対値が大きい方の値を返す
    /// (キーボードとゲームパッドの入力を合算すると2倍速になるのを防ぐため)
    /// </summary>
    /// <param name="a">値A</param>
    /// <param name="b">値B</param>
    /// <returns>絶対値が大きい方の値</returns>
    public static float AbsMax(float a, float b)
    {
        return Mathf.Abs(a) >= Mathf.Abs(b) ? a : b;
    }
}
