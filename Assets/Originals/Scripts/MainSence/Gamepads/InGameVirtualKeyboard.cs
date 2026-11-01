using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

/// <summary>
/// ゲーム内に表示するバーチャルキーボード(コントローラー・マウスで文字入力できる)
/// Steamのバーチャルキーボードが使えない環境(Unityエディタ・Steamを経由せずに起動したビルド・
/// Steamオーバーレイ無効時・Big Picture以外でSteamキーボードが開けない時)の代わりに使う
///
/// ・UIは実行時に自動生成するため、シーンへの配置・Prefabは不要(SteamGamepadNameInputが生成する)
/// ・文字の種類:ひらがな / カタカナ / 英数字 / アクセント付き文字 / キリル文字
///
/// 操作
///   A(×)          … キーを押す
///   B(○)          … 1文字消す
///   Y(△)          … 大文字/小文字の切替
///   LB/RB(L1/R1)  … 文字の種類の切替
///   マウスクリック … キーを押す
/// </summary>
[DisallowMultipleComponent]
public class InGameVirtualKeyboard : MonoBehaviour
{
    /*-------------------------------------------------------------
     * キー配列
     -------------------------------------------------------------*/

    /// <summary>
    /// キー配列の1ページ分(ひらがな・英数字等)
    /// </summary>
    private class KeyPage
    {
        /// <summary>タブに表示する名前</summary>
        public readonly string tabLabel;

        /// <summary>大文字(または通常)の行</summary>
        public readonly string[] upperRows;

        /// <summary>小文字の行(大文字/小文字の切替がない場合はnull)</summary>
        public readonly string[] lowerRows;

        public KeyPage(string tabLabel, string[] upperRows, string[] lowerRows)
        {
            this.tabLabel = tabLabel;
            this.upperRows = upperRows;
            this.lowerRows = lowerRows;
        }
    }

    /// <summary>
    /// 濁点キー
    /// </summary>
    private const char kDakutenKey = '゛';

    /// <summary>
    /// 半濁点キー
    /// </summary>
    private const char kHandakutenKey = '゜';

    /// <summary>
    /// 小文字(ぁ・っ等)切替キー
    /// </summary>
    private const char kSmallKanaKey = '小';

    /// <summary>
    /// 1行のキー数
    /// </summary>
    private const int kColumns = 10;

    /// <summary>
    /// 行数
    /// </summary>
    private const int kRows = 5;

    private const int kPageHiragana = 0;
    private const int kPageKatakana = 1;
    private const int kPageAlphabet = 2;
    private const int kPageAccent = 3;
    private const int kPageCyrillic = 4;

    /// <summary>
    /// 全ページのキー配列(1行最大10文字・5行)
    /// </summary>
    private static readonly KeyPage[] pages =
    {
        new KeyPage("あ",
            new[] { "あいうえおかきくけこ", "さしすせそたちつてと", "なにぬねのはひふへほ", "まみむめもらりるれろ", "やゆよわをんー゛゜小" },
            null),
        new KeyPage("ア",
            new[] { "アイウエオカキクケコ", "サシスセソタチツテト", "ナニヌネノハヒフヘホ", "マミムメモラリルレロ", "ヤユヨワヲンー゛゜小" },
            null),
        new KeyPage("ABC",
            new[] { "ABCDEFGHIJ", "KLMNOPQRST", "UVWXYZ-_.'", "!?&#@+*()~", "0123456789" },
            new[] { "abcdefghij", "klmnopqrst", "uvwxyz-_.'", "!?&#@+*()~", "0123456789" }),
        new KeyPage("ÀÉ",
            new[] { "ÁÀÂÃÄÅÇÉÈÊ", "ËÍÌÎÏÑÓÒÔÕ", "ÖØÚÙÛÜÝĞŞİ", "ÆŒ¿¡-_.'", "0123456789" },
            new[] { "áàâãäåçéèê", "ëíìîïñóòôõ", "öøúùûüýğşı", "æœß¿¡-_.'", "0123456789" }),
        new KeyPage("АБВ",
            new[] { "АБВГДЕЁЖЗИ", "ЙКЛМНОПРСТ", "УФХЦЧШЩЪЫЬ", "ЭЮЯ-_.'!?&", "0123456789" },
            new[] { "абвгдеёжзи", "йклмнопрст", "уфхцчшщъыь", "эюя-_.'!?&", "0123456789" }),
    };

