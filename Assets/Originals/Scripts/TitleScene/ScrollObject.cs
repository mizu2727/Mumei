using UnityEngine;

public class ScrollObject : MonoBehaviour
{
    [Header("スクロール開始地点")]
    public float startPosition;

    [Header("スクロール終了地点")]
    public float endPosition;

    [Header("スクロール速度")]
    public float speed;

    [Header("スクロールの向きの判定(isDirectionがtrueなら右向き、falseなら左向き)")]
    public bool isDirection;

    //スクロールの向きの決定
    private float direction = 1;

    private void Start()
    {
        if (!isDirection) direction = -1;
    }

    void Update()
    {
        //オブジェクトを指定の速度と向きでスクロールさせる
        transform.Translate(speed * Time.deltaTime * direction, 0, 0);

        //オブジェクトが終了地点までスクロールしたかを判定
        if (transform.position.x <= endPosition) Scroll();
    }

    //オブジェクトの位置を開始地点へ指定する
    void Scroll()
    {
        float difference = transform.position.x - endPosition;
        Vector3 resetPosition = transform.position;
        resetPosition.x = startPosition + difference;
        transform.position = resetPosition;
    }
}