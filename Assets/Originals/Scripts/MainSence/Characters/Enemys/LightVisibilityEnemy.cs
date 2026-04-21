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
    /// ライトのRaycastHit情報
    /// </summary>
    RaycastHit lightHit;

    [Header("プレイヤーライト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform playerLight;

    /// <summary>
    /// プレイヤーライト発見フラグ
    /// </summary>
    protected bool isViewPlayerLight = false;

    [Header("ステージ内のオブジェクトライト(ヒエラルキー上からアタッチすること)")]
    [SerializeField] private Transform objectLight;

    /// <summary>
    /// オブジェクトライト発見フラグ
    /// </summary>
    protected bool isViewObjectLight = false;


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

        //フラッシュライトからレイを飛ばす
        if (Physics.Raycast(playerLight.position, playerLight.forward, out lightHit, Mathf.Infinity))
        { 

        }
    }
}
