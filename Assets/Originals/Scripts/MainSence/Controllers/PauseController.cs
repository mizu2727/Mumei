using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseController : MonoBehaviour
{
    public static PauseController instance;


    [SerializeField] private Player player;//�v���C���[
    //[SerializeField] private Goal goal;//�S�[��
    [SerializeField] private GameObject pausePanel;//�|�[�Y�p�l��

    [SerializeField] private GameObject viewItemsPanel;//�A�C�e���m�F�p�l��

    [SerializeField] private GameObject DocumentInventoryPanel;//�h�L�������g�m�F�p�l��
    [SerializeField] private GameObject DocumentExplanationPanel;//�h�L�������g�������p�l��
    [SerializeField] private Text DocumentNameText;//�h�L�������g���̃e�L�X�g
    [SerializeField] private Text DocumentExplanationText;//�h�L�������g�������e�L�X�g

    [SerializeField] private GameObject MysteryItemInventoryPanel;//�~�X�e���[�A�C�e���m�F�p�l��
    [SerializeField] private Button[] mysteryItemNameButton;//�~�X�e���[�A�C�e�����̃{�^��
    [SerializeField] private Text[] mysteryItemNameText;//�~�X�e���[�A�C�e�����̃e�L�X�g
    [SerializeField] private Text[] mysteryItemExplanationText;//�~�X�e���[�A�C�e���������e�L�X�g
    [SerializeField] private GameObject MysteryItemExplanationPanel;//�~�X�e���[�A�C�e���������p�l��


    [SerializeField] public bool isPause = false;
    [SerializeField] public bool isViewItemsPanel = false;
    [SerializeField] public bool isDocumentPanels = false;
    [SerializeField] public bool isDocumentExplanationPanel = false;
    [SerializeField] public bool isMysteryItemPanels = false;
    [SerializeField] public bool isMysteryItemExplanationPanel = false;


    private List<string> mysteryItemNames = new(); // �~�X�e���[�A�C�e�����̃��X�g
    private List<string> mysteryItemExplanations = new(); // �~�X�e���[�A�C�e���������̃��X�g


    //���ʂ�ScriptableObject���A�^�b�`����K�v������
    [SerializeField] public SO_Item sO_Item;

    //Audio�n
    //public BGM BGMScript;//���C���Q�[��BGM
    //public AudioClip pauseSE;//�N���b�NSE


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


    private void Start()
    {
        // �p�l����������ԂŔ�\����
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();


        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        // �~�X�e���[�A�C�e���̃{�^���ƃe�L�X�g��������
        InitializeMysteryItemUI();
    }


    //P�L�[�Ń|�[�Y/�|�[�Y����
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanels && !isDocumentExplanationPanel && !isMysteryItemPanels
            && !isMysteryItemExplanationPanel)
        {
            ViewPausePanel();
        }
        else if (Input.GetKeyDown(KeyCode.P) && !player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
        }
    }

    public void ViewPausePanel() 
    {
        Time.timeScale = 0;
        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    //�|�[�Y����
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            Time.timeScale = 1;
            isPause = false;
            ChangeViewPausePanel();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }


    //�A�C�e���m�F
    public void OnClickedViewItemButton()
    {
        isPause = false;
        ChangeViewPausePanel();


        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("�^�C�g���֖߂�");
    }



    public void OnClickedViewDocumentButton() 
    {
        isDocumentPanels = true;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    public void OnClickedViewMysteryItemButton()
    {
        isMysteryItemPanels = true;
        ChangeViewMysteryItemPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();
    }


    //�|�[�Y��ʂ֖߂�
    public void OnClickedReturnToPausePanel()
    {
        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
    }


    public void OnClickedDocumentNameButton() 
    {
        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();
    }


    //�|�[�Y�p�l���̕\��/��\��
    void ChangeViewPausePanel()
    {
        if (isPause)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }


    //�A�C�e���n�m�F�p�l���̕\��/��\��
    void ChangeViewItemsPanel() 
    {
        if (isViewItemsPanel)
        {
            viewItemsPanel.SetActive(true);
        }
        else
        {
            viewItemsPanel.SetActive(false);
        }
    }


    //�h�L�������g�A�C�e���p�l���̕\��/��\��
    void ChangeViewDocumentPanel() 
    {
        if (isDocumentPanels)
        {
            DocumentInventoryPanel.transform.SetAsLastSibling();
            DocumentInventoryPanel.SetActive(true);

        }
        else 
        {
            DocumentInventoryPanel.SetActive(false);

            isDocumentExplanationPanel = false;

            ChangeViewDocumentExplanationPanel();
        }
    }

    //�h�L�������g�������p�l���̕\��/��\��
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            DocumentExplanationPanel.SetActive(true);
        }
        else
        {
            DocumentExplanationPanel.SetActive(false);
        }
    }


    //DocumentNameText�̋L�ړ��e��ύX
    public void ChangeDocumentNameText(string documentName) 
    {
        DocumentNameText = DocumentNameText.GetComponent<Text>();
        DocumentNameText.text = documentName;
    }

    //DocumentExplanationText�̋L�ړ��e��ύX
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        DocumentExplanationText = DocumentExplanationText.GetComponent<Text>();
        DocumentExplanationText.text = documentDescription;
        Debug.Log("�h�L�������g�������F"+ DocumentExplanationText.text);
    }


    //�~�X�e���[�A�C�e���m�F�p�l���̕\��/��\��
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanels)
        {
            MysteryItemInventoryPanel.transform.SetAsLastSibling();
            MysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            MysteryItemInventoryPanel.SetActive(false);

            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();
        }
    }

    
    //�~�X�e���[�A�C�e���������p�l���̕\��/��\��
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            MysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            MysteryItemExplanationPanel.SetActive(false);
        }
    }


    // �~�X�e���[�A�C�e����UI��������
    private void InitializeMysteryItemUI()
    {
        if (mysteryItemNameButton == null || mysteryItemNameText == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        // �{�^���ɃN���b�N�C�x���g��ǉ�
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            int index = i; // ���[�J���ϐ��ŃC���f�b�N�X���L���v�`��
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));
            mysteryItemNameText[i].text = "?????????";
        }
    }



    public void OnClickedMysteryItemNameButton(int index) 
    {
        if (index < mysteryItemNames.Count)
        {
            isMysteryItemExplanationPanel = true;
            ChangeViewMysteryItemExplanationPanel();

            // �A�C�e���̐�����\��
            var item = sO_Item.itemList[index]; 

            if (item != null && mysteryItemExplanationText.Length > 0)
            {
                mysteryItemExplanationText[0].text = item.description;
            }
            else 
            {
                Debug.LogError("mysteryItemNameText��mysteryItemExplanationText�̗v�f������v���Ă��Ȃ�");
            }
            Debug.Log($"Clicked Mystery Item: {mysteryItemNames[index]}");
        }
    }


    // �~�X�e���[�A�C�e������ǉ����AUI�ɔ��f
    public void ChangeMysteryItemTexts(string mysteryItemName, string mysteryItemDescription)
    {
        if (!mysteryItemNames.Contains(mysteryItemName))
        {
            mysteryItemNames.Add(mysteryItemName);
            mysteryItemExplanations.Add(mysteryItemName);
            UpdateMysteryItemUI();
        }
    }

    // �~�X�e���[�A�C�e����UI���X�V
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            //�{�^���ƃe�L�X�g��index����v����悤�ɂ���
            if (i < mysteryItemNames.Count)
            {
                mysteryItemNameText[i].text = mysteryItemNames[i];
                mysteryItemNameButton[i].interactable = true;

                mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
            }
            else
            {
                mysteryItemNameText[i].text = "?????????";
                mysteryItemNameButton[i].interactable = false;

                mysteryItemExplanationText[i].text = "";
            }
        }
    }
}
