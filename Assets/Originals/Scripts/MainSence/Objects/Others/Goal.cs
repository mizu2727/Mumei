using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class Goal : MonoBehaviour
{
    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("����p�̃A�C�e��id")]
    [SerializeField] public int anserItemId;

    [Header("�S�[���p�l��(�q�G�����L�[��̃p�l�����A�^�b�`����K�v������)")]
    [SerializeField] private GameObject GoalPanel;

    [Header("�~�X�e���[�A�C�e�����̃{�^��(�q�G�����L�[��̃{�^�����A�^�b�`����K�v������)")]
    [SerializeField] private Button[] selectMysteryItemButton;

    [Header("�~�X�e���[�A�C�e���摜(�q�G�����L�[��̉摜���A�^�b�`����K�v������)")]
    [SerializeField] private Image[] selectMysteryItemImage;

    [Header("�S�[������")]
    public bool isGoalPanel;

    private void Start()
    {
        isGoalPanel = false;
        ViewGoalPanel();

        InitializeSelectMysteryItemUI();
    }

    public async void GoalCheck()
    {
        sO_Item.CleanNullItems();

        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {

            MessageController.instance.ShowGoalMessage(1);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }


        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            MessageController.instance.ShowGoalMessage(2);

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
        MessageController.instance.ShowGoalMessage(3);

        // MysteryItem��UpdateSelectMysteryItemUI�ŏ������邽�߁A�����ł͕ێ����Ȃ�
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
        isGoalPanel = true;
        ViewGoalPanel();

    }

    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            GoalPanel.SetActive(true);

            // CanvasGroup �̏�Ԃ��m�F
            CanvasGroup canvasGroup = GoalPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            // �摜���X�V
            UpdateSelectMysteryItemUI();
        }
        else
        {
            Time.timeScale = 1;
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
        // Image �z��𓮓I�Ɏ擾�i�{�^���̎q�v�f����j
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            // �{�^�����g�� Image ���m�F
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true; // �{�^���� raycast ��L����
            }

            // �q�v�f�� Image ���擾
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} �Ɏq�v�f�� Image �R���|�[�l���g��������܂���");
            }
            else
            {
                selectMysteryItemImage[i].raycastTarget = false; // �摜�̃N���b�N�𖳌���
                selectMysteryItemImage[i].enabled = false; // ������ԂŔ�\��
            }

            // �{�^���̏�Ԃ�ݒ�
            selectMysteryItemButton[i].interactable = false;

            // �N���b�N�C�x���g��ǉ�
            int index = i;
            selectMysteryItemButton[i].onClick.RemoveAllListeners(); // �������X�i�[���N���A
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }

        
    }

    public void OnClickedselectMysteryItemButton(int index)
    {
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        if (index < mysteryItems.Count && mysteryItems[index] != null)
        {
            if (mysteryItems[index].id == anserItemId)
            {
                Debug.Log("�����̃A�C�e�����I������܂����I");
                SceneManager.LoadScene("GameClearScene");
            }
            else
            {
                Debug.Log("�s�����̃A�C�e�����I������܂���");
                // �s�������̏����i��F���b�Z�[�W�\���j
            }
        }
    }

    // �~�X�e���[�A�C�e����UI���X�V
    private void UpdateSelectMysteryItemUI()
    {
        // itemList����MysteryItem���擾
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i < mysteryItems.Count && mysteryItems[i] != null)
            {
                selectMysteryItemButton[i].interactable = true;
                Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    selectMysteryItemImage[i].sprite = mysteryItems[i].icon;
                    selectMysteryItemImage[i].enabled = (mysteryItems[i].icon != null);
                    selectMysteryItemImage[i].color = Color.white;
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