    //濁点・半濁点・小文字の変換表(同じ位置の文字同士が対応する)
    private const string kDakuBase = "かきくけこさしすせそたちつてとはひふへほカキクケコサシスセソタチツテトハヒフヘホウ";
    private const string kDakuVoiced = "がぎぐげござじずぜぞだぢづでどばびぶべぼガギグゲゴザジズゼゾダヂヅデドバビブベボヴ";
    private const string kHandakuBase = "はひふへほハヒフヘホ";
    private const string kHandakuVoiced = "ばびぶべぼバビブベボ";
    private const string kHandakuResult = "ぱぴぷぺぽパピプペポ";
    private const string kLargeKana = "あいうえおつやゆよわアイウエオツヤユヨワ";
    private const string kSmallKana = "ぁぃぅぇぉっゃゅょゎァィゥェォッャュョヮ";

    /*-------------------------------------------------------------
     * 表示文言(言語別)
     * 並び:日本語, 英語, 簡体字, 繁体字, スペイン語, ポルトガル語, ロシア語, ドイツ語, フランス語, トルコ語
     -------------------------------------------------------------*/

    private static readonly string[] shiftLabels = { "大/小", "Shift", "大/小写", "大/小寫", "Mayús.", "Maiúsc.", "Регистр", "Umschalt", "Maj", "Büyük/küçük" };
    private static readonly string[] spaceLabels = { "スペース", "Space", "空格", "空格", "Espacio", "Espaço", "Пробел", "Leerzeichen", "Espace", "Boşluk" };
    private static readonly string[] deleteLabels = { "1字消す", "Delete", "删除", "刪除", "Borrar", "Apagar", "Стереть", "Löschen", "Effacer", "Sil" };
    private static readonly string[] okLabels = { "決定", "OK", "确定", "確定", "Aceptar", "OK", "Готово", "OK", "OK", "Tamam" };

    /// <summary>
    /// 操作説明({0}:決定 {1}:戻る {2}:上ボタン {3}:Lボタン {4}:Rボタン)
    /// </summary>
    private static readonly string[] hintFormats =
    {
        "{0}：入力　{1}：1字消す　{2}：大/小　{3}/{4}：文字の種類",
        "{0}: Type   {1}: Delete   {2}: Shift   {3}/{4}: Switch keys",
        "{0}：输入　{1}：删除　{2}：大/小写　{3}/{4}：切换字符",
        "{0}：輸入　{1}：刪除　{2}：大/小寫　{3}/{4}：切換字元",
        "{0}: Escribir   {1}: Borrar   {2}: Mayús.   {3}/{4}: Cambiar teclado",
        "{0}: Digitar   {1}: Apagar   {2}: Maiúsc.   {3}/{4}: Trocar teclado",
        "{0}: Ввод   {1}: Стереть   {2}: Регистр   {3}/{4}: Раскладка",
        "{0}: Eingabe   {1}: Löschen   {2}: Umschalt   {3}/{4}: Zeichensatz",
        "{0} : Saisir   {1} : Effacer   {2} : Maj   {3}/{4} : Changer de clavier",
        "{0}: Yaz   {1}: Sil   {2}: Büyük/küçük   {3}/{4}: Klavye değiştir",
    };

    /*-------------------------------------------------------------
     * 状態
     -------------------------------------------------------------*/

    /// <summary>
    /// キーボードを表示中か
    /// </summary>
    public bool IsOpen { get; private set; }

    /// <summary>
    /// 現在入力されている文字列
    /// </summary>
    public string CurrentText => text;

    private string text = "";
    private int maxLength = 10;
    private Action<string> onSubmit;
    private int pageIndex = kPageHiragana;
    private bool isUpper = true;

    /*-------------------------------------------------------------
     * UI
     -------------------------------------------------------------*/

    private Font font;
    private GameObject content;
    private Text descriptionText;
    private Text previewText;
    private Text countText;
    private Text hintText;
    private Button[] tabButtons;
    private Text[] tabLabels;
    private Button[] keyButtons;
    private Text[] keyLabels;
    private Button shiftButton;
    private Text shiftLabel;
    private Text spaceLabel;
    private Text deleteLabel;
    private Text okLabel;

    private static readonly Color kTextColor = Color.white;
    private static readonly Color kActiveTabTextColor = new Color(1f, 0.8f, 0.25f);

