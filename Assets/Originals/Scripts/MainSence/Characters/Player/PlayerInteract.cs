using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PlayerInteract : MonoBehaviour
{
    [Header("インタラクトできる距離")]
    [SerializeField]  public float distance = 3f;
    GameObject pickUpItem;//拾ったアイテム
    GameObject interactDoor;//インタラクトするドア
    GameObject interactStageLight;//インタラクトするステージライト
    GameObject interactDrawer;//インタラクトする引き出し
    public bool isInteract;

    private Item item;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;


    private Door door;
    private Goal goal;
    private StageLight stageLight;
    private Drawer drawer;


    private string itemTag = "Item";
    private string doorTag = "Door";
    private string goalTag = "Goal";
    private string stageLightTag = "StageLight";
    private string drawerTag = "Drawer";
    private string outlineTag = "Outline";
    private string itemLayer = "Item"; // アイテムの元のレイヤー
    private string doorLayer = "Door"; // ドアの元のレイヤー
    private string goalLayer = "Goal"; // ゴールの元のレイヤー
    private string stageLightLayer = "StageLight"; // ゴールの元のレイヤー
    private string drawerLayer = "Drawer"; // 引き出しの元のレイヤー

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("SE関係")]
    //private AudioSource audioSourceSE;
    private AudioSource audioSourceItemSE; // アイテム取得音専用
    //[SerializeField] private AudioClip getItemSE;
    private readonly int getItemSEid = 2;//アイテム取得時のSEのID

    [Header("アイテムリセット(デバッグ用)")]
    public bool isDebugResetItem = false;


    // 現在照準が当たっているオブジェクトを追跡
    private GameObject currentHighlightedObject;
    // オブジェクトの元のレイヤーを保存
    private int originalLayer;
    // 現在照準が当たっているオブジェクトのタグを保存
    private string currentObjectTag;

    private void Start()
    {
        isInteract = false;

        // AudioSourceの初期化
        InitializeAudioSource();

        if (isDebugResetItem) sO_Item.ResetItems();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定するためのイベント登録解除
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移時にAudioSourceを再設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {       
        // すべての AudioSource を取得
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2)
        {
            // 2つ目の AudioSource が不足している場合、追加
            audioSourceItemSE = gameObject.AddComponent<AudioSource>();
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }
        else
        {
            // 2番目の AudioSource をアイテム取得音用に割り当て
            //(PlayerオブジェクトにこのスクリプトとPlayer.csをアタッチしている。
            //移動音とアイテム取得音の競合を回避する用)
            audioSourceItemSE = audioSources[1];
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }


    }

    private void Update()
    {
        //Rayを可視化
        Debug.DrawRay(Camera.main.transform.position, 
            Camera.main.transform.forward, Color.green, 3);

        Interact();

        // 強調処理を追加
        HighlightObject(); 
    }

    /// <summary>
    /// インタラクト処理
    /// </summary>
    async void Interact() 
    {
        RaycastHit raycastHit;

        //CameraからRayを飛ばす
        if (Physics.Raycast(Camera.main.transform.position, 
            Camera.main.transform.forward, out raycastHit, distance) )
        {
            if (PlayInteract() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
            {
                //アイテム
                if (raycastHit.transform.tag == itemTag)
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Itemコンポーネントを取得
                    item = pickUpItem.GetComponent<Item>();

                    //アイテムを拾う
                    if (item != null)
                    {
                        if (sO_Item == null) Debug.LogError("SO_Itemが初期化されていません！");

                        Debug.Log($"拾ったアイテムのタイプ: {item.itemType}");

                        if ((item.itemType == ItemType.Document) 
                            || (item.itemType == ItemType.MysteryItem))
                        {
                            sO_Item.AddDocumentORMysteryItem(item);
                            Debug.Log("Player側SO_ItemのインスタンスID: " + sO_Item.GetInstanceID());
                            DestroyItem(pickUpItem);
                        }
                        else if (item.itemType == ItemType.UseItem)
                        {
                            if ((Inventory.instance.keepItemId == 99999) || (Inventory.instance.keepItemId == item.id))
                            {
                                sO_Item.AddUseItem(item);
                                DestroyItem(pickUpItem);
                            }
                            else 
                            {
                                //インベントリのアイテムがいっぱいの場合の処理
                                MessageController.instance.ShowInventoryMessage(1);

                                await UniTask.Delay(TimeSpan.FromSeconds(3));

                                MessageController.instance.ResetMessage();
                            }
                        }

                    }
                    else 
                    {
                        Debug.LogError("Itemコンポーネントがアタッチされていません: " + pickUpItem.name);
                    }
                    
                }

                //ドア
                if (raycastHit.transform.tag == doorTag) 
                {
                    isInteract = true;
                    interactDoor = raycastHit.transform.gameObject;
                    door = interactDoor.GetComponent<Door>();

                    //ドアの開閉
                    if (door != null) door.DoorSystem();                    
                }

                //ステージライト
                if (raycastHit.transform.tag == stageLightTag)
                {
                    isInteract = true;
                    interactStageLight = raycastHit.transform.gameObject;
                    stageLight = interactStageLight.GetComponent<StageLight>();

                    //ステージライトを点灯
                    if (stageLight != null) stageLight.LitStageLight();
                }

                //引き出し
                if (raycastHit.transform.tag == drawerTag)
                {
                    isInteract = true;
                    interactDrawer = raycastHit.transform.gameObject;
                    drawer = interactDrawer.GetComponent<Drawer>();

                    //ドアの開閉
                    if (drawer != null) drawer.DrawerSystem();
                }

                //ゴール
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //ゴールチェック
                    if (!goal.isGoalPanel && goal != null) 
                    {
                        goal.GoalCheck();

                        //ゴールパネルを非表示にする際に、
                        //goal.isGoalPanelがtrueのままになってしまうバグを防ぐ
                        goal.isGoalPanel = false;
                    } 
                }
            }   
        }
        else
        {
            isInteract = false;
            goal = null;

            // Rayが何にも当たっていない場合もリセット
            ResetLayer(); 
        }
    }

    /// <summary>
    /// 左クリック・Rボタンでインタラクト操作
    /// Interact…"joystick button 5"を割り当てている。コントローラーではRボタンになる
    /// </summary>
    /// <returns>ボタン押下でtrue</returns>
    bool PlayInteract() 
    {
        return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Interact");
    }


    /// <summary>
    /// オブジェクトのレイヤーを変更する
    /// </summary>
    /// <param name="obj">対象オブジェクト</param>
    /// <param name="layerName">レイヤー名</param>
    void SwitchLayer(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"レイヤー '{layerName}' が見つかりません。プロジェクト設定で確認してください。");
            return;
        }
        obj.layer = layer;
        Debug.Log($"オブジェクト {obj.name} のレイヤーを {layerName} へ変更");
    }
 
    /// <summary>
    /// 現在のオブジェクトのレイヤーを元に戻す
    /// </summary>
    void ResetLayer()
    {
        if (currentHighlightedObject != null)
        {
            string targetLayer = "";
            // タグに応じて元のレイヤーを決定
            if (currentObjectTag == itemTag)
            {
                targetLayer = itemLayer;
            }
            else if (currentObjectTag == doorTag)
            {
                targetLayer = doorLayer;
            }
            else if (currentObjectTag == goalTag)
            {
                targetLayer = goalLayer;
            }
            else if (currentObjectTag == stageLightTag) 
            {
                targetLayer = stageLightLayer;
            }
            else if (currentObjectTag == drawerTag)
            {
                targetLayer = drawerLayer;
            }
            else
            {
                Debug.LogWarning($"オブジェクト {currentHighlightedObject.name} のタグ {currentObjectTag} は認識されません。'Default' にフォールバックします。");
                targetLayer = "Default";
            }

            SwitchLayer(currentHighlightedObject, targetLayer);
            Debug.Log($"オブジェクト {currentHighlightedObject.name} のレイヤーを {targetLayer} に戻しました");
            currentHighlightedObject = null;
            currentObjectTag = null;
        }
    }
 
    /// <summary>
    /// インタラクト可能なオブジェクトを強調表示する処理
    /// </summary>
    void HighlightObject()
    {
        RaycastHit raycastHit;

        // Camera から Ray を飛ばす
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out raycastHit, distance))
        {
            // アイテム、ドア、ゴールのいずれかに当たった場合
            if (raycastHit.transform.tag == itemTag ||
                raycastHit.transform.tag == doorTag ||
                raycastHit.transform.tag == goalTag ||
                raycastHit.transform.tag == stageLightTag ||
                raycastHit.transform.tag == drawerTag)
            {
                GameObject hitObject = raycastHit.transform.gameObject;

                // 現在の強調対象が異なる場合、前の強調を解除
                if (currentHighlightedObject != hitObject)
                {
                    ResetLayer(); // 前のオブジェクトのレイヤーを元に戻す
                    currentHighlightedObject = hitObject;
                    // 現在のレイヤーを保存
                    originalLayer = currentHighlightedObject.layer;
                    // 現在のオブジェクトのタグを保存
                    currentObjectTag = currentHighlightedObject.tag;
                    // レイヤーを Outline に変更
                    SwitchLayer(currentHighlightedObject, outlineTag);
                }
            }
            else
            {
                // インタラクト可能なオブジェクト以外に当たった場合、強調を解除
                ResetLayer();
            }
        }
        else
        {
            // Ray が何にも当たっていない場合、強調を解除
            ResetLayer();
        }
    }

    /// <summary>
    /// 入手アイテムのゲームオブジェクトをシーンのフィールド上から削除する
    /// </summary>
    /// <param name="pickUpItem">入手アイテム</param>
    void DestroyItem(GameObject pickUpItem) 
    {
        if (audioSourceItemSE != null && sO_SE.GetSEClip(getItemSEid) != null)
        {
            // アイテム取得時の効果音を再生
            audioSourceItemSE.clip = sO_SE.GetSEClip(getItemSEid);
            audioSourceItemSE.loop = false;
            audioSourceItemSE.Play();
        }
        else
        {
            Debug.LogWarning($"AudioSource or getItemSE is null in DestroyItem");
        }

        Destroy(pickUpItem);

        // アイテムを拾ったらレイヤーをリセット
        ResetLayer();
    }
}
