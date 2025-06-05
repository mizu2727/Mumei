using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInteract : MonoBehaviour
{
    [Header("�C���^���N�g�ł��鋗��")]
    [SerializeField]  public float distance = 30f;
    GameObject pickUpItem;//�E�����A�C�e��
    GameObject interactDoor;//�C���^���N�g����h�A
    public bool isInteract;

    private Item item;

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;


    private Door door;
    private Goal goal;


    private string itemTag = "Item";
    private string doorTag = "Door";
    private string goalTag = "Goal";
    private string outlineTag = "Outline";

    [Header("SE�֌W")]
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip getItemSE;


    [Header("�A�C�e�����Z�b�g(�f�o�b�O�p)")]
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
            if (PlayInteract() && !PauseController.instance.isPause) 
            {
                //�A�C�e��
                if (raycastHit.transform.tag == itemTag)
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;

                    //Item�R���|�[�l���g���擾
                    item = pickUpItem.GetComponent<Item>();

                    //SwitchLayer(pickUpItem, outlineTag);

                    //�A�C�e�����E��
                    if (item != null)
                    {
                        //SwitchLayer(pickUpItem, itemTag);

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

                //�h�A
                if (raycastHit.transform.tag == doorTag) 
                {
                    isInteract = true;
                    interactDoor = raycastHit.transform.gameObject;
                    door = interactDoor.GetComponent<Door>();

                    //�h�A�̊J��
                    if (door != null) door.DoorSystem();                    
                }

                //�S�[��
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //�S�[���`�F�b�N
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

    //�I�u�W�F�N�g�̃��C���[��ύX����
    void SwitchLayer(GameObject obj, string layerName)
    {
        obj.layer = LayerMask.NameToLayer(layerName);
        Debug.Log("���C���[��"+ layerName + "�֕ύX");
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
