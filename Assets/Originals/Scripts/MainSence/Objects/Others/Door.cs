using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    bool isOpenDoor = false;//�h�A�̊J�̔���
    [SerializeField] public bool isNeedKeyDoor = false;//�����|�����Ă��邩�𔻒�
    [SerializeField] float openDirenctionValue = 90.0f;//�h�A���J����p�x
    [SerializeField] float closeDirenctionValue = 0.0f;//�h�A�����p�x
    
    private Player player;//�v���C���[

    public void DoorSystem() 
    {
        if (isOpenDoor)
        {
            CloseDoor();
        }
        else 
        {
            //�����K�v�ȏꍇ
            if (isNeedKeyDoor && player.isHoldKey)
            {
                Debug.Log("�������܂����B");
                player.isHoldKey = false;
                isNeedKeyDoor = false;
            }
            else if (!isNeedKeyDoor)
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("�{������Ă���B");
            }
        }
    }

    //�h�A���J����
    public void OpenDoor() 
    {
        isOpenDoor = true;
        transform.Rotate(0, openDirenctionValue, 0);
    }

    //�h�A��߂�
    void CloseDoor()
    {
        isOpenDoor = false;
        transform.Rotate(0, closeDirenctionValue, 0);
    }
}
