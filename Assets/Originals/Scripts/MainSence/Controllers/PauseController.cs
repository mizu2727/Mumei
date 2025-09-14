using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PauseController : MonoBehaviour
{
    public static PauseController instance;

    [Header("�v���C���[")]
    [SerializeField] private Player player;//�v���C���[

    [Header("�S�[��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private Goal goal;//�S�[��

    [Header("�G(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private BaseEnemy[] baseEnemy;

    [Header("�|�[�Y�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject pausePanel;

    [Header("�A�C�e���m�F�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject viewItemsPanel;

    [Header("�h�L�������g�p�l���֘A(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject documentInventoryPanel;//�h�L�������g�m�F�p�l��
    [SerializeField] private GameObject documentExplanationPanel;//�h�L�������g�������p�l��
    [SerializeField] private Text documentNameText;//�h�L�������g���̃e�L�X�g
    [SerializeField] private Text documentExplanationText;//�h�L�������g�������e�L�X�g

    [Header("�~�X�e���[�A�C�e���p�l���֘A(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject mysteryItemInventoryPanel;//�~�X�e���[�A�C�e���m�F�p�l��
    [SerializeField] private Button[] mysteryItemNameButton;//�~�X�e���[�A�C�e�����̃{�^��
    [SerializeField] private Text[] mysteryItemNameText;//�~�X�e���[�A�C�e�����̃e�L�X�g
    [SerializeField] private Image[] mysteryItemImage;//�~�X�e���[�A�C�e���摜
    [SerializeField] private Text[] mysteryItemExplanationText;//�~�X�e���[�A�C�e���������e�L�X�g
    [SerializeField] private GameObject mysteryItemExplanationPanel;//�~�X�e���[�A�C�e���������p�l��

    [Header("�I�v�V�����p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject optionPanel;

    [Header("�^�C�g���֖߂�p�l��(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] private GameObject returnToTitlePanel;

    public bool isPause = false;
    public bool isViewItemsPanel = false;
    public bool isOptionPanel = false;
    public bool isReturnToTitlePanel = false;
    public bool isDocumentPanels = false;
    public bool isDocumentExplanationPanel = false;
    public bool isMysteryItemPanels = false;
    public bool isMysteryItemExplanationPanel = false;

    [Header("�`���[�g���A���p�t���O(�ҏW�֎~)")]
    public bool isTutorialNextMessageFlag = false;

    [Header("�`���[�g���A���p�n���}�[����t���O(�ҏW�֎~)")]
    public bool isGetHammer_Tutorial = false;

    [Header("�`���[�g���A���p���[�v����t���O(�ҏW�֎~)")]
    public bool isGetRope_Tutorial = false;

    [Header("�`���[�g���A���p�~�X�e���[�A�C�e���{������t���O(�ҏW�֎~)")]
    public bool isViewMysteryItem_Tutorial = false;

    private List<int> mysteryItemIds = new(); // �~�X�e���[�A�C�e��ID�̃��X�g
    private List<string> mysteryItemNames = new(); // �~�X�e���[�A�C�e�����̃��X�g
    private List<string> mysteryItemExplanations = new(); // �~�X�e���[�A�C�e���������̃��X�g

    // �`���[�g���A���p�n���}�[ID
    private const int hammer_TutorialID = 9;

    // �`���[�g���A���p���[�vID
    private const int rope_TutorialID = 10;

    // �`���[�g���A���p�h�L�������gID
    private const int documentBook_TutorialID = 7;

    //����������̃h�L�������gID
    private const int defaultDocumentBookID = 99999;

    //�h�L�������gID
    private int keepDocumentBookID;

    [Header("�A�C�e���f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_Item sO_Item;


    [Header("SE�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_SE sO_SE;

    [Header("�T�E���h�֘A")]
    private AudioSource audioSourceSE;
    private readonly int documentNameButtonSEid = 3;//�h�L�������g���̃{�^��SE
    private readonly int buttonSEid = 4;//�{�^��SE��ID
    

    [Header("Input Actions")]
    public GameInput gameInput;


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

        gameInput = new GameInput();

        // �A�N�V�����ɃR�[���o�b�N��o�^
        gameInput.Gameplay.PressPlusButton.performed += OnPlusButtonPressed;

        // Input System��L���ɂ���
        gameInput.Enable(); 
    }

    /// <summary>
    /// Player�̌��ʉ�����Ȃ��o�O��h�~�p�BInput System��L���ɂ���
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        gameInput.Enable();
    }

    /// <summary>
    /// Player�̌��ʉ�����Ȃ��o�O��h�~�p�BInput System�ƃV�[���J�ڂ̃R�[���o�b�N�𖳌��ɂ���
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        gameInput.Disable();
    }

    /// <summary>
    /// Player�̌��ʉ�����Ȃ��o�O��h�~�p�B�V�[���J�ڎ���Player�Q�Ƃ��X�V����
    /// </summary>
    /// <param name="scene">�V�[����</param>
    /// <param name="mode">�V�[�����[�h</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Player.instance != null)
        {
            player = Player.instance;
        }
        else
        {
            Debug.LogWarning("Player instance is null in scene: " + scene.name);
        }


        isTutorialNextMessageFlag = false;

        isGetHammer_Tutorial = false;

        isGetRope_Tutorial = false;

        isViewMysteryItem_Tutorial = false;

        keepDocumentBookID = defaultDocumentBookID;
    }



    private void Start()
    {
        audioSourceSE = MusicController.Instance.GetAudioSource();

        // �p�l����������ԂŔ�\����
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        isOptionPanel = false;
        ChangeOptionPanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();

        isTutorialNextMessageFlag = false;

        isGetHammer_Tutorial = false;

        isGetRope_Tutorial = false;

        isViewMysteryItem_Tutorial = false;

        // �~�X�e���[�A�C�e���̃{�^���ƃe�L�X�g��������
        InitializeMysteryItemUI();

        // �h�L�������gID��������
        keepDocumentBookID = defaultDocumentBookID;
    }


    //P�L�[orZ�L�[�Ń|�[�Y/�|�[�Y����
    public void Update()
    {
        if ((Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Z)) && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) TogglePause();
    }

    //�R���g���[���[��+�{�^���Ń|�[�Y/�|�[�Y����
    private void OnPlusButtonPressed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    /// <summary>
    /// �|�[�Y��ʂ̕\��/��\����؂�ւ���
    /// </summary>
    private void TogglePause()
    {
        // �|�[�Y���J������
        if (!player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanels && !isDocumentExplanationPanel && !isMysteryItemPanels
            && !isMysteryItemExplanationPanel && !goal.isGoalPanel && Time.timeScale != 0)
        {
            ViewPausePanel();
        }
        // �|�[�Y��������
        else if (!player.IsDead && isPause)
        {
            OnClickedClosePauseButton();
        }
    }

    //�|�[�Y
    public void ViewPausePanel() 
    {
        isPause = true;
        Time.timeScale = 0;
        pausePanel.transform.SetAsLastSibling();
        ChangeViewPausePanel();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MusicController.Instance.PauseBGM();

        if (Player.instance != null && Player.instance.audioSourceSE != null)
        {
            MusicController.Instance.PauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);
        }
        else
        {
            Debug.LogWarning("Player or AudioSource is null in ViewPausePanel");
        }

        for (int i = 0; i < baseEnemy.Length; i++) 
        {
            if (baseEnemy[i] != null && baseEnemy[i].audioSourceSE != null)
            {
                MusicController.Instance.PauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }
        }

        
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
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

            MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
            MusicController.Instance.UnPauseSE(Player.instance.audioSourceSE, Player.instance.currentSE);

            for (int i = 0; i < baseEnemy.Length; i++)
            {
                if (baseEnemy[i] != null) MusicController.Instance.UnPauseSE(baseEnemy[i].audioSourceSE, baseEnemy[i].currentSE);
            }

            MusicController.Instance.UnPauseBGM();
        }
        
    }


    //�A�C�e���m�F
    public void OnClickedViewItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isPause = false;
        ChangeViewPausePanel();


        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    //�I�v�V�����ݒ�
    public void OnClickedOptionButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isOptionPanel = true;
        ChangeOptionPanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    //�I�v�V�����ݒ肩��|�[�Y��ʂ֖߂�
    public void OnClickedFromOptionToPauseButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        isPause = true;
        ChangeViewPausePanel();

        isOptionPanel = false;
        ChangeOptionPanel();
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        isReturnToTitlePanel = true;
        ChangeReturnToTitlePanel();

        isPause = false;
        ChangeViewPausePanel();
    }

    //�u�͂��v����
    public void OnClickedYesButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        MusicController.Instance.StopBGM();
        //GameController.instance.ReturnToTitle();
        SceneManager.LoadScene("TitleScene");
    }

    //�u�������v����
    public void OnClickedNoButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));
        isPause = true;
        ChangeViewPausePanel();

        isReturnToTitlePanel = false;
        ChangeReturnToTitlePanel();
    }



    public void OnClickedViewDocumentButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        // �h�L�������g�p�l����\��
        isDocumentPanels = true;
        ChangeViewDocumentPanel();

        // �~�X�e���[�A�C�e���p�l�����\��
        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    public void OnClickedViewMysteryItemButton()
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

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
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

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

    /// <summary>
    /// �h�L�������g���̃{�^��������
    /// </summary>
    public void OnClickedDocumentNameButton() 
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(documentNameButtonSEid));

        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();

        if (keepDocumentBookID == documentBook_TutorialID) 
        {
            isTutorialNextMessageFlag = true;
        }
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

    //�I�v�V�����p�l���̕\��/��\��
    void ChangeOptionPanel()
    {
        if (isOptionPanel)
        {
            optionPanel.SetActive(true);
        }
        else
        {
            optionPanel.SetActive(false);
        }
    }

    //�^�C�g���֖߂�p�l���̕\��/��\��
    void ChangeReturnToTitlePanel()
    {
        if (isReturnToTitlePanel)
        {
            returnToTitlePanel.SetActive(true);
        }
        else
        {
            returnToTitlePanel.SetActive(false);
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


    /// <summary>
    /// DocumentNameText�̋L�ړ��e��ύX
    /// </summary>
    /// <param name="documentId">�擾����id</param>
    /// <param name="documentName">�ύX��̋L�ړ��e</param>
    public void ChangeDocumentNameText(int documentId, string documentName) 
    {
        // �`���[�g���A���p�h�L�������g�̏ꍇ
        if (documentId == documentBook_TutorialID) 
        {
            
            isTutorialNextMessageFlag = true;
            keepDocumentBookID = documentId;
        }

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

    /// <summary>
    /// �~�X�e���[�A�C�e�����̃{�^��������
    /// </summary>
    /// <param name="index"></param>
    public void OnClickedMysteryItemNameButton(int index)
    {
        MusicController.Instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        if (isGetHammer_Tutorial && isGetRope_Tutorial)
        {
            isViewMysteryItem_Tutorial = true;
        }

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
    public void ChangeMysteryItemTexts(int mysteryItemID, string mysteryItemName, string mysteryItemDescription)
    {
        mysteryItemIds.Add(mysteryItemID);

        for (int i = 0; i < mysteryItemIds.Count; i++)
        {
            if (mysteryItemIds[i] == hammer_TutorialID) 
            {
                isGetHammer_Tutorial = true;
                Debug.Log("isGetHammer_Tutorial" + isGetHammer_Tutorial);
            }

            if (mysteryItemIds[i] == rope_TutorialID)
            {
                isGetRope_Tutorial = true;
                Debug.Log("rope_TutorialID" + rope_TutorialID);
            }
        }

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
