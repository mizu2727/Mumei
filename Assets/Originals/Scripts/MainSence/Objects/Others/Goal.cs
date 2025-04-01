using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Player player;


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

        Debug.Log("このドキュメントに関係するアイテムを選択せよ");

        MysteryItemCheck();
    }

    void MysteryItemCheck() 
    {
        
    }
}
