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
    [SerializeField] private Goal goal;//�S�[��
    [SerializeField] private GameObject pausePanel;//�|�[�Y�p�l��

    [SerializeField] private GameObject viewItemsPanel;//�A�C�e���m�F�p�l��

    [SerializeField] private GameObject documentInventoryPanel;//�h�L�������g�m�F�p�l��
    [SerializeField] private GameObject documentExplanationPanel;//�h�L�������g�������p�l��
    [SerializeField] private Text documentNameText;//�h�L�������g���̃e�L�X�g
    [SerializeField] private Text documentExplanationText;//�h�L�������g�������e�L�X�g

    [SerializeField] private GameObject mysteryItemInventoryPanel;//�~�X�e���[�A�C�e���m�F�p�l��
    [SerializeField] private Button[] mysteryItemNameButton;//�~�X�e���[�A�C�e�����̃{�^��
    [SerializeField] private Text[] mysteryItemNameText;//�~�X�e���[�A�C�e�����̃e�L�X�g
    [SerializeField] private Image[] mysteryItemImage;//�~�X�e���[�A�C�e���摜
    [SerializeField] private Text[] mysteryItemExplanationText;//�~�X�e���[�A�C�e���������e�L�X�g
    [SerializeField] private GameObject mysteryItemExplanationPanel;//�~�X�e���[�A�C�e���������p�l��


    public bool isPause = false;
    public bool isViewItemsPanel = false;
    public bool isDocumentPanels = false;
    public bool isDocumentExplanationPanel = false;
    public bool isMysteryItemPanels = false;
    public bool isMysteryItemExplanationPanel = false;


    private List<string> mysteryItemNames = new(); // �~�X�e���[�A�C�e�����̃��X�g
    private List<string> mysteryItemExplanations = new(); // �~�X�e���[�A�C�e���������̃��X�g


    //���ʂ�ScriptableObject���A�^�b�`����K�v������
    [SerializeField] public SO_Item sO_Item;

    //SE�n
    private AudioSource audioSourceSE;
    public AudioClip buttonSE;//�{�^��SE
    public AudioClip documentNameButtonSE;//�h�L�������g���̃{�^��SE

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

        //MusicController.instance.PauseBGM();
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);
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

            MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);
            //MusicController.instance.UnPauseBGM();
        }
        
    }


    //�A�C�e���m�F
    public void OnClickedViewItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);

        isPause = false;
        ChangeViewPausePanel();


        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);
        //MusicController.instance.StopBGM();
        //GameController.instance.ReturnToTitle();
        SceneManager.LoadScene("TitleScene");
    }



    public void OnClickedViewDocumentButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);

        // �h�L�������g�p�l����\��
        isDocumentPanels = true;
        ChangeViewDocumentPanel();

        // �~�X�e���[�A�C�e���p�l�����\��
        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    public void OnClickedViewMysteryItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);

        // �~�X�e���[�A�C�e���p�l����\��
        isMysteryItemPanels = true;
        ChangeViewMysteryItemPanel();

        // �h�L�������g�p�l�����\��
        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        // �摜�Ɛ����e�L�X�g���N���A
        if (mysteryItemImage.Length > 0)
        {
            mysteryItemImage[0].sprite = null;
            mysteryItemImage[0].enabled = false;
        }
        if (mysteryItemExplanationText.Length > 0)
        {
            mysteryItemExplanationText[0].text = "";
        }
    }


    //�|�[�Y��ʂ֖߂�
    public void OnClickedReturnToPausePanel()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);

        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }


    public void OnClickedDocumentNameButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, documentNameButtonSE);

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
            documentInventoryPanel.transform.SetAsLastSibling();
            documentInventoryPanel.SetActive(true);

        }
        else 
        {
            documentInventoryPanel.SetActive(false);

            isDocumentExplanationPanel = false;

            ChangeViewDocumentExplanationPanel();
        }
    }

    //�h�L�������g�������p�l���̕\��/��\��
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            documentExplanationPanel.SetActive(false);
        }
    }


    //DocumentNameText�̋L�ړ��e��ύX
    public void ChangeDocumentNameText(string documentName) 
    {
        documentNameText = documentNameText.GetComponent<Text>();
        documentNameText.text = documentName;
    }

    //DocumentExplanationText�̋L�ړ��e��ύX
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        documentExplanationText = documentExplanationText.GetComponent<Text>();
        documentExplanationText.text = documentDescription;
        Debug.Log("�h�L�������g�������F"+ documentExplanationText.text);
    }


    //�~�X�e���[�A�C�e���m�F�p�l���̕\��/��\��
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanels)
        {
            mysteryItemInventoryPanel.transform.SetAsLastSibling();
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            mysteryItemInventoryPanel.SetActive(false);

            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();

            // �摜�Ɛ����e�L�X�g�����Z�b�g
            if (mysteryItemImage.Length > 0)
            {
                mysteryItemImage[0].sprite = null;
                mysteryItemImage[0].enabled = false;
            }
            if (mysteryItemExplanationText.Length > 0)
            {
                mysteryItemExplanationText[0].text = "";
            }
        }
    }


    //�~�X�e���[�A�C�e���������p�l���̕\��/��\��
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            mysteryItemExplanationPanel.SetActive(false);
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
        MusicController.Instance.PlayAudioSE(audioSourceSE, buttonSE);

        if (index < mysteryItemNames.Count)
        {
            string itemName = mysteryItemNames[index];
            var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

            if (item != null)
            {
                // �~�X�e���[�A�C�e�������p�l����\��
                isMysteryItemExplanationPanel = true;
                ChangeViewMysteryItemExplanationPanel();

                // �h�L�������g�����p�l�����\���ɂ���
                isDocumentExplanationPanel = false;
                ChangeViewDocumentExplanationPanel();

                // �����e�L�X�g���X�V
                if (mysteryItemExplanationText.Length > 0)
                {
                    mysteryItemExplanationText[0].text = item.description;
                    Debug.Log($"Set explanation text to: {item.description}");
                }

                // �摜���X�V
                if (mysteryItemImage.Length > 0)
                {
                    mysteryItemImage[0].sprite = item.icon;
                    mysteryItemImage[0].enabled = (item.icon != null);
                    Debug.Log($"Set image to: {(item.icon != null ? item.icon.name : "null")}");
                }
                else
                {
                    Debug.LogWarning("mysteryItemImage �����ݒ�ł�");
                }

                Debug.Log($"Clicked Mystery Item: {itemName}");
            }
            else
            {
                Debug.LogError($"�A�C�e�� '{itemName}' ��������܂���");
            }
        }
    }


    // �~�X�e���[�A�C�e������ǉ����AUI�ɔ��f
    public void ChangeMysteryItemTexts(string mysteryItemName, string mysteryItemDescription)
    {
        // �A�C�e�����X�g����Y������A�C�e��������
        var item = sO_Item.itemList.Find(x => x.itemName == mysteryItemName && x.itemType == ItemType.MysteryItem);
        if (item != null && !mysteryItemNames.Contains(mysteryItemName))
        {
            mysteryItemNames.Add(mysteryItemName);
            mysteryItemExplanations.Add(mysteryItemDescription);
            UpdateMysteryItemUI();
        }
        else 
        {
            Debug.LogWarning($"MysteryItem '{mysteryItemName}' ��������Ȃ����A���łɒǉ��ς݂ł�");
        }
    }

    // �~�X�e���[�A�C�e����UI���X�V
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            if (i < mysteryItemNames.Count)
            {
                string itemName = mysteryItemNames[i];
                var item = sO_Item.itemList.Find(x => x.itemName == itemName && x.itemType == ItemType.MysteryItem);

                if (item != null)
                {
                    mysteryItemNameText[i].text = itemName;
                    mysteryItemNameButton[i].interactable = true;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        mysteryItemExplanationText[i].text = mysteryItemExplanations[i];
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        mysteryItemImage[i].sprite = item.icon;
                        mysteryItemImage[i].enabled = (item.icon != null);
                    }
                }
                else
                {
                    Debug.LogWarning($"�A�C�e�� '{itemName}' ��������܂���");
                    mysteryItemNameText[i].text = "?????????";
                    mysteryItemNameButton[i].interactable = false;

                    if (i < mysteryItemExplanationText.Length)
                    {
                        mysteryItemExplanationText[i].text = "";
                    }

                    if (i < mysteryItemImage.Length)
                    {
                        mysteryItemImage[i].sprite = null;
                        mysteryItemImage[i].enabled = false;
                    }
                }
            }
            else
            {
                mysteryItemNameText[i].text = "?????????";
                mysteryItemNameButton[i].interactable = false;

                if (i < mysteryItemExplanationText.Length)
                {
                    mysteryItemExplanationText[i].text = "";
                }

                if (i < mysteryItemImage.Length)
                {
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
    }
}
