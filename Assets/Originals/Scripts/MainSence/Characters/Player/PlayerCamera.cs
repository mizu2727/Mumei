using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;
using static GameController;

public class PlayerCamera : MonoBehaviour
{
    [Header("マウス/ゲームパッドの右スティックの感度")]
    [SerializeField]  public float lookSensitivity = 100f;

    [Header("マウス/ゲームパッドの右スティックの旋回速度のSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("マウス/ゲームパッドの右スティックの感度最大値")]
    [SerializeField] float maxLookSensitivity = 500f;

    //マウスの横移動
    float lookX;

    //マウスの縦移動
    float lookY;

    //ゲームパッドの右スティックの横移動
    float lookX2;

    //ゲームパッドの右スティックの縦移動
    float lookY2;

    [Header("カメラのX軸回転角度")]
    private float xRotation = 0f;//カメラのX軸回転角度

    [Header("カメラのX軸回転範囲")]
    [SerializeField] private float xRotationRange = 45f ;

    // 前フレームの後ろを向く状態を保持
    private bool wasTrunLastFrame = false;

    private Transform playerTransform; // プレイヤーのTransform



    private void Start()
    {
        // マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //マウス旋回速度のSliderの最大値を設定
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;

        // プレイヤーのTransformを取得
        playerTransform = Player.instance.transform;

        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance == null || Player.instance.isFallDown) return;

        // マウス感度をスライダーから取得
        if (mouseSensitivitySlider)
        {
            lookSensitivity = mouseSensitivitySlider.value;
            if (lookSensitivity > maxLookSensitivity) lookSensitivity = maxLookSensitivity;
        }


        //Ctrl押下で視点が後ろを向く
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.playerIsBackRotate) 
        {
            if (!wasTrunLastFrame)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を後方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, -0.25f);
                

                // カメラを即座に180度回転（プレイヤーの背後）
                transform.rotation = Quaternion.LookRotation(-playerTransform.forward, Vector3.up);

                // マウス入力を無効化
                lookSensitivity = 0f; 

                if (mouseSensitivitySlider) mouseSensitivitySlider.value = 0f;
                wasTrunLastFrame = true;
            }
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && wasTrunLastFrame)
        {
            

            // カメラをプレイヤーの前方に戻す
            transform.rotation = Quaternion.LookRotation(playerTransform.forward, Vector3.up);
            if (mouseSensitivitySlider)
            {
                //プレイヤーの頭で視界の邪魔になるのを防ぐためにカメラの位置を前方部分へ変更する
                transform.localPosition = new Vector3(0, 1.5f, 0.1f);

                mouseSensitivitySlider.value = maxLookSensitivity / 2f;
                lookSensitivity = mouseSensitivitySlider.value;
            }
            wasTrunLastFrame = false;
        }

        if ( 0 < lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.PlayerIsBackRotate()) 
        {
            //マウスの移動
            lookX =
                Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            lookY =
                Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;

            //ゲームパッドの右スティックの移動

            //Mouse X2…Axis欄で"4th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookX2 =
                Input.GetAxis("Mouse X2") * lookSensitivity * Time.deltaTime;

            //Mouse Y2…Axis欄で"5th axis (Joysticks)"を選択。コントローラーでは右スティックになる
            lookY2 =
                Input.GetAxis("Mouse Y2") * lookSensitivity * Time.deltaTime;

            xRotation -= (lookY + lookY2);
            xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

            // カメラの上下回転
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // プレイヤーの左右回転（カメラの親オブジェクト）
            playerTransform.Rotate(Vector3.up * (lookX + lookX2));
        }
    }
}
