using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]  public float distance = 30f;//�C���^���N�g�ł��鋗��
    GameObject pickUpItem;//�E�����A�C�e��
    GameObject interactDoor;//�C���^���N�g����h�A
    public bool isInteract;

    private Inventory inventory;
    private Item item;
    private Door door;

    private void Start()
    {
        isInteract = false;

        //Inventory�R���|�[�l���g���擾���ď�����
        inventory = GetComponent<Inventory>();  
    }

    private void Update()
    {
        //Ray������
        Debug.DrawRay(Camera.main.transform.position, 
            Camera.main.transform.forward, Color.green, 3);

        Interact();
    }

    void Interact() 
    {
        RaycastHit raycastHit;

        //Camera����Ray���΂�
        if (Physics.Raycast(Camera.main.transform.position, 
            Camera.main.transform.forward, out raycastHit, distance))
        {
            if (Input.GetKeyDown(KeyCode.E)) 
            {
                //�A�C�e�����E��
                if (raycastHit.transform.tag == "Item")
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Item�R���|�[�l���g���擾
                    item = pickUpItem.GetComponent<Item>();

                    if (item != null)
                    {
                        Debug.Log("Item�R���|�[�l���g���A�^�b�`����Ă���");

                        if (inventory == null) Debug.LogError("Inventory������������Ă��܂���I");


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
                        Debug.LogError("Item�R���|�[�l���g���A�^�b�`����Ă��܂���: " + pickUpItem.name);
                    }
                    
                }

                //�h�A�̊J��
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
