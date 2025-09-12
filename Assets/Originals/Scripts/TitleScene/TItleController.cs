using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;
using static GameController;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    [Header("�^�C�g����ʂ�Canvas")]
    [SerializeField] private Canvas titlesCanvas;

    [Header("���[�h������Scene��")]
    [SerializeField] private string SceneName;

    [Header("BGM�f�[�^(���ʂ�ScriptableObject���A�^�b�`����K�v������)")]
    [SerializeField] public SO_BGM sO_BGM;

    [Header("�T�E���h�֘A")]
    public AudioSource audioSourceBGM;
    private readonly int titleBGMid = 0; // �^�C�g��BGM��ID


    private void Awake()
    {
        Time.timeScale = 1;
        titlesCanvas.enabled = true;
        Cursor.visible = true;

        //�}�E�X�J�[�\�����E�B���h�E�̊O�ɏo��
        Cursor.lockState = CursorLockMode.None;

        GameController.instance.SetGameModeStatus(GameModeStatus.StopInGame);

        //�S�Ă�BGM�̏�Ԃ�Stop�ɕύX
        sO_BGM.StopAllBGM();

        //audioSourceBGM��ݒ�TODO
        //audioSourceBGM = MusicController.Instance.GetAudioSource();

        //�^�C�g��BGM���Đ�TODO
        //MusicController.Instance.PlayBGM(titleBGMid);
    }

    public void OnStartButtonClicked()
    {
        //GameController.instance.playCount++;
        GameController.playCount++;
        SceneManager.LoadScene(SceneName);        
    }

    //�Q�[���I��
    public void EndGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}