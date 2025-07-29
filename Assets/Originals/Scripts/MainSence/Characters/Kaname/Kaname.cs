using UnityEngine;
using UnityEngine.UI;

public class Kaname : MonoBehaviour
{
    public static Kaname instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        
    }

    public void WarpPostion(float x, float y, float z) 
    {
        transform.position = new Vector3(x, y, z);
    }
}
