using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class ScreenAspect : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static ScreenAspect instance;

    

    /// <summary>
    /// フルスクリーンフラグ
    /// </summary>
    private bool isFullScreenFlag = true;

    /// <summary>
    /// 画面解像度1366*768のX部分
    /// </summary>
    const int kCoustomResolutionX01 = 1366;

    /// <summary>
    /// 画面解像度1366*768のY部分
    /// </summary>
    const int kCoustomResolutionY01 = 768;

    /// <summary>
    /// 画面解像度1280*1024のX部分
    /// </summary>
    const int kCoustomResolutionX02 = 1280;

    /// <summary>
    /// 画面解像度1280*1024のY部分
    /// </summary>
    const int kCoustomResolutionY02 = 1024;

    /// <summary>
    /// 画面解像度1600*1200のX部分
    /// </summary>
    const int kCoustomResolutionX03 = 1600;

    /// <summary>
    /// 画面解像度1600*1200のY部分
    /// </summary>
    const int kCoustomResolutionY03 = 1200;

    /// <summary>
    /// デフォルトの画面解像度1920*1080、1920*1200のX部分
    /// </summary>
    const int kDefaultResolutionX = 1920;

    /// <summary>
    /// デフォルトの画面解像度1920*1080のY部分
    /// </summary>
    const int kDefaultResolutionY = 1080;

    /// <summary>
    /// 画面解像度2560*1440のX部分
    /// </summary>
    const int kCoustomResolutionX06 = 2560;

    /// <summary>
    /// 画面解像度2560*1440のY部分
    /// </summary>
    const int kCoustomResolutionY06 = 1440;

    /// <summary>
    /// 画面解像度3840*2160のX部分
    /// </summary>
    const int kCoustomResolutionX07 = 3840;

    /// <summary>
    /// 画面解像度3840*2160のY部分
    /// </summary>
    const int kCoustomResolutionY07 = 2160;

    /// <summary>
    /// 画面解像度7680*4320のX部分
    /// </summary>
    const int kCoustomResolutionX08 = 7680;

    /// <summary>
    /// 画面解像度7680*4320のY部分
    /// </summary>
    const int kCoustomResolutionY08 = 4320;

    /// <summary>
    /// 画面解像度X配列
    /// </summary>
    int[] ResolutionXArray = { kCoustomResolutionX01, kCoustomResolutionX02
            , kCoustomResolutionX03, kDefaultResolutionX
            , kDefaultResolutionX, kCoustomResolutionX06
            , kCoustomResolutionX07, kCoustomResolutionX08};

    /// <summary>
    /// 画面解像度Y配列
    /// </summary>
    int[] ResolutionYArray = { kCoustomResolutionY01, kCoustomResolutionY02
            , kCoustomResolutionY03, kDefaultResolutionY
            , kCoustomResolutionY03, kCoustomResolutionY06
            , kCoustomResolutionY07, kCoustomResolutionY08};

    /// <summary>
    /// 解像度の配列インデックス番号
    /// </summary>
    int ResolutionArrayIndex;

    /// <summary>
    /// デフォルトの解像度の配列インデックス番号
    /// </summary>
    int kDefaultResolutionArrayIndex = 3;

    /// <summary>
    /// 解像度の配列インデックス番号を設定
    /// </summary>
    /// <param name="index">解像度の配列インデックス番号</param>
    public void SetResolutionArrayIndex(int index)
    {
        ResolutionArrayIndex = index;
    }

    /// <summary>
    /// フルスクリーンフラグ値を設定
    /// </summary>
    /// <param name="flag">フルスクリーンフラグ値</param>
    public void SetFullScreenFlag(bool flag)
    {
        isFullScreenFlag = flag;
    }

    private void OnEnable()
    {
        //sceneLoadedに「OnSceneLoaded」関数を追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //シーン遷移時に設定するための関数登録解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }


    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else 
        {
            Destroy(this.gameObject); 
        }


    }

    private void Start()
    {
        //スクリーンモードとサイズを設定
        SetScreenResolution(ResolutionArrayIndex, isFullScreenFlag);
    }

    /// <summary>
    /// 「ウィンドウ」ボタン押下時の処理
    /// </summary>
    public void OnclickedWindowModeButton() 
    {

        isFullScreenFlag = false;

        //スクリーンモードとサイズを設定
        SetScreenResolution(ResolutionArrayIndex, isFullScreenFlag);
    }

    /// <summary>
    /// 「フルスクリーン」ボタン押下時の処理
    /// </summary>
    public void OnclickedFullScreenModeButton() 
    {

        isFullScreenFlag = true;

        //スクリーンモードとサイズを設定
        SetScreenResolution(ResolutionArrayIndex, isFullScreenFlag);
    }

    /// <summary>
    /// 「-」ボタン押下時の処理
    /// </summary>
    public void OnClickedReduceResolutionArrayIndexButton()
    {

        //インデックス番号が0の場合
        if (ResolutionArrayIndex == 0)
        {
            //インデックス番号を最後尾に設定
            ResolutionArrayIndex = ResolutionXArray.Length - 1;
        }
        else 
        {
            //解像度の配列インデックス番号を設定
            ResolutionArrayIndex -= 1;
        }

        //スクリーンモードとサイズを設定
        SetScreenResolution(ResolutionArrayIndex, isFullScreenFlag);
    }

    /// <summary>
    /// 「+」ボタン押下時の処理
    /// </summary>
    public void OnClickedAddResolutionArrayIndexButton()
    {
        //インデックス番号が最後尾の場合
        if (ResolutionArrayIndex == ResolutionXArray.Length - 1)
        {
            //インデックス番号を0に設定
            ResolutionArrayIndex = 0;
        }
        else
        {
            //解像度の配列インデックス番号を設定
            ResolutionArrayIndex += 1;
        }

        //スクリーンモードとサイズを設定
        SetScreenResolution(ResolutionArrayIndex, isFullScreenFlag);
    }

    /// <summary>
    /// スクリーンモードとサイズを設定
    /// </summary>
    /// <param name="index">インデックス番号</param>
    public void SetScreenResolution(int index, bool flag) 
    {
        //セーブ用フルスクリーンフラグ値を引数から取得
        isFullScreen = flag;

        //セーブ用解像度の配列インデックス番号を引数から取得
        resolutionArrayIndexNumber = index;

        //スクリーンモードとサイズを設定
        Screen.SetResolution(ResolutionXArray[index]
            , ResolutionYArray[index], flag);

        //設定した解像度をUIに反映
        GameController.instance.resolutionText.text = ResolutionXArray[index] + " x " + ResolutionYArray[index];
    }
}
