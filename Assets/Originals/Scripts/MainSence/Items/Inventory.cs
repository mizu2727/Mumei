//�p�~���ė��p�\��
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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


    //�A�C�e��ID�Ǘ�(�A�C�e���C���x���g�������g���p)
    private readonly List<int> idList = new();

    [Header("�A�C�e��ID�Ǘ�(�A�C�e���C���x���g��1�g���p�B�ҏW�֎~)")]
    public int keepItemId;

    //�A�C�e������������ID(�A�C�e���C���x���g��1�g���p)
    private const int noneItemId = 99999;

    //�A�C�e���������Ǘ�(�A�C�e���C���x���g�������g���p)
    private readonly List<int> countList = new();

    //�A�C�e���������Ǘ�(�A�C�e���C���x���g��1�g���p)
    private int keepItemCount;

    private const int minKeepItemCount = 0;

    //�A�C�e�����X�g�̃C���f�b�N�X�ԍ�
    int checkIndex;

    private Player player;


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
        keepItemId = noneItemId;
        keepItemCount = minKeepItemCount;
        useItemCountText.text = keepItemCount.ToString();
        useItemImage.sprite = null;
        useItemImage.color = new Color(255, 255, 255, 0.05f);
    }

    void Update()
    {
        if (UseInventoryItem()) UseItem();
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
    /// �C���x���g�����ɃA�C�e����ǉ�
    /// </summary>
    /// <param name="id">�A�C�e��id</param>
    /// <param name="icon">�A�C�e���̉摜</param>
    /// <param name="itemName">�A�C�e���̖��O</param>
    /// <param name="count">�A�C�e���̌�</param>
    public void GetItem(int id, Sprite icon, string itemName, int count)
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

            useItemImage.sprite = icon;
            useItemImage.color = new Color(255, 255, 255, 1);

            //�@�A�C�e����������ݒ�
            countList.Add(count);
            keepItemCount = count;
            useItemCountText.text = count.ToString();

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
            Debug.Log("�C���x���g���A�C�e��item���g�p");
            --keepItemCount;
            useItemCountText.text = keepItemCount.ToString();

            sO_Item.ReduceUseItem(keepItemId, keepItemCount);

            Debug.Log("id:" + keepItemId + "��keepItemCount(�g�p�A�C�e��(����)):" + keepItemCount);

            if (keepItemCount == minKeepItemCount)
            {
                keepItemId = noneItemId;
                useItemImage.sprite = null;
                useItemImage.color = new Color(255, 255, 255, 0.05f);
            }
        }
        else 
        {
            Debug.Log("�g�p�ł���A�C�e��item���Ȃ��悤��");
        }
        
    }
}
