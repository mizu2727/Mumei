using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    void Start()
    {
        // 子オブジェクトのMeshFilterを取得
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false); // 元のメッシュを非表示
        }

        // 新しいメッシュを作成して結合
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }
}