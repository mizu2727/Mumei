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
    public static Inventory instance;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("使用アイテムパネル関連(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject useItemPanel;//使用アイテム確認パネル
    [SerializeField] private Text useItemCountText;//使用アイテム所持カウントテキスト
    [SerializeField] private Image useItemImage;//使用アイテム画像
    [SerializeField] public GameObject useItemTextPanel;//使用アイテムテキスト確認パネル
    [SerializeField] public Text useItemNameText;//使用アイテム名テキスト
    [SerializeField] public Text useItemExplanationText;//使用アイテム説明テキスト


    //アイテムID管理(アイテムインベントリ複数枠分用)
    private readonly List<int> idList = new();

    [Header("アイテムID管理(アイテムインベントリ1枠分用。編集禁止)")]
    public int keepItemId;

    //アイテム未所持時のID(アイテムインベントリ1枠分用)
    private const int noneItemId = 99999;

    //アイテムのプレハブのAddressables名
    private string keepItemPrefabPath;

    //アイテムのプレハブのAddressables名(空白)
    private const string noneItemPrefabPath = "";

    //アイテム生成位置
    private Vector3 keepItemSpawnPosition;

    //アイテム生成位置(デフォルト)
    private Vector3 defaultItemSpawnPosition = new Vector3(0, 0, 0);

    //アイテムの回転数値
    private Quaternion keepItemSpawnRotation;

    //アイテムの回転数値(デフォルト)
    private Quaternion defaultItemSpawnRotation = Quaternion.identity;

    //アイテム所持数管理(アイテムインベントリ複数枠分用)
    private readonly List<int> countList = new();

    //アイテム所持数管理(アイテムインベントリ1枠分用)
    private int keepItemCount;

    private const int minKeepItemCount = 0;

    //アイテム効果値
    private int keepItemEffectValue;

    //アイテム効果値(デフォルト値)
    private const int defaultKeepItemEffectValue = 0;

    //アイテムリストのインデックス番号
    int checkIndex;

    // スタミナ増強効果のタスクを管理するためのCancellationTokenSource
    //private CancellationTokenSource staminaEffectCts;

    //スタミナ増強剤を使用しているかを判定
    private bool isUseStaminaItem;

    private Player player;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("サウンド関連")]
    private AudioSource audioSourceInventorySE;//Inventory専用のAudioSource
    private readonly int useStaminaEnhancerSEid = 12; // スタミナ増強剤SEのID


    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時に使用アイテムパネル関連を再設定するためのイベント登録解除
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //CancelStaminaEffect();
    }

    /// <summary>
    /// シーン遷移時に使用アイテムパネル関連を再設定及びリセット
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();

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

        ResetInventoryItem();
        //アイテムDBをリセット
        sO_Item.ResetItems();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        // すべての AudioSource を取得
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 3)
        {
            // 3つ目の AudioSource が不足している場合、追加
            audioSourceInventorySE = gameObject.AddComponent<AudioSource>();
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
        else
        {
            // 3番目の AudioSource をアイテム取得音用に割り当て
            //(PlayerオブジェクトにこのスクリプトとPlayer.csをアタッチしている。
            //移動音とアイテム取得音の競合を回避する用)
            audioSourceInventorySE = audioSources[2];
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
    }

    private void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時に破棄されないようにする（必要に応じて）
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は破棄
        }
    }

    void Start()
    {
        player = GetComponent<Player>();
        //Addressablesを初期化
        Addressables.InitializeAsync();
        ResetInventoryItem();
    }

    void Update()
    {
        if (UseInventoryItem() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) UseItem();
    }

    /// <summary>
    /// 右クリックでインベントリアイテム使用する関数
    /// </summary>
    /// <returns>true / false</returns>
    bool UseInventoryItem()
    {
        return Input.GetMouseButtonDown(1);
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
        //リストの中にアイテムが何番目に存在するのかを確認
        //存在しない場合は-1を返す
        //checkIndex = idList.IndexOf(id);

        //インベントリに新規追加する処理
        //checkIndex == -1
        if (keepItemId == noneItemId)
        {

            //　アイテムidを設定
            idList.Add(id);

            keepItemId = id;
            keepItemPrefabPath = path;
            keepItemSpawnPosition = position;
            keepItemSpawnRotation = rotation;
            useItemImage.sprite = icon;
            keepItemEffectValue = effectValue;
            useItemImage.color = new Color(255, 255, 255, 1);

            //　アイテム所持数を設定
            countList.Add(count);
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            //　アイテム名と説明文を設定
            useItemNameText.text = itemName;
            useItemExplanationText.text = description;

            Debug.Log("id:" + id + "のアイテムitemを" + count + "個新規追加");
            Debug.Log("keepItemCount(使用アイテム):" + keepItemCount);
            
        }
        else
        {
            //アイテム所持数を追加
            countList[checkIndex] += count;
            keepItemCount = count;
            useItemCountText.text = count.ToString();


            Debug.Log("id:" + id + "のアイテムitemを" + count + "個へ増加");

            Debug.Log("keepItemCount(使用アイテム):" + keepItemCount);
        }
    }

    /// <summary>
    /// アイテムを使用する
    /// </summary>
    public void UseItem() 
    {
        if (minKeepItemCount < keepItemCount)
        {
            ActivationUseItem(keepItemId);
            Debug.Log("id:" + keepItemId + "のkeepItemCount(使用アイテムの個数):" + keepItemCount);

            if (keepItemCount == minKeepItemCount)
            {
                //アイテム数が0になった場合の処理
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
            case 11:
                // スタミナ効果が適用中の場合
                if (isUseStaminaItem)
                {
                    MessageController.instance.ShowInventoryMessage(3);

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    MessageController.instance.ResetMessage();
                    return;
                }

                // アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                // スタミナゲージの元の色を保持
                Color keepStaminaColor = Player.instance.staminaSlider.fillRect.GetComponent<Image>().color;

                //スタミナ増強剤SEを再生
                audioSourceInventorySE.clip = sO_SE.GetSEClip(useStaminaEnhancerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //スタミナ消費率を25%に変更し、スタミナゲージの色を緑色に変更
                Player.instance.staminaConsumeRatio = 25;
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                isUseStaminaItem = true;

                //効果時間待機
                await UniTask.Delay(TimeSpan.FromSeconds(keepItemEffectValue));

                //スタミナ消費率を50%に戻して、スタミナゲージの色を元に戻す
                Player.instance.staminaConsumeRatio = 50;
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = keepStaminaColor;
                isUseStaminaItem = false;
                break;

            //テスト用使用アイテム①
            case 995:
                // アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                // ローカル座標をワールド座標に変換
                Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
                Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

                // Addressablesを使用してプレハブをステージ上に非同期生成
                await Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
                break;
        }
    }

    /// <summary>
    /// インベントリをリセットする
    /// </summary>
    void ResetInventoryItem() 
    {
        keepItemId = noneItemId;
        keepItemCount = minKeepItemCount;
        keepItemEffectValue = defaultKeepItemEffectValue;
        useItemCountText.text = keepItemCount.ToString();
        useItemNameText.text = "";
        useItemExplanationText.text = "";
        keepItemPrefabPath = noneItemPrefabPath;
        keepItemSpawnPosition = defaultItemSpawnPosition;
        keepItemSpawnRotation = defaultItemSpawnRotation;
        useItemImage.sprite = null;
        useItemImage.color = new Color(255, 255, 255, 0.05f);
        isUseStaminaItem = false;
    }
}
