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

    private Item item;

    //���ʂ�ScriptableObject���A�^�b�`����K�v������
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
            Camera.main.transform.forward, out raycastHit, distance) )
        {
            if (Input.GetKeyDown(KeyCode.E) && !PauseController.instance.isPause) 
            {
                //�A�C�e�����E��
                if (raycastHit.transform.tag == "Item")
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Item�R���|�[�l���g���擾
                    item = pickUpItem.GetComponent<Item>();

                    if (item != null )
                    {
                        if (sO_Item == null) Debug.LogError("SO_Item������������Ă��܂���I");

                        Debug.Log($"�E�����A�C�e���̃^�C�v: {item.itemType}");

                        MusicController.Instance.PlayAudioSE(audioSourceSE,getItemSE);

                        if ((item.itemType == ItemType.Document) 
                            || (item.itemType == ItemType.MysteryItem))
                        {
                            sO_Item.AddDocumentORMysteryItem(item);
                            Debug.Log("Player��SO_Item�̃C���X�^���XID: " + sO_Item.GetInstanceID());

                        }
                        else
                        {
                            sO_Item.AddItem(item);
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

                //�S�[��
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
