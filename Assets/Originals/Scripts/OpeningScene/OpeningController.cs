using UnityEngine;
using static GameController;

/// <summary>
/// OpeningSceneで使用する管理クラス
/// </summary>
public class OpeningController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static OpeningController instance;


    private void OnDestroy() 
    {
        //インスタンスが存在する場合
        if (instance != null)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }

    void Awake()
    {
        //インスタンスがnullの場合
        if (instance == null)
        {
            //インスタンス生成
            instance = this;
        }
        else
        {
            //ゲームオブジェクト破棄
            Destroy(gameObject);
        }

        //シーンステータスをkOpeningSceneに設定
        GameController.instance.SetViewScene(ViewScene.kOpeningScene);

        //ゲームモードステータスをStopInGameに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
    }
}
