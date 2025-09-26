using UnityEngine;
using UnityEngine.UI;

public class Kaname : MonoBehaviour
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static Kaname instance;

    private void Awake()
    {
        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ワープ
    /// </summary>
    /// <param name="x">ワープ先のX座標</param>
    /// <param name="y">ワープ先のY座標</param>
    /// <param name="z">ワープ先のZ座標</param>
    public void WarpPostion(float x, float y, float z) 
    {
        transform.position = new Vector3(x, y, z);
    }
}
