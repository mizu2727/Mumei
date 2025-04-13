using Mono.Cecil.Cil;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;



public class Goal : MonoBehaviour
{
    //���ʂ�ScriptableObject���A�^�b�`����K�v������
    [SerializeField] public SO_Item sO_Item;

    [SerializeField] public int anserItemId;//����p�̃A�C�e��id


    [SerializeField] private GameObject selectMysteryItemPanel;
    [SerializeField] private Button[] selectMysteryItemNameButton;//�~�X�e���[�A�C�e�����̃{�^��
    [SerializeField] private Image[] selectMysteryItemImage;//�~�X�e���[�A�C�e���摜

    public bool isSelectMysteryItemPanel;

    private void Start()
    {
        isSelectMysteryItemPanel = false;
        ViewSelectMysteryItemPanel();
    }

    public async void GoalCheck()
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
            string message = "�h�L�������g���W�߂Ă�������";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }


        if (!sO_Item.GetItemByType(ItemType.MysteryItem))
        {
            string message = "�~�X�e���[�A�C�e�����W�߂Ă�������";

            MessageController.instance.ShowMessage(message);

            await UniTask.Delay(TimeSpan.FromSeconds(3));

            MessageController.instance.ResetMessage();

            return;
        }
        else 
        {
            MysteryItemCheck();
        }
        

        
    }

    void MysteryItemCheck() 
    {
        string message = "���̃h�L�������g�Ɋ֌W����A�C�e����I������";

        MessageController.instance.ShowMessage(message);


    }

    public void ViewSelectMysteryItemPanel()
    {
        if (isSelectMysteryItemPanel)
        {
            selectMysteryItemPanel.SetActive(true);
        }
        else
        {
            selectMysteryItemPanel.SetActive(false);
        }
    }
}
