using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleController : MonoBehaviour
{
    //Canvas�n
    [SerializeField] private Canvas titlesCanvas;//�^�C�g����ʂ�Canvas
    //[SerializeField] private Canvas optionsCanvas;//�I�v�V������ʂ�Canvas

    //�V�[������
    [SerializeField] private string SceneName;

    //Audio�n
    //private AudioSource audioSource;//AudioSource
    //[SerializeField] private BGM BGMScript;//�^�C�g��BGM
    //[SerializeField] private AudioClip clickButtonSE;//�N���b�NSE

    //�J�n���ɃI�v�V������ʂ�Canvas���\��
    private void Awake()
    {
        //optionsCanvas.enabled = false;

        //�|�[�Y��ʂ���^�C�g���֖߂����ꍇ�ɁA�ꎞ��~���������邽�߂ɕK�v
        Time.timeScale = 1;
    }

    //�J�n���Ƀ^�C�g����ʂ�Canvas�̕\��
    private void Start()
    {
        titlesCanvas.enabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        ///audioSource = GetComponent<AudioSource>();
    }

    //�Q�[���X�^�[�g
    public void OnStartButtonClicked()
    {
        //BGMScript.StopBGM();
        //PlayAudioSE(clickButtonSE);

        SceneManager.LoadScene(SceneName);
    }

    //�I�v�V�������J��
    //public void OnOptionButtonClicked()
    //{
    //    PlayAudioSE(clickButtonSE);
    //    titlesCanvas.enabled = false;
    //    optionsCanvas.enabled = true;
    //}

    //���ʉ��̃e�X�g
    //public void ONSETestButton()
    //{
    //    PlayAudioSE(clickButtonSE);
    //}

    //�I�v�V���������
    //public void OnReturnButtonClicked()
    //{
    //    //PlayAudioSE(clickButtonSE);
    //    titlesCanvas.enabled = true;
    //    optionsCanvas.enabled = false;
    //}

    //�Q�[���I��
    public void EndGame()
    {
        //BGMScript.StopBGM();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // �G�f�B�^�̍Đ����~
        #else
            Application.Quit(); // �r���h�ς݃A�v���ŏI��
        #endif
    }

    //SE��炷���\�b�h
    //public void PlayAudioSE(AudioClip audioClip)
    //{
    //    if (audioSource != null)
    //    {
    //        audioSource.PlayOneShot(audioClip);
    //    }
    //    else
    //    {
    //        Debug.Log("No Setting AudioSource!");
    //    }
    //}
}