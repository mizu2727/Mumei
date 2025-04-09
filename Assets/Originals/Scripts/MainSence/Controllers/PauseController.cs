using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseController : MonoBehaviour
{
    public static PauseController instance;


    [SerializeField] private Player player;//プレイヤー
    //[SerializeField] private Goal goal;//ゴール
    [SerializeField] private GameObject pausePanel;//ポーズパネル
    [SerializeField] private GameObject viewItemsPanel;//アイテム確認パネル


    [SerializeField] public bool isPause = false;

    //Audio系
    //public BGM BGMScript;//メインゲームBGM
    //public AudioClip pauseSE;//クリックSE


    private void Awake()
    {
        // シングルトンの設定
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移時に破棄されないようにする（必要に応じて）
        }
        else
        {
            Destroy(gameObject); // すでにインスタンスが存在する場合は破棄
        }
    }


    private void Start()
    {
        // パネルを初期状態で非表示に
        pausePanel.SetActive(false);

        isPause = false;
    }


    //Pキーでポーズ/ポーズ解除
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

    //ポーズ解除
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


    //アイテム確認
    public void OnClickedViewItemButton()
    {
        viewItemsPanel.transform.SetAsLastSibling();
        viewItemsPanel.SetActive(true);
        pausePanel.SetActive(false);
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
    }



    //DocumentNameTextの記載内容を変更
    public void ChangeDocumentNameText() 
    {
        
    }
}
