using UnityEngine;
using UnityEngine.UI;

public class Kaname : MonoBehaviour
{
    /// <summary>
    /// �C���X�^���X
    /// </summary>
    public static Kaname instance;

    private void Awake()
    {
        //�C���X�^���X����
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
    /// ���[�v
    /// </summary>
    /// <param name="x">���[�v���X���W</param>
    /// <param name="y">���[�v���Y���W</param>
    /// <param name="z">���[�v���Z���W</param>
    public void WarpPostion(float x, float y, float z) 
    {
        transform.position = new Vector3(x, y, z);
    }
}
