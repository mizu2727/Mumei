using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class LanguageController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static LanguageController instance;

    [Header("ボタンテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ButtonMessage buttonMessage;

    [Header("UIテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private UITextMessage uITextMessage;

    [Header("言語ステータス(ヒエラルキー上からの編集禁止)")]
    public LanguageStatus languageStatus;

    [Header("SEデータ(共通のScriptableObjectをアタッチする必要がある)")]
    [SerializeField] public SO_SE sO_SE;

    /// <summary>
    /// SE用audioSource
    /// </summary>
    private AudioSource audioSourceSE;

    /// <summary>
    /// ボタンSEのID
    /// </summary>
    private readonly int buttonSEid = 4;

    /// <summary>
    /// 言語ステータス
    /// </summary>
    public enum LanguageStatus 
    {
        /// <summary>
        /// 日本語
        /// </summary>
        kJapanese,

        /// <summary>
        /// 英語
        /// </summary>
        kEnglish,
    }

    [Header("ボタン配下内のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] buttonTextArray;

    [Header("ボタンメッセージ番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] buttonTextNumberArray;

    [Header("通常のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] uITextArray;

    [Header("通常テキスト番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] uITextNumberArray;

    [Header("アイテムを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Item[] itemArray;


    /// <summary>
    /// 言語ステータスを取得する
    /// </summary>
    /// <returns>言語ステータス</returns>
    public LanguageStatus GetLanguageStatus() 
    {
        return languageStatus;
    }

    /// <summary>
    /// 言語ステータスを設定する
    /// </summary>
    /// <param name="languageStatus">言語ステータス</param>
    public void SetLanguageStatus(LanguageStatus languageStatus) 
    {
        this.languageStatus = languageStatus;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;

        //SE音量変更時のイベント登録
        MusicController.OnSEVolumeChangedEvent += UpdateSEVolume;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;

        //SE音量変更時のイベント登録解除
        MusicController.OnSEVolumeChangedEvent -= UpdateSEVolume;
    }

    /// <summary>
    /// SE音量を0～1へ変更
    /// </summary>
    /// <param name="volume">音量</param>
    private void UpdateSEVolume(float volume)
    {
        if (audioSourceSE != null)
        {
            audioSourceSE.volume = volume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        
    }

    /// <summary>
    /// AudioSourceの初期化
    /// </summary>
    private void InitializeAudioSource()
    {
        //AudioSourceを取得
        audioSourceSE = gameObject.AddComponent<AudioSource>();

        //MusicControllerで設定されているSE用のAudioMixerGroupを設定する
        audioSourceSE.outputAudioMixerGroup = MusicController.instance.audioMixerGroupSE;
        audioSourceSE.playOnAwake = false;
    }

    private void Awake()
    {
        //シングルトンの実装
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeAudioSource();

        //言語を設定する
        SettingLanguageText();
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    private void SettingLanguageText() 
    {
        //保存用変数に言語設定を保存する
        saveLanguageStatus = languageStatus;

        //言語ステータスに応じて、テキストを変更する
        switch (languageStatus) 
        {
            //日本語
            case LanguageStatus.kJapanese:

                //ボタンテキストを日本語に変更する
                for (int i = 0; i < buttonTextArray.Length; i++)
                {
                    buttonTextArray[i].text = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageJapanese;
                }

                //ボタンサイズを日本語用に変更する
                for (int i = 0; i < buttonTextArray.Length; i++)
                {
                    buttonTextArray[i].fontSize = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageSizeJapanese;
                }

                //UIテキストを日本語に変更する
                for (int i = 0; i < uITextArray.Length; i++)
                {
                    uITextArray[i].text = uITextMessage.uITextMessage[uITextNumberArray[i]].messageJapanese;
                }

                //UIサイズを日本語用に変更する
                for (int i = 0; i < uITextArray.Length; i++)
                {
                    uITextArray[i].fontSize = uITextMessage.uITextMessage[uITextNumberArray[i]].messageSizeJapanese;
                }

                break;

            //英語
            case LanguageStatus.kEnglish:

                //ボタンテキストを英語に変更する
                for (int i = 0; i < buttonTextArray.Length; i++)
                {
                    buttonTextArray[i].text = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageEnglish;
                }

                //ボタンサイズを英語用に変更する
                for (int i = 0; i < buttonTextArray.Length; i++)
                {
                    buttonTextArray[i].fontSize = buttonMessage.buttonMessage[buttonTextNumberArray[i]].messageSizeEnglish;
                }

                //UIテキストを英語に変更する
                for (int i = 0; i < uITextArray.Length; i++)
                {
                    uITextArray[i].text = uITextMessage.uITextMessage[uITextNumberArray[i]].messageEnglish;
                }

                //UIサイズを英語用に変更する
                for (int i = 0; i < uITextArray.Length; i++)
                {
                    uITextArray[i].fontSize = uITextMessage.uITextMessage[uITextNumberArray[i]].messageSizeEnglish;
                }

                break;
        }

        //アイテムのテキスト関連を設定する
        for (int i = 0; i < itemArray.Length; i++)
        {
            itemArray[i].SettingLanguageText();
        }

        //ポーズコントローラーが存在する場合
        if (PauseController.instance != null)
        {
            //ポーズコントローラーのテキスト関連を設定する
            PauseController.instance.SettingLanguageText();
        }
    }

    /// <summary>
    /// 日本語ボタンがクリックされたときの処理
    /// </summary>
    public void OnClickedJapaseneButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //言語ステータスを日本語に設定する
        languageStatus = LanguageStatus.kJapanese;

        //言語を設定する
        SettingLanguageText();
    }

    /// <summary>
    /// 英語ボタンがクリックされたときの処理
    /// </summary>
    public void OnClickedEnglishButton() 
    {
        //ボタンSE
        MusicController.instance.PlayAudioSE(audioSourceSE, sO_SE.GetSEClip(buttonSEid));

        //言語ステータスを英語に設定する
        languageStatus = LanguageStatus.kEnglish;

        //言語を設定する
        SettingLanguageText();
    }
}
