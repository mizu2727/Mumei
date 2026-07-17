using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using static GameController;

public class LanguageController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static LanguageController instance;

    [Header("ボタンテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ButtonMessage buttonMessage;

    [Header("UIテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private UITextMessage uITextMessage;

    [Header("言語ステータス(ヒエラルキー上からの編集禁止)")]
    public LanguageStatus languageStatus;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE用audioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ボタンSEのID
    /// </summary>
    private readonly int buttonSEid = 4;

    /// <summary>
    /// 言語ステータス
    /// </summary>
    public enum LanguageStatus 
    {
        /// <summary>
        /// 日本語
        /// </summary>
        kJapanese,

        /// <summary>
        /// 英語
        /// </summary>
        kEnglish,
    }

    

    [Header("ボタン配下内のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] buttonTextArray;

    /// <summary>
    /// buttonTextArrayと同じGameObjectから取得したTMP_Text版(実行時に自動生成)
    /// </summary>
    private TMP_Text[] buttonTMPTextArray;

    /// <summary>
    /// ボタン配下内のテキスト配列を取得する
    /// </summary>
    /// <returns>ボタン配下内のテキスト配列</returns>
    public TMP_Text[] GetButtonTMPTextArray()
    {
        return buttonTMPTextArray;
    }


    [Header("ボタンメッセージ番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] buttonTextNumberArray;

    /// <summary>
    /// ボタンメッセージ番号配列を取得する
    /// </summary>
    /// <returns>ボタンメッセージ番号配列</returns>
    public int[] GetButtonTextNumberArray()
    {
        return buttonTextNumberArray;
    }



    [Header("通常のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] uITextArray;

    /// <summary>
    /// uITextArrayと同じGameObjectから取得したTMP_Text版(実行時に自動生成)
    /// </summary>
    private TMP_Text[] uITMPTextArray;

    [Header("通常テキスト番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] uITextNumberArray;

    [Header("アイテムを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Item[] itemArray;

    /// <summary>
    /// Item.cs
    /// </summary>
    private Item item;


    /// <summary>
    /// 言語ステータスを取得する
    /// </summary>
    /// <returns>言語ステータス</returns>
    public LanguageStatus GetLanguageStatus() 
    {
        return languageStatus;
    }

    /// <summary>
    /// 言語ステータスを設定する
    /// </summary>
    /// <param name="languageStatus">言語ステータス</param>
    public void SetLanguageStatus(LanguageStatus languageStatus) 
    {
        this.languageStatus = languageStatus;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //AudioSourceを取得
        audioSourceSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceSE.playOnAwake = false;
    }

    /// <summary>
    /// buttonTextArray / uITextArrayの各GameObjectについて、
    /// レガシーのTextコンポーネントを削除し、TextMeshProUGUIを新規アタッチする。
    /// 変換後のTMP_TextはbuttonTMPTextArray / uITMPTextArrayに格納する。
    /// </summary>
    private void InitializeTMPTextArrays()
    {
        buttonTMPTextArray = new TMP_Text[buttonTextArray.Length];
        for (int i = 0; i < buttonTextArray.Length; i++)
        {
            if (buttonTextArray[i] == null)
            {
                Debug.LogError($"buttonTextArray[{i}] がnullです！ Inspectorでアタッチし忘れています。");
                continue;
            }
            buttonTMPTextArray[i] = ReplaceTextWithTMP(buttonTextArray[i]);
        }

        uITMPTextArray = new TMP_Text[uITextArray.Length];
        for (int i = 0; i < uITextArray.Length; i++)
        {
            if (uITextArray[i] == null)
            {
                Debug.LogError($"uITextArray[{i}] がnullです！ Inspectorでアタッチし忘れています。");
                continue;
            }
            uITMPTextArray[i] = ReplaceTextWithTMP(uITextArray[i]);
        }
    }

    /// <summary>
    /// レガシーのTextコンポーネントを削除し、同じGameObjectにTextMeshProUGUIを追加する。
    /// 削除前にtext・fontSize・color・alignmentなどの設定値を退避し、TMP側へ引き継ぐ。
    /// </summary>
    /// <param name="legacyText">変換対象のTextコンポーネント</param>
    /// <returns>アタッチされたTMP_Text(TextMeshProUGUI)</returns>
    private TMP_Text ReplaceTextWithTMP(Text legacyText)
    {
        if (legacyText == null)
        {
            Debug.LogError("ReplaceTextWithTMP に null が渡されました");
            return null;
        }

        GameObject targetObject = legacyText.gameObject;

        //Textの設定値を退避する
        string text = legacyText.text;
        int fontSize = legacyText.fontSize;
        Color color = legacyText.color;
        FontStyle fontStyle = legacyText.fontStyle;
        TextAnchor alignment = legacyText.alignment;
        bool raycastTarget = legacyText.raycastTarget;

        // Textコンポーネントを即座に削除する
        DestroyImmediate(legacyText);

        //TextMeshProUGUIコンポーネントを追加する
        TextMeshProUGUI tmpText = targetObject.AddComponent<TextMeshProUGUI>();

        //退避した設定値をTMP側へ反映する
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.color = color;
        tmpText.fontStyle = ConvertFontStyle(fontStyle);
        tmpText.alignment = ConvertAlignment(alignment);
        tmpText.raycastTarget = raycastTarget;

        return tmpText;
    }

    /// <summary>
    /// レガシーのTextAnchorをTMPのTextAlignmentOptionsへ変換する
    /// </summary>
    /// <param name="anchor">レガシーの整列設定</param>
    /// <returns>TMP用の整列設定</returns>
    private TextAlignmentOptions ConvertAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft: 

                return TextAlignmentOptions.TopLeft;

            case TextAnchor.UpperCenter: 
                
                return TextAlignmentOptions.Top;

            case TextAnchor.UpperRight: 
                
                return TextAlignmentOptions.TopRight;

            case TextAnchor.MiddleLeft: 
                
                return TextAlignmentOptions.Left;

            case TextAnchor.MiddleCenter: 
                
                return TextAlignmentOptions.Center;

            case TextAnchor.MiddleRight: 
                
                return TextAlignmentOptions.Right;

            case TextAnchor.LowerLeft: 
                
                return TextAlignmentOptions.BottomLeft;

            case TextAnchor.LowerCenter: 
                
                return TextAlignmentOptions.Bottom;

            case TextAnchor.LowerRight: 
                
                return TextAlignmentOptions.BottomRight;

            default: 
                
                return TextAlignmentOptions.Center;
        }
    }

    /// <summary>
    /// レガシーのFontStyleをTMPのFontStylesへ変換する
    /// </summary>
    /// <param name="style">レガシーのフォントスタイル</param>
    /// <returns>TMP用のフォントスタイル</returns>
    private FontStyles ConvertFontStyle(FontStyle style)
    {
        switch (style)
        {
            case FontStyle.Bold: return FontStyles.Bold;
            case FontStyle.Italic: return FontStyles.Italic;
            case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
            default: return FontStyles.Normal;
        }
    }

    private void Awake()
    {
        //シングルトンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

    private void Start()
    {
        InitializeAudioSource();

        //Text型の配列から、同じGameObjectに付いているTMP_Textを取得して配列化する
        InitializeTMPTextArrays();

        //言語を設定する
        SettingLanguageText();
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    private void SettingLanguageText() 
    {
        //保存用変数に言語設定を保存する
        saveLanguageStatus = languageStatus;

        //言語ステータスに応じて、テキストを変更する
        switch (languageStatus) 
        {
            //日本語
            case LanguageStatus.kJapanese:

                //ボタンテキストを日本語に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].text = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageJapanese;
                }

                //ボタンサイズを日本語用に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].fontSize = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageSizeJapanese;
                }

                //ボタンフォントを日本語用に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].font = CommonController.instance.GetJapaneseFont();
                }

                //UIテキストを日本語に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].text = uITextMessage.uITextMessage[uITextNumberArray[i]].messageJapanese;
                }

                //UIサイズを日本語用に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].fontSize = uITextMessage.uITextMessage[uITextNumberArray[i]].messageSizeJapanese;
                }

                //UIフォントを日本語用に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].font = CommonController.instance.GetJapaneseFont();
                }

                break;

            //英語
            case LanguageStatus.kEnglish:

                //ボタンテキストを英語に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].text = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageEnglish;
                }

                //ボタンサイズを英語用に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].fontSize = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageSizeEnglish;
                }

                //ボタンフォントを英語用に変更する
                for (int i = 0; i < buttonTMPTextArray.Length; i++)
                {
                    buttonTMPTextArray[i].font = CommonController.instance.GetEnglishFont();
                }

                //UIテキストを英語に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].text = uITextMessage.uITextMessage[uITextNumberArray[i]].messageEnglish;
                }

                //UIサイズを英語用に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].fontSize = uITextMessage.uITextMessage[uITextNumberArray[i]].messageSizeEnglish;
                }

                //UIフォントを英語用に変更する
                for (int i = 0; i < uITMPTextArray.Length; i++)
                {
                    uITMPTextArray[i].font = CommonController.instance.GetEnglishFont();
                }

                break;
        }

        //アイテムのテキスト関連を設定する
        for (int i = 0; i < itemArray.Length; i++)
        {
            itemArray[i].SettingLanguageText();
        }

        //MapAreaGenerateが存在する場合
        if (MapAreaGenerate.instance != null)
        {
            //MapAreaGenerateのテキスト関連を設定する
            MapAreaGenerate.instance.ChangeLanguageText();
        }

        //ポーズコントローラーが存在する場合
        if (PauseController.instance != null)
        {
            //ポーズコントローラーのテキスト関連を設定する
            PauseController.instance.SettingLanguageText();
        }

        //Compassが存在する場合
        if (Compass.instance != null)
        {
            //Compassのテキスト関連を設定する
            Compass.instance.SettingLanguageText();
        }

        //Inventoryが存在する場合
        if (Inventory.instance != null)
        {
            //Inventoryのテキスト関連を設定する
            Inventory.instance.SettingLanguageText();
        }
    }

    /// <summary>
    /// 日本語ボタンがクリックされたときの処理
    /// </summary>
    public void OnClickedJapaseneButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //言語ステータスを日本語に設定する
        languageStatus = LanguageStatus.kJapanese;

        //言語を設定する
        SettingLanguageText();
    }

    /// <summary>
    /// 英語ボタンがクリックされたときの処理
    /// </summary>
    public void OnClickedEnglishButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //言語ステータスを英語に設定する
        languageStatus = LanguageStatus.kEnglish;

        //言語を設定する
        SettingLanguageText();
    }
}
