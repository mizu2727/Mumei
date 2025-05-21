#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AddRoomTagToPrefabs
{
    //�w��p�X���̃v���n�u�ɁuRoom�v�^�O�������Œǉ�����X�N���v�g
    //Unity�̃��j���[����uTools > Add Room Tag to Stage Prefabs�v�����s�B



    [MenuItem("Tools/Add Room Tag to Stage Prefabs")]
    static void AddRoomTag()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Originals/Prefabs/MainSence/Maps/StagePrefabs" }); // �v���n�u�t�H���_���w��
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
                roomObj.transform.localScale = new Vector3(10, 1, 10); // �f�t�H���g�̕����T�C�Y
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