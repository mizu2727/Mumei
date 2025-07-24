using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

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


    public enum  GameModeStatus
    {
        Story,
        PlayInGame,
        StopInGame,
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
}
