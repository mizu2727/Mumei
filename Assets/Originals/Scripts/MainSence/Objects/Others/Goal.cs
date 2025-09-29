using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;



public class Goal : MonoBehaviour
{
    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;

    [Header("����p�̃A�C�e��id")]
    [SerializeField] private int anserItemId;

    [Header("����p�̃`���[�g���A���A�C�e��id")]
    [SerializeField] private int anserTutorialItemId;

    [Header("�S�[���p�l��(�q�G�����L�[��̃p�l�����A�^�b�`����K�v������)")]
    [SerializeField] private GameObject GoalPanel;

    [Header("�~�X�e���[�A�C�e�����̃{�^��(�q�G�����L�[��̃{�^�����A�^�b�`����K�v������)")]
    [SerializeField] private Button[] selectMysteryItemButton;

    [Header("�~�X�e���[�A�C�e���摜(�q�G�����L�[��̉摜���A�^�b�`����K�v������)")]
    [SerializeField] private Image[] selectMysteryItemImage;

    [Header("�S�[���t���O")]
    public bool isGoalPanel;

    [Header("�`���[�g���A���t���O(�`���[�g���A���X�e�[�W�ŃI���ɂȂ�)")]
    [SerializeField] public bool isTutorial;



    private void Start()
    {
        //�S�[���p�l�����\��
        isGoalPanel = false;
        ViewGoalPanel();

        //�S�[���p�l�����̃~�X�e���[�A�C�e����UI��������
        InitializeSelectMysteryItemUI();
    }

