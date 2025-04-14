using Mono.Cecil.Cil;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;



public class Goal : MonoBehaviour
{
    //���ʂ�ScriptableObject���A�^�b�`����K�v������
    [SerializeField] public SO_Item sO_Item;

    [SerializeField] public int anserItemId;//����p�̃A�C�e��id


    [SerializeField] private GameObject GoalPanel;
    [SerializeField] private Button[] selectMysteryItemButton;//�~�X�e���[�A�C�e�����̃{�^��
    [SerializeField] private Image[] selectMysteryItemImage;//�~�X�e���[�A�C�e���摜

    public bool isGoalPanel;

    private SO_Item.ItemData mysteryItem; // MysteryItem �^�C�v�̃A�C�e����ێ�

    private void Start()
    {
        isGoalPanel = false;
        ViewGoalPanel();

        InitializeSelectMysteryItemUI();
    }

    public async void GoalCheck()
    {
        sO_Item.CleanNullItems();

        Debug.Log($"GoalCheck ���s����itemList��: {sO_Item.itemList.Count}");
        for (int i = 0; i < sO_Item.itemList.Count; i++)
        {
            var item = sO_Item.itemList[i];
            if (item != null)
            {
                Debug.Log($"itemList[{i}]: Name={item.itemName}, Type={item.itemType}, Icon={(item.icon != null ? item.icon.name : "null")}");
            }
            else
            {
                Debug.LogWarning($"itemList[{i}] �� null ���܂܂�Ă��܂��I");
            }
        }

        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {
            string message = "�h�L�������g���W�߂Ă�������";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }


        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            string message = "�~�X�e���[�A�C�e�����W�߂Ă�������";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        string message = "���̃h�L�������g�Ɋ֌W����A�C�e����I������";

        MessageController.instance.ShowMessage(message);

        // MysteryItem ���������ĕێ�
        mysteryItem = sO_Item.itemList.Find(item => item != null && item.itemType == ItemType.MysteryItem);


        if (mysteryItem == null)
        {
            Debug.LogWarning("MysteryItem ��������܂���ł���");
        }
        else
        {
            Debug.Log($"MysteryItem ��������܂���: {mysteryItem.itemName}");
        }


        isGoalPanel = true;
        ViewGoalPanel();

    }

    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            GoalPanel.SetActive(true);

            // �摜���X�V
            UpdateSelectMysteryItemUI(); // �摜�\�����X�V
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GoalPanel.SetActive(false);

            // �摜���\���ɂ���i�K�v�ɉ����āj
            for (int i = 0; i < selectMysteryItemImage.Length; i++)
            {
                if (selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }

    public void OnClickedReturnToInGameButton() 
    {
        isGoalPanel = false;
        ViewGoalPanel();

        MessageController.instance.ResetMessage();
    }


    // �~�X�e���[�A�C�e����UI��������
    private void InitializeSelectMysteryItemUI()
    {
        if (selectMysteryItemButton == null)
        {
            Debug.LogError("mysteryItemNameButton is not assigned!");
            return;
        }

        // Image �z��𓮓I�Ɏ擾�i�{�^���̎q�v�f����j
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            // �{�^�����g�� Image �𖳌���
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = false;
                Debug.Log($"Button {i} �� Image �𖳌������܂���");
            }

            // �q�v�f�� Image ���擾
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} �� Image �R���|�[�l���g��������܂���");
            }
            else
            {
                selectMysteryItemImage[i].raycastTarget = false; // �摜�̃N���b�N�𖳌���

            }
        }

        // �{�^���ɃN���b�N�C�x���g��ǉ�
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            int index = i;
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }
    }

    public void OnClickedselectMysteryItemButton(int index)
    {
        if (mysteryItem != null && index == 0) // �ŏ��̃{�^���̂ݏ���
        {
            Debug.Log($"Clicked Mystery Item: {mysteryItem.itemName}");
            // ���𔻒���s���ꍇ�́AanserItemId ���g�p
            if (mysteryItem.id == anserItemId)
            {
                Debug.Log("�����̃A�C�e�����I������܂����I");
            }
            else
            {
                Debug.Log("�s�����̃A�C�e�����I������܂���");
            }
        }
    }


    

    // �~�X�e���[�A�C�e����UI���X�V
    private void UpdateSelectMysteryItemUI()
    {
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i == 0 && mysteryItem != null && mysteryItem.itemType == ItemType.MysteryItem)
            {
                selectMysteryItemButton[i].interactable = true;
                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    Debug.Log($"Setting image for {mysteryItem.itemName}, Icon: {(mysteryItem.icon != null ? mysteryItem.icon.name : "null")}");
                    selectMysteryItemImage[i].sprite = mysteryItem.icon;
                    selectMysteryItemImage[i].enabled = (mysteryItem.icon != null);
                    selectMysteryItemImage[i].color = Color.white; // �A���t�@�l�������I��1��
                    Debug.Log($"Image {i} - Enabled: {selectMysteryItemImage[i].enabled}, Sprite: {(selectMysteryItemImage[i].sprite != null ? selectMysteryItemImage[i].sprite.name : "null")}, Alpha: {selectMysteryItemImage[i].color.a}, Scale: {selectMysteryItemImage[i].transform.localScale}");
                }
            }
            else
            {
                selectMysteryItemButton[i].interactable = false;
                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }


}
