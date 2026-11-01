//SteamManager.csと同じ条件でSteamworksを無効にする(Steam非対応のプラットフォームでもコンパイルできるようにするため)
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

/// <summary>
/// コントローラー操作時に、バーチャルキーボードでプレイヤー名を入力させるクラス
/// プレイヤー名入力用のInputField(MessageController.inputPlayerNameField)と同じオブジェクトにアタッチする
/// (MessageControllerのAwakeで自動的に追加されるため、手動でのアタッチは不要)
///
/// InputFieldが表示された時(isInputPlayerNameFieldStatus == 1)に、環境に応じて以下を開く
///   ・Steamから起動 + Big Picture / Steam Deck  … Steam公式のバーチャルキーボード
///   ・Unityエディタ / Steamを経由しないビルド起動 / Steamキーボードを開けない場合 … ゲーム内バーチャルキーボード
///   ・コントローラー未接続(キーボード・マウス操作) … 通常のInputField入力
///
/// ・決定 → MessageController.SavePlayerName()を呼び、既存の確認パネルの流れへ進む
/// ・Steamキーボードでキャンセル → A(決定ボタン)で再度開く
/// ・バーチャルキーボード表示中に物理キーボードで文字を打つと、通常のInputField入力に切り替わる
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(InputField))]
public class SteamGamepadNameInput : MonoBehaviour
{
    [Header("名前の最大文字数(MessageControllerの判定(10文字以下)と合わせること)")]
    [SerializeField] private uint maxNameLength = 10;

    [Header("Steamキーボードをキャンセルした後に表示する案内(任意。例:「A:キーボードを開く」)")]
    [SerializeField] private GameObject reopenGuide;

    [Header("コントローラー未接続でもバーチャルキーボードを表示する(マウスでの動作確認用)")]
    [SerializeField] private bool showKeyboardWithoutGamepad = false;

    [Header("ゲーム内キーボードのフォント(未設定の場合はInputFieldのフォントを使う)")]
    [SerializeField] private Font keyboardFont;

    /// <summary>
    /// プレイヤー名入力用のInputField
    /// </summary>
    private InputField inputField;

    /// <summary>
    /// ゲーム内バーチャルキーボード(初めて使う時に生成する)
    /// </summary>
    private InGameVirtualKeyboard inGameKeyboard;

    /// <summary>
    /// Steamキーボードを表示中か
    /// </summary>
    private bool isSteamKeyboardOpen = false;

    /// <summary>
    /// Steamキーボードで入力するモードか(キャンセル後、再度開くのを待っている状態を含む)
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
        isSteamKeyboardOpen = false;
        isSteamKeyboardMode = false;
        hasDismissedResult = false;
        SetReopenGuide(false);

        //バーチャルキーボードを使う場合は、InputFieldが先に選択・入力状態になるのを防ぐため、一旦操作不可にする
        //(InputFieldが入力状態のままキーボードを開くと、フォーカスが外れた際にOn End Editが呼ばれてしまうため)
        if (ShouldUseVirtualKeyboard())
        {
            inputField.interactable = false;
        }

        //シーン読み込み直後にMessageControllerのAwakeで非表示にされる場合があるため、1フレーム待ってから開く
        //(非表示にされた場合はコルーチンも止まる)
        StartCoroutine(OpenKeyboardNextFrame());
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
        isSteamKeyboardOpen = false;
        isSteamKeyboardMode = false;

        //ゲーム内キーボードを閉じる
        CloseInGameKeyboard();

        //次に表示した時のためにInputFieldを操作可能に戻す
        if (inputField != null)
        {
            inputField.interactable = true;
        }

