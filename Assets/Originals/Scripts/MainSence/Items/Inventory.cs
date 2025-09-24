using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;
using static SO_Item;


public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("�g�p�A�C�e���p�l���֘A(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject useItemPanel;//�g�p�A�C�e���m�F�p�l��
    [SerializeField] private Text useItemCountText;//�g�p�A�C�e�������J�E���g�e�L�X�g
    [SerializeField] private Image useItemImage;//�g�p�A�C�e���摜
    [SerializeField] public GameObject useItemTextPanel;//�g�p�A�C�e���e�L�X�g�m�F�p�l��
    [SerializeField] public Text useItemNameText;//�g�p�A�C�e�����e�L�X�g
    [SerializeField] public Text useItemExplanationText;//�g�p�A�C�e�������e�L�X�g


    //�A�C�e��ID�Ǘ�(�A�C�e���C���x���g�������g���p)
    private readonly List<int> idList = new();

    [Header("�A�C�e��ID�Ǘ�(�A�C�e���C���x���g��1�g���p�B�ҏW�֎~)")]
    public int keepItemId;

    //�A�C�e������������ID(�A�C�e���C���x���g��1�g���p)
    private const int noneItemId = 99999;

    //�A�C�e���̃v���n�u��Addressables��
    private string keepItemPrefabPath;

    //�A�C�e���̃v���n�u��Addressables��(��)
    private const string noneItemPrefabPath = "";

    //�A�C�e�������ʒu
    private Vector3 keepItemSpawnPosition;

    //�A�C�e�������ʒu(�f�t�H���g)
    private Vector3 defaultItemSpawnPosition = new Vector3(0, 0, 0);

    //�A�C�e���̉�]���l
    private Quaternion keepItemSpawnRotation;

    //�A�C�e���̉�]���l(�f�t�H���g)
    private Quaternion defaultItemSpawnRotation = Quaternion.identity;

    //�A�C�e���������Ǘ�(�A�C�e���C���x���g�������g���p)
    private readonly List<int> countList = new();

    //�A�C�e���������Ǘ�(�A�C�e���C���x���g��1�g���p)
    private int keepItemCount;

    private const int minKeepItemCount = 0;

    //�A�C�e�����ʒl
    private int keepItemEffectValue;

    //�A�C�e�����ʒl(�f�t�H���g�l)
    private const int defaultKeepItemEffectValue = 0;

    //�A�C�e�����X�g�̃C���f�b�N�X�ԍ�
    int checkIndex;

    // �X�^�~�i�������ʂ̃^�X�N���Ǘ����邽�߂�CancellationTokenSource
    //private CancellationTokenSource staminaEffectCts;

    //�X�^�~�i�����܂��g�p���Ă��邩�𔻒�
    private bool isUseStaminaItem;

    private Player player;

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("�T�E���h�֘A")]
    private AudioSource audioSourceInventorySE;//Inventory��p��AudioSource
    private readonly int useStaminaEnhancerSEid = 12; // �X�^�~�i������SE��ID


    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڎ��Ɏg�p�A�C�e���p�l���֘A���Đݒ肷�邽�߂̃C�x���g�o�^����
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //CancelStaminaEffect();
    }

    /// <summary>
    /// �V�[���J�ڎ��Ɏg�p�A�C�e���p�l���֘A���Đݒ�y�у��Z�b�g
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeAudioSource();

        if (GameController.instance.useItemPanel != null) useItemPanel = GameController.instance.useItemPanel;
        else Debug.LogError("GameController��useItemPanel���ݒ肳��Ă��܂���");

        if (GameController.instance.useItemCountText != null) useItemCountText = GameController.instance.useItemCountText;
        else Debug.LogError("GameController��useItemCountText���ݒ肳��Ă��܂���");

        if (GameController.instance.useItemImage != null) useItemImage = GameController.instance.useItemImage;
        else Debug.LogError("GameController��useItemImage���ݒ肳��Ă��܂���");

        if (GameController.instance.useItemTextPanel != null) useItemTextPanel = GameController.instance.useItemTextPanel;
        else Debug.LogError("GameController��useItemTextPanel���ݒ肳��Ă��܂���");

        if (GameController.instance.useItemNameText != null) useItemNameText = GameController.instance.useItemNameText;
        else Debug.LogError("GameController��useItemNameText���ݒ肳��Ă��܂���");

        if (GameController.instance.useItemExplanationText != null) useItemExplanationText = GameController.instance.useItemExplanationText;
        else Debug.LogError("GameController��useItemExplanationText���ݒ肳��Ă��܂���");

        ResetInventoryItem();
        //�A�C�e��DB�����Z�b�g
        sO_Item.ResetItems();
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {
        // ���ׂĂ� AudioSource ���擾
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 3)
        {
            // 3�ڂ� AudioSource ���s�����Ă���ꍇ�A�ǉ�
            audioSourceInventorySE = gameObject.AddComponent<AudioSource>();
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
        else
        {
            // 3�Ԗڂ� AudioSource ���A�C�e���擾���p�Ɋ��蓖��
            //(Player�I�u�W�F�N�g�ɂ��̃X�N���v�g��Player.cs���A�^�b�`���Ă���B
            //�ړ����ƃA�C�e���擾���̋������������p)
            audioSourceInventorySE = audioSources[2];
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
    }

    private void Awake()
    {
        // �V���O���g���̐ݒ�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �V�[���J�ڎ��ɔj������Ȃ��悤�ɂ���i�K�v�ɉ����āj
        }
        else
        {
            Destroy(gameObject); // ���łɃC���X�^���X�����݂���ꍇ�͔j��
        }
    }

    void Start()
    {
        player = GetComponent<Player>();
        //Addressables��������
        Addressables.InitializeAsync();
        ResetInventoryItem();
    }

    void Update()
    {
        if (UseInventoryItem() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) UseItem();
    }

    /// <summary>
    /// �E�N���b�N�ŃC���x���g���A�C�e���g�p����֐�
    /// </summary>
    /// <returns>true / false</returns>
    bool UseInventoryItem()
    {
        return Input.GetMouseButtonDown(1);
    }

    /// <summary>
    /// �C���x���g���ɃA�C�e����ǉ�
    /// </summary>
    /// <param name="id">�A�C�e��id</param>
    /// <param name="path">�A�C�e���̃p�X</param>
    /// <param name="position">�A�C�e���̐����ʒu</param>
    /// <param name="rotation">�A�C�e���̉�]</param>
    /// <param name="icon">�A�C�e���̉摜</param>
    /// <param name="itemName">�A�C�e����</param>
    /// <param name="description">�A�C�e���̐���</param>
    /// <param name="count">�A�C�e����</param>
    /// <param name="effectValue">�A�C�e�����ʒl</param>
    public void GetItem(int id, string path, Vector3 position, Quaternion rotation, Sprite icon, string itemName, string description,int count, int effectValue)
    {
        //���X�g�̒��ɃA�C�e�������Ԗڂɑ��݂���̂����m�F
        //���݂��Ȃ��ꍇ��-1��Ԃ�
        //checkIndex = idList.IndexOf(id);

        //�C���x���g���ɐV�K�ǉ����鏈��
        //checkIndex == -1
        if (keepItemId == noneItemId)
        {

            //�@�A�C�e��id��ݒ�
            idList.Add(id);

            keepItemId = id;
            keepItemPrefabPath = path;
            keepItemSpawnPosition = position;
            keepItemSpawnRotation = rotation;
            useItemImage.sprite = icon;
            keepItemEffectValue = effectValue;
            useItemImage.color = new Color(255, 255, 255, 1);

            //�@�A�C�e����������ݒ�
            countList.Add(count);
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            //�@�A�C�e�����Ɛ�������ݒ�
            useItemNameText.text = itemName;
            useItemExplanationText.text = description;

            Debug.Log("id:" + id + "�̃A�C�e��item��" + count + "�V�K�ǉ�");
            Debug.Log("keepItemCount(�g�p�A�C�e��):" + keepItemCount);
            
        }
        else
        {
            //�A�C�e����������ǉ�
            countList[checkIndex] += count;
            keepItemCount = count;
            useItemCountText.text = count.ToString();


            Debug.Log("id:" + id + "�̃A�C�e��item��" + count + "�֑���");

            Debug.Log("keepItemCount(�g�p�A�C�e��):" + keepItemCount);
        }
    }

    /// <summary>
    /// �A�C�e�����g�p����
    /// </summary>
    public void UseItem() 
    {
        if (minKeepItemCount < keepItemCount)
        {
            ActivationUseItem(keepItemId);
            Debug.Log("id:" + keepItemId + "��keepItemCount(�g�p�A�C�e���̌�):" + keepItemCount);

            if (keepItemCount == minKeepItemCount)
            {
                //�A�C�e������0�ɂȂ����ꍇ�̏���
                ResetInventoryItem();
            }
        }
        else 
        {
            Debug.Log("�g�p�ł���A�C�e��item���Ȃ��悤��");
        }
    }

    /// <summary>
    /// �g�p�����A�C�e����ID�ɂ���āA���ꂼ��̏������s��
    /// </summary>
    /// <param name="keepItemId">�A�C�e��ID</param>
    async void ActivationUseItem(int keepItemId) 
    {
        //�g�p����A�C�e��ID�ɂ���ď����𕪊�
        switch (keepItemId) 
        {
            //�X�^�~�i������
            case 11:
                // �X�^�~�i���ʂ��K�p���̏ꍇ
                if (isUseStaminaItem)
                {
                    MessageController.instance.ShowInventoryMessage(3);

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    MessageController.instance.ResetMessage();
                    return;
                }

                // �A�C�e���J�E���g������
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                // �X�^�~�i�Q�[�W�̌��̐F��ێ�
                Color keepStaminaColor = Player.instance.staminaSlider.fillRect.GetComponent<Image>().color;

                //�X�^�~�i������SE���Đ�
                audioSourceInventorySE.clip = sO_SE.GetSEClip(useStaminaEnhancerSEid);
                audioSourceInventorySE.loop = false;
                audioSourceInventorySE.Play();

                //�X�^�~�i�����25%�ɕύX���A�X�^�~�i�Q�[�W�̐F��ΐF�ɕύX
                Player.instance.staminaConsumeRatio = 25;
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = new Color(0, 1, 0, 1);
                isUseStaminaItem = true;

                //���ʎ��ԑҋ@
                await UniTask.Delay(TimeSpan.FromSeconds(keepItemEffectValue));

                //�X�^�~�i�����50%�ɖ߂��āA�X�^�~�i�Q�[�W�̐F�����ɖ߂�
                Player.instance.staminaConsumeRatio = 50;
                Player.instance.staminaSlider.fillRect.GetComponent<Image>().color = keepStaminaColor;
                isUseStaminaItem = false;
                break;

            //�e�X�g�p�g�p�A�C�e���@
            case 995:
                // �A�C�e���J�E���g������
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                // ���[�J�����W�����[���h���W�ɕϊ�
                Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
                Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

                // Addressables���g�p���ăv���n�u���X�e�[�W��ɔ񓯊�����
                await Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
                break;
        }
    }

    /// <summary>
    /// �C���x���g�������Z�b�g����
    /// </summary>
    void ResetInventoryItem() 
    {
        keepItemId = noneItemId;
        keepItemCount = minKeepItemCount;
        keepItemEffectValue = defaultKeepItemEffectValue;
        useItemCountText.text = keepItemCount.ToString();
        useItemNameText.text = "";
        useItemExplanationText.text = "";
        keepItemPrefabPath = noneItemPrefabPath;
        keepItemSpawnPosition = defaultItemSpawnPosition;
        keepItemSpawnRotation = defaultItemSpawnRotation;
        useItemImage.sprite = null;
        useItemImage.color = new Color(255, 255, 255, 0.05f);
        isUseStaminaItem = false;
    }
}
