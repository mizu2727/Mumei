using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲームパッド(Xbox / PlayStation)入力をまとめたクラス
/// Input SystemのGamepadクラスを使うため、XboxとPSで同じボタン配置(南/東/北ボタン等)として扱える
///
/// ■ストーリーモード
///   会話を進める      … A / ×  または RT / R2
/// ■それ以外のモード
///   移動              … 左スティック
///   視点移動          … 右スティック
///   ダッシュ          … LT / L2 長押し
///   インタラクト/決定 … A / ×
///   アイテム使用      … RT / R2
///   ライト点滅        … LB / L1
///   振り返る          … RB / R1 長押し
///   アイテムを捨てる  … Y / △ 長押し
///   ポーズ/ポーズ解除 … Menu / OPTIONS
///   UIの「戻る」      … B / ○
/// </summary>
public static class GamepadInput
{
    /// <summary>
    /// 現在接続されているゲームパッド(未接続の場合はnull)
    /// </summary>
    private static Gamepad Pad => Gamepad.current;

    /// <summary>
    /// ゲームパッドが接続されているか
    /// </summary>
    public static bool IsConnected => Gamepad.current != null;

    /// <summary>
    /// 移動入力(左スティック)を取得
    /// </summary>
    /// <returns>移動入力(-1～1)</returns>
    public static Vector2 GetMove()
    {
        return Pad != null ? Pad.leftStick.ReadValue() : Vector2.zero;
    }

    /// <summary>
    /// 視点移動入力(右スティック)を取得
    /// </summary>
    /// <returns>視点移動入力(-1～1)</returns>
    public static Vector2 GetLook()
    {
        return Pad != null ? Pad.rightStick.ReadValue() : Vector2.zero;
    }

    /// <summary>
    /// ダッシュ(LT / L2)を押しているか
    /// </summary>
    public static bool IsDashHeld()
    {
        return Pad != null && Pad.leftTrigger.isPressed;
    }

    /// <summary>
    /// インタラクト・決定(A / ×)を押した瞬間か
    /// </summary>
    public static bool IsInteractPressed()
    {
        return Pad != null && Pad.buttonSouth.wasPressedThisFrame;
    }

    /// <summary>
    /// 会話を進める(A / × または RT / R2)を押した瞬間か
    /// </summary>
    public static bool IsStoryAdvancePressed()
    {
        return Pad != null && (Pad.buttonSouth.wasPressedThisFrame || Pad.rightTrigger.wasPressedThisFrame);
    }

    /// <summary>
    /// アイテム使用(RT / R2)を押した瞬間か
    /// </summary>
    public static bool IsUseItemPressed()
    {
        return Pad != null && Pad.rightTrigger.wasPressedThisFrame;
    }

    /// <summary>
    /// ライト点滅(LB / L1)を押した瞬間か
    /// </summary>
    public static bool IsLightPressed()
    {
        return Pad != null && Pad.leftShoulder.wasPressedThisFrame;
    }

    /// <summary>
    /// 振り返る(RB / R1)を押しているか
    /// </summary>
    public static bool IsBackRotateHeld()
    {
        return Pad != null && Pad.rightShoulder.isPressed;
    }

    /// <summary>
    /// アイテムを捨てる(Y / △)を押しているか(長押し判定は呼び出し側で行う)
    /// </summary>
    public static bool IsDropItemHeld()
    {
        return Pad != null && Pad.buttonNorth.isPressed;
    }

    /// <summary>
    /// ポーズ(Menu / OPTIONS)を押した瞬間か
    /// </summary>
    public static bool IsPausePressed()
    {
        return Pad != null && Pad.startButton.wasPressedThisFrame;
    }

    /// <summary>
    /// UIの「戻る」(B / ○)を押した瞬間か
    /// </summary>
    public static bool IsCancelPressed()
    {
        return Pad != null && Pad.buttonEast.wasPressedThisFrame;
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
            || Pad.buttonSouth.wasPressedThisFrame;
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
