using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Player;

public class HiddenObject : MonoBehaviour
{
    /*---------------------------------------------
     * 隠れる状態モード関連
     --------------------------------------------*/

    /// <summary>
    /// 隠れる状態モード
    /// </summary>
    private HideMode hideMode;

    enum HideMode
    {
        /// <summary>
        /// 何もない
        /// </summary>
        None,

        /// <summary>
        /// 隠れるモードを開始
        /// </summary>
        StartHide,       
    }

    /*---------------------------------------------
     * プレイヤー関連
     --------------------------------------------*/

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


    /*---------------------------------------------
     * ステージ名関連
     --------------------------------------------*/

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


    /*---------------------------------------------
     * SE関連
     --------------------------------------------*/

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


    /*---------------------------------------------
     * ハイドポイントの種類関連
     --------------------------------------------*/

    /// <summary>
    /// ハイドポイントの種類
    /// </summary>
    public enum HidePointType
    {
        /// <summary>
        /// 宝箱
        /// </summary>
        Chest,

        /// <summary>
        /// ゴミ箱
        /// </summary>
        TrashCan,

        /// <summary>
        /// ロッカー
        /// </summary>
        Locker,

        /// <summary>
        /// ベッドの下(うつ伏せで隠れる)
        /// </summary>
        UnderBed,
    }

    /// <summary>
    /// ベッドの下に隠れるハイドポイントかを判定する
    /// </summary>
    /// <returns>ベッドの下の場合true</returns>
    public bool IsUnderBed()
    {
        return hidePointType == HidePointType.UnderBed;
    }

    [Header("ベッドの下でうつ伏せになる位置(ハイドポイント基準のローカル座標。UnderBedのみ使用)")]
    [Header("※頭はハイドポイントのZ軸(青)方向を向く。Yは床からの高さ")]
    [SerializeField] private Vector3 proneLocalPosition = new Vector3(0f, 0.15f, -0.9f);

    [Header("ベッドの下に隠れる・出る時のSE ID(UnderBedのみ使用。-1の場合はステージ既定のSE)")]
    [SerializeField] private int underBedSEid = 31;

    /// <summary>
    /// 保存用プレイヤーの回転(ベッドの下から出る時に元の向きへ戻すため)
    /// </summary>
    private Quaternion savePlayerRotation = Quaternion.identity;

    [Header("ハイドポイントの種類(ヒエラルキー上で設定すること)")]
    [SerializeField] private HidePointType hidePointType = HidePointType.Chest;

    /// <summary>
    /// ハイドポイントの種類を取得
    /// </summary>
    /// <returns>ハイドポイントの種類</returns>
    public HidePointType GetHidePointType()
    {
        return hidePointType;
    }

    [Header("敵が覗き込む位置(任意。未設定の場合はハイドポイントの前方へ自動配置)")]
    [Header("※Z軸(青)がハイドポイントの方を向くように配置すること")]
    [SerializeField] private Transform enemyPeekPoint;

    /// <summary>
    /// 敵が覗き込む位置を取得(未設定の場合はnull)
    /// </summary>
    /// <returns>敵が覗き込む位置</returns>
    public Transform GetEnemyPeekPoint()
    {
        return enemyPeekPoint;
    }

    /// <summary>
    /// 敵によって扉が開けられたフラグ(二重に回転させないため)
    /// </summary>
    private bool isDoorOpenedByEnemy = false;


    /// <summary>
    /// このハイドポイントの中(子オブジェクト)にプレイヤーが隠れているかを判定する
    /// </summary>
    /// <returns>隠れている場合true</returns>
    public bool IsPlayerHiddenInside()
    {
        //プレイヤーが存在しない場合
        if (Player.instance == null)
        {
            return false;
        }

        //隠れているフラグがオン&&プレイヤーの親がこのハイドポイントの場合
        return Player.instance.GetIsPlayerHidden() && Player.instance.transform.parent == transform;
    }

