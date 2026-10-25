using UnityEngine;
using static LanguageController;

/// <summary>
/// ゴール関連のメッセージを管理するシステム
/// </summary>
public partial class MessageController
{
    /// <summary>
    /// ゴールメッセージを表示
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    public void ShowGoalMessage(int number)
    {
        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            //日本語
            case LanguageStatus.kJapanese:

                //メッセージテキストのフォントサイズを日本語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeJapanese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].message);
                break;

            //英語
            case LanguageStatus.kEnglish:

                //メッセージテキストのフォントサイズを英語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeEnglish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageEnglish);
                break;

            //簡体字中国語
            case LanguageStatus.kSimplifiedChinese:

                //メッセージテキストのフォントサイズを簡体字中国語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeChinese01;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageChinese01);
                break;

            //繁体字中国語
            case LanguageStatus.kTraditionalChinese:

                //メッセージテキストのフォントサイズを繁体字中国語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeChinese02;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageChinese02);
                break;

            //スペイン語
            case LanguageStatus.kSpanish:

                //メッセージテキストのフォントサイズをスペイン語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeSpanish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageSpanish);
                break;

            //ポルトガル語
            case LanguageStatus.kPortuguese:

                //メッセージテキストのフォントサイズをポルトガル語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizePortuguese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messagePortuguese);
                break;

            //ロシア語
            case LanguageStatus.kRussian:
                //メッセージテキストのフォントサイズをロシア語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeRussian;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageRussian);
                break;

            //ドイツ語
            case LanguageStatus.kGerman:
                //メッセージテキストのフォントサイズをドイツ語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeGerman;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageGerman);
                break;

            //フランス語
            case LanguageStatus.kFrench:
                //メッセージテキストのフォントサイズをフランス語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeFrench;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageFrench);
                break;

            //トルコ語
            case LanguageStatus.kTurkish:
                //メッセージテキストのフォントサイズをトルコ語用に設定
                messageText.fontSize = goalMessage.goalMessage[number].messageSizeTurkish;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(goalMessage.goalMessage[number].messageTurkish);
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
