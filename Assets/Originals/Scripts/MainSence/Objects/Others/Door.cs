using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    bool isOpenDoor = false;//ドアの開閉の判定
    [SerializeField] public bool isNeedKeyDoor = false;//鍵が掛かっているかを判定
    [SerializeField] float openDirenctionValue = 90.0f;//ドアを開ける角度
    [SerializeField] float closeDirenctionValue = 0.0f;//ドアを閉じる角度
    
    private Player player;//プレイヤー

    public void DoorSystem() 
    {
        if (isOpenDoor)
        {
            CloseDoor();
        }
        else 
        {
            //鍵が必要な場合
            if (isNeedKeyDoor && player.isHoldKey)
            {
                Debug.Log("解錠しました。");
                player.isHoldKey = false;
                isNeedKeyDoor = false;
            }
            else if (!isNeedKeyDoor)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("施錠されている。");
            }
        }
    }

    //ドアを開ける
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
    }

    //ドアを閉める
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
    }
}
