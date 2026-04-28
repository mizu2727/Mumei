using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static GameController;

/// <summary>
/// 周囲の特定の光を検知できる敵クラス
/// </summary>
public class LightVisibilityEnemy : BaseEnemy
{
    /// <summary>
    /// 敵の視線のRaycastHit情報
    /// </summary>
    RaycastHit seeTheLightHit;

    [Header("敵の視線を飛ばしているオブジェクト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform viewLightObject;

    /// <summary>
    /// プレイヤーライトのRaycastHit情報
    /// </summary>
    RaycastHit playerlightHit;

    /// <summary>
    /// プレイヤータグ
    /// </summary>
    private string playerTag = "Player";

    [Header("プレイヤーライト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform playerLight;

    /// <summary>
    /// プレイヤーライト発見フラグ
    /// </summary>
    protected bool isViewPlayerLight = false;

    /// <summary>
    /// オブジェクトライトのRaycastHit情報
    /// </summary>
    RaycastHit objectlightHit;

    /// <summary>
    /// オブジェクトタグ
    /// </summary>
    private string otherStageObjectTag = "OtherStageObject";

    [Header("ステージ内のオブジェクトライト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform objectLight;

    /// <summary>
    /// オブジェクトライト発見フラグ
    /// </summary>
    protected bool isViewObjectLight = false;

    /// <summary>
    /// 最後に発見した光の位置
    /// </summary>
    private Vector3 lastViewLightPosition;

    /// <summary>
    /// 光を調査するフラグ
    /// </summary>
    private bool isInvestigatingLight = false;

    /// <summary>
    /// 調査時間カウンター
    /// </summary>
    private float lightInvestigateTimer = 0f;


    protected override async void Update() 
    {
        //ゲームがプレイ中でない、またはプレイヤーが死亡している場合
        if (GameController.instance.gameModeStatus != GameModeStatus.PlayInGame || Player.instance == null
            || Player.instance.IsDead || targetPoint == null)
        {
            navMeshAgent.isStopped = true;

            //処理をスキップ
            return;
        }

        //フラッシュライトから飛ばしているRaycastにオブジェクトが当たった場合&&オブジェクトライトが見えていない場合
        if (Physics.Raycast(playerLight.position, playerLight.forward, out playerlightHit, Mathf.Infinity) 
            && Player.instance.IsLight && !isViewObjectLight)
        {
            //敵の顔から1つ目のRaycastの当たった場所への方向
            Vector3 headToPlayerLightHitDir = Vector3.Normalize(playerlightHit.point - viewLightObject.position);

            //Debug.DrawRay(viewLightObject.position, headToPlayerLightHitDir * 10, Color.red, 0.1f);
            //Debug.Log("テスト001:フラッシュライトから飛ばしているRaycastにオブジェクトが当たった");

            

            //フラッシュライトからのRaycastが当たったオブジェクトに敵のRaycastが当たった場合
            if (Physics.Raycast(viewLightObject.position, headToPlayerLightHitDir, out seeTheLightHit, Mathf.Infinity))
            {
                //Debug.DrawRay(viewLightObject.position, headToPlayerLightHitDir * 10, Color.blue, 0.1f);
                //Debug.Log("テスト002:敵のRaycastが当たった場合");


                

                // ライトと視線が同じ場所に当たった場合（距離が近い場合も含む）
                //if (playerlightHit.point == seeTheLightHit.point)に近い考え方である
                if (Vector3.Distance(playerlightHit.point, seeTheLightHit.point) < 0.1f)
                {
                    //プレイヤーライト発見フラグをオンにする
                    isViewPlayerLight = true;

                    //追従モード以外の場合
                    if (currentState != EnemyState.Chase)
                    {
                        //ノイズ画面を表示
                        noiseScreenPanel.SetActive(true);
                        Debug.Log("テスト002.1:光を検知したため、ノイズ画面を表示");
                    }

                    // 敵の顔からライトが当たった場所への方向と、敵の顔からライトが当たった場所への方向の内積を計算
                    float dot = Vector3.Dot(headToPlayerLightHitDir, viewLightObject.forward);
                    Debug.Log("テスト003:ライトと視線が同じ場所に当たった場合");

                    //光の位置を記録
                    lastViewLightPosition = playerLight.position;
                    isInvestigatingLight = true;
                    lightInvestigateTimer = 0f;

                    //光を検知した場合、調査状態に移行
                    currentState = EnemyState.Investigate;

                    //光の位置へ移動
                    navMeshAgent.SetDestination(lastViewLightPosition);

                    //内積が正の場合、ライトが敵の視線の前方にあることを意味する
                    if (dot > 0)
                    {
                        Debug.Log("テスト004:ライトが見えている");
                        currentState = EnemyState.Chase;

                        
                    }
                    else
                    {
                        //ライトが見えていない
                        //ノイズ画面を非表示
                        noiseScreenPanel.SetActive(false);

                        //プレイヤーライト発見フラグをオフにする
                        //isViewPlayerLight = false;
                    }

                    Debug.Log("isViewPlayerLight = " + isViewPlayerLight);
                }
                else
                {
                    //ライトが見えていない
                    //ノイズ画面を非表示
                    noiseScreenPanel.SetActive(false);

                    //プレイヤーライト発見フラグをオフにする
                    isViewPlayerLight = false;
                }
            }
            else
            {
                //ライトが見えていない
                //ノイズ画面を非表示
                noiseScreenPanel.SetActive(false);

                //プレイヤーライト発見フラグをオフにする
                isViewPlayerLight = false;
            }
        }
        else
        {
            //ライトが見えていない
            //ノイズ画面を非表示
            noiseScreenPanel.SetActive(false);

            //プレイヤーライト発見フラグをオフにする
            isViewPlayerLight = false;
        }

        if (objectLight != null) 
        {
            //オブジェクトのライトから飛ばしているRaycastにオブジェクトが当たった場合&&プレイヤーのライトが見えていない場合
            if (Physics.Raycast(objectLight.position, objectLight.forward, out objectlightHit, Mathf.Infinity) && !isViewPlayerLight)
            {
                //敵の顔から1つ目のRaycastの当たった場所への方向
                Vector3 headToObjectLightHitDir = Vector3.Normalize(objectlightHit.point - viewLightObject.position);

                Debug.DrawRay(viewLightObject.position, headToObjectLightHitDir * 10, Color.red, 0.1f);
                //Debug.Log("テスト005:オブジェクトのライトから飛ばしているRaycastにオブジェクトが当たった");

                //オブジェクトのライトからのRaycastが当たったオブジェクトに敵のRaycastが当たった場合
                if (Physics.Raycast(viewLightObject.position, headToObjectLightHitDir, out seeTheLightHit, Mathf.Infinity))
                {
                    //Debug.DrawRay(viewLightObject.position, headToObjectLightHitDir * 10, Color.blue, 0.1f);
                    //Debug.Log("テスト006:敵のRaycastが当たった場合");

                    //オブジェクトのライトと視線が同じ場所に当たった場合（距離が近い場合も含む）
                    //if (lightHit.point == seeTheLightHit.point)に近い考え方である
                    if (Vector3.Distance(objectlightHit.point, seeTheLightHit.point) < 0.1f)
                    {
                        //オブジェクトライト発見フラグをオンにする
                        isViewObjectLight = true;

                        //追従モード以外の場合
                        if (currentState != EnemyState.Chase)
                        {
                            //ノイズ画面を表示
                            Debug.Log("テスト007:オブジェクトの光を検知");
                        }

                        //敵の顔からライトが当たった場所への方向と、敵の顔からライトが当たった場所への方向の内積を計算
                        float dot = Vector3.Dot(headToObjectLightHitDir, viewLightObject.forward);
                        Debug.Log("テスト008:オブジェクトのライトと視線が同じ場所に当たった場合");

                        //光の位置を記録
                        lastViewLightPosition = objectLight.position;
                        isInvestigatingLight = true;
                        lightInvestigateTimer = 0f;

                        //光を検知した場合、調査状態に移行
                        currentState = EnemyState.Investigate;

                        //光の位置へ移動
                        navMeshAgent.SetDestination(lastViewLightPosition);

                        //内積が正の場合、ライトが敵の視線の前方にあることを意味する
                        if (dot > 0)
                        {
                            Debug.Log("テスト009:オブジェクトのライトが見えている");
                        }
                    }
                    else
                    {
                        //オブジェクトライト発見フラグをオフにする
                        isViewObjectLight = false;
                    }
                }
                else
                {
                    //オブジェクトライト発見フラグをオフにする
                    isViewObjectLight = false;
                }
            }
            else
            {
                //オブジェクトライト発見フラグをオフにする
                isViewObjectLight = false;
            }
        }
        else
        {
            //オブジェクトライト発見フラグをオフにする
            isViewObjectLight = false;
        }


        //BaseEnemy.csのUpdate関数を呼び出す
        base.Update();
    }
}
