using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [Header("Prefab����GameController�̎q�I�u�W�F�N�g���A�^�b�`���邱��")]
    [SerializeField] private SaveLoad saveLoad;

    [Header("�Q�[�����[�h�̃X�e�[�^�X")]
    public GameModeStatus gameModeStatus;

    [Header("�`���[�g���A���p�h�L�������g")]
    [SerializeField] public GameObject tutorialDocument;

    [Header("�`���[�g���A���p�~�X�e���[�A�C�e���֘A")]
    [SerializeField] public GameObject tutorialMysteryItem01;
    [SerializeField] public GameObject tutorialMysteryItem02;

    [Header("�`���[�g���A���p�A�C�e���e�I�u�W�F�N�g")]
    [SerializeField] public GameObject tutorialItems;

    [Header("Player�X�^�~�iSlider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider staminaSlider;

    [Header("PlayerCamera�}�E�X/�Q�[���p�b�h�̉E�X�e�B�b�N�̐��񑬓x��Slider(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("Player�̎g�p�A�C�e���C���x���g���p�l���֘A(�q�G�����L�[�ォ��A�^�b�`���邱��)")]
    [SerializeField] public GameObject useItemPanel;//�g�p�A�C�e���m�F�p�l��
    [SerializeField] public Text useItemCountText;//�g�p�A�C�e�������J�E���g�e�L�X�g
    [SerializeField] public Image useItemImage;//�g�p�A�C�e���摜
    [SerializeField] public GameObject useItemTextPanel;//�g�p�A�C�e���e�L�X�g�m�F�p�l��
    [SerializeField] public Text useItemNameText;//�g�p�A�C�e�����e�L�X�g
    [SerializeField] public Text useItemExplanationText;//�g�p�A�C�e�������e�L�X�g

    //[Header("�`���[�g���A���p�S�[��")]
    //[SerializeField] public GameObject tutorialGoal;

    [Header("�Z�[�u����v���C���[��")]
    public static string playerName;

    [Header("�Z�[�u����v���C��")]
    public static int playCount = 0;

    


    public enum  GameModeStatus
    {
        Story,
        PlayInGame,
        StopInGame,
        GoalGameMode,
        GameOver,
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

        Time.timeScale = 1;
    }

    private void Update()
    {

    }

    //�Q�[�����[�h�̃X�e�[�^�X��ݒ�
    public void SetGameModeStatus(GameModeStatus status) 
    {
        gameModeStatus = status;

        if (gameModeStatus == GameModeStatus.Story) 
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// �f�[�^��ۑ����郁�\�b�h���Ăяo��(��)
    /// </summary>
    public void CallSaveUserDataMethod() 
    {
        saveLoad.SaveUserData();
    }

    /// <summary>
    /// �f�[�^�����[�h���郁�\�b�h���Ăяo��(��)
    /// </summary>
    public void CallLoadUserDataMethod() 
    {
        saveLoad.LoadUserData();
    }

    /// <summary>
    /// �f�[�^�����������郁�\�b�h���Ăяo��(��)
    /// </summary>
    public void CallRestDataMethod() 
    {
        saveLoad.ResetUserData();
    }

    /// <summary>
    /// �Q�[���I�[�o�[��ʂ֑J�ڂ���
    /// </summary>
    public void ViewGameOver() 
    {
        if (Player.instance.IsDead) 
        {
            SceneManager.LoadScene("GameOverScene");
        }
    }

}
