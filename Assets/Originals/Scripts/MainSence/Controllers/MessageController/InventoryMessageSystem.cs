using UnityEngine;
using static LanguageController;

/// <summary>
/// インベントリ関連のメッセージを管理するシステム
/// </summary>
public partial class MessageController
{
    /// <summary>
    /// インベントリメッセージを表示
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    public void ShowInventoryMessage(int number)
    {
        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            //日本語
            case LanguageStatus.kJapanese:

                //メッセージテキストのフォントサイズを日本語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeJapanese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].message);
                break;

            //英語
            case LanguageStatus.kEnglish:

                //メッセージテキストのフォントサイズを英語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeEnglish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageEnglish);
                break;

            //簡体字中国語
            case LanguageStatus.kSimplifiedChinese:

                //メッセージテキストのフォントサイズを簡体字中国語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeChinese01;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageChinese01);
                break;

            //繁体字中国語
            case LanguageStatus.kTraditionalChinese:

                //メッセージテキストのフォントサイズを繁体字中国語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeChinese02;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageChinese02);
                break;

            //スペイン語
            case LanguageStatus.kSpanish:

                //メッセージテキストのフォントサイズをスペイン語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeSpanish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageSpanish);
                break;

            //ポルトガル語
            case LanguageStatus.kPortuguese:

                //メッセージテキストのフォントサイズをポルトガル語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizePortuguese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messagePortuguese);
                break;

            //ロシア語
            case LanguageStatus.kRussian:
                //メッセージテキストのフォントサイズをロシア語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeRussian;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageRussian);
                break;

            //ドイツ語
            case LanguageStatus.kGerman:
                //メッセージテキストのフォントサイズをドイツ語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeGerman;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageGerman);
                break;

            //フランス語
            case LanguageStatus.kFrench:
                //メッセージテキストのフォントサイズをフランス語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeFrench;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageFrench);
                break;

            //トルコ語
            case LanguageStatus.kTurkish:
                //メッセージテキストのフォントサイズをトルコ語用に設定
                messageText.fontSize = inventoryMessage.inventoryMessage[number].messageSizeTurkish;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(inventoryMessage.inventoryMessage[number].messageTurkish);
                break;

            //それ以外の場合
            default:
                Debug.LogError("想定していない言語ステータスです");
                break;
        }

        //メッセージパネルを表示
        isMessagePanel = true;
        ViewMessagePanel();
    }
}
