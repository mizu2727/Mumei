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
            Debug.Log("�h�L�������g���K�v���I");
            return;
        }

        Debug.Log("���̃h�L�������g�Ɋ֌W����A�C�e����I������");

        MysteryItemCheck();
    }

    void MysteryItemCheck() 
    {
        
    }
}
