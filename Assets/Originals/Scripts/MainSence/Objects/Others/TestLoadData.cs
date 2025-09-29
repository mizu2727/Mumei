using UnityEngine;

/// <summary>
/// データをセーブ・ロード・リセットするクラス(テスト用)
/// </summary>
public class TestLoadData : MonoBehaviour
{
  

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            //セーブ
            GameController.instance.CallSaveUserDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //リセット
            GameController.instance.CallRestDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.L)) 
        {
            //ロード
            GameController.instance.CallLoadUserDataMethod();
        }
    }
}
