using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField]  public float distance = 30f;//�C���^���N�g�ł��鋗��
    GameObject pickUpItem;//�E�����A�C�e��
    GameObject interactDoor;//�C���^���N�g����h�A
    public bool isInteract;

    private Door door;

    private void Start()
    {
        isInteract = false;
    }

    private void Update()
    {
        //Ray������
        Debug.DrawRay(Camera.main.transform.position, 
            Camera.main.transform.forward, Color.green, 3);

        Interact();
    }

    void Interact() 
    {
        RaycastHit raycastHit;

        //Camera����Ray���΂�
        if (Physics.Raycast(Camera.main.transform.position, 
            Camera.main.transform.forward, out raycastHit, distance))
        {
            if (Input.GetKeyDown(KeyCode.E)) 
            {
                //�A�C�e�����E��
                if (raycastHit.transform.tag == "Item")
                {
                    isInteract = true;
                    pickUpItem = raycastHit.transform.gameObject;
                    Destroy(pickUpItem);
                }

                //�h�A�̊J��
                if (raycastHit.transform.tag == "Door") 
                {
                    isInteract = true;
                    interactDoor = raycastHit.transform.gameObject;
                    door = interactDoor.GetComponent<Door>();
                    door.DoorSystem();
                }
            }   
        }
        else
        {
            isInteract = false;
        }
    }
}
