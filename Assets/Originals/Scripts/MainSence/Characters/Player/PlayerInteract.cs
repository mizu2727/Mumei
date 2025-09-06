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
    [Header("�C���^���N�g�ł��鋗��")]
    [SerializeField]  public float distance = 3f;
    GameObject pickUpItem;//�E�����A�C�e��
    GameObject interactDoor;//�C���^���N�g����h�A
    GameObject interactStageLight;//�C���^���N�g����X�e�[�W���C�g
    GameObject interactDrawer;//�C���^���N�g��������o��
    public bool isInteract;

    private Item item;

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;


    private Door door;
    private Goal goal;
    private StageLight stageLight;
    private Drawer drawer;


    private string itemTag = "Item";
    private string doorTag = "Door";
    private string goalTag = "Goal";
    private string stageLightTag = "StageLight";
    private string drawerTag = "Drawer";
    private string outlineTag = "Outline";
    private string itemLayer = "Item"; // �A�C�e���̌��̃��C���[
    private string doorLayer = "Door"; // �h�A�̌��̃��C���[
    private string goalLayer = "Goal"; // �S�[���̌��̃��C���[
    private string stageLightLayer = "StageLight"; // �S�[���̌��̃��C���[
    private string drawerLayer = "Drawer"; // �����o���̌��̃��C���[

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("SE�֌W")]
    //private AudioSource audioSourceSE;
    private AudioSource audioSourceItemSE; // �A�C�e���擾����p
    //[SerializeField] private AudioClip getItemSE;
    private readonly int getItemSEid = 2;//�A�C�e���擾����SE��ID

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

        // AudioSource�̏�����
        InitializeAudioSource();

        if (isDebugResetItem) sO_Item.ResetItems();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ肷�邽�߂̃C�x���g�o�^����
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ���AudioSource���Đݒ�
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {       
        // ���ׂĂ� AudioSource ���擾
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2)
        {
            // 2�ڂ� AudioSource ���s�����Ă���ꍇ�A�ǉ�
            audioSourceItemSE = gameObject.AddComponent<AudioSource>();
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }
        else
        {
            // 2�Ԗڂ� AudioSource ���A�C�e���擾���p�Ɋ��蓖��
            //(Player�I�u�W�F�N�g�ɂ��̃X�N���v�g��Player.cs���A�^�b�`���Ă���B
            //�ړ����ƃA�C�e���擾���̋������������p)
            audioSourceItemSE = audioSources[1];
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }


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

    /// <summary>
    /// �C���^���N�g����
    /// </summary>
    async void Interact() 
    {
        RaycastHit raycastHit;

        //Camera����Ray���΂�
        if (Physics.Raycast(Camera.main.transform.position, 
            Camera.main.transform.forward, out raycastHit, distance) )
        {
            if (PlayInteract() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
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

                        if ((item.itemType == ItemType.Document) 
                            || (item.itemType == ItemType.MysteryItem))
                        {
                            sO_Item.AddDocumentORMysteryItem(item);
                            Debug.Log("Player��SO_Item�̃C���X�^���XID: " + sO_Item.GetInstanceID());
                            DestroyItem(pickUpItem);
                        }
                        else if (item.itemType == ItemType.UseItem)
                        {
                            if ((Inventory.instance.keepItemId == 99999) || (Inventory.instance.keepItemId == item.id))
                            {
                                sO_Item.AddUseItem(item);
                                DestroyItem(pickUpItem);
                            }
                            else 
                            {
                                //�C���x���g���̃A�C�e���������ς��̏ꍇ�̏���
                                MessageController.instance.ShowInventoryMessage(1);

                                await UniTask.Delay(TimeSpan.FromSeconds(3));

                                MessageController.instance.ResetMessage();
                            }
                        }

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

                //�X�e�[�W���C�g
                if (raycastHit.transform.tag == stageLightTag)
                {
                    isInteract = true;
                    interactStageLight = raycastHit.transform.gameObject;
                    stageLight = interactStageLight.GetComponent<StageLight>();

                    //�X�e�[�W���C�g��_��
                    if (stageLight != null) stageLight.LitStageLight();
                }

                //�����o��
                if (raycastHit.transform.tag == drawerTag)
                {
                    isInteract = true;
                    interactDrawer = raycastHit.transform.gameObject;
                    drawer = interactDrawer.GetComponent<Drawer>();

                    //�h�A�̊J��
                    if (drawer != null) drawer.DrawerSystem();
                }

                //�S�[��
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //�S�[���`�F�b�N
                    if (!goal.isGoalPanel && goal != null) 
                    {
                        goal.GoalCheck();

                        //�S�[���p�l�����\���ɂ���ۂɁA
                        //goal.isGoalPanel��true�̂܂܂ɂȂ��Ă��܂��o�O��h��
                        goal.isGoalPanel = false;
                    } 
                }
            }   
        }
        else
        {
            isInteract = false;
            goal = null;

            // Ray�����ɂ��������Ă��Ȃ��ꍇ�����Z�b�g
            ResetLayer(); 
        }
    }

    /// <summary>
    /// ���N���b�N�ER�{�^���ŃC���^���N�g����
    /// Interact�c"joystick button 5"�����蓖�ĂĂ���B�R���g���[���[�ł�R�{�^���ɂȂ�
    /// </summary>
    /// <returns>�{�^��������true</returns>
    bool PlayInteract() 
    {
        return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Interact");
    }


    /// <summary>
    /// �I�u�W�F�N�g�̃��C���[��ύX����
    /// </summary>
    /// <param name="obj">�ΏۃI�u�W�F�N�g</param>
    /// <param name="layerName">���C���[��</param>
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
 
    /// <summary>
    /// ���݂̃I�u�W�F�N�g�̃��C���[�����ɖ߂�
    /// </summary>
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
                Debug.LogWarning($"�I�u�W�F�N�g {currentHighlightedObject.name} �̃^�O {currentObjectTag} �͔F������܂���B'Default' �Ƀt�H�[���o�b�N���܂��B");
                targetLayer = "Default";
            }

            SwitchLayer(currentHighlightedObject, targetLayer);
            Debug.Log($"�I�u�W�F�N�g {currentHighlightedObject.name} �̃��C���[�� {targetLayer} �ɖ߂��܂���");
            currentHighlightedObject = null;
            currentObjectTag = null;
        }
    }
 
    /// <summary>
    /// �C���^���N�g�\�ȃI�u�W�F�N�g�������\�����鏈��
    /// </summary>
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
                raycastHit.transform.tag == goalTag ||
                raycastHit.transform.tag == stageLightTag ||
                raycastHit.transform.tag == drawerTag)
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

    /// <summary>
    /// ����A�C�e���̃Q�[���I�u�W�F�N�g���V�[���̃t�B�[���h�ォ��폜����
    /// </summary>
    /// <param name="pickUpItem">����A�C�e��</param>
    void DestroyItem(GameObject pickUpItem) 
    {
        if (audioSourceItemSE != null && sO_SE.GetSEClip(getItemSEid) != null)
        {
            // �A�C�e���擾���̌��ʉ����Đ�
            audioSourceItemSE.clip = sO_SE.GetSEClip(getItemSEid);
            audioSourceItemSE.loop = false;
            audioSourceItemSE.Play();
        }
        else
        {
            Debug.LogWarning($"AudioSource or getItemSE is null in DestroyItem");
        }

        Destroy(pickUpItem);

        // �A�C�e�����E�����烌�C���[�����Z�b�g
        ResetLayer();
    }
}