        SetReopenGuide(false);
    }

    private void OnDestroy()
    {
        //ゲーム内キーボードを破棄
        if (inGameKeyboard != null)
        {
            Destroy(inGameKeyboard.gameObject);
        }

        //参照をnullに設定(メモリリークを防ぐため)
        inGameKeyboard = null;
        inputField = null;
        reopenGuide = null;
        keyboardFont = null;
    }

    /// <summary>
    /// 1フレーム待ってからキーボードを開く
    /// </summary>
    private IEnumerator OpenKeyboardNextFrame()
    {
        yield return null;

        if (ShouldUseVirtualKeyboard())
        {
            OpenVirtualKeyboard();
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
            HandleSteamDismissedResult();
            return;
        }

        //ゲーム内キーボード表示中
        if (inGameKeyboard != null && inGameKeyboard.IsOpen)
        {
            //物理キーボードで文字を打った場合、通常のInputField入力に切り替える(入力途中の文字は引き継ぐ)
            if (IsTypingKeyPressed())
            {
                string currentText = inGameKeyboard.CurrentText;
                CloseInGameKeyboard();
                inputField.text = currentText;
                SwitchToInputField();
            }
            return;
        }

        //Steamキーボード入力モードでない場合||Steamキーボード表示中の場合
        if (!isSteamKeyboardMode || isSteamKeyboardOpen)
        {
            //処理をスキップ
            return;
        }

        //Steamキーボードのキャンセル後にA(決定ボタン)を押した場合、キーボードを開き直す
        if (GamepadInput.IsInteractPressed())
        {
            OpenVirtualKeyboard();
            return;
        }

        //Steamキーボードのキャンセル後にキーボードを打った場合、通常のInputField入力に切り替える
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SwitchToInputField();
        }
    }

    /// <summary>
    /// バーチャルキーボードを使うか
    /// </summary>
    private bool ShouldUseVirtualKeyboard()
    {
        return GamepadInput.IsConnected || showKeyboardWithoutGamepad || IsRunningOnSteamDeck();
    }

    /// <summary>
    /// バーチャルキーボードを開く(Steamキーボードを優先し、開けない場合はゲーム内キーボード)
    /// </summary>
    private void OpenVirtualKeyboard()
    {
        if (TryOpenSteamKeyboard())
        {
            return;
        }

        OpenInGameKeyboard();
    }

    /*-------------------------------------------------------------
     * Steamキーボード
     -------------------------------------------------------------*/

    /// <summary>
    /// Steamキーボードを使える環境か
    /// ・エディタ実行時はSteamオーバーレイが表示されないため使わない
    /// ・Steamを経由せずにexeを直接起動した場合もオーバーレイが無効なので使わない
    /// </summary>
    private static bool CanUseSteamKeyboard()
    {
#if DISABLESTEAMWORKS || UNITY_EDITOR
        return false;
#else
        try
        {
            if (!SteamManager.Initialized)
            {
                return false;
            }
            return SteamUtils.IsSteamRunningOnSteamDeck() || SteamUtils.IsOverlayEnabled();
        }
        catch (Exception)
        {
            return false;
        }
#endif
    }

    /// <summary>
    /// Steam Deckで動いているか
    /// </summary>
    private static bool IsRunningOnSteamDeck()
    {
#if DISABLESTEAMWORKS || UNITY_EDITOR
        return false;
#else
        try
        {
            return SteamManager.Initialized && SteamUtils.IsSteamRunningOnSteamDeck();
        }
        catch (Exception)
        {
            return false;
        }
#endif
    }

    /// <summary>
    /// Steamキーボードを開く
    /// </summary>
    /// <returns>開けたか(falseの場合はゲーム内キーボードを使う)</returns>
    private bool TryOpenSteamKeyboard()
    {
        if (!CanUseSteamKeyboard())
        {
            return false;
        }

#if !DISABLESTEAMWORKS
        try
        {
            //閉じられた時のコールバックを登録
            if (dismissedCallback == null)
            {
                dismissedCallback = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);
            }

            //Steamキーボードを表示(Big Picture・Steam Deck以外ではfalseが返る場合がある)
            bool isOpened = SteamUtils.ShowGamepadTextInput(
                EGamepadTextInputMode.k_EGamepadTextInputModeNormal,
                EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine,
                GetDescriptionText(),
                maxNameLength,
                inputField.text ?? "");

            if (!isOpened)
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("[SteamGamepadNameInput] Steamキーボードを開けませんでした。ゲーム内キーボードを使います: " + e.Message);
            return false;
        }

        isSteamKeyboardMode = true;
        isSteamKeyboardOpen = true;
        inputField.interactable = false;
        SetReopenGuide(false);
        return true;
