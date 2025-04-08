using Mono.Cecil.Cil;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //���ʂ�ScriptableObject���A�^�b�`����K�v������
    [SerializeField] public SO_Item sO_Item;


    public bool isDebugGoal = false;

    private void Start()
    {

    }

    public void GoalCheck()
    {
        sO_Item.CleanNullItems();

        Debug.Log($"GoalCheck ���s����itemList��: {sO_Item.itemList.Count}");
        foreach (var item in sO_Item.itemList)
        {
            if (item != null)
            {
                Debug.Log($"itemList��: {item.itemName} - {item.itemType}");
            }
            else
            {
                Debug.LogWarning("itemList �� null ���܂܂�Ă��܂��I");
            }
        }

        if (sO_Item.GetItemByType(ItemType.Document) == false)
        {
            Debug.Log("�h�L�������g���K�v���I");
            return;
        }

        Debug.Log("�h�L�������g��������܂����I");

        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
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
