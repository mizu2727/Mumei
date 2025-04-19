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
    [SerializeField] private Goal goal;//ゴール
    [SerializeField] private GameObject pausePanel;//ポーズパネル

    [SerializeField] private GameObject viewItemsPanel;//アイテム確認パネル

    [SerializeField] private GameObject documentInventoryPanel;//ドキュメント確認パネル
    [SerializeField] private GameObject documentExplanationPanel;//ドキュメント説明欄パネル
    [SerializeField] private Text documentNameText;//ドキュメント名称テキスト
    [SerializeField] private Text documentExplanationText;//ドキュメント説明欄テキスト

    [SerializeField] private GameObject mysteryItemInventoryPanel;//ミステリーアイテム確認パネル
    [SerializeField] private Button[] mysteryItemNameButton;//ミステリーアイテム名称ボタン
    [SerializeField] private Text[] mysteryItemNameText;//ミステリーアイテム名称テキスト
    [SerializeField] private Image[] mysteryItemImage;//ミステリーアイテム画像
    [SerializeField] private Text[] mysteryItemExplanationText;//ミステリーアイテム説明欄テキスト
    [SerializeField] private GameObject mysteryItemExplanationPanel;//ミステリーアイテム説明欄パネル


    public bool isPause = false;
    public bool isViewItemsPanel = false;
    public bool isDocumentPanels = false;
    public bool isDocumentExplanationPanel = false;
    public bool isMysteryItemPanels = false;
    public bool isMysteryItemExplanationPanel = false;


    private List<string> mysteryItemNames = new(); // ミステリーアイテム名のリスト
    private List<string> mysteryItemExplanations = new(); // ミステリーアイテム説明欄のリスト


    //共通のScriptableObjectをアタッチする必要がある
    [SerializeField] public SO_Item sO_Item;

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
        isPause = false;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();


        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        // ミステリーアイテムのボタンとテキストを初期化
        InitializeMysteryItemUI();
    }


    //Pキーでポーズ/ポーズ解除
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !player.IsDead && !isPause && !isViewItemsPanel
            && !isDocumentPanels && !isDocumentExplanationPanel && !isMysteryItemPanels
            && !isMysteryItemExplanationPanel)
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
        isPause = true;
        ChangeViewPausePanel();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    //ポーズ解除
    public void OnClickedClosePauseButton()
    {
        if (!viewItemsPanel.activeSelf) 
        {
            Time.timeScale = 1;
            isPause = false;
            ChangeViewPausePanel();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
    }


    //アイテム確認
    public void OnClickedViewItemButton()
    {
        isPause = false;
        ChangeViewPausePanel();


        viewItemsPanel.transform.SetAsLastSibling();
        isViewItemsPanel = true;
        ChangeViewItemsPanel();
    }

    //タイトルへ戻る
    public void OnClickedReturnToTitleButton()
    {
        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
        SceneManager.LoadScene("TitleScene");
    }



    public void OnClickedViewDocumentButton() 
    {
        isDocumentPanels = true;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();
    }

    public void OnClickedViewMysteryItemButton()
    {
        isMysteryItemPanels = true;
        ChangeViewMysteryItemPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();
    }


    //ポーズ画面へ戻る
    public void OnClickedReturnToPausePanel()
    {
        pausePanel.transform.SetAsLastSibling();
        isPause = true;
        ChangeViewPausePanel();

        isViewItemsPanel = false;
        ChangeViewItemsPanel();

        isDocumentPanels = false;
        ChangeViewDocumentPanel();

        isMysteryItemPanels = false;
        ChangeViewMysteryItemPanel();

        //BGMScript.StopBGM();
        //GameController.instance.ReturnToTitle();
    }


    public void OnClickedDocumentNameButton() 
    {
        isDocumentExplanationPanel = true;
        ChangeViewDocumentExplanationPanel();
    }


    //ポーズパネルの表示/非表示
    void ChangeViewPausePanel()
    {
        if (isPause)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }


    //アイテム系確認パネルの表示/非表示
    void ChangeViewItemsPanel() 
    {
        if (isViewItemsPanel)
        {
            viewItemsPanel.SetActive(true);
        }
        else
        {
            viewItemsPanel.SetActive(false);
        }
    }


    //ドキュメントアイテムパネルの表示/非表示
    void ChangeViewDocumentPanel() 
    {
        if (isDocumentPanels)
        {
            documentInventoryPanel.transform.SetAsLastSibling();
            documentInventoryPanel.SetActive(true);

        }
        else 
        {
            documentInventoryPanel.SetActive(false);

            isDocumentExplanationPanel = false;

            ChangeViewDocumentExplanationPanel();
        }
    }

    //ドキュメント説明欄パネルの表示/非表示
    void ChangeViewDocumentExplanationPanel()
    {
        if (isDocumentExplanationPanel)
        {
            documentExplanationPanel.SetActive(true);
        }
        else
        {
            documentExplanationPanel.SetActive(false);
        }
    }


    //DocumentNameTextの記載内容を変更
    public void ChangeDocumentNameText(string documentName) 
    {
        documentNameText = documentNameText.GetComponent<Text>();
        documentNameText.text = documentName;
    }

    //DocumentExplanationTextの記載内容を変更
    public void ChangeDocumentExplanationText(string documentDescription)
    {
        documentExplanationText = documentExplanationText.GetComponent<Text>();
        documentExplanationText.text = documentDescription;
        Debug.Log("ドキュメント説明欄："+ documentExplanationText.text);
    }


    //ミステリーアイテム確認パネルの表示/非表示
    void ChangeViewMysteryItemPanel()
    {
        if (isMysteryItemPanels)
        {
            mysteryItemInventoryPanel.transform.SetAsLastSibling();
            mysteryItemInventoryPanel.SetActive(true);
        }
        else
        {
            mysteryItemInventoryPanel.SetActive(false);

            isMysteryItemExplanationPanel = false;
            ChangeViewMysteryItemExplanationPanel();
        }
    }

    
    //ミステリーアイテム説明欄パネルの表示/非表示
    void ChangeViewMysteryItemExplanationPanel()
    {
        if (isMysteryItemExplanationPanel)
        {
            mysteryItemExplanationPanel.SetActive(true);
        }
        else
        {
            mysteryItemExplanationPanel.SetActive(false);
        }
    }


    // ミステリーアイテムのUIを初期化
    private void InitializeMysteryItemUI()
    {
        if (mysteryItemNameButton == null || mysteryItemNameText == null)
        {
            Debug.LogError("mysteryItemNameButton or mysteryItemNameText is not assigned!");
            return;
        }

        // ボタンにクリックイベントを追加
        for (int i = 0; i < mysteryItemNameButton.Length; i++)
        {
            int index = i; // ローカル変数でインデックスをキャプチャ
            mysteryItemNameButton[i].onClick.AddListener(() => OnClickedMysteryItemNameButton(index));
            mysteryItemNameText[i].text = "?????????";
        }
    }



    public void OnClickedMysteryItemNameButton(int index) 
    {
        if (index < mysteryItemNames.Count && index < sO_Item.itemList.Count)
        {
            isMysteryItemExplanationPanel = true;
            ChangeViewMysteryItemExplanationPanel();

            var item = sO_Item.itemList[index];
            if (item != null)
            {
                // 説明テキストを更新
                if (mysteryItemExplanationText != null)
                {
                    mysteryItemExplanationText[0].text = item.description;
                    Debug.Log($"Set explanation text to: {item.description}");
                }

                // 画像を更新
                if (mysteryItemImage != null)
                {
                    mysteryItemImage[0].sprite = item.icon;
                    mysteryItemImage[0].enabled = (item.icon != null); // アイコンがない場合は非表示
                    Debug.Log($"Set image to: {(item.icon != null ? item.icon.name : "null")}");
                }
                else
                {
                    Debug.LogWarning("mysteryItemImage が未設定です");
                }
            }
            else
            {
                Debug.LogError("アイテムが見つかりません");
            }
            Debug.Log($"Clicked Mystery Item: {mysteryItemNames[index]}");
        }
    }


    // ミステリーアイテム名を追加し、UIに反映
    public void ChangeMysteryItemTexts(string mysteryItemName, string mysteryItemDescription)
    {
        if (!mysteryItemNames.Contains(mysteryItemName))
        {
            mysteryItemNames.Add(mysteryItemName);
            mysteryItemExplanations.Add(mysteryItemName);
            UpdateMysteryItemUI();
        }
    }

    // ミステリーアイテムのUIを更新
    private void UpdateMysteryItemUI()
    {
        for (int i = 0; i < mysteryItemNameText.Length; i++)
        {
            //ボタンとテキストのindexが一致するようにする
            if (i < mysteryItemNames.Count)
            {
                mysteryItemNameText[i].text = mysteryItemNames[i];
                mysteryItemNameButton[i].interactable = true;

                mysteryItemExplanationText[i].text = mysteryItemExplanations[i];


                if (i < mysteryItemImage.Length)
                {
                    mysteryItemImage[i].sprite = sO_Item.itemList[i].icon;
                    mysteryItemImage[i].enabled = (sO_Item.itemList[i].icon != null);
                }
            }
            else
            {
                mysteryItemNameText[i].text = "?????????";
                mysteryItemNameButton[i].interactable = false;

                mysteryItemExplanationText[i].text = "";


                if (i < mysteryItemImage.Length)
                {
                    mysteryItemImage[i].sprite = null;
                    mysteryItemImage[i].enabled = false;
                }
            }
        }
    }
}
