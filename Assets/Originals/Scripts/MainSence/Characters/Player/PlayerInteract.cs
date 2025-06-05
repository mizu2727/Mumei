using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInteract : MonoBehaviour
{
    [Header("インタラクトできる距離")]
    [SerializeField]  public float distance = 30f;
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

    [Header("SE関係")]
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip getItemSE;


    [Header("アイテムリセット(デバッグ用)")]
    public bool isDebugResetItem = false;

    private void Start()
    {
        isInteract = false;

        if (isDebugResetItem) sO_Item.ResetItems();
    }

    private void Update()
    {
        //Rayを可視化
        Debug.DrawRay(Camera.main.transform.position, 
            Camera.main.transform.forward, Color.green, 3);

        Interact();
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

                    //SwitchLayer(pickUpItem, outlineTag);

                    //アイテムを拾う
                    if (item != null)
                    {
                        //SwitchLayer(pickUpItem, itemTag);

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
        }
    }

    bool PlayInteract() 
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    //オブジェクトのレイヤーを変更する
    void SwitchLayer(GameObject obj, string layerName)
    {
        obj.layer = LayerMask.NameToLayer(layerName);
        Debug.Log("レイヤーを"+ layerName + "へ変更");
    }

    //
    //void resetCurrentDrawer()
    //{
    //    if (currentDrawer != null)
    //    {
    //        SwitchLayer(currentDrawer, "Default");
    //        currentDrawer = null;
    //    }
    //}
}
