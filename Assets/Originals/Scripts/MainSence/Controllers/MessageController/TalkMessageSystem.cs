using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;
using static LanguageController;

/// <summary>
/// 会話関連のメッセージを管理するシステム
/// </summary>
public partial class MessageController
{
    /// <summary>
    /// 会話メッセージを表示
    /// </summary>
    /// <param name="number">会話番号</param>
    /// <returns></returns>
    public async UniTask ShowTalkMessage(int number)
    {
        //既にメッセージを書いてる途中である場合は、以降の処理を中断
        if (isWrite)
        {
            //書き込み速度を上げて高速表示
            writeSpeed = 0;
            return;
        }

        //前のメッセージが書いてる途中であるかを判断。書き途中ならtrue
        if (Time.timeScale == 1)
        {
            //メッセージパネルを表示
            isMessagePanel = true;
            ViewMessagePanel();

            //メッセージ系のテキストの色を変更
            ChangeTextColor(number);

            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {
                //日本語の場合
                case LanguageStatus.kJapanese:

                    //メッセージテキストのフォントサイズを日本語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeJapanese;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].message);
                    break;

                //英語の場合
                case LanguageStatus.kEnglish:

                    //メッセージテキストのフォントサイズを英語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeEnglish;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageEnglish);
                    break;

                //簡体字中国語の場合
                case LanguageStatus.kSimplifiedChinese:

                    //メッセージテキストのフォントサイズを簡体字中国語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeChinese01;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageChinese01);
                    break;

                //繁体字中国語の場合
                case LanguageStatus.kTraditionalChinese:

                    //メッセージテキストのフォントサイズを繁体字中国語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeChinese02;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageChinese02);
                    break;

                //スペイン語の場合
                case LanguageStatus.kSpanish:

                    //メッセージテキストのフォントサイズをスペイン語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeSpanish;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageSpanish);
                    break;

                //ポルトガル語の場合
                case LanguageStatus.kPortuguese:

                    //メッセージテキストのフォントサイズをポルトガル語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizePortuguese;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messagePortuguese);
                    break;

                //ロシア語
                case LanguageStatus.kRussian:
                    //メッセージテキストのフォントサイズをロシア語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeRussian;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageRussian);
                    break;

                //ドイツ語
                case LanguageStatus.kGerman:
                    //メッセージテキストのフォントサイズをドイツ語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeGerman;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageGerman);
                    break;

                //フランス語
                case LanguageStatus.kFrench:
                    //メッセージテキストのフォントサイズをフランス語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeFrench;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageFrench);
                    break;

                //トルコ語
                case LanguageStatus.kTurkish:
                    //メッセージテキストのフォントサイズをトルコ語用に設定
                    messageText.fontSize = talkMessage.talkMessage[number].messageSizeTurkish;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(talkMessage.talkMessage[number].messageTurkish);
                    break;

                //それ以外の場合
                default:
                    Debug.LogError("想定していない言語ステータスです");
                    break;
            }


            //会話している人の名前を表示
            isViewSpeakerNamePanel = true;
            ViewSpeakerNamePanel();

            //speakerNameTextが存在する場合
            if (speakerNameText != null)
            {
                //言語ステータスに応じて、テキストを変更する
                switch (LanguageController.instance.GetLanguageStatus())
                {
                    //日本語の場合
                    case LanguageStatus.kJapanese:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerName;
                        break;

                    //英語の場合
                    case LanguageStatus.kEnglish:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameEnglish;
                        break;

                    //簡体字中国語の場合
                    case LanguageStatus.kSimplifiedChinese:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameChinese01;
                        break;

                    //繁体字中国語の場合
                    case LanguageStatus.kTraditionalChinese:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameChinese02;
                        break;

                    //スペイン語の場合
                    case LanguageStatus.kSpanish:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameSpanish;
                        break;

                    //ポルトガル語の場合
                    case LanguageStatus.kPortuguese:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNamePortuguese;
                        break;

                    //ロシア語の場合
                    case LanguageStatus.kRussian:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameRussian;
                        break;

                    //ドイツ語の場合
                    case LanguageStatus.kGerman:
                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameGerman;
                        break;

                    //フランス語の場合
                    case LanguageStatus.kFrench:

                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameFrench;
                        break;

                    //トルコ語の場合
                    case LanguageStatus.kTurkish:
                        //会話している人の名前を設定
                        speakerNameText.text = talkMessage.talkMessage[number].speakerNameTurkish;
                        break;
                }
            }

            //SceneがGameClearSceneでない場合(GameClearSceneからデバッグプレイする際に発生するエラーを防ぐため)
            if (SceneManager.GetActiveScene().name != CommonController.instance.GetGameClearSceneName())
            {
                //wall_Tutorialを消してゴールオブジェクトが見えるようにする
                if (HomeController.instance.wall_Tutorial != null && number == kMessageNumber47)
                {
                    HomeController.instance.wall_Tutorial.SetActive(false);
                }
            }

            await ShowNextMessage();


            //後ろを振り向くステータスが1の場合
            if (talkMessage.talkMessage[number].isplayerBackRotateStatus == 1)
            {
                //メッセージテキストと会話している人の名前テキストをリセット
                messageText.text = "";
                speakerNameText.text = "";

                //後ろを振り返る
                Player.instance.playerIsBackRotate = true;

                await UniTask.Delay(TimeSpan.FromSeconds(talkMessage.talkMessage[number].waitTime));

                //次のメッセージ番号へ
                number++;

                //プレイヤーの回転を保存
                savePlayerQuaternion = Player.instance.transform.rotation;

                //スペースキー押下で次のメッセージを書く
                showTalkMessage.ShowGameTalkMessage(number);

                //処理をスキップ
                return;
            }

            //isPlaySoundNoiseStatusが1の場合
            if (talkMessage.talkMessage[number].isPlaySoundNoiseStatus == 1)
            {
                messageText.text = "";
                speakerNameText.text = "";
                number++;

                await UniTask.Delay(TimeSpan.FromSeconds(1));

                //BGMを一時停止してノイズを流す
                MusicController.instance.PauseBGM(HomeController.instance.GetAudioSourceBGM(),
                    sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());

                audioSourceSE.clip = sO_SE.GetSEClip(noiseSEid);
                MusicController.instance.PlayMomentAudioSE(audioSourceSE, audioSourceSE.clip);

                //メッセージテキストの色を赤色に変更するフラグをtrueに設定
                isChangeMessageTextColorRed = true;

                //文字の色を赤色に設定
                messageText.color = Color.red;

                //メッセージ系のテキストの色を変更
                ChangeTextColor(number);

                //言語ステータスに応じて、テキストを変更して表示する
                SettingLanguageTalkMessage(number);

                //speakerNameTextが存在する場合
                if (speakerNameText != null)
                {
                    //言語ステータスに応じて、テキストを変更する
                    switch (LanguageController.instance.GetLanguageStatus())
                    {
                        //日本語
                        case LanguageStatus.kJapanese:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerName;
                            break;

                        //英語
                        case LanguageStatus.kEnglish:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameEnglish;
                            break;

                        //簡体字中国語
                        case LanguageStatus.kSimplifiedChinese:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameChinese01;
                            break;

                        //繁体字中国語
                        case LanguageStatus.kTraditionalChinese:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameChinese02;
                            break;

                        //スペイン語
                        case LanguageStatus.kSpanish:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameSpanish;
                            break;

                        //ポルトガル語
                        case LanguageStatus.kPortuguese:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNamePortuguese;
                            break;

                        //ロシア語
                        case LanguageStatus.kRussian:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameRussian;
                            break;

                        //ドイツ語
                        case LanguageStatus.kGerman:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameGerman;
                            break;

                        //フランス語
                        case LanguageStatus.kFrench:

                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameFrench;
                            break;

                        //トルコ語
                        case LanguageStatus.kTurkish:
                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerNameTurkish;
                            break;
                    }
                }

                await ShowNextMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //BGMの一時停止を解除
                MusicController.instance.UnPauseBGM(HomeController.instance.GetAudioSourceBGM(),
                    sO_BGM.GetBGMClip(HomeController.instance.GetHomeSceneBGMId()), HomeController.instance.GetHomeSceneBGMId());


                //メッセージテキストの色を赤色に変更するフラグをfalseに設定
                isChangeMessageTextColorRed = false;

                messageText.text = "";
                speakerNameText.text = "";
                number++;

                showTalkMessage.ShowGameTalkMessage(number);

                //処理をスキップ
                return;
            }

            //isPlaySoundNoiseStatusが2の場合
            if (talkMessage.talkMessage[number].isPlaySoundNoiseStatus == 2)
            {
                messageText.text = "";

                //BGMを止める
                MusicController.instance.StopBGM(GameClearController.instance.GetAudioSourceBGM(),
                    sO_BGM.GetBGMClip(GameClearController.instance.GetGameClearSceneBGMId()), GameClearController.instance.GetGameClearSceneBGMId());

                //画面ブラックアウト
                isBlackOutPanel = true;
                ViewBlackOutPanel();

                await UniTask.Delay(TimeSpan.FromSeconds(talkMessage.talkMessage[number].waitTime));

                number++;

                //ノイズを流す
                audioSourceSE.clip = sO_SE.GetSEClip(noiseSEid);
                MusicController.instance.PlayMomentAudioSE(audioSourceSE, audioSourceSE.clip);

                //文字の色を赤色に設定
                messageText.color = Color.red;

                //言語ステータスに応じて、テキストを変更して表示する
                SettingLanguageTalkMessage(number);

                await ShowNextMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                ResetMessage();

                //ゲームクリアパネルを表示
                GameClearController.instance.ViewGameClearUI();

                //画面ブラックアウトを解除
                isBlackOutPanel = false;
                ViewBlackOutPanel();

                //処理をスキップ
                return;
            }

            //待ち時間を発生させるステータスが1の場合
            if (talkMessage.talkMessage[number].isWaitStatus == 1)
            {
                messageText.text = "";

                //指定の秒数間待機
                await UniTask.Delay(TimeSpan.FromSeconds(talkMessage.talkMessage[number].waitTime));

                number++;

                //スペースキー押下で次のメッセージを書く
                showTalkMessage.ShowGameTalkMessage(number);

                //処理をスキップ
                return;
            }

            //チュートリアルフラグが1の場合
            if (talkMessage.talkMessage[number].isTutorialStatus == 1)
            {
                //会話終了してチュートリアルに入る
                ResetMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //画面をブラックアウト
                isBlackOutPanel = true;
                ViewBlackOutPanel();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //カナメをワープ
                Kaname.instance.WarpPostion(1, 0.505f, 7);

                //チュートリアル用ドキュメントの引き出しを表示
                GameController.instance.GetTutorialDocumentDrawer().SetActive(true);

                //画面ブラックアウトを解除
                isBlackOutPanel = false;
                ViewBlackOutPanel();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //システムメッセージ
                showSystemMessage.ShowGameSystemMessage(talkMessage.talkMessage[number].showSystemMessageNumber);

                //処理をスキップ
                return;
            }

            //チュートリアルフラグが2の場合
            if (talkMessage.talkMessage[number].isTutorialStatus == 2)
            {
                ResetMessage();

                GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                showSystemMessage.ShowGameSystemMessage(talkMessage.talkMessage[number].showSystemMessageNumber);

                //処理をスキップ
                return;
            }

            //チュートリアルフラグが3の場合
            if (talkMessage.talkMessage[number].isTutorialStatus == 3)
            {
                ResetMessage();

                //チュートリアル用ミステリーアイテム関連の引き出しを表示
                GameController.instance.GetTutorialMysteryItemDrawer01().SetActive(true);
                GameController.instance.GetTutorialMysteryItemDrawer02().SetActive(true);

                GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                showSystemMessage.ShowGameSystemMessage(12);

                //処理をスキップ
                return;
            }

            //会話終了パターンその1の場合
            if (talkMessage.talkMessage[number].isEndStatus == 1)
            {
                ResetMessage();

                //チュートリアル用引き出しを表示
                GameController.instance.GetTutorialDrawer().SetActive(true);

                //プレイヤーカメラの回転を元に戻す
                PlayerCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                PlayerCamera.instance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                //ゲームモードステータスをInGameに設定
                GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                //チュートリアルのストーリを閲覧したことを保存する
                saveViewStoryStatusArray[stringTutorialClearStatus] = 1;

                //彷徨う者ボタンを表示する
                PauseController.instance.SetWandererButtonActive(true);

                //処理をスキップ
                return;
            }

            //ブラックアウト関連ステータスが2の場合
            if (talkMessage.talkMessage[number].isBlackOutStatus == 2)
            {
                messageText.text = "";
                number++;

                //画面ブラックアウトを解除
                isBlackOutPanel = false;
                ViewBlackOutPanel();

                await UniTask.Delay(TimeSpan.FromSeconds(1));

                showTalkMessage.ShowGameTalkMessage(number);

                //処理をスキップ
                return;
            }

            //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
            messageText.text = "";
            number++;

            //スペースキー押下で次のメッセージを書く
            showTalkMessage.ShowGameTalkMessage(number);
        }
    }