    /// <summary>
    /// 敵が扉だけを開ける処理(プレイヤーは外に出さない)
    /// </summary>
    /// <returns>扉が完全に開くまでの待機時間（秒）。扉が無い場合は0</returns>
    public float OpenDoorByEnemy()
    {
        //扉が無い場合||既に開けている場合
        if (!hasDoor || doorObject == null || isDoorOpenedByEnemy)
        {
            return 0f;
        }

        //敵によって扉が開けられたフラグをオンにする
        isDoorOpenedByEnemy = true;

        //扉を開ける
        SetDoorOpen(true, openDirenctionValue);

        return doorOpenWaitTime;
    }


    /*---------------------------------------------
     * 扉付きオブジェクト関連
     --------------------------------------------*/

    [Header("扉付きオブジェクト設定(ヒエラルキー上で設定すること)")]
    [SerializeField] private bool hasDoor = false;

    /// <summary>
    /// 付属のドアオブジェクト
    /// hasDoorがtrueの場合にアタッチする必要がある
    /// </summary>
    [SerializeField] private GameObject doorObject;

    [Header("ドアを開ける角度(ヒエラルキー上で設定すること)")]
    [SerializeField] private float openDirenctionValue;

    [Header("ドアを閉じる角度(ヒエラルキー上で設定すること)")]
    [SerializeField] private float closeDirenctionValue;

    [Header("扉が完全に開くまでの待機時間（秒）(ヒエラルキー上で設定すること)")]
    [SerializeField] private float doorOpenWaitTime;

    /// <summary>
    /// 扉が完全に開くまでの待機時間を取得
    /// </summary>
    /// <returns>扉が完全に開くまでの待機時間（秒）</returns>
    public float GetDoorOpenWaitTime()
    {
        return doorOpenWaitTime;
    }

    [Header("扉が完全に閉まるまでの待機時間（秒）(ヒエラルキー上で設定すること)")]
    [SerializeField] private float doorCloseWaitTime;

    /// <summary>
    /// ドア開閉時間
    /// </summary>
    private float doorMoveTime;

    /// <summary>
    /// 扉の開閉シーケンス実行中フラグ
    /// シーケンス中の重複入力を防ぐ
    /// </summary>
    private bool isDoorSequenceRunning = false;

    /// <summary>
    /// 扉の開閉シーケンス実行中フラグを取得する
    /// </summary>
    /// <returns>扉の開閉シーケンス実行中フラグ</returns>
    public bool GetIsDoorSequenceRunning()
    {
        return isDoorSequenceRunning;
    }

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

        //隠れる状態モードを初期化
        hideMode = HideMode.None;

        //ドア開閉時間を初期化
        doorMoveTime = 0.0f;

        //時間カウントスタートフラグをオフにする
        isStartCountTime = false;

        //時間カウントを初期化
        countTime = 0.0f;

        //扉の開閉シーケンス実行中フラグをオフにする
        isDoorSequenceRunning = false;

