using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]  public float mouseSensitivity = 100f;//マウス感度
    private float xRotation = 0f;//カメラのX軸回転角度

    void Start()
    {
        // マウスカーソルを非表示にし、位置を固定
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //マウスの移動
        float mouseX = 
            Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = 
            Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;


        xRotation -= mouseY;

        //視点の回転範囲を-90度から90度に制限する
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //回転角度を更新
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //このスクリプトの親オブジェクトを回転させる
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
