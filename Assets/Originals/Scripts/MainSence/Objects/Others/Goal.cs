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
            Debug.Log("ドキュメントが必要だ！");
            return;
        }

        if (!player.isHoldMysteryItem)
        {
            Debug.Log("ミステリーアイテムが必要だ！");
            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        Debug.Log("このドキュメントに関係するアイテムを選択せよ");

        if (isDebugGoal) 
        {
         
        }
    }
}
