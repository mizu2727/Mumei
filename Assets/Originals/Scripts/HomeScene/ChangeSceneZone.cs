using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
using static GameController;

public class ChangeSceneZone : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static ChangeSceneZone instance;

    /// <summary>
    /// 現在のシーン名
    /// </summary>
    private string nowSceneName;

    /// <summary>
    /// HomeScene(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringHomeScene = "HomeScene";

    /// <summary>
    /// Stage01(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage01Scene = "Stage01";

    /// <summary>
    /// Stage02(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage02Scene = "Stage02";

    /// <summary>
    /// Stage03(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage03Scene = "Stage03";

    /// <summary>
    /// Stage04(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage04Scene = "Stage04";

    /// <summary>
    /// シーン名の配列(0番目のみ空けている)
    /// </summary>
    private string[] stageSceneNameArray = {""
            ,stringStage01Scene, stringStage02Scene
            , stringStage03Scene, stringStage04Scene};

    /// <summary>
    ///  シーン名配列インデックス番号
    /// </summary>
    private int stageSceneNameArrayIndex;

    /// <summary>
    /// EasyLevel(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringEasyLevel = "EasyLevel";

    /// <summary>
    /// NormalLevel(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringNormalLevel = "NormalLevel";

    /// <summary>
    /// NightmareLevel(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringNightmareLevel = "NightmareLevel";

    /// <summary>
    /// 比較用の最速クリア時間
    /// </summary>
    TimeSpan saveTimeSpan;


    /// <summary>
    /// シーン名配列インデックス番号を設定する
    /// </summary>
    /// <param name="index">シーン名配列インデックス番号</param>
    public void SetStageSceneNameArrayIndex(int index) 
    {
        stageSceneNameArrayIndex = index;
    }


    private void OnDestroy()
    {
        //インスタンスが存在する場合
        if (instance != null)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }

    private void Awake()
    {
        //インスタンスが存在しない場合
        if (instance == null)
        {
            //インスタンスを設定
            instance = this;
        }
        else
        {
            //このオブジェクトを破壊する
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //現在のシーン名を取得
        nowSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// オブジェクトのコリジョンと衝突した場合の処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトのコリジョン</param>
    private void OnCollisionEnter(Collision collision)
    {
        //プレイヤー以外が触れた場合||プレイヤーが存在しない場合||プレイヤーが死亡している場合
        if (!collision.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) || Player.instance == null || Player.instance.IsDead)
        {
            //処理をスキップ
            return;
        }

        //現在のシーン名によって、それぞれの処理を実行する
        ExecutionNowScene();
    }

    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">貫通したオブジェクトのコライダー</param>
    private void OnTriggerEnter(Collider collider)
    {
        //プレイヤー以外が触れた場合||プレイヤーが存在しない場合||プレイヤーが死亡している場合
        if (!collider.gameObject.CompareTag(CommonController.instance.GetPlayerTag()) || Player.instance == null || Player.instance.IsDead)
        {
            //処理をスキップ
            return;
        }

        //現在のシーン名によって、それぞれの処理を実行する
        ExecutionNowScene();
    }

    /// <summary>
    /// 現在のシーン名によって、それぞれの処理を実行する
    /// </summary>
    private void ExecutionNowScene() 
    {
        //現在のシーン名によって処理を変更する
        switch (nowSceneName)
        {
            //HomeSceneの場合
            case stringHomeScene:

                //ステージ1へシーン遷移する
                LoadSceneStage01();
                break;

            //Stage01の場合
            case stringStage01Scene:

                //タイマーを停止する
                Stage01Controller.instance.SetIsTimer(false);

                //デモ版の場合
                if (GameController.instance.GetIsDemoPlayFlag())
                {
                    //デモ版クリアステータス番号を1(通常クリア)にする
                    saveStageClearStatusArray[CommonController.instance.GetDemoStage01SceneName()] = 1;

                    //デモ版ステージ1難易度クリアステータス情報を更新する
                    SettingStageDifficultyLevelClearStatus(saveDemoStage01DifficultyLevelClearStatusArray);

                    //デモ版ステージ1難易度クリア時間情報を更新する
                    SettingDifficultyLevelClearTime(saveDemoStage01DifficultyLevelClearTimeArray);
                }
                //製品版の場合
                else
                {
                    //ステージ1クリアステータス番号を1(通常クリア)にする
                    saveStageClearStatusArray[stringStage01Scene] = 1;

                    //ステージ1難易度クリアステータス情報を更新する
                    SettingStageDifficultyLevelClearStatus(saveStage01DifficultyLevelClearStatusArray);

                    //ステージ1難易度クリア時間情報を更新する
                    SettingDifficultyLevelClearTime(saveStage01DifficultyLevelClearTimeArray);
                }

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //プレイヤーを削除
                Player.instance.DestroyPlayer();

                //画面遷移
                SceneManager.LoadScene(CommonController.instance.GetGameClearSceneName());


                break;

            //Stage02の場合
            case stringStage02Scene:

                //タイマーを停止する
                Stage01Controller.instance.SetIsTimer(false);

                //ステージ2クリアステータス番号を1(通常クリア)にする
                saveStageClearStatusArray[stringStage02Scene] = 1;

                //ステージ2難易度クリアステータス情報を更新する
                SettingStageDifficultyLevelClearStatus(saveStage02DifficultyLevelClearStatusArray);

                //ステージ2難易度クリア時間情報を更新する
                SettingDifficultyLevelClearTime(saveStage02DifficultyLevelClearTimeArray);

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //プレイヤーを削除
                Player.instance.DestroyPlayer();

                //画面遷移
                SceneManager.LoadScene(CommonController.instance.GetGameClearSceneName());

                break;

            //Stage03の場合
            case stringStage03Scene:

                //タイマーを停止する
                Stage01Controller.instance.SetIsTimer(false);

                //ステージ3クリアステータス番号を1(通常クリア)にする
                saveStageClearStatusArray[stringStage03Scene] = 1;

                //ステージ3難易度クリアステータス情報を更新する
                SettingStageDifficultyLevelClearStatus(saveStage03DifficultyLevelClearStatusArray);

                //ステージ3難易度クリア時間情報を更新する
                SettingDifficultyLevelClearTime(saveStage03DifficultyLevelClearTimeArray);

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //プレイヤーを削除
                Player.instance.DestroyPlayer();

                //画面遷移
                SceneManager.LoadScene(CommonController.instance.GetGameClearSceneName());

                break;

            //Stage04の場合
            case stringStage04Scene:

                //タイマーを停止する
                Stage01Controller.instance.SetIsTimer(false);

                //ステージ4クリアステータス番号を1(通常クリア)にする
                saveStageClearStatusArray[stringStage04Scene] = 1;

                //ステージ4難易度クリアステータス情報を更新する
                SettingStageDifficultyLevelClearStatus(saveStage04DifficultyLevelClearStatusArray);

                //ステージ4難易度クリア時間情報を更新する
                SettingDifficultyLevelClearTime(saveStage04DifficultyLevelClearTimeArray);

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //プレイヤーを削除
                Player.instance.DestroyPlayer();

                //画面遷移
                SceneManager.LoadScene(CommonController.instance.GetGameClearSceneName());

                break;
        }
    }

    /// <summary>
    /// Stage01へシーン遷移するための処理
    /// </summary>
    private async void LoadSceneStage01() 
    {
        //プレイヤーライトを持っていない場合
        if (!Player.instance.GetIsHavePlayerLight()) 
        {
            //暗すぎてよく見えない旨のメッセージを表示
            MessageController.instance.ShowInventoryMessage(5);

            await UniTask.Delay(TimeSpan.FromSeconds(2));

            MessageController.instance.ResetMessage();

            //処理をスキップ
            return;
        }

        //メッセージをリセット
        MessageController.instance.ResetMessage();

        //プレイヤー効果音を停止
        MusicController.instance.StopSE(Player.instance.audioSourceSE);

        //ゲームモードステータスをStopInGameに設定
        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

        //ブラックアウトパネルを表示する
        MessageController.instance.SetIsBlackOutPanel(true);
        MessageController.instance.ViewBlackOutPanel();

        //ステージ及び難易度選択パネルを表示にする
        DifficultyLevelController.instance.SetIsViewStageAndDifficultyLevelChoosePanel(true);
        DifficultyLevelController.instance.ChangeViewStageAndDifficultyLevelChoosePanel();

        //マウスカーソルを表示し、固定を解除する
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Stage系のシーンへ遷移する処理
    /// </summary>
    public async void ChangeStageScene() 
    {
        //ステージ及び難易度選択パネルを非表示にする
        DifficultyLevelController.instance.SetIsViewStageAndDifficultyLevelChoosePanel(false);
        DifficultyLevelController.instance.ChangeViewStageAndDifficultyLevelChoosePanel();

        //マウスカーソルを非表示にし、固定する
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //3秒待機
        await UniTask.Delay(TimeSpan.FromSeconds(3));

        //プレイヤーカメラの回転を元に戻す
        PlayerCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        PlayerCamera.instance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        //ゲームモードステータスをInGameに設定
        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

        //保存用シーン名配列インデックス番号を設定
        saveStageSceneNameArrayIndex = stageSceneNameArrayIndex;

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //ステージへ移動
        SceneManager.LoadScene(stageSceneNameArray[stageSceneNameArrayIndex]);
    }

    /// <summary>
    /// クリアしたステージの難易度レベルに応じて、対応するステージ難易度のクリアステータス情報を更新する
    /// </summary>
    /// <param name="targetkeys">該当ステージ難易度情報</param>
    private void SettingStageDifficultyLevelClearStatus(Dictionary<string, int> targetkeys)
    {
        //セーブする難易度ステータスによって、対応するステージクリアステータス情報を更新する
        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus())
        {
            //イージーレベルの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //該当ステージのEasyクリアステータス番号を1(通常クリア)にする
                targetkeys[stringEasyLevel] = 1;
                break;
            //ノーマルレベルの場合
            case DifficultyLevelController.DifficultyLevel.kNormal:

                //該当ステージのNormalクリアステータス番号を1(通常クリア)にする
                targetkeys[stringNormalLevel] = 1;
                break;
            //ナイトメアレベルの場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //該当ステージのNightmareクリアステータス番号を1(通常クリア)にする
                targetkeys[stringNightmareLevel] = 1;
                break;

            default:
                Debug.LogError("不正な難易度ステータスです");
                break;
        }
        ;
    }

    /// <summary>
    /// クリアしたステージの難易度レベルに応じて、対応するステージ難易度のクリア時間情報を更新する
    /// </summary>
    /// <param name="targetkeys">該当ステージ難易度のクリア時間情報</param>
    private void SettingDifficultyLevelClearTime(Dictionary<string, string> targetkeys)
    {
        //TimeSpanのインスタンスを生成。時分は0で良い
        TimeSpan timespan = new TimeSpan(0, 0, (int)Stage01Controller.instance.GetElapsedTime());

        //hh:mm:ss形式に変換（String）
        string clearTime = timespan.ToString(@"hh\:mm\:ss");



        switch (DifficultyLevelController.instance.GetDifficultyLevelStatus())
        {
            //イージーレベルの場合
            case DifficultyLevelController.DifficultyLevel.kEasy:

                //既にクリア時間を保存している場合、
                if (targetkeys[stringEasyLevel] != "--:--:--")
                {
                    //クリア時間の文字列をTimeSpanに変換
                    saveTimeSpan = TimeSpan.Parse(targetkeys[stringEasyLevel]);
                }

                //初クリアの場合||今回のクリア時間の記録が最速の場合
                if (targetkeys[stringEasyLevel] == "--:--:--" || timespan < saveTimeSpan)
                {
                    //該当ステージのEasyクリア時間を更新する
                    targetkeys[stringEasyLevel] = clearTime;
                }
                break;

            //ノーマルレベルの場合
            case DifficultyLevelController.DifficultyLevel.kNormal:

                //既にクリア時間を保存している場合、
                if (targetkeys[stringNormalLevel] != "--:--:--")
                {
                    //クリア時間の文字列をTimeSpanに変換
                    saveTimeSpan = TimeSpan.Parse(targetkeys[stringNormalLevel]);
                }

                //初クリアの場合||今回のクリア時間の記録が最速の場合
                if (targetkeys[stringNormalLevel] == "--:--:--" || timespan < saveTimeSpan)
                {
                    //該当ステージのNormalクリア時間を更新する
                    targetkeys[stringNormalLevel] = clearTime;
                }

                break;

            //ナイトメアレベルの場合
            case DifficultyLevelController.DifficultyLevel.kNightmare:

                //既にクリア時間を保存している場合、
                if (targetkeys[stringNightmareLevel] != "--:--:--")
                {
                    //クリア時間の文字列をTimeSpanに変換
                    saveTimeSpan = TimeSpan.Parse(targetkeys[stringNightmareLevel]);
                }

                //初クリアの場合||今回のクリア時間の記録が最速の場合
                if (targetkeys[stringNightmareLevel] == "--:--:--" || timespan < saveTimeSpan)
                {
                    //該当ステージのNightmareクリア時間を更新する
                    targetkeys[stringNightmareLevel] = clearTime;
                }

                break;

            default:
                Debug.LogError("不正な難易度ステータスのため、クリア時間を更新できません");
                break;
        };
    }
}
