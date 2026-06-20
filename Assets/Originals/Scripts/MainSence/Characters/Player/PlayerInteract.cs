using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    /// 隠れたいオブジェクト
    /// </summary>
    GameObject targetHiddenObject;


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

    /// <summary>
    /// HiddenObject.cs
    /// </summary>
    private HiddenObject hiddenObject;

    /// <summary>
    /// プレイヤーが隠れている状態で使用している該当の隠れる用オブジェクト
    /// </summary>
    private HiddenObject saveHiddenObject;


    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;

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

    /// <summary>
    /// ドキュメント取得時のSEのID
    /// </summary>
    private readonly int getDocumentSEid = 15;

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


    /*---------------------------
    * 隠れた後のクールタイム処理関連
    --------------------------*/

    /// <summary>
    /// 隠れた後のクールタイム（秒）
    /// </summary>
    private const float kHideWaitTime = 1.0f;

    /// <summary>
    /// 隠れた後のクールタイムカウント
    /// </summary>
    private float countHideWaitTime;

    /// <summary>
    /// 隠れた後のクールタイムカウントスタートフラグ
    /// </summary>
    private bool isStartHideWaitCountTime = false;



    private void Start()
    {


        isInteract = false;

        //AudioSourceの初期化
        InitializeAudioSource();

        //アイテムデータをリセット(デバッグ時以外でもリセットを行うのかは要検討)
        if (isDebugResetItem) sO_Item.ResetItems();


        //隠れた後のクールタイムカウントスタートフラグをオフにする
        isStartHideWaitCountTime = false;

        //隠れた後のクールタイムカウントを初期化
        countHideWaitTime = 0.0f;
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
        audioSourceItemSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
    }

    private void Update()
    {
        //隠れた後のクールタイムカウントスタートフラグがオン場合&&隠れた後のクールタイム時間カウントが指定時間以内の場合
        //インタラクト操作を連打して隠れた後に誤って隠れるモーションが解除されるのを防ぐための処理
        if (isStartHideWaitCountTime && countHideWaitTime < kHideWaitTime)
        {
            //隠れた後のクールタイムカウントをカウント
            countHideWaitTime += Time.deltaTime;
        }
        else
        {
            //隠れた後のクールタイムカウントスタートフラグをオフにする
            isStartHideWaitCountTime = false;

            //隠れた後のクールタイムカウントを初期化
            countHideWaitTime = 0.0f;
        }

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
        //プレイヤーが隠れている状態でインタラクト操作を行った場合
        if (PlayInteract() && Player.instance.GetIsPlayerHidden() && Time.timeScale != 0 && !isStartHideWaitCountTime)
        {
            //扉のシーケンスが実行中の場合
            if (saveHiddenObject == null || saveHiddenObject.GetIsDoorSequenceRunning())
            {
                //処理をスキップ
                return;
            }

            //隠れている状態を解除系の処理
            saveHiddenObject.ShowThePlayer();

            //保存していた隠れる用オブジェクトをリセット
            saveHiddenObject = null;

            //脱出したらこのフレームの処理は終了
            return; 
        }

        RaycastHit raycastHit;

        //Cameraから飛ばしているRayにオブジェクトが当たった場合
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, distance))
        {
            //Rayにオブジェクトが当たった状態&&インタラクト操作を行う&&プレイヤーが隠れていない場合
            if (PlayInteract() && !PauseController.instance.isPause && Time.timeScale != 0
                && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.GetIsPlayerHidden())
            {
                //隠れる用オブジェクト
                if (raycastHit.transform.tag == CommonController.instance.GetHiddenObjectTag()) 
                {
                    isInteract = true;

                    //対象の隠れる用オブジェクト
                    targetHiddenObject = raycastHit.transform.gameObject;

                    //HiddenObjectコンポーネントを取得
                    hiddenObject = targetHiddenObject.GetComponent<HiddenObject>();

                    //hiddenObjectが存在する場合&&隠れる用オブジェクトのドアシーケンスが実行中でない場合
                    if (hiddenObject != null && !hiddenObject.GetIsDoorSequenceRunning()) 
                    {
                        //隠れた後のクールタイムカウントスタートフラグをオンにする
                        isStartHideWaitCountTime = true;

                        //現在使用している隠れる用オブジェクトを保存
                        saveHiddenObject = hiddenObject;

                        //プレイヤーが隠れる処理を実行
                        hiddenObject.HiddenPlayer();
                    }
                }


                //アイテム
                if (raycastHit.transform.tag == CommonController.instance.GetItemTag())
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

                        //対象アイテムがプレイヤーライトの場合
                        if (item.GetItemType() == ItemType.PlayerLight) 
                        {
                            //拾ったアイテムをステージ上から削除
                            DestroyItem(pickUpItem);

                            //ライト入手フラグをtrueにする
                            Player.instance.SetIsHavePlayerLight(true);

                            //フラッシュライト関係のメッセージを表示
                            MessageController.instance.ShowInventoryMessage(4);
                        }
                        //対象アイテムがコンパスの場合
                        else if (item.GetItemType() == ItemType.Compass)
                        {
                            //コンパスの針のUIを表示
                            Compass.instance.ViewOrHiddenCompassArrowImage(true);

                            //拾ったアイテムをステージ上から削除
                            DestroyItem(pickUpItem);
                        }
                        //対象アイテムがドキュメントorミステリーアイテムの場合
                        else if ((item.GetItemType() == ItemType.Document) || (item.GetItemType() == ItemType.MysteryItem))
                        {
                            //ポーズ画面内のアイテムのパネル内に追加する
                            sO_Item.AddDocumentORMysteryItem(item);

                            //ドキュメントの場合
                            if (item.GetItemType() == ItemType.Document)
                            {
                                //ドキュメントの場合は取得SEを再生してから削除
                                DestroyDocument(pickUpItem);
                            }
                            else 
                            {
                                //拾ったアイテムをステージ上から削除
                                DestroyItem(pickUpItem);
                            }
                        }
                        //対象アイテムがプレイヤーが使用できるアイテムの場合
                        else if (item.GetItemType() == ItemType.UseItem)
                        {
                            //インベントリに空きがあるかを確認
                            if ((Inventory.instance.GetKeepItemId() == 99999) || (Inventory.instance.GetKeepItemId() == item.GetId()))
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
                if (raycastHit.transform.tag == CommonController.instance.GetDoorTag())
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
                if (raycastHit.transform.tag == CommonController.instance.GetStageLightTag())
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
                if (raycastHit.transform.tag == CommonController.instance.GetDrawerTag())
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
                if (raycastHit.transform.tag == CommonController.instance.GetGoalTag())
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
    /// <returns>ボタン押下&&振り返り操作オフ(振り返りながら裁きの青玉に触れるとプレイヤーが見えてしまうバグを防ぐため)でtrue</returns>
    bool PlayInteract() 
    {
        return (Input.GetMouseButtonDown(0) || Input.GetButtonDown(CommonController.instance.GetStringInteract()))
            && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.Slash);
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
            if (currentObjectTag == CommonController.instance.GetItemTag())
            {
                targetLayer = CommonController.instance.GetItemLayer();
            }
            else if (currentObjectTag == CommonController.instance.GetDoorTag())
            {
                targetLayer = CommonController.instance.GetDoorLayer();
            }
            else if (currentObjectTag == CommonController.instance.GetGoalTag())
            {
                targetLayer = CommonController.instance.GetGoalLayer();
            }
            else if (currentObjectTag == CommonController.instance.GetStageLightTag()) 
            {
                targetLayer = CommonController.instance.GetStageLightLayer();
            }
            else if (currentObjectTag == CommonController.instance.GetDrawerTag())
            {
                targetLayer = CommonController.instance.GetDrawerLayer();
            }
            else if (currentObjectTag == CommonController.instance.GetHiddenObjectTag())
            {
                targetLayer = CommonController.instance.GetHiddenObjectLayer();
            }
            else
            {
                Debug.LogWarning($"オブジェクト {currentHighlightedObject.name} のタグ {currentObjectTag} は認識されません。'Default' にフォールバックします。");
                targetLayer = CommonController.instance.GetDefaultLayer();
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
            //壁orドアorゴールorその他オブジェクトに当たった場合
            if (raycastHit.transform.tag == CommonController.instance.GetWallTag() ||
                raycastHit.transform.tag == CommonController.instance.GetDoorTag() ||
                raycastHit.transform.tag == CommonController.instance.GetGoalTag() ||
                raycastHit.transform.tag == CommonController.instance.GetOtherStageObjectTag())
            {
                Player.instance.SetIsRaycastHitWall(true);
            }
            else
            {
                Player.instance.SetIsRaycastHitWall(false);
            }

            //インタラクト可能なオブジェクトのいずれかに当たった場合
            if (raycastHit.transform.tag == CommonController.instance.GetItemTag() ||
                raycastHit.transform.tag == CommonController.instance.GetDoorTag() ||
                raycastHit.transform.tag == CommonController.instance.GetGoalTag() ||
                raycastHit.transform.tag == CommonController.instance.GetStageLightTag() ||
                raycastHit.transform.tag == CommonController.instance.GetDrawerTag() ||
                raycastHit.transform.tag == CommonController.instance.GetHiddenObjectTag())
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
                    SwitchLayer(currentHighlightedObject, CommonController.instance.GetOutlineTag());
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
    /// ドキュメントのゲームオブジェクトをシーンのフィールド上から削除する
    /// </summary>
    /// <param name="pickUpItem">入手アイテム</param>
    void DestroyDocument(GameObject pickUpItem)
    {
        if (audioSourceItemSE != null && sO_SE.GetSEClip(getDocumentSEid) != null)
        {
            //アイテム取得時の効果音を再生
            audioSourceItemSE.clip = sO_SE.GetSEClip(getDocumentSEid);
            audioSourceItemSE.loop = false;
            audioSourceItemSE.Play();
        }
        else
        {
            Debug.LogWarning($"AudioSource or getDocumentSEid is null in DestroyItem");
        }

        //アイテムを削除
        Destroy(pickUpItem);

        //アイテムを拾ったらレイヤーをリセット
        ResetLayer();
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
