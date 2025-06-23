using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    void Start()
    {
        // �q�I�u�W�F�N�g��MeshFilter���擾
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false); // ���̃��b�V�����\��
        }

        // �V�������b�V�����쐬���Č���
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh = combinedMesh;
    }
}