using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static PlayerCamera instance;

    /// <summary>
    /// マウス/ゲームパッドの右スティックの感度を保存
    /// </summary>
    private float keepMouseSensitivitySlider;

    /// <summary>
    /// マウスの横移動
    /// </summary>
    float lookX;

    /// <summary>
    /// マウスの縦移動
    /// </summary>
    float lookY;

    /// <summary>
    /// ゲームパッドの右スティックの横移動
    /// </summary>
    float lookX2;

    /// <summary>
    /// ゲームパッドの右スティックの縦移動
    /// </summary>
    float lookY2;

    /// <summary>
    /// 90度X軸回転ベクトル
    /// </summary>
    private Vector3 rotateX90 = new Vector3(90.0f, 0, 0);

    /// <summary>
    /// カメラのX軸回転角度
    /// </summary>
    private float xRotation = 0.0f;

    /// <summary>
    /// カメラのX軸回転範囲(上側)
    /// </summary>
    private const float kXRotationUpRange = 45.0f;

    /// <summary>
    /// 現在のカメラのX軸回転範囲(下側)
    /// </summary>
    private float currentXRotationDownRange;

    /// <summary>
    /// 通常時のカメラのX軸回転範囲(下側)
    /// </summary>
    private const float kXRotationDownRange = 80.0f;

    /// <summary>
    /// 隠れている時のカメラのX軸回転範囲(下側)
    /// </summary>
    private const float kHiddenXRotationDownRange = 45.0f;

    /// <summary>
    /// 回転速度倍率1
    /// </summary>
    private const float kRotationSpeedMagnification1 = 1.0f;

    /// <summary>
    /// 前フレームの後ろを向くフラグ
    /// </summary>
    private bool wasTrunLastFrame = false;

    /// <summary>
    /// X軸回転リセットフラグ
    /// </summary>
    private bool isResetXRotate = false;

    /// <summary>
    /// プレイヤーのTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// X軸回転リセットフラグを取得
    /// </summary>
    /// <returns>X軸回転リセットフラグ</returns>
    public bool GetIsResetXRotate()
    {
        return isResetXRotate;
    }

    /// <summary>
    /// X軸回転リセットフラグを取得
    /// </summary>
    /// <param name="isResetXRotateValue">X軸回転リセットフラグ</param>
    public void SetIsResetXRotate(bool isResetXRotateValue)
    {
        isResetXRotate = isResetXRotateValue;
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

    /// <summary>
    /// シーン遷移時に設定
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

    }

    private void Awake()
    {
        //シングルトンの設定
        if (instance == null)
        {
            instance = this;

            //シーン遷移時に破棄されないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //すでにインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //プレイヤーのTransformを取得
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;

        //カメラの下回転範囲を通常に戻す
        currentXRotationDownRange = kXRotationDownRange;

        //X軸回転リセットフラグを初期化
        isResetXRotate = false;
    }

    private void Update()
    {
        //ストーリーモードでX軸回転をリセットする
        if (isResetXRotate) ResetXRotate();

        //通常のプレイ以外の場合
        if (Player.instance == null || Player.instance.GetIsFallDown()
            || Time.timeScale == 0) 
        {
            //処理をスキップ
            return; 
        }

        

        //プレイヤーが隠れている場合
        if (Player.instance.GetIsPlayerHidden())
        {
            //マウスカーソルを非表示にし、位置を固定
            //(ポーズ解除後隠れている最中にマウスカーソルが表示されてしまうバグを防ぐため)
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        //Ctrl押下で視点が後ろを向く
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame 
            && Player.instance.playerIsBackRotate && !Player.instance.GetIsPlayerHidden()) 
        {
            if (!wasTrunLastFrame)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を後方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, -0.25f);
                
                // カメラを即座に180度回転（プレイヤーの背後）
                transform.rotation = Quaternion.LookRotation(-playerTransform.forward, Vector3.up);

                //現在のマウス感度を保存
                keepMouseSensitivitySlider = GameController.lookSensitivity;

                wasTrunLastFrame = true;
            }
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame 
            && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
        {
            // カメラをプレイヤーの前方に戻す
            transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            if (GameController.instance.mouseSensitivitySlider)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を前方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, 0.1f);
            }
            wasTrunLastFrame = false;
        }

        if ( 0 < GameController.lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame 
            && !Player.instance.PlayerIsBackRotate()) 
        {
            //マウスの移動
            lookX = Input.GetAxis("Mouse X") * GameController.lookSensitivity;
            lookY = Input.GetAxis("Mouse Y") * GameController.lookSensitivity;

            //ゲームパッドの右スティックの移動
            //Mouse X2…Axis欄で"4th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookX2 = Input.GetAxis("Mouse X2") * GameController.lookSensitivity * Time.deltaTime;

            //Mouse Y2…Axis欄で"5th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookY2 = Input.GetAxis("Mouse Y2") * GameController.lookSensitivity * Time.deltaTime;

            //プレイヤーが隠れている場合
            if (Player.instance.GetIsPlayerHidden())
            {
                //カメラの下回転範囲を狭くする
                currentXRotationDownRange = kHiddenXRotationDownRange;
            }
            else 
            {
                //カメラの下回転範囲を通常に戻す
                currentXRotationDownRange = kXRotationDownRange;
            }

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -kXRotationUpRange, currentXRotationDownRange);

            //カメラの上下回転
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            //プレイヤーの左右回転（カメラの親オブジェクト）
            playerTransform.Rotate(Vector3.up * (lookX + lookX2));
        }
    }

    /// <summary>
    /// カメラの回転をリセットする
    /// </summary>
    public void ResetCameraRotation()
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.Story) 
        {
            //上下回転をリセット
            xRotation = 0f;

            //カメラのローカル回転を初期状態に戻す
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            //カメラのローカル回転を初期状態に戻す
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Debug.Log("カメラの回転をリセット");
        }
    }

    /// <summary>
    /// カメラのX軸回転をリセットする
    /// </summary>
    public void ResetXRotate()
    {
        //カメラのX軸の向きを0度になるように回転させる
        if (transform.rotation.x < 0)
        {
            transform.Rotate(-rotateX90 * (Time.deltaTime * kRotationSpeedMagnification1));
            Debug.Log("transform.rotation.xを下へ回転");
        }
        else if (transform.rotation.x > 0)
        {
            transform.Rotate(rotateX90 * (Time.deltaTime * kRotationSpeedMagnification1));
            Debug.Log("transform.rotation.xを上へ回転");
        }
        else
        {
            //回転終了
            isResetXRotate = false;
            Debug.Log("transform.rotation.x回転終了");
        }
    }
}
