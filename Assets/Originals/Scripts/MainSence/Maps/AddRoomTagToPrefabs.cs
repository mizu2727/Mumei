#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AddRoomTagToPrefabs
{
    //指定パス内のプレハブに「Room」タグを自動で追加するスクリプト
    //Unityのメニューから「Tools > Add Room Tag to Stage Prefabs」を実行。



    [MenuItem("Tools/Add Room Tag to Stage Prefabs")]
    static void AddRoomTag()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Originals/Prefabs/MainSence/Maps/StagePrefabs" }); // プレハブフォルダを指定
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject roomObj = prefab.transform.Find("RoomCenter")?.gameObject;
            if (roomObj == null)
            {
                roomObj = new GameObject("RoomCenter");
                roomObj.transform.SetParent(prefab.transform, false);
                roomObj.transform.localPosition = Vector3.zero;
                roomObj.transform.localScale = new Vector3(10, 1, 10); // デフォルトの部屋サイズ
            }
            roomObj.tag = "Room";
            EditorUtility.SetDirty(prefab);
            Debug.Log($"Added Room tag to {path}");
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
#endif