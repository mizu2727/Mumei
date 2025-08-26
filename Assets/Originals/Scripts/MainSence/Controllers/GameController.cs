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

    //[Header("�`���[�g���A���p�S�[��")]
    //[SerializeField] public GameObject tutorialGoal;

    [Header("�Z�[�u����v���C���[��")]
    public static string playerName;

    [Header("�v���C��")]
    public static int playCount = 0;


    public enum  GameModeStatus
    {
        Story,
        PlayInGame,
        StopInGame,
        GoalGamenMode
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
}
