using UnityEngine;

interface CharacterInterface
{
    //アニメーション再生
    Animator PlayAnimator { get; set; }

    //名前
    string CharacterName { get; set; }

    //通常の移動速度
    float NormalSpeed { get; set; }

    //ダッシュ時の移動速度
    float SprintSpeed { get; set; }

    //キャラクター検出範囲
    float DetectionRange { get; set; }

    //重力
    float Gravity { get; set; }

    //HP
    int HP { get; set; }

    //死亡判定
    bool IsDead { get; set; }

    //死亡
    void Dead();

    //移動判定
    bool IsMove { get; set; }

    //ダッシュ判定
    bool IsDash { get; set; }

    //後ろを向く判定医
    bool IsBackRotate { get; set; }

    //ライトの切り替え
    bool IsLight { get; set; }

    //攻撃
    void Attack();

    //キャラクターの初期位置
    Vector3 StartPosition { get; set; }
}
