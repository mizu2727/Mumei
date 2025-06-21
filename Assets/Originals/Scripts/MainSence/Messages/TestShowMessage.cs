using UnityEngine;
using Cysharp.Threading.Tasks;

public class TestShowMessage : MonoBehaviour
{
    [Header("���b�Z�[�W�ԍ�(�f�o�b�O�p)")]
    [SerializeField] public int number = 1;
    void Start()
    {
        TestShowTalkMessage(number);
    }

    public async void TestShowTalkMessage(int number) 
    {
        await MessageController.instance.ShowTalkMessage(number);
    }
}
