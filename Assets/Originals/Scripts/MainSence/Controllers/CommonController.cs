using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 共通の変数や関数を管理するクラス
/// </summary>
public class CommonController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static CommonController instance;


    /// <summary>
    /// TitleSceneのシーン名
    /// </summary>
    private const string stringTitleScene = "TitleScene";

    /// <summary>
    /// TitleSceneのシーン名を取得する関数
    /// </summary>
    /// <returns>TitleSceneのシーン名</returns>
    public string GetTitleSceneName()
    {
        return stringTitleScene;
    }

    /// <summary>
    /// OpeningSceneのシーン名
    /// </summary>
    private const string stringOpeningScene = "OpeningScene";

    /// <summary>
    /// OpeningSceneのシーン名を取得する関数
    /// </summary>
    /// <returns>OpeningSceneのシーン名</returns>
    public string GetOpeningSceneName()
    {
        return stringOpeningScene;
    }

    /// <summary>
    /// DemoStage01
    /// </summary>
    private const string stringDemoStage01 = "DemoStage01";

    /// <summary>
    /// DemoStage01を取得する関数
    /// </summary>
    /// <returns>DemoStage01</returns>
    public string GetDemoStage01SceneName()
    {
        return stringDemoStage01;
    }

    /// <summary>
    /// HomeScene
    /// </summary>
    private const string stringHomeScene = "HomeScene";

    /// <summary>
    /// HomeSceneのシーン名を取得する関数
    /// </summary>
    /// <returns>HomeSceneのシーン名</returns>
    public string GetHomeSceneName()
    {
        return stringHomeScene;
    }

    /// <summary>
    /// GameClearScene
    /// </summary>
    private const string stringGameClearScene = "GameClearScene";

    /// <summary>
    /// GameClearSceneのシーン名を取得する関数
    /// </summary>
    /// <returns>GameClearSceneのシーン名</returns>
    public string GetGameClearSceneName()
    {
        return stringGameClearScene;
    }

    /// <summary>
    /// Stage01
    /// </summary>
    private const string stringStage01 = "Stage01";

    /// <summary>
    /// stringStage01を取得する関数
    /// </summary>
    /// <returns>stringStage01</returns>
    public string GetStringStage01()
    {
        return stringStage01;
    }

    /// <summary>
    /// Stage02
    /// </summary>
    private const string stringStage02 = "Stage02";

    /// <summary>
    /// stringStage02を取得する関数
    /// </summary>
    /// <returns>stringStage02</returns>
    public string GetStringStage02()
    {
        return stringStage02;
    }

    /// <summary>
    /// Stage03
    /// </summary>
    private const string stringStage03 = "Stage03";

    /// <summary>
    /// stringStage03を取得する関数
    /// </summary>
    /// <returns>stringStage03</returns>
    public string GetStringStage03()
    {
        return stringStage03;
    }

    /// <summary>
    /// Stage04
    /// </summary>
    private const string stringStage04 = "Stage04";

    /// <summary>
    /// stringStage04を取得する関数
    /// </summary>
    /// <returns>stringStage04</returns>
    public string GetStringStage04()
    {
        return stringStage04;
    }


    /// <summary>
    /// "Interact"
    /// </summary>
    private const string stringInteract = "Interact";

    /// <summary>
    /// Interactを取得する関数
    /// </summary>
    /// <returns>Interact</returns>
    public string GetStringInteract()
    {
        return stringInteract;
    }


    /// <summary>
    /// タグ："Untagged"
    /// </summary>
    private const string stringUntaggedTag = "Untagged";

    /// <summary>
    /// Untaggedタグを取得する関数
    /// </summary>
    /// <returns>Untaggedタグ</returns>
    public string GetStringUntaggedTag()
    {
        return stringUntaggedTag;
    }

    /// <summary>
    /// "Player"タグ
    /// </summary>
    private const string playerTag = "Player";

    /// <summary>
    /// "Player"タグを取得する関数
    /// </summary>
    /// <returns>"Player"タグ</returns>
    public string GetPlayerTag()
    {
        return playerTag;
    }

    /// <summary>
    /// "Wall"タグ
    /// </summary>
    private const string wallTag = "Wall";

    /// <summary>
    /// "Wall"タグを取得する関数
    /// </summary>
    /// <returns>"Wall"タグ</returns>
    public string GetWallTag()
    {
        return wallTag;
    }

    /// <summary>
    /// "Door"タグ
    /// </summary>
    private const string doorTag = "Door";

    /// <summary>
    /// "Door"タグを取得する関数
    /// </summary>
    /// <returns>"Door"タグ</returns>
    public string GetDoorTag()
    {
        return doorTag;
    }

    /// <summary>
    /// その他オブジェクトタグ
    /// </summary>
    private const string otherStageObjectTag = "OtherStageObject";

    /// <summary>
    /// その他オブジェクトタグを取得する関数
    /// </summary>
    /// <returns>その他オブジェクトタグ</returns>
    public string GetOtherStageObjectTag()
    {
        return otherStageObjectTag;
    }

    /// <summary>
    /// アイテムタグ
    /// </summary>
    private const string itemTag = "Item";

    /// <summary>
    /// アイテムタグを取得する関数
    /// </summary>
    /// <returns>アイテムタグ</returns>
    public string GetItemTag()
    {
        return itemTag;
    }

    /// <summary>
    /// ゴールタグ
    /// </summary>
    private const string goalTag = "Goal";

    /// <summary>
    /// ゴールタグを取得する関数
    /// </summary>
    /// <returns>ゴールタグ</returns>
    public string GetGoalTag()
    {
        return goalTag;
    }

    /// <summary>
    /// ステージライトタグ
    /// </summary>
    private const string stageLightTag = "StageLight";

    /// <summary>
    /// ステージライトタグを取得する関数
    /// </summary>
    /// <returns>ステージライトタグ</returns>
    public string GetStageLightTag()
    {
        return stageLightTag;
    }

    /// <summary>
    /// 引き出しタグ
    /// </summary>
    private const string drawerTag = "Drawer";

    /// <summary>
    /// 引き出しタグを取得する関数
    /// </summary>
    /// <returns>引き出しタグ</returns>
    public string GetDrawerTag()
    {
        return drawerTag;
    }

    /// <summary>
    /// 隠れる用オブジェクトタグ
    /// </summary>
    private const string hiddenObjectTag = "HiddenObject";

    /// <summary>
    /// 隠れる用オブジェクトタグを取得する関数
    /// </summary>
    /// <returns>隠れる用オブジェクトタグ</returns>
    public string GetHiddenObjectTag()
    {
        return hiddenObjectTag;
    }

    /// <summary>
    /// アウトラインタグ
    /// </summary>
    private const string outlineTag = "Outline";

    /// <summary>
    /// アウトラインタグを取得する関数
    /// </summary>
    /// <returns>アウトラインタグ</returns>
    public string GetOutlineTag()
    {
        return outlineTag;
    }


    /// <summary>
    /// デフォルトレイヤー
    /// </summary>
    private const string defaultLayer = "Default";

    /// <summary>
    /// デフォルトレイヤーを取得する関数
    /// </summary>
    /// <returns>デフォルトレイヤー</returns>
    public string GetDefaultLayer()
    {
        return defaultLayer;
    }

    /// <summary>
    ///"Wall"レイヤー
    /// </summary>
    private const string WallLayer = "Wall";

    /// <summary>
    /// "Wall"レイヤーを取得する関数
    /// </summary>
    /// <returns>"Wall"レイヤー</returns>
    public string GetWallLayer()
    {
        return WallLayer;
    }

    /// <summary>
    /// アイテムレイヤー
    /// </summary>
    private const string itemLayer = "Item";

    /// <summary>
    /// アイテムレイヤーを取得する関数
    /// </summary>
    /// <returns>アイテムレイヤー</returns>
    public string GetItemLayer()
    {
        return itemLayer;
    }

    /// <summary>
    /// ドアレイヤー
    /// </summary>
    private const string doorLayer = "Door";

    /// <summary>
    /// ドアレイヤーを取得する関数
    /// </summary>
    /// <returns>ドアレイヤー</returns>
    public string GetDoorLayer()
    {
        return doorLayer;
    }

    /// <summary>
    /// ゴールレイヤー
    /// </summary>
    private const string goalLayer = "Goal";

    /// <summary>
    /// ゴールレイヤーを取得する関数
    /// </summary>
    /// <returns>ゴールレイヤー</returns>
    public string GetGoalLayer()
    {
        return goalLayer;
    }

    /// <summary>
    /// ステージライトレイヤー
    /// </summary>
    private const string stageLightLayer = "StageLight";

    /// <summary>
    /// ステージライトレイヤーを取得する関数
    /// </summary>
    /// <returns>ステージライトレイヤー</returns>
    public string GetStageLightLayer()
    {
        return stageLightLayer;
    }

    /// <summary>
    /// 引き出しレイヤー
    /// </summary>
    private const string drawerLayer = "Drawer";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetDrawerLayer()
    {
        return drawerLayer;
    }

    /// <summary>
    /// 隠れる用オブジェクトレイヤー
    /// </summary>
    private const string hiddenObjectLayer = "HiddenObject";

    /// <summary>
    /// 隠れる用オブジェクトレイヤー
    /// </summary>
    /// <returns>隠れる用オブジェクトレイヤー</returns>
    public string GetHiddenObjectLayer()
    {
        return hiddenObjectLayer;
    }


    [Header("日本語用フォント(TMP_FontAssetをアタッチ)")]
    [SerializeField] private TMP_FontAsset japaneseFont;

    /// <summary>
    /// 日本語用フォントを取得する関数
    /// </summary>
    /// <returns>日本語用フォント</returns>
    public TMP_FontAsset GetJapaneseFont()
    {
        return japaneseFont;
    }

    [Header("英語用フォント(TMP_FontAssetをアタッチ)")]
    [SerializeField] private TMP_FontAsset englishFont;

    /// <summary>
    /// 英語用フォントを取得する関数
    /// </summary>
    /// <returns>英語用フォント</returns>
    public TMP_FontAsset GetEnglishFont()
    {
        return englishFont;
    }

    /// <summary>
    /// ボタンの文字の色
    /// </summary>
    private Color kButtonTextColor = new Color(200f / 255f, 200f / 255f, 200f / 255f, 1f);

    /// <summary>
    /// マウスカーソルにボタンが重なった時のボタンの文字の色
    /// </summary>
    //private Color kButtonTextColorMouseOver = Color.yellow;
    private Color kButtonTextColorMouseOver = new Color(239f / 255f, 227f / 255f, 26f / 255f, 1f);

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    /// <summary>
    /// ボタンの文字の色を変更する関数
    /// </summary>
    /// <param name="targetButtonTextNumber">対象のボタン番号</param>
    public void ChangeButtonTextColor(int targetButtonTextNumber)
    {
        for (int i = 0; i < LanguageController.instance.GetButtonTextNumberArray().Length; i++)
        {
            //指定の番号が存在する場合
            if (LanguageController.instance.GetButtonTextNumberArray()[i] == targetButtonTextNumber)
            {
                //その番号と一致するボタンの文字の色を変更
                LanguageController.instance.GetButtonTMPTextArray()[i].color = kButtonTextColorMouseOver;
            }
        }
    }

    /// <summary>
    /// ボタンの文字の色を元に戻す関数
    /// </summary>
    /// <param name="targetButtonTextNumber">対象のボタン番号</param>
    public void ReturnButtonTextColor(int targetButtonTextNumber)
    {
        for (int i = 0; i < LanguageController.instance.GetButtonTextNumberArray().Length; i++)
        {
            //指定の番号が存在する場合
            if (LanguageController.instance.GetButtonTextNumberArray()[i] == targetButtonTextNumber)
            {
                //その番号と一致するボタンの文字の色を元に戻す
                LanguageController.instance.GetButtonTMPTextArray()[i].color = kButtonTextColor;
            }
        }
    }

    /// <summary>
    /// ボタンのホバー・クリックイベントを設定する関数
    /// </summary>
    public void SetupButtonHoverEvents()
    {
        //ボタンの TextMeshProUGUI 配列と番号配列を取得
        var textArray = LanguageController.instance.GetButtonTMPTextArray();
        var numberArray = LanguageController.instance.GetButtonTextNumberArray();

        for (int i = 0; i < textArray.Length; i++)
        {
            //TextMeshProUGUIがnullの場合
            if (textArray[i] == null) 
            {
                //処理をスキップして次のループへ
                continue; 
            }

            //1. TextMeshProUGUIのRaycast Targetは常にオフにしておく（ボタンのクリック判定の邪魔にならないようにしたいため）
            textArray[i].raycastTarget = false;

            //2. 親オブジェクトからButtonコンポーネントを取得。()内にtrueを指定することで、非アクティブな親オブジェクトも検索対象に含める
            Button parentButton = textArray[i].GetComponentInParent<Button>(true);
            //親オブジェクトが存在しない場合
            if (parentButton == null) 
            {
                //処理をスキップして次のループへ
                continue;
            }

            // 3.親オブジェクトにEventTriggerが無ければ追加
            EventTrigger trigger = parentButton.gameObject.GetComponent<EventTrigger>();
            //EventTriggerが存在しない場合
            if (trigger == null)
            {
                //EventTriggerを追加
                trigger = parentButton.gameObject.AddComponent<EventTrigger>();
            }

            //ボタン番号を取得
            int buttonNumber = numberArray[i];

            //4.PointerEnter（マウスホバー時）のイベント作成と登録
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => {
                ChangeButtonTextColor(buttonNumber);
            });
            trigger.triggers.Add(entryEnter);

            //5.PointerExit（マウス外れた時）のイベント作成と登録
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => {
                ReturnButtonTextColor(buttonNumber);
            });
            trigger.triggers.Add(entryExit);

            //6.PointerClick（ボタン押下時）のイベント作成と登録
            EventTrigger.Entry entryClick = new EventTrigger.Entry();
            entryClick.eventID = EventTriggerType.PointerClick;
            entryClick.callback.AddListener((data) => {
                ReturnButtonTextColor(buttonNumber);
            });
            trigger.triggers.Add(entryClick);
        }
    }
}
