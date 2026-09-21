using UnityEngine;

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

                /*
                    if (wandererExplanationText.Length > 0)
                {
                    //コンポーネントを取得して配列に格納する
                    wandererExplanationTextRubyComponent[i] = wandererExplanationText[i].GetComponent<TMP_Ruby.TextMeshProRuby>();
                    wandererExplanationText[i].text = "";
                    wandererExplanationTextRubyComponent[i].Text = wandererExplanationText[i].text;
                }
                    */
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
    /// 彷徨う者関連情報のUIを更新
    /// </summary>
    private void UpdateEnemyInformationUI()
    {
        for (int i = 0; i < wandererNameText.Length; i++)
        {
            if (i < enemyInformationNames.Count)
            {
                //入手した彷徨う者関連情報がリスト内に存在するかを確認
                string itemName = enemyInformationNames[i];
                SO_Item.ItemData item = sO_Item.enemyInformationList.Find(x => x.itemName == itemName && x.itemType == ItemType.EnemyInforｍation);

                if (item != null)
                {
                    //ボタンに表示されるテキストを"?????????"から彷徨う者関連情報名に変更する
                    wandererNameText[i].text = itemName;
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
                    Debug.LogWarning($"アイテム '{itemName}' が見つかりません");

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

            int itemId = enemyInformationIds[i];
            SO_Item.ItemData item = sO_Item.enemyInformationList.Find(x => x.id == itemId && x.itemType == ItemType.EnemyInforｍation);

            Debug.Log($"enemyInformationIds[{i}] = {enemyInformationIds[i]}");
            Debug.Log($"item = {item}");

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
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNameJapanese);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズを日本語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeJapanese;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを日本語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionJapanese;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionJapanese);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズを日本語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeJapanese;
                    }

                    Debug.Log("enemyInformationNames[i]" + enemyInformationNames[i]);
                    Debug.Log("wandererNameText[i].text" + wandererNameText[i].text);
                    Debug.Log("enemyInformationExplanations[i]" + enemyInformationExplanations[i]);
                    Debug.Log("wandererExplanationText[i].text" + wandererExplanationText[i].text);

                    break;

                //英語
                case LanguageController.LanguageStatus.kEnglish:

                    //彷徨う者関連情報名称テキストを英語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameEnglish;
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNameEnglish);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズを英語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeEnglish;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを英語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionEnglish;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionEnglish);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズを英語に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeEnglish;
                    }
                    break;

                //簡体字中国語
                case LanguageController.LanguageStatus.kSimplifiedChinese:

                    //彷徨う者関連情報名称テキストを簡体字中国語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese01;
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNameChinese01);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズを簡体字中国語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeChinese01;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを簡体字中国語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese01;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionChinese01);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズを簡体字中国語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese01;
                    }
                    break;

                //繁体字中国語
                case LanguageController.LanguageStatus.kTraditionalChinese:

                    //彷徨う者関連情報名称テキストを繁体字中国語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameChinese02;
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNameChinese02);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズを繁体字中国語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeChinese02;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストを繁体字中国語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionChinese02;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionChinese02);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズを繁体字中国語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese02;
                    }
                    break;

                //スペイン語
                case LanguageController.LanguageStatus.kSpanish:

                    //彷徨う者関連情報名称テキストをスペイン語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNameSpanish;
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNameSpanish);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズをスペイン語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeSpanish;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストをスペイン語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionSpanish;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionSpanish);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズをスペイン語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeSpanish;
                    }
                    break;

                //ポルトガル語
                case LanguageController.LanguageStatus.kPortuguese:

                    //彷徨う者関連情報名称テキストをポルトガル語にする
                    enemyInformationNames[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemNamePortuguese;
                    sO_Item.SetEnemyInformationItemName(item.id, itemMessage.itemMessage[item.id].itemNamePortuguese);
                    wandererNameText[i].text = item.itemName;

                    //彷徨う者関連情報名称テキストサイズをポルトガル語用に設定する
                    wandererNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizePortuguese;

                    if (0 < wandererExplanationText.Length)
                    {
                        //彷徨う者関連情報説明テキストをポルトガル語にする
                        enemyInformationExplanations[i] = itemMessage.itemMessage[enemyInformationIds[i]].itemDescriptionPortuguese;
                        sO_Item.SetEnemyInformationItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionPortuguese);
                        wandererExplanationText[i].text = item.description;

                        //彷徨う者関連情報説明テキストサイズをポルトガル語用に設定する
                        wandererExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizePortuguese;
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
