using UnityEngine;

public partial class PauseController
{
    /// <summary>
    /// ドキュメントパネルの表示/非表示
    /// </summary>
    private void ChangeViewDocumentPanel()
    {
        if (isDocumentPanel)
        {
            //UIのレイヤーを手前側にする
            documentInventoryPanel.transform.SetAsLastSibling();

            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            documentInventoryPanel.SetActive(true);
        }
        else
        {
            //非表示
            documentInventoryPanel.SetActive(false);

            //ドキュメント説明欄パネルを非表示
            isDocumentExplanationPanel = false;
            ChangeViewDocumentExplanationPanel();
        }
    }

    /// <summary>
    /// ドキュメント説明欄パネルの表示/非表示
    /// </summary>
    private void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            documentExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// DocumentNameTextの記載内容を変更
    /// </summary>
    /// <param name="documentId">取得したid</param>
    /// <param name="documentName">変更先の記載内容</param>
    public void ChangeDocumentNameText(int documentId, string documentName)
    {
        //チュートリアル用ドキュメントの場合
        if (documentId == documentBook_TutorialID)
        {
            //フラグ値をオン
            GameController.instance.SetIsTutorialNextMessageFlag(true);


        }

        //IDを保存
        keepDocumentBookID = documentId;

        //シーン内で取得したドキュメントオブジェクトの名前を保存
        documentNameText.text = documentName;
        documentNameTextRubyComponent.Text = documentNameText.text;
    }

    /// <summary>
    /// DocumentExplanationTextの記載内容を変更
    /// </summary>
    /// <param name="documentDescription"></param>
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        //シーン内で取得したドキュメントオブジェクトの説明を保存
        documentExplanationTextRubyComponent = documentExplanationText.GetComponent<TMP_Ruby.TextMeshProRuby>();
        documentExplanationText.text = documentDescription;
        documentExplanationTextRubyComponent.Text = documentExplanationText.text;
    }

    /// <summary>
    /// ドキュメントの言語設定を行う
    /// </summary>
    private void SettingLanguageDocumentText() 
    {
        //ドキュメントIDを取得している場合
        if (keepDocumentBookID != defaultDocumentBookID)
        {
            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {
                //日本語
                case LanguageController.LanguageStatus.kJapanese:
                    //ドキュメント名称テキストを日本語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameJapanese;

                    //ドキュメント名称を日本語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeJapanese;

                    //説明テキストも日本語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionJapanese;

                    //ドキュメント説明テキストサイズを日本語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeJapanese;
                    break;

                //英語
                case LanguageController.LanguageStatus.kEnglish:
                    //\ドキュメント名称テキストを英語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameEnglish;

                    //ドキュメント名称を英語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeEnglish;

                    //説明テキストも英語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionEnglish;

                    //ドキュメント説明テキストサイズを英語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeEnglish;
                    break;

                //簡体字中国語
                case LanguageController.LanguageStatus.kSimplifiedChinese:
                    //ドキュメント名称テキストを簡体字中国語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameChinese01;

                    //ドキュメント名称を簡体字中国語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeChinese01;

                    //説明テキストも簡体字中国語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionChinese01;

                    //ドキュメント説明テキストサイズを簡体字中国語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeChinese01;
                    break;

                //繁体字中国語
                case LanguageController.LanguageStatus.kTraditionalChinese:
                    //ドキュメント名称テキストを繁体字中国語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameChinese02;

                    //ドキュメント名称を繁体字中国語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeChinese02;

                    //説明テキストも繁体字中国語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionChinese02;

                    //ドキュメント説明テキストサイズを繁体字中国語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeChinese02;
                    break;

                //スペイン語
                case LanguageController.LanguageStatus.kSpanish:
                    //ドキュメント名称テキストをスペイン語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameSpanish;

                    //ドキュメント名称をスペイン語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeSpanish;

                    //説明テキストもスペイン語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSpanish;

                    //ドキュメント説明テキストサイズをスペイン語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeSpanish;
                    break;

                //ポルトガル語
                case LanguageController.LanguageStatus.kPortuguese:
                    //ドキュメント名称テキストをポルトガル語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNamePortuguese;

                    //ドキュメント名称をポルトガル語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizePortuguese;

                    //説明テキストもポルトガル語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionPortuguese;

                    //ドキュメント説明テキストサイズをポルトガル語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizePortuguese;
                    break;

                default:
                    Debug.LogWarning("その他の言語ステータス");
                    break;
            }


            //TextMeshProRubyコンポーネントにドキュメント名称を設定する
            documentNameTextRubyComponent.Text = documentNameText.text;


            //TextMeshProRubyコンポーネントにドキュメント説明文を設定する
            documentExplanationTextRubyComponent.Text = documentExplanationText.text;
        }
    }
}