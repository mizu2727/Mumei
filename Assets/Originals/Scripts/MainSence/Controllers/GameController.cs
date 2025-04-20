using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [SerializeField] private TestMap01 map; // TestMap01への参照

    private void Start()
    {
        //ゲーム開始後にワープを実行
        InvokeWarpAfterDelay().Forget();
    }

    private async UniTask InvokeWarpAfterDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(10f));
        await map.TriggerWarpAsync();
    }

    //特定のイベントでワープをトリガー
    public void OnSomeEvent()
    {
        map.TriggerWarpAsync().Forget();
    }
}
