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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        
    }

    public void WarpPostion01() 
    {
        transform.position = new Vector3(1, 0.505f, 7);
    }

    public void WarpPostion02()
    {
        Debug.Log("WarpPosition02 Ç™åƒÇ—èoÇ≥ÇÍÇ‹ÇµÇΩÅB");
        transform.position = new Vector3(1, 0.505f, 2);
    }
}
