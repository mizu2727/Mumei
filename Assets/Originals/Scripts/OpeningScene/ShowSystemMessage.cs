using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ShowSystemMessage : MonoBehaviour
{
    [Header("���b�Z�[�W�ԍ�")]
    [SerializeField] public int number = 1;
    async void Start()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(3));

        ShowGameSystemMessage(number);
    }

    public async void ShowGameSystemMessage(int number)
    {
        await MessageController.instance.ShowSystemMessage(number);
    }
}
