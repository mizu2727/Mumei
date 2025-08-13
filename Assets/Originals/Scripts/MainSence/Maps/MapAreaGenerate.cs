
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class MapAreaGenerate : MonoBehaviour
{
    [Header("�}�b�v�G���A���i�[����(�q�G�����L�[��̃}�b�v�G���A���A�^�b�`���邱��)")]
    [SerializeField] private List <GameObject> areaPrefabList;

    [Header("�����_���ɑI�΂ꂽ�}�b�v�G���A���i�[�����(�A�^�b�`�֎~)")]
    public List<GameObject> useMapAreaList = new ();

    [Header("�}�b�v�G���A�����n�_��Transform�z��(�q�G�����L�[��̃}�b�v�G���A�����n�_���A�^�b�`���邱��)")]
    [SerializeField] private Transform[] mapAreaPoint;


    [Header("�A�C�e�����i�[����(�q�G�����L�[��̃A�C�e�����A�^�b�`���邱�ƁB���EmptyPrefab���i�[���邱�ƁB)")]
    [SerializeField] private List<GameObject> itemPrefabList;

    [Header("�����_���ɑI�΂ꂽ�A�C�e�����i�[�����(�A�^�b�`�֎~)")]
    public List<GameObject> useItemList = new();

    [Header("�A�C�e�������n�_��Transform�z��(�q�G�����L�[���Drawer�X�N���v�g��drawerItemTransform���A�^�b�`���邱��)")]
    [SerializeField] private Transform[] itemPoint;

    void Start()
    {
        MapGenerate();

        ItemGenerate();
    }

    /// <summary>
    /// �}�b�v�����_���������\�b�h
    /// </summary>
    void MapGenerate() 
    {
        // areaPrefabList�̃R�s�[���쐬
        List<GameObject> shuffledMapAreaPrefabList = new List<GameObject>(areaPrefabList);

        // areaPrefabList���V���b�t��
        ShuffleList(shuffledMapAreaPrefabList);

        // �V���b�t�����ꂽ���X�g���i�[
        useMapAreaList.AddRange(shuffledMapAreaPrefabList);


        for (int i = 0; i < useMapAreaList.Count; i++)
        {
            useMapAreaList[i].transform.position = mapAreaPoint[i].position;

            Debug.Log("GameObject " + i + ": " + useMapAreaList[i].name);

        }
    }

    /// <summary>
    /// �A�C�e�������_���������\�b�h
    /// </summary>
    void ItemGenerate()
    {
        // areaPrefabList�̃R�s�[���쐬
        List<GameObject> shuffledItemPrefabList = new List<GameObject>(itemPrefabList);

        // areaPrefabList���V���b�t��
        ShuffleList(shuffledItemPrefabList);

        // �V���b�t�����ꂽ���X�g���i�[
        useItemList.AddRange(shuffledItemPrefabList);

        for (int i = 0; i < useItemList.Count; i++)
        {
            useItemList[i].transform.position = itemPoint[i].position;

            Debug.Log("GameObject " + i + ": " + useItemList[i].name);
        }



        for (int i = 0; i < useItemList.Count; i++)
        {
            // �A�C�e�������n�_��Drawer�R���|�[�l���g�����邩�m�F
            Drawer drawer = itemPoint[i].GetComponent<Drawer>();

            if (drawer != null)
            {
                // �A�C�e���𐶐�����ہA�ʒu����itemPoint[i].position�ł͂Ȃ��ADrawer�̐e��Transform�ɍ��킹��
                GameObject newItem = Instantiate(useItemList[i]);

                // �A�C�e���̈ʒu��itemPoint�ɍ��킹��
                newItem.transform.position = itemPoint[i].position;

                // ���������A�C�e����Drawer�ɃA�^�b�`
                drawer.SetItemTransform(newItem.transform);
            }
            else
            {
                Debug.LogError(itemPoint[i].name + "��Drawer�R���|�[�l���g��������܂���ł����I");
            }
        }
    }

    /// <summary>
    /// ���X�g�̗v�f�������_���ɃV���b�t�����郁�\�b�h
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
