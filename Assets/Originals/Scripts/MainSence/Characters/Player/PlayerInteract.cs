using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]  public float distance = 30f;//インタラクトできる距離
    GameObject pickUpItem;//拾ったアイテム
    GameObject interactDoor;//インタラクトするドア
    public bool isInteract;

    private Item item;

    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;


    private Door door;
    private Goal goal;


    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip getItemSE;


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
            if (Input.GetKeyDown(KeyCode.E) && !PauseController.instance.isPause) 
            {
                //アイテムを拾う
                if (raycastHit.transform.tag == "Item")
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Itemコンポーネントを取得
                    item = pickUpItem.GetComponent<Item>();

                    if (item != null )
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
                    }
                    else 
                    {
                        Debug.LogError("Itemコンポーネントがアタッチされていません: " + pickUpItem.name);
                    }
                    
                }

                //ドアの開閉
                if (raycastHit.transform.tag == "Door") 
                {
                    isInteract = true;
                    interactDoor = raycastHit.transform.gameObject;
                    door = interactDoor.GetComponent<Door>();
                    door.DoorSystem();
                }

                //ゴール
                if (raycastHit.transform.tag == "Goal")
                {
                    isInteract = true;
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    if (!goal.isGoalPanel) goal.GoalCheck();

                }
            }   
        }
        else
        {
            isInteract = false;
        }
    }
}
