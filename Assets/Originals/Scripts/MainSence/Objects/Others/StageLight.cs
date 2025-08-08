using UnityEngine;

public class StageLight : MonoBehaviour
{
    [Header("ステージライトが光っているかを判定(Inspector上で調整すること)")]
    [SerializeField] public bool isLitLight = false;

    [Header("ポイントライト(Prefabをアタッチすること)")]
    [SerializeField] Light lightPrefab;

    [Header("パーティクルシステム(Prefabをアタッチすること)")]
    [SerializeField] ParticleSystem particleSystemPrefab;

    void Start()
    {
        if (!isLitLight) 
        {
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }
    }

    /// <summary>
    ///     ステージ内のライトを点灯させる
    /// </summary>
    public void LitStageLight() 
    {
        if (!isLitLight)
        {
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);
            isLitLight = true;
        }
    }
}
