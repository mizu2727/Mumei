using UnityEngine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    [Header("マウス感度")]
    [SerializeField]  public float mouseSensitivity = 100f;

    [Header("マウスの旋回速度のSlider(ヒエラルキー上からアタッチすること)")]
    [SerializeField] public Slider mouseSensitivitySlider;

    [Header("マウス感度最大値")]
    [SerializeField] float maxMouseSensitivity = 500f;

    //マウスの横移動
    float mouseX;

    //マウスの縦移動
    float mouseY;

    [Header("カメラのX軸回転角度")]
    private float xRotation = 0f;//カメラのX軸回転角度

    [Header("カメラのX軸回転範囲")]
    [SerializeField] private float xRotationRange = 45f ;



    private void Start()
    {
        // マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //マウス旋回速度のSliderの最大値を設定
        if (mouseSensitivitySlider) mouseSensitivitySlider.maxValue = maxMouseSensitivity;
    }

    private void Update()
    {
        
        if (maxMouseSensitivity <= mouseSensitivitySlider.value) mouseSensitivitySlider.value = maxMouseSensitivity;

        mouseSensitivity = mouseSensitivitySlider.value;

        if ( 0 < mouseSensitivity) 
        {
            //マウスの移動
            mouseX =
                Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY =
                Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        }
        


        xRotation -= mouseY;

        //視点の回転範囲を-A度からB度に制限する
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //回転角度を更新
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //このスクリプトの親オブジェクトを回転させる
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
