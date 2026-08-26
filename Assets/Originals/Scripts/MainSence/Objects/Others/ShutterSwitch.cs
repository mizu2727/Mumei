using UnityEngine;
using UnityEngine.SceneManagement;

public class ShutterSwitch : MonoBehaviour
{
    [Header("シャッターオブジェクト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject shutter;

    //シャッター開閉フラグ
    private bool isShutterOpen;

    /// <summary>
    /// シャッター開閉フラグを取得する関数
    /// </summary>
    /// <returns>シャッター開閉フラグ</returns>
    public bool GetIsShutterOpen()
    {
        return isShutterOpen;
    }


    private void OnDestroy()
    {
        //シャッターが存在する場合
        if (shutter != null)
        {
            //シャッターをnullに設定
            shutter = null;
        }
    }


    private void Start()
    {
        //初期化
        isShutterOpen = false;
    }

    /// <summary>
    /// シャッターを開く処理
    /// </summary>
    public void OpenShutter() 
    {
        //シャッターオブジェクトを非表示にする
        shutter.SetActive(false);

        //シャッター開閉フラグをオンにする
        isShutterOpen = true;
    }
}
