using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageController : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static LanguageController instance;

    [Header("ボタンテキストメッセージ(Prefabをアタッチ)")]
    [SerializeField] private ButtonMessage buttonMessage;

    [Header("言語ステータス(ヒエラルキー上からの編集禁止)")]
    public LanguageStatus languageStatus;

    /// <summary>
    /// 言語ステータス
    /// </summary>
    public enum LanguageStatus 
    {
        /// <summary>
        /// 日本語
        /// </summary>
        Japanese,

        /// <summary>
        /// 英語
        /// </summary>
        English,
    }

    [Header("ボタン配下内のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] buttonTextArray;

    [Header("ボタンメッセージ番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] buttonTextNumberArray;

    [Header("通常のテキストを格納(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Text[] uITextArray;

    [Header("通常テキスト番号を記載(ヒエラルキー上から記載すること)")]
    [SerializeField] public int[] uITextNumberArray;


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
        //デバッグ用…英語に設定する
        languageStatus = LanguageStatus.English;

        //言語を設定する
        SettingLanguageText();
    }

    /// <summary>
    /// 言語を設定する
    /// </summary>
    private void SettingLanguageText() 
    {
        //言語ステータスに応じて、テキストを変更する
        switch (languageStatus) 
        {
            //日本語
            case LanguageStatus.Japanese:

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

                break;

            //英語
            case LanguageStatus.English:

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

                break;
        }
    }
}
