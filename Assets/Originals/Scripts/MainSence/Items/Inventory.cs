//�p�~
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //�@�A�C�e��ID�Ǘ�
    private readonly List<int> idList = new();

    //�@�A�C�e���������Ǘ�
    private readonly List<int> countList = new();

    //SO_Item sO_Item;
    //private Item item;

    private Player player;

    void Start()
    {
        player = GetComponent<Player>();
    }
    void Update()
    {

    }

    //�C���x���g���ɃA�C�e����ǉ�
    public void GetItem(int id, int count)
    {
        


        //���X�g�̒��ɃA�C�e�������Ԗڂɑ��݂���̂����m�F
        //���݂��Ȃ��ꍇ��-1��Ԃ�
        int checkIndex = idList.IndexOf(id);

        //�C���x���g���ɐV�K�ǉ����鏈��
        if (checkIndex == -1)
        {
            //�@�A�C�e��id��ݒ�
            idList.Add(id);

            //�@�A�C�e����������ݒ�
            countList.Add(count);

            Debug.Log("id:" + id + "�̃A�C�e����" + count + "�V�K�ǉ�");
        }
        //�A�C�e����������ǉ�
        else
        {
            countList[checkIndex] += count;

            Debug.Log("id:" + id + "�̃A�C�e����" + count + "����");
        }
    }

    public void GetDocument(int id, int count)
    {
        //player.isHoldDocument = true;

        //���X�g�̒��ɃA�C�e�������Ԗڂɑ��݂���̂����m�F
        //���݂��Ȃ��ꍇ��-1��Ԃ�
        int checkIndex = idList.IndexOf(id);

        //sO_Item.GetItemById(checkIndex);
        

        //�C���x���g���ɐV�K�ǉ����鏈��
        if (checkIndex == -1)
        {
            //�A�C�e��id��ݒ�
            idList.Add(id);

            //�A�C�e����������ݒ�
            countList.Add(count);

            Debug.Log("id:" + id + "�̃h�L�������g��" + count + "�V�K�ǉ�");
        }
        //�A�C�e����������ǉ�
        else
        {
            Debug.LogError("id:" + id + "�̃h�L�������g�����łɏ������Ă��܂�");
        }
    }

    public void GetMysteryItem(int id, int count)
    {
        //player.isHoldMysteryItem = true;

        //���X�g�̒��ɃA�C�e�������Ԗڂɑ��݂���̂����m�F
        //���݂��Ȃ��ꍇ��-1��Ԃ�
        int checkIndex = idList.IndexOf(id);

        //�C���x���g���ɐV�K�ǉ����鏈��
        if (checkIndex == -1)
        {
            //�A�C�e��id��ݒ�
            idList.Add(id);

            //�A�C�e����������ݒ�
            countList.Add(count);

            Debug.Log("id:" + id + "�̃~�X�e���[�A�C�e����" + count + "�V�K�ǉ�");
        }
        //�A�C�e����������ǉ�
        else
        {
            Debug.LogError("id:" + id + "�̃~�X�e���[�A�C�e�������łɏ������Ă��܂�");
        }
    }
}
