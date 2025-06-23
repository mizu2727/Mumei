using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    [Header("�C���^���N�g�ł��鋗��")]
    [SerializeField]  public float distance = 3f;
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
    private string itemLayer = "Item"; // �A�C�e���̌��̃��C���[
    private string doorLayer = "Door"; // �h�A�̌��̃��C���[
    private string goalLayer = "Goal"; // �S�[���̌��̃��C���[

    [Header("SE�֌W")]
    private AudioSource audioSourceSE;
    [SerializeField] private AudioClip getItemSE;


    [Header("�A�C�e�����Z�b�g(�f�o�b�O�p)")]
    public bool isDebugResetItem = false;


    // ���ݏƏ����������Ă���I�u�W�F�N�g��ǐ�
    private GameObject currentHighlightedObject;
    // �I�u�W�F�N�g�̌��̃��C���[��ۑ�
    private int originalLayer;
    // ���ݏƏ����������Ă���I�u�W�F�N�g�̃^�O��ۑ�
    private string currentObjectTag;

    private void Start()
    {
        isInteract = false;
        audioSourceSE = MusicController.Instance.GetAudioSource();

        if (isDebugResetItem) sO_Item.ResetItems();
    }

    private void Update()
    {
        //Ray������
        Debug.DrawRay(Camera.main.transform.position, 
            Camera.main.transform.forward, Color.green, 3);

        Interact();

        // ����������ǉ�
        HighlightObject(); 
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

                    //�A�C�e�����E��
                    if (item != null)
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

                        // �A�C�e�����E�����烌�C���[�����Z�b�g
                        ResetLayer(); 
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

            // Ray�����ɂ��������Ă��Ȃ��ꍇ�����Z�b�g
            ResetLayer(); 
        }
    }

    //���N���b�N�ŃC���^���N�g����
    bool PlayInteract() 
    {
        return Input.GetMouseButtonDown(0);
    }

    // �I�u�W�F�N�g�̃��C���[��ύX����
    void SwitchLayer(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogError($"���C���[ '{layerName}' ��������܂���B�v���W�F�N�g�ݒ�Ŋm�F���Ă��������B");
            return;
        }
        obj.layer = layer;
        Debug.Log($"�I�u�W�F�N�g {obj.name} �̃��C���[�� {layerName} �֕ύX");
    }

    // ���݂̃I�u�W�F�N�g�̃��C���[�����ɖ߂�
    void ResetLayer()
    {
        if (currentHighlightedObject != null)
        {
            string targetLayer = "";
            // �^�O�ɉ����Č��̃��C���[������
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
                Debug.LogWarning($"�I�u�W�F�N�g {currentHighlightedObject.name} �̃^�O {currentObjectTag} �͔F������܂���B'Default' �Ƀt�H�[���o�b�N���܂��B");
                targetLayer = "Default";
            }

            SwitchLayer(currentHighlightedObject, targetLayer);
            Debug.Log($"�I�u�W�F�N�g {currentHighlightedObject.name} �̃��C���[�� {targetLayer} �ɖ߂��܂���");
            currentHighlightedObject = null;
            currentObjectTag = null;
        }
    }


    // �C���^���N�g�\�ȃI�u�W�F�N�g�������\�����鏈��
    void HighlightObject()
    {
        RaycastHit raycastHit;

        // Camera ���� Ray ���΂�
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out raycastHit, distance))
        {
            // �A�C�e���A�h�A�A�S�[���̂����ꂩ�ɓ��������ꍇ
            if (raycastHit.transform.tag == itemTag ||
                raycastHit.transform.tag == doorTag ||
                raycastHit.transform.tag == goalTag)
            {
                GameObject hitObject = raycastHit.transform.gameObject;

                // ���݂̋����Ώۂ��قȂ�ꍇ�A�O�̋���������
                if (currentHighlightedObject != hitObject)
                {
                    ResetLayer(); // �O�̃I�u�W�F�N�g�̃��C���[�����ɖ߂�
                    currentHighlightedObject = hitObject;
                    // ���݂̃��C���[��ۑ�
                    originalLayer = currentHighlightedObject.layer;
                    // ���݂̃I�u�W�F�N�g�̃^�O��ۑ�
                    currentObjectTag = currentHighlightedObject.tag;
                    // ���C���[�� Outline �ɕύX
                    SwitchLayer(currentHighlightedObject, outlineTag);
                }
            }
            else
            {
                // �C���^���N�g�\�ȃI�u�W�F�N�g�ȊO�ɓ��������ꍇ�A����������
                ResetLayer();
            }
        }
        else
        {
            // Ray �����ɂ��������Ă��Ȃ��ꍇ�A����������
            ResetLayer();
        }
    }
}
