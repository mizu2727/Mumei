using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

/// <summary>
/// GameClearSceneで使用する管理クラス
/// </summary>
public class GameClearController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static GameClearController instance;

    [Header("ゲームクリア画面のCanvas")]
    [SerializeField] public Canvas gameClearCanvas;

    /// <summary>
    /// ゲームクリア画面のCanvasGroup(非表示中にボタンを押せないようにするため)
    /// </summary>
    private CanvasGroup gameClearCanvasGroup;

    [Header("コントローラー操作用のボタン(ヒエラルキー上からアタッチすること)")]
    [Header("アンケートボタン(左上。ゲームクリア画面表示時に最初に選択する)")]
    [SerializeField] private Selectable questionnaireButton;

    [Header("ウィッシュリストボタン(右上)")]
    [SerializeField] private Selectable wishlistButton;

    [Header("タイトルへ戻るボタン(下)")]
    [SerializeField] private Selectable returnToTitleButton;

    /// <summary>
    /// 最初のボタンが押せる状態になるまで待つ最大フレーム数
    /// </summary>
    private const int kMaxWaitFramesForInteractable = 10;

    /// <summary>
    /// アンケートURL
    /// </summary>
    private const string questionnaireURL = "https://docs.google.com/forms/d/13qMmaottZOaX7lg4lu8kzzvfE5aXFS3kNgPPoCmxI6M/viewform";

    /// <summary>
    /// XのURL
    /// </summary>
    private const string X_URL = "https://x.com/Tomanegi0707";

    /// <summary>
    /// BlueSkyのURL
    /// </summary>
    private const string BlueSky_URL = "https://bsky.app/profile/tomanegi0707.bsky.social";

    /// <summary>
    /// インデイ市のデモ版ストアページURL
    /// </summary>
    private const string demo_Indieichi_URL = "https://indieichi.com/games/cmg1xsi4x00093ty3rwizl7op";

    /// <summary>
    /// インデイ市の製品版ストアページURL
    /// </summary>
    private const string indieichi_URL = "https://indieichi.com/games/cmo2x0dzr0063ntwhtu1in4xa";

    /// <summary>
    /// Steamの製品版ストアページURL
    /// </summary>
    private const string full_Steam_URL = "https://store.steampowered.com/app/4034690/_Mumei/";

    [Header("BGMデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_BGM sO_BGM;

    /// <summary>
    /// audioSourceBGM
    /// </summary>
    private AudioSource audioSourceBGM;

    /// <summary>
    /// audioClipBGM
    /// </summary>
    private AudioClip audioClipBGM;

    /// <summary>
    /// GameClearSceneBGMのID
    /// </summary>
    private readonly int gameClearSceneBGMId = 3;

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy()
    {
        //gameClearCanvasが存在する場合
        if (gameClearCanvas != null)
        {
            //gameClearCanvasをnullに設定
            gameClearCanvas = null;
        }

        //gameClearCanvasGroupをnullに設定
        gameClearCanvasGroup = null;

        //コントローラー操作用のボタンをnullに設定
        questionnaireButton = null;
        wishlistButton = null;
        returnToTitleButton = null;

        //instanceが存在する場合
        if (instance != null)
        {
            //instanceをnullに設定
            instance = null;
        }
    }

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        //全てのBGMの状態をStopに変更
        sO_BGM.StopAllBGM();
    }

    /// <summary>
    /// AudioSourceBGMを取得する
    /// </summary>
    /// <returns>AudioSourceBGM</returns>
    public AudioSource GetAudioSourceBGM()
    {
        return audioSourceBGM;
    }

    /// <summary>
    /// AudioClipBGMを取得する
    /// </summary>
    /// <returns>AudioClipBGM</returns>
    public AudioClip GetAudioClipBGM()
    {
        return audioClipBGM;
    }

    /// <summary>
    /// GameClearSceneBGMのIDを取得する
    /// </summary>
    /// <returns>GameClearSceneBGMのID</returns>
    public int GetGameClearSceneBGMId()
    {
        return gameClearSceneBGMId;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //BGM音量変更時のイベント登録
        MusicController.OnBGMVolumeChangedEvent += UpdateBGMVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnBGMVolumeChangedEvent -= UpdateBGMVolume;
    }

    /// <summary>
    /// BGM音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateBGMVolume(float volume)
    {
        if (audioSourceBGM != null)
        {
            audioSourceBGM.volume = volume;
        }
    }

    /// <summary>
    /// シーン遷移時に処理を呼び出す関数
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //audioSourceBGMを設定
        audioSourceBGM = MusicController.instance.GetAudioSource();

        //audioClipBGMを設定
        audioClipBGM = sO_BGM.GetBGMClip(gameClearSceneBGMId);

        //MusicControllerで設定されているBGM用のAudioMixerGroupを設定する
        audioSourceBGM.outputAudioMixerGroup = MusicController.instance.audioMixerGroupBGM;
    }


    void Start()
    {
        //シーンステータスをkGameClearSceneに設定
        GameController.instance.SetViewScene(ViewScene.kGameClearScene);

        //ゲームモードステータスをStoryに変更
        GameController.instance.SetGameModeStatus(GameModeStatus.Story);
        HiddenGameClearUI();

        //ゲームクリアシーンBGMを再生
        MusicController.instance.PlayLoopBGM(audioSourceBGM, sO_BGM.GetBGMClip(gameClearSceneBGMId), gameClearSceneBGMId);
    }

    /// <summary>
    /// タイトルへ戻る
    /// </summary>
    public void OnClickedReturnToTitleButton()
    {
        //シーン遷移時用データを保存
        GameController.instance.CallSaveSceneTransitionUserDataMethod();

        //タイトルへ戻る
        SceneManager.LoadScene(CommonController.instance.GetTitleSceneName());
    }

    /// <summary>
    /// ゲームクリア時のUIを表示
    /// </summary>
    public void ViewGameClearUI()
    {
        gameClearCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //ボタンを押せるようにする
        SetGameClearUIInteractable(true);

        //コントローラーでの選択枠の移動先を設定する
        SetupGameClearNavigation();

        //コントローラー接続時は最初のボタンを選択する
        //(CanvasGroup.interactableの変更はボタン側に反映されるまで1フレームかかるため、待ってから選択する)
        if (GamepadInput.IsConnected)
        {
            StartCoroutine(SelectFirstButtonNextFrame());
        }
    }

    /// <summary>
    /// 1フレーム待ってから、ゲームクリア画面の最初のボタンを選択する
    /// </summary>
    private IEnumerator SelectFirstButtonNextFrame()
    {
        yield return null;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null || gameClearCanvas == null)
        {
            yield break;
        }

        //一旦選択を解除する(同じボタンが選択済みだとSelectイベントが呼ばれず、選択枠・文字色が反映されないため)
        eventSystem.SetSelectedGameObject(null);

        //アンケートボタンが設定されている場合は、最優先で選択する
        if (questionnaireButton != null)
        {
            //CanvasGroupの変更がボタンに反映され、押せる状態になるまで待つ(最大kMaxWaitFramesForInteractableフレーム)
            for (int i = 0; i < kMaxWaitFramesForInteractable && !questionnaireButton.IsInteractable(); i++)
            {
                yield return null;
            }

            if (questionnaireButton.IsInteractable() && questionnaireButton.gameObject.activeInHierarchy)
            {
                eventSystem.SetSelectedGameObject(questionnaireButton.gameObject);
                yield break;
            }

            Debug.LogWarning($"[GameClearController] {questionnaireButton.name} が押せない状態のため選択できません(ButtonのInteractableを確認してください)", questionnaireButton);
        }

        //GamepadUISelectorがある場合は、そちらの設定(First Selected)で選択する
        GamepadUISelector selector = gameClearCanvas.GetComponentInChildren<GamepadUISelector>();
        if (selector != null)
        {
            selector.SelectFirst();
        }

        //GamepadUISelectorが無い・選択できなかった場合は、最初に見つかった押せるボタンを選択する
        if (eventSystem.currentSelectedGameObject == null)
        {
            foreach (Selectable selectable in gameClearCanvas.GetComponentsInChildren<Selectable>())
            {
                if (selectable.IsInteractable() && selectable.gameObject.activeInHierarchy)
                {
                    eventSystem.SetSelectedGameObject(selectable.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// コントローラー(十字キー・スティック)での選択枠の移動先を設定する
    /// (プレハブ側のNavigation設定に関係なく、3つのボタン間を確実に移動できるようにする)
    ///
    ///   [アンケート] ←→ [ウィッシュリスト]
    ///          ↓              ↓
    ///         [ タイトルへ戻る ]
    /// </summary>
    private void SetupGameClearNavigation()
    {
        //アンケートボタン:右→ウィッシュリスト、下→タイトルへ戻る
        SetExplicitNavigation(questionnaireButton, null, returnToTitleButton, null, wishlistButton);

        //ウィッシュリストボタン:左→アンケート、下→タイトルへ戻る
        SetExplicitNavigation(wishlistButton, null, returnToTitleButton, questionnaireButton, null);

        //タイトルへ戻るボタン:上・左→アンケート、右→ウィッシュリスト
        SetExplicitNavigation(returnToTitleButton, questionnaireButton, null, questionnaireButton, wishlistButton);
    }

    /// <summary>
    /// ボタンの移動先を手動(Explicit)で設定する
    /// </summary>
    /// <param name="target">設定するボタン</param>
    /// <param name="up">上の移動先(無い場合はnull)</param>
    /// <param name="down">下の移動先(無い場合はnull)</param>
    /// <param name="left">左の移動先(無い場合はnull)</param>
    /// <param name="right">右の移動先(無い場合はnull)</param>
    private void SetExplicitNavigation(Selectable target, Selectable up, Selectable down, Selectable left, Selectable right)
    {
        //ボタンが設定されていない場合
        if (target == null)
        {
            //処理をスキップ
            return;
        }

        //Navigationは構造体のため、コピーして書き換えてから代入し直す
        Navigation navigation = target.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = up;
        navigation.selectOnDown = down;
        navigation.selectOnLeft = left;
        navigation.selectOnRight = right;
        target.navigation = navigation;
    }

    /// <summary>
    /// ゲームクリア時のUIを非表示
    /// </summary>
    private void HiddenGameClearUI()
    {
        gameClearCanvas.enabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        //非表示中はボタンを押せないようにする
        //(Canvas.enabled = falseは見た目が消えるだけで、ボタンはEventSystemで選択・決定できてしまう。
        //そのため、会話を進めるEnter・A / ×ボタンで選択中のSteamストアボタンが押されていた)
        SetGameClearUIInteractable(false);
    }

    /// <summary>
    /// ゲームクリア画面のボタンを押せる・押せない状態に切り替える
    /// </summary>
    /// <param name="isInteractable">true:押せる / false:押せない</param>
    private void SetGameClearUIInteractable(bool isInteractable)
    {
        //gameClearCanvasが存在しない場合
        if (gameClearCanvas == null)
        {
            //処理をスキップ
            return;
        }

        //CanvasGroupを取得(無ければ追加)
        if (gameClearCanvasGroup == null)
        {
            gameClearCanvasGroup = gameClearCanvas.GetComponent<CanvasGroup>();
            if (gameClearCanvasGroup == null)
            {
                gameClearCanvasGroup = gameClearCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        //ボタンの操作可否・クリック判定を切り替える
        gameClearCanvasGroup.interactable = isInteractable;
        gameClearCanvasGroup.blocksRaycasts = isInteractable;

        //押せない状態にする場合
        if (!isInteractable)
        {
            //ゲームクリア画面内のボタンが選択されたままの場合は選択を解除する
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentSelectedGameObject != null
                && eventSystem.currentSelectedGameObject.transform.IsChildOf(gameClearCanvas.transform))
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }
    }

    /// <summary>
    /// アンケートURLを開く
    /// </summary>
    public void OnClickedQuestionnaire_Button() 
    {
        Application.OpenURL(questionnaireURL);
    }

    /// <summary>
    /// XのURLを開く
    /// </summary>
    public void OnClickedX_Button()
    {
        Application.OpenURL(X_URL);
    }

    /// <summary>
    /// BlueSkyのURLを開く
    /// </summary>
    public void OnClickedBlueSky_Button()
    {
        Application.OpenURL(BlueSky_URL);
    }

    /// <summary>
    /// インデイ市のデモ版ストアページのURLを開く
    /// </summary>
    public void OnClickedDemo_Indieichi_URL_Button()
    {
        Application.OpenURL(demo_Indieichi_URL);
    }

    /// <summary>
    /// インデイ市の製品版ストアページのURLを開く
    /// </summary>
    public void OnClickedIndieichi_URL_Button()
    {
        Application.OpenURL(indieichi_URL);
    }

    /// <summary>
    /// Steamの製品版ストアページのURLを開く
    /// </summary>
    public void OnClickedFull_Steam_URL_Button()
    {
        Application.OpenURL(full_Steam_URL);
    }
}
