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

    /// <summary>
    /// 拾ったアイテム
    /// </summary>
    GameObject pickUpItem;

    /// <summary>
    /// 開閉したいドア
    /// </summary>
    GameObject interactDoor;

    /// <summary>
    /// 点けたいステージライト
    /// </summary>
    GameObject interactStageLight;

    /// <summary>
    /// 開閉したい引き出し
    /// </summary>
    GameObject interactDrawer;

    /// <summary>
    /// インタラクトフラグ
    /// </summary>
    public bool isInteract;

    /// <summary>
    /// Item.cs
    /// </summary>
    private Item item;

    /// <summary>
    /// Door.cs
    /// </summary>
    private Door door;

    /// <summary>
    /// Goal.cs
    /// </summary>
    private Goal goal;

    /// <summary>
    /// StageLight.cs
    /// </summary>
    private StageLight stageLight;

    /// <summary>
    /// Drawer.cs
    /// </summary>
    private Drawer drawer;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

    /// <summary>
    /// アイテムタグ
    /// </summary>
    private string itemTag = "Item";

    /// <summary>
    /// ドアタグ
    /// </summary>
    private string doorTag = "Door";

    /// <summary>
    /// ゴールタグ
    /// </summary>
    private string goalTag = "Goal";

    /// <summary>
    /// ステージライトタグ
    /// </summary>
    private string stageLightTag = "StageLight";

    /// <summary>
    /// 引き出しタグ
    /// </summary>
    private string drawerTag = "Drawer";

    /// <summary>
    /// アウトラインタグ
    /// </summary>
    private string outlineTag = "Outline";

    /// <summary>
    /// アイテムレイヤー
    /// </summary>
    private string itemLayer = "Item";

    /// <summary>
    /// ドアレイヤー
    /// </summary>
    private string doorLayer = "Door";

    /// <summary>
    /// ゴールレイヤー
    /// </summary>
    private string goalLayer = "Goal";

    /// <summary>
    /// ゴールレイヤー
    /// </summary>
    private string stageLightLayer = "StageLight";

    /// <summary>
    /// 引き出しレイヤー
    /// </summary>
    private string drawerLayer = "Drawer";

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// アイテム取得音専用audioSource
    /// </summary>
    private AudioSource audioSourceItemSE;

    /// <summary>
    /// アイテム取得時のSEのID
    /// </summary>
    private readonly int getItemSEid = 2;

    [Header("アイテムリセット(デバッグ用)")]
    public bool isDebugResetItem = false;

    /// <summary>
    /// 現在照準が当たっているオブジェクトを追跡
    /// </summary>
    private GameObject currentHighlightedObject;

    /// <summary>
    /// オブジェクトの元のレイヤーを保存
    /// </summary>
    private int originalLayer;

    /// <summary>
    /// 現在照準が当たっているオブジェクトのタグを保存
    /// </summary>
    private string currentObjectTag;

    private void Start()
    {
        isInteract = false;

        //AudioSourceの初期化
        InitializeAudioSource();

        //アイテムデータをリセット(デバッグ時以外でもリセットを行うのかは要検討)
        if (isDebugResetItem) sO_Item.ResetItems();
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
    /// シーン遷移時にAudioSourceを再設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceItemSE != null)
        {
            audioSourceItemSE.volume = volume;
        }
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {       
        //すべてのAudioSourceを取得
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2)
        {
            //2つ目のAudioSourceが不足している場合、追加する
            audioSourceItemSE = gameObject.AddComponent<AudioSource>();
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }
        else
        {
            //2番目のAudioSourceをアイテム取得音用に割り当て
            //(PlayerオブジェクトにこのスクリプトとPlayer.csをアタッチしている。
            //移動音とアイテム取得音の競合を回避する用)
            audioSourceItemSE = audioSources[1];
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceItemSE.outputAudioMixerGroup = MusicController.Instance.audioMixerGroupSE;
    }

    private void Update()
    {
        //Rayを可視化
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.green, 3);

        //インタラクト
        Interact();

        //強調処理を追加
        HighlightObject(); 
    }

    /// <summary>
    /// インタラクト処理
    /// </summary>
    async void Interact() 
    {
        RaycastHit raycastHit;

        //Cameraから飛ばしているRayにオブジェクトが当たった場合
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, distance) )
        {
            //Rayにオブジェクトが当たった状態でインタラクト操作を行う場合
            if (PlayInteract() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
            {
                //アイテム
                if (raycastHit.transform.tag == itemTag)
                {
                    isInteract = true;

                    //対象のアイテム
                    pickUpItem = raycastHit.transform.gameObject;

                    //Itemコンポーネントを取得
                    item = pickUpItem.GetComponent<Item>();

                    //対象のアイテムを拾う場合
                    if (item != null)
                    {

                        if (sO_Item == null) Debug.LogError("SO_Itemが初期化されていません！");

                        //対象アイテムがドキュメントorミステリーアイテムの場合
                        if ((item.itemType == ItemType.Document) || (item.itemType == ItemType.MysteryItem))
                        {
                            //ポーズ画面内のアイテムのパネル内に追加する
                            sO_Item.AddDocumentORMysteryItem(item);
                            
                            //拾ったアイテムをステージ上から削除
                            DestroyItem(pickUpItem);
                        }
                        //対象アイテムがプレイヤーが使用できるアイテムの場合
                        else if (item.itemType == ItemType.UseItem)
                        {
                            //インベントリに空きがあるかを確認
                            if ((Inventory.instance.keepItemId == 99999) || (Inventory.instance.keepItemId == item.id))
                            {
                                //インベントリに追加
                                sO_Item.AddUseItem(item);

                                //拾ったアイテムをステージ上から削除
                                DestroyItem(pickUpItem);
                            }
                            //インベントリのアイテムがいっぱいの場合の処理
                            else
                            {
                                //インベントリに空きがない旨のメッセージを表示
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

                    //対象のドア
                    interactDoor = raycastHit.transform.gameObject;

                    //ドアのコンポーネント取得
                    door = interactDoor.GetComponent<Door>();

                    //対象のドアを開閉
                    if (door != null) door.DoorSystem();                    
                }

                //ステージライト
                if (raycastHit.transform.tag == stageLightTag)
                {
                    isInteract = true;

                    //対象のステージライト
                    interactStageLight = raycastHit.transform.gameObject;

                    //ステージライトのコンポーネント取得
                    stageLight = interactStageLight.GetComponent<StageLight>();

                    //対象のステージライトを点灯
                    if (stageLight != null) stageLight.LitStageLight();
                }

                //引き出し
                if (raycastHit.transform.tag == drawerTag)
                {
                    isInteract = true;

                    //対象の引き出し
                    interactDrawer = raycastHit.transform.gameObject;

                    //引き出しのコンポーネントを取得
                    drawer = interactDrawer.GetComponent<Drawer>();

                    //対象の引き出しを開閉
                    if (drawer != null) drawer.DrawerSystem();
                }

                //ゴール
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;

                    //対象のゴール
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //ゴールチェック
                    if (!goal.isGoalPanel && goal != null) 
                    {
                        //ゴールの処理
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

            //Rayが何にも当たっていない場合もリセット
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
    }
 
    /// <summary>
    /// 現在のオブジェクトのレイヤーを元に戻す
    /// </summary>
    void ResetLayer()
    {
        if (currentHighlightedObject != null)
        {
            string targetLayer = "";
            //タグに応じて元のレイヤーを決定
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

        //Cameraから飛ばしているRayにオブジェクトが当たった場合
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out raycastHit, distance))
        {
            //インタラクト可能なオブジェクトのいずれかに当たった場合
            if (raycastHit.transform.tag == itemTag ||
                raycastHit.transform.tag == doorTag ||
                raycastHit.transform.tag == goalTag ||
                raycastHit.transform.tag == stageLightTag ||
                raycastHit.transform.tag == drawerTag)
            {
                //Rayがヒットしたオブジェクト
                GameObject hitObject = raycastHit.transform.gameObject;

                //現在の強調対象が異なる場合、前の強調を解除
                if (currentHighlightedObject != hitObject)
                {
                    //前のオブジェクトのレイヤーを元に戻す
                    ResetLayer(); 
                    currentHighlightedObject = hitObject;


                    //現在のレイヤーを保存
                    originalLayer = currentHighlightedObject.layer;

                    //現在のオブジェクトのタグを保存
                    currentObjectTag = currentHighlightedObject.tag;

                    //レイヤーをOutlineに変更
                    SwitchLayer(currentHighlightedObject, outlineTag);
                }
            }
            else
            {
                //インタラクト可能なオブジェクト以外に当たった場合、強調を解除
                ResetLayer();
            }
        }
        else
        {
            //Rayが何にも当たっていない場合、強調を解除
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
            //アイテム取得時の効果音を再生
            audioSourceItemSE.clip = sO_SE.GetSEClip(getItemSEid);
            audioSourceItemSE.loop = false;
            audioSourceItemSE.Play();
        }
        else
        {
            Debug.LogWarning($"AudioSource or getItemSE is null in DestroyItem");
        }

        //アイテムを削除
        Destroy(pickUpItem);

        //アイテムを拾ったらレイヤーをリセット
        ResetLayer();
    }
}
