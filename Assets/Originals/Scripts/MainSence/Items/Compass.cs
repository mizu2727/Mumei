using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Compass instance;

    [Header("アイテムメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ItemMessage itemMessage;

    /// <summary>
    /// コンパスのアイテムID
    /// </summary>
    private const int kItemId = 12;

    [Header("コンパスの針の画像(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Image compassArrowImage;

    [Header("CompassTextPanel(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject compassTextPanel;

    [Header("CompassNameText(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Text CompassNameText;
    
    [Header("CompassExplanationText(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Text CompassExplanationText;

    /// <summary>
    /// コンパス関係を表示・非表示するフラグ
    /// </summary>
    private bool isViewCompassArrowImage = false;

    /// <summary>
    /// プレイヤーの位置
    /// </summary>
    private Transform player;

    [Header("ゴール(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Transform goal;

    /// <summary>
    /// RectTransform
    /// </summary>
    RectTransform rectTransform_;

    [Header("針のオフセット角度（度）(デフォルトでは北が0度)")]
    [SerializeField] private float needleOffsetAngle = 0f;

    /// <summary>
    /// コンパスの針の画像を表示・非表示にする
    /// </summary>
    /// <param name="isVisible">表示ならtrue</param>
    public void ViewOrHiddenCompassArrowImage(bool isVisible)
    {
        //表示・非表示フラグを更新
        isViewCompassArrowImage = isVisible;

        //コンパスの針の画像の表示・非表示を切り替え
        compassArrowImage.enabled = isVisible;
    }

    /// <summary>
    /// オブジェクト破棄時の処理
    /// </summary>
    private void OnDestroy() 
    {
        //compassArrowImageが存在する場合
        if (compassArrowImage != null)
        {
            //compassArrowImageをnullに設定
            compassArrowImage = null;
        }

        //CompassNameTextが存在する場合
        if (CompassNameText != null)
        {
            //CompassNameTextをnullに設定
            CompassNameText = null;
        }

        //CompassExplanationTextが存在する場合
        if (CompassExplanationText != null)
        {
            //CompassExplanationTextをnullに設定
            CompassExplanationText = null;
        }

        //compassTextPanelが存在する場合
        if (compassTextPanel != null)
        {
            //compassTextPanelをnullに設定
            compassTextPanel = null;
        }

        //goalが存在する場合
        if (goal != null)
        {
            //goalをnullに設定
            goal = null;
        }

        //インスタンスが存在する場合
        if (instance == this)
        {
            //インスタンスをnullにする(メモリリークを防ぐため)
            instance = null;
        }
    }

    private void Awake()
    {
        //コンパスの針の画像を非表示
        ViewOrHiddenCompassArrowImage(false);

        //コンパステキストパネルを非表示
        compassTextPanel.SetActive(false);

        //シングルトンの設定
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //すでにインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //プレイヤーのTransformを取得
        player = Player.instance.transform;

        //RectTransformを取得
        rectTransform_ = compassArrowImage.rectTransform;
    }

    void Update()
    {
        //プレイヤーまたはゴールが設定されていない場合
        if (player == null || goal == null)
        {
            //処理をスキップ
            return;
        }

        //プレイヤー→ゴール方向
        Vector3 dir = goal.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        //ゴール方向の絶対角度
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        //プレイヤーの向き
        float playerAngle = player.eulerAngles.y;

        //相対角度（コンパス針が向くべき角度）
        float relativeAngle = targetAngle - playerAngle;

        //UI針を回転
        rectTransform_.rotation = Quaternion.Euler(0f, 0f, -relativeAngle + needleOffsetAngle);


        //OperationExplanationControllerのインスタンスが存在しない場合
        if (OperationExplanationController.instance == null) 
        {
            //処理をスキップ
            return;
        }

        //コンパス表示フラグがオン&&CompassTextPanel手動閲覧フラグがオンの場合
        if (isViewCompassArrowImage && OperationExplanationController.instance.GetIsSelfViewCompassTextPanel()) 
        {
            //コンパステキストパネルを表示
            compassTextPanel.SetActive(true);
        }
        else
        {
            //コンパステキストパネルを非表示
            compassTextPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    public void SettingLanguageText()
    {
        //言語ステータスに応じて、テキストを変更する
        switch (LanguageController.instance.GetLanguageStatus())
        {
            case LanguageController.LanguageStatus.kJapanese:

                //日本語アイテム名を設定する
                CompassNameText.text = itemMessage.itemMessage[kItemId].itemNameJapanese;

                //アイテム名のフォントサイズを日本語用に設定する
                CompassNameText.fontSize = itemMessage.itemMessage[kItemId].itemNameSizeJapanese;

                //日本語アイテム説明を設定する
                CompassExplanationText.text = itemMessage.itemMessage[kItemId].itemDescriptionJapanese;

                //アイテム説明のフォントサイズを日本語用に設定する
                CompassExplanationText.fontSize = itemMessage.itemMessage[kItemId].itemDescriptionSizeJapanese;
                break;

            case LanguageController.LanguageStatus.kEnglish:

                //英語アイテムを設定する
                CompassNameText.text = itemMessage.itemMessage[kItemId].itemNameEnglish;

                //アイテム名のフォントサイズを英語用に設定する
                CompassNameText.fontSize = itemMessage.itemMessage[kItemId].itemNameSizeEnglish;

                //英語アイテム説明名を設定する
                CompassExplanationText.text = itemMessage.itemMessage[kItemId].itemDescriptionEnglish;

                //アイテム説明のフォントサイズを英語用に設定する
                CompassExplanationText.fontSize = itemMessage.itemMessage[kItemId].itemDescriptionSizeEnglish;
                break;
        }
    }
}
