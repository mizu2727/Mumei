using UnityEngine;
using UnityEngine.SceneManagement;

public class TestWarp01 : MonoBehaviour
{
    [SerializeField] public Vector3 testWarpPos;
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SceneManager.LoadScene(targetSceneName);
            Player.instance.transform.position = testWarpPos;
            Debug.Log("testWarpPos:" + testWarpPos);
        }
    }
}