        if (Player.instance.transform != null) 
        {
            //プレイヤーのTransformを取得
            player = Player.instance.transform;
        }
    }


    private void Update()
    {
        //GameClearSceneの場合
        if (SceneManager.GetActiveScene().name == CommonController.instance.GetGameClearSceneName() || player == null || Player.instance.transform == null) 
        {
            //処理をスキップ
            return;
        }

        //プレイヤーとの距離を計算
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        //隠れるモード開始する場合
        if (hideMode == HideMode.StartHide) 
        {
            //ドア開閉時間をカウント
            doorMoveTime += Time.deltaTime;

            //ドアが完全に開いた場合
            if (doorMoveTime >= doorOpenWaitTime && Player.instance.GetDieMode() != DieMode.Die) 
            {
                //ドアが付属している場合のプレイヤーが隠れる処理を実行
                HiddenPlayerImmediate();
            }
        }

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
            if (hideMode == HideMode.None && Player.instance.GetDieMode() != DieMode.Die) 
            {
                //隠れる状態モードを隠れるモード開始に設定
                hideMode = HideMode.StartHide;

                //扉を開ける
                SetDoorOpen(true, openDirenctionValue);
            }
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

        //プレイヤーの回転を保存する
        savePlayerRotation = player.rotation;

        //ベッドの下の場合
        if (IsUnderBed())
        {
            //うつ伏せで隠れる
            HidePlayerProne();
        }
        else
        {
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
        }

        //ドアが付属していない場合
        if (!hasDoor)
        {
            //プレイヤーが隠れるアニメーションを再生(うつ伏せの場合は膝立ちを再生しない)
            Player.instance.PlayOrStopKneelingAnimation();

            //隠れたときのSE
            PlayHideSE();
        }
        else 
        {
            //扉を閉める
            SetDoorOpen(false, closeDirenctionValue);
        }

        //ドア開閉時間を初期化
        doorMoveTime = 0.0f;

        //隠れる状態モードを初期化
        hideMode = HideMode.None;
    }


    /// <summary>
    /// プレイヤーの姿を現す処理関係
    /// </summary>
    public void ShowThePlayer() 
    {
        //敵に見つかって死亡が確定している場合(発見演出中に外へ出られないようにする)
        if (Player.instance == null || Player.instance.GetDieMode() == DieMode.Die)
        {
            //処理をスキップ
            return;
        }

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

        //ベッドの下の場合
        if (IsUnderBed())
        {
            //うつ伏せ状態を解除(アニメーションは無しで即座に立った姿勢へ戻す)
            ShowPlayerFromProne();
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

        //ドアが付属していない場合
        if (!hasDoor) 
        {
            //プレイヤーが姿を現すときのSE
            PlayHideSE();
        }

    }

    /// <summary>
    /// うつ伏せでベッドの下に隠れる処理(アニメーション無し)
    ///
    /// 処理の主な流れ…
    /// プレイヤーの親をハイドポイントに設定(ワールドスケールは維持)
    /// → ベッドの下の位置へ移動
    /// → X軸を90度回転させてうつ伏せの姿勢にする(頭がハイドポイントのZ軸方向を向く)
    /// → カメラをうつ伏せモードにする
    /// </summary>
    private void HidePlayerProne()
    {
        //うつ伏せで隠れているフラグをオン(膝立ちアニメーションを再生させないため、アニメーション処理より先に設定)
        Player.instance.SetIsProneHidden(true);

        //playerの親を隠れるオブジェクトに設定(ワールドスケールを維持する)
        player.SetParent(transform, true);

        //ベッドの下の位置へ移動
        player.localPosition = proneLocalPosition;

        //うつ伏せの姿勢にする
        //(X軸+90度回転で、プレイヤーのY軸(頭)がハイドポイントの前方、Z軸(顔)が床方向を向く)
        player.localRotation = Quaternion.Euler(90f, 0f, 0f);

        //カメラをうつ伏せモードにする
        if (PlayerCamera.instance != null)
        {
            PlayerCamera.instance.EnterProneMode();
        }
    }

    /// <summary>
    /// うつ伏せ状態を解除する処理(アニメーション無し)
    /// ※プレイヤーの親をnullにした後に呼ぶこと
    /// </summary>
    private void ShowPlayerFromProne()
    {
        //うつ伏せで隠れているフラグをオフ
        Player.instance.SetIsProneHidden(false);

        //プレイヤーの向きを、隠れる前の水平な向きに戻す
        Vector3 forward = Vector3.ProjectOnPlane(savePlayerRotation * Vector3.forward, Vector3.up);
        player.rotation = forward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : Quaternion.identity;

        //カメラのうつ伏せモードを解除
        if (PlayerCamera.instance != null)
        {
            PlayerCamera.instance.ExitProneMode();
        }
    }

    /// <summary>
    /// 隠れる・姿を現す時のSEを再生する
    /// </summary>
    private void PlayHideSE()
    {
        //ベッドの下でSE IDが指定されている場合はそちらを使う
        int seId = (IsUnderBed() && underBedSEid >= 0) ? underBedSEid : currentHiddenPlayerSEid;

        audioSourceSE.clip = sO_SE.GetSEClip(seId);
        if (audioSourceSE.clip != null)
        {
            audioSourceSE.PlayOneShot(audioSourceSE.clip);
        }
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
