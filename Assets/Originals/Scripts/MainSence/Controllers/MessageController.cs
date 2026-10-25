using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMP_Ruby;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static LanguageController;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// メッセージ管理クラス
/// </summary>
public partial class MessageController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static MessageController instance;


    /// <summary>
    /// TutorialClearStatus(Dictionaryのキーに、他クラスのインスタンスメソッドの戻り値を宣言と同時に入れることができないため)
    /// </summary>
    private const string stringTutorialClearStatus = "TutorialClearStatus";


    [Header("メッセージパネル関連")]
    [Header("メッセージパネル(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject messagePanel;

    [Header("メッセージテキスト(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private TMP_Text messageText;

    /// <summary>
    /// メッセージテキストをTextMeshProRubyコンポーネントに変換して保存する変数
    /// </summary>
    private TextMeshProRuby messageTextRubyComponent;

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
    /// 47番目のメッセージ番号
    /// </summary>
    private const int kMessageNumber47 = 47;

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

    /// <summary>
    /// プレイヤーの名前の最小文字数
    /// </summary>
    private const int kMinPlayerNameLength = 1;

    /// <summary>
    /// プレイヤーの名前の最大文字数
    /// </summary>
    private const int kMaxPlayerNameLength = 11;

    /// <summary>
    /// プレイヤーのTransform保存用
    /// </summary>
    private Quaternion savePlayerQuaternion;

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

    /// <summary>
    /// ブラックアウトフラグ
    /// </summary>
    private bool isBlackOutPanel = false;

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


    /// <summary>
    /// ブラックアウトフラグを取得
    /// </summary>
    /// <returns>ブラックアウトフラグ</returns>
    public bool GetIsBlackOutPanel() 
    {
        return isBlackOutPanel;
    }

    /// <summary>
    /// ブラックアウトフラグを設定
    /// </summary>
    /// <param name="flag">ブラックアウトフラグ</param>
    public void SetIsBlackOutPanel(bool flag) 
    {
        isBlackOutPanel = flag;
    }

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
        //messageTextがnullの場合、エラーを出力
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
        instance = this;
        /*
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyController();
        }
        */

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

        //現在の言語用フォントを適用する(テキストを入れる前に行わないと□化けする)
        if (CommonController.instance != null)
        {
            CommonController.instance.ApplyLanguageFont(messageText);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //開発時のみ、フォントに存在しない文字をログに出す
            CommonController.instance.LogMissingCharacters(messageText, s);
#endif
        }

        //毎回テキストをクリアしてから書き始める
        messageText.text = "";

        //TextMeshProRubyコンポーネントがアタッチされているか確認
        messageTextRubyComponent = messageText.GetComponent<TMP_Ruby.TextMeshProRuby>();

        for (int i = 0; i < s.Length; i++)
        {
            //書き込み速度が0の場合、一気に表示
            if (writeSpeed <= 0)
            {
                //TextMeshProRubyコンポーネントがアタッチされている場合
                if (messageTextRubyComponent != null)
                {
                    //TextMeshProRubyコンポーネントにテキストを設定
                    messageTextRubyComponent.Text = s;
                }
                //TextMeshProRubyコンポーネントがアタッチされていない場合
                else
                {
                    //メッセージテキストにテキストを設定
                    messageText.text = s;
                }
                break;
            }

            // 現在の進行度までの文字列を切り出す
            string currentText = s.Substring(0, i + 1);

            // タグの途中で区切られると表示が崩れるのを防ぐため、
            // もし直前がタグの開始文字 '<' で、閉じ文字 '>' がまだ来ていない場合は演出をスキップして一気にタグの終わりまで進める
            if (currentText.Contains("<") && !currentText.EndsWith(">"))
            {
                int closingTagIndex = s.IndexOf('>', i);
                if (closingTagIndex != -1)
                {
                    i = closingTagIndex;
                    currentText = s.Substring(0, i + 1);
                }
            }

            //TextMeshProRubyコンポーネントがアタッチされている場合
            if (messageTextRubyComponent != null)
            {
                //TextMeshProRubyを通してテキストを適用する
                messageTextRubyComponent.Text = currentText;
            }
            //TextMeshProRubyコンポーネントがアタッチされていない場合
            else
            {
                //メッセージテキストにテキストを適用する
                messageText.text = currentText;
            }

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
            || Input.GetMouseButtonDown(0) || Input.GetButtonDown(CommonController.instance.GetStringInteract()));

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


        CancelAsyncTasks();

        //もしこのインスタンスがシングルトンインスタンス自身であれば、staticな参照をクリアする
        if (instance == this)
        {
            instance = null;
        }
    }
}
