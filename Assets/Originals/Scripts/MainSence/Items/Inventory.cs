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
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static Inventory instance;

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("�g�p�A�C�e���p�l���֘A")]
    [Header("�g�p�A�C�e���p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject useItemPanel;

    [Header("�g�p�A�C�e�������J�E���g�e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Text useItemCountText;

    [Header("�g�p�A�C�e���摜(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Image useItemImage;

    [Header("�g�p�A�C�e���e�L�X�g�m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public GameObject useItemTextPanel;

    [Header("�g�p�A�C�e�����e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Text useItemNameText;

    [Header("�g�p�A�C�e�������e�L�X�g(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Text useItemExplanationText;


    /// <summary>
    /// �A�C�e��ID�Ǘ�
    /// </summary>
    private int keepItemId;

    /// <summary>
    /// �A�C�e������������ID
    /// </summary>
    private const int noneItemId = 99999;

    /// <summary>
    /// �A�C�e���̃v���n�u��Addressables��
    /// </summary>
    private string keepItemPrefabPath;

    /// <summary>
    /// �A�C�e���̃v���n�u��Addressables��(��)
    /// </summary>
    private const string noneItemPrefabPath = "";

    /// <summary>
    /// �A�C�e�������ʒu
    /// </summary>
    private Vector3 keepItemSpawnPosition;

    /// <summary>
    /// �A�C�e�������ʒu(�f�t�H���g)
    /// </summary>
    private Vector3 defaultItemSpawnPosition = new Vector3(0, 0, 0);

    /// <summary>
    /// �A�C�e���̉�]���l
    /// </summary>
    private Quaternion keepItemSpawnRotation;

    /// <summary>
    /// �A�C�e���̉�]���l(�f�t�H���g)
    /// </summary>
    private Quaternion defaultItemSpawnRotation = Quaternion.identity;

    /// <summary>
    /// �A�C�e��������
    /// </summary>
    private int keepItemCount;

    /// <summary>
    /// �A�C�e��������(�f�t�H���g)
    /// </summary>
    private const int minKeepItemCount = 0;

    /// <summary>
    /// �A�C�e�����ʒl
    /// </summary>
    private int keepItemEffectValue;

    /// <summary>
    /// �A�C�e�����ʒl(�f�t�H���g�l)
    /// </summary>
    private const int defaultKeepItemEffectValue = 0;

    /// <summary>
    /// �X�^�~�i�����܎g�p�t���O
    /// </summary>
    private bool isUseStaminaItem;

    /// <summary>
    /// Player.cs
    /// </summary>
    private Player player;

    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// Inventory��p��AudioSource
    /// </summary>
    private AudioSource audioSourceInventorySE;

    /// <summary>
    /// �X�^�~�i������SE��ID
    /// </summary>
    private readonly int useStaminaEnhancerSEid = 12;

    /// <summary>
    /// �A�C�e��ID�Ǘ����擾
    /// </summary>
    /// <returns>�A�C�e��ID�Ǘ�</returns>
    public int GetKeepItemId() 
    {
        return keepItemId;
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
    /// SE���ʂ�0�`1�֕ύX
    /// </summary>
    /// <param name="volume">����</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceInventorySE != null)
        {
            audioSourceInventorySE.volume = volume;
        }
    }

    /// <summary>
    /// �V�[���J�ڎ��Ɏg�p�A�C�e���p�l���֘A���Đݒ�y�у��Z�b�g
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //AudioSource�̏�����
        InitializeAudioSource();

        //GameController.cs��UI�𔽉f������
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

        //�C���x���g�������Z�b�g����
        ResetInventoryItem();

        //�A�C�e��DB�����Z�b�g
        sO_Item.ResetItems();
    }

    /// <summary>
    /// AudioSource�̏�����
    /// </summary>
    private void InitializeAudioSource()
    {
        //���ׂĂ�AudioSource���擾
        var audioSources = GetComponents<AudioSource>();
        if (audioSources.Length < 3)
        {
            //3�ڂ�AudioSource���s�����Ă���ꍇ�A�ǉ�����
            audioSourceInventorySE = gameObject.AddComponent<AudioSource>();
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }
        else
        {
            //3�Ԗڂ�AudioSource���A�C�e���g�p���p�Ɋ��蓖��
            //(Player�I�u�W�F�N�g�ɂ��̃X�N���v�g��Player.cs���A�^�b�`���Ă���B
            //�ړ����ƃA�C�e���擾���̋������������p)
            audioSourceInventorySE = audioSources[2];
            audioSourceInventorySE.playOnAwake = false;
            audioSourceInventorySE.volume = 1.0f;
        }

        //MusicController�Őݒ肳��Ă���SE�p��AudioMixerGroup��ݒ肷��
        audioSourceInventorySE.outputAudioMixerGroup = MusicController.Instance.audioMixerGroupSE;
    }

    private void Awake()
    {
        //�V���O���g���̐ݒ�
        if (instance == null)
        {
            instance = this;

            //�V�[���J�ڎ��ɔj������Ȃ��悤�ɂ���
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            //���łɃC���X�^���X�����݂���ꍇ�͔j��
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = GetComponent<Player>();

        //Addressables��������
        Addressables.InitializeAsync();

        //�C���x���g�������Z�b�g
        ResetInventoryItem();
    }

    void Update()
    {
        //�C���x���g���A�C�e���g�p
        if (UseInventoryItem() && !PauseController.instance.isPause && Time.timeScale != 0 && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) UseItem();
    }

    /// <summary>
    /// �E�N���b�N�ŃC���x���g���A�C�e���g�p����֐�
    /// </summary>
    /// <returns>�E�N���b�N��true</returns>
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
        //�C���x���g���ɐV�K�ǉ����鏈��
        if (keepItemId == noneItemId)
        {

            //�A�C�e��id��ݒ�
            keepItemId = id;

            //�A�C�e���v���n�u�̃p�X��ݒ�
            keepItemPrefabPath = path;

            //�A�C�e���̍��W��ݒ�
            keepItemSpawnPosition = position;

            //�A�C�e���̉�]�l��ݒ�
            keepItemSpawnRotation = rotation;

            //�A�C�e���摜��ݒ�
            useItemImage.sprite = icon;

            //�A�C�e�����ʒl��ݒ�
            keepItemEffectValue = effectValue;

            //�A�C�e���摜�̕s�����x��100%�ɂ���
            useItemImage.color = new Color(255, 255, 255, 1);

            //�A�C�e����������ݒ�
            keepItemCount = count;
            useItemCountText.text = count.ToString();

            //�A�C�e�����Ɛ�������ݒ�
            useItemNameText.text = itemName;
            useItemExplanationText.text = description;          
        }
        else
        {
            //�A�C�e����������ǉ�
            keepItemCount = count;
            useItemCountText.text = count.ToString();
        }
    }

    /// <summary>
    /// �A�C�e�����g�p����
    /// </summary>
    public void UseItem() 
    {
        //�A�C�e����������0���傫���ꍇ
        if (minKeepItemCount < keepItemCount)
        {
            //�A�C�e���g�p���̌��ʂ�K�p����
            ActivationUseItem(keepItemId);

            //�A�C�e������0�ɂȂ����ꍇ
            if (keepItemCount == minKeepItemCount)
            {
                //�C���x���g�������Z�b�g
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
                //�X�^�~�i���ʂ��K�p���̏ꍇ
                if (isUseStaminaItem)
                {
                    //�X�^�~�i���ʂ��K�p���ł���|�̃��b�Z�[�W��\��
                    MessageController.instance.ShowInventoryMessage(3);

                    await UniTask.Delay(TimeSpan.FromSeconds(3));

                    MessageController.instance.ResetMessage();
                    return;
                }

                //�A�C�e���J�E���g������
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //�X�^�~�i�Q�[�W�̌��̐F��ۑ�
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
                //�A�C�e���J�E���g������
                --keepItemCount;
                useItemCountText.text = keepItemCount.ToString();
                sO_Item.ReduceUseItem(keepItemId, keepItemCount);

                //���[�J�����W�����[���h���W�ɕϊ�
                Vector3 worldPosition = Player.instance.transform.TransformPoint(keepItemSpawnPosition);
                Quaternion worldRotation = Player.instance.transform.rotation * keepItemSpawnRotation;

                //Addressables���g�p���ăv���n�u���X�e�[�W��ɔ񓯊�����
                await Addressables.InstantiateAsync(keepItemPrefabPath, worldPosition, worldRotation);
                break;
        }
    }

    /// <summary>
    /// �C���x���g�������Z�b�g����
    /// </summary>
    void ResetInventoryItem() 
    {
        //���ꂼ��̕ϐ��̒l�E�t���O�l������������
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
