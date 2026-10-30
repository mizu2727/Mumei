using UnityEngine;
using UnityEngine.UI;

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

            //ドキュメントボタンの下は、ドキュメント名称ボタンを優先し、押せなければ戻るボタンを設定する
            SetSelectOnDown(viewDocumentButton, documentNameButton, returnToPausePanelButton);
        }
        else
        {
            //非表示
            documentInventoryPanel.SetActive(false);

            //ドキュメント説明欄パネルを非表示
            isDocumentExplanationPanel = false;
            ChangeViewDocumentExplanationPanel();


            /*----------------------------------
             * TODO:ボタンが増えるたびに調整すること
             -----------------------------------*/

            //ミステリーアイテムパネルが表示されている場合
            if (!isDocumentPanel && !isArchivePanel && isMysteryItemPanel)
            {
                //ドキュメントボタンの下は、1つ目のミステリーアイテム名称ボタン、戻るボタンの順に優先する
                SetSelectOnDown(viewDocumentButton
                    , mysteryItemNameButton != null ? mysteryItemNameButton[0] : null
                    , returnToPausePanelButton);

                Debug.Log("ミステリーアイテムパネルが表示されている場合:ドキュメントボタンの下は、1つ目のミステリーアイテム名称ボタン、戻るボタンの順に優先する");
            }
            //彷徨う者パネルが表示されている場合
            else if (isWandererPanel)
            {
                //ドキュメントボタンの下は、1つ目の彷徨う者名称ボタン、戻るボタンの順に優先する
                SetSelectOnDown(viewDocumentButton
                    , wandererNameButton != null ? wandererNameButton[0] : null
                    , returnToPausePanelButton);

                Debug.Log("彷徨う者パネルが表示されている場合:ドキュメントボタンの下は、1つ目の彷徨う者名称ボタン、戻るボタンの順に優先する");
            }
            //アーカイブパネルが表示されている場合
            else if (isArchivePanel)
            {
                //ドキュメントボタンの下は、彷徨う者ボタン、戻るボタンの順に優先する
                SetSelectOnDown(viewDocumentButton
                        , wandererButton != null ? wandererButton.GetComponent<Button>() : null
                        , returnToPausePanelButton);

                Debug.Log("アーカイブパネルが表示されている場合:ドキュメントボタンの下は、彷徨う者ボタン、戻るボタンの順に優先する");
            }
            else 
            {
                //ドキュメントボタンの下は、戻るボタンを優先する
                SetSelectOnDown(viewDocumentButton, returnToPausePanelButton);

                Debug.Log("ドキュメントパネルが非表示の場合:ドキュメントボタンの下は、戻るボタンを優先する");
            }
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

        //現在の言語用フォントを適用する
        CommonController.instance.ApplyLanguageFont(documentNameText);

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

        //現在の言語用フォントを適用する
        CommonController.instance.ApplyLanguageFont(documentExplanationText);

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

                //ロシア語
                case LanguageController.LanguageStatus.kRussian:
                    //ドキュメント名称テキストをロシア語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameRussian;

                    //ドキュメント名称をロシア語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeRussian;

                    //説明テキストもロシア語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionRussian;

                    //ドキュメント説明テキストサイズをロシア語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeRussian;
                    break;

                //ドイツ語
                case LanguageController.LanguageStatus.kGerman:
                    //ドキュメント名称テキストをドイツ語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameGerman;
                    //ドキュメント名称をドイツ語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeGerman;
                    //説明テキストもドイツ語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionGerman;
                    //ドキュメント説明テキストサイズをドイツ語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeGerman;
                    break;

                //フランス語
                case LanguageController.LanguageStatus.kFrench:
                    //ドキュメント名称テキストをフランス語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameFrench;
                    //ドキュメント名称をフランス語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeFrench;
                    //説明テキストもフランス語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionFrench;
                    //ドキュメント説明テキストサイズをフランス語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeFrench;
                    break;

                //トルコ語
                case LanguageController.LanguageStatus.kTurkish:
                    //ドキュメント名称テキストをトルコ語にする
                    documentNameText.text = itemMessage.itemMessage[keepDocumentBookID].itemNameTurkish;
                    //ドキュメント名称をトルコ語用にサイズを設定する
                    documentNameText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemNameSizeTurkish;
                    //説明テキストもトルコ語にする
                    documentExplanationText.text = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionTurkish;
                    //ドキュメント説明テキストサイズをトルコ語用に設定する
                    documentExplanationText.fontSize = itemMessage.itemMessage[keepDocumentBookID].itemDescriptionSizeTurkish;
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