    /// <summary>
    /// キーボードを生成する(非表示の状態で生成される)
    /// </summary>
    /// <param name="useFont">使用するフォント(nullの場合はUnity標準フォント)</param>
    /// <returns>生成したキーボード</returns>
    public static InGameVirtualKeyboard Create(Font useFont)
    {
        GameObject go = new GameObject("InGameVirtualKeyboard");
        InGameVirtualKeyboard keyboard = go.AddComponent<InGameVirtualKeyboard>();
        keyboard.Build(useFont);
        return keyboard;
    }

    /// <summary>
    /// キーボードを表示する
    /// </summary>
    /// <param name="initialText">最初に入っている文字列</param>
    /// <param name="maxTextLength">最大文字数</param>
    /// <param name="description">上部に表示する説明文</param>
    /// <param name="submitCallback">「決定」が押された時に呼ばれる処理</param>
    public void Open(string initialText, int maxTextLength, string description, Action<string> submitCallback)
    {
        maxLength = Mathf.Max(1, maxTextLength);
        text = initialText ?? "";
        if (text.Length > maxLength)
        {
            text = text.Substring(0, maxLength);
        }

        onSubmit = submitCallback;
        descriptionText.text = description ?? "";

        pageIndex = GetDefaultPage();
        isUpper = true;

        ApplyLanguageLabels();

        content.SetActive(true);
        IsOpen = true;

        RefreshKeys();
        RefreshPreview();
        RefreshHint();
        SelectDefault();
    }

    /// <summary>
    /// キーボードを閉じる
    /// </summary>
    public void Close()
    {
        IsOpen = false;
        onSubmit = null;

        //キーボード内のボタンが選択されたままにならないよう選択を解除
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && eventSystem.currentSelectedGameObject != null
            && eventSystem.currentSelectedGameObject.transform.IsChildOf(transform))
        {
            eventSystem.SetSelectedGameObject(null);
        }

        if (content != null)
        {
            content.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        //コントローラーのボタン操作(Aでのキー入力はEventSystemのSubmitで処理される)
        Gamepad pad = Gamepad.current;
        if (pad != null)
        {
            if (GamepadInput.IsCancelPressed()) Backspace();
            if (pad.buttonNorth.wasPressedThisFrame) ToggleShift();
            if (pad.leftShoulder.wasPressedThisFrame) ChangePage(-1);
            if (pad.rightShoulder.wasPressedThisFrame) ChangePage(1);
        }

        //物理キーボードのBackspaceでも1文字消せるようにする
        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            Backspace();
        }

