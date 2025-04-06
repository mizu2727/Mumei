using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Player player;//プレイヤー
    //[SerializeField] private Goal goal;//ゴール
    [SerializeField] private GameObject pausePanel;//ポーズパネル

    [SerializeField] public bool isPause = false;

    //Audio系
    //public BGM BGMScript;//メインゲームBGM
    //public AudioClip pauseSE;//クリックSE

    private void Start()
    {
        pausePanel.SetActive(false);
        isPause = false;
    }


    //Pキーでポーズ/ポーズ解除
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

        //ポーズ時に他のUIに触れないようにするために必要
        pausePanel.transform.SetAsLastSibling();
        pausePanel.SetActive(true);
        Debug.Log(isPause);
    }


    //ポーズ解除
    public void OnClickedClosePauseButton()
    {
        pausePanel.SetActive(false);
        isPause = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("ポーズ解除");
        //GameController.instance.PlayAudioSE(pauseSE);
        //BGMScript.UnPauseBGM();
        Debug.Log(isPause);
        Time.timeScale = 1;
    }


    //アイテム確認
    public void OnClickedViewItemButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToStageSelect();
        Debug.Log("アイテム確認");
    }

    //タイトルへ戻る
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("タイトルへ戻る");
    }
}
