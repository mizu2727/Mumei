using UnityEngine;

public class OutsideOfDrawer : MonoBehaviour
{
    /// <summary>
    /// オイラー角
    /// </summary>
    private Vector3 worldEulerAngles;

    /// <summary>
    /// オイラー角を取得するメソッド
    /// </summary>
    /// <returns>オイラー角</returns>
    public Vector3 GetWorldEulerAngles() 
    {
        return worldEulerAngles;
    }

    private void Awake()
    {
        //オイラー角を取得して保存する
        worldEulerAngles = transform.rotation.eulerAngles;
    }

}
