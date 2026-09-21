using System;
using System.Linq;
using UnityEngine;
using static GameController;

public partial class PauseController
{
    /// <summary>
    /// 彷徨う者パネルの表示/非表示
    /// </summary>
    private void ChangeViewWandererPanel()
    {
        if (isWandererPanel)
        {
            //UIのレイヤーを手前側にする
            wandererPanel.transform.SetAsLastSibling();

            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            wandererPanel.SetActive(true);

            /*---------------------------------------------------------
             * TODO:他のアーカイブパネル内の子パネルを非表示にする処理を追加する
             --------------------------------------------------------*/

        }
        else
        {
            //非表示
            wandererPanel.SetActive(false);


            /*------------------------------------------
             * 彷徨う者説明テキストをリセット
             ---------------------------------------------*/

            //wandererExplanationTextRubyComponentを初期化
            wandererExplanationTextRubyComponent = new TMP_Ruby.TextMeshProRuby[wandererExplanationText.Length];

            //説明テキストが重なるのを防止するため、全ての説明テキストを一旦クリアする
            for (int i = 0; i < wandererExplanationText.Length; i++)
            {
                // wandererExplanationText[i]が存在する場合
                if (wandererExplanationText[i] != null)
                {
                    //コンポーネントを取得して配列に格納する
                    wandererExplanationTextRubyComponent[i] = wandererExplanationText[i].GetComponent<TMP_Ruby.TextMeshProRuby>();
                    wandererExplanationText[i].text = "";

                    //wandererExplanationTextRubyComponent[i]が存在する場合
                    if (wandererExplanationTextRubyComponent[i] != null)
                    {
                        //説明テキストをリセットする
                        wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                    }
                }
            }

            //彷徨う者説明欄を非表示
            isWandererExplanationPanel = false;
            ChangeViewWandererExplanationPanel();
        }
    }

