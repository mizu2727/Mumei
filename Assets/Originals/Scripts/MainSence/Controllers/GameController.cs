using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    [SerializeField] private TestMap01 map; // TestMap01�ւ̎Q��

    private void Start()
    {
        //�Q�[���J�n��Ƀ��[�v�����s
        InvokeWarpAfterDelay().Forget();
    }

    private async UniTask InvokeWarpAfterDelay()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(10f));
        await map.TriggerWarpAsync();
    }

    //����̃C�x���g�Ń��[�v���g���K�[
    public void OnSomeEvent()
    {
        map.TriggerWarpAsync().Forget();
    }
}
