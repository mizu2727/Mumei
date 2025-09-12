using UnityEngine;
using static GameController;

public class HomeController : MonoBehaviour
{
    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    [Header("サウンド関連")]
    public AudioSource audioSourceBGM;
    private readonly int homeSceneBGMid = 1; // ホームシーンBGMのID

    void Awake()
    {
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();

        //audioSourceBGMを設定TODO(2番目の要素に追加する？)
        //audioSourceBGM = MusicController.Instance.GetAudioSource();

        //ホームシーンBGMを再生TODO
        //MusicController.Instance.PlayBGM(homeSceneBGMid);
    }
}
