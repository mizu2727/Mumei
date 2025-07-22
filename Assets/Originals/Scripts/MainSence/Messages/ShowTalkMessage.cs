using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class ShowTalkMessage : MonoBehaviour
{
    [Header("���b�Z�[�W�ԍ�(�f�o�b�O�p)")]
    [SerializeField] public int number = 1;
    async void Start()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        ShowGameTalkMessage(number);
    }

    public async void ShowGameTalkMessage(int number) 
    {
        await MessageController.instance.ShowTalkMessage(number);
    }
}
