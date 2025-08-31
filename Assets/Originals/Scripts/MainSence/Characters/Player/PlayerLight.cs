using Unity.VisualScripting;
using UnityEngine;
using static GameController;

public class PlayerLight : MonoBehaviour
{
    [Header("プレイヤーカメラ(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform cameraTransform;

    [Header("ライト(ヒエラルキー上からアタッチすること)")]
    public GameObject playerHasLight;

    private void Start()
    {
        playerHasLight.SetActive(false);
        Player.instance.IsLight = false;
    }

    void Update()
    {
        TranceCamera();

        TurnOnAndOfLight();


    }

    //F・1キーでライト切り替え
    //Light…"joystick button 0"を割り当て。コントローラーではAボタンになる
    bool PlayerIsLight()
    {
        return Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1) || Input.GetButtonDown("Light");
    }

    void TranceCamera()
    {
        //座標追従
        this.transform.position = cameraTransform.position;
        //角度追従
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, cameraTransform.rotation, 0.5f); 
    }

    void TurnOnAndOfLight() 
    {
        if (GameController.instance.gameModeStatus == GameModeStatus.PlayInGame) 
        {
            if (PlayerIsLight() && !Player.instance.IsLight && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                // ライトをアクティブ状態にする・・・＞ライトが点く
                playerHasLight.SetActive(true);
                Player.instance.IsLight = true;
            }
            else if ((PlayerIsLight() && Player.instance.IsLight) && !PauseController.instance.isPause && Time.timeScale != 0)
            {
                // ライトをノン・アクティブ状態にする・・・＞ライトが消える
                playerHasLight.SetActive(false);
                Player.instance.IsLight = false;
            }
        } 
    }
}


