using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private Player player;//プレイヤー
    //[SerializeField] private Goal goal;//ゴール
    [SerializeField] private GameObject pausePanel;//ポーズパネル
    [SerializeField] private GameObject viewItemsPanel;//アイテム確認パネル


    [SerializeField] public bool isPause = false;

    //Audio系
    //public BGM BGMScript;//メインゲームBGM
    //public AudioClip pauseSE;//クリックSE

    private void Start()
    {
        isPause = false;
    }


    //Pキーでポーズ/ポーズ解除
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
        Debug.Log("アイテム確認1");
        Debug.Log($"viewItemsPanel is null: {viewItemsPanel == null}");
        Debug.Log($"pausePanel is null: {pausePanel == null}");
        viewItemsPanel.transform.SetAsLastSibling();
        viewItemsPanel.SetActive(true);
        pausePanel.SetActive(false);
        Debug.Log("アイテム確認2");
    }

    //タイトルへ戻る
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("タイトルへ戻る");
    }


    //ポーズ画面へ戻る
    public void OnClickedReturnToPausePanel()
    {
        pausePanel.transform.SetAsLastSibling();
        pausePanel.SetActive(true);
        viewItemsPanel.SetActive(false);

        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        Debug.Log("ポーズ画面へ戻る");
    }
}
