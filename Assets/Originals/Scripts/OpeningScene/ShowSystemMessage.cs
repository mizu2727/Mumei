using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class ShowSystemMessage : MonoBehaviour
{
    [Header("メッセージ番号")]
    [SerializeField] public int number = 1;

    private async void Start()
    {
        //オブジェクトが破棄された際のキャンセル処理
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        //Delay中に破棄された場合、SuppressCancellationThrowで例外を出さずに中断する
        bool isCanceled = await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: token).SuppressCancellationThrow();

        //キャンセルされている場合
        if (isCanceled) 
        {
            //処理を中断する
            return;
        }

        //メッセージを表示する
        ShowGameSystemMessage(number);
    }

    public void ShowGameSystemMessage(int number)
    //public async void ShowGameSystemMessage(int number)
    {
        //ShowSystemMessageを非同期実行しつつ、キャンセル例外を握りつぶしてForget()する
        MessageController.instance.ShowSystemMessage(number).SuppressCancellationThrow().Forget();
    }
}
