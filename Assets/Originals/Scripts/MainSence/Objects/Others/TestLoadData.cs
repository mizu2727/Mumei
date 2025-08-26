using UnityEngine;

public class TestLoadData : MonoBehaviour
{
  

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            GameController.instance.CallSaveUserDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            GameController.instance.CallRestDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.L)) 
        {
            GameController.instance.CallLoadUserDataMethod();
        }
    }
}
