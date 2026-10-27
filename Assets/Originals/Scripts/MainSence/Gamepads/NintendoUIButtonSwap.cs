using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Switch Proコントローラー使用時に、UIの決定・戻るを任天堂の慣習(A(右)=決定 / B(下)=戻る)に入れ替えるクラス
/// EventSystem(InputSystemUIInputModuleと同じオブジェクト)にアタッチする
/// ※GamepadUISelectorが自動で追加するため、手動でのアタッチは不要
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(InputSystemUIInputModule))]
public class NintendoUIButtonSwap : MonoBehaviour
{
    /// <summary>
    /// InputSystemUIInputModule
    /// </summary>
    private InputSystemUIInputModule inputModule;

    /// <summary>
    /// 決定アクション
    /// </summary>
    private InputAction submitAction;

    /// <summary>
    /// 戻るアクション
    /// </summary>
    private InputAction cancelAction;

    /// <summary>
    /// 決定アクションに追加したSwitch用バインディングの番号
    /// </summary>
    private int submitSwitchBindingIndex = -1;

    /// <summary>
    /// 戻るアクションに追加したSwitch用バインディングの番号
    /// </summary>
    private int cancelSwitchBindingIndex = -1;

    /// <summary>
    /// 初期化済みフラグ
    /// </summary>
    private bool isInitialized = false;

    /// <summary>
    /// 現在、任天堂配置を適用しているか
    /// </summary>
    private bool isAppliedNintendoLayout = false;

    /// <summary>
    /// Switch用 決定ボタン(右のAボタン)
    /// </summary>
    private const string kSwitchSubmitPath = "<SwitchProControllerHID>/buttonEast";

    /// <summary>
    /// Switch用 戻るボタン(下のBボタン)
    /// </summary>
    private const string kSwitchCancelPath = "<SwitchProControllerHID>/buttonSouth";

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        //使用中のコントローラーが切り替わった場合のみ適用し直す
        bool isNintendo = GamepadInput.IsNintendoLayout;
        if (isNintendo != isAppliedNintendoLayout)
        {
            ApplyLayout(isNintendo);
        }
    }

    /// <summary>
    /// 初期化(UI用アクションを複製し、Switch用のバインディングを追加する)
    /// </summary>
    private void Initialize()
    {
        inputModule = GetComponent<InputSystemUIInputModule>();
        if (inputModule == null || inputModule.actionsAsset == null)
        {
            Debug.LogWarning("[NintendoUIButtonSwap] InputSystemUIInputModuleのActions Assetが設定されていません");
            return;
        }

        //プロジェクト内のアセット(DefaultInputActions等)を直接書き換えないよう、複製して差し替える
        //(InputSystemUIInputModuleはアセット差し替え時に、同名のアクションへ参照を付け替える)
        inputModule.actionsAsset = Instantiate(inputModule.actionsAsset);

        submitAction = inputModule.submit != null ? inputModule.submit.action : null;
        cancelAction = inputModule.cancel != null ? inputModule.cancel.action : null;
        if (submitAction == null || cancelAction == null)
        {
            Debug.LogWarning("[NintendoUIButtonSwap] Submit / Cancelアクションが見つかりません");
            return;
        }

        //バインディングの追加はアクションが無効な状態で行う必要がある
        inputModule.actionsAsset.Disable();
        submitSwitchBindingIndex = AddBinding(submitAction, kSwitchSubmitPath);
        cancelSwitchBindingIndex = AddBinding(cancelAction, kSwitchCancelPath);
        isInitialized = true;

        //現在のコントローラーに合わせて適用(内部でアクションを有効化する)
        ApplyLayout(GamepadInput.IsNintendoLayout);
    }

    /// <summary>
    /// アクションにバインディングを追加し、その番号を返す
    /// </summary>
    /// <param name="action">対象のアクション</param>
    /// <param name="path">バインディングのパス</param>
    /// <returns>追加したバインディングの番号</returns>
    private int AddBinding(InputAction action, string path)
    {
        action.AddBinding(path);
        return action.bindings.Count - 1;
    }

    /// <summary>
    /// 決定・戻るのバインディングを切り替える
    /// </summary>
    /// <param name="isNintendo">true:任天堂配置(A=右で決定) / false:通常配置(下で決定)</param>
    private void ApplyLayout(bool isNintendo)
    {
        //バインディングの変更中は一旦無効化する
        inputModule.actionsAsset.Disable();

        ApplyLayoutToAction(submitAction, submitSwitchBindingIndex, isNintendo);
        ApplyLayoutToAction(cancelAction, cancelSwitchBindingIndex, isNintendo);

        inputModule.actionsAsset.Enable();
        isAppliedNintendoLayout = isNintendo;
    }

    /// <summary>
    /// 1つのアクションのバインディングを切り替える
    /// </summary>
    /// <param name="action">対象のアクション</param>
    /// <param name="switchBindingIndex">Switch用バインディングの番号</param>
    /// <param name="isNintendo">任天堂配置にするか</param>
    private void ApplyLayoutToAction(InputAction action, int switchBindingIndex, bool isNintendo)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            //コンポジット(WASD等)は対象外
            if (binding.isComposite || binding.isPartOfComposite)
            {
                continue;
            }

            //追加したSwitch用バインディング:任天堂配置時のみ有効(空文字のオーバーライドで無効化される)
            if (i == switchBindingIndex)
            {
                if (isNintendo) action.RemoveBindingOverride(i);
                else action.ApplyBindingOverride(i, "");
                continue;
            }

            if (!isNintendo)
            {
                //通常配置:元のバインディングに戻す
                action.RemoveBindingOverride(i);
                continue;
            }

            //任天堂配置:ゲームパッドの下ボタン(Submit)・右ボタン(Cancel)の反応を止め、キーボード等は残す
            string path = binding.path;
            if (path.StartsWith("*/"))
            {
                //"*/{Submit}"等の全デバイス共通のバインディングは、キーボードのみに限定する
                action.ApplyBindingOverride(i, "<Keyboard>/" + path.Substring(2));
            }
            else if (path.StartsWith("<Gamepad>") || path.StartsWith("<SwitchProControllerHID>"))
            {
                //ゲームパッド用のバインディングは無効化
                action.ApplyBindingOverride(i, "");
            }
        }
    }
}
