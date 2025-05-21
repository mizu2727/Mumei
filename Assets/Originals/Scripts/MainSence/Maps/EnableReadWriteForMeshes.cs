#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;



public class EnableReadWriteForMeshes
{
    //指定パス内のプレハブのメッシュの「Read/Write」を全て有効化するスクリプト
    //Unityのメニューから「Tools > Enable Read-Write for Stage Prefabs」を実行。

    [MenuItem("Tools/Enable Read-Write for Stage Prefabs")]
    static void EnableReadWrite()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Originals/Prefabs/MainSence/Maps/StagePrefabs" }); // プレハブフォルダを指定
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