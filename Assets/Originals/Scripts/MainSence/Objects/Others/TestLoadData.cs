using UnityEngine;

/// <summary>
/// �f�[�^���Z�[�u�E���[�h�E���Z�b�g����N���X(�e�X�g�p)
/// </summary>
public class TestLoadData : MonoBehaviour
{
  

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            //�Z�[�u
            GameController.instance.CallSaveUserDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //���Z�b�g
            GameController.instance.CallRestDataMethod();
        }

        if (Input.GetKeyDown(KeyCode.L)) 
        {
            //���[�h
            GameController.instance.CallLoadUserDataMethod();
        }
    }
}
