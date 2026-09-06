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


    private void Update()
    {
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
