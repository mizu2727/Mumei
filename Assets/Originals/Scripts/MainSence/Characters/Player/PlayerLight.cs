using Unity.VisualScripting;
using UnityEngine;

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

    //右クリックでライト切り替え
    bool PlayerIsLight()
    {
        return Input.GetMouseButtonDown(1);
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
        if (PlayerIsLight() && !Player.instance.IsLight)
        {
            // ライトをアクティブ状態にする・・・＞ライトが点く
            playerHasLight.SetActive(true);
            Player.instance.IsLight = true;
            Debug.Log("ライト点灯");
        }
        else if ((PlayerIsLight() && Player.instance.IsLight))
        {
            // ライトをノン・アクティブ状態にする・・・＞ライトが消える
            playerHasLight.SetActive(false);
            Player.instance.IsLight = false;
            Debug.Log("ライト消灯");
        }
    }
}


