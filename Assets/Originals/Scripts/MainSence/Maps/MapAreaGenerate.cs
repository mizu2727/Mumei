
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapAreaGenerate : MonoBehaviour
{
    [Header("マップエリアを格納する(ヒエラルキー上のマップエリアをアタッチすること)")]
    [SerializeField] private List <GameObject> areaPrefabList;

    [Header("ランダムに選ばれたマップエリアが格納される(アタッチ禁止)")]
    public List<GameObject> useList = new ();


    [Header("マップエリア生成地点のTransform配列(ヒエラルキー上のマップエリア生成地点をアタッチすること)")]
    [SerializeField] private Transform[] areaPoint;


    void Start()
    {
        // areaPrefabListのコピーを作成
        List<GameObject> shuffledPrefabList = new List<GameObject>(areaPrefabList);

        // areaPrefabListをシャッフル
        ShuffleList(shuffledPrefabList);

        // シャッフルされたリストを格納
        useList.AddRange(shuffledPrefabList);


        for (int i = 0; i < useList.Count; i++)
        {
            useList[i].transform.position = areaPoint[i].position;

            Debug.Log("GameObject " + i + ": " + useList[i].name);
        }
    }

    /// <summary>
    /// リストの要素をランダムにシャッフルするメソッド
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