    /// <summary>
    /// 彷徨う者説明欄パネルの表示/非表示
    /// </summary>
    private void ChangeViewWandererExplanationPanel()
    {
        if (isWandererExplanationPanel)
        {
            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            wandererExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            wandererExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 彷徨う者関連情報のUIを初期化
    /// </summary>
    private void InitializeWandererItemUI()
    {
        //wandererNameTextRubyComponentを初期化
        wandererNameTextRubyComponent = new TMP_Ruby.TextMeshProRuby[wandererNameText.Length];

        //nullチェック
        if (wandererNameButton == null || wandererNameText == null || wandererNameTextRubyComponent == null)
        {
            Debug.LogError("wandererNameButton or wandererNameText is not assigned!");
            return;
        }


        //彷徨う者関連情報を設定するためのキーを取得する
        string[] targetKeys = GameController.instance.GetIsDemoPlayFlag()
            ? new string[] { stringDemoVeinVainWandererStatus, stringBeauteousBewilderWandererStatus, stringSingSongWandererStatus } // デモ版
            : new string[] { stringVeinVainWandererStatus, stringBeauteousBewilderWandererStatus, stringSingSongWandererStatus };// 製品版

        //ボタンにクリックイベントを追加
        for (int i = 0; i < wandererNameButton.Length; i++)
        {
            //クリックイベント二重登録を防止
            wandererNameButton[i].onClick.RemoveAllListeners();

            //クリックイベントを追加
            wandererNameButton[i].onClick.AddListener(() => OnClickedWandererNameButton(i));

            //現在のボタンに対応するキーを取得
            string key = targetKeys[i];

            // ステータス配列に対象のキーが存在するか確認し、値を取得
            if (saveEnemyInformationStatusArray.TryGetValue(key, out int statusValue))
            {
                //保存用彷徨う者関連情報ステータス配列の値が1の場合
                if (statusValue == 1)
                {
                    //言語ステータスに応じて、テキストを変更する
                    switch (LanguageController.instance.GetLanguageStatus())
                    {
                        //日本語
                        case LanguageController.LanguageStatus.kJapanese:

                            //彷徨う者名称欄に日本語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameJapanese;

                            //彷徨う者名称テキストサイズを日本語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizeJapanese;

                            //彷徨う者説明欄欄に日本語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionJapanese;

                            //彷徨う者説明欄テキストサイズを日本語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizeJapanese;
                            break;

                        //英語
                        case LanguageController.LanguageStatus.kEnglish:

                            //彷徨う者名称欄に英語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameEnglish;

                            //彷徨う者名称テキストサイズを英語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizeEnglish;

                            //彷徨う者説明欄欄に英語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionEnglish;

                            //彷徨う者説明欄テキストサイズを英語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizeEnglish;
                            break;

                        //簡体字中国語
                        case LanguageController.LanguageStatus.kSimplifiedChinese:

                            //彷徨う者名称欄に簡体字中国語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameChinese01;

                            //彷徨う者名称テキストサイズを簡体字中国語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizeChinese01;

                            //彷徨う者説明欄欄に簡体字中国語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionChinese01;

                            //彷徨う者説明欄テキストサイズを簡体字中国語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizeChinese01;
                            break;

                        //繁体字中国語
                        case LanguageController.LanguageStatus.kTraditionalChinese:

                            //彷徨う者名称欄に繁体字中国語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameChinese02;

                            //彷徨う者名称テキストサイズを繁体字中国語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizeChinese02;

                            //彷徨う者説明欄欄に繁体字中国語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionChinese02;

                            //彷徨う者説明欄テキストサイズを繁体字中国語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizeChinese02;
                            break;

                        //スペイン語
                        case LanguageController.LanguageStatus.kSpanish:

                            //彷徨う者名称欄にスペイン語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSpanish;

                            //彷徨う者名称テキストサイズをスペイン語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizeSpanish;

                            //彷徨う者説明欄欄にスペイン語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSpanish;

                            //彷徨う者説明欄テキストサイズをスペイン語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizeSpanish;
                            break;

                        //ポルトガル語
                        case LanguageController.LanguageStatus.kPortuguese:

                            //彷徨う者名称欄にポルトガル語用の名称テキストを設定する
                            wandererNameText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNamePortuguese;

                            //彷徨う者名称テキストサイズをポルトガル語用に設定する
                            wandererNameText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemNameSizePortuguese;

                            //彷徨う者説明欄欄にポルトガル語用の名称テキストを設定する
                            wandererExplanationText[i].text = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionPortuguese;

                            //彷徨う者説明欄テキストサイズをポルトガル語用に設定する
                            wandererExplanationText[i].fontSize = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemDescriptionSizePortuguese;
                            break;

                        default:
                            Debug.LogWarning("その他の言語ステータス");
                            break;
                    }

                    //彷徨う者関連情報IDのリストにIDを追加する
                    enemyInformationIds[i] = itemMessage.itemMessage[kDefaultInterlockingOfFirstEnemyInformationIdAndItemId + i].itemId;

                    //彷徨う者関連情報名称のリストに名称を追加する
                    enemyInformationNames[i] = wandererNameText[i].text;

                    //彷徨う者関連情報の説明のリストに説明を追加する
                    enemyInformationExplanations[i] = wandererExplanationText[i].text;
                }
                //保存用彷徨う者関連情報ステータス配列の値が初期値の場合
                else if (saveEnemyInformationStatusArray.ElementAt(i).Value == kDefaultSaveEnemyInformationKey)
                {
                    //彷徨う者名称のサイズを初期化する
                    wandererNameText[i].fontSize = kDefaultWandererNameTextSize;

                    //入手していない彷徨う者名の初期表示を"?????????"にする
                    wandererNameText[i].text = defaultItemName;
                }
            }
        }

        //RubyComponentへの反映
        for (int i = 0; i < wandererNameTextRubyComponent.Length; i++)
        {
            ////wandererNameTextRubyComponentに入手していないアイテム名の初期表示を設定
            wandererNameTextRubyComponent[i] = wandererNameText[i].GetComponent<TMP_Ruby.TextMeshProRuby>();
            wandererNameTextRubyComponent[i].Text = wandererNameText[i].text;
        }

        //wandererExplanationTextRubyComponent初期化処理は、ChangeViewWandererPanel()内で先に実行している。
    }

    /// <summary>
    /// 彷徨う者情報関連の名前を追加し、UIに反映する関数
    /// </summary>
    /// <param name="itemID">アイテムID</param>
    /// <param name="wandererID">彷徨う者ID</param>
    /// <param name="wandererName">彷徨う者の名前</param>
    /// <param name="wandererDescription">彷徨う者の説明</param>
    public void ChangeWandererTexts(int itemID, int wandererID, string wandererName, string wandererDescription)
    {
        string targetKey;

        //デモ版で静声に熱する彷徨う者の関連情報を入手した場合
        if (GameController.instance.GetIsDemoPlayFlag() && wandererID == 1)
        {
            //デモ版の静声に熱する彷徨う者の関連情報の値を取得
            targetKey = saveEnemyInformationStatusArray.Keys.ElementAt(wandererID - 1);
        }
        else
        {
            //彷徨う者の関連情報の値を取得
            targetKey = saveEnemyInformationStatusArray.Keys.ElementAt(wandererID);
        }

        //保存用彷徨う者関連情報ステータス配列の値を1に変更する  
        saveEnemyInformationStatusArray[targetKey] = 1;

        //デモ版の静声に熱する彷徨う者の関連情報のIDを保存
        enemyInformationIds[wandererID - 1] = itemID;

        //デモ版の静声に熱する彷徨う者の関連情報の名称を保存
        enemyInformationNames[wandererID - 1] = wandererName;

        //デモ版の静声に熱する彷徨う者の関連情報の説明を保存
        enemyInformationExplanations[wandererID - 1] = wandererDescription;

        //TODO:UIのテキストを更新する処理を追加する
        UpdateEnemyInformationUI();
    }

    /// <summary>
    /// 彷徨う者関連情報のUIを更新
    /// </summary>
    private void UpdateEnemyInformationUI()
    {
        for (int i = 0; i < wandererNameText.Length; i++)
        {
            if (i < enemyInformationNames.Count)
            {
                //既に取得済みの彷徨う者関連情報の場合
                if (enemyInformationIds[i] != kDefaultSaveEnemyInformationKey)
                {
                    //ボタンに表示されるテキストを"?????????"から彷徨う者関連情報名に変更する
                    wandererNameText[i].text = enemyInformationNames[i];
                    wandererNameTextRubyComponent[i].Text = wandererNameText[i].text;

                    //ボタンクリックを有効
                    wandererNameButton[i].interactable = true;

                    if (i < wandererExplanationText.Length)
                    {
                        //説明欄テキストに彷徨う者関連情報説明を反映させる
                        wandererExplanationText[i].text = enemyInformationExplanations[i];
                        wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                    }
                }
                else
                {
                    Debug.LogWarning($"アイテム '{enemyInformationNames[i]}' が見つかりません");

                    //ボタンに表示されるテキストを"?????????"にする
                    wandererNameText[i].text = defaultItemName;
                    wandererNameTextRubyComponent[i].Text = wandererNameText[i].text;

                    //ボタンクリックを無効
                    wandererNameButton[i].interactable = false;

                    if (i < wandererExplanationText.Length)
                    {
                        //説明欄テキストを空にする
                        wandererExplanationText[i].text = "";
                        wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                    }
                }
            }
            else
            {
                //ボタンに表示されるテキストを"?????????"にする
                wandererNameText[i].text = defaultItemName;
                wandererNameTextRubyComponent[i].Text = wandererNameText[i].text;

                //ボタンクリックを無効
                wandererNameButton[i].interactable = false;

                if (i < wandererExplanationText.Length)
                {
                    //説明欄テキストを空にする
                    wandererExplanationText[i].text = "";
                    wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                }
            }
        }
    }

    /// <summary>
    /// 彷徨う者関連情報の言語設定を行う
    /// </summary>
    private void SettingLanguageWandererInformationText() 
    {
        for (int i = 0; i < enemyInformationIds.Count; i++)
        {
            //保存用彷徨う者関連情報ステータス配列の値が初期値の場合
            if (enemyInformationIds[i] == kDefaultSaveEnemyInformationKey)
            {
                //次のループへ
                continue;
            }

            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {
                //日本語
                case LanguageController.LanguageStatus.kJapanese:

                    //彷徨う者関連情報名称テキストを日本語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameJapanese;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNameJapanese;

                    //彷徨う者関連情報名称テキストサイズを日本語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizeJapanese;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを日本語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionJapanese;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionJapanese;

                        //彷徨う者関連情報説明テキストサイズを日本語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizeJapanese;
                    }
                    break;

                //英語
                case LanguageController.LanguageStatus.kEnglish:

                    //彷徨う者関連情報名称テキストを英語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameEnglish;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNameEnglish;

                    //彷徨う者関連情報名称テキストサイズを英語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizeEnglish;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを英語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionEnglish;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionEnglish;

                        //彷徨う者関連情報説明テキストサイズを英語に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizeEnglish;
                    }
                    break;

                //簡体字中国語
                case LanguageController.LanguageStatus.kSimplifiedChinese:

                    //彷徨う者関連情報名称テキストを簡体字中国語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese01;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese01;

                    //彷徨う者関連情報名称テキストサイズを簡体字中国語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizeChinese01;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを簡体字中国語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese01;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese01;

                        //彷徨う者関連情報説明テキストサイズを簡体字中国語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizeChinese01;
                    }
                    break;

                //繁体字中国語
                case LanguageController.LanguageStatus.kTraditionalChinese:

                    //彷徨う者関連情報名称テキストを繁体字中国語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese02;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese02;

                    //彷徨う者関連情報名称テキストサイズを繁体字中国語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizeChinese02;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを繁体字中国語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese02;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese02;

                        //彷徨う者関連情報説明テキストサイズを繁体字中国語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizeChinese02;
                    }
                    break;

                //スペイン語
                case LanguageController.LanguageStatus.kSpanish:

                    //彷徨う者関連情報名称テキストをスペイン語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSpanish;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSpanish;

                    //彷徨う者関連情報名称テキストサイズをスペイン語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizeSpanish;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストをスペイン語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSpanish;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSpanish;

                        //彷徨う者関連情報説明テキストサイズをスペイン語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizeSpanish;
                    }
                    break;

                //ポルトガル語
                case LanguageController.LanguageStatus.kPortuguese:

                    //彷徨う者関連情報名称テキストをポルトガル語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNamePortuguese;
                    wandererNameText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemNamePortuguese;

                    //彷徨う者関連情報名称テキストサイズをポルトガル語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSizePortuguese;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストをポルトガル語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionPortuguese;
                        wandererExplanationText[i].text = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionPortuguese;

                        //彷徨う者関連情報説明テキストサイズをポルトガル語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSizePortuguese;
                    }
                    break;

            }

            //TextMeshProRubyコンポーネントに彷徨う者関連情報名称を設定する
            wandererNameTextRubyComponent[i].Text = wandererNameText[i].text;

            //TextMeshProRubyコンポーネントに彷徨う者関連情報説明文を設定する
            wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
        }
    }
}