        //選択が外れている状態でコントローラーを操作した場合、キーを選択し直す
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
        {
            GameObject current = eventSystem.currentSelectedGameObject;
            bool isLost = current == null || !current.activeInHierarchy || !current.transform.IsChildOf(transform);
            if (isLost && GamepadInput.HasNavigationInput())
            {
                SelectDefault();
            }
        }
    }

    private void OnDestroy()
    {
        onSubmit = null;
        tabButtons = null;
        tabLabels = null;
        keyButtons = null;
        keyLabels = null;
    }

    /*-------------------------------------------------------------
     * 入力処理
     -------------------------------------------------------------*/

    /// <summary>
    /// 文字キーが押された
    /// </summary>
    /// <param name="index">キー番号(0～49)</param>
    private void OnKeyPressed(int index)
    {
        string row = GetRow(index / kColumns);
        int column = index % kColumns;
        if (row == null || column >= row.Length)
        {
            return;
        }

        char c = row[column];
        switch (c)
        {
            case kDakutenKey: ApplyDakuten(); break;
            case kHandakutenKey: ApplyHandakuten(); break;
            case kSmallKanaKey: ToggleSmallKana(); break;
            default: AddChar(c); break;
        }

        RefreshPreview();
    }

    /// <summary>
    /// 1文字追加
    /// </summary>
    private void AddChar(char c)
    {
        if (text.Length >= maxLength)
        {
            return;
        }
        text += c;
    }

    /// <summary>
    /// 1文字消す
    /// </summary>
    private void Backspace()
    {
        if (text.Length > 0)
        {
            text = text.Substring(0, text.Length - 1);
            RefreshPreview();
        }
    }

    /// <summary>
    /// 「決定」が押された
    /// </summary>
    private void Submit()
    {
        Action<string> callback = onSubmit;
        callback?.Invoke(text);
    }

    /// <summary>
    /// 最後の文字が from に含まれていれば、同じ位置の to の文字に置き換える
    /// </summary>
    /// <returns>置き換えたか</returns>
    private bool ReplaceLastChar(string from, string to)
    {
        if (text.Length == 0)
        {
            return false;
        }

        int index = from.IndexOf(text[text.Length - 1]);
        if (index < 0)
        {
            return false;
        }

        text = text.Substring(0, text.Length - 1) + to[index];
        return true;
    }

    /// <summary>
    /// 濁点(か→が / が→か / ぱ→ば)
    /// </summary>
    private void ApplyDakuten()
    {
        if (ReplaceLastChar(kDakuVoiced, kDakuBase)) return;
        if (ReplaceLastChar(kHandakuResult, kHandakuVoiced)) return;
        ReplaceLastChar(kDakuBase, kDakuVoiced);
    }

    /// <summary>
    /// 半濁点(は→ぱ / ぱ→は / ば→ぱ)
    /// </summary>
    private void ApplyHandakuten()
    {
        if (ReplaceLastChar(kHandakuResult, kHandakuBase)) return;
        if (ReplaceLastChar(kHandakuVoiced, kHandakuResult)) return;
        ReplaceLastChar(kHandakuBase, kHandakuResult);
    }

    /// <summary>
    /// 小文字(あ⇔ぁ / つ⇔っ 等)
    /// </summary>
    private void ToggleSmallKana()
    {
        if (ReplaceLastChar(kLargeKana, kSmallKana)) return;
        ReplaceLastChar(kSmallKana, kLargeKana);
    }

    /// <summary>
    /// 大文字/小文字の切替
    /// </summary>
    private void ToggleShift()
    {
        if (pages[pageIndex].lowerRows == null)
        {
            return;
        }
        isUpper = !isUpper;
        RefreshKeys();
    }

    /// <summary>
    /// 文字の種類を前後に切り替える
    /// </summary>
    private void ChangePage(int delta)
    {
        ChangePageTo((pageIndex + delta + pages.Length) % pages.Length);
    }

    /// <summary>
    /// 文字の種類を指定のページに切り替える
    /// </summary>
    private void ChangePageTo(int index)
    {
        pageIndex = index;
        isUpper = true;
        RefreshKeys();
    }

    /*-------------------------------------------------------------
     * 表示更新
     -------------------------------------------------------------*/

    /// <summary>
    /// 現在のページ・大文字/小文字に応じた行を取得
    /// </summary>
    private string GetRow(int row)
    {
        KeyPage page = pages[pageIndex];
        string[] rows = (!isUpper && page.lowerRows != null) ? page.lowerRows : page.upperRows;
        return row < rows.Length ? rows[row] : null;
    }

    /// <summary>
    /// キーの文字・表示を更新
    /// </summary>
    private void RefreshKeys()
    {
        for (int i = 0; i < keyButtons.Length; i++)
        {
            string row = GetRow(i / kColumns);
            int column = i % kColumns;
            bool hasKey = row != null && column < row.Length;

            keyButtons[i].gameObject.SetActive(hasKey);
            if (hasKey)
            {
                keyLabels[i].text = row[column].ToString();
            }
        }

        for (int i = 0; i < tabLabels.Length; i++)
        {
            tabLabels[i].color = i == pageIndex ? kActiveTabTextColor : kTextColor;
        }

        shiftButton.interactable = pages[pageIndex].lowerRows != null;

        //選択中のキーが非表示になった場合は、最初のキーを選択し直す
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null && IsOpen)
        {
            GameObject current = eventSystem.currentSelectedGameObject;
            if (current == null || !current.activeInHierarchy)
            {
                SelectDefault();
            }
        }
    }

    /// <summary>
    /// 入力中の文字列の表示を更新
    /// </summary>
    private void RefreshPreview()
    {
        previewText.text = text + (text.Length < maxLength ? "_" : "");
        countText.text = text.Length + "/" + maxLength;
    }

    /// <summary>
    /// 操作説明を現在のコントローラーに合わせて更新
    /// </summary>
    private void RefreshHint()
    {
        string confirm, back, north, left, right;

        if (Gamepad.current is DualShockGamepad)
        {
            confirm = "×"; back = "○"; north = "△"; left = "L1"; right = "R1";
        }
        else if (GamepadInput.IsSwitchProController)
        {
            confirm = GamepadInput.IsNintendoLayout ? "A" : "B";
            back = GamepadInput.IsNintendoLayout ? "B" : "A";
            north = "X"; left = "L"; right = "R";
        }
        else
        {
            confirm = "A"; back = "B"; north = "Y"; left = "LB"; right = "RB";
        }

        hintText.text = string.Format(hintFormats[GetLanguageIndex()], confirm, back, north, left, right);
    }

    /// <summary>
    /// ファンクションキーの文言を現在の言語に合わせる
    /// </summary>
    private void ApplyLanguageLabels()
    {
        int lang = GetLanguageIndex();
        shiftLabel.text = shiftLabels[lang];
        spaceLabel.text = spaceLabels[lang];
        deleteLabel.text = deleteLabels[lang];
        okLabel.text = okLabels[lang];
    }

    /// <summary>
    /// 最初のキーを選択
    /// </summary>
    private void SelectDefault()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("[InGameVirtualKeyboard] EventSystemがシーンに存在しないため、コントローラーで操作できません");
            return;
        }

        foreach (Button button in keyButtons)
        {
            if (button.gameObject.activeInHierarchy)
            {
                eventSystem.SetSelectedGameObject(button.gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// 言語の番号(表示文言の配列の番号)を取得
    /// </summary>
    private static int GetLanguageIndex()
    {
        if (LanguageController.instance == null)
        {
            return 0;
        }

        switch (LanguageController.instance.GetLanguageStatus())
        {
            case LanguageController.LanguageStatus.kJapanese: return 0;
            case LanguageController.LanguageStatus.kEnglish: return 1;
            case LanguageController.LanguageStatus.kSimplifiedChinese: return 2;
            case LanguageController.LanguageStatus.kTraditionalChinese: return 3;
            case LanguageController.LanguageStatus.kSpanish: return 4;
            case LanguageController.LanguageStatus.kPortuguese: return 5;
            case LanguageController.LanguageStatus.kRussian: return 6;
            case LanguageController.LanguageStatus.kGerman: return 7;
            case LanguageController.LanguageStatus.kFrench: return 8;
            case LanguageController.LanguageStatus.kTurkish: return 9;
            default: return 1;
        }
    }

    /// <summary>
    /// 言語に応じて最初に表示するページ
    /// </summary>
    private static int GetDefaultPage()
    {
        switch (GetLanguageIndex())
        {
            case 0: return kPageHiragana;
            case 6: return kPageCyrillic;
            default: return kPageAlphabet;
        }
    }

    /*-------------------------------------------------------------
     * UI生成
     -------------------------------------------------------------*/

    /// <summary>
    /// UIを生成する(基準解像度1920x1080)
    /// </summary>
    private void Build(Font useFont)
    {
        font = useFont != null ? useFont : LoadBuiltinFont();

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        //全体(表示・非表示を切り替える親)
        RectTransform contentRect = CreateRect("Content", transform);
        Stretch(contentRect);
        content = contentRect.gameObject;

        //背景を暗くする(後ろのUIをクリックできないようにする役割もある)
        Image dim = CreateImage("Dim", contentRect, new Color(0f, 0f, 0f, 0.6f));
        Stretch(dim.rectTransform);

        //パネル
        Image panel = CreateImage("Panel", contentRect, new Color(0.08f, 0.09f, 0.12f, 0.96f));
        panel.rectTransform.sizeDelta = new Vector2(1240, 900);
        RectTransform panelRect = panel.rectTransform;

        //説明文
        descriptionText = CreateText("Description", panelRect, new Vector2(0, 395), new Vector2(1180, 50), 30);

        //入力中の文字列
        Image previewBg = CreateImage("PreviewBg", panelRect, new Color(0f, 0f, 0f, 0.6f));
        previewBg.rectTransform.anchoredPosition = new Vector2(0, 320);
        previewBg.rectTransform.sizeDelta = new Vector2(760, 70);
        previewText = CreateText("PreviewText", previewBg.rectTransform, Vector2.zero, new Vector2(620, 70), 40);
        previewText.supportRichText = false;
        countText = CreateText("CountText", previewBg.rectTransform, new Vector2(320, 0), new Vector2(110, 70), 24);
        countText.alignment = TextAnchor.MiddleRight;
        countText.color = new Color(0.7f, 0.7f, 0.7f);

        //文字の種類のタブ
        tabButtons = new Button[pages.Length];
        tabLabels = new Text[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            int pageNumber = i;
            float x = (i - (pages.Length - 1) * 0.5f) * 160f;
            tabButtons[i] = CreateButton("Tab_" + i, panelRect, new Vector2(x, 235), new Vector2(150, 56), 28, out tabLabels[i]);
            tabLabels[i].text = pages[i].tabLabel;
            tabButtons[i].onClick.AddListener(() => ChangePageTo(pageNumber));
        }

        //文字キー(10列×5行)
        keyButtons = new Button[kColumns * kRows];
        keyLabels = new Text[kColumns * kRows];
        for (int i = 0; i < keyButtons.Length; i++)
        {
            int keyNumber = i;
            int row = i / kColumns;
            int column = i % kColumns;
            Vector2 pos = new Vector2((column - (kColumns - 1) * 0.5f) * 112f, 150f - row * 92f);
            keyButtons[i] = CreateButton("Key_" + i, panelRect, pos, new Vector2(100, 80), 40, out keyLabels[i]);
            keyButtons[i].onClick.AddListener(() => OnKeyPressed(keyNumber));
        }

        //ファンクションキー(大/小・スペース・1字消す・決定)
        float[] widths = { 220f, 360f, 240f, 240f };
        float gap = 12f;
        float total = gap * (widths.Length - 1);
        foreach (float w in widths) total += w;
        float left = -total * 0.5f;
        Button[] functionButtons = new Button[widths.Length];
        Text[] functionLabels = new Text[widths.Length];
        for (int i = 0; i < widths.Length; i++)
        {
            Vector2 pos = new Vector2(left + widths[i] * 0.5f, -310f);
            functionButtons[i] = CreateButton("Function_" + i, panelRect, pos, new Vector2(widths[i], 70), 30, out functionLabels[i]);
            functionLabels[i].resizeTextForBestFit = true;
            functionLabels[i].resizeTextMinSize = 14;
            functionLabels[i].resizeTextMaxSize = 30;
            functionLabels[i].horizontalOverflow = HorizontalWrapMode.Wrap;
            functionLabels[i].rectTransform.sizeDelta = new Vector2(widths[i] - 16f, 64f);
            left += widths[i] + gap;
        }

        shiftButton = functionButtons[0];
        shiftLabel = functionLabels[0];
        spaceLabel = functionLabels[1];
        deleteLabel = functionLabels[2];
        okLabel = functionLabels[3];

        functionButtons[0].onClick.AddListener(ToggleShift);
        functionButtons[1].onClick.AddListener(() => { AddChar(' '); RefreshPreview(); });
        functionButtons[2].onClick.AddListener(Backspace);
        functionButtons[3].onClick.AddListener(Submit);

        //決定ボタンは目立つ色にする
        ColorBlock okColors = functionButtons[3].colors;
        okColors.normalColor = new Color(0.15f, 0.45f, 0.25f);
        functionButtons[3].colors = okColors;

        //操作説明
        hintText = CreateText("Hint", panelRect, new Vector2(0, -392), new Vector2(1180, 44), 24);
        hintText.color = new Color(0.75f, 0.75f, 0.75f);

        content.SetActive(false);
    }

    /// <summary>
    /// Unity標準フォントを読み込む(Unityのバージョンによってファイル名が異なる)
    /// </summary>
    private static Font LoadBuiltinFont()
    {
        Font builtin = null;
        try { builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch (Exception) { }
        if (builtin == null)
        {
            try { builtin = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch (Exception) { }
        }
        return builtin;
    }

    private static RectTransform CreateRect(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(objectName, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string objectName, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        RectTransform rect = CreateRect(objectName, parent);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text label = rect.gameObject.AddComponent<Text>();
        label.font = font;
        label.fontSize = fontSize;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = kTextColor;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        return label;
    }

    private Button CreateButton(string objectName, Transform parent, Vector2 position, Vector2 size, int fontSize, out Text label)
    {
        Image image = CreateImage(objectName, parent, Color.white);
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;

        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.22f, 0.24f, 0.30f);
        colors.highlightedColor = new Color(0.36f, 0.40f, 0.50f);
        colors.selectedColor = new Color(0.90f, 0.55f, 0.10f);
        colors.pressedColor = new Color(1.00f, 0.80f, 0.40f);
        colors.disabledColor = new Color(0.15f, 0.15f, 0.15f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.05f;
        button.colors = colors;

        //十字キー・スティックで上下左右のキーへ移動できるようにする
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.Automatic;
        button.navigation = navigation;

        label = CreateText("Label", image.rectTransform, Vector2.zero, size, fontSize);
        return button;
    }
}
