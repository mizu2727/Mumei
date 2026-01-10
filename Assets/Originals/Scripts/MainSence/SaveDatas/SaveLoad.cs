using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SaveLoad : MonoBehaviour
{
    UserData userData;

    /// <summary>
    /// データを保存するメソッド
    /// </summary>
    public void SaveUserData() 
    {
        //userData = GetComponent<UserData>();

        //ユーザーデータを保存するクラス
        userData = new UserData()
        {
            //入力したプレイヤー名を保存
            playerName = GameController.playerName,

            //プレイ回数を保存
            playCount = GameController.playCount,

            //現在のシーン名を取得して保存
            sceneName = SceneManager.GetActiveScene().name,

            //マウス/ゲームパッドの右スティックの感度を保存
            sensitivityValue = GameController.lookSensitivity,

            //BGM音量を保存
            bGMVolume = GameController.bGMVolume,

            //SE音量を保存
            sEVolume = GameController.sEVolume,

            //明るさを保存
            brightnessValue = GameController.brightnessValue,

            //フルスクリーンフラグを保存
            isFullScreen = GameController.isFullScreen,

            //画面解像度の配列インデックス番号を保存
            resolutionArrayIndexNumber = GameController.resolutionArrayIndexNumber,

            //OperationPanel手動閲覧フラグを保存
            isSelfViewOperationPanel = GameController.isSaveSelfViewOperationPanel,

            //UseItemTextPanel手動閲覧フラグを保存
            isSelfViewUseItemTextPanel = GameController.isSaveSelfViewUseItemTextPanel,

            //CompassTextPanel手動閲覧フラグを保存
            isSelfViewCompassTextPanel = GameController.isSaveSelfViewCompassTextPanel,
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
            GameController.lookSensitivity = userData.sensitivityValue;
            GameController.bGMVolume = userData.bGMVolume;
            GameController.sEVolume = userData.sEVolume;
            GameController.brightnessValue = userData.brightnessValue;
            GameController.isFullScreen = userData.isFullScreen;
            GameController.resolutionArrayIndexNumber = userData.resolutionArrayIndexNumber;
            GameController.isSaveSelfViewOperationPanel = userData.isSelfViewOperationPanel;
            GameController.isSaveSelfViewUseItemTextPanel = userData.isSelfViewUseItemTextPanel;
            GameController.isSaveSelfViewCompassTextPanel = userData.isSelfViewCompassTextPanel;
        }
        else
        {
            Debug.LogWarning("PlayerUserDataが存在しません");
        }
    }

    /// <summary>
    /// シーン遷移時用データを保存するメソッド
    /// </summary>
    public void SaveSceneTransitionUserData()
    {
 
        //ユーザーデータを保存するクラス
        userData = new UserData()
        {
            //マウス/ゲームパッドの右スティックの感度を保存
            sensitivityValue = GameController.lookSensitivity,

            //BGM音量を保存
            bGMVolume = GameController.bGMVolume,

            //SE音量を保存
            sEVolume = GameController.sEVolume,

            //明るさを保存
            brightnessValue = GameController.brightnessValue,

            //フルスクリーンフラグを保存
            isFullScreen = GameController.isFullScreen,

            //画面解像度の配列インデックス番号を保存
            resolutionArrayIndexNumber = GameController.resolutionArrayIndexNumber,

            //OperationPanel手動閲覧フラグを保存
            isSelfViewOperationPanel = GameController.isSaveSelfViewOperationPanel,

            //UseItemTextPanel手動閲覧フラグを保存
            isSelfViewUseItemTextPanel = GameController.isSaveSelfViewUseItemTextPanel,

            //CompassTextPanel手動閲覧フラグを保存
            isSelfViewCompassTextPanel = GameController.isSaveSelfViewCompassTextPanel,
        };

        Debug.Log("シーン遷移用Json形式でデータを保存した内容:" + JsonUtility.ToJson(userData, true));

        //ユーザーデータをJSON形式で保存
        //UserDataオブジェクトをJSON文字列に変換
        string json = JsonUtility.ToJson(userData, true);

        //PlayerPrefsに保存
        PlayerPrefs.SetString("SceneTransitionPlayerUserData", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// シーン遷移時用データをロードするメソッド
    /// </summary>
    public void LoadSceneTransitionUserData()
    {
        if (PlayerPrefs.HasKey("SceneTransitionPlayerUserData"))
        {
            //PlayerPrefsからJSON文字列を取得
            string josn = PlayerPrefs.GetString("SceneTransitionPlayerUserData");

            //JSON文字列をUserDataオブジェクトに変換
            UserData userData = JsonUtility.FromJson<UserData>(josn);

            //各パラメーターにシーン遷移時用データを設定
            GameController.lookSensitivity = userData.sensitivityValue;
            GameController.bGMVolume = userData.bGMVolume;
            GameController.sEVolume = userData.sEVolume;
            GameController.brightnessValue = userData.brightnessValue;
            GameController.isFullScreen = userData.isFullScreen;
            GameController.resolutionArrayIndexNumber = userData.resolutionArrayIndexNumber;
            GameController.isSaveSelfViewOperationPanel = userData.isSelfViewOperationPanel;
            GameController.isSaveSelfViewUseItemTextPanel = userData.isSelfViewUseItemTextPanel;
            GameController.isSaveSelfViewCompassTextPanel = userData.isSelfViewCompassTextPanel;
        }
        else
        {
            Debug.LogWarning("SceneTransitionPlayerUserDataが存在しません");
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

        // 特定のキーのデータを削除する場合
        if (PlayerPrefs.HasKey("SceneTransitionPlayerUserData"))
        {
            PlayerPrefs.DeleteKey("SceneTransitionPlayerUserData");
            Debug.Log("SceneTransitionPlayerUserDataを初期化しました");
        }

        // 変更を保存する
        PlayerPrefs.Save();
    }
}
