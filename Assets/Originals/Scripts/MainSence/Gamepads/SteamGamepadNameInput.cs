using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

/// <summary>
/// コントローラー操作時に、Steam公式のバーチャルキーボード(ShowGamepadTextInput)で
/// プレイヤー名を入力させるクラス
/// プレイヤー名入力用のInputField(MessageController.inputPlayerNameField)と同じオブジェクトにアタッチする
///
/// ・InputFieldが表示された時、コントローラーが接続されていればSteamキーボードを開く
/// ・Steamキーボードで決定 → MessageController.SavePlayerName()を呼び、既存の確認パネルの流れへ進む
/// ・Steamキーボードでキャンセル → A(決定ボタン)で再度開く / キーボードを打てば通常のInputField入力に切り替わる
/// ・Steamキーボードを開けない場合(Big Picture・Steam Deck以外、エディタ実行時、Steam未起動時)は通常のInputField入力になる
///
/// ※Steamworks.NETが必要。Steamworks.NETを使わないビルドではDISABLESTEAMWORKSを定義すると通常入力のみになる
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(InputField))]
public class SteamGamepadNameInput : MonoBehaviour
{
    [Header("名前の最大文字数(MessageControllerの判定(10文字以下)と合わせること)")]
    [SerializeField] private uint maxNameLength = 10;

    [Header("Steamキーボードをキャンセルした後に表示する案内(任意。例:「A:キーボードを開く」)")]
    [SerializeField] private GameObject reopenGuide;

    /// <summary>
    /// プレイヤー名入力用のInputField
    /// </summary>
    private InputField inputField;

    /// <summary>
    /// Steamキーボードを表示中か
    /// </summary>
    private bool isKeyboardOpen = false;

    /// <summary>
    /// Steamキーボードで入力するモードか(falseの場合は通常のInputField入力)
    /// </summary>
    private bool isSteamKeyboardMode = false;

    /// <summary>
    /// Steamキーボードが閉じられた結果を受け取ったか
    /// (Steamのコールバック内でInputFieldを非表示にするとコールバックの破棄と重なるため、次のUpdateで処理する)
    /// </summary>
    private bool hasDismissedResult = false;

    /// <summary>
    /// Steamキーボードで「決定」されたか(falseはキャンセル)
    /// </summary>
    private bool isSubmitted = false;

#if !DISABLESTEAMWORKS
    /// <summary>
    /// Steamキーボードが閉じられた時のコールバック
    /// </summary>
    private Callback<GamepadTextInputDismissed_t> dismissedCallback;
#endif

    private void Awake()
    {
        inputField = GetComponent<InputField>();
    }

    private void OnEnable()
    {
        isKeyboardOpen = false;
        isSteamKeyboardMode = false;
        hasDismissedResult = false;
        SetReopenGuide(false);

        //コントローラー操作時は、InputFieldが先に選択・入力状態になるのを防ぐため、一旦操作不可にする
        //(InputFieldが入力状態のままSteamキーボードを開くと、フォーカスが外れた際にOn End Editが呼ばれてしまうため)
        if (GamepadInput.IsConnected)
        {
            inputField.interactable = false;
        }

        //シーン読み込み直後にMessageControllerのAwakeで非表示にされる場合があるため、1フレーム待ってから開く
        //(非表示にされた場合はコルーチンも止まる)
        StartCoroutine(OpenSteamKeyboardNextFrame());
    }

    private void OnDisable()
    {
#if !DISABLESTEAMWORKS
        //コールバックを破棄
        if (dismissedCallback != null)
        {
            dismissedCallback.Dispose();
            dismissedCallback = null;
        }
#endif
        isKeyboardOpen = false;
        isSteamKeyboardMode = false;

        //次に表示した時のためにInputFieldを操作可能に戻す
        if (inputField != null)
        {
            inputField.interactable = true;
        }

        SetReopenGuide(false);
    }

    private void OnDestroy()
    {
        //参照をnullに設定(メモリリークを防ぐため)
        inputField = null;
        reopenGuide = null;
    }

    /// <summary>
    /// 1フレーム待ってからSteamキーボードを開く
    /// </summary>
    private IEnumerator OpenSteamKeyboardNextFrame()
    {
        yield return null;

        if (GamepadInput.IsConnected)
        {
            TryOpenSteamKeyboard();
        }
        else
        {
            //キーボード・マウス操作時は通常のInputField入力
            SwitchToInputField();
        }
    }

