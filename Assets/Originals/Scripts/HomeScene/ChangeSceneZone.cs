using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;
using static GameController;

public class ChangeSceneZone : MonoBehaviour
{
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


    void Start()
    {
        //現在のシーン名を取得
        nowSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// オブジェクトのコリジョンと衝突した場合の処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトのコリジョン</param>
    private async void OnCollisionEnter(Collision collision)
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

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                //ゲームモードステータスをStopInGameに設定
                GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

                //ブラックアウトパネルを表示する
                MessageController.instance.SetIsBlackOutPanel(true);
                MessageController.instance.ViewBlackOutPanel();

                //3秒待機
                await UniTask.Delay(TimeSpan.FromSeconds(3));

                //プレイヤーカメラの回転を元に戻す
                PlayerCamera.instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                PlayerCamera.instance.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                //プレイヤーの回転を元に戻す
                Player.instance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                //シーン遷移時用データを保存
                GameController.instance.CallSaveSceneTransitionUserDataMethod();

                //ステージ1へ移動
                SceneManager.LoadScene(stringStage01Scene);
                break;
        }
    }

    /// <summary>
    /// オブジェクトのコライダーを貫通した場合の処理
    /// </summary>
    /// <param name="collider">貫通したオブジェクトのコライダー</param>
    private async void OnTriggerEnter(Collider collider)
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

                //プレイヤー効果音を停止
                MusicController.instance.StopSE(Player.instance.audioSourceSE);

                //ゲームモードステータスをStopInGameに設定
                GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

                //ブラックアウトパネルを表示する
                MessageController.instance.SetIsBlackOutPanel(true);
                MessageController.instance.ViewBlackOutPanel();

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
                break;
        }
    }
}
