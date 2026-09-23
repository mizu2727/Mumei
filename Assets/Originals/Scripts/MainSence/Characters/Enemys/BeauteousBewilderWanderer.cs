using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 微美しき魅忘の彷徨う者
/// </summary>
public class BeauteousBewilderWanderer : LightVisibilityEnemy
{
    [Header("通常形態")]
    [SerializeField] private GameObject normalModel;

    [Header("クリーチャー形態")]
    [SerializeField] private GameObject creatureModel;

    /// <summary>
    /// 各モデルの初期ローカル位置・回転(親に追従させるための基準値)
    /// </summary>
    private Vector3 normalModelLocalPosition;
    private Quaternion normalModelLocalRotation;
    private Vector3 creatureModelLocalPosition;
    private Quaternion creatureModelLocalRotation;

    /// <summary>
    /// 現在クリーチャー形態かどうか(null = 未初期化)
    /// </summary>
    private bool? isCreatureForm = null;

    private void Awake()
    {
        //各モデルが「親とは別に独立して動く」原因になるコンポーネントを無効化
        DisableIndependentMovement(normalModel);
        DisableIndependentMovement(creatureModel);

        //初期ローカル位置・回転を記録
        if (normalModel != null)
        {
            normalModelLocalPosition = normalModel.transform.localPosition;
            normalModelLocalRotation = normalModel.transform.localRotation;
        }
        if (creatureModel != null)
        {
            creatureModelLocalPosition = creatureModel.transform.localPosition;
            creatureModelLocalRotation = creatureModel.transform.localRotation;
        }
    }

    /// <summary>
    /// 子モデル側に付いている NavMeshAgent / Rigidbody / Collider / 敵スクリプト等を無効化し、
    /// 親(このスクリプトが付いている空のObject)の移動にだけ追従するようにする
    /// </summary>
    private void DisableIndependentMovement(GameObject model)
    {
        if (model == null) return;

        //NavMeshAgent:ワールド座標を直接書き換えるため、子に付いていると親に追従しない
        foreach (NavMeshAgent agent in model.GetComponentsInChildren<NavMeshAgent>(true))
        {
            agent.enabled = false;
        }

        //Rigidbody:非Kinematicだと物理演算で独立して動くため、親に追従しない
        foreach (Rigidbody rb in model.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        //Collider:親のCapsuleColliderと衝突して押し出されるのを防ぐ
        foreach (Collider col in model.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        //敵スクリプト:子にも付いていると、子が独自にNavMeshで動こうとする
        foreach (BaseEnemy enemy in model.GetComponentsInChildren<BaseEnemy>(true))
        {
            if (enemy != this) enemy.enabled = false;
        }

        //子のAnimator:Root Motionで位置が動かないようにする
        foreach (Animator anim in model.GetComponentsInChildren<Animator>(true))
        {
            anim.applyRootMotion = false;
        }
    }


    /// <summary>
    /// 「private void Update()」の場合、 override ではなく「隠蔽(hide)」になってしまい、
    /// 継承元（LightVisibilityEnemy → BaseEnemy）のUpdate()が呼ばれなくなる。
    /// </summary>
    protected override void Update()
    {
        base.Update();

        //調査状態の場合||追跡状態の場合 → クリーチャー形態
        bool shouldBeCreature = currentState == EnemyState.Investigate || currentState == EnemyState.Chase;

        //形態が変わった時だけ切り替える(毎フレームSetActiveしない)
        if (isCreatureForm != shouldBeCreature)
        {
            isCreatureForm = shouldBeCreature;
            normalModel.SetActive(!shouldBeCreature);
            creatureModel.SetActive(shouldBeCreature);
        }
    }

    /// <summary>
    /// アニメーション・物理の後に、モデルのローカル位置を親基準に戻す
    /// (アニメーションや物理でモデルがずれても、必ず親と一緒に移動させる)
    /// </summary>
    private void LateUpdate()
    {
        if (normalModel != null)
        {
            normalModel.transform.localPosition = normalModelLocalPosition;
            normalModel.transform.localRotation = normalModelLocalRotation;
        }
        if (creatureModel != null)
        {
            creatureModel.transform.localPosition = creatureModelLocalPosition;
            creatureModel.transform.localRotation = creatureModelLocalRotation;
        }
    }
}