    /// <summary>
    /// �v���C���[���S�[���I�u�W�F�N�g�ɐG�ꂽ�ꍇ�̏���
    /// </summary>
    public async void GoalCheck()
    {
        //�A�C�e����null�`�F�b�N
        sO_Item.CleanNullItems();

        //�h�L�������g����肵�Ă��Ȃ��ꍇ
        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {
            //�h�L�������g���K�v�ł���|�̃��b�Z�[�W��\�����A�������X�L�b�v
            MessageController.instance.ShowGoalMessage(1);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();
            return;
        }

        //�~�X�e���[�A�C�e������肵�Ă��Ȃ��ꍇ
        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            //�~�X�e���[�A�C�e�����K�v�ł���|�̃��b�Z�[�W��\�����A�������X�L�b�v
            MessageController.instance.ShowGoalMessage(2);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();
            return;
        }
        else 
        {
            //�~�X�e���[�A�C�e����I������̏�����
            MysteryItemCheck();
        } 
    }

    /// <summary>
    /// �~�X�e���[�A�C�e����I������̏���
    /// </summary>
    void MysteryItemCheck() 
    {
        //�~�X�e���[�A�C�e����I������|�̃��b�Z�[�W��\��
        isGoalPanel = true;
        MessageController.instance.ShowGoalMessage(3);

        //MysteryItem��UpdateSelectMysteryItemUI�ŏ������邽�߁A�����ł͕ێ����Ȃ�
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);
        
        //�S�[���p�l����\��
        ViewGoalPanel();

    }

    /// <summary>
    /// �S�[���p�l���̕\��/��\��
    /// </summary>
    public void ViewGoalPanel()
    {
        if (isGoalPanel)
        {
            //�ꎞ��~
            Time.timeScale = 0;

            //�}�E�X�J�[�\����L���ɂ��A�Œ������
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            //�p�l����\��
            GoalPanel.SetActive(true);

            //CanvasGroup�̏�Ԃ��m�F
            CanvasGroup canvasGroup = GoalPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            //�摜���X�V
            UpdateSelectMysteryItemUI();
        }
        else
        {
            //�}�E�X�J�[�\�����\���ɂ��A�Œ肷��
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GoalPanel.SetActive(false);

            //�摜���\���ɂ���
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

    /// <summary>
    /// �u�߂�v�{�^������
    /// </summary>
    public async void OnClickedReturnToInGameButton() 
    {
        //�ꎞ��~����
        Time.timeScale = 1;

        //�p�l����\��
        isGoalPanel = false;
        ViewGoalPanel();

        //���b�Z�[�W�e�L�X�g����ɂ���
        MessageController.instance.ResetMessage();

        //�`���[�g���A���̏ꍇ�A�`���[�g���A���I���̎|�̃��b�Z�[�W��\������
        if (GameController.instance.isTutorialGoalFlag) await MessageController.instance.ShowSystemMessage(14);
    }

    /// <summary>
    /// �~�X�e���[�A�C�e����UI��������
    /// </summary>
    private void InitializeSelectMysteryItemUI()
    {
        //Image�z��𓮓I�Ɏ擾�i�{�^���̎q�v�f����j
        selectMysteryItemImage = new Image[selectMysteryItemButton.Length];
        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            //�{�^�����g��Image���m�F
            Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                //�{�^����raycast��L����
                buttonImage.raycastTarget = true;
            }

            //�q�v�f��Image���擾
            selectMysteryItemImage[i] = selectMysteryItemButton[i].GetComponentInChildren<Image>();
            if (selectMysteryItemImage[i] == null)
            {
                Debug.LogError($"Button {i} �Ɏq�v�f�� Image �R���|�[�l���g��������܂���");
            }
            else
            {
                //�摜�̃N���b�N�𖳌���
                selectMysteryItemImage[i].raycastTarget = false;

                //������ԂŔ�\���ɂ���
                selectMysteryItemImage[i].enabled = false; 
            }

            //�{�^���̏�Ԃ�ݒ�
            selectMysteryItemButton[i].interactable = false;

            //�N���b�N�C�x���g��ǉ�
            int index = i;

            //�������X�i�[���N���A
            selectMysteryItemButton[i].onClick.RemoveAllListeners(); 

            //�V�K���X�i�[��ǉ�
            selectMysteryItemButton[i].onClick.AddListener(() => OnClickedselectMysteryItemButton(index));
        }       
    }

    /// <summary>
    /// �~�X�e���[�A�C�e���摜�����������ꍇ�̏���
    /// </summary>
    /// <param name="index"></param>
    public void OnClickedselectMysteryItemButton(int index)
    {
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        if (index < mysteryItems.Count && mysteryItems[index] != null)
        {
            //�����̃~�X�e���[�A�C�e���ł��邩�𔻒�
            if (mysteryItems[index].id == anserItemId)
            {
                //�������̏���

                //�v���C���[���폜
                Player.instance.DestroyPlayer();
                
                //��ʑJ��
                SceneManager.LoadScene("GameClearScene");
            }
            //�����̃~�X�e���[�A�C�e��(�`���[�g���A����)�ł��邩�𔻒�
            else if (mysteryItems[index].id == anserTutorialItemId) 
            {
                //�`���[�g���A���I����̉�b��i�߂�
                GameController.instance.isTutorialGoalFlag = true;
                OnClickedReturnToInGameButton();
            }
            else
            {
                //�s�������̏���
                MessageController.instance.ShowGoalMessage(4);
            }
        }
    }

    /// <summary>
    /// �~�X�e���[�A�C�e����UI���X�V
    /// </summary>
    private void UpdateSelectMysteryItemUI()
    {
        //itemList����MysteryItem���擾
        var mysteryItems = sO_Item.itemList.FindAll(item => item != null && item.itemType == ItemType.MysteryItem);

        for (int i = 0; i < selectMysteryItemButton.Length; i++)
        {
            if (i < mysteryItems.Count && mysteryItems[i] != null)
            {
                //�{�^���N���b�N��L��
                selectMysteryItemButton[i].interactable = true;
                Image buttonImage = selectMysteryItemButton[i].GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.raycastTarget = true;
                }

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    //�~�X�e���[�A�C�e���摜��ݒ�
                    selectMysteryItemImage[i].sprite = mysteryItems[i].icon;
                    selectMysteryItemImage[i].enabled = (mysteryItems[i].icon != null);
                    selectMysteryItemImage[i].color = Color.white;
                }
            }
            else
            {
                //�{�^���N���b�N�𖳌�
                selectMysteryItemButton[i].interactable = false;

                if (i < selectMysteryItemImage.Length && selectMysteryItemImage[i] != null)
                {
                    //�~�X�e���[�A�C�e���摜��null�ɂ���
                    selectMysteryItemImage[i].sprite = null;
                    selectMysteryItemImage[i].enabled = false;
                }
            }
        }
    }
}
