using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Player player;//�v���C���[
    //[SerializeField] private Goal goal;//�S�[��
    [SerializeField] private GameObject pausePanel;//�|�[�Y�p�l��

    [SerializeField] public bool isPause = false;

    //Audio�n
    //public BGM BGMScript;//���C���Q�[��BGM
    //public AudioClip pauseSE;//�N���b�NSE

    private void Start()
    {
        pausePanel.SetActive(false);
        isPause = false;
    }


    //P�L�[�Ń|�[�Y/�|�[�Y����
    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P)&& player.IsDead == false && goal.isGoal == false)
        if (Input.GetKeyDown(KeyCode.P)
            && player.IsDead == false)
        {
            //GameController.instance.PlayAudioSE(pauseSE);
            if (Time.timeScale == 1)
            {
                ViewPausePanel();
            }
            else
            {
                OnClickedClosePauseButton();
            }
        }
    }

    void ViewPausePanel() 
    {
        //BGMScript.PauseBGM();
        Time.timeScale = 0;
        isPause = true;
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
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToStageSelect();
        Debug.Log("�A�C�e���m�F");
    }

    //�^�C�g���֖߂�
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("�^�C�g���֖߂�");
    }
}
