using UnityEngine;

interface CharacterInterface
{
    /// <summary>
    /// アニメーション再生
    /// </summary>
    Animator PlayAnimator { get; set; }

    /// <summary>
    /// キャラクター名
    /// </summary>
    string CharacterName { get; set; }

    /// <summary>
    /// 通常の移動速度
    /// </summary>
    float NormalSpeed { get; set; }

    /// <summary>
    /// ダッシュ時の移動速度
    /// </summary>
    float SprintSpeed { get; set; }

    /// <summary>
    /// キャラクター検出範囲
    /// </summary>
    float DetectionRange { get; set; }

    /// <summary>
    /// 重力
    /// </summary>
    float Gravity { get; set; }
    
    /// <summary>
    /// HP
    /// </summary>
    int HP { get; set; }

    /// <summary>
    /// 死亡フラグ
    /// </summary>
    bool IsDead { get; set; }

    /// <summary>
    /// 死亡メソッド
    /// </summary>
    void Dead();

    /// <summary>
    /// 移動判定
    /// </summary>
    bool IsMove { get; set; }

    /// <summary>
    /// ダッシュ判定
    /// </summary>
    bool IsDash { get; set; }

    /// <summary>
    /// 後ろを向くフラグ
    /// </summary>
    bool IsBackRotate { get; set; }

    /// <summary>
    /// ライトの切り替えフラグ
    /// </summary>
    bool IsLight { get; set; }

    /// <summary>
    /// 攻撃するメソッド
    /// </summary>
    void Attack();

    /// <summary>
    /// キャラクターの初期位置
    /// </summary>
    Vector3 StartPosition { get; set; }
}
