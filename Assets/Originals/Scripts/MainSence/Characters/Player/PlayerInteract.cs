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

    private Inventory inventory;
    private Item item;
    private Door door;

    private void Start()
    {
        isInteract = false;

        //Inventoryコンポーネントを取得して初期化
        inventory = GetComponent<Inventory>();  
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
            Camera.main.transform.forward, out raycastHit, distance))
        {
            if (Input.GetKeyDown(KeyCode.E)) 
            {
                //アイテムを拾う
                if (raycastHit.transform.tag == "Item")
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Itemコンポーネントを取得
                    item = pickUpItem.GetComponent<Item>();

                    if (item != null)
                    {
                        Debug.Log("Itemコンポーネントがアタッチされている");

                        if (inventory == null) Debug.LogError("Inventoryが初期化されていません！");


                        if (item.itemType == ItemType.Document)
                        {
                            inventory.GetDocument(item.id, item.count);
                        }
                        else 
                        {
                            inventory.GetItem(item.id, item.count);
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
            }   
        }
        else
        {
            isInteract = false;
        }
    }
}
