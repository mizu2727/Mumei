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



    private void Start()
    {
        // マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //マウス旋回速度のSliderの最大値を設定
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxLookSensitivity;


        wasTrunLastFrame = false;
    }

    private void Update()
    {
        if (Player.instance.isFallDown) return;


        //Ctrl押下で視点が後ろを向く
        if(GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && Player.instance.playerIsBackRotate && !wasTrunLastFrame) 
        {
            //プレイヤーが後ろを向いている時はマウス感度を0にする
            lookSensitivity = 0f;
            mouseSensitivitySlider.value = 0f;

            //プレイヤーと一緒にカメラを180度回転させる
            Player.instance.PlayerTurn();
        }
        else if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame && !Player.instance.playerIsBackRotate && mouseSensitivitySlider.value == 0f && wasTrunLastFrame)
        {
            //プレイヤーが前を向いている時はマウス感度を元に戻す
            mouseSensitivitySlider.value = maxLookSensitivity / 2f;
        }

        wasTrunLastFrame = Player.instance.playerIsBackRotate;

        if (maxLookSensitivity <= mouseSensitivitySlider.value) mouseSensitivitySlider.value = maxLookSensitivity;

        lookSensitivity = mouseSensitivitySlider.value;

        if ( 0 < lookSensitivity && GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
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
        }
        


        xRotation -= lookY;

        xRotation -= lookY2;

        //視点の回転範囲を-A度からB度に制限する
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //回転角度を更新
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //このスクリプトの親オブジェクトを回転させる
        transform.parent.Rotate(Vector3.up * lookX);

        transform.parent.Rotate(Vector3.up * lookX2);
    }
}
