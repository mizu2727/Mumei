using UnityEngine;

public partial class PauseController
{
    /// <summary>
    /// ミステリーアイテム確認パネルの表示/非表示
    /// </summary>
    private void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanel)
        {
            //UIのレイヤーを手前側にする
            mysteryItemInventoryPanel.transform.SetAsLastSibling();

            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            //非表示
            mysteryItemInventoryPanel.SetActive(false);

            //ミステリーアイテム説明欄を非表示
            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();

            //画像と説明テキストをリセット
            if (mysteryItemImage.Length > 0)
            {
                mysteryItemImage[0].sprite = null;
                mysteryItemImage[0].enabled = false;
            }

            //mysteryItemExplanationTextRubyComponentを初期化
            mysteryItemExplanationTextRubyComponent = new TMP_Ruby.TextMeshProRuby[mysteryItemExplanationText.Length];

            //説明テキストが重なるのを防止するため、全ての説明テキストを一旦クリアする
            for (int i = 0; i < mysteryItemExplanationText.Length; i++)
            {
                if (mysteryItemExplanationText.Length > 0)
                {
                    //コンポーネントを取得して配列に格納する
                    mysteryItemExplanationTextRubyComponent[i] = mysteryItemExplanationText[i].GetComponent<TMP_Ruby.TextMeshProRuby>();
                    mysteryItemExplanationText[i].text = "";
                    mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
                }
            }
        }
    }

    /// <summary>
    /// ミステリーアイテム説明欄パネルの表示/非表示
    /// </summary>
    private void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            //テキスト内容を変更する
            SettingLanguageText();

            //表示
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            //非表示
            mysteryItemExplanationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// ミステリーアイテムのUIを初期化
    /// </summary>
    private void InitializeMysteryItemUI()
    {
        //mysteryItemNameTextRubyComponentを初期化
        mysteryItemNameTextRubyComponent = new TMP_Ruby.TextMeshProRuby[mysteryItemNameText.Length];

        //nullチェック
        if (mysteryItemNameButton == null || mysteryItemNameText == null || mysteryItemNameTextRubyComponent == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        //ボタンにクリックイベントを追加
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            //ローカル変数でインデックスをキャプチャ
            int index = i;

            //クリックイベントを追加
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));

            //ミステリーアイテム名称のサイズを初期化する
            mysteryItemNameText[i].fontSize = kDefultMysteryItemNameTextSize;

            //入手していないアイテム名の初期表示を"?????????"にする
            mysteryItemNameText[i].text = defaultItemName;

        }


        for (int i = 0; i < mysteryItemNameTextRubyComponent.Length; i++)
        {
            ////mysteryItemNameTextRubyComponentに入手していないアイテム名の初期表示"?????????"を代入
            mysteryItemNameTextRubyComponent[i] = mysteryItemNameText[i].GetComponent<TMP_Ruby.TextMeshProRuby>();
            mysteryItemNameTextRubyComponent[i].Text = mysteryItemNameText[i].text;
        }

        //mysteryItemExplanationTextRubyComponent初期化処理は、ChangeViewMysteryItemPanel()内で先に実行している。
    }

    /// <summary>
    /// ミステリーアイテム名を追加し、UIに反映
    /// </summary>
    /// <param name="mysteryItemID">アイテムID</param>
    /// <param name="mysteryItemName">アイテム名</param>
    /// <param name="mysteryItemDescription">アイテム説明</param>
    public void ChangeMysteryItemTexts(int mysteryItemID, string mysteryItemName, string mysteryItemDescription)
    {
        //IDリストに追加
        mysteryItemIds.Add(mysteryItemID);

        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            //チュートリアル用ハンマーの場合
            if (mysteryItemIds[i] == hammer_TutorialID)
            {
                //フラグ値をオン
                isGetHammer_Tutorial = true;
            }

            //チュートリアル用ロープの場合
            if (mysteryItemIds[i] == rope_TutorialID)
            {
                //フラグ値をオン
                isGetRope_Tutorial = true;
            }
        }

        //アイテムリストから該当するアイテムを検索
        SO_Item.ItemData item = sO_Item.itemList.Find(x => x.itemName == mysteryItemName && x.itemType == ItemType.MysteryItem);
        if (item != null && !mysteryItemNames.Contains(mysteryItemName))
        {
            //アイテム名リストに追加
            mysteryItemNames.Add(mysteryItemName);

            //アイテム説明リストに追加
            mysteryItemExplanations.Add(mysteryItemDescription);

            //UIに反映させる
            UpdateMysteryItemUI();
        }
        else
        {
            Debug.LogWarning($"MysteryItem '{mysteryItemName}' が見つからないか、すでに追加済みです");
        }
    }

    /// <summary>
    /// ミステリーアイテムのUIを更新
    /// </summary>
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            if (i < mysteryItemNames.Count)
            {
                //入手したミステリーアイテムがリスト内に存在するかを確認
                string itemName = mysteryItemNames[i];
                SO_Item.ItemData item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

                if (item != null)
                {
                    //ボタンに表示されるテキストを"?????????"からミステリーアイテム名に変更する
                    mysteryItemNameText[i].text = itemName;
                    mysteryItemNameTextRubyComponent[i].Text = mysteryItemNameText[i].text;

                    //ボタンクリックを有効
                    mysteryItemNameButton[i].interactable = true;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //説明欄テキストにミステリーアイテム説明を反映させる
                        mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
                        mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //ミステリーアイテム画像を反映させる
                        mysteryItemImage[i].sprite = item.icon;
                        mysteryItemImage[i].enabled = (item.icon != null);
                    }
                }
                else
                {
                    Debug.LogWarning($"アイテム '{itemName}' が見つかりません");

                    //ボタンに表示されるテキストを"?????????"にする
                    mysteryItemNameText[i].text = defaultItemName;
                    mysteryItemNameTextRubyComponent[i].Text = mysteryItemNameText[i].text;

                    //ボタンクリックを無効
                    mysteryItemNameButton[i].interactable = false;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        //説明欄テキストを空にする
                        mysteryItemExplanationText[i].text = "";
                        mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        //ミステリーアイテム画像をnullする
                        mysteryItemImage[i].sprite = null;
                        mysteryItemImage[i].enabled = false;
                    }
                }
            }
            else
            {
                //ボタンに表示されるテキストを"?????????"にする
                mysteryItemNameText[i].text = defaultItemName;
                mysteryItemNameTextRubyComponent[i].Text = mysteryItemNameText[i].text;

                //ボタンクリックを無効
                mysteryItemNameButton[i].interactable = false;

                if (i < mysteryItemExplanationText.Length)
                {
                    //説明欄テキストを空にする
                    mysteryItemExplanationText[i].text = "";
                    mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
                }

                if (i < mysteryItemImage.Length)
                {
                    //ミステリーアイテム画像をnullする
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// ミステリーアイテムの言語設定を行う
    /// </summary>
    private void SettingLanguageMysteryItemText() 
    {
        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            int itemId = mysteryItemIds[i];
            SO_Item.ItemData item = sO_Item.itemList.Find(x => x.id == itemId && x.itemType == ItemType.MysteryItem);

            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {

                //日本語
                case LanguageController.LanguageStatus.kJapanese:

                    //ミステリーアイテム名称テキストを日本語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNameJapanese;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNameJapanese);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズを日本語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeJapanese;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストを日本語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionJapanese;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionJapanese);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズを日本語用に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeJapanese;
                    }
                    break;

                //英語
                case LanguageController.LanguageStatus.kEnglish:

                    //ミステリーアイテム名称テキストを英語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNameEnglish;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNameEnglish);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズを英語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeEnglish;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストを英語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionEnglish;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionEnglish);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズを英語に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeEnglish;
                    }
                    break;

                //簡体字中国語
                case LanguageController.LanguageStatus.kSimplifiedChinese:

                    //ミステリーアイテム名称テキストを簡体字中国語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNameChinese01;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNameChinese01);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズを簡体字中国語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeChinese01;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストを簡体字中国語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionChinese01;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionChinese01);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズを簡体字中国語用に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese01;
                    }
                    break;

                //繁体字中国語
                case LanguageController.LanguageStatus.kTraditionalChinese:

                    //ミステリーアイテム名称テキストを繁体字中国語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNameChinese02;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNameChinese02);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズを繁体字中国語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeChinese02;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストを繁体字中国語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionChinese02;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionChinese02);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズを繁体字中国語用に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeChinese02;
                    }
                    break;

                //スペイン語
                case LanguageController.LanguageStatus.kSpanish:

                    //ミステリーアイテム名称テキストをスペイン語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNameSpanish;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNameSpanish);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズをスペイン語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizeSpanish;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストをスペイン語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionSpanish;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionSpanish);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズをスペイン語用に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizeSpanish;
                    }
                    break;

                //ポルトガル語
                case LanguageController.LanguageStatus.kPortuguese:

                    //ミステリーアイテム名称テキストをポルトガル語にする
                    mysteryItemNames[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemNamePortuguese;
                    sO_Item.SetItemName(item.id, itemMessage.itemMessage[item.id].itemNamePortuguese);
                    mysteryItemNameText[i].text = item.itemName;

                    //ミステリーアイテム名称テキストサイズをポルトガル語用に設定する
                    mysteryItemNameText[i].fontSize = itemMessage.itemMessage[item.id].itemNameSizePortuguese;

                    if (0 < mysteryItemExplanationText.Length)
                    {
                        //ミステリーアイテム説明テキストをポルトガル語にする
                        mysteryItemExplanations[i] = itemMessage.itemMessage[mysteryItemIds[i]].itemDescriptionPortuguese;
                        sO_Item.SetItemDescription(item.id, itemMessage.itemMessage[item.id].itemDescriptionPortuguese);
                        mysteryItemExplanationText[i].text = item.description;

                        //ミステリーアイテム説明テキストサイズをポルトガル語用に設定する
                        mysteryItemExplanationText[i].fontSize = itemMessage.itemMessage[item.id].itemDescriptionSizePortuguese;
                    }
                    break;

            }

            //TextMeshProRubyコンポーネントにミステリーアイテム名称を設定する
            mysteryItemNameTextRubyComponent[i].Text = mysteryItemNameText[i].text;

            //TextMeshProRubyコンポーネントにミステリーアイテム説明文を設定する
            mysteryItemExplanationTextRubyComponent[i].Text = mysteryItemExplanationText[i].text;
        }
    }
}
