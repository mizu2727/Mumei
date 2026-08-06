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
    /// ライトの減光判定に使うRayCastの距離
    /// </summary>
    private const float kRayCheckDistance = 12.5f;

    /// <summary>
    /// 近距離に壁などがある場合のライトの明るさ
    /// </summary>
    private const float kDimmedLightIntensity = 0.01f;

    /// <summary>
    /// ライトの明るさを変化させる速度(点滅防止用)
    /// </summary>
    private const float kLightIntensityChangeSpeed = 500.0f;

    /// <summary>
    /// RayCastの対象にするレイヤー(初期値は全レイヤー)
    /// </summary>
    private LayerMask rayCheckLayerMask = ~0;

    /// <summary>
    /// playerHasLightにアタッチされているLightコンポーネント
    /// </summary>
    private Light playerLightComponent;

    /// <summary>
    /// ライト本来の明るさ(Inspectorで設定された値をStart時に保持)
    /// </summary>
    private float defaultLightIntensity;


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
        if (PlayerCamera.instance != null)
        {
            cameraTransform = PlayerCamera.instance.transform;
        }
        else
        {
            Debug.LogError("No Camera found as a child of the Player!");
        }
    }

    /// <summary>
    /// オブジェクトが破壊された際に呼ばれる関数
    /// </summary>
    private void OnDestroy() 
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
        //初期化
        playerHasLight.SetActive(false);
        Player.instance.IsLight = false;

        //現在のシーンがHomeSceneの場合
        if (SceneManager.GetActiveScene().name == CommonController.instance.GetHomeSceneName())
        {
            //HomeSceneの場合プレイヤーはライトを持っていないため、フラグをfalseに設定
            Player.instance.SetIsHavePlayerLight(false);
        }
        else 
        {
            //HomeScene以外の場合プレイヤーはライトを持っているため、フラグをtrueに設定
            Player.instance.SetIsHavePlayerLight(true);
        }


        //ライトの減光処理用にLightコンポーネントを取得(playerHasLight自身、または子オブジェクトから取得)
        playerLightComponent = playerHasLight.GetComponentInChildren<Light>(true);

        //Lightコンポーネントが取得できた場合
        if (playerLightComponent != null)
        {
            //Inspectorで設定された本来の明るさを保持しておく
            defaultLightIntensity = playerLightComponent.intensity;
        }
        else
        {
            Debug.LogError("playerHasLightにLightコンポーネントが見つかりません");
        }


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

        //近距離のオブジェクトを検知してライトの明るさを調整する(眩しさ防止)
        AdjustLightIntensityByRayCast();
    }

    /// <summary>
    /// ライトボタンを押下しているかを判定する
    /// ライト切り替え…F・Hキー
    /// Light…"joystick button 0"を割り当てており、コントローラーではAボタンになる
    /// </summary>
    /// <returns>trueでライトボタンを押下</returns>
    bool PlayerIsLight()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.H) 
             || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetButtonDown("Light");
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
        //通常プレイモードの場合&&プレイヤーライトを持っている場合
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.GetIsHavePlayerLight()) 
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

    /// <summary>
    /// 近距離に壁などのオブジェクトがある場合、RayCastで検知してライトの明るさを下げる処理
    /// (プレイヤーライトが近距離のオブジェクトを照らして眩しくなるのを防ぐため)
    /// </summary>
    void AdjustLightIntensityByRayCast()
    {
        //ライトコンポーネントが取得できていない場合
        if (playerLightComponent == null)
        {
            //処理をスキップ
            return;
        }

        //ライトが消えている場合
        if (!playerHasLight.activeSelf) 
        {
            //処理をスキップ
            return; 
        }

        //ライト本来の明るさ目標値を取得
        float targetIntensity = defaultLightIntensity;

        //カメラの正面方向にRayCastを飛ばし、近距離にオブジェクトがある場合
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, kRayCheckDistance, rayCheckLayerMask))
        {
            //距離が近いほど明るさを下げる(距離0でkDimmedLightIntensity、kRayCheckDistanceでdefaultLightIntensityになるよう補間)
            float t = Mathf.Clamp01(hit.distance / kRayCheckDistance);
            targetIntensity = Mathf.Lerp(kDimmedLightIntensity, defaultLightIntensity, t);
        }

        //明るさが急激に変化してちらつくのを防ぐため、なめらかに変化させる
        playerLightComponent.intensity = Mathf.MoveTowards(playerLightComponent.intensity, targetIntensity, kLightIntensityChangeSpeed * Time.deltaTime);
    }
}


