using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Compass instance;


    [Header("コンパスの針の画像(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Image compassArrowImage;

    [Header("プレイヤー(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Transform player;

    [Header("ゴール(ヒエラルキー上からアタッチする必要がある)")]
    [SerializeField] private Transform goal;

    /// <summary>
    /// RectTransform
    /// </summary>
    RectTransform rectTransform_;

    [Header("針のオフセット角度（度）(デフォルトでは北が0度)")]
    [SerializeField] private float needleOffsetAngle = 0f;

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
    }

    void Start()
    {
        rectTransform_ = compassArrowImage.rectTransform;
    }

    void Update()
    {
        //プレイヤーまたはゴールが設定されていない場合
        if (player == null || goal == null)
        {
            //処理をスキップ
            return;
        }

        //プレイヤー→ゴール方向
        Vector3 dir = goal.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        //ゴール方向の絶対角度
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        //プレイヤーの向き
        float playerAngle = player.eulerAngles.y;

        //相対角度（コンパス針が向くべき角度）
        float relativeAngle = targetAngle - playerAngle;

        //UI針を回転
        rectTransform_.rotation = Quaternion.Euler(0f, 0f, -relativeAngle + needleOffsetAngle);

    }
}
