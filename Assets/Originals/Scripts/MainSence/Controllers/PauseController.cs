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


    [SerializeField] public bool isPause = false;

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
        pausePanel.SetActive(false);

        isPause = false;
    }


    //P�L�[�Ń|�[�Y/�|�[�Y����
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !player.IsDead && !isPause)
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
        pausePanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        isPause = true;
    }

    //�|�[�Y����
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            isPause = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }


    //�A�C�e���m�F
    public void OnClickedViewItemButton()
    {
        viewItemsPanel.transform.SetAsLastSibling();
        viewItemsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("�^�C�g���֖߂�");
    }






    //�|�[�Y��ʂ֖߂�
    public void OnClickedReturnToPausePanel()
    {
        pausePanel.transform.SetAsLastSibling();
        pausePanel.SetActive(true);
        viewItemsPanel.SetActive(false);

        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
    }



    //DocumentNameText�̋L�ړ��e��ύX
    public void ChangeDocumentNameText() 
    {
        
    }
}
