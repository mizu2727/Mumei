using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] public SO_Item sO_Item;
    Item item;
    public bool isDebugGoal = false;

    private void Start()
    {

    }

    public void GoalCheck()
    {
        Debug.Log("GoalCheck()");
        if (!player.isHoldDocument)
        {
            Debug.Log("�h�L�������g���K�v���I");
            return;
        }

        if (!player.isHoldMysteryItem)
        {
            Debug.Log("�~�X�e���[�A�C�e�����K�v���I");
            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        Debug.Log("���̃h�L�������g�Ɋ֌W����A�C�e����I������");

        if (isDebugGoal) 
        {
         
        }
    }
}
