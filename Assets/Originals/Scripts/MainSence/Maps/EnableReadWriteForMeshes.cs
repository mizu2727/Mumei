#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;



public class EnableReadWriteForMeshes
{
    //�w��p�X���̃v���n�u�̃��b�V���́uRead/Write�v��S�ėL��������X�N���v�g
    //Unity�̃��j���[����uTools > Enable Read-Write for Stage Prefabs�v�����s�B

    [MenuItem("Tools/Enable Read-Write for Stage Prefabs")]
    static void EnableReadWrite()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Originals/Prefabs/MainSence/Maps/StagePrefabs" }); // �v���n�u�t�H���_���w��
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            foreach (MeshFilter meshFilter in prefab.GetComponentsInChildren<MeshFilter>())
            {
                Mesh mesh = meshFilter.sharedMesh;
                if (mesh != null && !mesh.isReadable)
                {
                    ModelImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(mesh)) as ModelImporter;
                    if (importer != null)
                    {
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                        Debug.Log($"Enabled Read/Write for mesh: {mesh.name} in {path}");
                    }
                }
            }
        }
        AssetDatabase.Refresh();
    }
}
#endif