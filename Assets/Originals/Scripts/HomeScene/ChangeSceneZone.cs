using Cysharp.Threading.Tasks;
using System;
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
    /// HomeScene
    /// </summary>
    private const string stringHomeScene = "HomeScene";

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stringStage01Scene = "Stage01";


    /// <summary>
    /// "Player"タグ
    /// </summary>
    private const string playerTag = "Player";


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
        if (!collision.gameObject.CompareTag(playerTag) || Player.instance == null || Player.instance.IsDead)
        {
            //処理をスキップ
            return;
        }

        //現在のシーン名によって処理を変更する
        switch (nowSceneName) 
        {
            //HomeSceneの場合
            case stringHomeScene:

                //ステージ1へシーン遷移する
                LoadSceneStage01();
                break;
        }
    }

    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">貫通したオブジェクトのコライダー</param>
    private void OnTriggerEnter(Collider collider)
    {
        //プレイヤー以外が触れた場合||プレイヤーが存在しない場合||プレイヤーが死亡している場合
        if (!collider.gameObject.CompareTag(playerTag) || Player.instance == null || Player.instance.IsDead)
        {
            //処理をスキップ
            return;
        }

        //現在のシーン名によって処理を変更する
        switch (nowSceneName)
        {
            //HomeSceneの場合
            case stringHomeScene:

                //ステージ1へシーン遷移する
                LoadSceneStage01();
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

        //難易度選択パネルを表示する
        DifficultyLevelController.instance.GetDifficultyLevelChoosePanel().SetActive(true);

        //マウスカーソルを表示し、固定を解除する
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Stage系のシーンへ遷移する処理
    /// </summary>
    public async void ChangeStageScene() 
    {
        //難易度選択パネルを非表示にする
        DifficultyLevelController.instance.GetDifficultyLevelChoosePanel().SetActive(false);

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

        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //ステージ1へ移動
        SceneManager.LoadScene(stringStage01Scene);
    }
}
