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

    void Awake()
    {
        //シーンステータスをkOpeningSceneに設定
        GameController.instance.SetViewScene(ViewScene.kOpeningScene);

        //ゲームモードステータスをStopInGameに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
    }
}
