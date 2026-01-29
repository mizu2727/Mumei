using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameController;

public class PlayerLight : MonoBehaviour
{
    [Header("プレイヤーカメラ(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform cameraTransform;

    [Header("ライト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private GameObject playerHasLight;

    /// <summary>
    /// エラー防止用に追加。シーンがロードされた際にカメラ参照を更新するために、SceneManager.sceneLoaded イベントを登録
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// エラー防止用に追加。メモリリークを防ぐため、シーン遷移イベントのリスナーを削除
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// エラー防止用に追加。シーンをロード
    /// </summary>
    /// <param name="scene">シーン名</param>
    /// <param name="mode">シーンモード</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //シーン遷移後にカメラを再設定
        UpdateCameraReference(); 
    }

    /// <summary>
    /// エラー防止用に追加。シーン遷移や初期化時にカメラのTransformを動的に取得
    /// </summary>
    void UpdateCameraReference()
    {
        //プレイヤーの子オブジェクトからカメラを取得
        Camera playerCamera = Player.instance.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }
        else
        {
            Debug.LogError("No Camera found as a child of the Player!");
        }
    }

    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    void OnDestroy() 
    {
        //cameraTransformが存在する場合
        if (cameraTransform != null) 
        {
            //cameraTransformをnullに設定
            cameraTransform = null;
        }

        //playerHasLightが存在する場合
        if (playerHasLight != null) 
        {
            //playerHasLightをnullに設定
            playerHasLight = null;
        }
    }

    private void Start()
    {
        playerHasLight.SetActive(false);
        Player.instance.IsLight = false;

        //シーン開始時にカメラを再取得
        UpdateCameraReference();
    }

    void Update()
    {
        //プレイヤー死亡時(プレイヤーオブジェクト削除時)にカメラの位置が参照できなくなるため、中断処理を追加
        if (cameraTransform == null) return;

        //ゲームプレイモード以外の場合、強制的にライトをオフにする
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame) 
        {
            playerHasLight.SetActive(false);
            Player.instance.IsLight = false;
        }

        //カメラの座標・角度を追従する
        TranceCamera();

        //ライトを点灯/消灯する
        TurnOnAndOfLight();


    }

    /// <summary>
    /// ライトボタンを押下しているかを判定する
    /// ライト切り替え…F・1キー
    /// Light…"joystick button 0"を割り当てており、コントローラーではAボタンになる
    /// </summary>
    /// <returns>trueでライトボタンを押下</returns>
    bool PlayerIsLight()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetButtonDown("Light");
    }

    /// <summary>
    /// カメラの座標・角度を追従する
    /// </summary>
    void TranceCamera()
    {
        //座標追従
        this.transform.position = cameraTransform.position;

        //角度追従
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraTransform.rotation, 0.5f); 
    }

    /// <summary>
    /// ライトを点灯/消灯する
    /// </summary>
    void TurnOnAndOfLight() 
    {
        //プレイヤーの子オブジェクトからカメラを取得
        Camera playerCamera = Player.instance.GetComponentInChildren<Camera>();

        //通常プレイモードの場合
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            if (PlayerIsLight() && !Player.instance.IsLight && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                //ライトが点く
                playerHasLight.SetActive(true);
                Player.instance.IsLight = true;
            }
            else if ((PlayerIsLight() && Player.instance.IsLight) && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                //ライトが消える
                playerHasLight.SetActive(false);
                Player.instance.IsLight = false;
            }
        } 
    }
}


