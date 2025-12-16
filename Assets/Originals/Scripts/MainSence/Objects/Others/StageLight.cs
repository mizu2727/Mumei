using UnityEngine;

/// <summary>
/// ステージライトクラス
/// </summary>
public class StageLight : MonoBehaviour
{
    [Header("ステージライトフラグ(Inspector上で調整すること)")]
    [SerializeField] public bool isLitLight = false;

    [Header("ポイントライト(Prefabをアタッチすること)")]
    [SerializeField] Light lightPrefab;

    [Header("パーティクルシステム(Prefabをアタッチすること)")]
    [SerializeField] ParticleSystem particleSystemPrefab;

    /// <summary>
    /// 敵との距離
    /// </summary>
    private float[] distanceArray;

    /// <summary>
    /// 点滅開始範囲(敵との距離)
    /// </summary>
    private float blinkingRange = 30f;

    /// <summary>
    /// ステージライト点滅フラグ
    /// </summary>
    private bool isBlinkingLightFlag = false;

    /// <summary>
    /// ステージライト点滅切り替え時間
    /// </summary>
    private const float changeBlinkTimer = 1.0f;

    /// <summary>
    /// ステージライト点滅時間
    /// </summary>
    private float blinkTimer = 0f;

    void Start()
    {
        //ステージライトフラグがオフの場合
        if (!isLitLight) 
        {
            //ステージライトを消灯させる
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }

        //ステージライト点滅フラグをオフにする
        isBlinkingLightFlag = false;

        //ステージライト点滅時間を初期化
        blinkTimer = 0.0f;

        //MapAreaGenerateのインスタンスが存在する場合
        if (MapAreaGenerate.instance != null) 
        {
            //敵との距離配列を初期化
            distanceArray = new float[MapAreaGenerate.instance.baseEnemyTransformArray.Length];
        }

        
    }


    private void Update()
    {
        //ポーズ中||MapAreaGenerateのインスタンスが存在しない場合
        if (Time.timeScale == 0 || MapAreaGenerate.instance == null)
        {
            //処理をスキップ
            return;
        }

        for (int i = 0; i < MapAreaGenerate.instance.baseEnemyTransformArray.Length; i++)
        {
            //敵との距離を測定
            distanceArray[i] = Vector3.Distance(MapAreaGenerate.instance.baseEnemyTransformArray[i].position, transform.position);


            //距離が点滅開始範囲以内の場合
            if (distanceArray[i] <= blinkingRange)
            {
                //ステージライト点滅フラグをオンにする
                isBlinkingLightFlag = true;
            }
            else 
            {
                //ステージライト点滅フラグをオフにする
                isBlinkingLightFlag = false;
            }
        }

        //ステージライトをプレイヤーがまだ点灯させていない場合
        if (!isLitLight) 
        {
            //処理をスキップ
            return;
        }

        //ステージライト点滅フラグがオフの場合
        if (!isBlinkingLightFlag) 
        {
            //ステージライトをそのまま点灯させた状態にする
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);

            //処理をスキップ
            return;
        }

        

        //点滅切り替え時間を超えていない場合
        if (blinkTimer < changeBlinkTimer) 
        {
            //点滅時間を加算
            blinkTimer += Time.deltaTime;

            //処理をスキップ
            return;
        }

        //ステージライト点滅処理
        BlinkingLight();
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

    /// <summary>
    /// ステージライト点滅処理
    /// </summary>
    private void BlinkingLight() 
    {
        //ステージライトが点灯している場合
        //(ステージライトフラグはプレイヤーが触れた場合のみに使用するため、プレハブのactiveSelfで判定する)
        if (lightPrefab.gameObject.activeSelf)
        {
            //ステージライトを消灯させる
            lightPrefab.gameObject.SetActive(false);
            particleSystemPrefab.gameObject.SetActive(false);
        }
        else 
        {
            //ステージライトを点灯させる
            lightPrefab.gameObject.SetActive(true);
            particleSystemPrefab.gameObject.SetActive(true);
        }

        //ステージライト点滅時間を初期化
        blinkTimer = 0.0f;
    }
}
