using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SaveLoad : MonoBehaviour
{
    UserData userData;


    private void Update()
    {

    }

    /// <summary>
    /// データを保存するメソッド
    /// </summary>
    public void SaveUserData() 
    {
        //userData = GetComponent<UserData>();

        //ユーザーデータを保存するクラス
        userData = new UserData() 
        {
            playerName = GameController.playerName,
            playCount = GameController.playCount,

            //現在のシーン名を取得して保存
            sceneName = SceneManager.GetActiveScene().name,
        };

        //ユーザーデータをJSON形式で保存
        //UserDataオブジェクトをJSON文字列に変換
        string json = JsonUtility.ToJson(userData, true);

        Debug.Log("Json形式でデータを保存した内容:" + json);

        //PlayerPrefsに保存
        PlayerPrefs.SetString("PlayerUserData", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// データをロードするメソッド
    /// </summary>
    public void LoadUserData() 
    {
        if (PlayerPrefs.HasKey("PlayerUserData")) 
        {
            //PlayerPrefsからJSON文字列を取得
            string josn = PlayerPrefs.GetString("PlayerUserData");

            //JSON文字列をUserDataオブジェクトに変換
            UserData userData = JsonUtility.FromJson<UserData>(josn);           

            SceneManager.LoadScene(userData.sceneName);

            //各パラメーターにユーザーデータを設定
            GameController.playerName = userData.playerName;
            GameController.playCount = ++userData.playCount;
        }
        else
        {
            Debug.LogWarning("PlayerUserDataが存在しません");
        }
    }

    /// <summary>
    /// 保存したデータを初期化するメソッド
    /// </summary>
    public void ResetUserData()
    {
        // 特定のキーのデータを削除する場合
        if (PlayerPrefs.HasKey("PlayerUserData"))
        {
            PlayerPrefs.DeleteKey("PlayerUserData");
            Debug.Log("PlayerUserDataを初期化しました");
        }

        // 変更を保存する
        PlayerPrefs.Save();
    }
}
