using Unity.AI.Navigation;
using UnityEngine;

public class StageGround : MonoBehaviour
{
    public void Build(NavMeshSurface navMeshSurface)
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        //Nullチェックを追加し、エラーを分かりやすくします
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurfaceがInspectorで設定されていません。NavMeshSurfaceコンポーネントをアタッチしてください！");
            return;
        }

        //NavMeshを構築する
        navMeshSurface.BuildNavMesh();
    }
}
