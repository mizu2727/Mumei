using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Player player;//�v���C���[
    //[SerializeField] private Goal goal;//�S�[��
    [SerializeField] private GameObject pausePanel;//�|�[�Y�p�l��
    [SerializeField] private GameObject viewItemsPanel;//�A�C�e���m�F�p�l��


    [SerializeField] public bool isPause = false;

    //Audio�n
    //public BGM BGMScript;//���C���Q�[��BGM
    //public AudioClip pauseSE;//�N���b�NSE

    private void Start()
    {
        isPause = false;
    }


    //P�L�[�Ń|�[�Y/�|�[�Y����
    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P)&& player.IsDead == false && goal.isGoal == false)
        if (Input.GetKeyDown(KeyCode.P)
            && !player.IsDead && !isPause)
        {
            //GameController.instance.PlayAudioSE(pauseSE);
            if (Time.timeScale == 1)
            {
                isPause = true;
                ViewPausePanel();
            }
            else
            {
                OnClickedClosePauseButton();
            }
        }
    }

    public void ViewPausePanel() 
    {
        //BGMScript.PauseBGM();
        Time.timeScale = 0;
        //isPause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        //�|�[�Y���ɑ���UI�ɐG��Ȃ��悤�ɂ��邽�߂ɕK�v
        pausePanel.transform.SetAsLastSibling();
        pausePanel.SetActive(true);
        Debug.Log(isPause);
    }


    //�|�[�Y����
    public void OnClickedClosePauseButton()
    {
        pausePanel.SetActive(false);
        isPause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("�|�[�Y����");
        //GameController.instance.PlayAudioSE(pauseSE);
        //BGMScript.UnPauseBGM();
        Debug.Log(isPause);
        Time.timeScale = 1;
    }


    //�A�C�e���m�F
    public void OnClickedViewItemButton()
    {
        Debug.Log("�A�C�e���m�F1");
        Debug.Log($"viewItemsPanel is null: {viewItemsPanel == null}");
        Debug.Log($"pausePanel is null: {pausePanel == null}");
        viewItemsPanel.transform.SetAsLastSibling();
        viewItemsPanel.SetActive(true);
        pausePanel.SetActive(false);
        Debug.Log("�A�C�e���m�F2");
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
        Debug.Log("�|�[�Y��ʂ֖߂�");
    }
}
