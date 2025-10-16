using Unity.AI.Navigation;
using UnityEngine;

public class StageGround : MonoBehaviour
{
    public void Build(NavMeshSurface navMeshSurface)
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        //Null�`�F�b�N��ǉ����A�G���[�𕪂���₷�����܂�
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface��Inspector�Őݒ肳��Ă��܂���BNavMeshSurface�R���|�[�l���g���A�^�b�`���Ă��������I");
            return;
        }

        //NavMesh���\�z����
        navMeshSurface.BuildNavMesh();
    }
}
