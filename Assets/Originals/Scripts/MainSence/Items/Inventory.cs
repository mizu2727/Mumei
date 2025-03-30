using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //　アイテムID管理
    private readonly List<int> idList = new();

    //　アイテム所持数管理
    private readonly List<int> countList = new();

    void Start()
    {

    }
    void Update()
    {

    }

    //インベントリにアイテムを追加
    public void GetItem(int id, int count)
    {

        //リストの中にアイテムが何番目に存在するのかを確認
        //存在しない場合は-1を返す
        int checkIndex = idList.IndexOf(id);

        //インベントリに新規追加する処理
        if (checkIndex == -1)
        {
            //　アイテムidを設定
            idList.Add(id);

            //　アイテム所持数を設定
            countList.Add(count);

            Debug.Log("id:" + id + "のアイテムを" + count + "個新規追加");
        }
        //アイテム所持数を追加
        else
        {
            countList[checkIndex] += count;

            Debug.Log("id:" + id + "のアイテムを" + count + "個増加");
        }
    }

    public void GetDocument(int id, int count)
    {

        //リストの中にアイテムが何番目に存在するのかを確認
        //存在しない場合は-1を返す
        int checkIndex = idList.IndexOf(id);

        //インベントリに新規追加する処理
        if (checkIndex == -1)
        {
            //　アイテムidを設定
            idList.Add(id);

            //　アイテム所持数を設定
            countList.Add(count);

            Debug.Log("id:" + id + "のアイテムを" + count + "個新規追加");
        }
        //アイテム所持数を追加
        else
        {
            Debug.LogError("id:" + id + "のドキュメントをすでに所持しています");
        }
    }
}
