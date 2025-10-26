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
    private const int noneItemId = 99999;

    /// <summary>
    /// アイテムのプレハブのAddressables名
    /// </summary>
    private string keepItemPrefabPath;

    /// <summary>
    /// アイテムのプレハブのAddressables名(空白)
    /// </summary>
    private const string noneItemPrefabPath = "";

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
    private const int minKeepItemCount = 0;

    /// <summary>
    /// アイテム効果値
    /// </summary>
    private int keepItemEffectValue;

    /// <summary>
    /// アイテム効果値(デフォルト値)
    /// </summary>
    private const int defaultKeepItemEffectValue = 0;

    /// <summary>
    /// スタミナ増強剤使用フラグ
    /// </summary>
    private bool isUseStaminaItem;

    /// <summary>
    /// Player.cs
    /// </summary>
    private Player player;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// Inventory専用のAudioSource
    /// </summary>
    private AudioSource audioSourceInventorySE;

    /// <summary>
    /// スタミナ増強剤SEのID
    /// </summary>
    private readonly int useStaminaEnhancerSEid = 12;

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
        if (audioSources.Length < 3)
        {
            //3つ目のAudioSourceが不足している場合、追加する
            audioSourceInventorySE = gameObject.AddComponent<AudioSource>();
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
        else
        {
            //3番目のAudioSourceをアイテム使用音用に割り当て
            //(PlayerオブジェクトにこのスクリプトとPlayer.csをアタッチしている。
            //移動音とアイテム取得音の競合を回避する用)
            audioSourceInventorySE = audioSources[2];
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceInventorySE.outputAudioMixerGroup = MusicController.Instance.audioMixerGroupSE;
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
        if (UseInventoryItem() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) UseItem();
    }

    /// <summary>
    /// 右クリックでインベントリアイテム使用する関数
    /// </summary>
    /// <returns>右クリックでtrue</returns>
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
        //インベントリに新規追加する処理
        if (keepItemId == noneItemId)
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
            useItemImage.color = new Color(255, 255, 255, 1);

            //アイテム所持数を設定
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            //アイテム名と説明文を設定
            useItemNameText.text = itemName;
            useItemExplanationText.text = description;          
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
        if (minKeepItemCount < keepItemCount)
        {
            //アイテム使用時の効果を適用する
            ActivationUseItem(keepItemId);

            //アイテム数が0になった場合
            if (keepItemCount == minKeepItemCount)
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
            case 11:
                //スタミナ効果が適用中の場合
                if (isUseStaminaItem)
                {
                    //スタミナ効果が適用中である旨のメッセージを表示
                    MessageController.instance.ShowInventoryMessage(3);

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    MessageController.instance.ResetMessage();
                    return;
                }

                //アイテムカウントを減少
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //スタミナゲージの元の色を保存
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
    /// インベントリをリセットする
    /// </summary>
    void ResetInventoryItem() 
    {
        //それぞれの変数の値・フラグ値を初期化する
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
