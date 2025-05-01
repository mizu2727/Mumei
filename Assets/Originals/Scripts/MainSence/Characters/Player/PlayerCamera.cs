using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("マウス感度")]
    [SerializeField]  public float mouseSensitivity = 100f;

    [Header("カメラのX軸回転角度")]
    private float xRotation = 0f;//カメラのX軸回転角度

    [Header("カメラのX軸回転範囲")]
    [SerializeField] private float xRotationRange = 90f ;

    private void Start()
    {
        // マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        //マウスの移動
        float mouseX = 
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        xRotation -= mouseY;

        //視点の回転範囲を-A度からB度に制限する
        xRotation = Mathf.Clamp(xRotation, -xRotationRange, xRotationRange);

        //回転角度を更新
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //このスクリプトの親オブジェクトを回転させる
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
