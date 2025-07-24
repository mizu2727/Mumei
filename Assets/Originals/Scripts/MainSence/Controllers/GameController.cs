using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("ゲームモードのステータス")]
    public GameModeStatus gameModeStatus;


    [Header("チュートリアル用ドキュメント")]
    [SerializeField] public GameObject tutorialDocument;

    [Header("チュートリアル用ミステリーアイテム関連")]
    [SerializeField] public GameObject tutorialMysteryItem01;
    [SerializeField] public GameObject tutorialMysteryItem02;

    [Header("チュートリアル用アイテム親オブジェクト")]
    [SerializeField] public GameObject tutorialItems;

    //[Header("チュートリアル用ゴール")]
    //[SerializeField] public GameObject tutorialGoal;


    public enum  GameModeStatus
    {
        Story,
        PlayInGame,
        StopInGame,
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        Time.timeScale = 1;
    }

    //ゲームモードのステータスを設定
    public void SetGameModeStatus(GameModeStatus status) 
    {
        gameModeStatus = status;

        if (gameModeStatus == GameModeStatus.Story) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
