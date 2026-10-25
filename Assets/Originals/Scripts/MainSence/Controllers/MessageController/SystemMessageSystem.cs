using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;
using static LanguageController;

/// <summary>
/// システムメッセージを管理するシステム
/// </summary>
public partial class MessageController
{
    /// <summary>
    /// システムメッセージを表示
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    /// <returns></returns>
    public async UniTask ShowSystemMessage(int number)
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

            //言語ステータスに応じて、テキストを変更する
            switch (LanguageController.instance.GetLanguageStatus())
            {
                //日本語
                case LanguageStatus.kJapanese:

                    //メッセージテキストのフォントサイズを日本語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeJapanese;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].message);
                    break;

                //英語
                case LanguageStatus.kEnglish:

                    //メッセージテキストのフォントサイズを英語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeEnglish;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageEnglish);
                    break;

                //簡体字中国語
                case LanguageStatus.kSimplifiedChinese:

                    //メッセージテキストのフォントサイズを簡体字中国語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeChinese01;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageChinese01);
                    break;

                //繁体字中国語
                case LanguageStatus.kTraditionalChinese:

                    //メッセージテキストのフォントサイズを繁体字中国語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeChinese02;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageChinese02);
                    break;

                //スペイン語
                case LanguageStatus.kSpanish:

                    //メッセージテキストのフォントサイズをスペイン語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeSpanish;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageSpanish);
                    break;

                //ポルトガル語
                case LanguageStatus.kPortuguese:

                    //メッセージテキストのフォントサイズをポルトガル語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizePortuguese;

                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messagePortuguese);
                    break;

                //ロシア語
                case LanguageStatus.kRussian:
                    //メッセージテキストのフォントサイズをロシア語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeRussian;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageRussian);
                    break;

                //ドイツ語
                case LanguageStatus.kGerman:
                    //メッセージテキストのフォントサイズをドイツ語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeGerman;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageGerman);
                    break;

                //フランス語
                case LanguageStatus.kFrench:
                    //メッセージテキストのフォントサイズをフランス語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeFrench;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageFrench);
                    break;

                //トルコ語
                case LanguageStatus.kTurkish:
                    //メッセージテキストのフォントサイズをトルコ語用に設定
                    messageText.fontSize = systemMessage.systemMessage[number].messageSizeTurkish;
                    //エクセルデータ型.リスト型[番号].カラム名
                    Write(systemMessage.systemMessage[number].messageTurkish);
                    break;

