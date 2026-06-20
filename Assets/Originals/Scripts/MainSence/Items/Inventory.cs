using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static SO_Item;


public class Inventory : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Inventory instance;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("アイテムメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ItemMessage itemMessage;


    [Header("使用アイテムパネル関連")]
    [Header("使用アイテムパネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject useItemPanel;

    [Header("使用アイテム所持カウントテキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Text useItemCountText;

    [Header("使用アイテム画像(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Image useItemImage;

    [Header("使用アイテムテキスト確認パネル(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public GameObject useItemTextPanel;

    [Header("使用アイテム名テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text useItemNameText;

    [Header("使用アイテム説明テキスト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text useItemExplanationText;


    /// <summary>
    /// アイテムID管理
    /// </summary>
    private int keepItemId;

    /// <summary>
    /// アイテム未所持時のID
    /// </summary>
    private const int kNoneItemId = 99999;

    /// <summary>
    /// スタミナ増強剤のID
    /// </summary>
    private const int kStaminaEnhancerId = 11;

    /// <summary>
    /// クラッカーのID
    /// </summary>
    private const int kCrackerId = 19;

    /// <summary>
    /// アイテムのプレハブのAddressables名
    /// </summary>
    private string keepItemPrefabPath;

    /// <summary>
    /// アイテムのプレハブのAddressables名(空白)
    /// </summary>
    private const string kNoneItemPrefabPath = "";

    /// <summary>
    /// アイテム生成位置
    /// </summary>
    private Vector3 keepItemSpawnPosition;

    /// <summary>
    /// アイテム生成位置(デフォルト)
    /// </summary>
    private Vector3 defaultItemSpawnPosition = new Vector3(0, 0, 0);

    /// <summary>
    /// アイテムの回転数値
    /// </summary>
    private Quaternion keepItemSpawnRotation;

    /// <summary>
    /// アイテムの回転数値(デフォルト)
    /// </summary>
    private Quaternion defaultItemSpawnRotation = Quaternion.identity;

    /// <summary>
    /// アイテム所持数
    /// </summary>
    private int keepItemCount;

    /// <summary>
    /// アイテム所持数(デフォルト)
    /// </summary>
    private const int kMinKeepItemCount = 0;

    /// <summary>
    /// アイテム効果値
    /// </summary>
    private int keepItemEffectValue;

    /// <summary>
    /// アイテム効果値(デフォルト値)
    /// </summary>
    private const int kDefaultKeepItemEffectValue = 0;

    /// <summary>
    /// スタミナ増強剤使用フラグ
    /// </summary>
    private bool isUseStaminaItem;

    /// <summary>
    /// スタミナ増強剤適用時のスタミナ消費率
    /// </summary>
    private const float kSpecifiedStaminaConsumeRatio = 12.5f;

    /// <summary>
    /// スタミナ増強剤適用時のスタミナ回復値
    /// </summary>
    private const float kSpecifiedStaminaRecoveryRatio = 30.0f;


    [Header("クラッカー使用時のパーティクルシステム(Prefabをアタッチすること)")]
    [SerializeField] private ParticleSystem crackerParticleSystemPrefab;

    /// <summary>
    /// クラッカー使用時のパーティクルの表示位置
    /// </summary>
    private Vector3 spawnCrackerParticlePosition;

    /// <summary>
    /// クラッカー使用時のパーティクルシステムのクローン
    /// </summary>
    private ParticleSystem spawnedCrackerParticle;

    /// <summary>
    /// クラッカー使用フラグ
    /// </summary>
    private bool isUseCrackerItem;

    /// <summary>
    /// クラッカーのRaycastHit情報
    /// </summary>
    private RaycastHit crackerRaycast;

    /// <summary>
    /// RaycastからSphereCastへの変換用
    /// </summary>
    private bool isRacastHit;

    /// <summary>
    /// クラッカーのRayCastの長さ(飛距離)
    /// </summary>
    private const float kCrackerRaycastDistance = 7.5f;

    /// <summary>
    /// クラッカーの判定の太さ（半径）
    /// </summary>
    private const float kCrackerRaycastRadius = 1.0f;

    /// <summary>
    /// Raycastの開始位置(カメラの前方)
    /// </summary>
    private Vector3 castStartPosition;

    /// <summary>
    /// Raycastの開始位置微調整(カメラの後方)(敵の至近距離で発動しても当たらない問題を防ぐため要調整)
    /// </summary>
    private const float kCastStartPositionForward = -0.1f;


    [Header("検知対象のレイヤー（Enemyを設定すること）")]
    [SerializeField] private LayerMask enemyLayer;

    /// <summary>
    /// Player.cs
    /// </summary>
    private Player player;

    /// <summary>
    /// BaseEnemy.cs
    /// </summary>
    private BaseEnemy baseEnemy;

    /// <summary>
    /// RaycastHitから取得した彷徨う者
    /// </summary>
    private GameObject pickUpEnemy;


    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// Inventory専用のAudioSource
    /// </summary>
    private AudioSource audioSourceInventorySE;

    /// <summary>
    /// AudioSource4つ分
    /// </summary>
    private const int kAudioSourceLength = 4;

    /// <summary>
    /// デフォルトのAudioSourceのSE音量
    /// </summary>
    private const float kDefaultAudioSourceSEVolume = 1.0f;

    /// <summary>
    /// スタミナ増強剤SEのID
    /// </summary>
    private readonly int useStaminaEnhancerSEid = 12;

    /// <summary>
    /// クラッカーSEのID
    /// </summary>
    private readonly int crackerSEid = 21;

    /// <summary>
    /// 不透明色
    /// </summary>
    private Color opaqueColor = new Color(255, 255, 255, 1);

    /// <summary>
    /// 緑色
    /// </summary>
    private Color greenColor = new Color32(0, 255, 0, 255);

    /// <summary>
    /// 使用アイテム画像の初期色
    /// </summary>
    private Color defaultUseItemImageColor = new Color(255, 255, 255, 0.05f);


    /// <summary>
    /// スタミナ増強剤使用中メッセージID
    /// </summary>
    private const int kUsingStaminaEnhancerMessageId = 3;

    /// <summary>
    /// メッセージ表示時間(秒)
    /// </summary>
    private const double kViewMessageSeconds = 3.0;


    /// <summary>
    /// アイテムID管理を取得
    /// </summary>
    /// <returns>アイテムID管理</returns>
    public int GetKeepItemId() 
    {
        return keepItemId;
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
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceInventorySE != null)
        {
            audioSourceInventorySE.volume = volume;
        }
    }

    /// <summary>
    /// シーン遷移時に使用アイテムパネル関連を再設定及びリセット
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSourceの初期化
        InitializeAudioSource();

        //GameController.csのUIを反映させる
        if (GameController.instance.useItemPanel != null) useItemPanel = GameController.instance.useItemPanel;
        else Debug.LogError("GameControllerのuseItemPanelが設定されていません");

        if (GameController.instance.useItemCountText != null) useItemCountText = GameController.instance.useItemCountText;
        else Debug.LogError("GameControllerのuseItemCountTextが設定されていません");

        if (GameController.instance.useItemImage != null) useItemImage = GameController.instance.useItemImage;
        else Debug.LogError("GameControllerのuseItemImageが設定されていません");

        if (GameController.instance.useItemTextPanel != null) useItemTextPanel = GameController.instance.useItemTextPanel;
        else Debug.LogError("GameControllerのuseItemTextPanelが設定されていません");

        if (GameController.instance.useItemNameText != null) useItemNameText = GameController.instance.useItemNameText;
        else Debug.LogError("GameControllerのuseItemNameTextが設定されていません");

        if (GameController.instance.useItemExplanationText != null) useItemExplanationText = GameController.instance.useItemExplanationText;
        else Debug.LogError("GameControllerのuseItemExplanationTextが設定されていません");

        //インベントリをリセットする
        ResetInventoryItem();

        //アイテムDBをリセット
        sO_Item.ResetItems();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //すべてのAudioSourceを取得
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < kAudioSourceLength)
        {
            //3つ目のAudioSourceが不足している場合、追加する
            audioSourceInventorySE = gameObject.AddComponent<AudioSource>();
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = kDefaultAudioSourceSEVolume;
        }
        else
        {
            //3番目のAudioSourceをアイテム使用音用に割り当て
            //(PlayerオブジェクトにこのスクリプトとPlayer.csをアタッチしている。
            //移動音とアイテム取得音の競合を回避する用)
            audioSourceInventorySE = audioSources[kAudioSourceLength - 1];
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = kDefaultAudioSourceSEVolume;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceInventorySE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        //useItemPanelが存在する場合
        if (useItemPanel != null)
        {
            //useItemPanelをnullに設定
            useItemPanel = null;
        }

        //useItemCountTextが存在する場合
        if (useItemCountText != null)
        {
            //useItemCountTextをnullに設定
            useItemCountText = null;
        }

        //useItemImageが存在する場合
        if (useItemImage != null)
        {
            //useItemImageをnullに設定
            useItemImage = null;
        }

        //useItemTextPanelが存在する場合
        if (useItemTextPanel != null)
        {
            //useItemTextPanelをnullに設定
            useItemTextPanel = null;
        }

        //useItemNameTextが存在する場合
        if (useItemNameText != null)
        {
            //useItemNameTextをnullに設定
            useItemNameText = null;
        }

        //useItemExplanationTextが存在する場合
        if (useItemExplanationText != null)
        {
            //useItemExplanationTextをnullに設定
            useItemExplanationText = null;
        }

        //インスタンスが存在する場合
        if (instance == this)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }

    private void Awake()
    {
        //シングルトンの設定
        if (instance == null)
        {
            instance = this;

            //シーン遷移時に破棄されないようにする
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            //すでにインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GetComponent<Player>();

        //Addressablesを初期化
        Addressables.InitializeAsync();

        //インベントリをリセット
        ResetInventoryItem();
    }

    void Update()
    {
        //インベントリアイテム使用
        if (UseInventoryItem() && !PauseController.instance.isPause && Time.timeScale != 0 
            && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.GetIsPlayerHidden()) UseItem();
    }

    /// <summary>
    /// 右クリック or U or Eでインベントリアイテム使用する関数
    /// </summary>
    /// <returns>右クリック or U or Eでtrue</returns>
    bool UseInventoryItem()
    {
        return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.U);
    }

    /// <summary>
    /// インベントリにアイテムを追加
    /// </summary>
    /// <param name="id">アイテムid</param>
    /// <param name="path">アイテムのパス</param>
    /// <param name="position">アイテムの生成位置</param>
    /// <param name="rotation">アイテムの回転</param>
    /// <param name="icon">アイテムの画像</param>
    /// <param name="itemName">アイテム名</param>
    /// <param name="description">アイテムの説明</param>
    /// <param name="count">アイテム個数</param>
    /// <param name="effectValue">アイテム効果値</param>
    public void GetItem(int id, string path, Vector3 position, Quaternion rotation, Sprite icon, string itemName, string description,int count, int effectValue)
    {
        //インベントリに新規追加する処理
        if (keepItemId == kNoneItemId)
        {

            //アイテムidを設定
            keepItemId = id;

            //アイテムプレハブのパスを設定
            keepItemPrefabPath = path;

            //アイテムの座標を設定
            keepItemSpawnPosition = position;

            //アイテムの回転値を設定
            keepItemSpawnRotation = rotation;

            //アイテム画像を設定
            useItemImage.sprite = icon;

            //アイテム効果値を設定
            keepItemEffectValue = effectValue;

            //アイテム画像の不透明度を100%にする
            useItemImage.color = opaqueColor;

            //アイテム所持数を設定
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            //アイテム名と説明文を設定
            useItemNameText.text = itemName;
            useItemExplanationText.text = description;

            //アイテム名と説明文の言語を設定する
            SettingLanguageText();
        }
        else
        {
            //アイテム所持数を追加
            keepItemCount = count;
            useItemCountText.text = count.ToString();
        }
    }

    /// <summary>
    /// アイテムを使用する
    /// </summary>
    public void UseItem() 
    {
        //アイテム所持数が0より大きい場合
        if (kMinKeepItemCount < keepItemCount)
        {
            //アイテム使用時の効果を適用する
            ActivationUseItem(keepItemId);

            //アイテム数が0になった場合
            if (keepItemCount == kMinKeepItemCount)
            {
                //インベントリをリセット
                ResetInventoryItem();
            }
        }
        else 
        {
            Debug.Log("使用できるアイテムitemがないようだ");
        }
    }

    /// <summary>
    /// 使用したアイテムのIDによって、それぞれの処理を行う
    /// </summary>
    /// <param name="keepItemId">アイテムID</param>
    async void ActivationUseItem(int keepItemId) 
    {
        //使用するアイテムIDによって処理を分岐
        switch (keepItemId) 
        {
            //スタミナ増強剤
            case kStaminaEnhancerId:
                //スタミナ増強剤SEを再生
                audioSourceInventorySE.clip = sO_SE.GetSEClip(useStaminaEnhancerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //スタミナ効果が適用中の場合
                if (isUseStaminaItem)
                {
                    //アイテムカウントを減少
                    --keepItemCount;
                    useItemCountText.text = keepItemCount.ToString();
                    sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                    //処理をスキップ
                    return;
                }

                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //スタミナゲージの元の色を保存
                Color keepStaminaColor = Player.instance.staminaSlider.fillRect.GetComponent<Image>().color;

                //スタミナ消費率を12.5%に変更
                Player.instance.SetStaminaConsumeRatio(kSpecifiedStaminaConsumeRatio);

                //スタミナ回復値を30.0に変更
                Player.instance.SetStaminaRecoveryRatio(kSpecifiedStaminaRecoveryRatio);

                //スタミナゲージの色を緑色に変更
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = greenColor;

                //スタミナ増強剤使用フラグをtrueに設定
                isUseStaminaItem = true;

                //効果時間待機
                await UniTask.Delay(TimeSpan.FromSeconds(keepItemEffectValue));

                //スタミナ消費率を元に戻す
                Player.instance.SetStaminaConsumeRatio(Player.instance.GetDefaultStaminaConsumeRatio());

                //スタミナ回復値を元に戻す
                Player.instance.SetStaminaRecoveryRatio(Player.instance.GetStaminaRecoveryRatio());

                //スタミナゲージの色を元に戻す
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = keepStaminaColor;

                //スタミナ増強剤使用フラグをfalseに設定
                isUseStaminaItem = false;
                break;

            //クラッカー
            case kCrackerId:

                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //クラッカーSEを再生
                audioSourceInventorySE.clip = sO_SE.GetSEClip(crackerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //クラッカー使用フラグをオン
                isUseCrackerItem = true;

                //クラッカー関連処理
                CrackerRelatedProcessing();
                break;

            //テスト用使用アイテム①
            case 995:
                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //ローカル座標をワールド座標に変換
                Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
                Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

                //Addressablesを使用してプレハブをステージ上に非同期生成
                await Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
                break;
        }
    }

    /// <summary>
    /// クラッカー関連処理
    /// </summary>
    private void CrackerRelatedProcessing() 
    {
        //Raycastの開始位置をカメラの後方に設定(敵の至近距離で発動しても当たらない問題を防ぐため)
        castStartPosition = Camera.main.transform.position + (Camera.main.transform.forward * kCastStartPositionForward);

        //カメラの前方に飛ばした敵検知用のRaycastをSphereCastに変更(Raycastの太さを太くするため)
        isRacastHit = Physics.SphereCast(castStartPosition, kCrackerRaycastRadius
            , Camera.main.transform.forward, out crackerRaycast, kCrackerRaycastDistance, enemyLayer);

        //クラッカー使用時のパーティクルシステムを表示位置に移動
        spawnCrackerParticlePosition = Camera.main.transform.position + Camera.main.transform.forward;

        //Prefabからシーン内にパーティクルのクローンを生成する
        spawnedCrackerParticle = Instantiate(crackerParticleSystemPrefab, spawnCrackerParticlePosition, Quaternion.identity);

        spawnedCrackerParticle.gameObject.SetActive(true);

        //クラッカー使用時のパーティクルシステムを再生
        spawnedCrackerParticle.Play();

        //音を鳴らすフラグをtrueに設定
        Player.instance.SetIsMakeSound(true);

        //Raycastが当たった場合
        if (isRacastHit)
        {
            //当たったオブジェクトを取得
            //pickUpEnemy = crackerRaycast.collider.gameObject;
            pickUpEnemy = crackerRaycast.transform.gameObject;

            //BaseEnemyコンポーネントを取得
            baseEnemy = pickUpEnemy.GetComponent<BaseEnemy>();

            //彷徨う者が既にダメージを受けている状態の場合
            if (baseEnemy.GetIsReceiveDamage())
            {
                //処理をスキップ
                return;
            }

            //一定時間スタンさせる
            baseEnemy.SetIsReceiveDamage(true);
        }

        //SphereCastへの変換用フラグをfalseに設定
        isRacastHit = false;

        //クラッカー使用フラグをオフ
        isUseCrackerItem = false;

        //クラッカー使用時のパーティクルシステムの再生が終了したら、パーティクルシステムを破棄するように設定する
        var mainModule = spawnedCrackerParticle.main;
        mainModule.stopAction = ParticleSystemStopAction.Destroy;
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    public void SettingLanguageText() 
    {
        //使用アイテムを所持していない場合
        if (keepItemId == kNoneItemId) 
        {
            //処理をスキップ
            return;
        }

        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            case LanguageController.LanguageStatus.kJapanese:
                //日本語の場合は、使用アイテム名称テキストを日本語にする
                useItemNameText.text = itemMessage.itemMessage[keepItemId].itemNameJapanese;

                //使用アイテム名称を日本語用にサイズを設定する
                useItemNameText.fontSize = itemMessage.itemMessage[keepItemId].itemNameSizeJapanese;

                //説明テキストも日本語にする
                useItemExplanationText.text = itemMessage.itemMessage[keepItemId].itemDescriptionJapanese;

                //使用アイテム説明テキストサイズを日本語用に設定する
                useItemExplanationText.fontSize = itemMessage.itemMessage[keepItemId].itemDescriptionSizeJapanese;
                break;

            case LanguageController.LanguageStatus.kEnglish:
                //\使用アイテム名称テキストを英語にする
                useItemNameText.text = itemMessage.itemMessage[keepItemId].itemNameEnglish;

                //使用アイテム名称を英語用にサイズを設定する
                useItemNameText.fontSize = itemMessage.itemMessage[keepItemId].itemNameSizeEnglish;

                //説明テキストも英語にする
                useItemExplanationText.text = itemMessage.itemMessage[keepItemId].itemDescriptionEnglish;

                //使用アイテム説明テキストサイズを英語用に設定する
                useItemExplanationText.fontSize = itemMessage.itemMessage[keepItemId].itemDescriptionSizeEnglish;
                break;

            default:
                Debug.LogWarning("その他の言語ステータス");
                break;
        }
    }



    /// <summary>
    /// インベントリをリセットする
    /// </summary>
    void ResetInventoryItem() 
    {
        //それぞれの変数の値・フラグ値を初期化する
        keepItemId = kNoneItemId;
        keepItemCount = kMinKeepItemCount;
        keepItemEffectValue = kDefaultKeepItemEffectValue;
        useItemCountText.text = keepItemCount.ToString();
        useItemNameText.text = "";
        useItemExplanationText.text = "";
        keepItemPrefabPath = kNoneItemPrefabPath;
        keepItemSpawnPosition = defaultItemSpawnPosition;
        keepItemSpawnRotation = defaultItemSpawnRotation;
        useItemImage.sprite = null;
        useItemImage.color = defaultUseItemImageColor;
        isUseStaminaItem = false;
        isUseCrackerItem = false;
    }
    
    /// <summary>
    /// シーンビュー表示用のデバッグコード
    /// </summary>
    private void OnDrawGizmos()
    {
        // 実行中かつメインカメラが存在するときだけ描画
        if (Camera.main == null) return;

        Transform camTransform = Camera.main.transform;
        Vector3 startCenter = camTransform.position + (camTransform.forward * kCastStartPositionForward);
        Vector3 direction = camTransform.forward;

        // ギズモの色を設定（半透明の赤）
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

        // 1. スタート地点の球体を描画
        Gizmos.DrawSphere(startCenter, kCrackerRaycastRadius);

        // 2. ヒット状況に応じた終点（または最大距離）の球体と線を描画
        Vector3 endCenter;

        // アプリケーションが実行中で、実際に実行した結果（isRacastHit）がある場合
        if (Application.isPlaying)
        {
            if (isRacastHit)
            {
                // ヒットした場合は、その衝突位置（球体の中心）を終点にする
                endCenter = startCenter + direction * crackerRaycast.distance;
                Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // ヒット時は緑に
            }
            else
            {
                // 外れた場合は最大距離を終点にする
                endCenter = startCenter + direction * kCrackerRaycastDistance;
            }
        }
        else
        {
            // エディタ非実行（停止中）のプレビュー用
            endCenter = startCenter + direction * kCrackerRaycastDistance;
        }

        // 終点の球体を描画
        Gizmos.DrawSphere(endCenter, kCrackerRaycastRadius);

        // スタートと終点をつなぐ線を描画（太さの目安）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startCenter, endCenter);

        // 周囲に線を4本引くと、よりカプセル（太さ）っぽく見えます
        Gizmos.DrawLine(startCenter + camTransform.up * kCrackerRaycastRadius, endCenter + camTransform.up * kCrackerRaycastRadius);
        Gizmos.DrawLine(startCenter - camTransform.up * kCrackerRaycastRadius, endCenter - camTransform.up * kCrackerRaycastRadius);
        Gizmos.DrawLine(startCenter + camTransform.right * kCrackerRaycastRadius, endCenter + camTransform.right * kCrackerRaycastRadius);
        Gizmos.DrawLine(startCenter - camTransform.right * kCrackerRaycastRadius, endCenter - camTransform.right * kCrackerRaycastRadius);
    }
}
