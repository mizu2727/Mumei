using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;

public class HiddenObject : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの位置
    /// </summary>
    private Transform player;

    /// <summary>
    /// プレイヤー縮小用ローカルスケール
    /// </summary>
    private Vector3 smallPlayerLocalScale = new Vector3(0.25f, 0.25f, 0.25f);

    /// <summary>
    /// 保存用プレイヤーのローカルスケール
    /// </summary>
    private Vector3 savePlayerLocalScale;

    /// <summary>
    /// 保存用プレイヤーのローカルポジション
    /// </summary>
    private Vector3 savePlayerLocalPosition;

    /// <summary>
    /// プレイヤーとの距離
    /// </summary>
    private float distanceToPlayer;

    /// <summary>
    /// 指定の距離
    /// </summary>
    private const float hideDistance = 2.0f;

    /// <summary>
    /// 時間カウント
    /// </summary>
    private float countTime;

    /// <summary>
    /// 指定時間
    /// </summary>
    private const float kTimer = 1.0f;

    /// <summary>
    /// カウントスタートフラグ
    /// </summary>
    private bool isStartCountTime = false;


    /// <summary>
    /// Stage01(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage01 = "Stage01";

    /// <summary>
    /// Stage02(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage02 = "Stage02";

    /// <summary>
    /// Stage03(switch文で使用する。C#のswitch文のcaseは、「コンパイル時点で値が絶対に変わらないもの（定数）」のみコンパイルできるため)
    /// </summary>
    private const string stringStage03 = "Stage03";


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// audioSourceSE
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// 隠れるSEの現在のID
    /// </summary>
    private int currentHiddenPlayerSEid;

    /// <summary>
    /// ハイドポイントの扉を開閉するSEの現在のID
    /// </summary>
    private int currentHiddenDoorSEid;

    /// <summary>
    /// ハイドポイントの扉を開けるSEの現在のID
    /// </summary>
    private int currentHiddenDoorOpenSEid;

    /// <summary>
    /// ハイドポイントの扉を閉めるSEの現在のID
    /// </summary>
    private int currentHiddenDoorCloseSEid;


    /// <summary>
    /// チェストに隠れる時のSEのID
    /// </summary>
    private readonly int chestSEid = 13;

    /// <summary>
    /// ゴミ箱に隠れる時のSEのID
    /// </summary>
    private readonly int trashCanSEid = 14;

    /// <summary> 
    /// ロッカーの扉を開けるSEのID 
    /// </summary> 
    private readonly int lockerDoorOpenSEid = 22;

    /// <summary> 
    /// ロッカーの扉を閉めるSEのID 
    /// </summary> 
    private readonly int lockerDoorCloseSEid = 23;


    [Header("扉付きオブジェクト設定(ヒエラルキー上で設定すること)")]
    [SerializeField] private bool hasDoor = false;

    /// <summary>
    /// 付属のドアオブジェクト
    /// hasDoorがtrueの場合にアタッチする必要がある
    /// </summary>
    [SerializeField] private GameObject doorObject;

    [Header("ドアを開ける角度(ヒエラルキー上で設定すること)")]
    [SerializeField] private float openDirenctionValue = 90.0f;

    [Header("ドアを閉じる角度(ヒエラルキー上で設定すること)")]
    [SerializeField] private float closeDirenctionValue = 0.0f;


    [Header("扉が完全に開くまでの待機時間（秒）(ヒエラルキー上で設定すること)")]
    [SerializeField] private float doorOpenWaitTime = 0.5f;

    [Header("扉が完全に閉まるまでの待機時間（秒）(ヒエラルキー上で設定すること)")]
    [SerializeField] private float doorCloseWaitTime = 0.5f;

    /// <summary>
    /// 扉の開閉シーケンス実行中フラグ
    /// シーケンス中の重複入力を防ぐ
    /// </summary>
    private bool isDoorSequenceRunning = false;

    /// <summary>
    /// 扉の開閉シーケンスのキャンセルトークンソース
    /// OnDisable時にタスクを安全にキャンセルするために使用する
    /// </summary>
    private CancellationTokenSource doorCts;




    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時にAudioSourceを再設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;


        //TODO:扉シーケンスのUniTaskを安全にキャンセルする
        doorCts?.Cancel();
        doorCts?.Dispose();
        doorCts = null;
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
        audioSourceSE = GetComponent<AudioSource>();
        if (audioSourceSE == null)
        {
            audioSourceSE = gameObject.AddComponent<AudioSource>();
            audioSourceSE.playOnAwake = false;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;

        //SEのIDを現在のStageSceneによって切り替えて設定する
        switch (SceneManager.GetActiveScene().name) 
        {
            case stringStage01:
                currentHiddenPlayerSEid = chestSEid;
                break;

            case stringStage02:
                currentHiddenPlayerSEid = trashCanSEid;
                break;


            case stringStage03:
                currentHiddenDoorOpenSEid = lockerDoorOpenSEid;
                currentHiddenDoorCloseSEid = lockerDoorCloseSEid;
                break;

        }


        //デバッグ用
        currentHiddenDoorOpenSEid = lockerDoorOpenSEid;
        currentHiddenDoorCloseSEid = lockerDoorCloseSEid;
    }


    void Start()
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //時間カウントスタートフラグをオフにする
        isStartCountTime = false;

        //時間カウントを初期化
        countTime = 0.0f;
    }


    private void Update()
    {
        //GameClearSceneの場合
        if (SceneManager.GetActiveScene().name == CommonController.instance.GetGameClearSceneName() && Player.instance != null) 
        {
            //処理をスキップ
            return;
        }

        //プレイヤーとの距離を計算
        distanceToPlayer = Vector3.Distance(transform.position, Player.instance.transform.position);

        //カウントスタートフラグがオフの場合
        if (!isStartCountTime) 
        {
            //処理をスキップ
            return;
        }

        //プレイヤーとの距離が近い場合&&時間カウントが指定時間以内の場合
        if (distanceToPlayer <= hideDistance && countTime < kTimer)
        {
            //時間をカウント
            countTime += Time.deltaTime;

            //隠れポイントが近くに存在するかを判定するフラグをオンにする
            Player.instance.SetIsNearHidePoint(true);
        }
        else 
        {
            //時間カウントスタートフラグをオフにする
            isStartCountTime = false;

            //時間カウントを初期化
            countTime = 0.0f;

            //隠れポイントが近くに存在するかを判定するフラグをオフにする
            Player.instance.SetIsNearHidePoint(false);
        }
    }

    /// <summary>
    /// プレイヤーが隠れる処理関係
    /// </summary>
    public void HiddenPlayer()
    {
        //ドアが付属している場合
        if (hasDoor)
        {
            //扉の開閉シーケンスフラグがオンの場合
            if (isDoorSequenceRunning) 
            {
                //処理をスキップ
                return; 
            }

            //扉の開閉シーケンスを開始するフラグをオンにする
            HiddenPlayerWithDoorAsync().Forget();
        }
        else
        {
            //ドアが付属していない場合のプレイヤーが隠れる処理を実行
            HiddenPlayerImmediate();
        }
    }

    /// <summary>
    ///  プレイヤーが隠れる処理(ドアが付属していない場合)
    /// 
    /// 処理の主な流れ…
    /// プレイヤーの親オブジェクトを隠れるオブジェクトに設定
    /// → プレイヤーのスケールを小さくする
    /// → 隠れるオブジェクトの位置にプレイヤーのワールド座標を移動
    /// → プレイヤーのローカルポジションとローカル回転を初期化
    /// </summary>
    private void HiddenPlayerImmediate() 
    {
        //時間カウントスタートフラグをオフにする
        isStartCountTime = false;

        //プレイヤーのTransformを取得
        player = Player.instance.transform;

        //プレイヤーが隠れている状態にする
        Player.instance.SetIsPlayerHidden(true);

        //プレイヤーの移動を停止
        Player.instance.IsMove = false;

        //プレイヤーの移動音を消す
        Player.instance.audioSourceSE.Stop();

        //プレイヤーのローカルスケールを保存する
        savePlayerLocalScale = player.localScale;

        //プレイヤーのローカルポジションを保存する
        savePlayerLocalPosition = player.localPosition;


        //CharacterControllerを取得
        CharacterController characterController = player.GetComponent<CharacterController>();

        //nullチェック
        if (characterController != null)
        {
            //物理判定を一時的に無効化
            characterController.enabled = false;
        }


        //playerの親を隠れるオブジェクトに設定
        player.SetParent(transform);

        //playerのスケールを小さくする
        player.localScale = smallPlayerLocalScale;

        //隠れるオブジェクトの位置にplayerを移動
        player.position = transform.position;

        //playerのローカルポジションを初期化
        player.localPosition = Vector3.zero;

        //playerのローカル回転を初期化
        player.localRotation = Quaternion.identity;

        //プレイヤーが隠れるアニメーションを再生
        Player.instance.PlayOrStopKneelingAnimation();

        //隠れたときのSE
        audioSourceSE.clip = sO_SE.GetSEClip(currentHiddenPlayerSEid);
        audioSourceSE.PlayOneShot(audioSourceSE.clip);
    }

    /// <summary>
    /// プレイヤーが隠れる処理（扉あり）
    /// 扉を開ける → プレイヤーを隠す → 扉を閉める、の順でシーケンス実行する
    /// </summary>
    private async UniTaskVoid HiddenPlayerWithDoorAsync()
    {
        //扉の開閉シーケンス実行中フラグをオンにする
        isDoorSequenceRunning = true;

        //前回のトークンを破棄し新しいトークンを発行する
        doorCts?.Cancel();
        doorCts?.Dispose();
        doorCts = new CancellationTokenSource();
        CancellationToken token = doorCts.Token;

        try
        {
            //扉を開ける
            SetDoorOpen(true, openDirenctionValue);

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(doorOpenWaitTime),
                cancellationToken: token);

            //プレイヤーを隠す
            HiddenPlayerImmediate();

            //扉を閉める
            SetDoorOpen(false, closeDirenctionValue);

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(doorCloseWaitTime),
                cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            //オブジェクト破棄やシーン遷移によるキャンセルは正常系のため何もしない
        }
        finally
        {
            isDoorSequenceRunning = false;
        }
    }


    /// <summary>
    /// プレイヤーの姿を現す処理関係
    /// </summary>
    public void ShowThePlayer() 
    {
        //ドアが付属している場合
        if (hasDoor)
        {
            //扉の開閉シーケンスフラグがオンの場合
            if (isDoorSequenceRunning)
            {
                //処理をスキップ
                return;
            }

            //扉の開閉シーケンスを開始するフラグをオンにする
            ShowThePlayerWithDoorAsync().Forget();
        }
        else
        {
            //ドアが付属していない場合のプレイヤーが姿を現す処理を実行
            ShowThePlayerImmediate();
        }
    }

    /// <summary>
    /// プレイヤーの姿を現す処理
    /// </summary>
    private void ShowThePlayerImmediate() 
    {
        //時間カウントスタートフラグをオンにする
        isStartCountTime = true;

        //プレイヤーの親をnullに設定
        player.SetParent(null);

        //CharacterControllerを取得
        CharacterController characterController = player.GetComponent<CharacterController>();

        //nullチェック
        if (characterController != null)
        {
            //物理判定を一時的に無効化
            characterController.enabled = false;
        }

        //姿を現す位置へ移動
        player.position = transform.position + transform.forward * 1.5f;

        //nullチェック
        if (characterController != null)
        {
            //物理判定を有効化
            characterController.enabled = true;
        }

        //プレイヤーが隠れている状態をオフにする
        Player.instance.SetIsPlayerHidden(false);

        //プレイヤーのローカルスケールを元に戻す
        player.localScale = savePlayerLocalScale;

        //プレイヤーのローカルポジションを保存していた値に戻す
        player.localPosition = savePlayerLocalPosition;

        //プレイヤーが隠れるアニメーションを停止
        Player.instance.PlayOrStopKneelingAnimation();

        //プレイヤーが姿を現すときのSE
        audioSourceSE.clip = sO_SE.GetSEClip(currentHiddenPlayerSEid);
        audioSourceSE.PlayOneShot(audioSourceSE.clip);
    }

    /// <summary>
    /// TODO:プレイヤーの姿を現す処理（扉あり）
    /// 扉を開ける → プレイヤーを出す → 扉を閉める、の順でシーケンス実行する
    /// </summary>
    private async UniTaskVoid ShowThePlayerWithDoorAsync()
    {
        //扉の開閉シーケンス実行中フラグをオンにする
        isDoorSequenceRunning = true;

        //前回のトークンを破棄し新しいトークンを発行する
        doorCts?.Cancel();
        doorCts?.Dispose();
        doorCts = new CancellationTokenSource();
        CancellationToken token = doorCts.Token;

        try
        {
            //扉を開ける
            SetDoorOpen(true, openDirenctionValue);

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(doorOpenWaitTime),
                cancellationToken: token);

            //プレイヤーを出す
            ShowThePlayerImmediate();

            //扉を閉める
            SetDoorOpen(false, closeDirenctionValue);

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(doorCloseWaitTime),
                cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            //オブジェクト破棄やシーン遷移によるキャンセルは正常系のため何もしない
        }
        finally
        {
            isDoorSequenceRunning = false;
        }
    }

    /// <summary>
    /// 扉の開閉処理
    /// </summary>
    /// <param name="isOpen">trueで開く時の処理</param>
    /// <param name="y">回転角度</param>
    private void SetDoorOpen(bool isOpen, float y)
    {
        //ドアを回転させる
        doorObject.transform.Rotate(0, y, 0);

        //扉の開閉に対応するSEを再生する 
        currentHiddenDoorSEid = isOpen ? currentHiddenDoorOpenSEid : currentHiddenDoorCloseSEid;
        audioSourceSE.clip = sO_SE.GetSEClip(currentHiddenDoorSEid);
        audioSourceSE.PlayOneShot(audioSourceSE.clip);
    }
}