#else
        return false;
#endif
    }

#if !DISABLESTEAMWORKS
    /// <summary>
    /// Steamキーボードが閉じられた時に呼ばれる(SteamManagerのSteamAPI.RunCallbacks内から呼ばれる)
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
    private void HandleSteamDismissedResult()
    {
        isSteamKeyboardOpen = false;

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
        SubmitPlayerName(playerName);
    }

    /*-------------------------------------------------------------
     * ゲーム内キーボード
     -------------------------------------------------------------*/

    /// <summary>
    /// ゲーム内キーボードを開く
    /// </summary>
    private void OpenInGameKeyboard()
    {
        if (inGameKeyboard == null)
        {
            Font font = keyboardFont;
            if (font == null && inputField.textComponent != null)
            {
                font = inputField.textComponent.font;
            }
            inGameKeyboard = InGameVirtualKeyboard.Create(font);
        }

        isSteamKeyboardMode = false;
        isSteamKeyboardOpen = false;
        SetReopenGuide(false);
        inputField.interactable = false;

        inGameKeyboard.Open(inputField.text ?? "", (int)maxNameLength, GetDescriptionText(), OnInGameKeyboardSubmitted);
    }

    /// <summary>
    /// ゲーム内キーボードを閉じる
    /// </summary>
    private void CloseInGameKeyboard()
    {
        if (inGameKeyboard != null && inGameKeyboard.IsOpen)
        {
            inGameKeyboard.Close();
        }
    }

    /// <summary>
    /// ゲーム内キーボードで「決定」が押された
    /// </summary>
    /// <param name="playerName">入力された名前</param>
    private void OnInGameKeyboardSubmitted(string playerName)
    {
        CloseInGameKeyboard();
        SubmitPlayerName(playerName);
    }

    /*-------------------------------------------------------------
     * 共通
     -------------------------------------------------------------*/

    /// <summary>
    /// 入力された名前を既存の名前保存処理へ渡す
    /// </summary>
    /// <param name="playerName">入力された名前</param>
    private void SubmitPlayerName(string playerName)
    {
        playerName = (playerName ?? "").Trim();

        //InputFieldにも反映(確認パネルで「いいえ」を押して戻った時に表示されるようにするため)
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
        isSteamKeyboardOpen = false;
        SetReopenGuide(false);

        inputField.interactable = true;
        inputField.Select();
        inputField.ActivateInputField();

        //ActivateInputFieldは全選択状態になるため、次のフレームでカーソルを末尾へ移動する
        StartCoroutine(MoveCaretToEndNextFrame());
    }

    /// <summary>
    /// 1フレーム待ってからInputFieldのカーソルを末尾へ移動する
    /// </summary>
    private IEnumerator MoveCaretToEndNextFrame()
    {
        yield return null;

        if (inputField != null && inputField.isFocused)
        {
            inputField.MoveTextEnd(false);
        }
    }

    /// <summary>
    /// 物理キーボードで文字入力用のキーが押されたか
    /// (矢印・Enter・Space・Backspace等はゲーム内キーボードの操作に使うため除外する)
    /// </summary>
    private static bool IsTypingKeyPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.anyKey.wasPressedThisFrame)
        {
            return false;
        }

        foreach (UnityEngine.InputSystem.Controls.KeyControl key in keyboard.allKeys)
        {
            if (key == null || !key.wasPressedThisFrame)
            {
                continue;
            }

            switch (key.keyCode)
            {
                case Key.UpArrow:
                case Key.DownArrow:
                case Key.LeftArrow:
                case Key.RightArrow:
                case Key.Enter:
                case Key.NumpadEnter:
                case Key.Space:
                case Key.Backspace:
                case Key.Escape:
                case Key.Tab:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                    continue;
            }
            return true;
        }
        return false;
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
    /// キーボードに表示する説明文を現在の言語で取得する
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