                //それ以外の場合
                default:
                    Debug.LogError("想定していない言語ステータスです");
                    break;
            }

            //InputPlayerNameFieldに関するステータスが1の場合
            if (systemMessage.systemMessage[number].isInputPlayerNameFieldStatus == 1)
            {
                //名前入力UIを表示
                await ShowNextMessage();

                ResetMessage();

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                inputPlayerNameField.gameObject.SetActive(true);

                //処理をスキップ
                return;
            }

            //InputPlayerNameFieldに関するステータスが2の場合
            if (systemMessage.systemMessage[number].isInputPlayerNameFieldStatus == 2)
            {
                //名前入力制限に引っかかった後に、もう一度名前入力UIを表示
                await ShowNextMessage();

                ResetMessage();
                showSystemMessage.ShowGameSystemMessage(systemMessage.systemMessage[number].showSystemMessageNumber);

                //処理をスキップ
                return;
            }

            //isPlaySoundNoiseStatusが3の場合
            if (systemMessage.systemMessage[number].isPlaySoundNoiseStatus == 3)
            {
                await ShowNextMessage();

                messageText.text = "";

                number++;

                await UniTask.Delay(TimeSpan.FromSeconds(2));

                //ノイズを流す
                audioSourceSE.clip = sO_SE.GetSEClip(noiseSEid);
                MusicController.instance.PlayMomentAudioSE(audioSourceSE, audioSourceSE.clip);

                //文字の色を赤色に設定
                messageText.color = Color.red;

                //言語ステータスに応じて、テキストを変更して表示する
                SettingLanguageSystemMessage(number);

                await ShowNextMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(1));

                ResetMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(3));

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //HomeSceneへ移動
                SceneManager.LoadScene(CommonController.instance.GetHomeSceneName());

                //処理をスキップ
                return;
            }

            //TalkMessageStatus関連フラグが1の場合
            if (systemMessage.systemMessage[number].isShowTalkMessageStatus == 1)
            {
                await ShowNextMessage();

                ResetMessage();

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);
                //処理をスキップ
                return;
            }

            //チュートリアルフラグが4の場合
            if (systemMessage.systemMessage[number].isTutorialStatus == 4)
            {
                //ドキュメント(チュートリアル版)入手したらメッセージを勧める
                await UniTask.WaitUntil(() => GameController.instance.GetIsTutorialNextMessageFlag(), cancellationToken: cts.Token);
                GameController.instance.SetIsTutorialNextMessageFlag(false);

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                //左クリック…ドキュメント入手操作の説明
                ResetMessage();

                //ストーリーモードへ変更
                GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);
                //処理をスキップ
                return;
            }

            //チュートリアルフラグが5の場合
            if (systemMessage.systemMessage[number].isTutorialStatus == 5)
            {
                //ドキュメント(チュートリアル版)を閲覧後にポーズ解除したらメッセージを勧める
                await UniTask.WaitUntil(() => PauseController.instance != null && GameController.instance.GetIsTutorialNextMessageFlag()
                    && PauseController.instance.ViewTutorialItemExplanation() && OptionUIController.instance != null
                    && !OptionUIController.instance.GetIsOptionPanel(), cancellationToken: cts.Token);
                GameController.instance.SetIsTutorialNextMessageFlag(false);

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                ResetMessage();

                //ストーリーモードへ変更
                GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);
                //処理をスキップ
                return;
            }

            //チュートリアルフラグが6の場合
            if (systemMessage.systemMessage[number].isTutorialStatus == 6)
            {
                //ミステリーアイテム(チュートリアル版)入手したらメッセージを勧める
                await UniTask.WaitUntil(() => PauseController.instance != null && PauseController.instance.IsGetHammer_Tutorial()
                    && PauseController.instance.IsGetRope_Tutorial(), cancellationToken: cts.Token);

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                ResetMessage();

                //ストーリーモードへ変更
                GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);
                //処理をスキップ
                return;
            }

            //チュートリアルフラグが7の場合
            if (systemMessage.systemMessage[number].isTutorialStatus == 7)
            {
                //ミステリーアイテム(チュートリアル版)を閲覧後にポーズ解除したらメッセージを勧める
                await UniTask.WaitUntil(() => PauseController.instance != null && PauseController.instance.IsViewMysteryItem_Tutorial()
                    && PauseController.instance.ViewTutorialItemExplanation() && OptionUIController.instance != null
                    && !OptionUIController.instance.GetIsOptionPanel(), cancellationToken: cts.Token);
                PauseController.instance.SetIsViewMysteryItem_Tutorial(false);
                ResetMessage();

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                //ストーリーモードへ変更
                GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);
                //処理をスキップ
                return;
            }

            //チュートリアルフラグが8の場合
            if (systemMessage.systemMessage[number].isTutorialStatus == 8)
            {
                //チュートリアル終了

                //チュートリアル用ゴールの閲覧終了したらメッセージを勧める
                await UniTask.WaitUntil(() => Time.timeScale == 1
                && GameController.instance.GetIsTutorialGoalFlag());


                GameController.instance.SetIsTutorialGoalFlag(false);

                messageText.text = "";
                number++;

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //画面ブラックアウト
                isBlackOutPanel = true;
                ViewBlackOutPanel();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //wall_Tutorialを表示
                HomeController.instance.wall_Tutorial.SetActive(true);

                //wall_EndTutorialを非表示
                HomeController.instance.wall_EndTutorial.SetActive(false);

                //プレイヤー・カナメをワープ
                Kaname.instance.WarpPostion(1, 0.505f, 2);
                Player.instance.PlayerWarp(1, 0.562f, 0);

                //カメラの角度をリセット
                if (Player.instance != null)
                {
                    // カメラの上下回転をリセット
                    PlayerCamera.instance.ResetCameraRotation();

                    // プレイヤーの向きをカメラの正面に同期（必要に応じて）
                    Player.instance.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

                    //プレイヤーのtransform.rotationの値を保存していた値に戻す
                    Player.instance.transform.rotation = savePlayerQuaternion;
                }

                //プレイヤーカメラのX軸回転リセットフラグをtrueに設定
                PlayerCamera.instance.SetIsResetXRotate(true);

                await UniTask.Delay(TimeSpan.FromSeconds(1.5));

                //プレイヤーカメラのX軸回転リセットフラグをfalseに設定
                PlayerCamera.instance.SetIsResetXRotate(false);

                //チュートリアル用アイテム関係の引き出しを非表示
                GameController.instance.GetTutorialDocumentDrawer().SetActive(false);
                GameController.instance.GetTutorialMysteryItemDrawer01().SetActive(false);
                GameController.instance.GetTutorialMysteryItemDrawer02().SetActive(false);

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                //画面ブラックアウトを解除
                isBlackOutPanel = false;
                ViewBlackOutPanel();

                showSystemMessage.ShowGameSystemMessage(number);

                //処理をスキップ
                return;
            }

            //TalkMessageStatus関連フラグが2の場合
            if (systemMessage.systemMessage[number].isShowTalkMessageStatus == 2)
            {
                await ShowNextMessage();

                ResetMessage();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                showTalkMessage.ShowGameTalkMessage(systemMessage.systemMessage[number].ShowTalkMessageNumber);

                //処理をスキップ
                return;
            }

            //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
            await ShowNextMessage();

            messageText.text = "";
            number++;

            //スペースキー押下で次のメッセージを書く
            showSystemMessage.ShowGameSystemMessage(number);
        }
    }

    /// <summary>
    /// システムメッセージの言語を設定
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    private void SettingLanguageSystemMessage(int number)
    {
        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            //日本語
            case LanguageStatus.kJapanese:

                //メッセージテキストのフォントサイズを日本語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeJapanese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].message);
                break;

            //英語
            case LanguageStatus.kEnglish:

                //メッセージテキストのフォントサイズを英語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeEnglish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageEnglish);
                break;

            //簡体字中国語
            case LanguageStatus.kSimplifiedChinese:

                //メッセージテキストのフォントサイズを簡体字中国語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeChinese01;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageChinese01);
                break;

            //繁体字中国語
            case LanguageStatus.kTraditionalChinese:

                //メッセージテキストのフォントサイズを繁体字中国語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeChinese02;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageChinese02);
                break;

            //スペイン語
            case LanguageStatus.kSpanish:

                //メッセージテキストのフォントサイズをスペイン語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeSpanish;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageSpanish);
                break;

            //ポルトガル語
            case LanguageStatus.kPortuguese:

                //メッセージテキストのフォントサイズをポルトガル語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizePortuguese;

                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messagePortuguese);
                break;

            //ロシア語
            case LanguageStatus.kRussian:
                //メッセージテキストのフォントサイズをロシア語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeRussian;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageRussian);
                break;

            //ドイツ語
            case LanguageStatus.kGerman:
                //メッセージテキストのフォントサイズをドイツ語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeGerman;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageGerman);
                break;

            //フランス語
            case LanguageStatus.kFrench:
                //メッセージテキストのフォントサイズをフランス語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeFrench;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageFrench);
                break;

            //トルコ語
            case LanguageStatus.kTurkish:
                //メッセージテキストのフォントサイズをトルコ語用に設定
                messageText.fontSize = systemMessage.systemMessage[number].messageSizeTurkish;
                //エクセルデータ型.リスト型[番号].カラム名
                Write(systemMessage.systemMessage[number].messageTurkish);
                break;

            //それ以外の場合
            default:
                Debug.LogError("想定していない言語ステータスです");
                break;
        }
    }
}
