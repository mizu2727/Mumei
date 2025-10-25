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

    /// <summary>
    /// �E�����A�C�e��
    /// </summary>
    GameObject pickUpItem;

    /// <summary>
    /// �J�������h�A
    /// </summary>
    GameObject interactDoor;

    /// <summary>
    /// �_�������X�e�[�W���C�g
    /// </summary>
    GameObject interactStageLight;

    /// <summary>
    /// �J�����������o��
    /// </summary>
    GameObject interactDrawer;

    /// <summary>
    /// �C���^���N�g�t���O
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

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;

    /// <summary>
    /// �A�C�e���^�O
    /// </summary>
    private string itemTag = "Item";

    /// <summary>
    /// �h�A�^�O
    /// </summary>
    private string doorTag = "Door";

    /// <summary>
    /// �S�[���^�O
    /// </summary>
    private string goalTag = "Goal";

    /// <summary>
    /// �X�e�[�W���C�g�^�O
    /// </summary>
    private string stageLightTag = "StageLight";

    /// <summary>
    /// �����o���^�O
    /// </summary>
    private string drawerTag = "Drawer";

    /// <summary>
    /// �A�E�g���C���^�O
    /// </summary>
    private string outlineTag = "Outline";

    /// <summary>
    /// �A�C�e�����C���[
    /// </summary>
    private string itemLayer = "Item";

    /// <summary>
    /// �h�A���C���[
    /// </summary>
    private string doorLayer = "Door";

    /// <summary>
    /// �S�[�����C���[
    /// </summary>
    private string goalLayer = "Goal";

    /// <summary>
    /// �S�[�����C���[
    /// </summary>
    private string stageLightLayer = "StageLight";

    /// <summary>
    /// �����o�����C���[
    /// </summary>
    private string drawerLayer = "Drawer";

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// �A�C�e���擾����paudioSource
    /// </summary>
    private AudioSource audioSourceItemSE;

    /// <summary>
    /// �A�C�e���擾����SE��ID
    /// </summary>
    private readonly int getItemSEid = 2;

    [Header("�A�C�e�����Z�b�g(�f�o�b�O�p)")]
    public bool isDebugResetItem = false;

    /// <summary>
    /// ���ݏƏ����������Ă���I�u�W�F�N�g��ǐ�
    /// </summary>
    private GameObject currentHighlightedObject;

    /// <summary>
    /// �I�u�W�F�N�g�̌��̃��C���[��ۑ�
    /// </summary>
    private int originalLayer;

    /// <summary>
    /// ���ݏƏ����������Ă���I�u�W�F�N�g�̃^�O��ۑ�
    /// </summary>
    private string currentObjectTag;

    private void Start()
    {
        isInteract = false;

        //AudioSource�̏�����
        InitializeAudioSource();

        //�A�C�e���f�[�^�����Z�b�g(�f�o�b�O���ȊO�ł����Z�b�g���s���̂��͗v����)
        if (isDebugResetItem) sO_Item.ResetItems();
    }


    private void OnEnable()
    {
        //sceneLoaded�ɁuOnSceneLoaded�v�֐���ǉ�
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //�V�[���J�ڎ��ɐݒ肷�邽�߂̊֐��o�^����
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE���ʕύX���̃C�x���g�o�^����
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
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
    /// SE���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceItemSE != null)
        {
            audioSourceItemSE.volume = volume;
        }
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {       
        //���ׂĂ�AudioSource���擾
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 2)
        {
            //2�ڂ�AudioSource���s�����Ă���ꍇ�A�ǉ�����
            audioSourceItemSE = gameObject.AddComponent<AudioSource>();
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }
        else
        {
            //2�Ԗڂ�AudioSource���A�C�e���擾���p�Ɋ��蓖��
            //(Player�I�u�W�F�N�g�ɂ��̃X�N���v�g��Player.cs���A�^�b�`���Ă���B
            //�ړ����ƃA�C�e���擾���̋������������p)
            audioSourceItemSE = audioSources[1];
            audioSourceItemSE.playOnAwake = false;
            audioSourceItemSE.volume = 1.0f;
        }

        //MusicController�Őݒ肳��Ă���SE�p��AudioMixerGroup��ݒ肷��
        audioSourceItemSE.outputAudioMixerGroup = MusicController.Instance.audioMixerGroupSE;
    }

    private void Update()
    {
        //Ray������
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.green, 3);

        //�C���^���N�g
        Interact();

        //����������ǉ�
        HighlightObject(); 
    }

    /// <summary>
    /// �C���^���N�g����
    /// </summary>
    async void Interact() 
    {
        RaycastHit raycastHit;

        //Camera�����΂��Ă���Ray�ɃI�u�W�F�N�g�����������ꍇ
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit, distance) )
        {
            //Ray�ɃI�u�W�F�N�g������������ԂŃC���^���N�g������s���ꍇ
            if (PlayInteract() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
            {
                //�A�C�e��
                if (raycastHit.transform.tag == itemTag)
                {
                    isInteract = true;

                    //�Ώۂ̃A�C�e��
                    pickUpItem = raycastHit.transform.gameObject;

                    //Item�R���|�[�l���g���擾
                    item = pickUpItem.GetComponent<Item>();

                    //�Ώۂ̃A�C�e�����E���ꍇ
                    if (item != null)
                    {

                        if (sO_Item == null) Debug.LogError("SO_Item������������Ă��܂���I");

                        //�ΏۃA�C�e�����h�L�������gor�~�X�e���[�A�C�e���̏ꍇ
                        if ((item.itemType == ItemType.Document) || (item.itemType == ItemType.MysteryItem))
                        {
                            //�|�[�Y��ʓ��̃A�C�e���̃p�l�����ɒǉ�����
                            sO_Item.AddDocumentORMysteryItem(item);
                            
                            //�E�����A�C�e�����X�e�[�W�ォ��폜
                            DestroyItem(pickUpItem);
                        }
                        //�ΏۃA�C�e�����v���C���[���g�p�ł���A�C�e���̏ꍇ
                        else if (item.itemType == ItemType.UseItem)
                        {
                            //�C���x���g���ɋ󂫂����邩���m�F
                            if ((Inventory.instance.keepItemId == 99999) || (Inventory.instance.keepItemId == item.id))
                            {
                                //�C���x���g���ɒǉ�
                                sO_Item.AddUseItem(item);

                                //�E�����A�C�e�����X�e�[�W�ォ��폜
                                DestroyItem(pickUpItem);
                            }
                            //�C���x���g���̃A�C�e���������ς��̏ꍇ�̏���
                            else
                            {
                                //�C���x���g���ɋ󂫂��Ȃ��|�̃��b�Z�[�W��\��
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

                    //�Ώۂ̃h�A
                    interactDoor = raycastHit.transform.gameObject;

                    //�h�A�̃R���|�[�l���g�擾
                    door = interactDoor.GetComponent<Door>();

                    //�Ώۂ̃h�A���J��
                    if (door != null) door.DoorSystem();                    
                }

                //�X�e�[�W���C�g
                if (raycastHit.transform.tag == stageLightTag)
                {
                    isInteract = true;

                    //�Ώۂ̃X�e�[�W���C�g
                    interactStageLight = raycastHit.transform.gameObject;

                    //�X�e�[�W���C�g�̃R���|�[�l���g�擾
                    stageLight = interactStageLight.GetComponent<StageLight>();

                    //�Ώۂ̃X�e�[�W���C�g��_��
                    if (stageLight != null) stageLight.LitStageLight();
                }

                //�����o��
                if (raycastHit.transform.tag == drawerTag)
                {
                    isInteract = true;

                    //�Ώۂ̈����o��
                    interactDrawer = raycastHit.transform.gameObject;

                    //�����o���̃R���|�[�l���g���擾
                    drawer = interactDrawer.GetComponent<Drawer>();

                    //�Ώۂ̈����o�����J��
                    if (drawer != null) drawer.DrawerSystem();
                }

                //�S�[��
                if (raycastHit.transform.tag == goalTag)
                {
                    isInteract = true;

                    //�Ώۂ̃S�[��
                    goal = raycastHit.transform.gameObject.GetComponent<Goal>();

                    //�S�[���`�F�b�N
                    if (!goal.isGoalPanel && goal != null) 
                    {
                        //�S�[���̏���
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

            //Ray�����ɂ��������Ă��Ȃ��ꍇ�����Z�b�g
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
    }
 
    /// <summary>
    /// ���݂̃I�u�W�F�N�g�̃��C���[�����ɖ߂�
    /// </summary>
    void ResetLayer()
    {
        if (currentHighlightedObject != null)
        {
            string targetLayer = "";
            //�^�O�ɉ����Č��̃��C���[������
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

        //Camera�����΂��Ă���Ray�ɃI�u�W�F�N�g�����������ꍇ
        if (Physics.Raycast(Camera.main.transform.position,
            Camera.main.transform.forward, out raycastHit, distance))
        {
            //�C���^���N�g�\�ȃI�u�W�F�N�g�̂����ꂩ�ɓ��������ꍇ
            if (raycastHit.transform.tag == itemTag ||
                raycastHit.transform.tag == doorTag ||
                raycastHit.transform.tag == goalTag ||
                raycastHit.transform.tag == stageLightTag ||
                raycastHit.transform.tag == drawerTag)
            {
                //Ray���q�b�g�����I�u�W�F�N�g
                GameObject hitObject = raycastHit.transform.gameObject;

                //���݂̋����Ώۂ��قȂ�ꍇ�A�O�̋���������
                if (currentHighlightedObject != hitObject)
                {
                    //�O�̃I�u�W�F�N�g�̃��C���[�����ɖ߂�
                    ResetLayer(); 
                    currentHighlightedObject = hitObject;


                    //���݂̃��C���[��ۑ�
                    originalLayer = currentHighlightedObject.layer;

                    //���݂̃I�u�W�F�N�g�̃^�O��ۑ�
                    currentObjectTag = currentHighlightedObject.tag;

                    //���C���[��Outline�ɕύX
                    SwitchLayer(currentHighlightedObject, outlineTag);
                }
            }
            else
            {
                //�C���^���N�g�\�ȃI�u�W�F�N�g�ȊO�ɓ��������ꍇ�A����������
                ResetLayer();
            }
        }
        else
        {
            //Ray�����ɂ��������Ă��Ȃ��ꍇ�A����������
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
            //�A�C�e���擾���̌��ʉ����Đ�
            audioSourceItemSE.clip = sO_SE.GetSEClip(getItemSEid);
            audioSourceItemSE.loop = false;
            audioSourceItemSE.Play();
        }
        else
        {
            Debug.LogWarning($"AudioSource or getItemSE is null in DestroyItem");
        }

        //�A�C�e�����폜
        Destroy(pickUpItem);

        //�A�C�e�����E�����烌�C���[�����Z�b�g
        ResetLayer();
    }
}
