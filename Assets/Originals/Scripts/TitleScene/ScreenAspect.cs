using UnityEngine;

public class ScreenAspect : MonoBehaviour
{
    private void Awake()
    {
        // ウィンドウモードに設定し、サイズ変更を許可
        Screen.SetResolution(1920, 1080, true);
    }
}
