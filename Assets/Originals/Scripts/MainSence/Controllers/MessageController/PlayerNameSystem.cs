using UnityEngine;
using static LanguageController;

/// <summary>
/// プレイヤーの名前入力に関する処理をまとめたクラス
/// </summary>
public partial class MessageController
{
    /// <summary>
    /// プレイヤーの名前の入力が完了した際に呼ばれる
    /// </summary>
    /// <param name="playerName">入力したプレイヤー名</param>
    public void SavePlayerName(string playerName)
    {
        //2文字以上10文字以下であるか
        if ((kMinPlayerNameLength < playerName.Length) && (playerName.Length < kMaxPlayerNameLength))
        {
            //名前を一時的に保存
            inputPlayerNameField.text = playerName;


            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {
                //日本語
                case LanguageStatus.kJapanese:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = inputPlayerNameField.text + " でよろしいですか？";
                    break;

                //英語
                case LanguageStatus.kEnglish:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "Is it correct as " + inputPlayerNameField.text + "？";
                    break;

                //簡体字中国語
                case LanguageStatus.kSimplifiedChinese:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = inputPlayerNameField.text + " 正确吗？";
                    break;

                //繁体字中国語
                case LanguageStatus.kTraditionalChinese:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = inputPlayerNameField.text + " 正確嗎？";
                    break;

                //スペイン語
                case LanguageStatus.kSpanish:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "¿Es correcto " + inputPlayerNameField.text + "?";
                    break;

                //ポルトガル語
                case LanguageStatus.kPortuguese:

                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "Deseja usar " + inputPlayerNameField.text + "?";
                    break;

                //ロシア語
                case LanguageStatus.kRussian:
                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "Правильно ли " + inputPlayerNameField.text + "?";
                    break;

                //ドイツ語
                case LanguageStatus.kGerman:
                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "Ist es richtig als " + inputPlayerNameField.text + "?";
                    break;

                //フランス語
                case LanguageStatus.kFrench:
                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = "Est-ce correct comme " + inputPlayerNameField.text + "?";
                    break;

                //トルコ語
                case LanguageStatus.kTurkish:
                    //確認用テキストに入力した名前を表示
                    CheckInputNameText.text = inputPlayerNameField.text + " Doğru mu?";
                    break;

                //それ以外の場合
                default:
                    Debug.LogError("想定していない言語ステータスです");
                    break;
            }

            //入力内容確認パネルを表示
            CheckInputNamePanel.SetActive(true);

            inputPlayerNameField.gameObject.SetActive(false);
        }
        else
        {
            inputPlayerNameField.gameObject.SetActive(false);
            showSystemMessage.ShowGameSystemMessage(4);
        }
    }

    /// <summary>
    /// 入力内容確認パネルで「はい」ボタンが押された際に呼ばれる
    /// </summary>
    public void OnClickedYesCheckInputNameButton()
    {
        //名前を保存
        GameController.playerName = inputPlayerNameField.text;

        CheckInputNamePanel.SetActive(false);

        //次のシステムメッセージを表示
        showSystemMessage.ShowGameSystemMessage(5);
    }

    /// <summary>
    /// 入力内容確認パネルで「いいえ」ボタンが押された際に呼ばれる
    /// </summary>
    public void OnClickedNoCheckInputNameButton()
    {
        CheckInputNamePanel.SetActive(false);
        inputPlayerNameField.gameObject.SetActive(true);
    }
}
