using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
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

    [Header("カメラのX軸回転角度")]
    private float xRotation = 0f;

    [Header("カメラのX軸回転範囲")]
    [SerializeField] private float xRotationRange = 45f ;

    /// <summary>
    /// 前フレームの後ろを向くフラグ
    /// </summary>
    private bool wasTrunLastFrame = false;

    /// <summary>
    /// プレイヤーのTransform
    /// </summary>
    private Transform playerTransform;


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

    private void Start()
    {
        //マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //プレイヤーのTransformを取得
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance == null || Player.instance.isFallDown || Time.timeScale == 0) return;

        //Ctrl押下で視点が後ろを向く
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.playerIsBackRotate) 
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
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
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

        if ( 0 < GameController.lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.PlayerIsBackRotate()) 
        {
            //マウスの移動
            lookX = Input.GetAxis("Mouse X") * GameController.lookSensitivity;
            lookY = Input.GetAxis("Mouse Y") * GameController.lookSensitivity;

            //ゲームパッドの右スティックの移動
            //Mouse X2…Axis欄で"4th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookX2 = Input.GetAxis("Mouse X2") * GameController.lookSensitivity * Time.deltaTime;

            //Mouse Y2…Axis欄で"5th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookY2 = Input.GetAxis("Mouse Y2") * GameController.lookSensitivity * Time.deltaTime;

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

            // カメラの上下回転
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // プレイヤーの左右回転（カメラの親オブジェクト）
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
        }
    }
}
