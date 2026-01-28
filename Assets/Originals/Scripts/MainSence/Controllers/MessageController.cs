using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// メッセージ管理クラス
/// </summary>
public class MessageController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static MessageController instance;


    /// <summary>
    /// "Interact"
    /// </summary>
    private const string stringInteract = "Interact";


    [Header("メッセージパネル関連")]
    [Header("メッセージパネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject messagePanel;

    [Header("メッセージテキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public Text messageText;


    [Header("会話している人の名前を表示するパネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject speakerNamePanel;

    [Header("会話している人の名前を表示するテキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Text speakerNameText;


    [Header("名前確認パネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject CheckInputNamePanel;

    [Header("名前確認テキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Text CheckInputNameText;

    [Header("メッセージを書くスピード。数値が小さいほど素早く書く")]
    [SerializeField] private float writeSpeed = 0;

    /// <summary>
    /// メッセージ書き途中フラグ
    /// </summary>
    private bool isWrite = false;

    [Header("システムメッセージ(Prefabをアタッチ)")]
    [SerializeField] private SystemMessage systemMessage;

    [Header("システムメッセージ表示機能(Prefabをアタッチ)")]
    [SerializeField] private ShowSystemMessage showSystemMessage;

    [Header("会話メッセージ(Prefabをアタッチ)")]
    [SerializeField] private TalkMessage talkMessage;

    /// <summary>
    /// 2番目のメッセージ番号
    /// </summary>
    private const int kTakeMessageNumber2 = 2;

    /// <summary>
    /// "カナメ"
    /// </summary>
    private const string kSpeakerNameKanane = "カナメ";

    [Header("会話メッセージ表示機能(Prefabをアタッチ)")]
    [SerializeField] private ShowTalkMessage showTalkMessage;

    [Header("ゴールメッセージ(Prefabをアタッチ)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("プレイヤーの名前入力(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public InputField inputPlayerNameField;

    [Header("プレイヤーカメラ(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private PlayerCamera playerCamera;

    /// <summary>
    /// プレイヤーカメラのTransform保存用
    /// </summary>
    private Quaternion savePlayerCameraQuaternion;

    [Header("ゴール(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public Goal goal;


    [Header("インベントリメッセージ(Prefabをアタッチ)")]
    [SerializeField] private InventoryMessage inventoryMessage;

    [Header("メッセージパネルフラグ(ヒエラルキー上からの編集禁止)")]
    public bool isMessagePanel = false;

    /// <summary>
    /// 会話している人の名前を表示するフラグ
    /// </summary>
    private bool isViewSpeakerNamePanel = false;

    /// <summary>
    /// メッセージテキストの色を赤色に変更するフラグ
    /// </summary>
    private bool isChangeMessageTextColorRed = false;

    [Header("ブラックアウトフラグ(ヒエラルキー上からの編集禁止)")]
    public bool isBlackOutPanel = false;

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// noiseSE用audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ノイズ音のID
    /// </summary>
    private readonly int noiseSEid = 9;


    /// <summary>
    /// 非同期タスクのキャンセル用変数
    /// チュートリアル内のUniTask処理待機中にポーズ画面からタイトルへ戻る際のmessageTextでMissingReferenceExceptionエラーが起こるのを防止する用
    /// </summary>
    private CancellationTokenSource cts; 

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;

        //非同期タスクをキャンセル
        CancelAsyncTasks(); 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //messageTextを設定
        if (messageText == null)
        {
            Debug.LogWarning("MessageControllerのmessageTextがnullのため、messageText = messageTextを実行");
            messageText = GameController.instance.messageText;
        }

        if (messageText != null) messageText = GameController.instance.messageText;
        else Debug.LogError("GameControllerのmessageTextが設定されていません");

        if (messageText == null)
        {
            Debug.LogError("MessageControllerのmessageTextがnull");
        }

        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    /// <summary>
    /// トークンをキャンセルして非同期タスクを中断
    /// </summary>
    public void CancelAsyncTasks()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = new CancellationTokenSource();
        }
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyController();
        }

        //CancellationTokenSourceを初期化
        cts = new CancellationTokenSource();

        //メッセージリセット
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

        //メッセージテキストの色を赤色に変更するフラグを初期化
        isChangeMessageTextColorRed = false;

        //ブラックアウトパネル非表示
        isBlackOutPanel = false;
    }

    private void Start()
    {
        //MusicControllerのAwake関数の処理後に呼ばれるようにするため、
        //Start関数内でAudioSourceを取得する

        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// メッセージパネルの表示・非表示
    /// </summary>
    public void ViewMessagePanel()
    {
        if (isMessagePanel)
        {
            //表示
            messagePanel.SetActive(true);
        }
        else
        {
            //非表示
            messagePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 会話している人の名前を表示するパネルの表示・非表示
    /// </summary>
    public void ViewSpeakerNamePanel()
    {
        //speakerNamePanelがnullの場合
        if (speakerNamePanel == null) 
        {
            //処理をスキップ
            return;
        }

        if (isViewSpeakerNamePanel)
        {
            //表示
            speakerNamePanel.SetActive(true);
        }
        else
        {
            //非表示
            speakerNamePanel.SetActive(false);
        }
    }

    /// <summary>
    /// ブラックアウトパネルの表示・非表示
    /// </summary>
    public void ViewBlackOutPanel()
    {
        if (isBlackOutPanel)
        {
            //表示
            GameController.instance.blackOutPanel.SetActive(true);
        }
        else
        {
            //非表示
            GameController.instance.blackOutPanel.SetActive(false);
        }
    }

    /// <summary>
    /// メッセージをリセット
    /// </summary>
    public void ResetMessage()
    {
        //メッセージテキストをリセット
        messageText.color = Color.white;
        messageText.text = "";

        //メッセージパネルを非表示
        isMessagePanel = false;
        ViewMessagePanel();


        //speakerNameTextが存在する場合
        if (speakerNameText != null)
        {
            //会話している人の名前をリセット
            speakerNameText.color = Color.white;
            speakerNameText.text = "";
        }

        //会話している人の名前パネルを非表示
        isViewSpeakerNamePanel = false;
        ViewSpeakerNamePanel();
    }

    /// <summary>
    /// テキストを一文字ずつ表示
    /// </summary>
    /// <param name="s">テキスト</param>
    async void Write(string s)
    {
        //既に書き込み中の場合は、何もしない
        if (isWrite) return;

        isWrite = true;

        //毎回テキストをクリアしてから書き始める
        messageText.text = ""; 

        for (int i = 0; i < s.Length; i++)
        {
            //書き込み速度が0の場合、一気に表示
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


    /// <summary>
    /// スペースキー・Enterキー・左クリック・Rボタン押下で次のメッセージを表示
    /// Interact…"joystick button 5"を割り当てている。コントローラーではRボタンになる
    /// </summary>
    /// <returns></returns>
    async UniTask ShowNextMessage() 
    {
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) 
            || Input.GetMouseButtonDown(0) || Input.GetButtonDown(stringInteract));

        //ポーズパネル・ゴールパネルを開いている間は、次のメッセージを進めない
        if (goal != null && PauseController.instance != null) 
        {
            if (PauseController.instance.isPause || goal.isGoalPanel)
            {
                await ShowNextMessage();
            }
        }     
    }

    /// <summary>
    /// メッセージ系の色を変更する
    /// </summary>
    private void ChangeTextColor(int num) 
    {
        //話している人がカナメの場合||talkMessageの番号が2の場合
        if (talkMessage.talkMessage[num].speakerName == kSpeakerNameKanane
            || talkMessage.talkMessage[num].number == kTakeMessageNumber2)
        {
            //メッセージテキストの色を赤色に変更するフラグがfalseの場合
            if (!isChangeMessageTextColorRed) 
            {
                //文章の色をシアン色に設定
                messageText.color = Color.cyan;
            }

            //speakerNameTextが存在する場合
            if (speakerNameText != null)
            {
                //会話している人の名前の表示をシアン色に設定
                speakerNameText.color = Color.cyan;
            }
        }
        else
        {
            //メッセージテキストの色を赤色に変更するフラグがfalseの場合
            if (!isChangeMessageTextColorRed)
            {
                //文章の色を白色に設定
                messageText.color = Color.white;
            }

            //speakerNameTextが存在する場合
            if (speakerNameText != null)
            {
                //会話している人の名前の表示を白色に設定
                speakerNameText.color = Color.white;
            }
        }
    }


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

            //エクセルデータ型.リスト型[番号].カラム名
            Write(talkMessage.talkMessage[number].message);


            //会話している人の名前を表示
            isViewSpeakerNamePanel = true;
            ViewSpeakerNamePanel();

            //speakerNameTextが存在する場合
            if (speakerNameText != null) 
            {
                //会話している人の名前を設定
                speakerNameText.text = talkMessage.talkMessage[number].speakerName;
            }
            
            //チュートリアルの壁を消してゴールオブジェクトが見えるようにする
            if (HomeController.instance.wall_Tutorial != null && number == 47) HomeController.instance.wall_Tutorial.SetActive(false);

            await ShowNextMessage();

                switch (number)
                {
                    case 2:
                        //メッセージテキストと会話している人の名前テキストをリセット
                        messageText.text = "";
                        speakerNameText.text = "";

                        //次のメッセージ番号へ
                        number++;

                        //後ろを振り返る
                        Player.instance.playerIsBackRotate = true;

                        await UniTask.Delay(TimeSpan.FromSeconds(3));

                        //プレイヤーカメラの回転を保存
                        savePlayerCameraQuaternion = playerCamera.transform.rotation;

                        //スペースキー押下で次のメッセージを書く
                        showTalkMessage.ShowGameTalkMessage(number);
                        break;

                    case 11:
                    case 19:
                    case 60:
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

                        Write(talkMessage.talkMessage[number].message);

                        //speakerNameTextが存在する場合
                        if (speakerNameText != null)
                        {
                            //会話している人の名前を設定
                            speakerNameText.text = talkMessage.talkMessage[number].speakerName;
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

                        //画面をブラックアウト
                        isBlackOutPanel = true;
                        ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        //カナメをワープ
                        Kaname.instance.WarpPostion(1, 0.505f, 7);

                        ////チュートリアル用ドキュメントを表示
                        GameController.instance.tutorialDocument.SetActive(true);

                        //画面ブラックアウトを解除
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

                        //チュートリアル用アイテムを表示
                        GameController.instance.tutorialMysteryItem01.SetActive(true);
                        GameController.instance.tutorialMysteryItem02.SetActive(true);

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

                        //プレイヤーカメラの回転を元に戻す
                        playerCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        playerCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

                        
                        GameController.instance.SetGameModeStatus(GameModeStatus.PlayInGame);

                        //シーン遷移時用データを保存
                        GameController.instance.CallSaveSceneTransitionUserDataMethod();

                        //ステージ1へ移動
                        SceneManager.LoadScene("Stage01");
                        break;

                    case 70:
                        messageText.text = "";
                        number++;

                        //画面ブラックアウトを解除
                        isBlackOutPanel = false;
                        ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        showTalkMessage.ShowGameTalkMessage(number);
                        break;

                    case 86:
                        messageText.text = "";
                        number++;

                    //BGMを止める
                    MusicController.instance.StopBGM(GameClearController.instance.GetAudioSourceBGM(),
                        sO_BGM.GetBGMClip(GameClearController.instance.GetGameClearSceneBGMId()), GameClearController.instance.GetGameClearSceneBGMId());

                    //画面ブラックアウト
                    isBlackOutPanel = true;
                            ViewBlackOutPanel();

                        await UniTask.Delay(TimeSpan.FromSeconds(1));

                        //ノイズを流す
                        audioSourceSE.clip = sO_SE.GetSEClip(noiseSEid);
                        MusicController.instance.PlayMomentAudioSE(audioSourceSE, audioSourceSE.clip);

                        //文字の色を赤色に設定
                        messageText.color = Color.red;

                        Write(talkMessage.talkMessage[number].message);

                        await ShowNextMessage();

                        await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                        ResetMessage();

                        //ゲームクリアパネルを表示
                        GameClearController.instance.ViewGameClearUI();

                        //画面ブラックアウトを解除
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

            //エクセルデータ型.リスト型[番号].カラム名
            Write(systemMessage.systemMessage[number].message);

            switch (number)
            {
                //名前入力UIを表示
                case 3:
                    await ShowNextMessage();

                    ResetMessage();

                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;

                    inputPlayerNameField.gameObject.SetActive(true);
                    break;

                //名前入力制限に引っかかった後に、もう一度名前入力UIを表示
                case 4:
                    await ShowNextMessage();

                    ResetMessage();
                    showSystemMessage.ShowGameSystemMessage(3);
                    break;

                case 6:
                    await ShowNextMessage();

                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    //ノイズを流す
                    audioSourceSE.clip = sO_SE.GetSEClip(noiseSEid);
                    MusicController.instance.PlayMomentAudioSE(audioSourceSE, audioSourceSE.clip);

                    //文字の色を赤色に設定
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await ShowNextMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    //シーン遷移時用データを保存
                    GameController.instance.CallSaveSceneTransitionUserDataMethod();

                    //HomeSceneへ移動
                    SceneManager.LoadScene("HomeScene");
                    break;

                //会話メッセージを表示
                case 9:
                    await ShowNextMessage();

                    ResetMessage();

                    showTalkMessage.ShowGameTalkMessage(38);
                    break;

                case 10:
                    //ドキュメント(チュートリアル版)入手したらメッセージを勧める
                    await UniTask.WaitUntil(() => GameController.instance.GetIsTutorialNextMessageFlag());
                    GameController.instance.SetIsTutorialNextMessageFlag(false);

                    //プレイヤー効果音を停止
                    MusicController.instance.StopSE(Player.instance.audioSourceSE);

                    //左クリック…ドキュメント入手操作の説明
                    ResetMessage();

                    //ストーリーモードへ変更
                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(43);
                    break;

                case 11:
                    //ドキュメント(チュートリアル版)を閲覧後にポーズ解除したらメッセージを勧める
                    await UniTask.WaitUntil(() => GameController.instance.GetIsTutorialNextMessageFlag() && !PauseController.instance.isPause 
                        && !PauseController.instance.isViewItemsPanel && !OptionUIController.instance.GetIsOptionPanel());
                    GameController.instance.SetIsTutorialNextMessageFlag(false);

                    ResetMessage();

                    //ストーリーモードへ変更
                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(44);
                    break;

                case 12:
                    //ミステリーアイテム(チュートリアル版)入手したらメッセージを勧める
                    await UniTask.WaitUntil(() => PauseController.instance.isGetHammer_Tutorial && PauseController.instance.isGetRope_Tutorial);

                    //プレイヤー効果音を停止
                    MusicController.instance.StopSE(Player.instance.audioSourceSE);

                    ResetMessage();


                    //ストーリーモードへ変更
                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(46);
                    break;

                case 13:
                    //ミステリーアイテム(チュートリアル版)を閲覧後にポーズ解除したらメッセージを勧める
                    await UniTask.WaitUntil(() => !PauseController.instance.isPause && !PauseController.instance.isViewItemsPanel 
                        && PauseController.instance.isViewMysteryItem_Tutorial && !OptionUIController.instance.GetIsOptionPanel());
                    PauseController.instance.isViewMysteryItem_Tutorial = false;
                    ResetMessage();

                    //ストーリーモードへ変更
                    GameController.instance.SetGameModeStatus(GameModeStatus.Story);

                    showTalkMessage.ShowGameTalkMessage(47);
                    break;

                //チュートリアル終了
                case 14:
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

                    //壁を表示
                    HomeController.instance.wall_Tutorial.SetActive(true);

                    //プレイヤー・カナメをワープ
                    Kaname.instance.WarpPostion(1, 0.505f, 2);
                    Player.instance.PlayerWarp(1, 0.562f, 0);

                    //カメラの角度をリセット
                    if (Player.instance != null)
                    {
                        //プレイヤーカメラが存在する場合
                        if (playerCamera != null)
                        {
                            // カメラの上下回転をリセット
                            playerCamera.ResetCameraRotation();

                            //プレイヤーカメラのtransform.ratationの値を保存していた値に戻す
                            playerCamera.transform.rotation = savePlayerCameraQuaternion;

                            // プレイヤーの向きをカメラの正面に同期（必要に応じて）
                            Player.instance.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        }
                    }

                    //チュートリアル用アイテムを非表示
                    GameController.instance.tutorialItems.SetActive(false);

                    //画面ブラックアウトを解除
                    isBlackOutPanel = false;
                    ViewBlackOutPanel();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showSystemMessage.ShowGameSystemMessage(number);
                    break;

                case 15:
                    await ShowNextMessage();

                    ResetMessage();

                    await UniTask.Delay(TimeSpan.FromSeconds(0.5));

                    showTalkMessage.ShowGameTalkMessage(49);
                    break;


                //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
                default:
                    await ShowNextMessage();

                    messageText.text = "";
                    number++;

                    //スペースキー押下で次のメッセージを書く
                    showSystemMessage.ShowGameSystemMessage(number);
                    break;
            }
        }
    }

    /// <summary>
    /// ゴールメッセージを表示
    /// </summary>
    /// <param name="number">メッセージ番号</param>
    public void ShowGoalMessage(int number) 
    {
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
        //2文字以上10文字以下であるか
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

    /// <summary>
    /// このコントローラーを破棄する
    /// </summary>
    public void DestroyController()
    {
        CancelAsyncTasks();
        messageText.text = "";
        Destroy(gameObject);
    }

    /// <summary>
    /// オブジェクトが破棄される際に呼ばれる
    /// </summary>
    private void OnDestroy()
    {
        //メッセージテキストが存在する場合
        if (messageText != null) 
        {
            //メッセージテキストをnullにする(メモリリークを防ぐため)
            messageText = null;
        }

        //メッセージパネルが存在する場合
        if (messagePanel != null) 
        {
            //メッセージパネルをnullにする(メモリリークを防ぐため)
            messagePanel = null; 
        }

        //会話している人の名前パネルが存在する場合
        if (speakerNameText != null) 
        {
            //会話している人の名前テキストをnullにする(メモリリークを防ぐため)
            speakerNameText = null;
        }

        //会話している人の名前パネルが存在する場合
        if (speakerNamePanel != null) 
        {
            //会話している人の名前パネルをnullにする(メモリリークを防ぐため)
            speakerNamePanel = null;
        }

        //名前入力確認パネルが存在する場合
        if (CheckInputNameText != null) 
        {
            //確認用テキストをnullにする(メモリリークを防ぐため)
            CheckInputNameText = null;
        }

        //名前入力確認パネルが存在する場合
        if (CheckInputNamePanel != null) 
        {
            //名前入力確認パネルをnullにする(メモリリークを防ぐため)
            CheckInputNamePanel = null;
        }

        //名前入力フィールドが存在する場合
        if (inputPlayerNameField != null) 
        {
            //名前入力フィールドをnullにする(メモリリークを防ぐため)
            inputPlayerNameField = null;
        }

        //チュートリアル用のゴールオブジェクトが存在する場合
        if (goal != null) 
        {
            //チュートリアル用のゴールオブジェクトをnullにする(メモリリークを防ぐため)
            goal = null;
        }
            

        //もしこのインスタンスがシングルトンインスタンス自身であれば、staticな参照をクリアする
        if (instance == this)
        {
            instance = null;
        }
    }
}