    /// <summary>
    /// 会話メッセージの言語を設定
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    private void SettingLanguageTalkMessage(int number)
    {
        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            //日本語
            case LanguageStatus.kJapanese:

                //メッセージテキストのフォントサイズを日本語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeJapanese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].message);
                break;

            //英語
            case LanguageStatus.kEnglish:

                //メッセージテキストのフォントサイズを英語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeEnglish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageEnglish);
                break;

            //簡体字中国語
            case LanguageStatus.kSimplifiedChinese:

                //メッセージテキストのフォントサイズを簡体字中国語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeChinese01;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageChinese01);
                break;

            //繁体字中国語
            case LanguageStatus.kTraditionalChinese:

                //メッセージテキストのフォントサイズを繁体字中国語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeChinese02;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageChinese02);
                break;

            //スペイン語
            case LanguageStatus.kSpanish:

                //メッセージテキストのフォントサイズをスペイン語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeSpanish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageSpanish);
                break;

            //ポルトガル語
            case LanguageStatus.kPortuguese:

                //メッセージテキストのフォントサイズをポルトガル語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizePortuguese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messagePortuguese);
                break;

            //ロシア語
            case LanguageStatus.kRussian:
                //メッセージテキストのフォントサイズをロシア語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeRussian;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageRussian);
                break;

            //ドイツ語
            case LanguageStatus.kGerman:
                //メッセージテキストのフォントサイズをドイツ語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeGerman;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageGerman);
                break;

            //フランス語
            case LanguageStatus.kFrench:
                //メッセージテキストのフォントサイズをフランス語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeFrench;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageFrench);
                break;

            //トルコ語
            case LanguageStatus.kTurkish:
                //メッセージテキストのフォントサイズをトルコ語用に設定
                messageText.fontSize = talkMessage.talkMessage[number].messageSizeTurkish;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(talkMessage.talkMessage[number].messageTurkish);
                break;

            //それ以外の場合
            default:
                Debug.LogError("想定していない言語ステータスです");
                break;
        }
    }
}
