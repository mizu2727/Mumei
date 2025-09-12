using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static UnityEngine.Rendering.DebugUI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("メッセージパネル関連(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;
    [SerializeField] private GameObject CheckInputNamePanel;
    [SerializeField] private Text CheckInputNameText;

    [Header("ブラックアウト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject blackOutPanel;

    [Header("メッセージを書くスピード。数値が小さいほど素早く書く")]
    [SerializeField] private float writeSpeed = 0;
    private bool isWrite = false;//書いてる途中であるかを判定

    [Header("システムメッセージ(Prefabをアタッチ)")]
    [SerializeField] private SystemMessage systemMessage;

    [Header("システムメッセージ表示機能(Prefabをアタッチ)")]
    [SerializeField] private ShowSystemMessage showSystemMessage;

    [Header("会話メッセージ(Prefabをアタッチ)")]
    [SerializeField] private TalkMessage talkMessage;

    [Header("会話メッセージ表示機能(Prefabをアタッチ)")]
    [SerializeField] private ShowTalkMessage showTalkMessage;

    [Header("ゴールメッセージ(Prefabをアタッチ)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("プレイヤーの名前入力(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public InputField inputPlayerNameField;

    [Header("ゴール(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public Goal goal;

    [Header("インベントリメッセージ(Prefabをアタッチ)")]
    [SerializeField] private InventoryMessage inventoryMessage;

    [Header("メッセージパネル判定")]
    public bool isMessagePanel = false;

    [Header("ブラックアウト判定")]
    public bool isBlackOutPanel = false;


    [Header("サウンド関連")]
    [SerializeField] private AudioClip noiseSE;
    private AudioSource audioSourceSE;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        ResetMessage();

        
        if(inputPlayerNameField != null) 
        {
            inputPlayerNameField = inputPlayerNameField.GetComponent<InputField>();
            inputPlayerNameField.gameObject.SetActive(false);
        }

        if (CheckInputNamePanel != null) 
        {
            CheckInputNamePanel.SetActive(false);
        }

        if (CheckInputNameText != null) 
        {
            CheckInputNameText.text = "";
        }


        isBlackOutPanel = false;
    }

    private void Start()
    {
        //MusicControllerのAwake関数の処理後に呼ばれるようにするため、
        //Start関数内でAudioSourceを取得する
        audioSourceSE = MusicController.Instance.GetAudioSource();
    }

    //メッセージパネルの表示・非表示
    public void ViewMessagePanel()
    {
        if (isMessagePanel)
        {
            messagePanel.SetActive(true);
        }
        else
        {
            messagePanel.SetActive(false);
        }
    }

    //ブラックアウトパネルの表示・非表示
    public void ViewBlackOutPanel()
    {
        if (isBlackOutPanel)
        {
            blackOutPanel.SetActive(true);
        }
        else
        {
            blackOutPanel.SetActive(false);
        }
    }

    //メッセージをリセット
    public void ResetMessage()
    {
        //Debug.Log("goal.isGoalPanel =" + goal.isGoalPanel);
        messageText.color = Color.white;
        messageText.text = "";
        isMessagePanel = false;
        ViewMessagePanel();
    }

    //テキストを一文字ずつ表示
    async void Write(string s)
    {
        // 既に書き込み中の場合は、何もしない
        if (isWrite) return;

        isWrite = true;
        messageText.text = ""; // 毎回テキストをクリアしてから書き始める

        for (int i = 0; i < s.Length; i++)
        {
            // 書き込み速度が0の場合、一気に表示
            if (writeSpeed <= 0)
            {
                messageText.text = s;
                break;
            }

            messageText.text += s.Substring(i, 1);
            await UniTask.Delay(TimeSpan.FromSeconds(writeSpeed));
        }
        isWrite = false;
    }

    //スペースキー押下で次のメッセージを表示
    async UniTask ShowNextMessage() 
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        //ポーズパネル・ゴールパネルを開いている間は、次のメッセージを進めない
        if (goal != null && PauseController.instance != null) 
        {
            if (PauseController.instance.isPause || goal.isGoalPanel)
            {
                await ShowNextMessage();
            }
        }     
    }



    //会話メッセージを表示
    public async UniTask ShowTalkMessage(int number)
    {
        // 既にメッセージを書いてる途中である場合は、以降の処理を中断
        if (isWrite)
        {
            writeSpeed = 0; // 書き込み速度を上げて高速表示
            return;
        }

        //前のメッセージが書いてる途中であるかを判断。書き途中ならtrue
        if (Time.timeScale == 1)
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //話している人がカナメの場合、文章の色をシアン色に設定
            if (talkMessage.talkMessage[number].speakerName == "カナメ") messageText.color = Color.cyan;
            else messageText.color = Color.white;
            


            //エクセルデータ型.リスト型[番号].カラム名
            Write(talkMessage.talkMessage[number].message);

            await ShowNextMessage();

            switch (number)
            {
                case 2:
                    messageText.text = "";
                    number++;

                    Player.instance.playerIsBackRotate = true;

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //スペースキー押下で次のメッセージを書く
                    showTalkMessage.ShowGameTalkMessage(number);
                    //Debug.Log("Player.instance.playerName" + Player.instance.playerName);

                    break;

                case 11:
                case 19:
                case 60:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    MusicController.Instance.PauseBGM();

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // 色を赤色に設定
                    messageText.color = Color.red;

                    Write(talkMessage.talkMessage[number].message);

                    await ShowNextMessage();


                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    MusicController.Instance.UnPauseBGM();

                    messageText.text = "";
                    number++;

                    showTalkMessage.ShowGameTalkMessage(number);
                    break;

                case 27:
                case 33:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    //スペースキー押下で次のメッセージを書く
                    showTalkMessage.ShowGameTalkMessage(number);
                    break;


                //会話終了してチュートリアルに入る
                case 36:
                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    Kaname.instance.WarpPostion(1, 0.505f, 7);

                    ////チュートリアル用ドキュメントを表示
                    GameController.instance.tutorialDocument.SetActive(true);

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    //システムメッセージ
                    showSystemMessage.ShowGameSystemMessage(9);

                    break;

                case 42:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    showSystemMessage.ShowGameSystemMessage(10);


                    break;

                case 43:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    showSystemMessage.ShowGameSystemMessage(11);


                    break;

                //チュートリアルミステリーアイテムを表示
                case 45:
                    ResetMessage();

                    GameController.instance.tutorialMysteryItem01.SetActive(true);
                    GameController.instance.tutorialMysteryItem02.SetActive(true);

                    goal.OnTutorial();

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    showSystemMessage.ShowGameSystemMessage(12);

                    break;

                case 46:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    showSystemMessage.ShowGameSystemMessage(13);


                    break;

                
                case 48:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    showSystemMessage.ShowGameSystemMessage(14);

                    break;

                case 68:
                    ResetMessage();

                    Debug.Log("チュートリアル会話終了");

                    GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                    //ステージ1へ移動
                    SceneManager.LoadScene("Stage01");

                    break;

                case 70:
                    messageText.text = "";
                    number++;

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    //不穏なBGMを流す
                    //MusicController.Instance.PlayBGM();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    //スペースキー押下で次のメッセージを書く
                    showTalkMessage.ShowGameTalkMessage(number);

                    break;

                case 86:
                    messageText.text = "";
                    number++;

                    //不穏なBGMを止める
                    //MusicController.Instance.StopBGM();

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // 色を赤色に設定
                    messageText.color = Color.red;

                    Write(talkMessage.talkMessage[number].message);

                    await ShowNextMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    ResetMessage();


                    Debug.Log(showTalkMessage);
                    GameClearController.instance.ViewGameClearUI();

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    break;



                //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
                default:
                    messageText.text = "";
                    number++;

                    //スペースキー押下で次のメッセージを書く
                    showTalkMessage.ShowGameTalkMessage(number);
                    break;
                }
        }
    }


    //システムメッセージを表示
    public async UniTask ShowSystemMessage(int number)
    {
        // 既にメッセージを書いてる途中である場合は、以降の処理を中断
        if (isWrite)
        {
            writeSpeed = 0; // 書き込み速度を上げて高速表示
            return;
        }

        //前のメッセージが書いてる途中であるかを判断。書き途中ならtrue
        if (Time.timeScale == 1)
        {

            if (number == 15 && goal.isTutorial) 
            {
                goal.OffTutorial();
            } 

            isMessagePanel = true;
            ViewMessagePanel();

            //エクセルデータ型.リスト型[番号].カラム名
            Write(systemMessage.systemMessage[number].message);

            await ShowNextMessage();


            switch (number)
            {
                //名前入力UIを表示
                case 3:
                    ResetMessage();

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;

                    inputPlayerNameField.gameObject.SetActive(true);

                    break;

                //名前入力制限に引っかかった後に、もう一度名前入力UIを表示
                case 4:
                    ResetMessage();
                    showSystemMessage.ShowGameSystemMessage(3);

                    break;

                case 6:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    MusicController.Instance.PlayMomentAudioSE(audioSourceSE, noiseSE);

                    // 色を赤色に設定
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await ShowNextMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //ホームへ移動
                    SceneManager.LoadScene("HomeScene");
                    break;

                //会話メッセージを表示
                case 9:
                    ResetMessage();

                    showTalkMessage.ShowGameTalkMessage(38);

                    break;

                case 10:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(43);

                    break;

                case 11:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(44);

                    break;

                case 12:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(46);

                    break;

                case 13:
                    ResetMessage();

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(47);

                    break;

                //チュートリアル終了
                case 14:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    isBlackOutPanel = true;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    Kaname.instance.WarpPostion(1, 0.505f, 2);

                    Player.instance.PlayerWarp(1, 0.562f, 0);

                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    // カメラの角度をリセット
                    if (Player.instance != null)
                    {
                        PlayerCamera playerCamera = Player.instance.GetComponent<PlayerCamera>();
                        if (playerCamera != null)
                        {
                            // カメラの上下回転をリセット
                            playerCamera.ResetCameraRotation();
                            // プレイヤーの向きをカメラの正面に同期（必要に応じて）
                            Player.instance.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        }
                    }

                    GameController.instance.tutorialItems.SetActive(false);

                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showSystemMessage.ShowGameSystemMessage(number);

                    break;


                case 15:
                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showTalkMessage.ShowGameTalkMessage(49);

                    break;


                //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
                default:
                    messageText.text = "";
                    number++;

                    //スペースキー押下で次のメッセージを書く
                    showSystemMessage.ShowGameSystemMessage(number);
                    break;
            }
        }
    }

    //ゴールメッセージを表示
    public void ShowGoalMessage(int number) 
    {
        Debug.Log("ゴールメッセージスタート");
        messageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }

    /// <summary>
    /// インベントリメッセージを表示
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    public void ShowInventoryMessage(int number) 
    {
        Debug.Log("インベントリメッセージスタート"); 
        messageText.text = inventoryMessage.inventoryMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }

    /// <summary>
    /// プレイヤーの名前の入力が完了した際に呼ばれる
    /// </summary>
    /// <param name="playerName">入力したプレイヤー名</param>
    public void SavePlayerName(string playerName)
    {
        if ((1 < playerName.Length) && (playerName.Length < 11)) 
        {
            //名前を一時的に保存
            inputPlayerNameField.text = playerName;

            //確認用テキストに入力した名前を表示
            CheckInputNameText.text = inputPlayerNameField.text + " でよろしいですか？";

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
