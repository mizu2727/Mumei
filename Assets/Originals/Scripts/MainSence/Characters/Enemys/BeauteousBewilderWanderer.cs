using UnityEngine;

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
    /// 「private void Update()」の場合、 override ではなく「隠蔽(hide)」になってしまい、
    /// 継承元（LightVisibilityEnemy → BaseEnemy）のUpdate()が呼ばれなくなる。
    /// </summary>
    protected override void Update()
    {
        base.Update();

        //調査状態の場合||追跡状態の場合
        if (currentState == EnemyState.Investigate || currentState == EnemyState.Chase)
        {
            //クリーチャー形態を表示
            normalModel.SetActive(false);
            creatureModel.SetActive(true);
            
        }
        else
        {
            //通常形態を表示
            normalModel.SetActive(true);
            creatureModel.SetActive(false);
        }
    }
}