    private void Update()
    {
        //Steamキーボードが閉じられた結果を処理
        if (hasDismissedResult)
        {
            hasDismissedResult = false;
            HandleDismissedResult();
            return;
        }

        //Steamキーボード入力モードでない場合||Steamキーボード表示中の場合
        if (!isSteamKeyboardMode || isKeyboardOpen)
        {
            //処理をスキップ
            return;
        }

        //キャンセル後にA(決定ボタン)を押した場合、Steamキーボードを開き直す
        if (GamepadInput.IsInteractPressed())
        {
            TryOpenSteamKeyboard();
            return;
        }

        //キャンセル後にキーボードを打った場合、通常のInputField入力に切り替える
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SwitchToInputField();
        }
    }

    /// <summary>
    /// Steamキーボードを開く。開けない場合は通常のInputField入力に切り替える
    /// </summary>
    private void TryOpenSteamKeyboard()
    {
        bool isOpened = false;

#if !DISABLESTEAMWORKS
        try
        {
            //閉じられた時のコールバックを登録
            if (dismissedCallback == null)
            {
                dismissedCallback = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);
            }

            //Steamキーボードを表示(Big Picture・Steam Deck以外ではfalseが返る)
            isOpened = SteamUtils.ShowGamepadTextInput(
                EGamepadTextInputMode.k_EGamepadTextInputModeNormal,
                EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,
                GetDescriptionText(),
                maxNameLength,
                inputField.text ?? "");
        }
        catch (Exception e)
        {
            //Steamが初期化されていない場合(エディタ実行時・Steam未起動時等)
            Debug.LogWarning("[SteamGamepadNameInput] Steamキーボードを開けませんでした。通常の入力に切り替えます: " + e.Message);
            isOpened = false;
        }
#endif

        if (!isOpened)
        {
            SwitchToInputField();
            return;
        }

        isSteamKeyboardMode = true;
        isKeyboardOpen = true;
        inputField.interactable = false;
        SetReopenGuide(false);
    }

#if !DISABLESTEAMWORKS
    /// <summary>
    /// Steamキーボードが閉じられた時に呼ばれる(SteamAPI.RunCallbacks内から呼ばれる)
    /// </summary>
    /// <param name="result">結果</param>
    private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t result)
    {
        isSubmitted = result.m_bSubmitted;
        hasDismissedResult = true;
    }
#endif

    /// <summary>
    /// Steamキーボードが閉じられた結果を処理する
    /// </summary>
    private void HandleDismissedResult()
    {
        isKeyboardOpen = false;

        //キャンセルされた場合
        if (!isSubmitted)
        {
            //開き直しの案内を表示して待機
            SetReopenGuide(true);
            return;
        }

        //入力された文字を取得
        string playerName = "";
#if !DISABLESTEAMWORKS
        try
        {
            //文字数はUTF-8のバイト数で返る場合があるため、余裕を持ったサイズで受け取る
            uint length = SteamUtils.GetEnteredGamepadTextLength();
            SteamUtils.GetEnteredGamepadTextInput(out playerName, (length + 1) * 4);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SteamGamepadNameInput] 入力文字を取得できませんでした: " + e.Message);
        }
#endif
        playerName = (playerName ?? "").Trim();

        //InputFieldにも反映(確認パネル・「いいえ」で戻った時に表示されるようにするため)
        inputField.text = playerName;
        inputField.interactable = true;
        isSteamKeyboardMode = false;

        //既存の名前保存処理へ(文字数チェック・確認パネル表示はMessageController側で行う)
        if (MessageController.instance != null)
        {
            MessageController.instance.SavePlayerName(playerName);
        }
    }

    /// <summary>
    /// 通常のInputField入力に切り替える
    /// </summary>
    private void SwitchToInputField()
    {
        isSteamKeyboardMode = false;
        isKeyboardOpen = false;
        SetReopenGuide(false);

        inputField.interactable = true;
        inputField.Select();
        inputField.ActivateInputField();
    }

    /// <summary>
    /// 開き直しの案内の表示・非表示
    /// </summary>
    /// <param name="isView">表示するか</param>
    private void SetReopenGuide(bool isView)
    {
        if (reopenGuide != null)
        {
            reopenGuide.SetActive(isView);
        }
    }

    /// <summary>
    /// Steamキーボードに表示する説明文を現在の言語で取得する
    /// </summary>
    /// <returns>説明文</returns>
    private string GetDescriptionText()
    {
        if (LanguageController.instance == null)
        {
            return "プレイヤー名を入力してください（2～10文字）";
        }

        switch (LanguageController.instance.GetLanguageStatus())
        {
            case LanguageController.LanguageStatus.kEnglish: return "Enter your player name (2-10 characters)";
            case LanguageController.LanguageStatus.kSimplifiedChinese: return "请输入玩家名称（2～10个字符）";
            case LanguageController.LanguageStatus.kTraditionalChinese: return "請輸入玩家名稱（2～10個字元）";
            case LanguageController.LanguageStatus.kSpanish: return "Introduce tu nombre de jugador (2-10 caracteres)";
            case LanguageController.LanguageStatus.kPortuguese: return "Digite seu nome de jogador (2-10 caracteres)";
            case LanguageController.LanguageStatus.kRussian: return "Введите имя игрока (2–10 символов)";
            case LanguageController.LanguageStatus.kGerman: return "Gib deinen Spielernamen ein (2–10 Zeichen)";
            case LanguageController.LanguageStatus.kFrench: return "Saisissez votre nom de joueur (2 à 10 caractères)";
            case LanguageController.LanguageStatus.kTurkish: return "Oyuncu adını gir (2-10 karakter)";
            default: return "プレイヤー名を入力してください（2～10文字）";
        }
    }
}
