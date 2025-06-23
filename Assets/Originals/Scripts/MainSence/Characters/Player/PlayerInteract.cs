using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("インタラクトできる距離")]
    [SerializeField]  public float distance = 3f;
    GameObject pickUpItem;//拾ったアイテム
    GameObject interactDoor;//インタラクトするドア
    public bool isInteract;

    private Item item;

    [Header("アイテムデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_Item sO_Item;


    private Door door;
    private Goal goal;


    private string itemTag = "Item";
    private string doorTag = "Door";
    private string goalTag = "Goal";
    private string outlineTag = "Outline";
    private string itemLayer = "Item"; // アイテムの元のレイヤー
    private string doorLayer = "Door"; // ドアの元のレイヤー
    private string goalLayer = "Goal"; // ゴールの元のレイヤー

    [Header("SE関係")]
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip getItemSE;


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
        audioSourceSE = MusicController.Instance.GetAudioSource();

        if (isDebugResetItem) sO_Item.ResetItems();
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

    void Interact() 
    {
        RaycastHit raycastHit;

        //CameraからRayを飛ばす
        if (Physics.Raycast(Camera.main.transform.position, 
            Camera.main.transform.forward, out raycastHit, distance) )
        {
            if (PlayInteract() && !PauseController.instance.isPause) 
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

                        MusicController.Instance.PlayAudioSE(audioSourceSE,getItemSE);

                        if ((item.itemType == ItemType.Document) 
                            || (item.itemType == ItemType.MysteryItem))
                        {
                            sO_Item.AddDocumentORMysteryItem(item);
                            Debug.Log("Player側SO_ItemのインスタンスID: " + sO_Item.GetInstanceID());

                        }
                        else
                        {
                            sO_Item.AddItem(item);
                        }

                        Destroy(pickUpItem);

                        // アイテムを拾ったらレイヤーをリセット
                        ResetLayer(); 
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

                //ゴール
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //ゴールチェック
                    if (!goal.isGoalPanel && goal != null) goal.GoalCheck();

                }
            }   
        }
        else
        {
            isInteract = false;

            // Rayが何にも当たっていない場合もリセット
            ResetLayer(); 
        }
    }

    //左クリックでインタラクト操作
    bool PlayInteract() 
    {
        return Input.GetMouseButtonDown(0);
    }

    // オブジェクトのレイヤーを変更する
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

    // 現在のオブジェクトのレイヤーを元に戻す
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


    // インタラクト可能なオブジェクトを強調表示する処理
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
                raycastHit.transform.tag == goalTag)
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
}
