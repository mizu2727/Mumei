using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


public class MessageController : MonoBehaviour
{
    public static MessageController instance;

    [Header("メッセージパネル関連(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;

    [Header("メッセージを書くスピード。数値が小さいほど素早く書く")]
    [SerializeField] private float writeSpeed = 0;
    private bool isWrite = false;//書いてる途中であるかを判定

    [Header("システムメッセージ表示機能(Prefabをアタッチ)")]
    [SerializeField] private ShowSystemMessage showSystemMessage;

    [Header("会話メッセージ(Prefabをアタッチ)")]
    [SerializeField] private TalkMessage talkMessage;

    [Header("システムメッセージ(Prefabをアタッチ)")]
    [SerializeField] private SystemMessage systemMessage;

    [Header("ゴールメッセージ(Prefabをアタッチ)")]
    [SerializeField] private GoalMessage goalMessage;

    [Header("プレイヤーの名前入力(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] public InputField inputPlayerNameField;

    [Header("メッセージパネル判定")]
    public bool isMessagePanel = false;

    [Header("デバッグ用関連")]
    [SerializeField] private TestShowMessage testShowMessage;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        ResetMessage();

        inputPlayerNameField = inputPlayerNameField.GetComponent<InputField>();
        inputPlayerNameField.gameObject.SetActive(false);
    }

    //メッセージパネルの表示・非表示
    public void ViewMessagePanel()
    {
        if (isMessagePanel)
        {
            messagePanel.SetActive(true);
        }
        else
        {
            messagePanel.SetActive(false);
        }
    }

    //メッセージをリセット
    public void ResetMessage()
    {
        messageText.color = Color.white;
        messageText.text = "";
        isMessagePanel = false;
        ViewMessagePanel();
    }

    //テキストを一文字ずつ表示
    async void Write(string s)
    {
        writeSpeed = 0;
        isWrite = true;

        for (int i = 0; i < s.Length; i++)
        {
            messageText.text += s.Substring(i, 1);
            await UniTask.Delay(TimeSpan.FromSeconds(writeSpeed));
        }
        isWrite = false;
    }

    //会話メッセージを表示
    public async UniTask ShowTalkMessage(int number)
    {
        Debug.Log("会話スタート");

        //前のメッセージが書いてる途中であるかを判断。書き途中ならtrue
        if (isWrite) writeSpeed = 0;
        else
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //エクセルデータ型.リスト型[番号].カラム名
            Write(talkMessage.talkMessage[number].message);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            switch (number)
            {
                //各会話において、一番最後に表示するべきテキストを表示。次のメッセージ番号の用意は行わない
                case 2:
                    //スペースキー押下で会話終了
                    ResetMessage();

                    break;

                //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
                default:
                    messageText.text = "";
                    number++;

                    //スペースキー押下で次のメッセージを書く
                    testShowMessage.TestShowTalkMessage(number);
                    break;
            }
        }
    }


    //システムメッセージを表示
    public async UniTask ShowSystemMessage(int number)
    {
        Debug.Log("システムメッセージを表示");

        //前のメッセージが書いてる途中であるかを判断。書き途中ならtrue
        if (isWrite) writeSpeed = 0;
        else
        {
            isMessagePanel = true;
            ViewMessagePanel();

            //エクセルデータ型.リスト型[番号].カラム名
            Write(systemMessage.systemMessage[number].message);
            await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            switch (number)
            {
                //名前入力UIを表示
                case 3:
                    ResetMessage();

                    inputPlayerNameField.gameObject.SetActive(true);

                    break;

                case 6:
                    messageText.text = "";
                    number++;

                    await UniTask.Delay(TimeSpan.FromSeconds(2));

                    // 色を赤色に設定
                    messageText.color = Color.red;

                    Write(systemMessage.systemMessage[number].message);

                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    ResetMessage();

                    break ;


                //メッセージ番号に対応しているメッセージを記載＆次のメッセージ番号を用意
                default:
                    messageText.text = "";
                    number++;

                    //スペースキー押下で次のメッセージを書く
                    showSystemMessage.ShowGameSystemMessage(number);
                    break;
            }
        }
    }

    //ゴールメッセージを表示
    public void ShowGoalMessage(int number) 
    {
        Debug.Log("ゴールメッセージスタート");
        messageText.text = goalMessage.goalMessage[number].message;
        isMessagePanel = true;
        ViewMessagePanel();
    }

    //プレイヤーの名前の入力が完了した際に呼ばれる
    public void SavePlayerName(string playerName)
    {
        if (playerName.Length < 11) 
        {
            //名前を保存
            inputPlayerNameField.text = playerName;

            inputPlayerNameField.gameObject.SetActive(false);

            showSystemMessage.ShowGameSystemMessage(5);
        }
        else
        {
            showSystemMessage.ShowGameSystemMessage(4);
        }
    }  
}
