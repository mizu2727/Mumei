using UnityEngine;

public class StageLight : MonoBehaviour
{
    [Header("ステージライトフラグ(Inspector上で調整すること)")]
    [SerializeField] public bool isLitLight = false;

    [Header("ポイントライト(Prefabをアタッチすること)")]
    [SerializeField] Light lightPrefab;

    [Header("パーティクルシステム(Prefabをアタッチすること)")]
    [SerializeField] ParticleSystem particleSystemPrefab;

    void Start()
    {
        //ステージライトフラグがオフの場合
        if (!isLitLight) 
        {
            //ステージライトを消灯させる
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ステージ内のライトを点灯させる
    /// </summary>
    public void LitStageLight() 
    {
        //ステージライトフラグがオフの場合
        if (!isLitLight)
        {
            //ステージライトを点灯させる
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);

            //フラグ値をオン
            isLitLight = true;
        }
    }
}
