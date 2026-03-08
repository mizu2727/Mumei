using UnityEngine;
using UnityEngine.UI;

public class EnemyAttackScreen : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static EnemyAttackScreen instance;

    /// <summary>
    /// 敵がプレイヤーを襲う際のエフェクト画面を表示するフラグ
    /// </summary>
    private bool isViewEnemyAttackScreen;

    [Header("敵がプレイヤーを襲う際のエフェクト画面(ヒエラルキー上からアタッチする)")]
    [SerializeField] Image enemyAttackScreenImage;

    /// <summary>
    /// アルファ値の増加倍率
    /// </summary>
    private const float alphaMagnification = 0.1f;


    /// <summary>
    /// 敵がプレイヤーを襲う際のエフェクト画面を表示するフラグを取得
    /// </summary>
    /// <returns>敵がプレイヤーを襲う際のエフェクト画面を表示するフラグ</returns>
    public bool IsViewEnemyAttackScreen() 
    {
        return isViewEnemyAttackScreen;
    }

    /// <summary>
    /// 敵がプレイヤーを襲う際のエフェクト画面を表示するフラグを設定
    /// </summary>
    /// <param name="isView">敵がプレイヤーを襲う際のエフェクト画面を表示するフラグ</param>
    public void SetIsViewEnemyAttackScreen(bool isView) 
    {
        isViewEnemyAttackScreen = isView;
        enemyAttackScreenImage.gameObject.SetActive(isView);
    }

    private void Awake()
    {
        //シングルトンの設定
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //すでにインスタンスが存在する場合は破棄
            Destroy(gameObject);
        }

        //isViewEnemyAttackScreenを非表示
        SetIsViewEnemyAttackScreen(false);
    }

    void Update()
    {
        //敵がプレイヤーを襲う際のエフェクト画面を表示するフラグがtrueの場合
        if (isViewEnemyAttackScreen) 
        {
            //画面のアルファ値を徐々に増加させる
            Color color = enemyAttackScreenImage.color;
            color.a += Time.deltaTime * alphaMagnification;
            enemyAttackScreenImage.color = color;
        }
    }
}
